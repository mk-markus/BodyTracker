using BodyTracker.Models;
using BodyTracker.Models.WorkoutLog;
using BodyTracker.Services;
using BodyTracker.Services.WorkoutLog;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace BodyTracker.ViewModels
{
    /// <summary>
    /// Represents the view model for the Hevy API settings page, managing API key configuration, 
    /// form validation rules, credential persistence, and resource disposal patterns.
    /// </summary>
    public partial class LauncherSettingsViewModel : ObservableObject, IDisposable
    {
        /// <summary>
        /// Backing field for the Hevy API key entered by the user, utilizing the CommunityToolkit.Mvvm ObservableProperty attribute 
        /// and notifying dependent validation properties.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAPIKeyValid))]
        [NotifyPropertyChangedFor(nameof(HasApiKeyError))]
        private string apiKey = string.Empty;

        /// <summary>
        /// Gets or sets the validation error message associated with the API key text input.
        /// </summary>
        [ObservableProperty]
        public string apiKeyError = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the current API key string matches the standard UUID format.
        /// </summary>
        public bool IsAPIKeyValid => Regex.IsMatch(ApiKey, @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$");

        /// <summary>
        /// Gets a value indicating whether an API key validation error is present.
        /// </summary>
        public bool HasApiKeyError => !IsAPIKeyValid;

        /// <summary>
        /// Called when the API key changes, updating validation messages and form state.
        /// </summary>
        /// <param name="value">The new API key value.</param>
        partial void OnApiKeyChanged(string value)
        {
            if (string.IsNullOrEmpty(ApiKey)) ApiKeyError = "Please enter a correct API Key";
            else if (!IsAPIKeyValid) ApiKeyError = "API Key Syntax wrong";
            else ApiKeyError = string.Empty;
        }

        /// <summary>
        /// Backing field for the shortened display path of the default import directory.
        /// </summary>
        [ObservableProperty]
        private string defaultImportShortPath = string.Empty;

        /// <summary>
        /// Backing field for the full file system path of the default import directory.
        /// </summary>
        private string defaultImportPath = string.Empty;

        /// <summary>
        /// Gets or sets the full file system path of the default import directory, automatically updating the compact short path representation.
        /// </summary>
        public string DefaultImportPath
        {
            get { return defaultImportPath; }
            set
            {
                defaultImportPath = value;
                DefaultImportShortPath = CompactPath(value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the specified default import path is structurally valid.
        /// </summary>
        public bool IsDefaultImportPathValid => CheckingPathOK(defaultImportPath);

        /// <summary>
        /// Backing field and generated property for the validation message displayed to the user, 
        /// utilizing the CommunityToolkit.Mvvm ObservableProperty attribute for change notification.
        /// </summary>
        [ObservableProperty]
        private string validationMessage = string.Empty;

        /// <summary>
        /// Gets the command that performs the save operation asynchronously.
        /// </summary>
        public IAsyncRelayCommand CommandSaveApiKey { get; }

        /// <summary>
        /// Gets the command that opens a folder selection dialog to configure the default import directory asynchronously.
        /// </summary>
        public IAsyncRelayCommand CommandSelectDefaultImportFolder { get; }

        /// <summary>
        /// Service responsible for managing Hevy API configuration settings and credential encryption.
        /// </summary>
        private HevyAppAPIService hevyAppApiService = new HevyAppAPIService();

        /// <summary>
        /// Service responsible for loading and persisting general application settings.
        /// </summary>
        private AppSettingsService appSettingsService = new AppSettingsService();


        /// <summary>
        /// Initializes a new instance of the <see cref="LauncherSettingsViewModel"/> class, 
        /// instantiating the save command, loading persisted configuration credentials, and updating form validation state.
        /// </summary>
        public LauncherSettingsViewModel()
        {
            CommandSaveApiKey = new AsyncRelayCommand(SaveApiKeyAsync);
            CommandSelectDefaultImportFolder = new AsyncRelayCommand(SelectDefaultImportFolder);

            var cfgApiKey = hevyAppApiService.LoadConfigurationFile();

            if (string.IsNullOrEmpty(CryptoHelper.Unprotect(cfgApiKey.ApiKey))) ValidationMessage = "NO API KEY SAVED";
            else ApiKey = HevyAppAPIService.GetMaskApiKey(CryptoHelper.Unprotect(cfgApiKey.ApiKey));

            var settings = appSettingsService.LoadConfigurationFile();
            DefaultImportPath = settings.DefaultImportFolder;

        }

        /// <summary>
        /// Performs the save operation asynchronously by validating form data, saving encrypted credentials to the configuration file, 
        /// and sending a navigation message via the messenger.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SaveApiKeyAsync()
        {
            if (!IsAPIKeyValid || ApiKey.Contains("*"))
            {
                ValidationMessage = "Please enter a correct API Key";
                return;
            }

            ValidationMessage = string.Empty;

            hevyAppApiService.SaveCredentials(new HevyAppAPIConfigurationModel() { ApiKey = this.ApiKey });

            WeakReferenceMessenger.Default.Send(new NavigationMessage(NavigationMessage.ShowLauncher));
        }

        /// <summary>
        /// Asynchronously prompts the user to select a default import folder using a folder browser dialog and persists the selection.
        /// </summary>
        /// <returns>A task representing the asynchronous folder selection operation.</returns>
        public async Task SelectDefaultImportFolder()
        {
            try
            {
                var dlg = new OpenFolderDialog();

                if (dlg.ShowDialog() == true)
                {
                    DefaultImportPath = dlg.FolderName;

                    var cfg = appSettingsService.LoadConfigurationFile();
                    cfg.DefaultImportFolder = DefaultImportPath;

                    appSettingsService.Save(cfg);
                }
                else
                {
                    DefaultImportShortPath = string.Empty;
                    defaultImportPath = string.Empty;
                }

            }
            catch (Exception ex)
            {
                ValidationMessage += ex.ToString();
            }
        }

        /// <summary>
        /// Compresses a long directory path into a truncated format for user-friendly UI display.
        /// </summary>
        /// <param name="path">The full file system path to compress.</param>
        /// <returns>A compacted path string containing the root directories and the final folder name.</returns>
        public static string CompactPath(string path)
        {
            string[] parts = path.Split('\\');

            if (parts.Length <= 4)
                return path;

            return $"{parts[0]}\\{parts[1]}\\...\\{parts[^1]}";
        }

        /// <summary>
        /// Verifies whether a given string represents a structurally valid file system path.
        /// </summary>
        /// <param name="path">The path string to check.</param>
        /// <returns><c>true</c> if the path is valid; otherwise, <c>false</c>.</returns>
        private bool CheckingPathOK(string path)
        {
            try
            {
                Path.GetFullPath(path);
                return true;
            }
            catch { return false; }
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