using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BodyTracker.Models.WorkoutLog
{
    /// <summary>
    /// Represents the paginated JSON response container returned by the Hevy workout API, containing pagination metadata and workout session lists.
    /// </summary>
    public record HevyAppWorkoutResponseJsonModel
    {
        /// <summary>
        /// Gets or sets the current page number in the paginated response.
        /// </summary>
        [JsonPropertyName("page")]
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the total number of available pages.
        /// </summary>
        [JsonPropertyName("page_count")]
        public int PageCount { get; set; }

        /// <summary>
        /// Contains the complete collection of exported workout sessions retrieved from the external application.
        /// </summary>
        [JsonPropertyName("workouts")]
        public List<HevyAppWorkoutJsonModel> Workouts { get; set; } = new();
    }
}