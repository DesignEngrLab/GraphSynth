using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GraphSynth.GraphDisplay
{
    public partial class GraphGUI : InkCanvas
    {

        #region Redraw, Resize, and Reposition

        public void RedrawResizeAndReposition(Boolean recalc = false, double oldScale = double.NaN)
        {
            if (double.IsNaN(oldScale)) oldScale = ScaleFactor;
            if (recalc) recalculateBoundingBox();
            try
            {
                /* first determine the boundingBox of the shapes. NB: this adds a buffer (= bufferRadius) s.t.
                 * there is a margin around the shapes. */
                // SearchIO.output("Redraw", 2);
                if (ZoomToFit)
                {
                    /* if we are zoom to Fit, we define a new ScaleFactor. Why is _zoomToFit cycled here? Because
                     * ScaleFactor re-invokes this function (n little recursion). So in the second subpass we turn off
                     * ZoomToFit, and then turn it back on when it gets back here. */
                    _zoomToFit = false;
                    var zoomFactor = Math.Min((ScrollOwner.ActualWidth / boundingBox.Width),
                                           (ScrollOwner.ActualHeight / boundingBox.Height));
                    if (double.IsNaN(zoomFactor)) ScaleFactor = 1.0;
                    else
                    {
                        ScaleFactor = zoomFactor;
                        _zoomToFit = true;
                    }
                }
                else
                {
                    #region Adjust Axis Position

                    /* this if clause catches whether the bounding box is lower than 
                 * before - which would require us to move the Axes. */
                    if ((boundingBox.Top != 0.0) || (boundingBox.Left != 0.0))
                    {
                        Origin = new Point(Origin.X - boundingBox.Left,
                                           Origin.Y - boundingBox.Top);
                        foreach (var nShape in nodeShapes)
                        {
                            var u = (Shape)nShape;
                            var x = u.RenderTransform.Value.OffsetX;
                            var y = u.RenderTransform.Value.OffsetY;
                            u.RenderTransform = new MatrixTransform(
                                u.RenderTransform.Value.M11,
                                u.RenderTransform.Value.M12,
                                u.RenderTransform.Value.M21,
                                u.RenderTransform.Value.M22,
                                x - boundingBox.Left,
                                y - boundingBox.Top);
                        }
                        recalculateBoundingBox();
                    }

                    #endregion

                    #region Adjust graph display size

                    /* Argh! This little bit of code took me the whole winter break of 
                 * 2008-09. Is it right? The way it is now seems to solve the problem.
                 * The graphGUI is wrapped into a Grid which is then wrapped in a 
                 * scrollviewer. The problem seems to be scroll viewer's extend size. With 
                 * the existing code we prevent this from getting too large. */
                    //((Panel)Parent).Width = Math.Max(ScrollOwner.ActualWidth, ScaleFactor * boundingBox.Width);
                    //((Panel)Parent).Height = Math.Max(ScrollOwner.ActualHeight, ScaleFactor * boundingBox.Height);
                    Width = Math.Max(ScrollOwner.ActualWidth, ScaleFactor * boundingBox.Width);
                    Height = Math.Max(ScrollOwner.ActualHeight, ScaleFactor * boundingBox.Height);
                    gridAndAxes.WindowWidth =
                        MinWidth = Math.Max(ScrollOwner.ActualWidth / ScaleFactor, boundingBox.Width);
                    gridAndAxes.WindowHeight =
                        MinHeight = Math.Max(ScrollOwner.ActualHeight / ScaleFactor, boundingBox.Height);

                    #endregion

                    #region Scale graph display based on zoom setting, ScaleFactor

                    /* Scale the canvas accordingly. */
                    RenderTransform = new MatrixTransform(ScaleFactor, 0, 0, -ScaleFactor, 0, ((Panel)Parent).Height);

                    #endregion

                    #region Set Scrollbars at logical position

                    /* if a center point is defined move the the sliders to keep the center
                 * in the center of the screen. */
                    //Point center = new Point(ScrollOwner.ViewportWidth / 2, ScrollOwner.ViewportHeight / 2);
                    var center = new Point(ScrollOwner.ActualWidth / 2, ScrollOwner.ActualHeight / 2);
                    var mouseWRTScroll = Mouse.GetPosition(ScrollOwner);
                    var delta = new Point((mouseWRTScroll.X - center.X) / 3, (mouseWRTScroll.Y - center.Y) / 3);
                    center.X = mouseWRTScroll.X + Math.Sign(ScaleFactor - oldScale) * delta.X;
                    center.Y = mouseWRTScroll.Y + Math.Sign(ScaleFactor - oldScale) * delta.Y;
                    /* else we center the sliders about the current position. */
                    ScrollOwner.ScrollToHorizontalOffset((ScaleFactor * ScrollOwner.HorizontalOffset / oldScale)
                                                         + ((1 - oldScale / ScaleFactor) * center.X));
                    ScrollOwner.ScrollToVerticalOffset((ScaleFactor * ScrollOwner.VerticalOffset / oldScale)
                                                       + ((1 - oldScale / ScaleFactor) * center.Y));

                    #endregion
                }
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        #endregion

        public ScrollViewer ScrollOwner { get; set; }

        public Point Origin
        {
            get { return gridAndAxes.Origin; }
            private set { gridAndAxes.Origin = value; }
        }

        public new double Height
        {
            get { return base.Height; }
            set { ((Panel)Parent).Height = base.Height = value; }
        }

        public new double Width
        {
            get { return base.Width; }
            set { ((Panel)Parent).Width = base.Width = value; }
        }

        #region Zoom limit constants

        public const double minZoom = 0.01; // 1% 
        public const double maxZoom = 10; //1000%

        #endregion

        #region Zooming

        public static readonly DependencyProperty ScaleFactorProperty
            = DependencyProperty.Register("ScaleFactor",
                                          typeof(double), typeof(GraphGUI),
                                          new FrameworkPropertyMetadata(1.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        private Boolean _zoomToFit;

        public Boolean ZoomToFit
        {
            get { return _zoomToFit; }
            set
            {
                try
                {
                    if (!_zoomToFit && value)
                    {
                        /* if zoomToFit is false, but it is now being
                         * changed to true, then call the Redraw function */
                        _zoomToFit = true;
                        RedrawResizeAndReposition();
                    }
                    else _zoomToFit = value;
                    mainObject.SetCanvasPropertyScaleFactor(ScaleFactor, _zoomToFit);
                }
                catch (Exception exc)
                {
                    ErrorLogger.Catch(exc);
                }
            }
        }

        public double ScaleFactor
        {
            get { return (double)GetValue(ScaleFactorProperty); }
            set
            {
                try
                {
                    double newScale;
                    if (value <= minZoom) newScale = minZoom;
                    else if (value >= maxZoom) newScale = maxZoom;
                    else newScale = Math.Round(value, 4);
                    /* in order to prevent too many redraws of the screen
                     * we check to see if the value is actually a new one. */
                    if (ScaleFactor != newScale)
                    {
                        _zoomToFit = false;
                        var oldScale = ScaleFactor;
                        SetValue(ScaleFactorProperty, newScale);
                        RedrawResizeAndReposition(false, oldScale);
                        mainObject.SetCanvasPropertyScaleFactor(newScale, null);
                    }
                }
                catch (Exception exc)
                {
                    ErrorLogger.Catch(exc);
                }
            }
        }

        public void zoomIn()
        {
            ZoomToFit = false;
            ScaleFactor *= 1.05;
        }

        public void zoomOut()
        {
            ZoomToFit = false;
            ScaleFactor /= 1.05;
        }

        #endregion

        #region Bounding Box

        // defines a Rect about the coord center - used to determine
        // the screen bounds
        private Rect boundingBox;

        private void recalculateBoundingBox()
        {
            /* first define a square around the coordinate axis which should always be included
                     * in the bounding box. */
            var bbTemp = new Rect(Origin.X - defaultLength,
                                  Origin.Y - defaultLength,
                                  2 * defaultLength, 2 * defaultLength);
            bbTemp = (from Shape u in nodeShapes
                      select new Rect(u.RenderTransform.Value.OffsetX - defaultLength,
                                      u.RenderTransform.Value.OffsetY - defaultLength,
                                      u.RenderSize.Width + 2 * defaultLength,
                                      u.RenderSize.Height + 2 * defaultLength))
                .Aggregate(bbTemp, Rect.Union);
            if (!double.IsNaN(bbTemp.Top) && !double.IsNaN(bbTemp.Bottom)
                && !double.IsNaN(bbTemp.Left) && !double.IsNaN(bbTemp.Right))
                boundingBox = bbTemp;
        }

        #endregion

        private Point GoToNearestGridIntersection(Point p)
        {
            try
            {
                var gridLineOffset = p.X % gridAndAxes.GridSpacing;
                if (gridLineOffset > gridAndAxes.GridSpacing / 2)
                {
                    p.X = p.X + gridAndAxes.GridSpacing - gridLineOffset;
                }
                else
                    p.X = p.X - gridLineOffset;

                gridLineOffset = p.Y % gridAndAxes.GridSpacing;
                if (gridLineOffset > gridAndAxes.GridSpacing / 2)
                {
                    p.Y = p.Y + gridAndAxes.GridSpacing - gridLineOffset;
                }
                else
                    p.Y = p.Y - gridLineOffset;

                return p;
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
                return new Point();
            }
        }
    }
}