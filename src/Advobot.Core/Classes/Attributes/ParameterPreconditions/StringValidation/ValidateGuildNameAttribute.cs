namespace Advobot.Classes.Attributes.ParameterPreconditions.StringValidation
{
	/// <summary>
	/// Validates the guild name by making sure it is between 2 and 100 characters.
	/// </summary>
	public sealed class ValidateGuildNameAttribute : ValidateStringAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateGuildNameAttribute"/>.
		/// </summary>
		public ValidateGuildNameAttribute() : base(2, 100) { }
	}
}
