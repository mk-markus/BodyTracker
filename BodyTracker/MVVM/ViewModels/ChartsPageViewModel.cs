using BodyTracker.MVVM.Views;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace BodyTracker.MVVM.ViewModels
{

    public partial class ChartsPageViewModel : ObservableObject
    {
        private readonly DatabaseService _db;

        [ObservableProperty]
        private ISeries[] series = Array.Empty<ISeries>();

        [ObservableProperty]
        private ICartesianAxis[] xAxes = Array.Empty<ICartesianAxis>();

        [ObservableProperty]
        private ICartesianAxis[] yAxes = Array.Empty<ICartesianAxis>();

        [ObservableProperty]
        private DateTime startDate = new DateTime(2025,1,1);

        [ObservableProperty]
        private DateTime endDate = DateTime.Now;

        public ChartsPageViewModel(DatabaseService db)
        {
            _db = db;
          

            XAxes = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yyyy")) { Name = "Datum" }
        };
        }

        /// <summary>
        /// Wird automatisch aufgerufen, wenn sich das StartDatum ändert.
        /// </summary>
        partial void OnStartDateChanged(DateTime value)
        {
            _ = RefreshChartAsync();
        }
        /// <summary>
        /// Wird automatisch aufgerufen, wenn sich das EndDatum ändert.
        /// </summary>
        partial void OnEndDateChanged(DateTime value)
        {
           _ = RefreshChartAsync();
        }

        public async Task RefreshChartAsync()
        {
            if (AppState.SelectedPersonId <= 0) return;

            var data = await _db.GetBodyMeasurementAsync(AppState.SelectedPersonId);
            //var ordered = data.OrderBy(d => d.MeasurementDate).ToList();
            var ordered = data.Where(d => d.MeasurementDate >= StartDate && d.MeasurementDate <= EndDate).ToList();
            Series = new ISeries[]
            {
                    new LineSeries<DateTimePoint>
                    {
                        Name = "Gewicht (kg)",
                        LineSmoothness = 0,
                        Stroke = new SolidColorPaint(SKColors.Blue){StrokeThickness = 4},
                        GeometrySize=3,
                        GeometryStroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 4 },
                        Values = ordered.Select(d => new DateTimePoint(d.MeasurementDate, (double)(d.BodyWeight ?? 0))).ToArray(),
                        Fill = null,
                        ScalesYAt = 0
                    },
                    new LineSeries<DateTimePoint>
                    {
                        Name = "Körperfett (%)",
                        LineSmoothness = 0,
                        Stroke = new SolidColorPaint(SKColors.Red){StrokeThickness = 4},
                        GeometrySize=3,
                        GeometryStroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 4 },
                        Values = ordered.Select(d => new DateTimePoint(d.MeasurementDate, (double)(d.BodyFatPercentage ?? 0))).ToArray(),
                        Fill = null,
                        ScalesYAt = 1
                    },
                    new LineSeries<DateTimePoint>
                    {
                        Name = "Muskelmasse (%)",
                        LineSmoothness = 0,
                        Stroke = new SolidColorPaint(SKColors.Green){StrokeThickness = 4},
                        GeometrySize=3,
                        GeometryStroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 4 },
                        Values = ordered.Select(d => new DateTimePoint(d.MeasurementDate, (double)(d.BodyMusclePercentage ?? 0))).ToArray(),
                        Fill = null,
                        ScalesYAt = 1
                    }
            };
            XAxes = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yyyy")) { Name = "Datum" } };
            
            YAxes = new ICartesianAxis[]
            {
                    new Axis { Name = "kg" },
                    new Axis { Name = "%", Position = AxisPosition.End, ShowSeparatorLines = false }
            };
        }
    }
}
