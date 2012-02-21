using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    public abstract class ArcController : AbstractController
    {
        private readonly Random rand = new Random();

        protected const double defaultLength = 50.0;

        protected ArcController(Shape displayShape)
            : base(displayShape)
        {
        }

        protected ArcController(Shape displayShape, double[] parameters)
            : base(displayShape, parameters)
        {
        }

        #region ShortCut Properties to ArcShapeBaseClass

        protected Transform FromLocation
        {
            get { return displayArc.FromLocation; }
            set { displayArc.FromLocation = value; }
        }

        protected Transform ToLocation
        {
            get { return displayArc.ToLocation; }
            set { displayArc.ToLocation = value; }
        }

        protected double FromWidth
        {
            get { return displayArc.FromWidth; }
            set { displayArc.FromWidth = value; }
        }

        protected double ToWidth
        {
            get { return displayArc.ToWidth; }
            set { displayArc.ToWidth = value; }
        }

        protected double FromHeight
        {
            get { return displayArc.FromHeight; }
            set { displayArc.FromHeight = value; }
        }

        private double ToHeight
        {
            get { return displayArc.ToHeight; }
            //set { displayArc.ToHeight = value; }
        }

        protected Point toPoint
        {
            get { return displayArc.toPoint; }
            set { displayArc.toPoint = value; }
        }

        protected Point fromPoint
        {
            get { return displayArc.fromPoint; }
            set { displayArc.fromPoint = value; }
        }

        protected double toAngle
        {
            get { return displayArc.toAngle; }
            set { displayArc.toAngle = value; }
        }

        protected double fromAngle
        {
            get { return displayArc.fromAngle; }
            set { displayArc.fromAngle = value; }
        }

        protected double straightLineLength
        {
            get { return displayArc.straightLineLength; }
            set { displayArc.straightLineLength = value; }
        }

        #endregion

        #region Set up Endpoints
        protected void SetupCenterPoints()
        {
            if ((FromLocation != null) && (!double.IsNaN(FromLocation.Value.OffsetX))
                && (!double.IsNaN(FromWidth)))
                fromPoint = new Point(FromLocation.Value.OffsetX + FromWidth / 2,
                                      FromLocation.Value.OffsetY + FromHeight / 2);
            else if ((FromLocation != null) && (!double.IsNaN(FromLocation.Value.OffsetX)))
                fromPoint = new Point(FromLocation.Value.OffsetX, FromLocation.Value.OffsetY);
            else if (double.IsNaN(fromPoint.X))
                fromPoint = new Point();

            if ((ToLocation != null) && (!double.IsNaN(ToLocation.Value.OffsetX))
                && (!double.IsNaN(ToWidth)))
                toPoint = new Point(ToLocation.Value.OffsetX + ToWidth / 2,
                    ToLocation.Value.OffsetY + ToHeight / 2);
            else if ((ToLocation != null) && (!double.IsNaN(ToLocation.Value.OffsetX)))
                toPoint = new Point(ToLocation.Value.OffsetX, ToLocation.Value.OffsetY);
            else if (double.IsNaN(toPoint.X))
                toPoint = new Point();
        }



        protected double findRadiusFrom(double approachAngle, double shapeRotAngle)
        {
            if (FromWidth * FromHeight == 0.0) return 0.0;
            /* shapeRotAngle is the angle that the shape has been rotated from 0 to 2*PI. 
             * We could have looked at the acos or asin of M11 and M21 but that would
             * only give us 0 to 180. */
            if (displayArc.FromShape is Rectangle)
                /* if the shape is a Rectangle, then we extend the sides of the rectangle indefinitely
                 * this will make two intersections with the line segment connecting the two points. 
                 * the smaller one is on the actual shape. */
                return Math.Min(Math.Abs(FromWidth / (2 * Math.Cos(approachAngle - shapeRotAngle))),
                                Math.Abs(FromHeight / (2 * Math.Sin(approachAngle - shapeRotAngle))));

            /* Else, we assume it to be an ellipse or at least close to that. This radius is the function
             * for the radius of an ellipse. Note that most ellipse equations deal with half-widths and
             * half-heights. The equation looks a little different when using the full values here. */
            return (FromWidth * FromHeight)
                   / (2 * Math.Sqrt(Math.Pow(FromWidth * Math.Sin(approachAngle - shapeRotAngle), 2) +
                                 Math.Pow(FromHeight * Math.Cos(approachAngle - shapeRotAngle), 2)));

        }
        protected double findRadiusTo(double approachAngle, double shapeRotAngle)
        {
            if (ToWidth * ToHeight == 0.0) return 0.0;
            if (displayArc.ToShape is Rectangle)
                return Math.Min(Math.Abs(ToWidth / (2 * Math.Cos(approachAngle - shapeRotAngle))),
                                Math.Abs(ToHeight / (2 * Math.Sin(approachAngle - shapeRotAngle))));
            return (ToWidth * ToHeight)
                   / (2 * Math.Sqrt(Math.Pow(ToWidth * Math.Sin(approachAngle - shapeRotAngle), 2) +
                                 Math.Pow(ToHeight * Math.Cos(approachAngle - shapeRotAngle), 2)));
        }

        #endregion

        public ArcShape displayArc { get { return (ArcShape)displayShape; } }

        internal abstract PathFigure DefineSegment();

        protected override void SlidersValuesChanged(object sender, RoutedEventArgs e)
        {
            var gui = (GraphGUI)displayArc.Parent;
            if (gui != null) gui.ArcPropertyChanged((arc)displayArc.icon.GraphElement);
            Redraw();
        }

    }
}