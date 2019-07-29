using System.ComponentModel.DataAnnotations;
using AdvorangesUtils;

namespace Advobot.UI.ValidationAttributes
{
	/// <summary>
	/// Validation attribute for Twitch.tv streams.
	/// </summary>
	public sealed class TwitchStreamValidationAttribute : ValidationAttribute
	{
		/// <summary>
		/// Determines whether the passed in object is a Twitch.tv stream.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="validationContext"></param>
		/// <returns></returns>
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			return RegexUtils.IsValidTwitchName(value?.ToString())
				? ValidationResult.Success
				: new ValidationResult("Invalid Twitch stream.");
		}
	}
}
