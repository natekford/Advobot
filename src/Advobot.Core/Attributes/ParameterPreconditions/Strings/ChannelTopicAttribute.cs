using System;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Validates the channel topic by making sure it is between 0 and 1024 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ChannelTopicAttribute : StringParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string StringType => "channel topic";

		/// <summary>
		/// Creates an instance of <see cref="ChannelTopicAttribute"/>.
		/// </summary>
		public ChannelTopicAttribute() : base(0, 1024) { }
	}
}