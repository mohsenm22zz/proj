using System;
using System.Windows;
using System.Windows.Controls;
using circuitUI.Simulation;

namespace circuitUI
{
    public partial class SimulationSettingsWindow : Window
    {
        public SimulationParameters Parameters { get; private set; }

        public SimulationSettingsWindow()
        {
            InitializeComponent();
            Parameters = new SimulationParameters();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DCOperatingPointTab.IsSelected)
                {
                    Parameters.CurrentAnalysis = SimulationParameters.AnalysisType.DCOperatingPoint;
                }
                else if (TransientTab.IsSelected)
                {
                    Parameters.CurrentAnalysis = SimulationParameters.AnalysisType.Transient;
                    Parameters.StopTime = double.Parse(StopTimeTextBox.Text);
                    Parameters.MaxTimestep = double.Parse(MaxTimestepTextBox.Text);
                }
                else if (AcSweepTab.IsSelected)
                {
                    Parameters.CurrentAnalysis = SimulationParameters.AnalysisType.ACSweep;
                    Parameters.SweepType = ((ComboBoxItem)SweepTypeComboBox.SelectedItem).Content.ToString();
                    Parameters.StartFrequency = double.Parse(StartFrequencyTextBox.Text);
                    Parameters.StopFrequency = double.Parse(StopFrequencyTextBox.Text);
                    Parameters.NumberOfPoints = int.Parse(NumPointsAcTextBox.Text);
                }
                else if (PhaseSweepTab.IsSelected)
                {
                    Parameters.CurrentAnalysis = SimulationParameters.AnalysisType.PhaseSweep;
                    Parameters.BaseFrequency = double.Parse(BaseFrequencyTextBox.Text);
                    Parameters.StartPhase = double.Parse(StartPhaseTextBox.Text);
                    Parameters.StopPhase = double.Parse(StopPhaseTextBox.Text);
                    Parameters.NumberOfPoints = int.Parse(NumPointsPhaseTextBox.Text);
                }

                DialogResult = true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Invalid input. Please check the values.\nError: {ex.Message}", "Input Error");
            }
        }
    }
}