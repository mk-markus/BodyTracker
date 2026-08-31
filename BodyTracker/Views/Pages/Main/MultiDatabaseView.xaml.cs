using BodyTracker.Services;
using BodyTracker.ViewModels;
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
    /// Interaktionslogik für MultiDatabaseView.xaml
    /// </summary>
    public partial class MultiDatabaseView : Page
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

        /// <summary>
        /// 
        /// </summary>
        private readonly MultiDatabaseViewModel multiDatabaseViewModel;

        public MultiDatabaseView(DatabaseService db)
        {
            InitializeComponent();
            databaseService = db;

            //mainWindow.DataContext = this;

            multiDatabaseViewModel = new MultiDatabaseViewModel(databaseService);

            DataContext = multiDatabaseViewModel;

            Unloaded += (s, e) => multiDatabaseViewModel.Dispose();
        }
    }
}
