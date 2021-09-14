﻿namespace Advobot.Attributes.ParameterPreconditions.Numbers
{
	/// <summary>
	/// Validates the passed in number allowing 1 to <see cref="int.MaxValue"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class PositiveAttribute : RangeParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string NumberType => "positive number";

		/// <summary>
		/// Creates an instance of <see cref="PositiveAttribute"/>.
		/// </summary>
		public PositiveAttribute() : base(1, int.MaxValue) { }
	}
}