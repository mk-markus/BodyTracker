using System;
using System.Globalization;
using System.Windows.Data;

namespace BodyTracker.Services
{
    // Converter für MultiBinding: (percentage, availableWidth) -> width
    public sealed class PercentageToWidthConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
        {
            // Erwartet: values[0] = percentage (0..1), values[1] = available width (double)
            if (values == null || values.Length < 2)
                return 0d;

            // Lese Percentage
            double percentage;
            try
            {
                if (values[0] is double d0) percentage = d0;
                else if (values[0] is float f0) percentage = f0;
                else if (values[0] is decimal m0) percentage = (double)m0;
                else if (values[0] is int i0) percentage = i0;
                else if (values[0] is string s0 && double.TryParse(s0, NumberStyles.Any, culture, out var p0)) percentage = p0;
                else if (!double.TryParse(values[0]?.ToString(), NumberStyles.Any, culture, out percentage)) percentage = 0d;
            }
            catch
            {
                percentage = 0d;
            }

            // Lese AvailableWidth
            double available;
            try
            {
                if (values[1] is double d1) available = d1;
                else if (values[1] is float f1) available = f1;
                else if (values[1] is decimal m1) available = (double)m1;
                else if (values[1] is int i1) available = i1;
                else if (values[1] is string s1 && double.TryParse(s1, NumberStyles.Any, culture, out var p1)) available = p1;
                else if (!double.TryParse(values[1]?.ToString(), NumberStyles.Any, culture, out available)) available = 0d;
            }
            catch
            {
                available = 0d;
            }

            if (double.IsNaN(percentage) || double.IsInfinity(percentage)) percentage = 0d;
            if (double.IsNaN(available) || double.IsInfinity(available)) available = 0d;

            // Clamp percentage to [0,1]
            if (percentage < 0d) percentage = 0d;
            if (percentage > 1d) percentage = 1d;

            // Berechne Breite
            var width = available * percentage;

            // Optional: stelle sicher, dass width im gültigen Bereich ist
            if (width < 0d) width = 0d;
            if (width > available) width = available;

            return width;
        }

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported for PercentageToWidthConverter.");
        }
    }
}