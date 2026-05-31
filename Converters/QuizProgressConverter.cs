using System.Globalization;
using System.Windows.Data;

namespace BayBrain.Converters
{
    public class QuizProgressConverter : IMultiValueConverter
    {
        public object Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values?.Length != 2) return 0.0;
            if (values[0] is not int correct || values[1] is not int total) return 0.0;

            return total > 0 ? (double)correct / total * 100 : 0.0;
        }

        public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
