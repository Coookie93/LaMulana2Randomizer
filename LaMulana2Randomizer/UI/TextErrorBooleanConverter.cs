using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace LM2Randomizer.UI
{
    [ValueConversion(typeof(ValidationError), typeof(bool))]
    public class TextErrorBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value as ValidationError == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
