using BodyTracker.Services;
using BodyTracker.ViewModels.Main;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages.Main
{
    /// <summary>
    /// Interaktionslogik für HeartRateImportView.xaml
    /// </summary>
    public partial class HeartRateImportView : Page
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// Serves as the primary Data Access Layer (DAL) for retrieving and persisting 
        /// body bodyMeasurement records and user profiles.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// The view model instance managing the data and logic for the heart rate view.
        /// </summary>
        private readonly HeartRateImportViewModel heartRateImportVM;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeartRateImportView"/> class, assigning the database service and file path references, initializing the workout import view model, setting the data context, and registering the unload disposal handler.
        /// </summary>
        /// <param name="db">The database service instance used for data persistence.</param>
        /// <param name="path">The file path used to initialize the import view model.</param>
        public HeartRateImportView(DatabaseService db)
        {
            InitializeComponent();
           
            databaseService = db;

            heartRateImportVM = new HeartRateImportViewModel(databaseService);

            DataContext = heartRateImportVM;

            Unloaded += (s, e) => heartRateImportVM.Dispose();

        }
    }
}
