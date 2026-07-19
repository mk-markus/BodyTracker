using BodyTracker.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BodyTracker.Validation
{
    public class IpAddressRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var service = new ConfigrationService();
            string input = value as string ?? string.Empty;

            var (isValid, _, _) = service.checkIPAdressOK(input);

            return isValid
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Ungültiges IP-Format (IPv4/IPv6 erforderlich)");
        }
    }
}
