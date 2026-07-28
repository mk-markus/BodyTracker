using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;

namespace BodyTracker.Converters
{
    public sealed class FlexibleDateTimeConverter : DefaultTypeConverter
    {
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
                ["Feb"] = 2,
                
                ["Apr"] = 4,

                

                ["Aug"] = 8,
                ["Sept"] = 9,
                ["Okt"] = 10,
                ["Nov"] = 11,
                ["Dez"] = 12
            };

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