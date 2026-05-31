using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BayBrain.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var isVisible = value switch
            {
                bool boolValue => boolValue,
                int intValue => intValue > 0,
                long longValue => longValue > 0,
                string stringValue => !string.IsNullOrWhiteSpace(stringValue),
                null => false,
                _ => false
            };

            if (string.Equals(parameter as string, "invert", StringComparison.OrdinalIgnoreCase))
            {
                isVisible = !isVisible;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => (Visibility?)value == Visibility.Visible;
    }
}
