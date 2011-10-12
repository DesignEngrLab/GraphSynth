using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSynth.UI;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    /// <summary>
    ///   Interaction logic for CircleHyperArcController.xaml
    /// </summary>
    public partial class CircleHyperArcController : HyperArcController
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
            binding = new Binding
                          {
                              Source = displayArc,
                              Mode = BindingMode.OneWay,
                              Path = new PropertyPath(HyperArcShape.NodeCentersProperty),
                              Converter = new SelectCenterObservableCollectionConverter(),
                              ConverterParameter = displayArc
                          };
            cmbNodeIndex.SetBinding(ItemsControl.ItemsSourceProperty, binding);
            binding = new Binding
                          {
                              Source = cmbNodeIndex,
                              Mode = BindingMode.TwoWay,
                              Path = new PropertyPath(Selector.SelectedValueProperty)
                          };
            SetBinding(NodeIndexProperty, binding);
        }


        public CircleHyperArcController(Shape _displayArc, Geometry initGeometry)
            : base(_displayArc)
        {
            BufferRadius = (((EllipseGeometry)initGeometry).RadiusX + ((EllipseGeometry)initGeometry).RadiusY) / 2;
        }

        public CircleHyperArcController(Shape _displayArc, double[] parameters)
            : base(_displayArc, parameters)
        { }
        #endregion

        #region Shape Adjustment Parameters

        #region Buffer Radius

        public static readonly DependencyProperty BufferRadiusProperty
            = DependencyProperty.Register("BufferRadius",
                                          typeof(double), typeof(CircleHyperArcController),
                                          new FrameworkPropertyMetadata(25.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public double BufferRadius
        {
            get { return (double)GetValue(BufferRadiusProperty); }
            set { SetValue(BufferRadiusProperty, value); }
        }


        #endregion

        #region Node Index

        public static readonly DependencyProperty NodeIndexProperty
            = DependencyProperty.Register("NodeIndex",
                                          typeof(int), typeof(CircleHyperArcController),
                                          new FrameworkPropertyMetadata(-1,
                                              FrameworkPropertyMetadataOptions.AffectsRender));
        public int NodeIndex
        {
            get { return (int)GetValue(NodeIndexProperty); }
            set { SetValue(NodeIndexProperty, value); }
        }

        #endregion

        #endregion


        #region Required Override Methods
        public override double[] parameters
        {
            get { return new[] { BufferRadius, NodeIndex }; }
            set
            {
                BufferRadius = value[0];
                NodeIndex = (int)value[1];
            }
        }

        internal override Geometry DefineSegment()
        {
            double radius = BufferRadius;
            if (displayArc.NodeCenters.Count == 1)
            {
                displayArc.Center = new Point(displayArc.NodeCenters[0].X, displayArc.NodeCenters[0].Y);
                radius += Math.Max(((hyperarc)displayArc.icon.GraphElement).nodes[0].DisplayShape.Width,
                    ((hyperarc)displayArc.icon.GraphElement).nodes[0].DisplayShape.Height) / 2;
            }
            else if (displayArc.NodeCenters.Count > 1)
            {
                if (NodeIndex == -1)
                    displayArc.Center = new Point(displayArc.NodeCenters.Average(n => n.X),
                                                  displayArc.NodeCenters.Average(n => n.Y));
                else displayArc.Center = displayArc.NodeCenters[NodeIndex];
                radius += displayArc.NodeCenters.Max(p => (p - displayArc.Center).Length);
            }
            return new EllipseGeometry
                                  {
                                      Center = displayArc.Center,
                                      RadiusX = radius + BufferRadius,
                                      RadiusY = radius + BufferRadius
                                  };
        }
        #endregion

        private void cmbNodeIndex_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (displayArc.StrokeDashCap == PenLineCap.Square)
                displayArc.StrokeDashCap = PenLineCap.Flat;
            else displayArc.StrokeDashCap = PenLineCap.Square;
        }
    }
}
