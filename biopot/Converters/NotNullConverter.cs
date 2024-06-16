using System;
using System.Globalization;
using Xamarin.Forms;

namespace biopot.Converters
{
    /// <summary>
    /// Converts any <c>nullable</c> to <c>false</c> or <c>true</c>.
    /// </summary>
    public class NotNullConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object aValue, Type aTargetType, object aParameter, CultureInfo aCulture)
        {
            return aValue != null;
        }

        /// <inheritdoc />
        public object ConvertBack(object aValue, Type aTargetType, object aParameter, CultureInfo aCulture)
        {
            throw new NotSupportedException();
        }
    }
}