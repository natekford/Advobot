using System;
using System.Globalization;
using System.Windows.Data;

namespace Advobot.UILauncher.Classes.Converters
{
	public class NullToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//Extremely complex converter
			return value != null;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
