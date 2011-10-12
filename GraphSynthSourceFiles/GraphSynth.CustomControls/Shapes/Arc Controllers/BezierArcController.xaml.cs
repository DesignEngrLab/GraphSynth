using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSynth.UI;

namespace GraphSynth.GraphDisplay
{
    /// <summary>
    ///   Interaction logic for BezierArcController.xaml
    /// </summary>
    public partial class BezierArcController : ArcController
    {
        #region Constructors

        public BezierArcController(Shape _displayArc, PathSegment seg, Point startPoint)
            : base(_displayArc)
        {
            var bseg = (BezierSegment)seg;
            straightLineLength = Math.Max(defaultLength, (bseg.Point3 - startPoint).Length);
            FromLength = (startPoint - bseg.Point1).Length/ straightLineLength;
            
            ToLength = (bseg.Point2 - bseg.Point3).Length/ straightLineLength;

            var approachAngle = Math.Atan2(bseg.Point3.Y - startPoint.Y, bseg.Point3.X - startPoint.X);
            fromAngle = Math.Atan2(bseg.Point1.Y - startPoint.Y, bseg.Point1.X - startPoint.X);
            toAngle = Math.Atan2(bseg.Point2.Y - bseg.Point3.Y, bseg.Point2.X - bseg.Point3.X);
            _fromAngleBezier = fromAngle - approachAngle;
            _toAngleBezier = toAngle - approachAngle - Math.PI;
        }

        public BezierArcController(Shape _displayArc, double[] parameters)
            : base(_displayArc, parameters)
        {
        }


        public BezierArcController(IList<ArcController> selectedACs)
            : base(null)
        {
            FromAngleBezier = ((BezierArcController)selectedACs[0]).FromAngleBezier;
            ToAngleBezier = ((BezierArcController)selectedACs[0]).ToAngleBezier;
            FromLength = ((BezierArcController)selectedACs[0]).FromLength;
            ToLength = ((BezierArcController)selectedACs[0]).ToLength;
            Boolean differFA = false, differTA = false, differFL = false, differTL = false;
            for (var i = 1; i < selectedACs.Count; i++)
            {
                if (((BezierArcController)selectedACs[i]).FromAngleBezier != FromAngleBezier)
                    differFA = true;
                if (((BezierArcController)selectedACs[i]).ToAngleBezier != ToAngleBezier)
                    differTA = true;
                if (((BezierArcController)selectedACs[i]).FromLength != FromLength)
                    differFL = true;
                if (((BezierArcController)selectedACs[i]).ToLength != ToLength)
                    differTL = true;
            }
            if (differFA) FromAngleBezier = 0.0;
            if (differTA) ToAngleBezier = 0.0;
            if (differFL) FromLength = 1.0;
            if (differTL) ToLength = 1.0;
        }

        #endregion

        #region Shape Adjustment Parameters

        #region From Angle Bezier

        public static readonly DependencyProperty FromAngleBezierProperty
            = DependencyProperty.Register("FromAngleBezier",
                                          typeof(double), typeof(BezierArcController),
                                          new FrameworkPropertyMetadata(30.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public double FromAngleBezier
        {
            get { return (double)GetValue(FromAngleBezierProperty); }
            set { SetValue(FromAngleBezierProperty, value); }
        }

        private double _fromAngleBezier
        {
            get { return Math.PI * FromAngleBezier / 180; }
            set { FromAngleBezier = 180 * value / Math.PI; }
        }

        #endregion

        #region To Angle Bezier

        public static readonly DependencyProperty ToAngleBezierProperty
            = DependencyProperty.Register("ToAngleBezier",
                                          typeof(double), typeof(BezierArcController),
                                          new FrameworkPropertyMetadata(-30.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public double ToAngleBezier
        {
            get { return (double)GetValue(ToAngleBezierProperty); }
            set { SetValue(ToAngleBezierProperty, value); }
        }

        private double _toAngleBezier
        {
            get { return Math.PI * ToAngleBezier / 180; }
            set { ToAngleBezier = 180 * value / Math.PI; }
        }

        #endregion

        #region From Length

        public static readonly DependencyProperty FromLengthProperty
            = DependencyProperty.Register("FromLength",
                                          typeof(double), typeof(BezierArcController),
                                          new FrameworkPropertyMetadata(1.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public double FromLength
        {
            get { return (double)GetValue(FromLengthProperty); }
            set { SetValue(FromLengthProperty, value); }
        }

        #endregion

        #region To Length

        public static readonly DependencyProperty ToLengthProperty
            = DependencyProperty.Register("ToLength",
                                          typeof(double), typeof(BezierArcController),
                                          new FrameworkPropertyMetadata(1.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public double ToLength
        {
            get { return (double)GetValue(ToLengthProperty); }
            set { SetValue(ToLengthProperty, value); }
        }

        #endregion

        #endregion

        #region Required Override Methods

        internal override PathFigure DefineSegment()
        {
            SetupCenterPoints();

            straightLineLength = Math.Max(defaultLength, (toPoint - fromPoint).Length);

            var approachAngle = Math.Atan2(toPoint.Y - fromPoint.Y, toPoint.X - fromPoint.X);
            fromAngle = _fromAngleBezier + approachAngle;
            toAngle = Math.PI + _toAngleBezier + approachAngle;

            /* shapeRotAngle is the angle that the shape has been rotated from 0 to 2*PI. 
             * We could have looked at the acos or asin of M11 and M21 but that would
             * only give us 0 to 180. */
            var shapeRotAngle = Math.Atan2(FromLocation.Value.M21, FromLocation.Value.M11);
            /* the effective radius from FromLocation is declared and then evaluated in the following condition. */
            var radius = findRadiusFrom(fromAngle, shapeRotAngle);
            /* from the radius, we can not find the point on the surface of the shape. */
            fromPoint = new Point(fromPoint.X + radius * Math.Cos(fromAngle), fromPoint.Y + radius * Math.Sin(fromAngle));

            shapeRotAngle = Math.Atan2(ToLocation.Value.M21, ToLocation.Value.M11);
            radius = findRadiusTo(toAngle, shapeRotAngle);
            toPoint = new Point(toPoint.X + radius * Math.Cos(toAngle), toPoint.Y + radius * Math.Sin(toAngle));

            ///////////////// Completing Bezier Curve ///////////////////// 
            return new PathFigure
            {
                IsFilled = false,
                StartPoint = fromPoint,
                Segments = new PathSegmentCollection {
                    new BezierSegment {
                        Point1 = new Point(
                                                                 fromPoint.X +
                                                                 FromLength*straightLineLength*Math.Cos(fromAngle),
                                                                 fromPoint.Y +
                                                                 FromLength*straightLineLength*Math.Sin(fromAngle)),
                                                             Point2 =
                                                                 new Point(
                                                                 toPoint.X +
                                                                 ToLength*straightLineLength*Math.Cos(toAngle),
                                                                 toPoint.Y +
                                                                 ToLength*straightLineLength*Math.Sin(toAngle)),
                                                             Point3 = toPoint
                                                         }
                                                 }
            };
        }

        public override double[] parameters
        {
            get { return new[] { FromAngleBezier, ToAngleBezier, FromLength, ToLength }; }
            set
            {
                FromAngleBezier = value[0];
                ToAngleBezier = value[1];
                FromLength = value[2];
                ToLength = value[3];
            }
        }

        internal override Point DetermineTextPoint(FormattedText text, double loc, double distance)
        {
            var start = displayArc.arcBody.StartPoint;
            var pt1 = ((BezierSegment)displayArc.arcBody.Segments[0]).Point1;
            var pt2 = ((BezierSegment)displayArc.arcBody.Segments[0]).Point2;
            var end = ((BezierSegment)displayArc.arcBody.Segments[0]).Point3;
            var opploc = 1 - loc;

            var x = opploc * opploc * opploc * start.X +
                    3 * loc * opploc * opploc * pt1.X +
                    3 * loc * loc * opploc * pt2.X +
                    loc * loc * loc * end.X;
            var dx_dloc = -3 * opploc * opploc * start.X +
                          (3 * opploc * opploc - 6 * loc * opploc) * pt1.X +
                          (6 * loc - 9 * loc * loc) * pt2.X +
                          3 * loc * loc * end.X;

            var y = opploc * opploc * opploc * start.Y +
                    3 * loc * opploc * opploc * pt1.Y +
                    3 * loc * loc * opploc * pt2.Y +
                    loc * loc * loc * end.Y;
            var dy_dloc = -3 * opploc * opploc * start.Y +
                          (3 * opploc * opploc - 6 * loc * opploc) * pt1.Y +
                          (6 * loc - 9 * loc * loc) * pt2.Y +
                          3 * loc * loc * end.Y;

            var p = new Point(x, y);

            var v = new Vector(-dy_dloc, dx_dloc);
            v.Normalize();
            var radius = Math.Min(Math.Abs(text.Width / (2 * v.X)),
                                  Math.Abs(text.Height / (2 * v.Y)));

            p = p + v * radius * distance;
            return new Point(p.X - text.Width / 2, p.Y + text.Height / 2);
        }

        protected override void DefineSliders()
        {
            InitializeComponent();
            var binding = new Binding
            {
                Source = sldtxtBFromAngle,
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(SldAndTextbox.ValueProperty)
            };
            SetBinding(FromAngleBezierProperty, binding);

            binding = new Binding
            {
                Source = sldtxtBToAngle,
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(SldAndTextbox.ValueProperty)
            };
            SetBinding(ToAngleBezierProperty, binding);

            binding = new Binding
            {
                Source = sldtxtFromLength,
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(SldAndTextbox.ValueProperty)
            };
            SetBinding(FromLengthProperty, binding);

            binding = new Binding
            {
                Source = sldtxtToLength,
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(SldAndTextbox.ValueProperty)
            };
            SetBinding(ToLengthProperty, binding);
        }

        #endregion
    }
}