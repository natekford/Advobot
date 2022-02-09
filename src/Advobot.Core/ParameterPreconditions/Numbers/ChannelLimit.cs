namespace Advobot.ParameterPreconditions.Numbers;

/// <summary>
/// Validates the channel limit allowing 0 to 99.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class ChannelLimit : RangeParameterPrecondition
{
	/// <inheritdoc />
	public override string NumberType => "channel limit";

	/// <summary>
	/// Creates an instance of <see cref="ChannelLimit"/>.
	/// </summary>
	public ChannelLimit() : base(0, 99) { }
}