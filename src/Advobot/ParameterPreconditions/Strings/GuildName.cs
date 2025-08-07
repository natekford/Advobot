namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Validates the guild name by making sure it is between 2 and 100 characters.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class GuildName : StringLengthParameterPrecondition
{
	/// <inheritdoc />
	public override string StringType => "guild name";

	/// <summary>
	/// Creates an instance of <see cref="GuildName"/>.
	/// </summary>
	public GuildName() : base(2, 100) { }
}