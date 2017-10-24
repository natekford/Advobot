using System;
using System.Globalization;
using System.Windows.Data;

namespace Advobot.UILauncher.Classes.Converters
{
	public class UriConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new Uri(value as string);
		}
		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
