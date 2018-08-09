using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.CloseWords;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Timer = System.Timers.Timer;

namespace Advobot.Services.Timers
{
	/// <summary>
	/// Handles time based punishments and message removal
	/// </summary>
	/// <remarks>
	/// I have absolutely no idea if this class works as intended under stress.
	/// </remarks>
	internal sealed class TimerService : ITimerService, IUsesDatabase, IDisposable
	{
		private LiteDatabase _Db;
		private readonly DiscordShardedClient _Client;
		private readonly ILowLevelConfig _Config;
		private readonly Timer _MinuteTimer = new Timer(60 * 1000);
		private readonly Timer _SecondTimer = new Timer(1000);
		private readonly Punisher _PunishmentRemover;
		private readonly RequestOptions _PunishmentReason = ClientUtils.CreateRequestOptions("automatic punishment removal.");
		private readonly RequestOptions _MessageReason = ClientUtils.CreateRequestOptions("automatic message deletion.");
		private readonly ProcessingQueue _RemovablePunishments;
		private readonly ProcessingQueue _TimedMessages;
		private readonly ProcessingQueue _RemovableMessages;
		private readonly ProcessingQueue _CloseHelpEntries;
		private readonly ProcessingQueue _CloseQuotes;

		public TimerService(IIterableServiceProvider provider)
		{
			_Client = provider.GetRequiredService<DiscordShardedClient>();
			_Config = provider.GetRequiredService<ILowLevelConfig>();
			_PunishmentRemover = new Punisher(TimeSpan.FromMinutes(0), this);

			_RemovablePunishments = new ProcessingQueue(1, async () =>
			{
				var col = _Db.GetCollection<RemovablePunishment>();
				foreach (var punishment in col.Find(x => x.Time < DateTime.UtcNow))
				{
					col.Delete(punishment.Id);
					await punishment.RemoveAsync(_Client, _PunishmentRemover, _PunishmentReason).CAF();
				}
			});
			_TimedMessages = new ProcessingQueue(1, async () =>
			{
				var col = _Db.GetCollection<TimedMessage>();
				foreach (var timedMessage in col.Find(x => x.Time < DateTime.UtcNow))
				{
					col.Delete(timedMessage.Id);
					if (!(_Client.GetUser(timedMessage.UserId) is SocketUser user))
					{
						continue;
					}

					await user.SendMessageAsync(timedMessage.Text).CAF();
				}
			});
			_RemovableMessages = new ProcessingQueue(1, async () =>
			{
				await RemoveRemovableMessages(_Db.GetCollection<RemovableMessage>()).CAF();
			});
			_CloseHelpEntries = new ProcessingQueue(1, async () =>
			{
				await RemoveRemovableMessages(_Db.GetCollection<CloseHelpEntries>()).CAF();
			});
			_CloseQuotes = new ProcessingQueue(1, async () =>
			{
				await RemoveRemovableMessages(_Db.GetCollection<CloseQuotes>()).CAF();
			});

			_MinuteTimer.Elapsed += (sender, e) =>
			{
				_RemovablePunishments.Process();
				_TimedMessages.Process();
			};
			_SecondTimer.Elapsed += (sender, e) =>
			{
				_RemovableMessages.Process();
				_CloseHelpEntries.Process();
				_CloseQuotes.Process();
			};
		}

		/// <inheritdoc />
		public void Start()
		{
			//Use mode=exclusive to not have ioexceptions
			_Db = new LiteDatabase(new ConnectionString
			{
				Filename = _Config.GetBaseBotDirectoryFile("TimedDatabase.db").FullName,
				Mode = FileMode.Exclusive,
			});
			ConsoleUtils.DebugWrite($"Started the database connection for {nameof(TimerService)}.");
			_MinuteTimer.Enabled = true;
			_SecondTimer.Enabled = true;
		}
		/// <inheritdoc />
		public void Dispose()
		{
			_MinuteTimer.Stop();
			_SecondTimer.Stop();
			_Db.Dispose();
		}
		/// <inheritdoc />
		public async Task AddAsync(RemovablePunishment punishment)
		{
			var col = _Db.GetCollection<RemovablePunishment>();
			var entry = col.FindOne(x => x.UserId == punishment.UserId && x.GuildId == punishment.GuildId && x.PunishmentType == punishment.PunishmentType);
			if (entry != null)
			{
				col.Delete(entry.Id);
				await entry.RemoveAsync(_Client, _PunishmentRemover, _PunishmentReason).CAF();
			}
			col.Insert(punishment);
		}
		/// <inheritdoc />
		public async Task AddAsync(CloseHelpEntries helpEntries)
		{
			var col = _Db.GetCollection<CloseHelpEntries>();
			await HandleRemovableMessage(col, helpEntries.GuildId, helpEntries.UserId).CAF();
			col.Insert(helpEntries);
		}
		/// <inheritdoc />
		public async Task AddAsync(CloseQuotes quotes)
		{
			var col = _Db.GetCollection<CloseQuotes>();
			await HandleRemovableMessage(col, quotes.GuildId, quotes.UserId).CAF();
			col.Insert(quotes);
		}
		/// <inheritdoc />
		public void Add(RemovableMessage message)
		{
			_Db.GetCollection<RemovableMessage>().Insert(message);
		}
		/// <inheritdoc />
		public void Add(TimedMessage message)
		{
			var col = _Db.GetCollection<TimedMessage>();
			//Only allow one timed message per user at a time.
			col.Delete(x => x.UserId == message.UserId);
			col.Insert(message);
		}
		/// <inheritdoc />
		public async Task<RemovablePunishment> RemovePunishmentAsync(ulong guildId, ulong userId, Punishment punishment)
		{
			var col = _Db.GetCollection<RemovablePunishment>();
			var entry = col.FindOne(x => x.UserId == userId && x.GuildId == guildId && x.PunishmentType == punishment);
			if (entry != null)
			{
				col.Delete(entry.Id);
				await entry.RemoveAsync(_Client, _PunishmentRemover, _PunishmentReason).CAF();
			}
			return entry;
		}
		/// <inheritdoc />
		public async Task<CloseHelpEntries> RemoveActiveCloseHelpAsync(ulong guildId, ulong userId)
		{
			return await HandleRemovableMessage(_Db.GetCollection<CloseHelpEntries>(), guildId, userId).CAF();
		}
		/// <inheritdoc />
		public async Task<CloseQuotes> RemoveActiveCloseQuoteAsync(ulong guildId, ulong userId)
		{
			return await HandleRemovableMessage(_Db.GetCollection<CloseQuotes>(), guildId, userId).CAF();
		}

		/// <summary>
		/// Deletes messages past their expiry time.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="col"></param>
		/// <returns></returns>
		private async Task RemoveRemovableMessages<T>(LiteCollection<T> col) where T : RemovableMessage
		{
			foreach (var guildGroup in col.Find(x => x.Time < DateTime.UtcNow).GroupBy(x => x.GuildId))
			{
				//Remove them from the database
				foreach (var g in guildGroup)
				{
					col.Delete(g.Id);
				}
				if (!(_Client.GetGuild(guildGroup.Key) is SocketGuild guild))
				{
					continue;
				}
				foreach (var channelGroup in guildGroup.GroupBy(x => x.ChannelId))
				{
					if (!(guild.GetTextChannel(channelGroup.Key) is SocketTextChannel channel))
					{
						continue;
					}

					var tasks = channelGroup
						.SelectMany(g => g.MessageIds)
						.Select(async m => m == 0 ? null : await channel.GetMessageAsync(m).CAF());
					var messages = (await Task.WhenAll(tasks).CAF()).Where(x => x != null).ToList();
					await MessageUtils.DeleteMessagesAsync(channel, messages, _MessageReason).CAF();
				}
			}
		}
		/// <summary>
		/// Retrieves close quotes and help entries before they're deleted.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="col"></param>
		/// <param name="guildId"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		private async Task<T> HandleRemovableMessage<T>(LiteCollection<T> col, ulong guildId, ulong userId) where T : RemovableMessage
		{
			var entry = col.FindOne(x => x.GuildId == guildId && x.UserId == userId);
			if (entry == null)
			{
				return null;
			}
			if (_Client.GetGuild(entry.GuildId) is SocketGuild guild &&
				guild.GetTextChannel(entry.ChannelId) is SocketTextChannel channel)
			{
				var tasks = entry.MessageIds
					.Where(m => m != 0)
					.Select(async m => m == 0 ? null : await channel.GetMessageAsync(m).CAF());
				var messages = (await Task.WhenAll(tasks).CAF()).Where(x => x != null).ToList();
				await MessageUtils.DeleteMessagesAsync(channel, messages, _MessageReason).CAF();
			}
			col.Delete(entry.Id);
			return entry;
		}

		//ITimersService
		Task ITimerService.AddAsync(RemovableMessage message)
		{
			Add(message);
			return Task.CompletedTask;
		}
		Task ITimerService.AddAsync(TimedMessage message)
		{
			Add(message);
			return Task.CompletedTask;
		}
	}
}
