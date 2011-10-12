using System;
using System.Globalization;
using System.Windows.Data;

namespace GraphSynth.UI
{
    public class SliderToTextBoxTextLinearConverter : IValueConverter
    {
        public SliderToTextBoxTextLinearConverter()
        {
            SigDigs = 2;
        }

        public int SigDigs { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Math.Round((double)value, SigDigs));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valString = value.ToString();
            double numericalValue;

            if ((string.IsNullOrWhiteSpace(valString)) || (!Double.TryParse(valString, out numericalValue)))
                return 0.0;

            return numericalValue;
        }

        #endregion
    }
    public class SliderToTextBoxTextLogarithmicConverter : IValueConverter
    {
        public SliderToTextBoxTextLogarithmicConverter()
        {
            SigDigs = 2;
        }

        public int SigDigs { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //var val = value.ToString();
            //double Value;
            //if (string.IsNullOrWhiteSpace(val) || !Double.TryParse(val, out Value))
            //    return 1.0;
            return Math.Round(Math.Pow(10.0, (double)value), SigDigs);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valString = value.ToString();
            double numericalValue;

            if ((string.IsNullOrWhiteSpace(valString)) || (!Double.TryParse(valString, out numericalValue)))
                return 0.0;
            return (Math.Log(numericalValue, 10.0));
        }
        #endregion

    }

    public class TextToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = value.ToString();
            double Value;
            if (string.IsNullOrWhiteSpace(val) || !Double.TryParse(val, out Value))
                return 0;
            return Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value).ToString();
        }

    }
}