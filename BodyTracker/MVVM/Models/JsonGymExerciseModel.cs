using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BodyTracker.MVVM.Models
{
    public class JsonGymExerciseModel
    {

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("force")]
        public string? Force { get; set; }

        [JsonPropertyName("level")]
        public string? Level { get; set; }

        [JsonPropertyName("mechanic")]
        public string? Mechanic { get; set; }

        [JsonPropertyName("equipment")]
        public string? Equipment { get; set; }

        [JsonPropertyName("primaryMuscles")]
        public List<string> PrimaryMuscles { get; set; } = new();

        [JsonPropertyName("secondaryMuscles")]
        public List<string> SecondaryMuscles { get; set; } = new();

        [JsonPropertyName("instructions")]
        public List<string> Instructions { get; set; } = new();

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

    }
}
