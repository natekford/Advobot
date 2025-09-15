using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;
using Advobot.Modules;
using Advobot.Services;

namespace Advobot.Logging.Resetters;

public sealed class LogActionsResetter(LoggingDatabase db) : IResetter
{
	public static IReadOnlyList<LogAction> All { get; }
		= Enum.GetValues<LogAction>();

	public static IReadOnlyList<LogAction> Default { get; } =
	[
		LogAction.UserJoined,
		LogAction.UserLeft,
		LogAction.MessageReceived,
		LogAction.MessageUpdated,
		LogAction.MessageDeleted
	];

	public async Task ResetAsync(IGuildContext context)
	{
		await db.DeleteLogActionsAsync(context.Guild.Id, All).ConfigureAwait(false);
		await db.AddLogActionsAsync(context.Guild.Id, Default).ConfigureAwait(false);
	}
}