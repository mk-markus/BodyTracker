using System;

namespace BodyTracker.MVVM.Models
{
    /// <summary>
    /// Represent the structure of a human person value like Weight, BMI, Muscle Percentage, Fat Percentage,...
    /// </summary>
    public class BodyMetricModel
    {
        /// <summary>
        /// Contains the Metric ID from the database table
        /// </summary>
        public int MetricID { get; set; }

        /// <summary>
        /// Contains the PersonModel ID from the database table
        /// </summary>
        public int PersonID { get; set; }

        /// <summary>
        /// Cotnains the the date of the measurement entry --> When the measurement is entered into the database table.
        /// </summary>
        public DateTime MeasurementDate { get; set; }

        /// <summary>
        /// Contains the perons Weight
        /// </summary>
        public float? BodyWeight { get; set; }

        /// <summary>
        /// Contains the perons Weight
        /// </summary>
        public float? BMI { get; set; }

        /// <summary>
        /// Contains the perons body fat percentage
        /// </summary>
        public float? BodyFatPercentage { get; set; }

        /// <summary>
        /// Gets or sets the upper bound of the body fat percentage range.
        /// </summary>
        public float? BodyFatPercentageTop { get; set; }

        /// <summary>
        /// Gets or sets the lower bound for the acceptable body fat percentage range.
        /// </summary>
        public float? BodyFatPercentageBottom { get; set; }

        /// <summary>
        /// Gets or sets the percentage of body water relative to total body weight.
        /// </summary>
        public float? BodyWaterPercentage { get; set; }

        /// <summary>
        /// Contains the perons body muscle Percentage
        /// </summary>
        public float? BodyMusclePercentage { get; set; }

        /// <summary>
        /// Gets or sets the highest recorded body muscle percentage value.
        /// </summary>
        public float? BodyMusclePercentageTop { get; set; }

        /// <summary>
        /// Gets or sets the lower bound of the body muscle percentage range.
        /// </summary>
        public float? BodyMusclePercentageBottom { get; set; }

        /// <summary>
        /// Gets or sets the mass of the body bone, in kilograms.
        /// </summary>
        public float? BodyBoneMass { get; set; }

        /// <summary>
        /// Contains the perons body visceral fat
        /// </summary>
        public int? BodyVisceralFat { get; set; }
    }
}
