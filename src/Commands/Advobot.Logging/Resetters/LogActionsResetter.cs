using Advobot.Logging.Database;
using Advobot.Services;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Logging.OptionSetters;

public sealed class LogActionsResetter(ILoggingDatabase db) : IResetter
{
	private readonly ILoggingDatabase _Db = db;

	public static IReadOnlyList<LogAction> All { get; }
		= AdvobotUtils.GetValues<LogAction>();

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
		await _Db.DeleteLogActionsAsync(context.Guild.Id, All).CAF();
		await _Db.AddLogActionsAsync(context.Guild.Id, Default).CAF();
	}
}