using BodyTracker.Resources.Languages;
using System.ComponentModel;
using System.Globalization;

namespace BodyTracker.Services.Localization
{
    /// <summary>
    /// Represents the data binding wrapper for localized strings, implementing <see cref="INotifyPropertyChanged"/> 
    /// to dynamically update UI text when the application culture changes.
    /// </summary>
    public class TranslateData : INotifyPropertyChanged
    {
        /// <summary>
        /// The resource key used to look up translated strings in the resource manager.
        /// </summary>
        private readonly string _key;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslateData"/> class with the specified resource key, 
        /// subscribing to localization service change notifications.
        /// </summary>
        /// <param name="key">The resource key identifying the target translation string.</param>
        public TranslateData(string key)
        {
            _key = key;

            LocalizationService.Instance.PropertyChanged += (_, _) =>
            {
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(Value)));
            };
        }

        /// <summary>
        /// Gets the localized string corresponding to the resource key for the current UI culture, 
        /// or falls back to the key itself if the resource is not found.
        /// </summary>
        public string Value =>
            UiStrings.ResourceManager.GetString(
                _key,
                CultureInfo.CurrentUICulture) ?? _key;
    }
}
