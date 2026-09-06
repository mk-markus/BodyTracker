using BodyTracker.Models;
using System;
using System.Collections.Generic;
using System.Data;

public static class HevyAppDataTable
{
    public static DataTable CreateHevyAppDataTable(
        int personId,
        IEnumerable<HevyAppCSVModel> workouts)
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

        table.Columns.Add("weight", typeof(double));

        table.Columns.Add("reps", typeof(int));

        table.Columns.Add("distance", typeof(double));

        table.Columns.Add("duration_sec", typeof(int));

        table.Columns.Add("rpe", typeof(double));

        table.Columns.Add("uuid", typeof(string));

        foreach (var item in workouts)
        {
            var row = table.NewRow();

            row["person_id"] = personId;
            row["workout_title"] = (object?)item.Title ?? DBNull.Value;

            row["start_time"] = (object?)item.StartTime ?? DBNull.Value;
            row["end_time"] = (object?)item.EndTime ?? DBNull.Value;
            row["update_at"] = DateTime.Now;
            
            row["description"] = (object?)item.Description ?? DBNull.Value;
            row["exercise_title"] = (object?)item.ExerciseTitle ?? DBNull.Value;
            row["superset_id"] = (object?)item.SupersetId ?? DBNull.Value;
            row["exercise_notes"] = (object?)item.ExerciseNotes ?? DBNull.Value;
            row["set_index"] = (object?)item.SetIndex ?? DBNull.Value;
            row["set_type"] = (object?)item.SetType ?? DBNull.Value;

            row["weight"] = (object?)item.WeightKg ?? DBNull.Value;

            row["reps"] = (object?)item.Reps ?? DBNull.Value;

            row["distance"] = (object?)item.DistanceKm ?? DBNull.Value;

            row["duration_sec"] = (object?)item.DurationSeconds ?? DBNull.Value;

            row["rpe"] = (object?)item.Rpe ?? DBNull.Value;

            row["uuid"] = (object?)item.DataUuid ?? DBNull.Value;

            table.Rows.Add(row);
        }

        return table;
    }
}