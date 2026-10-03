using BodyTracker.Models;
using BodyTracker.Models.Languages;
using BodyTracker.Models.WorkoutLog;
using BodyTracker.Resources.Languages;
using BodyTracker.Services;
using BodyTracker.Services.App;
using BodyTracker.Services.Localization;
using BodyTracker.Services.WorkoutLog;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using System.Web;
using System.Windows;
using Windows.Devices.SerialCommunication;

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
        /// Gets or sets the collection of available language strings supported by the application interface.
        /// </summary>
        [ObservableProperty]
        public ObservableCollection<string> languagesList;

        /// <summary>
        /// Gets or sets the currently selected language item from the available languages collection.
        /// </summary>
        [ObservableProperty]
        public string selectedLanguagesItem;

        /// <summary>
        /// Tracks whether the view or component is undergoing its initial load sequence to suppress redundant event handling.
        /// </summary>
        private bool firstLoad = true;

        /// <summary>
        /// Gets or sets the identifier or name string representing the active scale user profile.
        /// </summary>
        [ObservableProperty]
        public string scaleUser;

        /// <summary>
        /// Gets or sets the user PIN string utilized for authentication and secure Bluetooth data exchange with the scale.
        /// </summary>
        [ObservableProperty]
        public string scaleUserPin;

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
        /// Tracks whether the application interface language has been modified during the active session.
        /// </summary>
        private bool _isLanguageChanged = false;

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
        /// Gets the asynchronous command that handles changing and persisting the application's interface language setting.
        /// </summary>
        public IAsyncRelayCommand CommandChangeLanguage { get; }

        /// <summary>
        /// Gets the asynchronous command responsible for validating and saving the scale user profile configuration.
        /// </summary>
        public IAsyncRelayCommand CommandSaveScaleUser { get; }

        /// <summary>
        /// Gets the asynchronous command responsible for validating, encrypting or persisting the scale user authentication PIN.
        /// </summary>
        public IAsyncRelayCommand CommandSaveScaleUserPin { get; }

        /// <summary>
        /// Service responsible for managing Hevy API configuration settings and credential encryption.
        /// </summary>
        private HevyAppAPIService hevyAppApiService = new HevyAppAPIService();

        /// <summary>
        /// Service responsible for loading and persisting general application settings.
        /// </summary>
        private AppSettingsService appSettingsService = new AppSettingsService();

        /// <summary>
        /// Backing field for the general error message string.
        /// </summary>
        private string generalInfoMessage = "";

        /// <summary>
        /// Gets or sets the general error message, sending a database error message via the messenger when the value changes.
        /// </summary>
        public string GeneralInfoMessage
        {
            get => generalInfoMessage;
            set
            {
                generalInfoMessage = value;
                WeakReferenceMessenger.Default.Send(new DatabaseErrorMessage(generalInfoMessage));
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="LauncherSettingsViewModel"/> class, 
        /// instantiating the save command, loading persisted configuration credentials, and updating form validation state.
        /// </summary>
        public LauncherSettingsViewModel()
        {
            CommandSaveApiKey = new AsyncRelayCommand(SaveApiKeyAsync);
            CommandSelectDefaultImportFolder = new AsyncRelayCommand(SelectDefaultImportFolder);
            CommandChangeLanguage = new AsyncRelayCommand(ChangeLanguage);
            CommandSaveScaleUser = new AsyncRelayCommand(SaveScaleUserAsync);
            CommandSaveScaleUserPin = new AsyncRelayCommand(SaveScaleUserPinAsync);

            if (firstLoad)
            {

            }

            LanguagesList = new ObservableCollection<string>(CultureCodeModel.Languages());
            if (SelectedLanguagesItem == null || !LanguagesList.Contains(SelectedLanguagesItem))
            {
                SelectedLanguagesItem = LanguagesList?.FirstOrDefault() ?? "";
            }

            var cfgApiKey = hevyAppApiService.LoadConfigurationFile();
            var cfgAppSettings = appSettingsService.LoadConfigurationFile();

            if (string.IsNullOrEmpty(CryptoHelper.Unprotect(cfgApiKey.ApiKey))) GeneralInfoMessage = UiStrings.Error_ApiKeyNotSaved;
            else ApiKey = HevyAppAPIService.GetMaskApiKey(CryptoHelper.Unprotect(cfgApiKey.ApiKey));

            if (string.IsNullOrEmpty(cfgAppSettings.ScaleUser)) GeneralInfoMessage = UiStrings.Error_ScaleUserNotSaved;
            else ScaleUser = cfgAppSettings.ScaleUser;

            if (string.IsNullOrEmpty(CryptoHelper.Unprotect(cfgAppSettings.ScaleUserPin))) GeneralInfoMessage = UiStrings.Error_ScaleUserPinNotSaved;
            else ScaleUserPin = CryptoHelper.Unprotect(cfgAppSettings.ScaleUserPin);

            var settings = appSettingsService.LoadConfigurationFile();
            DefaultImportPath = settings.DefaultImportFolder;
            var lang = settings.Language;

            if (firstLoad)
            {
                LanguagesList = new ObservableCollection<string>(CultureCodeModel.Languages());

                if ((SelectedLanguagesItem == null || !LanguagesList.Contains(SelectedLanguagesItem)) && string.IsNullOrEmpty(lang))
                {
                    SelectedLanguagesItem = LanguagesList?.FirstOrDefault() ?? "";
                }
                else SelectedLanguagesItem = lang;
                firstLoad = false;
            }
        }

        /// <summary>
        /// Asynchronously validates the API key format and content, checks for masking characters, 
        /// updates validation status messages, and persists the encrypted API credentials via the Hevy service.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation.</returns>
        public async Task SaveApiKeyAsync()
        {
            if (!IsAPIKeyValid || ApiKey.Contains("*"))
            {
                GeneralInfoMessage = UiStrings.Validation_ApiKey_Invalid;
                return;
            }

            GeneralInfoMessage = string.Empty;

            hevyAppApiService.SaveCredentials(new HevyAppAPIConfigurationModel() { ApiKey = this.ApiKey });
        }

        /// <summary>
        /// Asynchronously validates the scale user name and PIN input fields, clears validation feedback on success, 
        /// loads the application configuration file, updates the scale user credentials, and saves the configuration state.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation.</returns>
        public async Task SaveScaleUserAsync()
        {
            var _userOK = int.TryParse(ScaleUser, out int user);
            var _pinOK =int.TryParse(ScaleUserPin, out int pin);

            if (user <=0 || user > 10 || !_userOK)
            {
                GeneralInfoMessage = UiStrings.Validation_User_InvalidRange;
                return;
            }

            if (pin < 0  || pin > 9999 || !_pinOK)
            {
                GeneralInfoMessage = UiStrings.Validation_UserPin_InvalidRange;
                return;
            }
            
            GeneralInfoMessage = string.Empty;

            var cfg = appSettingsService.LoadConfigurationFile();
            cfg.ScaleUser = ScaleUser;
            cfg.ScaleUserPin = ScaleUserPin;
            appSettingsService.Save(cfg);
        }

        /// <summary>
        /// Asynchronously validates the scale user PIN input, resets validation messages upon success, 
        /// loads the current application settings, updates the persisted user PIN property, and saves the updated configuration.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SaveScaleUserPinAsync()
        {
            if (string.IsNullOrEmpty(ScaleUserPin))
            {
                GeneralInfoMessage = UiStrings.Validation_UserPin_InvalidRange;
                return;
            }

            GeneralInfoMessage = string.Empty;
            var cfg = appSettingsService.LoadConfigurationFile();
            cfg.ScaleUserPin = ScaleUserPin;
            appSettingsService.Save(cfg);
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
                GeneralInfoMessage += ex.ToString();
            }
        }

        /// <summary>
        /// Asynchronously toggles the application's interface language between English (en-US) 
        /// and German (de-AT) based on the current modification state flag, and updates the localization service.
        /// </summary>
        /// <returns>A task representing the asynchronous language change operation.</returns>
        public async Task ChangeLanguage()
        {
            if (!_isLanguageChanged)
            {
                _isLanguageChanged = !_isLanguageChanged;
                LocalizationService.Instance.ChangeLanguage("en-US");
            }
            else
            {
                _isLanguageChanged = !_isLanguageChanged;
                LocalizationService.Instance.ChangeLanguage("de-AT");
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


        /// <summary>
        /// Handles changes to the selected exercise item by logging the selection and fetching the corresponding workout progress chart data.
        /// </summary>
        /// <param name="value">The name or identifier of the newly selected exercise.</param>
        partial void OnSelectedLanguagesItemChanged(string? value)
        {
            if (!firstLoad)
            {
                var cfg = appSettingsService.LoadConfigurationFile();
                cfg.Language = SelectedLanguagesItem;

                appSettingsService.Save(cfg);

                MessageBoxResult result = MessageBox.Show(
                    UiStrings.Dialog_RestartPromptMessage,
                    UiStrings.Dialog_RestartPromptTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    RestartApplication.Restart();
                }
                else
                {
                    MessageBox.Show(
                        UiStrings.Dialog_RestartRequiredMessage,
                        UiStrings.InformationTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

            }

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

