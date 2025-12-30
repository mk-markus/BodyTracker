using BodyTracker.Models;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Windows.Controls;

namespace BodyTracker.Services
{
    public class ConfigrationService
    {
        public string   sAppSettingsPath {  get; private set; } = string.Empty;
        public bool     bPathOK {  get; private set; } = false;
        public string   sUser {  get; private set; } = string.Empty ;
        private string  Passwort = string.Empty;
        public string   sDatabse { get; private set; } = string.Empty;
        public int      iPortNumber { get; private set; } = 0;

        public ConfigrationService(string sAppSettingsPath)
        {
            // Check if path is empty than use an default path. 
            (bPathOK, this.sAppSettingsPath) = checkConfigFileAvialable(sAppSettingsPath);
         
            if(!bPathOK) throw new ArgumentException("The app setting file does not exist at: " + sAppSettingsPath);
            
        }
        
        /// <summary>
        /// This function load the app settings file. if there is now file, then it will be gernate automaticaly an file.
        /// </summary>
        /// <returns>returns the existing file or the new generated file</returns>
        public SQLConfigurationModel Load()
        {
            return JsonSerializer.Deserialize<SQLConfigurationModel>(File.ReadAllText(sAppSettingsPath)) ?? new SQLConfigurationModel();
        }

        /// <summary>
        /// Save the changed parameters into the json file appsettings
        /// </summary>
        /// <param name="cfg"></param>
        public void Save(SQLConfigurationModel cfg)
        {
            var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions{ WriteIndented = true });
            File.WriteAllText(sAppSettingsPath, json);
        }


        public void SaveCredentials(string user, string plainPassword, string ServerIP, int PortNumber, string datbase)
        {
            var cfg = Load();
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

        public string BuildConnectionString()
        {
            var cfg = Load();
            var pwd = string.IsNullOrEmpty(cfg.PasswordEnc) ? "" : CryptoHelper.Decrypt(cfg.PasswordEnc, cfg.Key, cfg.IV);
            return $"Server={cfg.ServerIP};Port={cfg.PortNumber};Database={cfg.DatabaseName};Uid={cfg.User};Pwd={pwd};SslMode=Preferred";
        }


        public (bool, string) checkConfigFileAvialable(string sPath)
        {
            if (string.IsNullOrEmpty(sPath) && File.Exists(sPath)) return (true, sPath);
            else
            {
                if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))) return (true, Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
                else
                {
                    generateAppSettingsFile();
                    if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))) return (true, Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
                    else return (false, string.Empty);
                }
            }
        }

  
        /// <summary>
        /// Generates the specifig app settigns file
        /// </summary>
        public void generateAppSettingsFile()
        {
           
            var config = new SQLConfigurationModel
            {
                ServerIP = string.Empty,
                PortNumber = 3306,
                DatabaseName = string.Empty,
                User = string.Empty,
                PasswordEnc = string.Empty,
                Key = string.Empty,
                IV = string.Empty
            };

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
