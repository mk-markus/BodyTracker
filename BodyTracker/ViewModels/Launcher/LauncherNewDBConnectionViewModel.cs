using BodyTracker.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels
{
    /// <summary>
    /// Represents the view model for the launcher new database connection page, managing input properties, form validation rules, credential persistence, and disposal patterns.
    /// </summary>
    public partial class LauncherNewDBConnectionViewModel : ObservableObject
    {
        /// <summary>
        /// Backing field for the SQL Server IP address entered by the user, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute and notifying dependent validation properties.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsIpValid))]
        [NotifyPropertyChangedFor(nameof(IsFormValid))]
        private string sqlIpAddress = string.Empty;

        /// <summary>
        /// Backing field for the SQL Server port entered by the user, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute and notifying dependent validation properties.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPortValid))]
        [NotifyPropertyChangedFor(nameof(IsFormValid))]
        private string sqlPort = string.Empty;

        /// <summary>
        /// Backing field for the SQL Server username entered by the user, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute and notifying dependent validation properties.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AreFieldsFilled))]
        [NotifyPropertyChangedFor(nameof(IsFormValid))]
        private string sqlUsername = string.Empty;

        /// <summary>
        /// Backing field for the SQL Server password entered by the user, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute and notifying dependent validation properties.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AreFieldsFilled))]
        [NotifyPropertyChangedFor(nameof(IsFormValid))]
        private string sqlPassword = string.Empty;

        /// <summary>
        /// Backing field for the SQL Server database name entered by the user, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute and notifying dependent validation properties.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AreFieldsFilled))]
        [NotifyPropertyChangedFor(nameof(IsFormValid))]
        private string sqlDatabaseName = string.Empty;

        /// <summary>
        /// Backing field and generated property for the validation message displayed to the user, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute for change notification.
        /// </summary>
        [ObservableProperty]
        private string validationMessage = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the SQL Server IP address entered by the user is valid.
        /// </summary>
        public bool IsIpValid => IPAddress.TryParse(SqlIpAddress, out _);

        /// <summary>
        /// Gets a value indicating whether the SQL Server port entered by the user is valid.
        /// </summary>
        public bool IsPortValid => int.TryParse(SqlPort, out var p) && p >= 1 && p <= 65535;

        /// <summary>
        /// Gets a value indicating whether all required fields are filled.
        /// </summary>
        public bool AreFieldsFilled =>
            !string.IsNullOrWhiteSpace(SqlUsername) &&
            !string.IsNullOrWhiteSpace(SqlPassword) &&
            !string.IsNullOrWhiteSpace(SqlIpAddress) &&
            !string.IsNullOrWhiteSpace(SqlPort) &&
            !string.IsNullOrWhiteSpace(SqlDatabaseName);

        /// <summary>
        /// Gets a value indicating whether the entire form is valid.
        /// </summary>
        public bool IsFormValid => IsIpValid && IsPortValid && AreFieldsFilled;

        /// <summary>
        /// Gets the command that performs the login operation asynchronously.
        /// </summary>
        public IAsyncRelayCommand CommandLogin { get; }

        /// <summary>
        /// Called when the SQL Server IP address changes, updating validation messages and form state.
        /// </summary>
        /// <param name="value">The new IP address value.</param>
        partial void OnSqlIpAddressChanged(string value)
        {
            if (!IsIpValid) ValidationMessage = "Invalid IP address.";
            else ValidationMessage = string.Empty;
            UpdateValidation();
        }

        /// <summary>
        /// Called when the SQL Server port changes, updating validation messages and form state.
        /// </summary>
        /// <param name="value">The new port value.</param>
        partial void OnSqlPortChanged(string value)
        {
            if (!IsPortValid) ValidationMessage = "Invalid port number (1–65535).";
            else ValidationMessage = string.Empty;
            UpdateValidation();
        }

        /// <summary>
        /// Called when the SQL Server username changes, updating validation messages and form state.
        /// </summary>
        /// <param name="value">The new username value.</param>
        partial void OnSqlUsernameChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) ValidationMessage = "Username cannot be empty.";
            else ValidationMessage = string.Empty;
            UpdateValidation();
        }

        /// <summary>
        /// Called when the SQL Server password changes, updating validation messages and form state.
        /// </summary>
        /// <param name="value">The new password value.</param>
        partial void OnSqlPasswordChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) ValidationMessage = "Password cannot be empty.";
            else ValidationMessage = string.Empty;
            UpdateValidation();
        }

        /// <summary>
        /// Called when the SQL Server database name changes, updating validation messages and form state.
        /// </summary>
        /// <param name="value">The new database name value.</param>
        partial void OnSqlDatabaseNameChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) ValidationMessage = "Database name cannot be empty.";
            else ValidationMessage = string.Empty;
            UpdateValidation();
        }

        /// <summary>
        /// Service responsible for managing application configuration settings, including loading and saving database connection strings from the configuration file.
        /// </summary>
        private DatabaseConfigrationService configurationService = new DatabaseConfigrationService();

        /// <summary>
        /// Initializes a new instance of the LauncherNewDatabaseConnectionPageViewModel class, instantiating the login command, loading persisted configuration credentials, and updating form validation state.
        /// </summary>
        public LauncherNewDBConnectionViewModel()
        {
            CommandLogin = new AsyncRelayCommand(LoginAsync);

            configurationService = new DatabaseConfigrationService();

            var cfg = configurationService.LoadConfigurationFile();

            if (!string.IsNullOrEmpty(cfg.User)) SqlUsername = CryptoHelper.Unprotect(cfg.User);
            if (!string.IsNullOrEmpty(cfg.DatabaseName)) SqlDatabaseName = CryptoHelper.Unprotect(cfg.DatabaseName);
            if (!string.IsNullOrEmpty(cfg.ServerIP)) SqlIpAddress = CryptoHelper.Unprotect(cfg.ServerIP);
            if (!string.IsNullOrEmpty(cfg.PortNumber)) SqlPort = CryptoHelper.Unprotect(cfg.PortNumber);
            UpdateValidation();
        }

        /// <summary>
        /// Validates the form fields, updates the validation message accordingly, and notifies the UI of property changes for validation states.
        /// </summary>
        private void UpdateValidation()
        {
            // Behalte bereits gesetzte feldspezifische Meldungen, ansonsten generische Meldung setzen
            if (!IsFormValid)
            {
                if (string.IsNullOrEmpty(ValidationMessage))
                    ValidationMessage = "All fields must be filled in or corrected.";
            }
            else
            {
                ValidationMessage = string.Empty;
            }

            // Benachrichtige alle abgeleiteten Properties, damit die UI (IsEnabled) aktualisiert wird
            OnPropertyChanged(nameof(IsIpValid));
            OnPropertyChanged(nameof(IsPortValid));
            OnPropertyChanged(nameof(AreFieldsFilled));
            OnPropertyChanged(nameof(IsFormValid));
        }

        /// <summary>
        /// Performs the login operation asynchronously by validating form data, saving credentials to the configuration file, and sending a navigation message via the messenger.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoginAsync()
        {
            if (!IsFormValid)
            {
                ValidationMessage = "The form contains errors.";
                return;
            }

            ValidationMessage = string.Empty;

            configurationService.SaveCredentials(SqlUsername, SqlPassword, SqlIpAddress, Convert.ToInt16(SqlPort), SqlDatabaseName);

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
                    //databaseService?.Dispose();
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