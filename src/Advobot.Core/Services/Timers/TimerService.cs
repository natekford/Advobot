using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.CloseWords;
using Advobot.Classes.DatabaseWrappers;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Timer = System.Timers.Timer;

namespace Advobot.Services.Timers
{
	/// <summary>
	/// Handles timed things.
	/// </summary>
	/// <remarks>
	/// I have absolutely no idea if this class works as intended under stress.
	/// </remarks>
	internal sealed class TimerService : DatabaseWrapperConsumer, ITimerService
	{
		/// <inheritdoc />
		public override string DatabaseName => "TimedDatabase";
		private readonly DiscordShardedClient Client;
		private readonly Timer HourTimer = new Timer(60 * 60 * 1000);
		private readonly Timer MinuteTimer = new Timer(60 * 1000);
		private readonly Timer SecondTimer = new Timer(1000);
		private readonly PunishmentArgs PunishmentArgs;
		private readonly AsyncProcessingQueue RemovablePunishments;
		private readonly AsyncProcessingQueue TimedMessages;
		private readonly AsyncProcessingQueue RemovableMessages;
		private readonly AsyncProcessingQueue RemovableCloseWords;
		private readonly ConcurrentDictionary<ulong, byte> _AlreadyDeletedMessages = new ConcurrentDictionary<ulong, byte>();

		/// <summary>
		/// Creates an instance of <see cref="TimerService"/>.
		/// </summary>
		/// <param name="provider"></param>
		public TimerService(IServiceProvider provider) : base(provider)
		{
			Client = provider.GetRequiredService<DiscordShardedClient>();
			PunishmentArgs = new PunishmentArgs
			{
				Timers = this,
				Time = TimeSpan.FromMinutes(0),
				Options = DiscordUtils.GenerateRequestOptions("Automatically done from the timer service."),
			};

			RemovablePunishments = new AsyncProcessingQueue(1, () =>
			{
				var values = DatabaseWrapper.ExecuteQuery(DatabaseQuery<RemovablePunishment>.Delete(x => x.Time < DateTime.UtcNow));
				return ProcessRemovablePunishments(Client, PunishmentArgs, values);
			});
			TimedMessages = new AsyncProcessingQueue(1, () =>
			{
				var values = DatabaseWrapper.ExecuteQuery(DatabaseQuery<TimedMessage>.Delete(x => x.Time < DateTime.UtcNow));
				return ProcessTimedMessages(Client, values);
			});
			RemovableMessages = new AsyncProcessingQueue(1, () =>
			{
				var values = DatabaseWrapper.ExecuteQuery(DatabaseQuery<RemovableMessage>.Delete(x => x.Time < DateTime.UtcNow));
				return ProcessRemovableMessagesAsync(Client, PunishmentArgs, _AlreadyDeletedMessages, values);
			});
			RemovableCloseWords = new AsyncProcessingQueue(1, () =>
			{
				var values = DatabaseWrapper.ExecuteQuery(DatabaseQuery<RemovableCloseWords>.Delete(x => x.Time < DateTime.UtcNow));
				return ProcessRemovableMessagesAsync(Client, PunishmentArgs, _AlreadyDeletedMessages, values);
			});

			HourTimer.Elapsed += (sender, e) =>
			{
				//Clear this bag every hour because most of these errors only happen a few seconds after input
				//Meaning there's not much of a need for longer term storage of message ids
				_AlreadyDeletedMessages.Clear();
			};
			MinuteTimer.Elapsed += (sender, e) =>
			{
				RemovablePunishments.Process();
				TimedMessages.Process();
			};
			SecondTimer.Elapsed += (sender, e) =>
			{
				RemovableMessages.Process();
				RemovableCloseWords.Process();
			};

			Client.MessageDeleted += (cached, channel) =>
			{
				_AlreadyDeletedMessages.TryAdd(cached.Id, 0);
				return Task.CompletedTask;
			};
		}

		/// <inheritdoc />
		public async Task AddAsync(RemovablePunishment value)
		{
			var values = DatabaseWrapper.ExecuteQuery(DatabaseQuery<RemovablePunishment>.Delete(
				x => x.UserId == value.UserId && x.GuildId == value.GuildId && x.PunishmentType == value.PunishmentType));
			await ProcessRemovablePunishments(Client, PunishmentArgs, values);
			DatabaseWrapper.ExecuteQuery(DatabaseQuery<RemovablePunishment>.Insert(new[] { value }));
		}
		/// <inheritdoc />
		public async Task AddAsync(RemovableCloseWords value)
		{
			var values = DatabaseWrapper.ExecuteQuery(DatabaseQuery<RemovableCloseWords>.Delete(x => x.GuildId == value.GuildId && x.UserId == value.UserId));
			await ProcessRemovableMessagesAsync(Client, PunishmentArgs, _AlreadyDeletedMessages, values).CAF();
			DatabaseWrapper.ExecuteQuery(DatabaseQuery<RemovableCloseWords>.Insert(new[] { value }));
		}
		/// <inheritdoc />
		public Task AddAsync(RemovableMessage value)
		{
			DatabaseWrapper.ExecuteQuery(DatabaseQuery<RemovableMessage>.Insert(new[] { value }));
			return Task.FromResult(0);
		}
		/// <inheritdoc />
		public Task AddAsync(TimedMessage value)
		{
			DatabaseWrapper.ExecuteQuery(DatabaseQuery<TimedMessage>.Insert(new[] { value }));
			return Task.FromResult(0);
		}
		/// <inheritdoc />
		public async Task<RemovablePunishment> RemovePunishmentAsync(ulong guildId, ulong userId, Punishment punishment)
		{
			var values = DatabaseWrapper.ExecuteQuery(DatabaseQuery<RemovablePunishment>.Delete(
				x => x.UserId == userId && x.GuildId == guildId && x.PunishmentType == punishment));
			await ProcessRemovablePunishments(Client, PunishmentArgs, values).CAF();
			return values.SingleOrDefault();
		}
		/// <inheritdoc />
		public async Task<RemovableCloseWords> RemoveActiveCloseWords(ulong guildId, ulong userId)
		{
			var values = DatabaseWrapper.ExecuteQuery(DatabaseQuery<RemovableCloseWords>.Delete(x => x.GuildId == guildId && x.UserId == userId));
			await ProcessRemovableMessagesAsync(Client, PunishmentArgs, _AlreadyDeletedMessages, values).CAF();
			return values.SingleOrDefault();
		}
		/// <inheritdoc />
		protected override void AfterStart()
		{
			HourTimer.Start();
			MinuteTimer.Start();
			SecondTimer.Start();
			base.AfterStart();
		}
		/// <inheritdoc />
		protected override void BeforeDispose()
		{
			HourTimer.Dispose();
			MinuteTimer.Dispose();
			SecondTimer.Dispose();
			base.BeforeDispose();
		}

		private static async Task ProcessTimedMessages(BaseSocketClient client, IEnumerable<TimedMessage> timedMessages)
		{
			foreach (var userGroup in timedMessages.GroupBy(x => x.UserId))
			{
				if (!(client.GetUser(userGroup.Key) is SocketUser user))
				{
					continue;
				}
				foreach (var task in userGroup.Select(x => user.SendMessageAsync(x.Text)))
				{
					await task.CAF();
				}
			}
		}
		private static async Task ProcessRemovablePunishments(BaseSocketClient client, PunishmentArgs args, IEnumerable<RemovablePunishment> punishments)
		{
			foreach (var guildGroup in punishments.Where(x => x != null).GroupBy(x => x.GuildId))
			{
				if (!(client.GetGuild(guildGroup.Key) is SocketGuild guild))
				{
					continue;
				}
				foreach (var punishmentGroup in guildGroup.GroupBy(x => x.PunishmentType))
				{
					foreach (var task in punishmentGroup.Select(x => PunishmentUtils.RemoveAsync(x.PunishmentType, guild, x.UserId, x.RoleId, args)))
					{
						await task.CAF();
					}
				}
			}
		}
		private static async Task ProcessRemovableMessagesAsync(BaseSocketClient client, PunishmentArgs args, ConcurrentDictionary<ulong, byte> alreadyDeleted, IEnumerable<RemovableMessage> removableMessages)
		{
			foreach (var guildGroup in removableMessages.Where(x => x != null).GroupBy(x => x.GuildId))
			{
				if (!(client.GetGuild(guildGroup.Key) is SocketGuild guild))
				{
					continue;
				}
				foreach (var channelGroup in guildGroup.GroupBy(x => x.ChannelId))
				{
					if (!(guild.GetTextChannel(channelGroup.Key) is SocketTextChannel channel))
					{
						continue;
					}

					var ids = channelGroup.SelectMany(g => g.MessageIds);
					var messages = new List<IMessage>();
					foreach (var id in ids.Where(x => x != 0 && alreadyDeleted.TryAdd(x, 0)))
					{
						messages.Add(await channel.GetMessageAsync(id).CAF());
					}
					await MessageUtils.DeleteMessagesAsync(channel, messages, args.Options).CAF();
				}
			}
		}
	}
}