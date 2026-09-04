using System;

namespace BodyTracker.Models
{
    /// <summary>
    /// Represents a Samsung Health food info record with all corresponding nutritional and metadata fields.
    /// </summary>
    public class SamsungFoodInfoModel
    {
        /// <summary>
        /// Gets or sets the unique exercise record identifier.
        /// </summary>
        public int? FoodInfoID { get; set; }

        /// <summary>
        /// Potassium content.
        /// </summary>
        public double? Potassium { get; set; }

        /// <summary>
        /// Vitamin A content.
        /// </summary>
        public double? VitaminA { get; set; }

        /// <summary>
        /// Vitamin C content.
        /// </summary>
        public double? VitaminC { get; set; }

        /// <summary>
        /// Vitamin D content.
        /// </summary>
        public double? VitaminD { get; set; }

        /// <summary>
        /// Cholesterol content.
        /// </summary>
        public double? Cholesterol { get; set; }

        /// <summary>
        /// Food description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Custom information.
        /// </summary>
        public string? Custom { get; set; }

        /// <summary>
        /// Unique identifier of the food provider.
        /// </summary>
        public string? ProviderFoodId { get; set; }

        /// <summary>
        /// Metric serving amount.
        /// </summary>
        public double? MetricServingAmount { get; set; }

        /// <summary>
        /// Sodium content.
        /// </summary>
        public double? Sodium { get; set; }

        /// <summary>
        /// Dietary fiber content.
        /// </summary>
        public double? DietaryFiber { get; set; }

        /// <summary>
        /// Total fat content.
        /// </summary>
        public double? TotalFat { get; set; }

        /// <summary>
        /// Update timestamp.
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// Creation timestamp.
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// Monounsaturated fat content.
        /// </summary>
        public double? MonosaturatedFat { get; set; }

        /// <summary>
        /// Protein content.
        /// </summary>
        public double? Protein { get; set; }

        /// <summary>
        /// Polysaturated fat content.
        /// </summary>
        public double? PolysaturatedFat { get; set; }

        /// <summary>
        /// Iron content.
        /// </summary>
        public double? Iron { get; set; }

        /// <summary>
        /// Food name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Sugar content.
        /// </summary>
        public double? Sugar { get; set; }

        /// <summary>
        /// Added sugar content.
        /// </summary>
        public double? AddedSugar { get; set; }

        /// <summary>
        /// Calcium content.
        /// </summary>
        public double? Calcium { get; set; }

        /// <summary>
        /// Calorie count.
        /// </summary>
        public double? Calorie { get; set; }

        /// <summary>
        /// Serving description.
        /// </summary>
        public string? ServingDescription { get; set; }

        /// <summary>
        /// Information provider.
        /// </summary>
        public string? InfoProvider { get; set; }

        /// <summary>
        /// Device UUID.
        /// </summary>
        public string? DeviceUuid { get; set; }

        /// <summary>
        /// Metric serving unit.
        /// </summary>
        public string? MetricServingUnit { get; set; }

        /// <summary>
        /// Saturated fat content.
        /// </summary>
        public double? SaturatedFat { get; set; }

        /// <summary>
        /// Trans fat content.
        /// </summary>
        public double? TransFat { get; set; }

        /// <summary>
        /// Package name.
        /// </summary>
        public string? PkgName { get; set; }

        /// <summary>
        /// Carbohydrate content.
        /// </summary>
        public double? Carbohydrate { get; set; }

        /// <summary>
        /// Unit count per calorie.
        /// </summary>
        public double? UnitCountPerCalorie { get; set; }

        /// <summary>
        /// Data UUID.
        /// </summary>
        public string? DataUuid { get; set; }

        /// <summary>
        /// Default number of serving units.
        /// </summary>
        public double? DefaultNumberOfServingUnit { get; set; }
    }
}