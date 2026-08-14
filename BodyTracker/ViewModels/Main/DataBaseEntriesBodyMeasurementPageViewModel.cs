using BodyTracker.Models;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BodyTracker.ViewModels
{
    // <summary>
    /// Serves as the primary logic controller for the main application view, managing body bodyMeasurement data and user interactions.
    /// </summary>
    public partial class DataBaseEntriesBodyMeasurementPageViewModel : ObservableObject
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





        #region Observiable Properties

        /// <summary>
        /// Gets or sets the collection of body bodyMeasurement data displayed in the UI.
        /// Uses <see cref="ObservableCollection{T}"/> to automatically notify the UI 
        /// of additions, removals, or list clears.
        /// </summary>
        [ObservableProperty] private ObservableCollection<FullBodyMeasurementDatasModel> bodyMeasurement = new();


        /// <summary>
        /// Gets or sets the collection of body initial bodyMeasurement data displayed in the UI.
        /// Uses <see cref="ObservableCollection{T}"/> to automatically notify the UI 
        /// of additions, removals, or list clears.
        /// </summary>
        [ObservableProperty] private ObservableCollection<FullBodyMeasurementDatasModel> initialBodyMeasurement = new();

        /// <summary>
        /// Gets or sets the collection of mean full body bodyMeasurement data.
        /// </summary>
        /// <remarks>The collection is observable, allowing UI elements or other components to react to
        /// changes such as additions or removals of bodyMeasurement data. This property is typically used for data binding
        /// scenarios.</remarks>
        [ObservableProperty] private ObservableCollection<FullBodyMeasurementDatasModel> meanMeasurement = new();

        /// <summary>
        /// Gets or sets the currently selected bodyMeasurement record from the list.
        /// Nullable, as no record may be selected.
        /// </summary>
        [ObservableProperty] private FullBodyMeasurementDatasModel? selectedMeasurement;

        /// <summary>
        /// Gets or sets the name of the person currently being viewed.
        /// Used for display purposes in headers or titles.
        /// </summary>
        [ObservableProperty] private string userName = string.Empty;

        /// <summary>
        /// Gets or sets the mean start date used for calculations or scheduling.
        /// </summary>
        [ObservableProperty] private DateTime meanStartDate = new DateTime(2025, 1, 1);

        /// <summary>
        /// Gets or sets the mean end date for the operation.
        /// </summary>
        [ObservableProperty] private DateTime meanEndDate = DateTime.Now;

        #endregion

        #region Relay Commans Async | Sync

       
        /// <summary>
        /// Gets the command responsible for refreshing the bodyMeasurement history from the database.
        /// Triggers an asynchronous reload of the <see cref="Measurement"/> collection.
        /// </summary>
        public IAsyncRelayCommand CommandBodyMeasurementRowEditEnding { get; }

        /// <summary>
        /// Gets the command that initiates the deletion of the currently selected bodyMeasurement.
        /// This operation removes the record from the database and updates the UI collection.
        /// </summary>
        /// <remarks>
        /// This command should typically check if <see cref="selectedMeasurement"/> is not null 
        /// before execution (via CanExecute logic).
        /// </remarks>
        public IAsyncRelayCommand CommandDeleteBodyDatabaseEntry { get; }

        #endregion

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
        /// Initializes a new instance of the <see cref="DataBaseEntriesBodyMeasurementPageViewModel"/> class.
        /// Sets up database access, initializes asynchronous commands, and retrieves 
        /// context information from the global application state.
        /// </summary>
        /// <param name="databaseService">
        /// An injected instance of the <see cref="DatabaseService"/> used for 
        /// historical data retrieval and record deletion.
        /// </param>
        /// <remarks>
        /// The constructor links commands to their respective asynchronous implementations 
        /// and ensures that the <see cref="CommandDeleteBodyDatabaseEntry"/> is governed by selection-based 
        /// execution logic (<see cref="CanDelete"/>).
        /// </remarks>
        public DataBaseEntriesBodyMeasurementPageViewModel(DatabaseService db)
        {
            databaseService = db;
            CommandDeleteBodyDatabaseEntry = new AsyncRelayCommand(DeleteSelectedAsync, CanDelete);
            CommandBodyMeasurementRowEditEnding = new AsyncRelayCommand<DataGridRowEditEndingEventArgs>(MeasurementsGrid_RowEditEnding);
            UserName = AppState.SelectedPersonName;
        }

        /// <summary>
        /// Triggers the initial asynchronous loading sequence for the ViewModel.
        /// </summary>
        /// <returns>A task that represents the initialization process.</returns>
        /// <remarks>
        /// This method acts as a wrapper for <see cref="ReloadAsync"/>. By isolating the 
        /// initial load in this method, the ViewModel remains compatible with common 
        /// asynchronous initialization patterns in WPF/MVVM architectures.
        /// </remarks>
        public async Task InitializeAsync()
        {
            await ReloadAsync();          
        }

        /// <summary>
        /// Synchronizes the local <see cref="Measurement"/> collection with the data stored in the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous reload operation.</returns>
        /// <remarks>
        /// The process involves three steps:
        /// <list type="number">
        /// <item><description>Clearing the existing local collection.</description></item>
        /// <item><description>Fetching all bodyMeasurement records for the currently selected person from the database.</description></item>
        /// <item><description>Populating the observable collection with the retrieved records to trigger UI updates.</description></item>
        /// </list>
        /// </remarks>
        private async Task ReloadAsync()
        {
            var pid = AppState.SelectedPersonId;
             
            await GetCurrentBodyMeaurements(pid);
            await GetInitialBodyMeaurements(pid);
          
          
        }


        /// <summary>
        /// Asynchronously retrieves the current body measurements for the specified person identifier from the database and updates the observable collection on the calling thread.
        /// </summary>
        /// <param name="pid">The unique person identifier used to query current measurements.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task GetCurrentBodyMeaurements(int pid)
        {
            var all = await databaseService.GetBodyMeasurementAsync(pid, databaseService.DatabaseCommands.CmdGetMeasurement());

            BodyMeasurement.Clear();
            foreach (var m in all)
            {
                BodyMeasurement.Add(m);
            }
            OnPropertyChanged(nameof(BodyMeasurement));
        }

        /// <summary>
        /// Asynchronously retrieves the initial body measurements for the specified person identifier from the database and updates the observable collection on the calling thread.
        /// </summary>
        /// <param name="pid">The unique person identifier used to query initial measurements.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task GetInitialBodyMeaurements(int pid)
        {
            var all = await databaseService.GetBodyMeasurementAsync(pid, databaseService.DatabaseCommands.CmdGetInitialMeasurement());

            InitialBodyMeasurement.Clear();
            foreach (var m in all)
            {
                InitialBodyMeasurement.Add(m);
            }
            OnPropertyChanged(nameof(InitialBodyMeasurement));
        }

        /// <summary>
        /// Evaluates whether the delete operation can currently be performed.
        /// </summary>
        /// <returns>
        /// <c>true</c> if a bodyMeasurement is selected in the UI (<see cref="SelectedMeasurement"/> is not null); 
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method is used as the predicate for the <see cref="CommandDeleteBodyDatabaseEntry"/>. 
        /// In WPF, the command's associated UI element (e.g., a Button) will be 
        /// automatically enabled or disabled based on this return value.
        /// </remarks>
        private bool CanDelete()
        {
           return SelectedMeasurement != null;
        }

        /// <summary>
        /// Executed automatically by the Source Generator whenever the <see cref="SelectedMeasurement"/> property changes.
        /// </summary>
        /// <param name="value">The new selected bodyMeasurement record (or null if deselected).</param>
        /// <remarks>
        /// This method ensures the UI remains responsive by forcing the <see cref="CommandDeleteBodyDatabaseEntry"/> 
        /// to re-evaluate its execution logic (<see cref="CanDelete"/>). 
        /// It utilizes a safe cast to <see cref="AsyncRelayCommand"/> to trigger the notification.
        /// </remarks>
        partial void OnSelectedMeasurementChanged(FullBodyMeasurementDatasModel? value)
        {
            (CommandDeleteBodyDatabaseEntry as AsyncRelayCommand)?.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Asynchronously deletes the currently selected bodyMeasurement record from the database.
        /// It removes entries from both physiological metrics and body dimensions tables if their respective IDs exist.
        /// </summary>
        /// <returns>A task representing the asynchronous deletion and subsequent UI refresh.</returns>
        /// <remarks>
        /// This method performs a sequential deletion based on the <see cref="SelectedMessung"/>. 
        /// After the database operation is complete, it triggers <see cref="ReloadAsync"/> to 
        /// synchronize the UI collection with the updated database state.
        /// </remarks>
        private async Task DeleteSelectedAsync()
        {
            try
            {
                var result = MessageBox.Show("Are you sure you want to delete the selected measurement?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;
                if (SelectedMeasurement == null) return;
                if (SelectedMeasurement.MetricID.HasValue) await databaseService.DeleteBodyMetricAsync(SelectedMeasurement.MetricID.Value);
                if (SelectedMeasurement.DemensionID.HasValue) await databaseService.DeleteBodyDimensionAsync(SelectedMeasurement.DemensionID.Value);
                await ReloadAsync();
            }
            catch(Exception ex)
            {
                GeneralErrorMessage = $"Delete Measurement error: {ex}";
            }
            
        }

        /// <summary>
        /// Asynchronously updates an existing bodyMeasurement record or inserts a new one into the database.
        /// This method acts as a wrapper for the data access layer, ensuring that all metric and 
        /// dimensional data is persisted before triggering a full UI refresh.
        /// </summary>
        /// <param name="personId">The unique identifier of the person to whom the measurements belong.</param>
        /// <param name="row">The view model containing the bodyMeasurement data to be synchronized.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// The "Upsert" logic (Update or Insert) is determined by the presence of existing IDs 
        /// within the <paramref name="row"/>. Post-execution, <see cref="ReloadAsync"/> is invoked 
        /// to ensure the local collection remains consistent with the database state, 
        /// including any server-generated identifiers.
        /// </remarks>
        public async Task UpdateRowMeasurementAsync(int personId, FullBodyMeasurementDatasModel row)
        {
            try
            {
                await databaseService.UpdateMeasurementAsync(
                    personId,
                    row.MetricID,
                    row.DemensionID,
                    row.MeasurementDate,
                    row.BodyWeight,
                    row.BMI = BodyCalculationToolsService.CalculateBmi((float)row.BodyWeight, (float)AppState.SelectedPersonHeight),
                    row.BodyFatPercentage,
                    row.BodyFatPercentageTop,
                    row.BodyFatPercentageBottom,
                    row.BodyMusclePercentage,
                    row.BodyMusclePercentageTop,
                    row.BodyMusclePercentageBottom,
                    row.BodyWaterPercentage,
                    row.BodyBoneMass,
                    row.BodyVisceralFat,
                    row.ChestCircumference,
                    row.WaistCircumference,
                    row.HipsCircumference,
                    row.FatTongBreastCrease,
                    row.FatTongArmpitCrease,
                    row.FatTongAbdominalCrease,
                    row.FatTongHipCrease,
                    row.FatTongThighCrease,
                    row.FatTongBackCrease,
                    row.FatTongTricepsCrease
                );

                await ReloadAsync();
            }
            catch(Exception ex)
            {
                GeneralErrorMessage = $"Error updating measurement: {ex.Message}";
            }
        }


        /// <summary>
        /// Handles the <see cref="DataGrid.RowEditEnding"/> event to persist modified bodyMeasurement data to the database.
        /// This method ensures that only committed changes are processed and utilizes the Dispatcher 
        /// to decouple the database update from the UI validation cycle.
        /// </summary>
        /// <param name="sender">The source of the event, specifically the <see cref="DataGrid"/>.</param>
        /// <param name="e">Event data containing the edit action and the row being edited.</param>
        private async Task MeasurementsGrid_RowEditEnding(DataGridRowEditEndingEventArgs e)
        {
            if(e == null) return;

            if (e.EditAction != DataGridEditAction.Commit) return;

            if (e.Row.Item is not FullBodyMeasurementDatasModel editedRow) return;

            int personId = AppState.SelectedPersonId;
            await UpdateRowMeasurementAsync(personId, editedRow);
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
