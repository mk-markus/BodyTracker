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
using BodyTracker.MVVM.ViewModels;

namespace BodyTracker.MVVM.Views.Pages
{
    /// <summary>
    /// Interaktionslogik für DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {

        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service provides the low-level infrastructure for all SQL Server interactions, 
        /// including CRUD operations for body metrics and dimensions.
        /// </summary>
        /// <remarks>
        /// By maintaining this reference at the page level, the component can facilitate 
        /// dependency injection and ensure consistent data access across all sub-routines.
        /// </remarks>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// A private, read-only reference to the <see cref="MainWindow"/>, acting as the application's "Shell".
        /// This reference provides the page with access to top-level UI orchestration, 
        /// navigation controls, and global application state management.
        /// </summary>
        /// <remarks>
        /// Following the Shell pattern, this field allows the current page to interact with 
        /// the main window's container, for example, to trigger navigation or update global status bars.
        /// </remarks>
        private readonly MainWindow mainWindow;

        private readonly DashboardViewModel dashboardViewModel;

        public DashboardPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();

            databaseService = db;
            mainWindow = shell;
            dashboardViewModel = new DashboardViewModel(databaseService);
            DataContext = dashboardViewModel;

            Loaded += async (s, e) => await dashboardViewModel.InitializeAsync();
        }
    }
}
