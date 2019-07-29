using System;

namespace Advobot.Classes.Attributes.ParameterPreconditions.StringLengthValidation
{
	/// <summary>
	/// Validates the channel name by making sure it is between 2 and 100 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class ValidateChannelNameAttribute : ValidateStringLengthAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateChannelNameAttribute"/>.
		/// </summary>
		public ValidateChannelNameAttribute() : base(2, 100) { }
	}
}
