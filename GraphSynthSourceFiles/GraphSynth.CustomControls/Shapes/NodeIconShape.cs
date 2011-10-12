using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    public class NodeIconShape : IconShape
    {
        const double radiusMultiplier = 30.0;
        const double radiusAddition = 4.0;
        const double maxOpacity = 1.0;
        public Shape nodeShape { get; private set; }

        #region Constructor

        public NodeIconShape(graphElement n, Shape displayShape, GraphGUI gd)
            : base(n, gd, displayShape.Tag, maxOpacity, radiusMultiplier, radiusAddition, null)
        {
            nodeShape = displayShape;
            var dt = (DataTemplate)Application.Current.Resources["NodeIconShape"];
            var nodeIcon = (Shape)dt.LoadContent();
            Radius = defaultRadius = nodeIcon.Width / 2;
            defaultBrush = nodeIcon.Stroke;
            defaultThickness = nodeIcon.StrokeThickness;

            var multiBinding = new MultiBinding
                                   {
                                       Converter = new NodeIconTransformConverter(),
                                       ConverterParameter = new[] { defaultRadius + defaultThickness / 2, ScaleReduction },
                                       Mode = BindingMode.OneWay
                                   };
            {
                var binding = new Binding { Source = nodeShape, Path = new PropertyPath(RenderTransformProperty) };
                multiBinding.Bindings.Add(binding);

                binding = new Binding { Source = nodeShape, Path = new PropertyPath(WidthProperty) };
                multiBinding.Bindings.Add(binding);

                binding = new Binding { Source = nodeShape, Path = new PropertyPath(HeightProperty) };
                multiBinding.Bindings.Add(binding);

                binding = new Binding { Source = gd, Path = new PropertyPath(GraphGUI.ScaleFactorProperty) };
                multiBinding.Bindings.Add(binding);
            }
            SetBinding(RenderTransformProperty, multiBinding);

            multiBinding = new MultiBinding { Converter = new NodeIconCenterConverter() };
            {
                var binding = new Binding { Source = nodeShape, Path = new PropertyPath(RenderTransformProperty) };
                multiBinding.Bindings.Add(binding);

                binding = new Binding { Source = nodeShape, Path = new PropertyPath(WidthProperty) };
                multiBinding.Bindings.Add(binding);

                binding = new Binding { Source = nodeShape, Path = new PropertyPath(HeightProperty) };
                multiBinding.Bindings.Add(binding);
            }
            SetBinding(CenterProperty, multiBinding);

        }

        #endregion


        /// <summary>
        ///   Called when [render].
        /// </summary>
        /// <param name = "dc">The dc.</param>
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            var scaleFactor = Math.Pow(ScaleFactor, (ScaleReduction - 1));
            Radius = scaleFactor * defaultRadius;
            if (StrokeOpacity >= opacityCutoff)
            {
                var brush = defaultBrush.Clone();
                brush.Opacity = StrokeOpacity;
                var thickness = scaleFactor * defaultThickness;
                var halfWidth = Radius + thickness / 2;
                dc.DrawEllipse(FillIn ? brush : Brushes.Transparent,
                    new Pen { Brush = brush, Thickness = thickness }, new Point(halfWidth, halfWidth), Radius, Radius);

                Width = Height = 2 * halfWidth;
            }
            if ((ShowName || ShowLabels) && (DisplayText != null))
                dc.DrawText(DisplayText, new Point(Radius + TextPoint.X, Radius - TextPoint.Y));
            Panel.SetZIndex(this, int.MaxValue);
        }

        public Boolean IsPointContained(Point p)
        {
            return ((p - Center).Length <= Radius + radiusAddition);
        }
    }
}