using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BodyTracker.Models;
using BodyTracker.Services;
using BodyTracker.State;

namespace BodyTracker.ViewModels
{
    public partial class DataEntryViewModel : ObservableObject
    {
        private readonly DatabaseService _db;
        [ObservableProperty] private DateTime datum = DateTime.Today;
        [ObservableProperty] private decimal? gewichtKg;
        [ObservableProperty] private decimal? bmi;
        [ObservableProperty] private decimal? koerperfettProzent;
        [ObservableProperty] private decimal? muskelmasseProzent;
        [ObservableProperty] private int? viszeralfett;
        [ObservableProperty] private decimal? brustumfangCm;
        [ObservableProperty] private decimal? bauchumfangCm;
        [ObservableProperty] private decimal? hueftumfangCm;

        public IAsyncRelayCommand SaveCommand { get; }

        public DataEntryViewModel(DatabaseService db) { _db = db; SaveCommand = new AsyncRelayCommand(SaveAsync); }

        public async Task InitializeAsync()
        {
            var pid = AppState.SelectedPersonId;
            var lastM = await _db.GetLastMetrikAsync(pid, DateTime.Today);
            GewichtKg = lastM?.GewichtKg ?? null;
            Bmi = lastM?.Bmi ?? null;
            KoerperfettProzent = lastM?.KoerperfettProzent ?? null;
            MuskelmasseProzent = lastM?.MuskelmasseProzent ?? null;
            Viszeralfett = lastM?.Viszeralfett ?? null;

            var lastA = await _db.GetLastAbmessungAsync(pid, DateTime.Today);
            BrustumfangCm = lastA?.BrustumfangCm ?? null;
            BauchumfangCm = lastA?.BauchumfangCm ?? null;
            HueftumfangCm = lastA?.HueftumfangCm ?? null;
        }

        private async Task SaveAsync()
        {
            var pid = AppState.SelectedPersonId;
            await _db.InsertMetrikAsync(new KoerperMetrik
            {
                PersonId = pid,
                Messdatum = Datum,
                GewichtKg = GewichtKg,
                Bmi = Bmi,
                KoerperfettProzent = KoerperfettProzent,
                MuskelmasseProzent = MuskelmasseProzent,
                Viszeralfett = Viszeralfett
            });
            await _db.InsertAbmessungAsync(new Abmessung
            {
                PersonId = pid,
                Messdatum = Datum,
                BrustumfangCm = BrustumfangCm,
                BauchumfangCm = BauchumfangCm,
                HueftumfangCm = HueftumfangCm
            });
        }
    }
}
