namespace BodyTracker.Models
{
    public class Config
    {
        public string Server { get; set; } = "localhost";
        public int Port { get; set; } = 3306;
        public string Database { get; set; } = "bodytracker";
        public string User { get; set; } = string.Empty;
        public string PasswordEnc { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string IV { get; set; } = string.Empty;
    }
}
