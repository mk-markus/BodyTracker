using BodyTracker.Models.SamsungHealth;
using System;
using System.Collections.Generic;
using System.Data;

namespace BodyTracker.Services.SamsungHelath
{
    public static class SamsungHealthOxygenDataTable
    {
        public static DataTable CreateOxygenDataTable(
            int personId,
            IEnumerable<SamsungOxygenSaturationModel> oxygenSaturations)
        {
            var table = new DataTable();

            table.Columns.Add("PersonId", typeof(int));

            table.Columns.Add("IntegratedId", typeof(string));
            table.Columns.Add("ClientDataId", typeof(string));
            table.Columns.Add("TagId", typeof(int));

            table.Columns.Add("StartTime", typeof(DateTime));
            table.Columns.Add("EndTime", typeof(DateTime));
            table.Columns.Add("UpdateTime", typeof(DateTime));
            table.Columns.Add("CreateTime", typeof(DateTime));

            table.Columns.Add("TimeOffset", typeof(string));

            table.Columns.Add("Custom", typeof(string));

            table.Columns.Add("Spo2", typeof(double));
            table.Columns.Add("Spo2Max", typeof(double));
            table.Columns.Add("Spo2Min", typeof(double));

            table.Columns.Add("LowSpo2Duration", typeof(int));
            table.Columns.Add("CoverageRate", typeof(int));

            table.Columns.Add("HeartRate", typeof(double));

            table.Columns.Add("Comment", typeof(string));

            table.Columns.Add("DataUuid", typeof(string));
            table.Columns.Add("DeviceUuid", typeof(string));

            table.Columns.Add("PackageName", typeof(string));

            table.Columns.Add("CreateShVer", typeof(int));
            table.Columns.Add("ModifyShVer", typeof(int));
            table.Columns.Add("ClientDataVer", typeof(int));

            table.Columns.Add("Source", typeof(string));
            table.Columns.Add("BinningData", typeof(string));

            foreach (var oxygen in oxygenSaturations)
            {
                var row = table.NewRow();

                row["PersonId"] =
                    personId;

                row["IntegratedId"] =
                    (object?)oxygen.IntegratedId ?? DBNull.Value;

                row["ClientDataId"] =
                    (object?)oxygen.ClientDataId ?? DBNull.Value;

                row["TagId"] =
                    (object?)oxygen.TagId ?? DBNull.Value;

                row["StartTime"] =
                    (object?)oxygen.StartTime ?? DBNull.Value;

                row["EndTime"] =
                    (object?)oxygen.EndTime ?? DBNull.Value;

                row["UpdateTime"] =
                    (object?)oxygen.UpdateTime ?? DBNull.Value;

                row["CreateTime"] =
                    (object?)oxygen.CreateTime ?? DBNull.Value;

                row["TimeOffset"] =
                    (object?)oxygen.TimeOffset ?? DBNull.Value;

                row["Custom"] =
                    (object?)oxygen.Custom ?? DBNull.Value;

                row["Spo2"] =
                    (object?)oxygen.SpO2 ?? DBNull.Value;

                row["Spo2Max"] =
                    (object?)oxygen.SpO2Max ?? DBNull.Value;

                row["Spo2Min"] =
                    (object?)oxygen.SpO2Min ?? DBNull.Value;

                row["LowSpo2Duration"] =
                    (object?)oxygen.LowSpO2Duration ?? DBNull.Value;

                row["CoverageRate"] =
                    (object?)oxygen.SpO2CoverageRate ?? DBNull.Value;

                row["HeartRate"] =
                    (object?)oxygen.HeartRate ?? DBNull.Value;

                row["Comment"] =
                    (object?)oxygen.Comment ?? DBNull.Value;

                row["DataUuid"] =
                    (object?)oxygen.DataUuid ?? DBNull.Value;

                row["DeviceUuid"] =
                    (object?)oxygen.DeviceUuid ?? DBNull.Value;

                row["PackageName"] =
                    (object?)oxygen.PackageName ?? DBNull.Value;

                row["CreateShVer"] =
                    (object?)oxygen.CreateShVer ?? DBNull.Value;

                row["ModifyShVer"] =
                    (object?)oxygen.ModifyShVer ?? DBNull.Value;

                row["ClientDataVer"] =
                    (object?)oxygen.ClientDataVer ?? DBNull.Value;

                row["Source"] =
                    (object?)oxygen.Source ?? DBNull.Value;

                row["BinningData"] =
                    (object?)oxygen.Binning ?? DBNull.Value;

                table.Rows.Add(row);
            }

            return table;
        }
    }
}