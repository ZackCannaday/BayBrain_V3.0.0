using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BayBrain.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool invert = parameter as string == "invert";
            bool isNotNull = value != null;

            if (invert)
                return isNotNull ? Visibility.Collapsed : Visibility.Visible;
            else
                return isNotNull ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
