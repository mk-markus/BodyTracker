
using System;

namespace BF105.Bluetooth.Model
{
    /// <summary>
    /// Represents a synchronized user profile record retrieved from the device's user list characteristic, 
    /// containing demographic attributes and configuration settings[cite: 3].
    /// </summary>
    public class UserListModel
    {
        /// <summary>
        /// Gets or sets the unique memory slot index of the user profile on the scale[cite: 3].
        /// </summary>
        public int? Index { get; set; }

        /// <summary>
        /// Gets or sets the short text identifier or initials associated with the user profile[cite: 3].
        /// </summary>
        public string? Initials { get; set; }

        /// <summary>
        /// Gets or sets the user's birth date used for analytical algorithm computations[cite: 3].
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Gets or sets the height configuration of the user profile[cite: 3].
        /// </summary>
        public int? Heigth { get; set; }

        /// <summary>
        /// Gets or sets the gender classification code for the user profile[cite: 3].
        /// </summary>
        public int? Gender { get; set; }

        /// <summary>
        /// Gets or sets the activity level configuration associated with the user profile[cite: 3].
        /// </summary>
        public int? Activity { get; set; }
    }
}