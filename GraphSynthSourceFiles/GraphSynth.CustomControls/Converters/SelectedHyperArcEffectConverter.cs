using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Effects;

namespace GraphSynth.GraphDisplay  
{
    class SelectedHyperArcEffectConverter:IValueConverter
    {
        private readonly Effect selectedEffect;

        public SelectedHyperArcEffectConverter(Effect selectedEffect)
        {
            this.selectedEffect = selectedEffect;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Boolean)value) return selectedEffect;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
