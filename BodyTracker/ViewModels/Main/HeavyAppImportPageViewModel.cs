using BodyTracker.Models;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels
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
        private readonly DatabaseService databaseService;

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

        /// <summary>
        /// Backing field for the general error message string.
        /// </summary>
        private string generalErrorMessage = "";

        /// <summary>
        /// Gets or sets the general error message, sending a database error message via the messenger when the value changes.
        /// </summary>
        public string GeneralErrorMessage
        {
            get => generalErrorMessage;
            set
            {
                generalErrorMessage = value;
                WeakReferenceMessenger.Default.Send(new DatabaseErrorMessage(generalErrorMessage));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shell"></param>
        /// <param name="db"></param>
        public HeavyAppImportPageViewModel(MainWindow shell, DatabaseService db)
        {
            databaseService = db;

            CommandOpenHeavyAppCsvFile = new AsyncRelayCommand(OpenFileDialogHeavyAppAsync);
            CommandInsertHeavyAppCsvDatas = new AsyncRelayCommand(DownloadHeavyAppToDatabaseAsync);
        }

        /// <summary>
        /// Asynchronously uploads the loaded Hevy app workout data entries to the database for the currently selected person, reporting progress along the way.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task DownloadHeavyAppToDatabaseAsync()
        {
            if (HeavyAppDatas == null || HeavyAppDatas.Count == 0)
            {
                GeneralErrorMessage = "No uploading data available.";
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

                await databaseService.InsertHeavyAppData(pid, HeavyAppDatas, progress);

                ProgressText = $"Uploaded ({HeavyAppDatas.Count} Data entries)";
            }
            catch (Exception ex)
            {
                GeneralErrorMessage = $"Error during upload: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Asynchronously opens a file dialog allowing the user to select a Hevy app CSV export file, then parses and imports the data entries while updating progress indicators.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
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
            }
            catch (Exception ex)
            {
                GeneralErrorMessage = $"Error - use the correct file: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region Disposal Pattern

        /// <summary>
        /// Tracks whether the object has been disposed to prevent double disposal.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources, suppressing finalization.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of the Dispose pattern, releasing managed resources such as the database service and unregistering messenger listeners when disposing is true.
        /// </summary>
        /// <param name="disposing">A value indicating whether managed resources should be released.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                Debug.WriteLine($"{this.GetType().Name} Disposing managed resources {GetHashCode()}");

                try
                {
                    databaseService?.Dispose();
                    WeakReferenceMessenger.Default.UnregisterAll(this);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{this.GetType().Name} Disposal Error: {ex.Message}");
                }
            }

            disposed = true;
        }

        #endregion

    }
}
