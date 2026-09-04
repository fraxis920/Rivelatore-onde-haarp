using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DataHandler
{
    public class BoolToBrushConverter : IValueConverter
    {
        public static readonly BoolToBrushConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isOk = value is bool b && b;
            return isOk ? new SolidColorBrush(Color.FromRgb(0, 230, 118)) : new SolidColorBrush(Color.FromRgb(255, 82, 82));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class StatusToBrushConverter : IValueConverter
    {
        public static readonly StatusToBrushConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isOk = value is bool b && b;
            return isOk ? new SolidColorBrush(Color.FromRgb(0, 230, 118)) : new SolidColorBrush(Color.FromRgb(255, 193, 7));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BoolToVisConverter : IValueConverter
    {
        public static readonly BoolToVisConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = value is bool b && b;
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class InverseBoolToVisConverter : IValueConverter
    {
        public static readonly InverseBoolToVisConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTrue = value is bool b && b;
            return isTrue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class TabToBoolConverter : IValueConverter
    {
        public static readonly TabToBoolConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int currentIndex && parameter != null && int.TryParse(parameter.ToString(), out int targetIndex))
            {
                return currentIndex == targetIndex;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter != null && int.TryParse(parameter.ToString(), out int targetIndex))
            {
                return targetIndex;
            }
            return Binding.DoNothing;
        }
    }

    public class TabToVisConverter : IValueConverter
    {
        public static readonly TabToVisConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int currentIndex && parameter != null && int.TryParse(parameter.ToString(), out int targetIndex))
            {
                return currentIndex == targetIndex ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
