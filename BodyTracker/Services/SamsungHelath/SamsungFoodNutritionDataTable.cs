using System;
using System.Collections.Generic;
using System.Data;

public static class SamsungFoodNutritionDataTable
{
    public static DataTable CreateFoodNutritionDataTable(
        int personId,
        IEnumerable<SamsungFoodNutritionModel> nutritionList)
    {
        var table = new DataTable();

        table.Columns.Add("person_id", typeof(int));

        table.Columns.Add("magnesium", typeof(double));
        table.Columns.Add("potassium", typeof(double));
        table.Columns.Add("create_sh_ver", typeof(string));

        table.Columns.Add("vitamin_a", typeof(double));
        table.Columns.Add("vitamin_c", typeof(double));
        table.Columns.Add("vitamin_d", typeof(double));
        table.Columns.Add("vitamin_e", typeof(double));
        table.Columns.Add("vitamin_k", typeof(double));
        table.Columns.Add("vitamin_b12", typeof(double));

        table.Columns.Add("cholesterol", typeof(double));

        table.Columns.Add("start_time", typeof(DateTime));

        table.Columns.Add("biotin", typeof(double));
        table.Columns.Add("phosphorus", typeof(double));
        table.Columns.Add("copper", typeof(double));

        table.Columns.Add("custom", typeof(string));

        table.Columns.Add("thiamin", typeof(double));
        table.Columns.Add("folate", typeof(double));
        table.Columns.Add("iodine", typeof(double));
        table.Columns.Add("niacin", typeof(double));
        table.Columns.Add("sodium", typeof(double));
        table.Columns.Add("molybdenum", typeof(double));
        table.Columns.Add("folic_acid", typeof(double));

        table.Columns.Add("modify_sh_ver", typeof(string));

        table.Columns.Add("dietary_fiber", typeof(double));
        table.Columns.Add("total_fat", typeof(double));

        table.Columns.Add("update_time", typeof(DateTime));
        table.Columns.Add("create_time", typeof(DateTime));

        table.Columns.Add("meal_type", typeof(int));

        table.Columns.Add("caffeine", typeof(double));
        table.Columns.Add("monosaturated_fat", typeof(double));
        table.Columns.Add("protein", typeof(double));
        table.Columns.Add("manganese", typeof(double));
        table.Columns.Add("polysaturated_fat", typeof(double));
        table.Columns.Add("iron", typeof(double));
        table.Columns.Add("zinc", typeof(double));
        table.Columns.Add("riboflavin", typeof(double));

        table.Columns.Add("sugar", typeof(double));
        table.Columns.Add("title", typeof(string));

        table.Columns.Add("added_sugar", typeof(double));
        table.Columns.Add("calories_from_fat", typeof(double));
        table.Columns.Add("calcium", typeof(double));
        table.Columns.Add("calorie", typeof(double));

        table.Columns.Add("time_offset", typeof(string));

        table.Columns.Add("deviceuuid", typeof(string));

        table.Columns.Add("polyunsaturated_fat", typeof(double));
        table.Columns.Add("saturated_fat", typeof(double));
        table.Columns.Add("trans_fat", typeof(double));

        table.Columns.Add("pkg_name", typeof(string));

        table.Columns.Add("selenium", typeof(double));
        table.Columns.Add("vitamin_b6", typeof(double));
        table.Columns.Add("carbohydrate", typeof(double));
        table.Columns.Add("unsaturated_fat", typeof(double));
        table.Columns.Add("chloride", typeof(double));
        table.Columns.Add("pantothenic_acid", typeof(double));

        table.Columns.Add("datauuid", typeof(string));

        table.Columns.Add("chromium", typeof(double));

        foreach (var item in nutritionList)
        {
            table.Rows.Add(
                personId,

                item.Magnesium ?? (object)DBNull.Value,
                item.Potassium ?? (object)DBNull.Value,
                item.CreateShVer ?? (object)DBNull.Value,

                item.VitaminA ?? (object)DBNull.Value,
                item.VitaminC ?? (object)DBNull.Value,
                item.VitaminD ?? (object)DBNull.Value,
                item.VitaminE ?? (object)DBNull.Value,
                item.VitaminK ?? (object)DBNull.Value,
                item.VitaminB12 ?? (object)DBNull.Value,

                item.Cholesterol ?? (object)DBNull.Value,

                item.StartTime,

                item.Biotin ?? (object)DBNull.Value,
                item.Phosphorus ?? (object)DBNull.Value,
                item.Copper ?? (object)DBNull.Value,

                item.Custom ?? (object)DBNull.Value,

                item.Thiamin ?? (object)DBNull.Value,
                item.Folate ?? (object)DBNull.Value,
                item.Iodine ?? (object)DBNull.Value,
                item.Niacin ?? (object)DBNull.Value,
                item.Sodium ?? (object)DBNull.Value,
                item.Molybdenum ?? (object)DBNull.Value,
                item.FolicAcid ?? (object)DBNull.Value,

                item.ModifyShVer ?? (object)DBNull.Value,

                item.DietaryFiber ?? (object)DBNull.Value,
                item.TotalFat ?? (object)DBNull.Value,

                item.UpdateTime,
                item.CreateTime,

                item.MealType ?? (object)DBNull.Value,

                item.Caffeine ?? (object)DBNull.Value,
                item.MonosaturatedFat ?? (object)DBNull.Value,
                item.Protein ?? (object)DBNull.Value,
                item.Manganese ?? (object)DBNull.Value,
                item.PolysaturatedFat ?? (object)DBNull.Value,
                item.Iron ?? (object)DBNull.Value,
                item.Zinc ?? (object)DBNull.Value,
                item.Riboflavin ?? (object)DBNull.Value,

                item.Sugar ?? (object)DBNull.Value,
                item.Title ?? (object)DBNull.Value,

                item.AddedSugar ?? (object)DBNull.Value,
                item.CaloriesFromFat ?? (object)DBNull.Value,
                item.Calcium ?? (object)DBNull.Value,
                item.Calorie ?? (object)DBNull.Value,

                item.TimeOffset ?? (object)DBNull.Value,

                item.DeviceUuid ?? (object)DBNull.Value,

                item.PolyunsaturatedFat ?? (object)DBNull.Value,
                item.SaturatedFat ?? (object)DBNull.Value,
                item.TransFat ?? (object)DBNull.Value,

                item.PkgName ?? (object)DBNull.Value,

                item.Selenium ?? (object)DBNull.Value,
                item.VitaminB6 ?? (object)DBNull.Value,
                item.Carbohydrate ?? (object)DBNull.Value,
                item.UnsaturatedFat ?? (object)DBNull.Value,
                item.Chloride ?? (object)DBNull.Value,
                item.PantothenicAcid ?? (object)DBNull.Value,

                item.DataUuid ?? (object)DBNull.Value,

                item.Chromium ?? (object)DBNull.Value
            );
        }

        return table;
    }
}