using Discord.Commands;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions
{
	public abstract class ParameterlessParameterPreconditions_TestsBase<T>
		: ParameterPreconditions_TestsBase<T>
		where T : ParameterPreconditionAttribute, new()
	{
		public override T Instance { get; } = new T();
	}
}