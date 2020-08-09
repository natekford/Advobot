using Discord.Commands;

namespace Advobot.Tests.TestBases
{
	public abstract class ParameterlessPreconditions_TestBase<T>
		: Preconditions_TestBase<T>
		where T : PreconditionAttribute, new()
	{
		protected override T Instance { get; } = new T();
	}
}