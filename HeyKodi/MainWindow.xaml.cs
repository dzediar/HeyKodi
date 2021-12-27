using System.Windows;
using System.Windows.Input;
using zComp.Wpf;

namespace HeyKodi
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : zSmoothWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Point thumbOrigin;
        private Point windowOrigin;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (this.IsMouseCaptured)
            {
                Point loc = this.PointToScreen(e.GetPosition(this));
                double deltaX = loc.X - thumbOrigin.X;
                double deltaY = loc.Y - thumbOrigin.Y;
                if (deltaX != 0 || deltaY != 0)
                {
                    this.Left = windowOrigin.X + deltaX;
                    this.Top = windowOrigin.Y + deltaY;
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            Point pos = e.GetPosition(this);
            windowOrigin = new Point(this.Left, this.Top);
            thumbOrigin = this.PointToScreen(pos);
            thumbOrigin = this.PointToScreen(pos);
            this.CaptureMouse();
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (this.IsMouseCaptured)
            {
                this.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
        }
    }
}
