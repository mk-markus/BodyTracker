using BodyTracker.Services;
using BodyTracker.Views.Pages;
using BodyTracker.Views.windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels
{
    /// <summary>
    /// Represents the view model for the launcher window, managing page navigation, connection state messages, error timeouts, and disposal patterns.
    /// </summary>
    public partial class LauncherWindowViewModel : ObservableObject, IDisposable
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
        /// A private, read-only reference to the parent launcher window shell.
        /// </summary>
        private readonly LauncherWindow launcherWindow;

        /// <summary>
        /// Gets or sets the currently active view or page displayed within the navigation container.
        /// </summary>
        /// <remarks>Bound to the UI content presenter to dynamically switch views based on user navigation commands.</remarks>
        [ObservableProperty]
        private object? currentPage;

        /// <summary>
        /// Gets the command responsible for refreshing the measurement history from the database.
        /// Triggers an asynchronous reload of the measurement collection.
        /// </summary>
        public IAsyncRelayCommand ReloadCommand { get; }

        /// <summary>
        /// Gets the command responsible for showing the new database person page asynchronously.
        /// </summary>
        public IRelayCommand CommandShowNewDatabasePersonPageAsnyc { get; }

        /// <summary>
        /// Gets the command responsible for showing the new database connection page asynchronously.
        /// </summary>
        public IRelayCommand CommandShowNewDatabaseConnectionPageAsnyc { get; }

        /// <summary>
        /// Gets the command responsible for showing the dashboard launcher page asynchronously.
        /// </summary>
        public IRelayCommand CommandShowDashboardLancherPageAsnyc { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the dashboard navigation option is currently selected.
        /// </summary>
        /// <remarks>Used by UI navigation controls to highlight active menu items and manage visual states.</remarks>
        [ObservableProperty]
        private bool isHomeSelected;

        /// <summary>
        /// Gets or sets a value indicating whether the charts analytics navigation option is currently selected.
        /// </summary>
        /// <remarks>Used by UI navigation controls to highlight active menu items and manage visual states.</remarks>
        [ObservableProperty]
        private bool isNewDatabasePersonSelected;

        /// <summary>
        /// Gets or sets a value indicating whether the database connection navigation option is currently selected.
        /// </summary>
        /// <remarks>Used by UI navigation controls to highlight active menu items and manage visual states.</remarks>
        [ObservableProperty]
        private bool isNewDatabaseConnectionSelected;

        /// <summary>
        /// Backing field and generated property for the general failure message string, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute for change notification.
        /// </summary>
        [ObservableProperty]
        private string generalFailureMessage = "";

        /// <summary>
        /// Backing field and generated property for the database failure message string, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute for change notification.
        /// </summary>
        [ObservableProperty]
        private string databaseFailureMessage = "";

        /// <summary>
        /// Backing field and generated property for the SQL connection status message string, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute for change notification.
        /// </summary>
        [ObservableProperty]
        private string sqlConnectionStatusMessage = "";

        /// <summary>
        /// Backing field tracking whether the SQL Server connection is active.
        /// </summary>
        private bool connectionStateSqlServer = false;

        /// <summary>
        /// Backing field and generated property for additional SQL connection status messages, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute for change notification.
        /// </summary>
        [ObservableProperty]
        private string sqlAddtionalConnectionStatusMessage = "";

        /// <summary>
        /// Gets or sets the name of the currently active logged-in user.
        /// </summary>
        [ObservableProperty]
        private string actualUser = string.Empty;

        /// <summary>
        /// Gets or sets the name of the currently active database.
        /// </summary>
        [ObservableProperty]
        private string actualDatabase = string.Empty;

        /// <summary>
        /// Counter/version used to detect whether an error message changed after scheduling a delayed clear.
        /// </summary>
        private long generalFailureMessageVersion = 0;

        /// <summary>
        /// Represents a version counter used to track and manage the state updates of database error messages.
        /// </summary>
        private long databaseErrorMessageVersion = 0;

        /// <summary>
        /// A flag indicating whether programmatic selection actions should suppress event handling routines.
        /// </summary>
        /// <remarks>Prevents recursive loops or unwanted side effects during automated state synchronization of navigation toggles.</remarks>
        private bool suppressSelectionAction = false;

        /// <summary>
        /// Initializes a new instance of the LauncherWindowViewModel class, assigning the shell reference, instantiating navigation commands, registering messenger listeners, and loading the default home page.
        /// </summary>
        /// <param name="shell">The parent launcher window reference.</param>
        public LauncherWindowViewModel(LauncherWindow shell)
        {
            launcherWindow = shell;
            CommandShowDashboardLancherPageAsnyc = new AsyncRelayCommand(ShowHomePageAsync);
            CommandShowNewDatabaseConnectionPageAsnyc = new AsyncRelayCommand(ShowNewDatabaseConnectionPageAsync);
            CommandShowNewDatabasePersonPageAsnyc = new AsyncRelayCommand(ShowNewDatabasePersonPageAsync);

            InitializeWeakReferenceMessenger();

            _ = ShowHomePageAsync();
        }

        /// <summary>
        /// Registers all WeakReferenceMessenger subscriptions for handling error messages, connection state changes, logged-in user info, active database updates, and navigation requests.
        /// </summary>
        private void InitializeWeakReferenceMessenger()
        {
            WeakReferenceMessenger.Default.Register<GeneralInfoMessage>(this, (r, m) =>
            {
                var value = m.Value ?? string.Empty;
                var version = Interlocked.Increment(ref generalFailureMessageVersion);
                _ = HandleGenralErrorMessageWithTimeoutAsync(value, version, TimeSpan.FromSeconds(30));
            });

            WeakReferenceMessenger.Default.Register<DatabaseErrorMessage>(this, (r, m) =>
            {
                var value = m.Value ?? string.Empty;
                var version = Interlocked.Increment(ref databaseErrorMessageVersion);

                _ = HandleDatabaseErrorMessageWithConnectionCheckAsync(value, version, TimeSpan.FromSeconds(30));
            });

            WeakReferenceMessenger.Default.Register<DatabaseConnectionStateMessage>(this, (r, m) =>
            {
                bool isConnected = m.Value;

                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    new Action(() => SqlConnectionStatusMessage = isConnected ? "OK" : "NOK"
                    ));

            });

            WeakReferenceMessenger.Default.Register<AdditionalDatabaseConnectionMessage>(this, (r, m) =>
            {
                string message = m.Value;

                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        if (string.IsNullOrEmpty(message)) return;

                        SqlAddtionalConnectionStatusMessage = message;
                    }));
            });

            WeakReferenceMessenger.Default.Register<DatabaseUserLoggedInMessage>(this, (r, m) =>
            {
                string message = m.Value;

                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        if (string.IsNullOrEmpty(message)) return;

                        ActualUser = message;
                    }));
            });

            WeakReferenceMessenger.Default.Register<ActiveDatabaseNameMessage>(this, (r, m) =>
            {
                string message = m.Value;

                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        if (string.IsNullOrEmpty(message)) return;

                        ActualDatabase = message;
                    }));
            });

            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                if (m.Target == NavigationMessage.ShowLauncher) _ = ShowHomePageAsync();
            });
        }

        /// <summary>
        /// Asynchronously handles the display and automatic clearing of database error messages on the UI thread, 
        /// incorporating a version check to prevent race conditions and waiting until the database server connection is restored before clearing the message.
        /// </summary>
        /// <param name="message">The database error message to display.</param>
        /// <param name="targetVersion">The expected version number of the database error message state to ensure validity.</param>
        /// <param name="delay">The initial time delay before checking the server connection status.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task HandleDatabaseErrorMessageWithConnectionCheckAsync(string message, long targetVersion, TimeSpan delay)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (Volatile.Read(ref databaseErrorMessageVersion) == targetVersion)
                {
                    DatabaseFailureMessage = message;
                }
            });

            if (string.IsNullOrEmpty(message)) return;

            await Task.Delay(delay);

            while (!connectionStateSqlServer)
            {
                if (Volatile.Read(ref databaseErrorMessageVersion) != targetVersion) return;

                await Task.Delay(500);
            }

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (Volatile.Read(ref databaseErrorMessageVersion) == targetVersion)
                {
                    DatabaseFailureMessage = string.Empty;
                }
            });
        }

        /// <summary>
        /// Asynchronously handles the display and timed clearing of general error messages on the UI thread, 
        /// utilizing a version check to prevent race conditions and automatically resetting the message after a specified delay.
        /// </summary>
        /// <param name="message">The general error message to display.</param>
        /// <param name="targetVersion">The expected version number of the general failure message state to ensure validity.</param>
        /// <param name="delay">The duration for which the error message remains visible before being cleared.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task HandleGenralErrorMessageWithTimeoutAsync(string message, long targetVersion, TimeSpan delay)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (Volatile.Read(ref generalFailureMessageVersion) == targetVersion)
                {
                    GeneralFailureMessage = message;
                }
            });

            if (string.IsNullOrEmpty(message)) return;

            await Task.Delay(delay);

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (Volatile.Read(ref generalFailureMessageVersion) == targetVersion)
                {
                    GeneralFailureMessage = string.Empty;
                }
            });
        }

        /// <summary>
        /// Handles the change event for the home selection property, triggering navigation when selected.
        /// </summary>
        /// <remarks>Ignores unselection events or changes suppressed during programmatic state updates.</remarks>
        /// <param name="value">The new boolean selection state.</param>
        partial void OnIsHomeSelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowHomePageAsync();
        }

        /// <summary>
        /// Handles the change event for the new database person selection property, triggering navigation when selected.
        /// </summary>
        /// <remarks>Ignores unselection events or changes suppressed during programmatic state updates.</remarks>
        /// <param name="value">The new boolean selection state.</param>
        partial void OnIsNewDatabasePersonSelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowNewDatabasePersonPageAsync();
        }

        /// <summary>
        /// Handles the change event for the new database connection selection property, triggering navigation when selected.
        /// </summary>
        /// <remarks>Ignores unselection events or changes suppressed during programmatic state updates.</remarks>
        /// <param name="value">The new boolean selection state.</param>
        partial void OnIsNewDatabaseConnectionSelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowNewDatabaseConnectionPageAsync();
        }

        /// <summary>
        /// Asynchronously navigates to the home page view and updates the navigation toggle states.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowHomePageAsync()
        {
            CurrentPage = new LauncherHomePage(launcherWindow);

            suppressSelectionAction = true;
            IsHomeSelected = true;
            IsNewDatabaseConnectionSelected = false;
            IsNewDatabasePersonSelected = false;
            suppressSelectionAction = false;
        }

        /// <summary>
        /// Asynchronously navigates to the new database connection page view and updates the navigation toggle states.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowNewDatabaseConnectionPageAsync()
        {
            CurrentPage = new LauncherNewDatabaseConnectionPage();

            suppressSelectionAction = true;
            IsHomeSelected = false;
            IsNewDatabaseConnectionSelected = true;
            IsNewDatabasePersonSelected = false;
            suppressSelectionAction = false;
        }

        /// <summary>
        /// Asynchronously navigates to the new database person page view and updates the navigation toggle states.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowNewDatabasePersonPageAsync()
        {
            CurrentPage = new LauncherNewDatabasePersonPage();

            suppressSelectionAction = true;
            IsHomeSelected = false;
            IsNewDatabaseConnectionSelected = false;
            IsNewDatabasePersonSelected = true;
            suppressSelectionAction = false;
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