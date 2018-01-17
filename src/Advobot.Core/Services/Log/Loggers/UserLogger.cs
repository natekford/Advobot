using Advobot.Core.Utilities;
using Advobot.Core.Classes;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
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
			_Logging.TotalUsers.Increment();
			_Logging.UserJoins.Increment();

			var logInstanceInfo = new LogInstance(_BotSettings, _GuildSettings, user, LogAction.UserJoined);
			if (!logInstanceInfo.IsValid || logInstanceInfo.GuildSettings.BannedPhraseNames.Any(x => user.Username.CaseInsContains(x.Phrase)))
			{
				return;
			}

			if (logInstanceInfo.HasServerLog)
			{
				var invite = "";
				var inviteUserJoinedOn = await InviteUtils.GetInviteUserJoinedOnAsync(logInstanceInfo.GuildSettings, user).CAF();
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
					Color = Constants.JOIN,
				};
				embed.TryAddAuthor(user, out var authorErrors);
				embed.TryAddFooter(user.IsBot ? "Bot Joined" : "User Joined", null, out var footerErrors);
				await MessageUtils.SendEmbedMessageAsync(logInstanceInfo.GuildSettings.ServerLog, embed).CAF();
			}
		}
		/// <summary>
		/// Does nothing if the bot is the user, logs their leave to the server log, or says the goodbye message.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task OnUserLeft(SocketGuildUser user)
		{
			_Logging.TotalUsers.Decrement();
			_Logging.UserLeaves.Increment();

			var logInstanceInfo = new LogInstance(_BotSettings, _GuildSettings, user, LogAction.UserLeft);
			if (!logInstanceInfo.IsValid || logInstanceInfo.GuildSettings.BannedPhraseNames.Any(x => user.Username.CaseInsContains(x.Phrase)))
			{
				return;
			}

			if (logInstanceInfo.HasServerLog)
			{
				var userStayLength = "";
				if (user.JoinedAt.HasValue)
				{
					var t = (DateTime.UtcNow - user.JoinedAt.Value.ToUniversalTime());
					userStayLength = $"**Stayed for:** {t.Days}:{t.Hours:00}:{t.Minutes:00}:{t.Seconds:00}";
				}

				var embed = new EmbedWrapper
				{
					Description = $"**ID:** {user.Id}\n{userStayLength}",
					Color = Constants.LEAV,
				};
				embed.TryAddAuthor(user, out var authorErrors);
				embed.TryAddFooter(user.IsBot ? "Bot Left" : "User Left", null, out var footerErrors);
				await MessageUtils.SendEmbedMessageAsync(logInstanceInfo.GuildSettings.ServerLog, embed).CAF();
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
			if (_BotSettings.Pause || beforeUser.Username.CaseInsEquals(afterUser.Username))
			{
				return;
			}

			var guilds = await _Client.GetGuildsAsync().CAF();
			var guildsContainingUser = guilds.Where(x => (x as SocketGuild).Users.Select(y => y.Id).Contains(afterUser.Id));
			foreach (var guild in guildsContainingUser)
			{
				_Logging.UserChanges.Increment();

				var logInstanceInfo = new LogInstance(_BotSettings, _GuildSettings, guild, LogAction.UserUpdated);
				if (!logInstanceInfo.IsValid)
				{
					continue;
				}

				if (logInstanceInfo.HasServerLog)
				{
					var embed = new EmbedWrapper
					{
						Color = Constants.UEDT,
					};
					embed.TryAddAuthor(afterUser, out var authorErrors);
					embed.TryAddField("Before:", $"`{beforeUser.Username}`", false, out var firstFieldErrors);
					embed.TryAddField("After:", $"`{afterUser.Username}`", false, out var secondFieldErrors);
					embed.TryAddFooter("Name Changed", null, out var footerErrors);
					await MessageUtils.SendEmbedMessageAsync(logInstanceInfo.GuildSettings.ServerLog, embed).CAF();
				}
			}
		}
	}
}
