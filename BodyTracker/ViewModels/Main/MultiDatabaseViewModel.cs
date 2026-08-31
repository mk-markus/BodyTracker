using BodyTracker.Services;
using BodyTracker.Views;
using BodyTracker.Views.Pages;
using BodyTracker.Views.Pages.Main;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels.Main
{
    public partial class MultiDatabaseViewModel : ObservableObject
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
        /// Gets or sets the currently active view or page displayed within the import navigation container.
        /// </summary>
        /// <remarks>Bound to the UI content presenter to dynamically switch between different import sub-pages.</remarks>
        [ObservableProperty] private object? currentPage;

        /// <summary>
        /// Gets the command responsible for navigating to the step daily trend import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the step tracking and daily trend data import interface.</remarks>
        public IAsyncRelayCommand CommandShowBodyMeasurementEntryView { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the step daily trend import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the step tracking and daily trend data import interface.</remarks>
        public IAsyncRelayCommand CommandShowStepTrendEntryView { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the food intake import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the nutritional and food intake logging import interface.</remarks>
        public IAsyncRelayCommand CommandShowFoodIntakeEntryView { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the heavy app workout data import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the heavy workout tracking data import interface.</remarks>
        public IAsyncRelayCommand CommandShowHeavyAppEntryView { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the exercise data import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the heavy workout tracking data import interface.</remarks>
        public IAsyncRelayCommand CommandShowExerciseEntryView { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the heart rate data import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the heavy workout tracking data import interface.</remarks>
        public IAsyncRelayCommand CommandShowHeartRateEntryView { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the oxygen saturation data import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the heavy workout tracking data import interface.</remarks>
        public IAsyncRelayCommand CommandOxygenSaturationEntryView { get; }
       
        /// <summary>
        /// A cached instance of the body measurements import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private DBEntryBodyMeasurementView dBEntryBodyMeasurementView;

        /// <summary>
        /// A cached instance of the step daily trend import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private DBEntryStepTrendView dBEntryStepTrendView;

        /// <summary>
        /// A cached instance of the food intake import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private DBEntryFoodIntakeView dBEntryFoodIntakeView;

        /// <summary>
        /// A cached instance of the heavy app data import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private DBEntryWorkoutLogView dBEntryWorkoutLogView;

        /// <summary>
        /// A cached instance of the exercise data import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private DBEntryExerciseView dBEntryExerciseView;

        /// <summary>
        /// A cached instance of the heart rate data import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private DBEntryHeartRateView dBEntryHeartRateView;

        /// <summary>
        /// A cached instance of the heart rate data import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private DBEntryOxygenSaturationView dBEntryOxygenSaturationView;


        /// <summary>
        /// Default Searching Path for the import files
        /// </summary>
        private string searchPath = "\\\\192.168.178.9\\Handy\\Documents\\BodyTracker_Interface";

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiImportViewModel"/> class with the specified main window shell and database service.
        /// </summary>
        /// <remarks>Assigns window and database dependencies, initializes import navigation commands, and loads the default food intake import view upon startup.</remarks>
        /// <param name="shell">The reference to the primary application window instance.</param>
        /// <param name="db">The database service instance used for data import and persistence operations.</param>
        public MultiDatabaseViewModel(DatabaseService db)
        {
            databaseService = db;

            CommandShowBodyMeasurementEntryView = new AsyncRelayCommand(ShowDBEntryBodyMeasurementViewAsync);

            CommandShowStepTrendEntryView = new AsyncRelayCommand(ShowDBEntryStepTrendViewAsync);

            CommandShowFoodIntakeEntryView = new AsyncRelayCommand(ShowDBEntryFoodIntakeViewAsync);

            CommandShowHeavyAppEntryView = new AsyncRelayCommand(ShowDBEntryHeavyAppViewAsync);

            CommandShowHeartRateEntryView = new AsyncRelayCommand(ShowDBEntryHeartRateViewAsync);

            CommandShowExerciseEntryView = new AsyncRelayCommand(ShowDBEntryExerciseViewAsync);

            CommandOxygenSaturationEntryView = new AsyncRelayCommand(ShowDBEntryOxygenSaturationViewAsync);

            _ = ShowDBEntryBodyMeasurementViewAsync();

        }

        /// <summary>
        /// Asynchronously navigates to the step daily trend import view.
        /// </summary>
        /// <remarks>Instantiates the step daily trend import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowDBEntryBodyMeasurementViewAsync()
        {
            if (dBEntryBodyMeasurementView == null) dBEntryBodyMeasurementView = new DBEntryBodyMeasurementView(databaseService);


            CurrentPage = dBEntryBodyMeasurementView;
        }

        /// <summary>
        /// Asynchronously navigates to the step daily trend import view.
        /// </summary>
        /// <remarks>Instantiates the step daily trend import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowDBEntryStepTrendViewAsync()
        {
            if (dBEntryStepTrendView == null) dBEntryStepTrendView = new DBEntryStepTrendView(databaseService);


            CurrentPage = dBEntryStepTrendView;
        }

        /// <summary>
        /// Asynchronously navigates to the food intake import view.
        /// </summary>
        /// <remarks>Instantiates the food intake import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowDBEntryFoodIntakeViewAsync()
        {
            if (dBEntryFoodIntakeView == null) dBEntryFoodIntakeView = new DBEntryFoodIntakeView(databaseService);
            CurrentPage = dBEntryFoodIntakeView;
        }

        /// <summary>
        /// Asynchronously navigates to the exercise import view.
        /// </summary>
        /// <remarks>Instantiates the food intake import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowDBEntryExerciseViewAsync()
        {
            if (dBEntryExerciseView == null) dBEntryExerciseView = new DBEntryExerciseView(databaseService);
            CurrentPage = dBEntryExerciseView;
        }

        /// <summary>
        /// Asynchronously navigates to the heart rate import view.
        /// </summary>
        /// <remarks>Instantiates the food intake import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowDBEntryHeartRateViewAsync()
        {
            if (dBEntryHeartRateView == null) dBEntryHeartRateView = new DBEntryHeartRateView(databaseService);
            CurrentPage = dBEntryHeartRateView;
        }

        /// <summary>
        /// Asynchronously navigates to the heart rate import view.
        /// </summary>
        /// <remarks>Instantiates the food intake import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowDBEntryOxygenSaturationViewAsync()
        {
            if (dBEntryOxygenSaturationView == null) dBEntryOxygenSaturationView = new DBEntryOxygenSaturationView(databaseService);
            CurrentPage = dBEntryOxygenSaturationView;
        }

        /// <summary>
        /// Asynchronously navigates to the heavy app workout data import view.
        /// </summary>
        /// <remarks>Instantiates the heavy app import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowDBEntryHeavyAppViewAsync()
        {

            if (dBEntryWorkoutLogView == null) dBEntryWorkoutLogView = new DBEntryWorkoutLogView(databaseService);
            CurrentPage = dBEntryWorkoutLogView;

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
