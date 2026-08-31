using System;

namespace BodyTracker.Models.SamsungHealth
{
    public class SamsungOxygenSaturationModel
    {
        public int? OxygenSaturationID { get; set; }
        public string? IntegratedId { get; set; }
        public string? ClientDataId { get; set; }
        public int? TagId { get; set; }
        
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public DateTime? CreateTime { get; set; }
        public string? TimeOffset { get; set; }

        public string? Custom { get; set; }
       
        public double? SpO2 { get; set; }
        public double? SpO2Max { get; set; }
        public double? SpO2Min { get; set; }
        public int? LowSpO2Duration { get; set; }
        public double? SpO2CoverageRate { get; set; }

        public double? HeartRate { get; set; }

        public string? Comment { get; set; }

        public string? DataUuid { get; set; }
        public string? DeviceUuid { get; set; }

        public string? PackageName { get; set; }
        public int? CreateShVer { get; set; }
        public int? ModifyShVer { get; set; }
        public int? ClientDataVer { get; set; }
        
        public string? Source { get; set; }
        public string? Binning { get; set; }
    }
}