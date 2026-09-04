using BodyTracker.Models.Chart;
using BodyTracker.Models.SamsungHealth;
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
    public partial class OxygenSaturationChartViewModel : ObservableObject, IDisposable
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/> for retrieving oxygen saturation dashboard data.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// The backing collection storing the complete list of Samsung oxygen saturation dashboard records.
        /// </summary>
        private List<SamsungOxygenSaturationDashboardModel> data;

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the SpO2 chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty]
        private ISeries[] seriesSpO2 = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the low SpO2 duration chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty]
        private ISeries[] seriesLowSpO2Duration = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the SpO2 coverage rate chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty]
        private ISeries[] seriesSpO2CoverageRate = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the SpO2 Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] xAxesSpO2 = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the SpO2 Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting, and grid line intervals.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] yAxesSpO2 = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the low SpO2 duration Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] xAxesLowSpO2Duration = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the low SpO2 duration Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting, and grid line intervals.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] yAxesLowSpO2Duration = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the SpO2 coverage rate Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting, and grid line intervals.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] yAxesSpO2CoverageRate = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the SpO2 coverage rate Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] xAxesSpO2CoverageRate = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the inclusive start date for the oxygen saturation data filter.
        /// This property determines the earliest record to be displayed in the charts and lists.
        /// </summary>
        [ObservableProperty]
        private DateTime startDate;

        /// <summary>
        /// Gets or sets the inclusive end date for the oxygen saturation data filter.
        /// This property defines the latest point in time for which records are retrieved 
        /// and displayed in the UI components.
        /// </summary>
        [ObservableProperty]
        private DateTime endDate;

        /// <summary>
        /// Minimum date of all available oxygen saturation records for the selected person. This value is used to set 
        /// the lower bound of the date range filter and to initialize the StartDate property on first load.
        /// </summary>
        public DateTime minMeasurementsDate;

        /// <summary>
        /// Maximum date of all available oxygen saturation records for the selected person. This value is used to set the upper bound of the date range 
        /// filter and to initialize the EndDate property on first load.
        /// </summary>
        public DateTime maxMeasurementsDate;

        /// <summary>
        /// Gets or sets a value indicating whether mean SpO2 metrics are displayed.
        /// </summary>
        [ObservableProperty]
        private bool showSpO2 = true;

        /// <summary>
        /// Gets or sets a value indicating whether minimum SpO2 metrics are displayed.
        /// </summary>
        [ObservableProperty]
        private bool showMinSpO2 = true;

        /// <summary>
        /// Gets or sets a value indicating whether maximum SpO2 metrics are displayed.
        /// </summary>
        [ObservableProperty]
        private bool showMaxSpO2 = true;

        /// <summary>
        /// Gets or sets a value indicating whether SpO2 coverage rate metrics are displayed.
        /// </summary>
        [ObservableProperty]
        private bool showSpO2CoverageRate = true;

        /// <summary>
        /// Gets or sets a value indicating whether low SpO2 duration metrics are displayed.
        /// </summary>
        [ObservableProperty]
        private bool showLowSpO2Duration = true;

        /// <summary>
        /// Gets or sets a value indicating whether the mean SpO2 trend line is displayed.
        /// </summary>
        [ObservableProperty]
        private bool showSpO2Trend = false;

        /// <summary>
        /// Gets or sets a value indicating whether the minimum SpO2 trend line is displayed.
        /// </summary>
        [ObservableProperty]
        private bool showMinSpO2Trend = false;

        /// <summary>
        /// Gets or sets a value indicating whether the maximum SpO2 trend line is displayed.
        /// </summary>
        [ObservableProperty]
        private bool showMaxSpO2Trend = false;

        /// <summary>
        /// Gets or sets a value indicating whether the SpO2 coverage rate trend line is displayed.
        /// </summary>
        [ObservableProperty]
        private bool showSpO2CoverageRateTrend = false;

        /// <summary>
        /// Gets or sets a value indicating whether the low SpO2 duration trend line is displayed.
        /// </summary>
        [ObservableProperty]
        private bool showLowSpO2DurationTrend = false;

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
        /// Constants for the geometry size of the data points in the chart. Setting this to 0 effectively hides the individual point markers.
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
        /// Initializes a new instance of the <see cref="OxygenSaturationChartViewModel"/> class.
        /// </summary>
        /// <param name="db">The <see cref="DatabaseService"/> instance used for data retrieval.</param>
        public OxygenSaturationChartViewModel(DatabaseService db)
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
        /// Asynchronously fetches the latest oxygen saturation records for the selected person from the database 
        /// and triggers a subsequent reload and re-rendering of the associated charts.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RefreshChartDataAsync()
        {
            data = await databaseService.GetSpO2DashboardSqlAsync(AppState.SelectedPersonId);
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
        /// It queries oxygen saturation data, filters them by date, and conditionally creates 
        /// normal data series or smoothed LOESS trend series depending on the global settings.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ReloadChartAsync()
        {
            if (!await reloadLock.WaitAsync(0)) return;

            if (AppState.SelectedPersonId <= 0)
            {
                GeneralInfoMessage = "No person selected. Please select a person to view their oxygen saturation data.";
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

                var spo2Result = ChartTemplateService.CreateSpO2Chart(StartDate, EndDate, source, isTrendLineLegendVisible,
                    strokeThickness, geometrySize, LoessFraction, ShowSpO2, ShowSpO2Trend, ShowMaxSpO2, ShowMaxSpO2Trend, ShowMinSpO2, ShowMinSpO2Trend);

                var spo2CoverageResult = ChartTemplateService.CreateSpO2CoverageChart(StartDate, EndDate, source, isTrendLineLegendVisible,
                    strokeThickness, geometrySize, LoessFraction, ShowSpO2CoverageRate, ShowSpO2CoverageRateTrend);

                var lowSpo2DurationResult = ChartTemplateService.CreateLowSpO2DurationChart(StartDate, EndDate, source, isTrendLineLegendVisible,
                    strokeThickness, geometrySize, LoessFraction, ShowLowSpO2Duration, ShowLowSpO2DurationTrend);

                SeriesSpO2 = spo2Result.Series;
                XAxesSpO2 = spo2Result.XAxis;
                YAxesSpO2 = spo2Result.YAxis;

                SeriesSpO2CoverageRate = spo2CoverageResult.Series;
                XAxesSpO2CoverageRate = spo2CoverageResult.XAxis;
                YAxesSpO2CoverageRate = spo2CoverageResult.YAxis;

                SeriesLowSpO2Duration = lowSpo2DurationResult.Series;
                XAxesLowSpO2Duration = lowSpo2DurationResult.XAxis;
                YAxesLowSpO2Duration = lowSpo2DurationResult.YAxis;
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
        /// Resets the date range to encompass all available oxygen saturation data.
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