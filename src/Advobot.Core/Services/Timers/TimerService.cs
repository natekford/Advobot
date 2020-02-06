using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Databases;
using Advobot.Databases.Abstract;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Services.Time;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

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
		private static readonly RequestOptions _Options
			= DiscordUtils.GenerateRequestOptions("Automatically done from the timer service.");

		private readonly ConcurrentDictionary<ulong, byte> _AlreadyDeletedMessages = new ConcurrentDictionary<ulong, byte>();
		private readonly BaseSocketClient _Client;
		private readonly Timer _HourTimer = new Timer(60 * 60 * 1000);
		private readonly Timer _MinuteTimer = new Timer(60 * 1000);
		private readonly AsyncProcessingQueue _RemovableMessages;
		private readonly AsyncProcessingQueue _RemovablePunishments;
		private readonly Timer _SecondTimer = new Timer(1000);
		private readonly ITime _Time;
		private readonly AsyncProcessingQueue _TimedMessages;

		/// <inheritdoc />
		public override string DatabaseName => "TimedDatabase";

		/// <summary>
		/// Creates an instance of <see cref="TimerService"/>.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="dbFactory"></param>
		/// <param name="client"></param>
		public TimerService(
			ITime time,
			IDatabaseWrapperFactory dbFactory,
			BaseSocketClient client)
			: base(dbFactory)
		{
			_Time = time;
			_Client = client;

			_RemovablePunishments = new AsyncProcessingQueue(1, () =>
			{
				var now = _Time.UtcNow;
				var values = DatabaseWrapper.ExecuteQuery(DatabaseQuery<RemovablePunishment>.Delete(x => x.Time < now));
				return ProcessRemovablePunishments(_Client, values);
			});
			_TimedMessages = new AsyncProcessingQueue(1, () =>
			{
				var now = _Time.UtcNow;
				var values = DatabaseWrapper.ExecuteQuery(DatabaseQuery<TimedMessage>.Delete(x => x.Time < now));
				return ProcessTimedMessages(_Client, values);
			});
			_RemovableMessages = new AsyncProcessingQueue(1, () =>
			{
				var now = _Time.UtcNow;
				var values = DatabaseWrapper.ExecuteQuery(DatabaseQuery<RemovableMessage>.Delete(x => x.Time < now));
				return ProcessRemovableMessagesAsync(_Client, _AlreadyDeletedMessages, values);
			});

			_HourTimer.Elapsed += (sender, e) =>
			{
				//Clear this bag every hour because most of these errors only happen a few seconds after input
				//Meaning there's not much of a need for longer term storage of message ids
				_AlreadyDeletedMessages.Clear();
			};
			_MinuteTimer.Elapsed += (sender, e) =>
			{
				_RemovablePunishments.Process();
				_TimedMessages.Process();
			};
			_SecondTimer.Elapsed += (sender, e) => _RemovableMessages.Process();

			_Client.MessageDeleted += (cached, _) =>
			{
				_AlreadyDeletedMessages.TryAdd(cached.Id, 0);
				return Task.CompletedTask;
			};
		}

		/// <inheritdoc />
		public void Add(RemovablePunishment value)
		{
			var deleteQuery = DatabaseQuery<RemovablePunishment>.Delete(
				x => x.UserId == value.UserId && x.GuildId == value.GuildId
					&& x.PunishmentType == value.PunishmentType);
			DatabaseWrapper.ExecuteQuery(deleteQuery);
			var insertQuery = DatabaseQuery<RemovablePunishment>.Insert(new[] { value });
			DatabaseWrapper.ExecuteQuery(insertQuery);
		}

		/// <inheritdoc />
		public void Add(RemovableMessage value)
		{
			var insertQuery = DatabaseQuery<RemovableMessage>.Insert(new[] { value });
			DatabaseWrapper.ExecuteQuery(insertQuery);
		}

		/// <inheritdoc />
		public void Add(TimedMessage value)
		{
			var insertQuery = DatabaseQuery<TimedMessage>.Insert(new[] { value });
			DatabaseWrapper.ExecuteQuery(insertQuery);
		}

		/// <inheritdoc />
		public bool RemovePunishment(ulong guildId, ulong userId, Punishment punishment)
		{
			var deleteQuery = DatabaseQuery<RemovablePunishment>.Delete(
				x => x.UserId == userId && x.GuildId == guildId
					&& x.PunishmentType == punishment);
			var values = DatabaseWrapper.ExecuteQuery(deleteQuery);
			return values.SingleOrDefault() != default;
		}

		/// <inheritdoc />
		protected override void AfterStart(int schema)
		{
			_HourTimer.Start();
			_MinuteTimer.Start();
			_SecondTimer.Start();
			base.AfterStart(schema);
		}

		/// <inheritdoc />
		protected override void BeforeDispose()
		{
			_HourTimer.Dispose();
			_MinuteTimer.Dispose();
			_SecondTimer.Dispose();
			base.BeforeDispose();
		}

		private static async Task ProcessRemovablePunishments(
			BaseSocketClient client,
			IEnumerable<RemovablePunishment> punishments)
		{
			foreach (var group in punishments.Where(x => x != null).GroupBy(x => x.GuildId))
			{
				if (!(client.GetGuild(group.Key) is SocketGuild guild))
				{
					continue;
				}

				var punisher = new PunishmentManager(guild, null);
				foreach (var p in group)
				{
					var args = new PunishmentArgs
					{
						Options = _Options,
						Role = punisher.Guild.GetRole(p.RoleId),
					};
					await punisher.RemoveAsync(p.PunishmentType, p.UserId, args).CAF();
				}
			}
		}

		private static async Task ProcessTimedMessages(
			BaseSocketClient client,
			IEnumerable<TimedMessage> messages)
		{
			foreach (var group in messages.GroupBy(x => x.Id))
			{
				if (!(client.GetUser(group.Key) is SocketUser user))
				{
					continue;
				}

				foreach (var m in group)
				{
					await user.SendMessageAsync(m.Text).CAF();
				}
			}
		}

		private async Task ProcessRemovableMessagesAsync(
			BaseSocketClient client,
			ConcurrentDictionary<ulong, byte> alreadyDeleted,
			IEnumerable<RemovableMessage> messages)
		{
			foreach (var group in messages.Where(x => x != null).GroupBy(x => x.GuildId))
			{
				if (!(client.GetGuild(group.Key) is SocketGuild guild))
				{
					continue;
				}

				foreach (var innerGroup in group.GroupBy(x => x.ChannelId))
				{
					if (!(guild.GetTextChannel(innerGroup.Key) is SocketTextChannel channel))
					{
						continue;
					}

					var temp = new List<IMessage>();
					var ids = innerGroup
						.SelectMany(g => g.MessageIds)
						.Where(x => x != 0 && alreadyDeleted.TryAdd(x, 0));
					foreach (var id in ids)
					{
						temp.Add(await channel.GetMessageAsync(id).CAF());
					}

					var now = _Time.UtcNow;
					await MessageUtils.DeleteMessagesAsync(channel, temp, now, _Options).CAF();
				}
			}
		}
	}
}