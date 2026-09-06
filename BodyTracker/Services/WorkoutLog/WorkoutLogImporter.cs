using BodyTracker.Models;
using BodyTracker.Services.WorkoutLog;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BodyTracker.Services
{
    /// <summary>
    /// Represents an asynchronous extractor for parsing Hevy workout application CSV data files.
    /// </summary>
    /// <remarks>Reads CSV files line by line using CsvHelper configured with custom mappings, generates deterministic GUIDs for records based on timestamps, and reports extraction progress asynchronously.</remarks>
    public class WorkoutLogImporter
    {
        /// <summary>
        /// Asynchronously extracts, parses, and processes Hevy workout records from a specified CSV file.
        /// </summary>
        /// <remarks>Counts total lines, configures CsvHelper with custom delimiters and relaxed validation rules, registers <see cref="HevyAppCSCMap"/>, assigns deterministic data UUIDs, and reports progress percentages.</remarks>
        /// <param name="filePath">The file system path to the source CSV data file.</param>
        /// <param name="progress">An optional progress reporter for tracking percentage completion. Defaults to null.</param>
        /// <param name="status">An optional progress reporter for tracking status messages. Defaults to null.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of extracted <see cref="HevyAppCSVModel"/> records.</returns>
        public static async Task<List<HevyAppCSVModel>> ImportCSVAsync(string filePath,
                                                                   IProgress<double>? progress = null,
                                                                   IProgress<string>? status = null)
        {
            var result = new List<HevyAppCSVModel>();

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

            csv.Context.RegisterClassMap<HevyAppCSCMap>();

            await foreach (var item in csv.GetRecordsAsync<HevyAppCSVModel>())
            {
                item.DataUuid = GenerateWorkoutHash(item);

                result.Add(item);

                currentLine++;

                progress?.Report(currentLine * 100.0 / totalLines);
            }

            return result;
        }

        /// <summary>
        /// Asynchronously fetches workout sessions from the online Hevy API and flattens them 
        /// into a structured collection of <see cref="HevyAppCSVModel"/> entries for tracking and analysis.
        /// </summary>
        /// <param name="progress">An optional progress reporter for tracking the completion percentage (0 to 100).</param>
        /// <param name="status">An optional progress reporter for transmitting status or error messages during execution.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing a list of flattened <see cref="HevyAppCSVModel"/> records representing individual exercise sets.
        /// </returns>
        public static async Task<List<HevyAppCSVModel>> ImportOnlineAsync(IProgress<(double value, string prgressText)>? progress = null)
        {
            var result = new List<HevyAppCSVModel>();

            var api = new HevyAppAPIService();

            try
            {
                var workouts = await api.FetchAndProcessWorkoutsAsync();
                var totalWorkouts = workouts.Count;

                progress?.Report((1,
                           $"Downloaded: "));

                var currentLine = 0;

                foreach (var item in workouts)
                {
                    foreach (var exercise in item.Exercises)
                    {
                        foreach (var set in exercise.Sets)
                        {
                            var csvModel = new HevyAppCSVModel
                            {
                                StartTime = item.StartTime.UtcDateTime,
                                EndTime = item.EndTime.UtcDateTime,
                                Title = item.Title,
                                Description = item.Description,
                                ExerciseTitle = exercise.Title,
                                ExerciseNotes = exercise.Notes,
                                SetIndex = set.Index,
                                SetType = set.Type,
                                WeightKg = set.WeightKg,
                                Reps = set.Reps,
                                DistanceKm = set.DistanceMeters / 1000,
                                DurationSeconds = set.DurationSeconds,
                                Rpe = set.Rpe
                            };
                            csvModel.DataUuid = GenerateWorkoutHash(csvModel);
                            result.Add(csvModel);
                 
                           
                        }
                    }
                    currentLine++;
                    var percentage = currentLine * 100.0 / totalWorkouts;
                    progress?.Report((percentage,
                            $"Downloaded: "));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching workouts: {ex.Message}");
            }

            return result;
        }





        /// <summary>
        /// Generates a deterministic <see cref="Guid"/> for a workout record based on its start and end timestamps.
        /// </summary>
        /// <remarks>Constructs a composite string key from the record's start and end times in round-trip format, computes an MD5 hash of the UTF-8 bytes, and instantiates a GUID from the resulting hash.</remarks>
        /// <param name="item">The <see cref="HevyAppCSVModel"/> record for which to generate a deterministic identifier.</param>
        /// <returns>A deterministic <see cref="Guid"/> derived from the record timestamps.</returns>
        private static Guid CreateDeterministicGuid(HevyAppCSVModel workout)
        {
            var key =
                $"{workout.StartTime:yyyyMMddHHmmss}|" +
                $"{workout.EndTime:yyyyMMddHHmmss}|" +
                $"{workout.Title}|" +
                $"{workout.Description}|" +
                $"{workout.ExerciseTitle}|" +
                $"{workout.ExerciseNotes}|" +
                $"{workout.SetIndex}|" +
                $"{workout.SetType}|" +
                $"{workout.WeightKg}|" +
                $"{workout.Reps}|" +
                $"{workout.DistanceKm}|" +
                $"{workout.DurationSeconds}|" +
                $"{workout.Rpe}";

            var bytes = Encoding.UTF8.GetBytes(key);

            var hash = MD5.HashData(bytes);

            return new Guid(hash);
        }

        public static string GenerateWorkoutHash(HevyAppCSVModel workout)
        {
            var rawString =
                $"{workout.StartTime:yyyyMMddHHmmss}|" +
                $"{workout.EndTime:yyyyMMddHHmmss}|" +
                $"{workout.Title}|" +
                $"{workout.Description}|" +
                $"{workout.ExerciseTitle}|" +
                $"{workout.ExerciseNotes}|" +
                $"{workout.SetIndex}|" +
                $"{workout.SetType}|" +
                $"{workout.WeightKg}|" +
                $"{workout.Reps}|" +
                $"{workout.DistanceKm}|" +
                $"{workout.DurationSeconds}|" +
                $"{workout.Rpe}";

            using var sha = SHA256.Create();

            var bytes = Encoding.UTF8.GetBytes(rawString);

            return Convert.ToHexString(sha.ComputeHash(bytes));
        }

    }
}
