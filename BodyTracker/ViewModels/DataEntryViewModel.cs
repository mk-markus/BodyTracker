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
        [ObservableProperty] private float? gewichtKg;
        [ObservableProperty] private float? bmi;
        [ObservableProperty] private float? koerperfettProzent;
        [ObservableProperty] private float? muskelmasseProzent;
        [ObservableProperty] private int? viszeralfett;
        [ObservableProperty] private float? brustumfangCm;
        [ObservableProperty] private float? bauchumfangCm;
        [ObservableProperty] private float? hueftumfangCm;

        public IAsyncRelayCommand SaveCommand { get; }

        public DataEntryViewModel(DatabaseService db) { _db = db; SaveCommand = new AsyncRelayCommand(SaveAsync); }

        public async Task InitializeAsync()
        {
            var pid = AppState.SelectedPersonId;
            var lastM = await _db.GetLastMetrikAsync(pid, DateTime.Today);
            GewichtKg = lastM?.BodyWeight ?? null;
            Bmi = lastM?.BMI ?? null;
            KoerperfettProzent = lastM?.BodyFatPercentage ?? null;
            MuskelmasseProzent = lastM?.BodyMusclePercentage ?? null;
            Viszeralfett = lastM?.BodyVisceralFat ?? null;

            var lastA = await _db.GetLastAbmessungAsync(pid, DateTime.Today);
            BrustumfangCm = lastA?.Chestcircumference ?? null;
            BauchumfangCm = lastA?.WaistCircumference ?? null;
            HueftumfangCm = lastA?.HipsCircumference ?? null;
        }

        private async Task SaveAsync()
        {
            var pid = AppState.SelectedPersonId;
            await _db.InsertMetrikAsync(new BodyMetricModel
            {
                PersonID = pid,
                MeasurementDate = Datum,
                BodyWeight = GewichtKg,
                BMI = Bmi,
                BodyFatPercentage = KoerperfettProzent,
                BodyMusclePercentage = MuskelmasseProzent,
                BodyVisceralFat = Viszeralfett
            });
            await _db.InsertAbmessungAsync(new BodyDimensionsModel
            {
                PersonID = pid,
                MeasurementDate = Datum,
                Chestcircumference = BrustumfangCm,
                WaistCircumference = BauchumfangCm,
                HipsCircumference = HueftumfangCm
            });
        }
    }
}
