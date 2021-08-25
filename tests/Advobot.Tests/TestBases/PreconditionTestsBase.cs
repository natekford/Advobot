
using Discord.Commands;

namespace Advobot.Tests.TestBases
{
	public abstract class PreconditionTestsBase : TestsBase
	{
		protected abstract PreconditionAttribute Instance { get; }

		protected Task<PreconditionResult> CheckPermissionsAsync(CommandInfo? command = null)
			=> Instance.CheckPermissionsAsync(Context, command, Services);
	}
}