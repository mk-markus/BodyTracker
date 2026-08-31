using BodyTracker.Services;
using System.Windows.Controls;

namespace BodyTracker.ViewModels.Main
{
    /// <summary>
    /// Interaktionslogik für ExerciseChartView.xaml
    /// </summary>
    public partial class ExerciseChartView : Page
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// Serves as the primary Data Access Layer (DAL) for retrieving and persisting 
        /// exercise and user profile records.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// A private, read-only reference to the <see cref="ExerciseChartViewModel"/> 
        /// that handles the business logic and state for the exercise charts.
        /// </summary>
        private readonly ExerciseChartViewModel exerciseChartVM;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExerciseChartView"/> class.
        /// </summary>
        /// <param name="db">The database service instance used for data access operations.</param>
        public ExerciseChartView(DatabaseService db)
        {
            InitializeComponent();
            databaseService = db;

            exerciseChartVM = new ExerciseChartViewModel(databaseService);

            DataContext = exerciseChartVM;

            Unloaded += (s, e) => exerciseChartVM.Dispose();
        }
    }
}
