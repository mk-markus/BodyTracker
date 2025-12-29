using System.Windows;
using BodyTracker.ViewModels;
using BodyTracker.Services;
using BodyTracker.State;

namespace BodyTracker.Views
{
    public partial class UserSelectWindow : Window
    {
        private ConfigService _config;
        private DatabaseService? _db;
        private UserSelectViewModel? _vm;

        public UserSelectWindow()
        {
            InitializeComponent();
            _config = new ConfigService();
            // gespeicherte Credentials anzeigen und ggf. automatisch verbinden
            var cfg = _config.Load();
            if (!string.IsNullOrEmpty(cfg.User)) DbUserBox.Text = cfg.User;
            if (!string.IsNullOrEmpty(cfg.User) && !string.IsNullOrEmpty(cfg.PasswordEnc))
            {
                var cs = _config.BuildConnectionString();
                _db = new DatabaseService(cs);
                _vm = new UserSelectViewModel(_db);
                DataContext = _vm;
                Loaded += async (s,e) => await _vm.InitializeAsync();
            }
        }

        private async void OnSaveCredentials(object sender, RoutedEventArgs e)
        {
            _config.SaveCredentials(DbUserBox.Text.Trim(), DbPassBox.Password);
            var cs = _config.BuildConnectionString();
            _db = new DatabaseService(cs);
            _vm = new UserSelectViewModel(_db);
            DataContext = _vm;
            await _vm.InitializeAsync();
            MessageBox.Show("Verbindung gespeichert.");
        }

        private async void OnWeiter(object sender, RoutedEventArgs e)
        {
            if (_vm == null)
            {
                MessageBox.Show("Bitte zuerst die Verbindung speichern.");
                return;
            }
            _vm.BestaetigenCommand.Execute(null);
            if (AppState.SelectedPersonId <= 0)
            {
                MessageBox.Show("Bitte eine Person auswählen oder neu anlegen.");
                return;
            }
            var main = new MainWindow(_db!);
            Application.Current.MainWindow = main;
            main.Show();
            this.Close();
        }

        private void OnBeenden(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}
