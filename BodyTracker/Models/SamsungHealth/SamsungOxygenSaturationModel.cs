using System;

namespace BodyTracker.Models.SamsungHealth
{
    /// <summary>
    /// Represents a Samsung Health oxygen saturation (SpO2) data record including measurement metrics, heart rate, timestamps, device information, and metadata.
    /// </summary>
    public record SamsungOxygenSaturationModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the oxygen saturation record.
        /// </summary>
        public int? OxygenSaturationID { get; set; }

        /// <summary>
        /// Gets or sets the integrated record identifier.
        /// </summary>
        public string? IntegratedId { get; set; }

        /// <summary>
        /// Gets or sets the client-specific data identifier.
        /// </summary>
        public string? ClientDataId { get; set; }

        /// <summary>
        /// Gets or sets the tag identifier associated with the measurement.
        /// </summary>
        public int? TagId { get; set; }

        /// <summary>
        /// Gets or sets the start time of the oxygen saturation measurement session.
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the oxygen saturation measurement session.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the record was last updated.
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the record was created.
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// Gets or sets the time offset relative to UTC.
        /// </summary>
        public string? TimeOffset { get; set; }

        /// <summary>
        /// Gets or sets custom user or application data.
        /// </summary>
        public string? Custom { get; set; }

        /// <summary>
        /// Gets or sets the average blood oxygen saturation percentage (SpO2).
        /// </summary>
        public double? SpO2 { get; set; }

        /// <summary>
        /// Gets or sets the maximum recorded blood oxygen saturation percentage (SpO2).
        /// </summary>
        public double? SpO2Max { get; set; }

        /// <summary>
        /// Gets or sets the minimum recorded blood oxygen saturation percentage (SpO2).
        /// </summary>
        public double? SpO2Min { get; set; }

        /// <summary>
        /// Gets or sets the duration spent with low blood oxygen saturation levels.
        /// </summary>
        public int? LowSpO2Duration { get; set; }

        /// <summary>
        /// Gets or sets the SpO2 coverage rate or data quality percentage.
        /// </summary>
        public double? SpO2CoverageRate { get; set; }

        /// <summary>
        /// Gets or sets the heart rate recorded during the measurement.
        /// </summary>
        public double? HeartRate { get; set; }

        /// <summary>
        /// Gets or sets additional comments or notes for the measurement.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Gets or sets the unique data universal unique identifier (UUID).
        /// </summary>
        public string? DataUuid { get; set; }

        /// <summary>
        /// Gets or sets the unique device universal unique identifier (UUID) that recorded the data.
        /// </summary>
        public string? DeviceUuid { get; set; }

        /// <summary>
        /// Gets or sets the package name of the application that generated the record.
        /// </summary>
        public string? PackageName { get; set; }

        /// <summary>
        /// Gets or sets the Samsung Health schema version at creation.
        /// </summary>
        public int? CreateShVer { get; set; }

        /// <summary>
        /// Gets or sets the Samsung Health schema version at modification.
        /// </summary>
        public int? ModifyShVer { get; set; }

        /// <summary>
        /// Gets or sets the client data version.
        /// </summary>
        public int? ClientDataVer { get; set; }

        /// <summary>
        /// Gets or sets the data source descriptor.
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Gets or sets the binning configuration or aggregation method.
        /// </summary>
        public string? Binning { get; set; }
    }
}