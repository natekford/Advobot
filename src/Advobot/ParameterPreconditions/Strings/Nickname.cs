namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Validates the nickname by making sure it is between 1 and 32 characters.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class Nickname : StringLengthParameterPrecondition
{
	/// <inheritdoc />
	public override string StringType => "nickname";

	/// <summary>
	/// Creates an instance of <see cref="Nickname"/>.
	/// </summary>
	public Nickname() : base(1, 32) { }
}