using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Punishments;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Services.Log.Loggers
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

			var logInstanceInfo = new LogInstanceInformation(_BotSettings, _GuildSettings, user, LogAction.UserJoined);
			if (!logInstanceInfo.IsValidToLog || logInstanceInfo.GuildSettings.BannedPhraseNames.Any(x => user.Username.CaseInsContains(x.Phrase)))
			{
				return;
			}

			if (logInstanceInfo.HasServerLog)
			{
				var invite = "";
				var inviteUserJoinedOn = await InviteActions.GetInviteUserJoinedOnAsync(logInstanceInfo.GuildSettings, user);
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

				var embed = new AdvobotEmbed(null, $"**ID:** {user.Id}\n{invite}\n{ageWarning}", Colors.JOIN)
					.AddAuthor(user)
					.AddFooter(user.IsBot ? "Bot Joined" : "User Joined");
				await MessageActions.SendEmbedMessageAsync(logInstanceInfo.GuildSettings.ServerLog, embed);
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

			var logInstanceInfo = new LogInstanceInformation(_BotSettings, _GuildSettings, user, LogAction.UserLeft);
			if (!logInstanceInfo.IsValidToLog || logInstanceInfo.GuildSettings.BannedPhraseNames.Any(x => user.Username.CaseInsContains(x.Phrase)))
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

				var embed = new AdvobotEmbed(null, $"**ID:** {user.Id}\n{userStayLength}", Colors.LEAV)
					.AddAuthor(user)
					.AddFooter(user.IsBot ? "Bot Left" : "User Left");
				await MessageActions.SendEmbedMessageAsync(logInstanceInfo.GuildSettings.ServerLog, embed);
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

			foreach (var guild in (await _Client.GetGuildsAsync()).Where(x => (x as SocketGuild).Users.Select(y => y.Id).Contains(afterUser.Id)))
			{
				_Logging.UserChanges.Increment();

				var logInstanceInfo = new LogInstanceInformation(_BotSettings, _GuildSettings, guild, LogAction.UserUpdated);
				if (!logInstanceInfo.IsValidToLog)
				{
					continue;
				}

				if (logInstanceInfo.HasServerLog)
				{
					var embed = new AdvobotEmbed(null, null, Colors.UEDT)
						.AddAuthor(afterUser)
						.AddField("Before:", "`" + beforeUser.Username + "`")
						.AddField("After:", "`" + afterUser.Username + "`", false)
						.AddFooter("Name Changed");
					await MessageActions.SendEmbedMessageAsync(logInstanceInfo.GuildSettings.ServerLog, embed);
				}
			}
		}
	}
}
