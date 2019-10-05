using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Logging.Caches;
using Advobot.Logging.Context;
using Advobot.Services.GuildSettings.Settings;
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

		private readonly ConcurrentDictionary<ulong, InviteCache> _Invites
			= new ConcurrentDictionary<ulong, InviteCache>();

		private readonly ILogService _Service;
		private readonly ITime _Time;

		public UserLogger(BaseSocketClient client, ILogService service, ITime time)
		{
			_Client = client;
			_Service = service;
			_Time = time;
		}

		public Task OnUserJoined(SocketGuildUser user)
		{
			return _Service.HandleAsync(user, new LoggingArgs<IUserLoggingContext>
			{
				Action = LogAction.UserJoined,
				Actions = new Func<IUserLoggingContext, Task>[]
				{
					HandleJoinLogging,
				},
			});
		}

		public Task OnUserLeft(SocketGuildUser user)
		{
			return _Service.HandleAsync(user, new LoggingArgs<IUserLoggingContext>
			{
				Action = LogAction.UserLeft,
				Actions = new Func<IUserLoggingContext, Task>[]
				{
					HandleLeftLogging,
				},
			});
		}

		public async Task OnUserUpdated(SocketUser before, SocketUser _)
		{
			foreach (var guild in _Client.Guilds)
			{
				if (!(guild.GetUser(before.Id) is IGuildUser user))
				{
					continue;
				}

				await _Service.HandleAsync(user, new LoggingArgs<IUserLoggingContext>
				{
					Action = LogAction.UserUpdated,
					Actions = new Func<IUserLoggingContext, Task>[]
					{
						x => HandleUsernameUpdated(x, before),
					},
				}).CAF();
			}
		}

		private async Task HandleJoinLogging(IUserLoggingContext context)
		{
			var cache = _Invites.GetOrAdd(context.Guild.Id, _ => new InviteCache());
			var inv = await cache.GetInviteUserJoinedOnAsync(context.User).CAF();
			var invite = inv != null
				? $"**Invite:** {inv}"
				: "";
			var time = _Time.UtcNow - context.User.CreatedAt.ToUniversalTime();
			var age = time.TotalHours < 24
				? $"**New Account:** {(int)time.TotalHours} hours, {time.Minutes} minutes old."
				: "";

			await MessageUtils.SendMessageAsync(context.ServerLog!, embed: new EmbedWrapper
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
				var time = _Time.UtcNow - context.User.JoinedAt.Value.ToUniversalTime();
				stay = $"**Stayed for:** {time.Days}:{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";
			}

			return MessageUtils.SendMessageAsync(context.ServerLog!, embed: new EmbedWrapper
			{
				Description = $"**ID:** {context.User.Id}\n{stay}",
				Color = EmbedWrapper.Leave,
				Author = context.User.CreateAuthor(),
				Footer = new EmbedFooterBuilder { Text = context.User.IsBot ? "Bot Left" : "User Left", },
			});
		}

		private Task HandleUsernameUpdated(IUserLoggingContext context, IUser before)
		{
			if (before.Username == context.User.Username)
			{
				return Task.CompletedTask;
			}

			return MessageUtils.SendMessageAsync(context.ServerLog!, embed: new EmbedWrapper
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