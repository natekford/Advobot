﻿namespace Advobot.ParameterPreconditions.Numbers;

/// <summary>
/// Validates the passed in number allowing 0 to <see cref="int.MaxValue"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotNegative : RangeParameterPrecondition
{
	/// <inheritdoc />
	public override string NumberType => "not negative number";

	/// <summary>
	/// Creates an instance of <see cref="NotNegative"/>.
	/// </summary>
	public NotNegative() : base(0, int.MaxValue) { }
}