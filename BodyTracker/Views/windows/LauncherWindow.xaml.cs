using BodyTracker.Models;
using BodyTracker.Services;
using BodyTracker.ViewModels;
using System.Windows;

namespace BodyTracker.Views.windows
{
    /// <summary>
    /// Represents the launcher window within the application, managing window-level lifecycle events, data context initialization, and UI interaction logic.
    /// </summary>
    public partial class LauncherWindow : Window
    {
        /// <summary>
        /// A private reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the application.
        /// </summary>
        private DatabaseService databaseService;

        /// <summary>
        /// Represents the persisted SQL configuration settings loaded from the configuration file.
        /// </summary>
        private SQLConfigurationModel sqlConfigurationModel;

        /// <summary>
        /// A private reference to the view model that handles the business logic, commands, navigation, and data binding for the launcher window.
        /// </summary>
        private LauncherWindowViewModel launcherWindowViewModel;

        /// <summary>
        /// Service responsible for managing application configuration settings, 
        /// including loading and saving database connection strings from the configuration file.
        /// </summary>
        private DatabaseConfigrationService configurationService;

        /// <summary>
        /// Initializes a new instance of the LauncherWindow class, instantiating the view model, setting the data context, and registering an event handler to dispose of resources when the window is unloaded.
        /// </summary>
        public LauncherWindow()
        {
            InitializeComponent();

            launcherWindowViewModel = new LauncherWindowViewModel(this);
            DataContext = launcherWindowViewModel;

            this.Unloaded += (s, e) => launcherWindowViewModel.Dispose();
        }
    }
}