using System;
using System.Threading.Tasks;
using Advobot.Services.Logging.Interfaces;
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

		/// <inheritdoc />
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