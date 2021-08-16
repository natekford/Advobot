using System.Collections.Concurrent;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Logging.Context;
using Advobot.Logging.Context.Users;
using Advobot.Logging.Database;
using Advobot.Logging.Utilities;
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
			new();
		private readonly ITime _Time;

		#region Handlers
		private readonly LogHandler<UserState> _UserJoined;
		private readonly LogHandler<UserState> _UserLeft;
		private readonly LogHandler<UserUpdatedState> _UserUpdated;
		#endregion Handlers

		public UserLogger(ILoggingDatabase db, BaseSocketClient client, ITime time)
		{
			_Client = client;
			_Time = time;

			_UserJoined = new(LogAction.UserJoined, db)
			{
				HandleJoinLogging,
			};
			_UserLeft = new(LogAction.UserLeft, db)
			{
				HandleLeftLogging,
			};
			_UserUpdated = new(LogAction.UserUpdated, db)
			{
				HandleUsernameUpdated,
			};
		}

		public Task OnUserJoined(SocketGuildUser user)
			=> _UserJoined.HandleAsync(new(user));

		public Task OnUserLeft(SocketGuildUser user)
			=> _UserLeft.HandleAsync(new(user));

		public async Task OnUserUpdated(SocketUser before, SocketUser after)
		{
			if (before.Username == after.Username)
			{
				return;
			}

			foreach (var guild in _Client.Guilds)
			{
				if (guild.GetUser(before.Id) is not IGuildUser user)
				{
					continue;
				}

				await _UserUpdated.HandleAsync(new(before, user)).CAF();
			}
		}

		private async Task HandleJoinLogging(ILogContext<UserState> context)
		{
			if (context.ServerLog == null)
			{
				return;
			}

			var cache = _Invites.GetOrAdd(context.Guild.Id, _ => new());
			var inv = await cache.GetInviteUserJoinedOnAsync(context.State.User).CAF();
			var invite = inv != null ? $"**Invite:** {inv}" : "";
			var time = _Time.UtcNow - context.State.User.CreatedAt.ToUniversalTime();
			var age = time.TotalHours < 24
				? $"**New Account:** {(int)time.TotalHours} hours, {time.Minutes} minutes old."
				: "";

			await context.ServerLog.SendMessageAsync(new EmbedWrapper
			{
				Description = $"**ID:** {context.State.User.Id}\n{invite}\n{age}",
				Color = EmbedWrapper.Join,
				Author = context.State.User.CreateAuthor(),
				Footer = new()
				{
					Text = context.State.User.IsBot ? "Bot Joined" : "User Joined"
				},
			}.ToMessageArgs()).CAF();
		}

		private Task HandleLeftLogging(ILogContext<UserState> context)
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

			return context.ServerLog.SendMessageAsync(new EmbedWrapper
			{
				Description = $"**ID:** {context.State.User.Id}\n{stay}",
				Color = EmbedWrapper.Leave,
				Author = context.State.User.CreateAuthor(),
				Footer = new()
				{
					Text = context.State.User.IsBot ? "Bot Left" : "User Left",
				},
			}.ToMessageArgs());
		}

		private Task HandleUsernameUpdated(ILogContext<UserUpdatedState> context)
		{
			if (context.ServerLog == null)
			{
				return Task.CompletedTask;
			}

			return context.ServerLog.SendMessageAsync(new EmbedWrapper
			{
				Color = EmbedWrapper.UserEdit,
				Author = context.State.User.CreateAuthor(),
				Footer = new() { Text = "Name Changed" },
				Fields = new()
				{
					new()
					{
						Name = "Before",
						Value = $"`{context.State.Before.Username}`",
						IsInline = true
					},
					new()
					{
						Name = "After",
						Value = $"`{context.State.User.Username}`",
						IsInline = true
					},
				},
			}.ToMessageArgs());
		}
	}
}