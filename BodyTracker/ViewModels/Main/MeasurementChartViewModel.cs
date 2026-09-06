using BodyTracker.Models;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels
{

    public partial class MeasurementChartViewModel : ObservableObject
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service provides the low-level infrastructure for all SQL Server interactions, 
        /// including CRUD operations for body metrics and dimensions.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// The backing collection storing the complete list of comprehensive body measurement records.
        /// </summary>
        private List<FullBodyMeasurementDatasModel> data;

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty] private ISeries[] seriesBodyWeigth = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty] private ISeries[] seriesBodyFatMuscle = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty] private ISeries[] seriesBodyWater = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty] private ICartesianAxis[] xAxesBodyWeight = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting (e.g., "kg" or "%"), and grid line intervals.
        /// </summary>
        [ObservableProperty] private ICartesianAxis[] yAxesBodyWeight = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty] private ICartesianAxis[] xAxesBodyFatMuscle = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting (e.g., "kg" or "%"), and grid line intervals.
        /// </summary>
        [ObservableProperty] private ICartesianAxis[] yAxesBodyFatMuscle = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty] private ICartesianAxis[] xAxesBodyWater = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting (e.g., "kg" or "%"), and grid line intervals.
        /// </summary>
        [ObservableProperty] private ICartesianAxis[] yAxesBodyWater = Array.Empty<ICartesianAxis>();

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

        /// <summary>
        /// Minimum date of all available measurements for the selected person. This value is used to set 
        /// the lower bound of the date range filter and to initialize the StartDate property on first load.
        /// </summary>
        public DateTime minMeasurementsDate;

        /// <summary>
        /// Maximum date of all available measurements for the selected person. This value is used to set the upper bound of the date range 
        /// filter and to initialize the EndDate property on first load.
        /// </summary>
        public DateTime maxMeasurementsDate;

        /// <summary>
        /// Gets or sets a boolean flag indicating whether the body weight metric should be displayed in the chart.
        /// </summary>
        [ObservableProperty] public bool showBodyWeight = true;

        /// <summary>
        /// Gets or sets a boolean flag indicating whether the body water metric should be displayed in the chart.
        /// </summary>  
        [ObservableProperty] public bool showBodyWater = true;

        /// <summary>
        /// Gets or sets a value indicating whether the body muscle is displayed.
        /// </summary>
        [ObservableProperty] public bool showBodyMuscle = true;

        /// <summary>
        /// Gets or sets a value indicating whether body fat information is displayed.
        /// </summary>
        [ObservableProperty] public bool showBodyFat = true;

        /// <summary>
        /// Gets or sets a boolean flag indicating whether the LOESS trend line for body weight should be displayed in the chart.
        /// </summary>
        [ObservableProperty] public bool showBodyWeightTrend = false;

        /// <summary>
        /// // Gets or sets a boolean flag indicating whether the LOESS trend line for body water should be displayed in the chart.
        /// </summary>
        [ObservableProperty] public bool showBodyWaterTrend = false;

        /// <summary>
        /// Gets or sets a boolean flag indicating whether the LOESS trend line for body muscle should be displayed in the chart.
        /// </summary>
        [ObservableProperty] public bool showBodyMuscleTrend = false;

        /// <summary>
        /// Gets or sets a boolean flag indicating whether the LOESS trend line for body fat should be displayed in the chart.
        /// </summary>
        [ObservableProperty] public bool showBodyFatTrend = false;

        /// <summary>
        /// Gets or sets the fraction parameter for the LOESS smoothing algorithm, which determines 
        /// the degree of local averaging applied to the trend line.
        /// </summary>
        [ObservableProperty] private double loessFraction = 0.5;

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
        /// constants for the geometry size of the data points in the chart. Setting this to 1 effectively hides the individual point markers,
        /// </summary>
        private static int geometrySize = 0;

        /// <summary>
        /// Constants for the visibility of trend lines in the chart legend. Setting this to false will hide the trend line entries from the legend,
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
            get => GeneralInfoMessage;
            set
            {
                generalInfoMessage = value;
                WeakReferenceMessenger.Default.Send(new DatabaseErrorMessage(generalInfoMessage));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementChartViewModel"/> class.
        /// Sets up the database dependency and configures the default Cartesian axes 
        /// for time-series data visualization.
        /// </summary>
        /// <param name="db">The database service instance used to retrieve bodyMeasurement data for chart generation.</param>
        /// <remarks>
        /// During initialization, a <see cref="DateTimeAxis"/> is established as the primary X-axis. 
        /// It is configured with a one-day interval and a German date format (dd.MM.yyyy) 
        /// to ensure that body measurements are plotted accurately over time.
        /// </remarks>
        public MeasurementChartViewModel(DatabaseService db)
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
        /// Asynchronously fetches the latest body measurement records for the selected person from the database 
        /// and triggers a subsequent reload and re-rendering of the associated charts.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RefreshChartDataAsync()
        {
            try
            {
                data = await databaseService.GetBodyMeasurementAsync(AppState.SelectedPersonId, databaseService.DatabaseCommands.GetPersonMeasurementsSql());
                await ReloadChartAsync();
            }
            catch(Exception ex)
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
        /// It queries body bodyMeasurement metrics (weight, water, muscle, fat), filters them by date, and conditionally creates 
        /// normal data series or smoothed LOESS trend series depending on the global settings.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ReloadChartAsync()
        {
            if (!await reloadLock.WaitAsync(0)) return;

            try
            {

                if (data == null) return;

                minMeasurementsDate = data[0].MeasurementDate;
                maxMeasurementsDate = data[0].MeasurementDate;

                foreach (var item in data)
                {
                    if (item.MeasurementDate < minMeasurementsDate)
                        minMeasurementsDate = item.MeasurementDate;

                    if (item.MeasurementDate > maxMeasurementsDate)
                        maxMeasurementsDate = item.MeasurementDate;
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
                .Where(d => d.MeasurementDate >= StartDate &&
                            d.MeasurementDate <= EndDate)
                .OrderBy(d => d.MeasurementDate)
                .ToList();

                var resultBodyWeight=ChartTemplateService.CreateWeightChart(StartDate, EndDate, source, isTrendLineLegendVisible,
                                                                                           strokeThickness, geometrySize, loessFraction,
                                                                                           ShowBodyWeight, ShowBodyWeightTrend);

                var resultBodyFatMuscle = ChartTemplateService.CreateBodyMuscleFatPercantageChart(StartDate, EndDate, source, isTrendLineLegendVisible,
                                                                                                      strokeThickness, geometrySize, loessFraction, ShowBodyFat, ShowBodyFatTrend, ShowBodyMuscle, ShowBodyMuscleTrend);


                var resultBodyWater = ChartTemplateService.CreateBodyWaterChart(StartDate, EndDate, source, isTrendLineLegendVisible,
                                                                                       strokeThickness, geometrySize, loessFraction, ShowBodyWater, ShowBodyWaterTrend);


                    SeriesBodyWeigth = resultBodyWeight.Series;
                    XAxesBodyWeight = resultBodyWeight.XAxis;
                    YAxesBodyWeight = resultBodyWeight.YAxis;

                    SeriesBodyFatMuscle = resultBodyFatMuscle.Series;
                    XAxesBodyFatMuscle = resultBodyFatMuscle.XAxis;
                    YAxesBodyFatMuscle = resultBodyFatMuscle.YAxis;

                    SeriesBodyWater = resultBodyWater.Series;
                    XAxesBodyWater = resultBodyWater.XAxis;
                    YAxesBodyWater = resultBodyWater.YAxis;

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
            StartDate = new DateTime(
                DateTime.Today.Year, 1, 1);

            EndDate = DateTime.Today;
        }

        /// <summary>
        /// Sets the date range to cover from the first day of the current month up to today.
        /// </summary>
        private void SetActualMonth()
        {
            StartDate = new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);

            EndDate = DateTime.Today;
        }

        /// <summary>
        /// Sets the date range to cover from the start of the current week (Monday) up to today.
        /// </summary>
        private void SetActualWeek()
        {
            var today = DateTime.Today;

            // Abstand zum Montag berechnen (Montag = 0, Dienstag = 1, ..., Sonntag = 6)
            int diff = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0)
            {
                diff += 7; // Falls heute Sonntag ist
            }

            // Start ist immer der Montag dieser Woche
            StartDate = today.AddDays(-diff);

            // Ende ist immer der Sonntag dieser Woche (Montag + 6 Tage)
            EndDate = StartDate.AddDays(6);

        }

        /// <summary>
        /// Resets the date range to encompass all available bodyMeasurement data.
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

