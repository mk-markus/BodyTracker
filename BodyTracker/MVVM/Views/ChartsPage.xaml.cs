using BodyTracker.MVVM.ViewModels;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BodyTracker.MVVM.Views
{
    

    /// <summary>
    /// Interaktionslogik für ChartsPage.xaml
    /// </summary>
    public partial class ChartsPage : Page
    {

        //public ISeries[] Series { get; set; } = Array.Empty<ISeries>();
        private readonly MainWindow _shell;
        //public ICartesianAxis[] XAxes { get; set; } = Array.Empty<ICartesianAxis>();
       // public ICartesianAxis[] YAxes { get; set; } = Array.Empty<ICartesianAxis>();
        private readonly DatabaseService _db;

       //private DateTime startDate = new DateTime();
       //private DateTime endDate = new DateTime();



        private readonly ChartsPageViewModel _chartsPageViewModel;



        public ChartsPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();
            _shell = shell;
            _db = db;
            
            _chartsPageViewModel = new ChartsPageViewModel(_db);
            DataContext = _chartsPageViewModel;
            Debug.WriteLine($"DataContext ist: {this.DataContext?.GetType().Name}");
            Loaded += async (s, e) => await _chartsPageViewModel.RefreshChartAsync();
            
        }



        //public ChartsPage(MainWindow shell, DatabaseService db)
        //{
        //    InitializeComponent();
        //    _db = db;
        //    _shell = shell;

        //    dpEndDate.SelectedDate = DateTime.Now;
        //    dpStartDate.SelectedDate = DateTime.Parse("01.01.2025");

        //    Loaded += async (s, e) => await RefreshChart();

        //    //Loaded += async (s, e) =>
        //    //{
        //    //    var data = await _db.GetBodyMeasurementAsync(AppState.SelectedPersonId);
        //    //    //var ordered = data.OrderBy(d => d.MeasurementDate).ToList();
        //    //    var ordered = data.Where(d => d.MeasurementDate >= dpStartDate.SelectedDate && d.MeasurementDate <= dpEndDate.SelectedDate).ToList();
        //    //    Series = new ISeries[]
        //    //    {
        //    //        new LineSeries<DateTimePoint>
        //    //        {
        //    //            Name = "Gewicht (kg)",
        //    //            Values = ordered.Select(d => new DateTimePoint(d.MeasurementDate, (double)(d.BodyWeight ?? 0))).ToArray(),
        //    //            Fill = null,
        //    //            ScalesYAt = 0
        //    //        },
        //    //        new LineSeries<DateTimePoint>
        //    //        {
        //    //            Name = "Körperfett (%)",
        //    //            Values = ordered.Select(d => new DateTimePoint(d.MeasurementDate, (double)(d.BodyFatPercentage ?? 0))).ToArray(),
        //    //            Fill = null,
        //    //            ScalesYAt = 1
        //    //        },
        //    //        new LineSeries<DateTimePoint>
        //    //        {
        //    //            Name = "Muskelmasse (%)",
        //    //            Values = ordered.Select(d => new DateTimePoint(d.MeasurementDate, (double)(d.BodyMusclePercentage ?? 0))).ToArray(),
        //    //            Fill = null,
        //    //            ScalesYAt = 1
        //    //        }
        //    //    };
        //    //    XAxes = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yyyy")) { Name = "Datum" } };
        //    //    YAxes = new ICartesianAxis[]
        //    //    {
        //    //        new Axis { Name = "kg" },
        //    //        new Axis { Name = "%", Position = AxisPosition.End, ShowSeparatorLines = false }
        //    //    };
        //    //    DataContext = this;
        //    //};
        //}



        //private async Task RefreshChart()
        //{
        //    var data = await _db.GetBodyMeasurementAsync(AppState.SelectedPersonId);
        //    //var ordered = data.OrderBy(d => d.MeasurementDate).ToList();
        //    var ordered = data.Where(d => d.MeasurementDate >= dpStartDate.SelectedDate && d.MeasurementDate <= dpEndDate.SelectedDate).ToList();
        //    Series = new ISeries[]
        //    {
        //            new LineSeries<DateTimePoint>
        //            {
        //                Name = "Gewicht (kg)",
        //                Values = ordered.Select(d => new DateTimePoint(d.MeasurementDate, (double)(d.BodyWeight ?? 0))).ToArray(),
        //                Fill = null,
        //                ScalesYAt = 0
        //            },
        //            new LineSeries<DateTimePoint>
        //            {
        //                Name = "Körperfett (%)",
        //                Values = ordered.Select(d => new DateTimePoint(d.MeasurementDate, (double)(d.BodyFatPercentage ?? 0))).ToArray(),
        //                Fill = null,
        //                ScalesYAt = 1
        //            },
        //            new LineSeries<DateTimePoint>
        //            {
        //                Name = "Muskelmasse (%)",
        //                Values = ordered.Select(d => new DateTimePoint(d.MeasurementDate, (double)(d.BodyMusclePercentage ?? 0))).ToArray(),
        //                Fill = null,
        //                ScalesYAt = 1
        //            }
        //    };
        //    XAxes = new ICartesianAxis[] { new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd.MM.yyyy")) { Name = "Datum" } };
        //    YAxes = new ICartesianAxis[]
        //    {
        //            new Axis { Name = "kg" },
        //            new Axis { Name = "%", Position = AxisPosition.End, ShowSeparatorLines = false }
        //    };

        //    DataContext = null;
        //    DataContext = this;
        //}


        ////private void RefreshChart()
        ////{
        ////    var start = dpStartDate.SelectedDate ?? DateTime.Parse("01.01.2025");


        ////}

        //public async void dpStartDate_SelectedChanged(object sender, SelectionChangedEventArgs e)
        //{
        //   await RefreshChart();
        //}
    }
}
