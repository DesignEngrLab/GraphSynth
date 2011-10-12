using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using GraphSynth.Representation;

namespace GraphSynth.GraphDisplay
{
    /* DisplayTextConverter determines WHAT the text should be: the text string. 
     * TPositionTextConverter determines WHERE the text should be. */

    public class DisplayTextConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var n = (graphElement)parameter;
            var showName = (Boolean)values[0];
            var showLabels = (Boolean)values[1];
            var fontSize = System.Convert.ToDouble(values[2]);
            var text = "";
            if (n != null)
            {
                if (showName)
                    text += n.name;
                if (showName && showLabels && (n.localLabels.Count > 0))
                    text += " (";
                if (showLabels)
                    for (var i = 0; i < n.localLabels.Count; i++)
                    {
                        text += n.localLabels[i];
                        if (i < n.localLabels.Count - 1) text += ", ";
                    }
                if (showName && showLabels && (n.localLabels.Count > 0))
                    text += ")";
            }
            if ((text.Length > 0) && (fontSize > 0))
                return new FormattedText(text, CultureInfo.GetCultureInfo("en-us"),
                                         FlowDirection.LeftToRight, new Typeface("Calibri"), fontSize, Brushes.Black);
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }


    public class PositionTextConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var text = (FormattedText)values[2];
            if (text == null) return new Point();
            try
            {
                var distance = System.Convert.ToDouble(values[0]);
                var location = System.Convert.ToDouble(values[1]);
                if (parameter == null)
                {
                    var angle = 2 * Math.PI * location;
                    var radius = Math.Min(Math.Abs(text.Width / (2 * Math.Cos(angle))),
                                          Math.Abs(text.Height / (2 * Math.Sin(angle))));

                    return new Point(distance * radius * Math.Cos(-angle) - text.Width / 2,
                                     distance * radius * Math.Sin(-angle) + text.Height / 2);
                }
                return ((AbstractController)parameter).DetermineTextPoint(text, location, distance);
            }
            catch
            {
                return new Point();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}