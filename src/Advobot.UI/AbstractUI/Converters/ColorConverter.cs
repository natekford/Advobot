using System;
using System.Globalization;

using Advobot.UI.AbstractUI.Colors;

namespace Advobot.UI.AbstractUI.Converters
{
	/// <summary>
	/// Converts a <typeparamref name="TBrush"/> to a string and vice versa.
	/// </summary>
	/// <typeparam name="TBrush"></typeparam>
	/// <typeparam name="TBrushFactory"></typeparam>
	public abstract class ColorConverter<TBrush, TBrushFactory>
		where TBrushFactory : BrushFactory<TBrush>, new()
	{
#pragma warning disable RCS1163 // Unused parameter.

		private static readonly TBrushFactory _Factory = new TBrushFactory();

		/// <summary>
		/// Converts a <typeparamref name="TBrush"/> to a string.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="_"></param>
		/// <param name="_2"></param>
		/// <param name="_3"></param>
		/// <returns></returns>
		public object Convert(object value, Type _, object _2, CultureInfo _3)
		{
			if (value is TBrush brush)
			{
				return _Factory.FormatBrush(brush);
			}
			throw new InvalidOperationException("Invalid brush supplied for converting.");
		}

		/// <summary>
		/// Converts a string to a <typeparamref name="TBrush"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="_"></param>
		/// <param name="_2"></param>
		/// <param name="_3"></param>
		/// <returns></returns>
		public object ConvertBack(object value, Type _, object _2, CultureInfo _3)
		{
			if (value is string str && _Factory.CreateBrush(str) is TBrush brush)
			{
				return brush;
			}
			throw new InvalidOperationException("Brush cannot be null when converting back.");
		}

#pragma warning enable RCS1163 // Unused parameter.
	}
}