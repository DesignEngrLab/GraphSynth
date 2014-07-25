using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    public abstract class IconShape : FrameworkElement
    {
        /* the scale reduction determines how much the node icons change with zoom
         * when it is zero, they do not change shape at all. When it is 1, they change
         * the same as the rest of the figure. In playing with various values, my
         * preferred ranged in [0.3 0.5] */
        protected const double ScaleReduction = 0.5;
        protected const double opacityCutoff = 0.05;
        protected Brush defaultBrush;
        protected double defaultRadius;
        protected double defaultThickness;
        protected DashStyle defaultDashStyle;
        public Boolean FillIn { get; set; }
        public double Radius { get; protected set; }
        public Boolean UniqueTextProperties { get; set; }

        public virtual Point Center
        {
            get { return (Point)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        public graphElement GraphElement { get; private set; }

        #region Dependency Properties
        public static readonly DependencyProperty CenterProperty
            = DependencyProperty.Register("Center",
                                          typeof(Point), typeof(IconShape),
                                          new FrameworkPropertyMetadata(new Point(),
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty SelectedProperty
            = DependencyProperty.Register("Selected",
                                          typeof(Boolean), typeof(IconShape),
                                          new FrameworkPropertyMetadata(false,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty ScaleFactorProperty
            = DependencyProperty.Register("ScaleFactor",
                                          typeof(double), typeof(IconShape),
                                          new FrameworkPropertyMetadata(1.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowNameProperty
            = DependencyProperty.Register("ShowName",
                                          typeof(Boolean), typeof(IconShape),
                                          new FrameworkPropertyMetadata(true,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowLabelsProperty
            = DependencyProperty.Register("ShowLabels",
                                          typeof(Boolean), typeof(IconShape),
                                          new FrameworkPropertyMetadata(true,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FontSizeProperty
            = DependencyProperty.Register("FontSize",
                                          typeof(double), typeof(IconShape),
                                          new FrameworkPropertyMetadata(12.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty DisplayTextDistanceProperty
            = DependencyProperty.Register("DisplayTextDistance",
                                          typeof(double), typeof(IconShape),
                                          new FrameworkPropertyMetadata(1.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty DisplayTextPositionProperty
            = DependencyProperty.Register("DisplayTextPosition",
                                          typeof(double), typeof(IconShape),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty DisplayTextProperty
            = DependencyProperty.Register("DisplayText",
                                          typeof(FormattedText), typeof(IconShape),
                                          new FrameworkPropertyMetadata(null,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TextPointProperty =
            DependencyProperty.Register("TextPoint",
                                        typeof(Point), typeof(IconShape),
                                        new FrameworkPropertyMetadata(new Point(),
                                                                      FrameworkPropertyMetadataOptions.AffectsRender));


        public static readonly DependencyProperty StrokeOpacityProperty
            = DependencyProperty.Register("StrokeOpacity",
                                          typeof(double), typeof(IconShape),
                                          new FrameworkPropertyMetadata(1.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));


        protected IconShape(graphElement e, GraphGUI gd, object textDisplayData,
           double maxOpacity, double radiusMultiplier, double radiusAddition, object arcController)
        {
            GraphElement = e;
            if (textDisplayData == null) textDisplayData = "";
            else textDisplayData = textDisplayData.ToString().Split(new[] { ':' })[0];
            var tDDataList = StringCollectionConverter.Convert(textDisplayData.ToString());
            double result;
            if ((tDDataList.Count > 1) && (double.TryParse(tDDataList[1], out result)))
                DisplayTextDistance = result;
            else DisplayTextDistance = double.NaN;
            if ((tDDataList.Count > 2) && (double.TryParse(tDDataList[2], out result)))
                DisplayTextPosition = result;
            else DisplayTextPosition = double.NaN;
            if ((tDDataList.Count > 3) && (double.TryParse(tDDataList[3], out result)))
                FontSize = result;
            else FontSize = double.NaN;

            var binding = new Binding
            {
                Source = gd,
                Mode = BindingMode.OneWay,
                Path = new PropertyPath(GraphGUI.ScaleFactorProperty)
            };
            SetBinding(ScaleFactorProperty, binding);
            binding = new Binding
            {
                Converter = new IconOpacityConverter(this, maxOpacity, radiusMultiplier, radiusAddition),
                Source = gd,
                Mode = BindingMode.OneWay,
                Path = new PropertyPath(GraphGUI.MouseLocationProperty)
            };
            SetBinding(StrokeOpacityProperty, binding);

            /* whether or not to show the text depends on various parameters */
            var multiBinding = new MultiBinding
            {
                Converter = new DisplayTextConverter(),
                Mode = BindingMode.OneWay,
                ConverterParameter = GraphElement
            };
            {
                binding = new Binding { Source = this, Path = new PropertyPath(ShowNameProperty) };
                multiBinding.Bindings.Add(binding);

                binding = new Binding { Source = this, Path = new PropertyPath(ShowLabelsProperty) };
                multiBinding.Bindings.Add(binding);

                binding = new Binding { Source = this, Path = new PropertyPath(FontSizeProperty) };
                multiBinding.Bindings.Add(binding);
            }
            SetBinding(DisplayTextProperty, multiBinding);

            /* where is text to be shown? */
            multiBinding = new MultiBinding
            {
                Converter = new PositionTextConverter(),
                ConverterParameter = arcController,
                Mode = BindingMode.OneWay
            };
            {
                binding = new Binding { Source = this, Path = new PropertyPath(DisplayTextDistanceProperty) };
                multiBinding.Bindings.Add(binding);

                binding = new Binding { Source = this, Path = new PropertyPath(DisplayTextPositionProperty) };
                multiBinding.Bindings.Add(binding);

                binding = new Binding { Source = this, Path = new PropertyPath(DisplayTextProperty) };
                multiBinding.Bindings.Add(binding);
            }
            SetBinding(TextPointProperty, multiBinding);
        }

        public Boolean Selected
        {
            get { return (Boolean)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }
        public double ScaleFactor
        {
            get { return (double)GetValue(ScaleFactorProperty); }
            set { SetValue(ScaleFactorProperty, value); }
        }

        public Boolean ShowName
        {
            get { return (Boolean)GetValue(ShowNameProperty); }
            set { SetValue(ShowNameProperty, value); }
        }

        public Boolean ShowLabels
        {
            get { return (Boolean)GetValue(ShowLabelsProperty); }
            set { SetValue(ShowLabelsProperty, value); }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public double DisplayTextDistance
        {
            get { return (double)GetValue(DisplayTextDistanceProperty); }
            set { SetValue(DisplayTextDistanceProperty, value); }
        }

        public double DisplayTextPosition
        {
            get { return (double)GetValue(DisplayTextPositionProperty); }
            set { SetValue(DisplayTextPositionProperty, value); }
        }

        public FormattedText DisplayText
        {
            get { return (FormattedText)GetValue(DisplayTextProperty); }
            set { SetValue(DisplayTextProperty, value); }
        }

        public Point TextPoint
        {
            get { return (Point)GetValue(TextPointProperty); }
            set { SetValue(TextPointProperty, value); }
        }

        public double StrokeOpacity
        {
            get { return (double)GetValue(StrokeOpacityProperty); }
            set { SetValue(StrokeOpacityProperty, value); }
        }
        #endregion

        internal string UpdateTag()
        {
            return GraphElement.name + "," + DisplayTextDistance + "," + DisplayTextPosition + ","
                + FontSize;
        }

    }
}