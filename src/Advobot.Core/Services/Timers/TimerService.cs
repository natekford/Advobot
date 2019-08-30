using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Databases;
using Advobot.Databases.Abstract;
using Advobot.Services.GuildSettings.Settings;
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
		private readonly ConcurrentDictionary<ulong, byte> _AlreadyDeletedMessages = new ConcurrentDictionary<ulong, byte>();

		private readonly BaseSocketClient _Client;

		private readonly Timer _HourTimer = new Timer(60 * 60 * 1000);

		private readonly Timer _MinuteTimer = new Timer(60 * 1000);

		private readonly PunishmentArgs _PunishmentArgs;

		private readonly AsyncProcessingQueue _RemovableMessages;

		private readonly AsyncProcessingQueue _RemovablePunishments;

		private readonly Timer _SecondTimer = new Timer(1000);

		private readonly AsyncProcessingQueue _TimedMessages;

		/// <inheritdoc />
		public override string DatabaseName => "TimedDatabase";

		/// <summary>
		/// Creates an instance of <see cref="TimerService"/>.
		/// </summary>
		/// <param name="dbFactory"></param>
		/// <param name="client"></param>
		public TimerService(
			IDatabaseWrapperFactory dbFactory,
			BaseSocketClient client)
			: base(dbFactory)
		{
			_Client = client;
			_PunishmentArgs = new PunishmentArgs
			{
				Options = DiscordUtils.GenerateRequestOptions("Automatically done from the timer service."),
			};

			_RemovablePunishments = new AsyncProcessingQueue(1, () =>
			{
				var values = DatabaseWrapper.ExecuteQuery(DatabaseQuery<RemovablePunishment>.Delete(x => x.Time < DateTime.UtcNow));
				return ProcessRemovablePunishments(_Client, _PunishmentArgs, values);
			});
			_TimedMessages = new AsyncProcessingQueue(1, () =>
			{
				var values = DatabaseWrapper.ExecuteQuery(DatabaseQuery<TimedMessage>.Delete(x => x.Time < DateTime.UtcNow));
				return ProcessTimedMessages(_Client, values);
			});
			_RemovableMessages = new AsyncProcessingQueue(1, () =>
			{
				var values = DatabaseWrapper.ExecuteQuery(DatabaseQuery<RemovableMessage>.Delete(x => x.Time < DateTime.UtcNow));
				return ProcessRemovableMessagesAsync(_Client, _PunishmentArgs, _AlreadyDeletedMessages, values);
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

		private static async Task ProcessTimedMessages(BaseSocketClient client, IEnumerable<TimedMessage> timedMessages)
		{
			foreach (var userGroup in timedMessages.GroupBy(x => x.Id))
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
	}
}