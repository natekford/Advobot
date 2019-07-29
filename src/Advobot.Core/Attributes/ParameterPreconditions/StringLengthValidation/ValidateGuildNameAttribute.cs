using System;

namespace Advobot.Attributes.ParameterPreconditions.StringLengthValidation
{
	/// <summary>
	/// Validates the guild name by making sure it is between 2 and 100 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateGuildNameAttribute : ValidateStringLengthAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateGuildNameAttribute"/>.
		/// </summary>
		public ValidateGuildNameAttribute() : base(2, 100) { }
	}
}
