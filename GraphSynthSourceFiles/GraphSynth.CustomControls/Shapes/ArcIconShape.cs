using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    public class ArcIconShape : IconShape
    {
        const double radiusMultiplier = 2.0;
        const double radiusAddition = 0.0;
        const double maxOpacity = 0.8;
        #region Additional Dependency Properties
        public static readonly DependencyProperty StrokeThicknessProperty
            = DependencyProperty.Register("SelectedStrokeThickness",
                                          typeof(double), typeof(ArcIconShape),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));


        public double SelectedStrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty) + 2 * selectedBufferThickness; }
        }
        public double DefaultStrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty) + 2 * defaultThickness; }
        }
        #endregion

        #region Fields
        private readonly ArcShape arcShape;
        private readonly Brush selectedBrush;
        private readonly double selectedBufferThickness;
        private readonly DashStyle selectedDashStyle;
        #endregion

        #region Constructor
        public ArcIconShape(graphElement a, ArcShape arcShape, GraphGUI gd)
            : base(a, gd, arcShape.Tag, maxOpacity, radiusMultiplier, radiusAddition, arcShape.Controller)
        {
            this.arcShape = arcShape;
            this.arcShape.icon = this;

            var dt = (DataTemplate)Application.Current.Resources["NodeIconShape"];
            var penData = (Shape)dt.LoadContent();

            defaultBrush = penData.Stroke;
            defaultThickness = penData.StrokeThickness;
            defaultDashStyle = new DashStyle(penData.StrokeDashArray, 0);

            dt = (DataTemplate)Application.Current.Resources["SelectedArcPen"];
            penData = (Shape)dt.LoadContent();

            selectedBrush = penData.Stroke;
            selectedBufferThickness = penData.StrokeThickness;
            selectedDashStyle = new DashStyle(penData.StrokeDashArray, 0);


            /* bind the thickness property s.t. the iconArc shows up thick enough. */
            var binding = new Binding
              {
                  Source = arcShape,
                  Path = new PropertyPath(Shape.StrokeThicknessProperty),
                  Mode = BindingMode.OneWay
              };
            SetBinding(StrokeThicknessProperty, binding);

        }

        #endregion

        protected override void OnRender(DrawingContext dc)
        {
            if (Selected || (StrokeOpacity >= opacityCutoff))
            {
                var geometry = new PathGeometry();
                geometry.Figures.Add(arcShape.arcBody);
                if (arcShape.directed) geometry.Figures.Add(arcShape.toArrowHead);
                if (arcShape.doublyDirected) geometry.Figures.Add(arcShape.fromArrowHead);
                Pen pen;
                if (Selected)
                    pen = new Pen
                    {
                        Brush = selectedBrush,
                        Thickness = SelectedStrokeThickness,
                        DashStyle = selectedDashStyle
                    };
                else
                {
                    var brush = defaultBrush.Clone();
                    brush.Opacity = StrokeOpacity;
                    pen = new Pen
                    {
                        Brush = brush,
                        Thickness = DefaultStrokeThickness,
                        DashStyle = defaultDashStyle
                    };
                }
                dc.DrawGeometry(selectedBrush, pen, geometry);
            }
            if ((ShowName || ShowLabels) && (DisplayText != null))
            {
                var mbe = BindingOperations.GetMultiBindingExpression(this, TextPointProperty);
                if (mbe != null) mbe.UpdateTarget();
                dc.PushTransform(new MatrixTransform(1, 0, 0, -1, TextPoint.X, TextPoint.Y));
                dc.DrawText(DisplayText, new Point());
                dc.Pop();
            }
            Panel.SetZIndex(this, int.MaxValue);
            var RadPoint = new Vector(
                Math.Min(Math.Abs(arcShape.FromLocation.Value.OffsetX - TextPoint.X),
                Math.Abs(arcShape.ToLocation.Value.OffsetX - TextPoint.X)),
                Math.Min(Math.Abs(arcShape.FromLocation.Value.OffsetY - TextPoint.Y),
                Math.Abs(arcShape.ToLocation.Value.OffsetY - TextPoint.Y)));
            Radius = RadPoint.Length;
        }

        #region Overrides of IconShape
        public override Point Center
        {
            get { return TextPoint; }
            set { throw new NotImplementedException(); }
        }
        #endregion
    }
}