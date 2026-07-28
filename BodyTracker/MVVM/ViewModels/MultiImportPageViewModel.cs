using BodyTracker.MVVM.Views;
using BodyTracker.MVVM.Views.Pages;
using BodyTracker.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace BodyTracker.MVVM.ViewModels
{
    public partial class MultiImportPageViewModel : ObservableObject
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
        public IAsyncRelayCommand CommandShowStepDailyTrendImportPage { get; }

        /// <summary>
        /// 
        /// </summary>
        public IAsyncRelayCommand CommandShowFoodIntakeImportPage { get; }

        /// <summary>
        /// 
        /// </summary>
        public IAsyncRelayCommand CommandShowHeavyAppData { get; }


        private StepDailyTrendImportPage stepDailyTrendImportPage;

        private FoodIntakeImportPage foodIntakeImportPage;

        private HeavyAppImportPage heavyAppImportPage;

        public MultiImportPageViewModel(MainWindow shell, DatabaseService db)
        {


            mainWindow = shell;
            databaseService = db;

            CommandShowStepDailyTrendImportPage = new AsyncRelayCommand(ShowStepDailyTrendImportPage);

            CommandShowFoodIntakeImportPage = new AsyncRelayCommand(ShowFoodIntakeImportPage);

            CommandShowHeavyAppData = new AsyncRelayCommand(ShowHeavyAppDatasAsync);

            _ = ShowFoodIntakeImportPage();



        }


        private async Task ShowStepDailyTrendImportPage()
        {
            if (stepDailyTrendImportPage == null) stepDailyTrendImportPage = new StepDailyTrendImportPage(mainWindow, databaseService);


            CurrentPage = stepDailyTrendImportPage;
        }


        private async Task ShowFoodIntakeImportPage()
        {
            if(foodIntakeImportPage == null)  foodIntakeImportPage = new FoodIntakeImportPage(mainWindow, databaseService);
            CurrentPage = foodIntakeImportPage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task ShowHeavyAppDatasAsync()
        {
            
            if(heavyAppImportPage == null) heavyAppImportPage = new HeavyAppImportPage(mainWindow, databaseService);
            CurrentPage = heavyAppImportPage;

        }
    }
}
