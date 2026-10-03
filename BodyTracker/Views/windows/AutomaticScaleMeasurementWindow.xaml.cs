using BodyTracker.ViewModels.Main;
using BodyTracker.Views;
using BodyTracker.Views.Pages.Main;
using OpenTK.Platform.Windows;
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
using System.Windows.Shapes;
using BF105.Bluetooth.Model;
using System.Diagnostics;
using BodyTracker.Models.FullBodyMeasurement;

namespace BodyTracker.Views.windows
{
    /// <summary>
    /// Represents a specialized popup window within the application designed to host and display 
    /// the automated body measurement view and its associated user interactions.
    /// </summary>
    public partial class AutomaticScaleMeasurementWindow : Window
    {
        /// <summary>
        /// Holds an instance of the automated measurement view hosted within this window container.
        /// </summary>
        private AutomaticScaleMeasurementView _popupMeasurementView;

        /// <summary>
        /// Gets the final body measurement result obtained from the underlying view model of the hosted measurement view. 
        /// Returns null if the view or its data context is unavailable or uninitialized.
        /// </summary>
        public BodyMeasurementModel MeasurementResult
        {
            get
            {
                if (_popupMeasurementView != null &&
                    _popupMeasurementView.DataContext is AutomaticScaleMeasurementViewModel viewModel)
                {
                    return viewModel.Result;
                }
                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticScaleMeasurementWindow"/> class, 
        /// loads the component layout, instantiates the automated measurement view with the specified scan mode, 
        /// and embeds it into the main content container.
        /// </summary>
        /// <param name="bodyScanMode">The specific body scan mode configuration for the measurement session.</param>
        public AutomaticScaleMeasurementWindow(BodyScanMode.ScanMode bodyScanMode)
        {
            InitializeComponent();

            _popupMeasurementView = new AutomaticScaleMeasurementView(this, bodyScanMode);

            MainChartContent.Content = _popupMeasurementView;
        }

        /// <summary>
        /// Raises the <see cref="Window.Closed"/> event, logs the closure, checks if the hosted view model 
        /// implements <see cref="IDisposable"/>, and invokes its dispose method to release resources.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (_popupMeasurementView != null &&
                _popupMeasurementView.DataContext is IDisposable disposableViewModel)
            {
                Debug.WriteLine("On Closed ausgeführt");
                disposableViewModel.Dispose();
            }
        }
    }
}