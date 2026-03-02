using BodyTracker.MVVM.Models;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace BodyTracker.ViewModels
{
    public partial class DataEntryViewModel : ObservableObject
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the ViewModel.
        /// </summary>
        private readonly DatabaseService databaseService;
                
        /// <summary>
        /// Gets or sets the date of the measurement. Defaults to the current system date.
        /// </summary>
        [ObservableProperty] private DateTime measurementDate = DateTime.Today;

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
        /// Gets or sets the measured body fat percentage for the upper body region.
        /// </summary>
        [ObservableProperty] private float? bodyFatPercentageTop;

        /// <summary>
        /// Gets or sets the minimum body fat percentage value for the range filter.
        /// </summary>
        [ObservableProperty] private float? bodyFatPercentageBottom;

        /// <summary>
        /// Gets or sets the skeletal muscle percentage.
        /// </summary>
        [ObservableProperty] private float? bodyMusclePercentage;

        /// <summary>
        /// Gets or sets the percentage of muscle mass in the upper body, if available.
        /// </summary>
        [ObservableProperty] private float? bodyMusclePercentageTop;

        /// <summary>
        /// Gets or sets the lower bound for the body muscle percentage range.
        /// </summary>
        [ObservableProperty] private float? bodyMusclePercentageBottom;

        /// <summary>
        /// Gets or sets the percentage of body water, if available.
        /// </summary>
        [ObservableProperty] private float? bodyWaterPercentage;

        /// <summary>
        /// Gets or sets the mass of the body bone, in kilograms.
        /// </summary>
        [ObservableProperty] private float? bodyBoneMass;

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
        [ObservableProperty] private float? hipsCircumference;


        /// <summary>
        /// Gets or sets the fat tong measurement.
        /// </summary>
        [ObservableProperty] private float? fatTongs;

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
            BodyFatPercentageTop = lastM?.BodyFatPercentageTop ?? null;
            BodyFatPercentageBottom = lastM?.BodyFatPercentageBottom ?? null;
            BodyMusclePercentage = lastM?.BodyMusclePercentage ?? null;
            BodyMusclePercentageTop = lastM?.BodyMusclePercentageTop ?? null;
            BodyMusclePercentageBottom = lastM?.BodyMusclePercentageBottom ?? null;
            BodyWaterPercentage = lastM?.BodyWaterPercentage ?? null;
            BodyBoneMass = lastM?.BodyBoneMass ?? null;
            BodyVisceralFat = lastM?.BodyVisceralFat ?? null;

            var lastA = await databaseService.GetLastBodyDimensionsAsync(pid, DateTime.Today);
            ChestCircumference = lastA?.ChestCircumference ?? null;
            WaistCircumference = lastA?.WaistCircumference ?? null;
            HipsCircumference = lastA?.HipsCircumference ?? null;
            FatTongs = lastA?.FatTongs ?? null;
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
        public async Task<bool> SaveAsync()
        {
            var pid = AppState.SelectedPersonId;

            var bodyMetric = new BodyMetricModel
            {
                PersonID = pid,
                MeasurementDate = MeasurementDate,
                BodyWeight = BodyWeight,
                BMI = Bmi,
                BodyFatPercentage = BodyFatPercentage,
                BodyFatPercentageTop = BodyFatPercentageTop,
                BodyFatPercentageBottom = BodyFatPercentageBottom,
                BodyMusclePercentage = BodyMusclePercentage,
                BodyMusclePercentageTop = BodyMusclePercentageTop,
                BodyMusclePercentageBottom = BodyMusclePercentageBottom,
                BodyWaterPercentage = BodyWaterPercentage,
                BodyBoneMass = BodyBoneMass,
                BodyVisceralFat = BodyVisceralFat
            };

            var bodyDimension = new BodyDimensionsModel
            {
                PersonID = pid,
                MeasurementDate = MeasurementDate,
                ChestCircumference = ChestCircumference,
                WaistCircumference = WaistCircumference,
                HipsCircumference = HipsCircumference,
                FatTongs = FatTongs
            };

            bool measurementAlreadyExists = await databaseService.IsMeasurementExistingAsync(bodyMetric.MeasurementDate, bodyMetric.PersonID);

            if (measurementAlreadyExists)
            {
                MessageBox.Show("For the " + bodyMetric.MeasurementDate.ToString() + " already exists a measurement", "Information",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }

            else
            {
                await databaseService.InsertBodyMetricAsync(bodyMetric);

                await databaseService.InsertBodyDimensionAsync(bodyDimension);

            }

            return measurementAlreadyExists;
          
        }




        public bool CheckBodyValuesValid(string[] values)
        {
            foreach (var val in values)
            {
                if (!double.TryParse(val, out _)) return false;
            }
            return true;
        }
    }
}
