using System.Globalization;
using System.Windows.Data;

namespace BayBrain.Converters
{
    public class ScoreBarWidthConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not int score) return 0;
            return Math.Max(0, Math.Min(100, score * 10)); // Scale 0-10 to 0-100 width
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
