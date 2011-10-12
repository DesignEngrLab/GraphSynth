using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSynth.Representation;
using GraphSynth.UI;

namespace GraphSynth.GraphDisplay
{
    /// <summary>
    ///   Interaction logic for BezierArcController.xaml
    /// </summary>
    public partial class ConvexPolygonArcController : HyperArcController
    {
        #region Constructors
        protected override void DefineSliders()
        {
            InitializeComponent();
            var binding = new Binding
                              {
                                  Source = sldtxtBufferRadius,
                                  Mode = BindingMode.TwoWay,
                                  Path = new PropertyPath(SldAndTextbox.ValueProperty)
                              };
            SetBinding(BufferRadiusProperty, binding);
        }


        public ConvexPolygonArcController(Shape _displayArc, Geometry initGeometry)
            : base(_displayArc)
        {
            try
            {
                var pathGeom = (PathGeometry)initGeometry;
                var arcSeg = pathGeom.Figures[0].Segments.FirstOrDefault(s => typeof(ArcSegment).IsInstanceOfType(s));
                if (arcSeg == null) BufferRadius = 0;
                else BufferRadius = ((ArcSegment)arcSeg).RotationAngle;
            }
            catch { }
        }

        public ConvexPolygonArcController(Shape _displayArc, double[] parameters)
            : base(_displayArc, parameters)
        { }
        #endregion

        #region Shape Adjustment Parameters


        public static readonly DependencyProperty BufferRadiusProperty
            = DependencyProperty.Register("BufferRadius",
                                          typeof(double), typeof(ConvexPolygonArcController),
                                          new FrameworkPropertyMetadata(15.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public double BufferRadius
        {
            get { return (double)GetValue(BufferRadiusProperty); }
            set { SetValue(BufferRadiusProperty, value); }
        }
        #endregion

        #region Required Override Methods

        public override double[] parameters
        {
            get { return new[] { BufferRadius }; }
            set
            {
                BufferRadius = value[0];
            }
        }

        internal override Geometry DefineSegment()
        {
            double radius = BufferRadius;
            if (displayArc.NodeCenters.Count <= 1)
            {
                if (displayArc.NodeCenters.Count == 1)
                {
                    displayArc.Center = new Point(displayArc.NodeCenters[0].X, displayArc.NodeCenters[0].Y);
                    radius += Math.Max(((hyperarc)displayArc.icon.GraphElement).nodes[0].DisplayShape.Width,
                        ((hyperarc)displayArc.icon.GraphElement).nodes[0].DisplayShape.Height) / 2;
                }
                return new EllipseGeometry
                                      {
                                          Center = displayArc.Center,
                                          RadiusX = radius + BufferRadius,
                                          RadiusY = radius + BufferRadius
                                      };
            }
            var rawPoints = MIConvexHull.Find(displayArc.NodeCenters);
            displayArc.Center = new Point(displayArc.NodeCenters.Average(n => n.X),
                               displayArc.NodeCenters.Average(n => n.Y));

            var geometry = new StreamGeometry();
            if (BufferRadius <= 0)
            {
                using (var ctx = geometry.Open())
                {
                    ctx.BeginFigure(rawPoints[0], true, true);
                    for (int i = 1; i < rawPoints.Count; i++)
                        ctx.LineTo(rawPoints[i], true, true);
                }
            }
            else
            {
                var size = new Size(BufferRadius, BufferRadius);
                using (StreamGeometryContext ctx = geometry.Open())
                {
                    ctx.BeginFigure(findNextPoint(rawPoints[rawPoints.Count - 1], rawPoints[0]), true, true);
                    for (int i = 0; i < rawPoints.Count - 1; i++)
                    {
                        ctx.ArcTo(findThisPoint(rawPoints[i], rawPoints[i + 1]), size, 0.0, false,
                                  SweepDirection.Clockwise,
                                  true, true);
                        ctx.LineTo(findNextPoint(rawPoints[i], rawPoints[i + 1]), true, true);
                    }
                    ctx.ArcTo(findThisPoint(rawPoints[rawPoints.Count - 1], rawPoints[0]), size, 0.0, false,
                              SweepDirection.Clockwise,
                              true, true);
                }
            }
            return geometry;
        }

        Point findThisPoint(Point thisPt, Point nextPt)
        {
            var outVector = new Vector((nextPt.Y - thisPt.Y), (thisPt.X - nextPt.X));
            outVector.Normalize();
            return thisPt + (BufferRadius * outVector);
        }
        Point findNextPoint(Point thisPt, Point nextPt)
        {
            var outVector = new Vector((nextPt.Y - thisPt.Y), (thisPt.X - nextPt.X));
            outVector.Normalize();
            return nextPt + (BufferRadius * outVector);
        }
        #endregion
    }
}