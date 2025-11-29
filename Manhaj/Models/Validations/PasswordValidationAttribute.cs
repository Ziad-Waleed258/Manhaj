using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Manhaj.Models.Validation
{
    public class PasswordValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string ?? "";
            var errors = new List<string>();

            if (password.Length < 8)
                errors.Add("At least 8 characters");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                errors.Add("At least one uppercase letter");

            if (!Regex.IsMatch(password, @"[a-z]"))
                errors.Add("At least one lowercase letter");

            if (!Regex.IsMatch(password, @"[0-9]"))
                errors.Add("At least one number");

            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?{}|<>]"))
                errors.Add("At least one special character");

            if (errors.Any())
                return new ValidationResult(string.Join(", ", errors));

            return ValidationResult.Success;
        }
    }
}
