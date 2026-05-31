using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BayBrain.Converters
{
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var isVisible = value switch
            {
                bool boolValue => !boolValue,
                int intValue => intValue <= 0,
                long longValue => longValue <= 0,
                string stringValue => string.IsNullOrWhiteSpace(stringValue),
                null => true,
                _ => false
            };

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => (Visibility?)value != Visibility.Visible;
    }
}
