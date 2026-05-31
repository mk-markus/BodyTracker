using BodyTracker.MVVM.Models;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels
{
    /// <summary>
    /// Orchestrates the logic for user selection and person management within the application.
    /// </summary>
    public partial class UserSelectViewModel : ObservableObject
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

        // <summary>
        /// Gets or sets the collection of all available persons.
        /// Uses <see cref="ObservableCollection{T}"/> to automatically notify the UI 
        /// when persons are added, removed, or the entire list is refreshed.
        /// </summary>
        [ObservableProperty] private ObservableCollection<PersonModel> persons = new();

        /// <summary>
        /// Gets or sets the currently selected person from the list.
        /// This property is typically bound to the 'SelectedItem' of a DataGrid or ListView.
        /// </summary>
        /// <remarks>
        /// Changing this value can trigger dependent logic, such as enabling 
        /// or disabling commands related to a specific person.
        /// </remarks>
        [ObservableProperty] private PersonModel? selected;

        /// <summary>
        /// Current Logged in user shown in the UI, retrieved from the SQL configuration.
        /// </summary>
        [ObservableProperty] private string actualUser = string.Empty;

        /// <summary>
        /// Current Database name shown in the UI, retrieved from the SQL configuration.
        /// </summary>
        [ObservableProperty] private string acutalDatabase = string.Empty;

        /// <summary>
        /// Current connection state shown in the UI.
        /// </summary>
        [ObservableProperty] private string actualConnectionState = "Not Connected";

        /// <summary>
        /// Gets the command responsible for asynchronously loading the list of persons from the database.
        /// Triggers the population of the <see cref="Personen"/> collection.
        /// </summary>
        public IAsyncRelayCommand LoadCommand { get; }

        /// <summary>
        /// Gets the command that prepares the UI for creating a new person record.
        /// Typically clears input fields or initializes a new data entry state.
        /// </summary>
        public IAsyncRelayCommand NewUserCommand { get; }

        /// <summary>
        /// Gets the command that confirms and saves the current input or changes to the database.
        /// Acts as the final execution step for data persistence operations.
        /// </summary>
        /// <remarks>
        /// As a <see cref="IRelayCommand"/>, it often includes validation logic via 'CanExecute' 
        /// to ensure that mandatory fields like <see cref="Vorname"/> are not empty.
        /// </remarks>
        public IRelayCommand ConfirmCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSelectViewModel"/> class.
        /// Sets up the database service and initializes the commands for loading, 
        /// creating, and selecting users.
        /// </summary>
        /// <param name="db">
        /// The injected <see cref="DatabaseService"/> used for person-related 
        /// database operations.
        /// </param>
        /// <remarks>
        /// This constructor follows the Dependency Injection pattern, ensuring 
        /// that the ViewModel is provided with its required data access layer 
        /// upon instantiation.
        /// </remarks>
        public UserSelectViewModel(DatabaseService db)
        {
            databaseService = db;
            LoadCommand = new AsyncRelayCommand(LoadPersonAsync);
            ConfirmCommand = new RelayCommand(ConfirmSelectedPerson);
        }

        /// <summary>
        /// initialize the connection to the sql server and loads the person from the table.
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            try
            {
                Selected = null;
                ActualConnectionState = "Connecting";

                try
                {
                    await databaseService.InitializeAsync();

                    ActualConnectionState = databaseService.ServerConnectionOK ? "Connected" : "Not Connected";

                    if (!databaseService.ServerConnectionOK)
                    {
                        Persons = new System.Collections.ObjectModel.ObservableCollection<PersonModel>();
                        return false;
                    }

                    await LoadPersonAsync();
                    return true;
                }
                catch
                {
                    Persons = new System.Collections.ObjectModel.ObservableCollection<PersonModel>();
                    Selected = null;
                    ActualConnectionState = "Not Connected";
                    return false;
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Prepares the ViewModel for use by establishing a database connection 
        /// and retrieving the initial dataset.
        /// </summary>
        /// <returns>A task that represents the asynchronous initialization process.</returns>
        /// <remarks>
        /// This method performs two critical startup tasks:
        /// <list type="number">
        /// <item><description>Initializes the <see cref="DatabaseService"/> to ensure the SQL connection is active.</description></item>
        /// <item><description>Calls <see cref="LoadPersonAsync"/> to populate the UI with available person records.</description></item>
        /// </list>
        /// </remarks>
        private async Task LoadPersonAsync()
        {
            var list = await databaseService.GetPersonsAsync();

            Persons = new System.Collections.ObjectModel.ObservableCollection<PersonModel>(list);
        }

        /// <summary>
        /// Transfers the identifiers of the currently selected person to the global application state.
        /// </summary>
        /// <remarks>
        /// This method acts as a state transitioner. It ensures that subsequent ViewModels 
        /// can access the chosen person's ID and display name without needing direct 
        /// references to the selection UI.
        /// </remarks>
        private void ConfirmSelectedPerson()
        {
            if (Selected != null)
            {
                AppState.SelectedPersonId = Selected.PersonID;
                AppState.SelectedPersonHeight = Selected.PersonHeight;
                AppState.SelectedPersonName = Selected.ToString();
            }
        }

        /// <summary>
        /// Updates the UI-visible connection information from the persisted SQL configuration.
        /// This method is intentionally part of the ViewModel so the View only binds to properties.
        /// </summary>
        public void UpdateConnectionInfo(SQLConfigurationModel sqlConfigurationModel)
        {
            ActualUser = !string.IsNullOrWhiteSpace(sqlConfigurationModel.User) ? CryptoHelper.Unprotect(sqlConfigurationModel.User) : "-";

            AcutalDatabase = !string.IsNullOrWhiteSpace(sqlConfigurationModel.DatabaseName) ? CryptoHelper.Unprotect(sqlConfigurationModel.DatabaseName) : "-";

        }

        /// <summary>
        /// Asynchronously updates an existing measurement record or inserts a new one into the database.
        /// This method acts as a wrapper for the data access layer, ensuring that all metric and 
        /// dimensional data is persisted before triggering a full UI refresh.
        /// </summary>
        /// <param name="personId">The unique identifier of the person to whom the measurements belong.</param>
        /// <param name="row">The view model containing the measurement data to be synchronized.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// The "Upsert" logic (Update or Insert) is determined by the presence of existing IDs 
        /// within the <paramref name="row"/>. Post-execution, <see cref="ReloadAsync"/> is invoked 
        /// to ensure the local collection remains consistent with the database state, 
        /// including any server-generated identifiers.
        /// </remarks>
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
    }
}
