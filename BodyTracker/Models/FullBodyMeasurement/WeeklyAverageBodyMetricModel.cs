using System;

namespace BodyTracker.Models.FullBodyMeasurement
{
    /// <summary>
    /// Represents an aggregated data model containing weekly average body metrics, 
    /// calendar week information, and corresponding date boundaries.
    /// </summary>
    public class WeeklyAverageBodyMetricModel
    {
        /// <summary>
        /// Gets or sets the ISO calendar week number for the aggregated period.
        /// </summary>
        public int CalendarWeek { get; set; }

        /// <summary>
        /// Gets or sets the start date of the corresponding calendar week.
        /// </summary>
        public DateTime WeekStartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the corresponding calendar week.
        /// </summary>
        public DateTime WeekEndDate { get; set; }

        /// <summary>
        /// Gets or sets the body metric model containing the calculated average values for the week.
        /// </summary>
        public BodyMetricModel AverageValues { get; set; }
    }
}

