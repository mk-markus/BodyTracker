using System;

namespace BodyTracker.Models.SamsungHealth
{
    /// <summary>
    /// Represents oxygen saturation and related respiratory metrics recorded from Samsung Health data exports.
    /// </summary>
    public class SamsungOxygenSaturationDashboardModel
    {
        /// <summary>
        /// Gets or sets the start timestamp when the oxygen saturation measurement interval began.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the mean blood oxygen saturation percentage (SpO2).
        /// </summary>
        public double? SpO2 { get; set; }

        /// <summary>
        /// Gets or sets the maximum recorded blood oxygen saturation percentage.
        /// </summary>
        public double? MaxSpO2 { get; set; }

        /// <summary>
        /// Gets or sets the minimum recorded blood oxygen saturation percentage.
        /// </summary>
        public double? MinSpO2 { get; set; }

        /// <summary>
        /// Gets or sets the duration of low blood oxygen saturation events.
        /// </summary>
        public int? LowSpO2Duration { get; set; }

        /// <summary>
        /// Gets or sets the coverage rate of the measurement period.
        /// </summary>
        public int? CoverageRate { get; set; }
    }
}