using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace BodyTracker.Services.Localization
{
    /// <summary>
    /// Implements a custom XAML markup extension to provide seamless localization and text translation 
    /// support for UI elements using unique resource keys.
    /// </summary>
    public class TranslateExtension : MarkupExtension
    {
        /// <summary>
        /// Gets the localization resource key used to look up translated text values.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslateExtension"/> class with the specified translation key.
        /// </summary>
        /// <param name="key">The resource key identifying the target translation string.</param>
        public TranslateExtension(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Returns an object that is provided as the value of the target property for this markup extension, 
        /// creating a one-way binding to the localized translation data.
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>A binding object evaluating to the localized text value.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new Binding(nameof(TranslateData.Value))
            {
                Source = new TranslateData(Key),
                Mode = BindingMode.OneWay
            }.ProvideValue(serviceProvider);
        }
    }
}