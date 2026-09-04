using System;

namespace BodyTracker.Models.Export
{
    /// <summary>
    /// Represents the export model containing initial and current general body metrics, composition data, and fitness indices.
    /// </summary>
    public class BodyMetricsExportModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the body metrics record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the current record was created or recorded.
        /// </summary>
        public DateTime CurrentTime { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the record was last updated.
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// Gets or sets the initial body weight measurement.
        /// </summary>
        public float? InitialBodyWeight { get; set; }

        /// <summary>
        /// Gets or sets the current body weight measurement.
        /// </summary>
        public float? CurrentBodyWeight { get; set; }

        /// <summary>
        /// Gets or sets the initial Body Mass Index (BMI).
        /// </summary>
        public float? InitialBMI { get; set; }

        /// <summary>
        /// Gets or sets the current Body Mass Index (BMI).
        /// </summary>
        public float? CurrentBMI { get; set; }

        /// <summary>
        /// Gets or sets the initial overall body fat percentage.
        /// </summary>
        public float? InitialBodyFatPercentage { get; set; }

        /// <summary>
        /// Gets or sets the current overall body fat percentage.
        /// </summary>
        public float? CurrentBodyFatPercentage { get; set; }

        /// <summary>
        /// Gets or sets the initial body fat percentage of the upper body.
        /// </summary>
        public float? InitialBodyFatPercentageTop { get; set; }

        /// <summary>
        /// Gets or sets the current body fat percentage of the upper body.
        /// </summary>
        public float? CurrentBodyFatPercentageTop { get; set; }

        /// <summary>
        /// Gets or sets the initial body fat percentage of the lower body.
        /// </summary>
        public float? InitialBodyFatPercentageBottom { get; set; }

        /// <summary>
        /// Gets or sets the current body fat percentage of the lower body.
        /// </summary>
        public float? CurrentBodyFatPercentageBottom { get; set; }

        /// <summary>
        /// Gets or sets the initial body water percentage.
        /// </summary>
        public float? InitialBodyWaterPercentage { get; set; }

        /// <summary>
        /// Gets or sets the current body water percentage.
        /// </summary>
        public float? CurrentBodyWaterPercentage { get; set; }

        /// <summary>
        /// Gets or sets the initial overall body muscle percentage.
        /// </summary>
        public float? InitialBodyMusclePercentage { get; set; }

        /// <summary>
        /// Gets or sets the current overall body muscle percentage.
        /// </summary>
        public float? CurrentBodyMusclePercentage { get; set; }

        /// <summary>
        /// Gets or sets the initial body muscle percentage of the upper body.
        /// </summary>
        public float? InitialBodyMusclePercentageTop { get; set; }

        /// <summary>
        /// Gets or sets the current body muscle percentage of the upper body.
        /// </summary>
        public float? CurrentBodyMusclePercentageTop { get; set; }

        /// <summary>
        /// Gets or sets the initial body muscle percentage of the lower body.
        /// </summary>
        public float? InitialBodyMusclePercentageBottom { get; set; }

        /// <summary>
        /// Gets or sets the current body muscle percentage of the lower body.
        /// </summary>
        public float? CurrentBodyMusclePercentageBottom { get; set; }

        /// <summary>
        /// Gets or sets the initial Fat-Free Mass (FFM) in kilograms.
        /// </summary>
        public float? InitialFFM_kg { get; set; }

        /// <summary>
        /// Gets or sets the current Fat-Free Mass (FFM) in kilograms.
        /// </summary>
        public float? CurrentFFM_kg { get; set; }

        /// <summary>
        /// Gets or sets the initial Fat-Free Mass Index (FFMI).
        /// </summary>
        public float? InitialFFMIndex { get; set; }

        /// <summary>
        /// Gets or sets the current Fat-Free Mass Index (FFMI).
        /// </summary>
        public float? CurrentFFMIndex { get; set; }

        /// <summary>
        /// Gets or sets the initial descriptive classification of the Fat-Free Mass Index (FFMI).
        /// </summary>
        public string? InitialFFMIndexDescription { get; set; }

        /// <summary>
        /// Gets or sets the current descriptive classification of the Fat-Free Mass Index (FFMI).
        /// </summary>
        public string? CurrentFFMIndexDescription { get; set; }

        /// <summary>
        /// Gets or sets the initial body bone mass measurement.
        /// </summary>
        public float? InitialBodyBoneMass { get; set; }

        /// <summary>
        /// Gets or sets the current body bone mass measurement.
        /// </summary>
        public float? CurrentBodyBoneMass { get; set; }

        /// <summary>
        /// Gets or sets the initial visceral fat rating.
        /// </summary>
        public int? InitialBodyVisceralFat { get; set; }

        /// <summary>
        /// Gets or sets the current visceral fat rating.
        /// </summary>
        public int? CurrentBodyVisceralFat { get; set; }
    }
}