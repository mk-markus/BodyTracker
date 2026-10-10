using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyTracker.Models.WorkoutLog
{
    /// <summary>
    /// Represents configuration settings required for authenticating and connecting to the Hevy workout application API.
    /// </summary>
    public record HevyAppAPIConfigurationModel
    {
        /// <summary>
        /// Gets or sets the API key used for authorized access to the Hevy app services.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

    }
}