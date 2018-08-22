using System.Globalization;
using System.Windows.Controls;
using Advobot.NetFrameworkUI.Classes.Colors;

namespace Advobot.NetFrameworkUI.Classes.ValidationRules
{
	/// <summary>
	/// Validation rule for colors.
	/// </summary>
	public sealed class ColorValidationRule : ValidationRule
	{
		private static readonly NetFrameworkBrushFactory _Factory = new NetFrameworkBrushFactory();

		/// <summary>
		/// Determines whether the passed in object is a valid color.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="cultureInfo"></param>
		/// <returns></returns>
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			return !(value is string str) || _Factory.TryCreateBrush(str, out var brush)
				? ValidationResult.ValidResult
				: new ValidationResult(false, "Invalid color.");
		}
	}
}