using BodyTracker.Models;
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
using System.Threading;
using System.Threading.Tasks;

namespace BodyTracker.ViewModels.Main
{
    /// <summary>
    /// Represents the view model responsible for managing and visualizing nutritional food information 
    /// and macro/micronutrient chart metrics using LiveCharts within an MVVM architecture.
    /// </summary>
    public partial class FoodInfoChartViewModel : ObservableObject, IDisposable
    {
        private readonly DatabaseService databaseService;
        private List<SamsungFoodInfoModel> data;

        /// <summary>
        /// Gets or sets the chart series collection for tracking calories.
        /// </summary>
        [ObservableProperty] private ISeries[] seriesCalories = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the chart series collection for tracking protein intake.
        /// </summary>
        [ObservableProperty] private ISeries[] seriesProtein = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the chart series collection for tracking carbohydrate intake.
        /// </summary>
        [ObservableProperty] private ISeries[] seriesCarbs = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the chart series collection for tracking fat intake.
        /// </summary>
        [ObservableProperty] private ISeries[] seriesFat = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the chart series collection for tracking Vitamin A levels.
        /// </summary>
        [ObservableProperty] private ISeries[] seriesVitaminA = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the chart series collection for tracking Vitamin C levels.
        /// </summary>
        [ObservableProperty] private ISeries[] seriesVitaminC = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the chart series collection for tracking Vitamin D levels.
        /// </summary>
        [ObservableProperty] private ISeries[] seriesVitaminD = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the chart series collection for tracking iron levels.
        /// </summary>
        [ObservableProperty] private ISeries[] seriesIron = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the chart series collection for tracking calcium levels.
        /// </summary>
        [ObservableProperty] private ISeries[] seriesCalcium = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the chart series collection for tracking sodium levels.
        /// </summary>
        [ObservableProperty] private ISeries[] seriesNatrium = Array.Empty<ISeries>();

        /// <summary>
        /// Gets or sets the chart series collection for tracking potassium levels.
        /// </summary>
        [ObservableProperty] private ISeries[] seriesKalium = Array.Empty<ISeries>();

        /// <summary>Gets or sets the X-axis configuration for the calories chart.</summary>
        [ObservableProperty] private ICartesianAxis[] xAxesCalories = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the Y-axis configuration for the calories chart.</summary>
        [ObservableProperty] private ICartesianAxis[] yAxesCalories = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the X-axis configuration for the protein chart.</summary>
        [ObservableProperty] private ICartesianAxis[] xAxesProtein = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the Y-axis configuration for the protein chart.</summary>
        [ObservableProperty] private ICartesianAxis[] yAxesProtein = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the X-axis configuration for the carbohydrates chart.</summary>
        [ObservableProperty] private ICartesianAxis[] xAxesCarbs = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the Y-axis configuration for the carbohydrates chart.</summary>
        [ObservableProperty] private ICartesianAxis[] yAxesCarbs = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the X-axis configuration for the fat chart.</summary>
        [ObservableProperty] private ICartesianAxis[] xAxesFat = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the Y-axis configuration for the fat chart.</summary>
        [ObservableProperty] private ICartesianAxis[] yAxesFat = Array.Empty<ICartesianAxis>();

        /// <summary>Gets or sets the X-axis configuration for the Vitamin A chart.</summary>
        [ObservableProperty] private ICartesianAxis[] xAxesVitaminA = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the Y-axis configuration for the Vitamin A chart.</summary>
        [ObservableProperty] private ICartesianAxis[] yAxesVitaminA = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the X-axis configuration for the Vitamin C chart.</summary>
        [ObservableProperty] private ICartesianAxis[] xAxesVitaminC = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the Y-axis configuration for the Vitamin C chart.</summary>
        [ObservableProperty] private ICartesianAxis[] yAxesVitaminC = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the X-axis configuration for the Vitamin D chart.</summary>
        [ObservableProperty] private ICartesianAxis[] xAxesVitaminD = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the Y-axis configuration for the Vitamin D chart.</summary>
        [ObservableProperty] private ICartesianAxis[] yAxesVitaminD = Array.Empty<ICartesianAxis>();

        /// <summary>Gets or sets the X-axis configuration for the iron chart.</summary>
        [ObservableProperty] private ICartesianAxis[] xAxesIron = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the Y-axis configuration for the iron chart.</summary>
        [ObservableProperty] private ICartesianAxis[] yAxesIron = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the X-axis configuration for the calcium chart.</summary>
        [ObservableProperty] private ICartesianAxis[] xAxesCalcium = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the Y-axis configuration for the calcium chart.</summary>
        [ObservableProperty] private ICartesianAxis[] yAxesCalcium = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the X-axis configuration for the sodium chart.</summary>
        [ObservableProperty] private ICartesianAxis[] xAxesNatrium = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the Y-axis configuration for the sodium chart.</summary>
        [ObservableProperty] private ICartesianAxis[] yAxesNatrium = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the X-axis configuration for the potassium chart.</summary>
        [ObservableProperty] private ICartesianAxis[] xAxesKalium = Array.Empty<ICartesianAxis>();
        /// <summary>Gets or sets the Y-axis configuration for the potassium chart.</summary>
        [ObservableProperty] private ICartesianAxis[] yAxesKalium = Array.Empty<ICartesianAxis>();

        /// <summary>Gets or sets a value indicating whether the calories series is displayed.</summary>
        [ObservableProperty] private bool showCalories = true;
        /// <summary>Gets or sets a value indicating whether the calories trend line is displayed.</summary>
        [ObservableProperty] private bool showCaloriesTrend = false;

        /// <summary>Gets or sets a value indicating whether the protein series is displayed.</summary>
        [ObservableProperty] private bool showProtein = true;
        /// <summary>Gets or sets a value indicating whether the protein trend line is displayed.</summary>
        [ObservableProperty] private bool showProteinTrend = false;

        /// <summary>Gets or sets a value indicating whether the carbohydrates series is displayed.</summary>
        [ObservableProperty] private bool showCarbs = true;
        /// <summary>Gets or sets a value indicating whether the carbohydrates trend line is displayed.</summary>
        [ObservableProperty] private bool showCarbsTrend = false;

        /// <summary>Gets or sets a value indicating whether the fat series is displayed.</summary>
        [ObservableProperty] private bool showFat = true;
        /// <summary>Gets or sets a value indicating whether the fat trend line is displayed.</summary>
        [ObservableProperty] private bool showFatTrend = false;

        /// <summary>Gets or sets a value indicating whether the Vitamin A series is displayed.</summary>
        [ObservableProperty] private bool showVitaminA = true;
        /// <summary>Gets or sets a value indicating whether the Vitamin A trend line is displayed.</summary>
        [ObservableProperty] private bool showVitaminATrend = false;

        /// <summary>Gets or sets a value indicating whether the Vitamin C series is displayed.</summary>
        [ObservableProperty] private bool showVitaminC = true;
        /// <summary>Gets or sets a value indicating whether the Vitamin C trend line is displayed.</summary>
        [ObservableProperty] private bool showVitaminCTrend = false;

        /// <summary>Gets or sets a value indicating whether the Vitamin D series is displayed.</summary>
        [ObservableProperty] private bool showVitaminD = true;
        /// <summary>Gets or sets a value indicating whether the Vitamin D trend line is displayed.</summary>
        [ObservableProperty] private bool showVitaminDTrend = false;

        /// <summary>Gets or sets a value indicating whether the iron series is displayed.</summary>
        [ObservableProperty] private bool showIron = true;
        /// <summary>Gets or sets a value indicating whether the iron trend line is displayed.</summary>
        [ObservableProperty] private bool showIronTrend = false;

        /// <summary>Gets or sets a value indicating whether the calcium series is displayed.</summary>
        [ObservableProperty] private bool showCalcium = true;
        /// <summary>Gets or sets a value indicating whether the calcium trend line is displayed.</summary>
        [ObservableProperty] private bool showCalciumTrend = false;

        /// <summary>Gets or sets a value indicating whether the sodium series is displayed.</summary>
        [ObservableProperty] private bool showNatrium = true;
        /// <summary>Gets or sets a value indicating whether the sodium trend line is displayed.</summary>
        [ObservableProperty] private bool showNatriumTrend = false;

        /// <summary>Gets or sets a value indicating whether the potassium series is displayed.</summary>
        [ObservableProperty] private bool showKalium = true;
        /// <summary>Gets or sets a value indicating whether the potassium trend line is displayed.</summary>
        [ObservableProperty] private bool showKaliumTrend = false;

        /// <summary>
        /// Gets or sets the start date of the filtering window for chart data.
        /// </summary>
        [ObservableProperty]
        private DateTime startDate;

        /// <summary>
        /// Gets or sets the end date of the filtering window for chart data.
        /// </summary>
        [ObservableProperty]
        private DateTime endDate;

        /// <summary>The earliest recorded measurement date available in the dataset.</summary>
        public DateTime minMeasurementsDate;
        /// <summary>The latest recorded measurement date available in the dataset.</summary>
        public DateTime maxMeasurementsDate;

        /// <summary>
        /// Gets or sets the fraction parameter used for Local Regression (LOESS) smoothing curves.
        /// </summary>
        [ObservableProperty] private double loessFraction = 0.5;


        /// <summary>Gets the command to refresh chart data from the database.</summary>
        public IAsyncRelayCommand CommandRefreshAsync { get; }
        /// <summary>Gets the command to reload and recompute all chart views.</summary>
        public IAsyncRelayCommand CommandReloadChartsAsync { get; }
        /// <summary>Gets the command to set the date filter to the current year.</summary>
        public IRelayCommand CommandSetActualYear { get; }
        /// <summary>Gets the command to set the date filter to the current month.</summary>
        public IRelayCommand CommandSetActualMonth { get; }
        /// <summary>Gets the command to set the date filter to the current calendar week.</summary>
        public IRelayCommand CommandSetActualWeek { get; }
        /// <summary>Gets the command to expand the date filter to show all available data records.</summary>
        public IRelayCommand CommandShowAllData { get; }

        private static int strokeThickness = 2;
        private static int geometrySize = 0;
        private static bool isTrendLineLegendVisible = false;

        private bool firstLoad = true;
        private bool suppressReload;
        private readonly SemaphoreSlim reloadLock = new(1, 1);

        private string generalInfoMessage = "";

        /// <summary>
        /// Gets or sets the general informational or error message, dispatching notifications via <see cref="WeakReferenceMessenger"/>.
        /// </summary>
        public string GeneralInfoMessage
        {
            get => generalInfoMessage;
            set
            {
                generalInfoMessage = value;
                WeakReferenceMessenger.Default.Send(new DatabaseErrorMessage(generalInfoMessage));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FoodInfoChartViewModel"/> class with the specified database service.
        /// </summary>
        /// <param name="db">The database service used to retrieve nutritional data.</param>
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

        /// <summary>
        /// Asynchronously fetches raw nutritional food data from the database and triggers chart reloads.
        /// </summary>
        /// <returns>A task representing the asynchronous refresh operation.</returns>
        private async Task RefreshChartDataAsync()
        {
            try
            {
                data = await databaseService.GetSamsungFoodInfoSqlAsync(AppState.SelectedPersonId);
                await ReloadChartAsync();
            }
            catch (Exception ex)
            {
                GeneralInfoMessage = ex.Message;
            }

        }

        /// <summary>
        /// Called when the start date property changes to trigger a chart reload unless suppressed.
        /// </summary>
        /// <param name="value">The new start date value.</param>
        partial void OnStartDateChanged(DateTime value)
        {
            if (suppressReload) return;
            _ = ReloadChartAsync();
        }

        /// <summary>
        /// Called when the end date property changes to trigger a chart reload unless suppressed.
        /// </summary>
        /// <param name="value">The new end date value.</param>
        partial void OnEndDateChanged(DateTime value)
        {
            if (suppressReload) return;
            _ = ReloadChartAsync();
        }

        /// <summary>
        /// Asynchronously recomputes and updates all nutritional series and axes based on the active date filter and settings.
        /// </summary>
        /// <returns>A task representing the asynchronous chart reload operation.</returns>
        public async Task ReloadChartAsync()
        {
            if (!await reloadLock.WaitAsync(0)) return;

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

                var caloriesTask = Task.Run(() => ChartTemplateService.CreateFoodCaloriesChart(StartDate, EndDate, source,
                    isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowCalories, ShowCaloriesTrend));

                var proteinTask = Task.Run(() => ChartTemplateService.CreateFoodProteinChart(StartDate, EndDate, source,
                    isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowProtein, ShowProteinTrend));

                var carbsTask = Task.Run(() => ChartTemplateService.CreateFoodCarbsChart(StartDate, EndDate, source,
                    isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowCarbs, ShowCarbsTrend));

                var fatTask = Task.Run(() => ChartTemplateService.CreateFoodFatChart(StartDate, EndDate, source,
                    isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowFat, ShowFatTrend));

                var vitaminATask = Task.Run(() => ChartTemplateService.CreateFoodVitaminAChart(StartDate, EndDate, source,
                    isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowVitaminA, ShowVitaminATrend));

                var vitaminCTask = Task.Run(() => ChartTemplateService.CreateFoodVitaminCChart(StartDate, EndDate, source,
                    isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowVitaminC, ShowVitaminCTrend));

                var vitaminDTask = Task.Run(() => ChartTemplateService.CreateFoodVitaminDChart(StartDate, EndDate, source,
                    isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowVitaminD, ShowVitaminDTrend));

                var ironTask = Task.Run(() => ChartTemplateService.CreateFoodIronChart(StartDate, EndDate, source,
                    isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowIron, ShowIronTrend));

                var calciumTask = Task.Run(() => ChartTemplateService.CreateFoodCalciumChart(StartDate, EndDate, source,
                    isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowCalcium, ShowCalciumTrend));

                var natriumTask = Task.Run(() => ChartTemplateService.CreateFoodNatriumChart(StartDate, EndDate, source,
                    isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowNatrium, ShowNatriumTrend));

                var kaliumTask = Task.Run(() => ChartTemplateService.CreateFoodKaliumChart(StartDate, EndDate, source,
                    isTrendLineLegendVisible, strokeThickness, geometrySize, LoessFraction, ShowKalium, ShowKaliumTrend));

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

        /// <summary>
        /// Sets the date filter range to cover the current calendar year.
        /// </summary>
        private void SetActualYear()
        {
            StartDate = new DateTime(DateTime.Today.Year, 1, 1);
            EndDate = DateTime.Today;
        }

        /// <summary>
        /// Sets the date filter range to cover the current calendar month.
        /// </summary>
        private void SetActualMonth()
        {
            StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            EndDate = DateTime.Today;
        }

        /// <summary>
        /// Sets the date filter range to cover the current calendar week starting from Monday.
        /// </summary>
        private void SetActualWeek()
        {
            var today = DateTime.Today;
            int diff = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;

            StartDate = today.AddDays(-diff);
            EndDate = StartDate.AddDays(6);
        }

        /// <summary>
        /// Expands the date filter range to encompass all available measurement records.
        /// </summary>
        private void ShowAllData()
        {
            StartDate = minMeasurementsDate;
            EndDate = maxMeasurementsDate;
        }

        #region Disposal Pattern

        private bool disposed = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the view model and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
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