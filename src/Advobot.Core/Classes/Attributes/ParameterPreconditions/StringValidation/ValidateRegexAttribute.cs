namespace Advobot.Classes.Attributes.ParameterPreconditions.StringValidation
{
	/// <summary>
	/// Validates the regex by making sure it is between 1 and 100 characters.
	/// </summary>
	public sealed class ValidateRegexAttribute : ValidateStringAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateRegexAttribute"/>.
		/// </summary>
		public ValidateRegexAttribute() : base(1, 100) { }
	}
}
