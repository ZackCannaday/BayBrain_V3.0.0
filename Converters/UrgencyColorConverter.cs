using BayBrain.Models;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BayBrain.Converters
{
    public class UrgencyColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UrgencyScore score)
            {
                var color = (Color)ColorConverter.ConvertFromString(score.ColorHex);
                return new SolidColorBrush(color);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class AnswerButtonStyleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 4) return new SolidColorBrush(Color.FromRgb(44, 44, 46));

            bool hasAnswered = values[3] is bool ha && ha;
            if (!hasAnswered) return new SolidColorBrush(Color.FromRgb(44, 44, 46));

            int thisIdx = parameter is string s && int.TryParse(s, out int pi) ? pi : -1;
            int correctIdx = values[2] is int ci ? ci : -1;
            int selectedIdx = values[1] is int si ? si : -1;

            if (thisIdx == correctIdx)
                return new SolidColorBrush(Color.FromRgb(52, 199, 89));    // green
            if (thisIdx == selectedIdx && thisIdx != correctIdx)
                return new SolidColorBrush(Color.FromRgb(255, 59, 48));    // red
            return new SolidColorBrush(Color.FromRgb(44, 44, 46));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
