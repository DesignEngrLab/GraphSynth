using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    /// <summary>
    ///   Interaction logic for InferredHyperArcController.xaml
    /// </summary>
    public partial class InferredHyperArcController : HyperArcController
    {
        #region Constructors
        protected override void DefineSliders()
        {
            InitializeComponent();
        }


        public InferredHyperArcController(Shape _displayArc, Geometry initGeometry)
            : base(_displayArc) { }

        public InferredHyperArcController(Shape _displayArc, double[] parameters)
            : base(_displayArc, parameters)
        { }
        #endregion

        public static readonly DependencyProperty RefreshProperty
    = DependencyProperty.Register("Refresh",
                                  typeof(Boolean), typeof(InferredHyperArcController),
                                  new FrameworkPropertyMetadata(false,
                                      FrameworkPropertyMetadataOptions.AffectsRender));
        public Boolean Refresh
        {
            get { return (Boolean)GetValue(RefreshProperty); }
            set
            {
                SetValue(RefreshProperty, value);
                if (value) displayShape.StrokeThickness += 0.001;
                else displayShape.StrokeThickness -= 0.001;
            }
        }


        #region Required Override Methods

        public override double[] parameters { get; set; }
        private List<arc> pathArcs;
        internal override Geometry DefineSegment()
        {
            if (displayArc.NodeCenters.Count == 0)
                return new EllipseGeometry(displayArc.Center,
                                           displayArc.icon.Radius, displayArc.icon.Radius);

            try
            {
                var h = (hyperarc)displayArc.icon.GraphElement;
                var tempArcs = new List<arc>(pathArcs);
                var minX = tempArcs.Min(a => a.From.X);
                var start = h.nodes.First(n => n.X == minX);
                var endNode = start;
                PathFigure pf;
                var forward = true;
                arc pathEdge = tempArcs.FirstOrDefault(a => a.From == endNode);
                if (pathEdge != null)
                    pf = new PathFigure
                                {
                                    StartPoint = ((ArcShape)pathEdge.DisplayShape.Shape).fromPoint,
                                    IsClosed = true,
                                    IsFilled = true
                                };
                else
                {
                    pathEdge = tempArcs.FirstOrDefault(a => a.To == endNode);
                    pf = new PathFigure
                    {
                        StartPoint = ((ArcShape)pathEdge.DisplayShape.Shape).toPoint,
                        IsClosed = true,
                        IsFilled = true
                    };
                    forward = false;
                }
                tempArcs.Remove(pathEdge);
                do
                {
                    if (forward)
                        pf.Segments.Add(((ArcShape)pathEdge.DisplayShape.Shape).arcBody.Segments[0].CloneCurrentValue());
                    else pf.Segments.Add(ReverseSegment0(((ArcShape)pathEdge.DisplayShape.Shape).arcBody));
                    endNode = pathEdge.otherNode(endNode);
                    if (endNode == start) continue;
                    pathEdge = tempArcs.Find(a => a.From == endNode);
                    if (pathEdge != null)
                    {
                        pf.Segments.Add(new LineSegment(((ArcShape)pathEdge.DisplayShape.Shape).fromPoint, true));
                        forward = true;
                    }
                    else
                    {
                        pathEdge = tempArcs.FirstOrDefault(a => a.To == endNode);
                        if (pathEdge != null)
                        {
                            pf.Segments.Add(new LineSegment(((ArcShape)pathEdge.DisplayShape.Shape).toPoint, true));
                            forward = false;
                        }
                    }
                    tempArcs.Remove(pathEdge);

                } while ((pathEdge != null) && (endNode != start));
                var geometry = new PathGeometry();
                geometry.Figures.Add(pf);
                return geometry;
            }
            catch
            {
                if (displayArc.NodeCenters.Count == 1)
                {
                    var wNode = ((hyperarc)displayArc.icon.GraphElement).nodes[0].DisplayShape.Width;
                    var hNode = ((hyperarc)displayArc.icon.GraphElement).nodes[0].DisplayShape.Height;
                    return new EllipseGeometry(displayArc.NodeCenters[0], wNode + displayArc.icon.Radius,
                                               hNode + displayArc.icon.Radius);
                }
                else
                    return new EllipseGeometry(displayArc.Center,
                                            displayArc.icon.Radius, displayArc.icon.Radius);
            }
        }

        private static PathSegment ReverseSegment0(PathFigure arcBody)
        {
            var toPt = arcBody.StartPoint;
            var origSegment = arcBody.Segments[0];
            if (origSegment is LineSegment)
                return new LineSegment(toPt, origSegment.IsStroked);

            if (origSegment is PolyLineSegment)
            {
                var Points = ((PolyLineSegment)origSegment).Points.Reverse().ToList();
                Points.RemoveAt(0);
                Points.Add(toPt);

                return new PolyLineSegment(Points, origSegment.IsStroked);
            }
            if (origSegment is BezierSegment)
                return new BezierSegment(((BezierSegment)origSegment).Point2, ((BezierSegment)origSegment).Point1,
                                         toPt, origSegment.IsStroked);
            if (origSegment is ArcSegment)
            {
                var sweep = ((ArcSegment)origSegment).SweepDirection == SweepDirection.Clockwise
                                ? SweepDirection.Counterclockwise
                                : SweepDirection.Clockwise;
                return new ArcSegment(toPt, ((ArcSegment)origSegment).Size, ((ArcSegment)origSegment).RotationAngle,
                                      ((ArcSegment)origSegment).IsLargeArc, sweep, origSegment.IsStroked);
            }
            else return null;
        }

        #endregion

        /// <summary>
        /// Bind this controller to the arcs as well - So that it updates automatically.
        /// It's clunky and not working. What I need to do is to put a change even in the 
        /// arc controller which is retrieved and tied to this update.
        /// </summary>
        internal void BindToArcs()
        {
            var h = (hyperarc)displayArc.icon.GraphElement;
            pathArcs = new List<arc>(h.IntraArcs);

            // the following seems to have no effect. Why? 
            //BindingOperations.ClearBinding(this, RefreshProperty);
            //var multiBinding = new MultiBinding
            //{
            //    Mode = BindingMode.OneWay,
            //    Converter = new DummyArcChangeConverter(this)
            //};
            //foreach (var a in pathArcs)
            //{
            //    var ac = ((ArcShape) a.DisplayShape.Shape).Controller;
            //    foreach (var dp in GetArcControllerDependencyProperties(ac))
            //    {
            //        var binding = new Binding
            //                          {
            //                              Source = ac,
            //                              Mode = BindingMode.OneWay,
            //                              Path = new PropertyPath(dp)
            //                          };
            //        multiBinding.Bindings.Add(binding);
            //    }
            //}
            //SetBinding(RefreshProperty, multiBinding);
        }
        // This clever bit of code which I modified from some forum, dynamically finds the
        // the dependency properties added to each arc controller.
        public static IList<DependencyProperty> GetArcControllerDependencyProperties(Object element)
        {
            var properties = new List<DependencyProperty>();
            var allDependencyProperties = TypeDescriptor.GetProperties(element,
                                                                       new Attribute[]
                                                                           {
                                                                               new PropertyFilterAttribute(
                                                                                   PropertyFilterOptions.SetValues |
                                                                                   PropertyFilterOptions.UnsetValues |
                                                                                   PropertyFilterOptions.Valid)
                                                                           });
            var numNewDepProps = allDependencyProperties.Count - 263;
            for (int i = 0; i < numNewDepProps; i++)
            {
                var dpd = DependencyPropertyDescriptor.FromProperty(allDependencyProperties[i]);
                if (dpd != null)
                    properties.Add(dpd.DependencyProperty);
            }
            return properties;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            BindToArcs();
            Refresh = !Refresh;
        }
    }

    /// <summary>
    /// this is meant simply to update the above controller when there is a change in one of the
    /// arcs. Doesn't seem to work anyway
    /// </summary>
    public class DummyArcChangeConverter : IMultiValueConverter
    {
        private readonly InferredHyperArcController controller;
        public DummyArcChangeConverter(InferredHyperArcController controller)
        {
            this.controller = controller;
        }

        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var i = values.GetLength(0);
            return !controller.Refresh;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}