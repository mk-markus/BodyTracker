using BodyTracker.Models;
using BodyTracker.Services;
using BodyTracker.Services.SamsungHelath;
using BodyTracker.State;
using BodyTracker.Views.windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BodyTracker.ViewModels
{
    /// <summary>
    /// Orchestrates the logic for user selection and person management within the application, implementing the CommunityToolkit.Mvvm ObservableObject and IDisposable patterns.
    /// </summary>
    public partial class LauncherDashboardViewModel : ObservableObject, IDisposable
    {
        /// <summary>
        /// Backing field for the database service responsible for data access operations.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// Backing field for the parent launcher window instance.
        /// </summary>
        private readonly LauncherWindow launcherWindow;

        /// <summary>
        /// Service responsible for managing application configuration settings, including loading and saving database connection strings from the configuration file.
        /// </summary>
        private DatabaseConfigrationService configurationService;

        /// <summary>
        /// Represents the persisted SQL configuration settings loaded from the configuration file.
        /// </summary>
        private SQLConfigurationModel sqlConfigurationModel;

        /// <summary>
        /// Gets or sets the collection of all available persons, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute to automatically notify the UI when items are added, removed, or the collection is refreshed.
        /// </summary>
        [ObservableProperty] private ObservableCollection<PersonModel> persons = new();

        /// <summary>
        /// Gets or sets the currently selected person from the list, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute to automatically notify the UI and trigger dependent selection logic.
        /// </summary>
        [ObservableProperty] private PersonModel? selectedPerson;

        /// <summary>
        /// Backing field for the current logged-in user name shown in the UI.
        /// </summary>
        private string actualUser = string.Empty;

        /// <summary>
        /// Gets or sets the current logged-in user name shown in the UI, sending a logged-in database user message via the messenger when the value changes.
        /// </summary>
        public string ActualUser
        {
            get => actualUser;
            set
            {
                if (SetProperty(ref actualUser, value))
                {
                    WeakReferenceMessenger.Default.Send(new DatabaseUserLoggedInMessage(actualUser));
                }
            }
        }

        /// <summary>
        /// Backing field for the current database name shown in the UI.
        /// </summary>
        private string acutalDatabase = string.Empty;

        /// <summary>
        /// Gets or sets the current database name shown in the UI, sending an actual database name message via the messenger when the value changes.
        /// </summary>
        public string ActualDatabase
        {
            get => acutalDatabase;
            set
            {
                if (SetProperty(ref acutalDatabase, value))
                {
                    WeakReferenceMessenger.Default.Send(new ActiveDatabaseNameMessage(acutalDatabase));
                }
            }
        }

        /// <summary>
        /// Gets or sets the current connection state message shown in the UI, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute for change notification.
        /// </summary>
        [ObservableProperty] private string actualConnectionState = "Not Connected";

        /// <summary>
        /// Backing field for the additional database connection state information string.
        /// </summary>
        private string databaseAdditonalConncetionStateInfo = "";

        /// <summary>
        /// Gets or sets the additional database connection state information, sending a database additional connection state message via the messenger when the value changes.
        /// </summary>
        public string DatabaseAdditonalConncetionStateInfo
        {
            get => databaseAdditonalConncetionStateInfo;
            set
            {
                if (SetProperty(ref databaseAdditonalConncetionStateInfo, value))
                {
                    WeakReferenceMessenger.Default.Send(new AdditionalDatabaseConnectionMessage(databaseAdditonalConncetionStateInfo));
                }
            }
        }

        /// <summary>
        /// Backing field for the general error message string.
        /// </summary>
        private string generalErrorMessage = "";

        /// <summary>
        /// Gets or sets the general error message, sending a database error message via the messenger when the value changes.
        /// </summary>
        public string GeneralErrorMessage
        {
            get => generalErrorMessage;
            set
            {
                if (SetProperty(ref generalErrorMessage, value))
                {
                    WeakReferenceMessenger.Default.Send(new DatabaseErrorMessage(generalErrorMessage));
                }
            }
        }

        /// <summary>
        /// Gets the command that prepares the UI for creating a new person record.
        /// </summary>
        public IAsyncRelayCommand NewUserCommand { get; }

        /// <summary>
        /// Gets the command that sets new application state values based on the current selection.
        /// </summary>
        public IRelayCommand CommandSetNewAppStateValues { get; }

        /// <summary>
        /// Gets the command that asynchronously loads and transitions to the body tracker tool page.
        /// </summary>
        public IRelayCommand CommandLoadBodyTrackerToolAsync { get; }

        /// <summary>
        /// Gets the command that handles person data grid row edit ending events asynchronously.
        /// </summary>
        public IAsyncRelayCommand CommandPersonRowEditEnding { get; }

        /// <summary>
        /// Initializes a new instance of the LauncherHomePageViewModel class, establishing window references, loading configuration settings, instantiating services and commands, and triggering initial asynchronous data loading.
        /// </summary>
        /// <param name="shell">The parent launcher window instance hosting the application navigation.</param>
        public LauncherDashboardViewModel(LauncherWindow shell)
        {
            launcherWindow = shell;
            configurationService = new DatabaseConfigrationService();
            sqlConfigurationModel = configurationService.LoadConfigurationFile();
            databaseService = new DatabaseService(configurationService.BuildConnectionString());

            CommandSetNewAppStateValues = new RelayCommand(SetNewAppStateValues);

            CommandLoadBodyTrackerToolAsync = new AsyncRelayCommand(ShowBodyTrackerToolPage);
            CommandPersonRowEditEnding = new AsyncRelayCommand<DataGridRowEditEndingEventArgs>(PersonGridRowEditEnding);

            _ = InitializeAsync();
        }

        /// <summary>
        /// Handles property change notifications when the selected person changes, automatically updating the application state values.
        /// </summary>
        /// <param name="value">The newly selected person model instance.</param>
        partial void OnSelectedPersonChanged(PersonModel? value)
        {
            SetNewAppStateValues();
        }

        /// <summary>
        /// Asynchronously initializes the connection to the SQL server, updates connection states, and loads person records from the database table.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a boolean value indicating whether initialization succeeded.</returns>
        public async Task<bool> InitializeAsync()
        {
            try
            {
                SelectedPerson = null;
                databaseAdditonalConncetionStateInfo = "Connecting";



                try
                {
                    await databaseService.InitializeDatabaseAsync();
                    ActualUser = databaseService.DatabaseUser;
                    ActualDatabase = databaseService.DatabaseName;

                    databaseAdditonalConncetionStateInfo = databaseService.IsConnected ? "Connected" : "Not Connected";

                    if (!databaseService.IsConnected)
                    {
                        Persons = new ObservableCollection<PersonModel>();
                        return false;
                    }

                    await LoadPersonAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    GeneralErrorMessage = $"Initialization error: {ex}";
                    ActualConnectionState = "Not Connected";
                    return false;
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Asynchronously retrieves all person records from the database service and populates the observable persons collection.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadPersonAsync()
        {
            var list = await databaseService.GetPersonsAsync();
            Persons = new ObservableCollection<PersonModel>(list);
        }

        /// <summary>
        /// Updates global application state values with the currently selected person's ID, height, and formatted name.
        /// </summary>
        private void SetNewAppStateValues()
        {
            if (SelectedPerson != null)
            {
                AppState.SelectedPersonId = SelectedPerson.PersonID;
                AppState.SelectedPersonHeight = SelectedPerson.PersonHeight;
                AppState.SelectedPersonName = SelectedPerson.ToString();
            }
        }

        /// <summary>
        /// Retrieves and unprotects the saved user name and database name from the provided SQL configuration model.
        /// </summary>
        /// <param name="sqlConfigurationModel">The persisted SQL configuration model containing encrypted credentials.</param>
        public void GetSavedUserAndDatabaseName(SQLConfigurationModel sqlConfigurationModel)
        {
            ActualUser = !string.IsNullOrWhiteSpace(sqlConfigurationModel.User)
                ? CryptoHelper.Unprotect(sqlConfigurationModel.User)
                : "-";

            ActualDatabase = !string.IsNullOrWhiteSpace(sqlConfigurationModel.DatabaseName)
                ? CryptoHelper.Unprotect(sqlConfigurationModel.DatabaseName)
                : "-";
        }

        /// <summary>
        /// Asynchronously updates an existing person record in the database and refreshes the local person collection.
        /// </summary>
        /// <param name="personId">The unique identifier of the person being updated.</param>
        /// <param name="row">The person model containing updated information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdatePersonAsync(int personId, PersonModel row)
        {
            await databaseService.UpdatePersonAsync(
                personId,
                row.PersonFirstName,
                row.PersonLastName,
                row.PersonBirthDate,
                row.PersonHeight
            );

            await LoadPersonAsync();
        }

        /// <summary>
        /// Validates that a user is selected in the application state and asynchronously displays the main body tracker tool page.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowBodyTrackerToolPage()
        {
            if (AppState.SelectedPersonId <= 0)
            {
                GeneralErrorMessage = "No user has been selected. Please select a user!";
                return;
            }

            var main = new MainWindow(databaseService);
            main.Show();
            //launcherWindow.Close();
        }

        /// <summary>
        /// Handles the data grid row edit ending event, committing changes and updating the corresponding person record in the database.
        /// </summary>
        /// <param name="e">An instance containing data grid row edit ending event arguments.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task PersonGridRowEditEnding(DataGridRowEditEndingEventArgs e)
        {
            if (e == null) return;

            if (e.EditAction != DataGridEditAction.Commit) return;

            if (e.Row.Item is not PersonModel editedRow) return;

            int personId = editedRow.PersonID;
            await UpdatePersonAsync(personId, editedRow);
        }

        #region Disposal Pattern

        /// <summary>
        /// Backing field tracking whether the view model instance has already been disposed.
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
        /// Protected implementation of the Dispose pattern, releasing managed resources such as the database service when disposing is true.
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
