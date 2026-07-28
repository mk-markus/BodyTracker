using BodyTracker.MVVM.Models;
using BodyTracker.MVVM.Views;
using BodyTracker.MVVM.Views.Pages;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;

namespace BodyTracker.MVVM.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
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
        private readonly DatabaseService databaseServerice;

        /// <summary>
        /// Gets the command responsible for refreshing the measurement history from the database.
        /// Triggers an asynchronous reload of the <see cref="Measurement"/> collection.
        /// </summary>
        public IAsyncRelayCommand ReloadCommand { get; }



        public IAsyncRelayCommand CommandShowNewDatabaseEntryPage { get; }

        #region Observable Property Members

        /// <summary>
        /// Gets or sets the collection of body measurement data displayed in the UI.
        /// Uses <see cref="ObservableCollection{T}"/> to automatically notify the UI 
        /// of additions, removals, or list clears.
        /// </summary>
        [ObservableProperty] private ObservableCollection<FullBodyMeasurementDatasModel> measurement = new();

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty] private ISeries[] series = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty] private ICartesianAxis[] xAxes = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting (e.g., "kg" or "%"), and grid line intervals.
        /// </summary>
        [ObservableProperty] private ICartesianAxis[] yAxes = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the currently selected measurement record from the list.
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
        /// Gets or sets the mean start date used for calculations or scheduling.
        /// </summary>
        [ObservableProperty] private DateTime meanStartDateCurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        /// <summary>
        /// Gets or sets the mean end date for the operation.
        /// </summary>
        [ObservableProperty] private DateTime meanEndDateCurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1);

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

        // <summary>
        /// Gets or sets the inclusive start date for the measurement data filter.
        /// This property determines the earliest record to be displayed in the charts and lists.
        /// </summary>
        [ObservableProperty] private DateTime startDate;

        /// <summary>
        /// Gets or sets the inclusive end date for the measurement data filter.
        /// This property defines the latest point in time for which records are retrieved 
        /// and displayed in the UI components.
        /// </summary>
        [ObservableProperty] private DateTime endDate;

        ///// <summary>
        ///// Minimum date of all available measurements for the selected person. This value is used to set 
        ///// the lower bound of the date range filter and to initialize the StartDate property on first load.
        ///// </summary>
        //public DateTime minMeasurementsDate;

        ///// <summary>
        ///// Maximum date of all available measurements for the selected person. This value is used to set the upper bound of the date range 
        ///// filter and to initialize the EndDate property on first load.
        ///// </summary>
        //public DateTime maxMeasurementsDate;

        /// <summary>
        /// Gets or sets the fraction parameter for the LOESS smoothing algorithm, which determines 
        /// the degree of local averaging applied to the trend line.
        /// </summary>
        [ObservableProperty] private double loessFraction = 0.5;


        #endregion


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
        /// 
        /// </summary>
        private bool firstLoad = true;

        /// <summary>
        /// 
        /// </summary>
        private bool isRefreshing = false;

        public DashboardViewModel(DatabaseService databaseService)
        {
            this.databaseServerice = databaseService;

            ReloadCommand = new AsyncRelayCommand(ReloadAsync);
            UserName = AppState.SelectedPersonName;
            CommandShowNewDatabaseEntryPage = new AsyncRelayCommand(ShowNewDatabaseEntryPage);


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
        /// <item><description>Fetching all measurement records for the currently selected person from the database.</description></item>
        /// <item><description>Populating the observable collection with the retrieved records to trigger UI updates.</description></item>
        /// </list>
        /// </remarks>
        private async Task ReloadAsync()
        {
            var pid = AppState.SelectedPersonId;
            var all = await databaseServerice.GetBodyMeasurementAsync(pid);

            Measurement.Clear();
            foreach (var m in all)
            {
                Measurement.Add(m);
            }


       

           await CalculateAndSetKpiMetrics(MeanStartDateCurrentMonth, MeanEndDateCurrentMonth);

            await RefreshChartAsync();

            OnPropertyChanged(nameof(Measurement));
        }


     

        /// <summary>
        /// Calculates averages for the current period and previous month using GetAverageValues,
        /// updates the bar chart properties, and sets the trend arrows and difference texts.
        /// </summary>
        /// <param name="start">Start date of the current period.</param>
        /// <param name="end">End date of the current period.</param>
        public async Task CalculateAndSetKpiMetrics(DateTime start, DateTime end)
        {
            // 1. Get current period averages using your method
            //var currentPeriodAverages = GetAverageValues(start, end);

            var currentPeriodAverages = BodyCalculationTools.GetAverageValues(start, end, Measurement);
            

            // 2. Get previous period / month averages (shifted back by 1 month)
            var prevStart = start.AddMonths(-1);
            var prevEnd = end.AddMonths(-1);
            var previousPeriodAverages = BodyCalculationTools.GetAverageValues(prevStart, prevEnd, Measurement);

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
            (BodyWaterTrendArrow, BodyWaterDifference) =  GetArrow(previousBodyFat, currentBodyFat);

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
        /// Asynchronously refreshes and reloads the chart series and configuration based on the active state and configured date range.
        /// It queries body measurement metrics (weight, water, muscle, fat), filters them by date, and conditionally creates 
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
                var data = await databaseServerice.GetBodyMeasurementAsync(AppState.SelectedPersonId);

               StartDate = DateTime.Now.AddMonths(-3);
               EndDate = DateTime.Now;

                (Series, XAxes, YAxes) = ChartTemplateService.BodyMeasurementChart( StartDate, EndDate, data, isTrendLineLegendVisible,
                                                                                    strokeThickness, geometrySize, loessFraction,
                                                                                    true, true,
                                                                                    true, true,
                                                                                    true, true,
                                                                                    true, true);

            }

            finally
            {
                isRefreshing = false;
            }
        }

        private async Task ShowNewDatabaseEntryPage()
        {
            WeakReferenceMessenger.Default.Send(new NavigationMessage(NavigationMessage.ShowNewEntryPage));
        }


    }
}
