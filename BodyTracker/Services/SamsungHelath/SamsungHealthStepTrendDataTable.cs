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

            table.Columns.Add("PersonID_FK", typeof(int));

            table.Columns.Add("BinningData", typeof(string));

            table.Columns.Add("UpdateTime", typeof(DateTime));
            table.Columns.Add("CreateTime", typeof(DateTime));

            table.Columns.Add("SourcePkgName", typeof(string));
            table.Columns.Add("SourceType", typeof(int));

            table.Columns.Add("StepCount", typeof(double));

            table.Columns.Add("Speed", typeof(double));

            table.Columns.Add("Distance", typeof(double));

            table.Columns.Add("Calorie", typeof(double));

            table.Columns.Add("DeviceUuid", typeof(string));

            table.Columns.Add("PkgName", typeof(string));

            table.Columns.Add("DataUuid", typeof(string));

            foreach (var item in stepTrends)
            {
                var row = table.NewRow();

                row["PersonID_FK"] =
                    personId;

                row["BinningData"] =
                    (object?)item.BinningData ?? DBNull.Value;

                row["UpdateTime"] =
                    (object?)item.UpdateTime ?? DBNull.Value;

                row["CreateTime"] =
                    (object?)item.CreateTime ?? DBNull.Value;

                row["SourcePkgName"] =
                    (object?)item.SourcePkgName ?? DBNull.Value;

                row["SourceType"] =
                    (object?)item.SourceType ?? DBNull.Value;

                row["StepCount"] =
                    (object?)item.Count ?? DBNull.Value;

                row["Speed"] =
                    (object?)item.Speed ?? DBNull.Value;

                row["Distance"] =
                    (object?)item.Distance ?? DBNull.Value;

                row["Calorie"] =
                    (object?)item.Calorie ?? DBNull.Value;

                row["DeviceUuid"] =
                    (object?)item.DeviceUuid ?? DBNull.Value;

                row["PkgName"] =
                    (object?)item.PkgName ?? DBNull.Value;

                row["DataUuid"] =
                    (object?)item.DataUuid ?? DBNull.Value;

                table.Rows.Add(row);
            }

            return table;
        }
    }
}