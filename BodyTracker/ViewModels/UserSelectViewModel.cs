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
        [ObservableProperty] private ObservableCollection<PersonModel> personen = new();
        [ObservableProperty] private PersonModel? ausgewaehlt;
        [ObservableProperty] private string vorname = string.Empty;
        [ObservableProperty] private string nachname = string.Empty;
        [ObservableProperty] private DateTime? geburtsdatum = null;

        public IAsyncRelayCommand LadenCommand { get; }
        public IAsyncRelayCommand NeuAnlegenCommand { get; }
        public IRelayCommand BestaetigenCommand { get; }

        public UserSelectViewModel(DatabaseService db)
        {
            _db = db;
            LadenCommand = new AsyncRelayCommand(LoadPersonAsync);
            NeuAnlegenCommand = new AsyncRelayCommand(CreatePersonAsync);
            BestaetigenCommand = new RelayCommand(ConfirmSelectedPerson);
        }

        /// <summary>
        /// initialize the connection to the sql server and loads the person from the table.
        /// </summary>
        public async Task InitializeAsync()
        {
            await _db.InitializeAsync();
            await LoadPersonAsync();
        }

        /// <summary>
        /// Load the Perons from the table and store it into the list
        /// </summary>
        /// <returns></returns>
        private async Task LoadPersonAsync()
        {
            Personen.Clear();
            var list = await _db.GetPersonsAsync();
            foreach (var p in list) Personen.Add(p);
        }

        /// <summary>
        /// Creates 
        /// </summary>
        /// <returns></returns>
        private async Task CreatePersonAsync()
        {
            var id = await _db.CreatePersonAsync(Vorname.Trim(), Nachname.Trim(), Geburtsdatum);
            await LoadPersonAsync();
            Ausgewaehlt = new PersonModel { PersonID = id, PersonFirstName = Vorname.Trim(), PersonLastName = Nachname.Trim(), PersonBirthDate = Geburtsdatum };
        }

        private void ConfirmSelectedPerson()
        {
            if (Ausgewaehlt != null)
            {
                AppState.SelectedPersonId = Ausgewaehlt.PersonID;
                AppState.SelectedPersonName = Ausgewaehlt.ToString();
            }
        }
    }
}
