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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace BodyTracker.ViewModels
{
    public partial class WorkoutInsightsViewModel : ObservableObject
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
        public IRelayCommand CommandSetActualYear { get; }

        /// <summary>
        /// Gets the command responsible for setting the mean start and end dates to encompass the entire current calendar month.
        /// </summary>
        public IRelayCommand CommandSetActualMonth { get; }

        /// <summary>
        /// Gets the command responsible for setting the mean start and end dates to encompass the current work week (Monday to Sunday).
        /// </summary>
        public IRelayCommand CommandSetActualWeek { get; }


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


        #region Observable Property Members

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

        // <summary>
        /// Gets or sets the inclusive start date for the bodyMeasurement data filter.
        /// This property determines the earliest record to be displayed in the charts and lists.
        /// </summary>
        [ObservableProperty] private DateTime startDateDatas;

        /// <summary>
        /// Gets or sets the inclusive end date for the bodyMeasurement data filter.
        /// This property defines the latest point in time for which records are retrieved 
        /// and displayed in the UI components.
        /// </summary>
        [ObservableProperty] private DateTime endDateDatas;


        // <summary>
        /// Gets or sets the inclusive start date for the bodyMeasurement data filter.
        /// This property determines the earliest record to be displayed in the charts and lists.
        /// </summary>
        [ObservableProperty] private DateTime prevStartDateDatas;

        /// <summary>
        /// Gets or sets the inclusive end date for the bodyMeasurement data filter.
        /// This property defines the latest point in time for which records are retrieved 
        /// and displayed in the UI components.
        /// </summary>
        [ObservableProperty] private DateTime prevEndDateDatas;

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
        /// A flag indicating whether the dashboard view is undergoing its initial load cycle.
        /// </summary>
        /// <remarks>Used to control conditional initialization tasks, such as setting default date boundaries for charts on startup.</remarks>
        private bool firstLoad = true;

        /// <summary>
        /// A flag indicating whether a data refresh operation is currently in progress.
        /// </summary>
        /// <remarks>Acts as a concurrency guard to prevent overlapping asynchronous refresh cycles and avoid redundant database queries.</remarks>
        private bool isRefreshing = false;


        private string labelChartCurr = string.Empty;

        private string labelChartPrev = string.Empty;



        /// <summary>
        /// Initializes a new instance of the <see cref="WorkoutInsightsViewModel"/> class with the specified main window shell and database service.
        /// </summary>
        /// <remarks>Assigns service dependencies and initiates the initial asynchronous data load upon view activation.</remarks>
        /// <param name="shell">The reference to the primary application window instance.</param>
        /// <param name="db">The database service instance used for data retrieval operations.</param>
        public WorkoutInsightsViewModel(MainWindow shell, DatabaseService db)
        {
            databaseService = db;

            ReloadCommand = new AsyncRelayCommand(ReloadAsync);
            CommandSetActualYear = new RelayCommand(SetActualYear);
            CommandSetActualMonth = new RelayCommand(SetActualMonth);
            CommandSetActualWeek = new RelayCommand(SetActualWeek);

           CommandSetActualYear.Execute(null);
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
           await RefreshChartAsync();

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
                var list = await databaseService.GetHeavyAppWorkoutsAsync(AppState.SelectedPersonId);
                var analyzer = new AppWorkoutLoadAnalyzer("", list, 1, 0.5);

                await GetAppDashboardValuesAsync(analyzer, StartDateDatas, EndDateDatas);
                await GetChartMuscleDistributionSpiderChart(analyzer, StartDateDatas, EndDateDatas, PrevStartDateDatas, PrevEndDateDatas);
                await GetChartMonthlyTraningsVolumeAsync(analyzer, StartDateDatas, EndDateDatas, PrevStartDateDatas, PrevEndDateDatas);
                

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
        /// Asynchronously calculates muscle distribution metrics for the specified date range and the preceding month, 
        /// then configures the corresponding spider chart series and polar axes.
        /// </summary>
        /// <param name="analyzer">The workout load analyzer instance used to compute muscle distribution data.</param>
        /// <param name="startDate">The start date for the current period analysis.</param>
        /// <param name="endDate">The end date for the current period analysis.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetChartMuscleDistributionSpiderChart(AppWorkoutLoadAnalyzer analyzer, DateTime startDate, DateTime endDate,
                                                              DateTime prevStartDate, DateTime prevEndDate)
        {
            var prevstartDate = startDate.AddMonths(-1);

            var prevendDate = prevstartDate.AddMonths(1).AddDays(-1);

            var actualMonthMuscleDistribution = analyzer.CalculateMuscleSplit(analyzer.WorkoutEntries,
                                                                              startDate: startDate,
                                                                              endDate: endDate);
            var previousMonthMuscleDistribution = analyzer.CalculateMuscleSplit(analyzer.WorkoutEntries,
                                                                                startDate: prevstartDate,
                                                                                endDate: prevendDate);

            (SeriesMuscleDistributionSpiderChart, AngleAxisMuscleDistirbutionSpiderChart, RadiusAxisMuscleDistirbutionSpiderChart) = ChartTemplateService.CreateMuscleSpiderChart(actualMonthMuscleDistribution, previousMonthMuscleDistribution,
                                                                                                                                                                                 labelChartCurr,
                                                                                                                                                                                 labelChartPrev, geometrySize: 0, strokeThickness: 1);
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
        private async Task GetChartMonthlyTraningsVolumeAsync(AppWorkoutLoadAnalyzer analyzer, DateTime startDate, DateTime endDate,
                                                              DateTime prevStartDate, DateTime prevEndDate)
        {
            if (analyzer == null) return;

            var data = analyzer.WorkoutEntries;

            startDate = startDate.AddMonths(-1);

           

            var results = await analyzer.CalculateMonthlyVolumeAsync(startDate, endDate);


            (SeriesMonthlyTraningsVolume, XAxesMonthlyTraningsVolume, YAxesMonthlyTraningsVolume) = ChartTemplateService.CreateMonthlyVolumeChart(startDate, endDate, results, isTrendLineLegendVisible,
                                                                                                                                               strokeThickness, geometrySize, LoessFractionMonthlyTraningsVolume,
                                                                                                                                               true, true);

        }

        //// <summary>
        /// Sets the mean start and end dates to encompass the entire current calendar year (January 1st to December 31st).
        /// </summary>
        private void SetActualYear()
        {
            DateTime today = DateTime.Today;
            StartDateDatas = new DateTime(today.Year, 1, 1);
            EndDateDatas = new DateTime(today.Year, 12, 31);

            PrevStartDateDatas = StartDateDatas.AddYears(-1);
            PrevEndDateDatas = EndDateDatas.AddYears(-1);

            labelChartCurr = "Actual Year";
            labelChartPrev = "Previous Year";

        }

        /// <summary>
        /// Sets the mean start and end dates to encompass the entire current calendar month (from the first day to the last day).
        /// </summary>
        private void SetActualMonth()
        {
            DateTime today = DateTime.Today;

            // First Day of Month
            StartDateDatas = new DateTime(today.Year, today.Month, 1);

            // Last day of the current month:
            // We take the first day of the next month and subtract one day.
            EndDateDatas = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);

            PrevStartDateDatas = StartDateDatas.AddMonths(-1);

            PrevEndDateDatas = PrevStartDateDatas.AddMonths(1).AddDays(-1);

            labelChartCurr = "Actual Month";
            labelChartPrev = "Previous Month";
        }

        /// <summary>
        /// Sets the mean start and end dates to encompass the current work week, assuming the week begins on Monday and ends on Sunday.
        /// </summary>
        private void SetActualWeek()
        {
            DateTime today = DateTime.Today;

            // Calculating Monday of this week (assuming the week starts on Monday)
            // DayOfWeek.Sunday is 0, Monday is 1... Saturday is 6.
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = today.AddDays(-1 * diff);

            StartDateDatas = startOfWeek;
            EndDateDatas = startOfWeek.AddDays(6); // Sunday

            PrevStartDateDatas = startOfWeek.AddDays(-7);
            PrevEndDateDatas = startOfWeek.AddDays(-1);


            labelChartCurr = "Actual Week";
            labelChartPrev = "Previous Week";
        }

        partial void OnStartDateDatasChanged(DateTime value)
        {
            _ = RefreshChartAsync();
        }

        partial void OnEndDateDatasChanged(DateTime value)
        {
           _ = RefreshChartAsync();
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