using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BayBrain.Converters
{
    /// <summary>Converts a hex color string like "#FF3B30" to a SolidColorBrush.</summary>
    public class HexStringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var hex = value?.ToString() ?? "#8E8E93";
                var color = (Color)ColorConverter.ConvertFromString(hex);
                return new SolidColorBrush(color);
            }
            catch
            {
                return new SolidColorBrush(Color.FromRgb(142, 142, 147));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    /// <summary>Multiplies a double by a scalar parameter for bar chart widths.</summary>
    public class MultiplyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d && parameter is string s && double.TryParse(s, out double factor))
                return d * factor;
            if (value is int i && parameter is string s2 && double.TryParse(s2, out double factor2))
                return i * factor2;
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>Returns True if the string is NOT null or empty (for IsEnabled bindings).</summary>
    public class StringNotEmptyToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => !string.IsNullOrWhiteSpace(value?.ToString());

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
