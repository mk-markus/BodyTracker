
using System;

namespace BF105.Bluetooth.Model
{
    /// <summary>
    /// Represents detailed body composition metrics parsed from the Bluetooth LE Body Composition characteristic (UUID 2A9C), 
    /// including body fat, muscle mass, water weight, and bioelectrical impedance[cite: 1].
    /// </summary>
    public class BodyCompositionMeasurementModel
    {
        /// <summary>
        /// Gets or sets the exact timestamp when the body composition measurement was recorded[cite: 1].
        /// </summary>
        public DateTime? TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the identifier or index of the user associated with this measurement[cite: 1].
        /// </summary>
        public string? UserIndex { get; set; }

        /// <summary>
        /// Gets or sets the height of the person measured, utilized for physiological calculations[cite: 1].
        /// </summary>
        public float? PersonHeight { get; set; }

        /// <summary>
        /// Gets or sets the body weight recorded during the composition analysis session[cite: 1].
        /// </summary>
        public float? BodyWeight { get; set; }

        /// <summary>
        /// Gets or sets the measured body fat percentage[cite: 1].
        /// </summary>
        public float? BodyFatPercentage { get; set; }

        /// <summary>
        /// Gets or sets the skeletal muscle percentage relative to total body weight[cite: 1].
        /// </summary>
        public float? BodyMusclePercentage { get; set; }

        /// <summary>
        /// Gets or sets the absolute muscle mass measured in kilograms[cite: 1].
        /// </summary>
        public float? BodyMuscleMassKg { get; set; }

        /// <summary>
        /// Gets or sets the Fat-Free Mass (FFM) value[cite: 1].
        /// </summary>
        public float? BodyFFM { get; set; }

        /// <summary>
        /// Gets or sets the soft lean mass value[cite: 1].
        /// </summary>
        public float? BodySoftLeanMass { get; set; }

        /// <summary>
        /// Gets or sets the total body water percentage[cite: 1].
        /// </summary>
        public float? BodyWaterPercentage { get; set; }

        /// <summary>
        /// Gets or sets the raw bioelectrical impedance value measured in ohms[cite: 1].
        /// </summary>
        public float? BodyImpedanz { get; set; }

        /// <summary>
        /// Gets or sets the Basal Metabolic Rate (BMR)[cite: 1].
        /// </summary>
        public float BodyBMR { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether more measurement data packets are 
        /// available from a multi-packet transmission [2A9C][cite: 1].
        /// </summary>
        public bool? MoreMeasurementsAvailable { get; set; }
    }
}