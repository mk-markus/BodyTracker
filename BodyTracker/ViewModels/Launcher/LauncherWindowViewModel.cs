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
        /// This service acts as the primary data gateway for all persistence operations initiated by the ViewModel.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// A private, read-only reference to the parent launcher window shell.
        /// </summary>
        private readonly LauncherWindow launcherWindow;

        /// <summary>
        /// Gets or sets the currently active view or page displayed within the navigation container.
        /// </summary>
        [ObservableProperty]
        private object? currentPage;

        /// <summary>
        /// Gets the command responsible for refreshing the measurement history from the database.
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
        /// Gets the command responsible for showing the Hevy API settings view asynchronously.
        /// </summary>
        public IRelayCommand CommandShowHevyAppSettingsLancherViewAsnyc { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the dashboard navigation option is currently selected.
        /// </summary>
        [ObservableProperty]
        private bool isHomeSelected;

        /// <summary>
        /// Gets or sets a value indicating whether the database person navigation option is currently selected.
        /// </summary>
        [ObservableProperty]
        private bool isNewDatabasePersonSelected;

        /// <summary>
        /// Gets or sets a value indicating whether the Hevy app settings navigation option is currently selected.
        /// </summary>
        [ObservableProperty]
        private bool isHevyAppSettingsSelected;

        /// <summary>
        /// Gets or sets a value indicating whether the database connection navigation option is currently selected.
        /// </summary>
        [ObservableProperty]
        private bool isNewDatabaseConnectionSelected;

        /// <summary>
        /// Backing field and generated property for the general failure message string.
        /// </summary>
        [ObservableProperty]
        private string generalFailureMessage = "";

        /// <summary>
        /// Backing field and generated property for the database failure message string.
        /// </summary>
        [ObservableProperty]
        private string databaseFailureMessage = "";

        /// <summary>
        /// Backing field and generated property for the SQL connection status message string.
        /// </summary>
        [ObservableProperty]
        private string sqlConnectionStatusMessage = "";

        /// <summary>
        /// Backing field tracking whether the SQL Server connection is active.
        /// </summary>
        private bool connectionStateSqlServer = false;

        /// <summary>
        /// Backing field and generated property for additional SQL connection status messages.
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
        private bool suppressSelectionAction = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="LauncherWindowViewModel"/> class.
        /// </summary>
        /// <param name="shell">The parent launcher window reference.</param>
        public LauncherWindowViewModel(LauncherWindow shell)
        {
            launcherWindow = shell;
            CommandShowDashboardLancherPageAsnyc = new AsyncRelayCommand(ShowHomePageAsync);
            CommandShowNewDatabaseConnectionPageAsnyc = new AsyncRelayCommand(ShowNewDatabaseConnectionPageAsync);
            CommandShowNewDatabasePersonPageAsnyc = new AsyncRelayCommand(ShowNewDatabasePersonPageAsync);
            CommandShowHevyAppSettingsLancherViewAsnyc = new AsyncRelayCommand(ShowHevyAppSettingsViewAsnyc);

            InitializeWeakReferenceMessenger();

            _ = ShowHomePageAsync();
        }

        /// <summary>
        /// Registers all WeakReferenceMessenger subscriptions for handling error messages, connection state changes, and navigation requests.
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
        /// Asynchronously handles the display and automatic clearing of database error messages on the UI thread.
        /// </summary>
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
        /// Asynchronously handles the display and timed clearing of general error messages on the UI thread.
        /// </summary>
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

        partial void OnIsHomeSelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowHomePageAsync();
        }

        partial void OnIsNewDatabasePersonSelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowNewDatabasePersonPageAsync();
        }

        partial void OnIsNewDatabaseConnectionSelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowNewDatabaseConnectionPageAsync();
        }

        partial void OnIsHevyAppSettingsSelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowHevyAppSettingsViewAsnyc();
        }

        private async Task ShowHomePageAsync()
        {
            CurrentPage = new LauncherHomePage(launcherWindow);

            suppressSelectionAction = true;
            IsHomeSelected = true;
            IsNewDatabaseConnectionSelected = false;
            IsNewDatabasePersonSelected = false;
            IsHevyAppSettingsSelected = false; 
            suppressSelectionAction = false;
        }

        private async Task ShowNewDatabaseConnectionPageAsync()
        {
            CurrentPage = new LauncherNewDatabaseConnectionPage();

            suppressSelectionAction = true;
            IsHomeSelected = false;
            IsNewDatabaseConnectionSelected = true;
            IsNewDatabasePersonSelected = false;
            IsHevyAppSettingsSelected = false;
            suppressSelectionAction = false;
        }

        private async Task ShowNewDatabasePersonPageAsync()
        {
            CurrentPage = new LauncherNewDatabasePersonPage();

            suppressSelectionAction = true;
            IsHomeSelected = false;
            IsNewDatabaseConnectionSelected = false;
            IsNewDatabasePersonSelected = true;
            IsHevyAppSettingsSelected = false;
            suppressSelectionAction = false;
        }

        private async Task ShowHevyAppSettingsViewAsnyc()
        {
            CurrentPage = new LauncherSettingsView();

            suppressSelectionAction = true;
            IsHomeSelected = false;
            IsNewDatabaseConnectionSelected = false;
            IsNewDatabasePersonSelected = false;
            IsHevyAppSettingsSelected = true;
            suppressSelectionAction = false;
        }

        #region Disposal Pattern

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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