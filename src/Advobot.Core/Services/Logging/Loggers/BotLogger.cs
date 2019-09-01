using System.Threading.Tasks;

using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using Advobot.Services.Logging.Interfaces;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

namespace Advobot.Services.Logging.Loggers
{
	internal sealed class BotLogger : Logger, IBotLogger
	{
		public BotLogger(IBotSettings botSettings, IGuildSettingsFactory settingsFactory)
			: base(botSettings, settingsFactory) { }

		public Task OnLogMessageSent(LogMessage message)
		{
			message.Write();
			return Task.CompletedTask;
		}
	}
}