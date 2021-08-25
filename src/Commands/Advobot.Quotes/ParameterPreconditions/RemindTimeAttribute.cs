
using Advobot.Attributes.ParameterPreconditions.Numbers;

namespace Advobot.Quotes.ParameterPreconditions
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class RemindTimeAttribute : RangeParameterPreconditionAttribute
	{
		public override string NumberType => "remind time";

		public RemindTimeAttribute() : base(1, 525600)
		{
		}
	}
}