using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BodyTracker.Models;
using BodyTracker.Services;
using BodyTracker.State;

namespace BodyTracker.ViewModels
{
    public partial class UserSelectViewModel : ObservableObject
    {
        private readonly DatabaseService _db;
        [ObservableProperty] private ObservableCollection<Person> personen = new();
        [ObservableProperty] private Person? ausgewaehlt;
        [ObservableProperty] private string vorname = string.Empty;
        [ObservableProperty] private string nachname = string.Empty;
        [ObservableProperty] private DateTime? geburtsdatum = null;

        public IAsyncRelayCommand LadenCommand { get; }
        public IAsyncRelayCommand NeuAnlegenCommand { get; }
        public IRelayCommand BestaetigenCommand { get; }

        public UserSelectViewModel(DatabaseService db)
        {
            _db = db;
            LadenCommand = new AsyncRelayCommand(LadenAsync);
            NeuAnlegenCommand = new AsyncRelayCommand(NeuAnlegenAsync);
            BestaetigenCommand = new RelayCommand(Bestaetigen);
        }

        public async Task InitializeAsync()
        {
            await _db.InitializeAsync();
            await LadenAsync();
        }

        private async Task LadenAsync()
        {
            Personen.Clear();
            var list = await _db.GetPersonsAsync();
            foreach (var p in list) Personen.Add(p);
        }

        private async Task NeuAnlegenAsync()
        {
            var id = await _db.CreatePersonAsync(Vorname.Trim(), Nachname.Trim(), Geburtsdatum);
            await LadenAsync();
            Ausgewaehlt = new Person { Id = id, Vorname = Vorname.Trim(), Nachname = Nachname.Trim(), Geburtsdatum = Geburtsdatum };
        }

        private void Bestaetigen()
        {
            if (Ausgewaehlt != null)
            {
                AppState.SelectedPersonId = Ausgewaehlt.Id;
                AppState.SelectedPersonName = Ausgewaehlt.ToString();
            }
        }
    }
}
