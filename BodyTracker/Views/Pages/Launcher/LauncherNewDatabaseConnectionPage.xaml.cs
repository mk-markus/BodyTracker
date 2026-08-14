using BodyTracker.Services;
using BodyTracker.ViewModels;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages
{
    /// <summary>
    /// Represents the launcher new database connection page within the application, managing UI interaction logic for establishing database connections.
    /// </summary>
    public partial class LauncherNewDatabaseConnectionPage : Page
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
        /// Represents the view model that handles the business logic, commands, and data binding for the new database connection page.
        /// </summary>
        private readonly LauncherNewDatabaseConnectionPageViewModel launcherNewDatabaseConnectionPageViewModel;

        /// <summary>
        /// Initializes a new instance of the LauncherNewDatabaseConnectionPage class, configuring component UI elements, instantiating the view model, setting the data context, and registering an event handler to dispose of resources when the page is unloaded.
        /// </summary>
        public LauncherNewDatabaseConnectionPage()
        {
            InitializeComponent();
            launcherNewDatabaseConnectionPageViewModel = new LauncherNewDatabaseConnectionPageViewModel();
            DataContext = launcherNewDatabaseConnectionPageViewModel;

            this.Unloaded += (s, e) => launcherNewDatabaseConnectionPageViewModel.Dispose();
        }
    }
}