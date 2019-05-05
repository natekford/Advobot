using System;
using System.Globalization;

namespace Advobot.NetCoreUI.Classes.AbstractUI.Converters
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
		public object Convert(object value, Type _1, object _2, CultureInfo _3)
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
		public object ConvertBack(object _1, Type _2, object _3, CultureInfo _4)
			=> throw new NotImplementedException();
	}
}
