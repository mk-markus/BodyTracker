using BodyTracker.Services;
using BodyTracker.ViewModels;
using System.Web;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages
{
    /// <summary>
    /// Interaktionslogik für WorkoutImportView.xaml
    /// </summary>
    public partial class WorkoutImportView : Page
    {

        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// Serves as the primary Data Access Layer (DAL) for retrieving and persisting 
        /// body bodyMeasurement records and user profiles.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// The view model instance managing the data and logic for the Hevy app view.
        /// </summary>
        private readonly WorkoutImportViewModel workoutImportVM;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkoutImportView"/> class, assigning the database service and file path references, initializing the workout import view model, setting the data context, and registering the unload disposal handler.
        /// </summary>
        /// <param name="db">The database service instance used for data persistence.</param>
        /// <param name="path">The file path used to initialize the import view model.</param>
        public WorkoutImportView(DatabaseService db)
        {
            InitializeComponent();

            databaseService = db;

            workoutImportVM = new WorkoutImportViewModel(databaseService);

            DataContext = workoutImportVM;

            Unloaded += (s, e) => workoutImportVM.Dispose();
        }
    }
}
