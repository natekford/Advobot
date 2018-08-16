using System;
using System.Globalization;
using System.Windows.Data;

namespace Advobot.NetFrameworkUI.Classes.Converters
{
	/// <summary>
	/// Resizes text.
	/// </summary>
	public sealed class FontResizeConverter : IValueConverter
	{
		/// <summary>
		/// What to shrink by.
		/// </summary>
		public double ConvertFactor { get; }

		/// <summary>
		/// Creates an instance of <see cref="FontResizeConverter"/>.
		/// </summary>
		/// <param name="convertFactor"></param>
		public FontResizeConverter(double convertFactor)
		{
			ConvertFactor = convertFactor;
		}

		/// <summary>
		/// Converts the passed in value to a smaller number.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Math.Max((int)(System.Convert.ToInt16(value) * ConvertFactor), 1);
		}
		/// <summary>
		/// Not implemented.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
