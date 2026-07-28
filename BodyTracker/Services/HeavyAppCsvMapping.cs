using BodyTracker.Converters;
using BodyTracker.MVVM.Models;
using BodyTracker.Services;
using CsvHelper.Configuration;
using System.ComponentModel;
using System.Globalization;

namespace BodyTracker.MVVM.Mapping
{
    public sealed class HeavyAppCSCMap : ClassMap<HeavyAppCSVModel>
    {
        public HeavyAppCSCMap()
        {
            Map(m => m.Title).Name("title");

            Map(m => m.StartTime)
                .Name("start_time")
                .TypeConverter<FlexibleDateTimeConverter>();

            Map(m => m.EndTime)
                .Name("end_time")
                .TypeConverter<FlexibleDateTimeConverter>();

            Map(m => m.Description)
                .Name("description");

            Map(m => m.ExerciseTitle)
                .Name("exercise_title");

            Map(m => m.SupersetId)
                .Name("superset_id");

            Map(m => m.ExerciseNotes)
                .Name("exercise_notes");

            Map(m => m.SetIndex)
                .Name("set_index");

            Map(m => m.SetType)
                .Name("set_type");

            Map(m => m.WeightKg)
                .Name("weight_kg")
                .TypeConverter<FlexibleDoubleConverter>();

            Map(m => m.Reps)
               .Name("reps")
               .TypeConverter<FlexibleDoubleConverter>();

            Map(m => m.DistanceKm)
                .Name("distance_km")
                .TypeConverter<FlexibleDoubleConverter>();

            Map(m => m.DurationSeconds)
                .Name("duration_seconds");

            Map(m => m.Rpe)
               .Name("rpe")
               .TypeConverter<FlexibleDoubleConverter>();
        }
    }
}