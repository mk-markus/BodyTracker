using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyTracker.Models.Chart
{
    public class SamsungExerciseDashboardModel
    {
        /// <summary>
        /// Gets or sets the start date and time of the exercise session.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the optional title or description of the exercise session.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the numeric identifier representing the type of exercise performed.
        /// </summary>
        public int? ExerciseType { get; set; }

        /// <summary>
        /// Gets or sets the total duration of the exercise session, typically measured in minutes.
        /// </summary>
        public double? Duration { get; set; }

        /// <summary>
        /// Gets or sets the total distance covered during the exercise session, typically measured in kilometers.
        /// </summary>
        public double? Distance { get; set; }

        /// <summary>
        /// Gets or sets the total burned calories recorded during the exercise session.
        /// </summary>
        public double? Calorie { get; set; }

        /// <summary>
        /// Gets or sets the average heart rate recorded during the session, measured in beats per minute (bpm).
        /// </summary>
        public double? MeanHeartRate { get; set; }

        /// <summary>
        /// Gets or sets the maximum heart rate recorded during the session, measured in beats per minute (bpm).
        /// </summary>
        public double? MaxHeartRate { get; set; }

        /// <summary>
        /// Gets or sets the minimum heart rate recorded during the session, measured in beats per minute (bpm).
        /// </summary>
        public double? MinHeartRate { get; set; }

        /// <summary>
        /// Gets or sets the average movement speed during the exercise session, typically measured in km/h.
        /// </summary>
        public double? MeanSpeed { get; set; }
    }

}
