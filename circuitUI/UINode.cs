using System.Windows;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace wpfUI
{
    public class UINode
    {
        public Point Position { get; set; }
        
        public Shape Visual { get; set; }
        
        public List<UIElement> Connections { get; private set; }

        public UINode(Point position)
        {
            Position = position;
            Connections = new List<UIElement>();
            Visual = new Rectangle
            {
                Width = 6,
                Height = 6,
                Fill = System.Windows.Media.Brushes.Black,
                Margin = new Thickness(-3, -3, 0, 0)
            };
        }
    }
}