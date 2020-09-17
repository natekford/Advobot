using System;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Validates the channel name by making sure it is between 2 and 100 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class ChannelNameAttribute : StringRangeParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string StringType => "channel name";

		/// <summary>
		/// Creates an instance of <see cref="ChannelNameAttribute"/>.
		/// </summary>
		public ChannelNameAttribute() : base(2, 100) { }
	}
}