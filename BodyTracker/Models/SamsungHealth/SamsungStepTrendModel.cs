using System;

namespace BodyTracker.Models
{
    /// <summary>
    /// Represents a daily step trend record exported from Samsung Health.
    /// </summary>
    public class SamsungStepTrendModel
    {
        /// <summary>
        /// Reference to the associated binary/JSON data file.
        /// </summary>
        public string BinningData { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp indicating when the record was last updated.
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// The timestamp indicating when the record was created.
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// The package name of the data source.
        /// </summary>
        public string SourcePkgName { get; set; } = string.Empty;

        /// <summary>
        /// The type identifier of the data source.
        /// </summary>
        public int SourceType { get; set; }

        /// <summary>
        /// The total number of steps recorded.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The average speed during the recorded activity.
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// The total distance traveled, measured in meters.
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// The number of calories burned during the recorded activity.
        /// </summary>
        public double Calorie { get; set; }

        /// <summary>
        /// The unique identifier of the device that recorded the data.
        /// </summary>
        public string DeviceUuid { get; set; } = string.Empty;

        /// <summary>
        /// The package name of the application that generated the record.
        /// </summary>
        public string PkgName { get; set; } = string.Empty;

        /// <summary>
        /// The globally unique identifier of the record.
        /// </summary>
        public string? DataUuid { get; set; }

        /// <summary>
        /// The date associated with the daily trend record.
        /// </summary>
        public DateTime DayTime { get; set; }
    }
}