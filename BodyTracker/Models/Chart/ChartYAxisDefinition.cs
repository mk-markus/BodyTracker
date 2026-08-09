using LiveChartsCore.Measure;
using System.Globalization;

namespace BodyTracker.Services
{
    public class ChartYAxisDefinition
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
        /// Gets the formatting string applied to numerical values along the axis.
        /// </summary>
        /// <remarks>Retrieves the format string using invariant culture rules.</remarks>
        public string Format => format.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets or sets the internal backing string that defines the numeric format applied to axis values.
        /// </summary>
        /// <remarks>Defaults to <c>"N0"</c> for whole number formatting without decimal places.</remarks>
        private string format = "N0";
    }
}
