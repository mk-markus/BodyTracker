using BodyTracker.Services;
using BodyTracker.ViewModels;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages
{
    /// <summary>
    /// Interaktionslogik für MultiImportView.xaml
    /// </summary>
    public partial class MultiChartsView : Page
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
        /// A private, read-only reference to the <see cref="MultiChartViewModel"/> that manages chart data, series collections, and filtering logic for the multi-charts view.
        /// </summary>
        private readonly MultiChartViewModel multiChartViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiChartsView"/> class, assigning the main window and database service references, setting data contexts, initializing the multi-chart view model, and registering the unload disposal handler.
        /// </summary>
        /// <param name="shell">The parent main window reference.</param>
        /// <param name="db">The database service instance used for querying chart data.</param>
        public MultiChartsView(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();
            mainWindow = shell;
            databaseService = db;

            mainWindow.DataContext = this;

            multiChartViewModel = new MultiChartViewModel(shell, db);

            DataContext = multiChartViewModel;

            Unloaded += (s, e) => multiChartViewModel.Dispose();
        }


    }
}
