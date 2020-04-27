using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutomatedWorkplace.Converters {
    public class NewPlaceholderToNullConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value ?? DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || value.GetType().Name == "NamedObject") {
                return null;
            }

            return value;
        }
    }
}