using BodyTracker.MVVM.Models;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;

namespace BodyTracker.ViewModels
{
    /// <summary>
    /// Represents the view model for entering and managing body measurement data, including physiological metrics and
    /// physical dimensions. Provides properties for user input, commands for saving data, and methods for initializing
    /// and persisting measurements asynchronously.
    /// </summary>
    /// <remarks>This view model is designed for use in data entry scenarios where users record body metrics
    /// such as weight, BMI, body fat percentage, and circumferences. It integrates with a database service for data
    /// persistence and exposes an asynchronous command for saving entered data. The view model supports initialization
    /// by loading the most recent historical data for the selected person, enabling pre-population of fields. All
    /// properties are observable, allowing UI elements to update automatically in response to changes. Thread safety is
    /// not guaranteed; interactions should occur on the UI thread.</remarks>
    public partial class NewDataEntryViewModel : ObservableObject
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
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBodyWeightValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))] 
        private string bodyWeight;

        /// <summary>
        /// Gets or sets the Body Mass Index (BMI).
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBmiValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))] 
        private string bmi;

        /// <summary>
        /// Gets or sets the body fat percentage.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBodyFatPercentageValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))]
        private string bodyFatPercentage;

        /// <summary>
        /// Gets or sets the measured body fat percentage for the upper body region.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBodyFatPercentageTopValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))] 
        private string bodyFatPercentageTop;

        /// <summary>
        /// Gets or sets the minimum body fat percentage value for the range filter.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBodyFatPercentageBottomValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))] 
        private string bodyFatPercentageBottom;

        /// <summary>
        /// Gets or sets the skeletal muscle percentage.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBodyMusclePercentageValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))] 
        private string bodyMusclePercentage;

        /// <summary>
        /// Gets or sets the percentage of muscle mass in the upper body, if available.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBodyMusclePercentageTopValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))] 
        private string bodyMusclePercentageTop;

        /// <summary>
        /// Gets or sets the lower bound for the body muscle percentage range.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBodyMusclePercentageBottomValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))] 
        private string bodyMusclePercentageBottom;

        /// <summary>
        /// Gets or sets the percentage of body water, if available.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBodyWaterPercentageValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))] 
        private string bodyWaterPercentage;

        /// <summary>
        /// Gets or sets the mass of the body bone, in kilograms.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBodyBoneMassValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))] 
        private string bodyBoneMass;

        /// <summary>
        /// Gets or sets the visceral fat rating (typically an integer index).
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBodyVisceralFatValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))] 
        private string bodyVisceralFat;

        /// <summary>
        /// Gets or sets the chest circumference measurement.
        /// </summary> 
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsChestCircumferenceValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))] 
        private string chestCircumference;

        /// <summary>
        /// Gets or sets the waist circumference measurement.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsWaistCircumferenceValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))] 
        private string waistCircumference;

        /// <summary>
        /// Gets or sets the hip circumference measurement.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsHipsCircumferenceValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))] 
        private string hipsCircumference;


        /// <summary>
        /// Gets or sets the breast skinfold measurement value.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFatTongBreastCreaseValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))]
        private string fatTongBreastCrease;

        /// <summary>
        /// Gets or sets the armpit skinfold measurement value.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFatTongArmpitCreaseValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))]
        private string fatTongArmpitCrease;

        /// <summary>
        /// Gets or sets the abdominal skinfold measurement value.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFatTongAbdominalCreaseValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))]
        private string fatTongAbdominalCrease;

        /// <summary>
        /// Gets or sets the hip skinfold measurement value.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFatTongHipCreaseValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))]
        private string fatTongHipCrease;

        /// <summary>
        /// Gets or sets the thigh skinfold measurement value.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFatTongThighCreaseValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))]
        private string fatTongThighCrease;

        /// <summary>
        /// Gets or sets the back skinfold measurement value.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFatTongBackCreaseValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))]
        private string fatTongBackCrease;

        /// <summary>
        /// Gets or sets the triceps skinfold measurement value.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFatTongTricepsCreaseValid))]
        [NotifyPropertyChangedFor(nameof(IsNewEntryPageFormValid))]
        private string fatTongTricepsCrease;

        /// <summary>
        /// Gets or sets the validation message displayed to the user.
        /// </summary>
        [ObservableProperty]
        private string validationMessage = string.Empty;

        /// <summary>
        /// Handles changes to the body weight input value. Validates that the input is a valid floating-point number, 
        /// automatically recalculates and formats the BMI if a valid height is available in the application state, 
        /// updates the validation message, and notifies the system of changes to the form validity state.
        /// </summary>
        /// <param name="value">The new string representation of the body weight.</param>
        partial void OnBodyWeightChanged(string value)
        {
            if (TryParseFloat(value, out var v))
            {
                if (AppState.SelectedPersonHeight > 0) Bmi = CalculateBmi(v, (float)AppState.SelectedPersonHeight).ToString("F2");
                ValidationMessage = string.Empty;
            }
            else
            {
                ValidationMessage = "Weight: Please enter a valid number.";
            }
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles changes to the body fat percentage value. Validates that the input is a valid floating-point number, 
        /// updates the validation message accordingly, and notifies the system of changes to the form validity state.
        /// </summary>
        /// <param name="value">The new string representation of the body fat percentage.</param>
        partial void OnBodyFatPercentageChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Body fat: Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles changes to the top body fat percentage value. Validates that the input is a valid floating-point number, 
        /// updates the validation message for the top range, and notifies the system of changes to the form validity state.
        /// </summary>
        /// <param name="value">The new string representation of the top body fat percentage.</param>
        partial void OnBodyFatPercentageTopChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Body fat (top): Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles changes to the bottom body fat percentage value. Validates that the input is a valid floating-point number, 
        /// updates the general body fat validation message, and notifies the system of changes to the form validity state.
        /// </summary>
        /// <param name="value">The new string representation of the bottom body fat percentage.</param>
        partial void OnBodyFatPercentageBottomChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Body fat: Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles changes to the body muscle percentage value. Validates that the input is a valid floating-point number, 
        /// updates the muscle validation message, and notifies the system of changes to the form validity state.
        /// </summary>
        /// <param name="value">The new string representation of the body muscle percentage.</param>
        partial void OnBodyMusclePercentageChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Muscle: Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles changes to the top body muscle percentage value. Validates that the input is a valid floating-point number, 
        /// updates the validation message for the top range, and notifies the system of changes to the form validity state.
        /// </summary>
        /// <param name="value">The new string representation of the top body muscle percentage.</param>
        partial void OnBodyMusclePercentageTopChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Muscle (top): Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles changes to the bottom body muscle percentage value. Validates that the input is a valid floating-point number, 
        /// updates the validation message for the bottom range, and notifies the system of changes to the form validity state.
        /// </summary>
        /// <param name="value">The new string representation of the bottom body muscle percentage.</param>
        partial void OnBodyMusclePercentageBottomChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Muscle (bottom): Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles changes to the body water percentage value. Validates that the input is a valid floating-point number, 
        /// updates the water validation message, and notifies the system of changes to the form validity state.
        /// </summary>
        /// <param name="value">The new string representation of the body water percentage.</param>
        partial void OnBodyWaterPercentageChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Water: Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles changes to the body bone mass value. Validates that the input is a valid floating-point number, 
        /// updates the bone mass validation message, and notifies the system of changes to the form validity state.
        /// </summary>
        /// <param name="value">The new string representation of the body bone mass.</param>
        partial void OnBodyBoneMassChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Bone mass: Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles changes to the body visceral fat value. Validates that the input is a valid integer based on current culture 
        /// settings, updates the visceral fat validation message, and notifies the system of changes to the form validity state.
        /// </summary>
        /// <param name="value">The new string representation of the visceral fat value.</param>
        partial void OnBodyVisceralFatChanged(string value)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out var i)) ValidationMessage = string.Empty;
            else ValidationMessage = "Visceral fat: Please enter a valid integer.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles changes to the chest circumference value. Validates that the input is a valid floating-point number, 
        /// updates the chest circumference validation message, and notifies the system of changes to the form validity state.
        /// </summary>
        /// <param name="value">The new string representation of the chest circumference.</param>
        partial void OnChestCircumferenceChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Chest circumference: Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles changes to the waist circumference value. Validates that the input is a valid floating-point number, 
        /// updates the waist circumference validation message, and notifies the system of changes to the form validity state.
        /// </summary>
        /// <param name="value">The new string representation of the waist circumference.</param>
        partial void OnWaistCircumferenceChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Waist circumference: Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles changes to the hips circumference value. Validates that the input is a valid floating-point number, 
        /// updates the hips circumference validation message, and notifies the system of changes to the form validity state.
        /// </summary>
        /// <param name="value">The new string representation of the hips circumference.</param>
        partial void OnHipsCircumferenceChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Hips circumference: Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles changes to the fat tongs (skinfold caliper) value. Validates that the input is a valid floating-point number, 
        /// updates the skinfold validation message, and notifies the system of changes to the form validity state.
        /// </summary>
        /// <param name="value">The new string representation of the fat tongs measurement.</param>
        partial void OnFatTongBreastCreaseChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Skinfold (fat tongs - breast crease): Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles the validation logic when the armpit skinfold measurement changes.
        /// </summary>
        /// <param name="value">The new input value as a string.</param>
        partial void OnFatTongArmpitCreaseChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Skinfold (fat tongs - armpit crease): Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles the validation logic when the abdominal skinfold measurement changes.
        /// </summary>
        /// <param name="value">The new input value as a string.</param>
        partial void OnFatTongAbdominalCreaseChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Skinfold (fat tongs - abdominal crease): Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles the validation logic when the hip skinfold measurement changes.
        /// </summary>
        /// <param name="value">The new input value as a string.</param>
        partial void OnFatTongHipCreaseChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Skinfold (fat tongs - hip crease): Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles the validation logic when the thigh skinfold measurement changes.
        /// </summary>
        /// <param name="value">The new input value as a string.</param>
        partial void OnFatTongThighCreaseChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Skinfold (fat tongs - thigh crease): Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles the validation logic when the back skinfold measurement changes.
        /// </summary>
        /// <param name="value">The new input value as a string.</param>
        partial void OnFatTongBackCreaseChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Skinfold (fat tongs - back crease): Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Handles the validation logic when the triceps skinfold measurement changes.
        /// </summary>
        /// <param name="value">The new input value as a string.</param>
        partial void OnFatTongTricepsCreaseChanged(string value)
        {
            if (TryParseFloat(value, out var v)) ValidationMessage = string.Empty;
            else ValidationMessage = "Skinfold (fat tongs - tricep crease): Please enter a valid number.";
            OnPropertyChanged(nameof(IsNewEntryPageFormValid));
        }

        /// <summary>
        /// Gets a value indicating whether the body weight input is valid.
        /// Parses the string representation to a floating-point number and validates its range.
        /// </summary>
        bool IsBodyWeightValid => TryParseFloat(BodyWeight, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the BMI value is valid. 
        /// Currently returns true by default without performing underlying validation.
        /// </summary>
        bool IsBmiValid => true;

        /// <summary>
        /// Gets a value indicating whether the body fat percentage input is valid.
        /// Parses the string representation to a floating-point number and validates its range.
        /// </summary>
        bool IsBodyFatPercentageValid => TryParseFloat(BodyFatPercentage, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the top body fat percentage input is valid.
        /// Parses the string representation to a floating-point number and validates its range.
        /// </summary>
        bool IsBodyFatPercentageTopValid => TryParseFloat(BodyFatPercentageTop, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the bottom body fat percentage input is valid.
        /// Parses the string representation to a floating-point number and validates its range.
        /// </summary>
        bool IsBodyFatPercentageBottomValid => TryParseFloat(BodyFatPercentageBottom, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the body muscle percentage input is valid.
        /// Parses the string representation to a floating-point number and validates its range.
        /// </summary>
        bool IsBodyMusclePercentageValid => TryParseFloat(BodyMusclePercentage, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the top body muscle percentage input is valid.
        /// Parses the string representation to a floating-point number and validates its range.
        /// </summary>
        bool IsBodyMusclePercentageTopValid => TryParseFloat(BodyMusclePercentageTop, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the bottom body muscle percentage input is valid.
        /// Parses the string representation to a floating-point number and validates its range.
        /// </summary>
        bool IsBodyMusclePercentageBottomValid => TryParseFloat(BodyMusclePercentageBottom, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the body water percentage input is valid.
        /// Parses the string representation to a floating-point number and validates its range.
        /// </summary>
        bool IsBodyWaterPercentageValid => TryParseFloat(BodyWaterPercentage, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the body bone mass input is valid.
        /// Parses the string representation to a floating-point number and validates its range.
        /// </summary>
        bool IsBodyBoneMassValid => TryParseFloat(BodyBoneMass, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the body visceral fat input is valid.
        /// Parses the string representation to a floating-point number and validates its range.
        /// </summary>
        bool IsBodyVisceralFatValid => TryParseFloat(BodyVisceralFat, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the chest circumference input is valid.
        /// Parses the string representation to a floating-point number and validates its range.
        /// </summary>
        bool IsChestCircumferenceValid => TryParseFloat(ChestCircumference, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the waist circumference input is valid.
        /// Parses the string representation to a floating-point number and validates its range.
        /// </summary>
        bool IsWaistCircumferenceValid => TryParseFloat(WaistCircumference, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the hips circumference input is valid.
        /// Parses the string representation to a floating-point number and validates its range.
        /// </summary>
        bool IsHipsCircumferenceValid => TryParseFloat(HipsCircumference, out var v) && IsNumberValid(v);
        /// <summary>
        /// Gets a value indicating whether the breast skinfold measurement is valid and can be parsed as a float.
        /// </summary>
        bool IsFatTongBreastCreaseValid => TryParseFloat(FatTongBreastCrease, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the armpit skinfold measurement is valid and can be parsed as a float.
        /// </summary>
        bool IsFatTongArmpitCreaseValid => TryParseFloat(FatTongArmpitCrease, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the abdominal skinfold measurement is valid and can be parsed as a float.
        /// </summary>
        bool IsFatTongAbdominalCreaseValid => TryParseFloat(FatTongAbdominalCrease, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the hip skinfold measurement is valid and can be parsed as a float.
        /// </summary>
        bool IsFatTongHipCreaseValid => TryParseFloat(FatTongHipCrease, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the thigh skinfold measurement is valid and can be parsed as a float.
        /// </summary>
        bool IsFatTongThighCreaseValid => TryParseFloat(FatTongThighCrease, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the back skinfold measurement is valid and can be parsed as a float.
        /// </summary>
        bool IsFatTongBackCreaseValid => TryParseFloat(FatTongBackCrease, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the triceps skinfold measurement is valid and can be parsed as a float.
        /// </summary>
        bool IsFatTongTricepsCreaseValid => TryParseFloat(FatTongTricepsCrease, out var v) && IsNumberValid(v);

        /// <summary>
        /// Gets a value indicating whether the entire form is valid.
        /// </summary>
        public bool IsNewEntryPageFormValid => IsBodyWeightValid && IsBmiValid && IsBodyFatPercentageValid &&
                                               IsBodyFatPercentageTopValid && IsBodyFatPercentageBottomValid &&
                                               IsBodyMusclePercentageValid && IsBodyMusclePercentageTopValid &&
                                               IsBodyMusclePercentageBottomValid && IsBodyWaterPercentageValid &&
                                               IsBodyBoneMassValid && IsBodyVisceralFatValid &&
                                               IsChestCircumferenceValid && IsWaistCircumferenceValid &&
                                               IsHipsCircumferenceValid && IsFatTongBreastCreaseValid &&
                                               IsFatTongArmpitCreaseValid && IsFatTongAbdominalCreaseValid &&
                                               IsFatTongHipCreaseValid && IsFatTongThighCreaseValid &&
                                               IsFatTongBackCreaseValid && IsFatTongTricepsCreaseValid;

        /// <summary>
        /// Validates whether the specified floating-point value falls within the acceptable range.
        /// Returns true if the value is not null and is between 0 and 300 (inclusive).
        /// </summary>
        /// <param name="value">The nullable floating-point value to validate.</param>
        /// <returns><c>true</c> if the value is within the range [0, 300]; otherwise, <c>false</c>.</returns>
        private bool IsNumberValid(float? value) => value.HasValue && value.Value >= 0 && value.Value <= 300;

        /// <summary>
        /// Gets the command that triggers the asynchronous saving of entered data.
        /// </summary>
        public IAsyncRelayCommand SaveNewDataCommand { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="NewDataEntryViewModel"/> class.
        /// </summary>
        /// <param name="db">
        /// An instance of the <see cref="DatabaseService"/>, injected to provide access 
        /// to the underlying database operations.
        /// </param>
        /// <remarks>
        /// The constructor performs dependency injection for the database service and 
        /// initializes the <see cref="SaveNewDataCommand"/> as an <see cref="AsyncRelayCommand"/> 
        /// to enable asynchronous data persistence from the UI.
        /// </remarks>
        public NewDataEntryViewModel(DatabaseService db) 
        { 
            databaseService = db;
           
            SaveNewDataCommand = new AsyncRelayCommand(SaveAsync); 
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
            var height = AppState.SelectedPersonHeight;
            var weight = lastM?.BodyWeight ?? 0f;

            BodyWeight = weight.ToString();
            Bmi = CalculateBmi(weight, (float)height).ToString("F2");
            BodyFatPercentage = lastM?.BodyFatPercentage.ToString() ?? string.Empty;
            BodyFatPercentageTop = lastM?.BodyFatPercentageTop.ToString() ?? string.Empty;
            BodyFatPercentageBottom = lastM?.BodyFatPercentageBottom.ToString() ?? string.Empty;
            BodyMusclePercentage = lastM?.BodyMusclePercentage.ToString() ?? string.Empty;
            BodyMusclePercentageTop = lastM?.BodyMusclePercentageTop.ToString() ?? string.Empty;
            BodyMusclePercentageBottom = lastM?.BodyMusclePercentageBottom.ToString() ?? string.Empty;
            BodyWaterPercentage = lastM?.BodyWaterPercentage.ToString() ?? string.Empty;
            BodyBoneMass = lastM?.BodyBoneMass.ToString() ?? string.Empty;
            BodyVisceralFat = lastM?.BodyVisceralFat.ToString() ?? string.Empty;

            var lastA = await databaseService.GetLastBodyDimensionsAsync(pid, DateTime.Today);
            ChestCircumference = lastA?.ChestCircumference.ToString() ?? string.Empty;
            WaistCircumference = lastA?.WaistCircumference.ToString() ?? string.Empty;
            HipsCircumference = lastA?.HipsCircumference.ToString() ?? string.Empty;
            FatTongBreastCrease = lastA?.FatTongBreastCrease.ToString() ?? string.Empty;
            FatTongArmpitCrease = lastA?.FatTongArmpitCrease.ToString() ?? string.Empty;
            FatTongAbdominalCrease = lastA?.FatTongAbdominalCrease.ToString() ?? string.Empty;
            FatTongHipCrease = lastA?.FatTongHipCrease.ToString() ?? string.Empty;
            FatTongThighCrease = lastA?.FatTongThighCrease.ToString() ?? string.Empty;
            FatTongBackCrease = lastA?.FatTongBackCrease.ToString() ?? string.Empty;
            FatTongTricepsCrease = lastA?.FatTongTricepsCrease.ToString() ?? string.Empty;
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

            if (TryParseFloat(BodyWeight, out var weight) &&
               TryParseFloat(Bmi, out var bmi) &&
               TryParseFloat(BodyFatPercentage, out var fatPercentage) &&
               TryParseFloat(BodyFatPercentageTop, out var fatPercentageTop) &&
               TryParseFloat(BodyFatPercentageBottom, out var fatPercentageBottom) &&
               TryParseFloat(BodyMusclePercentage, out var musclePercentage) &&
               TryParseFloat(BodyMusclePercentageTop, out var musclePercentageTop) &&
               TryParseFloat(BodyMusclePercentageBottom, out var musclePercentageBottom) &&
               TryParseFloat(BodyWaterPercentage, out var waterPercentage) &&
               TryParseFloat(BodyBoneMass, out var boneMass) &&
               int.TryParse(BodyVisceralFat, out var visceralFat) &&
               TryParseFloat(ChestCircumference, out var chestCircumference) &&
               TryParseFloat(WaistCircumference, out var waistCircumference) &&
               TryParseFloat(HipsCircumference, out var hipsCircumference) &&
               TryParseFloat(FatTongBreastCrease, out var fatTongBreastCrease) &&
               TryParseFloat(FatTongTricepsCrease, out var fatTongTricepsCrease) &&
               TryParseFloat(FatTongArmpitCrease, out var fatTongArmpitCrease) &&
               TryParseFloat(FatTongAbdominalCrease, out var fatTongAbdominalCrease) &&
               TryParseFloat(FatTongHipCrease, out var fatTongHipCrease) &&
               TryParseFloat(FatTongThighCrease, out var fatTongThighCrease) &&
               TryParseFloat(FatTongBackCrease, out var fatTongBackCrease))
            {
                var bodyMetric = new BodyMetricModel
                {
                    PersonID = pid,
                    MeasurementDate = MeasurementDate,
                    BodyWeight = weight,
                    BMI = bmi,
                    BodyFatPercentage = fatPercentage,
                    BodyFatPercentageTop = fatPercentageTop,
                    BodyFatPercentageBottom = fatPercentageBottom,
                    BodyMusclePercentage = musclePercentage,
                    BodyMusclePercentageTop = musclePercentageTop,
                    BodyMusclePercentageBottom = musclePercentageBottom,
                    BodyWaterPercentage = waterPercentage,
                    BodyBoneMass = boneMass,
                    BodyVisceralFat = visceralFat
                };

                var bodyDimension = new BodyDimensionsModel
                {
                    PersonID = pid,
                    MeasurementDate = MeasurementDate,
                    ChestCircumference = chestCircumference,
                    WaistCircumference = waistCircumference,
                    HipsCircumference = hipsCircumference,
                    FatTongBreastCrease = fatTongBreastCrease,
                    FatTongTricepsCrease = fatTongTricepsCrease,
                    FatTongArmpitCrease = fatTongArmpitCrease,
                    FatTongAbdominalCrease = fatTongAbdominalCrease,
                    FatTongHipCrease = fatTongHipCrease,
                    FatTongThighCrease = fatTongThighCrease,
                    FatTongBackCrease = fatTongBackCrease
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

            else return false;

        }

        /// <summary>
        /// Calculates the Body Mass Index (BMI) based on the specified weight and height.
        /// </summary>
        /// <param name="weight">The weight of the individual, in kilograms.</param>
        /// <param name="height">The height of the individual, in meters. Must be greater than zero.</param>
        /// <returns>The calculated BMI value as a floating-point number.</returns>
        public float CalculateBmi(float weight, float height)
        {
            if (height <= 0) MessageBox.Show("Height must be greater than zero.", nameof(height),MessageBoxButton.OK, MessageBoxImage.Error);
            return weight / (height * height);
        }

        /// <summary>
        /// Determines whether all elements in the specified array can be parsed as valid double-precision
        /// floating-point numbers.
        /// </summary>
        /// <param name="values">An array of strings to validate as double-precision floating-point values.</param>
        /// <returns>true if every element in the array can be parsed as a double; otherwise, false.</returns>
        public bool CheckBodyValuesValid(string[] values)
        {
            foreach (var val in values)
            {
                if (!double.TryParse(val, out _)) return false;
            }
            return true;
        }

        /// <summary>
        /// Attempts to convert the string representation of a number to its single-precision floating-point number equivalent 
        /// using the current culture settings and specific number styles.
        /// </summary>
        /// <param name="text">The string representation of the number to convert. Can be null or whitespace.</param>
        /// <param name="value">When this method returns, contains the parsed floating-point number if the conversion succeeded, 
        /// or 0.0f if the conversion failed or if <paramref name="text"/> is null or whitespace.</param>
        /// <returns><c>true</c> if <paramref name="text"/> was converted successfully; otherwise, <c>false</c>.</returns>
        private bool TryParseFloat(string? text, out float value)
        {
            value = 0f;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value);
        }
    }
}
