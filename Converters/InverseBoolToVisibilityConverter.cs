using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BayBrain.Converters
{
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => (bool?)value == false ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => (Visibility?)value != Visibility.Visible;
    }
}
