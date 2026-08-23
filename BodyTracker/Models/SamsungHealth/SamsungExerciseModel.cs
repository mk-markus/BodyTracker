using System;

public class SamsungExerciseModel
{
   
    public string? LiveDataInternal { get; set; }
    public string? MissionValue { get; set; }
    public string? RaceTarget { get; set; }
    public string? SubsetData { get; set; }
    public double? StartLongitude { get; set; }
    public string? RoutineDataUuid { get; set; }
    public double? TotalCalorie { get; set; }
    public int? CompletionStatus { get; set; }
    public long? PaceInfoId { get; set; }
    public int? ActivityType { get; set; }
    public string? PaceLiveData { get; set; }
    public string? SensingStatus { get; set; }
    public int? SourceType { get; set; }
    public int? MissionType { get; set; }
    public double? Ftp { get; set; }
    public int? TrackingStatus { get; set; }
    public long? ProgramId { get; set; }
    public string? Title { get; set; }
    public int? RewardStatus { get; set; }
    public int? HeartRateSampleCount { get; set; }
    public double? StartLatitude { get; set; }
    public string? MissionExtraValue { get; set; }
    public long? ProgramScheduleId { get; set; }
    public string? HeartRateDeviceUuid { get; set; }
    public string? LocationDataInternal { get; set; }
    public string? CustomId { get; set; }
    public string? AdditionalInternal { get; set; }

   
    public long? Duration { get; set; }
    public string? Additional { get; set; }
    public string? CreateShVer { get; set; }
    public double? MeanCaloricBurnRate { get; set; }
    public string? LocationData { get; set; }

   
    public DateTime? StartTime { get; set; }

    public int? ExerciseType { get; set; }
    public string? Custom { get; set; }
    public double? MaxAltitude { get; set; }
    public double? InclineDistance { get; set; }
    public double? MeanHeartRate { get; set; }
    public int? CountType { get; set; }
    public double? MeanRpm { get; set; }
    public double? MinAltitude { get; set; }
    public string? ModifyShVer { get; set; }
    public double? MaxHeartRate { get; set; }

    public DateTime? UpdateTime { get; set; }
    public DateTime? CreateTime { get; set; }

    public string? ClientDataId { get; set; }
    public double? MaxPower { get; set; }
    public double? MaxSpeed { get; set; }
    public double? MeanCadence { get; set; }
    public double? MinHeartRate { get; set; }
    public string? ClientDataVer { get; set; }

    public int? Count { get; set; }
    public double? Distance { get; set; }
    public double? MaxCaloricBurnRate { get; set; }
    public double? Calorie { get; set; }
    public double? MaxCadence { get; set; }
    public double? DeclineDistance { get; set; }
    public double? Vo2Max { get; set; }

    public string? TimeOffset { get; set; }

    public string? DeviceUuid { get; set; }
    public double? MaxRpm { get; set; }
    public string? Comment { get; set; }
    public string? LiveData { get; set; }
    public double? MeanPower { get; set; }
    public double? MeanSpeed { get; set; }
    public string? PkgName { get; set; }

    public double? AltitudeGain { get; set; }
    public double? AltitudeLoss { get; set; }

    public int? ExerciseCustomType { get; set; }

    public string? AuxiliaryDevices { get; set; }

    public DateTime? EndTime { get; set; }

    public string? DataUuid { get; set; }

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