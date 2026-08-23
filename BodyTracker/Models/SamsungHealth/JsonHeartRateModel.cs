using System;
using System.Text.Json.Serialization;

namespace BodyTracker.Models.SamsungHealth
{
    /// <summary>
    /// Represents a JSON data transfer model for heart rate records containing raw timestamps, average values, and computed offset properties.
    /// </summary>
    public class JsonHeartRateModel
    {
        /// <summary>
        /// Gets or sets the average heart rate value recorded during the session.
        /// </summary>
        [JsonPropertyName("heart_rate")]
        public float HeartRate { get; set; }

        /// <summary>
        /// Gets or sets the maximum heart rate value recorded during the session.
        /// </summary>
        [JsonPropertyName("heart_rate_max")]
        public float HeartRateMax { get; set; }

        /// <summary>
        /// Gets or sets the minimum heart rate value recorded during the session.
        /// </summary>
        [JsonPropertyName("heart_rate_min")]
        public float HeartRateMin { get; set; }

        /// <summary>
        /// Gets or sets the start time of the measurement as a Unix timestamp in milliseconds.
        /// </summary>
        [JsonPropertyName("start_time")]
        public long StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the measurement as a Unix timestamp in milliseconds.
        /// </summary>
        [JsonPropertyName("end_time")]
        public long EndTime { get; set; }

        /// <summary>
        /// Gets the start time converted to a localized <see cref="DateTimeOffset"/> instance.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset StartDateOffset => DateTimeOffset.FromUnixTimeMilliseconds(StartTime);

        /// <summary>
        /// Gets the end time converted to a localized <see cref="DateTimeOffset"/> instance.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset EndTimeOffset => DateTimeOffset.FromUnixTimeMilliseconds(EndTime);
    }
}
