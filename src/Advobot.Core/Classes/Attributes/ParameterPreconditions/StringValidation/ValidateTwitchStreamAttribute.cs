using AdvorangesUtils;

namespace Advobot.Classes.Attributes.ParameterPreconditions.StringValidation
{
	/// <summary>
	/// Validates the Twitch stream name by making sure it is between 4 and 25 characters and matches a Regex for Twitch usernames.
	/// </summary>
	public class ValidateTwitchStreamAttribute : ValidateStringAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateTwitchStreamAttribute"/>.
		/// </summary>
		public ValidateTwitchStreamAttribute() : base(4, 25) { }

		/// <inheritdoc />
		public override bool AdditionalValidation(string s, out string? error)
		{
			var success = RegexUtils.IsValidTwitchName(s);
			error = success ? null : "Invalid Twitch username supplied.";
			return success;
		}
	}
}
