using System.Windows;

namespace wpfUI
{
    public static class OkDialog
    {
        public static void Show(string message, string title = "OK")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
