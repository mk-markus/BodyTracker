using BodyTracker.Models;
using System;
using System.Collections.Generic;
using System.Data;

public static class HeavyAppDataTable
{
    public static DataTable CreateHeavyAppDataTable(
        int personId,
        IEnumerable<HeavyAppCSVModel> workouts)
    {
        var table = new DataTable();

        table.Columns.Add("person_id", typeof(int));
        table.Columns.Add("workout_title", typeof(string));
        table.Columns.Add("start_time", typeof(DateTime));
        table.Columns.Add("end_time", typeof(DateTime));
        table.Columns.Add("update_at", typeof(DateTime));
        table.Columns.Add("description", typeof(string));
        table.Columns.Add("exercise_title", typeof(string));
        table.Columns.Add("superset_id", typeof(string));
        table.Columns.Add("exercise_notes", typeof(string));
        table.Columns.Add("set_index", typeof(int));
        table.Columns.Add("set_type", typeof(string));

        table.Columns.Add("weight_current", typeof(double));
        table.Columns.Add("weight_initial", typeof(double));

        table.Columns.Add("reps_current", typeof(int));
        table.Columns.Add("reps_initial", typeof(int));

        table.Columns.Add("distance_current", typeof(double));
        table.Columns.Add("distance_initial", typeof(double));

        table.Columns.Add("duration_sec_current", typeof(double));
        table.Columns.Add("duration_sec_initial", typeof(double));

        table.Columns.Add("rpe_current", typeof(double));
        table.Columns.Add("rpe_initial", typeof(double));

        foreach (var item in workouts)
        {
            var row = table.NewRow();

            row["person_id"] = personId;
            row["workout_title"] = (object?)item.Title ?? DBNull.Value;
            row["start_time"] = (object?)item.StartTime ?? DBNull.Value;
            row["end_time"] = (object?)item.EndTime ?? DBNull.Value;
            row["update_at"] = (object?)item.StartTime ?? DBNull.Value;
            row["description"] = (object?)item.Description ?? DBNull.Value;
            row["exercise_title"] = (object?)item.ExerciseTitle ?? DBNull.Value;
            row["superset_id"] = (object?)item.SupersetId ?? DBNull.Value;
            row["exercise_notes"] = (object?)item.ExerciseNotes ?? DBNull.Value;
            row["set_index"] = (object?)item.SetIndex ?? DBNull.Value;
            row["set_type"] = (object?)item.SetType ?? DBNull.Value;

            row["weight_current"] = (object?)item.WeightKg ?? DBNull.Value;
            row["weight_initial"] = (object?)item.WeightKg ?? DBNull.Value;

            row["reps_current"] = (object?)item.Reps ?? DBNull.Value;
            row["reps_initial"] = (object?)item.Reps ?? DBNull.Value;

            row["distance_current"] = (object?)item.DistanceKm ?? DBNull.Value;
            row["distance_initial"] = (object?)item.DistanceKm ?? DBNull.Value;

            row["duration_sec_current"] = (object?)item.DurationSeconds ?? DBNull.Value;
            row["duration_sec_initial"] = (object?)item.DurationSeconds ?? DBNull.Value;

            row["rpe_current"] = (object?)item.Rpe ?? DBNull.Value;
            row["rpe_initial"] = (object?)item.Rpe ?? DBNull.Value;

            table.Rows.Add(row);
        }

        return table;
    }
}