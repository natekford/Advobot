using Advobot.Enums;
using Advobot.Interfaces;
using Discord;

namespace Advobot.Services.Log
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
		private bool IsFromThisBot;
		private bool IsFromOtherBot;
		private bool IsFromWebhook;
		private bool IsOnIgnoredChannel;
		private bool IsLoggedAction;
		private bool IsPaused;

		public LogInstance(IBotSettings botSettings, IGuildSettingsService guildSettings, IMessage message, LogAction action)
		{
			Action = action;
			Message = message as IUserMessage;
			User = message.Author as IGuildUser;
			Channel = message.Channel as IGuildChannel;
			Guild = User?.Guild;
			HasGuildSettings = guildSettings.TryGetSettings(Guild?.Id ?? 0, out GuildSettings);
			SetBools();
		}
		public LogInstance(IBotSettings botSettings, IGuildSettingsService guildSettings, IGuildUser user, LogAction action)
		{
			Action = action;
			Message = null;
			User = user;
			Channel = null;
			Guild = User?.Guild;
			HasGuildSettings = guildSettings.TryGetSettings(Guild?.Id ?? 0, out GuildSettings);
			SetBools();
		}
		public LogInstance(IBotSettings botSettings, IGuildSettingsService guildSettings, IGuild guild, LogAction action)
		{
			Action = action;
			Message = null;
			User = null;
			Channel = null;
			Guild = guild;
			HasGuildSettings = guildSettings.TryGetSettings(Guild?.Id ?? 0, out GuildSettings);
			SetBools();
		}

		private void SetBools()
		{
			HasServerLog = GuildSettings?.ServerLog != null;
			HasImageLog = GuildSettings?.ImageLog != null;
			IsFromThisBot = (User?.Id ?? 0).ToString() == Config.Configuration[ConfigKeys.BotId];
			IsFromOtherBot = !IsFromThisBot && (User?.IsBot ?? false);
			IsFromWebhook = User?.IsWebhook ?? false;
			IsOnIgnoredChannel = GuildSettings != null && GuildSettings.IgnoredLogChannels.Contains(Channel?.Id ?? 0);
			IsLoggedAction = GuildSettings != null && GuildSettings.LogActions.Contains(Action);
			IsValid = GetIfValid();
		}
		private bool GetIfValid()
		{
			var always = IsLoggedAction && HasGuildSettings;
			switch (Action)
			{
				case LogAction.UserJoined:
				case LogAction.UserLeft:
				case LogAction.UserUpdated:
				{
					return !IsFromThisBot && always;
				}
				case LogAction.MessageReceived:
				case LogAction.MessageUpdated:
				{
					return !IsFromThisBot && !IsFromOtherBot && !IsFromWebhook && !IsOnIgnoredChannel && always;
				}
				case LogAction.MessageDeleted:
				default:
				{
					return !IsOnIgnoredChannel && always;
				}
			}
		}
	}
}
