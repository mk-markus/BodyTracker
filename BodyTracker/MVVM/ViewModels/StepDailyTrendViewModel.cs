using BodyTracker.MVVM.Models;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace BodyTracker.MVVM.ViewModels
{
    public partial class StepDailyTrendViewModel : ObservableObject
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
        [ObservableProperty] private ObservableCollection<StepDailyTrendModel> stepDailyTrend;

        /// <summary>
        /// Command to trigger the file selection dialog for importing Samsung Health daily step trend data.
        /// </summary>
        public IAsyncRelayCommand OpenSamsungStepDailyTrendCommand { get; }

        /// <summary>
        /// Command to initiate the process of persisting the loaded step trend data to the database.
        /// </summary>
        public IAsyncRelayCommand InsertSamsungStepsDailyTrendToDatabaseCommand { get; }

        /// <summary>
        /// The service responsible for parsing Samsung Health data from exported CSV files.
        /// </summary>
        private readonly SamsungHealthDataCsvExtractor _extractor = new SamsungHealthDataCsvExtractor();

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

        /// <summary>
        /// Initializes a new instance of the <see cref="StepDailyTrendViewModel"/> class.
        /// </summary>
        /// <param name="db">The <see cref="DatabaseService"/> instance used for data persistence.</param>
        public StepDailyTrendViewModel(DatabaseService db)
        {
            databaseServerice = db;
            OpenSamsungStepDailyTrendCommand = new AsyncRelayCommand(OpenFileDialogStepDailyTrendAsync);
            InsertSamsungStepsDailyTrendToDatabaseCommand = new AsyncRelayCommand(DownloadSamsungStepsDailyTrendToDatabaseAsync);
        }

        /// <summary>
        /// Opens a file dialog for the user to select a CSV file and initiates the extraction process 
        /// for Samsung Health daily step trend records.
        /// </summary>
        /// <returns>A task representing the asynchronous file selection and extraction operation.</returns>
        private async Task OpenFileDialogStepDailyTrendAsync()
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

                var list = await SamsungHealthDataCsvExtractor.ExtractDailyStepTrendAsync(
                    dlg.FileName,
                    progress,
                    new Progress<string>(text => ProgressText = text));

                StepDailyTrend = new ObservableCollection<StepDailyTrendModel>(list);

                ProgressText = $"Imported ({list.Count} Data entries)";
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

        /// <summary>
        /// Initiates the asynchronous upload of the current <see cref="StepDailyTrendModel"/> collection 
        /// to the database via the <see cref="DatabaseService"/>.
        /// </summary>
        /// <returns>A task representing the asynchronous database insertion operation.</returns>
        private async Task DownloadSamsungStepsDailyTrendToDatabaseAsync()
        {
            if (StepDailyTrend == null || StepDailyTrend.Count == 0)
            {
                MessageBox.Show(
                    "No uploading data available.",
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

                await databaseServerice.InsertSamsungHealthStepDailyTrend(pid, StepDailyTrend, progress);

                ProgressText = $"Uploaded ({StepDailyTrend.Count} Data entries)";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error Uploading: {ex.Message}",
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