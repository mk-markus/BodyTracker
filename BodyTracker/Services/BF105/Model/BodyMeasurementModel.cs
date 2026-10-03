using System;

namespace BF105.Bluetooth.Model
{
    /// <summary>
    /// Represents a consolidated container model holding all distinct measurement data sets 
    /// received from the body analysis scale during a measurement session.
    /// </summary>
    public class BodyMeasurementModel
    {
        /// <summary>
        /// Gets or sets the body weight measurement data, including timestamps, units, 
        /// and weight values parsed from the BLE weight characteristic.
        /// </summary>
        public BodyWeightMeasurementModel? BodyWeightMeasurement { get; set; }

        /// <summary>
        /// Gets or sets the detailed body composition metrics (such as body fat percentage, 
        /// muscle mass, and water weight) parsed from the BLE body composition characteristic.
        /// </summary>
        public BodyCompositionMeasurementModel? BodyCompositionMeasurement { get; set; }

        /// <summary>
        /// Gets or sets the detailed segmental body composition metrics (such as body upper and lower fat percentage, 
        /// upper and lower) parsed from the BLE body composition characteristic.
        /// </summary>
        public  SegmentalBodyCompositionMeasurementModel? SegmentalBodyCompositionMeasurement{ get; set; }

    }
}