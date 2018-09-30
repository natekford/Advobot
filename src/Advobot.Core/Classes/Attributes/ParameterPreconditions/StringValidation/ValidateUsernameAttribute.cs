namespace Advobot.Classes.Attributes.ParameterPreconditions.StringValidation
{
	/// <summary>
	/// Validates the username by making sure it is between 2 and 32 characters.
	/// </summary>
	public class ValidateUsernameAttribute : ValidateStringAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateUsernameAttribute"/>.
		/// </summary>
		public ValidateUsernameAttribute() : base(2, 32) { }
	}
}
