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
            lastCenterPositionOnTarget = sv1.TranslatePoint(centerOfViewport, canvas1);
        }

        void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            set(e, sv1, canvas1);
            set(e, sv2, canvas2);

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
        

        private void set(ScrollChangedEventArgs e, ScrollViewer sv, Canvas canvas)
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
                        Point centerOfTargetNow = sv.TranslatePoint(centerOfViewport, canvas);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(canvas);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / canvas.Width;
                    double multiplicatorY = e.ExtentHeight / canvas.Height;

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

        //http://stackoverflow.com/questions/294220/dragging-an-image-in-wpf
        // Determine if we're presently dragging
        private static bool _isDragging = false;

        // The offset from the top, left of the item being dragged 
        // and the original mouse down
        private static Point _offset1;
        private static Point _offset2;

        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {

            // start dragging and get the offset of the mouse 
            // relative to the element
            _isDragging = true;
            _offset1 = e.GetPosition(image1);
            _offset2 = e.GetPosition(image2);
            this.Cursor = Cursors.Hand;
        }

        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            // If we're not dragging, don't bother - also validate the element
            if (!_isDragging) return;

            moveImage(image1, e, _offset1);
            moveImage(image2, e, _offset2);
        }

        private static void moveImage(Image image, MouseEventArgs e, Point offset)
        {
            var canvas = image.Parent as Canvas;
            if (canvas == null) return;

            // Get the position of the mouse relative to the canvas
            var mousePoint = e.GetPosition(canvas);

            // Offset the mouse position by the original offset position
            mousePoint.Offset(-offset.X, -offset.Y);

            // Move the element on the canvas
            image.SetValue(Canvas.LeftProperty, mousePoint.X);
            image.SetValue(Canvas.TopProperty, mousePoint.Y);
        }

        private void image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            this.Cursor = Cursors.Arrow;
        }

        private void sv_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var sv = sender as ScrollViewer;
            if (sv == null) return;

            var canvas = sv.Content as Canvas;
            if (canvas == null) return;

            lastMousePositionOnTarget = Mouse.GetPosition(canvas);

            if (e.Delta > 0)
            {
                slider1.Value += 0.2;
            }
            if (e.Delta < 0)
            {
                slider1.Value -= 0.2;
            }

            e.Handled = true;
        }
    }
}
