using System.ComponentModel.DataAnnotations;

namespace WebAPI.Validations
{
    public class FirstLetterUppercaseAttribute: ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(value is null || string.IsNullOrEmpty(value.ToString())) return null;

            var valueString = value.ToString()!;
            var firstLetter = valueString[0].ToString();

            if (firstLetter != firstLetter.ToUpper()) return new ValidationResult("La primera letra debe ser mayúscula");

            return ValidationResult.Success;

        }
    }
}
