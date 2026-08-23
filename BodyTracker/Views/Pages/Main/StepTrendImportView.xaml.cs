using BodyTracker.ViewModels;
using BodyTracker.Services;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages
{
    /// <summary>
    /// Interaktionslogik für StepTrendImportView.xaml
    /// </summary>
    public partial class StepTrendImportView : Page
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// Serves as the primary Data Access Layer (DAL) for retrieving and persisting 
        /// body bodyMeasurement records and user profiles.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// The view model instance managing the data and logic for the daily step trend view.
        /// </summary>
        private readonly StepTrendImportViewModel stepTrendImportVM;

        /// <summary>
        /// A private, read-only string representing the default file searching path used when importing Hevy app data.
        /// </summary>
        private readonly string searchPath = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepTrendImportView"/> class.
        /// </summary>
        /// <param name="db">The database service instance used for data persistence.</param>
        /// <param name="path">The file path used to initialize the import view model.</param>
        public StepTrendImportView(DatabaseService db, string path)
        {
            InitializeComponent();
            
            databaseService = db;

            searchPath = path;

            stepTrendImportVM = new StepTrendImportViewModel(db, searchPath);

            DataContext = stepTrendImportVM;

            Unloaded += (s, e) => stepTrendImportVM.Dispose();
        }
    }
}
