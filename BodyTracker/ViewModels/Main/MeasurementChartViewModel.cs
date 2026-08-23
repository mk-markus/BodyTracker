using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

        #region Observable Property Member

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
        [ObservableProperty] public bool showBodyWeightTrend = true;

        /// <summary>
        /// // Gets or sets a boolean flag indicating whether the LOESS trend line for body water should be displayed in the chart.
        /// </summary>
        [ObservableProperty] public bool showBodyWaterTrend = true;

        /// <summary>
        /// Gets or sets a boolean flag indicating whether the LOESS trend line for body muscle should be displayed in the chart.
        /// </summary>
        [ObservableProperty] public bool showBodyMuscleTrend = true;

        /// <summary>
        /// Gets or sets a boolean flag indicating whether the LOESS trend line for body fat should be displayed in the chart.
        /// </summary>
        [ObservableProperty] public bool showBodyFatTrend = true;

        /// <summary>
        /// Gets or sets the fraction parameter for the LOESS smoothing algorithm, which determines 
        /// the degree of local averaging applied to the trend line.
        /// </summary>
        [ObservableProperty] private double loessFraction = 0.5;

        #endregion

        /// <summary>
        /// Gets the command that triggers an asynchronous refresh of the chart data based on the current filter settings.
        /// </summary>
        public IAsyncRelayCommand RefreshChart { get; }

        /// <summary>
        /// 
        /// </summary>
        public IRelayCommand SetActualYearCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public IRelayCommand SetActualMonthCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public IRelayCommand SetActualWeekCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public IRelayCommand ShowAllDataCommand { get; }

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
            XAxes = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yyyy")) { Name = "Date" } };
            RefreshChart = new AsyncRelayCommand(RefreshChartAsync);
            SetActualYearCommand = new RelayCommand(SetActualYear);
            SetActualMonthCommand = new RelayCommand(SetActualMonth);
            SetActualWeekCommand = new RelayCommand(SetActualWeek);
            ShowAllDataCommand = new RelayCommand(ShowAllData);

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
        partial void OnStartDateChanged(DateTime value)
        {
            _ = RefreshChartAsync();
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
        partial void OnEndDateChanged(DateTime value)
        {
            _ = RefreshChartAsync();
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
            {
                GeneralErrorMessage = "No person selected. Please select a person to view their body measurements.";
                return;
            }

            if (isRefreshing) return;

            isRefreshing = true;

            try
            {
                var data = await databaseService.GetBodyMeasurementAsync(AppState.SelectedPersonId, databaseService.DatabaseCommands.GetPersonMeasurementsSql());

                if (data.Count<=0) return;
                minMeasurementsDate = data.Min(d => d.MeasurementDate);
                maxMeasurementsDate = data.Max(d => d.MeasurementDate);

                if (firstLoad)
                {
                    StartDate = minMeasurementsDate;
                    EndDate = maxMeasurementsDate;
                    firstLoad = false;
                }

                (Series, XAxes, YAxes) = ChartTemplateService.CreateBodyMeasurementChart( StartDate, EndDate, data, isTrendLineLegendVisible,
                                                                                       strokeThickness, geometrySize, loessFraction,
                                                                                       ShowBodyWeight, ShowBodyWeightTrend,
                                                                                       ShowBodyFat, ShowBodyFatTrend,
                                                                                       ShowBodyMuscle, ShowBodyMuscleTrend,
                                                                                       ShowBodyWater, ShowBodyWaterTrend);

            }
            catch (Exception ex)
            {
                GeneralErrorMessage = $"Error refreshing chart: {ex.Message}";
            }
            finally
            {
                isRefreshing = false;
            }        
        }

        /// <summary>
        /// Sets the date range to cover from the first day of the current year up to today.
        /// </summary>
        private void SetActualYear()
        {
            StartDate = new DateTime(
                DateTime.Today.Year,
                1,
                1);

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

