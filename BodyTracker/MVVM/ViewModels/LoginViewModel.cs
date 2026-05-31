using BodyTracker.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BodyTracker.MVVM.ViewModels
{
    partial class LoginViewModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets the SQL Server IP address entered by the user.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsIpValid))]
        [NotifyPropertyChangedFor(nameof(IsFormValid))]
        private string sqlIpAddress = string.Empty;

        /// <summary>
        /// Gets or sets the SQL Server port entered by the user.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPortValid))]
        [NotifyPropertyChangedFor(nameof(IsFormValid))]
        private string sqlPort = string.Empty;
        
        /// <summary>
        /// Gets or sets the SQL Server username entered by the user.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AreFieldsFilled))]
        [NotifyPropertyChangedFor(nameof(IsFormValid))]
        private string sqlUsername = string.Empty;
        
        /// <summary>
        /// Gets or sets the SQL Server password entered by the user.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AreFieldsFilled))]
        [NotifyPropertyChangedFor(nameof(IsFormValid))]
        private string sqlPassword = string.Empty;
        
        /// <summary>
        /// Gets or sets the SQL Server database name entered by the user.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AreFieldsFilled))]
        [NotifyPropertyChangedFor(nameof(IsFormValid))]
        private string sqlDatabaseName = string.Empty;

        /// <summary>
        /// Gets or sets the validation message displayed to the user.
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
        public IAsyncRelayCommand LoginCommand { get; }

        /// <summary>
        /// Called when the SQL Server IP address changes.
        /// </summary>
        /// <param name="value">The new IP address value.</param>
        partial void OnSqlIpAddressChanged(string value)
        {
            UpdateValidation();
        }

        /// <summary>
        /// Called when the SQL Server port changes.
        /// </summary>
        /// <param name="value"></param>
        partial void OnSqlPortChanged(string value)
        {
            UpdateValidation();
        }

        /// <summary>
        /// Called when the SQL Server username changes.
        /// </summary>
        /// <param name="value">The new username value.</param>
        partial void OnSqlUsernameChanged(string value)
        {
            UpdateValidation();
        }

        /// <summary>
        /// Called when the SQL Server password changes.
        /// </summary>
        /// <param name="value">The new password value.</param>
        partial void OnSqlPasswordChanged(string value)
        {
            UpdateValidation();
        }

        /// <summary>
        /// Called when the SQL Server database name changes.
        /// </summary>
        /// <param name="value">The new database name value.</param>
        partial void OnSqlDatabaseNameChanged(string value)
        {
            UpdateValidation();
        }

        /// <summary>
        /// Service responsible for managing application configuration settings, 
        /// including loading and saving database connection strings from the configuration file.
        /// </summary>
        private ConfigrationService configurationService = new ConfigrationService();


        /// <summary>
        /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
        /// </summary>
        public LoginViewModel()
        {
            LoginCommand = new AsyncRelayCommand(LoginAsync);

            configurationService = new ConfigrationService();

            var cfg = configurationService.LoadConfigurationFile();

            if (!string.IsNullOrEmpty(cfg.User)) SqlUsername = CryptoHelper.Unprotect(cfg.User);
            if (!string.IsNullOrEmpty(cfg.DatabaseName)) SqlDatabaseName= CryptoHelper.Unprotect(cfg.DatabaseName);
            if (!string.IsNullOrEmpty(cfg.ServerIP)) SqlIpAddress = CryptoHelper.Unprotect(cfg.ServerIP);
             if (!string.IsNullOrEmpty(cfg.PortNumber)) SqlPort = CryptoHelper.Unprotect(cfg.PortNumber);

        }

        /// <summary>
        /// Validates the form fields and updates the validation message accordingly.
        /// </summary>
        private void UpdateValidation()
        {
            if (string.IsNullOrWhiteSpace(SqlIpAddress) || string.IsNullOrWhiteSpace(SqlPort) ||
                string.IsNullOrWhiteSpace(SqlUsername) || string.IsNullOrWhiteSpace(SqlPassword) ||
                string.IsNullOrWhiteSpace(SqlDatabaseName)
                )
            {
                ValidationMessage = "All fields must be filled in.";
            }
            else if (!IsIpValid)
            {
                ValidationMessage = "Invalid IP address.";
            }
            else if (!IsPortValid)
            {
                ValidationMessage = "Invalid port number (1–65535).";
            }
            else
            {
                ValidationMessage = string.Empty;
            }

            OnPropertyChanged(nameof(IsFormValid));
        }

        /// <summary>
        /// Performs the login operation asynchronously.
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

        }
    }
}