using System;

namespace Advobot.Attributes.ParameterPreconditions.StringLengthValidation
{
	/// <summary>
	/// Validates the rule by making sure it is between 1 and 150 characters and that it does not already exist.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateRuleAttribute : ValidateStringLengthAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateRuleAttribute"/>.
		/// </summary>
		public ValidateRuleAttribute() : base(1, 150) { }
	}
}
