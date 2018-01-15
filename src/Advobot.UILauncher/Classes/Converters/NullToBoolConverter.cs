using System;
using System.Globalization;
using System.Windows.Data;

namespace Advobot.UILauncher.Classes.Converters
{
	public class NullToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is string s ? !String.IsNullOrWhiteSpace(s) : value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
