using BodyTracker.Models.SamsungHealth;
using System;
using System.Collections.Generic;
using System.Data;

namespace BodyTracker.Services.SamsungHelath
{
    public static class SamsungHealthOxygenDataTable
    {
        public static DataTable CreateOxygenDataTable(int personId, IEnumerable<SamsungOxygenSaturationModel> oxygenSaturations)
        {
            var table = new DataTable();

            table.Columns.Add("person_id", typeof(int));

            table.Columns.Add("integrated_id", typeof(string));
            table.Columns.Add("client_data_id", typeof(string));
            table.Columns.Add("tag_id", typeof(int));

            table.Columns.Add("start_time", typeof(DateTime));
            table.Columns.Add("end_time", typeof(DateTime));
            table.Columns.Add("update_time", typeof(DateTime));
            table.Columns.Add("create_time", typeof(DateTime));

            table.Columns.Add("time_offset", typeof(string));

            table.Columns.Add("custom", typeof(string));

            table.Columns.Add("spo2", typeof(double));
            table.Columns.Add("spo2_max", typeof(double));
            table.Columns.Add("spo2_min", typeof(double));

            table.Columns.Add("low_spo2duration", typeof(int));
            table.Columns.Add("coverage_rate", typeof(int));

            table.Columns.Add("heart_rate", typeof(double));

            table.Columns.Add("comment", typeof(string));

            table.Columns.Add("data_uuid", typeof(string));
            table.Columns.Add("device_uuid", typeof(string));

            table.Columns.Add("package_name", typeof(string));

            table.Columns.Add("createShVer", typeof(int));
            table.Columns.Add("modify_shver", typeof(int));
            table.Columns.Add("client_dataver", typeof(int));

            table.Columns.Add("source", typeof(string));
            table.Columns.Add("binning_data", typeof(string));

            foreach (var oxygen in oxygenSaturations)
            {
                var row = table.NewRow();

                row["person_id"] = personId;

                row["integrated_id"] = (object?)oxygen.IntegratedId ?? DBNull.Value;
                row["client_data_id"] = (object?)oxygen.ClientDataId ?? DBNull.Value;
                row["tag_id"] = (object?)oxygen.TagId ?? DBNull.Value;

                row["start_time"] = (object?)oxygen.StartTime ?? DBNull.Value;
                row["end_time"] = (object?)oxygen.EndTime ?? DBNull.Value;
                row["update_time"] = (object?)oxygen.UpdateTime ?? DBNull.Value;
                row["create_time"] = (object?)oxygen.CreateTime ?? DBNull.Value;

                row["time_offset"] = (object?)oxygen.TimeOffset ?? DBNull.Value;

                row["custom"] = (object?)oxygen.Custom ?? DBNull.Value;

                row["spo2"] = (object?)oxygen.SpO2 ?? DBNull.Value;
                row["spo2_max"] = (object?)oxygen.SpO2Max ?? DBNull.Value;
                row["spo2_min"] = (object?)oxygen.SpO2Min ?? DBNull.Value;

                row["low_spo2duration"] = (object?)oxygen.LowSpO2Duration ?? DBNull.Value;
                row["coverage_rate"] = (object?)oxygen.SpO2CoverageRate ?? DBNull.Value;

                row["heart_rate"] = (object?)oxygen.HeartRate ?? DBNull.Value;

                row["comment"] = (object?)oxygen.Comment ?? DBNull.Value;

                row["data_uuid"] = (object?)oxygen.DataUuid ?? DBNull.Value;
                row["device_uuid"] = (object?)oxygen.DeviceUuid ?? DBNull.Value;

                row["package_name"] = (object?)oxygen.PackageName ?? DBNull.Value;

                row["createShVer"] = (object?)oxygen.CreateShVer ?? DBNull.Value;
                row["modify_shver"] = (object?)oxygen.ModifyShVer ?? DBNull.Value;
                row["client_dataver"] = (object?)oxygen.ClientDataVer ?? DBNull.Value;

                row["source"] = (object?)oxygen.Source ?? DBNull.Value;
                row["binning_data"] = (object?)oxygen.Binning ?? DBNull.Value;

                table.Rows.Add(row);
            }

            return table;
        }
    }
}