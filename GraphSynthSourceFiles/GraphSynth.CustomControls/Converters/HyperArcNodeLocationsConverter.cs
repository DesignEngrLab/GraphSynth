using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GraphSynth.GraphDisplay
{
    public class HyperArcNodeLocationsConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var nodePoints = new PointCollection();
            var limit = values.GetLength(0);
            for (int i = 0; i < limit; i++)
            {
                nodePoints.Add((Point)values[i]);
            }
            return nodePoints;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
