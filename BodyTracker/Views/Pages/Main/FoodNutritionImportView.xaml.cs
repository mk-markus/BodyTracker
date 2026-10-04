using BodyTracker.Services;
using BodyTracker.ViewModels;
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

namespace BodyTracker.Views.Pages
{
    /// <summary>
    /// Interaktionslogik für FoodNutritionImportViewModel.xaml
    /// </summary>
    public partial class FoodNutritionImportView : Page
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
        private readonly FoodNutritionImportViewModel foodNutritionVM;

        /// <summary>
        /// Initializes a new instance of the <see cref="FoodNutritionImportView"/> class.
        /// </summary>
        /// <param name="db">The database service instance used for data persistence.</param>
        /// <param name="path">The file path used to initialize the import view model.</param>
        public FoodNutritionImportView(DatabaseService db)
        {
            InitializeComponent();
            databaseService = db;

            foodNutritionVM = new FoodNutritionImportViewModel(databaseService);

            DataContext = foodNutritionVM;

            Unloaded += (s, e) => foodNutritionVM.Dispose();
        }
    }
}
