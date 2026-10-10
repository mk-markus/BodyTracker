using BF105;
using BodyTracker.Models.FullBodyMeasurement;
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
    /// Represents the code-behind logic for the <see cref="AutomaticScaleMeasurementView"/> user control page, 
    /// handling window context association and view model initialization.
    /// </summary>
    public partial class AutomaticScaleMeasurementView : Page
    {
        /// <summary>
        /// Holds a reference to the parent window shell hosting this page.
        /// </summary>
        private Window _shell;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticScaleMeasurementView"/> class, 
        /// assigns the parent window shell reference, loads component layout definitions, 
        /// instantiates the view model configured for the specified body scan mode, 
        /// sets the data context, and registers an unloaded event handler for resource cleanup.
        /// </summary>
        /// <param name="shell">The parent window shell instance.</param>
        /// <param name="bodyScanMode">The specific body scan mode configuration for the measurement session.</param>
        public AutomaticScaleMeasurementView(Window shell, BF105BeurerWaage.ScanMode bodyScanMode)
        {
            _shell = shell;

            InitializeComponent();

            var vm = new AutomaticScaleMeasurementViewModel(_shell, bodyScanMode);

            DataContext = vm;

            Unloaded += (s, e) => vm.Dispose();
        }
    }
}