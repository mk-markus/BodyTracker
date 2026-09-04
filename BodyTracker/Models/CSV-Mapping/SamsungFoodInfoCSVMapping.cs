using CsvHelper.Configuration;
using BodyTracker.Models;

/// <summary>
/// Represents the CsvHelper class map for mapping <see cref="SamsungFoodInfoModel"/> properties to CSV file headers.
/// </summary>
/// <remarks>Configures column name mappings for Samsung Health food information records.</remarks>
public sealed class SamsungFoodInfoCSVMapping : ClassMap<SamsungFoodInfoModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SamsungFoodInfoCSVMapping"/> class, mapping model properties to CSV columns.
    /// </summary>
    public SamsungFoodInfoCSVMapping()
    {
        Map(m => m.Potassium).Name("potassium");
        Map(m => m.VitaminA).Name("vitamin_a");
        Map(m => m.VitaminC).Name("vitamin_c");
        Map(m => m.VitaminD).Name("vitamin_d");
        Map(m => m.Cholesterol).Name("cholesterol");
        Map(m => m.Description).Name("description");
        Map(m => m.Custom).Name("custom");
        Map(m => m.ProviderFoodId).Name("provider_food_id");
        Map(m => m.MetricServingAmount).Name("metric_serving_amount");
        Map(m => m.Sodium).Name("sodium");
        Map(m => m.DietaryFiber).Name("dietary_fiber");
        Map(m => m.TotalFat).Name("total_fat");
        Map(m => m.UpdateTime).Name("update_time");
        Map(m => m.CreateTime).Name("create_time");
        Map(m => m.MonosaturatedFat).Name("monosaturated_fat");
        Map(m => m.Protein).Name("protein");
        Map(m => m.PolysaturatedFat).Name("polysaturated_fat");
        Map(m => m.Iron).Name("iron");
        Map(m => m.Name).Name("name");
        Map(m => m.Sugar).Name("sugar");
        Map(m => m.AddedSugar).Name("added_sugar");
        Map(m => m.Calcium).Name("calcium");
        Map(m => m.Calorie).Name("calorie");
        Map(m => m.ServingDescription).Name("serving_description");
        Map(m => m.InfoProvider).Name("info_provider");
        Map(m => m.DeviceUuid).Name("deviceuuid");
        Map(m => m.MetricServingUnit).Name("metric_serving_unit");
        Map(m => m.SaturatedFat).Name("saturated_fat");
        Map(m => m.TransFat).Name("trans_fat");
        Map(m => m.PkgName).Name("pkg_name");
        Map(m => m.Carbohydrate).Name("carbohydrate");
        Map(m => m.UnitCountPerCalorie).Name("unit_count_per_calorie");
        Map(m => m.DataUuid).Name("datauuid");
        Map(m => m.DefaultNumberOfServingUnit).Name("default_number_of_serving_unit");
    }
}