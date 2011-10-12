using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSynth.UI;

namespace GraphSynth.GraphDisplay
{
    /// <summary>
    ///   Interaction logic for BezierArcController.xaml
    /// </summary>
    public partial class StarHyperArcController : HyperArcController
    {
        #region Constructors
        protected override void DefineSliders()
        {
            InitializeComponent();
            var binding = new Binding
                              {
                                  Source = sldtxtAngle,
                                  Mode = BindingMode.TwoWay,
                                  Path = new PropertyPath(SldAndTextbox.ValueProperty)
                              };
            SetBinding(MinimumAngleProperty, binding);
            binding = new Binding
            {
                Source = sldtxtInnerRadius,
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(SldAndTextbox.ValueProperty)
            };
            SetBinding(InnerRadiusProperty, binding);
            binding = new Binding
            {
                Source = sldtxtOuterRadius,
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(SldAndTextbox.ValueProperty)
            };
            SetBinding(OuterRadiusProperty, binding);
        }


        public StarHyperArcController(Shape _displayArc, Geometry initGeometry)
            : base(_displayArc)
        {
            try
            {
                var pathGeom = (PathGeometry)initGeometry;
                var iconPts = new List<Point>(((PolyLineSegment)pathGeom.Figures[0].Segments[0]).Points);
                iconPts.Insert(0, pathGeom.Figures[0].StartPoint);
                var midpoint = new Point(iconPts.Average(n => n.X), iconPts.Average(n => n.Y));
                OuterRadius = (from p in iconPts select (new Point(p.X, p.Y) - midpoint).Length).Max();
                InnerRadius = (from p in iconPts select (new Point(p.X, p.Y) - midpoint).Length).Min();
                MinimumAngle = 360.0 / iconPts.Count;
            }
            catch { }
        }

        public StarHyperArcController(Shape _displayArc, double[] parameters)
            : base(_displayArc, parameters)
        {  }
        #endregion

        #region Shape Adjustment Parameters


        public static readonly DependencyProperty InnerRadiusProperty
            = DependencyProperty.Register("InnerRadius",
                                          typeof(double), typeof(CircleHyperArcController),
                                          new FrameworkPropertyMetadata(15.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public double InnerRadius
        {
            get { return (double)GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }

        public static readonly DependencyProperty OuterRadiusProperty
            = DependencyProperty.Register("OuterRadius",
                                          typeof(double), typeof(CircleHyperArcController),
                                          new FrameworkPropertyMetadata(25.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public double OuterRadius
        {
            get { return (double)GetValue(OuterRadiusProperty); }
            set { SetValue(OuterRadiusProperty, value); }
        }

        public static readonly DependencyProperty MinimumAngleProperty
    = DependencyProperty.Register("MinimumAngle",
                                  typeof(double), typeof(CircleHyperArcController),
                                  new FrameworkPropertyMetadata(72.0,
                                                                FrameworkPropertyMetadataOptions.AffectsRender));

        public double MinimumAngle
        {
            get { return (double)GetValue(MinimumAngleProperty); }
            set { SetValue(MinimumAngleProperty, value); }
        }



        #endregion

        #region Required Override Methods

        public override double[] parameters
        {
            get { return new[] { MinimumAngle, InnerRadius, OuterRadius }; }
            set
            {
                MinimumAngle = value[0];
                InnerRadius = value[1];
                OuterRadius = value[2];
            }
        }

        internal override Geometry DefineSegment()
        {
            // Create a StreamGeometry to use to specify myPath.
            var geometry = new StreamGeometry();
            if (displayArc.NodeCenters.Count > 0)
                displayArc.Center = new Point(displayArc.NodeCenters.Average(n => n.X),
                                              displayArc.NodeCenters.Average(n => n.Y));
            List<Point> starPoints = FindHyperArcPoints();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(starPoints[0], true, true);
                for (int i = 1; i < starPoints.Count; i++)
                    ctx.LineTo(starPoints[i], true, true);
            }
            return geometry;
        }

        private List<Point> FindHyperArcPoints()
        {
            //Find the angles to each node.
            var angleOrdered = new SortedList<double, Point>(new OptimizeSort(optimize.minimize));
            // need to allow duplicate entries
            foreach (var n in displayArc.NodeCenters)
                angleOrdered.Add(Math.Atan2(n.Y - displayArc.Center.Y, n.X - displayArc.Center.X), n);

            var returnPoints = new List<Point>();
            if (angleOrdered.Count <= 1)
            {
                returnPoints.Add(new Point(displayArc.Center.X + OuterRadius, displayArc.Center.Y));
                var angle = 2 * Math.PI;
                // the % angleOrdered.Count in the previous statement is only to ensure that the last value of i returns to the zeroth position.
                var numIntermediate = (int)Math.Ceiling(180.0 * angle / (Math.PI * MinimumAngle));
                angle /= numIntermediate;
                for (int j = 0; j < numIntermediate; j++)
                    if (j % 2 == 0) //if odd, use innerRadius
                        returnPoints.Add(displayArc.Center +
                                         new Vector(InnerRadius * Math.Cos((j + 1) * angle),
                                                    InnerRadius * Math.Sin((j + 1) * angle)));
                    else
                        returnPoints.Add(displayArc.Center +
                                         new Vector(OuterRadius * Math.Cos((j + 1) * angle),
                                                    OuterRadius * Math.Sin((j + 1) * angle)));
                return returnPoints;
            }
            //for 2 or more points
            for (int i = 0; i < angleOrdered.Count; i++)
            {
                var initAngle = angleOrdered.Keys[i];
                returnPoints.Add(angleOrdered.Values[i]);
                var angle = angleOrdered.Keys[(i + 1) % angleOrdered.Count] - initAngle;
                if (angle <= 0) angle += 2 * Math.PI;
                // the % angleOrdered.Count in the previous statement is only to ensure that the last value of i returns to the zeroth position.
                var numIntermediate = (int)Math.Ceiling(180.0 * angle / (Math.PI * MinimumAngle));
                angle /= (numIntermediate + 1);
                for (int j = 0; j < numIntermediate; j++)
                    if (j % 2 == 0) //if odd, use innerRadius
                        returnPoints.Add(displayArc.Center +
                                         new Vector(InnerRadius * Math.Cos(initAngle + (j + 1) * angle),
                                                    InnerRadius * Math.Sin(initAngle + (j + 1) * angle)));
                    else
                        returnPoints.Add(displayArc.Center +
                                         new Vector(OuterRadius * Math.Cos(initAngle + (j + 1) * angle),
                                                    OuterRadius * Math.Sin(initAngle + (j + 1) * angle)));

            }
            return returnPoints;
        }
        #endregion
    }
}