using System;

namespace BodyTracker.MVVM.Models
{
    /// <summary>
    /// Represents the table structure of a human person's data for the view model.
    /// </summary
    public class FullBodyMeasurementDatas
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
        /// Contains the perons body muscle Percentage
        /// </summary>
        public float? BodyMusclePercentage { get; set; }

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
        /// Contains the value of the fat tongs in cm or inch, depends on the structure of the database table or the programm structure.
        /// </summary>
        public float? FatTong {  get; set; }
    }
}
