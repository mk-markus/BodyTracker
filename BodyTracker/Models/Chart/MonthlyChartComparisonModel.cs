using System.Windows.Controls;

namespace BodyTracker.Models
{
    public class MonthlyChartComparisonModel
    {

        /// <summary>
        /// Gets or sets the UI text block indicating the body weight trend direction.
        /// </summary>
        /// <remarks>Displays a directional arrow symbol representing whether body weight has increased, decreased, or remained stable compared to the previous bodyMeasurement period.</remarks>
        public TextBlock BodyWeightTrendArrow;

        /// <summary>
        /// Gets or sets the formatted string representing the absolute difference in body weight.
        /// </summary>
        /// <remarks>Displays the net change value with units for quick overview representation in the UI.</remarks>
        public string BodyWeightDifference;

        /// <summary>
        /// Gets or sets the numerical value for the previous period's bar representation of body weight.
        /// </summary>
        /// <remarks>Used for scaling and rendering comparative bar chart elements in the dashboard view.</remarks>
        public double BodyWeightPreviousBarValue;

        /// <summary>
        /// Gets or sets the numerical value for the current period's bar representation of body weight.
        /// </summary>
        /// <remarks>Used for scaling and rendering comparative bar chart elements in the dashboard view.</remarks>
        public double BodyWeightCurrentBarValue;

        /// <summary>
        /// Gets or sets the maximum reference value used to scale the body weight chart bars.
        /// </summary>
        /// <remarks>Ensures proper proportional scaling of comparative visual bars within the container.</remarks>
        public double BodyWeightCurrentMaxValue;

        /// <summary>
        /// Gets or sets the UI text block indicating the body fat trend direction.
        /// </summary>
        /// <remarks>Displays a directional arrow symbol representing whether body fat percentage has increased, decreased, or remained stable compared to the previous bodyMeasurement period.</remarks>
        public TextBlock BodyFatTrendArrow;

        /// <summary>
        /// Gets or sets the formatted string representing the absolute difference in body fat percentage.
        /// </summary>
        /// <remarks>Displays the net change value with units for quick overview representation in the UI.</remarks>
        public string BodyFatDifference;

        /// <summary>
        /// Gets or sets the numerical value for the previous period's bar representation of body fat.
        /// </summary>
        /// <remarks>Used for scaling and rendering comparative bar chart elements in the dashboard view.</remarks>
        public double BodyFatPreviousBarValue;

        /// <summary>
        /// Gets or sets the numerical value for the current period's bar representation of body fat.
        /// </summary>
        /// <remarks>Used for scaling and rendering comparative bar chart elements in the dashboard view.</remarks>
        public double BodyFatCurrentBarValue;

        /// <summary>
        /// Gets or sets the maximum reference value used to scale the body fat chart bars.
        /// </summary>
        /// <remarks>Ensures proper proportional scaling of comparative visual bars within the container.</remarks>
        public double BodyFatCurrentMaxValue;

        /// <summary>
        /// Gets or sets the UI text block indicating the body muscle mass trend direction.
        /// </summary>
        /// <remarks>Displays a directional arrow symbol representing whether muscle mass has increased, decreased, or remained stable compared to the previous bodyMeasurement period.</remarks>
        public TextBlock BodyMuscleMassTrendArrow;

        /// <summary>
        /// Gets or sets the formatted string representing the absolute difference in body muscle mass.
        /// </summary>
        /// <remarks>Displays the net change value with units for quick overview representation in the UI.</remarks>
        public string BodyMuscleMassDifference;

        /// <summary>
        /// Gets or sets the numerical value for the previous period's bar representation of body muscle mass.
        /// </summary>
        /// <remarks>Used for scaling and rendering comparative bar chart elements in the dashboard view.</remarks>
        public double BodyMuscleMassPreviousBarValue;

        /// <summary>
        /// Gets or sets the numerical value for the current period's bar representation of body muscle mass.
        /// </summary>
        /// <remarks>Used for scaling and rendering comparative bar chart elements in the dashboard view.</remarks>
        public double BodyMuscleMassCurrentBarValue;

        /// <summary>
        /// Gets or sets the maximum reference value used to scale the body muscle mass chart bars.
        /// </summary>
        /// <remarks>Ensures proper proportional scaling of comparative visual bars within the container.</remarks>
        public double BodyMuscleMassCurrentMaxValue;

        /// <summary>
        /// Gets or sets the UI text block indicating the body water trend direction.
        /// </summary>
        /// <remarks>Displays a directional arrow symbol representing whether body water percentage has increased, decreased, or remained stable compared to the previous bodyMeasurement period.</remarks>
        public TextBlock BodyWaterTrendArrow;

        /// <summary>
        /// Gets or sets the formatted string representing the absolute difference in body water percentage.
        /// </summary>
        /// <remarks>Displays the net change value with units for quick overview representation in the UI.</remarks>
        public string BodyWaterDifference;

        /// <summary>
        /// Gets or sets the numerical value for the previous period's bar representation of body water.
        /// </summary>
        /// <remarks>Used for scaling and rendering comparative bar chart elements in the dashboard view.</remarks>
        public double BodyWaterPreviousBarValue;

        /// <summary>
        /// Gets or sets the numerical value for the current period's bar representation of body water.
        /// </summary>
        /// <remarks>Used for scaling and rendering comparative bar chart elements in the dashboard view.</remarks>
        public double BodyWaterCurrentBarValue;

        /// <summary>
        /// Gets or sets the maximum reference value used to scale the body water chart bars.
        /// </summary>
        /// <remarks>Ensures proper proportional scaling of comparative visual bars within the container.</remarks>
        public double BodyWaterCurrentMaxValue;

        /// <summary>
        /// Gets or sets the UI text block indicating the body mass index (BMI) trend direction.
        /// </summary>
        /// <remarks>Displays a directional arrow symbol representing whether BMI has increased, decreased, or remained stable compared to the previous bodyMeasurement period.</remarks>
        public TextBlock BodyBmiTrendArrow;

        /// <summary>
        /// Gets or sets the formatted string representing the absolute difference in body mass index (BMI).
        /// </summary>
        /// <remarks>Displays the net change value with units for quick overview representation in the UI.</remarks>
        public string BodyBmiDifference;

        /// <summary>
        /// Gets or sets the numerical value for the previous period's bar representation of body mass index (BMI).
        /// </summary>
        /// <remarks>Used for scaling and rendering comparative bar chart elements in the dashboard view.</remarks>
        public double BodyBmiPreviousBarValue;

        /// <summary>
        /// Gets or sets the numerical value for the current period's bar representation of body mass index (BMI).
        /// </summary>
        /// <remarks>Used for scaling and rendering comparative bar chart elements in the dashboard view.</remarks>
        public double BodyBmiCurrentBarValue;

        /// <summary>
        /// Gets or sets the maximum reference value used to scale the body mass index (BMI) chart bars.
        /// </summary>
        /// <remarks>Ensures proper proportional scaling of comparative visual bars within the container.</remarks>
        public double BodyBmiCurrentMaxValue;

    }
}
