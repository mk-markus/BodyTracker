using BodyTracker.Models.Languages;
using BodyTracker.Services.Localization;
using BodyTracker.Services.WorkoutLog;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace BodyTracker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var appSettings = new AppSettingsService();
            var cfg = appSettings.LoadConfigurationFile();

            var cult = CultureCodeModel.GetCultureCode(cfg.Language);


            var culture = new CultureInfo(cult);
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(cult)));
            ShutdownMode = ShutdownMode.OnLastWindowClose;


        }
    }
}
