using BodyTracker.Models;
using BodyTracker.Models.Calculation;
using BodyTracker.Models.FullBodyMeasurement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace BodyTracker.Services
{
    public static class BodyCalculationToolsService
    {
        /// <summary>
        /// Calculates the average values for body measurements and skinfold data within a specified date range.
        /// </summary>
        /// <remarks>Filters the provided bodyMeasurement collection by the given start and end dates, calculates filtered averages for all physical metrics and skinfold measurements, and returns the aggregated result as an observable collection containing a single summary model.</remarks>
        /// <param name="start">The start date of the evaluation period (inclusive).</param>
        /// <param name="end">The end date of the evaluation period (inclusive).</param>
        /// <param name="data">The collection of full body bodyMeasurement records to be evaluated.</param>
        /// <returns>An <see cref="ObservableCollection{T}"/> containing a single <see cref="FullBodyMeasurementDatasModel"/> with the averaged values, or an empty collection if no valid data is found.</returns>
        public static ObservableCollection<FullBodyMeasurementDatasModel> GetAverageValues(
            DateTime start,
            DateTime end,
            ObservableCollection<FullBodyMeasurementDatasModel> data)
        {
            // Basic validation: Ensure date range is valid and data source exists
            if (start > end || data == null)
                return new ObservableCollection<FullBodyMeasurementDatasModel>();

            // Filter data by the specified date range
            var ordered = data
                .Where(d => d.MeasurementDate >= start && d.MeasurementDate <= end)
                .ToList();

            if (ordered.Count == 0)
                return new ObservableCollection<FullBodyMeasurementDatasModel>();

            // Wir berechnen den KFA für jeden Tag einzeln und bilden dann den Durchschnitt
            var kfaValues = ordered
                .Select(d => CalculateBodyFat7Point(d, true, 36))
                .Where(kfa => kfa > 0)
                .ToList();


            var result = new FullBodyMeasurementDatasModel();

            // Applying the filtered average logic to each bodyMeasurement field
            result.BodyWeight = GetFilteredAverage(ordered.Select(x => x.BodyWeight));
            result.BMI = GetFilteredAverage(ordered.Select(x => x.BMI));
            result.BodyFatPercentage = GetFilteredAverage(ordered.Select(x => x.BodyFatPercentage));
            result.BodyMusclePercentage = GetFilteredAverage(ordered.Select(x => x.BodyMusclePercentage));
            result.BodyWaterPercentage = GetFilteredAverage(ordered.Select(x => x.BodyWaterPercentage));
            result.ChestCircumference = GetFilteredAverage(ordered.Select(x => x.ChestCircumference));
            result.WaistCircumference = GetFilteredAverage(ordered.Select(x => x.WaistCircumference));
            result.HipsCircumference = GetFilteredAverage(ordered.Select(x => x.HipsCircumference));
            result.FatTongBreastCrease = GetFilteredAverage(ordered.Select(x => x.FatTongBreastCrease));
            result.FatTongArmpitCrease = GetFilteredAverage(ordered.Select(x => x.FatTongArmpitCrease));
            result.FatTongAbdominalCrease = GetFilteredAverage(ordered.Select(x => x.FatTongAbdominalCrease));
            result.FatTongHipCrease = GetFilteredAverage(ordered.Select(x => x.FatTongHipCrease));
            result.FatTongThighCrease = GetFilteredAverage(ordered.Select(x => x.FatTongThighCrease));
            result.FatTongBackCrease = GetFilteredAverage(ordered.Select(x => x.FatTongBackCrease));
            result.CaliperBodyFatPercentage = kfaValues.Any() ? kfaValues.Average() : 0f;
            result.FFM_kg = GetFilteredAverage(ordered.Select(x => x.FFM_kg));

            return new ObservableCollection<FullBodyMeasurementDatasModel> { result };
        }

        /// <summary>
        /// Calculates weekly average body metrics grouped by ISO calendar weeks within a specified date range.
        /// </summary>
        /// <param name="start">The start date of the filtering window (inclusive).</param>
        /// <param name="end">The end date of the filtering window (inclusive).</param>
        /// <param name="data">The collection of body metric records to analyze.</param>
        /// <returns>
        /// A list of <see cref="WeeklyAverageBodyMetricModel"/> objects containing aggregated weekly averages 
        /// and date bounds, ordered chronologically by ISO year and week.
        /// </returns>
        public static List<WeeklyAverageBodyMetricModel> GetWeeklyAverageValues(
            DateTime start,
            DateTime end,
            List<BodyMetricModel> data)
        {
            if (start > end || data == null) return new List<WeeklyAverageBodyMetricModel>();

            var filtered = data
                .Where(d => d.MeasurementDate >= start &&
                            d.MeasurementDate <= end)
                .ToList();

            if (!filtered.Any()) return new List<WeeklyAverageBodyMetricModel>();

            var weeklyResults = filtered
                .GroupBy(x => new
                {
                    Year = ISOWeek.GetYear(x.MeasurementDate),
                    Week = ISOWeek.GetWeekOfYear(x.MeasurementDate)
                })
                .OrderBy(x => x.Key.Year)
                .ThenBy(x => x.Key.Week)
                .Select(group =>
                {
                    var measurements = group.ToList();

                    var firstDate = measurements.Min(x => x.MeasurementDate);

                    return new WeeklyAverageBodyMetricModel
                    {
                        CalendarWeek = group.Key.Week,

                        WeekStartDate = measurements.Min(x => x.MeasurementDate),

                        WeekEndDate = measurements.Max(x => x.MeasurementDate),

                        AverageValues = new BodyMetricModel
                        {
                            MeasurementDate = firstDate,

                            BodyWeight = GetFilteredAverage(measurements.Select(x => x.BodyWeight)),

                            BMI = GetFilteredAverage(measurements.Select(x => x.BMI)),

                            BodyFatPercentage = GetFilteredAverage(measurements.Select(x => x.BodyFatPercentage)),

                            BodyMusclePercentage = GetFilteredAverage(measurements.Select(x => x.BodyMusclePercentage)),

                            BodyWaterPercentage = GetFilteredAverage(measurements.Select(x => x.BodyWaterPercentage))
                        }
                    };
                });

            return new List<WeeklyAverageBodyMetricModel>(weeklyResults);
        }

        /// <summary>
        /// Calculates the filtered arithmetic mean from a sequence of nullable float values.
        /// </summary>
        /// <remarks>Filters out null entries and values less than or equal to zero to prevent distorted calculations from unrecorded data entries.</remarks>
        /// <param name="source">The sequence of nullable float values to process.</param>
        /// <returns>The calculated average as a float, or <c>0f</c> if no valid values exist in the sequence.</returns>
        public static float GetFilteredAverage(IEnumerable<float?> source)
        {
            // Only include values that have a value and are greater than 0
            var validValues = source.Where(v => v.HasValue && v.Value > 0).ToList();

            // Return average if data exists, otherwise return 0 to avoid DivisionByZero
            return validValues.Any() ? validValues.Average(v => v.Value) : 0f;
        }

        /// <summary>
        /// Calculates the body fat percentage (BFP) using the Jackson & Pollock 7-site caliper method.
        /// The function determines body density based on gender and age, then converts it 
        /// into a percentage value using the Siri equation.
        /// </summary>
        /// <param name="d">The object containing the 7 skinfold bodyMeasurement values.</param>
        /// <param name="male">Specifies whether the calculation is for a male (true) or female (false).</param>
        /// <param name="age">The age of the individual in years.</param>
        /// <returns>The calculated body fat percentage; returns 0 if any bodyMeasurement values are missing or invalid.</returns>
        public static float CalculateBodyFat7Point(FullBodyMeasurementDatasModel d, bool male, int age)
        {
            // Collect the 7 values for this specific day
            float[] values = {
                         d.FatTongBreastCrease ?? 0, d.FatTongArmpitCrease ?? 0,
                         d.FatTongAbdominalCrease ?? 0, d.FatTongHipCrease ?? 0,
                         d.FatTongThighCrease ?? 0, d.FatTongBackCrease ?? 0,
                         d.FatTongTricepsCrease ?? 0};

            // If any value is missing (0), the bodyMeasurement for this day is considered invalid
            if (values.Any(v => v <= 0)) return 0f;

            float S = values.Sum();
            float d_density;

            // Jackson & Pollock formulas
            if (male)
            {
                d_density = 1.112f - (0.00043499f * S) + (0.00000055f * (S * S)) - (0.00028826f * age);
            }
            else
            {
                // Correct coefficients for women
                d_density = 1.097f - (0.00046971f * S) + (0.00000056f * (S * S)) - (0.00012828f * age);
            }

            // Siri equation to convert density into body fat percentage
            return (495f / d_density) - 450f;
        }

        /// <summary>
        /// Calculates the Body Mass Index (BMI) based on the specified weight and height.
        /// </summary>
        /// <param name="weight">The weight of the individual, in kilograms.</param>
        /// <param name="height">The height of the individual, in meters. Must be greater than zero.</param>
        /// <returns>The calculated BMI value as a floating-point number.</returns>
        public static float CalculateBmi(float weight, float height)
        {
            if (height <= 0) MessageBox.Show("Height must be greater than zero.", nameof(height), MessageBoxButton.OK, MessageBoxImage.Error);
            return weight / (height * height);
        }


        /// <summary>
        /// Calculates the absolute fat-free mass (FFM) in kilograms based on total body weight and body fat percentage.
        /// </summary>
        /// <param name="weight">The total body weight in kilograms.</param>
        /// <param name="kfa">The body fat percentage represented as a decimal fraction (e.g., 0.15 for 15%).</param>
        /// <returns>The calculated fat-free mass in kilograms.</returns>
        public static float CalculateFFM(float weight, float kfa)
        {
            return weight - (weight * (kfa / 100));
        }

        /// <summary>
        /// Calculates the Fat-Free Mass Index (FFMI) using the absolute fat-free mass and height.
        /// </summary>
        /// <param name="ffm_kg">The absolute fat-free mass in kilograms.</param>
        /// <param name="height">The body height in meters.</param>
        /// <returns>The calculated FFMI value.</returns>
        public static float GetFFMIndex(float ffm_kg, float height)
        {
            return ffm_kg / (height * height);
        }

        /// <summary>
        /// Evaluates the FFMI value against gender-specific thresholds to return a descriptive classification.
        /// </summary>
        /// <param name="iFFM_Index">The calculated Fat-Free Mass Index (FFMI) value.</param>
        /// <param name="gender">The gender model used to apply gender-specific classification thresholds.</param>
        /// <returns>A descriptive string classification of the FFMI value, or an empty string if no condition matches.</returns>
        public static string GetFFMIndexDescribing(float iFFM_Index, BodyCalculationGenderModel.Gender gender)
        {

            if (gender == BodyCalculationGenderModel.Gender.Male)
            {
                if (iFFM_Index < 18) return "Untrained";
                else if (iFFM_Index >= 18 && iFFM_Index <= 20) return "Average";
                else if (iFFM_Index > 20 && iFFM_Index <= 21) return "Well-trained";
                else if (iFFM_Index > 21 && iFFM_Index <= 23) return "Athletic / Advanced";
                else if (iFFM_Index > 23) return "Natural Limit";
            }

            if (gender == BodyCalculationGenderModel.Gender.Female)
            {
                if (iFFM_Index < 15) return "Untrained";
                else if (iFFM_Index >= 15 && iFFM_Index <= 16) return "Average";
                else if (iFFM_Index > 16 && iFFM_Index <= 19) return "Well-trained";
                else if (iFFM_Index > 19 && iFFM_Index <= 21) return "Very Muscular (Elite)";
                else if (iFFM_Index > 21) return "Limit / Enhanced";
            }

            return string.Empty;
        }
    }
}
