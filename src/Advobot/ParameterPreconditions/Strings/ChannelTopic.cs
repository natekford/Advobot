namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Validates the channel topic by making sure it is between 0 and 1024 characters.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class ChannelTopic : StringRangeParameterPrecondition
{
	/// <inheritdoc />
	public override string StringType => "channel topic";

	/// <summary>
	/// Creates an instance of <see cref="ChannelTopic"/>.
	/// </summary>
	public ChannelTopic() : base(0, 1024) { }
}