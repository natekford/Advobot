using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Services.Logging.Loggers
{
	/// <summary>
	/// Logs specific things.
	/// </summary>
	public abstract class Logger : ILogger
	{
		/// <summary>
		/// The bot client.
		/// </summary>
		protected DiscordShardedClient Client;
		/// <summary>
		/// The settings used in the bot.
		/// </summary>
		protected IBotSettings BotSettings;
		/// <summary>
		/// The settings used in guilds.
		/// </summary>
		protected IGuildSettingsService GuildSettings;
		/// <summary>
		/// Timers for punishments.
		/// </summary>
		protected ITimerService Timers;

		/// <inheritdoc />
		public event LogCounterIncrementEventHandler LogCounterIncrement;

		/// <summary>
		/// Creates an instance of logger.
		/// </summary>
		/// <param name="provider"></param>
		protected Logger(IServiceProvider provider)
		{
			Client = provider.GetRequiredService<DiscordShardedClient>();
			BotSettings = provider.GetRequiredService<IBotSettings>();
			GuildSettings = provider.GetRequiredService<IGuildSettingsService>();
			Timers = provider.GetRequiredService<ITimerService>();
		}

		/// <summary>
		/// Fires the log counter increment event.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="count"></param>
		protected void NotifyLogCounterIncrement(string name, int count)
		{
			LogCounterIncrement?.Invoke(this, new LogCounterIncrementEventArgs(name, count));
		}
		/// <summary>
		/// Attempts to get guild settings from a random Discord object.
		/// Returns false if unable to be logged due to settings on the guild.
		/// Returns true if able to be logged.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="action"></param>
		/// <param name="obj"></param>
		/// <param name="settings">This can be set if the method returns false.</param>
		/// <returns></returns>
		protected bool CanLog<T>(LogAction action, T obj, out IGuildSettings settings) where T : ISnowflakeEntity
		{
			settings = default;
			var channel = default(SocketGuildChannel);
			var user = default(SocketGuildUser);
			var guild = default(SocketGuild);
			switch (obj)
			{
				case SocketMessage tempM:
					user = (SocketGuildUser)tempM.Author;
					channel = (SocketGuildChannel)tempM.Channel;
					guild = channel.Guild;
					break;
				case SocketGuildUser tempU:
					user = tempU;
					guild = user.Guild;
					break;
				case SocketGuild tempG:
					guild = tempG;
					break;
				default:
					return false;
			}
			if (BotSettings.Pause || !GuildSettings.TryGet(guild.Id, out settings) || !settings.LogActions.Contains(action))
			{
				return false;
			}

			switch (action)
			{
				//Only log message updates and do actions on received messages if they're not a bot and not on an unlogged channel
				case LogAction.MessageReceived:
				case LogAction.MessageUpdated:
					return !(user.IsBot || user.IsWebhook) && !settings.IgnoredLogChannels.Contains(channel.Id);
				//Log all deleted messages, no matter the source user, unless they're on an unlogged channel
				case LogAction.MessageDeleted:
					return !settings.IgnoredLogChannels.Contains(channel.Id);
				//Only log if it wasn't this bot that left
				case LogAction.UserJoined:
				case LogAction.UserLeft:
					return user.Id != guild.CurrentUser.Id;
				//Only log if it wasn't any bot that was updated.
				case LogAction.UserUpdated:
					return !(user.IsBot || user.IsWebhook);
				default:
					throw new InvalidOperationException($"Invalid log action supplied: {action}.");
			}
		}
	}
}
