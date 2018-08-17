using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Advobot.NetFrameworkUI.Utilities;

namespace Advobot.NetFrameworkUI.Classes.Converters
{
	/// <summary>
	/// Converts a <see cref="SolidColorBrush"/>.
	/// </summary>
	public sealed class ColorConverter : IValueConverter
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
			return (value ?? "").ToString();
		}
		/// <summary>
		/// Converts a string to <see cref="SolidColorBrush"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new NetFrameworkBrushFactory().CreateBrush((value ?? "").ToString());
		}
	}
}