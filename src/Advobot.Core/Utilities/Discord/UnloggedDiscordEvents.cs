using Advobot.Core.Classes;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Events which are in this separate class to make the classes that use them smaller.
	/// </summary>
	public static class UnloggedDiscordEvents
	{
		/// <summary>
		/// Checks if this is the first instance of the bot starting, updates the game, says some start up messages.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="botSettings"></param>
		/// <returns></returns>
		public static async Task OnConnected(IDiscordClient client, IBotSettings botSettings)
		{
			if (Config.Configuration[ConfigKey.BotId] != client.CurrentUser.Id.ToString())
			{
				Config.Configuration[ConfigKey.BotId] = client.CurrentUser.Id.ToString();
				Config.Save();
				ConsoleUtils.WriteLine("The bot needs to be restarted in order for the config to be loaded correctly.");
				ClientUtils.RestartBot();
			}

			await ClientUtils.UpdateGameAsync(client, botSettings).CAF();

			var startTime = DateTime.UtcNow.Subtract(Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalMilliseconds;
			ConsoleUtils.WriteLine($"Current version: {Version.VersionNumber}");
			ConsoleUtils.WriteLine($"Current bot prefix is: {botSettings.Prefix}");
			ConsoleUtils.WriteLine($"Bot took {startTime:n} milliseconds to start up.");
		}
		/// <summary>
		/// Checks banned names, antiraid, persistent roles, and welcome messages.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="settings"></param>
		/// <param name="timers"></param>
		/// <returns></returns>
		public static async Task OnUserJoined(SocketGuildUser user, IBotSettings botSettings, IGuildSettings settings, ITimersService timers)
		{
			if (settings == null)
			{
				return;
			}

			//Banned names
			if (settings.BannedPhraseNames.Any(x => x.Phrase.CaseInsEquals(user.Username)))
			{
				var giver = new AutomaticPunishmentGiver(0, timers);
				await giver.AutomaticallyPunishAsync(PunishmentType.Ban, user, null, "banned name").CAF();
			}

			//Antiraid
			var antiRaid = settings.RaidPreventionDictionary[RaidType.Regular];
			if (antiRaid != null && antiRaid.Enabled)
			{
				await antiRaid.PunishAsync(settings, user).CAF();
			}
			var antiJoin = settings.RaidPreventionDictionary[RaidType.RapidJoins];
			if (antiJoin != null && antiJoin.Enabled)
			{
				antiJoin.Add(user.JoinedAt.Value.UtcDateTime);
				if (antiJoin.GetSpamCount() >= antiJoin.UserCount)
				{
					await antiJoin.PunishAsync(settings, user).CAF();
				}
			}

			//Persistent roles
			var roles = settings.PersistentRoles.Where(x => x.UserId == user.Id).Select(x => x.GetRole(user.Guild)).Where(x => x != null);
			if (roles.Any())
			{
				await RoleUtils.GiveRolesAsync(user, roles, new AutomaticModerationReason("persistent roles")).CAF();
			}

			//Welcome message
			if (settings.WelcomeMessage != null)
			{
				await settings.WelcomeMessage.SendAsync(user).CAF();
			}
		}
		/// <summary>
		/// Checks goodbye messages.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="settings"></param>
		/// <param name="timers"></param>
		/// <returns></returns>
		public static async Task OnUserLeft(SocketGuildUser user, IBotSettings botSettings, IGuildSettings settings, ITimersService timers)
		{
			//Check if the bot was the one that left
			if (settings == null || user.Id.ToString() == Config.Configuration[ConfigKey.BotId])
			{
				return;
			}

			//Goodbye message
			if (settings.GoodbyeMessage != null)
			{
				await settings.GoodbyeMessage.SendAsync(user).CAF();
			}
		}
		/// <summary>
		/// Checks close helpentries and quotes.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="timers"></param>
		/// <returns></returns>
		public static async Task OnMessageReceived(SocketMessage message, IBotSettings botSettings, IGuildSettings settings, ITimersService timers)
		{
			if (timers != null && int.TryParse(message.Content, out int number) && number > 0 && number < 7)
			{
				--number;

				var quotes = await timers.GetOutActiveCloseQuote(message.Author).CAF();
				var validQuote = quotes != null && quotes.List.Count > number;
				var helpEntries = await timers.GetOutActiveCloseHelp(message.Author).CAF();
				var validHelpEntry = helpEntries != null && helpEntries.List.Count > number;

				if (validQuote)
				{
					await MessageUtils.SendMessageAsync(message.Channel, quotes.List[number].Word.Description).CAF();
				}
				if (validHelpEntry)
				{
					var help = helpEntries.List[number].Word;
					var prefix = settings.GetPrefix(botSettings);
					var desc = help.ToString().Replace(Constants.PLACEHOLDER_PREFIX, prefix);
					var embed = new EmbedWrapper(help.Name, desc)
						.AddFooter("Help");
					await MessageUtils.SendEmbedMessageAsync(message.Channel, embed).CAF();
				}

				if (validQuote || validHelpEntry)
				{
					await MessageUtils.DeleteMessageAsync(message, new AutomaticModerationReason("help entry or quote")).CAF();
				}
			}
		}
	}
}