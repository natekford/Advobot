using System;
using System.Globalization;
using System.Windows.Controls;

namespace Advobot.Windows.Classes.Validators
{
	/// <summary>
	/// Validation rule for user ids.
	/// </summary>
	public sealed class UserIdValidationRule : ValidationRule
	{
		/// <summary>
		/// Determines whether the passed in object is a valid user id.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="cultureInfo"></param>
		/// <returns></returns>
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			return !(value is string str) || String.IsNullOrWhiteSpace(str) || ulong.TryParse(str, out var ul)
				? ValidationResult.ValidResult
				: new ValidationResult(false, "Invalid user id.");
		}
	}
}
