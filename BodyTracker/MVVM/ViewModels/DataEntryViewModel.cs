using BodyTracker.MVVM.Models;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels
{
    public partial class DataEntryViewModel : ObservableObject
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the ViewModel.
        /// </summary>
        /// <remarks>
        /// Marked as <c>readonly</c> to ensure that the service reference remains 
        /// immutable throughout the lifetime of the ViewModel instance, preventing 
        /// accidental reassignment and ensuring architectural stability.
        /// </remarks>
        private readonly DatabaseService databaseService;
                
        /// <summary>
        /// Gets or sets the date of the measurement. Defaults to the current system date.
        /// </summary>
        [ObservableProperty] private DateTime date = DateTime.Today;

        /// <summary>
        /// Gets or sets the body weight in kilograms. 
        /// Nullable to allow for empty input fields.
        /// </summary>
        [ObservableProperty] private float? bodyWeight;

        /// <summary>
        /// Gets or sets the Body Mass Index (BMI).
        /// </summary>
        [ObservableProperty] private float? bmi;

        /// <summary>
        /// Gets or sets the body fat percentage.
        /// </summary>
        [ObservableProperty] private float? bodyFatPercentage;

        /// <summary>
        /// Gets or sets the skeletal muscle percentage.
        /// </summary>
        [ObservableProperty] private float? bodyMusclePercentage;

        /// <summary>
        /// Gets or sets the visceral fat rating (typically an integer index).
        /// </summary>
        [ObservableProperty] private int? bodyVisceralFat;

        /// <summary>
        /// Gets or sets the chest circumference measurement.
        /// </summary> 
        [ObservableProperty] private float? chestCircumference;

        /// <summary>
        /// Gets or sets the waist circumference measurement.
        /// </summary>
        [ObservableProperty] private float? waistCircumference;

        /// <summary>
        /// Gets or sets the hip circumference measurement.
        /// </summary>
        [ObservableProperty] private float? hipCircumference;

        /// <summary>
        /// Gets the command that triggers the asynchronous saving of entered data.
        /// </summary>
        public IAsyncRelayCommand SaveCommand { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryViewModel"/> class.
        /// </summary>
        /// <param name="db">
        /// An instance of the <see cref="DatabaseService"/>, injected to provide access 
        /// to the underlying database operations.
        /// </param>
        /// <remarks>
        /// The constructor performs dependency injection for the database service and 
        /// initializes the <see cref="SaveCommand"/> as an <see cref="AsyncRelayCommand"/> 
        /// to enable asynchronous data persistence from the UI.
        /// </remarks>
        public DataEntryViewModel(DatabaseService db) 
        { 
            databaseService = db; 
            SaveCommand = new AsyncRelayCommand(SaveAsync); 
        }

        /// <summary>
        /// Asynchronously prepares the ViewModel for user interaction by loading 
        /// historical data from the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous initialization operation.</returns>
        /// <remarks>
        /// This method retrieves the most recent record for both body metrics and dimensions 
        /// based on the <see cref="AppState.SelectedPersonId"/>. 
        /// If no historical data is found, the properties remain at their default values (null).
        /// </remarks>
        public async Task InitializeAsync()
        {
            var pid = AppState.SelectedPersonId;
            var lastM = await databaseService.GetLastBodyMetricAsync(pid, DateTime.Today);
            BodyWeight = lastM?.BodyWeight ?? null;
            Bmi = lastM?.BMI ?? null;
            BodyFatPercentage = lastM?.BodyFatPercentage ?? null;
            BodyMusclePercentage = lastM?.BodyMusclePercentage ?? null;
            BodyVisceralFat = lastM?.BodyVisceralFat ?? null;

            var lastA = await databaseService.GetLastBodyDimensionsAsync(pid, DateTime.Today);
            ChestCircumference = lastA?.ChestCircumference ?? null;
            WaistCircumference = lastA?.WaistCircumference ?? null;
            HipCircumference = lastA?.HipsCircumference ?? null;
        }

        /// <summary>
        /// Asynchronously persists the current measurement data to the database.
        /// This method synchronizes both physiological metrics and physical dimensions.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation.</returns>
        /// <remarks>
        /// The method extracts the currently selected person's ID from <see cref="AppState"/> 
        /// and performs two separate insert operations. It maps the ViewModel properties 
        /// to their respective data models (<see cref="BodyMetricModel"/> and <see cref="BodyDimensionsModel"/>).
        /// Note: This operation currently lacks a database transaction; an error in the second 
        /// insert will not roll back the first one.
        /// </remarks>
        private async Task SaveAsync()
        {
            var pid = AppState.SelectedPersonId;

            await databaseService.InsertBodyMetricAsync(new BodyMetricModel
            {
                PersonID = pid,
                MeasurementDate = Date,
                BodyWeight = BodyWeight,
                BMI = Bmi,
                BodyFatPercentage = BodyFatPercentage,
                BodyMusclePercentage = BodyMusclePercentage,
                BodyVisceralFat = BodyVisceralFat
            });

            await databaseService.InsertBodyDimensionAsync(new BodyDimensionsModel
            {
                PersonID = pid,
                MeasurementDate = Date,
                ChestCircumference = ChestCircumference,
                WaistCircumference = WaistCircumference,
                HipsCircumference = HipCircumference
            });
        }
    }
}
