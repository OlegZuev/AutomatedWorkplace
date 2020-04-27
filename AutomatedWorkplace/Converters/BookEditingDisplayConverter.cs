using System;
using System.Globalization;
using System.Windows.Data;
using AutomatedWorkplace.Models;

namespace AutomatedWorkplace.Converters {
    public class BookEditingDisplayConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var temp = value as Book;
            return temp?.Name + " " + temp?.ISBN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}