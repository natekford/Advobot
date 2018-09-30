namespace Advobot.Classes.Attributes.ParameterPreconditions.StringValidation
{
	/// <summary>
	/// Validates the nickname by making sure it is between 1 and 32 characters.
	/// </summary>
	public class ValidateNicknameAttribute : ValidateStringAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateNicknameAttribute"/>.
		/// </summary>
		public ValidateNicknameAttribute() : base(1, 32) { }
	}
}
