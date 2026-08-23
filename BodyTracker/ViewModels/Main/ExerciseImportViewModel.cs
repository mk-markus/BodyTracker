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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BodyTracker.ViewModels.Main
{
    public partial class ExerciseImportViewModel : ObservableObject
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
        /// Gets or sets the collection of <see cref="SamsungExerciseModel"/> objects currently loaded in the view.
        /// </summary>
        [ObservableProperty] private ObservableCollection<SamsungExerciseModel> exerciseDatas;

        /// <summary>
        /// Gets the command that triggers the file auto-load mechanism.
        /// </summary>
        public IAsyncRelayCommand CommandRefresh { get; }

        /// <summary>
        /// Gets the command that triggers the file selection dialog.
        /// </summary>
        public IAsyncRelayCommand CommandOpen { get; }

        /// <summary>
        /// Gets the command that initiates the process of persisting the loaded data into the database.
        /// </summary>
        public IAsyncRelayCommand CommandInsert { get; }

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
        /// Gets or sets the observable string representing elapsed time durations or execution progress metrics.
        /// </summary>
        [ObservableProperty] private string timeElapse = string.Empty;

        /// <summary>
        /// A private string representing the root directory path being searched for data files.
        /// </summary>
        private string searchPath = string.Empty;

        /// <summary>
        /// A private string specifying the keyword or pattern used to filter file names during the search.
        /// </summary>
        private string searchTerm = "exercise";

        /// <summary>
        /// A private string representing the resolved target file path found during the search process.
        /// </summary>
        private string filePath = string.Empty;


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
        /// Initializes a new instance of the <see cref="ExerciseImportViewModel"/> class, 
        /// assigning the database service and file path references, initializing asynchronous relay commands for file operations, 
        /// and automatically triggering an initial CSV file load.
        /// </summary>
        /// <param name="db">The database service instance used for data persistence.</param>
        /// <param name="path">The file path or directory used for searching workout data files.</param>
        public ExerciseImportViewModel(DatabaseService db, string path)
        {
            databaseService = db;
            searchPath = path;

            CommandRefresh = new AsyncRelayCommand(RefreshAsync);
            CommandOpen = new AsyncRelayCommand(OpenAsync);
            CommandInsert = new AsyncRelayCommand(InsertAsync);

        }
        /// <summary>
        /// Asynchronously uploads the loaded <see cref="ExerciseDatas"/> entries 
        /// to the database for the currently selected person, reporting progress along the way.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task InsertAsync()
        {
            if (ExerciseDatas == null || ExerciseDatas.Count == 0)
            {
                GeneralErrorMessage = "No uploading data available.";
                return;
            }


            Stopwatch watch = Stopwatch.StartNew();

            using var cts = new CancellationTokenSource();

            var dispatcher = new DispatcherTimer();

            dispatcher.Interval = TimeSpan.FromSeconds(1);

            dispatcher.Tick += (s, e) => TimeElapse = $"Time Elapse: {watch.Elapsed.ToString(@"mm\:ss")}";

            dispatcher.Start();

            try
            {
                var pid = AppState.SelectedPersonId;

                IsLoading = true;
                ProgressValue = 0;

                var progress = new Progress<(double value, string Text)>(value =>
                {
                    ProgressValue = value.value;
                    ProgressText = $"{value.Text} {value.value:F0}%";
                });

                await databaseService.SyncExerciseAsync(pid, ExerciseDatas, progress);

                ProgressText = $"Uploaded ({ExerciseDatas.Count} Datas)";
            }
            catch (Exception ex)
            {
                GeneralErrorMessage = $"Error during upload: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                dispatcher.Stop();
                watch.Stop();
                TimeElapse = $"Total Upload Time: {watch.Elapsed.ToString(@"mm\:ss")}";
            }
        }

        /// <summary>
        /// Asynchronously opens a file dialog allowing the user to select a export file, 
        /// then parses and imports the data entries while updating progress indicators.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task OpenAsync()
        {
            var dlg = new OpenFileDialog
            {
                Filter = $"Specific Files (*{searchTerm}*.csv)|*{searchTerm}*.csv"
            };


            if (dlg.ShowDialog() != true)
                return;
            filePath = dlg.FileName;
            try
            {


                IsLoading = true;
                ProgressValue = 0;

                var progress = new Progress<double>(value =>
                {
                    ProgressValue = value * 100;
                    ProgressText = $"Imported: {value:F0}%";
                });

                var list = await SamsungDataCsvExtractor.ParseExerciseAsync(
                    filePath, progress,
                    new Progress<string>(text => ProgressText = text));

                ExerciseDatas = new ObservableCollection<SamsungExerciseModel>(list);

                ProgressText = $"Imported {list.Count} Datas";
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
        /// Asynchronously auto load a CSV export file, then parses and imports the data entries while updating progress indicators.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RefreshAsync()
        {
            try
            {

                var path = Directory.GetFiles(searchPath, $"*{searchTerm}*");

                if (!path.Any())
                {
                    GeneralErrorMessage = "No Files were found";
                    return;
                }

                if (path.Count() > 1)
                {
                    GeneralErrorMessage = "Several files were found. Please select one of the dialog files.";
                    _ = OpenAsync();
                    return;
                }

                else filePath = path[0];


                IsLoading = true;
                ProgressValue = 0;

                var progress = new Progress<double>(value =>
                {
                    ProgressValue = value * 100;
                    ProgressText = $"Imported: {value:F0}%";
                });

                var list = await SamsungDataCsvExtractor.ParseExerciseAsync(
                    filePath,
                    progress,
                    new Progress<string>(text => ProgressText = text));

                ExerciseDatas = new ObservableCollection<SamsungExerciseModel>(list);

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
