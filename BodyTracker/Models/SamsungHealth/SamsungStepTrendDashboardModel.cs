using System;

namespace BodyTracker.Models
{

    public class SamsungStepTrendDashboardModel
    {
        /// <summary>
        /// Gets or sets the timestamp indicating when the tracking entry or record was created.
        /// </summary>
        /// <remarks>Represents the specific date and time associated with the recorded metric.</remarks>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// Gets or sets the numerical identifier representing the data source type.
        /// </summary>
        /// <remarks>Used to distinguish between different origin devices, platforms, or import channels for the tracked data.</remarks>
        public int SourceType { get; set; }

        /// <summary>
        /// Gets or sets the numerical count value associated with the record.
        /// </summary>
        /// <remarks>Represents discrete metrics such as step counts or repetition totals for the given timeframe.</remarks>
        public int StepCount { get; set; }

        /// <summary>
        /// Gets or sets the distance metric associated with the activity entry.
        /// </summary>
        /// <remarks>Represents the total distance traveled, typically measured in kilometers or meters.</remarks>
        public double Distance { get; set; }

        /// <summary>
        /// Gets or sets the active calorie expenditure associated with the activity or entry.
        /// </summary>
        /// <remarks>Represents the estimated energy burned during the recorded timeframe, measured in kilocalories.</remarks>
        public double Calorie { get; set; }
    }

}
