using BodyTracker.MVVM.Views;
using BodyTracker.MVVM.Views.Pages;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace BodyTracker.MVVM.ViewModels
{

    public partial class StartPageViewModel : ObservableObject
    {


        /// <summary>
        /// 
        /// </summary>
        private readonly MainWindow mainWindow;


        private readonly DatabaseService databaseService;

        /// <summary>
        /// Gets or sets the name of the person currently being viewed.
        /// Used for display purposes in headers or titles.
        /// </summary>
        [ObservableProperty] private string userName = string.Empty;


        /// <summary>
        /// Gets or sets the name of the person currently being viewed.
        /// Used for display purposes in headers or titles.
        /// </summary>
        [ObservableProperty] private string userNameInitial = string.Empty;


        /// <summary>
        /// 
        /// </summary>
        [ObservableProperty] private object? currentPage;

        /// <summary>
        /// 
        /// </summary>
        public IAsyncRelayCommand CommandShowDashboardPageAsync  { get; }

        /// <summary>
        /// 
        /// </summary>
        public IAsyncRelayCommand CommandShowChartsPageAsync { get; }


        /// <summary>
        /// 
        /// </summary>
        public IAsyncRelayCommand CommandShowMeasurementDatabasePageAsync  { get; }


        /// <summary>
        /// 
        /// </summary>
        public IAsyncRelayCommand CommandNewDatabaseEntryPageAsync  { get; }

        /// <summary>
        /// 
        /// </summary>
        public IAsyncRelayCommand CommandMultiImportPageAsync { get; }


        [ObservableProperty] private bool isDashboardSelected;
        [ObservableProperty] private bool isChartsSelected;
        [ObservableProperty] private bool isMeasurementDatabaseSelected;
        [ObservableProperty] private bool isNewEntrySelected;
        [ObservableProperty] private bool isImportSelected;

        // Suppress-Flag, um Rekursion zu vermeiden, wenn ViewModel die Auswahl setzt
        private bool suppressSelectionAction = false;

        public StartPageViewModel(MainWindow shell, DatabaseService db)
        {
            UserName = AppState.SelectedPersonName;
            UserNameInitial = GetinitialsName(UserName);


            mainWindow = shell;
            databaseService = db;



            CommandShowChartsPageAsync = new AsyncRelayCommand(ShowChartsPageAsync);

            CommandShowDashboardPageAsync  = new AsyncRelayCommand(ShowDashboardPageAsync);

            CommandShowMeasurementDatabasePageAsync  = new AsyncRelayCommand(ShowMeasurementDatabasePageAsync);

            CommandNewDatabaseEntryPageAsync  = new AsyncRelayCommand(ShowNewDatabaseEntryPageAsync);
            CommandMultiImportPageAsync = new AsyncRelayCommand(ShowMeasurementDatabasePageAsync);


            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                if (m.Target == NavigationMessage.ShowDashboardPage) _= ShowDashboardPageAsync();
                if (m.Target == NavigationMessage.ShowNewEntryPage) _= ShowNewDatabaseEntryPageAsync();
                if (m.Target == NavigationMessage.ShowMeasurementDatabasePage) _= ShowMeasurementDatabasePageAsync();
                if (m.Target == NavigationMessage.ShowMultiChartPage) _= ShowChartsPageAsync();

            });

            _ = ShowDashboardPageAsync();

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="PersonName"></param>
        /// <returns></returns>
        public string GetinitialsName(string PersonName)
        {

            return string.Concat(PersonName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(wort => char.ToUpper(wort[0])));

        }


        partial void OnIsDashboardSelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowDashboardPageAsync();
        }

        partial void OnIsChartsSelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowChartsPageAsync();
        }

        partial void OnIsMeasurementDatabaseSelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowMeasurementDatabasePageAsync();
        }

        partial void OnIsNewEntrySelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowNewDatabaseEntryPageAsync();
        }


        partial void OnIsImportSelectedChanged(bool value)
        {
            if (!value || suppressSelectionAction) return;
            _ = ShowImportPageAsync();
        }




        private async Task ShowImportPageAsync()
        {
           
            var page = 
            //page.DataContext = this;
            CurrentPage = new MultiImportPage(mainWindow, databaseService); ;

            suppressSelectionAction = true;
            IsDashboardSelected = false;
            IsChartsSelected = false;
            IsMeasurementDatabaseSelected = false;
            IsNewEntrySelected = false;
            IsImportSelected = true;
            suppressSelectionAction = false;
        }



        private async Task ShowDashboardPageAsync()
        {
            // setze Page
            var page = new DashboardPage(mainWindow, databaseService);
            //page.DataContext = this;
            CurrentPage = page;

            suppressSelectionAction = true;
            IsDashboardSelected = true;
            IsChartsSelected = false;
            IsMeasurementDatabaseSelected = false;
            IsNewEntrySelected = false;
            suppressSelectionAction = false;
        }

        private async Task ShowChartsPageAsync()
        {
            var page = new MultiChartsPage(mainWindow, databaseService);
            //page.DataContext = this;
            CurrentPage = page;

            suppressSelectionAction = true;
            IsDashboardSelected = false;
            IsChartsSelected = true;
            IsMeasurementDatabaseSelected = false;
            IsNewEntrySelected = false;
            suppressSelectionAction = false;
        }

        private async Task ShowMeasurementDatabasePageAsync()
        {
            var page = new MeasurementPage(mainWindow, databaseService); 
            //page.DataContext = this;
            CurrentPage = page;

            suppressSelectionAction = true;
            IsDashboardSelected = false;
            IsChartsSelected = false;
            IsMeasurementDatabaseSelected = true;
            IsNewEntrySelected = false;
            suppressSelectionAction = false;
        }

        private async Task ShowNewDatabaseEntryPageAsync()
        {
            var page = new NewDataEntryPage(mainWindow, databaseService);
            //var vm = new BodyTracker.ViewModels.NewDataEntryViewModel(databaseService);
            //await vm.InitializeAsync();
            //page.DataContext = vm;
            CurrentPage = page;

            suppressSelectionAction = true;
            IsDashboardSelected = false;
            IsChartsSelected = false;
            IsMeasurementDatabaseSelected = false;
            IsNewEntrySelected = true;
            suppressSelectionAction = false;
        }


    }
}
