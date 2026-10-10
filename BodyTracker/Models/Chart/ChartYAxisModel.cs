using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using OpenTK.Graphics.ES11;
using SkiaSharp;
using System.Globalization;

namespace BodyTracker.Services
{
    public record ChartYAxisModel
    {
        /// <summary>
        /// Gets or sets the name identifier of the axis.
        /// </summary>
        /// <remarks>Used to Lbl or identify the specific axis within charting components.</remarks>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the minimum numerical boundary limit for the axis.
        /// </summary>
        /// <remarks>Nullable to allow automatic scaling calculation when no lower bound is explicitly defined.</remarks>
        public double? MinLimit { get; set; }

        /// <summary>
        /// Gets or sets the maximum numerical boundary limit for the axis.
        /// </summary>
        /// <remarks>Nullable to allow automatic scaling calculation when no upper bound is explicitly defined.</remarks>
        public double? MaxLimit { get; set; }

        /// <summary>
        /// Gets or sets the relative alignment position of the axis within the chart layout.
        /// </summary>
        /// <remarks>Determines where the axis is rendered relative to the plotting area using the <see cref="AxisPosition"/> enumeration.</remarks>
        public AxisPosition Position { get; set; } = AxisPosition.Start;

        /// <summary>
        /// Gets or sets a value indicating whether separator or grid lines are displayed along the axis.
        /// </summary>
        /// <remarks>Controls the visibility of background grid lines corresponding to axis tick intervals.</remarks>
        public bool ShowSeparatorLines { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether separator or grid lines are displayed along the axis.
        /// </summary>
        /// <remarks>Controls the visibility of background grid lines corresponding to axis tick intervals.</remarks>
        public SolidColorPaint SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 };

        /// <summary>
        /// Gets or sets the minimum step value for scaling or axis partitioning.
        /// </summary>
        public double? MinStep { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether step values should be forced to the minimum step limit.
        /// </summary>
        public bool ForceStepToMin { get; set; } = true;

        /// <summary>
        /// Formats a given numeric axis or data value into a string representation using the specified format.
        /// </summary>
        /// <param name="value">The numeric value to format.</param>
        /// <param name="format">The format string.</param>
        /// <returns>The formatted string representation of the value.</returns>
        public string Labeler(double value, string format) => value.ToString(format);

        /// <summary>
        /// Gets or sets the internal backing string that defines the numeric format applied to axis values.
        /// </summary>
        /// <remarks>Defaults to <c>"N0"</c> for whole number formatting without decimal places.</remarks>
        public string Format = "N0";




      
    }
}
