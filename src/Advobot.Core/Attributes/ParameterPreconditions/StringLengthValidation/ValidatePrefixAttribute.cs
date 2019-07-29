using System;

namespace Advobot.Attributes.ParameterPreconditions.StringLengthValidation
{
	/// <summary>
	/// Validates the bot prefix by making sure it is between 1 and 10 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidatePrefixAttribute : ValidateStringLengthAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidatePrefixAttribute"/>.
		/// </summary>
		public ValidatePrefixAttribute() : base(1, 10) { }
	}
}
