using System.Windows;
using BodyTracker.ViewModels;
using BodyTracker.Services;

namespace BodyTracker
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;
        private readonly DatabaseService _db;
        
        
        public MainWindow(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
            _vm = new MainViewModel(_db);
            DataContext = _vm;
            Loaded += async (s,e) => await _vm.InitializeAsync();
        }


        private void Eingabe_Click(object sender, RoutedEventArgs e)
        {
            var w = new BodyTracker.Views.DataEntryWindow(_db) { Owner = this };
            w.ShowDialog();
            _vm.ReloadCommand.Execute(null);
        }


        private void Diagramm_Click(object sender, RoutedEventArgs e)
        {
            var w = new BodyTracker.Views.ChartsWindow(_db) { Owner = this };
            w.ShowDialog();
        }


        private async void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedMessung == null)
            {
                MessageBox.Show("Bitte zuerst eine Zeile markieren.");
                return;
            }
            var m = _vm.SelectedMessung;
            var res = MessageBox.Show($"Eintrag vom {m.MeasurementDate:d} löschen?", "Löschen bestätigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                await _vm.DeleteCommand.ExecuteAsync(null);
            }
        }
        private void OnSchliessen(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
