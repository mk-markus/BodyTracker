using BodyTracker.MVVM.Models;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;

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
        [ObservableProperty] private ObservableCollection<FullBodyMeasurementDatas> measurement = new();

        /// <summary>
        /// Gets or sets the collection of mean full body measurement data.
        /// </summary>
        /// <remarks>The collection is observable, allowing UI elements or other components to react to
        /// changes such as additions or removals of measurement data. This property is typically used for data binding
        /// scenarios.</remarks>
        [ObservableProperty] private ObservableCollection<FullBodyMeasurementDatas> meanMeasurement = new();

        /// <summary>
        /// Gets or sets the currently selected measurement record from the list.
        /// Nullable, as no record may be selected.
        /// </summary>
        [ObservableProperty] private FullBodyMeasurementDatas? selectedMeasurement;

        /// <summary>
        /// Gets or sets the name of the person currently being viewed.
        /// Used for display purposes in headers or titles.
        /// </summary>
        [ObservableProperty] private string userName = string.Empty;

        /// <summary>
        /// Gets or sets the mean start date used for calculations or scheduling.
        /// </summary>
        [ObservableProperty] private DateTime meanStartDate = new DateTime(2025,1,1);

        /// <summary>
        /// Gets or sets the mean end date for the operation.
        /// </summary>
        [ObservableProperty] private DateTime meanEndDate = DateTime.Now;

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
            MeanMeasurement = GetAverageValues(MeanStartDate, MeanEndDate);
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
            var pid = AppState.SelectedPersonId;
            var all = await databaseServerice.GetBodyMeasurementAsync(pid);

            Measurement.Clear();
            foreach (var m in all)
            {
                Measurement.Add(m);
            }

            OnPropertyChanged(nameof(Measurement));
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
        partial void OnSelectedMeasurementChanged(FullBodyMeasurementDatas? value)
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
        
        /// <summary>
        /// Asynchronously updates an existing measurement record or inserts a new one into the database.
        /// This method acts as a wrapper for the data access layer, ensuring that all metric and 
        /// dimensional data is persisted before triggering a full UI refresh.
        /// </summary>
        /// <param name="personId">The unique identifier of the person to whom the measurements belong.</param>
        /// <param name="row">The view model containing the measurement data to be synchronized.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// The "Upsert" logic (Update or Insert) is determined by the presence of existing IDs 
        /// within the <paramref name="row"/>. Post-execution, <see cref="ReloadAsync"/> is invoked 
        /// to ensure the local collection remains consistent with the database state, 
        /// including any server-generated identifiers.
        /// </remarks>
        public async Task UpdateMeasurementAsync(int personId, FullBodyMeasurementDatas row)
        {
            await databaseServerice.UpdateMeasurementAsync(
                personId,
                row.MetricID, 
                row.DemensionID,
                row.MeasurementDate,
                row.BodyWeight, 
                row.BMI,
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
                row.FatTong
            );

            await ReloadAsync();
        }


        /// <summary>
        /// Executed automatically by the source generator when the <see cref="StartDate"/> property changes.
        /// Initiates an asynchronous refresh of the chart data to reflect the newly selected time range.
        /// </summary>
        /// <param name="value">The new <see cref="DateTime"/> value assigned to the start date filter.</param>
        /// <remarks>
        /// This method uses a "fire-and-forget" pattern (<c>_ = ...</c>) because partial methods 
        /// generated by the toolkit are synchronous by design. The actual data retrieval and 
        /// UI update are handled within the asynchronous <see cref="RefreshChartAsync"/> method 
        /// to maintain UI responsiveness.
        /// </remarks>
        partial void OnMeanStartDateChanged(DateTime value)
        {
            MeanMeasurement = GetAverageValues(MeanStartDate, MeanEndDate);
        }

        /// <summary>
        /// Executed automatically by the source generator when the <see cref="EndDate"/> property changes.
        /// Triggers an asynchronous update of the chart data to reflect the newly defined end of the observation period.
        /// </summary>
        /// <param name="value">The new <see cref="DateTime"/> value assigned to the end date filter.</param>
        /// <remarks>
        /// This partial method acts as an event hook provided by the CommunityToolkit.Mvvm. 
        /// Using the discard pattern (<c>_ = ...</c>) allows the UI to remain responsive by 
        /// launching the <see cref="RefreshChartAsync"/> task without blocking the property setter's execution thread.
        /// </remarks>
        partial void OnMeanEndDateChanged(DateTime value)
        {
            GetAverageValues(MeanStartDate, MeanEndDate);
        }

        private ObservableCollection<FullBodyMeasurementDatas> GetAverageValues(DateTime start, DateTime end)
        {
            // Basic validation: Ensure date range is valid and data source exists
            if (start > end || Measurement == null)
                return new ObservableCollection<FullBodyMeasurementDatas>();

            // Filter data by the specified date range
            var ordered = Measurement
                .Where(d => d.MeasurementDate >= start && d.MeasurementDate <= end)
                .ToList();

            if (ordered.Count == 0)
                return new ObservableCollection<FullBodyMeasurementDatas>();

            var result = new FullBodyMeasurementDatas();

            /* 
               LOCAL HELPER FUNCTION: GetFilteredAverage
               Logic: Filters out values that are null or <= 0.
               The average is only calculated based on existing, positive data points.
            */
            float GetFilteredAverage(IEnumerable<float?> source)
            {
                // Only include values that have a value and are greater than 0
                var validValues = source.Where(v => v.HasValue && v.Value > 0).ToList();

                // Return average if data exists, otherwise return 0 to avoid DivisionByZero
                return validValues.Any() ? validValues.Average(v => v.Value) : 0f;
            }

            // Applying the filtered average logic to each measurement field
            result.BodyWeight = GetFilteredAverage(ordered.Select(x => x.BodyWeight));
            result.BMI = GetFilteredAverage(ordered.Select(x => x.BMI));
            result.BodyFatPercentage = GetFilteredAverage(ordered.Select(x => x.BodyFatPercentage));
            result.BodyMusclePercentage = GetFilteredAverage(ordered.Select(x => x.BodyMusclePercentage));
            result.BodyWaterPercentage = GetFilteredAverage(ordered.Select(x => x.BodyWaterPercentage));
            result.ChestCircumference = GetFilteredAverage(ordered.Select(x => x.ChestCircumference));
            result.WaistCircumference = GetFilteredAverage(ordered.Select(x => x.WaistCircumference));
            result.HipsCircumference = GetFilteredAverage(ordered.Select(x => x.HipsCircumference));
            result.FatTong = GetFilteredAverage(ordered.Select(x => x.FatTong));

            return new ObservableCollection<FullBodyMeasurementDatas> { result };
        }
    }
}
