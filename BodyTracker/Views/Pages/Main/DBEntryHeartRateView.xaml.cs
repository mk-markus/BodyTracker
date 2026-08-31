using BodyTracker.Services;
using BodyTracker.ViewModels.Main;
using System.Windows.Controls;

namespace BodyTracker.Views.Pages.Main
{
    /// <summary>
    /// Interaktionslogik für DBEntryHeartRateView.xaml
    /// </summary>
    public partial class DBEntryHeartRateView : Page
    {

        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the ViewModel.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// A private, read-only reference to the <see cref="DBEntryHeartRateViewModel"/> 
        /// that serves as the data context and handles the business logic for heart rate entries.
        /// </summary>
        private readonly DBEntryHeartRateViewModel dBEntryHeartRateViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBEntryHeartRateView"/> class 
        /// using the specified database service for data persistence and heart rate management.
        /// </summary>
        /// <param name="db">The database service instance used to initialize data operations.</param>
        public DBEntryHeartRateView(DatabaseService db)
        {
            InitializeComponent();

            databaseService = db;

            dBEntryHeartRateViewModel = new DBEntryHeartRateViewModel(databaseService);

            DataContext = dBEntryHeartRateViewModel;

            Unloaded += (s, e) => dBEntryHeartRateViewModel.Dispose();
        }
    }
}
