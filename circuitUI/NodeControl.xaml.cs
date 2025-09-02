using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace wpfUI
{
    public partial class NodeControl : UserControl
    {
        public bool HasBeenPlaced { get; set; } = false;
        public static readonly DependencyProperty IsGroundProperty =
            DependencyProperty.Register("IsGround", typeof(bool), typeof(NodeControl), new PropertyMetadata(false, OnIsGroundChanged));

        public bool IsGround
        {
            get { return (bool)GetValue(IsGroundProperty); }
            set { SetValue(IsGroundProperty, value); }
        }

        private static void OnIsGroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as NodeControl;
            control?.UpdateVisualState();
        }

        public NodeControl()
        {
            InitializeComponent();
            UpdateVisualState();
        }
        private void UpdateVisualState()
        {
            if (IsGround)
            {
                NodeRectangle.Fill = Brushes.Gray;
            }
            else
            {
                NodeRectangle.Fill = Brushes.White;
            }
        }

        private Point startPoint;
        private bool isDragging = false;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (!HasBeenPlaced && Parent is Canvas)
            {
                startPoint = e.GetPosition(Parent as IInputElement);
                this.CaptureMouse();
                isDragging = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isDragging)
            {
                Point currentPoint = e.GetPosition(Parent as IInputElement);
                double newX = Canvas.GetLeft(this) + (currentPoint.X - startPoint.X);
                double newY = Canvas.GetTop(this) + (currentPoint.Y - startPoint.Y);

                Canvas.SetLeft(this, newX);
                Canvas.SetTop(this, newY);

                startPoint = currentPoint;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (isDragging)
            {
                this.ReleaseMouseCapture();
                isDragging = false;

                double gridSize = 20.0;
                Point currentPosition = new Point(Canvas.GetLeft(this) + this.Width / 2, Canvas.GetTop(this) + this.Height / 2);
                
                double snappedX = Math.Round(currentPosition.X / gridSize) * gridSize;
                double snappedY = Math.Round(currentPosition.Y / gridSize) * gridSize;

                Canvas.SetLeft(this, snappedX - this.Width / 2);
                Canvas.SetTop(this, snappedY - this.Height / 2);

                HasBeenPlaced = true;
            }
        }
    }
}
