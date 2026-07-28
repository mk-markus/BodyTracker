using BodyTracker.MVVM.Mapping;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BodyTracker.Services
{
    public class HeavyAppCSVExtractor
    {
        public static async Task<List<HeavyAppCSVModel>> ExtractAsync(  string filePath,
                                                                        IProgress<double>? progress = null,
                                                                        IProgress<string>? status = null)
        {
            var result = new List<HeavyAppCSVModel>();

            var totalLines = File.ReadLines(filePath).Count() - 2;

            var currentLine = 0;


            using var reader = new StreamReader(filePath);

            var config = new CsvConfiguration(CultureInfo.CurrentUICulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null
            };

            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<HeavyAppCSCMap>();

            await foreach (var item in csv.GetRecordsAsync<HeavyAppCSVModel>())
            {
                item.DataUuid = CreateDeterministicGuid(item);

                result.Add(item);
                
                currentLine++;

                progress?.Report(currentLine * 100.0 / totalLines);
            }

            return result;
        }

        private static Guid CreateDeterministicGuid(HeavyAppCSVModel item)
        {
            var key =
                $"{item.StartTime:O}|{item.EndTime:O}";

            var bytes = Encoding.UTF8.GetBytes(key);

            var hash = MD5.HashData(bytes);

            return new Guid(hash);
        }
    }
}
