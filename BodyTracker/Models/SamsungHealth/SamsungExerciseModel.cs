using System;

public class SamsungExerciseModel
{
    /// <summary>
    /// Gets or sets the unique exercise record identifier.
    /// </summary>
    public int? ExerciseID { get; set; }

    /// <summary>
    /// Gets or sets internal live data tracking information.
    /// </summary>
    public string? LiveDataInternal { get; set; }

    /// <summary>
    /// Gets or sets the target or value associated with a specific exercise mission.
    /// </summary>
    public string? MissionValue { get; set; }

    /// <summary>
    /// Gets or sets the target goal for a race or competitive exercise mode.
    /// </summary>
    public string? RaceTarget { get; set; }

    /// <summary>
    /// Gets or sets subset-specific data configurations.
    /// </summary>
    public string? SubsetData { get; set; }

    /// <summary>
    /// Gets or sets the starting longitude coordinate of the exercise route.
    /// </summary>
    public double? StartLongitude { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for associated routine data.
    /// </summary>
    public string? RoutineDataUuid { get; set; }

    /// <summary>
    /// Gets or sets the total calories burned during the exercise session.
    /// </summary>
    public double? TotalCalorie { get; set; }

    /// <summary>
    /// Gets or sets the completion status code of the exercise or goal.
    /// </summary>
    public int? CompletionStatus { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for pace-related information.
    /// </summary>
    public long? PaceInfoId { get; set; }

    /// <summary>
    /// Gets or sets the general category type of the activity.
    /// </summary>
    public int? ActivityType { get; set; }

    /// <summary>
    /// Gets or sets live pace tracking data during the workout.
    /// </summary>
    public string? PaceLiveData { get; set; }

    /// <summary>
    /// Gets or sets the sensor status during data acquisition.
    /// </summary>
    public string? SensingStatus { get; set; }

    /// <summary>
    /// Gets or sets the source type or origin identifier of the recorded data.
    /// </summary>
    public int? SourceType { get; set; }

    /// <summary>
    /// Gets or sets the type of mission associated with the workout.
    /// </summary>
    public int? MissionType { get; set; }

    /// <summary>
    /// Gets or sets the Functional Threshold Power (FTP) value during cycling or high-intensity efforts.
    /// </summary>
    public double? Ftp { get; set; }

    /// <summary>
    /// Gets or sets the tracking status code for the workout session.
    /// </summary>
    public int? TrackingStatus { get; set; }

    /// <summary>
    /// Gets or sets the unique program identifier associated with a training plan.
    /// </summary>
    public long? ProgramId { get; set; }

    /// <summary>
    /// Gets or sets the title or name of the exercise session.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the reward status achieved for completing the workout or mission.
    /// </summary>
    public int? RewardStatus { get; set; }

    /// <summary>
    /// Gets or sets the total count of recorded heart rate samples.
    /// </summary>
    public int? HeartRateSampleCount { get; set; }

    /// <summary>
    /// Gets or sets the starting latitude coordinate of the exercise route.
    /// </summary>
    public double? StartLatitude { get; set; }

    /// <summary>
    /// Gets or sets additional metadata or values related to exercise missions.
    /// </summary>
    public string? MissionExtraValue { get; set; }

    /// <summary>
    /// Gets or sets the unique schedule identifier for a training program.
    /// </summary>
    public long? ProgramScheduleId { get; set; }

    /// <summary>
    /// Gets or sets the unique device identifier of the connected heart rate monitor.
    /// </summary>
    public string? HeartRateDeviceUuid { get; set; }

    /// <summary>
    /// Gets or sets internal location tracking data structures.
    /// </summary>
    public string? LocationDataInternal { get; set; }

    /// <summary>
    /// Gets or sets a custom identifier defined by the user or application.
    /// </summary>
    public string? CustomId { get; set; }

    /// <summary>
    /// Gets or sets additional internal system data or flags.
    /// </summary>
    public string? AdditionalInternal { get; set; }

    /// <summary>
    /// Gets or sets the total duration of the exercise session in milliseconds or seconds.
    /// </summary>
    public long? Duration { get; set; }

    /// <summary>
    /// Gets or sets additional descriptive text or supplementary information.
    /// </summary>
    public string? Additional { get; set; }

    /// <summary>
    /// Gets or sets the creation synchronization version from Samsung Health.
    /// </summary>
    public string? CreateShVer { get; set; }

    /// <summary>
    /// Gets or sets the average rate of calorie consumption during the workout.
    /// </summary>
    public double? MeanCaloricBurnRate { get; set; }

    /// <summary>
    /// Gets or sets structured location and route tracking data.
    /// </summary>
    public string? LocationData { get; set; }

    /// <summary>
    /// Gets or sets the exact start timestamp of the exercise session.
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Gets or sets the specific exercise type identifier defined by Samsung Health.
    /// </summary>
    public int? ExerciseType { get; set; }

    /// <summary>
    /// Gets or sets custom user notes or custom attributes.
    /// </summary>
    public string? Custom { get; set; }

    /// <summary>
    /// Gets or sets the maximum recorded altitude during the exercise.
    /// </summary>
    public double? MaxAltitude { get; set; }

    /// <summary>
    /// Gets or sets the total distance covered while moving uphill.
    /// </summary>
    public double? InclineDistance { get; set; }

    /// <summary>
    /// Gets or sets the average heart rate during the exercise session.
    /// </summary>
    public double? MeanHeartRate { get; set; }

    /// <summary>
    /// Gets or sets the counting mechanism or type used during the exercise.
    /// </summary>
    public int? CountType { get; set; }

    /// <summary>
    /// Gets or sets the average revolutions per minute (RPM) for cycling or rotational activities.
    /// </summary>
    public double? MeanRpm { get; set; }

    /// <summary>
    /// Gets or sets the minimum recorded altitude during the exercise.
    /// </summary>
    public double? MinAltitude { get; set; }

    /// <summary>
    /// Gets or sets the modification synchronization version from Samsung Health.
    /// </summary>
    public string? ModifyShVer { get; set; }

    /// <summary>
    /// Gets or sets the peak heart rate recorded during the session.
    /// </summary>
    public double? MaxHeartRate { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the record was last updated.
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the record was initially created.
    /// </summary>
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// Gets or sets the client-side unique data identifier.
    /// </summary>
    public string? ClientDataId { get; set; }

    /// <summary>
    /// Gets or sets the maximum power output (in watts) recorded during the exercise.
    /// </summary>
    public double? MaxPower { get; set; }

    /// <summary>
    /// Gets or sets the maximum movement speed achieved during the workout.
    /// </summary>
    public double? MaxSpeed { get; set; }

    /// <summary>
    /// Gets or sets the average cadence (steps or pedal strokes per minute).
    /// </summary>
    public double? MeanCadence { get; set; }

    /// <summary>
    /// Gets or sets the lowest heart rate recorded during the session.
    /// </summary>
    public double? MinHeartRate { get; set; }

    /// <summary>
    /// Gets or sets the client-side data version identifier.
    /// </summary>
    public string? ClientDataVer { get; set; }

    /// <summary>
    /// Gets or sets the total repetition or step count.
    /// </summary>
    public int? Count { get; set; }

    /// <summary>
    /// Gets or sets the total distance covered during the exercise.
    /// </summary>
    public double? Distance { get; set; }

    /// <summary>
    /// Gets or sets the peak rate of calorie consumption during the workout.
    /// </summary>
    public double? MaxCaloricBurnRate { get; set; }

    /// <summary>
    /// Gets or sets the total calories burned (alternative or standardized property).
    /// </summary>
    public double? Calorie { get; set; }

    /// <summary>
    /// Gets or sets the maximum cadence recorded during the session.
    /// </summary>
    public double? MaxCadence { get; set; }

    /// <summary>
    /// Gets or sets the total distance covered while moving downhill.
    /// </summary>
    public double? DeclineDistance { get; set; }

    /// <summary>
    /// Gets or sets the estimated VO2 max (maximal oxygen uptake) value.
    /// </summary>
    public double? Vo2Max { get; set; }

    /// <summary>
    /// Gets or sets the timezone offset for the recorded timestamps.
    /// </summary>
    public string? TimeOffset { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the recording device.
    /// </summary>
    public string? DeviceUuid { get; set; }

    /// <summary>
    /// Gets or sets the maximum revolutions per minute (RPM) recorded.
    /// </summary>
    public double? MaxRpm { get; set; }

    /// <summary>
    /// Gets or sets user comments or notes attached to the workout.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets live telemetry or streaming exercise data.
    /// </summary>
    public string? LiveData { get; set; }

    /// <summary>
    /// Gets or sets the average power output (in watts) during the session.
    /// </summary>
    public double? MeanPower { get; set; }

    /// <summary>
    /// Gets or sets the average movement speed during the workout.
    /// </summary>
    public double? MeanSpeed { get; set; }

    /// <summary>
    /// Gets or sets the package name of the application that generated the record.
    /// </summary>
    public string? PkgName { get; set; }

    /// <summary>
    /// Gets or sets the total cumulative elevation gained during the exercise.
    /// </summary>
    public double? AltitudeGain { get; set; }

    /// <summary>
    /// Gets or sets the total cumulative elevation lost during the exercise.
    /// </summary>
    public double? AltitudeLoss { get; set; }

    /// <summary>
    /// Gets or sets a custom sub-type classification for the exercise.
    /// </summary>
    public int? ExerciseCustomType { get; set; }

    /// <summary>
    /// Gets or sets information regarding connected auxiliary devices or accessories.
    /// </summary>
    public string? AuxiliaryDevices { get; set; }

    /// <summary>
    /// Gets or sets the exact end timestamp of the exercise session.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the universally unique identifier for the specific data record.
    /// </summary>
    public string? DataUuid { get; set; }

    /// <summary>
    /// Gets or sets the estimated fluid/sweat loss during the workout.
    /// </summary>
    public double? SweatLoss { get; set; }


    //public DateTimeOffset? StartDateTime =>
    //StartTime.HasValue
    //    ? DateTimeOffset.FromUnixTimeMilliseconds(StartTime.Value)
    //    : null;

    //public DateTimeOffset? EndDateTime =>
    //    EndTime.HasValue
    //        ? DateTimeOffset.FromUnixTimeMilliseconds(EndTime.Value)
    //        : null;

}