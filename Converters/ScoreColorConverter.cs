using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BayBrain.Converters
{
    public class ScoreColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not int score) return new SolidColorBrush(Colors.Gray);

            return score switch
            {
                <= 3 => new SolidColorBrush(Color.FromRgb(76, 175, 80)),  // Green
                <= 6 => new SolidColorBrush(Color.FromRgb(255, 193, 7)),  // Yellow
                _ => new SolidColorBrush(Color.FromRgb(244, 67, 54))      // Red
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
