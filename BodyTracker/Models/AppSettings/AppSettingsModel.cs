using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyTracker.Models
{
    /// <summary>
    /// Represents the data model used for persisting general application settings, 
    /// including default import directories and language preferences.
    /// </summary>
    public record AppSettingsModel
    {
        /// <summary>
        /// Gets or sets the default file system path used as the initial directory for data imports.
        /// </summary>
        public string DefaultImportFolder { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the preferred language culture code (e.g., "en-US", "de-AT") configured for the application.
        /// </summary>
        public string Language { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scale user identifier or name string associated with the active Bluetooth device communication profile.
        /// </summary>
        public string ScaleUser { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the numerical PIN code required for authorizing user sessions and data synchronization with the scale.
        /// </summary>
        public string ScaleUserPin { get; set; } = "0000";


    }
}
