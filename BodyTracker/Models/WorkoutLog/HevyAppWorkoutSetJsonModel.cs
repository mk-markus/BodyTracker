using System.Text.Json.Serialization;

namespace BodyTracker.Models.WorkoutLog
{
    public class HevyAppWorkoutSetJsonModel
    {
        /// <summary>
        /// Represents the chronological sequence or position of the specific set within an exercise block.
        /// </summary>
        [JsonPropertyName("index")]
        public int Index { get; set; }

        /// <summary>
        /// Defines the classification or category of the set (such as normal, warm-up, failure, or drop set).
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Specifies the external resistance or load used for the exercise set, measured in kilograms.
        /// </summary>
        [JsonPropertyName("weight_kg")]
        public double? WeightKg { get; set; }

        /// <summary>
        /// Records the total number of completed repetitions performed during the given set.
        /// </summary>
        [JsonPropertyName("reps")]
        public int? Reps { get; set; }

        /// <summary>
        /// Quantifies the distance covered during distance-based or cardio exercises, measured in meters.
        /// </summary>
        [JsonPropertyName("distance_meters")]
        public double? DistanceMeters { get; set; }

        /// <summary>
        /// Captures the active time under tension or duration of the set, measured in seconds.
        /// </summary>
        [JsonPropertyName("duration_seconds")]
        public int? DurationSeconds { get; set; }

        /// <summary>
        /// Represents the Rate of Perceived Exertion (RPE), providing a quantification of the set's intensity.
        /// </summary>
        [JsonPropertyName("rpe")]
        public double? Rpe { get; set; }

        /// <summary>
        /// Acts as a flexible placeholder for user-defined or exercise-specific supplementary numerical data.
        /// </summary>
        [JsonPropertyName("custom_metric")]
        public double? CustomMetric { get; set; }
    }
}