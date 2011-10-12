using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphSynth.GraphDisplay
{
    /// <summary>
    ///   Interaction logic for StraightArcController.xaml
    /// </summary>
    public partial class StraightArcController : ArcController
    {
        #region Constructors
        public StraightArcController(Shape _displayArc)
            : base(_displayArc)
        {
        }
        public StraightArcController(Shape displayShape, double[] parameters)
            : base(displayShape, parameters)
        {
        }

        public StraightArcController(List<ArcController> selectedACs)
            : base(null)
        {
        }

        #endregion

        #region Required Override Methods
        internal override PathFigure DefineSegment()
        {
            SetupCenterPoints();

            straightLineLength = Math.Max(defaultLength, (toPoint - fromPoint).Length);

            var approachAngle = Math.Atan2(toPoint.Y - fromPoint.Y,
                                           toPoint.X - fromPoint.X);
            fromAngle = approachAngle;
            toAngle = Math.PI + approachAngle;

            /* shapeRotAngle is the angle that the shape has been rotated from 0 to 2*PI. 
             * We could have looked at the acos or asin of M11 and M21 but that would
             * only give us 0 to 180. */
            var shapeRotAngle = Math.Atan2(FromLocation.Value.M21, FromLocation.Value.M11);
            /* the effective radius from FromLocation is declared and then evaluated in the following condition. */
            var radius = findRadiusFrom(fromAngle, shapeRotAngle);
            /* from the radius, we can not find the point on the surface of the shape. */
            fromPoint = new Point(fromPoint.X + radius * Math.Cos(fromAngle),
                                  fromPoint.Y + radius * Math.Sin(fromAngle));
            // now, for the To arrow
            shapeRotAngle = Math.Atan2(ToLocation.Value.M21, ToLocation.Value.M11);
            radius = findRadiusTo(toAngle, shapeRotAngle);
            toPoint = new Point(toPoint.X + radius * Math.Cos(toAngle),
                                toPoint.Y + radius * Math.Sin(toAngle));

            ////////// Completing Straight line //////////////////
            return new PathFigure
                       {
                           IsFilled = false,
                           StartPoint = fromPoint,
                           Segments = new PathSegmentCollection { new LineSegment { Point = toPoint } }
                       };
        }

        public override double[] parameters
        {
            get { return new double[0]; }
            set { }
        }

        internal override Point DetermineTextPoint(FormattedText text, double location, double distance)
        {
            var start = displayArc.arcBody.StartPoint;
            var end = ((LineSegment)displayArc.arcBody.Segments[0]).Point;
            var v = (end - start);
            var length = v.Length;
            v.Normalize();
            length *= location;
            var p = start + v * length;
            var newY = v.X;
            v.X = -v.Y;
            v.Y = newY;

            var radius = Math.Min(Math.Abs(text.Width / (2 * v.X)),
                                  Math.Abs(text.Height / (2 * v.Y)));
            p = p + v * radius * distance;
            return new Point(p.X - text.Width / 2, p.Y + text.Height / 2);
        }
        #endregion

        #region UI control
        protected override void DefineSliders()
        {
            InitializeComponent();
        }
        #endregion
    }
}