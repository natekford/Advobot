using System;
using System.Globalization;

namespace Advobot.SharedUI.Converters
{
	/// <summary>
	/// Returns true if the object is not null or whitespace.
	/// </summary>
	public abstract class NullToBoolConverter
	{
		/// <summary>
		/// Checks if the value is null or whitespace.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> value is string s ? !string.IsNullOrWhiteSpace(s) : value != null;
		/// <summary>
		/// Not implemented.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}
