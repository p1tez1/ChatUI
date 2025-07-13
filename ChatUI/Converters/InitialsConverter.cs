using System;
using System.Globalization;
using System.Windows.Data;

namespace ChatUI
{
    public class InitialsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string username = value as string;
            if (string.IsNullOrWhiteSpace(username))
                return "?";

            return username.Substring(0, 1).ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
