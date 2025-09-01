using System.Collections.Generic;
using System.Windows;

namespace wpfUI
{
    /// <summary>
    /// Interaction logic for NetlistWindow.xaml
    /// </summary>
    public partial class NetlistWindow : Window
    {
        public NetlistWindow(List<string> netlistCommands)
        {
            InitializeComponent();
            // Join the list of netlist command strings into a single string,
            // with each command on a new line, and display it in the TextBox.
            NetlistTextBox.Text = string.Join("\r\n", netlistCommands);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the window when the OK button is clicked.
            this.Close();
        }
    }
}