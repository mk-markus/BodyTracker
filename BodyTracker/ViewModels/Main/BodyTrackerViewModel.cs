using BodyTracker.Services;
using BodyTracker.State;
using BodyTracker.Views;
using BodyTracker.Views.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels
{

    public partial class BodyTrackerViewModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets a read-only reference to the main application window instance.
        /// </summary>
        /// <remarks>Provides direct access to window-level operations, container frames, or parent UI contexts from within dependent components.</remarks>
        private readonly MainWindow mainWindow;

        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the ViewModel.
        /// </summary>
        /// <remarks>
        /// Marked as <c>readonly</c> to ensure that the service reference remains 
        /// immutable throughout the lifetime of the instance, preventing 
        /// accidental reassignment and ensuring architectural stability.
        /// </remarks>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// Gets or sets the name of the person currently being viewed.
        /// Used for display purposes in headers or titles.
        /// </summary>
        [ObservableProperty] private string userName = string.Empty;


        /// <summary>
        /// Gets or sets the name of the person currently being viewed.
        /// Used for display purposes in headers or titles.
        /// </summary>
        [ObservableProperty] private string userNameInitial = string.Empty;


        /// <summary>
        /// Gets or sets the currently active view or page displayed within the navigation container.
        /// </summary>
        /// <remarks>Bound to the UI content presenter to dynamically switch views based on user navigation commands.</remarks>
        [ObservableProperty] private object? currentPage;

        /// <summary>
        /// Gets the command responsible for navigating to the dashboard view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display the main overview dashboard and updates navigation selection states.</remarks>
        public IAsyncRelayCommand CommandShowDashboardPageAsync { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the charts analytics view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to load and display advanced chart analytics and visual reports.</remarks>
        public IAsyncRelayCommand CommandShowChartsPageAsync { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the bodyMeasurement database management view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to display tabular logs of recorded body measurements.</remarks>
        public IAsyncRelayCommand CommandShowMeasurementDatabasePageAsync { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the new database entry creation view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to open the form for recording new body measurements or workouts.</remarks>
        public IAsyncRelayCommand CommandNewDatabaseEntryPageAsync { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the multi-file data import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to facilitate batch imports of external workout and health tracking logs.</remarks>
        public IAsyncRelayCommand CommandMultiImportPageAsync { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the multi-file data import view.
        /// </summary>
        /// <remarks>Triggers an asynchronous page transition to facilitate batch imports of external workout and health tracking logs.</remarks>
        public IAsyncRelayCommand CommandInfoPageAsync { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the dashboard navigation option is currently selected.
        /// </summary>
        /// <remarks>Used by UI navigation controls to highlight active menu items and manage visual states.</remarks>
        [ObservableProperty] private bool isDashboardSelected;

        /// <summary>
        /// Gets or sets a value indicating whether the charts analytics navigation option is currently selected.
        /// </summary>
        /// <remarks>Used by UI navigation controls to highlight active menu items and manage visual states.</remarks>
        [ObservableProperty] private bool isChartsSelected;

        /// <summary>
        /// Gets or sets a value indicating whether the bodyMeasurement database navigation option is currently selected.
        /// </summary>
        /// <remarks>Used by UI navigation controls to highlight active menu items and manage visual states.</remarks>
        [ObservableProperty] private bool isMeasurementDatabaseSelected;

        /// <summary>
        /// Gets or sets a value indicating whether the new entry navigation option is currently selected.
        /// </summary>
        /// <remarks>Used by UI navigation controls to highlight active menu items and manage visual states.</remarks>
        [ObservableProperty] private bool isNewEntrySelected;

        /// <summary>
        /// Gets or sets a value indicating whether the import navigation option is currently selected.
        /// </summary>
        /// <remarks>Used by UI navigation controls to highlight active menu items and manage visual states.</remarks>
        [ObservableProperty] private bool isImportSelected;

        /// <summary>
        /// Gets or sets a value indicating whether the info navigation option is currently selected.
        /// </summary>
        /// <remarks>Used by UI navigation controls to highlight active menu items and manage visual states.</remarks>
        [ObservableProperty] private bool isInfoSelected;

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
        /// Backing field and generated property for the general failure message string, utilizing the CommunityToolkit.Mvvm 
        /// ObservableProperty attribute for change notification.
        /// </summary>
        [ObservableProperty] private string generalFailureMessage = "";

        /// <summary>
        /// Backing field and generated property for the database failure message string, utilizing the CommunityToolkit.Mvvm 
        /// ObservableProperty attribute for change notification.
        /// </summary>
        [ObservableProperty] private string databaseFailureMessage = "";
        /// <summary>
        /// Backing field and generated property for the SQL connection status message string, utilizing the CommunityToolkit.Mvvm 
        /// ObservableProperty attribute for change notification.
        /// </summary>
        [ObservableProperty] private string sqlConnectionStatusMessage = "";


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
        /// 
        /// </summary>
        private CancellationTokenSource? connectionCheckCancellationTokenSource;


        /// <summary>
        /// Initializes a new instance of the <see cref="BodyTrackerViewModel"/> class with the specified main window shell and database service.
        /// </summary>
        /// <remarks>Resolves user profile context, binds navigation commands, registers weak reference message handlers for cross-view navigation requests, and triggers the initial navigation to the dashboard view.</remarks>
        /// <param name="shell">The reference to the primary application window instance.</param>
        /// <param name="db">The database service instance used for data persistence and retrieval operations.</param>
        public BodyTrackerViewModel(MainWindow shell, DatabaseService db)
        {
            UserName = AppState.SelectedPersonName;
            UserNameInitial = GetinitialsName(UserName);


            mainWindow = shell;
            databaseService = db;

            ActualUser = databaseService.DatabaseUser + " / " + AppState.SelectedPersonName;
            ActualDatabase = databaseService.DatabaseName;

            CommandShowChartsPageAsync = new AsyncRelayCommand(ShowChartsPageAsync);
            CommandShowDashboardPageAsync = new AsyncRelayCommand(ShowDashboardPageAsync);
            CommandShowMeasurementDatabasePageAsync = new AsyncRelayCommand(ShowMeasurementDatabasePageAsync);
            CommandNewDatabaseEntryPageAsync = new AsyncRelayCommand(ShowNewDatabaseEntryPageAsync);
            CommandMultiImportPageAsync = new AsyncRelayCommand(ShowMeasurementDatabasePageAsync);
            CommandInfoPageAsync = new AsyncRelayCommand(ShowInfoPageAsync);

            WeakReferenceMessenger.Default.Register<GeneralErrorMessage>(this, (r, m) =>
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


            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                if (m.Target == NavigationMessage.ShowDashboard) _ = ShowDashboardPageAsync();
                if (m.Target == NavigationMessage.ShowNewEntry) _ = ShowNewDatabaseEntryPageAsync();
                if (m.Target == NavigationMessage.ShowMeasurement) _ = ShowMeasurementDatabasePageAsync();
                if (m.Target == NavigationMessage.ShowMultiChart) _ = ShowChartsPageAsync();
                if (m.Target == NavigationMessage.ShowInfo) _ = ShowInfoPageAsync();
            });

            _ = ShowDashboardPageAsync();
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

            while (!databaseService.IsConnected)
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
        /// Generates an uppercase initials string from a given person's full name.
        /// </summary>
        /// <remarks>Splits the input name by whitespace, removes empty entries, extracts the first character of each word, converts them to uppercase, and concatenates them into a single initials string.</remarks>
        /// <param name="PersonName">The full name string from which to extract initials.</param>
        /// <returns>A string consisting of the uppercase initials of the provided name.</returns>
        public string GetinitialsName(string PersonName)
        {

            return string.Concat(PersonName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(wort => char.ToUpper(wort[0])));

        }

        /// <summary>
        /// Handles the change event for the dashboard selection property, triggering navigation when selected.
        /// </summary>
        /// <remarks>Ignores unselection events or changes suppressed during programmatic state updates.</remarks>
        /// <param name="value">The new boolean selection state.</param>
        partial void OnIsDashboardSelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowDashboardPageAsync();
        }

        /// <summary>
        /// Handles the change event for the charts analytics selection property, triggering navigation when selected.
        /// </summary>
        /// <remarks>Ignores unselection events or changes suppressed during programmatic state updates.</remarks>
        /// <param name="value">The new boolean selection state.</param>
        partial void OnIsChartsSelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowChartsPageAsync();
        }

        /// <summary>
        /// Handles the change event for the bodyMeasurement database selection property, triggering navigation when selected.
        /// </summary>
        /// <remarks>Ignores unselection events or changes suppressed during programmatic state updates.</remarks>
        /// <param name="value">The new boolean selection state.</param>
        partial void OnIsMeasurementDatabaseSelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowMeasurementDatabasePageAsync();
        }

        /// <summary>
        /// Handles the change event for the new entry selection property, triggering navigation when selected.
        /// </summary>
        /// <remarks>Ignores unselection events or changes suppressed during programmatic state updates.</remarks>
        /// <param name="value">The new boolean selection state.</param>
        partial void OnIsNewEntrySelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowNewDatabaseEntryPageAsync();
        }

        /// <summary>
        /// Handles the change event for the import selection property, triggering navigation when selected.
        /// </summary>
        /// <remarks>Ignores unselection events or changes suppressed during programmatic state updates.</remarks>
        /// <param name="value">The new boolean selection state.</param>
        partial void OnIsImportSelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowImportPageAsync();
        }

        /// <summary>
        /// Handles the change event for the import selection property, triggering navigation when selected.
        /// </summary>
        /// <remarks>Ignores unselection events or changes suppressed during programmatic state updates.</remarks>
        /// <param name="value">The new boolean selection state.</param>
        partial void OnIsInfoSelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowInfoPageAsync();
        }

        /// <summary>
        /// Asynchronously navigates to the multi-import view and updates the navigation toggle states.
        /// </summary>
        /// <remarks>Instantiates the multi-import page view and uses suppression guards to synchronize navigation toggle flags without triggering recursive loops.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowImportPageAsync()
        {
            CurrentPage = new MultiImportPage(mainWindow, databaseService);

            suppressSelectionAction = true;
            IsDashboardSelected = false;
            IsChartsSelected = false;
            IsMeasurementDatabaseSelected = false;
            IsNewEntrySelected = false;
            IsImportSelected = true;
            suppressSelectionAction = false;

        }

        /// <summary>
        /// Asynchronously navigates to the dashboard view and updates the navigation toggle states.
        /// </summary>
        /// <remarks>Instantiates the dashboard page view and uses suppression guards to synchronize navigation toggle flags without triggering recursive loops.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowDashboardPageAsync()
        {
            var page = new DashboardPage(mainWindow, databaseService);
            CurrentPage = page;

            suppressSelectionAction = true;
            IsDashboardSelected = true;
            IsChartsSelected = false;
            IsMeasurementDatabaseSelected = false;
            IsNewEntrySelected = false;
            IsImportSelected = false;
            suppressSelectionAction = false;

        }

        /// <summary>
        /// Asynchronously navigates to the charts analytics view and updates the navigation toggle states.
        /// </summary>
        /// <remarks>Instantiates the multi-charts page view and uses suppression guards to synchronize navigation toggle flags without triggering recursive loops.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowChartsPageAsync()
        {
            var page = new MultiChartsPage(mainWindow, databaseService);
            CurrentPage = page;

            suppressSelectionAction = true;
            IsDashboardSelected = false;
            IsChartsSelected = true;
            IsMeasurementDatabaseSelected = false;
            IsNewEntrySelected = false;
            IsImportSelected = false;
            suppressSelectionAction = false;
        }

        /// <summary>
        /// Asynchronously navigates to the bodyMeasurement database view and updates the navigation toggle states.
        /// </summary>
        /// <remarks>Instantiates the bodyMeasurement page view and uses suppression guards to synchronize navigation toggle flags without triggering recursive loops.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowMeasurementDatabasePageAsync()
        {
            var page = new DataBaseEntriesBodyMeasurementPage(mainWindow, databaseService);
            CurrentPage = page;

            suppressSelectionAction = true;
            IsDashboardSelected = false;
            IsChartsSelected = false;
            IsMeasurementDatabaseSelected = true;
            IsNewEntrySelected = false;
            IsImportSelected = false;
            suppressSelectionAction = false;

        }

        /// <summary>
        /// Asynchronously navigates to the new data entry view and updates the navigation toggle states.
        /// </summary>
        /// <remarks>Instantiates the new data entry page view and uses suppression guards to synchronize navigation toggle flags without triggering recursive loops.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowNewDatabaseEntryPageAsync()
        {
            var page = new NewDataEntryPage(mainWindow, databaseService);
            CurrentPage = page;

            suppressSelectionAction = true;
            IsDashboardSelected = false;
            IsChartsSelected = false;
            IsMeasurementDatabaseSelected = false;
            IsNewEntrySelected = true;
            IsImportSelected = false;
            suppressSelectionAction = false;
        }

        /// <summary>
        /// Asynchronously navigates to the new data entry view and updates the navigation toggle states.
        /// </summary>
        /// <remarks>Instantiates the new data entry page view and uses suppression guards to synchronize navigation toggle flags without triggering recursive loops.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowInfoPageAsync()
        {
            var page = new InfoPage();
            CurrentPage = page;

            suppressSelectionAction = true;
            IsDashboardSelected = false;
            IsChartsSelected = false;
            IsMeasurementDatabaseSelected = false;
            IsNewEntrySelected = false;
            IsImportSelected = false;
            IsInfoSelected = true;
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
                    connectionCheckCancellationTokenSource?.Cancel();
                    connectionCheckCancellationTokenSource?.Dispose();
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
