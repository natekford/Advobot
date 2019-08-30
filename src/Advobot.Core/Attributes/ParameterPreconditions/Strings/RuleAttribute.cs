using System;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Validates the rule by making sure it is between 1 and 150 characters and that it does not already exist.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class RuleAttribute : StringParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string StringType => "rule";

		/// <summary>
		/// Creates an instance of <see cref="RuleAttribute"/>.
		/// </summary>
		public RuleAttribute() : base(1, 150) { }
	}
}