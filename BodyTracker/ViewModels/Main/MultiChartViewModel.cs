using BodyTracker.Services;
using BodyTracker.ViewModels.Main;
using BodyTracker.Views;
using BodyTracker.Views.Pages;
using BodyTracker.Views.Pages.Main;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels
{
    public partial class MultiChartViewModel : ObservableObject
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
        /// A private, read-only reference to the primary application window instance.
        /// </summary>
        /// <remarks>Provides direct access to window operations, navigation elements, or parent UI contexts from dependent components.</remarks>
        private readonly MainWindow mainWindow;

        /// <summary>
        /// Gets or sets the currently active view or page displayed within the multi-chart navigation container.
        /// </summary>
        /// <remarks>Bound to the UI content presenter to dynamically switch between different analytical chart sub-pages.</remarks>
        [ObservableProperty] private object? currentPage;

        /// <summary>
        /// Gets the command responsible for navigating to the body measurements charts view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display analytical charts of logged body measurements.</remarks>
        public IAsyncRelayCommand CommandShowMeasurements { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the daily step trend charts view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display analytical charts of daily step trends and activity.</remarks>
        public IAsyncRelayCommand CommandShowDailyStep { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the personal workout insights charts view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display analytical charts of workout volume and exercise distribution.</remarks>
        public IAsyncRelayCommand CommandShowWorkouts { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the personal workout insights charts view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display analytical charts of workout volume and exercise distribution.</remarks>
        public IAsyncRelayCommand CommandShowExercise { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the personal workout insights charts view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display analytical charts of workout volume and exercise distribution.</remarks>
        public IAsyncRelayCommand CommandShowHeartRate { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the personal workout insights charts view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display analytical charts of workout volume and exercise distribution.</remarks>
        public IAsyncRelayCommand CommandShowSpO2 { get; }

        /// <summary>
        /// 
        /// </summary>
        private  ChartsMeasurementsView chartMeasurementView;

        /// <summary>
        /// 
        /// </summary>
        private  ChartsStepDailyTrendView chartStepTrendView;

        private ExerciseChartView exerciseChartView;

        /// <summary>
        /// 
        /// </summary>
        private HeartRateChartView heartRateChartView;

        /// <summary>
        /// 
        /// </summary>
        private OxygenSaturationChartView oxygenSaturationChartView;


        /// <summary>
        /// Initializes a new instance of the <see cref="MultiChartViewModel"/> class with the specified main window shell and database service.
        /// </summary>
        /// <remarks>Assigns window and database dependencies, initializes chart navigation commands, and loads the default body measurements chart view upon startup.</remarks>
        /// <param name="shell">The reference to the primary application window instance.</param>
        /// <param name="db">The database service instance used for analytical data retrieval operations.</param>
        public MultiChartViewModel(MainWindow shell, DatabaseService db)
        {
            mainWindow = shell;
            databaseService = db;

            CommandShowMeasurements = new AsyncRelayCommand(ShowMeasurementsAsync);

            CommandShowWorkouts = new AsyncRelayCommand(ShowWorkoutsPage);

            CommandShowDailyStep = new AsyncRelayCommand(ShowDailyStepsAsync);
            CommandShowExercise = new AsyncRelayCommand(ShowExerciseAsync);
            CommandShowHeartRate = new AsyncRelayCommand(ShowHeartRateAsync);
            CommandShowSpO2 = new AsyncRelayCommand(ShowSpO2Async);

            _ = ShowMeasurementsAsync();
        }


        /// <summary>
        /// Asynchronously navigates to the body measurements charts view.
        /// </summary>
        /// <remarks>Instantiates the charts measurements page and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowMeasurementsAsync()
        {
            if (chartMeasurementView == null) chartMeasurementView = new ChartsMeasurementsView(databaseService);
            CurrentPage = chartMeasurementView;
        }


        /// <summary>
        /// Asynchronously navigates to the daily step trend charts view.
        /// </summary>
        /// <remarks>Instantiates the daily step trend charts page and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowDailyStepsAsync()
        {
            if (chartStepTrendView == null) chartStepTrendView = new ChartsStepDailyTrendView(databaseService);
            CurrentPage = chartStepTrendView;

        }


        /// <summary>
        /// Asynchronously navigates to the daily step trend charts view.
        /// </summary>
        /// <remarks>Instantiates the daily step trend charts page and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowExerciseAsync()
        {
            if (exerciseChartView == null) exerciseChartView = new ExerciseChartView(databaseService);
            CurrentPage = exerciseChartView;

        }


        /// <summary>
        /// Asynchronously navigates to the daily step trend charts view.
        /// </summary>
        /// <remarks>Instantiates the daily step trend charts page and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowHeartRateAsync()
        {
            if (heartRateChartView == null) heartRateChartView = new HeartRateChartView(databaseService);
            CurrentPage = heartRateChartView;

        }

        /// <summary>
        /// Asynchronously navigates to the daily step trend charts view.
        /// </summary>
        /// <remarks>Instantiates the daily step trend charts page and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowSpO2Async()
        {
            if (oxygenSaturationChartView == null) oxygenSaturationChartView = new OxygenSaturationChartView(databaseService);
            CurrentPage = oxygenSaturationChartView;

        }
        /// <summary>
        /// Asynchronously navigates to the personal workout insights charts view.
        /// </summary>
        /// <remarks>Instantiates the personal workout insights page and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowWorkoutsPage()
        {
            CurrentPage = new PersonalWorkoutInsightsPage(mainWindow, databaseService);

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