using BodyTracker.Models.WorkoutLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace BodyTracker.Services.WorkoutLog
{
    /// <summary>
    /// Manages communication with the Hevy REST API, handles paginated workout data retrieval, 
    /// and controls local configuration file storage and API key encryption.
    /// </summary>
    public class HevyAppAPIService
    {
        /// <summary>
        /// Gets the absolute path to the configuration settings file. 
        /// </summary>
        public string AppSettingsPath { get; private set; } = string.Empty;

        /// <summary>
        /// The default directory path for application settings, utilizing user profile environment variables for local storage.
        /// </summary>
        private string DefautlPath = @"%Userprofile%\AppData\Local\BodyTracker";

        /// <summary>
        /// The base uniform resource identifier for the Hevy web service.
        /// </summary>
        private string baseAdress = "https://api.hevyapp.com";

        /// <summary>
        /// The API endpoint path designated for retrieving workout sessions.
        /// </summary>
        private string workoutsEndpoint = "/v1/workouts";

        /// <summary>
        /// The HTTP client instance utilized for executing network requests.
        /// </summary>
        private HttpClient _httpClient;

        /// <summary>
        /// Gets a value indicating whether the configuration file and directory path were successfully validated or created.
        /// </summary>
        public bool PathOK { get; private set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="HevyAppAPIService"/> class and verifies the availability of the configuration file.
        /// </summary>
        public HevyAppAPIService()
        {
            (PathOK, AppSettingsPath) = ConfigFileAvialable(DefautlPath, "HeavyAppSettings.json");
        }

        /// <summary>
        /// Asynchronously fetches and aggregates all workout entries from the Hevy API, automatically handling multi-page pagination.
        /// </summary>
        /// <param name="apiKey">Optional API key string. If omitted, credentials are loaded and decrypted from the local configuration file.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of deserialized <see cref="HevyAppWorkoutJsonModel"/> objects.</returns>
        public async Task<List<HevyAppWorkoutJsonModel>> FetchAndProcessWorkoutsAsync(string apiKey = null)
        {
            var _workouts = new List<HevyAppWorkoutJsonModel>();

            try
            {
                if (string.IsNullOrEmpty(apiKey))
                {
                    var cfg = LoadConfigurationFile();
                    apiKey = CryptoHelper.Unprotect(cfg.ApiKey);
                    if (string.IsNullOrEmpty(apiKey)) throw new Exception("API Key is not set in the configuration.");
                }

                _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(baseAdress);
                _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

                HttpResponseMessage response = await _httpClient.GetAsync(workoutsEndpoint);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    var firstResponse = JsonSerializer.Deserialize<HevyAppWorkoutResponseJsonModel>(jsonResponse, options);

                    if (firstResponse != null)
                    {
                        _workouts.AddRange(firstResponse.Workouts);

                        for (int i = firstResponse.Page + 1; i <= firstResponse.PageCount; i++)
                        {
                            HttpResponseMessage nextPageResponse = await _httpClient.GetAsync($"{workoutsEndpoint}?page={i}");
                            if (nextPageResponse.IsSuccessStatusCode)
                            {
                                string pageJsonResponse = await nextPageResponse.Content.ReadAsStringAsync();
                                var pageExercises = JsonSerializer.Deserialize<HevyAppWorkoutResponseJsonModel>(pageJsonResponse, options);
                                if (pageExercises != null && pageExercises.Workouts != null)
                                {
                                    _workouts.AddRange(pageExercises.Workouts);
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception(response.StatusCode.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error fetching workouts: " + ex.Message);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            finally
            {
                _httpClient?.Dispose();
            }

            return _workouts;
        }

        /// <summary>
        /// Loads the Hevy API configuration from the JSON settings file. 
        /// If the file is empty or deserialization fails, a new instance of <see cref="HevyAppAPIConfigurationModel"/> is returned.
        /// </summary>
        /// <returns>
        /// The deserialized <see cref="HevyAppAPIConfigurationModel"/> containing API settings and encryption keys.
        /// </returns>
        /// <remarks>
        /// This method reads the entire text content from the path specified in <see cref="AppSettingsPath"/>.
        /// </remarks>
        public HevyAppAPIConfigurationModel LoadConfigurationFile()
        {
            return JsonSerializer.Deserialize<HevyAppAPIConfigurationModel>(File.ReadAllText(AppSettingsPath)) ?? new HevyAppAPIConfigurationModel();
        }

        /// <summary>
        /// Persists the provided configuration model to the JSON settings file on disk.
        /// </summary>
        /// <param name="cfg">The <see cref="HevyAppAPIConfigurationModel"/> instance containing the updated settings to be saved.</param>
        /// <remarks>
        /// The object is serialized into an indented, human-readable JSON format, overwriting existing file content at <see cref="AppSettingsPath"/>.
        /// </remarks>
        public void Save(HevyAppAPIConfigurationModel cfg)
        {
            var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(AppSettingsPath, json);
        }

        /// <summary>
        /// Updates and persists the API configuration credentials. 
        /// It loads the existing configuration, protects and encrypts the sensitive API key via <see cref="CryptoHelper"/>, and saves the updated state.
        /// </summary>
        /// <param name="newConfig">The <see cref="HevyAppAPIConfigurationModel"/> instance containing the new unencrypted settings to be stored.</param>
        public void SaveCredentials(HevyAppAPIConfigurationModel newConfig)
        {
            var cfg = LoadConfigurationFile();

            cfg.ApiKey = CryptoHelper.Protect(newConfig.ApiKey);
            Save(cfg);
        }

        /// <summary>
        /// Ensures the target configuration directory exists and validates the presence of the specified settings file.
        /// </summary>
        /// <param name="directoryPath">The target directory path. Falls back to <see cref="DefautlPath"/> if null or empty. Supports environment variables.</param>
        /// <param name="filename">The name of the configuration file.</param>
        /// <returns>
        /// A tuple containing a boolean success indicator and the absolute file path if successful, or an empty string otherwise.
        /// </returns>
        public (bool, string) ConfigFileAvialable(string directoryPath, string filename)
        {
            string filePath = string.Empty;

            if (!string.IsNullOrEmpty(directoryPath)) filePath = Environment.ExpandEnvironmentVariables(directoryPath);
            else filePath = Environment.ExpandEnvironmentVariables(DefautlPath);

            if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);

            filePath = Path.Combine(filePath, filename);

            if (File.Exists(filePath)) return (true, filePath);
            else
            {
                CreateAPISettingsFile(filePath);
                if (File.Exists(filePath)) return (true, filePath);
                else return (false, string.Empty);
            }
        }

        /// <summary>
        /// Retrieves the full absolute file path for the settings file if it exists; otherwise, returns an empty string.
        /// </summary>
        /// <param name="directoryPath">The target directory path.</param>
        /// <param name="filename">The name of the settings file.</param>
        /// <returns>The absolute file path if found; otherwise, an empty string.</returns>
        public string GetFullFilePath(string directoryPath, string filename)
        {
            string filePath = string.Empty;

            if (!string.IsNullOrEmpty(directoryPath)) filePath = Environment.ExpandEnvironmentVariables(directoryPath);
            else filePath = Environment.ExpandEnvironmentVariables(DefautlPath);

            filePath = Path.Combine(filePath, filename);

            if (File.Exists(filePath)) return filePath;
            else return string.Empty;
        }

        /// <summary>
        /// Creates a new API settings template file initialized with default values at the specified path.
        /// </summary>
        /// <param name="filePath">The absolute file path where the settings file should be generated.</param>
        /// <remarks>
        /// Initializes a <see cref="HevyAppAPIConfigurationModel"/> with default parameters and writes an indented JSON structure to disk.
        /// </remarks>
        public void CreateAPISettingsFile(string filePath)
        {
            var config = new HevyAppAPIConfigurationModel();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string jsonString = JsonSerializer.Serialize(config, options);
            File.WriteAllText(filePath, jsonString);
        }
    }
}