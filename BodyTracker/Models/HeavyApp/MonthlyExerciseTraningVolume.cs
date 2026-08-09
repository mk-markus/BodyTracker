using System;

namespace BodyTracker.Models
{
    public class MonthlyExerciseTraningVolume
    {
        /// <summary>
        /// Gets or sets the specific calendar date associated with the volume and activity record.
        /// </summary>
        /// <remarks>Represents the exact date for daily or time-series aggregation.</remarks>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the year component of the record.
        /// </summary>
        /// <remarks>Used for annual grouping, filtering, and chronological sorting.</remarks>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the month component of the record.
        /// </summary>
        /// <remarks>Represents the calendar month (1-12) used for monthly aggregation and overview charts.</remarks>
        public int Month { get; set; }

        /// <summary>
        /// Gets or sets the total workout volume attributed to primary muscle groups for the period.
        /// </summary>
        /// <remarks>Represents the cumulative lifting workload assigned directly to the targeted primary muscles.</remarks>
        public double PrimaryVolume { get; set; }

        /// <summary>
        /// Gets or sets the total workout volume attributed to secondary muscle groups for the period.
        /// </summary>
        /// <remarks>Represents the cumulative lifting workload assigned to synergistically cooperating secondary muscles.</remarks>
        public double SecondaryVolume { get; set; }

        /// <summary>
        /// Gets or sets the total count of performed exercises or workout sets during the period.
        /// </summary>
        /// <remarks>Serves as the aggregate numerical frequency count for statistical reporting.</remarks>
        public int TotalExercises { get; set; }

        /// <summary>
        /// Gets the combined total weight volume across all primary and secondary muscle groups for the period.
        /// </summary>
        /// <remarks>Computed dynamically as the sum of <see cref="PrimaryVolume"/> and <see cref="SecondaryVolume"/>.</remarks>
        public double? TotalWeight => PrimaryVolume + SecondaryVolume;

    }
}
