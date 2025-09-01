using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace wpfUI
{
    public partial class ComponentControl : UserControl
    {
        public static readonly DependencyProperty ComponentNameProperty =
            DependencyProperty.Register("ComponentName", typeof(string), typeof(ComponentControl), new PropertyMetadata("Cmp", OnComponentNameChanged));

        public string ComponentName
        {
            get { return (string)GetValue(ComponentNameProperty); }
            set { SetValue(ComponentNameProperty, value); }
        }

        public double Value { get; set; } = 1;
        public double AcPhase { get; set; } = 0;

        private static void OnComponentNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ComponentControl;
            if (control != null)
            {
                control.ComponentNameLabel.Text = e.NewValue.ToString();
                string name = e.NewValue.ToString();
                if (name.StartsWith("R")) control.Value = 1000;
                if (name.StartsWith("V")) control.Value = 5;
                if (name.StartsWith("C")) control.Value = 0.000001;
                if (name.StartsWith("L")) control.Value = 0.001;
                if (name.StartsWith("ACV")) control.Value = 1;
            }
        }

        public bool HasBeenPlaced { get; set; } = false;

        public ComponentControl()
        {
            InitializeComponent();
            this.MouseRightButtonUp += Component_MouseRightButtonUp;
        }

        private void Component_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem { Header = "Set Value..." };
            menuItem.Click += SetValue_Click;
            contextMenu.Items.Add(menuItem);
            contextMenu.IsOpen = true;
        }

        private void SetValue_Click(object sender, RoutedEventArgs e)
        {
            var valueWindow = new ComponentValueWindow(this.ComponentName, this.Value, this.AcPhase);
            valueWindow.Owner = Application.Current.MainWindow;
            if (valueWindow.ShowDialog() == true)
            {
                this.Value = valueWindow.ComponentValue;
                this.AcPhase = valueWindow.AcPhase;
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
                double currentLeft = Canvas.GetLeft(this);
                double currentTop = Canvas.GetTop(this);
                
                double centerX = currentLeft + this.Width / 2;
                double centerY = currentTop + this.Height / 2;

                double snappedCenterX = Math.Round(centerX / gridSize) * gridSize;
                double snappedCenterY = Math.Round(centerY / gridSize) * gridSize;
                
                Canvas.SetLeft(this, snappedCenterX - this.Width / 2);
                Canvas.SetTop(this, snappedCenterY - this.Height / 2);

                HasBeenPlaced = true;
            }
        }
    }
}