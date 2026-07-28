using BodyTracker.MVVM.Views;
using BodyTracker.MVVM.Views.Pages;
using BodyTracker.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace BodyTracker.MVVM.ViewModels
{
    public partial class MultiChartViewModel : ObservableObject
    {

        /// <summary>
        /// A private, read-only reference to the <see cref="DatabaseService"/>.
        /// This service acts as the primary data gateway for all persistence 
        /// operations initiated by the ViewModel.
        /// </summary>
        /// <remarks>
        /// Marked as <c>readonly</c> to ensure that the service reference remains 
        /// immutable throughout the lifetime of the ViewModel instance, preventing 
        /// accidental reassignment and ensuring architectural stability.
        /// </remarks>
        private readonly DatabaseService databaseService;

        /// <summary>
        /// 
        /// </summary>
        private readonly MainWindow mainWindow;


        /// <summary>
        /// 
        /// </summary>
        [ObservableProperty] private object? currentPage;

        /// <summary>
        /// 
        /// </summary>
        public IAsyncRelayCommand CommandShowMeasurements { get; }

        /// <summary>
        /// 
        /// </summary>
        public IAsyncRelayCommand CommandShowDailyStep { get; }

        /// <summary>
        /// 
        /// </summary>
        public IAsyncRelayCommand ShowWorkoutsCommand { get; }

        public MultiChartViewModel(MainWindow shell, DatabaseService db)
        {
            mainWindow = shell;
            databaseService = db;

            CommandShowMeasurements = new AsyncRelayCommand(ShowMeasurementsAsync);

            ShowWorkoutsCommand = new AsyncRelayCommand(ShowWorkoutsPage);
            CommandShowDailyStep = new AsyncRelayCommand(ShowDailyStepsAsync);




            _ = ShowMeasurementsAsync();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task ShowMeasurementsAsync()
        {
            // Erzeuge Page hier; falls teure Initialisierung nötig ist, mach sie asynchron
            CurrentPage = new ChartsMeasurementsPage(mainWindow, databaseService);
         }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task ShowDailyStepsAsync()
        {
            CurrentPage = new ChartsStepDailyTrendPage(mainWindow, databaseService);

        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task ShowWorkoutsPage()
        {
            CurrentPage = new PersonalWorkoutInsightsPage(mainWindow, databaseService);

        }


    }
}