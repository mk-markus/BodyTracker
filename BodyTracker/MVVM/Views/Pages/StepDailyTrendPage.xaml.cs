using BodyTracker.MVVM.ViewModels;
using BodyTracker.Services;
using System.Windows.Controls;

namespace BodyTracker.MVVM.Views.Pages
{
    /// <summary>
    /// Interaktionslogik für StepDailyTrendPage.xaml
    /// </summary>
    public partial class StepDailyTrendPage : Page
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
        /// The extractor service used to parse Samsung Health step trend data from CSV files.
        /// </summary>
        private SamsungHealthDataCsvExtractor samsungHealthDataCsvExtractor = new SamsungHealthDataCsvExtractor();

        /// <summary>
        /// The view model instance managing the data and logic for the daily step trend view.
        /// </summary>
        private readonly StepDailyTrendViewModel stepDailyTrendViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepDailyTrendPage"/> class.
        /// </summary>
        /// <param name="shell">The main window shell reference.</param>
        /// <param name="db">The <see cref="DatabaseService"/> instance used for data operations.</param>
        public StepDailyTrendPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();
            _shell = shell;

            databaseService = db;

            stepDailyTrendViewModel = new StepDailyTrendViewModel(databaseService);

            DataContext = stepDailyTrendViewModel;
        }
    }
}
