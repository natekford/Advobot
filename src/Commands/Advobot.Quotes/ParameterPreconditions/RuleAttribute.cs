using System;

using Advobot.Attributes.ParameterPreconditions.Strings;

namespace Advobot.Quotes.ParameterPreconditions
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class RuleAttribute : StringRangeParameterPreconditionAttribute
	{
		public override string StringType => "rule";

		public RuleAttribute() : base(1, 500)
		{
		}
	}
}