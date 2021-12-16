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

using System.Collections.Concurrent;

namespace Advobot.Logging.Service;

public sealed class UserLogger
{
	private readonly BaseSocketClient _Client;
	private readonly ConcurrentDictionary<ulong, InviteCache> _Invites = new();
	private readonly MessageSenderQueue _MessageQueue;
	private readonly ITime _Time;

	#region Handlers
	private readonly LogHandler<UserState> _UserJoined;
	private readonly LogHandler<UserState> _UserLeft;
	private readonly LogHandler<UserUpdatedState> _UserUpdated;
	#endregion Handlers

	public UserLogger(
		ILoggingDatabase db,
		BaseSocketClient client,
		MessageSenderQueue queue,
		ITime time)
	{
		_Client = client;
		_MessageQueue = queue;
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

	public Task OnUserLeft(SocketGuild guild, SocketUser user)
		=> _UserLeft.HandleAsync(new(guild, user));

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

		var description = $"**ID:** {context.State.User.Id}";

		var cache = _Invites.GetOrAdd(context.Guild.Id, _ => new());
		var invite = await cache.GetInviteUserJoinedOnAsync(context.State.Guild, context.State.User).CAF();
		if (invite is not null)
		{
			description += $"\n**Invite:** {invite}";
		}

		var age = _Time.UtcNow - context.State.User.CreatedAt.ToUniversalTime();
		if (age.TotalHours < 24)
		{
			description += $"\n**New Account:** {age:hh\\:mm\\:ss} old.";
		}

		_MessageQueue.Enqueue((context.ServerLog, new EmbedWrapper
		{
			Description = description,
			Color = EmbedWrapper.Join,
			Author = context.State.User.CreateAuthor(),
			Footer = new()
			{
				Text = context.State.User.IsBot ? "Bot Joined" : "User Joined"
			},
		}.ToMessageArgs()));
	}

	private Task HandleLeftLogging(ILogContext<UserState> context)
	{
		if (context.ServerLog == null)
		{
			return Task.CompletedTask;
		}

		var description = $"**ID:** {context.State.User.Id}";

		if ((context.State.User as IGuildUser)?.JoinedAt is DateTimeOffset joinedAt)
		{
			var length = _Time.UtcNow - joinedAt.ToUniversalTime();
			description += $"\n**Stayed for:** {length:d\\:hh\\:mm\\:ss}";
		}

		_MessageQueue.Enqueue((context.ServerLog, new EmbedWrapper
		{
			Description = description,
			Color = EmbedWrapper.Leave,
			Author = context.State.User.CreateAuthor(),
			Footer = new()
			{
				Text = context.State.User.IsBot ? "Bot Left" : "User Left",
			},
		}.ToMessageArgs()));
		return Task.CompletedTask;
	}

	private Task HandleUsernameUpdated(ILogContext<UserUpdatedState> context)
	{
		if (context.ServerLog == null)
		{
			return Task.CompletedTask;
		}

		_MessageQueue.Enqueue((context.ServerLog, new EmbedWrapper
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
		}.ToMessageArgs()));
		return Task.CompletedTask;
	}
}