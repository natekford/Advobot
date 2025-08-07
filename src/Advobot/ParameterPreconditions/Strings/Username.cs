namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Validates the username by making sure it is between 2 and 32 characters.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class Username : StringLengthParameterPrecondition
{
	/// <inheritdoc />
	public override string StringType => "username";

	/// <summary>
	/// Creates an instance of <see cref="Username"/>.
	/// </summary>
	public Username() : base(2, 32) { }
}