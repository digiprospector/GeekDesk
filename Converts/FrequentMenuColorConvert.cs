using GeekDesk.Constant;
using System;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Data;

namespace GeekDesk.Converts
{
    public class FrequentMenuColorConvert : IValueConverter
    {
        private const string GoldColor = "#FF2F4F4F";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isFrequent = value is MenuType menuType && menuType == MenuType.FREQUENT;
            if (!isFrequent && value is string color)
            {
                return new BrushConverter().ConvertFromString(color);
            }
            if (isFrequent)
            {
                return new BrushConverter().ConvertFromString(GoldColor);
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
