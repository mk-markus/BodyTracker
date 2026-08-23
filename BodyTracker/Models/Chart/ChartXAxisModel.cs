using LiveChartsCore.Measure;

namespace BodyTracker.Services
{
    public class ChartXAxisModel
    {
        /// <summary>
        /// Gets or sets the name identifier of the axis.
        /// </summary>
        /// <remarks>Used to label or identify the specific axis within charting components.</remarks>
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
        /// Gets or sets the formatting string applied to numerical values along the axis.
        /// </summary>
        /// <remarks>Uses standard .NET numeric format strings (such as <c>"F2"</c>) to control the presentation of tick labels.</remarks>
        public string Format { get; set; } = "F2";
    }
}
