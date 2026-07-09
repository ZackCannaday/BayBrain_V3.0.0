using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BayBrain.Converters
{
    public class StringNotEmptyConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool invert = parameter as string == "invert";
            bool isNotEmpty = !string.IsNullOrWhiteSpace(value as string);
            
            if (invert)
                return isNotEmpty ? Visibility.Collapsed : Visibility.Visible;
            else
                return isNotEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
