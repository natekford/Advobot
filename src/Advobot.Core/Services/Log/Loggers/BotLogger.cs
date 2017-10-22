using Advobot.Core.Actions;
using Advobot.Core.Interfaces;
using Discord;
using System;
using System.Threading.Tasks;

namespace Advobot.Core.Services.Log.Loggers
{
	internal sealed class BotLogger : Logger, IBotLogger
	{
		internal BotLogger(ILogService logging, IServiceProvider provider) : base(logging, provider) { }

		/// <summary>
		/// Logs system messages from the Discord .Net library.
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		public Task OnLogMessageSent(LogMessage message)
		{
			if (!String.IsNullOrWhiteSpace(message.Message))
			{
				ConsoleActions.WriteLine(message.Message, message.Source);
			}
			return Task.CompletedTask;
		}
	}
}
