using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChatApplication
{
    public class BoolToColorConverter : IValueConverter
    {
        public static readonly BoolToColorConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isConnected && isConnected)
            {
                return new SolidColorBrush(Color.FromRgb(37, 211, 102)); // Xanh lá
            }
            return new SolidColorBrush(Color.FromRgb(255, 107, 107)); // Đỏ
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}