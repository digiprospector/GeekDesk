using GeekDesk.Constant;
using GeekDesk.ViewModel;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GeekDesk.Converts
{
    public class FrequentMenuGeometryConvert : IValueConverter
    {
        private const string StarGeometry = "M12 2l2.9 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l7.1-1.01L12 2z";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MenuInfo menu = value as MenuInfo;
            if (menu != null && menu.MenuType == MenuType.FREQUENT)
            {
                return StarGeometry;
            }
            return menu?.MenuGeometry;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
