using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using System;
using System.Threading.Tasks;

namespace Advobot.Services.Logging.Loggers
{
	internal sealed class BotLogger : Logger, IBotLogger
	{
		internal BotLogger(IServiceProvider provider) : base(provider) { }

		/// <summary>
		/// Creates an instance of <see cref="BotLogger"/>.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public Task OnLogMessageSent(LogMessage message)
		{
			if (!String.IsNullOrWhiteSpace(message.Message))
			{
				ConsoleUtils.WriteLine(message.Message, name: message.Source);
			}
			return Task.CompletedTask;
		}
	}
}