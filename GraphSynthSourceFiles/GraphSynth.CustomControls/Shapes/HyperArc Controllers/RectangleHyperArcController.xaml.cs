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
    public partial class RectangleHyperArcController : HyperArcController
    {
        #region Constructors
        protected override void DefineSliders()
        {
            InitializeComponent();
            var binding = new Binding
                              {
                                  Source = sldtxtRadius,
                                  Mode = BindingMode.TwoWay,
                                  Path = new PropertyPath(SldAndTextbox.ValueProperty)
                              };
            SetBinding(BufferRadiusProperty, binding);
        }

        public RectangleHyperArcController(Shape _displayArc, Geometry initGeometry)
            : base(_displayArc)
        {
            BufferRadius = (((RectangleGeometry)initGeometry).RadiusX + ((RectangleGeometry)initGeometry).RadiusY) / 2;
            //what about nodeIndex?
        }

        public RectangleHyperArcController(Shape _displayArc, double[] parameters)
            : base(_displayArc, parameters)
        { }
        #endregion

        #region Shape Adjustment Parameters

        #region Buffer Radius

        public static readonly DependencyProperty BufferRadiusProperty
            = DependencyProperty.Register("BufferRadius",
                                          typeof(double), typeof(RectangleHyperArcController),
                                          new FrameworkPropertyMetadata(25.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public double BufferRadius
        {
            get { return (double)GetValue(BufferRadiusProperty); }
            set { SetValue(BufferRadiusProperty, value); }
        }


        #endregion

        #endregion

        private double maxX, minX, maxY, minY;
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
            var w = 2 * BufferRadius;
            var h = 2 * BufferRadius;

            if (displayArc.NodeCenters == null || displayArc.NodeCenters.Count == 0)
            {
                minX = maxX = displayArc.Center.X;
                minY = maxY = displayArc.Center.Y;
            }
            else if (displayArc.NodeCenters.Count == 1)
            {
                displayArc.Center = new Point(displayArc.NodeCenters[0].X, displayArc.NodeCenters[0].Y);
                var wNode = ((hyperarc)displayArc.icon.GraphElement).nodes[0].DisplayShape.Width;
                var hNode = ((hyperarc)displayArc.icon.GraphElement).nodes[0].DisplayShape.Height;
                minX = displayArc.NodeCenters[0].X - wNode / 2;
                maxX = minX + wNode;
                minY = displayArc.NodeCenters[0].Y - hNode / 2;
                maxY = minY + hNode;
            }
            else
            {
                maxX = displayArc.NodeCenters.Max(p => p.X);
                minX = displayArc.NodeCenters.Min(p => p.X);
                maxY = displayArc.NodeCenters.Max(p => p.Y);
                minY = displayArc.NodeCenters.Min(p => p.Y);
                displayArc.Center = new Point((maxX + minX) / 2, (maxY + minY) / 2);
            }
            w += (maxX - minX);
            h += (maxY - minY);
            var geom = new RectangleGeometry
                          {
                              Rect = new Rect(minX - BufferRadius, minY - BufferRadius, w, h),
                              RadiusX = BufferRadius,
                              RadiusY = BufferRadius,
                          };
            //forceRedraw();
            return geom;
        }
        #endregion
    }
}