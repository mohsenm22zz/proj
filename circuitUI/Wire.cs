using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace wpfUI
{
    public class Wire : UserControl
    {
        private readonly Path _wirePath;
        private readonly PathGeometry _pathGeometry;
        private readonly PathFigure _pathFigure;
        private readonly PolyLineSegment _polyLineSegment;

        public Point StartPoint
        {
            get => _pathFigure.StartPoint;
            set => _pathFigure.StartPoint = value;
        }

        public Point EndPoint
        {
            get => _polyLineSegment.Points.Count > 0 ? _polyLineSegment.Points[_polyLineSegment.Points.Count - 1] : StartPoint;
            set
            {
                // When EndPoint is set, finalize the wire shape
                Point elbow = GetElbowPoint(StartPoint, value);
                _polyLineSegment.Points.Clear();
                _polyLineSegment.Points.Add(elbow);
                _polyLineSegment.Points.Add(value);
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                UpdateVisualState();
            }
        }

        public Wire()
        {
            _polyLineSegment = new PolyLineSegment();
            _pathFigure = new PathFigure { IsClosed = false };
            _pathFigure.Segments.Add(_polyLineSegment);
            _pathGeometry = new PathGeometry();
            _pathGeometry.Figures.Add(_pathFigure);
            _wirePath = new Path
            {
                Data = _pathGeometry,
                StrokeThickness = 2,
                Stroke = Brushes.DarkCyan
            };

            Content = _wirePath;
            Panel.SetZIndex(this, -1); // Wires should be behind components
        }
        
        /// <summary>
        /// This is for loading pre-defined wires (like in the default circuit).
        /// </summary>
        public void AddPoint(Point newPoint)
        {
            _polyLineSegment.Points.Add(newPoint);
        }

        /// <summary>
        /// Updates the wire's preview path as the user moves the mouse.
        /// It creates a simple orthogonal line with one bend.
        /// </summary>
        public void UpdatePreview(Point previewPoint)
        {
            Point elbow = GetElbowPoint(StartPoint, previewPoint);

            _polyLineSegment.Points.Clear();
            _polyLineSegment.Points.Add(elbow);
            _polyLineSegment.Points.Add(previewPoint);
        }

        /// <summary>
        /// Calculates the "elbow" point for an orthogonal line.
        /// Defaults to a horizontal-then-vertical break.
        /// </summary>
        private Point GetElbowPoint(Point start, Point end)
        {
            return new Point(end.X, start.Y);
        }

        private void UpdateVisualState()
        {
            _wirePath.Stroke = IsSelected ? Brushes.Yellow : Brushes.DarkCyan;
            _wirePath.StrokeThickness = IsSelected ? 4 : 2;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            IsSelected = !IsSelected;
            e.Handled = true; // Prevent the canvas from handling this click
        }
    }
}
