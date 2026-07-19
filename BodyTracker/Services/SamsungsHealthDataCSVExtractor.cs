using BodyTracker.MVVM.Models;
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
    public class SamsungHealthDataCsvExtractor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="progress"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static async Task<List<FoodIntakeModel>> ExtractFoodIntakeAsync(string filePath, 
                                                                               IProgress<double>? progress = null,
                                                                               IProgress<string>? status = null)
        {
            var result = new List<FoodIntakeModel>();
  
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

            csvReader.Context.RegisterClassMap<FoodIntakeMap>();

            await foreach (var record in csvReader.GetRecordsAsync<FoodIntakeModel>())
            {
                result.Add(record);

                currentLine++;

                progress?.Report(currentLine * 100.0 / totalLines);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="progress"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static async Task<List<StepDailyTrendModel>> ExtractDailyStepTrendAsync(string filePath, 
                                                                                       IProgress<double>? progress = null,
                                                                                       IProgress<string>? status = null)
        {
            var result = new List<StepDailyTrendModel>();

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

            csv.Context.RegisterClassMap<StepDailyTrendMap>();

            await foreach (var record in csv.GetRecordsAsync<StepDailyTrendModel>())
            {
                result.Add(record);

                currentLine++;

                progress?.Report(currentLine * 100.0 / totalLines);
               
            }

            return result;
        }



    }
}
