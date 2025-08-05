using Advobot.Logging.Database;
using Advobot.Services;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Logging.Resetters;

public sealed class LogActionsResetter(ILoggingDatabase db) : IResetter
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

	public async Task ResetAsync(ICommandContext context)
	{
		await db.DeleteLogActionsAsync(context.Guild.Id, All).CAF();
		await db.AddLogActionsAsync(context.Guild.Id, Default).CAF();
	}
}