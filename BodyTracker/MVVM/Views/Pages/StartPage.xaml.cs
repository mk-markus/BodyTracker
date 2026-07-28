using BodyTracker.MVVM.ViewModels;
using BodyTracker.Services;
using System.Windows;
using System.Windows.Controls;


namespace BodyTracker.MVVM.Views.Pages
{
    /// <summary>
    /// Interaktionslogik für StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {

        /// <summary>
        /// Reference to the main application window, acting as the primary host (Shell) 
        /// for navigation, status updates, and top-level UI orchestration.
        /// </summary>
        private readonly MainWindow mainWindow;

        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the ViewModel.
        /// </summary>
        private readonly DatabaseService databaseService;

        private DashboardPage newDashboardPage;

        private ChartsMeasurementsPage chartsMeasurementsPage;

        private MultiImportPage multiChartPage;

        private  StepDailyTrendImportPage newStepDailyTrendPage;

        private readonly StartPageViewModel startpageViewModel;

        public StartPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();
            mainWindow = shell;
            databaseService = db;
            mainWindow.DataContext = this;

            startpageViewModel = new StartPageViewModel(mainWindow, databaseService);

            DataContext = startpageViewModel;

        }

    }
}
