using BodyTracker.Services;
using BodyTracker.ViewModels.Main;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages.Main
{
    /// <summary>
    /// Interaktionslogik für DBEntryFoodInfoView.xaml
    /// </summary>
    public partial class DBEntryFoodInfoView : Page
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
        private readonly DBEntryFoodInfoViewModel dBEntryFoodInfoViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBEntryFoodInfoView"/> class 
        /// using the specified database service for data persistence.
        /// </summary>
        /// <param name="db">The database service instance used to initialize data operations.</param>
        public DBEntryFoodInfoView(DatabaseService db)
        {
            InitializeComponent();

            databaseService = db;

            dBEntryFoodInfoViewModel = new DBEntryFoodInfoViewModel(databaseService);

            DataContext = dBEntryFoodInfoViewModel;

            Unloaded += (s, e) => dBEntryFoodInfoViewModel.Dispose();
        }
    }
}
