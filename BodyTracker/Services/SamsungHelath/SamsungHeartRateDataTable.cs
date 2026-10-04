using System;
using System.Collections.Generic;
using System.Data;

namespace BodyTracker.Services.SamsungHelath
{
    public static class SamsungHeartRateDataTable
    {
        public static DataTable CreateHeartRateDataTable(
            int personId,
            IEnumerable<SamsungHeartRateModel> heartRates)
        {
            var table = new DataTable();

            table.Columns.Add("PersonId", typeof(int));

            table.Columns.Add("Source", typeof(string));
            table.Columns.Add("TagId", typeof(int));
            table.Columns.Add("CreateShVer", typeof(int));

            table.Columns.Add("StartTime", typeof(DateTime));
            table.Columns.Add("EndTime", typeof(DateTime));
            table.Columns.Add("UpdateTime", typeof(DateTime));
            table.Columns.Add("CreateTime", typeof(DateTime));

            table.Columns.Add("TimeOffset", typeof(string));

            table.Columns.Add("Custom", typeof(string));
            table.Columns.Add("BinningData", typeof(string));

            table.Columns.Add("ModifyShVer", typeof(int));

            table.Columns.Add("ClientDataId", typeof(string));

            table.Columns.Add("HeartRate", typeof(double));
            table.Columns.Add("HeartRateMax", typeof(double));
            table.Columns.Add("HeartRateMin", typeof(double));

            table.Columns.Add("HeartBeatCount", typeof(int));

            table.Columns.Add("ClientDataVer", typeof(string));

            table.Columns.Add("Comment", typeof(string));

            table.Columns.Add("PackageName", typeof(string));

            table.Columns.Add("DeviceUuid", typeof(string));

            table.Columns.Add("DataUuid", typeof(string));

            foreach (var hr in heartRates)
            {
                var row = table.NewRow();

                row["PersonId"] =
                    personId;

                row["Source"] =
                    (object?)hr.Source ?? DBNull.Value;

                row["TagId"] =
                    (object?)hr.TagId ?? DBNull.Value;

                row["CreateShVer"] =
                    (object?)hr.CreateShVer ?? DBNull.Value;

                row["StartTime"] =
                    (object?)hr.StartTime ?? DBNull.Value;

                row["EndTime"] =
                    (object?)hr.EndTime ?? DBNull.Value;

                row["UpdateTime"] =
                    (object?)hr.UpdateTime ?? DBNull.Value;

                row["CreateTime"] =
                    (object?)hr.CreateTime ?? DBNull.Value;

                row["TimeOffset"] =
                    (object?)hr.TimeOffset ?? DBNull.Value;

                row["Custom"] =
                    (object?)hr.Custom ?? DBNull.Value;

                row["BinningData"] =
                    (object?)hr.BinningData ?? DBNull.Value;

                row["ModifyShVer"] =
                    (object?)hr.ModifyShVer ?? DBNull.Value;

                row["ClientDataId"] =
                    (object?)hr.ClientDataId ?? DBNull.Value;

                row["HeartRate"] =
                    (object?)hr.HeartRate ?? DBNull.Value;

                row["HeartRateMax"] =
                    (object?)hr.HeartRateMax ?? DBNull.Value;

                row["HeartRateMin"] =
                    (object?)hr.HeartRateMin ?? DBNull.Value;

                row["HeartBeatCount"] =
                    (object?)hr.HeartBeatCount ?? DBNull.Value;

                row["ClientDataVer"] =
                    (object?)hr.ClientDataVer ?? DBNull.Value;

                row["Comment"] =
                    (object?)hr.Comment ?? DBNull.Value;

                row["PackageName"] =
                    (object?)hr.PackageName ?? DBNull.Value;

                row["DeviceUuid"] =
                    (object?)hr.DeviceUuid ?? DBNull.Value;

                row["DataUuid"] =
                    (object?)hr.DataUuid ?? DBNull.Value;

                table.Rows.Add(row);
            }

            return table;
        }
    }
}