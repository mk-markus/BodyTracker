using System;

namespace BodyTracker.Models
{
    public class HeavyAppCSVModel
    {
        /// <summary>
        /// Gets or sets the unique global identifier for the workout or data entry record.
        /// </summary>
        /// <remarks>Serves as the primary database key or unique tracking reference for the entry.</remarks>
        public Guid DataUuid { get; set; }

        /// <summary>
        /// Gets or sets the title of the workout session or activity record.
        /// </summary>
        /// <remarks>Initialized to an empty string by default.</remarks>
        public string Title { get; set; } = string.Empty;


        /// <summary>
        /// The timestamp indicating when the record was last updated.
        /// </summary>
        public DateTime? UpdateTime { get; set; }


        /// <summary>
        /// Gets or sets the timestamp indicating when the workout session or activity started.
        /// </summary>
        /// <remarks>Represents the starting point of the recorded duration or event.</remarks>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the timestamp indicating when the workout session or activity ended.
        /// </summary>
        /// <remarks>Represents the completion point of the recorded duration or event.</remarks>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the optional textual description or notes for the session.
        /// </summary>
        /// <remarks>Can be null if no additional description is provided.</remarks>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the specific title or name of the exercise being performed.
        /// </summary>
        /// <remarks>Initialized to an empty string by default.</remarks>
        public string ExerciseTitle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional superset identifier grouping multiple exercises together.
        /// </summary>
        /// <remarks>Used to link exercises performed back-to-back in a superset routine. Can be null if not part of a superset.</remarks>
        public string? SupersetId { get; set; }

        /// <summary>
        /// Gets or sets optional notes specific to the individual exercise instance.
        /// </summary>
        /// <remarks>Can be null if no exercise-specific notes are recorded.</remarks>
        public string? ExerciseNotes { get; set; }

        /// <summary>
        /// Gets or sets the zero-based or one-based index of the specific set within the exercise.
        /// </summary>
        /// <remarks>Indicates the sequential order of the set during the workout.</remarks>
        public int SetIndex { get; set; }

        /// <summary>
        /// Gets or sets the type classification of the set (e.g., working set, warm-up, drop set).
        /// </summary>
        /// <remarks>Can be null if not explicitly categorized.</remarks>
        public string? SetType { get; set; }

        /// <summary>
        /// Gets or sets the weight utilized during the set, measured in kilograms.
        /// </summary>
        /// <remarks>Represented as a nullable double to accommodate bodyweight or non-weighted movements.</remarks>
        public double? WeightKg { get; set; }

        /// <summary>
        /// Gets or sets the number of repetitions performed during the set.
        /// </summary>
        /// <remarks>Represented as a nullable double to support fractional or averaged repetition metrics.</remarks>
        public double? Reps { get; set; }

        /// <summary>
        /// Gets or sets the distance covered during the set or exercise, measured in kilometers.
        /// </summary>
        /// <remarks>Applicable for endurance or distance-based exercises. Can be null if not applicable.</remarks>
        public double? DistanceKm { get; set; }

        /// <summary>
        /// Gets or sets the duration of the set or exercise, measured in seconds.
        /// </summary>
        /// <remarks>Applicable for timed holds or cardio intervals. Can be null if not applicable.</remarks>
        public int? DurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the Rate of Perceived Exertion (RPE) for the set.
        /// </summary>
        /// <remarks>Represents the subjective intensity scale (typically 1-10) reported by the user. Can be null if not recorded.</remarks>
        public double? Rpe { get; set; }
    }
}
