using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace BodyTracker.Services.Localization
{
    [MarkupExtensionReturnType(typeof(object))]
    public class TranslateExtension : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the localization resource key used to look up translated text values.
        /// </summary>
        [ConstructorArgument("key")]
        public string Key { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslateExtension"/> class.
        /// Required by XAML parser for default instantiation.
        /// </summary>
        public TranslateExtension()
        {
        }

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
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Key))
            {
                return string.Empty;
            }

            return new Binding(nameof(TranslateData.Value))
            {
                Source = new TranslateData(Key),
                Mode = BindingMode.OneWay
            }.ProvideValue(serviceProvider);
        }
    }
}