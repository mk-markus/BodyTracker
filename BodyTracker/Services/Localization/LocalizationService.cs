using BodyTracker.Resources.Languages;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace BodyTracker.Services.Localization
{

    /// <summary>
    /// Provides singleton localization management services, handling culture changes and notifying listeners 
    /// for dynamic runtime language switching across the application.
    /// </summary>
    public sealed class LocalizationService : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="LocalizationService"/> class.
        /// </summary>
        public static LocalizationService Instance { get; } = new();

        /// <summary>
        /// Occurs when a property value changes or when the application culture is updated.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Initializes a new private instance of the <see cref="LocalizationService"/> class for the singleton pattern.
        /// </summary>
        private LocalizationService()
        {
        }

        /// <summary>
        /// Changes the active application culture across all threads and resource managers, 
        /// and raises change notifications to refresh all localized UI bindings.
        /// </summary>
        /// <param name="culture">The target culture code string (e.g., "en-US", "de-AT") to apply.</param>
        public void ChangeLanguage(string culture)
        {
            var _culture = new CultureInfo(culture);

            CultureInfo.CurrentCulture = _culture;
            CultureInfo.CurrentUICulture = _culture;
            CultureInfo.DefaultThreadCurrentCulture = _culture;
            CultureInfo.DefaultThreadCurrentUICulture = _culture;

            UiStrings.Culture = _culture;

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(null));
        }
    }
}