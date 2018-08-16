using System;
using System.Threading.Tasks;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;

namespace Advobot.Services.Logging.Loggers
{
	/// <summary>
	/// Handles logging bot events.
	/// </summary>
	internal sealed class BotLogger : Logger, IBotLogger
	{
		/// <summary>
		/// Creates an instance of <see cref="BotLogger"/>.
		/// </summary>
		/// <param name="provider"></param>
		public BotLogger(IServiceProvider provider) : base(provider) { }

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