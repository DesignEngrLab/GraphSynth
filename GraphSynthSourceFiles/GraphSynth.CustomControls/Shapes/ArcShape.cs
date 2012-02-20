using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSynth.Representation;
using GraphSynth.UI.Shapes;

namespace GraphSynth.GraphDisplay
{
    public class ArcShape : Shape
    {
        #region Constructor

        public ArcShape(Path p)
        {
            /* Choose and Initialize a Controller */
            Controller = defineController(p);
            AdoptPathQualities(p);

            /*Set up Body. */
            arcBody = new PathFigure();
            /* a dummy segment is added so that the arc controllers just replace Segment[0] */
            arcBody.Segments.Add(new LineSegment());

            /* Set up from Arrow. */
            fromArrowHead = new PathFigure();
            fromArrowHead.Segments.Add(new PolyLineSegment());
            fromArrowHead.IsClosed = fromArrowHead.IsFilled = true;

            /* Set up to Arrow. */
            toArrowHead = new PathFigure();
            toArrowHead.Segments.Add(new PolyLineSegment());
            toArrowHead.IsClosed = toArrowHead.IsFilled = true;

            /* Bind the Fill to always be the same as the Stroke. This is really
             * only necessary in the arrrow heads. */
            var colorBind = new Binding
            {
                Source = this,

                Path = new PropertyPath(StrokeProperty)
            };
            SetBinding(FillProperty, colorBind);

        }



        #endregion
        #region Dependency Properties

        public static readonly DependencyProperty FromShapeProperty
            = DependencyProperty.Register("FromShape",
                                          typeof(FrameworkElement), typeof(ArcShape),
                                          new FrameworkPropertyMetadata(null,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ToShapeProperty
            = DependencyProperty.Register("ToShape",
                                          typeof(FrameworkElement), typeof(ArcShape),
                                          new FrameworkPropertyMetadata(null,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FromLocationProperty
            = DependencyProperty.Register("FromLocation",
                                          typeof(Transform), typeof(ArcShape),
                                          new FrameworkPropertyMetadata(null,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ToLocationProperty
            = DependencyProperty.Register("ToLocation",
                                          typeof(Transform), typeof(ArcShape),
                                          new FrameworkPropertyMetadata(null,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FromWidthProperty
            = DependencyProperty.Register("FromWidth",
                                          typeof(double), typeof(ArcShape),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ToWidthProperty
            = DependencyProperty.Register("ToWidth",
                                          typeof(double), typeof(ArcShape),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FromHeightProperty
            = DependencyProperty.Register("FromHeight",
                                          typeof(double), typeof(ArcShape),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ToHeightProperty
            = DependencyProperty.Register("ToHeight",
                                          typeof(double), typeof(ArcShape),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty DirectedProperty
            = DependencyProperty.Register("directed",
                                          typeof(Boolean), typeof(ArcShape),
                                          new FrameworkPropertyMetadata(true,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty DoublyDirectedProperty
            = DependencyProperty.Register("doublyDirected",
                                          typeof(Boolean), typeof(ArcShape),
                                          new FrameworkPropertyMetadata(false,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowArrowHeadsProperty
            = DependencyProperty.Register("ShowArrowHeads",
                                          typeof(Boolean), typeof(ArcShape),
                                          new FrameworkPropertyMetadata(true,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Gets or sets from location.
        /// </summary>
        /// <value>From location.</value>
        public FrameworkElement FromShape
        {
            get { return (FrameworkElement)GetValue(FromShapeProperty); }
            set { SetValue(FromShapeProperty, value); }
        }

        /// <summary>
        ///   Gets or sets to location.
        /// </summary>
        /// <value>To location.</value>
        public FrameworkElement ToShape
        {
            get { return (FrameworkElement)GetValue(ToShapeProperty); }
            set { SetValue(ToShapeProperty, value); }
        }

        /// <summary>
        ///   Gets or sets from location.
        /// </summary>
        /// <value>From location.</value>
        public Transform FromLocation
        {
            get { return (Transform)GetValue(FromLocationProperty); }
            set { SetValue(FromLocationProperty, value); }
        }

        public Transform ToLocation
        {
            get { return (Transform)GetValue(ToLocationProperty); }
            set { SetValue(ToLocationProperty, value); }
        }

        public double FromWidth
        {
            get { return (double)GetValue(FromWidthProperty); }
            set { SetValue(FromWidthProperty, value); }
        }

        public double ToWidth
        {
            get { return (double)GetValue(ToWidthProperty); }
            set { SetValue(ToWidthProperty, value); }
        }

        public double FromHeight
        {
            get { return (double)GetValue(FromHeightProperty); }
            set { SetValue(FromHeightProperty, value); }
        }

        public double ToHeight
        {
            get { return (double)GetValue(ToHeightProperty); }
            set { SetValue(ToHeightProperty, value); }
        }

        public Boolean directed
        {
            get { return (Boolean)GetValue(DirectedProperty); }
            set
            {
                SetValue(DirectedProperty, value);
                if (!value && doublyDirected) SetValue(DoublyDirectedProperty, value);
            }
        }

        public Boolean doublyDirected
        {
            get { return (Boolean)GetValue(DoublyDirectedProperty); }
            set
            {
                SetValue(DoublyDirectedProperty, value);
                if (value && !directed) SetValue(DirectedProperty, value);
            }
        }

        public Boolean ShowArrowHeads
        {
            get { return (Boolean)GetValue(ShowArrowHeadsProperty); }
            set { SetValue(ShowArrowHeadsProperty, value); }
        }

        #endregion

        #region Fields

        private const double arrowHalfWidth = 4.0;
        private const double arrowHeight = 10;
        public PathFigure arcBody;
        public PathFigure fromArrowHead;
        public PathFigure toArrowHead;
        #endregion

        #region Properties

        public AbstractController Controller { get; set; }
        public ArcIconShape icon { get; set; }
        public Point toPoint { get; set; }
        public Point fromPoint { get; set; }
        public double toAngle { get; set; }
        public double fromAngle { get; set; }
        public double straightLineLength { get; set; }
        #endregion


        private AbstractController defineController(Path p)
        {
            /* first try to find the new easy way, from data stored in the Tag */
            AbstractController ctrl;
            int colonPosition = p.Tag.ToString().IndexOf(':');
            if ((colonPosition >= 0)
                && AbstractController.ConstructFromString(p.Tag.ToString().Substring(colonPosition + 1),
                this, out ctrl))
                return ctrl;
            /* that didn't work? the old way is to try to parse the shape to get at the right parameters for each controller. */
            if ((p.Data is PathGeometry)
                && (((PathGeometry)p.Data).Figures.Count > 0)
                && (((PathGeometry)p.Data).Figures[0].Segments.Count > 0))
            // here we are assuming that the first figure, Figures[0], is the arcBody
            // if others exist, then they are the arrowHeads
            {
                var startPt = ((PathGeometry)p.Data).Figures[0].StartPoint;
                var segment = ((PathGeometry)p.Data).Figures[0].Segments[0];
                if ((typeof(BezierSegment)).IsInstanceOfType(segment))
                    return new BezierArcController(this, segment, startPt);
                if ((typeof(PolyLineSegment)).IsInstanceOfType(segment))
                    return new RectilinearArcController(this, segment, startPt);
                if ((typeof(ArcSegment)).IsInstanceOfType(segment))
                    return new CircleArcController(this, segment, startPt);
            }
            return new StraightArcController(this);
        }



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


        private void AdoptPathQualities(Path p)
        {
            Clip = p.Clip;
            Effect = p.Effect;
            Fill = p.Fill;
            Height = p.Height;
            HorizontalAlignment = p.HorizontalAlignment;
            Language = p.Language;
            LayoutTransform = p.LayoutTransform;
            Margin = p.Margin;
            Tag = p.Tag;
            Opacity = p.Opacity;
            OpacityMask = p.OpacityMask;
            RenderSize = p.RenderSize;
            RenderTransform = p.RenderTransform;
            RenderTransformOrigin = p.RenderTransformOrigin;
            SnapsToDevicePixels = p.SnapsToDevicePixels;
            Stretch = p.Stretch;
            Stroke = p.Stroke;
            StrokeDashArray = p.StrokeDashArray;
            StrokeDashCap = p.StrokeDashCap;
            StrokeDashOffset = p.StrokeDashOffset;
            StrokeEndLineCap = p.StrokeEndLineCap;
            StrokeLineJoin = p.StrokeLineJoin;
            StrokeMiterLimit = p.StrokeMiterLimit;
            StrokeStartLineCap = p.StrokeStartLineCap;
            StrokeThickness = p.StrokeThickness;
            Style = p.Style;
            Visibility = p.Visibility;
            VerticalAlignment = p.VerticalAlignment;
            Width = p.Width;
            if (((PathGeometry)p.Data).Figures.Count > 1) ShowArrowHeads = true;
            else ShowArrowHeads = false;
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
                InkCanvas.SetLeft(this, double.NaN);
                InkCanvas.SetTop(this, double.NaN);

                // define the body of the arc. This is done by the 
                // arcController specified for this arc.
                arcBody = ((ArcController)Controller).DefineSegment();
                var geom = new PathGeometry();
                geom.Figures.Add(arcBody);

                //define the arrow heads for the arc. Even if this 
                //particular arc is not showing them (ShowArrowHeads is false)
                //we still transform them s.t. they appears within the arcIcon.
                if (directed)
                {
                    tranformArrowHead(toArrowHead, toPoint, toAngle);
                    if (ShowArrowHeads) geom.Figures.Add(toArrowHead);
                }
                if (doublyDirected)
                {
                    tranformArrowHead(fromArrowHead, fromPoint, fromAngle);
                    if (ShowArrowHeads) geom.Figures.Add(fromArrowHead);
                }
                icon.InvalidateVisual();
                return geom;
            }
        }

        #endregion


        #region Arrowhead Geometry

        private void tranformArrowHead(PathFigure arrowHead, Point tip, double angle)
        {
            try
            {
                if ((!double.IsNaN(angle)) && (!double.IsNaN(tip.X)) && (!double.IsNaN(tip.Y)))
                {
                    var mt = new Matrix();
                    mt.Rotate(180 * angle / Math.PI);
                    mt.OffsetX = tip.X;
                    mt.OffsetY = tip.Y;

                    arrowHead.StartPoint = tip;
                    var arrowLines = makeArrowHead();

                    for (var i = 0; i < arrowLines.Points.Count; i++)
                        arrowLines.Points[i] *= mt;
                    arrowHead.Segments[0] = arrowLines;
                }
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }

        private PolyLineSegment makeArrowHead()
        {
            try
            {
                /* the characteristic GraphSynth arrow is a chevron shape. */
                var arrowLines = new PolyLineSegment { IsSmoothJoin = true, IsStroked = true };
                arrowLines.Points.Add(new Point(arrowHeight, arrowHalfWidth + 0.5 * StrokeThickness));
                arrowLines.Points.Add(new Point(0.7 * arrowHeight, 0.0));
                arrowLines.Points.Add(new Point(arrowHeight, -arrowHalfWidth - 0.5 * StrokeThickness));

                return arrowLines;
            }
            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
                return null;
            }
        }

        #endregion

        #region Create Shape Binding Method
        internal void CreateShapeBindings(FrameworkElement fromNodeShape, FrameworkElement toNodeShape, arc a)
        {
            try
            {
               // BindingOperations.ClearAllBindings(this);
                // From Shape
                var binding = new Binding { Source = fromNodeShape, Mode = BindingMode.OneWay };
                SetBinding(FromShapeProperty, binding);

                if (fromNodeShape is NullNodeIconShape)
                    binding = new Binding
                    {
                        Source = fromNodeShape,
                        Converter = new PointToTransformConverter(),
                        Path = new PropertyPath(IconShape.CenterProperty)
                    };
                else binding = new Binding { Source = fromNodeShape, Path = new PropertyPath(RenderTransformProperty) };
                SetBinding(FromLocationProperty, binding);

                binding = new Binding { Source = fromNodeShape, Path = new PropertyPath(WidthProperty) };
                SetBinding(FromWidthProperty, binding);

                binding = new Binding { Source = fromNodeShape, Path = new PropertyPath(HeightProperty) };
                SetBinding(FromHeightProperty, binding);

                // To Shape
                binding = new Binding { Source = toNodeShape, Mode = BindingMode.OneWay };
                SetBinding(ToShapeProperty, binding);


                if (toNodeShape is NullNodeIconShape)
                    binding = new Binding
                    {
                        Source = toNodeShape,
                        Converter = new PointToTransformConverter(),
                        Path = new PropertyPath(IconShape.CenterProperty)
                    };
                else binding = new Binding { Source = toNodeShape, Path = new PropertyPath(RenderTransformProperty) };
                SetBinding(ToLocationProperty, binding);

                binding = new Binding { Source = toNodeShape, Path = new PropertyPath(WidthProperty) };
                SetBinding(ToWidthProperty, binding);

                binding = new Binding { Source = toNodeShape, Path = new PropertyPath(HeightProperty) };
                SetBinding(ToHeightProperty, binding);

                //directed and doubly-directed
                binding = new Binding { Source = a.directed, Mode = BindingMode.OneWay };
                SetBinding(DirectedProperty, binding);

                binding = new Binding { Source = a.doublyDirected, Mode = BindingMode.OneWay };
                SetBinding(DoublyDirectedProperty, binding);
            }

            catch (Exception exc)
            {
                ErrorLogger.Catch(exc);
            }
        }
        #endregion


    }
}