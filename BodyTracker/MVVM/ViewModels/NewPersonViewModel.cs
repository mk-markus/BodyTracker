using BodyTracker.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace BodyTracker.MVVM.ViewModels
{
    partial class NewPersonViewModel : ObservableObject
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
        /// Gets or sets the height of the person, in meters.
        /// </summary>
        [ObservableProperty] private float personHeight = 0;

        /// <summary>
        /// Gets the command that prepares the UI for creating a new person record.
        /// Typically clears input fields or initializes a new data entry state.
        /// </summary>
        public IAsyncRelayCommand NewUserCommand { get; }

        /// <summary>
        /// Represents the constructor for the <see cref="NewPersonViewModel"/> class, which initializes the ViewModel with a reference to the <see cref="DatabaseService"/> and sets up the command for creating a new person.
        /// </summary>
        /// <param name="db">The <see cref="DatabaseService"/> instance used for data operations.</param>
        public NewPersonViewModel(DatabaseService db)
        {
            databaseService = db;
            NewUserCommand = new AsyncRelayCommand(CreatePersonAsync);
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
           await databaseService.CreatePersonAsync(FirstName.Trim(), LastName.Trim(), BirthDate, PersonHeight);
        }

    }
}
