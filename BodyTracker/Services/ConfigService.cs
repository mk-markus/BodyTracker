using System.IO;
using System.Text.Json;
using BodyTracker.Models;

namespace BodyTracker.Services
{
    public class ConfigService
    {
        private readonly string _path;
        public ConfigService(string? path = null)
        {
            _path = path ?? Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        }
        public Config Load()
        {
            if (!File.Exists(_path))
            {
                var cfg = new Config();
                Save(cfg);
                return cfg;
            }
            return JsonSerializer.Deserialize<Config>(File.ReadAllText(_path)) ?? new Config();
        }
        public void Save(Config cfg)
        {
            var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions{ WriteIndented = true });
            File.WriteAllText(_path, json);
        }
        public void SaveCredentials(string user, string plainPassword)
        {
            var cfg = Load();
            if (string.IsNullOrEmpty(cfg.Key) || string.IsNullOrEmpty(cfg.IV))
            {
                var (key, iv) = CryptoHelper.GenerateKeyIv();
                cfg.Key = key; cfg.IV = iv;
            }
            cfg.User = user;
            cfg.PasswordEnc = CryptoHelper.Encrypt(plainPassword, cfg.Key, cfg.IV);
            Save(cfg);
        }
        public string BuildConnectionString()
        {
            var cfg = Load();
            var pwd = string.IsNullOrEmpty(cfg.PasswordEnc) ? "" : CryptoHelper.Decrypt(cfg.PasswordEnc, cfg.Key, cfg.IV);
            return $"Server={cfg.Server};Port={cfg.Port};Database={cfg.Database};Uid={cfg.User};Pwd={pwd};SslMode=Preferred";
        }
    }
}
