using System;

public class SamsungHeartRateModel
{

    // <summary>
    /// Contains the heart rate ID from the database table
    /// </summary>
    public int? HeartRateID { get; set; }


    /// <summary>
    /// Gets or sets the data source identifier or origin.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the tag identifier associated with the heart rate record.
    /// </summary>
    public int? TagId { get; set; }

    /// <summary>
    /// Gets or sets the creation synchronization version from Samsung Health.
    /// </summary>
    public int? CreateShVer { get; set; }

    /// <summary>
    /// Gets or sets the exact start timestamp of the heart rate measurement period.
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Gets or sets the exact end timestamp of the heart rate measurement period.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the record was last updated.
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the record was initially created.
    /// </summary>
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// Gets or sets the timezone offset for the recorded timestamps.
    /// </summary>
    public string? TimeOffset { get; set; }

    /// <summary>
    /// Gets or sets custom user notes or custom attributes.
    /// </summary>
    public string? Custom { get; set; }

    /// <summary>
    /// Gets or sets aggregated or binned interval data payloads.
    /// </summary>
    public string? BinningData { get; set; }

    /// <summary>
    /// Gets or sets the modification synchronization version from Samsung Health.
    /// </summary>
    public int? ModifyShVer { get; set; }

    /// <summary>
    /// Gets or sets the client-side unique data identifier.
    /// </summary>
    public string? ClientDataId { get; set; }

    /// <summary>
    /// Gets or sets the primary recorded heart rate value (in beats per minute).
    /// </summary>
    public double? HeartRate { get; set; }

    /// <summary>
    /// Gets or sets the peak heart rate recorded during the measurement interval.
    /// </summary>
    public double? HeartRateMax { get; set; }

    /// <summary>
    /// Gets or sets the lowest heart rate recorded during the measurement interval.
    /// </summary>
    public double? HeartRateMin { get; set; }

    /// <summary>
    /// Gets or sets the total count of heart beats recorded during the session.
    /// </summary>
    public int? HeartBeatCount { get; set; }

    /// <summary>
    /// Gets or sets the client-side data version identifier.
    /// </summary>
    public int? ClientDataVer { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the recording device.
    /// </summary>
    public string? DeviceUuid { get; set; }

    /// <summary>
    /// Gets or sets user comments or notes attached to the heart rate record.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the package name of the application that generated the record.
    /// </summary>
    public string? PackageName { get; set; }

    /// <summary>
    /// Gets or sets the universally unique identifier for the specific data record.
    /// </summary>
    public string? DataUuid { get; set; }
}