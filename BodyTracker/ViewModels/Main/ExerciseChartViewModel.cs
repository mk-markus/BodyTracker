using BodyTracker.Models;
using BodyTracker.Models.Chart;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels.Main
{
    public partial class ExerciseChartViewModel : ObservableObject
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/> for retrieving exercise dashboard data.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// The backing collection storing the complete list of Samsung exercise dashboard records.
        /// </summary>
        private List<SamsungExerciseDashboardModel> data;

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the calories chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty]
        private ISeries[] seriesCalories = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the distance chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty]
        private ISeries[] seriesDistance = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the heart rate chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty]
        private ISeries[] seriesHeartRate = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the speed chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty]
        private ISeries[] seriesSpeed = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the duration chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty]
        private ISeries[] seriesDuration = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the calories Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] xAxesCalories = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the calories Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting, and grid line intervals.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] yAxesCalories = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the distance Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] xAxesDistance = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the distance Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting, and grid line intervals.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] yAxesDistance = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the heart rate Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting, and grid line intervals.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] yAxesHeartRate = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the heart rate Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] xAxesHeartRate = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the speed Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting, and grid line intervals.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] yAxesSpeed = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the speed Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] xAxesSpeed = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the duration Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting, and grid line intervals.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] yAxesDuration = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the duration Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] xAxesDuration = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the inclusive start date for the exercise data filter.
        /// This property determines the earliest record to be displayed in the charts and lists.
        /// </summary>
        [ObservableProperty]
        private DateTime startDate;

        /// <summary>
        /// Gets or sets the inclusive end date for the exercise data filter.
        /// This property defines the latest point in time for which records are retrieved 
        /// and displayed in the UI components.
        /// </summary>
        [ObservableProperty]
        private DateTime endDate;

        /// <summary>
        /// Minimum date of all available exercise records for the selected person. This value is used to set 
        /// the lower bound of the date range filter and to initialize the StartDate property on first load.
        /// </summary>
        public DateTime minMeasurementsDate;

        /// <summary>
        /// Maximum date of all available exercise records for the selected person. This value is used to set the upper bound of the date range 
        /// filter and to initialize the EndDate property on first load.
        /// </summary>
        public DateTime maxMeasurementsDate;

        /// <summary>
        /// Gets or sets a value indicating whether mean heart rate metrics are displayed.
        /// </summary>
        [ObservableProperty]
        private bool showHeartRate = true;

        /// <summary>
        /// Gets or sets a value indicating whether minimum heart rate metrics are displayed.
        /// </summary>
        [ObservableProperty]
        private bool showMinHeartRate = true;

        /// <summary>
        /// Gets or sets a value indicating whether maximum heart rate metrics are displayed.
        /// </summary>
        [ObservableProperty]
        private bool showMaxHeartRate = true;

        /// <summary>
        /// Gets or sets a value indicating whether distance metrics are displayed.
        /// </summary>
        [ObservableProperty]
        private bool showDistance = true;

        /// <summary>
        /// Gets or sets a value indicating whether calorie consumption metrics are displayed.
        /// </summary>
        [ObservableProperty]
        private bool showCalories = true;

        /// <summary>
        /// Gets or sets a value indicating whether speed metrics are displayed.
        /// </summary>
        [ObservableProperty]
        private bool showSpeed = true;

        /// <summary>
        /// Gets or sets a value indicating whether duration metrics are displayed.
        /// </summary>
        [ObservableProperty]
        private bool showDuration = true;

        /// <summary>
        /// Gets or sets a value indicating whether the mean heart rate trend line is displayed.
        /// </summary>
        [ObservableProperty]
        private bool showHeartRateTrend = false;

        /// <summary>
        /// Gets or sets a value indicating whether the minimum heart rate trend line is displayed.
        /// </summary>
        [ObservableProperty]
        private bool showMinHeartRateTrend = false;

        /// <summary>
        /// Gets or sets a value indicating whether the maximum heart rate trend line is displayed.
        /// </summary>
        [ObservableProperty]
        private bool showMaxHeartRateTrend = false;

        /// <summary>
        /// Gets or sets a value indicating whether the distance trend line is displayed.
        /// </summary>
        [ObservableProperty]
        private bool showDistanceTrend = false;

        /// <summary>
        /// Gets or sets a value indicating whether the calorie consumption trend line is displayed.
        /// </summary>
        [ObservableProperty]
        private bool showCaloriesTrend = false;

        /// <summary>
        /// Gets or sets a value indicating whether the speed trend line is displayed.
        /// </summary>
        [ObservableProperty]
        private bool showSpeedTrend = false;

        /// <summary>
        /// Gets or sets a value indicating whether the duration trend line is displayed.
        /// </summary>
        [ObservableProperty]
        private bool showDurationTrend = false;

        /// <summary>
        /// Gets or sets the fraction parameter for the LOESS smoothing algorithm, which determines 
        /// the degree of local averaging applied to the trend line.
        /// </summary>
        [ObservableProperty]
        private double loessFraction = 0.5;

        /// <summary>
        /// Gets the command that triggers an asynchronous refresh of the chart and application data based on current filter settings.
        /// </summary>
        public IAsyncRelayCommand CommandRefreshAsync { get; }

        /// <summary>
        /// Gets the command that triggers an asynchronous reload and re-rendering of all associated charts.
        /// </summary>
        public IAsyncRelayCommand CommandReloadChartsAsync { get; }

        /// <summary>
        /// Gets the command that sets the current filter period to the active/actual year.
        /// </summary>
        public IRelayCommand CommandSetActualYear { get; }

        /// <summary>
        /// Gets the command that sets the current filter period to the active/actual month.
        /// </summary>
        public IRelayCommand CommandSetActualMonth { get; }

        /// <summary>
        /// Gets the command that sets the current filter period to the active/actual calendar week.
        /// </summary>
        public IRelayCommand CommandSetActualWeek { get; }

        /// <summary>
        /// Gets the command that clears all active filters and displays the complete historical dataset.
        /// </summary>
        public IRelayCommand CommandShowAllData { get; }

        /// <summary>
        /// Constants for the stroke thickness of the line series in the chart. 
        /// Setting this to a higher value will make the lines more prominent, while a lower value will create a thinner appearance.
        /// </summary>
        private static int strokeThickness = 2;

        /// <summary>
        /// Constants for the geometry size of the data points in the chart. Setting this to 1 effectively hides the individual point markers.
        /// </summary>
        private static int geometrySize = 0;

        /// <summary>
        /// Constants for the visibility of trend lines in the chart legend. Setting this to false will hide the trend line entries from the legend.
        /// </summary>
        private static bool isTrendLineLegendVisible = false;

        /// <summary>
        /// Gets or sets a value indicating whether the view or view model is performing its initial load cycle.
        /// Used to bypass or handle startup-specific logic.
        /// </summary>
        private bool firstLoad = true;

        /// <summary>
        /// Gets or sets a value indicating whether reloading mechanisms or event triggers are temporarily suppressed 
        /// to prevent recursive updates or redundant data fetches.
        /// </summary>
        private bool suppressReload;

        /// <summary>
        /// A synchronization primitive used to ensure that data reloading or refresh operations 
        /// are thread-safe and prevent concurrent or overlapping executions.
        /// </summary>
        private readonly SemaphoreSlim reloadLock = new(1, 1);

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
        /// Initializes a new instance of the <see cref="ExerciseChartViewModel"/> class.
        /// </summary>
        /// <param name="db">The <see cref="DatabaseService"/> instance used for data retrieval.</param>
        public ExerciseChartViewModel(DatabaseService db)
        {
            databaseService = db;

            CommandReloadChartsAsync = new AsyncRelayCommand(ReloadChartAsync);
            CommandSetActualYear = new RelayCommand(SetActualYear);
            CommandSetActualMonth = new RelayCommand(SetActualMonth);
            CommandSetActualWeek = new RelayCommand(SetActualWeek);
            CommandShowAllData = new RelayCommand(ShowAllData);
            CommandRefreshAsync = new AsyncRelayCommand(RefreshChartDataAsync);

            _ = RefreshChartDataAsync();
        }

        /// <summary>
        /// Asynchronously fetches the latest exercise records for the selected person from the database 
        /// and triggers a subsequent reload and re-rendering of the associated charts.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RefreshChartDataAsync()
        {
            data = await databaseService.GetExerciseDashboardSqlAsync(AppState.SelectedPersonId);
            await ReloadChartAsync();
        }

        /// <summary>
        /// Executed automatically by the source generator when the <see cref="StartDate"/> property changes.
        /// Initiates an asynchronous refresh of the chart data to reflect the newly selected time range.
        /// </summary>
        /// <param name="value">The new <see cref="DateTime"/> value assigned to the start date filter.</param>
        partial void OnStartDateChanged(DateTime value)
        {
            if (suppressReload) return;
            _ = ReloadChartAsync();
        }

        /// <summary>
        /// Executed automatically by the source generator when the <see cref="EndDate"/> property changes.
        /// Triggers an asynchronous update of the chart data to reflect the newly defined end of the observation period.
        /// </summary>
        /// <param name="value">The new <see cref="DateTime"/> value assigned to the end date filter.</param>
        partial void OnEndDateChanged(DateTime value)
        {
            if (suppressReload) return;
            _ = ReloadChartAsync();
        }

        /// <summary>
        /// Asynchronously refreshes and reloads the chart series and configuration based on the active state and configured date range.
        /// It queries exercise data, filters them by date, and conditionally creates 
        /// normal data series or smoothed LOESS trend series depending on the global settings.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ReloadChartAsync()
        {
            if (!await reloadLock.WaitAsync(0)) return;

            if (AppState.SelectedPersonId <= 0)
            {
                GeneralErrorMessage = "No person selected. Please select a person to view their exercise data.";
                return;
            }

            try
            {
                if (data == null || !data.Any()) return;

                minMeasurementsDate = data[0].StartTime;
                maxMeasurementsDate = data[0].StartTime;

                foreach (var item in data)
                {
                    if (item.StartTime < minMeasurementsDate) minMeasurementsDate = item.StartTime;
                    if (item.StartTime > maxMeasurementsDate) maxMeasurementsDate = item.StartTime;
                }

                if (firstLoad)
                {
                    suppressReload = true;

                    StartDate = minMeasurementsDate;
                    EndDate = maxMeasurementsDate;

                    suppressReload = false;
                    firstLoad = false;
                }

                var source = data
               .Where(d => d.StartTime >= StartDate &&
                           d.StartTime <= EndDate)
               .OrderBy(d => d.StartTime)
               .ToList();

                var exerciseDuration = ChartTemplateService.CreateExerciseDurationChart(StartDate, EndDate, source, isTrendLineLegendVisible,
                    strokeThickness, geometrySize, LoessFraction, ShowDuration, ShowDurationTrend);

                var caloriesResult = ChartTemplateService.CreateExerciseCaloriesChart(StartDate, EndDate, source, isTrendLineLegendVisible,
                    strokeThickness, geometrySize, LoessFraction, ShowCalories, ShowCaloriesTrend);

                var distanceResult = ChartTemplateService.CreateExerciseDistanceChart(StartDate, EndDate, source, isTrendLineLegendVisible,
                    strokeThickness, geometrySize, LoessFraction, ShowDistance, ShowDistanceTrend);

                var exerciseHeartRate = ChartTemplateService.CreateExerciseHeartRateChart(StartDate, EndDate, source, isTrendLineLegendVisible,
                    strokeThickness, geometrySize, LoessFraction, ShowHeartRate, ShowHeartRateTrend, ShowMaxHeartRate, ShowMaxHeartRateTrend, ShowMinHeartRate, ShowMinHeartRateTrend);

                var speedResult = ChartTemplateService.CreateExerciseSpeedChart(StartDate, EndDate, source, isTrendLineLegendVisible,
                    strokeThickness, geometrySize, LoessFraction, ShowSpeed, ShowSpeedTrend);

                SeriesDuration = exerciseDuration.Series;
                XAxesDuration = exerciseDuration.XAxis;
                YAxesDuration = exerciseDuration.YAxis;

                SeriesCalories = caloriesResult.Series;
                XAxesCalories = caloriesResult.XAxis;
                YAxesCalories = caloriesResult.YAxis;

                SeriesDistance = distanceResult.Series;
                XAxesDistance = distanceResult.XAxis;
                YAxesDistance = distanceResult.YAxis;

                SeriesHeartRate = exerciseHeartRate.Series;
                XAxesHeartRate = exerciseHeartRate.XAxis;
                YAxesHeartRate = exerciseHeartRate.YAxis;

                SeriesSpeed = speedResult.Series;
                XAxesSpeed = speedResult.XAxis;
                YAxesSpeed = speedResult.YAxis;
            }
            catch (Exception ex)
            {
                GeneralErrorMessage = $"Error refreshing chart: {ex.Message}";
            }
            finally
            {
                reloadLock.Release();
            }
        }

        /// <summary>
        /// Sets the date range to cover from the first day of the current year up to today.
        /// </summary>
        private void SetActualYear()
        {
            StartDate = new DateTime(DateTime.Today.Year, 1, 1);
            EndDate = DateTime.Today;
        }

        /// <summary>
        /// Sets the date range to cover from the first day of the current month up to today.
        /// </summary>
        private void SetActualMonth()
        {
            StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            EndDate = DateTime.Today;
        }

        /// <summary>
        /// Sets the date range to cover from the start of the current week (Monday) up to today.
        /// </summary>
        private void SetActualWeek()
        {
            var today = DateTime.Today;

            int diff = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0)
            {
                diff += 7;
            }

            StartDate = today.AddDays(-diff);
            EndDate = StartDate.AddDays(6);
        }

        /// <summary>
        /// Resets the date range to encompass all available exercise data.
        /// </summary>
        private void ShowAllData()
        {
            StartDate = minMeasurementsDate;
            EndDate = maxMeasurementsDate;
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