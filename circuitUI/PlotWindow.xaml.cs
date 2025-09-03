using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ScottPlot;
using CircuitSimulator;

namespace circuitUI
{
    public partial class PlotWindow : Window
    {
        public PlotWindow()
        {
            InitializeComponent();
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
                    var node = circuit.Nodes.FirstOrDefault(n => n.Name == item);
                    if (node != null && node.VoltageHistory.Count > 1)
                    {
                        // Extract time and voltage data from the history
                        double[] times = node.VoltageHistory.Keys.ToArray();
                        double[] voltages = node.VoltageHistory.Values.ToArray();
                        
                        // Use Add.Signal for transient, which is efficient for evenly spaced time data
                        var sig = WpfPlot1.Plot.Add.Signal(voltages);
                        sig.LegendText = $"V({item})";
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
                    var node = circuit.Nodes.FirstOrDefault(n => n.Name == nodeName);
                    if (node != null && node.PhasorHistory.Count > 0)
                    {
                        // Extract frequency and magnitude data from the history
                        double[] frequencies = node.PhasorHistory.Keys.ToArray();
                        double[] magnitudes = node.PhasorHistory.Values.Select(v => System.Numerics.Complex.Abs(v)).ToArray();
                        
                        if (frequencies.Length > 0)
                        {
                            var scatter = WpfPlot1.Plot.Add.Scatter(frequencies, magnitudes);
                            scatter.LegendText = $"V({nodeName})";
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
            WpfPlot1.Plot.Axes.Bottom.Scale = ScottPlot.Scale.Log10;
            
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
                    var node = circuit.Nodes.FirstOrDefault(n => n.Name == nodeName);
                    if (node != null && node.PhasorHistory.Count > 0)
                    {
                        // Extract phase data from the history
                        double[] frequencies = node.PhasorHistory.Keys.ToArray();
                        double[] phases = node.PhasorHistory.Values.Select(v => 
                            Math.Atan2(v.Imaginary, v.Real) * 180 / Math.PI).ToArray();
                        double[] magnitudes = node.PhasorHistory.Values.Select(v => System.Numerics.Complex.Abs(v)).ToArray();
                        
                        if (phases.Length > 0)
                        {
                            var scatter = WpfPlot1.Plot.Add.Scatter(phases, magnitudes);
                            scatter.LegendText = $"V({nodeName})";
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
    }
}