using BodyTracker.Models;
using BodyTracker.Models.WorkoutLog;
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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
        public IAsyncRelayCommand CommandRefresh { get; }

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
        /// Gets or sets the series collection used to render the muscle distribution spider chart.
        /// </summary>
        [ObservableProperty] 
        private ISeries[] seriesMuscleDistributionSpiderChart = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the angle axis configuration for the muscle distribution spider chart.
        /// </summary>
        [ObservableProperty] 
        private IPolarAxis[] angleAxisMuscleDistirbutionSpiderChart = Array.Empty<IPolarAxis>();

        /// <summary>
        /// Gets or sets the radius axis configuration for the muscle distribution spider chart.
        /// </summary>
        [ObservableProperty] 
        private IPolarAxis[] radiusAxisMuscleDistirbutionSpiderChart = Array.Empty<IPolarAxis>();

        /// <summary>
        /// Gets or sets the inclusive start date for the active data filter.
        /// This property determines the earliest record to be displayed in the charts and lists.
        /// </summary>
        [ObservableProperty] 
        private DateTime startDate;

        /// <summary>
        /// Gets or sets the inclusive end date for the active data filter.
        /// This property defines the latest point in time for which records are retrieved 
        /// and displayed in the UI components.
        /// </summary>
        [ObservableProperty] 
        private DateTime endDate;

        /// <summary>
        /// Gets or sets the inclusive start date for the previous comparison period filter.
        /// </summary>
        [ObservableProperty] 
        private DateTime prevStartDate;

        /// <summary>
        /// Gets or sets the inclusive end date for the previous comparison period filter.
        /// </summary>
        [ObservableProperty] 
        private DateTime prevEndDate;

        /// <summary>
        /// Gets or sets the formatted string representing the total cumulative workout volume.
        /// </summary>
        /// <remarks>Displays the aggregate lifting volume formatted with localized thousand separators and unit indicators for direct UI binding.</remarks>
        [ObservableProperty] 
        private string totalWorkoutsVolume;

        /// <summary>
        /// Gets or sets the formatted string representing the total primary muscle workout volume.
        /// </summary>
        /// <remarks>Displays the weighted volume attributed to primary muscle groups, formatted for direct UI display.</remarks>
        [ObservableProperty] 
        private string totalWorkoutsPrimaryVolume;

        /// <summary>
        /// Gets or sets the formatted string representing the total secondary muscle workout volume.
        /// </summary>
        /// <remarks>Displays the weighted volume attributed to secondary muscle groups, formatted for direct UI display.</remarks>
        [ObservableProperty] 
        private string totalWorkoutsSecondaryVolume;

        /// <summary>
        /// Gets or sets the formatted string representing the total count of performed workouts or exercises.
        /// </summary>
        /// <remarks>Displays the aggregate frequency count formatted with numerical separators and unit indicators for the dashboard view.</remarks>
        [ObservableProperty] 
        private string totalWorkouts;

        /// <summary>
        /// Gets or sets the collection of chart series used to render the workout muscle distribution visualization.
        /// </summary>
        /// <remarks>Holds the configured pie series data representing the proportional workload share across different muscle groups.</remarks>
        [ObservableProperty] 
        private IEnumerable<ISeries> workoutMuscleDistributionSeries;

        /// <summary>
        /// Gets or sets the collection of frequently performed exercises displayed in the UI.
        /// </summary>
        /// <remarks>Provides an observable list of exercise frequency statistics used to populate ranking lists or summary grids.</remarks>
        [ObservableProperty] 
        private ObservableCollection<ExerciseFrequencyModel> topExercises;

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty] 
        private ISeries[] seriesMonthlyTraningsVolume = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty] 
        private ICartesianAxis[] xAxesMonthlyTraningsVolume = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting (e.g., "kg" or "%"), and grid line intervals.
        /// </summary>
        [ObservableProperty] 
        private ICartesianAxis[] yAxesMonthlyTraningsVolume = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty] 
        private ISeries[] seriesWorkoutPeakProgress = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty] 
        private ICartesianAxis[] xAxesWorkoutPeakProgress = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting (e.g., "kg" or "%"), and grid line intervals.
        /// </summary>
        [ObservableProperty] 
        private ICartesianAxis[] yAxesWorkoutPeakProgress = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty] 
        private ISeries[] seriesWorkoutTotalVolumeProgress = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty] 
        private ICartesianAxis[] xAxesWorkoutTotalVolumeProgress = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting (e.g., "kg" or "%"), and grid line intervals.
        /// </summary>
        [ObservableProperty] 
        private ICartesianAxis[] yAxesWorkoutTotalVolumeProgress = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the collection of data series to be displayed in the chart.
        /// This property is observable, meaning any changes to the series (e.g., adding or removing metrics) 
        /// will automatically trigger a UI update in the view.
        /// </summary>
        [ObservableProperty] 
        private ISeries[] seriesWorkoutMaxRepProgress = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the X-axes configuration for the Cartesian chart.
        /// This property defines the horizontal scale, including labels (e.g., dates), 
        /// unit spacing, and title formatting.
        /// </summary>
        [ObservableProperty]
        private ICartesianAxis[] xAxesWorkoutMaxRepProgress = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axes configuration for the Cartesian chart.
        /// This property defines the vertical scale, including the numerical range, 
        /// value formatting (e.g., "kg" or "%"), and grid line intervals.
        /// </summary>
        [ObservableProperty] 
        private ICartesianAxis[] yAxesWorkoutMaxRepProgress = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the observable collection of available exercise names for selection.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> exercisesList = new ObservableCollection<string>();

        /// <summary>
        /// Gets or sets the currently selected exercise item name.
        /// </summary>
        [ObservableProperty]
        private string? selectedExerciseItem;

        /// <summary>
        /// Holds the unindexed list of progress records for the selected exercise.
        /// </summary>
        private List<WorkoutExerciseProgressModel> exerciseProgressModel = new List<WorkoutExerciseProgressModel>();

        /// <summary>
        /// Gets or sets the fraction parameter for the LOESS smoothing algorithm, which determines 
        /// the degree of local averaging applied to the trend line.
        /// </summary>
        [ObservableProperty] 
        private double loessFraction = 0.5;

        /// <summary>
        /// Gets or sets the collection of mean full body bodyMeasurement data.
        /// </summary>
        /// <remarks>The collection is observable, allowing UI elements or other components to react to
        /// changes such as additions or removals of bodyMeasurement data. This property is typically used for data binding
        /// scenarios.</remarks>
        [ObservableProperty]
        private ObservableCollection<FullBodyMeasurementDatasModel> meanMeasurement = new();

        /// <summary>
        /// Gets or sets the fraction parameter for the LOESS smoothing algorithm specifically applied to the monthly training volume chart.
        /// </summary>
        [ObservableProperty] 
        private double loessFractionMonthlyTraningsVolume = 0.5;

        /// <summary>
        /// Gets or sets a value indicating whether the workload progress visualization is shown.
        /// </summary>
        [ObservableProperty]
        private bool showWorkloadProgress = true;

        /// <summary>
        /// Gets or sets a value indicating whether the workload progress trend line is shown.
        /// </summary>
        [ObservableProperty]
        private bool showWorkloadProgressTrend = false;

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
        /// A flag indicating whether a data refresh operation is currently in progress.
        /// </summary>
        /// <remarks>Acts as a concurrency guard to prevent overlapping asynchronous refresh cycles and avoid redundant database queries.</remarks>
        private bool isRefreshing = false;

        /// <summary>
        /// Stores the label string representing the current time period for charts.
        /// </summary>
        private string labelChartCurr = string.Empty;

        /// <summary>
        /// Stores the label string representing the previous comparison period for charts.
        /// </summary>
        private string labelChartPrev = string.Empty;

        /// <summary>
        /// Stores the active filter name string.
        /// </summary>
        private string filterName = string.Empty;

        /// <summary>
        /// Holds the instance of the workout load analyzer used for calculations.
        /// </summary>
        private AppWorkoutLoadAnalyzer analyzer;

        /// <summary>
        /// A synchronization primitive used to ensure that data reloading or refresh operations 
        /// are thread-safe and prevent concurrent or overlapping executions.
        /// </summary>
        private readonly SemaphoreSlim reloadLock = new(1, 1);

        private bool prevValuesAvailable = false;


        /// <summary>
        /// Initializes a new instance of the <see cref="WorkoutInsightsViewModel"/> class with the specified main window shell and database service.
        /// </summary>
        /// <remarks>Assigns service dependencies and initiates the initial asynchronous data load upon view activation.</remarks>
        /// <param name="shell">The reference to the primary application window instance.</param>
        /// <param name="db">The database service instance used for data retrieval operations.</param>
        public WorkoutInsightsViewModel(DatabaseService db)
        {
            databaseService = db;
            
            CommandRefresh = new AsyncRelayCommand(ReloadAsync);
            CommandSetActualYear = new RelayCommand(SetActualYear);
            CommandSetActualMonth = new RelayCommand(SetActualMonth);
            CommandSetActualWeek = new RelayCommand(SetActualWeek);

            _ = InitializeAsync();
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
            try
            {

                suppressReload = true;

                var list = await databaseService.GetHeavyAppWorkoutsAsync(AppState.SelectedPersonId);


                analyzer = new AppWorkoutLoadAnalyzer("", list, 1, 0.5);


                suppressReload = false;

                if (StartDate == default || EndDate == default)
                {
                    SetActualYear();
                }
                else
                {
                    await RefreshChartAsync();
                }
            }
            catch (Exception ex)
            {
                GeneralInfoMessage = $"Error initializing Gym dashboard: {ex.Message}";
            }
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

            if (suppressReload) return;


            if (AppState.SelectedPersonId <= 0) return;


            if (analyzer == null) return;


            if (!await reloadLock.WaitAsync(0)) return;
            try
            {
                await Task.WhenAll(
                    GetAppDashboardValuesAsync(analyzer, StartDate, EndDate),
                    GetChartMuscleDistributionSpiderChart(analyzer, StartDate, EndDate),
                    GetChartMonthlyTraningsVolumeAsync(analyzer, StartDate, EndDate),
                    GetWorkoutProgressChart(analyzer, StartDate, EndDate)
               );
            }
            catch (Exception ex)
            {
                GeneralInfoMessage = $"Error refreshing dashboard: {ex.Message}";
                MessageBox.Show(ex.Message);
            }
            finally
            {
                reloadLock.Release();
            }
        }

        /// <summary>
        /// Calculates the muscle distribution for the specified date range and the preceding month, then initializes the corresponding spider chart series and axes.
        /// </summary>
        /// <param name="analyzer">The workout load analyzer instance used to calculate muscle splits.</param>
        /// <param name="_startDate">The start date for the current distribution period.</param>
        /// <param name="_endDate">The end date for the current distribution period.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetChartMuscleDistributionSpiderChart(AppWorkoutLoadAnalyzer analyzer, DateTime _startDate, DateTime _endDate)
        {
            var prevStart = _startDate.AddMonths(-1);
            var prevEnd = prevStart.AddMonths(1).AddDays(-1);

            var actualMuscleDistribution = analyzer.CalculateMuscleSplit(analyzer.WorkoutEntries, startDate: _startDate, endDate: _endDate);
            var previousMuscleDistribution = analyzer.CalculateMuscleSplit(analyzer.WorkoutEntries, startDate: prevStart, endDate: prevEnd);
            string currlabel = "";
            string prevlabel = "";
            if (!previousMuscleDistribution.Any()) { prevlabel = ""; }
            else { currlabel = labelChartCurr; prevlabel = labelChartPrev; }


            (SeriesMuscleDistributionSpiderChart, AngleAxisMuscleDistirbutionSpiderChart, RadiusAxisMuscleDistirbutionSpiderChart) =
                    ChartTemplateService.CreateWorkloadMuscleSpiderChart(
                        actualMuscleDistribution, previousMuscleDistribution,
                        currlabel, prevlabel, geometrySize: 0, strokeThickness: 1);
        }

        /// <summary>
        /// Processes workout progress data for a selected exercise within a date range and populates the peak weight, maximum repetition, and total volume chart properties.
        /// </summary>
        /// <param name="analyzer">The workout load analyzer instance used to compute exercise progress.</param>
        /// <param name="startDate">The start date defining the evaluation window.</param>
        /// <param name="endDate">The end date defining the evaluation window.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetWorkoutProgressChart(AppWorkoutLoadAnalyzer analyzer, DateTime startDate, DateTime endDate)
        {
            var results = analyzer.CalculateExerciseProgress(analyzer.WorkoutEntries, startDate, endDate);
            var names = results.GroupBy(x => x.ExerciseName).Select(x => x.Key).ToList();

            if (firstLoad)
            {
                ExercisesList.Clear();
                ExercisesList = new ObservableCollection<string>(names);
                if (SelectedExerciseItem == null || !ExercisesList.Contains(SelectedExerciseItem))
                {
                    SelectedExerciseItem = ExercisesList.FirstOrDefault();
                }
                firstLoad = false;
            }
            
            if (SelectedExerciseItem == null) return;

            exerciseProgressModel.Clear();

            foreach (var result in results)
            {
                if (result.ExerciseName == SelectedExerciseItem)
                    exerciseProgressModel.Add(result);
            }

            exerciseProgressModel = exerciseProgressModel
                .Where(d => d.Date >= startDate && d.Date <= endDate)
                .OrderBy(x => x.Date).ToList();

            var peakWeightProgress = ChartTemplateService.CreateWorkoutPeakWeightBarChart(exerciseProgressModel, isTrendLineLegendVisible, 
                strokeThickness, geometrySize, SelectedExerciseItem, 90);

            var totalVolumeProgress = ChartTemplateService.CreateWorkoutTotalVolumeBarChart(exerciseProgressModel, isTrendLineLegendVisible, 
                strokeThickness, geometrySize, SelectedExerciseItem, 90);

            var maxRepProgress = ChartTemplateService.CreateWorkoutRepChart(exerciseProgressModel, isTrendLineLegendVisible, 
                strokeThickness, geometrySize, SelectedExerciseItem, LoessFraction, true, false);

            SeriesWorkoutPeakProgress = peakWeightProgress.Series;
            XAxesWorkoutPeakProgress = peakWeightProgress.XAxis;
            YAxesWorkoutPeakProgress = peakWeightProgress.YAxis;

            SeriesWorkoutMaxRepProgress = maxRepProgress.Series;
            XAxesWorkoutMaxRepProgress = maxRepProgress.XAxis;
            YAxesWorkoutMaxRepProgress = maxRepProgress.YAxis;

            SeriesWorkoutTotalVolumeProgress = totalVolumeProgress.Series;
            XAxesWorkoutTotalVolumeProgress = totalVolumeProgress.XAxis;
            YAxesWorkoutTotalVolumeProgress = totalVolumeProgress.YAxis;
        }

        /// <summary>
        /// Validates the active person ID and retrieves aggregated dashboard workload values and exercise frequencies based on an adjusted date range.
        /// </summary>
        /// <param name="analyzer">The workout load analyzer instance used to aggregate volume and frequency metrics.</param>
        /// <param name="startDate">The base start date for the dashboard period.</param>
        /// <param name="endDate">The end date for the dashboard period.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetAppDashboardValuesAsync(AppWorkoutLoadAnalyzer analyzer, DateTime startDate, DateTime endDate)
        {
            var totalWorkoutVolume = analyzer.GetVolume(analyzer.WorkoutEntries, startDate: startDate, endDate: endDate);

            TotalWorkoutsVolume = totalWorkoutVolume.TotalVolume.ToString("N0", new System.Globalization.CultureInfo("de-DE")) + " kg";
            TotalWorkoutsPrimaryVolume = totalWorkoutVolume.PrimaryVolume.ToString("N0", new System.Globalization.CultureInfo("de-DE")) + " kg";
            TotalWorkoutsSecondaryVolume = totalWorkoutVolume.SecondaryVolume.ToString("N0", new System.Globalization.CultureInfo("de-DE")) + " kg";
            TotalWorkouts = totalWorkoutVolume.TotalExercises.ToString("N0", new System.Globalization.CultureInfo("de-DE")) + " x";

            TopExercises = new ObservableCollection<ExerciseFrequencyModel>(
                analyzer.CalculateExerciseFrequency(analyzer.WorkoutEntries, startDate: startDate, endDate: endDate));
        }

        /// <summary>
        /// Asynchronously calculates monthly training volume data over an adjusted date range and configures the series and axis properties for the monthly volume chart.
        /// </summary>
        /// <param name="analyzer">The workout load analyzer instance used to calculate monthly volume.</param>
        /// <param name="startDate">The base start date for the chart period.</param>
        /// <param name="endDate">The end date for the chart period.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task GetChartMonthlyTraningsVolumeAsync(AppWorkoutLoadAnalyzer analyzer, DateTime startDate, DateTime endDate)
        {
            if (analyzer == null) return;

            var adjustedStartDate = startDate.AddMonths(-1);
            var results = await analyzer.CalculateMonthlyVolumeAsync(adjustedStartDate, endDate);
            var result = ChartTemplateService.CreateWorkloadMonthlyVolumeChart(adjustedStartDate, endDate, results, isTrendLineLegendVisible, 
                strokeThickness, geometrySize, LoessFractionMonthlyTraningsVolume, true, true);

            SeriesMonthlyTraningsVolume = result.Series;
            XAxesMonthlyTraningsVolume = result.XAxis;
            YAxesMonthlyTraningsVolume = result.YAxis;
        }

        /// <summary>
        /// Sets the date range to the current calendar year, from January 1st to December 31st.
        /// </summary>
        private void SetActualYear()
        {
            DateTime today = DateTime.Today;

            // Current year range
            StartDate = new DateTime(today.Year, 1, 1);
            EndDate = new DateTime(today.Year, 12, 31);

            // Previous year range (shifted back by 1 year)
            PrevStartDate = StartDate.AddYears(-1);
            PrevEndDate = EndDate.AddYears(-1);

            // Labels (e.g., "2026" and "2025" or custom format depending on your preference)
            labelChartCurr = StartDate.ToString("yyyy", CultureInfo.InvariantCulture);
            labelChartPrev = PrevStartDate.ToString("yyyy", CultureInfo.InvariantCulture);
            _ = RefreshChartAsync();
        }

        /// <summary>
        /// Sets the date range to the current calendar month, from the first to the last day of the month.
        /// </summary>
        private void SetActualMonth()
        {
            DateTime today = DateTime.Today;

            // Current month range
            StartDate = new DateTime(today.Year, today.Month, 1);
            EndDate = StartDate.AddMonths(1).AddDays(-1);

            // Previous month range (shifted back by 1 month)
            PrevStartDate = StartDate.AddMonths(-1);
            PrevEndDate = EndDate.AddMonths(-1);

            // Labels formatted without dots (e.g., "August 2026" and "July 2026")
            labelChartCurr = StartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
            labelChartPrev = PrevStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
            _ = RefreshChartAsync();
        }

        /// <summary>
        /// Sets the date range to the current week, starting on Monday and ending on Sunday.
        /// </summary>
        private void SetActualWeek()
        {
            DateTime today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;

            // Current week range (Monday to Sunday)
            StartDate = today.AddDays(-1 * diff);
            EndDate = StartDate.AddDays(6);

            // Previous week range (shifted back by 7 days)
            PrevStartDate = StartDate.AddDays(-7);
            PrevEndDate = EndDate.AddDays(-7);

            // Labels showing the date span or custom week representation
            labelChartCurr = $"{StartDate:dd MMM} - {EndDate:dd MMM yyyy}";
            labelChartPrev = $"{PrevStartDate:dd MMM} - {PrevEndDate:dd MMM yyyy}";
            _ = RefreshChartAsync();
        }

        /// <summary>
        /// Configures the current and previous date ranges and chart labels while suppressing intermediate reloads, then triggers an asynchronous chart refresh.
        /// </summary>
        /// <param name="start">The start date for the current period.</param>
        /// <param name="end">The end date for the current period.</param>
        /// <param name="currentLabel">The label identifier for the current period.</param>
        /// <param name="prevLabel">The label identifier for the previous comparison period.</param>
        private void SetDateRange(DateTime start, DateTime end, string currentLabel, string prevLabel)
        {
            suppressReload = true;
            StartDate = start;
            EndDate = end;
            PrevStartDate = start.AddYears(start == StartDate ? -1 : 0);
            PrevEndDate = end.AddYears(end == EndDate ? -1 : 0);
            labelChartCurr = currentLabel;
            labelChartPrev = prevLabel;
            suppressReload = false;

            _ = RefreshChartAsync();
        }

        /// <summary>
        /// Handles changes to the start date property and triggers an asynchronous chart refresh if reload suppression is disabled.
        /// </summary>
        /// <param name="value">The new start date value.</param>
        partial void OnStartDateChanged(DateTime value)
        {
            if (!suppressReload)
            {
                _ = RefreshChartAsync();
            }
        }

        /// <summary>
        /// Handles changes to the end date property and triggers an asynchronous chart refresh if reload suppression is disabled.
        /// </summary>
        /// <param name="value">The new end date value.</param>
        partial void OnEndDateChanged(DateTime value)
        {
            if (!suppressReload)
            {
               _ = RefreshChartAsync();
            }
        }

        /// <summary>
        /// Handles changes to the selected exercise item by logging the selection and fetching the corresponding workout progress chart data.
        /// </summary>
        /// <param name="value">The name or identifier of the newly selected exercise.</param>
        partial void OnSelectedExerciseItemChanged(string? value)
        {
            Debug.WriteLine("Exercise Selected: " + value);
            _ = GetWorkoutProgressChart(analyzer, StartDate, EndDate);
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