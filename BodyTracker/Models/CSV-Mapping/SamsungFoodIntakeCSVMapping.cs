using CsvHelper.Configuration;
using BodyTracker.Models;

public sealed class SamsungFoodIntakeMap : ClassMap<SamsungFoodIntakeModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SamsungFoodIntakeMap"/> class, mapping model properties to CSV columns.
    /// </summary>
    /// <remarks>Configures CsvHelper property mappings for <see cref="SamsungFoodIntakeModel"/> fields corresponding to Samsung Health data headers.</remarks>
    public SamsungFoodIntakeMap()
    {
        Map(m => m.CreateShVer).Name("create_sh_ver");
        Map(m => m.StartTime).Name("start_time");
        Map(m => m.Amount).Name("amount");
        Map(m => m.Custom).Name("custom");
        Map(m => m.ModifyShVer).Name("modify_sh_ver");
        Map(m => m.UpdateTime).Name("update_time");
        Map(m => m.CreateTime).Name("create_time");
        Map(m => m.MealType).Name("meal_type");
        Map(m => m.ClientDataId).Name("client_data_id");
        Map(m => m.Name).Name("name");
        Map(m => m.Unit).Name("unit");
        Map(m => m.ClientDataVer).Name("client_data_ver");
        Map(m => m.Calorie).Name("calorie");
        Map(m => m.TimeOffset).Name("time_offset");
        Map(m => m.DeviceUuid).Name("deviceuuid");
        Map(m => m.Comment).Name("comment");
        Map(m => m.PkgName).Name("pkg_name");
        Map(m => m.DataUuid).Name("datauuid");
        Map(m => m.FoodInfoId).Name("food_info_id");
    }
}
