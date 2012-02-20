using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace GraphSynth.UI
{
    /// <summary>
    ///   Interaction logic for SldAndTextbox.xaml
    /// </summary>
    public partial class BrushSelector : UserControl
    {
        private static List<ColorSwatch> predefinedColors;

        public static readonly DependencyProperty ValueProperty
            = DependencyProperty.Register("Value",
                                          typeof(Brush), typeof(BrushSelector),
                                          new FrameworkPropertyMetadata(Brushes.Transparent,
                                                                        FrameworkPropertyMetadataOptions.AffectsRender));

        public static List<ColorSwatch> PredefinedColors
        {
            get
            {
                if (predefinedColors == null) definePredefinedColors();
                return predefinedColors;
            }
        }

        public string Label
        {
            get { return (string)expBrushSelect.Header; }
            set { expBrushSelect.Header = value; }
        }

        public Brush Value
        {
            get { return (Brush)GetValue(ValueProperty); }
            private set { SetValue(ValueProperty, value); }
        }

        #region Event Handling

        //A RoutedEvent using standard RoutedEventArgs, event declaration
        //The actual event routing
        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble,
                                             typeof(RoutedEventHandler), typeof(BrushSelector));

        // Provides accessors for the event
        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        // This method raises the valueChanged event
        private void RaiseValueChangedEvent()
        {
            var newEventArgs = new RoutedEventArgs(ValueChangedEvent);
            RaiseEvent(newEventArgs);
        }

        //************************************************************************

        #endregion

        private static void definePredefinedColors()
        {
            predefinedColors = new List<ColorSwatch>();
            var propInfo = typeof(Brushes).GetProperties();
            foreach (var pi in propInfo)
                predefinedColors.Add(new ColorSwatch(pi.Name,
                                                     (SolidColorBrush)pi.GetGetMethod().Invoke(null, null)));
        }

        private void sliders_ValueChanged(object sender, RoutedEventArgs e)
        {
            var a = (byte)sldOpacity.Value;
            var r = (byte)sldRed.Value;
            var g = (byte)sldGreen.Value;
            var b = (byte)sldBlue.Value;
            Value = new SolidColorBrush(new Color { A = a, R = r, G = g, B = b });
            RaiseValueChangedEvent();
            string ColorName = null;
            try
            {
                ColorName = (from p in PredefinedColors
                             where ((p.A == a) && (p.Red == r) && (p.Green == g) && (p.Blue == b))
                             select p.ColorName).FirstOrDefault();
            }
            catch
            {
            }
            if (ColorName != null) textDescription.Text = ColorName;
            else textDescription.Text = "";
            sortColorsBySlider(new[] { a, r, g, b });
        }

        private void textDescription_KeyUp(object sender, KeyEventArgs e)
        {
            var input = textDescription.Text.Trim();
            if (input.Length == 0) sortColorsAlphabetically();
            else if (input[0].Equals('<'))
            {
                Brush temp;
                if (ValidateXAMLBrush(ref input, out temp))
                {
                    Value = temp;
                    textDescription.Text = input;
                    RaiseValueChangedEvent();
                    LabelColorTable.Content = "XAML Brush Complete";
                }
                else LabelColorTable.Content = "Partial XAML Brush?";
            }
            else if (input[0].Equals('#'))
            {
                if (input.Length == 7)
                {
                    var c = GetColorFromString(input.Insert(1, "FF"));
                    sldOpacity.UpdateValue(c.A);
                    sldRed.UpdateValue(c.R);
                    sldGreen.UpdateValue(c.G);
                    sldBlue.UpdateValue(c.B);
                    textDescription.Text = input;
                }
                else if (input.Length >= 9)
                {
                    var c = GetColorFromString(input);
                    sldOpacity.UpdateValue(c.A);
                    sldRed.UpdateValue(c.R);
                    sldGreen.UpdateValue(c.G);
                    sldBlue.UpdateValue(c.B);
                    textDescription.Text = input;
                }
            }
            else
            {
                sortColorsByText(input);
                //Value = new SolidColorBrush((ColorSwatch)WrapPanelForColors.Children[0]);
                RaiseValueChangedEvent();
            }
        }

        private Boolean ValidateXAMLBrush(ref string input, out Brush temp)
        {
            try
            {
                temp = (Brush)MyXamlHelpers.Parse(input);
                /***** Notice!: If you have crashed GS2.0 here, then
                 * the try-catch failed. This happens due to a setting
                 * in your Visual Studio environment. To fix this:
                 * 1) Go to Debug->Exceptions.
                 * 2) expand Common Language Runtime Exceptions
                 * 3) Scroll Down to System.Windows.Markup.XamlParseException
                 * 4) uncheck the box in the "Thrown" Column. */
                return true;
            }
            catch
            {
                temp = Value;
                return false;
            }
        }

        public void ReadInBrushValue(Brush datum)
        {
            sldOpacity.UpdateValue(double.NaN);
            sldRed.UpdateValue(double.NaN);
            sldGreen.UpdateValue(double.NaN);
            sldBlue.UpdateValue(double.NaN);
            if ((typeof(SolidColorBrush)).IsInstanceOfType(datum))
            //&& (!sameSolidColorBrush(datum,Value)))
            {
                var c = ((SolidColorBrush)datum).Color;
                sldOpacity.UpdateValue(c.A);
                sldRed.UpdateValue(c.R);
                sldGreen.UpdateValue(c.G);
                sldBlue.UpdateValue(c.B);
                Value = datum;
            }
            else if (datum != null)
            {
                textDescription.Text = XamlWriter.Save(datum);
                Value = datum;
            }
        }

        private void sortColorsBySlider(byte[] reference)
        {
            LabelColorTable.Content = "Sorted by Sliders";

            var sortedSwatches =
                PredefinedColors.OrderBy(a => a, new SliderValueComparer(reference))
                    .ThenBy(a => a.ColorName);
            WrapPanelForColors.Children.Clear();
            foreach (var c in sortedSwatches)
                WrapPanelForColors.Children.Add(c.copy());
        }

        private void sortColorsAlphabetically()
        {
            LabelColorTable.Content = "Sorted Alphabetically";
            var sortedSwatches =
                PredefinedColors.OrderBy(a => a.ColorName);
            WrapPanelForColors.Children.Clear();
            foreach (var c in sortedSwatches)
                WrapPanelForColors.Children.Add(c.copy());
        }

        private void sortColorsByText(string input)
        {
            LabelColorTable.Content = "Sorted by Text";

            var sortedSwatches =
                PredefinedColors.OrderBy(a => a, new TextInputValueComparer(input))
                    .ThenBy(a => a.ColorName);
            WrapPanelForColors.Children.Clear();
            foreach (var c in sortedSwatches)
                WrapPanelForColors.Children.Add(c.copy());
        }

        private void WrapPanelForColors_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ColorSwatch)
            {
                var cs = (ColorSwatch)e.OriginalSource;
                sldOpacity.UpdateValue(cs.A);
                sldRed.UpdateValue(cs.Red);
                sldGreen.UpdateValue(cs.Green);
                sldBlue.UpdateValue(cs.Blue);
                Value = cs.ColorBrush;
                RaiseValueChangedEvent();
            }
        }

        private void BrushSelector_Loaded(object sender, RoutedEventArgs e)
        {
            sortColorsAlphabetically();
        }

        #region Properties

        #endregion

        #region Static Helpers

        public static Brush GetBrushFromString(String sColor)
        {
            return new SolidColorBrush(GetColorFromString(sColor));
        }

        public static Color GetColorFromString(String sColor)
        {
            int intColor;
            byte[] colorBytes;
            if (int.TryParse(sColor, out intColor))
            {
                colorBytes = BitConverter.GetBytes(intColor);
                return Color.FromArgb(colorBytes[3], colorBytes[2], colorBytes[1], colorBytes[0]);
            }
            if (int.TryParse(sColor.Replace("#", ""), NumberStyles.AllowHexSpecifier,
                             CultureInfo.InvariantCulture, out intColor))
            {
                colorBytes = BitConverter.GetBytes(intColor);
                return Color.FromArgb(colorBytes[3], colorBytes[2], colorBytes[1], colorBytes[0]);
            }
            var propInfo = typeof(Colors).GetProperty(sColor);
            if (propInfo != null)
                return (Color)propInfo.GetGetMethod().Invoke(null, null);
            SearchIO.output(sColor + " is not a member of the Colors enumeration.", 3);
            return Colors.Transparent;
        }

        public static Boolean EqualBrushes(Brush b1, Brush b2)
        {
            if ((typeof(SolidColorBrush)).IsInstanceOfType(b1) &&
                (typeof(SolidColorBrush)).IsInstanceOfType(b2))
                return ((SolidColorBrush)b1).Color.Equals(((SolidColorBrush)b2).Color);
            /* I'm not inclined to write the remaining details of this function for
         * cases when neither is a SolidColorBrush. I was hoping the Brush.Equals
         * overload would do all this but it seems that it doesn't go into this detail. */
            return false;
        }

        #endregion

        #region Constructor

        public BrushSelector()
        {
            InitializeComponent();
        }

        #endregion
    }

    internal class SliderValueComparer : IComparer<ColorSwatch>
    {
        private readonly byte[] reference;

        public SliderValueComparer(byte[] reference)
        {
            this.reference = reference;
        }

        #region IComparer<ColorSwatch> Members

        public int Compare(ColorSwatch x, ColorSwatch y)
        {
            var xVect = new double[]
                            {
                                Math.Abs(x.A - reference[0]),
                                Math.Abs(x.Red - reference[1]),
                                Math.Abs(x.Green - reference[2]),
                                Math.Abs(x.Blue - reference[3])
                            };
            var yVect = new double[]
                            {
                                Math.Abs(y.A - reference[0]),
                                Math.Abs(y.Red - reference[1]),
                                Math.Abs(y.Green - reference[2]),
                                Math.Abs(y.Blue - reference[3])
                            };

            if (xVect.Sum() > yVect.Sum()) return 1;
            if (xVect.Sum() == yVect.Sum()) return 0;
            return -1;
        }

        #endregion
    }

    internal class TextInputValueComparer : IComparer<ColorSwatch>
    {
        private readonly string input;

        public TextInputValueComparer(string input)
        {
            this.input = input.ToLowerInvariant().Trim();
        }

        #region IComparer<ColorSwatch> Members

        public int Compare(ColorSwatch x, ColorSwatch y)
        {
            var xName = x.ColorName.ToLowerInvariant().Trim();
            var yName = y.ColorName.ToLowerInvariant().Trim();
            var back = input + " ";
            var front = " " + input;
            int xfront, xback, yfront, yback;
            do
            {
                back = back.Substring(0, back.Length - 1);
                front = front.Substring(1, front.Length - 1);
                xback = xName.IndexOf(back);
                yback = yName.IndexOf(back);
                xfront = xName.IndexOf(front);
                yfront = yName.IndexOf(front);
            } while ((xback == -1) && (yback == -1) && (xfront == -1) && (yfront == -1) && (back.Length > 0));

            if ((xback > -1) && (yback > -1)) return xback - yback;
            if ((xback == -1) && (yback > xback)) return 1;
            if ((yback == -1) && (xback > yback)) return -1;
            if ((xfront > -1) && (yfront > -1)) return xfront - yfront;
            if ((xfront == -1) && (yfront > xfront)) return 1;
            if ((yfront == -1) && (xfront > yfront)) return -1;
            return 0;
        }

        #endregion
    }
}