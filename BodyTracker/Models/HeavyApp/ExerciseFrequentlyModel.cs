namespace BodyTracker.Models
{
    public class ExerciseFrequencyModel
    {
        /// <summary>
        /// Gets or sets the name of the exercise.
        /// </summary>
        /// <remarks>Identifies the specific exercise for frequency analysis and reporting. Initialized to an empty string by default.</remarks>
        public string ExerciseName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the numerical count value associated with the exercise.
        /// </summary>
        /// <remarks>Represents the total number of times the exercise was performed or logged within the selected timeframe.</remarks>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the percentage share associated with the exercise.
        /// </summary>
        /// <remarks>Represents the proportional frequency or distribution share of this exercise relative to the total set of recorded activities.</remarks>
        public double Percentage { get; set; }
    }

}
