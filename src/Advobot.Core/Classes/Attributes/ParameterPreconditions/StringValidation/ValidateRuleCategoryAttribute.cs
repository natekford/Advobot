namespace Advobot.Classes.Attributes.ParameterPreconditions.StringValidation
{
	/// <summary>
	/// Validates the rule category by making sure it is between 1 and 250 characters.
	/// </summary>
	public class ValidateRuleCategoryAttribute : ValidateStringAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateRuleCategoryAttribute"/>.
		/// </summary>
		public ValidateRuleCategoryAttribute() : base(1, 250) { }
	}
}
