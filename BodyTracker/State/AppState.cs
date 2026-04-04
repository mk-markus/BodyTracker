namespace BodyTracker.State
{
    public static class AppState
    {
        /// <summary>
        /// Gets or sets the globally unique identifier (ID) of the currently active person.
        /// This ID serves as the primary foreign key filter for all database queries 
        /// within the current session.
        /// </summary>
        /// <value>
        /// An integer representing the <c>PersonID</c> from the database. 
        /// A value of 0 or less typically indicates that no person is currently selected.
        /// </value>
        public static int SelectedPersonId { get; set; }

        /// <summary>
        /// Gets or sets the height of the selected person, in meters.
        /// </summary>
        public static float SelectedPersonHeight { get; set; }


        // <summary>
        /// Gets or sets the display name of the currently selected person.
        /// This property provides a user-friendly identifier for the UI header or title bars.
        /// </summary>
        /// <value>
        /// A string representing the person's name. Defaults to <see cref="string.Empty"/> 
        /// to prevent null reference issues during initial UI binding.
        /// </value>
        public static string SelectedPersonName { get; set; } = string.Empty;
    }
}
