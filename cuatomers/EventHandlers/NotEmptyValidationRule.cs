using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace cuatomers.EventHandlers
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value as string;
            return string.IsNullOrWhiteSpace(input)
                ? new ValidationResult(false, "This field is required.")
                : ValidationResult.ValidResult;
        }
    }

    public class EmailValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value as string;
            return string.IsNullOrWhiteSpace(input) || !Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")
                ? new ValidationResult(false, "Invalid email address.")
                : ValidationResult.ValidResult;
        }
    }

    public class MobileValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value as string;
            return string.IsNullOrWhiteSpace(input) || !Regex.IsMatch(input, @"^\d{10}$")
                ? new ValidationResult(false, "Invalid mobile number.")
                : ValidationResult.ValidResult;
        }
    }
}
