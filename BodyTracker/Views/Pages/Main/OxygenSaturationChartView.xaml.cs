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
    /// Interaktionslogik für OxygenSaturationChartView.xaml
    /// </summary>
    public partial class OxygenSaturationChartView : Page
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// Serves as the primary Data Access Layer (DAL) for retrieving oxygen saturation records.
        /// </summary>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// A private, read-only reference to the <see cref="OxygenSaturationChartViewModel"/> 
        /// that handles the business logic and state for the oxygen saturation charts.
        /// </summary>
        private readonly OxygenSaturationChartViewModel oxygenSaturationChartVM;

        /// <summary>
        /// Initializes a new instance of the <see cref="OxygenSaturationChartView"/> class.
        /// </summary>
        /// <param name="db">The database service used for data retrieval.</param>
        public OxygenSaturationChartView(DatabaseService db)
        {
            InitializeComponent();

            databaseService = db ?? throw new ArgumentNullException(nameof(db));
            oxygenSaturationChartVM = new OxygenSaturationChartViewModel(databaseService);

            DataContext = oxygenSaturationChartVM;

            Unloaded += (s, e) => oxygenSaturationChartVM.Dispose();
        }
    }
}
