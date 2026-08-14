using BodyTracker.ViewModels;
using BodyTracker.Services;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages
{
    /// <summary>
    /// Interaktionslogik für StepDailyTrendImportPage.xaml
    /// </summary>
    public partial class StepDailyTrendImportPage : Page
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
        /// The view model instance managing the data and logic for the daily step trend view.
        /// </summary>
        private readonly StepDailyTrendViewModel stepDailyTrendViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepDailyTrendImportPage"/> class.
        /// </summary>
        /// <param name="shell">The main window shell reference.</param>
        /// <param name="db">The <see cref="DatabaseService"/> instance used for data operations.</param>
        public StepDailyTrendImportPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();
            _shell = shell;

            databaseService = db;

            stepDailyTrendViewModel = new StepDailyTrendViewModel(databaseService);

            DataContext = stepDailyTrendViewModel;

            Unloaded += (s, e) => stepDailyTrendViewModel.Dispose();
        }
    }
}
