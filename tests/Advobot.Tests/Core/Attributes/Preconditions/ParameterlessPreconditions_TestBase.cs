using Discord.Commands;

namespace Advobot.Tests.Core.Attributes.Preconditions
{
	public abstract class ParameterlessPreconditions_TestBase<T>
		: Preconditions_TestBase<T>
		where T : PreconditionAttribute, new()
	{
		public override T Instance { get; } = new T();
	}
}