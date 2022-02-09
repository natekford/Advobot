using Advobot.ParameterPreconditions.Strings;

namespace Advobot.Quotes.ParameterPreconditions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class Rule : StringRangeParameterPrecondition
{
	public override string StringType => "rule";

	public Rule() : base(1, 500)
	{
	}
}