using System;
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

