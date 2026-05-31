using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BayBrain.Converters
{
    public class PositiveIntToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => (int?)value > 0 ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
