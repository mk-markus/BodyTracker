using BodyTracker.MVVM.ViewModels;
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

namespace BodyTracker.MVVM.Views.Pages
{
    /// <summary>
    /// Interaktionslogik für HeavyAppImportPage.xaml
    /// </summary>
    public partial class HeavyAppImportPage : Page
    {

        /// <summary>
        /// A private, read-only reference to the application's <see cref="MainWindow"/>.
        /// This reference, often referred to as the 'shell', is used to coordinate 
        /// top-level UI actions, such as navigation between different pages or 
        /// accessing global window states.
        /// </summary>
        private readonly MainWindow mainWindow;

        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// Serves as the primary Data Access Layer (DAL) for retrieving and persisting 
        /// body measurement records and user profiles.
        /// </summary>
        private readonly DatabaseService databaseService;


        /// <summary>
        /// The view model instance managing the data and logic for the daily step trend view.
        /// </summary>
        private readonly HeavyAppImportPageViewModel heavyAppImportPageViewModel;



        public HeavyAppImportPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();

            mainWindow = shell;

            databaseService = db;

            heavyAppImportPageViewModel = new HeavyAppImportPageViewModel(mainWindow, databaseService);

            DataContext = heavyAppImportPageViewModel;

        }
    }
}
