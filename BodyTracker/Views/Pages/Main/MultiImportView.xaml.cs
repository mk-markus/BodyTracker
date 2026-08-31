using BodyTracker.Services;
using BodyTracker.ViewModels;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages
{
    /// <summary>
    /// Interaktionslogik für MultiImportPage.xaml
    /// </summary>
    public partial class MultiImportPage : Page
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

        /// <summary>
        /// 
        /// </summary>
        private readonly MultiImportViewModel multiImportPageViewModel;


        public MultiImportPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();
            mainWindow = shell;
            databaseService = db;
            
            mainWindow.DataContext = this;

            multiImportPageViewModel = new MultiImportViewModel(mainWindow, databaseService);

            DataContext = multiImportPageViewModel;

            Unloaded += (s, e) => multiImportPageViewModel.Dispose();

        }

      
    }
}
