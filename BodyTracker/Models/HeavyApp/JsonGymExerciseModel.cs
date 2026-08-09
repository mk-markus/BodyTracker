using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BodyTracker.Models
{
    public class JsonGymExerciseModel
    {
        /// <summary>
        /// Gets or sets the name of the exercise.
        /// </summary>
        /// <remarks>Mapped to the JSON property "name". Initialized to an empty string by default.</remarks>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of force involved in the exercise (e.g., push, pull).
        /// </summary>
        /// <remarks>Mapped to the JSON property "force". Can be null if not specified.</remarks>
        [JsonPropertyName("force")]
        public string? Force { get; set; }

        /// <summary>
        /// Gets or sets the difficulty level of the exercise (e.g., beginner, intermediate, expert).
        /// </summary>
        /// <remarks>Mapped to the JSON property "level". Can be null if not specified.</remarks>
        [JsonPropertyName("level")]
        public string? Level { get; set; }

        /// <summary>
        /// Gets or sets the mechanical nature or movement type of the exercise.
        /// </summary>
        /// <remarks>Mapped to the JSON property "mechanic". Can be null if not specified.</remarks>
        [JsonPropertyName("mechanic")]
        public string? Mechanic { get; set; }

        /// <summary>
        /// Gets or sets the equipment required or used to perform the exercise.
        /// </summary>
        /// <remarks>Mapped to the JSON property "equipment". Can be null if not specified.</remarks>
        [JsonPropertyName("equipment")]
        public string? Equipment { get; set; }

        /// <summary>
        /// Gets or sets the list of primary muscle groups targeted by the exercise.
        /// </summary>
        /// <remarks>Mapped to the JSON property "primaryMuscles". Initialized to an empty list by default.</remarks>
        [JsonPropertyName("primaryMuscles")]
        public List<string> PrimaryMuscles { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of secondary muscle groups synergistically involved in the exercise.
        /// </summary>
        /// <remarks>Mapped to the JSON property "secondaryMuscles". Initialized to an empty list by default.</remarks>
        [JsonPropertyName("secondaryMuscles")]
        public List<string> SecondaryMuscles { get; set; } = new();

        /// <summary>
        /// Gets or sets the step-by-step instructions for performing the exercise.
        /// </summary>
        /// <remarks>Mapped to the JSON property "instructions". Initialized to an empty list by default.</remarks>
        [JsonPropertyName("instructions")]
        public List<string> Instructions { get; set; } = new();

        /// <summary>
        /// Gets or sets the category classification of the exercise (e.g., strength, stretching).
        /// </summary>
        /// <remarks>Mapped to the JSON property "category". Can be null if not specified.</remarks>
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the exercise.
        /// </summary>
        /// <remarks>Mapped to the JSON property "id". Initialized to an empty string by default.</remarks>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

    }
}
