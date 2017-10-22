using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;

namespace Advobot.Core.Services.Log
{
	internal sealed class LogInstance
	{
		public readonly IUserMessage Message;
		public readonly IGuildUser User;
		public readonly IGuildChannel Channel;
		public readonly IGuild Guild;
		public readonly IGuildSettings GuildSettings;
		public readonly LogAction Action;

		public bool IsValid { get; private set; }
		public bool HasGuildSettings { get; private set; }
		public bool HasServerLog { get; private set; }
		public bool HasImageLog { get; private set; }
		private bool _IsFromThisBot;
		private bool _IsFromOtherBot;
		private bool _IsFromWebhook;
		private bool _IsOnIgnoredChannel;
		private bool _IsLoggedAction;
		private bool _IsPaused;

		public LogInstance(IBotSettings botSettings, IGuildSettingsService guildSettings, IMessage message, LogAction action)
		{
			Action = action;
			Message = message as IUserMessage;
			User = message.Author as IGuildUser;
			Channel = message.Channel as IGuildChannel;
			Guild = User?.Guild;
			HasGuildSettings = guildSettings.TryGetSettings(Guild?.Id ?? 0, out GuildSettings);
			SetBools(botSettings);
		}
		public LogInstance(IBotSettings botSettings, IGuildSettingsService guildSettings, IGuildUser user, LogAction action)
		{
			Action = action;
			Message = null;
			User = user;
			Channel = null;
			Guild = User?.Guild;
			HasGuildSettings = guildSettings.TryGetSettings(Guild?.Id ?? 0, out GuildSettings);
			SetBools(botSettings);
		}
		public LogInstance(IBotSettings botSettings, IGuildSettingsService guildSettings, IGuild guild, LogAction action)
		{
			Action = action;
			Message = null;
			User = null;
			Channel = null;
			Guild = guild;
			HasGuildSettings = guildSettings.TryGetSettings(Guild?.Id ?? 0, out GuildSettings);
			SetBools(botSettings);
		}

		private void SetBools(IBotSettings botSettings)
		{
			HasServerLog = GuildSettings?.ServerLog != null;
			HasImageLog = GuildSettings?.ImageLog != null;
			_IsFromThisBot = (User?.Id ?? 0).ToString() == Config.Configuration[ConfigKey.BotId];
			_IsFromOtherBot = !_IsFromThisBot && (User?.IsBot ?? false);
			_IsFromWebhook = User?.IsWebhook ?? false;
			_IsOnIgnoredChannel = GuildSettings != null && GuildSettings.IgnoredLogChannels.Contains(Channel?.Id ?? 0);
			_IsLoggedAction = GuildSettings != null && GuildSettings.LogActions.Contains(Action);
			_IsPaused = botSettings.Pause;
			IsValid = GetIfValid();
		}
		private bool GetIfValid()
		{
			var always = !_IsPaused && _IsLoggedAction && HasGuildSettings;
			switch (Action)
			{
				case LogAction.UserJoined:
				case LogAction.UserLeft:
				case LogAction.UserUpdated:
				{
					return !_IsFromThisBot && always;
				}
				case LogAction.MessageReceived:
				case LogAction.MessageUpdated:
				{
					return !_IsFromThisBot && !_IsFromOtherBot && !_IsFromWebhook && !_IsOnIgnoredChannel && always;
				}
				case LogAction.MessageDeleted:
				default:
				{
					return !_IsOnIgnoredChannel && always;
				}
			}
		}
	}
}
