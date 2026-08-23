using BodyTracker.ViewModels;
using BodyTracker.Services;
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
    /// Interaktionslogik für MultiImportPage.xaml
    /// </summary>
    public partial class MultiImportPage : Page
    {
        /// <summary>
        /// Reference to the main application window, acting as the primary host (Shell) 
        /// for navigation, status updates, and top-level UI orchestration.
        /// </summary>
        private readonly MainWindow mainWindow;

        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the ViewModel.
        /// </summary>
        private readonly DatabaseService databaseService;

        private ChartsMeasurementsPage chartsMeasurementsPage;

        private ChartsStepDailyTrendPage chartStepDailyTrendPage;


        private readonly MultiImportViewModel multiImportPageViewModel;


        public MultiImportPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();
            mainWindow = shell;
            databaseService = db;
            
            mainWindow.DataContext = this;

            multiImportPageViewModel = new MultiImportViewModel(mainWindow, databaseService);

            DataContext = multiImportPageViewModel;

            Unloaded += (s, e) => multiImportPageViewModel.Dispose();

        }

      
    }
}
