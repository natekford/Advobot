using System;

namespace Advobot.Classes.Attributes.ParameterPreconditions.StringLengthValidation
{
	/// <summary>
	/// Validates the emote name by making sure it is between 2 and 32 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateEmoteNameAttribute : ValidateStringLengthAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateEmoteNameAttribute"/>.
		/// </summary>
		public ValidateEmoteNameAttribute() : base(2, 32) { }
	}
}
