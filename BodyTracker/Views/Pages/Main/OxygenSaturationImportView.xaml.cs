using BodyTracker.Services;
using System.Windows.Controls;

namespace BodyTracker.ViewModels.Main
{
    /// <summary>
    /// Interaktionslogik für OxygenSaturationImportViewModel.xaml
    /// </summary>
    public partial class OxygenSaturationView : Page
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
        private readonly OxygenSaturationImportViewModel oxygenSaturationImportVM;

        /// <summary>
        /// A private, read-only string representing the default file searching path used when importing Hevy app data.
        /// </summary>
        private readonly string searchPath = string.Empty;

        public OxygenSaturationView(DatabaseService db, string path)
        {
            InitializeComponent();
            databaseService = db;

            searchPath = path;

            oxygenSaturationImportVM = new OxygenSaturationImportViewModel(databaseService, searchPath);

            DataContext = oxygenSaturationImportVM;

            Unloaded += (s, e) => oxygenSaturationImportVM.Dispose();
        }
    }
}
