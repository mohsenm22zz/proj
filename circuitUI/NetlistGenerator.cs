using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace wpfUI
{
    public static class NetlistGenerator
    {
        private class NetlistComponentInfo
        {
            public string Name { get; set; }
            public string Node1 { get; set; }
            public string Node2 { get; set; }
            public double Value { get; set; }
            public double AcPhase { get; set; }
        }

        public static Tuple<List<string>, Dictionary<Point, string>> Generate(Canvas canvas)
        {
            var commands = new List<string>();
            var componentInfos = new List<NetlistComponentInfo>();
            
            // This method now correctly identifies all electrically connected points as single nodes.
            var nodeMap = CreateIntelligentNodeMap(canvas);

            // Find the name of the node cluster that contains the ground symbol.
            string groundClusterName = FindGroundClusterName(canvas, nodeMap);

            // If a ground node exists, remap its entire cluster to "0".
            if (groundClusterName != null)
            {
                // Create a list of all keys (points) that belong to the ground cluster.
                var pointsInGroundCluster = nodeMap.Where(kvp => kvp.Value == groundClusterName)
                                                   .Select(kvp => kvp.Key)
                                                   .ToList();
                
                // Remap all those points to the official ground node "0".
                foreach (var point in pointsInGroundCluster)
                {
                    nodeMap[point] = "0";
                }
            }

            // Build the list of components with the newly mapped node names.
            foreach (var child in canvas.Children.OfType<ComponentControl>())
            {
                Point leftConnector = child.LeftConnector.TransformToAncestor(canvas).Transform(new Point(child.LeftConnector.ActualWidth / 2, child.LeftConnector.ActualHeight / 2));
                Point rightConnector = child.RightConnector.TransformToAncestor(canvas).Transform(new Point(child.RightConnector.ActualWidth / 2, child.RightConnector.ActualHeight / 2));

                string leftNodeName = nodeMap.ContainsKey(leftConnector) ? nodeMap[leftConnector] : $"UNCONNECTED_{child.ComponentName}_L";
                string rightNodeName = nodeMap.ContainsKey(rightConnector) ? nodeMap[rightConnector] : $"UNCONNECTED_{child.ComponentName}_R";

                // If a node was part of the ground cluster, its name is now "0".
                if (groundClusterName != null)
                {
                    if (leftNodeName == groundClusterName) leftNodeName = "0";
                    if (rightNodeName == groundClusterName) rightNodeName = "0";
                }

                var info = new NetlistComponentInfo
                {
                    Name = child.ComponentName,
                    Value = child.Value,
                    AcPhase = child.AcPhase,
                    Node1 = leftNodeName,
                    Node2 = rightNodeName,
                };
                
                componentInfos.Add(info);
            }

            // Generate the final command strings for the netlist.
            foreach (var info in componentInfos)
            {
                string type = new string(info.Name.TakeWhile(char.IsLetter).ToArray());
                string command = (type == "ACV")
                    ? $"{type} {info.Name} {info.Node1} {info.Node2} {info.Value} {info.AcPhase}"
                    : $"{type} {info.Name} {info.Node1} {info.Node2} {info.Value}";
                commands.Add(command);
            }
            
            // Add the .GND directive if a ground was present.
            if (groundClusterName != null)
            {
                commands.Add($"GND 0");
            }

            return Tuple.Create(commands, nodeMap);
        }

        /// <summary>
        /// Finds the cluster name (e.g., "N1") that is associated with an explicit ground symbol.
        /// </summary>
        private static string FindGroundClusterName(Canvas canvas, Dictionary<Point, string> nodeMap)
        {
            foreach (var node in canvas.Children.OfType<NodeControl>())
            {
                if (node.IsGround)
                {
                    Point nodeCenter = new Point(Canvas.GetLeft(node) + node.Width / 2, Canvas.GetTop(node) + node.Height / 2);
                    if (nodeMap.ContainsKey(nodeCenter))
                    {
                        return nodeMap[nodeCenter]; // Return the name of the cluster, e.g., "N2"
                    }
                }
            }
            return null; // No ground symbol found
        }
        
        /// <summary>
        /// Traverses the wires to find all electrically connected points and group them into nodes.
        /// </summary>
        private static Dictionary<Point, string> CreateIntelligentNodeMap(Canvas canvas)
        {
            var allPoints = new HashSet<Point>();
            var adjacency = new Dictionary<Point, List<Point>>();

            // 1. Gather all unique connection points from components and nodes.
            foreach (var child in canvas.Children)
            {
                if (child is ComponentControl component)
                {
                    Point left = component.LeftConnector.TransformToAncestor(canvas).Transform(new Point(component.LeftConnector.ActualWidth / 2, component.LeftConnector.ActualHeight / 2));
                    Point right = component.RightConnector.TransformToAncestor(canvas).Transform(new Point(component.RightConnector.ActualWidth / 2, component.RightConnector.ActualHeight / 2));
                    allPoints.Add(left);
                    allPoints.Add(right);
                }
                else if (child is NodeControl node)
                {
                    Point center = new Point(Canvas.GetLeft(node) + node.Width / 2, Canvas.GetTop(node) + node.Height / 2);
                    allPoints.Add(center);
                }
            }

            // 2. Build an adjacency list representing wire connections.
            foreach (var point in allPoints)
            {
                adjacency[point] = new List<Point>();
            }

            foreach (var wire in canvas.Children.OfType<Wire>())
            {
                // A wire connects its start and end points.
                var start = wire.StartPoint;
                var end = wire.EndPoint;

                // We need to find the actual component/node connection points that this wire touches.
                Point? actualStart = allPoints.FirstOrDefault(p => (p - start).Length < 0.1);
                Point? actualEnd = allPoints.FirstOrDefault(p => (p - end).Length < 0.1);

                if (actualStart.HasValue && actualEnd.HasValue)
                {
                    adjacency[actualStart.Value].Add(actualEnd.Value);
                    adjacency[actualEnd.Value].Add(actualStart.Value);
                }
            }
            
            // 3. Perform graph traversal (BFS) to find connected components (nodes)
            var nodeMap = new Dictionary<Point, string>();
            var visited = new HashSet<Point>();
            int nodeCounter = 1;

            foreach (var startPoint in allPoints)
            {
                if (visited.Contains(startPoint)) continue;

                string currentNodeName = $"N{nodeCounter++}";
                var queue = new Queue<Point>();
                
                queue.Enqueue(startPoint);
                visited.Add(startPoint);

                while (queue.Count > 0)
                {
                    var currentPoint = queue.Dequeue();
                    nodeMap[currentPoint] = currentNodeName;

                    if (adjacency.ContainsKey(currentPoint))
                    {
                        foreach (var neighbor in adjacency[currentPoint])
                        {
                            if (!visited.Contains(neighbor))
                            {
                                visited.Add(neighbor);
                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                }
            }

            return nodeMap;
        }

        public static string FindProbeTarget(Canvas canvas, Point clickPoint)
        {
            double tolerance = 10.0;

            foreach (var component in canvas.Children.OfType<ComponentControl>())
            {
                Point componentPos = component.TransformToAncestor(canvas).Transform(new Point(0, 0));
                Rect componentBounds = new Rect(componentPos, new Size(component.ActualWidth, component.ActualHeight));
                if (componentBounds.Contains(clickPoint))
                {
                    return $"I({component.ComponentName})";
                }
            }

            // Use the generated node map to find the node name.
            var nodeMap = CreateIntelligentNodeMap(canvas);
            string groundClusterName = FindGroundClusterName(canvas, nodeMap);

            foreach (var entry in nodeMap)
            {
                if ((clickPoint - entry.Key).Length < tolerance)
                {
                    // If the probed node is part of the ground cluster, return "0".
                    return entry.Value == groundClusterName ? "0" : entry.Value;
                }
            }

            return null;
        }
    }
}

