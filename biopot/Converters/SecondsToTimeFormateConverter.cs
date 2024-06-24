using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace biopot.Converters
{
    public class SecondsToTimeFormateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int seconds)
            {
                TimeSpan time = TimeSpan.FromSeconds(seconds);
                return $"{time:mm\\:ss}";
            }
            return "00:00"; // Default value or error handling
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
