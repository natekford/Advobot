using Advobot.Classes;
using Advobot.Classes.Settings;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Services.Logging.Loggers
{
	internal sealed class UserLogger : Logger, IUserLogger
	{
		internal UserLogger(IServiceProvider provider) : base(provider) { }

		/// <summary>
		/// Logs the user joining.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task OnUserJoined(SocketGuildUser user)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), 1);
			NotifyLogCounterIncrement(nameof(ILogService.UserJoins), 1);
			if (!TryGetSettings(user, out var settings)
				|| settings.BannedPhraseNames.Any(x => user.Username.CaseInsContains(x.Phrase))
				|| settings.ServerLogId == 0)
			{
				return;
			}

			var invite = "";
			var inviteUserJoinedOn = await DiscordUtils.GetInviteUserJoinedOnAsync(settings, user).CAF();
			if (inviteUserJoinedOn != null)
			{
				invite = $"**Invite:** {inviteUserJoinedOn.Code}";
			}

			var ageWarning = "";
			var userAccAge = (DateTime.UtcNow - user.CreatedAt.ToUniversalTime());
			if (userAccAge.TotalHours < 24)
			{
				ageWarning = $"**New Account:** {(int)userAccAge.TotalHours} hours, {userAccAge.Minutes} minutes old.";
			}

			var embed = new EmbedWrapper
			{
				Description = $"**ID:** {user.Id}\n{invite}\n{ageWarning}",
				Color = EmbedWrapper.Join
			};
			embed.TryAddAuthor(user, out _);
			embed.TryAddFooter(user.IsBot ? "Bot Joined" : "User Joined", null, out _);
			await MessageUtils.SendMessageAsync(user.Guild.GetTextChannel(settings.ServerLogId), null, embed).CAF();
		}
		/// <summary>
		/// Handles banned names, anti raid, persistent roles, and the welcome message.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task OnUserJoinedActions(SocketGuildUser user)
		{
			if (!(await GuildSettings.GetOrCreateAsync(user.Guild).CAF() is IGuildSettings settings))
			{
				return;
			}
			//Banned names
			if (settings.BannedPhraseNames.Any(x => x.Phrase.CaseInsEquals(user.Username)))
			{
				var giver = new Punisher(TimeSpan.FromMinutes(0), Timers);
				await giver.GiveAsync(Punishment.Ban, user.Guild, user.Id, 0, ClientUtils.CreateRequestOptions("banned name")).CAF();
			}
			//Antiraid
			if (settings.RaidPreventionDictionary[RaidType.Regular] is RaidPreventionInfo antiRaid && antiRaid.Enabled)
			{
				await antiRaid.PunishAsync(settings, user).CAF();
			}
			if (settings.RaidPreventionDictionary[RaidType.RapidJoins] is RaidPreventionInfo antiJoin && antiJoin.Enabled)
			{
				antiJoin.Add(user.JoinedAt?.UtcDateTime ?? default);
				if (antiJoin.GetSpamCount() >= antiJoin.UserCount)
				{
					await antiJoin.PunishAsync(settings, user).CAF();
				}
			}
			//Persistent roles
			var roles = settings.PersistentRoles.Where(x => x.UserId == user.Id)
				.Select(x => user.Guild.GetRole(x.RoleId)).Where(x => x != null).ToList();
			if (roles.Any())
			{
				await user.AddRolesAsync(roles, ClientUtils.CreateRequestOptions("persistent roles")).CAF();
			}
			//Welcome message
			if (settings.WelcomeMessage != null)
			{
				await settings.WelcomeMessage.SendAsync(user.Guild, user).CAF();
			}
		}
		/// <summary>
		/// Logs the user leaving.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task OnUserLeft(SocketGuildUser user)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), -1);
			NotifyLogCounterIncrement(nameof(ILogService.UserLeaves), 1);
			if (!TryGetSettings(user, out var settings)
				|| settings.BannedPhraseNames.Any(x => user.Username.CaseInsContains(x.Phrase))
				|| settings.ServerLogId == 0)
			{
				return;
			}

			var userStayLength = "";
			if (user.JoinedAt.HasValue)
			{
				var t = (DateTime.UtcNow - user.JoinedAt.Value.ToUniversalTime());
				userStayLength = $"**Stayed for:** {t.Days}:{t.Hours:00}:{t.Minutes:00}:{t.Seconds:00}";
			}

			var embed = new EmbedWrapper
			{
				Description = $"**ID:** {user.Id}\n{userStayLength}",
				Color = EmbedWrapper.Leave
			};
			embed.TryAddAuthor(user, out _);
			embed.TryAddFooter(user.IsBot ? "Bot Left" : "User Left", null, out _);
			await MessageUtils.SendMessageAsync(user.Guild.GetTextChannel(settings.ServerLogId), null, embed).CAF();
		}
		/// <summary>
		/// Handles the goodbye message.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task OnUserLeftActions(SocketGuildUser user)
		{
			//Check if the bot was the one that left
			if (user.Id == Config.BotId || !(await GuildSettings.GetOrCreateAsync(user.Guild).CAF() is IGuildSettings settings))
			{
				return;
			}
			//Goodbye message
			if (settings.GoodbyeMessage != null)
			{
				await settings.GoodbyeMessage.SendAsync(user.Guild, user).CAF();
			}
		}
		/// <summary>
		/// Logs their name change to every server that has OnUserUpdated enabled.
		/// </summary>
		/// <param name="beforeUser"></param>
		/// <param name="afterUser"></param>
		/// <returns></returns>
		public async Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser)
		{
			NotifyLogCounterIncrement(nameof(ILogService.UserChanges), 1);
			if (BotSettings.Pause || beforeUser.Username.CaseInsEquals(afterUser.Username))
			{
				return;
			}

			var guildsContainingUser = Client.Guilds.Where(g => g.Users.Select(u => u.Id).Contains(afterUser.Id));
			foreach (var guild in guildsContainingUser)
			{
				if (!TryGetSettings(guild, out var settings) || settings.ServerLogId == 0)
				{
					continue;
				}

				var embed = new EmbedWrapper
				{
					Color = EmbedWrapper.UserEdit
				};
				embed.TryAddAuthor(afterUser, out _);
				embed.TryAddField("Before:", $"`{beforeUser.Username}`", false, out _);
				embed.TryAddField("After:", $"`{afterUser.Username}`", false, out _);
				embed.TryAddFooter("Name Changed", null, out _);
				await MessageUtils.SendMessageAsync(guild.GetTextChannel(settings.ServerLogId), null, embed).CAF();
			}
		}
	}
}
