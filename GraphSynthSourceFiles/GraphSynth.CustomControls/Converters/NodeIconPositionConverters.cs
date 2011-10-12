using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace GraphSynth.GraphDisplay
{
    public class NodeIconTransformConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var dsTransform = (Transform)values[0];
            double dsWidth = System.Convert.ToDouble(values[1]);
            double dsHeight = System.Convert.ToDouble(values[2]);
            double scaleFactor = System.Convert.ToDouble(values[3]);
            double defaultRadius = ((double[])parameter)[0];
            double scaleReduction = ((double[])parameter)[1];
            double radius = defaultRadius * Math.Pow(scaleFactor, (scaleReduction - 1));
            var xOffSet = double.IsNaN(dsTransform.Value.OffsetX) ? 0.0 : dsTransform.Value.OffsetX;
            var yOffSet = double.IsNaN(dsTransform.Value.OffsetY) ? 0.0 : dsTransform.Value.OffsetY;
            return new MatrixTransform(1, 0, 0, -1, xOffSet + (dsWidth / 2) - radius, yOffSet + (dsHeight / 2) + radius);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NodeIconCenterConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var dsTransform = (Transform)values[0];
            var dsWidth = System.Convert.ToDouble(values[1]);
            var dsHeight = System.Convert.ToDouble(values[2]);
            var xOffSet = double.IsNaN(dsTransform.Value.OffsetX) ? 0.0 : dsTransform.Value.OffsetX;
            var yOffSet = double.IsNaN(dsTransform.Value.OffsetY) ? 0.0 : dsTransform.Value.OffsetY;
            return new Point(xOffSet + (dsWidth / 2), yOffSet + (dsHeight / 2));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }


}