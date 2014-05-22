using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageComparer.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //http://www.codeproject.com/Articles/97871/WPF-simple-zoom-and-drag-support-in-a-ScrollViewer
        Point? lastCenterPositionOnTarget;
        Point? lastMousePositionOnTarget;

        public MainWindow()
        {
            InitializeComponent();
        }
        void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (scaleTransform1 == null) return;
            scaleTransform1.ScaleX = e.NewValue;
            scaleTransform1.ScaleY = e.NewValue;
            scaleTransform2.ScaleX = e.NewValue;
            scaleTransform2.ScaleY = e.NewValue;

            var centerOfViewport = new Point(sv1.ViewportWidth / 2, sv1.ViewportHeight / 2);
            lastCenterPositionOnTarget = sv1.TranslatePoint(centerOfViewport, grid1);
        }

        void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            set(e, sv1, grid1);
            set(e, sv2, grid2);

            //http://stackoverflow.com/questions/15151974/synchronized-scrolling-of-two-scrollviewers-whenever-any-one-is-scrolled-in-wpf
            if (sender == sv1)
            {
                sv2.ScrollToVerticalOffset(e.VerticalOffset);
                sv2.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
            else
            {
                sv1.ScrollToVerticalOffset(e.VerticalOffset);
                sv1.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        }
        

        private void set(ScrollChangedEventArgs e, ScrollViewer sv, Grid grid)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(sv.ViewportWidth / 2, sv.ViewportHeight / 2);
                        Point centerOfTargetNow = sv.TranslatePoint(centerOfViewport, grid);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(grid);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / grid.Width;
                    double multiplicatorY = e.ExtentHeight / grid.Height;

                    double newOffsetX = sv.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                    double newOffsetY = sv.VerticalOffset - dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    sv.ScrollToHorizontalOffset(newOffsetX);
                    sv.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }
        
    }
}
