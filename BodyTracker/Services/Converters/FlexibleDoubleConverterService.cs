using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Globalization;

namespace BodyTracker.Services
{
    public class FlexibleDoubleConverterService : DefaultTypeConverter
    {
        /// <summary>
        /// Converts the specified string from a CSV field into a double value.
        /// </summary>
        /// <remarks>Trims or checks whitespace, replaces commas with periods to support regional input variations, and parses the string using invariant culture. Returns null if the text is empty or parsing fails.</remarks>
        /// <param name="text">The string to convert from the CSV record.</param>
        /// <param name="row">The current reader row context.</param>
        /// <param name="memberMapData">The member map data configuration.</param>
        /// <returns>A parsed <see cref="double"/> value, or <c>null</c> if conversion is not possible.</returns>
        public override object? ConvertFromString(
            string? text,
            IReaderRow row,
            MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            text = text.Replace(',', '.');

            if (double.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double value))
            {
                return value;
            }

            return null;
        }
    }
}
