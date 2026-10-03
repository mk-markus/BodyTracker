using System;

namespace BF105.Bluetooth.Model
{
    /// <summary>
    /// Represents standard weight measurement data parsed from the Bluetooth LE weight characteristic, 
    /// including BMI and basic dimensional parameters[cite: 2].
    /// </summary>
    public class BodyWeightMeasurementModel
    {
        /// <summary>
        /// Gets or sets the index of the user profile assigned to the weight record[cite: 2].
        /// </summary>
        public int UserIndex { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the weight measurement was captured[cite: 2].
        /// </summary>
        public DateTime? TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the measured body weight value[cite: 2].
        /// </summary>
        public double? Weight { get; set; }

        /// <summary>
        /// Gets or sets the calculated Body Mass Index (BMI)[cite: 2].
        /// </summary>
        public double? BMI { get; set; }

        /// <summary>
        /// Gets or sets the height associated with the measurement context[cite: 2].
        /// </summary>
        public double? Height { get; set; }
    }
}