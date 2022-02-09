namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Validates the role name by making sure it is between 1 and 100 characters.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class RoleName : StringRangeParameterPrecondition
{
	/// <inheritdoc />
	public override string StringType => "role name";

	/// <summary>
	/// Creates an instance of <see cref="RoleName"/>.
	/// </summary>
	public RoleName() : base(1, 100) { }
}