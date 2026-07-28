namespace BodyTracker.Services
{
    public sealed class NavigationMessage
    {
        public string Target { get; }
        public NavigationMessage(string target) => Target = target;
        public static string ShowDashboardPage => "ShowDashboardPage";
        public static string ShowMeasurementDatabasePage => "ShowMeasurementDatabasePageAsync";
        public static string ShowNewEntryPage => "ShowNewEntryPage";
        public static string ShowMultiChartPage => "ShowMultiChartPage";
    }
}
