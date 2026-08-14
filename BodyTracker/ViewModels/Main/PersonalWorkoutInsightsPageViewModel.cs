using BodyTracker.Models;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
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

namespace BodyTracker.ViewModels
{
    public partial class PersonalWorkoutInsightsPageViewModel : ObservableObject
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// </summary>
        /// <remarks>
        /// Acts as the primary data gateway for all read and write operations initiated by the ViewModel.
        /// The <c>readonly</c> keyword protects the instance reference from accidental reassignment after initialization.
        /// </remarks>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// A private, read-only reference to the primary application window instance.
        /// </summary>
        /// <remarks>
        /// Provides direct access to window operations, navigation elements, or parent UI contexts from dependent components.
        /// </remarks>
        private readonly MainWindow mainWindow;

        /// <summary>
        /// Gets or sets the formatted string representation of the total workout volume.
        /// </summary>
        /// <remarks>
        /// Represents the cumulative weight sum of all completed exercises, formatted with thousand separators and unit indicators for direct UI binding.
        /// </remarks>
        [ObservableProperty] private string totalWorkoutsVolume;

        /// <summary>
        /// Gets or sets the formatted string representation of the workload volume for primary muscle groups.
        /// </summary>
        /// <remarks>
        /// Displays the aggregated lifting volume directly assigned to the primary target muscles.
        /// </remarks>
        [ObservableProperty] private string totalWorkoutsPrimaryVolume;

        /// <summary>
        /// Gets or sets the formatted string representation of the workload volume for secondary muscle groups.
        /// </summary>
        /// <remarks>
        /// Displays the proportionally calculated lifting volume assigned to synergistically cooperating secondary muscles.
        /// </remarks>
        [ObservableProperty] private string totalWorkoutsSecondaryVolume;

        /// <summary>
        /// Gets or sets the formatted count of completed workout sessions or exercise executions.
        /// </summary>
        /// <remarks>
        /// Serves as a statistical metric for overall frequency within the selected dashboard evaluation period.
        /// </remarks>
        [ObservableProperty] private string totalWorkouts;

        /// <summary>
        /// Gets or sets the collection of chart series used to visualize the muscle group distribution.
        /// </summary>
        /// <remarks>
        /// Contains the configured <see cref="ISeries"/> data (e.g., PieSeries) representing the percentage workload distribution across different muscle groups.
        /// </remarks>
        [ObservableProperty] private IEnumerable<ISeries> workoutMuscleDistributionSeries;

        /// <summary>
        /// Gets or sets the observable collection of most frequently performed exercises.
        /// </summary>
        /// <remarks>
        /// Provides a dynamically updatable list of <see cref="ExerciseFrequencyModel"/> objects used to populate ranking tables or summary grids in the UI.
        /// </remarks>
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
        /// Minimum date of all available measurements for the selected person. This value is used to set 
        /// the lower bound of the date range filter and to initialize the StartDate property on first load.
        /// </summary>
        public DateTime minMeasurementsDateMonthlyTraningsVolume;

        /// <summary>
        /// Maximum date of all available measurements for the selected person. This value is used to set the upper bound of the date range 
        /// filter and to initialize the EndDate property on first load.
        /// </summary>
        public DateTime maxMeasurementsDateMonthlyTraningsVolume;

        /// <summary>
        /// Gets or sets the fraction parameter for the LOESS smoothing algorithm, which determines 
        /// the degree of local averaging applied to the trend line.
        /// </summary>
        [ObservableProperty] private double loessFractionMonthlyTraningsVolume = 0.5;

        /// <summary>
        /// A flag indicating whether a data refresh operation is currently in progress.
        /// </summary>
        /// <remarks>Acts as a concurrency guard to prevent overlapping asynchronous refresh cycles and avoid redundant database queries.</remarks>
        private bool isRefreshing = false;

        /// <summary>
        /// A flag indicating whether the dashboard view is undergoing its initial load cycle.
        /// </summary>
        /// <remarks>Used to control conditional initialization tasks, such as setting default date boundaries for charts on startup.</remarks>
        private bool firstLoad = true;

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
        /// Initializes a new instance of the <see cref="PersonalWorkoutInsightsPageViewModel"/> class with the specified main window shell and database service.
        /// </summary>
        /// <remarks>Assigns service dependencies and initiates the initial asynchronous data load upon view activation.</remarks>
        /// <param name="shell">The reference to the primary application window instance.</param>
        /// <param name="db">The database service instance used for data retrieval operations.</param>
        public PersonalWorkoutInsightsPageViewModel(MainWindow shell, DatabaseService db)
        {
            databaseService = db;
            mainWindow = shell;

            // Initiales Laden beim Start der View
            _ = RefreshDataAsync();
        }

        /// <summary>
        /// Asynchronously retrieves heavy workout entries, processes load calculations, and updates dashboard metrics and charts.
        /// </summary>
        /// <remarks>Validates the selected person identifier, queries the database, initializes the workout load analyzer, and updates observable volume metrics, exercise frequencies, muscle distribution, and training volume charts.</remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RefreshDataAsync()
        {
            try
            {

                if (AppState.SelectedPersonId < 0) throw new Exception("The Person ID is <0");

                var list = await databaseService.GetHeavyAppWorkoutEntriesAsync(AppState.SelectedPersonId);

                // 2. Analyzer initialisieren und Konvertierung durchführen
                var analyzer = new AppWorkoutLoadAnalyzer("", list, 1, 0.5);

                TotalWorkoutsVolume = analyzer.TotalWorkoutVolume.TotalVolume.ToString("N0", new System.Globalization.CultureInfo("de-DE")) + " kg";
                TotalWorkoutsPrimaryVolume = analyzer.TotalWorkoutVolume.PrimaryVolume.ToString("N0", new System.Globalization.CultureInfo("de-DE")) + " kg";
                TotalWorkoutsSecondaryVolume = analyzer.TotalWorkoutVolume.SecondaryVolume.ToString("N0", new System.Globalization.CultureInfo("de-DE")) + " kg";

                TotalWorkouts = analyzer.TotalWorkoutVolume.TotalExercises.ToString("N0", new System.Globalization.CultureInfo("de-DE")) + " x";

                TopExercises = new ObservableCollection<ExerciseFrequencyModel>(analyzer.FrequentlyPerformedExercises);

                WorkoutMuscleDistributionSeries = await GenerateMuscleDistributionSeriesAsync(analyzer.GetMuscleDistribution(analyzer.AppWorkoutEntries));

                await GenerateMonthlyTraningsVolumeAsync(analyzer);

            }
            // Korrekte Ausnahmebehandlung, um Abstürze bei Dateizugriffen zu verhindern
            catch (Exception ex)
            {
                GeneralErrorMessage = $"An error occurred while refreshing data: {ex.Message}";
            }
        }


        /// <summary>
        /// Asynchronously generates a collection of pie chart series representing the percentage distribution of muscle volumes.
        /// </summary>
        /// <remarks>Filters out muscle groups with zero or negative total volume, maps each remaining entry to a LiveCharts <see cref="PieSeries{T}"/> configuration, assigns custom data labels and tooltips, and formats percentage values to two decimal places.</remarks>
        /// <param name="muscleDistribution">The collection of muscle data results containing volume metrics and percentage shares.</param>
        /// <returns>A task representing the asynchronous operation, containing an array of configured chart series.</returns>
        private async Task<IEnumerable<ISeries>> GenerateMuscleDistributionSeriesAsync(IEnumerable<MuscleDataResults> muscleDistribution)
        {

            var pieSeries = muscleDistribution
               .Where(m => m.TotalVolume > 0)
               .Select(m => (ISeries)new PieSeries<double>
               {
                   Name = string.IsNullOrWhiteSpace(m.MuscleGroup) ? "<unknown>" : m.MuscleGroup,
                   Values = new double[] { m.PercentageShare },
                   DataLabelsPosition = PolarLabelsPosition.Middle,
                   DataLabelsFormatter = point => $"{(point.Context.Series.Name ?? point.Coordinate.PrimaryValue.ToString("F2"))} %",
                   ToolTipLabelFormatter = point => $"{(point.Context.Series.Name ?? point.Coordinate.PrimaryValue.ToString("F2"))} %"
               })
               .ToArray();

            return pieSeries;
        }

        /// <summary>
        /// Asynchronously generates and updates the monthly training volume chart series and configuration axes.
        /// </summary>
        /// <remarks>Acts as a thread-safe operation protected by a refresh guard, extracts workout date bounds for initial load ranges, computes monthly aggregated volume metrics via the analyzer, and generates cartesian chart templates.</remarks>
        /// <param name="analyzer">The initialized workout load analyzer instance providing access to parsed entries and volume calculations.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task GenerateMonthlyTraningsVolumeAsync(AppWorkoutLoadAnalyzer analyzer)
        {
            if (isRefreshing) return;

            isRefreshing = true;

            if (analyzer == null) return;

            try
            {
                var data = analyzer.AppWorkoutEntries;

                minMeasurementsDateMonthlyTraningsVolume = data.Min(d => d.ExcerciseDate);
                maxMeasurementsDateMonthlyTraningsVolume = data.Max(d => d.ExcerciseDate);

                if (firstLoad)
                {
                    StartDateMonthlyTraningsVolume = minMeasurementsDateMonthlyTraningsVolume;
                    EndDateMonthlyTraningsVolume = maxMeasurementsDateMonthlyTraningsVolume;
                    firstLoad = false;
                }

                var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddMonths(-6);
                var endDate = DateTime.Now;

                var results = await analyzer.GetMonthlyOverviewExerciseVolumeAsync(startDate, endDate);

                //var results = analyzer.GetDailyExerciseVolumeFor2026();



                (SeriesMonthlyTraningsVolume, XAxesMonthlyTraningsVolume, YAxesMonthlyTraningsVolume) = ChartTemplateService.MonthlyOverviewExerciseTraingingsVolume(startDate, endDate, results, isTrendLineLegendVisible,
                                                                                                                                                                                                strokeThickness, geometrySize, LoessFractionMonthlyTraningsVolume,
                                                                                                                                                                                                true, true);
            }

            catch(Exception ex)
            {
                GeneralErrorMessage = $"An error occurred while generating monthly training volume: {ex.Message}";
            }

            finally
            {
                isRefreshing = false;
            }
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