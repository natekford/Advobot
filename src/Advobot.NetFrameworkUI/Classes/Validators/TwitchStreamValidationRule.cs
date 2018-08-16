using System.Globalization;
using System.Windows.Controls;
using AdvorangesUtils;

namespace Advobot.NetFrameworkUI.Classes.Validators
{
	/// <summary>
	/// Validation rule for Twitch.tv stream names.
	/// </summary>
	public sealed class TwitchStreamValidationRule : ValidationRule
	{
		/// <summary>
		/// Determines whether the passed in object is a valid name.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="cultureInfo"></param>
		/// <returns></returns>
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			return RegexUtils.IsValidTwitchName(value.ToString())
				? ValidationResult.ValidResult
				: new ValidationResult(false, "Invalid Twitch stream name.");
		}
	}
}
