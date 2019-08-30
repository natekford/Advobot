using System;

namespace Advobot.Attributes.ParameterPreconditions.Numbers
{
	/// <summary>
	/// Validates the amount of days to prune with allowing specified valid values.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class PruneDaysAttribute : IntParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string NumberType => "prune days";

		/// <summary>
		/// Creates an instance of <see cref="PruneDaysAttribute"/>.
		/// </summary>
		public PruneDaysAttribute() : base(new[] { 1, 7, 30 }) { }
	}
}