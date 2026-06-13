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

        }
    }
}
