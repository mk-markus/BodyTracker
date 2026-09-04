using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using System.Collections.Generic;
using System.Linq;

namespace BodyTracker.Services;

public static class ChartSeriesBuilder
{
    /// <summary>
    /// Creates and compiles a collection of chart series and optional LOESS trend lines from a data source using series definitions.
    /// </summary>
    /// <remarks>Iterates through the provided series definitions, maps data points using configured selectors, and constructs normal bodyMeasurement series and smoothed trend series based on visibility and trend flags.</remarks>
    /// <typeparam name="T">The type of the data items in the source collection.</typeparam>
    /// <param name="source">The collection of raw data items to be visualized.</param>
    /// <param name="definitions">The collection of chart series configuration definitions.</param>
    /// <param name="loessFraction">The smoothing fraction parameter used for the LOESS trend line calculations.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ISeries"/> containing all generated chart and trend series.</returns>
    public static IEnumerable<ISeries> CreateSeries<T>(IEnumerable<T> source,
                                                       IEnumerable<ChartSeriesModel<T>> definitions,
                                                       double loessFraction)
    {
        var result = new List<ISeries>();

        foreach (var config in definitions)
        {
            var points = source
                .Where(x => config.ValueSelector(x).HasValue)
                .Select(x => new DateTimePoint(config.DateSelector(x), config.ValueSelector(x)!.Value)).ToArray();

            if (config.IsVisible)
            {
                result.Add(ChartServices.CreateNormalSeries(config.Name,
                                                            points,
                                                            config.Color,
                                                            config.YAxisIndex,
                                                            config.StrokeThickness,
                                                            config.GeometrySize));
            }

            if (config.ShowTrend)
            {
                var trend = ChartServices.CreateLoessSeries(config.TrendName,
                                                            points,
                                                            config.Color,
                                                            config.YAxisIndex,
                                                            loessFraction,
                                                            config.IsTrendLineVisible,
                                                            config.IsTrendLineHoverable,
                                                            config.StrokeThickness,
                                                            config.GeometrySize,
                                                            config.TrendLineStyle);

                if (trend != null) result.Add(trend);
            }
        }

        return result;
    }


    /// <summary>
    /// Creates and compiles a collection of chart series and optional LOESS trend lines from a data source using series definitions.
    /// </summary>
    /// <remarks>Iterates through the provided series definitions, maps data points using configured selectors, and constructs normal bodyMeasurement series and smoothed trend series based on visibility and trend flags.</remarks>
    /// <typeparam name="T">The type of the data items in the source collection.</typeparam>
    /// <param name="source">The collection of raw data items to be visualized.</param>
    /// <param name="definitions">The collection of chart series configuration definitions.</param>
    /// <param name="loessFraction">The smoothing fraction parameter used for the LOESS trend line calculations.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ISeries"/> containing all generated chart and trend series.</returns>
    public static IEnumerable<ISeries> CreateColumnSeries<T>(
        IEnumerable<T> source,
        IEnumerable<ChartSeriesModel<T>> definitions)
    {
        var result = new List<ISeries>();
        var sourceArray = source.ToArray();

        foreach (var config in definitions)
        {
            if (!config.IsVisible) continue;

            var points = sourceArray
                .Select((x, index) => new ObservablePoint((double)index, config.ValueSelector(x) ?? 0))
                .ToArray();

            result.Add(ChartServices.CreateNormalColumnSeries(
                config.Name,
                points,
                config.Color,
                config.YAxisIndex));
        }

        return result;
    }
}
