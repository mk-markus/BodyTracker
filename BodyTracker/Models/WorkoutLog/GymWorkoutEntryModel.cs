using System;

namespace BodyTracker.Models
{
    public class GymWorkoutEntryModel
    {
        /// <summary>
        /// Gets or sets the date and time when the exercise was performed.
        /// </summary>
        /// <remarks>Represents the specific calendar date and timestamp associated with the workout entry.</remarks>
        public DateTime ExcerciseDate { get; set; }

        /// <summary>
        /// Gets or sets the name of the exercise performed.
        /// </summary>
        /// <remarks>Initialized to an empty string by default.</remarks>
        public string ExerciseName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the weight utilized during the exercise set.
        /// </summary>
        /// <remarks>Represented as a nullable double to accommodate bodyweight or non-weighted movements.</remarks>
        public double? Weight { get; set; }

        /// <summary>
        /// Gets or sets the number of repetitions performed during the exercise set.
        /// </summary>
        /// <remarks>Represented as a nullable double to support fractional or averaged repetition metrics.</remarks>
        public double? Reps { get; set; }

        /// <summary>
        /// Gets or sets the zero-based or one-based index of the specific set within the exercise.
        /// </summary>
        /// <remarks>Indicates the sequential order of the set during the workout. Can be null if not explicitly tracked.</remarks>
        public int? SetIndex { get; set; }
    }
}
