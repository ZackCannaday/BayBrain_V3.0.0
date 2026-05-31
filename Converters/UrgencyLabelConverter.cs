using System.Globalization;
using System.Windows.Data;

namespace BayBrain.Converters
{
    public class UrgencyLabelConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not int score) return string.Empty;

            return score switch
            {
                <= 3 => "Low",
                <= 6 => "Moderate",
                _ => "Critical"
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
