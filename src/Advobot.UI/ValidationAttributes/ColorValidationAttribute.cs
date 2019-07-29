using Advobot.UI.Colors;
using System.ComponentModel.DataAnnotations;

namespace Advobot.UI.ValidationAttributes
{
	/// <summary>
	/// Validation attribute for colors.
	/// </summary>
	public sealed class ColorValidationAttribute : ValidationAttribute
	{
		private static readonly NetCoreBrushFactory _Factory = new NetCoreBrushFactory();

		/// <summary>
		/// Determines whether the passed in object is a valid color.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="validationContext"></param>
		/// <returns></returns>
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			return !(value is string str) || _Factory.TryCreateBrush(str, out var _)
				? ValidationResult.Success
				: new ValidationResult("Invalid color.");
		}
	}
}