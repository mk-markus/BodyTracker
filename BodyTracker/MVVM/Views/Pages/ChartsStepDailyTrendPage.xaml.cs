using BodyTracker.MVVM.ViewModels;
using BodyTracker.Services;
using System.Windows.Controls;

namespace BodyTracker.MVVM.Views.Pages
{
    /// <summary>
    /// Interaktionslogik für ChartsStepDailyTrendPage.xaml
    /// </summary>
    public partial class ChartsStepDailyTrendPage : Page
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
        /// body measurement records and user profiles.
        /// </summary>
        private readonly DatabaseService databaseService;
        
        /// <summary>
        /// The view model instance managing the data, logic, and chart configurations for the daily step trends view.
        /// </summary>
        private readonly ChartsStepsDailyTrendPageViewModel chartsStepsDailyTrendPageViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartsStepDailyTrendPage"/> class.
        /// </summary>
        /// <param name="shell">The main window shell reference.</param>
        /// <param name="db">The <see cref="DatabaseService"/> instance used for data operations.</param>
        public ChartsStepDailyTrendPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();

            _shell = shell;

            databaseService = db;

            chartsStepsDailyTrendPageViewModel = new ChartsStepsDailyTrendPageViewModel(databaseService);

            DataContext = chartsStepsDailyTrendPageViewModel;

            // Automatically refresh the chart data once the page is fully loaded.
            Loaded += async (s, e) => await chartsStepsDailyTrendPageViewModel.RefreshChartAsync();
        }
    }
}
