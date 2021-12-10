﻿namespace Advobot.Attributes.ParameterPreconditions.Strings;

/// <summary>
/// Validates the username by making sure it is between 2 and 32 characters.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class UsernameAttribute : StringRangeParameterPreconditionAttribute
{
	/// <inheritdoc />
	public override string StringType => "username";

	/// <summary>
	/// Creates an instance of <see cref="UsernameAttribute"/>.
	/// </summary>
	public UsernameAttribute() : base(2, 32) { }
}