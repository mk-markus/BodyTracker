using BodyTracker.Models;
using BodyTracker.Models.FullBodyMeasurement;
using BodyTracker.State;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;


namespace BodyTracker.Services
{
    /// <summary>
    /// Provides services for exporting full body measurement data, including dimensions and metrics, to CSV files or string formats.
    /// </summary>
    public class FullBodyMeasurementExporter
    {
        /// <summary>
        /// Asynchronously exports collections of full body measurement data into separate CSV files for body dimensions and metrics at the specified directory path.
        /// </summary>
        /// <param name="filePath">The directory path where the CSV files will be saved.</param>
        /// <param name="data">The collection of full body measurement export models to export.</param>
        /// <returns>A task representing the asynchronous export operation.</returns>
        public static async Task<bool> Export(string filePath, FullBodyMeasurementExportModel data)
        {
            bool status = false;
            
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            };

            try
            {
                string pathDimension = Path.Combine(filePath, $"{DateTime.Now:yyyyMMdd_HHmmss}_Measurements_BodyDimensions_{AppState.SelectedPersonName}.csv");

                string pathMetrics = Path.Combine(filePath, "BodyMetrics.csv");

                if (data.BodyDimensions == null || data.BodyDimensions.Count == 0 ||
                    data.BodyMetrics == null || data.BodyMetrics.Count == 0)
                    return false;


                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }


                await using (var writerMetrics = new StreamWriter(pathMetrics, append: true))
                {
                    using var csv = new CsvWriter(writerMetrics, config);
                    csv.WriteRecords(data.BodyMetrics);
                }
                await using (var writerDimension = new StreamWriter(pathDimension, append: true))
                {
                    using var csv = new CsvWriter(writerDimension, config);
                    csv.WriteRecords(data.BodyDimensions);
                }

                status = true;
            }
            catch (Exception ex)
            {
                status = false;
                throw new Exception($"An error occurred while exporting full body measurement data: {ex.Message}", ex);

            }


            return status;
        }

        /// <summary>
        /// Exports a collection of full body measurement data into a single comma-separated values (CSV) string format.
        /// </summary>
        /// <param name="data">The collection of full body measurement export models to export.</param>
        /// <returns>A string containing the serialized CSV data.</returns>
        public string ExportToString(IEnumerable<FullBodyMeasurementExportModel> data)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            };

            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, config);

            csv.WriteRecords(data);

            return writer.ToString();
        }
    }
}