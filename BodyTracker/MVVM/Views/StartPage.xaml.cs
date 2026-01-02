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

        /// <summary>
        /// 
        /// </summary>
        private NewEntryPage newEntryPage;

        /// <summary>
        /// 
        /// </summary>
        private DispatcherTimer dispatcherTimerMeasurementPage;

        private DispatcherTimer dispatcherTimerChartPage;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartPage"/> class.
        /// Sets up the primary dashboard by hosting the chart and measurement sub-pages.
        /// </summary>
        /// <param name="shell">The main window instance used for top-level navigation and UI control.</param>
        /// <param name="databaseService">The database service instance for data persistence operations.</param>
        public StartPage(MainWindow shell, DatabaseService databaseService)
        {
            InitializeComponent();

      
            dispatcherTimerMeasurementPage = createDesipatcherTimer(RefreshMeasurementPage, 3);
           


            mainWindow = shell;
            this.databaseService = databaseService;

            newEntryPage = new NewEntryPage(mainWindow, this.databaseService);
            newEntryPage.SwitchToMeasurements += () => MainTabControll.SelectedIndex = 0;


            ChartFrame.Content = new ChartsPage(mainWindow, this.databaseService);
            MeasurementFrame.Content = new MeasurementPage(mainWindow, this.databaseService);
            NewEntryFrame.Content = newEntryPage;
            
           
        }





        private void RefreshChartPage()
        {
            if (MainTabControll.SelectedIndex == 2) MainTabControll.SelectedIndex = 2;
            Debug.WriteLine("Refresh Chart Page");
        }

        private void RefreshMeasurementPage()
        {
            if (MainTabControll.SelectedIndex == 0)
            {
                MainTabControll.SelectedIndex = 0;
                Debug.WriteLine("Refresh MEasurement Side");
            }
            
        }


        public string debugGetCurrentCalledMethode(string methodeName)
        {
           return "TimeStamp: " + DateTime.Now.ToString() + " Called Methode: " + methodeName;
        }



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
