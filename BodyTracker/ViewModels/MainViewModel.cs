using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BodyTracker.Models;
using BodyTracker.Services;
using BodyTracker.State;

namespace BodyTracker.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _db;
        [ObservableProperty] private ObservableCollection<FullBodyMeasurementDatasViewModel> messungen = new();
        [ObservableProperty] private FullBodyMeasurementDatasViewModel? selectedMessung;
        [ObservableProperty] private string benutzerName = string.Empty;

        public IAsyncRelayCommand ReloadCommand { get; }
        public IAsyncRelayCommand DeleteCommand { get; }

        public MainViewModel(DatabaseService db)
        {
            _db = db;
            ReloadCommand = new AsyncRelayCommand(ReloadAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteSelectedAsync, CanDelete);
            BenutzerName = AppState.SelectedPersonName;
        }

        public async Task InitializeAsync() => await ReloadAsync();

        private async Task ReloadAsync()
        {
            Messungen.Clear();
            var pid = AppState.SelectedPersonId;
            var all = await _db.GetMessungenAsync(pid);
            foreach (var m in all) Messungen.Add(m);
        }

        private bool CanDelete() => SelectedMessung != null;
        partial void OnSelectedMessungChanged(FullBodyMeasurementDatasViewModel? value)
            => (DeleteCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();

        private async Task DeleteSelectedAsync()
        {
            if (SelectedMessung == null) return;
            if (SelectedMessung.MetricID.HasValue) await _db.DeleteMetrikAsync(SelectedMessung.MetricID.Value);
            if (SelectedMessung.DemensionID.HasValue) await _db.DeleteAbmessungAsync(SelectedMessung.DemensionID.Value);
            await ReloadAsync();
        }
    }
}
