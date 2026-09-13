using System.Collections.Generic;
using System.Linq;

namespace BodyTracker.Models.Languages
{
    /// <summary>
    /// Provides a static repository of supported application languages, mapping display names to culture codes.
    /// </summary>
    static class CultureCodeModel
    {
        /// <summary>
        /// A dictionary mapping human-readable language display names to their corresponding culture codes.
        /// </summary>
        private static Dictionary<string, string> CultureDictionary = new Dictionary<string, string>()
        {
            { "Englisch","en-US" },
            { "Deutsch", "de-AT" }
        };

        /// <summary>
        /// Retrieves a list of all available language display names supported by the application.
        /// </summary>
        /// <returns>A list containing the display names of all supported languages.</returns>
        static public List<string> Languages()
        {
            return CultureDictionary.Select(x => x.Key).ToList();
        }

        /// <summary>
        /// Gets the corresponding culture code for a specified language display name.
        /// </summary>
        /// <param name="language">The human-readable language display name.</param>
        /// <returns>The culture code string (e.g., "en-US"), or an empty string if the language is not found.</returns>
        static public string GetCultureCode(string language)
        {
            CultureDictionary.TryGetValue(language, out string? code);

            if (code == null) return "";
            return code;
        }
    }
}
