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
        public static (ISeries[], ICartesianAxis[], ICartesianAxis[]) CreateBodyMeasurementChart(  DateTime startDate,
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
            new ChartSeriesModel<FullBodyMeasurementDatasModel>
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

            new ChartSeriesModel<FullBodyMeasurementDatasModel>
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

            new ChartSeriesModel<FullBodyMeasurementDatasModel>
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

            new ChartSeriesModel<FullBodyMeasurementDatasModel>
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
                new ChartYAxisModel
                {
                    Name = "kg",
                    MinLimit = minWeight - bufferMinWeight,
                    MaxLimit = maxWeight + bufferMaxWeight,

                },

                new ChartYAxisModel
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
        /// Generates LiveCharts Cartesian series and axes configurations for daily step counts, distances, and burned calories over a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the filtering range.</param>
        /// <param name="endDate">The end date of the filtering range.</param>
        /// <param name="data">The collection of step daily trend chart models containing step counts, distances, and calorie metrics.</param>
        /// <param name="isTrendLineLegendVisible">A value indicating whether trend lines should appear in the chart legend.</param>
        /// <param name="strokeThickness">The stroke thickness for the chart lines.</param>
        /// <param name="geometrySize">The size of the data point geometries on the chart.</param>
        /// <param name="loessFraction">The LOESS smoothing fraction used for calculating trend lines.</param>
        /// <param name="showSteps">A value indicating whether the daily steps series is visible.</param>
        /// <param name="showStepsTrend">A value indicating whether the daily steps trend line is visible.</param>
        /// <param name="showDistance">A value indicating whether the daily distance series is visible.</param>
        /// <param name="showDistanceTrend">A value indicating whether the daily distance trend line is visible.</param>
        /// <param name="showCalories">A value indicating whether the daily burned calories series is visible.</param>
        /// <param name="showCaloriesTrend">A value indicating whether the daily burned calories trend line is visible.</param>
        /// <returns>A tuple containing the generated series array, X-axis array, and Y-axes array.</returns>
        public static (ISeries[], ICartesianAxis[], ICartesianAxis[]) CreateDailyActivityChart( DateTime startDate,
                                                                                                DateTime endDate,
                                                                                                List<StepDailyTrendChartModel> data,
                                                                                                bool isTrendLineLegendVisible,
                                                                                                float strokeThickness, float geometrySize, double loessFraction,
                                                                                                bool showSteps, bool showStepsTrend,
                                                                                                bool showDistance, bool showDistanceTrend,
                                                                                                bool showCalories, bool showCaloriesTrend)
        {
            var ordered = data
                        .Where(x =>
                            x.SourceType == -2 &&
                            x.CreateTime >= startDate &&
                            x.CreateTime <= endDate)
                        .OrderBy(x => x.CreateTime)
                        .ToList();

            var stepPoints = ordered
               .Select(x => new DateTimePoint(
                   x.CreateTime,
                   x.Count))
               .ToArray();

            var distancePoints = ordered
               .Select(x => new DateTimePoint(
                   x.CreateTime,
                   x.Distance))
               .ToArray();

            var caloriePoints = ordered
                .Select(x => new DateTimePoint(
                    x.CreateTime,
                    x.Calorie))
                .ToArray();

            var maxSteps = stepPoints.Length > 0
                 ? stepPoints.Max(x => x.Value)
                 : 10000;

            var maxDistance = distancePoints.Length > 0
                ? distancePoints.Max(x => x.Value)
                : 10000;

            var maxCalories = caloriePoints.Length > 0
                ? caloriePoints.Max(x => x.Value)
                : 1000;

            var chartSeries = new List<ISeries>();

            var chartDefinitions = new[]
            {
                new ChartSeriesModel<StepDailyTrendChartModel>
                {
                    Name = "Daily Distance",
                    TrendName = "Daily Distance Trend",
                    Color = SKColors.BlueViolet,
                    YAxisIndex = 1,

                    IsVisible = showDistance,
                    ShowTrend = showDistanceTrend,
                    IsTrendLineVisible = isTrendLineLegendVisible,

                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.CreateTime,
                    ValueSelector = x => (double?)x.Distance
                },

                new ChartSeriesModel<StepDailyTrendChartModel>
                {
                    Name = "Daily Burned Calories",
                    TrendName = "Daily Burned Calories Trend",
                    Color = SKColors.Red,
                    YAxisIndex = 2,

                    IsVisible = showCalories,
                    ShowTrend = showCaloriesTrend,
                    IsTrendLineVisible = isTrendLineLegendVisible,

                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.CreateTime,
                    ValueSelector = x => (double?)x.Calorie
                },

                new ChartSeriesModel<StepDailyTrendChartModel>
                {
                    Name = "Daily Steps",
                    TrendName = "Daily Steps Trend",
                    Color = SKColors.Blue,
                    YAxisIndex = 0,

                    IsVisible = showSteps,
                    ShowTrend = showStepsTrend,
                    IsTrendLineVisible = isTrendLineLegendVisible,

                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.CreateTime,
                    ValueSelector = x => (double?)x.Count
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
            new ChartYAxisModel
            {
                Name = "Steps",
                MinLimit = 0,
                MaxLimit = maxSteps * 1.05
            },
            new ChartYAxisModel
            {
                Name = "Distance (m)",
                Position = AxisPosition.End,
                MinLimit = 0,
                MaxLimit = maxDistance * 1.05
            },
            new ChartYAxisModel
            {
                Name = "Calories (kcal)",
                Position = AxisPosition.End,
                MinLimit = 0,
                MaxLimit = maxCalories * 1.05
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
        public static (ISeries[], ICartesianAxis[], ICartesianAxis[]) CreateMonthlyVolumeChart( DateTime startDate,
                                                                                                DateTime endDate,
                                                                                                List<MonthlyExerciseTrainingVolumeModel> data,
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
            new ChartSeriesModel<MonthlyExerciseTrainingVolumeModel>
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

             new ChartSeriesModel<MonthlyExerciseTrainingVolumeModel>
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
                new ChartYAxisModel
                {
                    Name = "kg",
                },
                new ChartYAxisModel
                {
                    Name = "Workouts",
                    Position = AxisPosition.End,
                    ShowSeparatorLines = false
                }
            };

            var YAxes = yAxisDefinitions.Select(ChartAxisBuilder.Create).ToArray();

            return (series, XAxis, YAxes);
        }



        /// <summary>
        /// Generates LiveCharts spider chart series and polar axes configurations from a list of muscle distribution data results.
        /// </summary>
        /// <param name="results">The collection of muscle data results containing percentage shares and muscle group labels.</param>
        /// <param name="seriesName">The name assigned to the polar line series.</param>
        /// <param name="strokeThickness">The miniature stroke thickness for the series line. Default is 1.</param>
        /// <param name="geometrySize">The size of the data point geometries on the chart. Default is 1.</param>
        /// <param name="textSize">The font size for the axis labels. Default is 12.</param>
        /// <param name="minStep">The minimum step value for the axes. Default is 0.5.</param>
        /// <param name="labelRotation">The rotation angle for the axis labels. Default is 0.</param>
        /// <returns>A tuple containing the generated series array, angle axis array, and radius axis array.</returns>
        public static (ISeries[], IPolarAxis[], IPolarAxis[]) CreateMuscleSpiderChart( List<MuscleDataResultsModel> results, string seriesName,
                                                                                       float strokeThickness = 1, float geometrySize = 1,
                                                                                       int textSize = 12, double minStep = 0.5, int labelRotation = 0)
        {
            ISeries[] seriesValues = Array.Empty<ISeries>();

            IPolarAxis[] angleAxis = Array.Empty<IPolarAxis>();

            IPolarAxis[] radiusAxis = Array.Empty<IPolarAxis>();

            if (results.Any())
            {
                var muscleDistributionValues = new List<double>();
                var muscleDistributionSpiderChartAxisName = new List<string>();
                foreach (var value in results)
                {
                    if (value == null) break;
                    muscleDistributionValues.Add(value.PercentageShare);
                    muscleDistributionSpiderChartAxisName.Add(value.MuscleGroup);
                }

                seriesValues = new ISeries[]
                {
             new PolarLineSeries<double>
             {
                Name=seriesName,
                Values= muscleDistributionValues,
                IsClosed= true,
                GeometrySize = geometrySize,
                MiniatureStrokeThickness = strokeThickness
             }
                };

                angleAxis = new IPolarAxis[]
                {
                new PolarAxis
                {
                    Labels = muscleDistributionSpiderChartAxisName.ToArray(),
                    LabelsRotation = labelRotation,
                    TextSize= textSize,
                    MinStep = minStep,
                }
                };

                radiusAxis = new IPolarAxis[]
               {
                new PolarAxis
                {
                    MinLimit= 0,
                    MaxLimit = 20,
                    MinStep= minStep,
                    Labeler = _ => string.Empty
                }
             };

            }

            return (seriesValues, angleAxis, radiusAxis);
        }

        /// <summary>
        /// Generates LiveCharts spider chart series and polar axes configurations comparing current period and previous period muscle distribution data results.
        /// </summary>
        /// <param name="actResults">The collection of current period muscle data results containing percentage shares and muscle group labels.</param>
        /// <param name="prevResults">The collection of previous period muscle data results containing percentage shares.</param>
        /// <param name="actSereiesName">The name assigned to the current period polar line series.</param>
        /// <param name="prevSeriesName">The name assigned to the previous period polar line series.</param>
        /// <param name="strokeThickness">The miniature stroke thickness for the series lines. Default is 1.</param>
        /// <param name="geometrySize">The size of the data point geometries on the chart. Default is 1.</param>
        /// <param name="textSize">The font size for the axis labels. Default is 12.</param>
        /// <param name="minStep">The minimum step value for the axes. Default is 1.</param>
        /// <param name="labelRotation">The rotation angle for the axis labels. Default is 0.</param>
        /// <returns>A tuple containing the generated series array, angle axis array, and radius axis array.</returns>
        public static (ISeries[], IPolarAxis[], IPolarAxis[]) CreateMuscleSpiderChart( List<MuscleDataResultsModel> actResults, List<MuscleDataResultsModel> prevResults,
                                                                                       string actSereiesName, string prevSeriesName, float strokeThickness = 1,
                                                                                       float geometrySize = 1, int textSize = 12, int minStep = 1, int labelRotation = 0)
        {
            ISeries[] seriesValues = Array.Empty<ISeries>();

            IPolarAxis[] angleAxis = Array.Empty<IPolarAxis>();

            IPolarAxis[] radiusAxis = Array.Empty<IPolarAxis>();

            if (actResults.Any() && prevResults.Any())
            {
                var actMuscleDistributionSpiderChartValues = new List<double>();
                var actMuscleDistributionSpiderChartAxisName = new List<string>();
                var prevMuscleDistributionSpiderChartValues = new List<double>();


                foreach (var value in actResults)
                {
                    if (value == null) break;
                    actMuscleDistributionSpiderChartValues.Add(value.PercentageShare);
                    actMuscleDistributionSpiderChartAxisName.Add(value.MuscleGroup);
                }

                foreach (var value in prevResults)
                {
                    if (value == null) break;
                    prevMuscleDistributionSpiderChartValues.Add(value.PercentageShare);
                }


                seriesValues = new ISeries[]
                {
             new PolarLineSeries<double>
             {
              Name=actSereiesName,
              Values= actMuscleDistributionSpiderChartValues,
              IsClosed= true,
              GeometrySize = geometrySize,
              MiniatureStrokeThickness = strokeThickness
             },

              new PolarLineSeries<double>
             {
              Name=prevSeriesName,
              Values= prevMuscleDistributionSpiderChartValues,
              IsClosed= true,
              GeometrySize = geometrySize,
              MiniatureStrokeThickness = strokeThickness
             }
                };

                angleAxis = new IPolarAxis[]
                {
                new PolarAxis
                {
                    Labels = actMuscleDistributionSpiderChartAxisName.ToArray(),
                    LabelsRotation = labelRotation,
                    TextSize= textSize
                }
                };

                radiusAxis = new IPolarAxis[]
                {
                new PolarAxis
                {
                    MinLimit= 0,
                    MaxLimit = 20,
                    MinStep= minStep,
                    Labeler = _ => string.Empty
                }
                };
            }

            return (seriesValues, angleAxis, radiusAxis);
        }
    }
}
