namespace BodyTracker.Models
{
    public class TotalWorkoutModel
    {
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
        /// Gets the combined total workout volume across all primary + secondary muscle groups.
        /// </summary>
        /// <remarks>Computed dynamically as the sum of <see cref="PrimaryVolume"/> and <see cref="SecondaryVolume"/>.</remarks>
        public double TotalVolumeTrainigStimulus => PrimaryVolume + SecondaryVolume;

        /// <summary>
        /// Gets or sets the overall cumulative workout volume.
        /// </summary>
        /// <remarks>Represents the total lifting volume across all tracked exercises and sets.</remarks>
        public double TotalVolume { get; set; }

        /// <summary>
        /// Gets or sets the total count of performed exercises or workout executions.
        /// </summary>
        /// <remarks>Serves as the aggregate numerical frequency count for statistical reporting.</remarks>
        public int TotalExercises { get; set; }
    }
}
