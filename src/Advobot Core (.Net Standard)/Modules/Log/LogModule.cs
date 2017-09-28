using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Advobot.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Modules.Log
{
	/// <summary>
	/// Logs certain events.
	/// </summary>
	/// <remarks>
	/// This is probably the second worst part of the bot, right behind the UI. Slightly ahead of saving settings though.
	/// </remarks>
	internal sealed class Logging : ILogModule
	{
		private LogCounter[] _LoggedCommands;
		private LogCounter[] _LoggedUserActions;
		private LogCounter[] _LoggedMessageActions;
		private LogCounter[] _LoggedAttachments;

		public List<LoggedCommand> RanCommands	{ get; private set; } = new List<LoggedCommand>();
		public LogCounter TotalUsers			{ get; private set; } = new LogCounter();
		public LogCounter TotalGuilds			{ get; private set; } = new LogCounter();
		public LogCounter AttemptedCommands		{ get; private set; } = new LogCounter();
		public LogCounter SuccessfulCommands	{ get; private set; } = new LogCounter();
		public LogCounter FailedCommands		{ get; private set; } = new LogCounter();
		public LogCounter UserJoins				{ get; private set; } = new LogCounter();
		public LogCounter UserLeaves			{ get; private set; } = new LogCounter();
		public LogCounter UserChanges			{ get; private set; } = new LogCounter();
		public LogCounter MessageEdits			{ get; private set; } = new LogCounter();
		public LogCounter MessageDeletes		{ get; private set; } = new LogCounter();
		public LogCounter Messages				{ get; private set; } = new LogCounter();
		public LogCounter Images				{ get; private set; } = new LogCounter();
		public LogCounter Gifs					{ get; private set; } = new LogCounter();
		public LogCounter Files					{ get; private set; } = new LogCounter();

		public IBotLogger BotLogger				{ get; private set; }
		public IGuildLogger GuildLogger			{ get; private set; }
		public IUserLogger UserLogger			{ get; private set; }
		public IMessageLogger MessageLogger		{ get; private set; }

		public Logging(IServiceProvider provider)
		{
			_LoggedCommands			= new[] { AttemptedCommands, SuccessfulCommands, FailedCommands };
			_LoggedUserActions		= new[] { UserJoins, UserLeaves, UserChanges };
			_LoggedMessageActions	= new[] { MessageEdits, MessageDeletes };
			_LoggedAttachments		= new[] { Images, Gifs, Files };

			BotLogger				= new BotLogger(this, provider);
			GuildLogger				= new GuildLogger(this, provider);
			UserLogger				= new UserLogger(this, provider);
			MessageLogger			= new MessageLogger(this, provider);
		}

		public string FormatLoggedCommands()
		{
			return LogCounter.FormatMultiple(true, _LoggedCommands);
		}
		public string FormatLoggedActions()
		{
			return LogCounter.FormatMultiple(true, _LoggedUserActions) +
				LogCounter.FormatMultiple(true, _LoggedMessageActions) +
				LogCounter.FormatMultiple(true, _LoggedAttachments);
		}

		/// <summary>
		/// Returns false if the message author is a webhook or a bot.
		/// </summary>
		/// <param name="message">The message to check if the author is a webhook or a bot.</param>
		/// <returns>A boolean stating whether or not the message author is a bot.</returns>
		public static bool DisallowBots(IMessage message)
		{
			return !message.Author.IsBot && !message.Author.IsWebhook;
		}
		/// <summary>
		/// Checks whether or not the guild settings have a log method enabled.
		/// </summary>
		/// <param name="guildSettings">The settings </param>
		/// <param name="callingMethod">The method name to search for.</param>
		/// <returns></returns>
		public static bool VerifyLogAction(IGuildSettings guildSettings, LogAction logAction)
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
		public static bool VerifyBotLogging(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, IMessage message, out IGuildSettings guildSettings)
		{
			var allOtherLogRequirements = VerifyBotLogging(botSettings, guildSettingsModule, message.Channel.GetGuild(), out guildSettings);
			var isNotWebhook = !message.Author.IsWebhook;
			var isNotBot = !message.Author.IsBot || message.Author.Id.ToString() == Config.Configuration[ConfigKeys.Bot_Id];
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
		public static bool VerifyBotLogging(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, IGuildUser user, out IGuildSettings guildSettings)
		{
			return VerifyBotLogging(botSettings, guildSettingsModule, user.Guild, out guildSettings);
		}
		/// <summary>
		/// Verifies that the bot is not paused, the guild has settings, and the channel should be logged.
		/// </summary>
		/// <param name="botSettings"></param>
		/// <param name="guildSettingsModule"></param>
		/// <param name="channel"></param>
		/// <param name="verifLoggingAction"></param>
		/// <returns></returns>
		public static bool VerifyBotLogging(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, IChannel channel, out IGuildSettings guildSettings)
		{
			var allOtherLogRequirements = VerifyBotLogging(botSettings, guildSettingsModule, channel.GetGuild(), out guildSettings);
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
		public static bool VerifyBotLogging(IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, IGuild guild, out IGuildSettings guildSettings)
		{
			if (botSettings.Pause || !guildSettingsModule.TryGetSettings(guild.Id, out guildSettings))
			{
				guildSettings = default(IGuildSettings);
				return false;
			}
			return true;
		}
	}
}