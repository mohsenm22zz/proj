using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ScottPlot; // Required for AxisScale enum in ScottPlot 5

namespace wpfUI
{
    public partial class PlotWindow : Window
    {
        public PlotWindow()
        {
            InitializeComponent();
        }

        public void LoadTransientData(CircuitSimulatorService simulator, string[] itemsToPlot)
        {
            WpfPlot1.Plot.Clear();
            bool hasData = false;

            if (itemsToPlot != null && itemsToPlot.Any())
            {
                foreach (var item in itemsToPlot)
                {
                    if (item.StartsWith("I(")) continue; // Skip currents for now

                    Tuple<double[], double[]> history = simulator.GetNodeVoltageHistory(item);
                    if (history?.Item1 != null && history.Item1.Length > 1)
                    {
                        // Use Add.Signal for transient, which is efficient for evenly spaced time data
                        var sig = WpfPlot1.Plot.Add.Signal(history.Item2);
                        sig.Label = $"V({item})";
                        hasData = true;
                    }
                }
            }
            
            WpfPlot1.Plot.Title("Transient Analysis Results");
            WpfPlot1.Plot.XLabel("Time (s)");
            WpfPlot1.Plot.YLabel("Voltage (V)");
            if (hasData)
            {
                WpfPlot1.Plot.ShowLegend();
            }
            
            WpfPlot1.Refresh();
        }

        public void LoadTransientDataFromCS(CircuitSimulator.Circuit circuit, string[] itemsToPlot)
        {
            WpfPlot1.Plot.Clear();
            bool hasData = false;

            if (itemsToPlot != null && itemsToPlot.Any())
            {
                foreach (var item in itemsToPlot)
                {
                    if (item.StartsWith("I(")) continue; // Skip currents for now

                    // Find the node in our C# circuit
                    var node = circuit.FindNode(item);
                    if (node != null && node.VoltageHistory.Count > 1)
                    {
                        // Extract time and voltage data from the history
                        double[] times = node.VoltageHistory.Select(p => p.Item1).ToArray();
                        double[] voltages = node.VoltageHistory.Select(p => p.Item2).ToArray();
                        
                        // Use Add.Signal for transient, which is efficient for evenly spaced time data
                        var sig = WpfPlot1.Plot.Add.Signal(voltages);
                        sig.Label = $"V({item})";
                        hasData = true;
                    }
                }
            }
            
            WpfPlot1.Plot.Title("Transient Analysis Results");
            WpfPlot1.Plot.XLabel("Time (s)");
            WpfPlot1.Plot.YLabel("Voltage (V)");
            if (hasData)
            {
                WpfPlot1.Plot.ShowLegend();
            }
            
            WpfPlot1.Refresh();
        }

        public void LoadACData(CircuitSimulatorService simulator, string[] nodesToPlot)
        {
            WpfPlot1.Plot.Clear();
            bool hasData = false;

            if (nodesToPlot != null && nodesToPlot.Any())
            {
                foreach (var nodeName in nodesToPlot)
                {
                    if (nodeName.StartsWith("I(")) continue;

                    Tuple<double[], double[]> history = simulator.GetNodeSweepHistory(nodeName);
                    if (history?.Item1 != null && history.Item1.Length > 0)
                    {
                        var scatter = WpfPlot1.Plot.Add.Scatter(history.Item1, history.Item2);
                        scatter.Label = $"V({nodeName})";
                        hasData = true;
                    }
                }
            }

            WpfPlot1.Plot.Title("AC Sweep Results");
            WpfPlot1.Plot.XLabel("Frequency (Hz)");
            WpfPlot1.Plot.YLabel("Magnitude (V)");
            
            if (hasData)
            {
                WpfPlot1.Plot.ShowLegend();
            }

            WpfPlot1.Refresh();
        }

        public void LoadACDataFromCS(CircuitSimulator.Circuit circuit, string[] nodesToPlot, double startFreq, double stopFreq, int numPoints, string sweepType)
        {
            WpfPlot1.Plot.Clear();
            bool hasData = false;

            if (nodesToPlot != null && nodesToPlot.Any())
            {
                foreach (var nodeName in nodesToPlot)
                {
                    if (nodeName.StartsWith("I(")) continue;

                    // Find the node in our C# circuit
                    var node = circuit.FindNode(nodeName);
                    if (node != null)
                    {
                        // Generate frequency points for plotting
                        var frequencies = new List<double>();
                        var magnitudes = new List<double>();
                        
                        for (int i = 0; i < numPoints; ++i)
                        {
                            double currentFreq;
                            if (numPoints == 1)
                            {
                                currentFreq = startFreq;
                            }
                            else if (sweepType == "Linear")
                            {
                                currentFreq = startFreq + i * (stopFreq - startFreq) / (numPoints - 1);
                            }
                            else
                            { // Logarithmic (Decade)
                                currentFreq = startFreq * Math.Pow(10.0, i / (double)(numPoints - 1) * Math.Log10(stopFreq / startFreq));
                            }

                            if (currentFreq <= 0) continue;
                            
                            // In a real implementation, we would retrieve the actual magnitude data
                            // For now, we'll generate some sample data
                            frequencies.Add(currentFreq);
                            // This is a placeholder - in a real implementation we would get the actual magnitude
                            magnitudes.Add(5.0 * Math.Exp(-currentFreq / 10000)); // Exponential decay sample
                        }
                        
                        if (frequencies.Count > 0)
                        {
                            var scatter = WpfPlot1.Plot.Add.Scatter(frequencies.ToArray(), magnitudes.ToArray());
                            scatter.Label = $"V({nodeName})";
                            scatter.MarkerSize = 5;
                            hasData = true;
                        }
                    }
                }
            }

            WpfPlot1.Plot.Title("AC Sweep Results");
            WpfPlot1.Plot.XLabel("Frequency (Hz)");
            WpfPlot1.Plot.YLabel("Magnitude (V)");
            // Set logarithmic scale for frequency axis
            //WpfPlot1.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.Logarithmic();
            
            if (hasData)
            {
                WpfPlot1.Plot.ShowLegend();
            }

            WpfPlot1.Refresh();
        }

        public void LoadPhaseData(CircuitSimulatorService simulator, string[] nodesToPlot)
        {
            WpfPlot1.Plot.Clear();
            bool hasData = false;
            
            if (nodesToPlot != null && nodesToPlot.Any())
            {
                foreach (var nodeName in nodesToPlot)
                {
                    if (nodeName.StartsWith("I(")) continue;
                    
                    Tuple<double[], double[]> history = simulator.GetNodePhaseSweepHistory(nodeName);
                    if (history?.Item1 != null && history.Item1.Length > 0)
                    {
                        var scatter = WpfPlot1.Plot.Add.Scatter(history.Item1, history.Item2);
                        scatter.Label = $"V({nodeName})";
                        hasData = true;
                    }
                }
            }

            WpfPlot1.Plot.Title("Phase Sweep Results");
            WpfPlot1.Plot.XLabel("Phase (degrees)");
            WpfPlot1.Plot.YLabel("Magnitude (V)");
            if (hasData)
            {
                WpfPlot1.Plot.ShowLegend();
            }

            WpfPlot1.Refresh();
        }

        public void LoadPhaseDataFromCS(CircuitSimulator.Circuit circuit, string[] nodesToPlot, double baseFreq, double startPhase, double stopPhase, int numPoints)
        {
            WpfPlot1.Plot.Clear();
            bool hasData = false;
            
            if (nodesToPlot != null && nodesToPlot.Any())
            {
                foreach (var nodeName in nodesToPlot)
                {
                    if (nodeName.StartsWith("I(")) continue;

                    // Find the node in our C# circuit
                    var node = circuit.FindNode(nodeName);
                    if (node != null)
                    {
                        // Generate phase points for plotting
                        var phases = new List<double>();
                        var magnitudes = new List<double>();
                        
                        for (int i = 0; i < numPoints; ++i)
                        {
                            double currentPhase = (numPoints == 1) ? startPhase : startPhase + i * (stopPhase - startPhase) / (numPoints - 1);
                            
                            phases.Add(currentPhase);
                            // This is a placeholder - in a real implementation we would get the actual magnitude
                            magnitudes.Add(5.0 + 2.0 * Math.Sin(currentPhase * Math.PI / 180)); // Sine wave sample
                        }
                        
                        if (phases.Count > 0)
                        {
                            var scatter = WpfPlot1.Plot.Add.Scatter(phases.ToArray(), magnitudes.ToArray());
                            scatter.Label = $"V({nodeName})";
                            scatter.MarkerSize = 5;
                            hasData = true;
                        }
                    }
                }
            }

            WpfPlot1.Plot.Title("Phase Sweep Results");
            WpfPlot1.Plot.XLabel("Phase (degrees)");
            WpfPlot1.Plot.YLabel("Magnitude (V)");
            
            if (hasData)
            {
                WpfPlot1.Plot.ShowLegend();
            }

            WpfPlot1.Refresh();
        }

        public void LoadDCSweepDataFromCS(Dictionary<string, Tuple<double[], double[]>> dcData)
        {
            WpfPlot1.Plot.Clear();
            bool hasData = false;

            foreach (var kvp in dcData)
            {
                var sweepValues = kvp.Value.Item1;
                var nodeVoltages = kvp.Value.Item2;
                
                if (sweepValues.Length > 0)
                {
                    var scatter = WpfPlot1.Plot.Add.Scatter(sweepValues, nodeVoltages);
                    scatter.Label = kvp.Key;
                    scatter.MarkerSize = 5;
                    hasData = true;
                }
            }

            WpfPlot1.Plot.Title("DC Sweep Results");
            WpfPlot1.Plot.XLabel("Sweep Value");
            WpfPlot1.Plot.YLabel("Node Voltage (V)");
            
            if (hasData)
            {
                WpfPlot1.Plot.ShowLegend();
            }

            WpfPlot1.Refresh();
        }
    }
}