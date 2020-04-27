using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using AutomatedWorkplace.Models;
using ReactiveValidation;

namespace AutomatedWorkplace.Converters {
    public class ValidatorMessagesConverter : IValueConverter{
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is IEnumerable<ValidationMessage> result)) {
                return null;
            }

            string message = result.Aggregate("", (current, validationMessage) => current + validationMessage + "\n").Trim();
            return message;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}