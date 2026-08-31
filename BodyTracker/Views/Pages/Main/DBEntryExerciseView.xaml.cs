using BodyTracker.Services;
using BodyTracker.ViewModels.Main;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages.Main
{
    /// <summary>
    /// Interaktionslogik für DBEntryExerciseView.xaml
    /// </summary>
    public partial class DBEntryExerciseView : Page
    {
        // <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the ViewModel.
        /// </summary>
        private readonly DatabaseService databaseService;
        /// <summary>
        /// A private, read-only reference to the <see cref="DBEntryExerciseViewModel"/> 
        /// that serves as the data context and handles the business logic for exercise entries.
        /// </summary>
        private readonly DBEntryExerciseViewModel dBEntryExerciseViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBEntryExerciseView"/> class 
        /// using the specified database service for data persistence and exercise management.
        /// </summary>
        /// <param name="db">The database service instance used to initialize data operations.</param>
        public DBEntryExerciseView(DatabaseService db)
        {
            InitializeComponent();

            databaseService = db;

            dBEntryExerciseViewModel = new DBEntryExerciseViewModel(databaseService);

            DataContext = dBEntryExerciseViewModel;

            Unloaded += (s, e) => dBEntryExerciseViewModel.Dispose();
        }
    }
}
