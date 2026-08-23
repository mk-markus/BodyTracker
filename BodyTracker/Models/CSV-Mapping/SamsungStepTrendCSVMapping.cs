using CsvHelper.Configuration;
using BodyTracker.Models;

/// <summary>
/// Represents the CsvHelper class map for mapping <see cref="SamsungStepTrendModel"/> properties to CSV file headers.
/// </summary>
/// <remarks>Configures column name mappings for Samsung Health step trend records.</remarks>
public sealed class SamsungStepTrendMap : ClassMap<SamsungStepTrendModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SamsungStepTrendMap"/> class, mapping model properties to CSV columns.
    /// </summary>
    /// <remarks>Configures CsvHelper property mappings for <see cref="SamsungStepTrendModel"/> fields corresponding to Samsung Health data headers.</remarks>
    public SamsungStepTrendMap()
    {
        Map(m => m.BinningData).Name("binning_data");
        Map(m => m.UpdateTime).Name("update_time");
        Map(m => m.CreateTime).Name("create_time");
        Map(m => m.SourcePkgName).Name("source_pkg_name");
        Map(m => m.SourceType).Name("source_type");
        Map(m => m.Count).Name("count");
        Map(m => m.Speed).Name("speed");
        Map(m => m.Distance).Name("distance");
        Map(m => m.Calorie).Name("calorie");
        Map(m => m.DeviceUuid).Name("deviceuuid");
        Map(m => m.PkgName).Name("pkg_name");
        Map(m => m.DataUuid).Name("datauuid");
        Map(m => m.DayTime).Name("day_time");
    }
}



