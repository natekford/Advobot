namespace Advobot.ParameterPreconditions.Numbers;

/// <summary>
/// Validates the passed in number allowing 1 to <see cref="int.MaxValue"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class Positive : RangeParameterPrecondition
{
	/// <inheritdoc />
	public override string NumberType => "positive number";

	/// <summary>
	/// Creates an instance of <see cref="Positive"/>.
	/// </summary>
	public Positive() : base(1, int.MaxValue) { }
}