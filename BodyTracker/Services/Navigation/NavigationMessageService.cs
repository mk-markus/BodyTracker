namespace BodyTracker.Services
{
    public sealed class NavigationMessage
    {/// <summary>
     /// Gets the target destination identifier for the navigation event.
     /// </summary>
     /// <remarks>Represents the unique string key indicating which view or page should be navigated to.</remarks>
        public string Target { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationMessage"/> class with the specified target destination.
        /// </summary>
        /// <remarks>Constructs a navigation message payload carrying the target view identifier.</remarks>
        /// <param name="target">The target view or page identifier string.</param>
        public NavigationMessage(string target) => Target = target;

        /// <summary>
        /// Gets the navigation target key for displaying the dashboard page.
        /// </summary>
        public static string ShowDashboard => "ShowDashboard";

        /// <summary>
        /// Gets the navigation target key for displaying the bodyMeasurement database page.
        /// </summary>
        public static string ShowMeasurement => "ShowMultiDBEntryViewAsync";

        /// <summary>
        /// Gets the navigation target key for displaying the new entry creation page.
        /// </summary>
        public static string ShowNewEntry => "ShowNewEntry";

        /// <summary>
        /// Gets the navigation target key for displaying the multi-chart visualization page.
        /// </summary>
        public static string ShowMultiChart => "ShowMultiChart";

        /// <summary>
        /// Gets the navigation target key for displaying the Launcher Home Page visualization page.
        /// </summary>
        public static string ShowLauncher => "ShowLauncher";

        /// <summary>
        /// Gets the navigation target key for displaying the Launcher Home Page visualization page.
        /// </summary>
        public static string ShowInfo => "ShowInfo";

    }
}
