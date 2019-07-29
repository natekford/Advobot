using System;

namespace Advobot.Classes.Attributes.ParameterPreconditions.StringLengthValidation
{
	/// <summary>
	/// Validates the role name by making sure it is between 1 and 100 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateRoleNameAttribute : ValidateStringLengthAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateRoleNameAttribute"/>.
		/// </summary>
		public ValidateRoleNameAttribute() : base(1, 100) { }
	}
}
