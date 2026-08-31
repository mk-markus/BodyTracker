using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;

namespace BodyTracker.Services
{
    public sealed class FlexibleDateTimeConverterService : DefaultTypeConverter
    {
        /// <summary>
        /// Represents a custom CsvHelper type converter for parsing date and time strings containing German month names into <see cref="DateTime"/> objects.
        /// </summary>
        /// <remarks>Inherits from CsvHelper's <see cref="DefaultTypeConverter"/> and utilizes a dictionary mapping of German month names and abbreviations to parse localized date-time formats such as "9 Juli 2026, 15:55".</remarks>
        private static readonly Dictionary<string, int> GermanMonths =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Januar"] = 1,
                ["Februar"] = 2,
                ["März"] = 3,
                ["April"] = 4,
                ["Mai"] = 5,
                ["Juni"] = 6,
                ["Juli"] = 7,
                ["August"] = 8,
                ["September"] = 9,
                ["Oktober"] = 10,
                ["November"] = 11,
                ["Dezember"] = 12,

                ["Jan"] = 1,
                ["Jan."] = 1,
                ["Feb"] = 2,
                ["Feb."] = 2,

                ["Apr"] = 4,
                ["Apr."] = 4,



                ["Aug"] = 8,
                ["Aug."] = 8,
                ["Sept"] = 9,
                ["Sept."] = 9,
                ["Okt"] = 10,
                ["Okt."] = 10,
                ["Nov"] = 11,
                ["Nov."] = 11,
                ["Dez"] = 12,
                ["Dez."] = 12
            };

        /// <summary>
        /// Converts the specified string from a CSV field into a <see cref="DateTime"/> object.
        /// </summary>
        /// <remarks>Splits the input string by comma and space delimiters, resolves German month names using a lookup dictionary, parses hours and minutes, and constructs a <see cref="DateTime"/> instance. Throws a <see cref="TypeConverterException"/> if parsing fails.</remarks>
        /// <param name="text">The string to convert from the CSV record.</param>
        /// <param name="row">The current reader row context.</param>
        /// <param name="memberMapData">The member map data configuration.</param>
        /// <returns>A parsed <see cref="DateTime"/> object, or <c>null</c> if the input text is null or whitespace.</returns>
        /// <exception cref="TypeConverterException">Thrown when the input string does not match the expected format or contains an unknown month name.</exception>
        public override object? ConvertFromString(
            string? text,
            IReaderRow row,
            MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            try
            {
                // Beispiel:
                // 9 Juli 2026, 15:55

                var parts = text.Split(',');

                if (parts.Length != 2)
                    throw new FormatException();

                var datePart = parts[0].Trim();
                var timePart = parts[1].Trim();

                var dateElements =
                    datePart.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (dateElements.Length != 3)
                    throw new FormatException();

                int day = int.Parse(dateElements[0]);

                string monthName = dateElements[1];

                int year = int.Parse(dateElements[2]);

                if (!GermanMonths.TryGetValue(monthName, out int month))
                    throw new FormatException(
                        $"Unknown month '{monthName}'");

                var timeElements = timePart.Split(':');

                if (timeElements.Length != 2)
                    throw new FormatException();

                int hour = int.Parse(timeElements[0]);
                int minute = int.Parse(timeElements[1]);

                return new DateTime(
                    year,
                    month,
                    day,
                    hour,
                    minute,
                    0);
            }
            catch (Exception ex)
            {
                throw new TypeConverterException(
                    this,
                    memberMapData,
                    text,
                    row.Context,
                    $"Could not convert '{text}' to DateTime. {ex.Message}");
            }
        }
    }
}