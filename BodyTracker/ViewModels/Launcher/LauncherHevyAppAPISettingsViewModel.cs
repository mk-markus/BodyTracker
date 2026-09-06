using BodyTracker.Models.WorkoutLog;
using BodyTracker.Services;
using BodyTracker.Services.WorkoutLog;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels
{
    /// <summary>
    /// Represents the view model for the Hevy API settings page, managing API key configuration, 
    /// form validation rules, credential persistence, and resource disposal patterns.
    /// </summary>
    public partial class LauncherHevyAppAPISettingsViewModel : ObservableObject, IDisposable
    {
        /// <summary>
        /// Backing field for the Hevy API key entered by the user, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute 
        /// and notifying dependent validation properties.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFormValid))]
        private string apiKey = string.Empty;

        /// <summary>
        /// Backing field and generated property for the validation message displayed to the user, 
        /// utilizing the CommunityToolkit.Mvvm ObservableProperty attribute for change notification.
        /// </summary>
        [ObservableProperty]
        private string validationMessage = string.Empty;

        /// <summary>
        /// Gets a value indicating whether all required fields are filled.
        /// </summary>
        public bool AreFieldsFilled => !string.IsNullOrWhiteSpace(ApiKey);

        /// <summary>
        /// Gets a value indicating whether the entire form is valid.
        /// </summary>
        public bool IsFormValid => AreFieldsFilled;

        /// <summary>
        /// Gets the command that performs the save operation asynchronously.
        /// </summary>
        public IAsyncRelayCommand CommandSave { get; }

        /// <summary>
        /// Called when the API key changes, updating validation messages and form state.
        /// </summary>
        /// <param name="value">The new API key value.</param>
        partial void OnApiKeyChanged(string value)
        {
            if (string.IsNullOrEmpty(ApiKey)) ValidationMessage = "Invalid API-KEY.";
            else ValidationMessage = string.Empty;
            UpdateValidation();
        }

        /// <summary>
        /// Service responsible for managing Hevy API configuration settings and credential encryption.
        /// </summary>
        private HevyAppAPIService hevyAppApiService = new HevyAppAPIService();

        /// <summary>
        /// Initializes a new instance of the <see cref="LauncherHevyAppAPISettingsViewModel"/> class, 
        /// instantiating the save command, loading persisted configuration credentials, and updating form validation state.
        /// </summary>
        public LauncherHevyAppAPISettingsViewModel()
        {
            CommandSave = new AsyncRelayCommand(SaveAsync);

            hevyAppApiService = new HevyAppAPIService();

            var cfg = hevyAppApiService.LoadConfigurationFile();

            if (string.IsNullOrEmpty(CryptoHelper.Unprotect(cfg.ApiKey))) ApiKey = "API KEY NOT FOUND - ENTER KEY";
            else ApiKey = "API KEY EXIST";

            UpdateValidation();
        }

        /// <summary>
        /// Validates the form fields, updates the validation message accordingly, and notifies the UI of property changes for validation states.
        /// </summary>
        private void UpdateValidation()
        {
            if (!IsFormValid)
            {
                if (string.IsNullOrEmpty(ValidationMessage))
                    ValidationMessage = "All fields must be filled in or corrected.";
            }
            else
            {
                ValidationMessage = string.Empty;
            }

            OnPropertyChanged(nameof(AreFieldsFilled));
        }

        /// <summary>
        /// Performs the save operation asynchronously by validating form data, saving encrypted credentials to the configuration file, 
        /// and sending a navigation message via the messenger.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SaveAsync()
        {
            if (!IsFormValid)
            {
                ValidationMessage = "The form contains errors.";
                return;
            }

            ValidationMessage = string.Empty;

            hevyAppApiService.SaveCredentials(new HevyAppAPIConfigurationModel() { ApiKey = this.ApiKey });

            WeakReferenceMessenger.Default.Send(new NavigationMessage(NavigationMessage.ShowLauncher));
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
        /// Protected implementation of the Dispose pattern, releasing managed resources when disposing is true.
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
                    // Clean up managed resources if necessary (e.g., event subscriptions or services)
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