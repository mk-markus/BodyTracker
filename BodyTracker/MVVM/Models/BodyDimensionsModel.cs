using System;

namespace BodyTracker.MVVM.Models
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
        /// Cotnains the the date of the measurement entry --> When the measurement is entered into the database table.
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

    }
}
