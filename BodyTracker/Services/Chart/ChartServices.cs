using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using System;
using System.Linq;

namespace BodyTracker.Services
{
    public static class ChartServices
    {
        /// <summary>
        /// Calculates a linear regression trend line for a set of data points and returns it as a dashed line series.
        /// </summary>
        /// <param name="name">The display name of the trend line series in the chart legend.</param>
        /// <param name="sourcePoints">The original chronological data points used to compute the slope and intercept.</param>
        /// <param name="color">The color to be applied to the rendered trend line.</param>
        /// <param name="yAxisIndex">The zero-based index mapping this series to either the primary or secondary Y-axis.</param>
        /// <returns>A configured line series representing the linear trend, or <c>null</c> if calculation fails.</returns>
        public static LineSeries<DateTimePoint>? CreateTrendSeries(string name,
                                                                    DateTimePoint[] sourcePoints,
                                                                    SKColor color,
                                                                    int yAxisIndex,
                                                                    float strokeThickness,
                                                                    float geometrySize)
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
        public static LineSeries<DateTimePoint> CreateNormalSeries(string name,
                                                                   DateTimePoint[] values,
                                                                   SKColor color,
                                                                   int yAxisIndex,
                                                                   float strokeThickness,
                                                                   float geometrySize)
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
                },

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
        /// <param name="isTrendLineLegendVisible"></param>
        /// <param name="strokeThickness"></param>
        /// <param name="geometrySize"></param>
        /// <returns></returns>
        public static LineSeries<DateTimePoint>? CreateLoessSeries(string name,
                                                                    DateTimePoint[] sourcePoints,
                                                                    SKColor color,
                                                                    int yAxisIndex,
                                                                    double fraction,
                                                                    bool isTrendLineLegendVisible,
                                                                    float strokeThickness,
                                                                    float geometrySize)
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
                },

            };
        }

        /// <summary>
        /// Creates a configured column series for a chart using the provided data points, display name, color, and Y-axis index.
        /// </summary>
        /// <param name="name">The display name of the column series in the chart legend.</param>
        /// <param name="dataSeries">The collection of observable points to be rendered as columns.</param>
        /// <param name="color">The color to be applied to the column fill.</param>
        /// <param name="yAxisIndex">The zero-based index mapping this series to either the primary or secondary Y-axis.</param>
        /// <returns>An ISeries instance representing the configured column series, or null if the data series is empty.</returns>
        public static ISeries CreateNormalColumnSeries(
            string name,
            ObservablePoint[] dataSeries,
            SKColor color,
            int yAxisIndex)
        {
            return new ColumnSeries<ObservablePoint>
            {
                Name = name,
                Values = dataSeries,
                Fill = new SolidColorPaint(color),
                ScalesYAt = yAxisIndex
            };
        }


        /// <summary>
        /// Computes Locally Estimated Scatterplot Smoothing (LOESS) for the given chronological data points.
        /// Uses a local neighborhood approach weighted via a tricube kernel function to evaluate a robust non-linear trend.
        /// </summary>
        /// <param name="sourcePoints">The original array of chronological data points to smooth.</param>
        /// <param name="fraction">The bandwidth factor that controls the percentage of total points utilized for local regression.</param>
        /// <returns>An array of smoothed data points matching the size and dates of the input array.</returns>
        public static DateTimePoint[] CalculateLoess(DateTimePoint[] sourcePoints, double fraction)
        {
            // Enforce safety constraints regarding mathematical baseline requirements.
            if (sourcePoints.Length < 3)
                return Array.Empty<DateTimePoint>();

            // Enforce rigid boundaries on user input to protect local windows against outlier extremes.
            fraction = Math.Max(0.1, Math.Min(0.95, fraction));

            // Enforce chronological sorting over the inputs.
            //var ordered = sourcePoints
            //    .OrderBy(p => p.DateTime)
            //    .ToArray();

            int n = sourcePoints.Length;

            // Convert parameters to double arrays to accelerate inner matrix calculations.
            var x = sourcePoints.Select(p => p.DateTime.ToOADate()).ToArray();
            var y = sourcePoints.Select(p => p.Value).ToArray();

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
                        result[i] = new DateTimePoint(sourcePoints[i].DateTime, y[i]);
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
                result[i] = new DateTimePoint(sourcePoints[i].DateTime, yi);
            }

            return result;
        }

        /// <summary>
        /// Calculates the weight factor using the tricube kernel function for a given normalized distance.
        /// Used to assign distance-based weightings to neighboring points during local regression analysis.
        /// </summary>
        /// <param name="x">The normalized distance to the target point (expected range [0, 1]).</param>
        /// <returns>A weight value between 0 and 1 representing the kernel intensity. Returns 0 if the distance is out of bounds.</returns>
        public static double Tricube(double x)
        {
            // Enforce strict kernel boundaries. Returns 0 for points outside the local neighborhood.
            if (x < 0 || x >= 1)
                return 0;

            // Evaluate standard cubic weighting calculation: W(x) = (1 - x^3)^3
            double t = 1 - Math.Pow(x, 3);
            return Math.Pow(t, 3);
        }


    }
}
