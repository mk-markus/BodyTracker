using BodyTracker.MVVM.Models;
using BodyTracker.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitnessTracker.Core;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BodyTracker.MVVM.ViewModels
{
    public partial class PersonalWorkoutInsightsPageViewModel : ObservableObject
    {
        private readonly DatabaseService databaseService;
        private readonly MainWindow mainWindow;

        [ObservableProperty] private string totalWorkoutsVolume;

        [ObservableProperty] private string totalWorkoutsPrimaryVolume;

        [ObservableProperty] private string totalWorkoutsSecondaryVolume;


        [ObservableProperty] private string totalWorkouts;

        [ObservableProperty] private IEnumerable<ISeries> workoutMuscleDistributionSeries;


        [ObservableProperty] private ObservableCollection<ExerciseFrequencyModel> topExercises;

       

        public PersonalWorkoutInsightsPageViewModel(MainWindow shell, DatabaseService db)
        {
            databaseService = db;
            mainWindow = shell;

            // Initiales Laden beim Start der View
            _ = RefreshDataAsync();
        }

        /// <summary>
        /// Asynchrone Methode zum Laden der CSV-Daten und Aktualisieren des Pie-Charts.
        /// Kann auch als Command an einen Refresh-Button im UI gebunden werden.
        /// </summary>
        [RelayCommand]
        public async Task RefreshDataAsync()
        {
            try
            {
                // 1. Asynchrones Extrahieren der CSV-Daten
                var list = await HeavyAppCSVExtractor.ExtractAsync("C:\\Users\\KEMA\\Documents\\Programming\\Visual Studio Projects\\Projects\\BodyTracker\\BodyTracker\\Ressources\\CsvFiles\\workout_data.csv");

                if (list == null || !list.Any())
                {
                    Debug.WriteLine("Keine Workout-Daten gefunden.");
                    return;
                }

                var heavyAppDatas = new ObservableCollection<HeavyAppCSVModel>(list);

                DateTime start = heavyAppDatas.Min(x => x.StartTime).Date;
                DateTime end = heavyAppDatas.Max(x => x.EndTime).Date;

                // 2. Analyzer initialisieren und Konvertierung durchführen
                var analyzer = new AppWorkoutLoadAnalyzer("", list, 1, 0.5);

                TotalWorkoutsVolume = analyzer.TotalWorkoutVolume.TotalVolume.ToString("N0", new System.Globalization.CultureInfo("de-DE")) + " kg";
                TotalWorkoutsPrimaryVolume = analyzer.TotalWorkoutVolume.PrimaryVolume.ToString("N0", new System.Globalization.CultureInfo("de-DE")) + " kg";
                TotalWorkoutsSecondaryVolume = analyzer.TotalWorkoutVolume.SecondaryVolume.ToString("N0", new System.Globalization.CultureInfo("de-DE")) + " kg";

                TotalWorkouts = analyzer.TotalWorkoutVolume.TotalExercises.ToString("N0", new System.Globalization.CultureInfo("de-DE")) + " x";

                TopExercises = new ObservableCollection<ExerciseFrequencyModel>(analyzer.FrequentlyPerformedExercises);
                //var logs = analyzer.ConvertToGymWorkoutEntries(heavyAppDatas);

                var muscleDistribution = analyzer.GetMuscleDistribution(start, end);

   
                // 4. PieSeries für Gruppen mit Volumen > 0 erzeugen
                var pieSeries = muscleDistribution
                   .Where(m => m.TotalVolume > 0)
                   .Select(m => (ISeries)new PieSeries<double>
                   {
                       Name = string.IsNullOrWhiteSpace(m.MuscleGroup) ? "<unknown>" : m.MuscleGroup,
                       Values = new double[] { m.PercentageShare },
                       DataLabelsPosition = PolarLabelsPosition.Middle,
                       DataLabelsFormatter = point => $"{point.Context.Label ?? point.Coordinate.PrimaryValue.ToString("F2")} %",
                       ToolTipLabelFormatter = point => $"{point.Context.Label ?? point.Coordinate.PrimaryValue.ToString("F2")} %"
                   })
                   .ToArray();

                WorkoutMuscleDistributionSeries = pieSeries;

            }
            // Korrekte Ausnahmebehandlung, um Abstürze bei Dateizugriffen zu verhindern
            catch (Exception ex)
            {
                Debug.WriteLine($"Fehler beim Aktualisieren der Workout-Insights: {ex.Message}");
            }
        }
    }
}