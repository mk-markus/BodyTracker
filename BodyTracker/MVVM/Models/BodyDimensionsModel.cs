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
        /// Contains the value of the Fat Tongs in cm or inch, depends on the structure of the database table or the programm structure.
        /// </summary>
        public float? FatTongs { get; set; }

    }
}
