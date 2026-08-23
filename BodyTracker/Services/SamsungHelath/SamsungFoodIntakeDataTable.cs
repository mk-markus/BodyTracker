using BodyTracker.Models;
using System;
using System.Collections.Generic;
using System.Data;

public static class SamsungFoodIntakeDataTable
{
    public static DataTable CreateFoodIntakeDataTable(int personId, IEnumerable<SamsungFoodIntakeModel> meals)
    {
        var table = new DataTable();

        table.Columns.Add("person_id", typeof(int));

        table.Columns.Add("create_sync_version", typeof(string));
        table.Columns.Add("start_time", typeof(DateTime));

        table.Columns.Add("amount_current", typeof(double));
        table.Columns.Add("amount_initial", typeof(double));

        table.Columns.Add("custom_text", typeof(string));
        table.Columns.Add("modify_sync_version", typeof(string));

        table.Columns.Add("update_at", typeof(DateTime));
        table.Columns.Add("create_at", typeof(DateTime));

        table.Columns.Add("meal_type", typeof(int));

        table.Columns.Add("client_data_id", typeof(string));
        table.Columns.Add("food_name", typeof(string));
        table.Columns.Add("unit_id", typeof(int));
        table.Columns.Add("client_data_version", typeof(string));

        table.Columns.Add("calories_current", typeof(double));
        table.Columns.Add("calories_initial", typeof(double));

        table.Columns.Add("time_offset", typeof(string));
        table.Columns.Add("device_uuid", typeof(string));

        table.Columns.Add("comment_text", typeof(string));
        table.Columns.Add("package_name", typeof(string));

        table.Columns.Add("data_uuid", typeof(string));
        table.Columns.Add("food_info_id", typeof(string));

        foreach (var meal in meals)
        {
            var row = table.NewRow();

            row["person_id"] = personId;

            row["create_sync_version"] = (object?)meal.CreateShVer ?? DBNull.Value;

            row["start_time"] = (object?)meal.StartTime ?? DBNull.Value;

            row["amount_current"] = (object?)meal.Amount ?? DBNull.Value;

            row["amount_initial"] = (object?)meal.Amount ?? DBNull.Value;

            row["custom_text"] =  (object?)meal.Custom ?? DBNull.Value;

            row["modify_sync_version"] =  (object?)meal.ModifyShVer ?? DBNull.Value;

            row["update_at"] =(object?)meal.UpdateTime ?? DBNull.Value;

            row["create_at"] =  (object?)meal.CreateTime ?? DBNull.Value;

            row["meal_type"] = (object?)meal.MealType ?? DBNull.Value;

            row["client_data_id"] = (object?)meal.ClientDataId ?? DBNull.Value;

            row["food_name"] = (object?)meal.Name ?? DBNull.Value;

            row["unit_id"] = (object?)meal.Unit ?? DBNull.Value;

            row["client_data_version"] = (object?)meal.ClientDataVer ?? DBNull.Value;

            row["calories_current"] = (object?)meal.Calorie ?? DBNull.Value;

            row["calories_initial"] = (object?)meal.Calorie ?? DBNull.Value;

            row["time_offset"] = (object?)meal.TimeOffset ?? DBNull.Value;

            row["device_uuid"] =(object?)meal.DeviceUuid ?? DBNull.Value;

            row["comment_text"] =  (object?)meal.Comment ?? DBNull.Value;

            row["package_name"] = (object?)meal.PkgName ?? DBNull.Value;

            row["data_uuid"] =(object?)meal.DataUuid ?? DBNull.Value;

            row["food_info_id"] = (object?)meal.FoodInfoId ?? DBNull.Value;

            table.Rows.Add(row);
        }

        return table;
    }
}