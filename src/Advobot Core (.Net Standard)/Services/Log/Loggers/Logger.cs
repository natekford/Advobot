using Advobot.Actions;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Advobot.Services.Log.Loggers
{
	public abstract class Logger
	{
		protected ILogService _Logging;
		protected IDiscordClient _Client;
		protected IBotSettings _BotSettings;
		protected IGuildSettingsService _GuildSettings;
		protected ITimersService _Timers;

		public Logger(ILogService logging, IServiceProvider provider)
		{
			_Logging = logging;
			_Client = provider.GetService<IDiscordClient>();
			_BotSettings = provider.GetService<IBotSettings>();
			_GuildSettings = provider.GetService<IGuildSettingsService>();
			_Timers = provider.GetService<ITimersService>();
		}

		/// <summary>
		/// Returns false if the message author is a webhook or a bot or is null.
		/// </summary>
		/// <param name="message">The message to check if the author is a webhook or a bot.</param>
		/// <returns>A boolean stating whether or not the message author is a bot.</returns>
		public bool DisallowBots(IMessage message)
		{
			return message != null && !message.Author.IsBot && !message.Author.IsWebhook;
		}
		/// <summary>
		/// Checks whether or not the guild settings have a log method enabled.
		/// </summary>
		/// <param name="guildSettings">The settings </param>
		/// <param name="callingMethod">The method name to search for.</param>
		/// <returns></returns>
		public bool VerifyLogAction(IGuildSettings guildSettings, LogAction logAction)
		{
			return guildSettings.LogActions.Contains(logAction);
		}
		/// <summary>
		/// Verifies that the bot is not paused, the guild has settings, the channel the message is on should be logged, and the author is not a webhook
		/// or bot which is not the client.
		/// </summary>
		/// <param name="botSettings"></param>
		/// <param name="guildSettingsModule"></param>
		/// <param name="message"></param>
		/// <param name="verifLoggingAction"></param>
		/// <returns></returns>
		public bool VerifyBotLogging(IMessage message, out IGuildSettings guildSettings)
		{
			var allOtherLogRequirements = VerifyBotLogging(message.Channel.GetGuild(), out guildSettings);
			var isNotWebhook = !message.Author.IsWebhook;
			var isNotBot = !message.Author.IsBot || message.Author.Id.ToString() == Config.Configuration[Config.ConfigKeys.Bot_Id];
			var channelShouldBeLogged = !guildSettings.IgnoredLogChannels.Contains(message.Channel.Id);
			return allOtherLogRequirements && isNotWebhook && isNotBot && channelShouldBeLogged;
		}
		/// <summary>
		/// Verifies that the bot is not paused and the guild has settings.
		/// </summary>
		/// <param name="botSettings"></param>
		/// <param name="guildSettingsModule"></param>
		/// <param name="user"></param>
		/// <param name="verifLoggingAction"></param>
		/// <returns></returns>
		public bool VerifyBotLogging(IGuildUser user, out IGuildSettings guildSettings)
		{
			return VerifyBotLogging(user.Guild, out guildSettings);
		}
		/// <summary>
		/// Verifies that the bot is not paused, the guild has settings, and the channel should be logged.
		/// </summary>
		/// <param name="botSettings"></param>
		/// <param name="guildSettingsModule"></param>
		/// <param name="channel"></param>
		/// <param name="verifLoggingAction"></param>
		/// <returns></returns>
		public bool VerifyBotLogging(IChannel channel, out IGuildSettings guildSettings)
		{
			var allOtherLogRequirements = VerifyBotLogging(channel.GetGuild(), out guildSettings);
			var channelShouldBeLogged = !guildSettings.IgnoredLogChannels.Contains(channel.Id);
			return allOtherLogRequirements && channelShouldBeLogged;
		}
		/// <summary>
		/// Verifies that the bot is not paused and the guild has settings.
		/// </summary>
		/// <param name="botSettings"></param>
		/// <param name="guildSettingsModule"></param>
		/// <param name="guild"></param>
		/// <param name="verifLoggingAction"></param>
		/// <returns></returns>
		public bool VerifyBotLogging(IGuild guild, out IGuildSettings guildSettings)
		{
			if (_BotSettings.Pause || !_GuildSettings.TryGetSettings(guild.Id, out guildSettings))
			{
				guildSettings = default;
				return false;
			}
			return true;
		}
	}
}
