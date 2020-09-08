using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Logging.Caches;
using Advobot.Logging.Context;
using Advobot.Logging.Context.Users;
using Advobot.Services.Time;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.Logging.Service
{
	public sealed class UserLogger
	{
		private readonly BaseSocketClient _Client;
		private readonly ConcurrentDictionary<ulong, InviteCache> _Invites =
			new ConcurrentDictionary<ulong, InviteCache>();
		private readonly ITime _Time;

		#region Handlers
		private readonly LoggingHandler<UserState> _UserJoined;
		private readonly LoggingHandler<UserState> _UserLeft;
		private readonly LoggingHandler<UserUpdatedState> _UserUpdated;
		#endregion Handlers

		public UserLogger(ILoggingService logging, BaseSocketClient client, ITime time)
		{
			_Client = client;
			_Time = time;

			_UserJoined = new LoggingHandler<UserState>(
				LogAction.UserJoined, logging)
			{
				Actions = new Func<ILoggingContext<UserState>, Task>[]
				{
					HandleJoinLogging,
				},
			};
			_UserLeft = new LoggingHandler<UserState>(
				LogAction.UserLeft, logging)
			{
				Actions = new Func<ILoggingContext<UserState>, Task>[]
				{
					HandleLeftLogging,
				},
			};
			_UserUpdated = new LoggingHandler<UserUpdatedState>(
				LogAction.UserUpdated, logging)
			{
				Actions = new Func<ILoggingContext<UserUpdatedState>, Task>[]
				{
					HandleUsernameUpdated,
				},
			};
		}

		public Task OnUserJoined(SocketGuildUser user)
			=> _UserJoined.HandleAsync(new UserState(user));

		public Task OnUserLeft(SocketGuildUser user)
			=> _UserLeft.HandleAsync(new UserState(user));

		public async Task OnUserUpdated(SocketUser before, SocketUser after)
		{
			if (before.Username == after.Username)
			{
				return;
			}

			foreach (var guild in _Client.Guilds)
			{
				if (!(guild.GetUser(before.Id) is IGuildUser user))
				{
					continue;
				}

				await _UserUpdated.HandleAsync(new UserUpdatedState(before, user)).CAF();
			}
		}

		private async Task HandleJoinLogging(ILoggingContext<UserState> context)
		{
			if (context.ServerLog == null)
			{
				return;
			}

			var cache = _Invites.GetOrAdd(context.Guild.Id, _ => new InviteCache());
			var inv = await cache.GetInviteUserJoinedOnAsync(context.State.User).CAF();
			var invite = inv != null ? $"**Invite:** {inv}" : "";
			var time = _Time.UtcNow - context.State.User.CreatedAt.ToUniversalTime();
			var age = time.TotalHours < 24
				? $"**New Account:** {(int)time.TotalHours} hours, {time.Minutes} minutes old."
				: "";

			await MessageUtils.SendMessageAsync(context.ServerLog, embed: new EmbedWrapper
			{
				Description = $"**ID:** {context.State.User.Id}\n{invite}\n{age}",
				Color = EmbedWrapper.Join,
				Author = context.State.User.CreateAuthor(),
				Footer = new EmbedFooterBuilder
				{
					Text = context.State.User.IsBot ? "Bot Joined" : "User Joined"
				},
			}).CAF();
		}

		private Task HandleLeftLogging(ILoggingContext<UserState> context)
		{
			if (context.ServerLog == null)
			{
				return Task.CompletedTask;
			}

			var stay = "";
			if (context.State.User.JoinedAt.HasValue)
			{
				var time = _Time.UtcNow - context.State.User.JoinedAt.Value.ToUniversalTime();
				stay = $"**Stayed for:** {time.Days}:{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";
			}

			return MessageUtils.SendMessageAsync(context.ServerLog, embed: new EmbedWrapper
			{
				Description = $"**ID:** {context.State.User.Id}\n{stay}",
				Color = EmbedWrapper.Leave,
				Author = context.State.User.CreateAuthor(),
				Footer = new EmbedFooterBuilder
				{
					Text = context.State.User.IsBot ? "Bot Left" : "User Left",
				},
			});
		}

		private Task HandleUsernameUpdated(ILoggingContext<UserUpdatedState> context)
		{
			if (context.ServerLog == null)
			{
				return Task.CompletedTask;
			}

			return MessageUtils.SendMessageAsync(context.ServerLog, embed: new EmbedWrapper
			{
				Color = EmbedWrapper.UserEdit,
				Author = context.State.User.CreateAuthor(),
				Footer = new EmbedFooterBuilder { Text = "Name Changed" },
				Fields = new List<EmbedFieldBuilder>
				{
					new EmbedFieldBuilder
					{
						Name = "Before",
						Value = $"`{context.State.Before.Username}`",
						IsInline = true
					},
					new EmbedFieldBuilder
					{
						Name = "After",
						Value = $"`{context.State.User.Username}`",
						IsInline = true
					},
				},
			});
		}
	}
}