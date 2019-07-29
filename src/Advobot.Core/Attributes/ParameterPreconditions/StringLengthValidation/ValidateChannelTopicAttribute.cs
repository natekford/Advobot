using System;

namespace Advobot.Classes.Attributes.ParameterPreconditions.StringLengthValidation
{
	/// <summary>
	/// Validates the channel topic by making sure it is between 0 and 1024 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateChannelTopicAttribute : ValidateStringLengthAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateChannelTopicAttribute"/>.
		/// </summary>
		public ValidateChannelTopicAttribute() : base(0, 1024) { }
	}
}
