using BodyTracker.Services;
using BodyTracker.ViewModels.Main;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages.Main
{
    /// <summary>
    /// Interaktionslogik für DBEntryFoodIntakeView.xaml
    /// </summary>
    public partial class DBEntryFoodIntakeView : Page
    {

        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the ViewModel.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// A private, read-only reference to the <see cref="DBEntryFoodIntakeViewModel"/> 
        /// that serves as the data context and handles the business logic for food intake entries.
        /// </summary>
        private readonly DBEntryFoodIntakeViewModel dBEntryFoodIntakeViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBEntryFoodIntakeView"/> class 
        /// using the specified database service for data persistence.
        /// </summary>
        /// <param name="db">The database service instance used to initialize data operations.</param>
        public DBEntryFoodIntakeView(DatabaseService db)
        {
            InitializeComponent();

            databaseService = db;

            dBEntryFoodIntakeViewModel = new DBEntryFoodIntakeViewModel(databaseService);

            DataContext = dBEntryFoodIntakeViewModel;

            Unloaded += (s, e) => dBEntryFoodIntakeViewModel.Dispose();
        }
    }
}
