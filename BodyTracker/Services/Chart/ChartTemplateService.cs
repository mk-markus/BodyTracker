using BodyTracker.Models;
using BodyTracker.Models.Chart;
using BodyTracker.Models.SamsungHealth;
using BodyTracker.Models.WorkoutLog;
using BodyTracker.Services.Chart;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BodyTracker.Services
{
    public static class ChartTemplateService
    {

        #region Body Measurement Chart Templates


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
        public static ChartResult CreateBodyMeasurementChart(DateTime startDate,
            DateTime endDate,
            List<FullBodyMeasurementDatasModel> data,
            bool isTrendLineLegendVisible, bool isTrendLineHoverable,
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
                IsTrendLineHoverable = isTrendLineHoverable,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.MeasurementDate,
                ValueSelector = x => (double?)x.BodyWeight,

                TrendLineStyle = new float[] { 0, 0 }
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
                IsTrendLineHoverable = isTrendLineHoverable,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.MeasurementDate,
                ValueSelector = x => (double?)x.BodyWaterPercentage,

                TrendLineStyle = new float[] { 0, 0 }

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
                IsTrendLineHoverable = isTrendLineHoverable,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.MeasurementDate,
                ValueSelector = x => (double?)x.BodyMusclePercentage,
                
                TrendLineStyle = new float[] { 0, 0 }

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
                IsTrendLineHoverable = isTrendLineHoverable,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.MeasurementDate,
                ValueSelector = x => (double?)x.BodyFatPercentage,

                TrendLineStyle = new float[] { 0, 0 }
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
                    //Name = "Date"
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

            return new ChartResult { Series = series, XAxis = XAxis, YAxis = YAxes };
        }

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
        public static ChartResult CreateWeightChart(DateTime startDate,
                                                                                                   DateTime endDate,
                                                                                                   List<FullBodyMeasurementDatasModel> data,
                                                                                                   bool isTrendLineLegendVisible,
                                                                                                   float strokeThickness, float geometrySize, double loessFraction,
                                                                                                   bool showBodyWeight, bool showBodyWeightTrend)
        {


            float max = 0;
            float min = 300;

            foreach (var value in data)
            {
                if (value.BodyWeight > max) max = (float)value.BodyWeight;
                if (value.BodyWeight < min) min = (float)value.BodyWeight;

            }
            max = max + max * 0.005f;
            min = min - min * 0.01f;

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
            }
        };

            var series = ChartSeriesBuilder.CreateSeries(data,
                                                         chartDefinitions,
                                                         loessFraction).ToArray();

            var XAxis = new ICartesianAxis[]
            {
                new DateTimeAxis(
                    TimeSpan.FromDays(1),
                    date => date.ToString("dd.MM.yyyy"))
                {
             SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                {
                    StrokeThickness = strokeThickness,
                    PathEffect = new DashEffect(new float[] { 4, 4 })
                },
                ShowSeparatorLines = true,
                }
            };


            var yAxisDefinitions = new[]
            {
                new ChartYAxisModel
                {
                    Name = "kg",
                    MinLimit = min,
                    MaxLimit = max


                }
        };

            var YAxes = yAxisDefinitions.Select(ChartAxisBuilder.Create).ToArray();

            return new ChartResult { Series = series, XAxis = XAxis, YAxis = YAxes };
        }

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
        public static ChartResult CreateBodyMuscleFatPercantageChart(DateTime startDate,
                                                                                                   DateTime endDate,
                                                                                                   List<FullBodyMeasurementDatasModel> data,
                                                                                                   bool isTrendLineLegendVisible,
                                                                                                   float strokeThickness, float geometrySize, double loessFraction,
                                                                                                   bool showBodyFat, bool showBodyFatTrend,
                                                                                                   bool showBodyMuscle, bool showBodyMuscleTrend)
        {

            float maxFat = 0;
            float minFat = 100;

            float maxMuscle = 0;
            float minMuscle = 100;

            foreach (var value in data)
            {
                if (value.BodyMusclePercentage > maxMuscle) maxMuscle = (float)value.BodyMusclePercentage;
                if (value.BodyFatPercentage > maxFat) maxFat = (float)value.BodyFatPercentage;
                if (value.BodyMusclePercentage < minMuscle) minMuscle = (float)value.BodyMusclePercentage;
                if (value.BodyFatPercentage < minFat) minFat = (float)value.BodyFatPercentage;

            }

            double max = 0;
            double min = 0;
            if (maxFat <= maxMuscle) { max = (double)maxMuscle + (double)maxMuscle * 0.01; }
            else { max = (double)maxFat + (double)maxFat * 0.01; }

            if (minFat <= minMuscle) { min = (double)minFat - (double)minFat * 0.01; }
            else { min = (double)minMuscle - (double)minMuscle * 0.01; }

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<FullBodyMeasurementDatasModel>
            {
                Name = "Body Muscle (%)",
                TrendName = "Body Muscle Trend (%)",
                Color = SKColors.Green,
                YAxisIndex = 0,

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
                YAxisIndex = 0,

                IsVisible = showBodyFat,
                ShowTrend = showBodyFatTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.MeasurementDate,
                ValueSelector = x => (double?)x.BodyFatPercentage,

            }
        };

            var series = ChartSeriesBuilder.CreateSeries(data,
                                                         chartDefinitions,
                                                         loessFraction).ToArray();

            var XAxis = new ICartesianAxis[]
            {
                new DateTimeAxis(
                    TimeSpan.FromDays(1),
                    date => date.ToString("dd.MM.yyyy"))
                {
                     SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                     {
                         StrokeThickness = strokeThickness,
                         PathEffect = new DashEffect(new float[] { 4, 4 })
                     },
                ShowSeparatorLines = true,
                }
            };


            var yAxisDefinitions = new[]
            {
                new ChartYAxisModel
                {
                    Name = "%",
                    Position = AxisPosition.Start,
                    ShowSeparatorLines = true,
                    MinLimit = min,
                    MaxLimit = max,

                }
        };

            var YAxes = yAxisDefinitions.Select(ChartAxisBuilder.Create).ToArray();

            return new ChartResult { Series = series, XAxis = XAxis, YAxis = YAxes };
        }

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
        public static ChartResult CreateBodyWaterChart(DateTime startDate,
                                                                                                   DateTime endDate,
                                                                                                   List<FullBodyMeasurementDatasModel> data,
                                                                                                   bool isTrendLineLegendVisible,
                                                                                                   float strokeThickness, float geometrySize, double loessFraction,
                                                                                                   bool showBodyWater, bool showBodyWaterTrend)
        {

            float max = 0;
            float min = 100;

            foreach (var value in data)
            {
                if (value.BodyWaterPercentage > max) max = (float)value.BodyWaterPercentage;
                if (value.BodyWaterPercentage < min) min = (float)value.BodyWaterPercentage;

            }
            max = max + max * 0.005f;
            min = min - min * 0.01f;

            var chartDefinitions = new[]
            {
                new ChartSeriesModel<FullBodyMeasurementDatasModel>
                {
                    Name = "Body Water (%)",
                    TrendName = "Body Water Trend (%)",
                    Color = SKColors.DarkBlue,
                    YAxisIndex = 0,

                    IsVisible = showBodyWater,
                    ShowTrend = showBodyWaterTrend,
                    IsTrendLineVisible = isTrendLineLegendVisible,

                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.MeasurementDate,
                    ValueSelector = x => (double?)x.BodyWaterPercentage
                }

            };

            var series = ChartSeriesBuilder.CreateSeries(data,
                                                         chartDefinitions,
                                                         loessFraction).ToArray();

            var XAxis = new ICartesianAxis[]
            {
                new DateTimeAxis(
                    TimeSpan.FromDays(1),
                    date => date.ToString("dd.MM.yyyy"))
                {
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                {
                    StrokeThickness = 1,
                    PathEffect = new DashEffect(new float[] { 4, 4 })
                },
                ShowSeparatorLines = true,
                }
            };


            var yAxisDefinitions = new[]
            {
                new ChartYAxisModel
                {
                    Name = "%",
                    Position = AxisPosition.Start,
                    ShowSeparatorLines = true,
                    MinLimit= min,
                    MaxLimit= max

                }
        };

            var YAxes = yAxisDefinitions.Select(ChartAxisBuilder.Create).ToArray();

            return new ChartResult { Series = series, XAxis = XAxis, YAxis = YAxes };
        }


        #endregion

        #region Samungs Step Tend Chart Template

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
        public static ChartResult CreateDailyActivityChart(DateTime startDate,
                                                                                                DateTime endDate,
                                                                                                List<SamsungStepTrendDashboardModel> data,
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
                   x.StepCount))
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
                new ChartSeriesModel<SamsungStepTrendDashboardModel>
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

                new ChartSeriesModel<SamsungStepTrendDashboardModel>
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

                new ChartSeriesModel<SamsungStepTrendDashboardModel>
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
                    ValueSelector = x => (double?)x.StepCount
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

            return new ChartResult { Series = series, XAxis = XAxis, YAxis = YAxes };
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
        public static ChartResult CreateStepsActivityChart(DateTime startDate,
            DateTime endDate,
            List<SamsungStepTrendDashboardModel> data,
            bool isTrendLineLegendVisible,
            float strokeThickness, float geometrySize, double loessFraction,
            bool showSteps, bool showStepsTrend)
        {


            float maxSteps = 0;
            float minSteps = 1000;

            foreach (var value in data)
            {
                if (value.StepCount > maxSteps) maxSteps = (float)value.StepCount;
                if (value.StepCount < minSteps) minSteps = (float)value.StepCount;

            }

            maxSteps = maxSteps + maxSteps * 0.005f;
            minSteps = minSteps - minSteps * 0.01f;

            var chartSeries = new List<ISeries>();

            var chartDefinitions = new[]
            {


                new ChartSeriesModel<SamsungStepTrendDashboardModel>
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
                    ValueSelector = x => (double?)x.StepCount
                }
            };

            var series = ChartSeriesBuilder.CreateSeries(data,
                                                         chartDefinitions,
                                                         loessFraction).ToArray();

            var XAxis = new ICartesianAxis[]
            {
              new DateTimeAxis(
                    TimeSpan.FromDays(1),
                    date => date.ToString("dd.MM.yyyy"))
                {
                     SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                     {
                         StrokeThickness = strokeThickness,
                         PathEffect = new DashEffect(new float[] { 4, 4 })
                     },
                ShowSeparatorLines = true,
                }
            };

            var yAxisDefinitions = new[]
            {

            new ChartYAxisModel
            {
                Name = "Steps",

                Position = AxisPosition.Start,
                ShowSeparatorLines = true,
                MinLimit = minSteps,
                MaxLimit = maxSteps
            },


        };

            var YAxes = yAxisDefinitions.Select(ChartAxisBuilder.Create).ToArray();

            return new ChartResult { Series = series, XAxis = XAxis, YAxis = YAxes };
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
        public static ChartResult CreateStepsCaloriesChart(DateTime startDate,
            DateTime endDate,
            List<SamsungStepTrendDashboardModel> data,
            bool isTrendLineLegendVisible,
            float strokeThickness, float geometrySize, double loessFraction,
            bool showCalories, bool showCaloriesTrend)
        {
            float maxCalories = 0;
            float minCalories = 100;

            float maxSteps = 0;
            float minSteps = 100;

            foreach (var value in data)
            {
                if (value.Calorie > maxCalories) maxCalories = (float)value.Calorie;
                if (value.Calorie < minCalories) minCalories = (float)value.Calorie;

            }



            maxCalories = maxCalories + maxCalories * 0.005f;
            minCalories = minCalories - minCalories * 0.01f;


            var chartSeries = new List<ISeries>();

            var chartDefinitions = new[]
            {


                new ChartSeriesModel<SamsungStepTrendDashboardModel>
                {
                    Name = "Daily Burned Calories",
                    TrendName = "Daily Burned Calories Trend",
                    Color = SKColors.Red,
                    YAxisIndex = 0,

                    IsVisible = showCalories,
                    ShowTrend = showCaloriesTrend,
                    IsTrendLineVisible = isTrendLineLegendVisible,

                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.CreateTime,
                    ValueSelector = x => (double?)x.Calorie
                },


            };

            var series = ChartSeriesBuilder.CreateSeries(data,
                                                         chartDefinitions,
                                                         loessFraction).ToArray();

            var XAxis = new ICartesianAxis[]
            {
              new DateTimeAxis(
                    TimeSpan.FromDays(1),
                    date => date.ToString("dd.MM.yyyy"))
                {
                     SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                     {
                         StrokeThickness = strokeThickness,
                         PathEffect = new DashEffect(new float[] { 4, 4 })
                     },
                ShowSeparatorLines = true,
                }
            };

            var yAxisDefinitions = new[]
            {
                 new ChartYAxisModel
            {
                Name = "Calories (kcal)",
               Position = AxisPosition.Start,
                ShowSeparatorLines = true,
                MinLimit = minCalories,
                MaxLimit = maxCalories
            },




        };

            var YAxes = yAxisDefinitions.Select(ChartAxisBuilder.Create).ToArray();

            return new ChartResult { Series = series, XAxis = XAxis, YAxis = YAxes };
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
        public static ChartResult CreateStepDistanceChart(DateTime startDate,
            DateTime endDate,
            List<SamsungStepTrendDashboardModel> data,
            bool isTrendLineLegendVisible,
            float strokeThickness, float geometrySize, double loessFraction,
            bool showDistance, bool showDistanceTrend)
        {

            float max = 0;
            float min = 300;

            foreach (var value in data)
            {
                if (value.Distance > max) max = (float)value.Distance;
                if (value.Distance < min) min = (float)value.Distance;

            }
            max = max + max * 0.005f;
            min = min - min * 0.01f;

            var chartSeries = new List<ISeries>();

            var chartDefinitions = new[]
            {
                new ChartSeriesModel<SamsungStepTrendDashboardModel>
                {
                    Name = "Daily Distance",
                    TrendName = "Daily Distance Trend",
                    Color = SKColors.BlueViolet,
                    YAxisIndex = 0,

                    IsVisible = showDistance,
                    ShowTrend = showDistanceTrend,
                    IsTrendLineVisible = isTrendLineLegendVisible,

                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.CreateTime,
                    ValueSelector = x => (double?)x.Distance
                }
            };

            var series = ChartSeriesBuilder.CreateSeries(data,
                                                         chartDefinitions,
                                                         loessFraction).ToArray();

            var XAxis = new ICartesianAxis[]
            {
             new DateTimeAxis(
                    TimeSpan.FromDays(1),
                    date => date.ToString("dd.MM.yyyy"))
                {
                     SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                     {
                         StrokeThickness = strokeThickness,
                         PathEffect = new DashEffect(new float[] { 4, 4 })
                     },
                ShowSeparatorLines = true,
                }
            };

            var yAxisDefinitions = new[]
            {

            new ChartYAxisModel
            {
                Name = "Distance (m)",
                Position = AxisPosition.Start,
                ShowSeparatorLines = true,
                MinLimit = min,
                MaxLimit = max
            }
            };

            var YAxes = yAxisDefinitions.Select(ChartAxisBuilder.Create).ToArray();

            return new ChartResult { Series = series, XAxis = XAxis, YAxis = YAxes };
        }



        #endregion

        #region Samsung Exercise Chart Templates

        /// <summary>
        /// Generates LiveCharts Cartesian series and axes configurations for exercise duration over a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the filtering range.</param>
        /// <param name="endDate">The end date of the filtering range.</param>
        /// <param name="data">The collection of exercise dashboard models containing duration metrics.</param>
        /// <param name="isTrendLineLegendVisible">A value indicating whether trend lines should appear in the chart legend.</param>
        /// <param name="strokeThickness">The stroke thickness for the chart lines.</param>
        /// <param name="geometrySize">The size of the data point geometries on the chart.</param>
        /// <param name="loessFraction">The LOESS smoothing fraction used for calculating trend lines.</param>
        /// <param name="showDuration">A value indicating whether the exercise duration series is visible.</param>
        /// <param name="showDurationTrend">A value indicating whether the exercise duration trend line is visible.</param>
        /// <returns>A <see cref="ChartResult"/> containing the generated series array, X-axis array, and Y-axes array.</returns>
        public static ChartResult CreateExerciseDurationChart(
                DateTime startDate,
                DateTime endDate,
                List<SamsungExerciseDashboardModel> data,
                bool isTrendLineLegendVisible,
                float strokeThickness,
                float geometrySize,
                double loessFraction,
                bool showDuration,
                bool showDurationTrend)
        {
            double max = data.Max(x => x.Duration ?? 0);
            double min = data.Min(x => x.Duration ?? 0);

            max += max * 0.05;

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<SamsungExerciseDashboardModel>
            {
                Name = "Duration (min)",
                TrendName = "Duration Trend",
                Color = SKColors.DodgerBlue,
                YAxisIndex = 0,

                IsVisible = showDuration,
                ShowTrend = showDurationTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.StartTime,
                ValueSelector = x => x.Duration
            }
        };

            var series = ChartSeriesBuilder
                .CreateSeries(data, chartDefinitions, loessFraction)
                .ToArray();

            var xAxis = new ICartesianAxis[]
            {
            new DateTimeAxis(
                TimeSpan.FromDays(1),
                date => date.ToString("dd.MM"))
            };

            var yAxes = new[]
            {
            ChartAxisBuilder.Create(
            new ChartYAxisModel
            {
                Name = "Min",
                MinLimit = min,
                MaxLimit = max
            })
        };

            return new ChartResult
            {
                Series = series,
                XAxis = xAxis,
                YAxis = yAxes
            };
        }

        /// <summary>
        /// Generates LiveCharts Cartesian series and axes configurations for burned calories during exercises over a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the filtering range.</param>
        /// <param name="endDate">The end date of the filtering range.</param>
        /// <param name="data">The collection of exercise dashboard models containing calorie metrics.</param>
        /// <param name="isTrendLineLegendVisible">A value indicating whether trend lines should appear in the chart legend.</param>
        /// <param name="strokeThickness">The stroke thickness for the chart lines.</param>
        /// <param name="geometrySize">The size of the data point geometries on the chart.</param>
        /// <param name="loessFraction">The LOESS smoothing fraction used for calculating trend lines.</param>
        /// <param name="showCalories">A value indicating whether the calorie series is visible.</param>
        /// <param name="showCaloriesTrend">A value indicating whether the calorie trend line is visible.</param>
        /// <returns>A <see cref="ChartResult"/> containing the generated series array, X-axis array, and Y-axes array.</returns>
        public static ChartResult CreateExerciseCaloriesChart(
                DateTime startDate,
                DateTime endDate,
                List<SamsungExerciseDashboardModel> data,
                bool isTrendLineLegendVisible,
                float strokeThickness,
                float geometrySize,
                double loessFraction,
                bool showCalories,
                bool showCaloriesTrend)
        {
            double max = data.Max(x => x.Calorie ?? 0);
            double min = data.Min(x => x.Calorie ?? 0);

            max += max * 0.05;

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<SamsungExerciseDashboardModel>
            {
                Name = "Calories",
                TrendName = "Calories Trend",
                Color = SKColors.OrangeRed,
                YAxisIndex = 0,

                IsVisible = showCalories,
                ShowTrend = showCaloriesTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.StartTime,
                ValueSelector = x => x.Calorie
            }
        };

            var series = ChartSeriesBuilder
                .CreateSeries(data, chartDefinitions, loessFraction)
                .ToArray();

            var xAxis = new ICartesianAxis[]
            {
            new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM"))
            };

            var yAxes = new[]
            {
            ChartAxisBuilder.Create(
                new ChartYAxisModel
                {
                    Name = "kcal",
                    MinLimit = min,
                    MaxLimit = max
                })
        };

            return new ChartResult
            {
                Series = series,
                XAxis = xAxis,
                YAxis = yAxes
            };
        }

        /// <summary>
        /// Generates LiveCharts Cartesian series and axes configurations for exercise distances over a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the filtering range.</param>
        /// <param name="endDate">The end date of the filtering range.</param>
        /// <param name="data">The collection of exercise dashboard models containing distance metrics.</param>
        /// <param name="isTrendLineLegendVisible">A value indicating whether trend lines should appear in the chart legend.</param>
        /// <param name="strokeThickness">The stroke thickness for the chart lines.</param>
        /// <param name="geometrySize">The size of the data point geometries on the chart.</param>
        /// <param name="loessFraction">The LOESS smoothing fraction used for calculating trend lines.</param>
        /// <param name="showDistance">A value indicating whether the distance series is visible.</param>
        /// <param name="showDistanceTrend">A value indicating whether the distance trend line is visible.</param>
        /// <returns>A <see cref="ChartResult"/> containing the generated series array, X-axis array, and Y-axes array.</returns>
        public static ChartResult CreateExerciseDistanceChart(
                DateTime startDate,
                DateTime endDate,
                List<SamsungExerciseDashboardModel> data,
                bool isTrendLineLegendVisible,
                float strokeThickness,
                float geometrySize,
                double loessFraction,
                bool showDistance,
                bool showDistanceTrend)
        {
            double max = data.Max(x => x.Distance ?? 0);
            double min = data.Min(x => x.Distance ?? 0);

            max += max * 0.05;

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<SamsungExerciseDashboardModel>
            {
                Name = "Distance",
                TrendName = "Distance Trend",
                Color = SKColors.ForestGreen,
                YAxisIndex = 0,

                IsVisible = showDistance,
                ShowTrend = showDistanceTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.StartTime,
                ValueSelector = x => x.Distance
            }
        };

            var series = ChartSeriesBuilder
                .CreateSeries(data, chartDefinitions, loessFraction)
                .ToArray();

            var xAxis = new ICartesianAxis[]
            {
            new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM"))
            };

            var yAxes = new[]
            {
            ChartAxisBuilder.Create(
                new ChartYAxisModel
                {
                    Name = "km",
                    MinLimit = min,
                    MaxLimit = max
                })
        };

            return new ChartResult
            {
                Series = series,
                XAxis = xAxis,
                YAxis = yAxes
            };
        }

        /// <summary>
        /// Generates LiveCharts Cartesian series and axes configurations for exercise heart rate metrics (Mean, Max, Min) over a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the filtering range.</param>
        /// <param name="endDate">The end date of the filtering range.</param>
        /// <param name="data">The collection of exercise dashboard models containing heart rate metrics.</param>
        /// <param name="isTrendLineLegendVisible">A value indicating whether trend lines should appear in the chart legend.</param>
        /// <param name="strokeThickness">The stroke thickness for the chart lines.</param>
        /// <param name="geometrySize">The size of the data point geometries on the chart.</param>
        /// <param name="loessFraction">The LOESS smoothing fraction used for calculating trend lines.</param>
        /// <param name="showMeanHR">A value indicating whether the mean heart rate series is visible.</param>
        /// <param name="showMeanHRTrend">A value indicating whether the mean heart rate trend line is visible.</param>
        /// <param name="showMaxHR">A value indicating whether the max heart rate series is visible.</param>
        /// <param name="showMaxHRTrend">A value indicating whether the max heart rate trend line is visible.</param>
        /// <param name="showMinHR">A value indicating whether the min heart rate series is visible.</param>
        /// <param name="showMinHRTrend">A value indicating whether the min heart rate trend line is visible.</param>
        /// <returns>A <see cref="ChartResult"/> containing the generated series array, X-axis array, and Y-axes array.</returns>
        public static ChartResult CreateExerciseHeartRateChart(
                DateTime startDate,
                DateTime endDate,
                List<SamsungExerciseDashboardModel> data,
                bool isTrendLineLegendVisible,
                float strokeThickness,
                float geometrySize,
                double loessFraction,
                bool showMeanHR,
                bool showMeanHRTrend,
                bool showMaxHR,
                bool showMaxHRTrend,
                bool showMinHR,
                bool showMinHRTrend)
        {
            double max = data.Max(x => x.MaxHeartRate ?? 0);
            double min = data.Min(x => x.MinHeartRate ?? 0);

            max += max * 0.05;

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<SamsungExerciseDashboardModel>
            {
                Name = "Ø",
                Color = SKColors.Red,
                YAxisIndex = 0,

                IsVisible = showMeanHR,
                ShowTrend = showMeanHRTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.StartTime,
                ValueSelector = x => x.MeanHeartRate
            },

            new ChartSeriesModel<SamsungExerciseDashboardModel>
            {
                Name = "max(x)",
                Color = SKColors.DarkRed,
                YAxisIndex = 0,

                IsVisible = showMaxHR,
                ShowTrend = showMaxHRTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.StartTime,
                ValueSelector = x => x.MaxHeartRate
            },

            new ChartSeriesModel<SamsungExerciseDashboardModel>
            {
                Name = "min(x)",
                Color = SKColors.Blue,
                YAxisIndex = 0,

                IsVisible = showMinHR,
                ShowTrend = showMinHRTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.StartTime,
                ValueSelector = x => x.MinHeartRate
            }
        };

            var series = ChartSeriesBuilder
                .CreateSeries(data, chartDefinitions, loessFraction)
                .ToArray();

            var xAxis = new ICartesianAxis[]
            {
            new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM"))
            };

            var yAxes = new[]
            {
            ChartAxisBuilder.Create(
                new ChartYAxisModel
                {
                    Name = "bpm",
                    MinLimit = min,
                    MaxLimit = max
                })
        };

            return new ChartResult
            {
                Series = series,
                XAxis = xAxis,
                YAxis = yAxes
            };
        }

        /// <summary>
        /// Generates LiveCharts Cartesian series and axes configurations for exercise speed over a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the filtering range.</param>
        /// <param name="endDate">The end date of the filtering range.</param>
        /// <param name="data">The collection of exercise dashboard models containing speed metrics.</param>
        /// <param name="isTrendLineLegendVisible">A value indicating whether trend lines should appear in the chart legend.</param>
        /// <param name="strokeThickness">The stroke thickness for the chart lines.</param>
        /// <param name="geometrySize">The size of the data point geometries on the chart.</param>
        /// <param name="loessFraction">The LOESS smoothing fraction used for calculating trend lines.</param>
        /// <param name="showSpeed">A value indicating whether the speed series is visible.</param>
        /// <param name="showSpeedTrend">A value indicating whether the speed trend line is visible.</param>
        /// <returns>A <see cref="ChartResult"/> containing the generated series array, X-axis array, and Y-axes array.</returns>
        public static ChartResult CreateExerciseSpeedChart(
                DateTime startDate,
                DateTime endDate,
                List<SamsungExerciseDashboardModel> data,
                bool isTrendLineLegendVisible,
                float strokeThickness,
                float geometrySize,
                double loessFraction,
                bool showSpeed,
                bool showSpeedTrend)
        {
            double max = data.Max(x => x.MeanSpeed ?? 0);
            double min = data.Min(x => x.MeanSpeed ?? 0);

            max += max * 0.05;

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<SamsungExerciseDashboardModel>
            {
                Name = "Ø Speed",
                Color = SKColors.Red,
                YAxisIndex = 0,

                IsVisible = showSpeed,
                ShowTrend = showSpeedTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                DateSelector = x => x.StartTime,
                ValueSelector = x => x.MeanSpeed // Korrigiert von MeanHeartRate auf MeanSpeed
            }
        };

            var series = ChartSeriesBuilder
                .CreateSeries(data, chartDefinitions, loessFraction)
                .ToArray();

            var xAxis = new ICartesianAxis[]
            {
            new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM"))
            };

            var yAxes = new[]
            {
            ChartAxisBuilder.Create(
                new ChartYAxisModel
                {
                    Name = "Ø km/h",
                    MinLimit = min,
                    MaxLimit = max
                })
        };

            return new ChartResult
            {
                Series = series,
                XAxis = xAxis,
                YAxis = yAxes
            };
        }

        #endregion

        #region Samsung Heart Rate Chart Templates

        /// <summary>
        /// Generates LiveCharts Cartesian series and axes configurations for exercise heart rate metrics (Mean, Max, Min) over a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the filtering range.</param>
        /// <param name="endDate">The end date of the filtering range.</param>
        /// <param name="data">The collection of exercise dashboard models containing heart rate metrics.</param>
        /// <param name="isTrendLineLegendVisible">A value indicating whether trend lines should appear in the chart legend.</param>
        /// <param name="strokeThickness">The stroke thickness for the chart lines.</param>
        /// <param name="geometrySize">The size of the data point geometries on the chart.</param>
        /// <param name="loessFraction">The LOESS smoothing fraction used for calculating trend lines.</param>
        /// <param name="showHR">A value indicating whether the mean heart rate series is visible.</param>
        /// <param name="showHRTrend">A value indicating whether the mean heart rate trend line is visible.</param>
        /// <param name="showMaxHR">A value indicating whether the max heart rate series is visible.</param>
        /// <param name="showMaxHRTrend">A value indicating whether the max heart rate trend line is visible.</param>
        /// <param name="showMinHR">A value indicating whether the min heart rate series is visible.</param>
        /// <param name="showMinHRTrend">A value indicating whether the min heart rate trend line is visible.</param>
        /// <returns>A <see cref="ChartResult"/> containing the generated series array, X-axis array, and Y-axes array.</returns>
        public static ChartResult CreateHeartRateChart(
            DateTime startDate,
            DateTime endDate,
            List<SamsungHeartRateDashboardModel> data,
            bool isTrendLineLegendVisible,
            float strokeThickness,
            float geometrySize,
            double loessFraction,
            bool showHR,
            bool showHRTrend,
            bool showMaxHR,
            bool showMaxHRTrend,
            bool showMinHR,
            bool showMinHRTrend)
        {
            double max = data != null && data.Any() ? data.Max(x => x.MaxHeartRate ?? 0) : 200;
            double min = data != null && data.Any() ? data.Min(x => x.MinHeartRate ?? 0) : 40;

            max += max * 0.05;

            var chartDefinitions = new[]
            {
                new ChartSeriesModel<SamsungHeartRateDashboardModel>
                {
                    Name = "HR",
                    Color = SKColors.Red,
                    YAxisIndex = 0,

                    IsVisible = showHR,
                    ShowTrend = showHRTrend,
                    IsTrendLineVisible = isTrendLineLegendVisible,

                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.CreateTime,
                    ValueSelector = x => x.HeartRate
                },
                new ChartSeriesModel<SamsungHeartRateDashboardModel>
                {
                    Name = "max(x)",
                    Color = SKColors.DarkRed,
                    YAxisIndex = 0,

                    IsVisible = showMaxHR,
                    ShowTrend = showMaxHRTrend,
                    IsTrendLineVisible = isTrendLineLegendVisible,

                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.CreateTime,
                    ValueSelector = x => x.MaxHeartRate
                },
                new ChartSeriesModel<SamsungHeartRateDashboardModel>
                {
                    Name = "min(x)",
                    Color = SKColors.Blue,
                    YAxisIndex = 0,

                    IsVisible = showMinHR,
                    ShowTrend = showMinHRTrend,
                    IsTrendLineVisible = isTrendLineLegendVisible,

                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.CreateTime,
                    ValueSelector = x => x.MinHeartRate
                }
            };

            var series = ChartSeriesBuilder
                .CreateSeries(data, chartDefinitions, loessFraction)
                .ToArray();

            var xAxis = new ICartesianAxis[]
            {
                new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM"))
            };

            var yAxes = new[]
            {
                ChartAxisBuilder.Create(
                    new ChartYAxisModel
                    {
                        Name = "bpm",
                        MinLimit = min,
                        MaxLimit = max
                    })
            };

            return new ChartResult
            {
                Series = series,
                XAxis = xAxis,
                YAxis = yAxes
            };
        }

        #endregion

        #region Samsung Food Info Chart Templates

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="data"></param>
        /// <param name="isTrendLineLegendVisible"></param>
        /// <param name="strokeThickness"></param>
        /// <param name="geometrySize"></param>
        /// <param name="loessFraction"></param>
        /// <param name="showCalories"></param>
        /// <param name="showCaloriesTrend"></param>
        /// <returns></returns>
        public static ChartResult CreateFoodCaloriesChart(
                    DateTime startDate, DateTime endDate, List<SamsungFoodInfoModel> data,
                    bool isTrendLineLegendVisible, float strokeThickness, float geometrySize,
                    double loessFraction, bool showCalories, bool showCaloriesTrend)
        {
            double max = data.Any() ? data.Max(x => x.Calorie ?? 0) : 100;
            double min = data.Any() ? data.Min(x => x.Calorie ?? 0) : 0;
            max += max * 0.05;

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<SamsungFoodInfoModel>
            {
                Name = "Calories",
                TrendName = "Calories Trend",
                Color = SKColors.OrangeRed,
                YAxisIndex = 0,
                IsVisible = showCalories,
                ShowTrend = showCaloriesTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,
                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,
                DateSelector = x => x.CreateTime,
                ValueSelector = x => x.Calorie
            }
        };

            var series = ChartSeriesBuilder.CreateSeries(data, chartDefinitions, loessFraction).ToArray();
            var xAxis = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yy")) };
            var yAxes = new[] { ChartAxisBuilder.Create(new ChartYAxisModel { Name = "kcal", MinLimit = min, MaxLimit = max }) };

            return new ChartResult { Series = series, XAxis = xAxis, YAxis = yAxes };
        }

        public static ChartResult CreateFoodProteinChart(
            DateTime startDate, DateTime endDate, List<SamsungFoodInfoModel> data,
            bool isTrendLineLegendVisible, float strokeThickness, float geometrySize,
            double loessFraction, bool showProtein, bool showProteinTrend)
        {
            double max = data.Any() ? data.Max(x => x.Protein ?? 0) : 100;
            double min = data.Any() ? data.Min(x => x.Protein ?? 0) : 0;
            max += max * 0.05;

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<SamsungFoodInfoModel>
            {
                Name = "Protein",
                TrendName = "Protein Trend",
                Color = SKColors.Crimson,
                YAxisIndex = 0,
                IsVisible = showProtein,
                ShowTrend = showProteinTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,
                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,
                DateSelector = x => x.CreateTime,
                ValueSelector = x => x.Protein
            }
        };

            var series = ChartSeriesBuilder.CreateSeries(data, chartDefinitions, loessFraction).ToArray();
            var xAxis = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yy")) };
            var yAxes = new[] { ChartAxisBuilder.Create(new ChartYAxisModel { Name = "g", MinLimit = min, MaxLimit = max }) };

            return new ChartResult { Series = series, XAxis = xAxis, YAxis = yAxes };
        }

        public static ChartResult CreateFoodCarbsChart(
            DateTime startDate, DateTime endDate, List<SamsungFoodInfoModel> data,
            bool isTrendLineLegendVisible, float strokeThickness, float geometrySize,
            double loessFraction, bool showCarbs, bool showCarbsTrend)
        {
            double max = data.Any() ? data.Max(x => x.Carbohydrate ?? 0) : 100;
            double min = data.Any() ? data.Min(x => x.Carbohydrate ?? 0) : 0;
            max += max * 0.05;

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<SamsungFoodInfoModel>
            {
                Name = "Carbohydrates",
                TrendName = "Carbs Trend",
                Color = SKColors.Goldenrod,
                YAxisIndex = 0,
                IsVisible = showCarbs,
                ShowTrend = showCarbsTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,
                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,
                DateSelector = x => x.CreateTime,
                ValueSelector = x => x.Carbohydrate
            }
        };

            var series = ChartSeriesBuilder.CreateSeries(data, chartDefinitions, loessFraction).ToArray();
            var xAxis = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yy")) };
            var yAxes = new[] { ChartAxisBuilder.Create(new ChartYAxisModel { Name = "g", MinLimit = min, MaxLimit = max }) };

            return new ChartResult { Series = series, XAxis = xAxis, YAxis = yAxes };
        }

        public static ChartResult CreateFoodFatChart(
            DateTime startDate, DateTime endDate, List<SamsungFoodInfoModel> data,
            bool isTrendLineLegendVisible, float strokeThickness, float geometrySize,
            double loessFraction, bool showFat, bool showFatTrend)
        {
            double max = data.Any() ? data.Max(x => x.TotalFat ?? 0) : 100;
            double min = data.Any() ? data.Min(x => x.TotalFat ?? 0) : 0;
            max += max * 0.05;

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<SamsungFoodInfoModel>
            {
                Name = "Fat",
                TrendName = "Fat Trend",
                Color = SKColors.ForestGreen,
                YAxisIndex = 0,
                IsVisible = showFat,
                ShowTrend = showFatTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,
                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,
                DateSelector = x => x.CreateTime,
                ValueSelector = x => x.TotalFat
            }
        };

            var series = ChartSeriesBuilder.CreateSeries(data, chartDefinitions, loessFraction).ToArray();
            var xAxis = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yy")) };
            var yAxes = new[] { ChartAxisBuilder.Create(new ChartYAxisModel { Name = "g", MinLimit = min, MaxLimit = max }) };

            return new ChartResult { Series = series, XAxis = xAxis, YAxis = yAxes };
        }

        public static ChartResult CreateFoodVitaminAChart(
            DateTime startDate, DateTime endDate, List<SamsungFoodInfoModel> data,
            bool isTrendLineLegendVisible, float strokeThickness, float geometrySize, double loessFraction,
            bool showVitaminA, bool showVitaminATrend)
        {
            double max = data.Any() ? data.Max(x => x.VitaminA ?? 0) : 100;
            double min = data.Any() ? data.Min(x => x.VitaminA ?? 0) : 0;
            max += max * 0.05;

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<SamsungFoodInfoModel>
            {
                Name = "Vitamin A",
                TrendName = "Vitamin A Trend",
                Color = SKColors.DarkOrange,
                YAxisIndex = 0,
                IsVisible = showVitaminA,
                ShowTrend = showVitaminATrend,
                IsTrendLineVisible = isTrendLineLegendVisible,
                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,
                DateSelector = x => x.CreateTime,
                ValueSelector = x => x.VitaminA
            }
        };

            var series = ChartSeriesBuilder.CreateSeries(data, chartDefinitions, loessFraction).ToArray();
            var xAxis = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yy")) };
            var yAxes = new[] { ChartAxisBuilder.Create(new ChartYAxisModel { Name = "µg", MinLimit = min, MaxLimit = max }) };

            return new ChartResult { Series = series, XAxis = xAxis, YAxis = yAxes };
        }

        public static ChartResult CreateFoodVitaminCChart(
            DateTime startDate, DateTime endDate, List<SamsungFoodInfoModel> data,
            bool isTrendLineLegendVisible, float strokeThickness, float geometrySize, double loessFraction,
            bool showVitaminC, bool showVitaminCTrend)
        {
            double max = data.Any() ? data.Max(x => x.VitaminC ?? 0) : 100;
            double min = data.Any() ? data.Min(x => x.VitaminC ?? 0) : 0;
            max += max * 0.05;

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<SamsungFoodInfoModel>
            {
                Name = "Vitamin C",
                TrendName = "Vitamin C Trend",
                Color = SKColors.Gold,
                YAxisIndex = 0,
                IsVisible = showVitaminC,
                ShowTrend = showVitaminCTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,
                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,
                DateSelector = x => x.CreateTime,
                ValueSelector = x => x.VitaminC
            }
        };

            var series = ChartSeriesBuilder.CreateSeries(data, chartDefinitions, loessFraction).ToArray();
            var xAxis = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yy")) };
            var yAxes = new[] { ChartAxisBuilder.Create(new ChartYAxisModel { Name = "mg", MinLimit = min, MaxLimit = max }) };

            return new ChartResult { Series = series, XAxis = xAxis, YAxis = yAxes };
        }

        public static ChartResult CreateFoodVitaminDChart(
            DateTime startDate, DateTime endDate, List<SamsungFoodInfoModel> data,
            bool isTrendLineLegendVisible, float strokeThickness, float geometrySize, double loessFraction,
            bool showVitaminD, bool showVitaminDTrend)
        {
            double max = data.Any() ? data.Max(x => x.VitaminD ?? 0) : 100;
            double min = data.Any() ? data.Min(x => x.VitaminD ?? 0) : 0;
            max += max * 0.05;

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<SamsungFoodInfoModel>
            {
                Name = "Vitamin D",
                TrendName = "Vitamin D Trend",
                Color = SKColors.Orange,
                YAxisIndex = 0,
                IsVisible = showVitaminD,
                ShowTrend = showVitaminDTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,
                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,
                DateSelector = x => x.CreateTime,
                ValueSelector = x => x.VitaminD
            }
        };

            var series = ChartSeriesBuilder.CreateSeries(data, chartDefinitions, loessFraction).ToArray();
            var xAxis = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yy")) };
            var yAxes = new[] { ChartAxisBuilder.Create(new ChartYAxisModel { Name = "µg", MinLimit = min, MaxLimit = max }) };

            return new ChartResult { Series = series, XAxis = xAxis, YAxis = yAxes };
        }

        public static ChartResult CreateFoodIronChart(
            DateTime startDate, DateTime endDate, List<SamsungFoodInfoModel> data,
            bool isTrendLineLegendVisible, float strokeThickness, float geometrySize, double loessFraction,
            bool showIron, bool showIronTrend)
        {
            double max = data.Any() ? data.Max(x => x.Iron ?? 0) : 100;
            double min = data.Any() ? data.Min(x => x.Iron ?? 0) : 0;
            max += max * 0.05;

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<SamsungFoodInfoModel>
            {
                Name = "Iron",
                TrendName = "Iron Trend",
                Color = SKColors.SaddleBrown,
                YAxisIndex = 0,
                IsVisible = showIron,
                ShowTrend = showIronTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,
                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,
                DateSelector = x => x.CreateTime,
                ValueSelector = x => x.Iron
            }
        };

            var series = ChartSeriesBuilder.CreateSeries(data, chartDefinitions, loessFraction).ToArray();
            var xAxis = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yy")) };
            var yAxes = new[] { ChartAxisBuilder.Create(new ChartYAxisModel { Name = "mg", MinLimit = min, MaxLimit = max }) };

            return new ChartResult { Series = series, XAxis = xAxis, YAxis = yAxes };
        }

        public static ChartResult CreateFoodCalciumChart(
            DateTime startDate, DateTime endDate, List<SamsungFoodInfoModel> data,
            bool isTrendLineLegendVisible, float strokeThickness, float geometrySize, double loessFraction,
            bool showCalcium, bool showCalciumTrend)
        {
            double max = data.Any() ? data.Max(x => x.Calcium ?? 0) : 100;
            double min = data.Any() ? data.Min(x => x.Calcium ?? 0) : 0;
            max += max * 0.05;

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<SamsungFoodInfoModel>
            {
                Name = "Calcium",
                TrendName = "Calcium Trend",
                Color = SKColors.SteelBlue,
                YAxisIndex = 0,
                IsVisible = showCalcium,
                ShowTrend = showCalciumTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,
                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,
                DateSelector = x => x.CreateTime,
                ValueSelector = x => x.Calcium
            }
        };

            var series = ChartSeriesBuilder.CreateSeries(data, chartDefinitions, loessFraction).ToArray();
            var xAxis = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yy.yy")) };
            var yAxes = new[] { ChartAxisBuilder.Create(new ChartYAxisModel { Name = "mg", MinLimit = min, MaxLimit = max }) };

            return new ChartResult { Series = series, XAxis = xAxis, YAxis = yAxes };
        }

        public static ChartResult CreateFoodNatriumChart(
            DateTime startDate, DateTime endDate, List<SamsungFoodInfoModel> data,
            bool isTrendLineLegendVisible, float strokeThickness, float geometrySize, double loessFraction,
            bool showNatrium, bool showNatriumTrend)
        {
            double max = data.Any() ? data.Max(x => x.Cholesterol ?? 0) : 100;
            double min = data.Any() ? data.Min(x => x.Cholesterol ?? 0) : 0;
            max += max * 0.05;

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<SamsungFoodInfoModel>
            {
                Name = "Sodium ",
                TrendName = "Sodium Trend",
                Color = SKColors.MediumPurple,
                YAxisIndex = 0,
                IsVisible = showNatrium,
                ShowTrend = showNatriumTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,
                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,
                DateSelector = x => x.CreateTime,
                ValueSelector = x => x.Sodium
            }
        };

            var series = ChartSeriesBuilder.CreateSeries(data, chartDefinitions, loessFraction).ToArray();
            var xAxis = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yy")) };
            var yAxes = new[] { ChartAxisBuilder.Create(new ChartYAxisModel { Name = "mg", MinLimit = min, MaxLimit = max }) };

            return new ChartResult { Series = series, XAxis = xAxis, YAxis = yAxes };
        }

        public static ChartResult CreateFoodKaliumChart(
            DateTime startDate, DateTime endDate, List<SamsungFoodInfoModel> data,
            bool isTrendLineLegendVisible, float strokeThickness, float geometrySize, double loessFraction,
            bool showPotassium, bool showPotassiumTrend)
        {
            double max = data.Any() ? data.Max(x => x.Potassium ?? 0) : 100;
            double min = data.Any() ? data.Min(x => x.Potassium ?? 0) : 0;
            max += max * 0.05;

            var chartDefinitions = new[]
            {
            new ChartSeriesModel<SamsungFoodInfoModel>
            {
                Name = "Kalium",
                TrendName = "Kalium Trend",
                Color = SKColors.Teal,
                YAxisIndex = 0,
                IsVisible = showPotassium,
                ShowTrend = showPotassiumTrend,
                IsTrendLineVisible = isTrendLineLegendVisible,
                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,
                DateSelector = x => x.CreateTime,
                ValueSelector = x => x.Potassium
            }
        };

            var series = ChartSeriesBuilder.CreateSeries(data, chartDefinitions, loessFraction).ToArray();
            var xAxis = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yy")) };
            var yAxes = new[] { ChartAxisBuilder.Create(new ChartYAxisModel { Name = "mg", MinLimit = min, MaxLimit = max }) };

            return new ChartResult { Series = series, XAxis = xAxis, YAxis = yAxes };
        }

        #endregion

        #region Samsung Oxygen Saturaton Chart Tempplates

        /// <summary>
        /// Generates LiveCharts Cartesian series and axes configurations for blood oxygen saturation metrics (Mean, Max, Min) over a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the filtering range.</param>
        /// <param name="endDate">The end date of the filtering range.</param>
        /// <param name="data">The collection of oxygen saturation dashboard models.</param>
        /// <param name="isTrendLineLegendVisible">A value indicating whether trend lines should appear in the chart legend.</param>
        /// <param name="strokeThickness">The stroke thickness for the chart lines.</param>
        /// <param name="geometrySize">The size of the data point geometries on the chart.</param>
        /// <param name="loessFraction">The LOESS smoothing fraction used for calculating trend lines.</param>
        /// <param name="showSpO2">A value indicating whether the mean SpO2 series is visible.</param>
        /// <param name="showSpO2Trend">A value indicating whether the mean SpO2 trend line is visible.</param>
        /// <param name="showMaxSpO2">A value indicating whether the max SpO2 series is visible.</param>
        /// <param name="showMaxSpO2Trend">A value indicating whether the max SpO2 trend line is visible.</param>
        /// <param name="showMinSpO2">A value indicating whether the min SpO2 series is visible.</param>
        /// <param name="showMinSpO2Trend">A value indicating whether the min SpO2 trend line is visible.</param>
        /// <returns>A <see cref="ChartResult"/> containing the generated series array, X-axis array, and Y-axes array.</returns>
        public static ChartResult CreateSpO2Chart(
            DateTime startDate,
            DateTime endDate,
            List<SamsungOxygenSaturationDashboardModel> data,
            bool isTrendLineLegendVisible,
            float strokeThickness,
            float geometrySize,
            double loessFraction,
            bool showSpO2,
            bool showSpO2Trend,
            bool showMaxSpO2,
            bool showMaxSpO2Trend,
            bool showMinSpO2,
            bool showMinSpO2Trend)
        {
            double max = data != null && data.Any() ? data.Max(x => x.MaxSpO2 ?? 0) : 100;
            double min = data != null && data.Any() ? data.Min(x => x.MinSpO2 ?? 0) : 60;

            max += max * 0.05;

            var chartDefinitions = new[]
            {
                new ChartSeriesModel<SamsungOxygenSaturationDashboardModel>
                {
                    Name = "Ø",
                    Color = SKColors.Red,
                    YAxisIndex = 0,

                    IsVisible = showSpO2,
                    ShowTrend = showSpO2Trend,
                    IsTrendLineVisible = isTrendLineLegendVisible,

                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.StartTime,
                    ValueSelector = x => x.SpO2
                },
                new ChartSeriesModel<SamsungOxygenSaturationDashboardModel>
                {
                    Name = "max(x)",
                    Color = SKColors.DarkRed,
                    YAxisIndex = 0,

                    IsVisible = showMaxSpO2,
                    ShowTrend = showMaxSpO2Trend,
                    IsTrendLineVisible = isTrendLineLegendVisible,

                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.StartTime,
                    ValueSelector = x => x.MaxSpO2
                },
                new ChartSeriesModel<SamsungOxygenSaturationDashboardModel>
                {
                    Name = "min(x)",
                    Color = SKColors.Blue,
                    YAxisIndex = 0,

                    IsVisible = showMinSpO2,
                    ShowTrend = showMinSpO2Trend,
                    IsTrendLineVisible = isTrendLineLegendVisible,

                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.StartTime,
                    ValueSelector = x => x.MinSpO2
                }
            };

            var series = ChartSeriesBuilder
                .CreateSeries(data, chartDefinitions, loessFraction)
                .ToArray();

            var xAxis = new ICartesianAxis[]
            {
                new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM"))
            };

            var yAxes = new[]
            {
                ChartAxisBuilder.Create(
                    new ChartYAxisModel
                    {
                        Name = "%",
                        MinLimit = min,
                        MaxLimit = max
                    })
            };

            return new ChartResult
            {
                Series = series,
                XAxis = xAxis,
                YAxis = yAxes
            };
        }

        /// <summary>
        /// Generates LiveCharts Cartesian series and axes configurations for oxygen saturation coverage rate over a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the filtering range.</param>
        /// <param name="endDate">The end date of the filtering range.</param>
        /// <param name="data">The collection of oxygen saturation dashboard models.</param>
        /// <param name="isTrendLineLegendVisible">A value indicating whether trend lines should appear in the chart legend.</param>
        /// <param name="strokeThickness">The stroke thickness for the chart lines.</param>
        /// <param name="geometrySize">The size of the data point geometries on the chart.</param>
        /// <param name="loessFraction">The LOESS smoothing fraction used for calculating trend lines.</param>
        /// <param name="showSpO2Coverage">A value indicating whether the coverage rate series is visible.</param>
        /// <param name="showSpO2CoverageTrend">A value indicating whether the coverage rate trend line is visible.</param>
        /// <returns>A <see cref="ChartResult"/> containing the generated series array, X-axis array, and Y-axes array.</returns>
        public static ChartResult CreateSpO2CoverageChart(
            DateTime startDate,
            DateTime endDate,
            List<SamsungOxygenSaturationDashboardModel> data,
            bool isTrendLineLegendVisible,
            float strokeThickness,
            float geometrySize,
            double loessFraction,
            bool showSpO2Coverage,
            bool showSpO2CoverageTrend)
        {
            double max = data != null && data.Any() ? data.Max(x => x.CoverageRate ?? 0) : 1500;
            double min = data != null && data.Any() ? data.Min(x => x.CoverageRate ?? 0) : 0;

            max += max * 0.05;

            var chartDefinitions = new[]
            {
                new ChartSeriesModel<SamsungOxygenSaturationDashboardModel>
                {
                    Name = "Coverage",
                    Color = SKColors.Green,
                    YAxisIndex = 0,

                    IsVisible = showSpO2Coverage,
                    ShowTrend = showSpO2CoverageTrend,
                    IsTrendLineVisible = isTrendLineLegendVisible,

                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.StartTime,
                    ValueSelector = x => x.CoverageRate
                }
            };

            var series = ChartSeriesBuilder
                .CreateSeries(data, chartDefinitions, loessFraction)
                .ToArray();

            var xAxis = new ICartesianAxis[]
            {
                new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM"))
            };

            var yAxes = new[]
            {
                ChartAxisBuilder.Create(
                    new ChartYAxisModel
                    {
                        Name = "%",
                        MinLimit = min,
                        MaxLimit = max
                    })
            };

            return new ChartResult
            {
                Series = series,
                XAxis = xAxis,
                YAxis = yAxes
            };
        }

        /// <summary>
        /// Generates LiveCharts Cartesian series and axes configurations for low blood oxygen saturation duration over a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the filtering range.</param>
        /// <param name="endDate">The end date of the filtering range.</param>
        /// <param name="data">The collection of oxygen saturation dashboard models.</param>
        /// <param name="isTrendLineLegendVisible">A value indicating whether trend lines should appear in the chart legend.</param>
        /// <param name="strokeThickness">The stroke thickness for the chart lines.</param>
        /// <param name="geometrySize">The size of the data point geometries on the chart.</param>
        /// <param name="loessFraction">The LOESS smoothing fraction used for calculating trend lines.</param>
        /// <param name="showSpO2Duration">A value indicating whether the low SpO2 duration series is visible.</param>
        /// <param name="showSpO2DurationTrend">A value indicating whether the low SpO2 duration trend line is visible.</param>
        /// <returns>A <see cref="ChartResult"/> containing the generated series array, X-axis array, and Y-axes array.</returns>
        public static ChartResult CreateLowSpO2DurationChart(
            DateTime startDate,
            DateTime endDate,
            List<SamsungOxygenSaturationDashboardModel> data,
            bool isTrendLineLegendVisible,
            float strokeThickness,
            float geometrySize,
            double loessFraction,
            bool showSpO2Duration,
            bool showSpO2DurationTrend)
        {
            double max = data != null && data.Any() ? data.Max(x => x.LowSpO2Duration ?? 0) : 100;
            double min = data != null && data.Any() ? data.Min(x => x.LowSpO2Duration ?? 0) : 0;

            max += max * 0.05;

            var chartDefinitions = new[]
            {
                new ChartSeriesModel<SamsungOxygenSaturationDashboardModel>
                {
                    Name = "Low SpO2 Durataion",
                    Color = SKColors.Orange,
                    YAxisIndex = 0,

                    IsVisible = showSpO2Duration,
                    ShowTrend = showSpO2DurationTrend,
                    IsTrendLineVisible = isTrendLineLegendVisible,

                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.StartTime,
                    ValueSelector = x => x.LowSpO2Duration
                }
            };

            var series = ChartSeriesBuilder
                .CreateSeries(data, chartDefinitions, loessFraction)
                .ToArray();

            var xAxis = new ICartesianAxis[]
            {
                new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM"))
            };

            var yAxes = new[]
            {
                ChartAxisBuilder.Create(
                    new ChartYAxisModel
                    {
                        Name = "min",
                        MinLimit = min,
                        MaxLimit = max
                    })
            };

            return new ChartResult
            {
                Series = series,
                XAxis = xAxis,
                YAxis = yAxes
            };
        }

        #endregion

        #region Workout Log Chart Templates

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
        public static ChartResult CreateWorkloadMonthlyVolumeChart(DateTime startDate,
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
                    //Name = "Date",
                  
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

            return new ChartResult { Series = series, XAxis = XAxis, YAxis = YAxes };
        }

        /// <summary>
        /// Generates a combined column chart result showing the monthly exercise training volume (in kg) 
        /// and total monthly workouts within a specified date range.
        /// </summary>
        /// <param name="startDate">The start date for filtering the training volume records.</param>
        /// <param name="endDate">The end date for filtering the training volume records.</param>
        /// <param name="data">The raw collection of monthly exercise training volume records.</param>
        /// <param name="strokeThickness">The thickness of chart strokes and separator lines.</param>
        /// <param name="geometrySize">The size of the data point geometries rendered in the chart series.</param>
        /// <param name="labelRotation">The rotation angle for the X-axis labels (default is 0).</param>
        /// <returns>A <see cref="ChartResult"/> containing the configured series, X-axis, and Y-axes definitions.</returns>
        public static ChartResult CreateWorkloadMonthlyVolumeBarChart(
            DateTime startDate,
            DateTime endDate,
            List<MonthlyExerciseTrainingVolumeModel> data,
            float strokeThickness,
            float geometrySize,
            int labelRotation = 0)
        {
            var ordered = data
                  .Where(d => d.Date >= startDate && d.Date <= endDate)
                  .Where(d => d.TotalWeight.HasValue && d.TotalWeight.Value > 0)
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
                Color = SKColors.IndianRed,
                YAxisIndex = 0,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                IsVisible = true,

                DateSelector = x => x.Date,
                ValueSelector = x => x.TotalWeight
            },

            new ChartSeriesModel<MonthlyExerciseTrainingVolumeModel>
            {
                Name = "Monthly Workouts",
                Color = SKColors.DarkGreen,
                YAxisIndex = 1,

                StrokeThickness = strokeThickness,
                GeometrySize = geometrySize,

                IsVisible = true,

                DateSelector = x => x.Date,
                ValueSelector = x => (double?)x.TotalExercises
            }
        };


            var series = ChartSeriesBuilder.CreateColumnSeries(ordered, chartDefinitions).ToArray();
            var monthLabels = ordered.Select(d => d.Date.ToString("dd.MM.yy")).ToArray();


            var XAxis = new ICartesianAxis[]
            {
                new Axis
                {
                    //Name = "Month",
                    Labels = monthLabels,

                    MinStep = 1,
                    ForceStepToMin = true,

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                    {
                        StrokeThickness = strokeThickness,
                        PathEffect = new DashEffect(new float[] { 4, 4 })
                    },
                    ShowSeparatorLines = true,

                    LabelsRotation = labelRotation
                }
            };

            var yAxisDefinitions = new[]
            {
                new ChartYAxisModel
                    {
                        Name = "Volume kg",
                    },
                    new ChartYAxisModel
                    {
                        Name = "Workouts",
                        Position = AxisPosition.End,
                        ShowSeparatorLines = false
                    }
            };

            var YAxes = yAxisDefinitions.Select(ChartAxisBuilder.Create).ToArray();

            return new ChartResult { Series = series, XAxis = XAxis, YAxis = YAxes };
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
        public static (ISeries[], IPolarAxis[], IPolarAxis[]) CreateWorkloadMuscleSpiderChart(List<MuscleDataResultsModel> results, string seriesName,
            float strokeThickness = 1, float geometrySize = 1,
            int textSize = 12, double minStep = 0.5, int labelRotation = 0)
        {
            ISeries[] seriesValues = Array.Empty<ISeries>();

            IPolarAxis[] angleAxis = Array.Empty<IPolarAxis>();

            IPolarAxis[] radiusAxis = Array.Empty<IPolarAxis>();

            var max = results
              .Where(d => d.PercentageShare.HasValue)
              .Max(d => d.PercentageShare) ?? 0;


           


            if (results.Any())
            {
                var muscleDistributionValues = new List<double>();
                var muscleDistributionSpiderChartAxisName = new List<string>();
                foreach (var value in results)
                {
                    if (value == null) break;
                    muscleDistributionValues.Add(value.PercentageShare ?? 0);
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
                    MaxLimit = max,
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
        public static (ISeries[], IPolarAxis[], IPolarAxis[]) CreateWorkloadMuscleSpiderChart(List<MuscleDataResultsModel> actResults, List<MuscleDataResultsModel> prevResults,
            string actSereiesName, string prevSeriesName, float strokeThickness = 1,
            float geometrySize = 1, int textSize = 12, int minStep = 1, int labelRotation = 0)
        {
            ISeries[] seriesValues = Array.Empty<ISeries>();

            IPolarAxis[] angleAxis = Array.Empty<IPolarAxis>();

            IPolarAxis[] radiusAxis = Array.Empty<IPolarAxis>();

            var maxPercentageActResults = actResults
                .Where(d => d.PercentageShare.HasValue)
                .Max(d => d.PercentageShare) ?? 0;

            var maxPercentagePrevResults = prevResults
                .Where(d => d.PercentageShare.HasValue)
                .Max(d => d.PercentageShare) ?? 0;

            double max = Math.Max(maxPercentageActResults, maxPercentagePrevResults);

            var actMuscleDistributionSpiderChartValues = new List<double>();
            var actMuscleDistributionSpiderChartAxisName = new List<string>();
            var prevMuscleDistributionSpiderChartValues = new List<double>();

            if (actResults.Any())
            {
                foreach (var value in actResults)
                {
                    if (value == null) break;
                    actMuscleDistributionSpiderChartValues.Add(value.PercentageShare ?? 0);
                    actMuscleDistributionSpiderChartAxisName.Add(value.MuscleGroup);
                }
            }
            if (prevResults.Any())
            {
                foreach (var value in prevResults)
                {
                    if (value == null) break;
                    prevMuscleDistributionSpiderChartValues.Add(value.PercentageShare ?? 0);
                    if(!actResults.Any()) actMuscleDistributionSpiderChartAxisName.Add(value.MuscleGroup);
                }
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
                    MaxLimit = max,
                    MinStep= minStep,
                    Labeler = _ => string.Empty
                }
            };


            return (seriesValues, angleAxis, radiusAxis);
        }


        /// <summary>
        /// Generates LiveCharts Cartesian series and axes configurations for exercise heart rate metrics (Mean, Max, Min) over a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the filtering range.</param>
        /// <param name="endDate">The end date of the filtering range.</param>
        /// <param name="data">The collection of exercise dashboard models containing heart rate metrics.</param>
        /// <param name="isTrendLineLegendVisible">A value indicating whether trend lines should appear in the chart legend.</param>
        /// <param name="strokeThickness">The stroke thickness for the chart lines.</param>
        /// <param name="geometrySize">The size of the data point geometries on the chart.</param>
        /// <param name="loessFraction">The LOESS smoothing fraction used for calculating trend lines.</param>
        /// <param name="showHR">A value indicating whether the mean heart rate series is visible.</param>
        /// <param name="showHRTrend">A value indicating whether the mean heart rate trend line is visible.</param>
        /// <param name="showMaxHR">A value indicating whether the max heart rate series is visible.</param>
        /// <param name="showMaxHRTrend">A value indicating whether the max heart rate trend line is visible.</param>
        /// <param name="showMinHR">A value indicating whether the min heart rate series is visible.</param>
        /// <param name="showMinHRTrend">A value indicating whether the min heart rate trend line is visible.</param>
        /// <returns>A <see cref="ChartResult"/> containing the generated series array, X-axis array, and Y-axes array.</returns>
        public static ChartResult CreateWorkoutPeakWeightBarChart(
            List<WorkoutExerciseProgressModel> data,
            bool isTrendLineLegendVisible,
            float strokeThickness,
            float geometrySize,
            string label,
            int labelRotation = 0)
        {

            double max = data != null && data.Any() ? data.Max(x => x.PeakWeight) : 100;
            double min = data != null && data.Any() ? data.Min(x => x.PeakWeight) : 0;

            max += max * 0.05;
            min -= min * 0.05;

            var chartDefinitions = new[]
            {
                new ChartSeriesModel<WorkoutExerciseProgressModel>
                {
                    Name = label,
                    Color = SKColors.DarkRed,
                    YAxisIndex = 0,
                    IsTrendLineVisible = isTrendLineLegendVisible,
                    IsVisible = true,
                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.Date,
                    ValueSelector = x => x.PeakWeight,

                },
            };

            var series = ChartSeriesBuilder.CreateColumnSeries(data, chartDefinitions).ToArray();
            var monthLabels = data.Select(d => d.Date.ToString("dd.MM.yy")).ToArray();


            var XAxis = new ICartesianAxis[]
            {
                new Axis
                {
                    //Name = "Month",
                    Labels = monthLabels,

                    MinStep = 1,
                    ForceStepToMin = true,

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                    {
                        StrokeThickness = strokeThickness,
                        PathEffect = new DashEffect(new float[] { 4, 4 })
                    },
                    ShowSeparatorLines = true,
                                        LabelsPaint = null,

                    LabelsRotation = labelRotation
                }
            };

            var yAxisDefinitions = new[]
            {
                new ChartYAxisModel
                    {
                        Name = "kg",
                    }
            };

            var YAxes = yAxisDefinitions.Select(ChartAxisBuilder.Create).ToArray();

            return new ChartResult
            {
                Series = series,
                XAxis = XAxis,
                YAxis = YAxes
            };
        }

        /// <summary>
        /// Generates LiveCharts Cartesian series and axes configurations for exercise heart rate metrics (Mean, Max, Min) over a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the filtering range.</param>
        /// <param name="endDate">The end date of the filtering range.</param>
        /// <param name="data">The collection of exercise dashboard models containing heart rate metrics.</param>
        /// <param name="isTrendLineLegendVisible">A value indicating whether trend lines should appear in the chart legend.</param>
        /// <param name="strokeThickness">The stroke thickness for the chart lines.</param>
        /// <param name="geometrySize">The size of the data point geometries on the chart.</param>
        /// <param name="loessFraction">The LOESS smoothing fraction used for calculating trend lines.</param>
        /// <param name="showHR">A value indicating whether the mean heart rate series is visible.</param>
        /// <param name="showHRTrend">A value indicating whether the mean heart rate trend line is visible.</param>
        /// <param name="showMaxHR">A value indicating whether the max heart rate series is visible.</param>
        /// <param name="showMaxHRTrend">A value indicating whether the max heart rate trend line is visible.</param>
        /// <param name="showMinHR">A value indicating whether the min heart rate series is visible.</param>
        /// <param name="showMinHRTrend">A value indicating whether the min heart rate trend line is visible.</param>
        /// <returns>A <see cref="ChartResult"/> containing the generated series array, X-axis array, and Y-axes array.</returns>
        public static ChartResult CreateWorkoutTotalVolumeBarChart(
            List<WorkoutExerciseProgressModel> data,
            bool isTrendLineLegendVisible,
            float strokeThickness,
            float geometrySize,
            string label,
            int labelRotation = 0)
        {
            double max = data != null && data.Any() ? data.Max(x => x.TotalVolume) : 100;
            double min = data != null && data.Any() ? data.Min(x => x.TotalVolume) : 0;

            max += max * 0.05;
            min -= min * 0.05;

            var chartDefinitions = new[]
            {
                new ChartSeriesModel<WorkoutExerciseProgressModel>
                {
                    Name = label,
                    Color = SKColors.DarkSeaGreen,
                    YAxisIndex = 0,
                    IsTrendLineVisible = isTrendLineLegendVisible,
                    IsVisible = true,
                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.Date,
                    ValueSelector = x => x.TotalVolume,

                },
            };

            var series = ChartSeriesBuilder.CreateColumnSeries(data, chartDefinitions).ToArray();
            var monthLabels = data.Select(d => d.Date.ToString("dd.MM.yy")).ToArray();


            var XAxis = new ICartesianAxis[]
            {
                new Axis
                {
                    //Name = "Month",
                    Labels = monthLabels,

                    MinStep = 1,
                    ForceStepToMin = true,

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray)
                    {
                        StrokeThickness = strokeThickness,
                        PathEffect = new DashEffect(new float[] { 4, 4 })
                    },
                    ShowSeparatorLines = true,
                    LabelsPaint = null,
                    LabelsRotation = labelRotation
                }
            };

            var yAxisDefinitions = new[]
            {
                new ChartYAxisModel
                    {
                        Name = "kg",
                    }
            };

            var YAxes = yAxisDefinitions.Select(ChartAxisBuilder.Create).ToArray();

            return new ChartResult
            {
                Series = series,
                XAxis = XAxis,
                YAxis = YAxes
            };
        }

        /// <summary>
        /// Generates LiveCharts Cartesian series and axes configurations for exercise heart rate metrics (Mean, Max, Min) over a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the filtering range.</param>
        /// <param name="endDate">The end date of the filtering range.</param>
        /// <param name="data">The collection of exercise dashboard models containing heart rate metrics.</param>
        /// <param name="isTrendLineLegendVisible">A value indicating whether trend lines should appear in the chart legend.</param>
        /// <param name="strokeThickness">The stroke thickness for the chart lines.</param>
        /// <param name="geometrySize">The size of the data point geometries on the chart.</param>
        /// <param name="loessFraction">The LOESS smoothing fraction used for calculating trend lines.</param>
        /// <param name="showHR">A value indicating whether the mean heart rate series is visible.</param>
        /// <param name="showHRTrend">A value indicating whether the mean heart rate trend line is visible.</param>
        /// <param name="showMaxHR">A value indicating whether the max heart rate series is visible.</param>
        /// <param name="showMaxHRTrend">A value indicating whether the max heart rate trend line is visible.</param>
        /// <param name="showMinHR">A value indicating whether the min heart rate series is visible.</param>
        /// <param name="showMinHRTrend">A value indicating whether the min heart rate trend line is visible.</param>
        /// <returns>A <see cref="ChartResult"/> containing the generated series array, X-axis array, and Y-axes array.</returns>
        public static ChartResult CreateWorkoutRepChart(
            List<WorkoutExerciseProgressModel> data,
            bool isTrendLineLegendVisible,
            float strokeThickness,
            float geometrySize,
            string label,
            double loessFraction,
            bool showMaxRep,
            bool showMaxRepTrend)
        {
            double max = data != null && data.Any() ? data.Max(x => x.MaxOneRepMax) : 100;
            double min = data != null && data.Any() ? data.Min(x => x.MaxOneRepMax) : 0;

            max += max * 0.05;

            var chartDefinitions = new[]
            {
                new ChartSeriesModel<WorkoutExerciseProgressModel>
                {
                    Name = label,
                    Color = SKColors.Green,
                    YAxisIndex = 0,

                    IsVisible = showMaxRep,
                    ShowTrend = showMaxRepTrend,
                    IsTrendLineVisible = isTrendLineLegendVisible,

                    StrokeThickness = strokeThickness,
                    GeometrySize = geometrySize,

                    DateSelector = x => x.Date,
                    ValueSelector = x => x.MaxOneRepMax
                },

            };

            var series = ChartSeriesBuilder
                .CreateSeries(data, chartDefinitions, loessFraction)
                .ToArray();

            var xAxis = new ICartesianAxis[]
            {
                new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM")) {LabelsPaint = null}
            };

            var yAxes = new[]
            {
                ChartAxisBuilder.Create(
                    new ChartYAxisModel
                    {
                        Name = "kg",
                        MinLimit = min,
                        MaxLimit = max
                    })
            };

            return new ChartResult
            {
                Series = series,
                XAxis = xAxis,
                YAxis = yAxes
            };
        }


        #endregion





    }
}
