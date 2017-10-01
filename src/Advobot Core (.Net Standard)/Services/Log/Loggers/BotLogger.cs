using Advobot.Actions;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Advobot.Services.Log.Loggers
{
	internal class BotLogger : Logger, IBotLogger
	{
		internal BotLogger(ILogService logging, IServiceProvider provider) : base(logging, provider) { }

		protected override void HookUpEvents()
		{
			if (_Client is DiscordSocketClient socketClient)
			{
				socketClient.Log += OnLogMessageSent;
			}
			else if (_Client is DiscordShardedClient shardedClient)
			{
				shardedClient.Log += OnLogMessageSent;
			}
			else
			{
				throw new ArgumentException($"Invalid client provided. Must be either a {nameof(DiscordSocketClient)} or a {nameof(DiscordShardedClient)}.");
			}
		}

		/// <summary>
		/// Logs system messages from the Discord .Net library.
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		internal Task OnLogMessageSent(LogMessage msg)
		{
			if (!String.IsNullOrWhiteSpace(msg.Message))
			{
				ConsoleActions.WriteLine(msg.Message, msg.Source);
			}
			return Task.CompletedTask;
		}
	}
}
