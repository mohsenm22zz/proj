using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace wpfUI
{
    public class Probe : UserControl
    {
        public Probe()
        {
            var grid = new Grid();
            var circle = new Ellipse
            {
                Width = 20,
                Height = 20,
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };
            var line1 = new Line
            {
                X1 = -5, Y1 = 0, X2 = 5, Y2 = 0,
                Stroke = Brushes.Red,
                StrokeThickness = 1
            };
            var line2 = new Line
            {
                X1 = 0, Y1 = -5, X2 = 0, Y2 = 5,
                Stroke = Brushes.Red,
                StrokeThickness = 1
            };

            grid.Children.Add(circle);
            grid.Children.Add(line1);
            grid.Children.Add(line2);

            Content = grid;
            IsHitTestVisible = false;
        }
    }
}