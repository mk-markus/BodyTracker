using BodyTracker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BodyTracker.Services
{

    public class AppWorkoutLoadAnalyzer
    {
        /// <summary>
        /// Gets or sets the path pointing from the execution directory up to the project root directory.
        /// </summary>
        /// <remarks>Navigates upwards from build output directories (such as bin\Debug\net8.0-windows) to locate source assets and configuration files.</remarks>
        private readonly string ProjectRootPath = string.Empty;

        /// <summary>
        /// Gets or sets the absolute or relative file path pointing to the exercise configuration JSON file.
        /// </summary>
        /// <remarks>Used by data loading routines to locate and read exercise metadata definitions from disk.</remarks>
        private readonly string JsonFilePath = ".\\Ressources\\JsonFiles\\Database_Exercises.json";

        /// <summary>
        /// Gets the dictionary mapping exercise names to their corresponding JSON configuration models.
        /// </summary>
        /// <remarks>Provides case-insensitive O(1) lookups for exercise details, primary muscles, and secondary muscle groups.</remarks>
        public Dictionary<string, JsonGymExerciseModel> ExerciseByName { get; private set; }

        /// <summary>
        /// Contains already the Converted to GymWorkoutEntryModel Datas form the App Data CSV
        /// </summary>
        /// <remarks>Holds the standardized workout log entries parsed from external application CSV imports for evaluation and charting.</remarks>
        public IEnumerable<GymWorkoutEntryModel> WorkoutEntries { get; private set; }

        /// <summary>
        /// Gets or sets the aggregated total workout volume metrics across all muscle groups.
        /// </summary>
        /// <remarks>Represents the generalized summary of lifting output incorporating primary and secondary weighting factors.</remarks>
        public TotalWorkoutModel WorkoutVolume { get; private set; }


        /// <summary>
        /// Gets or sets the aggregated total workout volume metrics across all muscle groups.
        /// </summary>
        /// <remarks>Represents the generalized summary of lifting output incorporating primary and secondary weighting factors.</remarks>
        public MuscleDataResultsModel MuscleData { get; private set; }

        /// <summary>
        /// Gets the collection of most frequently performed exercises.
        /// </summary>
        /// <remarks>Provides a publicly readable collection of <see cref="ExerciseFrequencyModel"/> entries populated by analysis routines.</remarks>
        public IEnumerable<ExerciseFrequencyModel> TopExercises { get; private set; }

        /// <summary>
        /// A private field representing the starting date boundary for exercise filtering.
        /// </summary>
        /// <remarks>Initialized to a default <see cref="DateTime"/> value and used to bound analytical queries.</remarks>
        private DateTime FilteredStartDate = new DateTime();

        /// <summary>
        /// A private field representing the ending date boundary for exercise filtering.
        /// </summary>
        /// <remarks>Initialized to a default <see cref="DateTime"/> value and used to bound analytical queries.</remarks>
        private DateTime FilteredEndDate = new DateTime();

        /// <summary>
        /// Initializes a new instance of the <see cref="AppWorkoutLoadAnalyzer"/> class with the specified configuration path, workout data, and weighting factors.
        /// </summary>
        /// <remarks>Resolves file paths if the dictionary path is unassigned, loads the exercise metadata dictionary from disk, converts raw CSV import models into 
        /// standardized workout entries, determines the overall evaluation date range, and computes initial aggregate workout volumes.</remarks>
        /// <param name="dictionaryJsonFilePath">The file path to the exercise definition JSON file; if null or empty, a default project directory path is used.</param>
        /// <param name="appDatas">The collection of raw workout entries imported from external application CSV data sources.</param>
        /// <param name="primaryMuscleFactor">The weighting factor applied to primary muscle volume calculations (defaults to 1.0).</param>
        /// <param name="secondaryMuscleFactor">The weighting factor applied to secondary muscle volume calculations (defaults to 0.5).</param>
        /// <exception cref="FileNotFoundException">Thrown when the target exercise definition JSON file cannot be found on disk.</exception>
        public AppWorkoutLoadAnalyzer(string dictionaryJsonFilePath, List<HeavyAppCSVModel> appDatas, double primaryMuscleFactor = 1.0, double secondaryMuscleFactor = 0.5)
        {

            if (string.IsNullOrEmpty(dictionaryJsonFilePath))
            {
                ProjectRootPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory));
                JsonFilePath = Path.Combine(ProjectRootPath, "Ressources", "JsonFiles", "Database_Exercises.json");

            }

            if (!File.Exists(JsonFilePath)) throw new FileNotFoundException($"Die JSON-Datei wurde unter '{JsonFilePath}' nicht gefunden.");

            ExerciseByName = LoadExerciseLookup(JsonFilePath);

            WorkoutEntries = ParseHeavyWorkouts(appDatas);

            WorkoutVolume = GetVolume(WorkoutEntries, primaryMuscleFactor, secondaryMuscleFactor);

            TopExercises = CalculateExerciseFrequency(WorkoutEntries);


        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AppWorkoutLoadAnalyzer"/> class with the specified configuration path, workout data, and weighting factors.
        /// </summary>
        /// <remarks>Resolves file paths if the dictionary path is unassigned, loads the exercise metadata dictionary from disk, converts raw CSV import models into 
        /// standardized workout entries, determines the overall evaluation date range, and computes initial aggregate workout volumes.</remarks>
        /// <param name="dictionaryJsonFilePath">The file path to the exercise definition JSON file; if null or empty, a default project directory path is used.</param>
        /// <param name="appDatas">The collection of raw workout entries imported from external application CSV data sources.</param>
        /// <param name="primaryMuscleFactor">The weighting factor applied to primary muscle volume calculations (defaults to 1.0).</param>
        /// <param name="secondaryMuscleFactor">The weighting factor applied to secondary muscle volume calculations (defaults to 0.5).</param>
        /// <exception cref="FileNotFoundException">Thrown when the target exercise definition JSON file cannot be found on disk.</exception>
        public AppWorkoutLoadAnalyzer(string dictionaryJsonFilePath, List<GymWorkoutEntryModel> appDatas, double primaryMuscleFactor = 1.0, double secondaryMuscleFactor = 0.5)
        {

            if (string.IsNullOrEmpty(dictionaryJsonFilePath))
            {
                ProjectRootPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
                JsonFilePath = Path.Combine(ProjectRootPath, "Ressources", "JsonFiles", "Database_Exercises.json");
            }

            if (!File.Exists(JsonFilePath)) throw new FileNotFoundException($"Die JSON-Datei wurde unter '{JsonFilePath}' nicht gefunden.");

            ExerciseByName = LoadExerciseLookup(JsonFilePath);

            WorkoutEntries = appDatas;

            WorkoutVolume = GetVolume(WorkoutEntries, primaryMuscleFactor, secondaryMuscleFactor);

            TopExercises = CalculateExerciseFrequency(WorkoutEntries);


        }

        /// <summary>
        /// Calculates the total cumulative workout volume across all recognized exercises within a specified date range.
        /// </summary>
        /// <remarks>Filters workout entries by date, validates exercises against a known dictionary, computes set volume (weight multiplied by repetitions), and applies primary and secondary weighting factors.</remarks>
        /// <param name="gymAppLogs">The collection of workout entries to process.</param>
        /// <param name="startDate">The start date of the evaluation period (inclusive).</param>
        /// <param name="endDate">The end date of the evaluation period (inclusive).</param>
        /// <param name="primaryFactor">The weighting factor applied to primary volume calculations (defaults to 1.0).</param>
        /// <param name="secondaryFactor">The weighting factor applied to secondary volume calculations (defaults to 0.5).</param>
        /// <returns>A <see cref="MuscleDataResultsModel"/> object containing the aggregated volume metrics for all muscles.</returns>
        public TotalWorkoutModel GetVolume(IEnumerable<GymWorkoutEntryModel> gymAppLogs,
                                           double primaryFactor = 1.0,
                                           double secondaryFactor = 0.5,
                                           DateTime startDate = default,
                                           DateTime endDate = default)
        {

            if (gymAppLogs == null || !gymAppLogs.Any()) throw new ArgumentException("The provided gymAppLogs collection is null or empty.", nameof(gymAppLogs));

            if (startDate == default || endDate == default)
            {
                FilteredStartDate = gymAppLogs.Min(x => x.ExcerciseDate).Date;
                FilteredEndDate = gymAppLogs.Max(x => x.ExcerciseDate).Date;
            }

            else
            {
                FilteredStartDate = startDate;
                FilteredEndDate = endDate;
            }

            //// 1. Filter by date range
            var filteredLogs = gymAppLogs.Where(log => log.ExcerciseDate.Date >= FilteredStartDate.Date && log.ExcerciseDate.Date <= FilteredEndDate.Date);

            double totalVolume = 0;

            int countExercises = 0;

            var lastDate = new DateTime(1500, 01, 01);

            foreach (var entry in filteredLogs)
            {
                // Optionally verify if the exercise exists in the dictionary (filtering for known exercises only)
                if (!ExerciseByName.ContainsKey(entry.ExerciseName))
                    continue;

                // Calculate set volume (weight * repetitions) and accumulate
                double setVolume = entry.Weight.Value * entry.Reps.Value;
                totalVolume += setVolume;

                if (entry.ExcerciseDate != lastDate)
                {
                    countExercises++;
                    lastDate = entry.ExcerciseDate;
                }

            }

            var priMuscleVol = totalVolume * primaryFactor;
            var secMuscleVol = totalVolume * secondaryFactor;

            return new TotalWorkoutModel
            {
                PrimaryVolume = priMuscleVol,
                SecondaryVolume = secMuscleVol,
                TotalExercises = countExercises
            };
        }


        /// <summary>
        /// Asynchronously calculates and returns the aggregated monthly exercise training volume within the specified date range.
        /// </summary>
        /// <remarks>Filters workout entries by the provided date bounds, groups entries by calendar date, applies weighting factors for primary and secondary volumes, and builds a chronological list of monthly exercise volume records.</remarks>
        /// <param name="startDate">The inclusive starting date boundary for filtering workout entries.</param>
        /// <param name="endDate">The inclusive ending date boundary for filtering workout entries.</param>
        /// <param name="primaryFactor">The multiplier factor applied to primary volume calculations. Defaults to 1.0.</param>
        /// <param name="secondaryFactor">The multiplier factor applied to secondary volume calculations. Defaults to 0.5.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="MonthlyExerciseTrainingVolumeModel"/> aggregated records.</returns>
        /// <exception cref="ArgumentException">Thrown when the <see cref="WorkoutEntries"/> collection is null or empty.</exception>
        public async Task<List<MonthlyExerciseTrainingVolumeModel>> CalculateMonthlyVolumeAsync(DateTime startDate, DateTime endDate, double primaryFactor = 1.0, double secondaryFactor = 0.5)
        {
            if (WorkoutEntries == null || !WorkoutEntries.Any()) throw new ArgumentException("The provided WorkoutEntries collection is null or empty.", nameof(WorkoutEntries));

            var filteredLogs = WorkoutEntries
                .Where(log =>
                    log.ExcerciseDate.Date >= startDate.Date &&
                    log.ExcerciseDate.Date <= endDate.Date)
                .ToList();

            var monthlyGroups = filteredLogs
                                    .GroupBy(x => x.ExcerciseDate.Date)
                                    .OrderBy(g => g.Key);

            var result = new List<MonthlyExerciseTrainingVolumeModel>();


            foreach (var group in monthlyGroups)
            {
                double totalVolume = 0;
                int totalExercises = 0;
                // Ermittelt das späteste Datum (letzter Trainingstag) in diesem Monat als "lastDate"
                DateTime lastDateInMonth = group.Max(x => x.ExcerciseDate.Date);

                foreach (var entry in group)
                {
                    double setVolume = entry.Weight.Value * entry.Reps.Value;
                    totalVolume += setVolume;
                    totalExercises++;
                }

                result.Add(new MonthlyExerciseTrainingVolumeModel
                {
                    Date = lastDateInMonth, // Das letzte konkrete Trainingsdatum des Monats
                    PrimaryVolume = totalVolume * primaryFactor,
                    SecondaryVolume = totalVolume * secondaryFactor,
                    TotalExercises = totalExercises // Anzahl der eindeutigen Trainingstage in diesem Monat
                });
            }

            return result;
        }

        /// <summary>
        /// Calculates the detailed volume distribution across specific muscle groups for a given date range.
        /// </summary>
        /// <remarks>Processes workout entries, maps exercises to their respective primary and secondary muscle groups using weighted factors, computes percentage shares, and sorts the results in descending order.</remarks>
        /// <param name="startDate">The start date of the evaluation period (inclusive).</param>
        /// <param name="endDate">The end date of the evaluation period (inclusive).</param>
        /// <param name="gymAppLogs">The optional collection of workout logs; falls back to default app entries if null or empty.</param>
        /// <param name="primaryFactor">The weighting factor for primary muscle involvement (defaults to 1.0).</param>
        /// <param name="secondaryFactor">The weighting factor for secondary muscle involvement (defaults to 0.5).</param>
        /// <returns>A list of <see cref="MuscleDataResultsModel"/> sorted by percentage share in descending order.</returns>
        public List<MuscleDataResultsModel> CalculateMuscleSplit(IEnumerable<GymWorkoutEntryModel> gymAppLogs,
                                                             double primaryFactor = 1.0,
                                                             double secondaryFactor = 0.5,
                                                             DateTime startDate = default,
                                                             DateTime endDate = default)
        {

            if (gymAppLogs == null || !gymAppLogs.Any()) throw new ArgumentException("The provided gymAppLogs collection is null or empty.", nameof(gymAppLogs));

            if (startDate == default || endDate == default)
            {
                FilteredStartDate = gymAppLogs.Min(x => x.ExcerciseDate).Date;
                FilteredEndDate = gymAppLogs.Max(x => x.ExcerciseDate).Date;
            }

            else
            {
                FilteredStartDate = startDate;
                FilteredEndDate = endDate;
            }



            //// Fall back to default app entries if no logs were provided or the list is empty
            //var logsToProcess = (gymAppLogs == null || !gymAppLogs.Any()) ? WorkoutEntries : gymAppLogs;

            // 1. Filter by date
            var filteredLogs = gymAppLogs.Where(log => log.ExcerciseDate.Date >= FilteredStartDate.Date && log.ExcerciseDate.Date <= FilteredEndDate.Date);

            var primaryMap = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var secondaryMap = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in filteredLogs)
            {
                if (!ExerciseByName.TryGetValue(entry.ExerciseName, out var exercise))
                    continue;

                // Calculate set volume (weight * repetitions)
                var setVolume = entry.Weight * entry.Reps;

                // Attribute volume to primary muscles (e.g., 100%)
                foreach (var muscle in exercise.PrimaryMuscles)
                {
                    var vol = setVolume * primaryFactor;
                    if (primaryMap.ContainsKey(muscle)) primaryMap[muscle] += vol.Value;
                    else primaryMap[muscle] = vol.Value;
                }

                // Attribute volume to secondary muscles (e.g., 50%)
                foreach (var muscle in exercise.SecondaryMuscles)
                {
                    double vol = setVolume.Value * secondaryFactor;
                    if (secondaryMap.ContainsKey(muscle)) secondaryMap[muscle] += vol;
                    else secondaryMap[muscle] = vol;
                }
            }

            // Merge all involved muscle groups
            var allMuscles = primaryMap.Keys.Union(secondaryMap.Keys, StringComparer.OrdinalIgnoreCase);
            var rawResults = new List<MuscleDataResultsModel>();

            foreach (var muscle in allMuscles)
            {
                primaryMap.TryGetValue(muscle, out double pVol);
                secondaryMap.TryGetValue(muscle, out double sVol);

                rawResults.Add(new MuscleDataResultsModel
                {
                    MuscleGroup = muscle,
                    PrimaryVolume = pVol,
                    SecondaryVolume = sVol
                });
            }

            // Determine grand total volume across all muscles to calculate percentage shares
            double grandTotalVolume = rawResults.Sum(x => x.TotalVolume);

            var chartData = new List<MuscleDataResultsModel>();
            foreach (var item in rawResults)
            {
                double share = grandTotalVolume > 0 ? item.TotalVolume / grandTotalVolume * 100.0 : 0.0;

                chartData.Add(new MuscleDataResultsModel
                {
                    MuscleGroup = item.MuscleGroup,
                    PrimaryVolume = item.PrimaryVolume,
                    SecondaryVolume = item.SecondaryVolume,
                    PercentageShare = Math.Round(share, 2) // Percentage share for pie or bar charts
                });
            }

            return chartData.OrderByDescending(x => x.PercentageShare).ToList();
        }

        /// <summary>
        /// Converts raw CSV import models from external workout applications into standardized application entities.
        /// </summary>
        /// <remarks>Safely maps CSV properties, handles null values with fallback defaults, and structures the records into domain-compliant workout entries.</remarks>
        /// <param name="csvModels">The collection of raw CSV import records.</param>
        /// <returns>A list of standardized <see cref="GymWorkoutEntryModel"/> instances.</returns>
        public List<GymWorkoutEntryModel> ParseHeavyWorkouts(IEnumerable<HeavyAppCSVModel> csvModels)
        {
            if (csvModels == null)
            {
                return new List<GymWorkoutEntryModel>();
            }

            return csvModels
                .Select(csv => new GymWorkoutEntryModel
                {
                    // Date is inherited from StartTime (date portion or full timestamp)
                    ExcerciseDate = csv.StartTime,

                    // Map exercise name safely
                    ExerciseName = csv.ExerciseTitle ?? string.Empty,

                    // Safeguard weight values (defaults to 0.0 if null)
                    Weight = csv.WeightKg ?? 0.0,

                    // Safeguard repetition values (defaults to 0.0 if null)
                    Reps = csv.Reps ?? 0.0,

                    // Direct assignment of set index
                    SetIndex = csv.SetIndex
                })
                .ToList();
        }


        /// <summary>
        /// Loads and parses exercise metadata from an external JSON configuration file into a case-insensitive dictionary.
        /// </summary>
        /// <remarks>Validates file existence, reads content text, deserializes JSON data with case-insensitive property matching, and builds a dictionary keyed by exercise name.</remarks>
        /// <param name="filePath">The absolute or relative path to the target JSON file.</param>
        /// <returns>A dictionary mapping exercise names to their corresponding <see cref="JsonGymExerciseModel"/> definitions.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the specified JSON file cannot be located.</exception>
        public Dictionary<string, JsonGymExerciseModel> LoadExerciseLookup(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Die JSON-Datei wurde unter '{filePath}' nicht gefunden.");
            }

            string jsonContent = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var exercises = JsonSerializer.Deserialize<List<JsonGymExerciseModel>>(jsonContent, options)
                            ?? new List<JsonGymExerciseModel>();

            return exercises.GroupBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        }




        /// <summary>
        /// Ermittelt die Häufigkeit aller Übungen innerhalb eines Zeitraums.
        /// Die Ergebnisse werden nach Häufigkeit absteigend sortiert.
        /// </summary>
        public IEnumerable<ExerciseFrequencyModel> CalculateExerciseFrequency(IEnumerable<GymWorkoutEntryModel> gymAppLogs,
                                                                              DateTime startDate = default,
                                                                              DateTime endDate = default)
        {

            if (startDate == default || endDate == default)
            {
                startDate = gymAppLogs.Min(x => x.ExcerciseDate).Date;
                endDate = gymAppLogs.Max(x => x.ExcerciseDate).Date;
            }

            // Zeitraum filtern
            var filteredLogs = gymAppLogs
                .Where(x => x.ExcerciseDate.Date >= startDate.Date &&
                            x.ExcerciseDate.Date <= endDate.Date);

            // Nur bekannte Übungen berücksichtigen
            var exerciseCounts = filteredLogs
                .Where(x => ExerciseByName.ContainsKey(x.ExerciseName))
                .GroupBy(x => x.ExerciseName)
                .Select(g => new
                {
                    ExerciseName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            if (!exerciseCounts.Any())
                return Enumerable.Empty<ExerciseFrequencyModel>();

            int maxCount = exerciseCounts.Max(x => x.Count);

            return exerciseCounts.Select(x => new ExerciseFrequencyModel
            {
                ExerciseName = x.ExerciseName,
                Count = x.Count,
                Percentage = (double)x.Count / maxCount
            });
        }

       ///// <summary>
       ///// Calculates and returns the daily exercise training volume for the year 2026.
       ///// </summary>
       ///// <remarks>Validates that the workout entries collection is not null or empty, filters entries for the year 2026, groups them by calendar date, validates exercises against the exercise dictionary, computes weighted primary and secondary volumes, and counts distinct exercises performed each day.</remarks>
       ///// <returns>A list of <see cref="MonthlyExerciseTraningVolume"/> aggregated records representing daily training volumes for 2026.</returns>
       ///// <exception cref="ArgumentException">Thrown when the <see cref="WorkoutEntries"/> collection is null or empty.</exception>
       // public List<MonthlyExerciseTraningVolume> GetDailyExerciseVolumeFor2026()
       // {
       //     if (WorkoutEntries == null || !WorkoutEntries.Any())
       //     {
       //         throw new ArgumentException(
       //             "The provided WorkoutEntries collection is null or empty.",
       //             nameof(WorkoutEntries));
       //     }

       //     var entries2026 = WorkoutEntries
       //         .Where(x => x.ExcerciseDate.Year == 2026)
       //         .ToList();

       //     var result = new List<MonthlyExerciseTraningVolume>();

       //     var dailyGroups = entries2026
       //         .GroupBy(x => x.ExcerciseDate.Date)
       //         .OrderBy(x => x.Key);

       //     foreach (var day in dailyGroups)
       //     {
       //         double totalVolume = 0;

       //         foreach (var entry in day)
       //         {
       //             if (!ExerciseByName.ContainsKey(entry.ExerciseName))
       //                 continue;

       //             totalVolume += entry.Weight.Value * entry.Reps.Value;
       //         }

       //         result.Add(new MonthlyExerciseTraningVolume
       //         {
       //             Date = day.Key,
       //             PrimaryVolume = totalVolume * 1,
       //             SecondaryVolume = totalVolume * 0.5,
       //             TotalExercises = day
       //                 .Select(x => x.ExerciseName)
       //                 .Distinct()
       //                 .Count()
       //         });
       //     }

       //     return result;
       // }
    }
}