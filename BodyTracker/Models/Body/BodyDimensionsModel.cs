using System;

namespace BodyTracker.Models
{
    /// <summary>
    /// Represents the structure of a human person value like hips-, waist-, chest-circumfernce
    /// </summary>
    public class BodyDimensionsModel
    {
        /// <summary>
        /// Contains the Demension ID from the database table
        /// </summary>
        public int DimensionID { get; set; }

        /// <summary>
        /// Contains the PersonModel ID from the database table
        /// </summary>
        public int PersonID { get; set; }

        /// <summary>
        /// Cotnains the the date of the bodyMeasurement entry --> When the bodyMeasurement is entered into the database table.
        /// </summary>
        public DateTime MeasurementDate { get; set; }

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
        /// Gets or sets the breast skinfold bodyMeasurement in millimeters or inches, depending on the application or database configuration.
        /// </summary>
        public float? FatTongBreastCrease { get; set; }

        /// <summary>
        /// Gets or sets the armpit skinfold bodyMeasurement in millimeters or inches, depending on the application or database configuration.
        /// </summary>
        public float? FatTongArmpitCrease { get; set; }

        /// <summary>
        /// Gets or sets the abdominal skinfold bodyMeasurement in millimeters or inches, depending on the application or database configuration.
        /// </summary>
        public float? FatTongAbdominalCrease { get; set; }

        /// <summary>
        /// Gets or sets the hip skinfold bodyMeasurement in millimeters or inches, depending on the application or database configuration.
        /// </summary>
        public float? FatTongHipCrease { get; set; }

        /// <summary>
        /// Gets or sets the thigh skinfold bodyMeasurement in millimeters or inches, depending on the application or database configuration.
        /// </summary>
        public float? FatTongThighCrease { get; set; }

        /// <summary>
        /// Gets or sets the back (subscapular) skinfold bodyMeasurement in millimeters or inches, depending on the application or database configuration.
        /// </summary>
        public float? FatTongBackCrease { get; set; }

        /// <summary>
        /// Gets or sets the triceps skinfold bodyMeasurement in millimeters or inches, depending on the application or database configuration.
        /// </summary>
        public float? FatTongTricepsCrease { get; set; }

    }
}
