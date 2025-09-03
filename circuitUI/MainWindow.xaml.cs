using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace wpfUI
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, int> _componentCounts = new Dictionary<string, int>
        {
            {"R", 1}, {"C", 1}, {"L", 1}, {"D", 1}, {"V", 1}, {"ACV", 1}, {"I", 1}
        };

        private bool _isWiringMode = false;
        private bool _isProbingMode = false;
        private bool _isCircuitLocked = false;
    private Wire? _currentWire = null;
        private SimulationParameters _simulationParameters;
        private readonly List<string> _probedItems = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            _simulationParameters = new SimulationParameters { CurrentAnalysis = SimulationParameters.AnalysisType.Transient, StopTime = 1, MaxTimestep = 0.001 }; // Default
            this.Loaded += MainWindow_Loaded;
            this.KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DrawGrid();
            //LoadDefaultCircuit(); //example
        }
        private void LoadDefaultCircuit()
        {
            var acSource = new ComponentControl
            {
                ComponentName = "ACV1",
                Width = 80,
                Height = 40,
                Value = 10,
                AcPhase = 0,
                HasBeenPlaced = true
            };
            Canvas.SetLeft(acSource, 80);
            Canvas.SetTop(acSource, 140);

            var resistor = new ComponentControl
            {
                ComponentName = "R1",
                Width = 80,
                Height = 40,
                Value = 1000,
                HasBeenPlaced = true
            };
            Canvas.SetLeft(resistor, 240);
            Canvas.SetTop(resistor, 140);

            var capacitor = new ComponentControl
            {
                ComponentName = "C1",
                Width = 80,
                Height = 40,
                Value = 0.000001,
                HasBeenPlaced = true
            };
            Canvas.SetLeft(capacitor, 400);
            Canvas.SetTop(capacitor, 140);
            var groundNode = new NodeControl
            {
                Width = 10,
                Height = 10,
                IsGround = true,
                HasBeenPlaced = true
            };
            Canvas.SetLeft(groundNode, 275);
            Canvas.SetTop(groundNode, 235);

            SchematicCanvas.Children.Add(acSource);
            SchematicCanvas.Children.Add(resistor);
            SchematicCanvas.Children.Add(capacitor);
            SchematicCanvas.Children.Add(groundNode);

            var wire1 = new Wire { StartPoint = new Point(160, 160) };
            wire1.AddPoint(new Point(240, 160));
            SchematicCanvas.Children.Add(wire1);

            var wire2 = new Wire { StartPoint = new Point(320, 160) };
            wire2.AddPoint(new Point(400, 160));
            SchematicCanvas.Children.Add(wire2);

            var wire3 = new Wire { StartPoint = new Point(80, 160) };
            wire3.AddPoint(new Point(80, 240));
            wire3.AddPoint(new Point(280, 240));
            SchematicCanvas.Children.Add(wire3);

            var wire4 = new Wire { StartPoint = new Point(480, 160) };
            wire4.AddPoint(new Point(480, 240));
            wire4.AddPoint(new Point(280, 240));
            SchematicCanvas.Children.Add(wire4);

            _componentCounts["ACV"] = 2;
            _componentCounts["R"] = 2;
            _componentCounts["C"] = 2;

            _isCircuitLocked = true;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ExitWiringMode();
                ExitProbingMode();
                DeselectAll();
            }
        }

        private void DrawGrid()
        {
            double gridSize = 20.0;
            var existingChildren = SchematicCanvas.Children.OfType<UIElement>().ToList();

            SchematicCanvas.Children.Clear();

            for (double x = 0; x < SchematicCanvas.ActualWidth; x += gridSize)
            {
                var line = new Line { X1 = x, Y1 = 0, X2 = x, Y2 = SchematicCanvas.ActualHeight, Stroke = new SolidColorBrush(Color.FromArgb(50, 80, 80, 80)), StrokeThickness = 1 };
                SchematicCanvas.Children.Add(line);
            }

            for (double y = 0; y < SchematicCanvas.ActualHeight; y += gridSize)
            {
                var line = new Line { X1 = 0, Y1 = y, X2 = SchematicCanvas.ActualWidth, Y2 = y, Stroke = new SolidColorBrush(Color.FromArgb(50, 80, 80, 80)), StrokeThickness = 1 };
                SchematicCanvas.Children.Add(line);
            }

            foreach (var child in existingChildren.Where(c => !(c is Line && ((Line)c).StrokeThickness == 1)))
            {
                SchematicCanvas.Children.Add(child);
            }
        }

        private void AddComponent_Click(object sender, RoutedEventArgs e)
        {
            if (_isCircuitLocked)
            {
                MessageBox.Show("Cannot add new components while circuit is wired.", "Circuit Locked");
                return;
            }

            if (sender is Button button && button.Tag is string type)
            {
                int count = _componentCounts[type];
                var componentControl = new ComponentControl
                {
                    ComponentName = $"{type}{count}",
                    Width = 80,
                    Height = 40
                };
                Canvas.SetLeft(componentControl, 100);
                Canvas.SetTop(componentControl, 100);
                SchematicCanvas.Children.Add(componentControl);
                _componentCounts[type]++;
            }
        }

        private void PlaceNode_Click(object sender, RoutedEventArgs e)
        {
            var nodeControl = new NodeControl { Width = 10, Height = 10 };
            Point position = new Point(SchematicCanvas.ActualWidth / 2, SchematicCanvas.ActualHeight / 2);
            double gridSize = 20.0;
            double snappedX = Math.Round(position.X / gridSize) * gridSize;
            double snappedY = Math.Round(position.Y / gridSize) * gridSize;
            Canvas.SetLeft(nodeControl, snappedX - nodeControl.Width / 2);
            Canvas.SetTop(nodeControl, snappedY - nodeControl.Height / 2);

            MessageBoxResult result = MessageBox.Show("Set this node as ground?", "Ground Node", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                foreach (var existingNode in SchematicCanvas.Children.OfType<NodeControl>())
                {
                    existingNode.IsGround = false;
                }
                nodeControl.IsGround = true;
            }

            SchematicCanvas.Children.Add(nodeControl);
        }

        private void PlaceWire_Click(object sender, RoutedEventArgs e)
        {
            if (!_isWiringMode) EnterWiringMode();
            else ExitWiringMode();
        }

        private void PlaceProbe_Click(object sender, RoutedEventArgs e)
        {
            if (!_isProbingMode) EnterProbingMode();
            else ExitProbingMode();
        }

        private void EnterWiringMode()
        {
            ExitProbingMode();
            _isWiringMode = true;
            _isCircuitLocked = true;
            WireMenuItem.IsChecked = true;
            ProbeMenuItem.IsChecked = false;
            SchematicCanvas.Cursor = Cursors.Cross;
            SchematicCanvas.MouseLeftButtonDown += Canvas_Wiring_MouseDown;
            SchematicCanvas.MouseRightButtonDown += Canvas_Wiring_MouseRightButtonDown;
            SchematicCanvas.MouseMove += Canvas_Wiring_MouseMove;
        }

        private void ExitWiringMode()
        {
            _isWiringMode = false;
            WireMenuItem.IsChecked = false;
            if (_currentWire != null)
            {
                // Clean up the temporary preview wire if it exists
                SchematicCanvas.Children.Remove(_currentWire);
                _currentWire = null;
            }
            if (!_isProbingMode)
            {
                SchematicCanvas.Cursor = Cursors.Arrow;
            }
            SchematicCanvas.MouseLeftButtonDown -= Canvas_Wiring_MouseDown;
            SchematicCanvas.MouseRightButtonDown -= Canvas_Wiring_MouseRightButtonDown;
            SchematicCanvas.MouseMove -= Canvas_Wiring_MouseMove;
        }

        private void EnterProbingMode()
        {
            ExitWiringMode();
            _isProbingMode = true;
            ProbeMenuItem.IsChecked = true;
            SchematicCanvas.Cursor = Cursors.Help;
            SchematicCanvas.MouseLeftButtonDown += Canvas_Probing_MouseDown;
        }

        private void ExitProbingMode()
        {
            _isProbingMode = false;
            ProbeMenuItem.IsChecked = false;
            if (!_isWiringMode)
            {
                SchematicCanvas.Cursor = Cursors.Arrow;
            }
            SchematicCanvas.MouseLeftButtonDown -= Canvas_Probing_MouseDown;
        }

        private void Canvas_Wiring_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isWiringMode) ExitWiringMode();
        }

        private void Canvas_Wiring_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isWiringMode) return;
            Point clickPoint = e.GetPosition(SchematicCanvas);

            Point? connectionPoint = FindNearestConnectionPoint(clickPoint);
            Point snappedPoint = connectionPoint ?? new Point(Math.Round(clickPoint.X / 20.0) * 20.0, Math.Round(clickPoint.Y / 20.0) * 20.0);

            if (_currentWire == null) // Starting a new wire
            {
                _currentWire = new Wire { StartPoint = snappedPoint };
                SchematicCanvas.Children.Add(_currentWire);
            }
            else // Finishing a wire segment
            {
                _currentWire.EndPoint = snappedPoint; // Finalize the current wire
                _currentWire = null; // Reset to allow starting a new wire
            }
        }

        private void Canvas_Probing_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = e.GetPosition(SchematicCanvas);
            string itemToProbe = NetlistGenerator.FindProbeTarget(SchematicCanvas, clickPoint);

            if (!string.IsNullOrEmpty(itemToProbe))
            {
                if (_probedItems.Contains(itemToProbe))
                {
                    _probedItems.Remove(itemToProbe);
                    MessageBox.Show($"Removed '{itemToProbe}' from plot list.");
                }
                else
                {
                    _probedItems.Add(itemToProbe);
                    MessageBox.Show($"Added '{itemToProbe}' to plot list.");
                }
            }
        }

        private void Canvas_Wiring_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isWiringMode && _currentWire != null)
            {
                Point currentPoint = e.GetPosition(SchematicCanvas);
                Point snappedPoint = new Point(Math.Round(currentPoint.X / 20.0) * 20.0, Math.Round(currentPoint.Y / 20.0) * 20.0);
                _currentWire.UpdatePreview(snappedPoint);
            }
        }

        private void DeselectAll()
        {
            foreach (var child in SchematicCanvas.Children.OfType<Wire>())
                child.IsSelected = false;
        }

        private Point? FindNearestConnectionPoint(Point clickPoint)
        {
            double tolerance = 10.0;
            foreach (var child in SchematicCanvas.Children)
            {
                if (child is ComponentControl component)
                {
                    Point left = component.LeftConnector.TransformToAncestor(SchematicCanvas).Transform(new Point(component.LeftConnector.ActualWidth / 2, component.LeftConnector.ActualHeight / 2));
                    if ((clickPoint - left).Length < tolerance) return left;
                    Point right = component.RightConnector.TransformToAncestor(SchematicCanvas).Transform(new Point(component.RightConnector.ActualWidth / 2, component.RightConnector.ActualHeight / 2));
                    if ((clickPoint - right).Length < tolerance) return right;
                }
                else if (child is NodeControl node)
                {
                    Point center = new Point(Canvas.GetLeft(node) + node.Width / 2, Canvas.GetTop(node) + node.Height / 2);
                    if ((clickPoint - center).Length < tolerance) return center;
                }
            }
            return null;
        }

        private void PlaceComponent_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Component Library window would open here.", "Place Component");
        }

        private void EditSimulationCmd_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SimulationSettingsWindow();
            settingsWindow.Owner = this;
            if (settingsWindow.ShowDialog() == true)
            {
                _simulationParameters = settingsWindow.Parameters;
                MessageBox.Show("Simulation settings updated!");
            }
        }

        private void RunAnalysis_Click(object sender, RoutedEventArgs e)
        {
            var netlistResult = NetlistGenerator.Generate(SchematicCanvas);
            List<string> netlistCommands = netlistResult.Item1;

            if (!netlistCommands.Any())
            {
                MessageBox.Show("The circuit is empty.", "Empty Circuit");
                return;
            }

            var netlistWindow = new NetlistWindow(netlistCommands);
            netlistWindow.Owner = this;
            netlistWindow.ShowDialog();

            try
            {
                // Create circuit using our C# implementation
                var circuit = new CircuitSimulator.Circuit();

                foreach (var command in netlistCommands)
                {
                    var parts = command.Split(' ');
                    string type = parts[0];
                    try
                    {
                        if (type == "GND")
                            circuit.FindOrCreateNode(parts[1]).SetGround(true);
                        else if (type[0] == 'R')
                            AddResistorToCircuit(circuit, parts[1], parts[2], parts[3], double.Parse(parts[4], CultureInfo.InvariantCulture));
                        else if (type[0] == 'C')
                            AddCapacitorToCircuit(circuit, parts[1], parts[2], parts[3], double.Parse(parts[4], CultureInfo.InvariantCulture));
                        else if (type[0] == 'L')
                            AddInductorToCircuit(circuit, parts[1], parts[2], parts[3], double.Parse(parts[4], CultureInfo.InvariantCulture));
                        else if (type[0] == 'V')
                            AddVoltageSourceToCircuit(circuit, parts[1], parts[2], parts[3], double.Parse(parts[4], CultureInfo.InvariantCulture));
                        else if (type[0] == 'A')
                            AddACVoltageSourceToCircuit(circuit, parts[1], parts[2], parts[3], double.Parse(parts[4], CultureInfo.InvariantCulture), double.Parse(parts[5], CultureInfo.InvariantCulture));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error adding component: {ex.Message}", "Error");
                        return;
                    }
                }

                bool success = false;
                string[] itemsToPlot = _probedItems.Any() ? _probedItems.ToArray() : GetNodeNames(circuit);
                string acSource = netlistCommands.FirstOrDefault(c => c.StartsWith("ACV"))?.Split(' ')[1] ?? "";

                switch (_simulationParameters.CurrentAnalysis)
                {
                    case SimulationParameters.AnalysisType.DCOperatingPoint:
                        // Error if ACV present
                        if (netlistCommands.Any(c => c.StartsWith("ACV")))
                        {
                            MessageBox.Show("DC analysis cannot be run with an AC voltage source in the circuit.", "Simulation Error");
                            return;
                        }
                        success = CircuitSimulator.Analysis.DCAnalysis(circuit);
                        if (success)
                        {
                            // For DC analysis, collect node voltages and component currents
                            var nodeVoltages = new List<string>();
                            var componentCurrents = new List<string>();
                            
                            // Collect node voltages
                            foreach (var node in circuit.Nodes)
                            {
                                if (!node.IsGround)
                                {
                                    nodeVoltages.Add($"V({node.Name}) = {node.GetVoltage():G4} V");
                                }
                            }
                            
                            // Collect component currents
                            foreach (var vs in circuit.VoltageSources)
                            {
                                componentCurrents.Add($"I({vs.Name}) = {vs.GetCurrent():G4} A");
                            }
                            
                            // Add other component currents as needed
                            foreach (var r in circuit.Resistors)
                            {
                                componentCurrents.Add($"I({r.Name}) = {r.GetCurrent():G4} A");
                            }
                            
                            var resultsWindow = new DCResultsWindow(nodeVoltages, componentCurrents) { Owner = this };
                            resultsWindow.ShowDialog();
                        }
                        break;
                    case SimulationParameters.AnalysisType.Transient:
                        success = CircuitSimulator.Analysis.TransientAnalysis(circuit, _simulationParameters.MaxTimestep, _simulationParameters.StopTime);
                        if (success)
                        {
                            var plotWindow = new PlotWindow { Owner = this };
                            plotWindow.LoadTransientDataFromCS(circuit, itemsToPlot);
                            plotWindow.Show();
                        }
                        break;
                    case SimulationParameters.AnalysisType.ACSweep:
                        if (string.IsNullOrEmpty(acSource))
                        {
                            MessageBox.Show("AC Sweep requires an ACV component in the circuit.", "Simulation Error");
                            return;
                        }
                        
                        int pointsCalculated = CircuitSimulator.Analysis.ACSweepAnalysis(
                            circuit, 
                            acSource, 
                            _simulationParameters.StartFrequency, 
                            _simulationParameters.StopFrequency, 
                            _simulationParameters.NumberOfPoints, 
                            _simulationParameters.SweepType);
                        
                        success = pointsCalculated > 0;
                        if (success)
                        {
                            var plotWindow = new PlotWindow { Owner = this };
                            plotWindow.LoadACDataFromCS(
                                circuit, 
                                itemsToPlot, 
                                _simulationParameters.StartFrequency, 
                                _simulationParameters.StopFrequency, 
                                _simulationParameters.NumberOfPoints, 
                                _simulationParameters.SweepType);
                            plotWindow.Show();
                        }
                        break;
                    case SimulationParameters.AnalysisType.PhaseSweep:
                        string phaseSource = netlistCommands.FirstOrDefault(c => c.StartsWith("ACV"))?.Split(' ')[1] ?? "";
                        if (string.IsNullOrEmpty(phaseSource))
                        {
                            MessageBox.Show("Phase Sweep requires an ACV component in the circuit.", "Simulation Error");
                            return;
                        }
                        
                        int phasePointsCalculated = CircuitSimulator.Analysis.PhaseSweepAnalysis(
                            circuit,
                            phaseSource,
                            _simulationParameters.BaseFrequency,
                            _simulationParameters.StartPhase,
                            _simulationParameters.StopPhase,
                            _simulationParameters.NumberOfPoints);
                        
                        success = phasePointsCalculated > 0;
                        if (success)
                        {
                            var plotWindow = new PlotWindow { Owner = this };
                            plotWindow.LoadPhaseDataFromCS(
                                circuit,
                                itemsToPlot,
                                _simulationParameters.BaseFrequency,
                                _simulationParameters.StartPhase,
                                _simulationParameters.StopPhase,
                                _simulationParameters.NumberOfPoints);
                            plotWindow.Show();
                        }
                        break;
                }

                if (!success)
                {
                    MessageBox.Show("The simulation failed to run.", "Simulation Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during simulation setup: {ex.Message}", "Error");
            }
        }

        // Helper methods to add components to the circuit
        private void AddResistorToCircuit(CircuitSimulator.Circuit circuit, string name, string node1, string node2, double value)
        {
            var resistor = new CircuitSimulator.Resistor();
            resistor.Name = name;
            resistor.Node1 = circuit.FindOrCreateNode(node1);
            resistor.Node2 = circuit.FindOrCreateNode(node2);
            resistor.Resistance = value;
            circuit.Resistors.Add(resistor);
        }

        private void AddCapacitorToCircuit(CircuitSimulator.Circuit circuit, string name, string node1, string node2, double value)
        {
            var capacitor = new CircuitSimulator.Capacitor();
            capacitor.Name = name;
            capacitor.Node1 = circuit.FindOrCreateNode(node1);
            capacitor.Node2 = circuit.FindOrCreateNode(node2);
            capacitor.Capacitance = value;
            circuit.Capacitors.Add(capacitor);
        }

        private void AddInductorToCircuit(CircuitSimulator.Circuit circuit, string name, string node1, string node2, double value)
        {
            var inductor = new CircuitSimulator.Inductor();
            inductor.Name = name;
            inductor.Node1 = circuit.FindOrCreateNode(node1);
            inductor.Node2 = circuit.FindOrCreateNode(node2);
            inductor.Inductance = value;
            circuit.Inductors.Add(inductor);
        }

        private void AddVoltageSourceToCircuit(CircuitSimulator.Circuit circuit, string name, string node1, string node2, double value)
        {
            var voltageSource = new CircuitSimulator.VoltageSource();
            voltageSource.Name = name;
            voltageSource.Node1 = circuit.FindOrCreateNode(node1);
            voltageSource.Node2 = circuit.FindOrCreateNode(node2);
            voltageSource.Value = value;
            circuit.VoltageSources.Add(voltageSource);
        }

        private void AddACVoltageSourceToCircuit(CircuitSimulator.Circuit circuit, string name, string node1, string node2, double magnitude, double phase)
        {
            var acVoltageSource = new CircuitSimulator.ACVoltageSource();
            acVoltageSource.Name = name;
            acVoltageSource.Node1 = circuit.FindOrCreateNode(node1);
            acVoltageSource.Node2 = circuit.FindOrCreateNode(node2);
            acVoltageSource.Magnitude = magnitude;
            acVoltageSource.Phase = phase;
            circuit.AcVoltageSources.Add(acVoltageSource);
        }

        private string[] GetNodeNames(CircuitSimulator.Circuit circuit)
        {
            var nodeNames = new List<string>();
            foreach (var node in circuit.Nodes)
            {
                if (!node.IsGround)
                {
                    nodeNames.Add(node.Name);
                }
            }
            return nodeNames.ToArray();
        }
    }
}
