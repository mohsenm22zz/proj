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
        private static Dictionary<Point, string> CreateIntelligentNodeMap(Canvas canvas)
        {
            var allPoints = new HashSet<Point>();
            var adjacency = new Dictionary<Point, List<Point>>();
            var allWires = canvas.Children.OfType<Wire>().ToList();
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

            // 2. Find wire intersections and add them as connection points
            var wireIntersections = new HashSet<Point>();
            for (int i = 0; i < allWires.Count; i++)
            {
                for (int j = i + 1; j < allWires.Count; j++)
                {
                    Wire wire1 = allWires[i];
                    Wire wire2 = allWires[j];

                    if (WireIntersectionHelper.LinesIntersect(wire1.StartPoint, wire1.EndPoint, 
                                                            wire2.StartPoint, wire2.EndPoint, 
                                                            out Point intersection))
                    {
                        wireIntersections.Add(intersection);
                        allPoints.Add(intersection);
                    }
                }
            }

            // 3. Build an adjacency list representing wire connections.
            foreach (var point in allPoints)
            {
                adjacency[point] = new List<Point>();
            }

            foreach (var wire in allWires)
            {
                var wirePoints = new List<Point> { wire.StartPoint, wire.EndPoint };
                
                // Add intersections that lie on this wire
                foreach (var intersection in wireIntersections)
                {
                    if (WireIntersectionHelper.IsPointOnLineSegment(wire.StartPoint, wire.EndPoint, intersection))
                    {
                        wirePoints.Add(intersection);
                    }
                }

                // Connect each point on the wire to its nearest connection points
                foreach (var point in wirePoints)
                {
                    Point? actualPoint = allPoints.FirstOrDefault(p => (p - point).Length < 0.1);
                    if (actualPoint.HasValue)
                    {
                        foreach (var otherPoint in wirePoints)
                        {
                            Point? actualOtherPoint = allPoints.FirstOrDefault(p => (p - otherPoint).Length < 0.1);
                            if (actualOtherPoint.HasValue && !actualPoint.Value.Equals(actualOtherPoint.Value))
                            {
                                adjacency[actualPoint.Value].Add(actualOtherPoint.Value);
                            }
                        }
                    }
                }
            }
            
            // 4. Perform graph traversal (BFS) to find connected components (nodes)
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

