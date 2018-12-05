namespace Advobot.Classes.Attributes.ParameterPreconditions.StringValidation
{
	/// <summary>
	/// Validates the text channel name by making sure it is between 2 and 100 characters and has no spaces.
	/// </summary>
	public class ValidateTextChannelNameAttribute : ValidateChannelNameAttribute
	{
		/// <inheritdoc />
		public override bool AdditionalValidation(string s, out string? error)
		{
			var success = !s.Contains(" ");
			error = success ? null : "Spaces are not allowed in a text channel name.";
			return success;
		}
	}
}
