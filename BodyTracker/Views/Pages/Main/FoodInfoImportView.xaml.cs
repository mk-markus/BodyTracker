using BodyTracker.Services;
using BodyTracker.ViewModels;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages
{
    /// <summary>
    /// Interaktionslogik für FoodInfoImportView.xaml
    /// </summary>
    public partial class FoodInfoImportView : Page
    {

        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// Serves as the primary Data Access Layer (DAL) for retrieving and persisting 
        /// body bodyMeasurement records and user profiles.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// The view model instance managing the data and logic for the food intake view.
        /// </summary>
        private readonly FoodInfoImportViewModel foodInfoVM;

        /// <summary>
        /// Initializes a new instance of the <see cref="FoodInfoImportView"/> class.
        /// </summary>
        /// <param name="db">The database service instance used for data persistence.</param>
        /// <param name="path">The file path used to initialize the import view model.</param>
        public FoodInfoImportView(DatabaseService db)
        {
            InitializeComponent();

            databaseService = db;

            foodInfoVM = new FoodInfoImportViewModel(databaseService);

            DataContext = foodInfoVM;

            Unloaded += (s, e) => foodInfoVM.Dispose();
        }
    }
}
