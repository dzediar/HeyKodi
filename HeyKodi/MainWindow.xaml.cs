using HeyKodi.Model;
using HeyKodi.ViewModels;
using HeyKodi.Views;
using KodiRPC.RPC.RequestResponse.Params.VideoLibrary;
using KodiRPC.RPC.Specifications;
using KodiRPC.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
        private Point balloonOrigin;

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
                    this.Left = balloonOrigin.X + deltaX;
                    this.Top = balloonOrigin.Y + deltaY;
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            Point pos = e.GetPosition(this);
            balloonOrigin = new Point(this.Left, this.Top);
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
