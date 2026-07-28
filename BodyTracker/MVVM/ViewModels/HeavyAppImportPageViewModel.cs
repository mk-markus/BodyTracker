using BodyTracker.MVVM.Models;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitnessTracker.Core;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BodyTracker.MVVM.ViewModels
{
    public partial class HeavyAppImportPageViewModel : ObservableObject
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the ViewModel.
        /// </summary>
        /// <remarks>
        /// Marked as <c>readonly</c> to ensure that the service reference remains 
        /// immutable throughout the lifetime of the ViewModel instance, preventing 
        /// accidental reassignment and ensuring architectural stability.
        /// </remarks>
        private readonly DatabaseService databaseServerice;

        /// <summary>
        /// A collection of <see cref="StepDailyTrendModel"/> objects representing the daily step trends currently loaded in the view.
        /// </summary>
        [ObservableProperty] private ObservableCollection<HeavyAppCSVModel> heavyAppDatas;

        /// <summary>
        /// Command to trigger the file selection dialog for importing Samsung Health daily step trend data.
        /// </summary>
        public IAsyncRelayCommand CommandOpenHeavyAppCsvFile { get; }

        /// <summary>
        /// Command to initiate the process of persisting the loaded step trend data to the database.
        /// </summary>
        public IAsyncRelayCommand CommandInsertHeavyAppCsvDatas { get; }

        /// <summary>
        /// The service responsible for parsing Samsung Health data from exported CSV files.
        /// </summary>
        private readonly HeavyAppCSVExtractor extractor = new HeavyAppCSVExtractor();

        /// <summary>
        /// Indicates whether an asynchronous data operation is currently in progress.
        /// Used to toggle UI loading states.
        /// </summary>
        [ObservableProperty] private bool isLoading;

        /// <summary>
        /// Represents the current completion percentage of an active data import or processing task.
        /// </summary>
        [ObservableProperty] private double progressValue;

        /// <summary>
        /// A descriptive status message detailing the current progress or state of an ongoing operation.
        /// </summary>
        [ObservableProperty] private string progressText = string.Empty;



        public HeavyAppImportPageViewModel(MainWindow shell, DatabaseService db)
        {

            databaseServerice = db;

            CommandOpenHeavyAppCsvFile = new AsyncRelayCommand(OpenFileDialogHeavyAppAsync);
            CommandInsertHeavyAppCsvDatas = new AsyncRelayCommand(DownloadHeavyAppToDatabaseAsync);

           
        }

        private async Task DownloadHeavyAppToDatabaseAsync()
        {
            if (HeavyAppDatas == null || HeavyAppDatas.Count == 0)
            {
                MessageBox.Show("No uploading data available.",
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            try
            {
                var pid = AppState.SelectedPersonId;

                IsLoading = true;
                ProgressValue = 0;

                var progress = new Progress<double>(value =>
                {
                    ProgressValue = value;
                    ProgressText = $"Uploaded: {value:F0}%";
                });

                await databaseServerice.InsertHeavyAppData(pid, HeavyAppDatas, progress);

                ProgressText = $"Uploaded ({HeavyAppDatas.Count} Data entries)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Uploading: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OpenFileDialogHeavyAppAsync()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog() != true)
                return;

            try
            {
                IsLoading = true;
                ProgressValue = 0;

                var progress = new Progress<double>(value =>
                {
                    ProgressValue = value * 100;
                    ProgressText = $"Imported: {value:F0}%";
                });

                var list = await HeavyAppCSVExtractor.ExtractAsync(dlg.FileName,
                    progress,
                    new Progress<string>(text => ProgressText = text));

                HeavyAppDatas = new ObservableCollection<HeavyAppCSVModel>(list);

                ProgressText = $"Imported ({list.Count} Data entries)";


                //DateTime start = new DateTime();
                //DateTime end = new DateTime();

                //// Debug: Inspect imported date range and numeric coverage
                //if (HeavyAppDatas != null && HeavyAppDatas.Count > 0)
                //{

                //    // Automatisch Bereich auf tatsächliche Daten setzen
                //    start = HeavyAppDatas.Min(x => x.StartTime).Date;
                //    end = HeavyAppDatas.Max(x => x.EndTime).Date;

                //    Debug.WriteLine($"Imported {HeavyAppDatas.Count} entries. Range: {HeavyAppDatas.Min(x => x.StartTime)} - {HeavyAppDatas.Max(x => x.EndTime)}");
                //    var defaultDates = HeavyAppDatas.Count(h => h.StartTime == start || h.EndTime == end);
                //    Debug.WriteLine($"Entries with default dates: {defaultDates}");

                //    var weightRepEntries = HeavyAppDatas.Where(h => h.WeightKg.HasValue && h.Reps.HasValue).ToList();
                //    var sumVolume = weightRepEntries.Sum(h => h.WeightKg!.Value * h.Reps!.Value);
                //    Debug.WriteLine($"Entries with weight+reps: {weightRepEntries.Count}. Sum volume: {sumVolume:F2}");

                   
                //}

                //Debug.WriteLine("Start Read Json: " + DateTime.Now);

                //var analyzer = new AppWorkoutLoadAnalyzer("C:\\Users\\KEMA\\Documents\\Programming\\Visual Studio Projects\\Projects\\BodyTracker\\BodyTracker\\Ressources\\JsonFiles\\extracted_exercises.json");

                //var logs = analyzer.ConvertToHeavyAppCSVDatasToGymWorkoutEntries(HeavyAppDatas);

                ////// Einstellbarer Datumsbereich (z.B. aktueller Monat)
                ////DateTime start = new DateTime(2025, 01, 01);
                ////DateTime end = new DateTime(2025, 12, 31);

                //// Auswertung abrufen (Brust = Primär 100%, Trizep/Schulter als Sekundär 50%)
                //var muscleDistribution = analyzer.GetMuscleDistribution(logs, start, end, primaryFactor: 1.0, secondaryFactor: 0.5);

                //Debug.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");
                //Debug.WriteLine("++++++++   Values Start     ++++++++++++++++");
                //Debug.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");
                //// Übergabe an das Diagramm (z.B. LiveCharts oder OxyPlot)
                //foreach (var item in muscleDistribution)
                //{
                //    Debug.WriteLine($"Muskel: {item.MuscleGroup} | " +
                //                      $"Anteil am Gesamtvolumen: {item.PercentageShare}% | " +
                //                      $"Gesamtlast: {item.TotalVolume} kg " +
                //                      $"(davon Primär: {item.PrimaryVolume} kg, Sekundär: {item.SecondaryVolume} kg)");
                //}


                //Debug.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");
                //Debug.WriteLine("++++++++   Values END       ++++++++++++++++");
                //Debug.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error during import: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

    }
}
