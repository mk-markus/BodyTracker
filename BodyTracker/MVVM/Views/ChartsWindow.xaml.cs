using System;
using System.Linq;
using System.Windows;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Measure;
using BodyTracker.Services;
using BodyTracker.State;
using LiveChartsCore.Kernel.Sketches;

namespace BodyTracker.Views
{
    public partial class ChartsWindow : Window
    {
        public ISeries[] Series { get; set; } = Array.Empty<ISeries>();
        public ICartesianAxis[] XAxes { get; set; } = Array.Empty<ICartesianAxis>();
        public ICartesianAxis[] YAxes { get; set; } = Array.Empty<ICartesianAxis>();
        private readonly DatabaseService _db;
        
        public ChartsWindow(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
            Loaded += async (s,e) =>
            {
                var data = await _db.GetBodyMeasurementAsync(AppState.SelectedPersonId);
                var ordered = data.OrderBy(d => d.MeasurementDate).ToList();
                Series = new ISeries[]
                {
                    new LineSeries<DateTimePoint>
                    {
                        Name = "Gewicht (kg)",
                        Values = ordered.Select(d => new DateTimePoint(d.MeasurementDate, (double)(d.BodyWeight ?? 0))).ToArray(),
                        Fill = null,
                        ScalesYAt = 0
                    },
                    new LineSeries<DateTimePoint>
                    {
                        Name = "Körperfett (%)",
                        Values = ordered.Select(d => new DateTimePoint(d.MeasurementDate, (double)(d.BodyFatPercentage ?? 0))).ToArray(),
                        Fill = null,
                        ScalesYAt = 1
                    },
                    new LineSeries<DateTimePoint>
                    {
                        Name = "Muskelmasse (%)",
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
                DataContext = this;
            };
        }
    }
}
