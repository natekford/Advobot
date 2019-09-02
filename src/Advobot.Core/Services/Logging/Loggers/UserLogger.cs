using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Services.Logging.Interfaces;
using Advobot.Services.Time;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.Services.Logging.Loggers
{
	internal sealed class UserLogger : Logger, IUserLogger
	{
		private static readonly RequestOptions _BannedNameOptions = DiscordUtils.GenerateRequestOptions("Banned name.");
		private static readonly RequestOptions _PersistentRolesOptions = DiscordUtils.GenerateRequestOptions("Persistent roles.");
		private readonly BaseSocketClient _Client;

		public UserLogger(
			ITime time,
			IBotSettings botSettings,
			IGuildSettingsFactory settingsFactory,
			BaseSocketClient client)
			: base(time, botSettings, settingsFactory)
		{
			_Client = client;
		}

		public Task OnUserJoined(SocketGuildUser user)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), 1);
			return HandleAsync(user, new LoggingContextArgs<IUserLoggingContext>
			{
				Action = LogAction.UserJoined,
				LogCounterName = nameof(ILogService.UserJoins),
				WhenCanLog = new Func<IUserLoggingContext, Task>[]
				{
					HandleJoinLogging,
				},
				AnyTime = new Func<IUserLoggingContext, Task>[]
				{
					HandleOtherJoinActions,
				},
			});
		}

		public Task OnUserLeft(SocketGuildUser user)
		{
			NotifyLogCounterIncrement(nameof(ILogService.TotalUsers), -1);
			return HandleAsync(user, new LoggingContextArgs<IUserLoggingContext>
			{
				Action = LogAction.UserLeft,
				LogCounterName = nameof(ILogService.UserLeaves),
				WhenCanLog = new Func<IUserLoggingContext, Task>[]
				{
					HandleLeftLogging,
				},
				AnyTime = new Func<IUserLoggingContext, Task>[]
				{
					HandleOtherLeftActions,
				},
			});
		}

		public async Task OnUserUpdated(SocketUser before, SocketUser after)
		{
			foreach (var guild in _Client.Guilds)
			{
				if (!(guild.GetUser(before.Id) is IGuildUser user))
				{
					continue;
				}

				await HandleAsync(user, new LoggingContextArgs<IUserLoggingContext>
				{
					Action = LogAction.UserUpdated,
					LogCounterName = nameof(ILogService.UserChanges),
					WhenCanLog = new Func<IUserLoggingContext, Task>[]
					{
						x => HandleUsernameUpdated(x, before),
					},
					AnyTime = Array.Empty<Func<IUserLoggingContext, Task>>(),
				}).CAF();
			}
		}

		private async Task HandleJoinLogging(IUserLoggingContext context)
		{
			var inv = await context.Settings.GetInviteCache().GetInviteUserJoinedOnAsync(context.User).CAF();
			var invite = inv != null
				? $"**Invite:** {inv}"
				: "";
			var time = Time.UtcNow - context.User.CreatedAt.ToUniversalTime();
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

		private Task HandleLeftLogging(IUserLoggingContext context)
		{
			var stay = "";
			if (context.User.JoinedAt.HasValue)
			{
				var time = Time.UtcNow - context.User.JoinedAt.Value.ToUniversalTime();
				stay = $"**Stayed for:** {time.Days}:{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";
			}

			return ReplyAsync(context.ServerLog, embedWrapper: new EmbedWrapper
			{
				Description = $"**ID:** {context.User.Id}\n{stay}",
				Color = EmbedWrapper.Leave,
				Author = context.User.CreateAuthor(),
				Footer = new EmbedFooterBuilder { Text = context.User.IsBot ? "Bot Left" : "User Left", },
			});
		}

		private async Task HandleOtherJoinActions(IUserLoggingContext context)
		{
			//Banned names
			if (context.Settings.BannedPhraseNames.Any(x => x.Phrase.CaseInsEquals(context.User.Username)))
			{
				var punishmentArgs = new PunishmentArgs()
				{
					Options = _BannedNameOptions,
				};
				await PunishmentUtils.GiveAsync(Punishment.Ban, context.Guild, context.User.Id, 0, punishmentArgs).CAF();
			}
			//Antiraid
			foreach (var antiRaid in context.Settings.RaidPrevention)
			{
				await antiRaid.PunishAsync(context.User).CAF();
			}
			//Persistent roles
			var roles = context.Settings.PersistentRoles
				.Where(x => x.UserId == context.User.Id)
				.Select(x => context.Guild.GetRole(x.RoleId))
				.Where(x => x != null).ToArray();
			if (roles.Length > 0)
			{
				await context.User.AddRolesAsync(roles, _PersistentRolesOptions).CAF();
			}
			//Welcome message
			if (context.Settings.WelcomeMessage != null)
			{
				await context.Settings.WelcomeMessage.SendAsync(context.Guild, context.User).CAF();
			}
		}

		private Task HandleOtherLeftActions(IUserLoggingContext context)
		{
			//Goodbye message
			if (context.Settings.GoodbyeMessage != null)
			{
				return context.Settings.GoodbyeMessage.SendAsync(context.Guild, context.User);
			}
			return Task.CompletedTask;
		}

		private Task HandleUsernameUpdated(IUserLoggingContext context, IUser before)
		{
			if (before.Username == context.User.Username)
			{
				return Task.CompletedTask;
			}

			return ReplyAsync(context.ServerLog, embedWrapper: new EmbedWrapper
			{
				Color = EmbedWrapper.UserEdit,
				Author = before.CreateAuthor(),
				Footer = new EmbedFooterBuilder { Text = "Name Changed" },
				Fields = new List<EmbedFieldBuilder>
				{
					new EmbedFieldBuilder { Name = "Before", Value = $"`{before.Username}`", IsInline = true },
					new EmbedFieldBuilder { Name = "After", Value = $"`{context.User.Username}`", IsInline = true },
				},
			});
		}
	}
}