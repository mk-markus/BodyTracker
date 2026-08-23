using BodyTracker.Services;
using BodyTracker.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BodyTracker.Views
{
    /// <summary>
    /// Interaktionslogik für DataBaseEntriesBodyMeasurementPage.xaml
    /// </summary>
    public partial class DataBaseEntriesBodyMeasurementPage : Page
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="BodyMeasurementEntriesViewModel"/>.
        /// This instance serves as the primary data context for the page, orchestrating 
        /// the business logic, data retrieval, and command execution for body measurements.
        /// </summary>
        /// <remarks>
        /// The ViewModel is instantiated during the page construction to ensure that 
        /// data binding is established before the UI is rendered.
        /// </remarks>
        private readonly BodyMeasurementEntriesViewModel measurementViewModel;

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
        private readonly MainWindow _shell;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataBaseEntriesBodyMeasurementPage"/> class.
        /// Sets up dependency injection for services, initializes the ViewModel, 
        /// and configures the event handler for automatic data synchronization.
        /// </summary>
        /// <param name="shell">The main application window instance used for top-level UI orchestration.</param>
        /// <param name="db">The database service instance used for persistent data operations.</param>
        /// <remarks>
        /// This constructor establishes the <see cref="DataContext"/> by creating a new <see cref="BodyMeasurementEntriesViewModel"/>
        /// and subscribes to the <see cref="UIElement.IsVisibleChanged"/> event to ensure data remains 
        /// current when the user navigates between tabs.
        /// </remarks>
        public DataBaseEntriesBodyMeasurementPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();
            databaseService = db;
            _shell = shell;
            measurementViewModel = new BodyMeasurementEntriesViewModel(databaseService);
            DataContext = measurementViewModel;

            this.IsVisibleChanged += MeasurementPage_IsVisibleChanged;
            this.Unloaded += (s, e) => measurementViewModel.Dispose();

        }

        /// <summary>
        /// Automatically refreshes the measurements whenever the page becomes visible.
        /// This ensures data consistency when switching between different tabs/pages.
        /// </summary>
        private async void MeasurementPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            ICollectionView view = CollectionViewSource.GetDefaultView(MeasurementsGrid.ItemsSource);

            // e.NewValue ist true, wenn die Seite sichtbar wird
            if ((bool)e.NewValue)
            {
                try
                {
                   await measurementViewModel.InitializeAsync();

                    // Sorting the List so that the new one is always on the top
                    if (view != null)
                    {
                        view.SortDescriptions.Clear();
                        view.SortDescriptions.Add(new SortDescription("MeasurementDate", ListSortDirection.Descending));
                        view.Refresh();
                    }

                    // Sort InitialMeasurementsGrid as well
                    ICollectionView initialView = CollectionViewSource.GetDefaultView(InitialMeasurementsGrid.ItemsSource);
                    if (initialView != null)
                    {
                        initialView.SortDescriptions.Clear();
                        initialView.SortDescriptions.Add(new SortDescription("MeasurementDate", ListSortDirection.Descending));
                        initialView.Refresh();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Refrehing Errror: {ex.Message}");
                }
            }
        }
    }
}
