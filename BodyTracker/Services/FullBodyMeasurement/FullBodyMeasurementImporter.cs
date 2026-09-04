using BodyTracker.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BodyTracker.Services
{
    public class FullBodyMeasurementImporter
    {
        public List<FullBodyMeasurementDatasModel> Import(string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                MissingFieldFound = null,
                HeaderValidated = null
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            return csv.GetRecords<FullBodyMeasurementDatasModel>().ToList();
        }

        public List<FullBodyMeasurementDatasModel> ImportFromStream(Stream stream)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                MissingFieldFound = null,
                HeaderValidated = null
            };

            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);

            return csv.GetRecords<FullBodyMeasurementDatasModel>().ToList();
        }
    }
}