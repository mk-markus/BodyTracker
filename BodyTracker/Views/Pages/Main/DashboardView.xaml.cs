using BodyTracker.Services;
using BodyTracker.ViewModels;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages
{
    /// <summary>
    /// Interaktionslogik für DashboardView.xaml
    /// </summary>
    public partial class DashboardView : Page
    {

        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service provides the low-level infrastructure for all SQL Server interactions, 
        /// including CRUD operations for body metrics and dimensions.
        /// </summary>
        /// <remarks>
        /// By maintaining this reference at the page level, the component can facilitate 
        /// dependency injection and ensure consistent data access across all sub-routines.
        /// </remarks>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// A private, read-only reference to the <see cref="MainWindow"/>, acting as the application's "Shell".
        /// This reference provides the page with access to top-level UI orchestration, 
        /// navigation controls, and global application state management.
        /// </summary>
        /// <remarks>
        /// Following the Shell pattern, this field allows the current page to interact with 
        /// the main window's container, for example, to trigger navigation or update global status bars.
        /// </remarks>
        private readonly MainWindow mainWindow;

        /// <summary>
        /// A private, read-only reference to the <see cref="DBEntryBodyMeasurementView"/>.
        /// This instance serves as the primary data context for the page, orchestrating 
        /// the business logic, data retrieval, and command execution for body measurements.
        /// </summary>
        /// <remarks>
        /// The ViewModel is instantiated during the page construction to ensure that 
        /// data binding is established before the UI is rendered.
        /// </remarks>
        private readonly DashboardPageViewModel dashBoardViewModel;

        /// <summary>
        /// Initializes a new instance of the DashboardView class, configuring component UI elements, establishing database and main window references, instantiating the dashboard view model, setting the data context, and subscribing to the loaded event to asynchronously initialize dashboard data.
        /// </summary>
        /// <param name="shell">The main window instance hosting the application navigation and layout.</param>
        /// <param name="db">The database service instance responsible for data access operations.</param>
        public DashboardView(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();

            databaseService = db;
            mainWindow = shell;
            dashBoardViewModel = new DashboardPageViewModel(databaseService);
            DataContext = dashBoardViewModel;

            Unloaded += (s, e) => dashBoardViewModel.Dispose();
        }
    }
}
