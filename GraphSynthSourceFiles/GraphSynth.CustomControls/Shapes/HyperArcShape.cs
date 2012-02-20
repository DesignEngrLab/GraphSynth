using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphSynth.GraphDisplay
{
    public class HyperArcShape : Shape
    {
        public HyperArcIconShape icon { get; set; }
        #region Constructor & other Initialization Functions

        public AbstractController Controller { get; set; }
        public Point Center { get; set; }

        public HyperArcShape(Shape s)
        {
            var initGeometry = ExtractSegment(s);
            AdoptPathQualities(s);
            /* Choose and Initialize a Controller */
            Controller = defineController(initGeometry, s);
        }


        private static Geometry ExtractSegment(Shape p)
        {
            if (p is Ellipse)
                return new EllipseGeometry
                           {
                               Center = new Point(p.RenderTransform.Value.OffsetX + p.Width / 2, p.RenderTransform.Value.OffsetY + p.Height / 2),
                               RadiusX = p.Width / 2,
                               RadiusY = p.Height / 2
                           };
            else if (p is Rectangle)
                return new RectangleGeometry
                {
                    Rect = new Rect(p.RenderTransform.Value.OffsetX + p.Width / 2, p.RenderTransform.Value.OffsetY + p.Height / 2, p.Width, p.Height),
                    RadiusX = ((Rectangle)p).RadiusX,
                    RadiusY = ((Rectangle)p).RadiusY
                };
            else if (p is Path)
                if (((Path)p).Data is StreamGeometry)
                    return ((Path)p).Data.GetFlattenedPathGeometry();
                else return ((Path)p).Data;
            else return null;
        }


        private AbstractController defineController(Geometry initGeometry, Shape p)
        {
            /* first try to find the new easy way, from data stored in the Tag */
            AbstractController ctrl;
            int colonPosition = p.Tag.ToString().IndexOf(':');
            if ((colonPosition >= 0)
                && AbstractController.ConstructFromString(p.Tag.ToString().Substring(colonPosition + 1),
                this, out ctrl))
                return ctrl;
            if ((typeof(EllipseGeometry)).IsInstanceOfType(initGeometry))
                return new CircleHyperArcController(this, initGeometry);
            if ((typeof(RectangleGeometry)).IsInstanceOfType(initGeometry))
                return new RectangleHyperArcController(this, initGeometry);
            return new StarHyperArcController(this, initGeometry);
        }
        #endregion

        #region Convert to and from Path

        internal string XamlWrite()
        {
            return MyXamlHelpers.XamlOfShape(
                new Path
                    {
                        Fill = Fill,
                        Height = Height,
                        HorizontalAlignment = HorizontalAlignment,
                        Language = Language,
                        LayoutTransform = LayoutTransform,
                        Margin = Margin,
                        Tag = icon.UpdateTag() + Controller.ToString(),
                        Opacity = Opacity,
                        RenderSize = RenderSize,
                        RenderTransform = RenderTransform,
                        RenderTransformOrigin = RenderTransformOrigin,
                        SnapsToDevicePixels = SnapsToDevicePixels,
                        Stretch = Stretch,
                        Stroke = Stroke,
                        StrokeDashArray = StrokeDashArray,
                        StrokeDashCap = StrokeDashCap,
                        StrokeDashOffset = StrokeDashOffset,
                        StrokeEndLineCap = StrokeEndLineCap,
                        StrokeLineJoin = StrokeLineJoin,
                        StrokeMiterLimit = StrokeMiterLimit,
                        StrokeStartLineCap = StrokeStartLineCap,
                        StrokeThickness = StrokeThickness,
                        Visibility = Visibility,
                        VerticalAlignment = VerticalAlignment,
                        Width = Width,
                        Data = DefiningGeometry
                    },
                    icon.UpdateTag() + Controller.ToString());
        }

        private void AdoptPathQualities(Shape s)
        {
            Clip = s.Clip;
            Effect = s.Effect;
            Fill = s.Fill;
            Language = s.Language;
            Margin = s.Margin;
            Tag = s.Tag;
            Opacity = s.Opacity;
            OpacityMask = s.OpacityMask;
            SnapsToDevicePixels = s.SnapsToDevicePixels;
            Stroke = s.Stroke;
            StrokeDashArray = s.StrokeDashArray;
            StrokeDashCap = s.StrokeDashCap;
            StrokeDashOffset = s.StrokeDashOffset;
            StrokeEndLineCap = s.StrokeEndLineCap;
            StrokeLineJoin = s.StrokeLineJoin;
            StrokeMiterLimit = s.StrokeMiterLimit;
            StrokeStartLineCap = s.StrokeStartLineCap;
            StrokeThickness = s.StrokeThickness;
            Style = s.Style;
            Visibility = s.Visibility;
        }


        #endregion

        #region Binding to Node locations

        public static readonly DependencyProperty NodeCentersProperty
            = DependencyProperty.Register("NodeCenters",
                                          typeof(PointCollection), typeof(HyperArcShape),
                                          new FrameworkPropertyMetadata(null,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public PointCollection NodeCenters
        {
            get { return (PointCollection)GetValue(NodeCentersProperty); }
            set { SetValue(NodeCentersProperty, value); }
        }


        #endregion
        #region Geometry

        /// <summary>
        ///   Gets a value that represents the Geometry of the ArrowLine.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                var geom = ((HyperArcController)Controller).DefineSegment();
                Panel.SetZIndex(this, int.MinValue);
                return geom;
            }
        }

        #endregion
    }
}