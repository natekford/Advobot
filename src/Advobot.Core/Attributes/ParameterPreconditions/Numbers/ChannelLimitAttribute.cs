using System;

namespace Advobot.Attributes.ParameterPreconditions.Numbers
{
	/// <summary>
	/// Validates the channel limit allowing 0 to 99.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ChannelLimitAttribute : RangeParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string NumberType => "channel limit";

		/// <summary>
		/// Creates an instance of <see cref="ChannelLimitAttribute"/>.
		/// </summary>
		public ChannelLimitAttribute() : base(0, 99) { }
	}
}