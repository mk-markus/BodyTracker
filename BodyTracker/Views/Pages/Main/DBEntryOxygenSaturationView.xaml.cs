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
    /// Interaktionslogik für DBEntryOxygenSaturationView.xaml
    /// </summary>
    public partial class DBEntryOxygenSaturationView : Page
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the ViewModel.
        /// </summary>
        private readonly DatabaseService databaseService;

        private readonly DBEntryOxygenSaturationViewModel dBEntryOxygenSaturationViewModel;

        public DBEntryOxygenSaturationView(DatabaseService db)
        {
            InitializeComponent();


            databaseService = db;

            dBEntryOxygenSaturationViewModel = new DBEntryOxygenSaturationViewModel(databaseService);

            DataContext = dBEntryOxygenSaturationViewModel;

            Unloaded += (s, e) => dBEntryOxygenSaturationViewModel.Dispose();
        }
    }
}
