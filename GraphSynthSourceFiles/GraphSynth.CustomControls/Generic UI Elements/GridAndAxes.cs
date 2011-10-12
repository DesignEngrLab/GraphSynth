using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GraphSynth.GraphDisplay
{
    public class GridAndAxes : FrameworkElement
    {
        #region Grid Field and Properties

        #region Fields

        private Brush _gridColor = Brushes.Black;
        private double _gridOpacity = 0.3;
        private double _gridSpacing = 24.0;
        private double _gridThick = 0.25;

        #endregion

        #region Properties

        public double GridSpacing
        {
            get { return _gridSpacing; }
            set
            {
                if (_gridSpacing == value) return;
                _gridSpacing = value;
                InvalidateVisual();
            }
        }

        public double GridThick
        {
            get { return _gridThick; }
            set
            {
                if (_gridThick == value) return;
                _gridThick = value;
                InvalidateVisual();
            }
        }

        public Brush GridColor
        {
            get
            {
                _gridColor = _gridColor.Clone();
                _gridColor.Opacity = GridOpacity;
                return _gridColor;
            }
            set
            {
                if (_gridColor == value) return;
                _gridColor = value;
                InvalidateVisual();
            }
        }

        public double GridOpacity
        {
            get { return _gridOpacity; }
            set
            {
                if (_gridOpacity == value) return;
                _gridOpacity = value;
                InvalidateVisual();
            }
        }

        #endregion

        #endregion

        #region Axes Fields and Properties

        #region Fields

        private Brush _axesColor = Brushes.Black;
        private double _axesOpacity = 1.0;
        private double _axesThick = 0.5;
        private double _originX, _originY, _windowHeight, _windowWidth;

        #endregion

        #region Properties

        public double AxesThick
        {
            get { return _axesThick; }
            set
            {
                if (_axesThick == value) return;
                _axesThick = value;
                InvalidateVisual();
            }
        }

        public Brush AxesColor
        {
            get
            {
                _axesColor = _axesColor.Clone();
                _axesColor.Opacity = AxesOpacity;
                return _axesColor;
            }
            set
            {
                if (_axesColor == value) return;
                _axesColor = value;
                InvalidateVisual();
            }
        }

        public double AxesOpacity
        {
            get { return _axesOpacity; }
            set
            {
                if (_axesOpacity == value) return;
                _axesOpacity = value;
                InvalidateVisual();
            }
        }


        public Point Origin
        {
            get { return new Point(_originX, _originY); }
            set
            {
                if (_originX != value.X)
                {
                    _originX = value.X;
                    InvalidateVisual();
                }
                if (_originY == value.Y) return;
                _originY = value.Y;
                InvalidateVisual();
            }
        }

        public double WindowHeight
        {
            get { return _windowHeight; }
            set
            {
                if (_windowHeight == value) return;
                _windowHeight = value;
                InvalidateVisual();
            }
        }

        public double WindowWidth
        {
            get { return _windowWidth; }
            set
            {
                if (_windowWidth == value) return;
                _windowWidth = value;
                InvalidateVisual();
            }
        }

        #endregion

        #endregion

        #region Constructor

        public GridAndAxes(double bufferRadius)
        {
            Origin = new Point(bufferRadius, bufferRadius);
        }

        #endregion

        protected override void OnRender(DrawingContext dc)
        {
            var lineThicknessHoriz = GridThick / WindowHeight;
            var gapThicknessHoriz = (GridSpacing - GridThick) / WindowHeight;
            var lineThicknessVert = GridThick / WindowWidth;
            var gapThicknessVert = (GridSpacing - GridThick) / WindowWidth;

            /* draw one special horizontal line to define the vertical grid lines */
            var gridPen = new Pen(GridColor, WindowHeight);
            gridPen.DashCap = PenLineCap.Flat;
            var dashArray = new DoubleCollection { lineThicknessHoriz, gapThicknessHoriz };

            gridPen.DashStyle = new DashStyle(dashArray, (-Origin.X % GridSpacing) / WindowHeight);
            dc.DrawLine(gridPen, new Point(0.0, WindowHeight / 2), new Point(WindowWidth, WindowHeight / 2));

            /* draw one special vertical line to define the horizontal grid lines */
            gridPen = new Pen(GridColor, WindowWidth);
            gridPen.DashCap = PenLineCap.Flat;
            dashArray = new DoubleCollection { lineThicknessVert, gapThicknessVert };
            gridPen.DashStyle = new DashStyle(dashArray, (-Origin.Y % GridSpacing) / WindowWidth);
            dc.DrawLine(gridPen, new Point((WindowWidth / 2), 0.0), new Point((WindowWidth / 2), WindowHeight));

            var axesBrush = AxesColor;
            axesBrush.Opacity = AxesOpacity;
            var axesPen = new Pen(axesBrush, AxesThick);

            dc.DrawLine(axesPen, new Point(0, Origin.Y), new Point(WindowWidth, Origin.Y));
            dc.DrawLine(axesPen, new Point(Origin.X, 0), new Point(Origin.X, WindowHeight));
            Panel.SetZIndex(this, int.MinValue);
        }
    }
}