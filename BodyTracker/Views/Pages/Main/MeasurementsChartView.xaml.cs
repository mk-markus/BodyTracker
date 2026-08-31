using BodyTracker.Services;
using BodyTracker.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace BodyTracker.Views
{
    

    /// <summary>
    /// Interaktionslogik für ChartsMeasurementsView.xaml
    /// </summary>
    public partial class ChartsMeasurementsView : Page
    {

        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// Serves as the primary Data Access Layer (DAL) for retrieving and persisting 
        /// body bodyMeasurement records and user profiles.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// A private, read-only reference to the <see cref="MeasurementChartViewModel"/>.
        /// Acts as the primary data context for the view, holding the business logic, 
        /// chart configurations, and observable data collections required for visualization.
        /// </summary>
        private readonly MeasurementChartViewModel chartsPageViewModel;


        /// <summary>
        /// Initializes a new instance of the <see cref="ChartsMeasurementsView"/> class.
        /// Configures dependency injection, establishes the data context for LiveCharts2, 
        /// and registers the initial data load routine.
        /// </summary>
        /// <param name="db">The database service providing access to the body bodyMeasurement records.</param>
        /// <remarks>
        /// This constructor facilitates the MVVM pattern by instantiating the <see cref="MeasurementChartViewModel"/> 
        /// with the provided database service. It also utilizes the <see cref="FrameworkElement.Loaded"/> 
        /// event to trigger an asynchronous chart refresh, ensuring that the visual data is 
        /// populated immediately after the page is rendered.
        /// </remarks>
        public ChartsMeasurementsView( DatabaseService db)
        {
            InitializeComponent();
            
            databaseService = db;
            
            chartsPageViewModel = new MeasurementChartViewModel(databaseService);
            
            DataContext = chartsPageViewModel;
         
            Unloaded += (s, e) => chartsPageViewModel.Dispose();

        }

    }
}
