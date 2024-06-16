using System;
using Xamarin.Forms;
using biopot.Helpers;
using System.Globalization;

namespace biopot.Converters
{
    public class SignalValuetoBgColorConverter : IValueConverter
    {
        private const int GreenMinValue = 1500;
        private const int GreenMaxValue = 10000;
        private const int RedMinValue = 15000;

        /// <inheritdoc />
        object IValueConverter.Convert(object aValue, Type aTargetType, object aParameter, CultureInfo aCulture)
        {
            var color = default(Color);

            if (aValue is int newValue)
            {
                if (GreenMinValue <= newValue && newValue <= GreenMaxValue)
                {
                    color = StyleManager.GetAppResource<Color>("impedanceGreenColor");
                }
                else if (newValue >= RedMinValue)
                {
                    color = StyleManager.GetAppResource<Color>("impedanceRedColor");
                }
                else
                {
                    color = StyleManager.GetAppResource<Color>("impedanceOrangeColor");
                }
            }

            return color;
        }

        /// <inheritdoc />
        object IValueConverter.ConvertBack(object aValue, Type aTargetType, object aParameter, CultureInfo aCulture)
        {
            throw new NotSupportedException();
        }
    }
}