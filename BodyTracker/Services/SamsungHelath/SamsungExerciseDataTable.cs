using System;
using System.Collections.Generic;
using System.Data;

namespace BodyTracker.Services.SamsungHealth
{
    public static class SamsungExerciseDataTable
    {
        public static DataTable CreateExerciseDataTable(
    int personId,
    IEnumerable<SamsungExerciseModel> exercises)
        {
            var table = new DataTable();

            table.Columns.Add("person_id", typeof(int));

            table.Columns.Add("LiveDataInternal", typeof(string));
            table.Columns.Add("MissionValue", typeof(string));
            table.Columns.Add("RaceTarget", typeof(string));
            table.Columns.Add("SubsetData", typeof(string));
            table.Columns.Add("StartLongitude", typeof(double));
            table.Columns.Add("RoutineDataUuid", typeof(string));
            table.Columns.Add("TotalCalorie", typeof(double));
            table.Columns.Add("CompletionStatus", typeof(int));
            table.Columns.Add("PaceInfoId", typeof(long));
            table.Columns.Add("ActivityType", typeof(int));
            table.Columns.Add("PaceLiveData", typeof(string));
            table.Columns.Add("SensingStatus", typeof(string));
            table.Columns.Add("SourceType", typeof(int));
            table.Columns.Add("MissionType", typeof(int));
            table.Columns.Add("Ftp", typeof(double));
            table.Columns.Add("TrackingStatus", typeof(int));
            table.Columns.Add("ProgramId", typeof(long));
            table.Columns.Add("Title", typeof(string));
            table.Columns.Add("RewardStatus", typeof(int));
            table.Columns.Add("HeartRateSampleCount", typeof(int));
            table.Columns.Add("StartLatitude", typeof(double));
            table.Columns.Add("MissionExtraValue", typeof(string));
            table.Columns.Add("ProgramScheduleId", typeof(long));
            table.Columns.Add("HeartRateDeviceUuid", typeof(string));
            table.Columns.Add("LocationDataInternal", typeof(string));
            table.Columns.Add("CustomId", typeof(string));
            table.Columns.Add("AdditionalInternal", typeof(string));

            table.Columns.Add("Duration", typeof(long));
            table.Columns.Add("Additional", typeof(string));
            table.Columns.Add("CreateShVer", typeof(string));
            table.Columns.Add("MeanCaloricBurnRate", typeof(double));
            table.Columns.Add("LocationData", typeof(string));
            table.Columns.Add("StartTime", typeof(DateTime));
            table.Columns.Add("ExerciseType", typeof(int));
            table.Columns.Add("Custom", typeof(string));
            table.Columns.Add("MaxAltitude", typeof(double));
            table.Columns.Add("InclineDistance", typeof(double));
            table.Columns.Add("MeanHeartRate", typeof(double));
            table.Columns.Add("CountType", typeof(int));
            table.Columns.Add("MeanRpm", typeof(double));
            table.Columns.Add("MinAltitude", typeof(double));
            table.Columns.Add("ModifyShVer", typeof(string));
            table.Columns.Add("MaxHeartRate", typeof(double));

            table.Columns.Add("UpdateTime", typeof(DateTime));
            table.Columns.Add("CreateTime", typeof(DateTime));

            table.Columns.Add("ClientDataId", typeof(string));
            table.Columns.Add("MaxPower", typeof(double));
            table.Columns.Add("MaxSpeed", typeof(double));
            table.Columns.Add("MeanCadence", typeof(double));
            table.Columns.Add("MinHeartRate", typeof(double));
            table.Columns.Add("ClientDataVer", typeof(string));
            table.Columns.Add("StepCount", typeof(int));
            table.Columns.Add("Distance", typeof(double));
            table.Columns.Add("MaxCaloricBurnRate", typeof(double));
            table.Columns.Add("Calorie", typeof(double));
            table.Columns.Add("MaxCadence", typeof(double));
            table.Columns.Add("DeclineDistance", typeof(double));
            table.Columns.Add("Vo2Max", typeof(double));
            table.Columns.Add("TimeOffset", typeof(string));
            table.Columns.Add("DeviceUuid", typeof(string));
            table.Columns.Add("MaxRpm", typeof(double));
            table.Columns.Add("Comment", typeof(string));
            table.Columns.Add("LiveData", typeof(string));
            table.Columns.Add("MeanPower", typeof(double));
            table.Columns.Add("MeanSpeed", typeof(double));
            table.Columns.Add("PkgName", typeof(string));
            table.Columns.Add("AltitudeGain", typeof(double));
            table.Columns.Add("AltitudeLoss", typeof(double));
            table.Columns.Add("ExerciseCustomType", typeof(int));
            table.Columns.Add("AuxiliaryDevices", typeof(string));
            table.Columns.Add("EndTime", typeof(DateTime));
            table.Columns.Add("DataUuid", typeof(string));
            table.Columns.Add("SweatLoss", typeof(double));

            foreach (var ex in exercises)
            {
                var row = table.NewRow();

                row["person_id"] = personId;

                row["LiveDataInternal"] = (object?)ex.LiveDataInternal ?? DBNull.Value;
                row["MissionValue"] = (object?)ex.MissionValue ?? DBNull.Value;
                row["RaceTarget"] = (object?)ex.RaceTarget ?? DBNull.Value;
                row["SubsetData"] = (object?)ex.SubsetData ?? DBNull.Value;
                row["StartLongitude"] = (object?)ex.StartLongitude ?? DBNull.Value;
                row["RoutineDataUuid"] = (object?)ex.RoutineDataUuid ?? DBNull.Value;
                row["TotalCalorie"] = (object?)ex.TotalCalorie ?? DBNull.Value;
                row["CompletionStatus"] = (object?)ex.CompletionStatus ?? DBNull.Value;
                row["PaceInfoId"] = (object?)ex.PaceInfoId ?? DBNull.Value;
                row["ActivityType"] = (object?)ex.ActivityType ?? DBNull.Value;
                row["PaceLiveData"] = (object?)ex.PaceLiveData ?? DBNull.Value;
                row["SensingStatus"] = (object?)ex.SensingStatus ?? DBNull.Value;
                row["SourceType"] = (object?)ex.SourceType ?? DBNull.Value;
                row["MissionType"] = (object?)ex.MissionType ?? DBNull.Value;
                row["Ftp"] = (object?)ex.Ftp ?? DBNull.Value;
                row["TrackingStatus"] = (object?)ex.TrackingStatus ?? DBNull.Value;
                row["ProgramId"] = (object?)ex.ProgramId ?? DBNull.Value;
                row["Title"] = (object?)ex.Title ?? DBNull.Value;
                row["RewardStatus"] = (object?)ex.RewardStatus ?? DBNull.Value;
                row["HeartRateSampleCount"] = (object?)ex.HeartRateSampleCount ?? DBNull.Value;
                row["StartLatitude"] = (object?)ex.StartLatitude ?? DBNull.Value;
                row["MissionExtraValue"] = (object?)ex.MissionExtraValue ?? DBNull.Value;
                row["ProgramScheduleId"] = (object?)ex.ProgramScheduleId ?? DBNull.Value;
                row["HeartRateDeviceUuid"] = (object?)ex.HeartRateDeviceUuid ?? DBNull.Value;
                row["LocationDataInternal"] = (object?)ex.LocationDataInternal ?? DBNull.Value;
                row["CustomId"] = (object?)ex.CustomId ?? DBNull.Value;
                row["AdditionalInternal"] = (object?)ex.AdditionalInternal ?? DBNull.Value;

                row["Duration"] = (object?)ex.Duration ?? DBNull.Value;
                row["Additional"] = (object?)ex.Additional ?? DBNull.Value;
                row["CreateShVer"] = (object?)ex.CreateShVer ?? DBNull.Value;
                row["MeanCaloricBurnRate"] = (object?)ex.MeanCaloricBurnRate ?? DBNull.Value;
                row["LocationData"] = (object?)ex.LocationData ?? DBNull.Value;
                row["StartTime"] = (object?)ex.StartTime ?? DBNull.Value;
                row["ExerciseType"] = (object?)ex.ExerciseType ?? DBNull.Value;
                row["Custom"] = (object?)ex.Custom ?? DBNull.Value;
                row["MaxAltitude"] = (object?)ex.MaxAltitude ?? DBNull.Value;
                row["InclineDistance"] = (object?)ex.InclineDistance ?? DBNull.Value;
                row["MeanHeartRate"] = (object?)ex.MeanHeartRate ?? DBNull.Value;
                row["CountType"] = (object?)ex.CountType ?? DBNull.Value;
                row["MeanRpm"] = (object?)ex.MeanRpm ?? DBNull.Value;
                row["MinAltitude"] = (object?)ex.MinAltitude ?? DBNull.Value;
                row["ModifyShVer"] = (object?)ex.ModifyShVer ?? DBNull.Value;
                row["MaxHeartRate"] = (object?)ex.MaxHeartRate ?? DBNull.Value;

                row["UpdateTime"] = (object?)ex.UpdateTime ?? DBNull.Value;
                row["CreateTime"] = (object?)ex.CreateTime ?? DBNull.Value;

                row["ClientDataId"] = (object?)ex.ClientDataId ?? DBNull.Value;
                row["MaxPower"] = (object?)ex.MaxPower ?? DBNull.Value;
                row["MaxSpeed"] = (object?)ex.MaxSpeed ?? DBNull.Value;
                row["MeanCadence"] = (object?)ex.MeanCadence ?? DBNull.Value;
                row["MinHeartRate"] = (object?)ex.MinHeartRate ?? DBNull.Value;
                row["ClientDataVer"] = (object?)ex.ClientDataVer ?? DBNull.Value;
                row["StepCount"] = (object?)ex.Count ?? DBNull.Value;
                row["Distance"] = (object?)ex.Distance ?? DBNull.Value;
                row["MaxCaloricBurnRate"] = (object?)ex.MaxCaloricBurnRate ?? DBNull.Value;
                row["Calorie"] = (object?)ex.Calorie ?? DBNull.Value;
                row["MaxCadence"] = (object?)ex.MaxCadence ?? DBNull.Value;
                row["DeclineDistance"] = (object?)ex.DeclineDistance ?? DBNull.Value;
                row["Vo2Max"] = (object?)ex.Vo2Max ?? DBNull.Value;
                row["TimeOffset"] = (object?)ex.TimeOffset ?? DBNull.Value;
                row["DeviceUuid"] = (object?)ex.DeviceUuid ?? DBNull.Value;
                row["MaxRpm"] = (object?)ex.MaxRpm ?? DBNull.Value;
                row["Comment"] = (object?)ex.Comment ?? DBNull.Value;
                row["LiveData"] = (object?)ex.LiveData ?? DBNull.Value;
                row["MeanPower"] = (object?)ex.MeanPower ?? DBNull.Value;
                row["MeanSpeed"] = (object?)ex.MeanSpeed ?? DBNull.Value;
                row["PkgName"] = (object?)ex.PkgName ?? DBNull.Value;
                row["AltitudeGain"] = (object?)ex.AltitudeGain ?? DBNull.Value;
                row["AltitudeLoss"] = (object?)ex.AltitudeLoss ?? DBNull.Value;
                row["ExerciseCustomType"] = (object?)ex.ExerciseCustomType ?? DBNull.Value;
                row["AuxiliaryDevices"] = (object?)ex.AuxiliaryDevices ?? DBNull.Value;
                row["EndTime"] = (object?)ex.EndTime ?? DBNull.Value;
                row["DataUuid"] = (object?)ex.DataUuid ?? DBNull.Value;
                row["SweatLoss"] = (object?)ex.SweatLoss ?? DBNull.Value;

                table.Rows.Add(row);
            }

            return table;
        }
    }
}