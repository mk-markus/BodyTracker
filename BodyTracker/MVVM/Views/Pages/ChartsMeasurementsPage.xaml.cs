using BodyTracker.MVVM.ViewModels;
using BodyTracker.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BodyTracker.MVVM.Views
{
    

    /// <summary>
    /// Interaktionslogik für ChartsMeasurementsPage.xaml
    /// </summary>
    public partial class ChartsMeasurementsPage : Page
    {
        /// <summary>
        /// A private, read-only reference to the application's <see cref="MainWindow"/>.
        /// This reference, often referred to as the 'shell', is used to coordinate 
        /// top-level UI actions, such as navigation between different pages or 
        /// accessing global window states.
        /// </summary>
        private readonly MainWindow _shell;

        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// Serves as the primary Data Access Layer (DAL) for retrieving and persisting 
        /// body measurement records and user profiles.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// A private, read-only reference to the <see cref="ChartsMeasurementPageViewModel"/>.
        /// Acts as the primary data context for the view, holding the business logic, 
        /// chart configurations, and observable data collections required for visualization.
        /// </summary>
        private readonly ChartsMeasurementPageViewModel chartsPageViewModel;


        /// <summary>
        /// Initializes a new instance of the <see cref="ChartsMeasurementsPage"/> class.
        /// Configures dependency injection, establishes the data context for LiveCharts2, 
        /// and registers the initial data load routine.
        /// </summary>
        /// <param name="shell">The main application window (<see cref="MainWindow"/>) used for shell-level coordination.</param>
        /// <param name="db">The database service providing access to the body measurement records.</param>
        /// <remarks>
        /// This constructor facilitates the MVVM pattern by instantiating the <see cref="ChartsMeasurementPageViewModel"/> 
        /// with the provided database service. It also utilizes the <see cref="FrameworkElement.Loaded"/> 
        /// event to trigger an asynchronous chart refresh, ensuring that the visual data is 
        /// populated immediately after the page is rendered.
        /// </remarks>
        public ChartsMeasurementsPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();
            
            _shell = shell;
            
            databaseService = db;
            
            chartsPageViewModel = new ChartsMeasurementPageViewModel(databaseService);
            
            DataContext = chartsPageViewModel;
         
            Loaded += async (s, e) => await chartsPageViewModel.RefreshChartAsync();

        }

    }
}
