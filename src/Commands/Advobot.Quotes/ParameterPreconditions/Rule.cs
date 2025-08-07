using Advobot.ParameterPreconditions.Strings;

namespace Advobot.Quotes.ParameterPreconditions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class Rule : StringLengthParameterPrecondition
{
	public override string StringType => "rule";

	public Rule() : base(1, 500)
	{
	}
}