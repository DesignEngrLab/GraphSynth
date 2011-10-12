using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace GraphSynth.GraphDisplay
{
    public class IconOpacityConverter : IValueConverter
    {
        readonly double radiusMultiplier;
        readonly double radiusAddition;
        readonly double maxOpacity;
        private readonly IconShape icon;

        public IconOpacityConverter(IconShape icon, double maxOpacity, double radiusMultiplier, double radiusAddition)
        {
            this.icon = icon;
            this.maxOpacity = maxOpacity;
            this.radiusMultiplier = radiusMultiplier;
            this.radiusAddition = radiusAddition;
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double dx = ((Point)value - icon.Center).Length;
            icon.FillIn = (dx <= icon.Radius + radiusAddition);
            if (dx <= radiusMultiplier * icon.Radius)
                return maxOpacity * (radiusMultiplier * icon.Radius - dx) / (radiusMultiplier * icon.Radius);
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
