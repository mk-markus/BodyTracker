using BodyTracker.Models;
using System;
using System.IO;
using System.Net;
using System.Text.Json;

namespace BodyTracker.Services
{
    /// <summary>
    /// Service for managing application configurations, including loading, saving, 
    /// and encrypting database credentials in a JSON file.
    /// </summary>
    public class DatabaseConfigrationService
    {
        /// <summary>
        /// Gets the absolute path to the appsettings.json file. 
        /// </summary>
        public string   AppSettingsPath {  get; private set; } = string.Empty;

        /// <summary>
        /// The default directory path for application settings, 
        /// using environment variables for user-specific local storage.
        /// </summary>
        private string DefautlPath = @"%Userprofile%\AppData\Local\BodyTracker";

        /// <summary>
        /// The default directory path for certificate file.
        /// </summary>
        private string CertPath = string.Empty ;

        /// <summary>
        /// Gets a value indicating whether a valid configuration file was found or created.
        /// </summary>
        public bool     PathOK {  get; private set; } = false;

        /// <summary>
        /// Gets the database username retrieved from the configuration.
        /// This property is used to identify the user for the SQL server authentication.
        /// </summary>
        public string   User {  get; private set; } = string.Empty ;

        /// <summary>
        /// Stores the decrypted or plain-text password temporarily during the connection string assembly.
        /// This field is private to prevent exposure of sensitive data outside of the service.
        /// </summary>
        private string  Passwort = string.Empty;

        /// <summary>
        /// Gets the name of the target database schema.
        /// This property defines which specific database on the server the application will interact with.
        /// </summary>
        public string   Databse { get; private set; } = string.Empty;

        //// <summary>
        /// Gets the network port number used for the SQL server connection.
        /// The default value is 3306 for MySQL/MariaDB environments.
        /// </summary>
        public int      PortNumber { get; private set; } = 3306;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConfigrationService"/>.
        /// Validates the provided path or falls back to the default application directory.
        /// </summary>
        /// <param name="sAppSettingsPath">The preferred path to the configuration file.</param>
        /// <exception cref="ArgumentException">Thrown if no valid configuration file can be accessed.</exception>
        public DatabaseConfigrationService()
        { 
            // Check if path is empty than use an default path. 
            (PathOK, this.AppSettingsPath) = ConfigFileAvialable(DefautlPath, "appsettings.json");
            CertPath = GetFullFilePath(DefautlPath, "ca.pem");
         
            if(!PathOK) throw new ArgumentException("The app setting file does not exist at: " + AppSettingsPath);
         
        }

        /// <summary>
        /// Loads the SQL configuration from the JSON settings file. 
        /// If the file is empty or the deserialization fails, a new instance of <see cref="SQLConfigurationModel"/> is returned.
        /// </summary>
        /// <returns>
        /// The deserialized <see cref="SQLConfigurationModel"/> containing database settings and encryption keys.
        /// </returns>
        /// <remarks>
        /// This method reads the entire content of the file specified in <see cref="AppSettingsPath"/>.
        /// Ensure that the path is validated before calling this method to avoid file system exceptions.
        /// </remarks>
        public SQLConfigurationModel LoadConfigurationFile()
        {
            return JsonSerializer.Deserialize<SQLConfigurationModel>(File.ReadAllText(AppSettingsPath)) ?? new SQLConfigurationModel();
        }

        /// <summary>
        /// Persists the provided configuration model to the JSON settings file.
        /// </summary>
        /// <param name="cfg">The <see cref="SQLConfigurationModel"/> instance containing the updated settings to be saved.</param>
        /// <remarks>
        /// This method serializes the object into a human-readable JSON format (indented) 
        /// and overwrites the existing content of the file at <see cref="AppSettingsPath"/>.
        /// </remarks>
        public void Save(SQLConfigurationModel cfg)
        {
            var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions{ WriteIndented = true });
            File.WriteAllText(AppSettingsPath, json);
        }

        /// <summary>
        /// Updates and persists the database connection credentials. 
        /// It ensures that encryption components (Key and IV) are present, encrypts the sensitive 
        /// password, and updates all server-related configuration parameters.
        /// </summary>
        /// <param name="user">The database username to be stored.</param>
        /// <param name="plainPassword">The unencrypted password provided by the user.</param>
        /// <param name="ServerIP">The network address (IP or hostname) of the database server.</param>
        /// <param name="PortNumber">The port number for the SQL connection (e.g., 3306).</param>
        /// <param name="datbase">The specific database name to connect to.</param>
        /// <remarks>
        /// This method follows a "LoadCinfigurationFile-Modify-Save" pattern. It lazily initializes encryption keys 
        /// using the <see cref="CryptoHelper"/> only when they are missing, ensuring that existing 
        /// keys are preserved for consistent decryption.
        /// </remarks>
        public void SaveCredentials(string user, string plainPassword, string ServerIP, int PortNumber, string datbase)
        {
            var cfg = LoadConfigurationFile();
            //if (string.IsNullOrEmpty(cfg.Key) || string.IsNullOrEmpty(cfg.IV))
            //{
            //    var (key, iv) = CryptoHelper.GenerateKeyIv();
            //    cfg.Key = key; cfg.IV = iv;
            //}
            cfg.User = CryptoHelper.Protect(user);
            cfg.PasswordEnc = CryptoHelper.Protect(plainPassword);
            cfg.ServerIP = CryptoHelper.Protect(ServerIP); 
            cfg.PortNumber = CryptoHelper.Protect(PortNumber.ToString());
            cfg.DatabaseName = CryptoHelper.Protect(datbase);
            Save(cfg);
        }

        /// <summary>
        /// Retrieves the stored configuration and assembles a complete MySQL connection string.
        /// It automatically decrypts the stored password using the associated Key and IV.
        /// </summary>
        /// <returns>
        /// A formatted string containing all necessary parameters (Server, Port, Database, Uid, Pwd) 
        /// to initialize a <see cref="MySqlConnection"/>.
        /// </returns>
        /// <remarks>
        /// This method uses 'SslMode=Preferred' to attempt an encrypted connection to the server 
        /// if supported, while maintaining compatibility with standard configurations. 
        /// If no encrypted password is found, an empty password is used for the string.
        /// </remarks>
        public string BuildConnectionString()
        {
            var cfg = LoadConfigurationFile();
            var pwd = string.IsNullOrEmpty(cfg.PasswordEnc) ? "" : CryptoHelper.Unprotect(cfg.PasswordEnc);
            return $"Server={CryptoHelper.Unprotect(cfg.ServerIP)};" +
                    $"Port={CryptoHelper.Unprotect(cfg.PortNumber)};" +
                    $"Database={CryptoHelper.Unprotect(cfg.DatabaseName)};" +
                    $"Uid={CryptoHelper.Unprotect(cfg.User)};" +
                    $"Pwd={pwd};" +
                    $"SslMode=VerifyCA;" +
                    $"AllowLoadLocalInfile = true;" +
                    $"SslCa={CertPath};";
        }

        /// <summary>
        /// Ensures the configuration directory exists and validates the presence of the settings file.
        /// </summary>
        /// <param name="directoryPath">
        /// The File Path needs the structure like %UserProfile\AppData\BodyTracker without the file name !!!!
        /// The target directory path. If null or empty, the method falls back to <see cref="DefautlPath"/>.
        /// Supports environment variables (e.g., %UserProfile%).
        /// </param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description><c>bool</c>: True if the file exists or was successfully generated; otherwise, false.</description></item>
        /// <item><description><c>string</c>: The absolute file path to 'appsettings.json' if successful, or an empty string.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method resolves environment variables, creates missing directories, and 
        /// automatically triggers <see cref="CreateAppSettingsFile"/> if the file is missing.
        /// </remarks>
        public (bool, string) ConfigFileAvialable(string directoryPath, string filename)
        {
            string filePath= string.Empty;

            // Check file Path exist when not create filepath
            if (!string.IsNullOrEmpty(directoryPath)) filePath = Environment.ExpandEnvironmentVariables(directoryPath);
            else filePath = Environment.ExpandEnvironmentVariables(DefautlPath);


            // Check Directory exist
            if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
         
            
            filePath = Path.Combine(filePath, filename);

            // Check if app settings availvaible. If so, return the available file.
            if (File.Exists(filePath)) return (true, filePath);
                
            // if no file is available a new app settings file will be created.
            else
            {
                CreateAppSettingsFile(filePath);
                if (File.Exists(filePath)) return (true, filePath);
                else return (false, string.Empty);
            }
        }


        public string GetFullFilePath(string directoryPath, string filename)
        {
            string filePath = string.Empty;

            // Check file Path exist when not create filepath
            if (!string.IsNullOrEmpty(directoryPath)) filePath = Environment.ExpandEnvironmentVariables(directoryPath);
            else filePath = Environment.ExpandEnvironmentVariables(DefautlPath);

            filePath = Path.Combine(filePath, filename);

            // Check if app settings availvaible. If so, return the available file.
            if (File.Exists(filePath)) return filePath;
            else return string.Empty;
        }


        /// <summary>
        /// Creates a new 'appsettings.json' template file with default values in the current application directory.
        /// </summary>
        /// <remarks>
        /// This method initializes a <see cref="SQLConfigurationModel"/> with empty strings and the 
        /// default MySQL port (3306). The resulting JSON is indented for better human readability.
        /// Note: Ensure the application has write permissions for the target directory.
        /// </remarks>
        public void CreateAppSettingsFile(string filePath)
        {

            var config = new SQLConfigurationModel();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true // Generate an json File with correct format
            };

            // generate json string
            string jsonString = JsonSerializer.Serialize(config, options);

            // create json file
            File.WriteAllText(filePath, jsonString);
        }


        /// <summary>
        /// Validates a string as a network address and identifies the IP version.
        /// </summary>
        /// <param name="IP">The string representation of the IP address to validate.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description><c>bool</c>: True if the address is a valid IPv4 or IPv6 address.</description></item>
        /// <item><description><c>int</c>: Version indicator (0 = invalid, 4 = IPv4, 6 = IPv6).</description></item>
        /// <item><description><c>IPAddress?</c>: The parsed <see cref="IPAddress"/> object, or null if invalid.</description></item>
        /// </list>
        /// </returns>
        public (bool, int, IPAddress?) IPAdressOK(string IP)
        {
            if (string.IsNullOrEmpty(IP)) return (false, 0, null);
            if(IPAddress.TryParse(IP, out IPAddress address))
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) return (true, 4, address);
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) return (true, 6, address);
                else return (false, 0, null);
            }
            else return (false, 0, null);
        }

    }
}
