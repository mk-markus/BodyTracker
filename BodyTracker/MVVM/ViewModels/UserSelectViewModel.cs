using BodyTracker.MVVM.Models;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        /// Gets or sets the first name of the person being edited or created.
        /// Defaults to an empty string to prevent null reference issues during data binding.
        /// </summary>
        [ObservableProperty] private string firstName = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the person being edited or created.
        /// Defaults to an empty string.
        /// </summary>
        [ObservableProperty] private string lastName = string.Empty;

        /// <summary>
        /// Gets or sets the date of birth for the person.
        /// Defined as nullable (<see cref="DateTime"/>?) to allow for cases where 
        /// the date has not yet been selected in the UI.
        /// </summary>
        [ObservableProperty] private DateTime? birthDate = null;

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
            NewUserCommand = new AsyncRelayCommand(CreatePersonAsync);
            ConfirmCommand = new RelayCommand(ConfirmSelectedPerson);
        }

        /// <summary>
        /// initialize the connection to the sql server and loads the person from the table.
        /// </summary>
        public async Task InitializeAsync()
        {
            await databaseService.InitializeAsync();
            await LoadPersonAsync();
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
            Persons.Clear();
            var list = await databaseService.GetPersonsAsync();
            foreach (var p in list) Persons.Add(p);
        }

        /// <summary>
        /// Asynchronously persists a new person record to the database and updates the local state.
        /// </summary>
        /// <returns>A task representing the asynchronous creation and selection process.</returns>
        /// <remarks>
        /// This method performs the following sequence:
        /// <list type="number">
        /// <item><description>Calls the <see cref="DatabaseService"/> to insert the trimmed name and birth date.</description></item>
        /// <item><description>Refreshes the <see cref="Personen"/> collection from the database to ensure synchronization.</description></item>
        /// <item><description>Sets the newly created person as the <see cref="Ausgewaehlt"/> item to provide immediate UI feedback.</description></item>
        /// </list>
        /// </remarks>
        private async Task CreatePersonAsync()
        {
            var id = await databaseService.CreatePersonAsync(FirstName.Trim(), LastName.Trim(), BirthDate);
            await LoadPersonAsync();
            Selected = new PersonModel { PersonID = id, PersonFirstName = FirstName.Trim(), PersonLastName = LastName.Trim(), PersonBirthDate = BirthDate };
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
                AppState.SelectedPersonName = Selected.ToString();
            }
        }
    }
}
