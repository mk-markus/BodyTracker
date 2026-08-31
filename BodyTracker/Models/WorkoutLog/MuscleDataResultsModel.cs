namespace BodyTracker.Models
{
    public class MuscleDataResultsModel
    {
        /// <summary>
        /// Gets or sets the name of the muscle group associated with the volume and percentage metrics.
        /// </summary>
        /// <remarks>Identifies the targeted anatomical muscle group (e.g., chest, back, legs) for distribution analysis and chart rendering.</remarks>
        public string MuscleGroup { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total workout volume attributed to primary muscle groups.
        /// </summary>
        /// <remarks>Represents the cumulative lifting workload assigned directly to the targeted primary muscles.</remarks>
        public double PrimaryVolume { get; set; }

        /// <summary>
        /// Gets or sets the total workout volume attributed to secondary muscle groups.
        /// </summary>
        /// <remarks>Represents the cumulative lifting workload assigned to synergistically cooperating secondary muscles.</remarks>
        public double SecondaryVolume { get; set; }

        /// <summary>
        /// Gets the combined total workout volume across all primary and secondary muscle groups.
        /// </summary>
        /// <remarks>Computed dynamically as the sum of <see cref="PrimaryVolume"/> and <see cref="SecondaryVolume"/>.</remarks>
        public double TotalVolume => PrimaryVolume + SecondaryVolume;

        /// <summary>
        /// Gets or sets the percentage share of the total volume attributed to this specific muscle group.
        /// </summary>
        /// <remarks>Represents the proportional workload distribution value used for pie chart visualization and statistical summaries.</remarks>
        public double PercentageShare { get; set; }
    }
}
