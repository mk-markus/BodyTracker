using BodyTracker.MVVM.Models;
using BodyTracker.Services;
using BodyTracker.State;
using BodyTracker.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace BodyTracker.MVVM.Views
{
    /// <summary>
    /// Interaktionslogik für MeasurementPage.xaml
    /// </summary>
    public partial class MeasurementPage : Page
    {
        /// <summary>
        /// A private, read-only reference to the <see cref="MeasurementViewModel"/>.
        /// This instance serves as the primary data context for the page, orchestrating 
        /// the business logic, data retrieval, and command execution for body measurements.
        /// </summary>
        /// <remarks>
        /// The ViewModel is instantiated during the page construction to ensure that 
        /// data binding is established before the UI is rendered.
        /// </remarks>
        private readonly MeasurementViewModel measurementViewModel;

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
        /// Initializes a new instance of the <see cref="MeasurementPage"/> class.
        /// Sets up dependency injection for services, initializes the ViewModel, 
        /// and configures the event handler for automatic data synchronization.
        /// </summary>
        /// <param name="shell">The main application window instance used for top-level UI orchestration.</param>
        /// <param name="db">The database service instance used for persistent data operations.</param>
        /// <remarks>
        /// This constructor establishes the <see cref="DataContext"/> by creating a new <see cref="MeasurementViewModel"/>
        /// and subscribes to the <see cref="UIElement.IsVisibleChanged"/> event to ensure data remains 
        /// current when the user navigates between tabs.
        /// </remarks>
        public MeasurementPage(MainWindow shell, DatabaseService db)
        {
            InitializeComponent();
            databaseService = db;
            _shell = shell;
            measurementViewModel = new MeasurementViewModel(databaseService);
            DataContext = measurementViewModel;

            // Wir abonnieren das Event, das feuert, wenn der Nutzer zu dieser Seite wechselt
            this.IsVisibleChanged += MeasurementPage_IsVisibleChanged;

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

                    //if (MeasurementsGrid != null)
                    //{
                    //    MeasurementsGrid.Items.Refresh();
                    //}
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Refrehing Errror: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the Delete button. 
        /// Validates the current selection, prompts the user for confirmation, 
        /// and orchestrates the asynchronous deletion of the selected measurement record.
        /// </summary>
        /// <param name="sender">The source of the event, typically the Delete <see cref="Button"/>.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// This method acts as a controller logic within the View. It ensures that no deletion 
        /// is attempted without a valid selection and provides a safety dialog to prevent accidental data loss. 
        /// The actual deletion logic is encapsulated within the <see cref="MeasurementViewModel.DeleteCommand"/>.
        /// </remarks>
        private async void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (measurementViewModel.SelectedMeasurement == null)
            {
                MessageBox.Show("Please select one line first.", 
                                "Information", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Information);
                return;
            }
            var m = measurementViewModel.SelectedMeasurement;
            var res = MessageBox.Show($"Delete entry from {m.MeasurementDate:d}?",
                                      "Confrim deletion", 
                                      MessageBoxButton.YesNo, 
                                      MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                await measurementViewModel.DeleteCommand.ExecuteAsync(null);
            }
        }


        private void SetActualYearClick(object sender, RoutedEventArgs e)
        {
            DateTime today = DateTime.Today;
            measurementViewModel.MeanStartDate = new DateTime(today.Year, 1, 1);
            measurementViewModel.MeanEndDate = new DateTime(today.Year, 12, 31);
        }

        private void SetActualMonthClick(object sender, RoutedEventArgs e)
        {
            DateTime today = DateTime.Today;
        
            // First Day of Month
            measurementViewModel.MeanStartDate = new DateTime(today.Year, today.Month, 1);

            // Last day of the current month:
            // We take the first day of the next month and subtract one day.
            measurementViewModel.MeanEndDate = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
        }

        private void SetActualWeekClick(object sender, RoutedEventArgs e)
        {
            DateTime today = DateTime.Today;

            // Calculating Monday of this week (assuming the week starts on Monday)
            // DayOfWeek.Sunday is 0, Monday is 1... Saturday is 6.
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = today.AddDays(-1 * diff);

            measurementViewModel.MeanStartDate = startOfWeek;
            measurementViewModel.MeanEndDate = startOfWeek.AddDays(6); // Sunday
        }




        /// <summary>
        /// Handles the <see cref="DataGrid.RowEditEnding"/> event to persist modified measurement data to the database.
        /// This method ensures that only committed changes are processed and utilizes the Dispatcher 
        /// to decouple the database update from the UI validation cycle.
        /// </summary>
        /// <param name="sender">The source of the event, specifically the <see cref="DataGrid"/>.</param>
        /// <param name="e">Event data containing the edit action and the row being edited.</param>
        private void MeasurementsGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;

            if (e.Row.Item is not FullBodyMeasurementDatas editedRow) return;

            Dispatcher.BeginInvoke(new Action(async () =>
            {        
                    int personId = AppState.SelectedPersonId;
                    await measurementViewModel.UpsertMeasurementAsync(personId, editedRow);
            }), DispatcherPriority.Background);
        }

     
        private void SortingView()
        {
            var view = CollectionViewSource.GetDefaultView(measurementViewModel.Measurement);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription(nameof(FullBodyMeasurementDatas.MeasurementDate), ListSortDirection.Descending));
            
        }
    }
}
