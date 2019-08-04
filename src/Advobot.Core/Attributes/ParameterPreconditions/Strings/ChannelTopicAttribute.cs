using System;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Validates the channel topic by making sure it is between 0 and 1024 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ChannelTopicAttribute : StringParameterPreconditionAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ChannelTopicAttribute"/>.
		/// </summary>
		public ChannelTopicAttribute() : base(1, 1024) { }

		/// <inheritdoc />
		public override string ToString()
			=> $"Valid channel topic ({ValidLength} long)";
	}
}
