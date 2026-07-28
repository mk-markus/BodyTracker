using System;

namespace BodyTracker.Services
{
    public class HeavyAppCSVModel
    {
        public Guid DataUuid { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string? Description { get; set; }

        public string ExerciseTitle { get; set; } = string.Empty;

        public string? SupersetId { get; set; }

        public string? ExerciseNotes { get; set; }

        public int SetIndex { get; set; }

        public string? SetType { get; set; }

        public double? WeightKg { get; set; }

        public double? Reps { get; set; }

        public double? DistanceKm { get; set; }

        public int? DurationSeconds { get; set; }

        public double? Rpe { get; set; }
    }
}
