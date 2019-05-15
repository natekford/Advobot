namespace Advobot.Classes.Attributes.ParameterPreconditions.StringValidation
{
	/// <summary>
	/// Validates the emote name by making sure it is between 2 and 32 characters.
	/// </summary>
	public sealed class ValidateEmoteNameAttribute : ValidateStringAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateEmoteNameAttribute"/>.
		/// </summary>
		public ValidateEmoteNameAttribute() : base(2, 32) { }
	}
}
