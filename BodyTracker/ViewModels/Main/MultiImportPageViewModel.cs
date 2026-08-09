using BodyTracker.Services;
using BodyTracker.Views.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels
{
    public partial class MultiImportPageViewModel : ObservableObject
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
        public IAsyncRelayCommand CommandShowStepDailyTrendImportPage { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the food intake import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the nutritional and food intake logging import interface.</remarks>
        public IAsyncRelayCommand CommandShowFoodIntakeImportPage { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the heavy app workout data import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the heavy workout tracking data import interface.</remarks>
        public IAsyncRelayCommand CommandShowHeavyAppData { get; }

        /// <summary>
        /// A cached instance of the step daily trend import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private StepDailyTrendImportPage stepDailyTrendImportPage;

        /// <summary>
        /// A cached instance of the food intake import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private FoodIntakeImportPage foodIntakeImportPage;

        /// <summary>
        /// A cached instance of the heavy app data import page.
        /// </summary>
        /// <remarks>Maintains a single instance in memory to preserve state and avoid redundant instantiation overhead during navigation.</remarks>
        private HeavyAppImportPage heavyAppImportPage;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiImportPageViewModel"/> class with the specified main window shell and database service.
        /// </summary>
        /// <remarks>Assigns window and database dependencies, initializes import navigation commands, and loads the default food intake import view upon startup.</remarks>
        /// <param name="shell">The reference to the primary application window instance.</param>
        /// <param name="db">The database service instance used for data import and persistence operations.</param>
        public MultiImportPageViewModel(MainWindow shell, DatabaseService db)
        {


            mainWindow = shell;
            databaseService = db;

            CommandShowStepDailyTrendImportPage = new AsyncRelayCommand(ShowStepDailyTrendImportPage);

            CommandShowFoodIntakeImportPage = new AsyncRelayCommand(ShowFoodIntakeImportPage);

            CommandShowHeavyAppData = new AsyncRelayCommand(ShowHeavyAppDatasAsync);

            _ = ShowFoodIntakeImportPage();

        }

        /// <summary>
        /// Asynchronously navigates to the step daily trend import view.
        /// </summary>
        /// <remarks>Instantiates the step daily trend import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowStepDailyTrendImportPage()
        {
            if (stepDailyTrendImportPage == null) stepDailyTrendImportPage = new StepDailyTrendImportPage(mainWindow, databaseService);


            CurrentPage = stepDailyTrendImportPage;
        }

        /// <summary>
        /// Asynchronously navigates to the food intake import view.
        /// </summary>
        /// <remarks>Instantiates the food intake import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowFoodIntakeImportPage()
        {
            if (foodIntakeImportPage == null) foodIntakeImportPage = new FoodIntakeImportPage(mainWindow, databaseService);
            CurrentPage = foodIntakeImportPage;
        }

        /// <summary>
        /// Asynchronously navigates to the heavy app workout data import view.
        /// </summary>
        /// <remarks>Instantiates the heavy app import page lazily if not already cached, and sets it as the active content page.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowHeavyAppDatasAsync()
        {

            if (heavyAppImportPage == null) heavyAppImportPage = new HeavyAppImportPage(mainWindow, databaseService);
            CurrentPage = heavyAppImportPage;

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
