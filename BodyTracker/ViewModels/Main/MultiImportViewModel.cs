using BodyTracker.Services;
using BodyTracker.ViewModels.Main;
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
    public partial class MultiImportViewModel : ObservableObject
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
        /// Gets or sets the currently active view or page displayed within the import navigation container.
        /// </summary>
        /// <remarks>Bound to the UI content presenter to dynamically switch between different import sub-pages.</remarks>
        [ObservableProperty] private object? currentPage;

        /// <summary>
        /// Gets the command responsible for navigating to the step daily trend import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the step tracking and daily trend data import interface.</remarks>
        public IAsyncRelayCommand CommandShowStepTrendImportView { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the food intake import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the nutritional and food intake logging import interface.</remarks>
        public IAsyncRelayCommand CommandShowFoodIntakeImportView { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the food info import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the nutritional and food info logging import interface.</remarks>
        public IAsyncRelayCommand CommandShowFoodInfoImportView { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the Hevy app workout data import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the Hevy workout tracking data import interface.</remarks>
        public IAsyncRelayCommand CommandShowHevyAppImportView { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the exercise data import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the Hevy workout tracking data import interface.</remarks>
        public IAsyncRelayCommand CommandShowExerciseImportView { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the heart rate data import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the Hevy workout tracking data import interface.</remarks>
        public IAsyncRelayCommand CommandShowHeartRateImportView { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the oxygen saturation data import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the Hevy workout tracking data import interface.</remarks>
        public IAsyncRelayCommand CommandOxygenSaturationImportView { get; }

        /// <summary>
        /// A cached instance of the step daily trend import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private StepTrendImportView stepDailyTrendImportView;

        /// <summary>
        /// A cached instance of the food intake import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private FoodIntakeImportView foodIntakeImportView;
        
        /// <summary>
        /// A cached instance of the food intake import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private FoodInfoImportView foodInfoImportView;

        /// <summary>
        /// A cached instance of the Hevy app data import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private WorkoutImportView HevyAppImportView;

        /// <summary>
        /// A cached instance of the exercise data import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private ExerciseImportView exerciseImportView;

        /// <summary>
        /// A cached instance of the heart rate data import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private HeartRateImportView heartRateImportView;

        /// <summary>
        /// A cached instance of the heart rate data import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private OxygenSaturationView oxygenSaturationView;


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
        public MultiImportViewModel(MainWindow shell, DatabaseService db)
        {


            mainWindow = shell;
            databaseService = db;

            CommandShowStepTrendImportView = new AsyncRelayCommand(ShowStepDailyTrendImportPage);

            CommandShowFoodIntakeImportView = new AsyncRelayCommand(ShowFoodIntakeImportPage);

            CommandShowFoodInfoImportView = new AsyncRelayCommand(ShowFoodInfoImportPage);

            CommandShowHevyAppImportView = new AsyncRelayCommand(ShowHevyAppDatasAsync);

            CommandShowHeartRateImportView = new AsyncRelayCommand(ShowHeartRateImportView);

            CommandShowExerciseImportView = new AsyncRelayCommand(ShowExerciseImportPage);

            CommandOxygenSaturationImportView = new AsyncRelayCommand(ShowOxygenSaturationImportView);

            _ = ShowFoodIntakeImportPage();

        }

        /// <summary>
        /// Asynchronously navigates to the step daily trend import view.
        /// </summary>
        /// <remarks>Instantiates the step daily trend import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowStepDailyTrendImportPage()
        {
            if (stepDailyTrendImportView == null) stepDailyTrendImportView = new StepTrendImportView(databaseService, searchPath);


            CurrentPage = stepDailyTrendImportView;
        }

        /// <summary>
        /// Asynchronously navigates to the food intake import view.
        /// </summary>
        /// <remarks>Instantiates the food intake import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowFoodIntakeImportPage()
        {
            if (foodIntakeImportView == null) foodIntakeImportView = new FoodIntakeImportView(databaseService, searchPath);
            CurrentPage = foodIntakeImportView;
        }

        /// <summary>
        /// Asynchronously navigates to the food intake import view.
        /// </summary>
        /// <remarks>Instantiates the food intake import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowFoodInfoImportPage()
        {
            if (foodInfoImportView == null) foodInfoImportView = new FoodInfoImportView(databaseService, searchPath);
            CurrentPage = foodInfoImportView;
        }

        /// <summary>
        /// Asynchronously navigates to the exercise import view.
        /// </summary>
        /// <remarks>Instantiates the food intake import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowExerciseImportPage()
        {
            if (exerciseImportView == null) exerciseImportView = new ExerciseImportView(databaseService, searchPath);
            CurrentPage = exerciseImportView;
        }

        /// <summary>
        /// Asynchronously navigates to the heart rate import view.
        /// </summary>
        /// <remarks>Instantiates the food intake import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowHeartRateImportView()
        {
            if (heartRateImportView == null) heartRateImportView = new HeartRateImportView(databaseService, searchPath);
            CurrentPage = heartRateImportView;
        }

        /// <summary>
        /// Asynchronously navigates to the heart rate import view.
        /// </summary>
        /// <remarks>Instantiates the food intake import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowOxygenSaturationImportView()
        {
            if (oxygenSaturationView == null) oxygenSaturationView = new OxygenSaturationView(databaseService, searchPath);
            CurrentPage = oxygenSaturationView;
        }

        /// <summary>
        /// Asynchronously navigates to the Hevy app workout data import view.
        /// </summary>
        /// <remarks>Instantiates the Hevy app import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowHevyAppDatasAsync()
        {

            if (HevyAppImportView == null) HevyAppImportView = new WorkoutImportView(databaseService, searchPath);
            CurrentPage = HevyAppImportView;

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
