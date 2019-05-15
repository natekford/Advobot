namespace Advobot.Classes.Attributes.ParameterPreconditions.StringValidation
{
	/// <summary>
	/// Validates the game by making sure it is between 0 and 128 characters.
	/// </summary>
	public sealed class ValidateGameAttribute : ValidateStringAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateGameAttribute"/>.
		/// </summary>
		public ValidateGameAttribute() : base(0, 128) { }
	}
}
