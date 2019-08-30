using System.Threading.Tasks;

using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using Advobot.Services.Logging.Interfaces;

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
			if (!string.IsNullOrWhiteSpace(message.Message))
			{
				ConsoleUtils.WriteLine(message.Message, name: message.Source);
			}
			return Task.CompletedTask;
		}
	}
}