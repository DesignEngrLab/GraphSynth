using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSynth.UI;

namespace GraphSynth.GraphDisplay
{
    /// <summary>
    ///   Interaction logic for CircleArcController.xaml
    /// </summary>
    public partial class CircleArcController : ArcController
    {
        #region Constructors
        /// <summary>
        ///   Initializes a new instance of the <see cref = "CircleArcController" /> class. This constructor is 
        ///   used to read in a shape from a a file and parse the controller parameters: sweep direction and
        ///   circleArcAngle. It essentially does the opposite of what define controller does.
        /// </summary>
        /// <param name = "_displayArc">The _display arc.</param>
        /// <param name = "seg">The seg.</param>
        /// <param name = "startPoint">The start point.</param>
        public CircleArcController(Shape _displayArc, PathSegment seg, Point startPoint)
            : base(_displayArc)
        {
            ArcSweepDirection = ((ArcSegment)seg).SweepDirection;
            var endPoint = ((ArcSegment)seg).Point;
            CircleArcAngle = 2 * Math.Asin((endPoint - startPoint).Length / (2 * ((ArcSegment)seg).Size.Width));
            CircleArcAngle = 180.0 * CircleArcAngle / Math.PI;
        }

        public CircleArcController(Shape _displayArc, double[] parameters)
            : base(_displayArc, parameters) { }

        public CircleArcController(List<ArcController> selectedACs)
            : base(null)
        {
            ArcSweepDirection = ((CircleArcController)selectedACs[0]).ArcSweepDirection;
            CircleArcAngle = ((CircleArcController)selectedACs[0]).CircleArcAngle;
            var differCAA = false;
            for (var i = 1; i < selectedACs.Count; i++)
                if (CircleArcAngle == ((CircleArcController)selectedACs[i]).CircleArcAngle) differCAA = true;

            if (differCAA) CircleArcAngle = 0.0;
        }

        #endregion

        #region Shape Adjustment Parameters

        private const double angleTolerance = 0.005;
        private const double Kmax = 250;
        private double reducedAngle;

        #region Circle Angle

        public static readonly DependencyProperty CircleArcAngleProperty
            = DependencyProperty.Register("CircleArcAngle",
                                          typeof(double), typeof(CircleArcController),
                                          new FrameworkPropertyMetadata(180.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public double CircleArcAngle
        {
            get { return (double)GetValue(CircleArcAngleProperty); }
            set { SetValue(CircleArcAngleProperty, value); }
        }

        private double _circleArcAngle
        {
            get { return Math.PI * CircleArcAngle / 180; }
            //set { CircleArcAngle = 180*value/Math.PI; }
        }

        #endregion

        #region Sweep Direction Angle

        public static readonly DependencyProperty ArcSweepDirectionProperty
            = DependencyProperty.Register("ArcSweepDirection",
                                          typeof(SweepDirection), typeof(CircleArcController),
                                          new FrameworkPropertyMetadata(SweepDirection.Clockwise,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public SweepDirection ArcSweepDirection
        {
            get { return (SweepDirection)GetValue(ArcSweepDirectionProperty); }
            set { SetValue(ArcSweepDirectionProperty, value); }
        }

        #endregion

        #endregion

        #region Required Override Methods
        internal override PathFigure DefineSegment()
        {
            SetupCenterPoints();

            /* in Circle Arc, straightLineLength ends up be the radius of the arc */
            straightLineLength = Math.Max(Math.Pow(defaultLength, 2), (toPoint - fromPoint).LengthSquared);
            straightLineLength /= (2 - 2 * Math.Cos(_circleArcAngle));
            straightLineLength = Math.Sqrt(straightLineLength);
            /* approachAngle is the angle of the line from 0 to 2*PI. ATAN2 is the only function
             * capable of returning a complete angle, since it takes the two argument independently. */
            double radius, baseAngle;

            if ((toPoint.Y == fromPoint.Y) && (toPoint.X == fromPoint.X))
                baseAngle = 5 * _circleArcAngle;
            else baseAngle = Math.Atan2(toPoint.Y - fromPoint.Y, toPoint.X - fromPoint.X);
            double fromSide = 1;
            double toSide = 1;
            var adjustAngle = 0.375 * _circleArcAngle;
            if (ArcSweepDirection == SweepDirection.Clockwise) fromSide = -1;
            else toSide = -1;
            //
            // From Point
            //
            var shapeRotAngle = Math.Atan2(FromLocation.Value.M21, FromLocation.Value.M11);
            double oldAngle;
            fromAngle = baseAngle + fromSide * adjustAngle;
            var k = 0;
            do
            {
                oldAngle = fromAngle;
                radius = findRadiusFrom(fromAngle, shapeRotAngle);
                adjustAngle = Math.Acos(radius / (2 * straightLineLength)) - (Math.PI - _circleArcAngle) / 2;
                fromAngle = baseAngle + fromSide * adjustAngle;
                k++;
            } while ((Math.Abs(fromAngle - oldAngle) > angleTolerance) && (k < Kmax));
            fromPoint = new Point(fromPoint.X + radius * Math.Cos(fromAngle), fromPoint.Y + radius * Math.Sin(fromAngle));
            reducedAngle = _circleArcAngle - Math.Acos(1 - ((radius * radius) / (2 * straightLineLength * straightLineLength)));

            // To Point
            //
            adjustAngle = 0.375 * _circleArcAngle;
            baseAngle += Math.PI;
            shapeRotAngle = Math.Atan2(ToLocation.Value.M21, ToLocation.Value.M11);
            toAngle = baseAngle + toSide * adjustAngle;
            k = 0;
            do
            {
                oldAngle = toAngle;
                radius = findRadiusTo(toAngle, shapeRotAngle);
                adjustAngle = Math.Acos(radius / (2 * straightLineLength)) - (Math.PI - _circleArcAngle) / 2;
                toAngle = baseAngle + toSide * adjustAngle;
                k++;
            } while ((Math.Abs(toAngle - oldAngle) > angleTolerance) && (k < Kmax));
            toPoint = new Point(toPoint.X + radius * Math.Cos(toAngle), toPoint.Y + radius * Math.Sin(toAngle));
            reducedAngle -= Math.Acos(1 - ((radius * radius) / (2 * straightLineLength * straightLineLength)));


            /* in Circle Arc, straightLineLength ends up be the radius of the arc */
            straightLineLength = Math.Max(Math.Pow(defaultLength, 2), (toPoint - fromPoint).LengthSquared);

            straightLineLength /= (2 - 2 * Math.Cos(reducedAngle));
            straightLineLength = Math.Sqrt(straightLineLength);

            ////////////////// Completing Circular Arc Segment ////////////////////
            return new PathFigure
            {
                IsFilled = false,
                StartPoint = fromPoint,
                Segments = new PathSegmentCollection {
                    new ArcSegment {
                        Point = toPoint,
                        SweepDirection = ArcSweepDirection,
                        IsLargeArc = reducedAngle > Math.PI,
                        Size = new Size(straightLineLength, straightLineLength)
                    }
                }
            };
        }

        public override double[] parameters
        {
            get { return new[] { CircleArcAngle, (double)ArcSweepDirection }; }
            set
            {
                CircleArcAngle = value[0];
                ArcSweepDirection = (SweepDirection)value[1];
            }
        }

        internal override Point DetermineTextPoint(FormattedText text, double location, double distance)
        {
            var end = displayArc.arcBody.StartPoint;
            var cSeg = (ArcSegment)displayArc.arcBody.Segments[0];
            var start = cSeg.Point;
            var radius = cSeg.Size.Width;
            var circum = reducedAngle * radius;
            var textAngle = location * circum / radius;
            var angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
            var chordAngle = (Math.PI - reducedAngle) / 2;
            // double chordAngle = (Math.PI - _circleArcAngle) / 2;
            if (ArcSweepDirection == SweepDirection.Clockwise)
                angle -= chordAngle;
            else angle += chordAngle;
            var center = new Point(start.X + radius * Math.Cos(angle), start.Y + radius * Math.Sin(angle));
            if (ArcSweepDirection == SweepDirection.Clockwise)
                angle = angle - Math.PI - textAngle;
            else angle = angle - Math.PI + textAngle;

            var textRadius = Math.Min(Math.Abs(text.Width / (2 * Math.Cos(angle))),
                                      Math.Abs(text.Height / (2 * Math.Sin(angle))));

            return new Point(center.X - text.Width / 2 + (radius + distance * textRadius) * Math.Cos(angle),
                             center.Y + (radius + text.Height / 2 + distance * textRadius) * Math.Sin(angle));
        }

        protected override void DefineSliders()
        {
            InitializeComponent();

            var binding = new Binding
            {
                Source = sldtxtCircleAngle,
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(SldAndTextbox.ValueProperty)
            };
            SetBinding(CircleArcAngleProperty, binding);

            optClockwise.IsChecked = (ArcSweepDirection == SweepDirection.Counterclockwise);
            optAntiClockwise.IsChecked = !optClockwise.IsChecked;
            var multiBinding = new MultiBinding
            {
                Converter = new RadioButtonsCheckedBoolean(),
                Mode = BindingMode.TwoWay
            };
            binding = new Binding { Source = optClockwise, Path = new PropertyPath(ToggleButton.IsCheckedProperty) };
            multiBinding.Bindings.Add(binding);
            binding = new Binding { Source = optAntiClockwise, Path = new PropertyPath(ToggleButton.IsCheckedProperty) };
            multiBinding.Bindings.Add(binding);
            multiBinding.Mode = BindingMode.TwoWay;

            SetBinding(ArcSweepDirectionProperty, multiBinding);
        }

        #endregion
    }

    internal class ClockWiseCheckedBoolean : IValueConverter
    {

        #region Implementation of IValueConverter

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Boolean)value) return SweepDirection.Clockwise;
            return SweepDirection.Counterclockwise;
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((SweepDirection)value == SweepDirection.Clockwise);
        }

        #endregion
    }
    internal class RadioButtonsCheckedBoolean : IMultiValueConverter
    {
        #region Implementation of IMultiValueConverter

        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of <see cref="T:System.Windows.DependencyProperty"/>.<see cref="F:System.Windows.DependencyProperty.UnsetValue"/> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> if it is available, or else will use the default value.A return value of <see cref="T:System.Windows.Data.Binding"/>.<see cref="F:System.Windows.Data.Binding.DoNothing"/> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> or the default value.
        /// </returns>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding"/> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue"/> indicates that the source binding has no value to provide for conversion.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Boolean)values[0]) return SweepDirection.Clockwise;
            return SweepDirection.Counterclockwise;
        }

        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        /// <param name="value">The value that the binding target produces.</param><param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if ((SweepDirection)value == SweepDirection.Clockwise)
                return new object[] { true, false };
            return new object[] { false, true };

        }

        #endregion
    }
}