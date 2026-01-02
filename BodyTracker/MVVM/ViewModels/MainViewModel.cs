using BodyTracker.MVVM.Models;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels
{
    // <summary>
    /// Serves as the primary logic controller for the main application view, managing body measurement data and user interactions.
    /// </summary>
    public partial class MeasurementViewModel : ObservableObject
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
        /// Gets or sets the collection of body measurement data displayed in the UI.
        /// Uses <see cref="ObservableCollection{T}"/> to automatically notify the UI 
        /// of additions, removals, or list clears.
        /// </summary>
        [ObservableProperty] private ObservableCollection<FullBodyMeasurementDatasViewModel> measurement = new();

        /// <summary>
        /// Gets or sets the currently selected measurement record from the list.
        /// Nullable, as no record may be selected.
        /// </summary>
        [ObservableProperty] private FullBodyMeasurementDatasViewModel? selectedMeasurement;

        /// <summary>
        /// Gets or sets the name of the person currently being viewed.
        /// Used for display purposes in headers or titles.
        /// </summary>
        [ObservableProperty] private string userName = string.Empty;

        /// <summary>
        /// Gets the command responsible for refreshing the measurement history from the database.
        /// Triggers an asynchronous reload of the <see cref="Measurement"/> collection.
        /// </summary>
        public IAsyncRelayCommand ReloadCommand { get; }

        /// <summary>
        /// Gets the command that initiates the deletion of the currently selected measurement.
        /// This operation removes the record from the database and updates the UI collection.
        /// </summary>
        /// <remarks>
        /// This command should typically check if <see cref="selectedMeasurement"/> is not null 
        /// before execution (via CanExecute logic).
        /// </remarks>
        public IAsyncRelayCommand DeleteCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementViewModel"/> class.
        /// Sets up database access, initializes asynchronous commands, and retrieves 
        /// context information from the global application state.
        /// </summary>
        /// <param name="databaseService">
        /// An injected instance of the <see cref="DatabaseService"/> used for 
        /// historical data retrieval and record deletion.
        /// </param>
        /// <remarks>
        /// The constructor links commands to their respective asynchronous implementations 
        /// and ensures that the <see cref="DeleteCommand"/> is governed by selection-based 
        /// execution logic (<see cref="CanDelete"/>).
        /// </remarks>
        public MeasurementViewModel(DatabaseService db)
        {
            databaseServerice = db;
            ReloadCommand = new AsyncRelayCommand(ReloadAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteSelectedAsync, CanDelete);
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
        /// <item><description>Fetching all measurement records for the currently selected person from the database.</description></item>
        /// <item><description>Populating the observable collection with the retrieved records to trigger UI updates.</description></item>
        /// </list>
        /// </remarks>
        private async Task ReloadAsync()
        {
            Measurement.Clear();
            var pid = AppState.SelectedPersonId;
            var all = await databaseServerice.GetBodyMeasurementAsync(pid);
            foreach (var m in all) Measurement.Add(m);
        }

        /// <summary>
        /// Evaluates whether the delete operation can currently be performed.
        /// </summary>
        /// <returns>
        /// <c>true</c> if a measurement is selected in the UI (<see cref="SelectedMeasurement"/> is not null); 
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method is used as the predicate for the <see cref="DeleteCommand"/>. 
        /// In WPF, the command's associated UI element (e.g., a Button) will be 
        /// automatically enabled or disabled based on this return value.
        /// </remarks>
        private bool CanDelete()
        {
            // Die Löschfunktion ist nur verfügbar, wenn eine Auswahl getroffen wurde.
            return SelectedMeasurement != null;
        }

        /// <summary>
        /// Executed automatically by the Source Generator whenever the <see cref="SelectedMeasurement"/> property changes.
        /// </summary>
        /// <param name="value">The new selected measurement record (or null if deselected).</param>
        /// <remarks>
        /// This method ensures the UI remains responsive by forcing the <see cref="DeleteCommand"/> 
        /// to re-evaluate its execution logic (<see cref="CanDelete"/>). 
        /// It utilizes a safe cast to <see cref="AsyncRelayCommand"/> to trigger the notification.
        /// </remarks>
        partial void OnSelectedMeasurementChanged(FullBodyMeasurementDatasViewModel? value)
        {
            (DeleteCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Asynchronously deletes the currently selected measurement record from the database.
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
            if (SelectedMeasurement == null) return;
            if (SelectedMeasurement.MetricID.HasValue) await databaseServerice.DeleteBodyMetricAsync(SelectedMeasurement.MetricID.Value);
            if (SelectedMeasurement.DemensionID.HasValue) await databaseServerice.DeleteBodyDimensionAsync(SelectedMeasurement.DemensionID.Value);
            await ReloadAsync();
        }


        public async Task UpsertMeasurementAsync(int personId, FullBodyMeasurementDatasViewModel row)
        {
            await databaseServerice.UpsertMeasurementAsync(
                personId,
                row.MetricID, row.DemensionID,
                row.MeasurementDate,
                row.BodyWeight, row.BMI,
                row.BodyFatPercentage,
                row.BodyMusclePercentage,
                row.BodyVisceralFat,
                row.ChestCircumference,
                row.WaistCircumference,
                row.HipsCircumference,
                row.FatTong
            );
        }
    }
}
