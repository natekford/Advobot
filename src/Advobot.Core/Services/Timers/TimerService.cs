using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.CloseWords;
using Advobot.Classes.DatabaseWrappers;
using Advobot.Enums;
using Advobot.Interfaces;
using AdvorangesUtils;
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
		/// <summary>
		/// The discord client for the bot.
		/// </summary>
		private DiscordShardedClient Client { get; }
		/// <summary>
		/// A timer which ticks once an hour. This will not tick every x:00, instead every 3600 seconds after the bot is start.
		/// </summary>
		private Timer HourTimer { get; } = new Timer(60 * 60 * 1000);
		/// <summary>
		/// A timer which ticks once a minute. This will not tick every x:xx, instead every 60 seconds after the bot is start.
		/// </summary>
		private Timer MinuteTimer { get; } = new Timer(60 * 1000);
		/// <summary>
		/// A timer which ticks once a second. This will not tick every x:xx:xx, instead every 1 second after the bot is start.
		/// </summary>
		private Timer SecondTimer { get; } = new Timer(1000);
		/// <summary>
		/// Used for giving and removing punishments.
		/// </summary>
		private Punisher Punisher { get; }
		/// <summary>
		/// Queue responsible for processing punishments.
		/// </summary>
		private ProcessingQueue RemovablePunishments { get; }
		/// <summary>
		/// Queue responsible for processing punishments.
		/// </summary>
		private ProcessingQueue TimedMessages { get; }
		/// <summary>
		/// Queue responsible for processing removable messages..
		/// </summary>
		private ProcessingQueue RemovableMessages { get; }
		/// <summary>
		/// Queue responsible for processing close help entries.
		/// </summary>
		private ProcessingQueue CloseHelpEntries { get; }
		/// <summary>
		/// Queue responsible for processing close quotes..
		/// </summary>
		private ProcessingQueue CloseQuotes { get; }
		/// <summary>
		/// Cached message ids which have already been deleted so there are less exceptions given when deleting messages.
		/// </summary>
		private ConcurrentBag<ulong> AlreadyDeletedMessages
		{
			get => _AlreadyDeletedMessages;
			set => _AlreadyDeletedMessages = value;
		}
		private ConcurrentBag<ulong> _AlreadyDeletedMessages = new ConcurrentBag<ulong>();

		/// <summary>
		/// Creates an instance of <see cref="TimerService"/>.
		/// </summary>
		/// <param name="provider"></param>
		public TimerService(IServiceProvider provider) : base(provider)
		{
			Client = provider.GetRequiredService<DiscordShardedClient>();
			Punisher = new Punisher(TimeSpan.FromMinutes(0), this);

			RemovablePunishments = new ProcessingQueue(1, async () =>
			{
				var values = DatabaseWrapper.ExecuteQuery(DBQuery<RemovablePunishment>.Delete(x => x.Time < DateTime.UtcNow));
				await RemovablePunishment.ProcessRemovablePunishments(Client, Punisher, values);
			});
			TimedMessages = new ProcessingQueue(1, async () =>
			{
				var values = DatabaseWrapper.ExecuteQuery(DBQuery<TimedMessage>.Delete(x => x.Time < DateTime.UtcNow));
				await TimedMessage.ProcessTimedMessages(Client, values).CAF();
			});
			RemovableMessages = new ProcessingQueue(1, async () =>
			{
				var values = DatabaseWrapper.ExecuteQuery(DBQuery<RemovableMessage>.Delete(x => x.Time < DateTime.UtcNow));
				await RemovableMessage.ProcessRemovableMessages(Client, AlreadyDeletedMessages, values).CAF();
			});
			CloseHelpEntries = new ProcessingQueue(1, async () =>
			{
				var values = DatabaseWrapper.ExecuteQuery(DBQuery<CloseHelpEntries>.Delete(x => x.Time < DateTime.UtcNow));
				await RemovableMessage.ProcessRemovableMessages(Client, AlreadyDeletedMessages, values).CAF();
			});
			CloseQuotes = new ProcessingQueue(1, async () =>
			{
				var values = DatabaseWrapper.ExecuteQuery(DBQuery<CloseQuotes>.Delete(x => x.Time < DateTime.UtcNow));
				await RemovableMessage.ProcessRemovableMessages(Client, AlreadyDeletedMessages, values).CAF();
			});

			HourTimer.Elapsed += (sender, e) =>
			{
				//Clear this bag every hour because most of these errors only happen a few seconds after input
				//Meaning there's not much of a need for longer term storage of message ids
				Interlocked.Exchange(ref _AlreadyDeletedMessages, new ConcurrentBag<ulong>());
			};
			MinuteTimer.Elapsed += (sender, e) =>
			{
				RemovablePunishments.Process();
				TimedMessages.Process();
			};
			SecondTimer.Elapsed += (sender, e) =>
			{
				RemovableMessages.Process();
				CloseHelpEntries.Process();
				CloseQuotes.Process();
			};

			Client.MessageDeleted += (cached, channel) =>
			{
				AlreadyDeletedMessages.Add(cached.Id);
				return Task.CompletedTask;
			};
		}

		/// <inheritdoc />
		public async Task AddAsync(RemovablePunishment value)
		{
			var values = DatabaseWrapper.ExecuteQuery(DBQuery<RemovablePunishment>.Delete(
				x => x.UserId == value.UserId && x.GuildId == value.GuildId && x.PunishmentType == value.PunishmentType));
			await RemovablePunishment.ProcessRemovablePunishments(Client, Punisher, values);
			DatabaseWrapper.ExecuteQuery(DBQuery<RemovablePunishment>.Insert(new[] { value }));
		}
		/// <inheritdoc />
		public async Task AddAsync(CloseHelpEntries value)
		{
			var values = DatabaseWrapper.ExecuteQuery(DBQuery<CloseHelpEntries>.Delete(x => x.GuildId == value.GuildId && x.UserId == value.UserId));
			await RemovableMessage.ProcessRemovableMessages(Client, AlreadyDeletedMessages, values).CAF();
			DatabaseWrapper.ExecuteQuery(DBQuery<CloseHelpEntries>.Insert(new[] { value }));
		}
		/// <inheritdoc />
		public async Task AddAsync(CloseQuotes value)
		{
			var values = DatabaseWrapper.ExecuteQuery(DBQuery<CloseQuotes>.Delete(x => x.GuildId == value.GuildId && x.UserId == value.UserId));
			await RemovableMessage.ProcessRemovableMessages(Client, AlreadyDeletedMessages, values).CAF();
			DatabaseWrapper.ExecuteQuery(DBQuery<CloseQuotes>.Insert(new[] { value }));
		}
		/// <inheritdoc />
		public Task AddAsync(RemovableMessage value)
		{
			DatabaseWrapper.ExecuteQuery(DBQuery<RemovableMessage>.Insert(new[] { value }));
			return Task.FromResult(0);
		}
		/// <inheritdoc />
		public Task AddAsync(TimedMessage value)
		{
			DatabaseWrapper.ExecuteQuery(DBQuery<TimedMessage>.Insert(new[] { value }));
			return Task.FromResult(0);
		}
		/// <inheritdoc />
		public async Task<RemovablePunishment> RemovePunishmentAsync(ulong guildId, ulong userId, Punishment punishment)
		{
			var values = DatabaseWrapper.ExecuteQuery(DBQuery<RemovablePunishment>.Delete(
				x => x.UserId == userId && x.GuildId == guildId && x.PunishmentType == punishment));
			await RemovablePunishment.ProcessRemovablePunishments(Client, Punisher, values).CAF();
			return values.SingleOrDefault();
		}
		/// <inheritdoc />
		public async Task<CloseHelpEntries> RemoveActiveCloseHelpAsync(ulong guildId, ulong userId)
		{
			var values = DatabaseWrapper.ExecuteQuery(DBQuery<CloseHelpEntries>.Delete(x => x.GuildId == guildId && x.UserId == userId));
			await RemovableMessage.ProcessRemovableMessages(Client, AlreadyDeletedMessages, values).CAF();
			return values.SingleOrDefault();
		}
		/// <inheritdoc />
		public async Task<CloseQuotes> RemoveActiveCloseQuoteAsync(ulong guildId, ulong userId)
		{
			var values = DatabaseWrapper.ExecuteQuery(DBQuery<CloseQuotes>.Delete(x => x.GuildId == guildId && x.UserId == userId));
			await RemovableMessage.ProcessRemovableMessages(Client, AlreadyDeletedMessages, values).CAF();
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
	}
}