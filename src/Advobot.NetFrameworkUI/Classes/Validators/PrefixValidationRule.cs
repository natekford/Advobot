using System;
using System.Globalization;
using System.Windows.Controls;

namespace Advobot.NetFrameworkUI.Classes.Validators
{
	/// <summary>
	/// Validation rule for the bot prefix.
	/// </summary>
	public sealed class PrefixValidationRule : ValidationRule
	{
		/// <summary>
		/// Determines whether the passed in object is a valid prefix.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="cultureInfo"></param>
		/// <returns></returns>
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			return !String.IsNullOrWhiteSpace(value.ToString())
				? ValidationResult.ValidResult
				: new ValidationResult(false, "Invalid prefix.");
		}
	}
}