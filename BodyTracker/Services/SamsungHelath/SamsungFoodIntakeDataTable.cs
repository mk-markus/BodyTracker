using BodyTracker.Models;
using System;
using System.Collections.Generic;
using System.Data;

public static class SamsungFoodIntakeDataTable
{
    public static DataTable CreateFoodIntakeDataTable(
        int personId,
        IEnumerable<SamsungFoodIntakeModel> meals)
    {
        var table = new DataTable();

        table.Columns.Add("PersonID_FK", typeof(int));

        table.Columns.Add("CreateShVer", typeof(string));
        table.Columns.Add("StartTime", typeof(DateTime));

        table.Columns.Add("Amount", typeof(double));

        table.Columns.Add("Custom", typeof(string));
        table.Columns.Add("ModifyShVer", typeof(string));

        table.Columns.Add("UpdateTime", typeof(DateTime));
        table.Columns.Add("CreateTime", typeof(DateTime));

        table.Columns.Add("MealType", typeof(int));

        table.Columns.Add("ClientDataId", typeof(string));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Unit", typeof(int));
        table.Columns.Add("ClientDataVer", typeof(string));

        table.Columns.Add("Calorie", typeof(double));

        table.Columns.Add("TimeOffset", typeof(string));
        table.Columns.Add("DeviceUuid", typeof(string));

        table.Columns.Add("Comment", typeof(string));
        table.Columns.Add("PkgName", typeof(string));

        table.Columns.Add("DataUuid", typeof(string));
        table.Columns.Add("FoodInfoId", typeof(string));

        foreach (var meal in meals)
        {
            var row = table.NewRow();

            row["PersonID_FK"] = personId;

            row["CreateShVer"] =
                (object?)meal.CreateShVer ?? DBNull.Value;

            row["StartTime"] =
                (object?)meal.StartTime ?? DBNull.Value;

            row["Amount"] =
                (object?)meal.Amount ?? DBNull.Value;

            row["Custom"] =
                (object?)meal.Custom ?? DBNull.Value;

            row["ModifyShVer"] =
                (object?)meal.ModifyShVer ?? DBNull.Value;

            row["UpdateTime"] =
                (object?)meal.UpdateTime ?? DBNull.Value;

            row["CreateTime"] =
                (object?)meal.CreateTime ?? DBNull.Value;

            row["MealType"] =
                (object?)meal.MealType ?? DBNull.Value;

            row["ClientDataId"] =
                (object?)meal.ClientDataId ?? DBNull.Value;

            row["Name"] =
                (object?)meal.Name ?? DBNull.Value;

            row["Unit"] =
                (object?)meal.Unit ?? DBNull.Value;

            row["ClientDataVer"] =
                (object?)meal.ClientDataVer ?? DBNull.Value;

            row["Calorie"] =
                (object?)meal.Calorie ?? DBNull.Value;

            row["TimeOffset"] =
                (object?)meal.TimeOffset ?? DBNull.Value;

            row["DeviceUuid"] =
                (object?)meal.DeviceUuid ?? DBNull.Value;

            row["Comment"] =
                (object?)meal.Comment ?? DBNull.Value;

            row["PkgName"] =
                (object?)meal.PkgName ?? DBNull.Value;

            row["DataUuid"] =
                (object?)meal.DataUuid ?? DBNull.Value;

            row["FoodInfoId"] =
                (object?)meal.FoodInfoId ?? DBNull.Value;

            table.Rows.Add(row);
        }

        return table;
    }
}