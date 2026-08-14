using BodyTracker.ViewModels;
using BodyTracker.Services;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages
{
    /// <summary>
    /// Interaktionslogik für PersonalWorkoutInsightsPage.xaml
    /// </summary>
    public partial class PersonalWorkoutInsightsPage : Page
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

        private readonly PersonalWorkoutInsightsPageViewModel personalWorkoutInsightsPageViewModel;



        public PersonalWorkoutInsightsPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();
            mainWindow = shell;
            databaseService = db;

            mainWindow.DataContext = this;

            personalWorkoutInsightsPageViewModel = new PersonalWorkoutInsightsPageViewModel(shell, db);

            DataContext = personalWorkoutInsightsPageViewModel;

            Unloaded += (s, e) => personalWorkoutInsightsPageViewModel.Dispose();

        }
    }
}
