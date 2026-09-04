using SkiaSharp;
using System;

namespace BodyTracker.Services;

public class ChartSeriesModel<T>
{
    /// <summary>
    /// Gets or sets the display name of the bodyMeasurement series.
    /// </summary>
    /// <remarks>Used to identify the series within chart legends and tooltips.</remarks>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the trend line.
    /// </summary>
    /// <remarks>Used to label the calculated trend line corresponding to this bodyMeasurement series in legends.</remarks>
    public string TrendName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the color of the line.
    /// </summary>
    /// <remarks>Defines the visual stroke color using SkiaSharp (<see cref="SKColor"/>) for rendering the series.</remarks>
    public SKColor Color { get; set; }

    /// <summary>
    /// Gets or sets the index of the Y-axis.
    /// </summary>
    /// <remarks>Determines which Y-axis coordinate system the bodyMeasurement series is bound to in multi-axis charts.</remarks>
    public int YAxisIndex { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the series is visible.
    /// </summary>
    /// <remarks>Controls whether the bodyMeasurement series is rendered in the charting component.</remarks>
    public bool IsVisible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the trend line is visible.
    /// </summary>
    /// <remarks>Controls the rendering visibility of the associated trend line overlay.</remarks>
    public bool IsTrendLineVisible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the trend line is hoverable.
    /// </summary>
    /// <remarks>Controls whether the trend line responds to mouse hover interactions and tooltips.</remarks>
    public bool IsTrendLineHoverable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a trend should be calculated.
    /// </summary>
    /// <remarks>Determines if algorithmic trend evaluation and plotting should be performed for this series.</remarks>
    public bool ShowTrend { get; set; }

    /// <summary>
    /// Gets or sets the thickness of the line.
    /// </summary>
    /// <remarks>Defines the stroke width in pixels used when rendering the series line.</remarks>
    public float StrokeThickness { get; set; } = 2;

    /// <summary>
    /// Gets or sets the size of the data points.
    /// </summary>
    /// <remarks>Defines the geometric point size used to highlight individual data markers along the series line.</remarks>
    public float GeometrySize { get; set; } = 8;

    /// <summary>
    /// Gets or sets the function that extracts the date.
    /// </summary>
    /// <remarks>A delegate used to map elements of type <typeparamref name="T"/> to their corresponding <see cref="DateTime"/> values for horizontal axis placement.</remarks>
    public Func<T, DateTime> DateSelector { get; set; } = default!;

    /// <summary>
    /// Gets or sets the function that extracts the bodyMeasurement value.
    /// </summary>
    /// <remarks>A delegate used to map elements of type <typeparamref name="T"/> to their corresponding nullable numerical bodyMeasurement values for vertical axis plotting.</remarks>
    public Func<T, double?> ValueSelector { get; set; } = default!;

    /// <summary>
    /// Gets or sets the optional dash pattern array applied to the trend line stroke for custom styling.
    /// </summary>
    public float[]? TrendLineStyle { get; set; } = null;


}