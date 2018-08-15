using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Advobot.Windows.Classes.Converters
{
	/// <summary>
	/// Tricks PropertyUpdated into working one way due to making ReferenceEquals fail by recreating the enumerable.
	/// </summary>
	public sealed class EnumerableConverter : IValueConverter
	{
		/// <summary>
		/// Casts the value to an IEnumerable then returns a newly created IEnumerable.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((IEnumerable)value).OfType<object>();
		}
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
		{
			throw new NotImplementedException();
		}
	}
}