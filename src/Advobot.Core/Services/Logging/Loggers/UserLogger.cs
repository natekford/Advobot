﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Settings;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Services.Logging.Interfaces;
using Advobot.Services.Logging.LoggingContexts;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Services.Logging.Loggers
{
	/// <summary>
	/// Handles logging user events.
	/// </summary>
	internal sealed class UserLogger : Logger, IUserLogger
	{
		private static RequestOptions _PersistentRolesOptions { get; } = DiscordUtils.GenerateRequestOptions("Persistent roles.");
		private static RequestOptions _BannedNameOptions { get; } = DiscordUtils.GenerateRequestOptions("Banned name.");

		/// <summary>
		/// Creates an instance of <see cref="UserLogger"/>.
		/// </summary>
		/// <param name="provider"></param>
		public UserLogger(IServiceProvider provider) : base(provider) { }

		/// <inheritdoc />
		public async Task OnUserJoined(SocketGuildUser user)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), 1);
			var context = new UserLoggingContext(GuildSettings, LogAction.UserJoined, user);
			await HandleAsync(context, nameof(ILogService.UserJoins), new[] { HandleOtherJoinActions(context) }, new Func<Task>[]
			{
				() => HandleJoinLogging(context),
			}).CAF();
		}
		/// <inheritdoc />
		public async Task OnUserLeft(SocketGuildUser user)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), -1);
			var context = new UserLoggingContext(GuildSettings, LogAction.UserJoined, user);
			await HandleAsync(context, nameof(ILogService.UserLeaves), new[] { HandleOtherLeftActions(context) }, new Func<Task>[]
			{
				() => HandleLeftLogging(context),
			}).CAF();
		}
		/// <inheritdoc />
		public async Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser)
		{
			if (beforeUser.Username.CaseInsEquals(afterUser.Username))
			{
				return;
			}

			foreach (var guild in Client.Guilds)
			{
				if (!guild.Users.TryGetFirst(x => x.Id == afterUser.Id, out var user))
				{
					continue;
				}

				var context = new UserLoggingContext(GuildSettings, LogAction.UserUpdated, user);
				await HandleAsync(context, nameof(ILogService.UserChanges), Array.Empty<Task>(), new Func<Task>[]
				{
					() => HandleUsernameUpdated(context, beforeUser),
				}).CAF();
			}
		}

		/// <summary>
		/// Handles logging joins to the server log.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private async Task HandleJoinLogging(UserLoggingContext context)
		{
			if (context.ServerLog == null)
			{
				return;
			}

			var inv = await context.Settings.CachedInvites.GetInviteUserJoinedOnAsync(context.User).CAF();
			var invite = inv != null
				? $"**Invite:** {inv.Code}"
				: "";
			var time = DateTime.UtcNow - context.User.CreatedAt.ToUniversalTime();
			var age = time.TotalHours < 24
				? $"**New Account:** {(int)time.TotalHours} hours, {time.Minutes} minutes old."
				: "";

			await ReplyAsync(context.ServerLog, embedWrapper: new EmbedWrapper
			{
				Description = $"**ID:** {context.User.Id}\n{invite}\n{age}",
				Color = EmbedWrapper.Join,
				Author = context.User.CreateAuthor(),
				Footer = new EmbedFooterBuilder { Text = context.User.IsBot ? "Bot Joined" : "User Joined" },
			}).CAF();
		}
		/// <summary>
		/// Handles banned names, antiraid, persistent roles, and the welcome message.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private async Task HandleOtherJoinActions(UserLoggingContext context)
		{
			//Banned names
			if (context.Settings.BannedPhraseNames.Any(x => x.Phrase.CaseInsEquals(context.User.Username)))
			{
				var giver = new Punisher(TimeSpan.FromMinutes(0), Timers);
				await giver.GiveAsync(Punishment.Ban, context.Guild, context.User.Id, 0, _BannedNameOptions).CAF();
			}
			//Antiraid
			var antiRaid = context.Settings[RaidType.Regular];
			if (antiRaid != null && antiRaid.Enabled)
			{
				await antiRaid.PunishAsync(context.Settings, context.User).CAF();
			}
			var antiJoin = context.Settings[RaidType.RapidJoins];
			if (antiJoin != null && antiJoin.Enabled)
			{
				antiJoin.Add(context.User.JoinedAt?.UtcDateTime ?? default);
				if (antiJoin.GetSpamCount() >= antiJoin.UserCount)
				{
					await antiJoin.PunishAsync(context.Settings, context.User).CAF();
				}
			}
			//Persistent roles
			var roles = context.Settings.PersistentRoles.Where(x => x.UserId == context.User.Id)
				.Select(x => context.Guild.GetRole(x.RoleId)).Where(x => x != null).ToArray();
			if (roles.Length > 0)
			{
				await context.User.AddRolesAsync(roles, _PersistentRolesOptions).CAF();
			}
			//Welcome message
			await context.Settings.WelcomeMessage.SendAsync(context.Guild, context.User).CAF();
		}
		/// <summary>
		/// Handles logging leaves to the server log.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private async Task HandleLeftLogging(UserLoggingContext context)
		{
			if (context.ServerLog == null)
			{
				return;
			}

			var stay = "";
			if (context.User.JoinedAt.HasValue)
			{
				var time = DateTime.UtcNow - context.User.JoinedAt.Value.ToUniversalTime();
				stay = $"**Stayed for:** {time.Days}:{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";
			}

			await ReplyAsync(context.ServerLog, embedWrapper: new EmbedWrapper
			{
				Description = $"**ID:** {context.User.Id}\n{stay}",
				Color = EmbedWrapper.Leave,
				Author = context.User.CreateAuthor(),
				Footer = new EmbedFooterBuilder { Text = context.User.IsBot ? "Bot Left" : "User Left", },
			}).CAF();
		}
		/// <summary>
		/// Handles the goodbye message.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private async Task HandleOtherLeftActions(UserLoggingContext context)
		{
			//Goodbye message
			await context.Settings.GoodbyeMessage.SendAsync(context.Guild, context.User).CAF();
		}
		/// <summary>
		/// Handles logging username changes.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="before"></param>
		/// <returns></returns>
		private async Task HandleUsernameUpdated(UserLoggingContext context, SocketUser before)
		{
			await ReplyAsync(context.ServerLog, embedWrapper: new EmbedWrapper
			{
				Color = EmbedWrapper.UserEdit,
				Author = before.CreateAuthor(),
				Footer = new EmbedFooterBuilder { Text = "Name Changed" },
				Fields = new List<EmbedFieldBuilder>
				{
					new EmbedFieldBuilder { Name = "Before", Value = $"`{before.Username}`", IsInline = true },
					new EmbedFieldBuilder { Name = "After", Value = $"`{context.User.Username}`", IsInline = true },
				},
			}).CAF();
		}
	}
}
