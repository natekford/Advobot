namespace Advobot.Classes.Attributes.ParameterPreconditions.StringValidation
{
	/// <summary>
	/// Validates the bot prefix by making sure it is between 1 and 10 characters.
	/// </summary>
	public sealed class ValidatePrefixAttribute : ValidateStringAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidatePrefixAttribute"/>.
		/// </summary>
		public ValidatePrefixAttribute() : base(1, 10) { }
	}
}
