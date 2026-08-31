using BodyTracker.Models;
using BodyTracker.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels
{
    /// <summary>
    /// Represents the view model for creating a new person record within the launcher database workflow, managing input properties, form validation rules, database persistence, and disposal patterns.
    /// </summary>
    partial class LauncherNewDBPersonViewModel : ObservableObject, IDisposable
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
        /// Backing field for the first name of the person, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute and notifying dependent validation properties.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AreFieldsFilled))]
        [NotifyPropertyChangedFor(nameof(IsNewPersonFormValid))]
        private string firstName = string.Empty;

        /// <summary>
        /// Backing field for the last name of the person, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute and notifying dependent validation properties.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AreFieldsFilled))]
        [NotifyPropertyChangedFor(nameof(IsNewPersonFormValid))]
        private string lastName = string.Empty;

        /// <summary>
        /// Backing field for the birth date of the person, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute and notifying dependent validation properties.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AreFieldsFilled))]
        [NotifyPropertyChangedFor(nameof(IsNewPersonFormValid))]
        private DateTime birthDate = default;

        /// <summary>
        /// Backing field for the string representation of the person's height, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute and notifying dependent validation properties.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AreFieldsFilled))]
        [NotifyPropertyChangedFor(nameof(IsNewPersonFormValid))]
        private string personHeight = string.Empty;

        /// <summary>
        /// Backing field and generated property for the current validation error message displayed in the user interface, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute.
        /// </summary>
        [ObservableProperty]
        private string validationMessage = string.Empty;

        /// <summary>
        /// Gets a value indicating whether all required input fields have been filled with non-empty and valid data.
        /// </summary>
        public bool AreFieldsFilled => !string.IsNullOrWhiteSpace(FirstName) &&
                                        !string.IsNullOrWhiteSpace(LastName) &&
                                        BirthDate != default &&
                                        double.TryParse(PersonHeight, out double height) && height > 0;

        /// <summary>
        /// Gets a value indicating whether the form for creating a new person is valid and ready for submission.
        /// Mirrors the state of <see cref="AreFieldsFilled"/>.
        /// </summary>
        public bool IsNewPersonFormValid => AreFieldsFilled;

        /// <summary>
        /// Handles changes to the <see cref="FirstName"/> property, validating that the name is not empty, updating the validation message, and re-evaluating the overall form state.
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
        /// Handles changes to the <see cref="LastName"/> property, validating that the name is not empty, updating the validation message, and re-evaluating the overall form state.
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
        /// Handles changes to the <see cref="BirthDate"/> property, validating that a non-default date has been chosen, updating the validation message, and re-evaluating the overall form state.
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
        /// Handles changes to the <see cref="PersonHeight"/> property, validating that the input represents a positive floating-point number, updating the validation message, and re-evaluating the overall form state.
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
        /// Initializes a new instance of the LauncherNewDatabasePersonPageViewModel class, loading database configuration settings, instantiating the database service and new user command, and updating validation state.
        /// </summary>
        public LauncherNewDBPersonViewModel()
        {
            var configurationService = new DatabaseConfigrationService();
            var sqlConfigurationModel = configurationService.LoadConfigurationFile();
            databaseService = new DatabaseService(configurationService.BuildConnectionString());

            NewUserCommand = new AsyncRelayCommand(CreatePersonAsync);

            UpdateValidation();
        }

        /// <summary>
        /// Asynchronously persists the newly entered person data into the database service after a final parsing check on the height, and sends a navigation message via the messenger.
        /// </summary>
        /// <returns>A task that represents the asynchronous database insert operation.</returns>
        private async Task CreatePersonAsync()
        {
            if (double.TryParse(PersonHeight, out var height)) await databaseService.GetPersonCreateSqlAsync(FirstName.Trim(), LastName.Trim(), BirthDate, height);
            else return;

            WeakReferenceMessenger.Default.Send(new NavigationMessage(NavigationMessage.ShowLauncher));
        }

        /// <summary>
        /// Synchronizes the <see cref="ValidationMessage"/> state based on field completion status and forces property change notifications for dependent validation flags to ensure the user interface stays updated.
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

        #region Disposal Pattern

        /// <summary>
        /// Backing field tracking whether the view model instance has already been disposed.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Releases all resources used by the <see cref="LauncherNewDBPersonViewModel"/> class, suppressing finalization.
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