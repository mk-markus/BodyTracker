using BodyTracker.Models;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.RightsManagement;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace BodyTracker.ViewModels
{
    public partial class DashboardPageViewModel : ObservableObject
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
        /// Gets or sets the body weight in kilograms. 
        /// Nullable to allow for empty input fields.
        /// </summary>
        [ObservableProperty] private string bodyWeight;

        /// <summary>
        /// Gets or sets the Body Mass Index (BMI).
        /// </summary>
        [ObservableProperty] private string bMI;

        /// <summary>
        /// Gets or sets the body fat percentage.
        /// </summary>
        [ObservableProperty] private string bodyFatPercentage;

        /// <summary>
        /// Gets or sets the measured body fat percentage for the upper body region.
        /// </summary>
        [ObservableProperty] private string bodyFatPercentageTop;

        /// <summary>
        /// Gets or sets the minimum body fat percentage value for the range filter.
        /// </summary>
        [ObservableProperty] private string bodyFatPercentageBottom;

        /// <summary>
        /// Gets or sets the skeletal muscle percentage.
        /// </summary>
        [ObservableProperty] private string bodyMusclePercentage;

        /// <summary>
        /// Gets or sets the percentage of muscle mass in the upper body, if available.
        /// </summary>
        [ObservableProperty] private string bodyMusclePercentageTop;

        /// <summary>
        /// Gets or sets the lower bound for the body muscle percentage range.
        /// </summary>
        [ObservableProperty] private string bodyMusclePercentageBottom;

        /// <summary>
        /// Gets or sets the percentage of body water, if available.
        /// </summary>
        [ObservableProperty] private string bodyWaterPercentage;

        /// <summary>
        /// Gets or sets the mass of the body bone, in kilograms.
        /// </summary>
        [ObservableProperty] private string bodyBoneMass;

        /// <summary>
        /// Gets or sets the chest circumference bodyMeasurement.
        /// </summary> 
        [ObservableProperty] private string chestCircumference;

        /// <summary>
        /// Gets or sets the waist circumference bodyMeasurement.
        /// </summary>
        [ObservableProperty] private string waistCircumference;

        /// <summary>
        /// Gets or sets the hip circumference bodyMeasurement.
        /// </summary>
        [ObservableProperty] private string hipsCircumference;

        /// <summary>
        /// 
        /// </summary>
        [ObservableProperty] private string bodyFatCaliper;

        /// <summary>
        /// 
        /// </summary>
        [ObservableProperty] private string fFM_kg;
        
        /// <summary>
        /// 
        /// </summary>
        [ObservableProperty] private string fFM_describing;

        /// <summary>
        /// Gets the command responsible for refreshing the bodyMeasurement history from the database.
        /// Triggers an asynchronous reload of the <see cref="Measurement"/> collection.
        /// </summary>
        public IAsyncRelayCommand ReloadCommand { get; }

        /// <summary>
        /// Gets the command responsible for navigating to the page for adding a new database entry.
        /// </summary>
        public IAsyncRelayCommand CommandShowNewDatabaseEntryPage { get; }


        /// <summary>
        /// Gets the command responsible for setting the mean start and end dates to encompass the entire current calendar year.
        /// </summary>
        public IRelayCommand CommandSetActualYearMeanValue { get; }

        /// <summary>
        /// Gets the command responsible for setting the mean start and end dates to encompass the entire current calendar month.
        /// </summary>
        public IRelayCommand CommandSetActualMonthMeanValue { get; }

        /// <summary>
        /// Gets the command responsible for setting the mean start and end dates to encompass the current work week (Monday to Sunday).
        /// </summary>
        public IRelayCommand CommandSetActualWeekMeanValue { get; }


        /// <summary>
        /// Backing field for the general error message string.
        /// </summary>
        private string generalErrorMessage = "";

        /// <summary>
        /// Gets or sets the general error message, sending a database error message via the messenger when the value changes.
        /// </summary>
        public string GeneralErrorMessage
        {
            get => generalErrorMessage;
            set
            {
                generalErrorMessage = value;
                WeakReferenceMessenger.Default.Send(new DatabaseErrorMessage(generalErrorMessage));
            }
        }

        /// <summary>
        /// Gets or sets the collection of body bodyMeasurement data displayed in the UI.
        /// Uses <see cref="ObservableCollection{T}"/> to automatically notify the UI 
        /// of additions, removals, or list clears.
        /// </summary>
        [ObservableProperty] private ObservableCollection<FullBodyMeasurementDatasModel> bodyMeasurement = new();

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty] private ISeries[] seriesBodyMeasurements = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty] private ICartesianAxis[] xAxesBodyMeasurements = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting (e.g., "kg" or "%"), and grid line intervals.
        /// </summary>
        [ObservableProperty] private ICartesianAxis[] yAxesBodyMeasurements = Array.Empty<ICartesianAxis>();

        /// <summary>
        ///  
        /// </summary>
        [ObservableProperty] private ISeries[] seriesMuscleDistributionSpiderChart = Array.Empty<ISeries>();

        /// <summary>
        /// 
        /// </summary>
        [ObservableProperty] private IPolarAxis[] angleAxisMuscleDistirbutionSpiderChart = Array.Empty<IPolarAxis>();

        /// <summary>
        /// 
        /// </summary>
        [ObservableProperty] private IPolarAxis[] radiusAxisMuscleDistirbutionSpiderChart = Array.Empty<IPolarAxis>();

        /// <summary>
        /// Gets or sets the currently selected bodyMeasurement record from the list.
        /// Nullable, as no record may be selected.
        /// </summary>
        [ObservableProperty] private FullBodyMeasurementDatasModel? selectedMeasurement;

        /// <summary>
        /// Gets or sets the name of the person currently being viewed.
        /// Used for display purposes in headers or titles.
        /// </summary>
        [ObservableProperty] private string userName = string.Empty;

        /// <summary>
        /// Gets or sets the name of the person currently being viewed.
        /// Used for display purposes in headers or titles.
        /// </summary>
        [ObservableProperty] private string userNameInitial = string.Empty;

        /// <summary>
        /// Gets or sets the UI text block indicating the trend direction of the body weight.
        /// </summary>
        /// <remarks>Used in the UI to visually display whether body weight has increased, decreased, or remained stable compared to the previous period.</remarks>
        [ObservableProperty] private TextBlock bodyWeightTrendArrow;

        /// <summary>
        /// Gets or sets the UI text block indicating the trend direction of the body fat.
        /// </summary>
        /// <remarks>Used in the UI to visually display whether body fat percentage has increased, decreased, or remained stable compared to the previous period.</remarks>
        [ObservableProperty] private TextBlock bodyFatTrendArrow;

        /// <summary>
        /// Gets or sets the UI text block indicating the trend direction of the body muscle mass.
        /// </summary>
        /// <remarks>Used in the UI to visually display whether body muscle mass has increased, decreased, or remained stable compared to the previous period.</remarks>
        [ObservableProperty] private TextBlock bodyMuscleMassTrendArrow;

        /// <summary>
        /// Gets or sets the UI text block indicating the trend direction of the body water.
        /// </summary>
        /// <remarks>Used in the UI to visually display whether body water has increased, decreased, or remained stable compared to the previous period.</remarks>
        [ObservableProperty] private TextBlock bodyWaterTrendArrow;

        /// <summary>
        /// Gets or sets the UI text block indicating the trend direction of the body BMI.
        /// </summary>
        /// <remarks>Used in the UI to visually display whether the Body Mass Index has increased, decreased, or remained stable compared to the previous period.</remarks>
        [ObservableProperty] private TextBlock bodyBmiTrendArrow;

        /// <summary>
        /// Gets or sets the formatted text representing the difference in body weight between comparison periods.
        /// </summary>
        /// <remarks>Contains the calculated numerical variance formatted as a string for direct UI text binding.</remarks>
        [ObservableProperty] private string bodyWeightDifference;

        /// <summary>
        /// Gets or sets the formatted text representing the difference in body fat between comparison periods.
        /// </summary>
        /// <remarks>Contains the calculated numerical variance formatted as a string for direct UI text binding.</remarks>
        [ObservableProperty] private string bodyFatDifference;

        /// <summary>
        /// Gets or sets the formatted text representing the difference in body muscle mass between comparison periods.
        /// </summary>
        /// <remarks>Contains the calculated numerical variance formatted as a string for direct UI text binding.</remarks>
        [ObservableProperty] private string bodyMuscleMassDifference;

        /// <summary>
        /// Gets or sets the formatted text representing the difference in body water between comparison periods.
        /// </summary>
        /// <remarks>Contains the calculated numerical variance formatted as a string for direct UI text binding.</remarks>
        [ObservableProperty] private string bodyWaterDifference;

        /// <summary>
        /// Gets or sets the formatted text representing the difference in body BMI between comparison periods.
        /// </summary>
        /// <remarks>Contains the calculated numerical variance formatted as a string for direct UI text binding.</remarks>
        [ObservableProperty] private string bodyBmiDifference;

        /// <summary>
        /// Gets or sets the bar chart value representing the previous period's body weight.
        /// </summary>
        /// <remarks>Used as a data point for rendering comparison charts in the user interface.</remarks>
        [ObservableProperty] private double bodyWeightPreviousBarValue;

        /// <summary>
        /// Gets or sets the bar chart value representing the current period's body weight.
        /// </summary>
        /// <remarks>Used as a data point for rendering comparison charts in the user interface.</remarks>
        [ObservableProperty] private double bodyWeightCurrentBarValue;

        /// <summary>
        /// Gets or sets the maximum scale value for the body weight bar chart.
        /// </summary>
        /// <remarks>Defines the upper bound boundary of the chart axis to ensure proportional rendering.</remarks>
        [ObservableProperty] private double bodyWeightCurrentMaxValue;

        /// <summary>
        /// Gets or sets the bar chart value representing the previous period's body fat.
        /// </summary>
        /// <remarks>Used as a data point for rendering comparison charts in the user interface.</remarks>
        [ObservableProperty] private double bodyFatPreviousBarValue;

        /// <summary>
        /// Gets or sets the bar chart value representing the current period's body fat.
        /// </summary>
        /// <remarks>Used as a data point for rendering comparison charts in the user interface.</remarks>
        [ObservableProperty] private double bodyFatCurrentBarValue;

        /// <summary>
        /// Gets or sets the maximum scale value for the body fat bar chart.
        /// </summary>
        /// <remarks>Defines the upper bound boundary of the chart axis to ensure proportional rendering.</remarks>
        [ObservableProperty] private double bodyFatCurrentMaxValue;

        /// <summary>
        /// Gets or sets the bar chart value representing the previous period's body muscle mass.
        /// </summary>
        /// <remarks>Used as a data point for rendering comparison charts in the user interface.</remarks>
        [ObservableProperty] private double bodyMuscleMassPreviousBarValue;

        /// <summary>
        /// Gets or sets the bar chart value representing the current period's body muscle mass.
        /// </summary>
        /// <remarks>Used as a data point for rendering comparison charts in the user interface.</remarks>
        [ObservableProperty] private double bodyMuscleMassCurrentBarValue;

        /// <summary>
        /// Gets or sets the maximum scale value for the body muscle mass bar chart.
        /// </summary>
        /// <remarks>Defines the upper bound boundary of the chart axis to ensure proportional rendering.</remarks>
        [ObservableProperty] private double bodyMuscleMassCurrentMaxValue;

        /// <summary>
        /// Gets or sets the bar chart value representing the previous period's body water.
        /// </summary>
        /// <remarks>Used as a data point for rendering comparison charts in the user interface.</remarks>
        [ObservableProperty] private double bodyWaterPreviousBarValue;

        /// <summary>
        /// Gets or sets the bar chart value representing the current period's body water.
        /// </summary>
        /// <remarks>Used as a data point for rendering comparison charts in the user interface.</remarks>
        [ObservableProperty] private double bodyWaterCurrentBarValue;

        /// <summary>
        /// Gets or sets the maximum scale value for the body water bar chart.
        /// </summary>
        /// <remarks>Defines the upper bound boundary of the chart axis to ensure proportional rendering.</remarks>
        [ObservableProperty] private double bodyWaterCurrentMaxValue;

        /// <summary>
        /// Gets or sets the formatted string representing the total cumulative workout volume.
        /// </summary>
        /// <remarks>Displays the aggregate lifting volume formatted with localized thousand separators and unit indicators for direct UI binding.</remarks>
        [ObservableProperty] private string totalWorkoutsVolume;

        /// <summary>
        /// Gets or sets the formatted string representing the total primary muscle workout volume.
        /// </summary>
        /// <remarks>Displays the weighted volume attributed to primary muscle groups, formatted for direct UI display.</remarks>
        [ObservableProperty] private string totalWorkoutsPrimaryVolume;

        /// <summary>
        /// Gets or sets the formatted string representing the total secondary muscle workout volume.
        /// </summary>
        /// <remarks>Displays the weighted volume attributed to secondary muscle groups, formatted for direct UI display.</remarks>
        [ObservableProperty] private string totalWorkoutsSecondaryVolume;

        /// <summary>
        /// Gets or sets the formatted string representing the total count of performed workouts or exercises.
        /// </summary>
        /// <remarks>Displays the aggregate frequency count formatted with numerical separators and unit indicators for the dashboard view.</remarks>
        [ObservableProperty] private string totalWorkouts;

        /// <summary>
        /// Gets or sets the collection of chart series used to render the workout muscle distribution visualization.
        /// </summary>
        /// <remarks>Holds the configured pie series data representing the proportional workload share across different muscle groups.</remarks>
        [ObservableProperty] private IEnumerable<ISeries> workoutMuscleDistributionSeries;

        /// <summary>
        /// Gets or sets the collection of frequently performed exercises displayed in the UI.
        /// </summary>
        /// <remarks>Provides an observable list of exercise frequency statistics used to populate ranking lists or summary grids.</remarks>
        [ObservableProperty] private ObservableCollection<ExerciseFrequencyModel> topExercises;

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty] private ISeries[] seriesMonthlyTraningsVolume = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty] private ICartesianAxis[] xAxesMonthlyTraningsVolume = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting (e.g., "kg" or "%"), and grid line intervals.
        /// </summary>
        [ObservableProperty] private ICartesianAxis[] yAxesMonthlyTraningsVolume = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the fraction parameter for the LOESS smoothing algorithm, which determines 
        /// the degree of local averaging applied to the trend line.
        /// </summary>
        [ObservableProperty] private double loessFraction = 0.5;

        /// <summary>
        /// Gets or sets the collection of mean full body bodyMeasurement data.
        /// </summary>
        /// <remarks>The collection is observable, allowing UI elements or other components to react to
        /// changes such as additions or removals of bodyMeasurement data. This property is typically used for data binding
        /// scenarios.</remarks>
        [ObservableProperty] private ObservableCollection<FullBodyMeasurementDatasModel> meanMeasurement = new();

        /// <summary>
        /// Gets or sets the fraction parameter for the LOESS smoothing algorithm, which determines 
        /// the degree of local averaging applied to the trend line.
        /// </summary>
        [ObservableProperty] private double loessFractionMonthlyTraningsVolume = 0.5;

        /// <summary>
        /// Gets or sets the mean start date used for calculations or scheduling.
        /// </summary>
        [ObservableProperty] private DateTime meanStartDate = new DateTime(2025, 1, 1);

        /// <summary>
        /// Gets or sets the mean end date for the operation.
        /// </summary>
        [ObservableProperty] private DateTime meanEndDate = DateTime.Now;

        /// <summary>
        /// Minimum date of all available measurements for the selected person. This value is used to set 
        /// the lower bound of the date range filter and to initialize the StartDate property on first load.
        /// </summary>
        public DateTime minMeasurementsDateMonthlyTraningsVolume;

        /// <summary>
        /// Maximum date of all available measurements for the selected person. This value is used to set the upper bound of the date range 
        /// filter and to initialize the EndDate property on first load.
        /// </summary>
        public DateTime maxMeasurementsDateMonthlyTraningsVolume;

         // <summary>
        /// Gets or sets the inclusive start date for the bodyMeasurement data filter.
        /// This property determines the earliest record to be displayed in the charts and lists.
        /// </summary>
        [ObservableProperty] private DateTime startDate;

        /// <summary>
        /// Gets or sets the inclusive end date for the bodyMeasurement data filter.
        /// This property defines the latest point in time for which records are retrieved 
        /// and displayed in the UI components.
        /// </summary>
        [ObservableProperty] private DateTime endDate;

        // <summary>
        /// Gets or sets the inclusive start date for the bodyMeasurement data filter.
        /// This property determines the earliest record to be displayed in the charts and lists.
        /// </summary>
        [ObservableProperty] private DateTime startDateMonthlyTraningsVolume;

        /// <summary>
        /// Gets or sets the inclusive end date for the bodyMeasurement data filter.
        /// This property defines the latest point in time for which records are retrieved 
        /// and displayed in the UI components.
        /// </summary>
        [ObservableProperty] private DateTime endDateMonthlyTraningsVolume;


        /// <summary>
        /// Constants for the stroke thickness of the line series in the chart. 
        /// Setting this to a higher value will make the lines more prominent, while a lower value will create a thinner appearance.
        /// </summary>
        private static int strokeThickness = 2;

        /// <summary>
        /// constants for the geometry size of the data points in the chart. Setting this to 1 effectively hides the individual point markers,
        /// </summary>
        private static int geometrySize = 0;

        /// <summary>
        /// Constants for the visibility of trend lines in the chart legend. Setting this to false will hide the trend line entries from the legend,
        /// </summary>
        private static bool isTrendLineLegendVisible = false;

        /// <summary>
        /// A flag indicating whether the dashboard view is undergoing its initial load cycle.
        /// </summary>
        /// <remarks>Used to control conditional initialization tasks, such as setting default date boundaries for charts on startup.</remarks>
        private bool firstLoad = true;

        /// <summary>
        /// A flag indicating whether a data refresh operation is currently in progress.
        /// </summary>
        /// <remarks>Acts as a concurrency guard to prevent overlapping asynchronous refresh cycles and avoid redundant database queries.</remarks>
        private bool isRefreshing = false;

        public DashboardPageViewModel(DatabaseService databaseService)
        {
            this.databaseService = databaseService;

            ReloadCommand = new AsyncRelayCommand(ReloadAsync);
            UserName = AppState.SelectedPersonName;
            CommandShowNewDatabaseEntryPage = new AsyncRelayCommand(ShowNewDatabaseEntryPage);

            CommandSetActualYearMeanValue = new RelayCommand(SetActualYearMeanValue);
            CommandSetActualMonthMeanValue = new RelayCommand(SetActualMonthMeanValue);
            CommandSetActualWeekMeanValue = new RelayCommand(SetActualWeekMeanValue);


        }

        /// <summary>
        /// Triggers the initial asynchronous loading sequence for the ViewModel.
        /// </summary>
        /// <returns>A task that represents the initialization process.</returns>
        /// <remarks>
        /// This method acts as a wrapper for <see cref="ReloadAsync"/>. By isolating the 
        /// initial load in this method, the ViewModel remains compatible with common 
        /// asynchronous initialization patterns in WPF/MVVM architectures.
        /// </remarks>
        public async Task InitializeAsync()
        {
            await ReloadAsync();
        }

        /// <summary>
        /// Synchronizes the local <see cref="Measurement"/> collection with the data stored in the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous reload operation.</returns>
        /// <remarks>
        /// The process involves three steps:
        /// <list type="number">
        /// <item><description>Clearing the existing local collection.</description></item>
        /// <item><description>Fetching all bodyMeasurement records for the currently selected person from the database.</description></item>
        /// <item><description>Populating the observable collection with the retrieved records to trigger UI updates.</description></item>
        /// </list>
        /// </remarks>
        private async Task ReloadAsync()
        {
            var pid = AppState.SelectedPersonId;
            var all = await databaseService.GetBodyMeasurementAsync(pid, databaseService.DatabaseCommands.GetPersonMeasurementsSql());

            BodyMeasurement.Clear();
            foreach (var m in all)
            {
                BodyMeasurement.Add(m);
            }

            await RefreshChartAsync();

            OnPropertyChanged(nameof(BodyMeasurement));
        }

        /// <summary>
        /// Asynchronously refreshes and reloads the chart series and configuration based on the active state and configured date range.
        /// It queries body bodyMeasurement metrics (weight, water, muscle, fat), filters them by date, and conditionally creates 
        /// normal data series or smoothed LOESS trend series depending on the global settings.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RefreshChartAsync()
        {

            if (AppState.SelectedPersonId <= 0)
                return;

            if (isRefreshing)
                return;

            isRefreshing = true;

            try
            {
                var list = await databaseService.GetHeavyAppAsync(AppState.SelectedPersonId);
                var analyzer = new AppWorkoutLoadAnalyzer("", list, 1, 0.5);

                var today = DateTime.Today;
                var startDate = new DateTime(today.Year, today.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                await GetKpiMetrics(startDate, endDate);
                await GetChartBodyMeasurementDashboardValuesAsync(startDate, endDate);
                await GetAppDashboardValuesAsync(analyzer, startDate, endDate);
                await GetChartMuscleDistributionSpiderChart(analyzer, startDate, endDate);
                await GetChartMonthlyTraningsVolumeAsync(analyzer, startDate, endDate);
                SetActualMonthMeanValue();

            }
            catch (Exception ex)
            {
                GeneralErrorMessage = $"Error refreshing dashboard: {ex.Message}";
            }

            finally
            {
                isRefreshing = false;
            }
        }

        /// <summary>
        /// Calculates averages for the current period and previous month using GetAverageValues,
        /// updates the bar chart properties, and sets the trend arrows and difference texts.
        /// </summary>
        /// <param name="start">Start date of the current period.</param>
        /// <param name="end">End date of the current period.</param>
        public async Task GetKpiMetrics(DateTime start, DateTime end)
        {

            // 1. Get current period averages using your method
            //var currentPeriodAverages = GetAverageValues(start, end);

            var currentPeriodAverages = BodyCalculationToolsService.GetAverageValues(start, end, BodyMeasurement);


            // 2. Get previous period / month averages (shifted back by 1 month)
            var prevStart = start.AddMonths(-1);
            var prevEnd = end.AddMonths(-1);
            var previousPeriodAverages = BodyCalculationToolsService.GetAverageValues(prevStart, prevEnd, BodyMeasurement);

            // Extract values safely (assuming GetAverageValues returns a collection containing at least one summary model, or empty)
            var currentModel = currentPeriodAverages?.FirstOrDefault();
            var previousModel = previousPeriodAverages?.FirstOrDefault();

            double currentWeight = currentModel?.BodyWeight ?? 0;
            double previousWeight = previousModel?.BodyWeight ?? 0;

            double currentBodyFat = currentModel?.BodyFatPercentage ?? 0;
            double previousBodyFat = previousModel?.BodyFatPercentage ?? 0;

            double currentBodyWater = currentModel?.BodyWaterPercentage ?? 0;
            double previousBodyWater = previousModel?.BodyWaterPercentage ?? 0;

            double currentBodyMuscle = currentModel?.BodyMusclePercentage ?? 0;
            double previousBodyMuscle = previousModel?.BodyMusclePercentage ?? 0;


            // --- WEIGHT METRICS ---
            BodyWeightCurrentBarValue = currentWeight;
            BodyWeightPreviousBarValue = previousWeight;
            double maxWeight = Math.Max(currentWeight, previousWeight);
            BodyWeightCurrentMaxValue = maxWeight > 0 ? maxWeight * 1.1 : 100.0;

            // Call your GetArrow method for Weight
            (BodyWeightTrendArrow, BodyWeightDifference) = GetArrow(previousWeight, currentWeight);

            // --- BODY FAT METRICS ---
            BodyFatCurrentBarValue = currentBodyFat;
            BodyFatPreviousBarValue = previousBodyFat;
            double maxBodyFat = Math.Max(currentBodyFat, previousBodyFat);
            BodyFatCurrentMaxValue = maxBodyFat > 0 ? maxBodyFat * 1.1 : 100.0;

            // Call your GetArrow method for Body Fat
            (BodyFatTrendArrow, BodyFatDifference) = GetArrow(previousBodyFat, currentBodyFat);


            // --- BODY MUSCLE METRICS ---
            BodyMuscleMassCurrentBarValue = currentBodyMuscle;
            BodyMuscleMassPreviousBarValue = previousBodyMuscle;
            double maxBodyMuscle = Math.Max(currentBodyMuscle, previousBodyMuscle);
            BodyMuscleMassCurrentMaxValue = maxBodyMuscle > 0 ? maxBodyMuscle * 1.1 : 100.0;

            // Call your GetArrow method for Body Fat
            (BodyMuscleMassTrendArrow, BodyMuscleMassDifference) = GetArrow(previousBodyMuscle, currentBodyMuscle);



            // --- BODY Water METRICS ---
            BodyWaterCurrentBarValue = currentBodyWater;
            BodyWaterPreviousBarValue = previousBodyWater;
            double maxBodyWater = Math.Max(currentBodyWater, previousBodyWater);
            BodyFatCurrentMaxValue = maxBodyWater > 0 ? maxBodyWater * 1.1 : 100.0;

            // Call your GetArrow method for Body Fat
            (BodyWaterTrendArrow, BodyWaterDifference) = GetArrow(previousBodyFat, currentBodyFat);

        }

        /// <summary>
        /// Updates the KPI data and bar chart values for weight comparison between the previous and current month.
        /// </summary>
        /// <param name="previousWeight">Weight value or average from the previous month.</param>
        /// <param name="currentWeight">Weight value or average from the current month.</param>
        public (TextBlock, string) GetArrow(double previousValue, double currentValue)
        {

            var valueArrow = new TextBlock();

            var difference = Math.Round(currentValue - previousValue, 2); // Round to 2 decimal places

            if (difference > 0)
            {
                valueArrow.Text = "▲";
                valueArrow.Foreground = Brushes.IndianRed; // Red indicates weight increase
                return (valueArrow, difference.ToString("N2"));
            }
            else if (difference < 0)
            {
                valueArrow.Text = "▼";
                valueArrow.Foreground = Brushes.SeaGreen; // Green indicates weight decrease
                return (valueArrow, difference.ToString("N2"));
            }
            else
            {
                valueArrow.Text = "■";
                valueArrow.Foreground = Brushes.Gray; // Gray indicates no change
                return (valueArrow, difference.ToString("0"));
            }

        }

        /// <summary>
        /// Asynchrone Methode zum Laden der CSV-Daten und Aktualisieren des Pie-Charts.
        /// Kann auch als Command an einen Refresh-Button im UI gebunden werden.
        /// </summary>
        public async Task GetChartBodyMeasurementDashboardValuesAsync(DateTime startDate, DateTime endDate)
        {

            var data = BodyMeasurement.ToList<FullBodyMeasurementDatasModel>();

            startDate = startDate.AddMonths(-1);
            
            var result  = ChartTemplateService.CreateBodyMeasurementChart(startDate, endDate, data, isTrendLineLegendVisible,
                                                                                                                                  strokeThickness, geometrySize, LoessFraction,
                                                                                                                                  true, false,
                                                                                                                                  true, false,
                                                                                                                                  true, false,
                                                                                                                                  true, false);



            // Sicherstellen, dass die UI-Zuweisung auf dem Dispatcher erfolgt
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                SeriesBodyMeasurements = result.Series;
                XAxesBodyMeasurements = result.XAxis;
                YAxesBodyMeasurements = result.YAxis;

            });

        }

        /// <summary>
        /// Asynchronously calculates muscle distribution metrics for the specified date range and the preceding month, 
        /// then configures the corresponding spider chart series and polar axes.
        /// </summary>
        /// <param name="analyzer">The workout load analyzer instance used to compute muscle distribution data.</param>
        /// <param name="startDate">The start date for the current period analysis.</param>
        /// <param name="endDate">The end date for the current period analysis.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetChartMuscleDistributionSpiderChart(AppWorkoutLoadAnalyzer analyzer, DateTime startDate, DateTime endDate)
        {
            var prevstartDate = startDate.AddMonths(-1);

            var prevendDate = prevstartDate.AddMonths(1).AddDays(-1);

            var actualMonthMuscleDistribution = analyzer.CalculateMuscleSplit(analyzer.WorkoutEntries,
                                                                              startDate: startDate,
                                                                              endDate: endDate);
            var previousMonthMuscleDistribution = analyzer.CalculateMuscleSplit(analyzer.WorkoutEntries,
                                                                                startDate: prevstartDate,
                                                                                endDate: prevendDate);

            (SeriesMuscleDistributionSpiderChart, AngleAxisMuscleDistirbutionSpiderChart, RadiusAxisMuscleDistirbutionSpiderChart) = ChartTemplateService.CreateWorkloadMuscleSpiderChart(actualMonthMuscleDistribution, previousMonthMuscleDistribution,
                                                                                                                                                                                 "Current",
                                                                                                                                                                                 "Previous" , geometrySize: 0, strokeThickness: 1);
        }

        /// <summary>
        /// Asynchrone Methode zum Laden der CSV-Daten und Aktualisieren des Pie-Charts.
        /// Kann auch als Command an einen Refresh-Button im UI gebunden werden.
        /// </summary>
        public async Task GetAppDashboardValuesAsync(AppWorkoutLoadAnalyzer analyzer, DateTime startDate, DateTime endDate)
        {

            if (AppState.SelectedPersonId < 0)
            {
                GeneralErrorMessage = "The Person ID is <0";
                return;
            }

            startDate = startDate.AddMonths(-1);

            var totalWorkoutVolume = analyzer.GetVolume(analyzer.WorkoutEntries, startDate: startDate, endDate: endDate);
            TotalWorkoutsVolume = totalWorkoutVolume.TotalVolume.ToString("N0", new System.Globalization.CultureInfo("de-DE")) + " kg";
            TotalWorkoutsPrimaryVolume = totalWorkoutVolume.PrimaryVolume.ToString("N0", new System.Globalization.CultureInfo("de-DE")) + " kg";
            TotalWorkoutsSecondaryVolume = totalWorkoutVolume.SecondaryVolume.ToString("N0", new System.Globalization.CultureInfo("de-DE")) + " kg";

            TotalWorkouts = totalWorkoutVolume.TotalExercises.ToString("N0", new System.Globalization.CultureInfo("de-DE")) + " x";

            TopExercises = new ObservableCollection<ExerciseFrequencyModel>(analyzer.CalculateExerciseFrequency(analyzer.WorkoutEntries, startDate: startDate, endDate: endDate));

        }

        /// <summary>
        /// Asynchronously generates a collection of pie chart series representing the percentage distribution of muscle volumes.
        /// </summary>
        /// <remarks>Filters out muscle groups with zero or negative total volume, maps each remaining entry to a LiveCharts <see cref="PieSeries{T}"/> configuration, assigns custom data labels and tooltips, and formats percentage values to two decimal places.</remarks>
        /// <param name="muscleDistribution">The collection of muscle data results containing volume metrics and percentage shares.</param>
        /// <returns>A task representing the asynchronous operation, containing an array of configured chart series.</returns>
        private async Task<IEnumerable<ISeries>> GenerateChartMuscleDistributionSeriesAsync(IEnumerable<MuscleDataResultsModel> muscleDistribution)
        {

            var pieSeries = muscleDistribution
               .Where(m => m.TotalVolume > 0)
               .Select(m => (ISeries)new PieSeries<double>
               {
                   Name = string.IsNullOrWhiteSpace(m.MuscleGroup) ? "<unknown>" : m.MuscleGroup,
                   Values = new double[] { m.PercentageShare },
                   DataLabelsPosition = PolarLabelsPosition.Middle,
                   DataLabelsFormatter = point =>
                   {
                       return $"{point.Coordinate.PrimaryValue.ToString("F2")} %";
                   },
                   ToolTipLabelFormatter = point =>
                   {
                       return $"{point.Coordinate.PrimaryValue.ToString("F2")} %";
                   }
               })
               .ToArray();

            return pieSeries;
        }

        /// <summary>
        /// Asynchronously generates and updates the monthly training volume chart series and configuration axes.
        /// </summary>
        /// <remarks>Extracts workout log date bounds to initialize filter ranges on first load, computes aggregated monthly volume metrics via the analyzer, and generates cartesian chart templates incorporating smoothing and trend line configurations.</remarks>
        /// <param name="analyzer">The initialized workout load analyzer instance providing access to parsed entries and volume calculations.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task GetChartMonthlyTraningsVolumeAsync(AppWorkoutLoadAnalyzer analyzer, DateTime startDate, DateTime endDate)
        {
            if (analyzer == null) return;

            var data = analyzer.WorkoutEntries;

            startDate = startDate.AddMonths(-3);

            minMeasurementsDateMonthlyTraningsVolume = data.Min(d => d.ExcerciseDate);
            maxMeasurementsDateMonthlyTraningsVolume = data.Max(d => d.ExcerciseDate);

            if (firstLoad)
            {
                StartDateMonthlyTraningsVolume = minMeasurementsDateMonthlyTraningsVolume;
                EndDateMonthlyTraningsVolume = maxMeasurementsDateMonthlyTraningsVolume;
                firstLoad = false;
            }

            var results = await analyzer.CalculateMonthlyVolumeAsync(startDate, endDate);


           var result = ChartTemplateService.CreateWorkloadMonthlyVolumeBarChart(startDate, endDate, results, strokeThickness, geometrySize, 0);

                SeriesMonthlyTraningsVolume = result.Series;
                XAxesMonthlyTraningsVolume = result.XAxis;
                YAxesMonthlyTraningsVolume = result.YAxis;
        }

        /// <summary>
        /// Sends a navigation message via the weak reference messenger to display the new database entry view.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task ShowNewDatabaseEntryPage()
        {
            WeakReferenceMessenger.Default.Send(new NavigationMessage(NavigationMessage.ShowNewEntry));
            await Task.CompletedTask;
        }

        //// <summary>
        /// Sets the mean start and end dates to encompass the entire current calendar year (January 1st to December 31st).
        /// </summary>
        private void SetActualYearMeanValue()
        {
            DateTime today = DateTime.Today;
            MeanStartDate = new DateTime(today.Year, 1, 1);
            MeanEndDate = new DateTime(today.Year, 12, 31);
        }

        /// <summary>
        /// Sets the mean start and end dates to encompass the entire current calendar month (from the first day to the last day).
        /// </summary>
        private void SetActualMonthMeanValue()
        {
            DateTime today = DateTime.Today;

            // First Day of Month
            MeanStartDate = new DateTime(today.Year, today.Month, 1);

            // Last day of the current month:
            // We take the first day of the next month and subtract one day.
            MeanEndDate = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
        }

        /// <summary>
        /// Sets the mean start and end dates to encompass the current work week, assuming the week begins on Monday and ends on Sunday.
        /// </summary>
        private void SetActualWeekMeanValue()
        {
            DateTime today = DateTime.Today;

            // Calculating Monday of this week (assuming the week starts on Monday)
            // DayOfWeek.Sunday is 0, Monday is 1... Saturday is 6.
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = today.AddDays(-1 * diff);

            MeanStartDate = startOfWeek;
            MeanEndDate = startOfWeek.AddDays(6); // Sunday
        }

        /// <summary>
        /// Executed automatically by the source generator when the <see cref="StartDate"/> property changes.
        /// Initiates an asynchronous refresh of the chart data to reflect the newly selected time range.
        /// </summary>
        /// <param name="value">The new <see cref="DateTime"/> value assigned to the start date filter.</param>
        /// <remarks>
        /// This method uses a "fire-and-forget" pattern (<c>_ = ...</c>) because partial methods 
        /// generated by the toolkit are synchronous by design. The actual data retrieval and 
        /// UI update are handled within the asynchronous <see cref="RefreshChartAsync"/> method 
        /// to maintain UI responsiveness.
        /// </remarks>
        partial void OnMeanStartDateChanged(DateTime value)
        {
          GetMeanValues();



        }

        /// <summary>
        /// Executed automatically by the source generator when the <see cref="EndDate"/> property changes.
        /// Triggers an asynchronous update of the chart data to reflect the newly defined end of the observation period.
        /// </summary>
        /// <param name="value">The new <see cref="DateTime"/> value assigned to the end date filter.</param>
        /// <remarks>
        /// This partial method acts as an event hook provided by the CommunityToolkit. 
        /// Using the discard pattern (<c>_ = ...</c>) allows the UI to remain responsive by 
        /// launching the <see cref="RefreshChartAsync"/> task without blocking the property setter's execution thread.
        /// </remarks>
        partial void OnMeanEndDateChanged(DateTime value)
        {
            GetMeanValues();

        }




        private void GetMeanValues()
        {
            try
            {
                var value = BodyCalculationToolsService.GetAverageValues(MeanStartDate, MeanEndDate, BodyMeasurement);

                if (!value.Any()) return;

                var ffm = BodyCalculationToolsService.CalculateFFM((float)value[0].BodyWeight, (float)value[0].BodyFatPercentage);
                var ffm_index = BodyCalculationToolsService.GetFFMIndex(ffm, AppState.SelectedPersonHeight);

                BodyWeight = value[0].BodyWeight?.ToString("N2");
                BMI = value[0].BMI?.ToString("N2");
                FFM_kg = ffm.ToString("F2");
                FFM_describing = $"{ffm_index.ToString("N2")} | {BodyCalculationToolsService.GetFFMIndexDescribing(ffm_index, Services.Calculation.BodyCalculationGenderModel.Gender.Male)}";
                BodyFatPercentage = value[0].BodyFatPercentage?.ToString("N2");
                BodyFatCaliper = value[0].CaliperBodyFatPercentage?.ToString("N2");
                BodyMusclePercentage = value[0].BodyMusclePercentage?.ToString("N2");
                BodyWaterPercentage = value[0].BodyWaterPercentage?.ToString("N2");
                ChestCircumference = value[0].ChestCircumference?.ToString("N2");
                WaistCircumference = value[0].WaistCircumference?.ToString("N2");
                HipsCircumference = value[0].HipsCircumference?.ToString("N2");

            }
            catch(Exception ex ) { GeneralErrorMessage = ex.ToString(); }






        }



        #region Disposal Pattern

        /// <summary>
        /// Tracks whether the object has been disposed to prevent double disposal.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources, suppressing finalization.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of the Dispose pattern, releasing managed resources such as the database service and unregistering messenger listeners when disposing is true.
        /// </summary>
        /// <param name="disposing">A value indicating whether managed resources should be released.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                Debug.WriteLine($"{this.GetType().Name} Disposing managed resources {GetHashCode()}");

                try
                {
                    databaseService?.Dispose();
                    WeakReferenceMessenger.Default.UnregisterAll(this);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{this.GetType().Name} Disposal Error: {ex.Message}");
                }
            }

            disposed = true;
        }

        #endregion

    }
}
