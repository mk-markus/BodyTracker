using System;

namespace BF105.Bluetooth.Model
{
    public class SegmentalBodyCompositionMeasurementModel
    {
        /// <summary>
        /// Gets or sets the upper body fat percentage.
        /// </summary>
        public float? UpperBodyFatPercentage { get; set; }

        /// <summary>
        /// Gets or sets the lower body fat percentage.
        /// </summary>
        public float? LowerBodyFatPercentage { get; set; }

        /// <summary>
        /// Gets or sets the upper body muscle percentage.
        /// </summary>
        public float? UpperBodyMusclePercentage { get; set; }

        /// <summary>
        /// Gets or sets the lower body muscle percentage.
        /// </summary>
        public float? LowerBodyMusclePercentage { get; set; }
    }
}