using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    public class HyperArcIconShape : IconShape
    {
        const double innerRadFactor = 0.7;
        const double radiusMultiplier = 30.0;
        const double radiusAddition = 4.0;
        const double maxOpacity = 1.0;
        public static readonly DependencyProperty NodeCentersProperty
            = DependencyProperty.Register("NodeCenters",
                                          typeof(PointCollection), typeof(HyperArcIconShape),
                                          new FrameworkPropertyMetadata(null,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));



        public PointCollection NodeCenters
        {
            get { return (PointCollection)GetValue(NodeCentersProperty); }
            set { SetValue(NodeCentersProperty, value); }
        }


        public HyperArcShape hyperArcShape { get; private set; }
        private readonly PathGeometry templateGeometry;
        private readonly double angleBtwPoints;
        private readonly Brush selectedBrush;
        private readonly double selectedBufferThickness;
        public readonly Effect selectedEffect;
        private readonly DashStyle selectedDashStyle;

        internal NullNodeIconShape newConnection { get; set; }

        #region Constructor
        public HyperArcIconShape(graphElement h, HyperArcShape hyperArcShape, GraphGUI gd)
            : base(h, gd, hyperArcShape.Tag, maxOpacity, radiusMultiplier, radiusAddition, hyperArcShape.Controller)
        {
            this.hyperArcShape = hyperArcShape;
            this.hyperArcShape.icon = this;
            this.Center = hyperArcShape.Center;

            var dt = (DataTemplate)Application.Current.Resources["HyperArcIconShape"];
            var templateShape = (Shape)dt.LoadContent();
            defaultBrush = templateShape.Stroke;
            defaultThickness = templateShape.StrokeThickness;
            defaultDashStyle = new DashStyle(templateShape.StrokeDashArray, 0.0);

            templateGeometry = (PathGeometry)((Path)templateShape).Data;
            var iconPts = new List<Point>(((PolyLineSegment)templateGeometry.Figures[0].Segments[0]).Points);
            iconPts.Insert(0, templateGeometry.Figures[0].StartPoint);
            var midpoint = new Point(iconPts.Average(n => n.X), iconPts.Average(n => n.Y));
            Radius = defaultRadius = (from p in iconPts select (new Point(p.X, p.Y) - midpoint).Length).Max();
            angleBtwPoints = 2 * Math.PI / iconPts.Count;

            dt = (DataTemplate)Application.Current.Resources["SelectedArcPen"];
            var penData = (Shape)dt.LoadContent();
            selectedBrush = penData.Stroke;
            selectedBufferThickness = penData.StrokeThickness;
            selectedDashStyle = new DashStyle(penData.StrokeDashArray, 0);
            selectedEffect = penData.Effect;

            var binding = new Binding
                             {
                                 Source = this,
                                 Converter = new SelectedHyperArcEffectConverter(selectedEffect),
                                 Mode = BindingMode.OneWay,
                                 Path = new PropertyPath(SelectedProperty)
                             };
            hyperArcShape.SetBinding(EffectProperty, binding);

            binding = new Binding
                         {
                             Source = hyperArcShape,
                             Mode = BindingMode.OneWay,
                             Path = new PropertyPath(HyperArcShape.NodeCentersProperty)
                         };
            SetBinding(NodeCentersProperty, binding);

        }

        #endregion


        /// <summary>
        ///   Called when [render].
        /// </summary>
        /// <param name = "dc">The dc.</param>
        protected override void OnRender(DrawingContext dc)
        {
            if (Selected || (StrokeOpacity >= opacityCutoff))
            {
                var scaleFactor = Math.Pow(ScaleFactor, (ScaleReduction - 1));
                Brush brush;
                Pen pen;
                if (Selected)
                    pen = new Pen
                    {
                        Brush = brush = selectedBrush,
                        Thickness = scaleFactor * selectedBufferThickness,
                        DashStyle = selectedDashStyle
                    };
                else
                {
                    brush = defaultBrush.Clone();
                    brush.Opacity = StrokeOpacity;
                    pen = new Pen
                    {
                        Brush = brush,
                        Thickness = scaleFactor * defaultThickness,
                        DashStyle = defaultDashStyle
                    };
                }
                Radius = scaleFactor * defaultRadius;
                if ((NodeCenters == null) || (NodeCenters.Count == 0))
                {
                    dc.PushTransform(new MatrixTransform(scaleFactor, 0, 0, -scaleFactor, Center.X, Center.Y));
                    dc.DrawGeometry(FillIn ? defaultBrush : Brushes.Transparent, pen, templateGeometry);
                    dc.Pop();
                }
                else
                {
                    // Create a StreamGeometry to use to specify myPath.
                    var geometry = new StreamGeometry();
                    // Open a StreamGeometryContext that can be used to describe this StreamGeometry 
                    // object's contents.
                    var starPoints = FindHyperArcPoints();
                    using (StreamGeometryContext ctx = geometry.Open())
                    {
                        ctx.BeginFigure(starPoints[0], FillIn, true);
                        for (int i = 1; i < starPoints.Count; i++)
                            ctx.LineTo(starPoints[i], true, true);
                    }
                    dc.DrawGeometry(brush, pen, geometry);
                }
                if (newConnection != null)
                {
                    var spikeGeom = new StreamGeometry();
                    using (StreamGeometryContext ctx = spikeGeom.Open())
                    {
                        ctx.BeginFigure(newConnection.Center, FillIn, true);
                        ctx.LineTo(Center, true, true);
                    }
                    dc.DrawGeometry(brush, pen, spikeGeom);
                }
            }
            if ((ShowName || ShowLabels) && (DisplayText != null))
            {
                var mbe = BindingOperations.GetMultiBindingExpression(this, TextPointProperty);
                if (mbe != null) mbe.UpdateTarget();
                dc.PushTransform(new MatrixTransform(1, 0, 0, -1, TextPoint.X, TextPoint.Y));
                dc.DrawText(DisplayText, new Point());
                dc.Pop();
            }

            base.OnRender(dc);
            Panel.SetZIndex(this, int.MaxValue);
        }

        private List<Point> FindHyperArcPoints()
        {
            Center = FindCenterPoint();
            //Find the angles to each node.
            var angleOrdered = new SortedList<double, Point>(new OptimizeSort(optimize.minimize));
            // need to allow duplicate entries
            foreach (var n in NodeCenters)
                angleOrdered.Add(Math.Atan2(n.Y - Center.Y, n.X - Center.X), n);

            var innerRadius = innerRadFactor * Radius;
            var returnPoints = new List<Point>();
            for (int i = 0; i < angleOrdered.Count; i++)
            {
                var initAngle = angleOrdered.Keys[i];
                returnPoints.Add(angleOrdered.Values[i]);
                var angle = angleOrdered.Keys[(i + 1) % angleOrdered.Count] - initAngle;
                if (angle <= 0) angle += 2 * Math.PI;
                // the % angleOrdered.Count in the previous statement is only to ensure that the last value of i returns to the zeroth position.
                int numIntermediate = (int)Math.Ceiling(angle / angleBtwPoints);
                angle /= (numIntermediate + 1);
                for (int j = 0; j < numIntermediate; j++)
                    if (j % 2 == 0) //if odd, use innerRadius
                        returnPoints.Add(Center + new Vector(innerRadius * Math.Cos(initAngle + (j + 1) * angle), innerRadius * Math.Sin(initAngle + (j + 1) * angle)));
                    else
                        returnPoints.Add(Center + new Vector(Radius * Math.Cos(initAngle + (j + 1) * angle), Radius * Math.Sin(initAngle + (j + 1) * angle)));
            }
            return returnPoints;
        }

        private Point FindCenterPoint()
        {
            var midpoint = new Point(NodeCenters.Average(n => n.X), NodeCenters.Average(n => n.Y));
            if (NodeCenters.Count == 1) return new Point(midpoint.X - templateGeometry.Bounds.Width,
                midpoint.Y - templateGeometry.Bounds.Height);
            // find true midpoint.
            // do PCA to find eigenvectors
            // move along eigenvector in direction that minimze the (max-min) radii 
            // to the nodes
            return midpoint;
        }

        public Boolean IsPointContained(Point p)
        {
            return ((p - Center).Length <= Radius);
        }

    }

}