using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    public class NullNodeIconShape : IconShape
    {
        const double radiusMultiplier = 30.0;
        const double radiusAddition = 4.0;
        const double maxOpacity = 0.8;
        public Boolean AttachedToHead { get; private set; }


        #region Constructor

        public NullNodeIconShape(Point pt, graphElement e, bool attachedToHead, GraphGUI gd)
            : base(e, gd, "", maxOpacity, radiusMultiplier, radiusAddition, null)
        {
            Center = pt;
            AttachedToHead = attachedToHead;

            var dt = (DataTemplate)Application.Current.Resources["NodeIconShapeNull"];
            var nullNodeIcon = (Shape)dt.LoadContent();

            Radius = defaultRadius = nullNodeIcon.Width / 2;
            defaultBrush = nullNodeIcon.Stroke;
            defaultDashStyle = new DashStyle(nullNodeIcon.StrokeDashArray, 0.0);
            defaultThickness = nullNodeIcon.StrokeThickness;
            Width = Height = 0.0;
        }
        #endregion


        /// <summary>
        ///   Called when [render].
        /// </summary>
        /// <param name = "dc">The dc.</param>
        protected override void OnRender(DrawingContext dc)
        {
            var scaleFactor = Math.Pow(ScaleFactor, (ScaleReduction - 1));
            var brush = defaultBrush.Clone();
            brush.Opacity = StrokeOpacity;
            var thickness = scaleFactor * defaultThickness;
            Radius = scaleFactor * defaultRadius;
            dc.DrawEllipse(Brushes.Transparent,
                new Pen { Brush = defaultBrush, Thickness = thickness, DashStyle = defaultDashStyle },
               Center, Radius, Radius);
            //SetValue(WidthProperty, 2 * Radius + thickness);
            //SetValue(HeightProperty, 2 * Radius + thickness);
            Panel.SetZIndex(this, int.MaxValue);            
            DriftingAnimation();
        }


        private void DriftingAnimation()
        {
            var driftAnimate = (Storyboard)FindResource("DiagonalDrift");
            driftAnimate.Begin(this);
        }
    }
}