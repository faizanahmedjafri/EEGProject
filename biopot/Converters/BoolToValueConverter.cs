using System;
using System.Globalization;
using Xamarin.Forms;

namespace biopot.Converters
{
    public class BoolToValueConverter<TValue> : IValueConverter
    {
        public TValue FalseValue { get; set; }
        public TValue TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return FalseValue;
            else
                return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? value.Equals(TrueValue) : false;
        }
    }

    public class BoolToStyleConverter : BoolToValueConverter<Style> { }
    public class BoolToColorConverter : BoolToValueConverter<Color> { }
    public class BoolToStringConverter : BoolToValueConverter<string> { }
    public class BoolToGridLengthConverter : BoolToValueConverter<GridLength> { }
}
