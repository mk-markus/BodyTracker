using System;
using System.Collections.Generic;
using System.Data;
using BodyTracker.Models;

namespace BodyTracker.Services.SamsungHealth
{
    /// <summary>
    /// Provides utility methods to generate a <see cref="DataTable"/> for Samsung Health food information records.
    /// </summary>
    public static class SamsungFoodInfoDataTable
    {
        /// <summary>
        /// Creates and populates a <see cref="DataTable"/> containing food information records for a specific person.
        /// </summary>
        /// <param name="personId">The unique identifier of the person.</param>
        /// <param name="foodInfos">The collection of Samsung food info records.</param>
        /// <returns>A populated <see cref="DataTable"/> ready for database operations or bulk inserts.</returns>
        public static DataTable CreateFoodInfoDataTable(int personId, IEnumerable<SamsungFoodInfoModel> foodInfos)
        {
            var table = new DataTable();

            // Exakte Reihenfolge & Namen wie in tbl_FoodInfo
            table.Columns.Add("person_id", typeof(int));
            table.Columns.Add("potassium", typeof(double));
            table.Columns.Add("vitamin_a", typeof(double));
            table.Columns.Add("vitamin_c", typeof(double));
            table.Columns.Add("vitamin_d", typeof(double));
            table.Columns.Add("cholesterol", typeof(double));
            table.Columns.Add("description", typeof(string));
            table.Columns.Add("custom", typeof(string));
            table.Columns.Add("provider_food_id", typeof(string));
            table.Columns.Add("metric_serving_amount", typeof(double));
            table.Columns.Add("metric_serving_unit", typeof(string)); // Korrigiert (war vorher falsch eingeordnet)
            table.Columns.Add("sodium", typeof(double));
            table.Columns.Add("dietary_fiber", typeof(double));
            table.Columns.Add("total_fat", typeof(double));
            table.Columns.Add("monosaturated_fat", typeof(double));
            table.Columns.Add("polysaturated_fat", typeof(double));
            table.Columns.Add("saturated_fat", typeof(double));
            table.Columns.Add("trans_fat", typeof(double));
            table.Columns.Add("protein", typeof(double));
            table.Columns.Add("iron", typeof(double));
            table.Columns.Add("sugar", typeof(double));
            table.Columns.Add("added_sugar", typeof(double));
            table.Columns.Add("calcium", typeof(double));
            table.Columns.Add("calorie", typeof(double));
            table.Columns.Add("serving_description", typeof(string));
            table.Columns.Add("info_provider", typeof(string));
            table.Columns.Add("name", typeof(string));
            table.Columns.Add("carbohydrate", typeof(double));
            table.Columns.Add("unit_count_per_calorie", typeof(double));
            table.Columns.Add("default_number_of_serving_unit", typeof(double));
            table.Columns.Add("device_uuid", typeof(string)); // Korrigiert von deviceuuid
            table.Columns.Add("pkg_name", typeof(string));
            table.Columns.Add("data_uuid", typeof(string));     // Korrigiert von datauuid
            table.Columns.Add("update_at", typeof(DateTime));   // Korrigiert von update_time
            table.Columns.Add("create_at", typeof(DateTime));   // Korrigiert von create_time

            foreach (var food in foodInfos)
            {
                var row = table.NewRow();

                row["person_id"] = personId;
                row["potassium"] = (object?)food.Potassium ?? DBNull.Value;
                row["vitamin_a"] = (object?)food.VitaminA ?? DBNull.Value;
                row["vitamin_c"] = (object?)food.VitaminC ?? DBNull.Value;
                row["vitamin_d"] = (object?)food.VitaminD ?? DBNull.Value;
                row["cholesterol"] = (object?)food.Cholesterol ?? DBNull.Value;
                row["description"] = (object?)food.Description ?? DBNull.Value;
                row["custom"] = (object?)food.Custom ?? DBNull.Value;
                row["provider_food_id"] = (object?)food.ProviderFoodId ?? DBNull.Value;
                row["metric_serving_amount"] = (object?)food.MetricServingAmount ?? DBNull.Value;
                row["metric_serving_unit"] = (object?)food.MetricServingUnit ?? DBNull.Value;
                row["sodium"] = (object?)food.Sodium ?? DBNull.Value;
                row["dietary_fiber"] = (object?)food.DietaryFiber ?? DBNull.Value;
                row["total_fat"] = (object?)food.TotalFat ?? DBNull.Value;
                row["monosaturated_fat"] = (object?)food.MonosaturatedFat ?? DBNull.Value;
                row["polysaturated_fat"] = (object?)food.PolysaturatedFat ?? DBNull.Value;
                row["saturated_fat"] = (object?)food.SaturatedFat ?? DBNull.Value;
                row["trans_fat"] = (object?)food.TransFat ?? DBNull.Value;
                row["protein"] = (object?)food.Protein ?? DBNull.Value;
                row["iron"] = (object?)food.Iron ?? DBNull.Value;
                row["sugar"] = (object?)food.Sugar ?? DBNull.Value;
                row["added_sugar"] = (object?)food.AddedSugar ?? DBNull.Value;
                row["calcium"] = (object?)food.Calcium ?? DBNull.Value;
                row["calorie"] = (object?)food.Calorie ?? DBNull.Value;
                row["serving_description"] = (object?)food.ServingDescription ?? DBNull.Value;
                row["info_provider"] = (object?)food.InfoProvider ?? DBNull.Value;
                row["name"] = (object?)food.Name ?? DBNull.Value;
                row["carbohydrate"] = (object?)food.Carbohydrate ?? DBNull.Value;
                row["unit_count_per_calorie"] = (object?)food.UnitCountPerCalorie ?? DBNull.Value;
                row["default_number_of_serving_unit"] = (object?)food.DefaultNumberOfServingUnit ?? DBNull.Value;
                row["device_uuid"] = (object?)food.DeviceUuid ?? DBNull.Value;
                row["pkg_name"] = (object?)food.PkgName ?? DBNull.Value;
                row["data_uuid"] = (object?)food.DataUuid ?? DBNull.Value;
                row["update_at"] = (object?)food.UpdateTime ?? DBNull.Value;
                row["create_at"] = (object?)food.CreateTime ?? DBNull.Value;

                table.Rows.Add(row);
            }

            return table;
        }
    }
}