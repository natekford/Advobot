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
	public static class EventUtils
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
				var giver = new PunishmentGiver(0, timers);
				await giver.PunishAsync(PunishmentType.Ban, user, null, new ModerationReason("banned name")).CAF();
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
				await RoleUtils.GiveRolesAsync(user, roles, new ModerationReason("persistent roles")).CAF();
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
			if (timers == null || int.TryParse(message.Content, out int i) || i < 0 || i > 7 || !(message.Author is IGuildUser user))
			{
				return;
			}
			--i;

			var quotes = await timers.GetOutActiveCloseQuote(user).CAF();
			var validQuotes = quotes != null && quotes.List.Length > i;
			if (validQuotes)
			{
				var quote = quotes.List[i].Word;
				var embed = new EmbedWrapper
				{
					Title = quote.Name,
					Description = quote.Description,
				};
				embed.TryAddFooter("Quote", null, out var footerErrors);
				await MessageUtils.SendEmbedMessageAsync(message.Channel, embed).CAF();
			}
			var helpEntries = await timers.GetOutActiveCloseHelp(user).CAF();
			var validHelpEntries = helpEntries != null && helpEntries.List.Length > i;
			if (validHelpEntries)
			{
				var help = helpEntries.List[i].Word;
				var embed = new EmbedWrapper
				{
					Title = help.Name,
					Description = help.ToString().Replace(Constants.PLACEHOLDER_PREFIX, settings.GetPrefix(botSettings)),
				};
				embed.TryAddFooter("Help", null, out var footerErrors);
				await MessageUtils.SendEmbedMessageAsync(message.Channel, embed).CAF();
			}

			if (validQuotes || validHelpEntries)
			{
				await MessageUtils.DeleteMessageAsync(message, new ModerationReason("help entry or quote")).CAF();
			}
		}
	}
}