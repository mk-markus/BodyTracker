using BodyTracker.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace BodyTracker.Services.SamsungHealth
{
    public static class SamsungHealthStepDailyTrendDataTable
    {
        public static DataTable CreateStepDailyTrendDataTable(
            int personId,
            IEnumerable<SamsungStepTrendModel> stepTrends)
        {
            var table = new DataTable();

            table.Columns.Add("person_id", typeof(int));
            table.Columns.Add("binning_data", typeof(string));
            table.Columns.Add("update_at", typeof(DateTime));
            table.Columns.Add("create_at", typeof(DateTime));
            table.Columns.Add("source_package", typeof(string));
            table.Columns.Add("source_type", typeof(int));

            table.Columns.Add("step_count_current", typeof(double));
            table.Columns.Add("step_count_initial", typeof(double));

            table.Columns.Add("speed_current", typeof(double));
            table.Columns.Add("speed_initial", typeof(double));

            table.Columns.Add("distance_current", typeof(double));
            table.Columns.Add("distance_initial", typeof(double));

            table.Columns.Add("calories_current", typeof(double));
            table.Columns.Add("calories_initial", typeof(double));

            table.Columns.Add("device_uuid", typeof(string));
            table.Columns.Add("package_name", typeof(string));
            table.Columns.Add("data_uuid", typeof(string));
            table.Columns.Add("record_date", typeof(DateTime));

            foreach (var item in stepTrends)
            {
                var row = table.NewRow();

                row["person_id"] = personId;
                row["binning_data"] = (object?)item.BinningData ?? DBNull.Value;
                row["update_at"] = (object?)item.UpdateTime ?? DBNull.Value;
                row["create_at"] = (object?)item.CreateTime ?? DBNull.Value;
                row["source_package"] = (object?)item.SourcePkgName ?? DBNull.Value;
                row["source_type"] = (object?)item.SourceType ?? DBNull.Value;

                row["step_count_current"] = (object?)item.Count ?? DBNull.Value;
                row["step_count_initial"] = (object?)item.Count ?? DBNull.Value;

                row["speed_current"] = (object?)item.Speed ?? DBNull.Value;
                row["speed_initial"] = (object?)item.Speed ?? DBNull.Value;

                row["distance_current"] = (object?)item.Distance ?? DBNull.Value;
                row["distance_initial"] = (object?)item.Distance ?? DBNull.Value;

                row["calories_current"] = (object?)item.Calorie ?? DBNull.Value;
                row["calories_initial"] = (object?)item.Calorie ?? DBNull.Value;

                row["device_uuid"] = (object?)item.DeviceUuid ?? DBNull.Value;
                row["package_name"] = (object?)item.PkgName ?? DBNull.Value;
                row["data_uuid"] = (object?)item.DataUuid ?? DBNull.Value;

                row["record_date"] = DateTime.Now;

                table.Rows.Add(row);
            }

            return table;
        }
    }
}