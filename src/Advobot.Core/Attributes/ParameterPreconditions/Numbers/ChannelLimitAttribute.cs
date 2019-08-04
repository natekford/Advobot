using System;

namespace Advobot.Attributes.ParameterPreconditions.Numbers
{
	/// <summary>
	/// Validates the channel limit allowing 0 to 99.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class ChannelLimitAttribute : IntParameterPreconditionAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ChannelLimitAttribute"/>.
		/// </summary>
		public ChannelLimitAttribute() : base(0, 99) { }

		/// <inheritdoc />
		public override string ToString()
			=> $"Valid channel limit ({Numbers})";
	}
}