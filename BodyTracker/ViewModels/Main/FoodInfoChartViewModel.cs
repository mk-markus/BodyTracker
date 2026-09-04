using BodyTracker.Models;
using BodyTracker.Models.Chart;
using BodyTracker.Services;
using BodyTracker.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels.Main
{
    public partial class FoodInfoChartViewModel : ObservableObject, IDisposable
    {
        private readonly DatabaseService databaseService;
        private List<SamsungFoodInfoModel> data;

        [ObservableProperty] private ISeries[] seriesCalories = Array.Empty<ISeries>();
        [ObservableProperty] private ISeries[] seriesProtein = Array.Empty<ISeries>();
        [ObservableProperty] private ISeries[] seriesCarbs = Array.Empty<ISeries>();
        [ObservableProperty] private ISeries[] seriesFat = Array.Empty<ISeries>();

        [ObservableProperty] private ISeries[] seriesVitaminA = Array.Empty<ISeries>();
        [ObservableProperty] private ISeries[] seriesVitaminC = Array.Empty<ISeries>();
        [ObservableProperty] private ISeries[] seriesVitaminD = Array.Empty<ISeries>();

        [ObservableProperty] private ISeries[] seriesIron = Array.Empty<ISeries>();
        [ObservableProperty] private ISeries[] seriesCalcium = Array.Empty<ISeries>();
        [ObservableProperty] private ISeries[] seriesNatrium = Array.Empty<ISeries>();
        [ObservableProperty] private ISeries[] seriesKalium = Array.Empty<ISeries>();

        [ObservableProperty] private ICartesianAxis[] xAxesCalories = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] yAxesCalories = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] xAxesProtein = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] yAxesProtein = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] xAxesCarbs = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] yAxesCarbs = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] xAxesFat = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] yAxesFat = Array.Empty<ICartesianAxis>();

        [ObservableProperty] private ICartesianAxis[] xAxesVitaminA = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] yAxesVitaminA = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] xAxesVitaminC = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] yAxesVitaminC = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] xAxesVitaminD = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] yAxesVitaminD = Array.Empty<ICartesianAxis>();

        [ObservableProperty] private ICartesianAxis[] xAxesIron = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] yAxesIron = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] xAxesCalcium = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] yAxesCalcium = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] xAxesNatrium = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] yAxesNatrium = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] xAxesKalium = Array.Empty<ICartesianAxis>();
        [ObservableProperty] private ICartesianAxis[] yAxesKalium = Array.Empty<ICartesianAxis>();

        [ObservableProperty] private bool showCalories = true;
        [ObservableProperty] private bool showCaloriesTrend = false;

        [ObservableProperty] private bool showProtein = true;
        [ObservableProperty] private bool showProteinTrend = false;

        [ObservableProperty] private bool showCarbs = true;
        [ObservableProperty] private bool showCarbsTrend = false;

        [ObservableProperty] private bool showFat = true;
        [ObservableProperty] private bool showFatTrend = false;

        [ObservableProperty] private bool showVitaminA = true;
        [ObservableProperty] private bool showVitaminATrend = false;

        [ObservableProperty] private bool showVitaminC = true;
        [ObservableProperty] private bool showVitaminCTrend = false;

        [ObservableProperty] private bool showVitaminD = true;
        [ObservableProperty] private bool showVitaminDTrend = false;

        [ObservableProperty] private bool showIron = true;
        [ObservableProperty] private bool showIronTrend = false;

        [ObservableProperty] private bool showCalcium = true;
        [ObservableProperty] private bool showCalciumTrend = false;

        [ObservableProperty] private bool showNatrium = true;
        [ObservableProperty] private bool showNatriumTrend = false;

        [ObservableProperty] private bool showKalium = true;
        [ObservableProperty] private bool showKaliumTrend = false;


        [ObservableProperty]
        private DateTime startDate;

        [ObservableProperty]
        private DateTime endDate;

        public DateTime minMeasurementsDate;
        public DateTime maxMeasurementsDate;

        [ObservableProperty] private double loessFraction = 0.5;

        // --- Commands ---
        public IAsyncRelayCommand CommandRefreshAsync { get; }
        public IAsyncRelayCommand CommandReloadChartsAsync { get; }
        public IRelayCommand CommandSetActualYear { get; }
        public IRelayCommand CommandSetActualMonth { get; }
        public IRelayCommand CommandSetActualWeek { get; }
        public IRelayCommand CommandShowAllData { get; }

        private static int strokeThickness = 2;
        private static int geometrySize = 0;
        private static bool isTrendLineLegendVisible = false;

        private bool firstLoad = true;
        private bool suppressReload;
        private readonly SemaphoreSlim reloadLock = new(1, 1);

        private string generalInfoMessage = "";
        public string GeneralInfoMessage
        {
            get => generalInfoMessage;
            set
            {
                generalInfoMessage = value;
                WeakReferenceMessenger.Default.Send(new DatabaseErrorMessage(generalInfoMessage));
            }
        }

        public FoodInfoChartViewModel(DatabaseService db)
        {
            databaseService = db;

            CommandReloadChartsAsync = new AsyncRelayCommand(ReloadChartAsync);
            CommandSetActualYear = new RelayCommand(SetActualYear);
            CommandSetActualMonth = new RelayCommand(SetActualMonth);
            CommandSetActualWeek = new RelayCommand(SetActualWeek);
            CommandShowAllData = new RelayCommand(ShowAllData);
            CommandRefreshAsync = new AsyncRelayCommand(RefreshChartDataAsync);

            _ = RefreshChartDataAsync();
        }

        private async Task RefreshChartDataAsync()
        {
            data = await databaseService.GetSamsungFoodInfoSqlAsync(AppState.SelectedPersonId);
            await ReloadChartAsync();
        }

        partial void OnStartDateChanged(DateTime value)
        {
            if (suppressReload) return;
            _ = ReloadChartAsync();
        }

        partial void OnEndDateChanged(DateTime value)
        {
            if (suppressReload) return;
            _ = ReloadChartAsync();
        }

        public async Task ReloadChartAsync()
        {
            if (!await reloadLock.WaitAsync(0)) return;

            if (AppState.SelectedPersonId <= 0)
            {
                GeneralInfoMessage = "No person selected. Please select a person to view their food info data.";
                return;
            }

            try
            {
                if (data == null || !data.Any()) return;

                minMeasurementsDate = data[0].CreateTime;
                maxMeasurementsDate = data[0].CreateTime;

                foreach (var item in data)
                {
                    if (item.CreateTime < minMeasurementsDate) minMeasurementsDate = item.CreateTime;
                    if (item.CreateTime > maxMeasurementsDate) maxMeasurementsDate = item.CreateTime;
                }

                if (firstLoad)
                {
                    suppressReload = true;
                    StartDate = DateTime.Today.AddMonths(-3);
                    EndDate = DateTime.Today;
                    suppressReload = false;
                    firstLoad = false;
                }

                var source = data
               .Where(d => d.CreateTime >= StartDate && d.CreateTime <= EndDate)
               .OrderBy(d => d.CreateTime)
               .ToList();

                var caloriesTask = Task.Run(() => ChartTemplateService.CreateFoodCaloriesChart(StartDate, EndDate, source, isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowCalories, ShowCaloriesTrend));
                var proteinTask = Task.Run(() => ChartTemplateService.CreateFoodProteinChart(StartDate, EndDate, source, isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowProtein, ShowProteinTrend));
                var carbsTask = Task.Run(() => ChartTemplateService.CreateFoodCarbsChart(StartDate, EndDate, source, isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowCarbs, ShowCarbsTrend));
                var fatTask = Task.Run(() => ChartTemplateService.CreateFoodFatChart(StartDate, EndDate, source, isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowFat, ShowFatTrend));

                var vitaminATask = Task.Run(() => ChartTemplateService.CreateFoodVitaminAChart(StartDate, EndDate, source, isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowVitaminA, ShowVitaminATrend));
                var vitaminCTask = Task.Run(() => ChartTemplateService.CreateFoodVitaminCChart(StartDate, EndDate, source, isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowVitaminC, ShowVitaminCTrend));
                var vitaminDTask = Task.Run(() => ChartTemplateService.CreateFoodVitaminDChart(StartDate, EndDate, source, isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowVitaminD, ShowVitaminDTrend));

                var ironTask = Task.Run(() => ChartTemplateService.CreateFoodIronChart(StartDate, EndDate, source, isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowIron, ShowIronTrend));
                var calciumTask = Task.Run(() => ChartTemplateService.CreateFoodCalciumChart(StartDate, EndDate, source, isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowCalcium, ShowCalciumTrend));
                var natriumTask = Task.Run(() => ChartTemplateService.CreateFoodNatriumChart(StartDate, EndDate, source, isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowNatrium, ShowNatriumTrend));
                var kaliumTask = Task.Run(() => ChartTemplateService.CreateFoodKaliumChart(StartDate, EndDate, source, isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowKalium, ShowKaliumTrend));

                await Task.WhenAll(
                    caloriesTask, proteinTask, carbsTask, fatTask,
                    vitaminATask, vitaminCTask, vitaminDTask,
                    ironTask, calciumTask, natriumTask, kaliumTask
                );

                var caloriesResult = caloriesTask.Result;
                var proteinResult = proteinTask.Result;
                var carbsResult = carbsTask.Result;
                var fatResult = fatTask.Result;

                var vitaminAResult = vitaminATask.Result;
                var vitaminCResult = vitaminCTask.Result;
                var vitaminDResult = vitaminDTask.Result;

                var ironResult = ironTask.Result;
                var calciumResult = calciumTask.Result;
                var natriumResult = natriumTask.Result;
                var kaliumResult = kaliumTask.Result;

                SeriesCalories = caloriesResult.Series; XAxesCalories = caloriesResult.XAxis; YAxesCalories = caloriesResult.YAxis;
                SeriesProtein = proteinResult.Series; XAxesProtein = proteinResult.XAxis; YAxesProtein = proteinResult.YAxis;
                SeriesCarbs = carbsResult.Series; XAxesCarbs = carbsResult.XAxis; YAxesCarbs = carbsResult.YAxis;
                SeriesFat = fatResult.Series; XAxesFat = fatResult.XAxis; YAxesFat = fatResult.YAxis;

                SeriesVitaminA = vitaminAResult.Series; XAxesVitaminA = vitaminAResult.XAxis; YAxesVitaminA = vitaminAResult.YAxis;
                SeriesVitaminC = vitaminCResult.Series; XAxesVitaminC = vitaminCResult.XAxis; YAxesVitaminC = vitaminCResult.YAxis;
                SeriesVitaminD = vitaminDResult.Series; XAxesVitaminD = vitaminDResult.XAxis; YAxesVitaminD = vitaminDResult.YAxis;

                SeriesIron = ironResult.Series; XAxesIron = ironResult.XAxis; YAxesIron = ironResult.YAxis;
                SeriesCalcium = calciumResult.Series; XAxesCalcium = calciumResult.XAxis; YAxesCalcium = calciumResult.YAxis;
                SeriesNatrium = natriumResult.Series; XAxesNatrium = natriumResult.XAxis; YAxesNatrium = natriumResult.YAxis;
                SeriesKalium = kaliumResult.Series; XAxesKalium = kaliumResult.XAxis; YAxesKalium = kaliumResult.YAxis;

            }
            catch (Exception ex)
            {
                GeneralInfoMessage = $"Error refreshing food info chart: {ex.Message}";
            }
            finally
            {
                reloadLock.Release();
            }
        }

        private void SetActualYear()
        {
            StartDate = new DateTime(DateTime.Today.Year, 1, 1);
            EndDate = DateTime.Today;
        }

        private void SetActualMonth()
        {
            StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            EndDate = DateTime.Today;
        }

        private void SetActualWeek()
        {
            var today = DateTime.Today;
            int diff = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;

            StartDate = today.AddDays(-diff);
            EndDate = StartDate.AddDays(6);
        }

        private void ShowAllData()
        {
            StartDate = minMeasurementsDate;
            EndDate = maxMeasurementsDate;
        }

        #region Disposal Pattern

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                Debug.WriteLine($"{this.GetType().Name} Disposing managed resources {GetHashCode()}");

                try
                {
                    databaseService?.Dispose();
                    WeakReferenceMessenger.Default.UnregisterAll(this);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{this.GetType().Name} Disposal Error: {ex.Message}");
                }
            }

            disposed = true;
        }

        #endregion
    }
}