using BodyTracker.Models.Calculation;
using System;

/// <summary>
/// Represents a comprehensive data model for nutritional information imported or synchronized from Samsung Health services.
/// </summary>
public record SamsungFoodNutritionModel
{

    /// <summary>
    /// Contains the Food Nutrition ID from the database table
    /// </summary>
    public int? FoodNutritionID { get; set; }

    /// <summary>
    /// Gets or sets the magnesium content (in milligrams).
    /// </summary>
    public double? Magnesium { get; set; }

    /// <summary>
    /// Gets or sets the potassium content (in milligrams).
    /// </summary>
    public double? Potassium { get; set; }

    /// <summary>
    /// Gets or sets the schema version or creation version string for the data source.
    /// </summary>
    public string? CreateShVer { get; set; }

    /// <summary>
    /// Gets or sets the Vitamin A content.
    /// </summary>
    public double? VitaminA { get; set; }

    /// <summary>
    /// Gets or sets the Vitamin C content (in milligrams).
    /// </summary>
    public double? VitaminC { get; set; }

    /// <summary>
    /// Gets or sets the Vitamin D content.
    /// </summary>
    public double? VitaminD { get; set; }

    /// <summary>
    /// Gets or sets the Vitamin E content.
    /// </summary>
    public double? VitaminE { get; set; }

    /// <summary>
    /// Gets or sets the Vitamin K content (in micrograms).
    /// </summary>
    public double? VitaminK { get; set; }

    /// <summary>
    /// Gets or sets the cholesterol content (in milligrams).
    /// </summary>
    public double? Cholesterol { get; set; }

    /// <summary>
    /// Gets or sets the Vitamin B12 content (in micrograms).
    /// </summary>
    public double? VitaminB12 { get; set; }

    /// <summary>
    /// Gets or sets the Unix timestamp (in milliseconds) representing the start time of the meal or nutrition entry.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the biotin content.
    /// </summary>
    public double? Biotin { get; set; }

    /// <summary>
    /// Gets or sets the phosphorus content (in milligrams).
    /// </summary>
    public double? Phosphorus { get; set; }

    /// <summary>
    /// Gets or sets the copper content (in milligrams).
    /// </summary>
    public double? Copper { get; set; }

    /// <summary>
    /// Gets or sets custom metadata or user-defined fields.
    /// </summary>
    public string? Custom { get; set; }

    /// <summary>
    /// Gets or sets the thiamin (Vitamin B1) content (in milligrams).
    /// </summary>
    public double? Thiamin { get; set; }

    /// <summary>
    /// Gets or sets the folate/folic acid content (in micrograms).
    /// </summary>
    public double? Folate { get; set; }

    /// <summary>
    /// Gets or sets the iodine content (in micrograms).
    /// </summary>
    public double? Iodine { get; set; }

    /// <summary>
    /// Gets or sets the niacin (Vitamin B3) content (in milligrams).
    /// </summary>
    public double? Niacin { get; set; }

    /// <summary>
    /// Gets or sets the sodium content (in milligrams).
    /// </summary>
    public double? Sodium { get; set; }

    /// <summary>
    /// Gets or sets the molybdenum content.
    /// </summary>
    public double? Molybdenum { get; set; }

    /// <summary>
    /// Gets or sets the synthetic folic acid content (in micrograms).
    /// </summary>
    public double? FolicAcid { get; set; }

    /// <summary>
    /// Gets or sets the modification version string for the data record.
    /// </summary>
    public string? ModifyShVer { get; set; }

    /// <summary>
    /// Gets or sets the dietary fiber content (in grams).
    /// </summary>
    public double? DietaryFiber { get; set; }

    /// <summary>
    /// Gets or sets the total fat content (in grams).
    /// </summary>
    public double? TotalFat { get; set; }

    /// <summary>
    /// Gets or sets the Unix timestamp (in milliseconds) of the last update.
    /// </summary>
    public DateTime UpdateTime { get; set; }

    /// <summary>
    /// Gets or sets the Unix timestamp (in milliseconds) of the record creation.
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// Gets or sets the type of meal (e.g., breakfast, lunch, dinner, snack).
    /// </summary>
    public int? MealType { get; set; }

    /// <summary>
    /// Gets or sets the caffeine content (in milligrams).
    /// </summary>
    public double? Caffeine { get; set; }

    /// <summary>
    /// Gets or sets the monounsaturated fat content (in grams).
    /// </summary>
    public double? MonosaturatedFat { get; set; }

    /// <summary>
    /// Gets or sets the protein content (in grams).
    /// </summary>
    public double? Protein { get; set; }

    /// <summary>
    /// Gets or sets the manganese content (in milligrams).
    /// </summary>
    public double? Manganese { get; set; }

    /// <summary>
    /// Gets or sets the polyunsaturated fat content (in grams). 
    /// Note: This is an alternate spelling property duplicating <see cref="PolyunsaturatedFat"/>.
    /// </summary>
    public double? PolysaturatedFat { get; set; }

    /// <summary>
    /// Gets or sets the iron content (in milligrams).
    /// </summary>
    public double? Iron { get; set; }

    /// <summary>
    /// Gets or sets the zinc content (in milligrams).
    /// </summary>
    public double? Zinc { get; set; }

    /// <summary>
    /// Gets or sets the riboflavin (Vitamin B2) content (in milligrams).
    /// </summary>
    public double? Riboflavin { get; set; }

    /// <summary>
    /// Gets or sets the sugar content (in grams).
    /// </summary>
    public double? Sugar { get; set; }

    /// <summary>
    /// Gets or sets the title or food item name of the nutrition entry.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the added sugar content (in grams).
    /// </summary>
    public double? AddedSugar { get; set; }

    /// <summary>
    /// Gets or sets the calories derived from fat.
    /// </summary>
    public double? CaloriesFromFat { get; set; }

    /// <summary>
    /// Gets or sets the calcium content (in milligrams).
    /// </summary>
    public double? Calcium { get; set; }

    /// <summary>
    /// Gets or sets the total energy value in calories (kcal).
    /// </summary>
    public double? Calorie { get; set; }

    /// <summary>
    /// Gets or sets the time offset from UTC in milliseconds.
    /// </summary>
    public string? TimeOffset { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the recording device.
    /// </summary>
    public string? DeviceUuid { get; set; }

    /// <summary>
    /// Gets or sets the polyunsaturated fat content (in grams).
    /// </summary>
    public double? PolyunsaturatedFat { get; set; }

    /// <summary>
    /// Gets or sets the saturated fat content (in grams).
    /// </summary>
    public double? SaturatedFat { get; set; }

    /// <summary>
    /// Gets or sets the trans fat content (in grams).
    /// </summary>
    public double? TransFat { get; set; }

    /// <summary>
    /// Gets or sets the package name of the application that created or synced the data.
    /// </summary>
    public string? PkgName { get; set; }

    /// <summary>
    /// Gets or sets the selenium content (in micrograms).
    /// </name>
    public double? Selenium { get; set; }

    /// <summary>
    /// Gets or sets the Vitamin B6 content (in milligrams).
    /// </summary>
    public double? VitaminB6 { get; set; }

    /// <summary>
    /// Gets or sets the total carbohydrate content (in grams).
    /// </summary>
    public double? Carbohydrate { get; set; }

    /// <summary>
    /// Gets or sets the total unsaturated fat content (in grams).
    /// </summary>
    public double? UnsaturatedFat { get; set; }

    /// <summary>
    /// Gets or sets the chloride content.
    /// </summary>
    public double? Chloride { get; set; }

    /// <summary>
    /// Gets or sets the pantothenic acid (Vitamin B5) content (in milligrams).
    /// </summary>
    public double? PantothenicAcid { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the data entry record.
    /// </summary>
    public string DataUuid { get; set; }

    /// <summary>
    /// Gets or sets the chromium content (in micrograms).
    /// </summary>
    public double? Chromium { get; set; }
}