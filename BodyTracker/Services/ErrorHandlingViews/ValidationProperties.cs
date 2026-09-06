using System.Windows;

namespace BodyTracker.Services.ErrorHandlingViews
{
    public static class ValidationProperties
    {
        // Eigenschaft für den Validierungsstatus (Boolean)
        public static readonly DependencyProperty HasErrorProperty =
            DependencyProperty.RegisterAttached(
                "HasError",
                typeof(bool),
                typeof(ValidationProperties),
                new PropertyMetadata(false));

        public static void SetHasError(UIElement element, bool value) => element.SetValue(HasErrorProperty, value);
        public static bool GetHasError(UIElement element) => (bool)element.GetValue(HasErrorProperty);

        // Eigenschaft für die Fehlermeldung im Tooltip (String)
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.RegisterAttached(
                "ErrorMessage",
                typeof(string),
                typeof(ValidationProperties),
                new PropertyMetadata(string.Empty));

        public static void SetErrorMessage(UIElement element, string value) => element.SetValue(ErrorMessageProperty, value);
        public static string GetErrorMessage(UIElement element) => (string)element.GetValue(ErrorMessageProperty);
    }
}
