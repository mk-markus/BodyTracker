using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BodyTracker.Models.WorkoutLog
{
    public class HevyAppWorkoutJsonModel
    {
        /// <summary>
        /// Represents the unique string identifier for the specific workout session as provided by the external platform.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Specifies the custom or predefined title of the workout session (e.g., Upper Body Strength).
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Holds the optional identifier linking this workout instance back to its originating routine template, if applicable.
        /// </summary>
        [JsonPropertyName("routine_id")]
        public string? RoutineId { get; set; }

        /// <summary>
        /// Captures any optional text notes, summaries, or general remarks regarding the overall workout session.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Records the precise starting timestamp of the workout session, including timezone offset information.
        /// </summary>
        [JsonPropertyName("start_time")]
        public DateTimeOffset StartTime { get; set; }

        /// <summary>
        /// Records the precise ending timestamp of the workout session, including timezone offset information.
        /// </summary>
        [JsonPropertyName("end_time")]
        public DateTimeOffset EndTime { get; set; }

        /// <summary>
        /// Indicates the timestamp when the workout record was last modified or updated in the source system.
        /// </summary>
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// Indicates the timestamp when the workout record was initially created or logged.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Contains the collection of individual exercises performed during this workout session, mapped to HevyAppExerciseJsonModel instances.
        /// </summary>
        [JsonPropertyName("exercises")]
        public List<HevyAppExerciseJsonModel> Exercises { get; set; } = new();
    }
}