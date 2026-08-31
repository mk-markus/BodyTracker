using System;

namespace BodyTracker.Models.WorkoutLog
{
    /// <summary>
    /// Represents the progress and performance metrics of a specific exercise on a given date.
    /// </summary>
    public class WorkoutExerciseProgressModel
    {
        /// <summary>
        /// Gets or sets the name of the exercise.
        /// </summary>
        public string ExerciseName { get; set; }

        /// <summary>
        /// Gets or sets the date when the exercise session took place.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the peak weight lifted during the session.
        /// </summary>
        public double PeakWeight { get; set; }

        /// <summary>
        /// Gets or sets the calculated maximum One-Rep Max (1RM) for the session.
        /// </summary>
        public double MaxOneRepMax { get; set; }

        /// <summary>
        /// Gets or sets the total volume (weight multiplied by reps and sets) accumulated during the session.
        /// </summary>
        public double TotalVolume { get; set; }
    }
}
