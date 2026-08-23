using System;

public class SamsungHeartRateModel
{
    public string? Source { get; set; }
    public int? TagId { get; set; }

    public int? CreateShVer { get; set; }
    
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public DateTime? CreateTime { get; set; }
    public string? TimeOffset { get; set; }

    public string? Custom { get; set; }
    public string? BinningData { get; set; }

    public int? ModifyShVer { get; set; }


    public string? ClientDataId { get; set; }

    public double? HeartRate { get; set; }
    public double? HeartRateMax { get; set; }
    public double? HeartRateMin { get; set; }
    public int? HeartBeatCount { get; set; }

    public int? ClientDataVer { get; set; }
    
    public string? DeviceUuid { get; set; }
    public string? Comment { get; set; }
    public string? PackageName { get; set; }


    public string? DataUuid { get; set; }

}
