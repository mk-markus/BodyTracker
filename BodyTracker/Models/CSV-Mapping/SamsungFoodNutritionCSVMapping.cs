using CsvHelper.Configuration;

/// <summary>
/// Defines the CSV mapping configuration for <see cref="SamsungNutritionModel"/> using CsvHelper,
/// mapping property names to their corresponding Samsung Health database/export column names.
/// </summary>
public sealed class SamsungFoodNutritionCSVMapping : ClassMap<SamsungFoodNutritionModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SamsungFoodNutritionCSVMapping"/> class and configures column mappings.
    /// </summary>
    public SamsungFoodNutritionCSVMapping()
    {
        Map(m => m.Magnesium).Name("magnesium");
        Map(m => m.Potassium).Name("potassium");
        Map(m => m.CreateShVer).Name("create_sh_ver");

        Map(m => m.VitaminA).Name("vitamin_a");
        Map(m => m.VitaminC).Name("vitamin_c");
        Map(m => m.VitaminD).Name("vitamin_d");
        Map(m => m.VitaminE).Name("vitamin_e");
        Map(m => m.VitaminK).Name("vitamin_k");

        Map(m => m.Cholesterol).Name("cholesterol");
        Map(m => m.VitaminB12).Name("vitamin_b12");

        Map(m => m.StartTime).Name("start_time");

        Map(m => m.Biotin).Name("biotin");
        Map(m => m.Phosphorus).Name("phosphorus");
        Map(m => m.Copper).Name("copper");

        Map(m => m.Custom).Name("custom");

        Map(m => m.Thiamin).Name("thiamin");
        Map(m => m.Folate).Name("folate");
        Map(m => m.Iodine).Name("iodine");
        Map(m => m.Niacin).Name("niacin");
        Map(m => m.Sodium).Name("sodium");
        Map(m => m.Molybdenum).Name("molybdenum");
        Map(m => m.FolicAcid).Name("folic_acid");

        Map(m => m.ModifyShVer).Name("modify_sh_ver");

        Map(m => m.DietaryFiber).Name("dietary_fiber");
        Map(m => m.TotalFat).Name("total_fat");

        Map(m => m.UpdateTime).Name("update_time");
        Map(m => m.CreateTime).Name("create_time");

        Map(m => m.MealType).Name("meal_type");

        Map(m => m.Caffeine).Name("caffeine");
        Map(m => m.MonosaturatedFat).Name("monosaturated_fat");
        Map(m => m.Protein).Name("protein");
        Map(m => m.Manganese).Name("manganese");
        Map(m => m.PolysaturatedFat).Name("polysaturated_fat");
        Map(m => m.Iron).Name("iron");
        Map(m => m.Zinc).Name("zinc");
        Map(m => m.Riboflavin).Name("riboflavin");
        Map(m => m.Sugar).Name("sugar");

        Map(m => m.Title).Name("title");

        Map(m => m.AddedSugar).Name("added_sugar");
        Map(m => m.CaloriesFromFat).Name("calories_from_fat");
        Map(m => m.Calcium).Name("calcium");
        Map(m => m.Calorie).Name("calorie");

        Map(m => m.TimeOffset).Name("time_offset");

        Map(m => m.DeviceUuid).Name("deviceuuid");

        Map(m => m.PolyunsaturatedFat).Name("polyunsaturated_fat");
        Map(m => m.SaturatedFat).Name("saturated_fat");
        Map(m => m.TransFat).Name("trans_fat");

        Map(m => m.PkgName).Name("pkg_name");

        Map(m => m.Selenium).Name("selenium");
        Map(m => m.VitaminB6).Name("vitamin_b6");
        Map(m => m.Carbohydrate).Name("carbohydrate");
        Map(m => m.UnsaturatedFat).Name("unsaturated_fat");
        Map(m => m.Chloride).Name("chloride");
        Map(m => m.PantothenicAcid).Name("pantothenic_acid");

        Map(m => m.DataUuid).Name("datauuid");

        Map(m => m.Chromium).Name("chromium");
    }
}