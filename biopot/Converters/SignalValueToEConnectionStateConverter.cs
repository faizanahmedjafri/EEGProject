using System;
using System.Globalization;
using biopot.Enums;
using Xamarin.Forms;

namespace biopot.Converters
{
	public class SignalValueToEConnectionStateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int newValue = 0;
			Int32.TryParse(value.ToString(), out newValue);
			ESensorConnectionState state = default(ESensorConnectionState);

			if (newValue <= 5000)
			{
				state = ESensorConnectionState.GoodConnection;
			}
			else if (newValue > 5000 && newValue <= 10000)
			{
				state = ESensorConnectionState.NormalConnection;
			}
			else if (newValue > 10000)
			{
				state = ESensorConnectionState.BadConnection;
			}

			return state;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
