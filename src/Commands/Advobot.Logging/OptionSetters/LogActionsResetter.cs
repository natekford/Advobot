using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Logging.Service;
using Advobot.Services;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Logging.OptionSetters
{
	public sealed class LogActionsResetter : IResetter
	{
		private readonly ILoggingService _Logging;

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

		public LogActionsResetter(ILoggingService logging)
		{
			_Logging = logging;
		}

		public async Task ResetAsync(ICommandContext context)
		{
			await _Logging.RemoveLogActionsAsync(context.Guild.Id, All).CAF();
			await _Logging.AddLogActionsAsync(context.Guild.Id, Default).CAF();
		}
	}
}