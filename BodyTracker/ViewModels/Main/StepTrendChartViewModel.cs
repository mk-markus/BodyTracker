using BodyTracker.Models;
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
using System.Threading;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels
{
    public partial class StepTrendChartViewModel : ObservableObject
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/> for retrieving step trend dashboard data.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// The backing collection storing the complete list of step trend dashboard records.
        /// </summary>
        private List<SamsungStepTrendDashboardModel> data;

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
        /// Gets or sets the collection of data series to be displayed in the steps chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty]
        private ISeries[] seriesSteps = Array.Empty<ISeries>();

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
        /// Gets or sets the Y-axes configuration for the steps Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting, and grid line intervals.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] yAxesSteps = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the steps Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] xAxesSteps = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the distance Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting (e.g., "km"), and grid line intervals.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] yAxesDistance = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the inclusive start date for the step trend data filter.
        /// This property determines the earliest record to be displayed in the charts and lists.
        /// </summary>
        [ObservableProperty]
        private DateTime startDate;

        /// <summary>
        /// Gets or sets the inclusive end date for the step trend data filter.
        /// This property defines the latest point in time for which records are retrieved 
        /// and displayed in the UI components.
        /// </summary>
        [ObservableProperty]
        private DateTime endDate;

        /// <summary>
        /// Minimum date of all available step trend records for the selected person. This value is used to set 
        /// the lower bound of the date range filter and to initialize the StartDate property on first load.
        /// </summary>
        public DateTime minMeasurementsDate;

        /// <summary>
        /// Maximum date of all available step trend records for the selected person. This value is used to set the upper bound of the date range 
        /// filter and to initialize the EndDate property on first load.
        /// </summary>
        public DateTime maxMeasurementsDate;

        /// <summary>
        /// Gets or sets a value indicating whether step counts are displayed.
        /// </summary>
        [ObservableProperty]
        private bool showSteps = true;

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
        /// Gets or sets a value indicating whether the step count trend line is displayed.
        /// </summary>
        [ObservableProperty]
        private bool showStepsTrend = false;

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
        private string generalInfoMessage = "";

        /// <summary>
        /// Gets or sets the general error message, sending a database error message via the messenger when the value changes.
        /// </summary>
        public string GeneralInfoMessage
        {
            get => generalInfoMessage;
            set
            {
                generalInfoMessage = value;
                WeakReferenceMessenger.Default.Send(new DatabaseErrorMessage(generalInfoMessage));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepTrendChartViewModel"/> class.
        /// </summary>
        /// <param name="db">The <see cref="DatabaseService"/> instance used for data retrieval.</param>
        public StepTrendChartViewModel(DatabaseService db)
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
        /// Asynchronously fetches the latest step trend records for the selected person from the database 
        /// and triggers a subsequent reload and re-rendering of the associated charts.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RefreshChartDataAsync()
        {
            try
            {
                data = await databaseService.GetStepTrendDashboardSqlAsync(AppState.SelectedPersonId);
                await ReloadChartAsync();
            }
            catch (Exception ex)
            {
                GeneralInfoMessage = ex.Message;
            }
        }

        /// <summary>
        /// Executed automatically by the source generator when the <see cref="StartDate"/> property changes.
        /// Initiates an asynchronous refresh of the chart data to reflect the newly selected time range.
        /// </summary>
        /// <param name="value">The new <see cref="DateTime"/> value assigned to the start date filter.</param>
        /// <remarks>
        /// This method uses a "fire-and-forget" pattern (<c>_ = ...</c>) because partial methods 
        /// generated by the toolkit are synchronous by design. The actual data retrieval and 
        /// UI update are handled within the asynchronous <see cref="ReloadChartAsync"/> method 
        /// to maintain UI responsiveness.
        /// </remarks>
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
        /// <remarks>
        /// This partial method acts as an event hook provided by the CommunityToolkit. 
        /// Using the discard pattern (<c>_ = ...</c>) allows the UI to remain responsive by 
        /// launching the <see cref="ReloadChartAsync"/> task without blocking the property setter's execution thread.
        /// </remarks>
        partial void OnEndDateChanged(DateTime value)
        {
            if (suppressReload) return;
            _ = ReloadChartAsync();
        }

        /// <summary>
        /// Asynchronously refreshes and reloads the chart series and configuration based on the active state and configured date range.
        /// It queries steps daily trend, filters them by date, and conditionally creates 
        /// normal data series or smoothed LOESS trend series depending on the global settings.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ReloadChartAsync()
        {
            if (!await reloadLock.WaitAsync(0)) return;

            try
            {
                if (data == null || !data.Any()) return;

                minMeasurementsDate = data[0].CreateTime;
                maxMeasurementsDate = data[0].CreateTime;

                foreach (var item in data)
                {
                    if (item.CreateTime < minMeasurementsDate)
                        minMeasurementsDate = item.CreateTime;

                    if (item.CreateTime > maxMeasurementsDate)
                        maxMeasurementsDate = item.CreateTime;
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
               .Where(d => d.CreateTime >= StartDate &&
                           d.CreateTime <= EndDate)
               .OrderBy(d => d.CreateTime)
               .ToList();

                var stepsResult = ChartTemplateService.CreateStepsActivityChart(StartDate, EndDate, source, isTrendLineLegendVisible,
                                                        strokeThickness, geometrySize, LoessFraction, ShowSteps, ShowStepsTrend);

                var caloriesResult = ChartTemplateService.CreateStepsCaloriesChart(StartDate, EndDate, source, isTrendLineLegendVisible,
                                                    strokeThickness, geometrySize, LoessFraction, ShowCalories, ShowCaloriesTrend);

                var distanceResult = ChartTemplateService.CreateStepDistanceChart(StartDate, EndDate, source, isTrendLineLegendVisible,
                                                        strokeThickness, geometrySize, LoessFraction, ShowDistance, ShowDistanceTrend);

                SeriesSteps = stepsResult.Series;
                XAxesSteps = stepsResult.XAxis;
                YAxesSteps = stepsResult.YAxis;

                SeriesCalories = caloriesResult.Series;
                XAxesCalories = caloriesResult.XAxis;
                YAxesCalories = caloriesResult.YAxis;

                SeriesDistance = distanceResult.Series;
                XAxesDistance = distanceResult.XAxis;
                YAxesDistance = distanceResult.YAxis;
            }
            catch (Exception ex)
            {
                GeneralInfoMessage = $"Error refreshing chart: {ex.Message}";
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
        /// Resets the date range to encompass all available step trend data.
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