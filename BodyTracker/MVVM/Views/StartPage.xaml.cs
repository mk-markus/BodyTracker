using BodyTracker.Services;
using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;


namespace BodyTracker.MVVM.Views
{
    /// <summary>
    /// Interaktionslogik für EntryPage.xaml
    /// </summary>
    public partial class StartPage : Page
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

        // <summary>
        /// A private field holding a cached instance of the <see cref="NewEntryPage"/>.
        /// This allows the application to persist the visual state and unsaved input 
        /// of the entry form during the current session's navigation cycle.
        /// </summary>
        private NewEntryPage newEntryPage;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartPage"/> class.
        /// Sets up the primary dashboard by hosting the chart and measurement sub-pages.
        /// </summary>
        /// <param name="shell">The main window instance used for top-level navigation and UI control.</param>
        /// <param name="databaseService">The database service instance for data persistence operations.</param>
        public StartPage(MainWindow shell, DatabaseService databaseService)
        {
            InitializeComponent();

            mainWindow = shell;
            
            this.databaseService = databaseService;

            newEntryPage = new NewEntryPage(mainWindow, this.databaseService);
            
            newEntryPage.SwitchToMeasurements += () => MainTabControll.SelectedIndex = 0;

            ChartFrame.Content = new ChartsPage(mainWindow, this.databaseService);

            InfoFrame.Content = new InfoPage();
            
            MeasurementFrame.Content = new MeasurementPage(mainWindow, this.databaseService);
            
            NewEntryFrame.Content = newEntryPage;
        }

        // <summary>
        /// Forces a visual refresh of the chart view by re-evaluating the current selection state 
        /// of the main navigation control. 
        /// </summary>
        /// <remarks>
        /// This method serves as a notification mechanism to ensure the <see cref="ChartsPage"/> 
        /// updates its graphical representation. The explicit re-assignment of the 
        /// <see cref="TabControl.SelectedIndex"/> can be used to trigger layout re-calculations 
        /// or focus events within the WPF framework.
        /// </remarks>
        private void RefreshChartPage()
        {
            if (MainTabControll.SelectedIndex == 2) MainTabControll.SelectedIndex = 2;
            Debug.WriteLine("Refresh Chart Page");
        }

        /// <summary>
        /// Forces a visual refresh of the measurement overview page by re-evaluating 
        /// the selection state of the primary <see cref="TabControl"/>.
        /// </summary>
        /// <remarks>
        /// This method is typically invoked after data modifications (e.g., after saving a new entry) 
        /// to ensure the UI reflects the most recent database state. Re-assigning the 
        /// <see cref="TabControl.SelectedIndex"/> ensures that any bound resources or 
        /// lifecycle events of the measurement page are re-triggered.
        /// </remarks>
        private void RefreshMeasurementPage()
        {
            if (MainTabControll.SelectedIndex == 0)
            {
                MainTabControll.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Generates a formatted diagnostic string containing a high-precision timestamp 
        /// and the name of the currently executed method.
        /// </summary>
        /// <param name="methodeName">The name of the method that is currently being tracked.</param>
        /// <returns>A string formatted as: "TimeStamp: [CurrentTime] Called Methode: [methodeName]".</returns>
        /// <remarks>
        /// This utility is primarily intended for logging and debugging purposes. It helps 
        /// developers trace the execution flow in the output console, especially when 
        /// diagnosing race conditions or identifying the order of reactive UI updates.
        /// </remarks>
        public string debugGetCurrentCalledMethode(string methodeName)
        {
           return "TimeStamp: " + DateTime.Now.ToString() + " Called Methode: " + methodeName;
        }

        /// <summary>
        /// Factory method to create, configure, and automatically start a <see cref="DispatcherTimer"/>.
        /// </summary>
        /// <param name="callingMethode">The <see cref="Action"/> (delegate) to be executed on every timer tick.</param>
        /// <param name="timeSpan">The execution interval in seconds.</param>
        /// <returns>A configured and running instance of a <see cref="DispatcherTimer"/>.</returns>
        /// <remarks>
        /// The <see cref="DispatcherTimer"/> is integrated into the <see cref="Dispatcher"/> queue, 
        /// ensuring that the <paramref name="callingMethode"/> is executed on the UI thread. 
        /// This is crucial for updating UI elements like charts or status labels without 
        /// encountering cross-thread exceptions.
        /// </remarks>
        private static DispatcherTimer createDesipatcherTimer (Action callingMethode, int timeSpan)
        {
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(timeSpan);
            timer.Tick +=  (s, e) => callingMethode();
            timer.Start();
            return timer;
        }
    }
}
