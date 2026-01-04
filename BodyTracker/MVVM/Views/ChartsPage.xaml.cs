using BodyTracker.MVVM.ViewModels;
using BodyTracker.Services;
using BodyTracker.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BodyTracker.MVVM.Views
{
    

    /// <summary>
    /// Interaktionslogik für ChartsPage.xaml
    /// </summary>
    public partial class ChartsPage : Page
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
        /// A private, read-only reference to the <see cref="ChartsPageViewModel"/>.
        /// Acts as the primary data context for the view, holding the business logic, 
        /// chart configurations, and observable data collections required for visualization.
        /// </summary>
        private readonly ChartsPageViewModel chartsPageViewModel;


        /// <summary>
        /// Initializes a new instance of the <see cref="ChartsPage"/> class.
        /// Configures dependency injection, establishes the data context for LiveCharts2, 
        /// and registers the initial data load routine.
        /// </summary>
        /// <param name="shell">The main application window (<see cref="MainWindow"/>) used for shell-level coordination.</param>
        /// <param name="db">The database service providing access to the body measurement records.</param>
        /// <remarks>
        /// This constructor facilitates the MVVM pattern by instantiating the <see cref="ChartsPageViewModel"/> 
        /// with the provided database service. It also utilizes the <see cref="FrameworkElement.Loaded"/> 
        /// event to trigger an asynchronous chart refresh, ensuring that the visual data is 
        /// populated immediately after the page is rendered.
        /// </remarks>
        public ChartsPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();
            
            _shell = shell;
            
            databaseService = db;
            
            chartsPageViewModel = new ChartsPageViewModel(databaseService);
            
            DataContext = chartsPageViewModel;
         
            Loaded += async (s, e) => await chartsPageViewModel.RefreshChartAsync();
        }




        private void SetActualYearClick(object sender, RoutedEventArgs e)
        {
            DateTime today = DateTime.Today;
            chartsPageViewModel.StartDate = new DateTime(today.Year, 1, 1);
            chartsPageViewModel.EndDate = new DateTime(today.Year, 12, 31);
        }

        private void SetActualMonthClick(object sender, RoutedEventArgs e)
        {
            DateTime today = DateTime.Today;

            // First Day of Month
            chartsPageViewModel.StartDate = new DateTime(today.Year, today.Month, 1);

            // Last day of the current month:
            // We take the first day of the next month and subtract one day.
            chartsPageViewModel.EndDate = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
        }

        private void SetActualWeekClick(object sender, RoutedEventArgs e)
        {
            DateTime today = DateTime.Today;

            // Calculating Monday of this week (assuming the week starts on Monday)
            // DayOfWeek.Sunday is 0, Monday is 1... Saturday is 6.
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = today.AddDays(-1 * diff);

            chartsPageViewModel.StartDate = startOfWeek;
            chartsPageViewModel.EndDate = startOfWeek.AddDays(6); // Sunday
        }



    }
}
