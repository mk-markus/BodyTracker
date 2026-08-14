using System;
using System.Globalization;
using System.Windows.Data;

namespace BodyTracker.Services
{
    /// <summary>
    /// Represents a multi-value converter that calculates a proportional width based on a percentage and an available width.
    /// </summary>
    /// <remarks>Implements <see cref="IMultiValueConverter"/> for WPF data binding to convert a percentage value (0.0 to 1.0) and an available size into a concrete pixel width.</remarks>
    public sealed class PercentageToWidthConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts source binding values into a proportional width value.
        /// </summary>
        /// <remarks>Expects <c>values[0]</c> to represent the percentage (0 to 1) and <c>values[1]</c> to represent the available width. Clamps inputs, handles parsing from multiple numeric and string types, and returns the calculated width.</remarks>
        /// <param name="values">The array of values produced by the source bindings in the multi-binding.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A calculated <see cref="double"/> representing the resulting width.</returns>
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

        /// <summary>
        /// Converts a binding target value back to the source binding values.
        /// </summary>
        /// <remarks>Not supported for this converter.</remarks>
        /// <param name="value">The value that the binding target produces.</param>
        /// <param name="targetTypes">The array of types to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Never returns; always throws a <see cref="NotSupportedException"/>.</returns>
        /// <exception cref="NotSupportedException">Thrown always because one-way conversion is the only supported direction.</exception>
        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported for PercentageToWidthConverter.");
        }
    }
}