using BodyTracker.Models;
using BodyTracker.Models.SamsungHealth;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BodyTracker.Services
{
    public class SamsungDataCsvExtractor
    {
        /// <summary>
        /// Asynchronously extracts and parses food intake data records from a specified CSV file.
        /// </summary>
        /// <remarks>Reads the file line by line using CsvHelper, skipping description and header rows, maps records using <see cref="SamsungFoodIntakeMap"/>, and reports extraction progress asynchronously.</remarks>
        /// <param name="filePath">The file system path to the source CSV data file.</param>
        /// <param name="progress">An optional progress reporter for tracking percentage completion. Defaults to null.</param>
        /// <param name="status">An optional progress reporter for tracking status messages. Defaults to null.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of extracted <see cref="SamsungFoodIntakeModel"/> records.</returns>
        public static async Task<List<SamsungFoodIntakeModel>> ParseFoodIntakeAsync(string filePath,
                                                                               IProgress<double>? progress = null,
                                                                               IProgress<string>? status = null)
        {
            var result = new List<SamsungFoodIntakeModel>();

            var totalLines = File.ReadLines(filePath).Count() - 2;
            var currentLine = 0;

            using var reader = new StreamReader(filePath);

            // The first line in the csv file can ingnore because it is just a description of the file.
            // The second line is the header line, which will be used by CsvHelper to map the columns to the properties of the model.
            await reader.ReadLineAsync();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };

            using var csvReader = new CsvReader(reader, config);

            csvReader.Context.RegisterClassMap<SamsungFoodIntakeMap>();

            await foreach (var record in csvReader.GetRecordsAsync<SamsungFoodIntakeModel>())
            {
                result.Add(record);

                currentLine++;

                progress?.Report(currentLine * 100.0 / totalLines);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously extracts and parses daily step trend records from a specified CSV file.
        /// </summary>
        /// <remarks>Reads the file line by line using CsvHelper, skipping description and header rows, maps records using <see cref="SamsungStepTrendMap"/>, and reports extraction progress asynchronously.</remarks>
        /// <param name="filePath">The file system path to the source CSV data file.</param>
        /// <param name="progress">An optional progress reporter for tracking percentage completion. Defaults to null.</param>
        /// <param name="status">An optional progress reporter for tracking status messages. Defaults to null.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of extracted <see cref="SamsungStepTrendModel"/> records.</returns>
        public static async Task<List<SamsungStepTrendModel>> ParseStepTrendAsync(string filePath,
                                                                                       IProgress<double>? progress = null,
                                                                                       IProgress<string>? status = null)
        {
            var result = new List<SamsungStepTrendModel>();

            var totalLines = File.ReadLines(filePath).Count() - 2;

            var currentLine = 0;

            using var reader = new StreamReader(filePath);

            // The first line in the csv file can ingnore because it is just a description of the file.
            // The second line is the header line, which will be used by CsvHelper to map the columns to the properties of the model.
            await reader.ReadLineAsync();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };

            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<SamsungStepTrendMap>();

            await foreach (var record in csv.GetRecordsAsync<SamsungStepTrendModel>())
            {
                result.Add(record);

                currentLine++;

                progress?.Report(currentLine * 100.0 / totalLines);

            }

            return result;
        }

        /// <summary>
        /// Asynchronously extracts and parses heart rate records from a specified CSV file.
        /// </summary>
        /// <param name="progress">An optional progress reporter for tracking percentage completion. Defaults to null.</param>
        /// <param name="status">An optional progress reporter for tracking status messages. Defaults to null.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of extracted <see cref="SamsungHeartRateModel"/> records.</returns>
        public static async Task<List<SamsungHeartRateModel>> ParseHeartRateAsync(string filePath,
                                                                                  IProgress<double>? progress = null,
                                                                                  IProgress<string>? status = null)
        {
            var result = new List<SamsungHeartRateModel>();

            var totalLines = File.ReadLines(filePath).Count() - 2;

            var currentLine = 0;

            using var reader = new StreamReader(filePath);

            // The first line in the csv file can ingnore because it is just a description of the file.
            // The second line is the header line, which will be used by CsvHelper to map the columns to the properties of the model.
            await reader.ReadLineAsync();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };

            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<SamsungHeartRateMap>();

            await foreach (var record in csv.GetRecordsAsync<SamsungHeartRateModel>())
            {
                result.Add(record);

                currentLine++;

                progress?.Report(currentLine * 100.0 / totalLines);

            }

            return result;
        }

        /// <summary>
        /// Asynchronously extracts and parses exercise records from a specified CSV file.
        /// </summary>
        /// <param name="progress">An optional progress reporter for tracking percentage completion. Defaults to null.</param>
        /// <param name="status">An optional progress reporter for tracking status messages. Defaults to null.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of extracted <see cref="SamsungExerciseModel"/> records.</returns>
        public static async Task<List<SamsungExerciseModel>> ParseExerciseAsync(string filePath,
                                                                                     IProgress<double>? progress = null,
                                                                                     IProgress<string>? status = null)
        {
            var result = new List<SamsungExerciseModel>();

            var totalLines = File.ReadLines(filePath).Count() - 2;

            var currentLine = 0;

            using var reader = new StreamReader(filePath);

            // The first line in the csv file can ingnore because it is just a description of the file.
            // The second line is the header line, which will be used by CsvHelper to map the columns to the properties of the model.
            await reader.ReadLineAsync();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };

            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<SamsungExerciseMap>();

            await foreach (var record in csv.GetRecordsAsync<SamsungExerciseModel>())
            {
                result.Add(record);

                currentLine++;

                progress?.Report(currentLine * 100.0 / totalLines);

            }

            return result;
        }

        /// <summary>
        /// Asynchronously extracts and parses oxygen saturation records from a specified CSV file.
        /// </summary>
        /// <param name="progress">An optional progress reporter for tracking percentage completion. Defaults to null.</param>
        /// <param name="status">An optional progress reporter for tracking status messages. Defaults to null.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of extracted <see cref="SamsungOxygenSaturationModel"/> records.</returns>
        public static async Task<List<SamsungOxygenSaturationModel>> ParseOxygenSaturationAsync(    string filePath,
                                                                                         IProgress<double>? progress = null,
                                                                                         IProgress<string>? status = null)
        {
            var result = new List<SamsungOxygenSaturationModel>();

            var totalLines = File.ReadLines(filePath).Count() - 2;

            var currentLine = 0;

            using var reader = new StreamReader(filePath);

            // The first line in the csv file can ingnore because it is just a description of the file.
            // The second line is the header line, which will be used by CsvHelper to map the columns to the properties of the model.
            await reader.ReadLineAsync();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };

            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<SamsungOxygenSaturationMap>();

            await foreach (var record in csv.GetRecordsAsync<SamsungOxygenSaturationModel>())
            {
                result.Add(record);

                currentLine++;

                progress?.Report(currentLine * 100.0 / totalLines);

            }

            return result;
        }

    }
}
