using System;
using System.Threading.Tasks;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;

namespace Advobot.Core.Services.Log.Loggers
{
	internal sealed class BotLogger : Logger, IBotLogger
	{
		internal BotLogger(ILogService logging, IServiceProvider provider) : base(logging, provider) { }

		/// <summary>
		/// Logs system messages from the Discord .Net library.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public Task OnLogMessageSent(LogMessage message)
		{
			if (!String.IsNullOrWhiteSpace(message.Message))
			{
				ConsoleUtils.WriteLine(message.Message, message.Source);
			}
			return Task.CompletedTask;
		}
	}
}
