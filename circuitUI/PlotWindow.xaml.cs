using System;
using System.Linq;
using System.Windows;
using ScottPlot;

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
                    if (item.StartsWith("I("))
                    {
                        string componentName = item.Substring(2, item.Length - 3);
                        Tuple<double[], double[]> history = simulator.GetComponentCurrentHistory(componentName);
                        if (history?.Item1 != null && history.Item1.Length > 1)
                        {
                            // Use Add.Scatter for X-Y data pairs (Time, Current)
                            var scatter = WpfPlot1.Plot.Add.Scatter(history.Item1, history.Item2);
                            scatter.Label = $"I({componentName})";
                            hasData = true;
                        }
                    }
                    else // It's a voltage plot
                    {
                        Tuple<double[], double[]> history = simulator.GetNodeVoltageHistory(item);
                        if (history?.Item1 != null && history.Item1.Length > 1)
                        {
                            // Also use Add.Scatter for X-Y data pairs (Time, Voltage)
                            var scatter = WpfPlot1.Plot.Add.Scatter(history.Item1, history.Item2);
                            scatter.Label = $"V({item})";
                            hasData = true;
                        }
                    }
                }
            }
            
            WpfPlot1.Plot.Title("Transient Analysis Results");
            WpfPlot1.Plot.XLabel("Time (s)");
            WpfPlot1.Plot.YLabel("Voltage (V) / Current (A)");
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
    }
}
