namespace Advobot.ParameterPreconditions.Numbers;

/// <summary>
/// Validates the amount of days to prune with allowing specified valid values.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class PruneDays : NumberParameterPrecondition
{
	/// <inheritdoc />
	public override string NumberType => "prune days";

	/// <summary>
	/// Creates an instance of <see cref="PruneDays"/>.
	/// </summary>
	public PruneDays() : base([1, 7, 30]) { }
}