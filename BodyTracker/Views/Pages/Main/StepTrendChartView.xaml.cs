using BodyTracker.ViewModels;
using BodyTracker.Services;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages
{
    /// <summary>
    /// Interaktionslogik für ChartsStepDailyTrendView.xaml
    /// </summary>
    public partial class ChartsStepDailyTrendView : Page
    {
        /// <summary>
        /// A private, read-only reference to the application's <see cref="MainWindow"/>.
        /// This reference, often referred to as the 'shell', is used to coordinate 
        /// top-level UI actions, such as navigation between different pages or 
        /// accessing global window states.
        /// </summary>
        private readonly MainWindow _shell;

        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// Serves as the primary Data Access Layer (DAL) for retrieving and persisting 
        /// body bodyMeasurement records and user profiles.
        /// </summary>
        private readonly DatabaseService databaseService;
        
        /// <summary>
        /// The view model instance managing the data, logic, and chart configurations for the daily step trends view.
        /// </summary>
        private readonly StepTrendChartViewModel chartsStepsDailyTrendPageViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartsStepDailyTrendView"/> class.
        /// </summary>
        /// <param name="shell">The main window shell reference.</param>
        /// <param name="db">The <see cref="DatabaseService"/> instance used for data operations.</param>
        public ChartsStepDailyTrendView(DatabaseService db)
        {
            InitializeComponent();

            databaseService = db;

            chartsStepsDailyTrendPageViewModel = new StepTrendChartViewModel(databaseService);

            DataContext = chartsStepsDailyTrendPageViewModel;

            Unloaded +=  (s, e) => chartsStepsDailyTrendPageViewModel.Dispose();
        }
    }
}
