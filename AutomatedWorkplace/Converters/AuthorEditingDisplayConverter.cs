using System;
using System.Globalization;
using System.Windows.Data;
using AutomatedWorkplace.Models;

namespace AutomatedWorkplace.Converters {
    public class AuthorEditingDisplayConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var temp = value as Author;
            return temp?.FIO + " " + temp?.Pseudonym;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}