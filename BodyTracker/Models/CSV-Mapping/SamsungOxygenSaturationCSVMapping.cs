using BodyTracker.Models.SamsungHealth;
using CsvHelper.Configuration;

public sealed class SamsungOxygenSaturationMap : ClassMap<SamsungOxygenSaturationModel>
{
    public SamsungOxygenSaturationMap()
    {
        Map(m => m.IntegratedId).Name("integrated_id");

        Map(m => m.Source).Name("source");

        Map(m => m.TagId).Name("tag_id");

        Map(m => m.SpO2CoverageRate).Name("coverage_rate");

        Map(m => m.CreateShVer).Name("com.samsung.health.oxygen_saturation.create_sh_ver");

        Map(m => m.StartTime).Name("com.samsung.health.oxygen_saturation.start_time");

        Map(m => m.Custom).Name("com.samsung.health.oxygen_saturation.custom");

        Map(m => m.ModifyShVer).Name("com.samsung.health.oxygen_saturation.modify_sh_ver");

        Map(m => m.UpdateTime).Name("com.samsung.health.oxygen_saturation.update_time");

        Map(m => m.CreateTime).Name("com.samsung.health.oxygen_saturation.create_time");

        Map(m => m.ClientDataId).Name("com.samsung.health.oxygen_saturation.client_data_id");

        Map(m => m.LowSpO2Duration).Name("com.samsung.health.oxygen_saturation.low_duration");

        Map(m => m.Binning).Name("com.samsung.health.oxygen_saturation.binning");

        Map(m => m.SpO2Max).Name("com.samsung.health.oxygen_saturation.max");

        Map(m => m.SpO2Min).Name("com.samsung.health.oxygen_saturation.min");

        Map(m => m.SpO2).Name("com.samsung.health.oxygen_saturation.spo2");

        Map(m => m.ClientDataVer).Name("com.samsung.health.oxygen_saturation.client_data_ver");

        Map(m => m.TimeOffset).Name("com.samsung.health.oxygen_saturation.time_offset");

        Map(m => m.DeviceUuid).Name("com.samsung.health.oxygen_saturation.deviceuuid");

        Map(m => m.Comment).Name("com.samsung.health.oxygen_saturation.comment");

        Map(m => m.PackageName).Name("com.samsung.health.oxygen_saturation.pkg_name");

        Map(m => m.EndTime).Name("com.samsung.health.oxygen_saturation.end_time");

        Map(m => m.DataUuid).Name("com.samsung.health.oxygen_saturation.datauuid");

        Map(m => m.HeartRate).Name("com.samsung.health.oxygen_saturation.heart_rate");
    }
}