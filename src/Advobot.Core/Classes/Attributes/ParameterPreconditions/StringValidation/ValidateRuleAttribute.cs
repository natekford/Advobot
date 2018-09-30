namespace Advobot.Classes.Attributes.ParameterPreconditions.StringValidation
{
	/// <summary>
	/// Validates the rule by making sure it is between 1 and 150 characters.
	/// </summary>
	public class ValidateRuleAttribute : ValidateStringAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateRuleAttribute"/>.
		/// </summary>
		public ValidateRuleAttribute() : base(1, 150) { }
	}
}
