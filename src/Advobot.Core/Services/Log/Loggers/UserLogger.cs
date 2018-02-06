using Advobot.Core.Classes;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Services.Log.Loggers
{
	internal sealed class UserLogger : Logger, IUserLogger
	{
		internal UserLogger(ILogService logging, IServiceProvider provider) : base(logging, provider) { }

		/// <summary>
		/// Checks for banned names and raid prevention, logs their join to the server log, or says the welcome message.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task OnUserJoined(SocketGuildUser user)
		{
			Logging.TotalUsers.Increment();
			Logging.UserJoins.Increment();

			if (!TryGetSettings(user, out var settings)
				|| settings.BannedPhraseNames.Any(x => user.Username.CaseInsContains(x.Phrase))
				|| !(settings.ServerLog is ITextChannel serverLog))
			{
				return;
			}

			var invite = "";
			var inviteUserJoinedOn = await InviteUtils.GetInviteUserJoinedOnAsync(settings, user).CAF();
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
			await MessageUtils.SendEmbedMessageAsync(serverLog, embed).CAF();
		}
		/// <summary>
		/// Does nothing if the bot is the user, logs their leave to the server log, or says the goodbye message.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task OnUserLeft(SocketGuildUser user)
		{
			Logging.TotalUsers.Decrement();
			Logging.UserLeaves.Increment();

			if (!TryGetSettings(user, out var settings)
				|| settings.BannedPhraseNames.Any(x => user.Username.CaseInsContains(x.Phrase))
				|| !(settings.ServerLog is ITextChannel serverLog))
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
			await MessageUtils.SendEmbedMessageAsync(serverLog, embed).CAF();
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

			var guilds = await Client.GetGuildsAsync().CAF();
			var guildsContainingUser = guilds.Where(g => ((SocketGuild)g).Users.Select(u => u.Id).Contains(afterUser.Id));
			foreach (var guild in guildsContainingUser)
			{
				Logging.UserChanges.Increment();
				if (!TryGetSettings(guild, out var settings) || !(settings.ServerLog is ITextChannel serverLog))
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
				await MessageUtils.SendEmbedMessageAsync(serverLog, embed).CAF();
			}
		}
	}
}
