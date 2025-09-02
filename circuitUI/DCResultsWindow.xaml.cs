using System.Collections.Generic;
using System.Windows;

namespace wpfUI
{
    public partial class DCResultsWindow : Window
    {
        public DCResultsWindow(List<string> nodeVoltages, List<string> componentCurrents)
        {
            InitializeComponent();
            var lines = new List<string>();
            lines.Add("Node Voltages:");
            lines.AddRange(nodeVoltages);
            lines.Add("");
            lines.Add("Component Currents:");
            lines.AddRange(componentCurrents);
            ResultsTextBox.Text = string.Join("\r\n", lines);
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}