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
    public partial class FoodIntakePageViewModel : ObservableObject
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
        /// A collection of <see cref="FoodIntakeModel"/> objects representing the nutritional data currently loaded in the view.
        /// </summary>
        [ObservableProperty] private ObservableCollection<FoodIntakeModel> foodIntakes = new();

        /// <summary>
        /// Command to trigger the file selection dialog for importing Samsung Health food intake data.
        /// </summary>
        public IAsyncRelayCommand OpenSamsungFoodIntakeCommand { get; }

        /// <summary>
        /// Command to initiate the process of parsing the selected file and persisting the data to the database.
        /// </summary>
        public IAsyncRelayCommand DownloadSamsungFoodIntakeToDatabaseCommand { get; }

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
        /// Initializes a new instance of the <see cref="FoodIntakePageViewModel"/> class.
        /// </summary>
        /// <param name="db">The <see cref="DatabaseService"/> instance used for data persistence.</param>
        public FoodIntakePageViewModel(DatabaseService db)
        {
            databaseService = db;

            OpenSamsungFoodIntakeCommand = new AsyncRelayCommand(OpenFoodIntakeCsvFileAsync);

            DownloadSamsungFoodIntakeToDatabaseCommand = new AsyncRelayCommand(InsertFoodIntakeToDatabaseAsync);
        }

        /// <summary>
        /// Opens a file dialog for the user to select a CSV file and initiates the extraction process 
        /// for Samsung Health food intake records.
        /// </summary>
        /// <returns>A task representing the asynchronous file selection and extraction operation.</returns>
        private async Task OpenFoodIntakeCsvFileAsync()
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
                    ProgressValue = value;
                    ProgressText = $"Imported: {value:F0}%";
                });

                var list = await SamsungHealthDataCsvExtractor.ExtractFoodIntakeAsync(
                    dlg.FileName,
                    progress,
                    new Progress<string>(text => ProgressText = text));

                FoodIntakes = new ObservableCollection<FoodIntakeModel>(list);

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

        /// <summary>
        /// Initiates the asynchronous upload of the current <see cref="FoodIntakeModel"/> collection 
        /// to the database via the <see cref="DatabaseService"/>.
        /// </summary>
        /// <returns>A task representing the asynchronous database insertion operation.</returns>
        private async Task InsertFoodIntakeToDatabaseAsync()
        {
            if (FoodIntakes == null || FoodIntakes.Count == 0)
            {
                GeneralErrorMessage = "No uploading data available.";
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

                await databaseService.InsertSamsungHealthFoodIntake(pid, FoodIntakes, progress);

                ProgressText = $"Uploaded ({FoodIntakes.Count} Data entries)";
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