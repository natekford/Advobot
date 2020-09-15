using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Logging.Database;
using Advobot.Services;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Logging.OptionSetters
{
	public sealed class LogActionsResetter : IResetter
	{
		private readonly ILoggingDatabase _Db;

		public static IReadOnlyList<LogAction> All { get; }
			= AdvobotUtils.GetValues<LogAction>();

		public static IReadOnlyList<LogAction> Default { get; } = new[]
		{
			LogAction.UserJoined,
			LogAction.UserLeft,
			LogAction.MessageReceived,
			LogAction.MessageUpdated,
			LogAction.MessageDeleted
		};

		public LogActionsResetter(ILoggingDatabase db)
		{
			_Db = db;
		}

		public async Task ResetAsync(ICommandContext context)
		{
			await _Db.DeleteLogActionsAsync(context.Guild.Id, All).CAF();
			await _Db.AddLogActionsAsync(context.Guild.Id, Default).CAF();
		}
	}
}