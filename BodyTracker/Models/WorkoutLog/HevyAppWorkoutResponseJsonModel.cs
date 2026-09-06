using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BodyTracker.Models.WorkoutLog
{
    public class HevyAppWorkoutResponseJsonModel
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("page_count")]
        public int PageCount { get; set; }



        /// <summary>
        /// Contains the complete collection of exported workout sessions retrieved from the external application.
        /// </summary>
        [JsonPropertyName("workouts")]
        public List<HevyAppWorkoutJsonModel> Workouts { get; set; } = new();
    }
}