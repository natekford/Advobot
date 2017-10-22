using System;
using System.Globalization;
using System.Windows.Data;

namespace Advobot.UILauncher.Classes
{
	internal class FontResizer : IValueConverter
	{
		private double _ConvertFactor;

		public FontResizer(double convertFactor)
		{
			_ConvertFactor = convertFactor;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var converted = (int)(System.Convert.ToInt16(value) * _ConvertFactor);
			return Math.Max(converted, -1);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
