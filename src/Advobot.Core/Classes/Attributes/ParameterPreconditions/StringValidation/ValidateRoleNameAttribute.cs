namespace Advobot.Classes.Attributes.ParameterPreconditions.StringValidation
{
	/// <summary>
	/// Validates the role name by making sure it is between 1 and 100 characters.
	/// </summary>
	public class ValidateRoleNameAttribute : ValidateStringAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateRoleNameAttribute"/>.
		/// </summary>
		public ValidateRoleNameAttribute() : base(1, 100) { }
	}
}
