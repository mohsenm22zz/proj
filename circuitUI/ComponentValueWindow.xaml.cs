using System.Windows;

namespace wpfUI
{
    public partial class ComponentValueWindow : Window
    {
        public double ComponentValue { get; private set; }
        public double AcPhase { get; private set; }

        public ComponentValueWindow(string componentName, double currentValue, double currentPhase)
        {
            InitializeComponent();

            if (componentName.StartsWith("ACV"))
            {
                GenericPanel.Visibility = Visibility.Collapsed;
                AcVoltagePanel.Visibility = Visibility.Visible;
                AcMagnitudeTextBox.Text = currentValue.ToString();
                AcPhaseTextBox.Text = currentPhase.ToString();
            }
            else
            {
                ValueTextBox.Text = currentValue.ToString();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (AcVoltagePanel.Visibility == Visibility.Visible)
            {
                double.TryParse(AcMagnitudeTextBox.Text, out double magnitude);
                double.TryParse(AcPhaseTextBox.Text, out double phase);
                ComponentValue = magnitude;
                AcPhase = phase;
            }
            else
            {
                double.TryParse(ValueTextBox.Text, out double value);
                ComponentValue = value;
            }
            DialogResult = true;
        }
    }
}