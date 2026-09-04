using BodyTracker.Services;
using BodyTracker.ViewModels.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BodyTracker.Views.Pages.Main
{
    /// <summary>
    /// Interaktionslogik für FoodInfoDashboardView.xaml
    /// </summary>
    public partial class FoodInfoChartView : Page
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
        private readonly FoodInfoChartViewModel foodInfoChartVM;

        /// <summary>
        /// Initializes a new instance of the <see cref="FoodInfoChartView"/> class.
        /// </summary>
        /// <param name="db">The database service instance used for data access operations.</param>
        public FoodInfoChartView(DatabaseService db)
        {
            InitializeComponent();
            databaseService = db;

            foodInfoChartVM = new FoodInfoChartViewModel(databaseService);

            DataContext = foodInfoChartVM;

            Unloaded += (s, e) => foodInfoChartVM.Dispose();
        }
    }
}
