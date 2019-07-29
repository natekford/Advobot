using System;

namespace Advobot.Attributes.ParameterPreconditions.StringLengthValidation
{
	/// <summary>
	/// Validates the game by making sure it is between 0 and 128 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateGameAttribute : ValidateStringLengthAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateGameAttribute"/>.
		/// </summary>
		public ValidateGameAttribute() : base(0, 128) { }
	}
}
