using BodyTracker.Models;
using CsvHelper.Configuration;

namespace BodyTracker.Services
{
    public sealed class HeavyAppCSCMap : ClassMap<HeavyAppCSVModel>
    {
        /// <summary>
        /// Initializes a new instance of the class map, mapping model properties to CSV columns for workout application records.
        /// </summary>
        /// <remarks>Configures CsvHelper property mappings for workout titles, flexible datetime converters for start and end times, description, exercise details, supersets, set indices, set types, and flexible double converters for weight, reps, distance, and RPE.</remarks>
        public HeavyAppCSCMap()
        {
            Map(m => m.Title).Name("title");

            Map(m => m.StartTime)
                .Name("start_time")
                .TypeConverter<FlexibleDateTimeConverterService>();

            Map(m => m.EndTime)
                .Name("end_time")
                .TypeConverter<FlexibleDateTimeConverterService>();

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
                .TypeConverter<FlexibleDoubleConverterService>();

            Map(m => m.Reps)
               .Name("reps")
               .TypeConverter<FlexibleDoubleConverterService>();

            Map(m => m.DistanceKm)
                .Name("distance_km")
                .TypeConverter<FlexibleDoubleConverterService>();

            Map(m => m.DurationSeconds)
                .Name("duration_seconds");

            Map(m => m.Rpe)
               .Name("rpe")
               .TypeConverter<FlexibleDoubleConverterService>();
        }
    }
}