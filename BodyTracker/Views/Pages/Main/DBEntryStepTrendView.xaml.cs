using BodyTracker.Services;
using BodyTracker.ViewModels.Main;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages.Main
{
    /// <summary>
    /// Interaktionslogik für DBEntryStepTrendView.xaml
    /// </summary>
    public partial class DBEntryStepTrendView : Page
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the ViewModel.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// A private, read-only reference to the <see cref="DBEntryStepTrendViewModel"/> 
        /// that serves as the data context and handles the business logic for step trend entries.
        /// </summary>
        private readonly DBEntryStepTrendViewModel dBEntryStepTrendViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBEntryStepTrendView"/> class 
        /// using the specified database service for data persistence and trend management.
        /// </summary>
        /// <param name="db">The database service instance used to initialize data operations.</param>
        public DBEntryStepTrendView(DatabaseService db)
        {
            InitializeComponent();

            databaseService = db;

            dBEntryStepTrendViewModel = new DBEntryStepTrendViewModel(databaseService);

            DataContext = dBEntryStepTrendViewModel;

            Unloaded += (s, e) => dBEntryStepTrendViewModel.Dispose();
        }
    }
}
