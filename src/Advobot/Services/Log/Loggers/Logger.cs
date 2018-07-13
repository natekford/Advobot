using Advobot.Enums;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Advobot.Services.Log.Loggers
{
	/// <summary>
	/// Logs specific things.
	/// </summary>
	public abstract class Logger
	{
		private static LogAction[] _LogActions = Enum.GetValues(typeof(LogAction)).Cast<LogAction>().ToArray();

		/// <summary>
		/// The logging service which acts as a parent to this.
		/// </summary>
		protected ILogService Logging;
		/// <summary>
		/// The bot client.
		/// </summary>
		protected IDiscordClient Client;
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
		protected ITimersService Timers;

		/// <summary>
		/// Creates an instance of logger.
		/// </summary>
		/// <param name="logging"></param>
		/// <param name="provider"></param>
		protected Logger(ILogService logging, IServiceProvider provider)
		{
			Logging = logging;
			Client = provider.GetRequiredService<IDiscordClient>();
			BotSettings = provider.GetRequiredService<IBotSettings>();
			GuildSettings = provider.GetRequiredService<IGuildSettingsService>();
			Timers = provider.GetRequiredService<ITimersService>();
		}

		/// <summary>
		/// Attempts to get guild settings from a random discord object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="settings"></param>
		/// <param name="caller"></param>
		/// <returns></returns>
		protected bool TryGetSettings<T>(T obj, out IGuildSettings settings, [CallerMemberName] string caller = null) where T : ISnowflakeEntity, IEntity<ulong>
		{
			var actionName = caller ?? throw new ArgumentException("Value cannot be null", nameof(caller));
			var actionEnum = _LogActions.First(x => actionName.CaseInsContains(x.ToString()));
			return TryGetSettings(actionEnum, obj, out settings);
		}
		private bool TryGetSettings<T>(LogAction logAction, T obj, out IGuildSettings settings) where T : ISnowflakeEntity, IEntity<ulong>
		{
			settings = default;

			IGuildChannel channel;
			IGuildUser user;
			IGuild guild;
			switch (obj)
			{
				case IMessage tempMessage:
					user = tempMessage.Author as IGuildUser;
					channel = tempMessage.Channel as IGuildChannel;
					guild = channel?.Guild;
					break;
				case IGuildUser tempUser:
					user = tempUser;
					channel = default;
					guild = user.Guild;
					break;
				case IGuild tempGuild:
					user = default;
					channel = default;
					guild = tempGuild;
					break;
				default:
					return false;
			}

			if (BotSettings.Pause || !GuildSettings.TryGet(guild?.Id ?? 0, out settings) || !settings.LogActions.Contains(logAction))
			{
				return false;
			}
			//Only a message will have channel as not null
			if (channel != null && user != null)
			{
				var isFromThisBot = user.Id.ToString() == Config.Configuration[Config.ConfigDict.ConfigKey.BotId];
				var isFromBot = !isFromThisBot && (user.IsBot || user.IsWebhook);
				var isOnIgnoredChannel = settings.IgnoredLogChannels.Contains(channel.Id);
				switch (logAction)
				{
					case LogAction.MessageReceived:
					case LogAction.MessageUpdated:
						return !isFromThisBot && !isFromBot && !isOnIgnoredChannel;
					default:
						return !isOnIgnoredChannel;
				}
			}
			//After a message, only a user will have user as not null
			if (user != null)
			{
				var isFromThisBot = user.Id.ToString() == Config.Configuration[Config.ConfigDict.ConfigKey.BotId];
				return !isFromThisBot && (user.IsBot || user.IsWebhook);
			}
			//After a message and user, guild is the last thing remaining
			return guild != null;
		}
	}
}
