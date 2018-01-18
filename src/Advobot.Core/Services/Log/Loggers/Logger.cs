using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Advobot.Core.Services.Log.Loggers
{
	public abstract class Logger
	{
		private static LogAction[] _LogActions = Enum.GetValues(typeof(LogAction)).Cast<LogAction>().ToArray();
		protected ILogService _Logging;
		protected IDiscordClient _Client;
		protected IBotSettings _BotSettings;
		protected IGuildSettingsService _GuildSettings;
		protected ITimersService _Timers;

		public Logger(ILogService logging, IServiceProvider provider)
		{
			_Logging = logging;
			_Client = provider.GetRequiredService<IDiscordClient>();
			_BotSettings = provider.GetRequiredService<IBotSettings>();
			_GuildSettings = provider.GetRequiredService<IGuildSettingsService>();
			_Timers = provider.GetRequiredService<ITimersService>();
		}

		internal protected bool TryGetSettings<T>(T obj, out IGuildSettings settings, [CallerMemberName] string caller = null) where T : ISnowflakeEntity, IEntity<ulong>
		{
			var actionName = caller ?? throw new ArgumentException("Value cannot be null", nameof(caller));
			var actionEnum = _LogActions.First(x => actionName.CaseInsContains(x.EnumName()));
			return TryGetSettings(actionEnum, obj, out settings);
		}
		protected bool TryGetSettings<T>(LogAction logAction, T obj, out IGuildSettings settings) where T : ISnowflakeEntity, IEntity<ulong>
		{
			settings = default;

			var channel = default(IGuildChannel);
			var user = default(IGuildUser);
			var guild = default(IGuild);
			if (obj is IMessage tempMessage)
			{
				user = tempMessage.Author as IGuildUser;
				channel = tempMessage.Channel as IGuildChannel;
				guild = channel.Guild;
			}
			else if (obj is IGuildUser tempUser)
			{
				user = tempUser;
				guild = user.Guild;
			}
			else if (obj is IGuild tempGuild)
			{
				guild = tempGuild;
			}
			else
			{
				return false;
			}
			if (_BotSettings.Pause || !_GuildSettings.TryGetSettings(guild.Id, out settings) || !settings.LogActions.Contains(logAction))
			{
				return false;
			}

			//Only a message will have channel as not null
			if (channel != null)
			{
				var isFromThisBot = user.Id.ToString() == Config.Configuration[ConfigKey.BotId];
				var isFromBot = !isFromThisBot && (user.IsBot || user.IsWebhook);
				var isOnIgnoredChannel = settings.IgnoredLogChannels.Contains(channel.Id);
				switch (logAction)
				{
					case LogAction.MessageReceived:
					case LogAction.MessageUpdated:
					{
						return !isFromThisBot && !isFromBot && !isOnIgnoredChannel;
					}
					case LogAction.MessageDeleted:
					default:
					{
						return !isOnIgnoredChannel;
					}
				}
			}
			//After a message, only a user will have user as not null
			else if (user != null)
			{
				var isFromThisBot = user.Id.ToString() == Config.Configuration[ConfigKey.BotId];
				var isFromBot = !isFromThisBot && (user.IsBot || user.IsWebhook);
				switch (logAction)
				{
					case LogAction.UserJoined:
					case LogAction.UserLeft:
					case LogAction.UserUpdated:
					default:
					{
						return !isFromThisBot;
					}
				}
			}
			//After a message and user, guild is the last thing remaining
			else if (guild != null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
