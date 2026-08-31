using BodyTracker.Services;
using BodyTracker.ViewModels.Main;
using System;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages.Main
{
    /// <summary>
    /// Interaktionslogik für HeartRateChartView.xaml
    /// </summary>
    public partial class HeartRateChartView : Page
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// Serves as the primary Data Access Layer (DAL) for retrieving heart rate records.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// The view model responsible for managing heart rate chart data and states.
        /// </summary>
        private readonly HeartRateChartViewModel heartRateChartVM;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeartRateChartView"/> class.
        /// </summary>
        /// <param name="db">The database service used for data retrieval.</param>
        public HeartRateChartView(DatabaseService db)
        {
            InitializeComponent();

            databaseService = db ?? throw new ArgumentNullException(nameof(db));
            heartRateChartVM = new HeartRateChartViewModel(databaseService);

            DataContext = heartRateChartVM;

            Unloaded += (s, e) => heartRateChartVM.Dispose();
        }
    }
}
