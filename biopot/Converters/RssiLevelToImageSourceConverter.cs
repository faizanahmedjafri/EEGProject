using System;
using System.Globalization;
using Xamarin.Forms;

namespace biopot.Converters
{
	public class RssiLevelToImageSourceConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int val = (int)value;
			ImageSource source = "connect_1";
			if (val < -64)
			{
				source = "connect_1";
			}
			else if (val < 0)
			{
				source = "connect_2";
			}
			else if (val < 64)
			{
				source = "connect_3";
			}
			else
			{
				source = "connect_4";
			}

			return source;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

