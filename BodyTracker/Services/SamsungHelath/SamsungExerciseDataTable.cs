using System;
using System.Collections.Generic;
using System.Data;

namespace BodyTracker.Services.SamsungHealth
{
    public static class SamsungExerciseDataTable
    {
        public static DataTable CreateExerciseDataTable(int personId, IEnumerable<SamsungExerciseModel> exercises)
        {
            var table = new DataTable();

            table.Columns.Add("person_id", typeof(int));

            table.Columns.Add("live_data_internal", typeof(string));
            table.Columns.Add("mission_value", typeof(string));
            table.Columns.Add("race_target", typeof(string));
            table.Columns.Add("subset_data", typeof(string));
            table.Columns.Add("start_longitude", typeof(double));
            table.Columns.Add("routine_data_uuid", typeof(string));
            table.Columns.Add("total_calorie", typeof(double));
            table.Columns.Add("completion_status", typeof(int));
            table.Columns.Add("pace_info_id", typeof(long));
            table.Columns.Add("activity_type", typeof(int));
            table.Columns.Add("pace_live_data", typeof(string));
            table.Columns.Add("sensing_status", typeof(string));
            table.Columns.Add("source_type", typeof(int));
            table.Columns.Add("mission_type", typeof(int));
            table.Columns.Add("ftp", typeof(double));
            table.Columns.Add("tracking_status", typeof(int));
            table.Columns.Add("program_id", typeof(long));
            table.Columns.Add("title", typeof(string));
            table.Columns.Add("reward_status", typeof(int));
            table.Columns.Add("heart_rate_sample_count", typeof(int));
            table.Columns.Add("start_latitude", typeof(double));
            table.Columns.Add("mission_extra_value", typeof(string));
            table.Columns.Add("program_schedule_id", typeof(long));
            table.Columns.Add("heart_rate_device_uuid", typeof(string));
            table.Columns.Add("location_data_internal", typeof(string));
            table.Columns.Add("custom_id", typeof(string));
            table.Columns.Add("additional_internal", typeof(string));

            table.Columns.Add("duration", typeof(long));
            table.Columns.Add("additional", typeof(string));
            table.Columns.Add("create_sync_version", typeof(string));
            table.Columns.Add("mean_caloric_burn_rate", typeof(double));
            table.Columns.Add("location_data", typeof(string));
            table.Columns.Add("start_time", typeof(DateTime));
            table.Columns.Add("exercise_type", typeof(int));
            table.Columns.Add("custom_text", typeof(string));
            table.Columns.Add("max_altitude", typeof(double));
            table.Columns.Add("incline_distance", typeof(double));
            table.Columns.Add("mean_heart_rate", typeof(double));
            table.Columns.Add("count_type", typeof(int));
            table.Columns.Add("mean_rpm", typeof(double));
            table.Columns.Add("min_altitude", typeof(double));
            table.Columns.Add("modify_sync_version", typeof(string));
            table.Columns.Add("max_heart_rate", typeof(double));

            table.Columns.Add("update_at", typeof(DateTime));
            table.Columns.Add("create_at", typeof(DateTime));

            table.Columns.Add("client_data_id", typeof(string));
            table.Columns.Add("max_power", typeof(double));
            table.Columns.Add("max_speed", typeof(double));
            table.Columns.Add("mean_cadence", typeof(double));
            table.Columns.Add("min_heart_rate", typeof(double));
            table.Columns.Add("client_data_version", typeof(string));
            table.Columns.Add("count_value", typeof(int));
            table.Columns.Add("distance", typeof(double));
            table.Columns.Add("max_caloric_burn_rate", typeof(double));
            table.Columns.Add("calorie", typeof(double));
            table.Columns.Add("max_cadence", typeof(double));
            table.Columns.Add("decline_distance", typeof(double));
            table.Columns.Add("vo2_max", typeof(double));
            table.Columns.Add("time_offset", typeof(string));
            table.Columns.Add("device_uuid", typeof(string));
            table.Columns.Add("max_rpm", typeof(double));
            table.Columns.Add("comment_text", typeof(string));
            table.Columns.Add("live_data", typeof(string));
            table.Columns.Add("mean_power", typeof(double));
            table.Columns.Add("mean_speed", typeof(double));
            table.Columns.Add("package_name", typeof(string));
            table.Columns.Add("altitude_gain", typeof(double));
            table.Columns.Add("altitude_loss", typeof(double));
            table.Columns.Add("exercise_custom_type", typeof(int));
            table.Columns.Add("auxiliary_devices", typeof(string));
            table.Columns.Add("end_time", typeof(DateTime));
            table.Columns.Add("data_uuid", typeof(string));
            table.Columns.Add("sweat_loss", typeof(double));

            foreach (var ex in exercises)
            {
                var row = table.NewRow();

                row["person_id"] = personId;

                row["live_data_internal"] = (object?)ex.LiveDataInternal ?? DBNull.Value;
                row["mission_value"] = (object?)ex.MissionValue ?? DBNull.Value;
                row["race_target"] = (object?)ex.RaceTarget ?? DBNull.Value;
                row["subset_data"] = (object?)ex.SubsetData ?? DBNull.Value;
                row["start_longitude"] = (object?)ex.StartLongitude ?? DBNull.Value;
                row["routine_data_uuid"] = (object?)ex.RoutineDataUuid ?? DBNull.Value;
                row["total_calorie"] = (object?)ex.TotalCalorie ?? DBNull.Value;
                row["completion_status"] = (object?)ex.CompletionStatus ?? DBNull.Value;
                row["pace_info_id"] = (object?)ex.PaceInfoId ?? DBNull.Value;
                row["activity_type"] = (object?)ex.ActivityType ?? DBNull.Value;
                row["pace_live_data"] = (object?)ex.PaceLiveData ?? DBNull.Value;
                row["sensing_status"] = (object?)ex.SensingStatus ?? DBNull.Value;
                row["source_type"] = (object?)ex.SourceType ?? DBNull.Value;
                row["mission_type"] = (object?)ex.MissionType ?? DBNull.Value;
                row["ftp"] = (object?)ex.Ftp ?? DBNull.Value;
                row["tracking_status"] = (object?)ex.TrackingStatus ?? DBNull.Value;
                row["program_id"] = (object?)ex.ProgramId ?? DBNull.Value;
                row["title"] = (object?)ex.Title ?? DBNull.Value;
                row["reward_status"] = (object?)ex.RewardStatus ?? DBNull.Value;
                row["heart_rate_sample_count"] = (object?)ex.HeartRateSampleCount ?? DBNull.Value;
                row["start_latitude"] = (object?)ex.StartLatitude ?? DBNull.Value;
                row["mission_extra_value"] = (object?)ex.MissionExtraValue ?? DBNull.Value;
                row["program_schedule_id"] = (object?)ex.ProgramScheduleId ?? DBNull.Value;
                row["heart_rate_device_uuid"] = (object?)ex.HeartRateDeviceUuid ?? DBNull.Value;
                row["location_data_internal"] = (object?)ex.LocationDataInternal ?? DBNull.Value;
                row["custom_id"] = (object?)ex.CustomId ?? DBNull.Value;
                row["additional_internal"] = (object?)ex.AdditionalInternal ?? DBNull.Value;

                row["duration"] = (object?)ex.Duration ?? DBNull.Value;
                row["additional"] = (object?)ex.Additional ?? DBNull.Value;
                row["create_sync_version"] = (object?)ex.CreateShVer ?? DBNull.Value;
                row["mean_caloric_burn_rate"] = (object?)ex.MeanCaloricBurnRate ?? DBNull.Value;
                row["location_data"] = (object?)ex.LocationData ?? DBNull.Value;
                row["start_time"] = (object?)ex.StartTime ?? DBNull.Value;
                row["exercise_type"] = (object?)ex.ExerciseType ?? DBNull.Value;
                row["custom_text"] = (object?)ex.Custom ?? DBNull.Value;
                row["max_altitude"] = (object?)ex.MaxAltitude ?? DBNull.Value;
                row["incline_distance"] = (object?)ex.InclineDistance ?? DBNull.Value;
                row["mean_heart_rate"] = (object?)ex.MeanHeartRate ?? DBNull.Value;
                row["count_type"] = (object?)ex.CountType ?? DBNull.Value;
                row["mean_rpm"] = (object?)ex.MeanRpm ?? DBNull.Value;
                row["min_altitude"] = (object?)ex.MinAltitude ?? DBNull.Value;
                row["modify_sync_version"] = (object?)ex.ModifyShVer ?? DBNull.Value;
                row["max_heart_rate"] = (object?)ex.MaxHeartRate ?? DBNull.Value;

                row["update_at"] = (object?)ex.UpdateTime ?? DBNull.Value;
                row["create_at"] = (object?)ex.CreateTime ?? DBNull.Value;

                row["client_data_id"] = (object?)ex.ClientDataId ?? DBNull.Value;
                row["max_power"] = (object?)ex.MaxPower ?? DBNull.Value;
                row["max_speed"] = (object?)ex.MaxSpeed ?? DBNull.Value;
                row["mean_cadence"] = (object?)ex.MeanCadence ?? DBNull.Value;
                row["min_heart_rate"] = (object?)ex.MinHeartRate ?? DBNull.Value;
                row["client_data_version"] = (object?)ex.ClientDataVer ?? DBNull.Value;
                row["count_value"] = (object?)ex.Count ?? DBNull.Value;
                row["distance"] = (object?)ex.Distance ?? DBNull.Value;
                row["max_caloric_burn_rate"] = (object?)ex.MaxCaloricBurnRate ?? DBNull.Value;
                row["calorie"] = (object?)ex.Calorie ?? DBNull.Value;
                row["max_cadence"] = (object?)ex.MaxCadence ?? DBNull.Value;
                row["decline_distance"] = (object?)ex.DeclineDistance ?? DBNull.Value;
                row["vo2_max"] = (object?)ex.Vo2Max ?? DBNull.Value;
                row["time_offset"] = (object?)ex.TimeOffset ?? DBNull.Value;
                row["device_uuid"] = (object?)ex.DeviceUuid ?? DBNull.Value;
                row["max_rpm"] = (object?)ex.MaxRpm ?? DBNull.Value;
                row["comment_text"] = (object?)ex.Comment ?? DBNull.Value;
                row["live_data"] = (object?)ex.LiveData ?? DBNull.Value;
                row["mean_power"] = (object?)ex.MeanPower ?? DBNull.Value;
                row["mean_speed"] = (object?)ex.MeanSpeed ?? DBNull.Value;
                row["package_name"] = (object?)ex.PkgName ?? DBNull.Value;
                row["altitude_gain"] = (object?)ex.AltitudeGain ?? DBNull.Value;
                row["altitude_loss"] = (object?)ex.AltitudeLoss ?? DBNull.Value;
                row["exercise_custom_type"] = (object?)ex.ExerciseCustomType ?? DBNull.Value;
                row["auxiliary_devices"] = (object?)ex.AuxiliaryDevices ?? DBNull.Value;
                row["end_time"] = (object?)ex.EndTime ?? DBNull.Value;
                row["data_uuid"] = (object?)ex.DataUuid ?? DBNull.Value;
                row["sweat_loss"] = (object?)ex.SweatLoss ?? DBNull.Value;

                table.Rows.Add(row);
            }

            return table;
        }
    }
}