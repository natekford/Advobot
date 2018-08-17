using System.Globalization;
using System.Windows.Controls;
using Advobot.NetFrameworkUI.Utilities;

namespace Advobot.NetFrameworkUI.Classes.Validators
{
	/// <summary>
	/// Validation rule for the bot prefix.
	/// </summary>
	public sealed class ColorValidator : ValidationRule
	{
		/// <summary>
		/// Determines whether the passed in object is a valid prefix.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="cultureInfo"></param>
		/// <returns></returns>
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			return !(value is string str) || new NetFrameworkBrushFactory().TryCreateBrush(str, out var brush)
				? ValidationResult.ValidResult
				: new ValidationResult(false, "Invalid color supplied.");
		}
	}
}