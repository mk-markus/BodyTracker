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
        /// Provides access to the data storage layer for managing person records in the database.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// Gets or sets the first name of the person. 
        /// Changing this value triggers property change notifications for <see cref="AreFieldsFilled"/> and <see cref="IsNewPersonFormValid"/>.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AreFieldsFilled))]
        [NotifyPropertyChangedFor(nameof(IsNewPersonFormValid))]
        private string firstName = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the person. 
        /// Changing this value triggers property change notifications for <see cref="AreFieldsFilled"/> and <see cref="IsNewPersonFormValid"/>.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AreFieldsFilled))]
        [NotifyPropertyChangedFor(nameof(IsNewPersonFormValid))]
        private string lastName = string.Empty;

        /// <summary>
        /// Gets or sets the birth date of the person. 
        /// Changing this value triggers property change notifications for <see cref="AreFieldsFilled"/> and <see cref="IsNewPersonFormValid"/>.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AreFieldsFilled))]
        [NotifyPropertyChangedFor(nameof(IsNewPersonFormValid))]
        private DateTime birthDate = default;

        /// <summary>
        /// Gets or sets the string representation of the person's height. 
        /// Changing this value triggers property change notifications for <see cref="AreFieldsFilled"/> and <see cref="IsNewPersonFormValid"/>.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AreFieldsFilled))]
        [NotifyPropertyChangedFor(nameof(IsNewPersonFormValid))]
        private string personHeight = string.Empty;

        /// <summary>
        /// Gets or sets the current validation error message to display in the user interface.
        /// </summary>
        [ObservableProperty] private string validationMessage = string.Empty;

        /// <summary>
        /// Gets a value indicating whether all required input fields have been filled with non-empty and valid data.
        /// </summary>
        public bool AreFieldsFilled =>
            !string.IsNullOrWhiteSpace(FirstName) &&
            !string.IsNullOrWhiteSpace(LastName) &&
            BirthDate != default &&
            double.TryParse(PersonHeight, out double height) && height > 0;

        /// <summary>
        /// Gets a value indicating whether the form for creating a new person is valid and ready for submission.
        /// Mirrors the state of <see cref="AreFieldsFilled"/>.
        /// </summary>
        public bool IsNewPersonFormValid => AreFieldsFilled;

        /// <summary>
        /// Handles changes to the <see cref="FirstName"/> property. Validates that the name is not empty, 
        /// updates the validation message, and re-evaluates the overall form state.
        /// </summary>
        /// <param name="value">The new first name string value.</param>
        partial void OnFirstNameChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                ValidationMessage = "First name cannot be empty.";
            else
                ValidationMessage = string.Empty;

            UpdateValidation();
        }

        /// <summary>
        /// Handles changes to the <see cref="LastName"/> property. Validates that the name is not empty, 
        /// updates the validation message, and re-evaluates the overall form state.
        /// </summary>
        /// <param name="value">The new last name string value.</param>
        partial void OnLastNameChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                ValidationMessage = "Last name cannot be empty.";
            else
                ValidationMessage = string.Empty;

            UpdateValidation();
        }

        /// <summary>
        /// Handles changes to the <see cref="BirthDate"/> property. Validates that a non-default date has been chosen, 
        /// updates the validation message, and re-evaluates the overall form state.
        /// </summary>
        /// <param name="value">The new birth date value.</param>
        partial void OnBirthDateChanged(DateTime value)
        {
            if (value == default)
                ValidationMessage = "Enter birth date.";
            else
                ValidationMessage = string.Empty;

            UpdateValidation();
        }

        /// <summary>
        /// Handles changes to the <see cref="PersonHeight"/> property. Validates that the input represents a positive 
        /// floating-point number, updates the validation message, and re-evaluates the overall form state.
        /// </summary>
        /// <param name="value">The new height string representation.</param>
        partial void OnPersonHeightChanged(string value)
        {
            if (!double.TryParse(value, out double height) || height <= 0)
                ValidationMessage = "Height must be a positive number.";
            else
                ValidationMessage = string.Empty;

            UpdateValidation();
        }

        /// <summary>
        /// Gets the command responsible for asynchronously creating and saving a new user record.
        /// </summary>
        public IAsyncRelayCommand NewUserCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewPersonViewModel"/> class, 
        /// dependency injecting the database service and initializing validation states.
        /// </summary>
        /// <param name="db">The data service instance used for persistence operations.</param>
        public NewPersonViewModel(DatabaseService db)
        {
            databaseService = db;
            NewUserCommand = new AsyncRelayCommand(CreatePersonAsync);
            UpdateValidation();
        }

        /// <summary>
        /// Asynchronously persists the newly entered person data into the database service after a final parsing check on the height.
        /// </summary>
        /// <returns>A task that represents the asynchronous database insert operation.</returns>
        private async Task CreatePersonAsync()
        {
            if (int.TryParse(PersonHeight, out var height))
                await databaseService.CreatePersonAsync(FirstName.Trim(), LastName.Trim(), BirthDate, height);
        }

        /// <summary>
        /// Synchronizes the <see cref="ValidationMessage"/> state based on field completion status and forces property change notifications 
        /// for dependent validation flags to ensure the user interface stays updated.
        /// </summary>
        private void UpdateValidation()
        {
            if (!AreFieldsFilled)
            {
                if (string.IsNullOrEmpty(ValidationMessage))
                    ValidationMessage = "All fields must be filled in.";
            }
            else
            {
                ValidationMessage = string.Empty;
            }
            OnPropertyChanged(nameof(AreFieldsFilled));
            OnPropertyChanged(nameof(IsNewPersonFormValid));
        }
    }
}
