using System.Globalization;
using System.Windows.Controls;

namespace BodyTracker.Services
{
    /// <summary>
    /// Represents a WPF validation rule used to validate Internet Protocol (IP) address inputs.
    /// </summary>
    /// <remarks>Inherits from <see cref="ValidationRule"/> and utilizes a <see cref="DatabaseConfigrationService"/> to verify whether the provided string input conforms to valid IPv4 or IPv6 address formats.</remarks>
    public class IpAddressRule : ValidationRule
    {
        /// <summary>
        /// Validates the specified value against IP address formatting criteria.
        /// </summary>
        /// <remarks>Instantiates a configuration service, converts the input object to a string, checks its validity, and returns a successful validation result or an error message accordingly.</remarks>
        /// <param name="value">The value from the binding target to evaluate.</param>
        /// <param name="cultureInfo">The culture to use in this rule.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating whether the input is valid.</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var service = new DatabaseConfigrationService();
            string input = value as string ?? string.Empty;

            var (isValid, _, _) = service.checkIPAdressOK(input);

            return isValid
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Ungültiges IP-Format (IPv4/IPv6 erforderlich)");
        }
    }
}
