using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BodyTracker.Converters
{
    [ValueConversion(typeof(decimal?), typeof(string))]
    public class DecimalNullableConverter : IValueConverter
    {
        public int? Decimals { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal d)
                return Decimals.HasValue ? d.ToString($"N{Decimals.Value}", culture) : d.ToString(culture);
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = (value as string)?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(s)) return null;

            var decSep = culture.NumberFormat.NumberDecimalSeparator;
            s = s.Replace(".", decSep);

            if (s.EndsWith(decSep) || s.StartsWith(decSep))
                return Binding.DoNothing; // Zwischenstände zulassen

            if (decimal.TryParse(s, NumberStyles.Number, culture, out var d))
                return d;

            return Binding.DoNothing; // ungültig -> Ziel nicht überschreiben
        }
    }
}
