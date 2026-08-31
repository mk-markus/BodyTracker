using System;

namespace BodyTracker.Models
{

    /// <summary>
    /// Represents a data record exported from Samsung Health, typically containing 
    /// nutritional or health-related activity information.
    /// </summary>
    public class SamsungFoodIntakeModel
    {
        /// <summary>
        /// Contains the Food Intake ID from the database table
        /// </summary>
        public int? FoodIntakeID { get; set; }

        /// <summary>
        /// The version of the Samsung Health app when the record was created.
        /// </summary>
        public long CreateShVer { get; set; }

        /// <summary>
        /// The starting timestamp of the recorded activity or event.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The measured quantity of the recorded data (e.g., amount of food or count of steps).
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Optional custom data or user-defined fields.
        /// </summary>
        public string? Custom { get; set; }

        /// <summary>
        /// The version of the Samsung Health app when the record was last modified.
        /// </summary>
        public long ModifyShVer { get; set; }

        /// <summary>
        /// The timestamp indicating when the record was last updated.
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// The timestamp indicating when the record was initially created.
        /// </summary>  
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// The category of the meal (e.g., breakfast, lunch, dinner).
        /// </summary>
        public int MealType { get; set; }

        /// <summary>
        /// A unique identifier for the data point within the client database.
        /// </summary>
        public string ClientDataId { get; set; } = string.Empty;

        /// <summary>
        /// The descriptive name associated with the data entry.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The unit of bodyMeasurement used for the 'Amount' field.
        /// </summary>
        public long Unit { get; set; }

        /// <summary>
        /// The version identifier of the client data structure.
        /// </summary>
        public string? ClientDataVer { get; set; }

        /// <summary
        /// >The caloric value associated with this record.
        /// </summary>
        public double Calorie { get; set; }

        /// <summary>
        /// The time zone offset applied to the record's timestamps.
        /// </summary>
        public string TimeOffset { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier of the device that recorded this data.
        /// </summary>
        public string DeviceUuid { get; set; } = string.Empty;

        /// <summary>
        /// Optional comments or notes attached to the record.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>The package name of the application that generated the data.
        /// </summary>
        public string PkgName { get; set; } = string.Empty;

        /// <summary>
        /// The globally unique identifier for this specific data record.
        /// </summary>
        public string? DataUuid { get; set; }

        /// <summary>
        /// The reference ID for specific food information in the database.
        /// </summary>
        public string? FoodInfoId { get; set; }
    }

}
