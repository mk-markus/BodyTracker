using BodyTracker.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BodyTracker.Services
{
    public static class ChartTemplateService
    {

        /// <summary>
        /// Generates and configures chart series, X-axis, and Y-axes for body bodyMeasurement data visualization.
        /// </summary>
        /// <remarks>Filters and orders the bodyMeasurement data by the specified date range, calculates dynamic axis scaling limits for body weight, builds the series definitions incorporating trend lines and styling parameters, and constructs the final cartesian axes and series arrays.</remarks>
        /// <param name="startDate">The start date for filtering bodyMeasurement records.</param>
        /// <param name="endDate">The end date for filtering bodyMeasurement records.</param>
        /// <param name="data">The collection of full body bodyMeasurement models to be visualized.</param>
        /// <param name="isTrendLineLegendVisible">A value indicating whether trend lines are visible in the chart legend.</param>
        /// <param name="strokeThickness">The stroke thickness applied to the chart lines.</param>
        /// <param name="geometrySize">The point geometry size used for data markers.</param>
        /// <param name="loessFraction">The smoothing fraction parameter used for LOESS trend calculations.</param>
        /// <param name="showBodyWeight">A value indicating whether the body weight series is visible.</param>
        /// <param name="showBodyWeightTrend">A value indicating whether the body weight trend line is displayed.</param>
        /// <param name="showBodyFat">A value indicating whether the body fat percentage series is visible.</param>
        /// <param name="showBodyFatTrend">A value indicating whether the body fat trend line is displayed.</param>
        /// <param name="showBodyMuscle">A value indicating whether the body muscle percentage series is visible.</param>
        /// <param name="showBodyMuscleTrend">A value indicating whether the body muscle trend line is displayed.</param>
        /// <param name="showBodyWater">A value indicating whether the body water percentage series is visible.</param>
        /// <param name="showBodyWaterTrend">A value indicating whether the body water trend line is displayed.</param>
        /// <returns>A tuple containing the generated array of chart series, X-axes, and Y-axes.</returns>
        public static (ISeries[], ICartesianAxis[], ICartesianAxis[]) BodyMeasurementChart(DateTime startDate,
                                                                                            DateTime endDate,
                                                                                            List<FullBodyMeasurementDatasModel> data,
                                                                                            bool isTrendLineLegendVisible,
                                                                                            float strokeThickness, float geometrySize, double loessFraction,
                                                                                            bool showBodyWeight, bool showBodyWeightTrend,
                                                                                            bool showBodyFat, bool showBodyFatTrend,
                                                                                            bool showBodyMuscle, bool showBodyMuscleTrend,
                                                                                            bool showBodyWater, bool showBodyWaterTrend)
        {

            var ordered = data
                    .Where(d => d.MeasurementDate >= startDate &&
                                d.MeasurementDate <= endDate)
                    .OrderBy(d => d.MeasurementDate)
                    .ToList();

            var weightPoints = ordered
                .Where(d => d.BodyWeight.HasValue)
                .Select(d => new DateTimePoint(
                    d.MeasurementDate,
                    (double)d.BodyWeight!.Value))
                .ToArray();

            var maxWeight = weightPoints.Length > 0
                ? weightPoints.Max(p => p.Value)
                : 100;

            var minWeight = weightPoints.Length > 0
                ? weightPoints.Min(p => p.Value)
                : 0;

            var bufferMaxWeight = maxWeight * 0.005;
            var bufferMinWeight = minWeight * 0.01;

            var chartDefinitions = new[]
            {
            new ChartSeriesDefinition<FullBodyMeasurementDatasModel>
            {
                Name = "Body Weight (kg)",
                TrendName = "Body Weight Trend (kg)",
                Color = SKColors.BlueViolet,
                YAxisIndex = 0,

                IsVisible = showBodyWeight,
                ShowTrend = showBodyWeightTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.MeasurementDate,
                ValueSelector = x => (double?)x.BodyWeight
            },

            new ChartSeriesDefinition<FullBodyMeasurementDatasModel>
            {
                Name = "Body Water (%)",
                TrendName = "Body Water Trend (%)",
                Color = SKColors.DarkBlue,
                YAxisIndex = 1,

                IsVisible = showBodyWater,
                ShowTrend = showBodyWaterTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.MeasurementDate,
                ValueSelector = x => (double?)x.BodyWaterPercentage
            },

            new ChartSeriesDefinition<FullBodyMeasurementDatasModel>
            {
                Name = "Body Muscle (%)",
                TrendName = "Body Muscle Trend (%)",
                Color = SKColors.Green,
                YAxisIndex = 1,

                IsVisible = showBodyMuscle,
                ShowTrend = showBodyMuscleTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.MeasurementDate,
                ValueSelector = x => (double?)x.BodyMusclePercentage
            },

            new ChartSeriesDefinition<FullBodyMeasurementDatasModel>
            {
                Name = "Body Fat (%)",
                TrendName = "Body Fat Trend (%)",
                Color = SKColors.Red,
                YAxisIndex = 1,

                IsVisible = showBodyFat,
                ShowTrend = showBodyFatTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.MeasurementDate,
                ValueSelector = x => (double?)x.BodyFatPercentage,

            }
        };

            var series = ChartSeriesBuilder.CreateSeries(ordered,
                                                         chartDefinitions,
                                                         loessFraction).ToArray();

            var XAxis = new ICartesianAxis[]
            {
                new DateTimeAxis(
                    TimeSpan.FromDays(1),
                    date => date.ToString("dd.MM.yyyy"))
                {
                    Name = "Date"
                }
            };


            var yAxisDefinitions = new[]
            {
                new ChartYAxisDefinition
                {
                    Name = "kg",
                    MinLimit = minWeight - bufferMinWeight,
                    MaxLimit = maxWeight + bufferMaxWeight,

                },

                new ChartYAxisDefinition
                {
                    Name = "%",
                    Position = AxisPosition.End,
                    ShowSeparatorLines = false
                }
        };

            var YAxes = yAxisDefinitions.Select(ChartAxisBuilder.Create).ToArray();

            return (series, XAxis, YAxes);
        }

        /// <summary>
        /// Generates and configures chart series, X-axis, and Y-axes for monthly exercise training volume data visualization.
        /// </summary>
        /// <remarks>Filters and orders the training volume data by the specified date range, builds series definitions incorporating volume and workout counts with trend lines, and constructs the final cartesian axes and series arrays.</remarks>
        /// <param name="startDate">The start date for filtering training volume records.</param>
        /// <param name="endDate">The end date for filtering training volume records.</param>
        /// <param name="data">The collection of monthly exercise training volume models to be visualized.</param>
        /// <param name="isTrendLineLegendVisible">A value indicating whether trend lines are visible in the chart legend.</param>
        /// <param name="strokeThickness">The stroke thickness applied to the chart lines.</param>
        /// <param name="geometrySize">The point geometry size used for data markers.</param>
        /// <param name="loessFraction">The smoothing fraction parameter used for LOESS trend calculations.</param>
        /// <param name="showMonthlyTraniningsVolume">A value indicating whether the training volume series is visible.</param>
        /// <param name="showMonthlyTraniningsVolumeTrend">A value indicating whether the training volume trend line is displayed.</param>
        /// <returns>A tuple containing the generated array of chart series, X-axes, and Y-axes.</returns>
        public static (ISeries[], ICartesianAxis[], ICartesianAxis[]) MonthlyOverviewExerciseTraingingsVolume(DateTime startDate,
                                                                                                           DateTime endDate,
                                                                                                           List<MonthlyExerciseTraningVolume> data,
                                                                                                           bool isTrendLineLegendVisible,
                                                                                                           float strokeThickness, float geometrySize, double loessFraction,
                                                                                                           bool showMonthlyTraniningsVolume, bool showMonthlyTraniningsVolumeTrend)
        {

            var ordered = data
                      .Where(d => d.Date >= startDate &&
                                  d.Date <= endDate)
                      .OrderBy(d => d.Date)
                      .ToList();

            var weightPoints = ordered
                   .Where(d => d.TotalWeight.HasValue)
                   .Select(d => new DateTimePoint(
                       d.Date,
                       (double)d.TotalWeight!.Value))
                   .ToArray();

            var maxWeight = weightPoints.Length > 0
                ? weightPoints.Max(p => p.Value)
                : 100;

            var minWeight = weightPoints.Length > 0
                ? weightPoints.Min(p => p.Value)
                : 0;

            var bufferMaxWeight = maxWeight * 0.005;
            var bufferMinWeight = minWeight * 0.01;

            var chartDefinitions = new[]
            {
            new ChartSeriesDefinition<MonthlyExerciseTraningVolume>
            {
                Name = "Volume (kg)",
                TrendName = "Volume Trend (kg)",
                Color = SKColors.BlueViolet,
                YAxisIndex = 0,

                IsVisible = showMonthlyTraniningsVolume,
                ShowTrend = showMonthlyTraniningsVolumeTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.Date,
                ValueSelector = x => x.TotalWeight
            },

             new ChartSeriesDefinition<MonthlyExerciseTraningVolume>
            {
                Name = "Monthly Workouts",
                TrendName = "Workouts Trend",
                Color = SKColors.GreenYellow,
                YAxisIndex = 1,

                IsVisible = showMonthlyTraniningsVolume,
                ShowTrend = showMonthlyTraniningsVolumeTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.Date,
                ValueSelector = x => (double?)x.TotalExercises
             }


        };

            var series = ChartSeriesBuilder.CreateSeries(ordered, chartDefinitions, loessFraction).ToArray();

            var XAxis = new ICartesianAxis[]
            {
                new DateTimeAxis(
                    TimeSpan.FromDays(1),
                    date => date.ToString("dd.MM.yyyy"))
                {
                    Name = "Date"
                }
            };


            var yAxisDefinitions = new[]
            {
                new ChartYAxisDefinition
                {
                    Name = "kg",
                },
                new ChartYAxisDefinition
                {
                    Name = "Workouts",
                    Position = AxisPosition.End,
                    ShowSeparatorLines = false
                }
            };

            var YAxes = yAxisDefinitions.Select(ChartAxisBuilder.Create).ToArray();

            return (series, XAxis, YAxes);
        }
    }
}
