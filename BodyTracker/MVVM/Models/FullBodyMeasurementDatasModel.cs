using System;

namespace BodyTracker.MVVM.Models
{
    /// <summary>
    /// Represents the table structure of a human person's data for the view model.
    /// </summary
    public class FullBodyMeasurementDatasModel
    {
        /// <summary>
        /// Contains the Metric ID from the database table
        /// </summary>
        public int? MetricID { get; set; }

        /// <summary>
        /// Contains the Demension ID from the database table
        /// </summary>
        public int? DemensionID { get; set; }

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

        /// <summary>
        /// Contains the value of the Chest circumfernce in cm or inch, depends on the structure of the database table or the programm structure.
        /// </summary>
        public float? ChestCircumference { get; set; }

        /// <summary>
        /// Contains the value of the Waist circumfernce in cm or inch, depends on the structure of the database table or the programm structure.
        /// </summary>
        public float? WaistCircumference { get; set; }

        /// <summary>
        /// Contains the value of the Hips circumfernce in cm or inch, depends on the structure of the database table or the programm structure.
        /// </summary>
        public float? HipsCircumference { get; set; }

        /// <summary>
        /// Gets or sets the breast skinfold measurement in millimeters or inches, depending on the application or database configuration.
        /// </summary>
        public float? FatTongBreastCrease { get; set; }

        /// <summary>
        /// Gets or sets the armpit skinfold measurement in millimeters or inches, depending on the application or database configuration.
        /// </summary>
        public float? FatTongArmpitCrease { get; set; }

        /// <summary>
        /// Gets or sets the abdominal skinfold measurement in millimeters or inches, depending on the application or database configuration.
        /// </summary>
        public float? FatTongAbdominalCrease { get; set; }

        /// <summary>
        /// Gets or sets the hip skinfold measurement in millimeters or inches, depending on the application or database configuration.
        /// </summary>
        public float? FatTongHipCrease { get; set; }

        /// <summary>
        /// Gets or sets the thigh skinfold measurement in millimeters or inches, depending on the application or database configuration.
        /// </summary>
        public float? FatTongThighCrease { get; set; }

        /// <summary>
        /// Gets or sets the back (subscapular) skinfold measurement in millimeters or inches, depending on the application or database configuration.
        /// </summary>
        public float? FatTongBackCrease { get; set; }

        /// <summary>
        /// Gets or sets the triceps skinfold measurement in millimeters or inches, depending on the application or database configuration.
        /// </summary>
        public float? FatTongTricepsCrease { get; set; }

        /// <summary>
        /// contains the value of the fat tongs in mm or inch, depends on the structure of the database table or the programm structure.
        /// </summary>
        public float? CaliperBodyFatPercentage { get; set; }



    }
}
