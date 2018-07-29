using Advobot.Classes;
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
		/// Checks for banned names and raid prevention, logs their join to the server log, or says the welcome message.
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
		/// Does nothing if the bot is the user, logs their leave to the server log, or says the goodbye message.
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
		/// Logs their name change to every server that has OnUserUpdated enabled.
		/// </summary>
		/// <param name="beforeUser"></param>
		/// <param name="afterUser"></param>
		/// <returns></returns>
		public async Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser)
		{
			if (BotSettings.Pause || beforeUser.Username.CaseInsEquals(afterUser.Username))
			{
				return;
			}

			NotifyLogCounterIncrement(nameof(ILogService.UserChanges), 1);

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
