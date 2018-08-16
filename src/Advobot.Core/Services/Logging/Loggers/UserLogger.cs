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
			if (CanLog(LogAction.UserJoined, user, out var settings))
			{
				await HandleJoinLogging(settings, user).CAF();
			}
			await HandleOtherJoinActions(settings, user).CAF();
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
			if (CanLog(LogAction.UserLeft, user, out var settings))
			{
				await HandleLeftLogging(settings, user).CAF();
			}
			await HandleOtherLeftActions(settings, user).CAF();
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

			foreach (var guild in Client.Guilds.Where(g => g.Users.Select(u => u.Id).Contains(afterUser.Id)))
			{
				if (!CanLog(LogAction.UserUpdated, guild, out var settings) || settings.ServerLogId == 0)
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

		/// <summary>
		/// Handles logging joins to the server log.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		private async Task HandleJoinLogging(IGuildSettings settings, SocketGuildUser user)
		{
			if (settings.ServerLogId == 0)
			{
				return;
			}

			var invite = await DiscordUtils.GetInviteUserJoinedOnAsync(settings, user).CAF() is CachedInvite inv
				? $"**Invite:** {inv.Code}"
				: "";
			var age = (DateTime.UtcNow - user.CreatedAt.ToUniversalTime()) is TimeSpan time && time.TotalHours < 24
				? $"**New Account:** {(int)time.TotalHours} hours, {time.Minutes} minutes old."
				: "";

			var embed = new EmbedWrapper
			{
				Description = $"**ID:** {user.Id}\n{invite}\n{age}",
				Color = EmbedWrapper.Join
			};
			embed.TryAddAuthor(user, out _);
			embed.TryAddFooter(user.IsBot ? "Bot Joined" : "User Joined", null, out _);
			await MessageUtils.SendMessageAsync(user.Guild.GetTextChannel(settings.ServerLogId), null, embed).CAF();
		}
		/// <summary>
		/// Handles banned names, antiraid, persistent roles, and the welcome message.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		private async Task HandleOtherJoinActions(IGuildSettings settings, SocketGuildUser user)
		{
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
		/// Handles logging leaves to the server log.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		private async Task HandleLeftLogging(IGuildSettings settings, SocketGuildUser user)
		{
			if (settings.ServerLogId == 0)
			{
				return;
			}

			var stay = user.JoinedAt.HasValue && (DateTime.UtcNow - user.JoinedAt.Value.ToUniversalTime()) is TimeSpan time
				? $"**Stayed for:** {time.Days}:{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}"
				: "";

			var embed = new EmbedWrapper
			{
				Description = $"**ID:** {user.Id}\n{stay}",
				Color = EmbedWrapper.Leave
			};
			embed.TryAddAuthor(user, out _);
			embed.TryAddFooter(user.IsBot ? "Bot Left" : "User Left", null, out _);
			await MessageUtils.SendMessageAsync(user.Guild.GetTextChannel(settings.ServerLogId), null, embed).CAF();
		}
		/// <summary>
		/// Handles the goodbye message.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		private async Task HandleOtherLeftActions(IGuildSettings settings, SocketGuildUser user)
		{
			//Goodbye message
			if (settings.GoodbyeMessage != null)
			{
				await settings.GoodbyeMessage.SendAsync(user.Guild, user).CAF();
			}
		}
	}
}
