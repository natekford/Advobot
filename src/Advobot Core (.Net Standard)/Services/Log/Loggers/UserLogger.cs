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
	internal class UserLogger : Logger, IUserLogger
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

			if (VerifyBotLogging(user, out var guildSettings))
			{
				//Bans people who join with a given word in their name
				if (guildSettings.BannedPhraseNames.Any(x => user.Username.CaseInsContains(x.Phrase)))
				{
					var giver = new AutomaticPunishmentGiver(0, _Timers);
					await giver.AutomaticallyPunishAsync(PunishmentType.Ban, user, null, "banned name");
					return;
				}
				else if (VerifyLogAction(guildSettings, LogAction.UserJoined))
				{
					var invite = "";
					var inviteUserJoinedOn = await InviteActions.GetInviteUserJoinedOnAsync(guildSettings, user);
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

					var embed = new MyEmbed(null, $"**ID:** {user.Id}\n{invite}\n{ageWarning}", Colors.JOIN)
						.AddAuthor(user)
						.AddFooter(user.IsBot ? "Bot Joined" : "User Joined");
					await MessageActions.SendEmbedMessageAsync(guildSettings.ServerLog, embed);
				}

				await HandleJoiningUsersForRaidPreventionAsync(guildSettings, user);

				//Welcome message
				if (guildSettings.WelcomeMessage != null)
				{
					await guildSettings.WelcomeMessage.SendAsync(user);
				}
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

			//Check if the bot was the one that left
			if (user.Id.ToString() == Config.Configuration[Config.ConfigKeys.Bot_Id])
			{
				return;
			}

			if (VerifyBotLogging(user, out var guildSettings))
			{
				//Don't log them to the server if they're someone who was just banned for joining with a banned name
				if (guildSettings.BannedPhraseNames.Any(x => user.Username.CaseInsContains(x.Phrase)))
				{
					return;
				}
				else if (VerifyLogAction(guildSettings, LogAction.UserLeft))
				{
					var userStayLength = "";
					if (user.JoinedAt.HasValue)
					{
						var timeStayed = (DateTime.UtcNow - user.JoinedAt.Value.ToUniversalTime());
						userStayLength = $"**Stayed for:** {timeStayed.Days}:{timeStayed.Hours:00}:{timeStayed.Minutes:00}:{timeStayed.Seconds:00}";
					}

					var embed = new MyEmbed(null, $"**ID:** {user.Id}\n{userStayLength}", Colors.LEAV)
						.AddAuthor(user)
						.AddFooter(user.IsBot ? "Bot Left" : "User Left");
					await MessageActions.SendEmbedMessageAsync(guildSettings.ServerLog, embed);
				}

				//Goodbye message
				if (guildSettings.GoodbyeMessage != null)
				{
					await guildSettings.GoodbyeMessage.SendAsync(user);
				}
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
				if (VerifyBotLogging(guild, out var guildSettings) &&
					VerifyLogAction(guildSettings, LogAction.UserUpdated))
				{
					_Logging.UserChanges.Increment();
					var embed = new MyEmbed(null, null, Colors.UEDT)
						.AddAuthor(afterUser)
						.AddField("Before:", "`" + beforeUser.Username + "`")
						.AddField("After:", "`" + afterUser.Username + "`", false)
						.AddFooter("Name Changed");
					await MessageActions.SendEmbedMessageAsync(guildSettings.ServerLog, embed);
				}
			}
		}

		internal async Task HandleJoiningUsersForRaidPreventionAsync(IGuildSettings guildSettings, IGuildUser user)
		{
			var antiRaid = guildSettings.RaidPreventionDictionary[RaidType.Regular];
			if (antiRaid != null && antiRaid.Enabled)
			{
				await antiRaid.PunishAsync(guildSettings, user);
			}
			var antiJoin = guildSettings.RaidPreventionDictionary[RaidType.RapidJoins];
			if (antiJoin != null && antiJoin.Enabled)
			{
				antiJoin.Add(user.JoinedAt.Value.UtcDateTime);
				if (antiJoin.GetSpamCount() < antiJoin.UserCount)
				{
					return;
				}

				await antiJoin.PunishAsync(guildSettings, user);
				if (guildSettings.ServerLog == null)
				{
					return;
				}

				await MessageActions.SendEmbedMessageAsync(guildSettings.ServerLog, new MyEmbed("Anti Rapid Join Mute", $"**User:** {user.FormatUser()}"));
			}
		}
	}
}
