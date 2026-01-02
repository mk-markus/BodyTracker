using BodyTracker.MVVM.Models;
using BodyTracker.Services;
using BodyTracker.State;
using BodyTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BodyTracker.MVVM.Views
{
    /// <summary>
    /// Interaktionslogik für MeasurementPage.xaml
    /// </summary>
    public partial class MeasurementPage : Page
    {
        private readonly MeasurementViewModel _vm;
        private readonly DatabaseService _db;
        private readonly MainWindow _shell;

        public MeasurementPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();
            _db = db;
            _shell = shell;
            _vm = new MeasurementViewModel(_db);
            
            Loaded += async (s, e) => await _vm.InitializeAsync();
            DataContext = null;
            DataContext = _vm;
        }

        private async void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedMeasurement == null)
            {
                MessageBox.Show("Bitte zuerst eine Zeile markieren.");
                return;
            }
            var m = _vm.SelectedMeasurement;
            var res = MessageBox.Show($"Eintrag vom {m.MeasurementDate:d} löschen?", "Löschen bestätigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                await _vm.DeleteCommand.ExecuteAsync(null);
            }
        }

        private void MeasurementsGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;

            Dispatcher.BeginInvoke(new Action(async () =>
            {
                if (e.Row.Item is not FullBodyMeasurementDatasViewModel vmRow) return;

                var mainVm = this.DataContext as MeasurementViewModel;
                if (mainVm == null) return;

                var personId = AppState.SelectedPersonId;
                await mainVm.UpsertMeasurementAsync(personId, vmRow);

                // Optional: Refresh UI
                // await mainVm.InitializeAsync();
            }), DispatcherPriority.Background);
        }
    }
}
