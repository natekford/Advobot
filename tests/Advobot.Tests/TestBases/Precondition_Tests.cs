using Discord.Commands;

namespace Advobot.Tests.TestBases;

public abstract class Precondition_Tests<T> : TestsBase
	where T : PreconditionAttribute
{
	protected abstract T Instance { get; }

	protected Task<PreconditionResult> CheckPermissionsAsync(CommandInfo? command = null)
		=> Instance.CheckPermissionsAsync(Context, command, Services);
}