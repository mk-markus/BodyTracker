using System;

namespace BodyTracker.Models.Export
{
    /// <summary>
    /// Represents the export model containing initial and current body dimensions, skinfold measurements, and caliper-based body fat percentages.
    /// </summary>
    public class BodyDimensionsExportModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the body dimensions record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the record was created.
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the record was last updated.
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// Gets or sets the initial chest circumference measurement.
        /// </summary>
        public float? InitialChestCircumference { get; set; }

        /// <summary>
        /// Gets or sets the current chest circumference measurement.
        /// </summary>
        public float? CurrentChestCircumference { get; set; }

        /// <summary>
        /// Gets or sets the initial waist circumference measurement.
        /// </summary>
        public float? InitialWaistCircumference { get; set; }

        /// <summary>
        /// Gets or sets the current waist circumference measurement.
        /// </summary>
        public float? CurrentWaistCircumference { get; set; }

        /// <summary>
        /// Gets or sets the initial hips circumference measurement.
        /// </summary>
        public float? InitialHipsCircumference { get; set; }

        /// <summary>
        /// Gets or sets the current hips circumference measurement.
        /// </summary>
        public float? CurrentHipsCircumference { get; set; }

        /// <summary>
        /// Gets or sets the initial breast skinfold measurement.
        /// </summary>
        public float? InitialFatTongBreastCrease { get; set; }

        /// <summary>
        /// Gets or sets the current breast skinfold measurement.
        /// </summary>
        public float? CurrentFatTongBreastCrease { get; set; }

        /// <summary>
        /// Gets or sets the initial armpit skinfold measurement.
        /// </summary>
        public float? InitialFatTongArmpitCrease { get; set; }

        /// <summary>
        /// Gets or sets the current armpit skinfold measurement.
        /// </summary>
        public float? CurrentFatTongArmpitCrease { get; set; }

        /// <summary>
        /// Gets or sets the initial abdominal skinfold measurement.
        /// </summary>
        public float? InitialFatTongAbdominalCrease { get; set; }

        /// <summary>
        /// Gets or sets the current abdominal skinfold measurement.
        /// </summary>
        public float? CurrentFatTongAbdominalCrease { get; set; }

        /// <summary>
        /// Gets or sets the initial hip skinfold measurement.
        /// </summary>
        public float? InitialFatTongHipCrease { get; set; }

        /// <summary>
        /// Gets or sets the current hip skinfold measurement.
        /// </summary>
        public float? CurrentFatTongHipCrease { get; set; }

        /// <summary>
        /// Gets or sets the initial thigh skinfold measurement.
        /// </summary>
        public float? InitialFatTongThighCrease { get; set; }

        /// <summary>
        /// Gets or sets the current thigh skinfold measurement.
        /// </summary>
        public float? CurrentFatTongThighCrease { get; set; }

        /// <summary>
        /// Gets or sets the initial back skinfold measurement.
        /// </summary>
        public float? InitialFatTongBackCrease { get; set; }

        /// <summary>
        /// Gets or sets the current back skinfold measurement.
        /// </summary>
        public float? CurrentFatTongBackCrease { get; set; }

        /// <summary>
        /// Gets or sets the initial triceps skinfold measurement.
        /// </summary>
        public float? InitialFatTongTricepsCrease { get; set; }

        /// <summary>
        /// Gets or sets the current triceps skinfold measurement.
        /// </summary>
        public float? CurrentFatTongTricepsCrease { get; set; }

        /// <summary>
        /// Gets or sets the initial body fat percentage calculated via caliper measurements.
        /// </summary>
        public float? InitialCaliperBodyFatPercentage { get; set; }

        /// <summary>
        /// Gets or sets the current body fat percentage calculated via caliper measurements.
        /// </summary>
        public float? CurrentCaliperBodyFatPercentage { get; set; }
    }
}