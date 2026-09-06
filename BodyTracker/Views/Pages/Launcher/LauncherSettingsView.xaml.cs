using BodyTracker.Services;
using BodyTracker.ViewModels;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages
{
    /// <summary>
    /// Represents the launcher new database connection page within the application, managing UI interaction logic for establishing database connections.
    /// </summary>
    public partial class LauncherSettingsView : Page
    {
        /// <summary>
        /// Represents the view model that handles the business logic, commands, and data binding for the new database connection page.
        /// </summary>
        private readonly LauncherSettingsViewModel launcherHevyAppAPISettingsViewModel;

        /// <summary>
        /// Initializes a new instance of the LauncherNewDatabaseConnectionPage class, configuring component UI elements, instantiating the view model, setting the data context, and registering an event handler to dispose of resources when the page is unloaded.
        /// </summary>
        public LauncherSettingsView()
        {
            InitializeComponent();
            launcherHevyAppAPISettingsViewModel = new LauncherSettingsViewModel();
            DataContext = launcherHevyAppAPISettingsViewModel;

            this.Unloaded += (s, e) => launcherHevyAppAPISettingsViewModel.Dispose();
        }

    }
}