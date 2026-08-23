using CsvHelper.Configuration;

public sealed class SamsungHeartRateMap : ClassMap<SamsungHeartRateModel>
{
    public SamsungHeartRateMap()
    {
        Map(m => m.Source).Name("source");

        Map(m => m.TagId).Name("tag_id");

        Map(m => m.CreateShVer).Name("com.samsung.health.heart_rate.create_sh_ver");

        Map(m => m.HeartBeatCount).Name("com.samsung.health.heart_rate.heart_beat_count");

        Map(m => m.StartTime).Name("com.samsung.health.heart_rate.start_time");

        Map(m => m.Custom).Name("com.samsung.health.heart_rate.custom");

        Map(m => m.BinningData).Name("com.samsung.health.heart_rate.binning_data");

        Map(m => m.ModifyShVer).Name("com.samsung.health.heart_rate.modify_sh_ver");

        Map(m => m.UpdateTime).Name("com.samsung.health.heart_rate.update_time");

        Map(m => m.CreateTime).Name("com.samsung.health.heart_rate.create_time");

        Map(m => m.ClientDataId).Name("com.samsung.health.heart_rate.client_data_id");

        Map(m => m.HeartRateMax).Name("com.samsung.health.heart_rate.max");

        Map(m => m.HeartRateMin).Name("com.samsung.health.heart_rate.min");

        Map(m => m.ClientDataVer).Name("com.samsung.health.heart_rate.client_data_ver");

        Map(m => m.TimeOffset).Name("com.samsung.health.heart_rate.time_offset");

        Map(m => m.DeviceUuid).Name("com.samsung.health.heart_rate.deviceuuid");

        Map(m => m.Comment).Name("com.samsung.health.heart_rate.comment");

        Map(m => m.PackageName).Name("com.samsung.health.heart_rate.pkg_name");

        Map(m => m.EndTime).Name("com.samsung.health.heart_rate.end_time");

        Map(m => m.DataUuid).Name("com.samsung.health.heart_rate.datauuid");

        Map(m => m.HeartRate).Name("com.samsung.health.heart_rate.heart_rate");
    }
}