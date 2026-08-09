using BodyTracker.ViewModels;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages
{
    /// <summary>
    /// Represents the launcher new database person page within the application, managing UI interaction logic for creating a new person record.
    /// </summary>
    public partial class LauncherNewDatabasePersonPage : Page
    {
        /// <summary>
        /// A private, read-only reference to the view model that handles the business logic, commands, and data binding for the new database person page.
        /// </summary>
        private readonly LauncherNewDatabasePersonPageViewModel launcherNewDatabasePersonPageViewModel;

        /// <summary>
        /// Initializes a new instance of the LauncherNewDatabasePersonPage class, configuring component UI elements, instantiating the view model, setting the data context, and registering an event handler to dispose of resources when the page is unloaded.
        /// </summary>
        public LauncherNewDatabasePersonPage()
        {
            InitializeComponent();

            launcherNewDatabasePersonPageViewModel = new LauncherNewDatabasePersonPageViewModel();

            DataContext = launcherNewDatabasePersonPageViewModel;

            this.Unloaded += (s, e) => launcherNewDatabasePersonPageViewModel.Dispose();
        }
    }
}