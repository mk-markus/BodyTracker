using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BodyTracker.Models.WorkoutLog
{
    public class HevyAppExerciseJsonModel
    {
        /// <summary>
        /// Represents the chronological sequence or position of the exercise within the overall workout session.
        /// </summary>
        [JsonPropertyName("index")]
        public int Index { get; set; }

        /// <summary>
        /// Specifies the name or title of the exercise performed (e.g., Bench Press or Squat).
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Captures any optional, user-entered commentary or instructions specific to this exercise instance.
        /// </summary>
        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        /// <summary>
        /// Holds the unique identifier linking this exercise entry to its corresponding master template definition.
        /// </summary>
        [JsonPropertyName("exercise_template_id")]
        public string ExerciseTemplateId { get; set; } = string.Empty;

        /// <summary>
        /// Identifies the superset grouping if the exercise is performed as part of a paired or chained superset block.
        /// </summary>
        [JsonPropertyName("superset_id")]
        public string? SupersetId { get; set; }

        /// <summary>
        /// Contains the collection of individual sets performed for this exercise, mapped to HevyAppWorkoutSetJsonModel instances.
        /// </summary>
        [JsonPropertyName("sets")]
        public List<HevyAppWorkoutSetJsonModel> Sets { get; set; } = new();
    }
}