using Advobot.ParameterPreconditions.Numbers;

namespace Advobot.Quotes.ParameterPreconditions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public class RemindTime : RangeParameterPrecondition
{
	public override string NumberType => "remind time";

	public RemindTime() : base(1, 525600)
	{
	}
}