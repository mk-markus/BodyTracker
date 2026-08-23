using BodyTracker.Services;
using BodyTracker.ViewModels;
using System;
using System.Diagnostics;
using System.Windows.Controls;


namespace BodyTracker.Views.Pages
{
    /// <summary>
    /// Interaktionslogik für StartPage.xaml
    /// </summary>
    public partial class BodyTrackerView : Page
    {

        /// <summary>
        /// Reference to the main application window, acting as the primary host (Shell) 
        /// for navigation, status updates, and top-level UI orchestration.
        /// </summary>
        private readonly MainWindow mainWindow;

        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the ViewModel.
        /// </summary>
        private readonly DatabaseService databaseService;

         /// <summary>
        /// Represents the read-only view model instance that handles the business logic, commands, and data binding for the start page.
        /// </summary>
        private readonly BodyTrackerViewModel bodyTrackerVM;

        /// <summary>
        /// Initializes a new instance of the StartPage class, configuring component UI elements, establishing window and database references, and binding the start page view model.
        /// </summary>
        /// <param name="shell">The main window instance hosting the application navigation and layout.</param>
        /// <param name="db">The database service instance responsible for data access operations.</param>
        public BodyTrackerView(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();
            mainWindow = shell;
            databaseService = db;
            mainWindow.DataContext = this;

            bodyTrackerVM = new BodyTrackerViewModel(mainWindow, databaseService);

            DataContext = bodyTrackerVM;

            Unloaded += (s, e) => bodyTrackerVM.Dispose();
        }

        #region Disposal Pattern

        // <summary>
        /// Tracks whether the object has been disposed to prevent double disposal.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Releases all resources used by the <see cref="LauncherDashboardViewModel"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="LauncherDashboardViewModel"/> class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                Debug.WriteLine($"{this.GetType().Name} Disposing managed resources {GetHashCode()}");

                try
                {
                    databaseService?.Dispose();
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
