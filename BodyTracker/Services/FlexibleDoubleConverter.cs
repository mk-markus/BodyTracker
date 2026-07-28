using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyTracker.Services
{
    public class FlexibleDoubleConverter : DefaultTypeConverter
    {
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
