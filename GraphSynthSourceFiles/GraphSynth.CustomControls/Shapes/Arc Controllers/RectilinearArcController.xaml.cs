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
    ///   Interaction logic for RectilinearArcController.xaml
    /// </summary>
    public partial class RectilinearArcController : ArcController
    {
        #region Constructors
        public RectilinearArcController(Shape _displayArc, PathSegment seg, Point startPoint)
            : base(_displayArc)
        {
            var pseg = (PolyLineSegment)seg;
            if ((pseg.Points.Count == 2) && (pseg.Points[0].X == startPoint.X)
                && (pseg.Points[0].Y == startPoint.Y) && (pseg.Points[1].X == startPoint.X)
                && (pseg.Points[1].Y == startPoint.Y)) return;
            var n = pseg.Points.Count - 1;
            var xlength = pseg.Points[n].X - startPoint.X;
            var ylength = pseg.Points[n].Y - startPoint.Y;

            // if first segment is vertical
            if (pseg.Points[0].X == startPoint.X)
            {
                _xLengthFactor = 0.0;
                if (ylength == 0.0) _yLengthFactor = 0.0;
                else _yLengthFactor = (pseg.Points[0].Y - startPoint.Y) / ylength;
            }
            else
            {
                if (xlength == 0.0) _xLengthFactor = 0.0;
                else _xLengthFactor = (pseg.Points[0].X - startPoint.X) / xlength;
                if ((pseg.Points.Count >= 2) && (ylength != 0.0))
                    _yLengthFactor = (pseg.Points[1].Y - pseg.Points[0].Y) / ylength;
                else _yLengthFactor = 0.0;
            }
        }

        public RectilinearArcController(Shape _displayArc, double[] parameters)
            : base(_displayArc, parameters)
        {
        }

        public RectilinearArcController(IList<ArcController> selectedACs)
            : base(null)
        {
            XLengthFactor = ((RectilinearArcController)selectedACs[0]).XLengthFactor;
            YLengthFactor = ((RectilinearArcController)selectedACs[0]).YLengthFactor;
            Boolean differX = false, differY = false;
            for (var i = 1; i < selectedACs.Count; i++)
            {
                if (XLengthFactor == ((RectilinearArcController)selectedACs[i]).XLengthFactor)
                    differX = true;
                if (YLengthFactor == ((RectilinearArcController)selectedACs[i]).YLengthFactor)
                    differY = true;
            }
            if (differX) XLengthFactor = 0.0;
            if (differY) YLengthFactor = 0.0;
        }

        #endregion

        #region Shape Adjustment Parameters

        public static readonly DependencyProperty XLengthFactorProperty
            = DependencyProperty.Register("XLengthFactor",
                                          typeof(double), typeof(RectilinearArcController),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty YLengthFactorProperty
            = DependencyProperty.Register("YLengthFactor",
                                          typeof(double), typeof(RectilinearArcController),
                                          new FrameworkPropertyMetadata(0.0,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public double XLengthFactor
        {
            get { return (double)GetValue(XLengthFactorProperty); }
            set { SetValue(XLengthFactorProperty, value); }
        }

        public double YLengthFactor
        {
            get { return (double)GetValue(YLengthFactorProperty); }
            set { SetValue(YLengthFactorProperty, value); }
        }

        private double _xLengthFactor
        {
            get { return (XLengthFactor + 1) / 2; }
            set { XLengthFactor = 2 * value - 1; }
        }

        private double _yLengthFactor
        {
            get { return (YLengthFactor + 1) / 2; }
            set { YLengthFactor = 2 * value - 1; }
        }

        #endregion

        #region Required Override Methods
        internal override PathFigure DefineSegment()
        {
            SetupCenterPoints();

            double xMid, yMid;

            if (((_xLengthFactor >= 0) && (_xLengthFactor <= 1.0)) ||
                (Math.Abs(toPoint.X - fromPoint.X) > defaultLength))
                xMid = fromPoint.X + _xLengthFactor * (toPoint.X - fromPoint.X);
            else xMid = fromPoint.X + _xLengthFactor * MySign(toPoint.X - fromPoint.X) * defaultLength;
            if (((_yLengthFactor >= 0) && (_yLengthFactor <= 1.0)) ||
                (Math.Abs(toPoint.Y - fromPoint.Y) > defaultLength))
                yMid = fromPoint.Y + _yLengthFactor * (toPoint.Y - fromPoint.Y);
            else yMid = fromPoint.Y + _yLengthFactor * MySign(toPoint.Y - fromPoint.Y) * defaultLength;
            if ((_xLengthFactor == 0.0) && (_yLengthFactor != 0.0))
                fromAngle = MySign(_yLengthFactor) * MySign(toPoint.Y - fromPoint.Y) * Math.PI / 2;
            else if (_xLengthFactor == 0.0)
                fromAngle = (Math.PI / 2) - MySign(toPoint.X - fromPoint.X) * (Math.PI / 2);
            else
                fromAngle = (Math.PI / 2) - MySign(_xLengthFactor) * MySign(toPoint.X - fromPoint.X) * (Math.PI / 2);

            if (_yLengthFactor != 1.0)
                toAngle = MySign(1 - _yLengthFactor) * MySign(fromPoint.Y - toPoint.Y) * Math.PI / 2;
            else
                toAngle = (Math.PI / 2) - MySign(1 - _xLengthFactor) * MySign(toPoint.X - fromPoint.X) * (Math.PI / 2);

            //
            // From Point
            //
            /* shapeRotAngle is the angle that the shape has been rotated from 0 to 2*PI. 
             * We could have looked at the acos or asin of M11 and M21 but that would
             * only give us 0 to 180. */
            var shapeRotAngle = Math.Atan2(FromLocation.Value.M21, FromLocation.Value.M11);
            /* the effective radius from p1 is declared and then evaluated in the following condition. */
            var radius = findRadiusFrom(fromAngle, shapeRotAngle);

            if (((fromAngle == 0.0) || (fromAngle == Math.PI)) && (radius > Math.Abs(fromPoint.X - xMid)))
            {
                xMid = fromPoint.X;
                fromAngle = MySign(_yLengthFactor) * MySign(toPoint.Y - fromPoint.Y) * Math.PI / 2;
                radius = findRadiusTo(fromAngle, Math.Atan2(FromLocation.Value.M21, FromLocation.Value.M11));
            }
            else if (((fromAngle == Math.PI / 2) || (fromAngle == -Math.PI / 2)) && (radius > Math.Abs(fromPoint.Y - yMid)))
            {
                yMid = fromPoint.Y;
                fromAngle = (Math.PI / 2) - MySign(_xLengthFactor) * MySign(toPoint.X - fromPoint.X) * (Math.PI / 2);
                radius = findRadiusFrom(fromAngle, Math.Atan2(FromLocation.Value.M21, FromLocation.Value.M11));
            }
            /* from the radius, we can not find the point on the surface of the shape. */
            fromPoint = new Point(fromPoint.X + radius * Math.Cos(fromAngle), fromPoint.Y + radius * Math.Sin(fromAngle));


            //
            // To Point
            //  
            shapeRotAngle = Math.Atan2(ToLocation.Value.M21, ToLocation.Value.M11);
            /* the effective radius from p1 is declared and then evaluated in the following condition. */
            radius = findRadiusTo(toAngle, shapeRotAngle);

            if (((toAngle == 0.0) || (toAngle == Math.PI)) && (radius > Math.Abs(toPoint.X - xMid)))
            {
                xMid = toPoint.X;
                toAngle = MySign(1 - _yLengthFactor) * MySign(fromPoint.Y - toPoint.Y) * Math.PI / 2;
                radius = findRadiusTo(toAngle, Math.Atan2(FromLocation.Value.M21, FromLocation.Value.M11));
            }
            else if (((toAngle == Math.PI / 2) || (toAngle == -Math.PI / 2)) && (radius > Math.Abs(toPoint.Y - yMid)))
            {
                yMid = toPoint.Y;
                toAngle = (Math.PI / 2) - MySign(1 - _xLengthFactor) * MySign(fromPoint.X - toPoint.X) * (Math.PI / 2);
                radius = findRadiusTo(toAngle, Math.Atan2(FromLocation.Value.M21, FromLocation.Value.M11));
            }
            /* from the radius, we can not find the point on the surface of the shape. */
            toPoint = new Point(toPoint.X + radius * Math.Cos(toAngle), toPoint.Y + radius * Math.Sin(toAngle));

            /////////// drawing PolyLine ////////////////
            var pSegment = new PolyLineSegment();

            if (((fromPoint.X == toPoint.X) || (fromPoint.Y == toPoint.Y))
                && (_xLengthFactor >= 0.0) && (_xLengthFactor <= 1.0)
                && (_yLengthFactor >= 0.0) && (_yLengthFactor <= 1.0))
                pSegment.Points.Add(toPoint);
            else if (((_xLengthFactor == 0.0) || (_xLengthFactor == 1.0))
                     && ((_yLengthFactor == 0.0) || (_yLengthFactor == 1.0)))
            {
                if ((_xLengthFactor == 0.0) && (_yLengthFactor == 1.0))
                    pSegment.Points.Add(new Point(fromPoint.X, toPoint.Y));
                else pSegment.Points.Add(new Point(toPoint.X, fromPoint.Y));
                pSegment.Points.Add(toPoint);
            }
            else if ((_xLengthFactor == 0.0) || (_xLengthFactor == 1.0) || (_yLengthFactor == 0.0) ||
                     (_yLengthFactor == 1.0))
            {
                pSegment.Points.Add(_xLengthFactor == 0.0
                                        ? new Point(fromPoint.X, toPoint.Y)
                                        : new Point(toPoint.X, fromPoint.Y));
                pSegment.Points.Add(toPoint);
            }
            else
            {
                pSegment.Points.Add(new Point(xMid, fromPoint.Y));
                pSegment.Points.Add(new Point(xMid, yMid));
                pSegment.Points.Add(new Point(toPoint.X, yMid));
                pSegment.Points.Add(toPoint);
            }
            return new PathFigure
            {
                IsFilled = false,
                StartPoint = fromPoint,
                Segments = new PathSegmentCollection { pSegment }
            };
        }

        private static int MySign(double n)
        {
            if (n == 0.0) return 1;
            return Math.Sign(n);
        }

        public override double[] parameters
        {
            get { return new[] { XLengthFactor, YLengthFactor }; }
            set
            {
                XLengthFactor = value[0];
                YLengthFactor = value[1];
            }
        }

        internal override Point DetermineTextPoint(FormattedText text, double location, double distance)
        {
            if (location > 1.0) location = 0.5;
            var start = displayArc.arcBody.StartPoint;
            var endPts = ((PolyLineSegment)displayArc.arcBody.Segments[0]).Points;
            var numSegs = endPts.Count;
            if (numSegs == 0) return new Point();

            var lengths = new double[numSegs];

            var startsHoriz = false;
            var isHoriz = false;
            if (start.Y == endPts[0].Y) startsHoriz = isHoriz = true;

            var totalLength = 0.0;
            for (var i = 0; i < numSegs; i++)
            {
                if (isHoriz) lengths[i] += Math.Abs(start.X - endPts[i].X);
                else lengths[i] += Math.Abs(start.Y - endPts[i].Y);
                totalLength += lengths[i];
                isHoriz = !isHoriz;
                start = endPts[i];
            }
            var pLength = location * totalLength;
            var j = 0;
            isHoriz = startsHoriz;
            while ((j < numSegs) && (pLength - lengths[j] > 0))
            {
                pLength -= lengths[j];
                isHoriz = !isHoriz;
                j++;
            }
            if (j == numSegs) j--;

            var p = displayArc.arcBody.StartPoint;
            if (j > 0) p = endPts[j - 1];
            if (isHoriz)
            {
                p = endPts[j].X > p.X ? new Point(p.X + pLength, p.Y) : new Point(p.X - pLength, p.Y);
                return new Point(p.X - text.Width / 2, p.Y + (1 + distance) * text.Height / 2);
            }
            p = endPts[j].Y > p.Y ? new Point(p.X, p.Y + pLength) : new Point(p.X, p.Y - pLength);
            return new Point(p.X - (1 - distance) * text.Width / 2, p.Y + text.Height / 2);
        }

        protected override void DefineSliders()
        {
            InitializeComponent();

            var binding = new Binding
            {
                Source = sldtxtXFactor,
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(SldAndTextbox.ValueProperty)
            };
            SetBinding(XLengthFactorProperty, binding);

            binding = new Binding
            {
                Source = sldtxtYFactor,
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(SldAndTextbox.ValueProperty)
            };
            SetBinding(YLengthFactorProperty, binding);
        }

        #endregion
    }
}