using System.Collections.Generic;
using System.Windows;

namespace wpfUI
{
    public partial class DCResultsWindow : Window
    {
        public DCResultsWindow(List<string> results)
        {
            InitializeComponent();
            ResultsListBox.ItemsSource = results;
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}