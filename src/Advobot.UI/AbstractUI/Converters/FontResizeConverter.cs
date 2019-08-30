﻿using System;
using System.Globalization;

namespace Advobot.UI.AbstractUI.Converters
{
	/// <summary>
	/// Resizes text.
	/// </summary>
	public abstract class FontResizeConverter
	{
		/// <summary>
		/// Creates an instance of <see cref="FontResizeConverter"/>.
		/// </summary>
		/// <param name="convertFactor"></param>
		protected FontResizeConverter(double convertFactor)
		{
			ConvertFactor = convertFactor;
		}

		/// <summary>
		/// What to shrink by.
		/// </summary>
		public double ConvertFactor { get; set; }

		/// <summary>
		/// Converts the passed in value to a smaller number.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object Convert(object value, Type _1, object _2, CultureInfo _3)
		{
			if (!(value is double dVal))
			{
				throw new InvalidOperationException("Unable to resize font if the passed in value is not a double.");
			}
			if (double.IsNaN(dVal))
			{
				return 1;
			}
			return Math.Max((int)(dVal * ConvertFactor), 1);
		}

		/// <summary>
		/// Not implemented.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object ConvertBack(object _1, Type _2, object _3, CultureInfo _4)
			=> throw new NotImplementedException();
	}
}