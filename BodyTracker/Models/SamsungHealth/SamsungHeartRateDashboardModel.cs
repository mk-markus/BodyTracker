using System;

namespace BodyTracker.Models
{
    /// <summary>
    /// Represents a dashboard record containing exercise metrics including heart rate statistics and timestamps.
    /// </summary>
    public class SamsungHeartRateDashboardModel
    {
        /// <summary>
        /// Gets or sets the timestamp when the exercise or measurement was recorded.
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// Gets or sets the average heart rate recorded during the session.
        /// </summary>
        public float? HeartRate { get; set; }

        /// <summary>
        /// Gets or sets the minimum heart rate recorded during the session.
        /// </summary>
        public float? MinHeartRate { get; set; }

        /// <summary>
        /// Gets or sets the maximum heart rate recorded during the session.
        /// </summary>
        public float? MaxHeartRate { get; set; }
    }
}