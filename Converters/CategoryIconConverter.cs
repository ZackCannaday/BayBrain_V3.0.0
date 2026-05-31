using System.Globalization;
using System.Windows.Data;

namespace BayBrain.Converters
{
    public class CategoryIconConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string category) return "❓";

            return category.ToLower() switch
            {
                "brakes" => "🛑",
                "fluids" => "💧",
                "suspension" => "🔧",
                "electrical" => "⚡",
                "engine" => "🔌",
                "transmission" => "⚙️",
                "cooling" => "❄️",
                "tires" => "🛞",
                "filters" => "🌫️",
                _ => "🔍"
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
