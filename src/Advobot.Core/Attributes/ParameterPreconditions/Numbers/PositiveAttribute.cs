using System;

namespace Advobot.Attributes.ParameterPreconditions.Numbers
{
	/// <summary>
	/// Validates the passed in number allowing 0 to <see cref="int.MaxValue"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class NotNegativeAttribute : IntParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string NumberType => "not negative number";

		/// <summary>
		/// Creates an instance of <see cref="NotNegativeAttribute"/>.
		/// </summary>
		public NotNegativeAttribute() : base(0, int.MaxValue) { }
	}

	/// <summary>
	/// Validates the passed in number allowing 1 to <see cref="int.MaxValue"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class PositiveAttribute : IntParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string NumberType => "positive number";

		/// <summary>
		/// Creates an instance of <see cref="PositiveAttribute"/>.
		/// </summary>
		public PositiveAttribute() : base(1, int.MaxValue) { }
	}
}