using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GraphSynth.UI
{
    public class ColorSwatch : FrameworkElement
    {
        private const double side = 10.0;

        public readonly byte A;
        public readonly SolidColorBrush ColorBrush;
        public readonly string ColorName;
        public byte Blue;
        public byte Green;
        public byte Red;

        public ColorSwatch(string name, SolidColorBrush c)
        {
            ColorBrush = c;
            ColorName = name;
            A = c.Color.A;
            Red = c.Color.R;
            Green = c.Color.G;
            Blue = c.Color.B;

            Margin = new Thickness(1, 2, 1, 2);
            Width = side;
            Height = 2*side;

            //add a tooltip
            var t = new ToolTip();
            t.Content = ColorName;
            ToolTip = t;
        }

        public static implicit operator Color(ColorSwatch cd)
        {
            return cd.ColorBrush.Color;
        }

        // Override of OnRender.
        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRoundedRectangle(ColorBrush, null,
                                    new Rect(0, 0, side, 2*side), side/4, side/4);
        }

        public ColorSwatch copy()
        {
            return new ColorSwatch(ColorName, ColorBrush);
        }
    }
}