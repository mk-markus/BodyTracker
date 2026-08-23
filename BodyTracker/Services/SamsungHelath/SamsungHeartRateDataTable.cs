using System;
using System.Collections.Generic;
using System.Data;

namespace BodyTracker.Services.SamsungHelath
{
    public static class SamsungHeartRateDataTable
    {

        public static DataTable CreateHeartRateDataTable(int personId, IEnumerable<SamsungHeartRateModel> heartRates)
        {
            var table = new DataTable();

            table.Columns.Add("person_id", typeof(int));

            table.Columns.Add("source", typeof(string));
            table.Columns.Add("tag_id", typeof(int));
            table.Columns.Add("createShVer", typeof(int));

            table.Columns.Add("start_time", typeof(DateTime));
            table.Columns.Add("end_time", typeof(DateTime));
            table.Columns.Add("update_time", typeof(DateTime));
            table.Columns.Add("create_time", typeof(DateTime));
            table.Columns.Add("time_offset", typeof(string));

            table.Columns.Add("custom", typeof(string));
            table.Columns.Add("binning_data", typeof(string));
            table.Columns.Add("modify_shver", typeof(int));
            table.Columns.Add("client_data_id", typeof(string));

            table.Columns.Add("heart_rate", typeof(double));
            table.Columns.Add("heart_rate_max", typeof(double));
            table.Columns.Add("heart_rate_min", typeof(double));
            table.Columns.Add("heart_beat_count", typeof(int));

            table.Columns.Add("client_dataver", typeof(string));

            table.Columns.Add("comment", typeof(string));
            table.Columns.Add("package_name", typeof(string));

            table.Columns.Add("device_uuid", typeof(string));
            table.Columns.Add("data_uuid", typeof(string));

            foreach (var hr in heartRates)
            {
                var row = table.NewRow();

                row["person_id"] = personId;

                row["source"] = (object?)hr.Source ?? DBNull.Value;
                row["tag_id"] = (object?)hr.TagId ?? DBNull.Value;
                row["createShVer"] = (object?)hr.CreateShVer ?? DBNull.Value;

                row["start_time"] = (object?)hr.StartTime ?? DBNull.Value;
                row["end_time"] = (object?)hr.EndTime ?? DBNull.Value;
                row["update_time"] = (object?)hr.UpdateTime ?? DBNull.Value;
                row["create_time"] = (object?)hr.CreateTime ?? DBNull.Value;
                row["time_offset"] = (object?)hr.TimeOffset ?? DBNull.Value;

                row["custom"] = (object?)hr.Custom ?? DBNull.Value;
                row["binning_data"] = (object?)hr.BinningData ?? DBNull.Value;
                row["modify_shver"] = (object?)hr.ModifyShVer ?? DBNull.Value;
                row["client_data_id"] = (object?)hr.ClientDataId ?? DBNull.Value;

                row["heart_rate"] = (object?)hr.HeartRate ?? DBNull.Value;
                row["heart_rate_max"] = (object?)hr.HeartRateMax ?? DBNull.Value;
                row["heart_rate_min"] = (object?)hr.HeartRateMin ?? DBNull.Value;
                row["heart_beat_count"] = (object?)hr.HeartBeatCount ?? DBNull.Value;

                row["client_dataver"] = (object?)hr.ClientDataVer ?? DBNull.Value;

                row["comment"] = (object?)hr.Comment ?? DBNull.Value;
                row["package_name"] = (object?)hr.PackageName ?? DBNull.Value;

                row["device_uuid"] = (object?)hr.DeviceUuid ?? DBNull.Value;
                row["data_uuid"] = (object?)hr.DataUuid ?? DBNull.Value;

                table.Rows.Add(row);

            }

            return table;
        }
    }
}
