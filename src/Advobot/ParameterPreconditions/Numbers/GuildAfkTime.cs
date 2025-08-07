namespace Advobot.ParameterPreconditions.Numbers;

/// <summary>
/// Validates the guild afk timer in seconds allowing specified valid values.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class GuildAfkTime : NumberParameterPrecondition
{
	/// <inheritdoc />
	public override string NumberType => "afk time";

	/// <summary>
	/// Creates an instance of <see cref="GuildAfkTime"/>.
	/// </summary>
	public GuildAfkTime() : base([60, 300, 900, 1800, 3600]) { }
}