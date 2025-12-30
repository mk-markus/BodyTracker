using BodyTracker.Services;
using BodyTracker.State;
using BodyTracker.ViewModels;
using System.IO;
using System.Windows;
using System;
using OpenTK.Graphics.OpenGL;
using MySqlConnector;
using System.Threading.Tasks;

namespace BodyTracker.Views
{
    public partial class UserSelectWindow : Window
    {
        private ConfigrationService _config;
        private DatabaseService? _db;
        private UserSelectViewModel? _vm;

        public UserSelectWindow()
        {
            InitializeComponent();
            _config = new ConfigrationService(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
            // gespeicherte Credentials anzeigen und ggf. automatisch verbinden
            var cfg = _config.Load();
            if (!string.IsNullOrEmpty(cfg.User)) DbUserBox.Text = cfg.User;
            if (!string.IsNullOrEmpty(cfg.DatabaseName)) DbDatenbank.Text = cfg.DatabaseName;
            if (!string.IsNullOrEmpty(cfg.ServerIP)) DbServer.Text = cfg.ServerIP;
            if (!string.IsNullOrEmpty(cfg.User)) DbPassBox.Password = "****";
            if (cfg.PortNumber != 0) DbPort.Text = cfg.PortNumber.ToString();

        
                if (!string.IsNullOrEmpty(cfg.User) && !string.IsNullOrEmpty(cfg.PasswordEnc) && !string.IsNullOrEmpty(cfg.DatabaseName))
                {
                    var cs = _config.BuildConnectionString();
                    _db = new DatabaseService(cs);
                    _vm = new UserSelectViewModel(_db);
                    DataContext = _vm;
                    Loaded += async (s, e) => _vm = await EtablisSQLConnection(_vm);
            }
            

            
        }

        private async void OnSaveCredentials(object sender, RoutedEventArgs e)
        {
               
            _config.SaveCredentials(DbUserBox.Text.Trim(), DbPassBox.Password, DbServer.Text, Convert.ToInt16(DbPort.Text.Trim()), DbDatenbank.Text);
            var cs = _config.BuildConnectionString();
            _db = new DatabaseService(cs);
            _vm = new UserSelectViewModel(_db);
            DataContext = _vm;


            _vm = await EtablisSQLConnection(_vm);


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
                MessageBox.Show("Bitte eine PersonModel auswählen oder neu anlegen.");
                return;
            }
            var main = new MainWindow(_db!);
            Application.Current.MainWindow = main;
            main.Show();
            this.Close();
        }

        private void OnBeenden(object sender, RoutedEventArgs e) => Application.Current.Shutdown();


        public async Task<UserSelectViewModel> EtablisSQLConnection(UserSelectViewModel vm)
        {
            try
            {
                await vm.InitializeAsync();
            }

            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error");
            }

            return vm;
        }


    }
}
