using BodyTracker.MVVM.Models;
using System;
using System.IO;
using System.Text.Json;

namespace BodyTracker.Services
{
    /// <summary>
    /// Service for managing application configurations, including loading, saving, 
    /// and encrypting database credentials in a JSON file.
    /// </summary>
    public class ConfigrationService
    {
        /// <summary>
        /// Gets the absolute path to the appsettings.json file.
        /// </summary>
        public string   sAppSettingsPath {  get; private set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether a valid configuration file was found or created.
        /// </summary>
        public bool     bPathOK {  get; private set; } = false;

        /// <summary>
        /// Gets the database username retrieved from the configuration.
        /// This property is used to identify the user for the SQL server authentication.
        /// </summary>
        public string   sUser {  get; private set; } = string.Empty ;

        /// <summary>
        /// Stores the decrypted or plain-text password temporarily during the connection string assembly.
        /// This field is private to prevent exposure of sensitive data outside of the service.
        /// </summary>
        private string  Passwort = string.Empty;

        /// <summary>
        /// Gets the name of the target database schema.
        /// This property defines which specific database on the server the application will interact with.
        /// </summary>
        public string   sDatabse { get; private set; } = string.Empty;

        //// <summary>
        /// Gets the network port number used for the SQL server connection.
        /// The default value is 3306 for MySQL/MariaDB environments.
        /// </summary>
        public int      iPortNumber { get; private set; } = 3306;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigrationService"/>.
        /// Validates the provided path or falls back to the default application directory.
        /// </summary>
        /// <param name="sAppSettingsPath">The preferred path to the configuration file.</param>
        /// <exception cref="ArgumentException">Thrown if no valid configuration file can be accessed.</exception>
        public ConfigrationService(string sAppSettingsPath)
        {
            // Check if path is empty than use an default path. 
            (bPathOK, this.sAppSettingsPath) = checkConfigFileAvialable(sAppSettingsPath);
         
            if(!bPathOK) throw new ArgumentException("The app setting file does not exist at: " + sAppSettingsPath);
            
        }

        /// <summary>
        /// Loads the SQL configuration from the JSON settings file. 
        /// If the file is empty or the deserialization fails, a new instance of <see cref="SQLConfigurationModel"/> is returned.
        /// </summary>
        /// <returns>
        /// The deserialized <see cref="SQLConfigurationModel"/> containing database settings and encryption keys.
        /// </returns>
        /// <remarks>
        /// This method reads the entire content of the file specified in <see cref="sAppSettingsPath"/>.
        /// Ensure that the path is validated before calling this method to avoid file system exceptions.
        /// </remarks>
        public SQLConfigurationModel LoadCinfigurationFile()
        {
            return JsonSerializer.Deserialize<SQLConfigurationModel>(File.ReadAllText(sAppSettingsPath)) ?? new SQLConfigurationModel();
        }

        /// <summary>
        /// Persists the provided configuration model to the JSON settings file.
        /// </summary>
        /// <param name="cfg">The <see cref="SQLConfigurationModel"/> instance containing the updated settings to be saved.</param>
        /// <remarks>
        /// This method serializes the object into a human-readable JSON format (indented) 
        /// and overwrites the existing content of the file at <see cref="sAppSettingsPath"/>.
        /// </remarks>
        public void Save(SQLConfigurationModel cfg)
        {
            var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions{ WriteIndented = true });
            File.WriteAllText(sAppSettingsPath, json);
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
            var cfg = LoadCinfigurationFile();
            if (string.IsNullOrEmpty(cfg.Key) || string.IsNullOrEmpty(cfg.IV))
            {
                var (key, iv) = CryptoHelper.GenerateKeyIv();
                cfg.Key = key; cfg.IV = iv;
            }
            cfg.User = user;
            cfg.PasswordEnc = CryptoHelper.Encrypt(plainPassword, cfg.Key, cfg.IV);
            cfg.ServerIP = ServerIP; 
            cfg.PortNumber = PortNumber;
            cfg.DatabaseName = datbase;
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
            var cfg = LoadCinfigurationFile();
            var pwd = string.IsNullOrEmpty(cfg.PasswordEnc) ? "" : CryptoHelper.Decrypt(cfg.PasswordEnc, cfg.Key, cfg.IV);
            return $"Server={cfg.ServerIP};Port={cfg.PortNumber};Database={cfg.DatabaseName};Uid={cfg.User};Pwd={pwd};SslMode=Preferred";
        }

        /// <summary>
        /// Validates the existence of the configuration file and manages fallback scenarios.
        /// If the primary path is invalid, it searches the application's current directory or triggers 
        /// the automatic generation of a new template file.
        /// </summary>
        /// <param name="sPath">The initial file path to be checked for the configuration file.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description><c>bool</c>: True if a valid file path was identified or created, otherwise false.</description></item>
        /// <item><description><c>string</c>: The confirmed absolute path to the configuration file.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method acts as a fail-safe mechanism during service initialization to prevent 
        /// file-not-found exceptions in subsequent load or save operations.
        /// </remarks>
        public (bool, string) checkConfigFileAvialable(string sPath)
        {
            if (string.IsNullOrEmpty(sPath) && File.Exists(sPath)) return (true, sPath);
            else
            {
                // Check if app settings availvaible. If so, return the available file.
                if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))) return (true, Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
                
                // if no file is available a new app settings file will be created.
                else
                {
                    generateAppSettingsFile();
                    if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))) return (true, Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
                    else return (false, string.Empty);
                }
            }
        }


        /// <summary>
        /// Creates a new 'appsettings.json' template file with default values in the current application directory.
        /// </summary>
        /// <remarks>
        /// This method initializes a <see cref="SQLConfigurationModel"/> with empty strings and the 
        /// default MySQL port (3306). The resulting JSON is indented for better human readability.
        /// Note: Ensure the application has write permissions for the target directory.
        /// </remarks>
        public void generateAppSettingsFile()
        {

            var config = new SQLConfigurationModel();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true // Generate an json File with correct format
            };

            // generate json string
            string jsonString = JsonSerializer.Serialize(config, options);

            // create json file
            File.WriteAllText(Directory.GetCurrentDirectory()+ "appsettings.json", jsonString);
        }

    }
}
