using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels
{
    public partial class ChartsStepsDailyTrendPageViewModel : ObservableObject
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/> for retrieving bodyMeasurement data.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// Gets or sets the collection of data series displayed on the chart.
        /// </summary>
        [ObservableProperty] private ISeries[] series = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the X-axis configuration for the chart.
        /// </summary>
        [ObservableProperty] private ICartesianAxis[] xAxes = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the Y-axis configuration for the chart.
        /// </summary>
        [ObservableProperty] private ICartesianAxis[] yAxes = Array.Empty<ICartesianAxis>();

        /// <summary>
        /// Gets or sets the start date of the visible chart range.
        /// </summary>
        [ObservableProperty] private DateTime startDate;

        /// <summary>
        /// Gets or sets the end date of the visible chart range.
        /// </summary>
        [ObservableProperty] private DateTime endDate;

        /// <summary>
        /// Gets or sets a value indicating whether step counts are displayed.
        /// </summary>
        [ObservableProperty] private bool showSteps = true;

        /// <summary>
        /// Gets or sets a value indicating whether distance metrics are displayed.
        /// </summary>
        [ObservableProperty] private bool showDistance = true;

        /// <summary>
        /// Gets or sets a value indicating whether calorie consumption metrics are displayed.
        /// </summary>
        [ObservableProperty] private bool showCalories = true;

        /// <summary>
        /// Gets or sets a value indicating whether the step count trend line is displayed.
        /// </summary>
        [ObservableProperty] private bool showStepsTrend = true;

        /// <summary>
        /// Gets or sets a value indicating whether the distance trend line is displayed.
        /// </summary>
        [ObservableProperty] private bool showDistanceTrend = true;

        /// <summary>
        /// Gets or sets a value indicating whether the calorie consumption trend line is displayed.
        /// </summary>
        [ObservableProperty] private bool showCaloriesTrend = true;

        /// <summary>
        /// Command to asynchronously refresh the chart data based on the selected criteria.
        /// </summary>
        public IAsyncRelayCommand RefreshChart { get; }

        /// <summary>
        /// Command to set the chart date range to the current year.
        /// </summary>
        public IRelayCommand SetActualYearCommand { get; }

        /// <summary>
        /// Command to set the chart date range to the current month.
        /// </summary>
        public IRelayCommand SetActualMonthCommand { get; }

        /// <summary>
        /// Command to set the chart date range to the current week.
        /// </summary>
        public IRelayCommand SetActualWeekCommand { get; }

        /// <summary>
        /// Command to set the chart date range to include all historical bodyMeasurement data.
        /// </summary>
        public IRelayCommand ShowAllDataCommand { get; }

        /// <summary>
        /// The earliest date found in the available bodyMeasurement dataset.
        /// </summary>
        private DateTime minMeasurementsDate;

        /// <summary>
        /// The latest date found in the available bodyMeasurement dataset.
        /// </summary>
        private DateTime maxMeasurementsDate;

        /// <summary>
        /// Tracks if the view model is performing its initial data load.
        /// </summary>
        private bool firstLoad = true;

        /// <summary>
        /// Tracks the current refresh status of the chart.
        /// </summary>
        private bool isRefreshing;

        /// <summary>
        /// Gets or sets the fraction parameter for the LOESS smoothing algorithm, which determines 
        /// the degree of local averaging applied to the trend line.
        /// </summary>
        [ObservableProperty] private double loessFraction = 0.5;

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
        /// Initializes a new instance of the <see cref="ChartsStepsDailyTrendPageViewModel"/> class.
        /// </summary>
        /// <param name="db">The <see cref="DatabaseService"/> instance used for data retrieval.</param>
        public ChartsStepsDailyTrendPageViewModel(DatabaseService db)
        {
            databaseService = db;

            XAxes =
            [
                new DateTimeAxis(
                TimeSpan.FromDays(1),
                date => date.ToString("dd.MM.yyyy"))
            {
                Name = "Date"
            }
            ];

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
        /// It queries steps daily trend, filters them by date, and conditionally creates 
        /// normal data series or smoothed LOESS trend series depending on the global settings.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RefreshChartAsync()
        {
            if (AppState.SelectedPersonId <= 0)
            {
                GeneralErrorMessage = "The Person ID is invalid.";
                return;
            }
            if (isRefreshing)
                return;

            isRefreshing = true;

            try
            {
                var data = await databaseService.GetStepDailyTrendAsync(AppState.SelectedPersonId);

                if (!data.Any()) return;

                minMeasurementsDate = data.Min(x => x.CreateTime);
                maxMeasurementsDate = data.Max(x => x.CreateTime);

                if (firstLoad)
                {
                    StartDate = minMeasurementsDate;
                    EndDate = maxMeasurementsDate;
                    firstLoad = false;
                }


                var filtered = data
                    .Where(x =>
                        x.SourceType == -2 &&
                        x.CreateTime >= StartDate &&
                        x.CreateTime <= EndDate)
                    .OrderBy(x => x.CreateTime)
                    .ToList();

                var chartSeries = new List<ISeries>();

                #region Steps

                var stepPoints = filtered
                    .Select(x => new DateTimePoint(
                        x.CreateTime,
                        x.Count))
                    .ToArray();

                if (ShowSteps)
                {
                    chartSeries.Add(
                        CreateNormalSeries(
                            "Daily Steps",
                            stepPoints,
                            SKColors.Blue,
                            0));
                }

                if (ShowStepsTrend)
                {
                    var trend = CreateLoessSeries(
                        "Daily Steps Trend",
                        stepPoints,
                        SKColors.Blue,
                        0,
                        LoessFraction);

                    if (trend != null)
                        chartSeries.Add(trend);
                }

                #endregion

                #region Distance

                var distancePoints = filtered
                    .Select(x => new DateTimePoint(
                        x.CreateTime,
                        x.Distance))
                    .ToArray();

                if (ShowDistance)
                {
                    chartSeries.Add(
                        CreateNormalSeries(
                            "Distance (m)",
                            distancePoints,
                            SKColors.Green,
                            1));
                }

                if (ShowDistanceTrend)
                {
                    var trend = CreateLoessSeries(
                        "Distance Trend",
                        distancePoints,
                        SKColors.Green,
                        1,
                        LoessFraction);

                    if (trend != null)
                        chartSeries.Add(trend);
                }

                #endregion

                #region Calories

                var caloriePoints = filtered
                    .Select(x => new DateTimePoint(
                        x.CreateTime,
                        x.Calorie))
                    .ToArray();

                if (ShowCalories)
                {
                    chartSeries.Add(
                        CreateNormalSeries(
                            "Calories kcal",
                            caloriePoints,
                            SKColors.Red,
                            2));
                }

                if (ShowCaloriesTrend)
                {
                    var trend = CreateLoessSeries(
                        "Calories Trend (kcal)",
                        caloriePoints,
                        SKColors.Red,
                        2,
                        LoessFraction);

                    if (trend != null)
                        chartSeries.Add(trend);
                }

                #endregion

                Series = chartSeries.ToArray();

                XAxes =
                [
                    new DateTimeAxis(
                TimeSpan.FromDays(1),
                date => date.ToString("dd.MM.yyyy"))
                {
                    Name = "Date"
                }];

                var maxSteps = stepPoints.Length > 0
                    ? stepPoints.Max(x => x.Value)
                    : 10000;

                var maxDistance = distancePoints.Length > 0
                    ? distancePoints.Max(x => x.Value)
                    : 10000;

                var maxCalories = caloriePoints.Length > 0
                    ? caloriePoints.Max(x => x.Value)
                    : 1000;

                YAxes =
                [
                    new Axis
                    {
                        Name = "Steps",
                        MinLimit = 0,
                        MaxLimit = maxSteps * 1.05,
                        Labeler = value => value.ToString("F0")
                    },

                    new Axis
                    {
                        Name = "Distance (m)",
                        Position = AxisPosition.End,
                        MinLimit = 0,
                        MaxLimit = maxDistance * 1.05,
                        Labeler = value => value.ToString("F0")
                    },

                    new Axis
                    {
                        Name = "Calories (kcal)",
                        Position = AxisPosition.End,
                        MinLimit = 0,
                        MaxLimit = maxCalories * 1.05,
                        Labeler = value => value.ToString("F0")
                    }
                ];
            }
            catch(Exception ex)
            {
                GeneralErrorMessage = $"An error occurred while refreshing the chart: {ex.Message}";
            }   
            finally
            {
                isRefreshing = false;
            }
        }


        /// <summary>
        /// Calculates a linear regression trend line for a set of data points and returns it as a dashed line series.
        /// </summary>
        /// <param name="name">The display name of the trend line series in the chart legend.</param>
        /// <param name="sourcePoints">The original chronological data points used to compute the slope and intercept.</param>
        /// <param name="color">The color to be applied to the rendered trend line.</param>
        /// <param name="yAxisIndex">The zero-based index mapping this series to either the primary or secondary Y-axis.</param>
        /// <returns>A configured line series representing the linear trend, or <c>null</c> if calculation fails.</returns>
        private static LineSeries<DateTimePoint>? CreateTrendSeries(string name,
                                                                    DateTimePoint[] sourcePoints,
                                                                    SKColor color,
                                                                    int yAxisIndex)
        {
            // Require at least two distinct mathematical data points to trace a linear line.
            if (sourcePoints.Length < 2)
                return null;

            // Map DateTime objects into OLE Automation compatible doubles (X values) along with raw bodyMeasurement entries (Y values).
            var points = sourcePoints
                .Select(p => new
                {
                    X = p.DateTime.ToOADate(),
                    Y = p.Value
                })
                .ToList();

            // Calculate sample coordinate arithmetic means.
            double xAvg = points.Average(p => p.X);
            double yAvg = (double)points.Average(p => p.Y);

            // Core Least Squares Regression implementation: Calculate numerator (covariance) and denominator (variance).
            double numerator = (double)points.Sum(p => (p.X - xAvg) * (p.Y - yAvg));
            double denominator = points.Sum(p => Math.Pow(p.X - xAvg, 2));

            // Prevent division-by-zero crashes due to data points stacked vertically on the identical millisecond timestamp.
            if (Math.Abs(denominator) < 0.0000001)
                return null;

            // Deduce slope (m) and intercept (c) according to standard linear form equation (y = mx + c).
            double slope = numerator / denominator;
            double intercept = yAvg - slope * xAvg;

            // Project every source date coordinate onto the solved mathematical straight trajectory.
            var trendPoints = sourcePoints
                .Select(p =>
                {
                    double x = p.DateTime.ToOADate();
                    double yTrend = slope * x + intercept;
                    return new DateTimePoint(p.DateTime, yTrend);
                })
                .ToArray();

            // Instantiate a distinct, dashed visual representation for the straight trend projection.
            return new LineSeries<DateTimePoint>
            {
                Name = name,
                Values = trendPoints,
                LineSmoothness = 0, // Enforce sharp geometric precision for straight lines
                Fill = null,
                ScalesYAt = yAxisIndex,
                GeometrySize = geometrySize, // Hide individual point dots to make the dashboard clearer
                Stroke = new SolidColorPaint(color)
                {
                    StrokeThickness = strokeThickness,
                    PathEffect = new DashEffect(new float[] { 10, 6 }) // Defines 10px dash pattern separated by 6px gaps
                }
            };
        }


        /// <summary>
        /// Generates a solid, styled line series displaying actual chronological measurements with visible geometric data points.
        /// </summary>
        /// <param name="name">The display name of the data series in the chart legend.</param>
        /// <param name="values">The array of raw chronological data points to plot.</param>
        /// <param name="color">The color used for the line stroke and point geometry.</param>
        /// <param name="yAxisIndex">The zero-based index mapping this series to either the primary or secondary Y-axis.</param>
        /// <returns>A configured line series ready to be rendered on the chart UI.</returns>
        private static LineSeries<DateTimePoint> CreateNormalSeries(
                string name,
                DateTimePoint[] values,
                SKColor color,
                int yAxisIndex)
        {
            return new LineSeries<DateTimePoint>
            {
                Name = name,
                Values = values,
                LineSmoothness = 0,
                Fill = null,
                ScalesYAt = yAxisIndex,
                Stroke = new SolidColorPaint(color)
                {
                    StrokeThickness = strokeThickness
                },
                GeometrySize = geometrySize,
                GeometryStroke = new SolidColorPaint(color)
                {
                    StrokeThickness = strokeThickness
                }
            };
        }

        /// <summary>
        /// Generates a smoothed trend line using Local Regression (LOESS) and returns it as a smooth, dashed chart series.
        /// </summary>
        /// <param name="name">The display name of the trend line series in the chart legend.</param>
        /// <param name="sourcePoints">The original bodyMeasurement data points to be smoothed.</param>
        /// <param name="color">The color to be applied to the rendered trend line.</param>
        /// <param name="yAxisIndex">The zero-based index mapping this series to either the primary or secondary Y-axis.</param>
        /// <param name="fraction">The smoothing parameter determining the proportion of local data points included in each local regression.</param>
        /// <returns>A configured smooth line series representing the local regression curve, or <c>null</c> if input points are insufficient.</returns>
        private static LineSeries<DateTimePoint>? CreateLoessSeries(
        string name,
        DateTimePoint[] sourcePoints,
        SKColor color,
        int yAxisIndex,
        double fraction)
        {
            // Local Polynomial Regression operations require an input dataset size of at least three samples.
            if (sourcePoints.Length < 3)
                return null;

            // Perform complex mathematical local polynomial smoothing operations over the coordinates.
            var smoothPoints = CalculateLoess(sourcePoints, fraction);

            // Check if the smoothing algorithm returned data successfully.
            if (smoothPoints.Length == 0)
                return null;

            // Build a curved, styled dashed path modeling localized weight fluctuations.
            return new LineSeries<DateTimePoint>
            {
                Name = name,
                Values = smoothPoints,
                LineSmoothness = 1, // Interpolates control metrics cleanly into fluid curves instead of segments
                Fill = null,
                ScalesYAt = yAxisIndex,
                GeometrySize = geometrySize, // Suppress localized nodes to present a clean visual profile
                IsVisibleAtLegend = isTrendLineLegendVisible,
                IsHoverable = false, // Disable hover interactions for trend lines to focus user attention on actual data points
                Stroke = new SolidColorPaint(color)
                {
                    StrokeThickness = strokeThickness,
                    PathEffect = new DashEffect(new float[] { 10, 6 }) // Match styling criteria across all trend representations
                }
            };
        }

        /// <summary>
        /// Computes Locally Estimated Scatterplot Smoothing (LOESS) for the given chronological data points.
        /// Uses a local neighborhood approach weighted via a tricube kernel function to evaluate a robust non-linear trend.
        /// </summary>
        /// <param name="sourcePoints">The original array of chronological data points to smooth.</param>
        /// <param name="fraction">The bandwidth factor that controls the percentage of total points utilized for local regression.</param>
        /// <returns>An array of smoothed data points matching the size and dates of the input array.</returns>
        private static DateTimePoint[] CalculateLoess(
        DateTimePoint[] sourcePoints,
        double fraction)
        {
            // Enforce safety constraints regarding mathematical baseline requirements.
            if (sourcePoints.Length < 3)
                return Array.Empty<DateTimePoint>();

            // Enforce rigid boundaries on user input to protect local windows against outlier extremes.
            fraction = Math.Max(0.1, Math.Min(0.95, fraction));

            // Enforce chronological sorting over the inputs.
            var ordered = sourcePoints
                .OrderBy(p => p.DateTime)
                .ToArray();

            int n = ordered.Length;

            // Convert parameters to double arrays to accelerate inner matrix calculations.
            var x = ordered.Select(p => p.DateTime.ToOADate()).ToArray();
            var y = ordered.Select(p => p.Value).ToArray();

            // Establish localized evaluation window span based on fraction scale.
            int bandwidth = Math.Max(3, (int)Math.Ceiling(fraction * n));
            var result = new DateTimePoint[n];

            // Process a localized weighted linear equation individually for each node item.
            for (int i = 0; i < n; i++)
            {
                double xi = x[i];

                // Measure chronological distance offsets originating from the target processing node index.
                var distances = x
                    .Select(xj => Math.Abs(xj - xi))
                    .OrderBy(d => d)
                    .ToArray();

                // Locate boundary distance of the farthest element located within the sliding neighborhood window.
                double maxDistance = distances[Math.Min(bandwidth - 1, distances.Length - 1)];

                // Workaround logic for clustered identical dates: find the closest non-zero neighbor distance.
                if (maxDistance <= 0)
                {
                    maxDistance = distances.LastOrDefault(d => d > 0);
                    if (maxDistance <= 0)
                    {
                        // Fall back to original entry value if all data values are stacked in a single point.
                        result[i] = new DateTimePoint(ordered[i].DateTime, y[i]);
                        continue;
                    }
                }

                // Reset accumulator variables for the local linear regression model.
                double sumW = 0.0;
                double sumWX = 0.0;
                double sumWY = 0.0;
                double sumWXX = 0.0;
                double sumWXY = 0.0;

                // Iterate over every node element within the system dataset to evaluate local weights.
                for (int j = 0; j < n; j++)
                {
                    double dist = Math.Abs(x[j] - xi);

                    // Ignore elements residing completely outside the calculated window span.
                    if (dist > maxDistance)
                        continue;

                    // Normalize distance bounds to fit standard [0, 1] kernel input space.
                    double u = dist / maxDistance;
                    double w = Tricube(u); // Convert distance offset ratio into weighted priority metric

                    // Ignore items exerting no numerical impact on the local system solution.
                    if (w <= 0)
                        continue;

                    // Accumulate standard regression calculation properties.
                    sumW += w;
                    sumWX += w * x[j];
                    sumWY += (double)(w * y[j]);
                    sumWXX += w * x[j] * x[j];
                    sumWXY += (double)(w * x[j] * y[j]);
                }

                double yi;

                // Calculate the denominator for the local linear equation slope solution.
                double denominator = (sumW * sumWXX) - (sumWX * sumWX);

                // Handle singular matrix configurations or data edge states safely.
                if (Math.Abs(denominator) < 1e-12 || sumW <= 1e-12)
                {
                    yi = (double)(sumW > 1e-12 ? sumWY / sumW : y[i]);
                }
                else
                {
                    // Execute standard weighted least squares formula calculations to solve for alpha and beta parameters.
                    double beta = ((sumW * sumWXY) - (sumWX * sumWY)) / denominator;
                    double alpha = (sumWY - beta * sumWX) / sumW;
                    yi = alpha + beta * xi; // Derive locally-adjusted trend value
                }

                // Save the computed, smoothed coordinate back into the result container.
                result[i] = new DateTimePoint(ordered[i].DateTime, yi);
            }

            return result;
        }

        /// <summary>
        /// Calculates the weight factor using the tricube kernel function for a given normalized distance.
        /// Used to assign distance-based weightings to neighboring points during local regression analysis.
        /// </summary>
        /// <param name="x">The normalized distance to the target point (expected range [0, 1]).</param>
        /// <returns>A weight value between 0 and 1 representing the kernel intensity. Returns 0 if the distance is out of bounds.</returns>
        private static double Tricube(double x)
        {
            // Enforce strict kernel boundaries. Returns 0 for points outside the local neighborhood.
            if (x < 0 || x >= 1)
                return 0;

            // Evaluate standard cubic weighting calculation: W(x) = (1 - x^3)^3
            double t = 1 - Math.Pow(x, 3);
            return Math.Pow(t, 3);
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