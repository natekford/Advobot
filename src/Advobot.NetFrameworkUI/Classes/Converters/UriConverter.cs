using System;
using System.Globalization;
using System.Windows.Data;

namespace Advobot.NetFrameworkUI.Classes.Converters
{
	/// <summary>
	/// Converts a string to a uri.
	/// </summary>
	public sealed class UriConverter : IValueConverter
	{
		/// <summary>
		/// Converts the passed in value to a uri.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new Uri(value as string);
		}
		/// <summary>
		/// Not implemented.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetTypes"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
