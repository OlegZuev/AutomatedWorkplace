using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using AutomatedWorkplace.Models;
using ReactiveValidation;

namespace AutomatedWorkplace.Validators {
    public class TableObjectValidator : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if (!(value is BindingGroup bindingGroup) || bindingGroup.Items.Count == 0)
                return ValidationResult.ValidResult;

            if (!(bindingGroup.Items[0] is ValidatableObject validatableObject))
                return new ValidationResult(false, "Row doesn't exist");

            return new ValidationResult(validatableObject.Validator.IsValid, null);
        }
    }
}