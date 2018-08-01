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
	//I have absolutely no idea if this class works as intended under stress.
	internal sealed class TimersService : ITimersService, IDisposable
	{
		private LiteDatabase _Db;
		private readonly DiscordShardedClient _Client;

		private readonly Timer _MinuteTimer = new Timer(60 * 1000);
		private readonly Timer _SecondTimer = new Timer(1000);
		private readonly Punisher _PunishmentRemover;
		private readonly RequestOptions _PunishmentReason = ClientUtils.CreateRequestOptions("automatic punishment removal.");
		private readonly RequestOptions _MessageReason = ClientUtils.CreateRequestOptions("automatic message deletion.");

		private readonly ProcessQueue _RemovablePunishments;
		private readonly ProcessQueue _TimedMessages;
		private readonly ProcessQueue _RemovableMessages;
		private readonly ProcessQueue _CloseHelpEntries;
		private readonly ProcessQueue _CloseQuotes;

		public TimersService(IServiceProvider provider)
		{
			_Client = provider.GetRequiredService<DiscordShardedClient>();
			_PunishmentRemover = new Punisher(TimeSpan.FromMinutes(0), this);

			_RemovablePunishments = new ProcessQueue(1, async () =>
			{
				var col = _Db.GetCollection<RemovablePunishment>();
				foreach (var punishment in col.Find(x => x.Time < DateTime.UtcNow))
				{
					col.Delete(punishment.Id);
					await punishment.RemoveAsync(_Client, _PunishmentRemover, _PunishmentReason).CAF();
				}
			});
			_TimedMessages = new ProcessQueue(1, async () =>
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
			_RemovableMessages = new ProcessQueue(1, async () =>
			{
				await HandleRemovableMessages(_Db.GetCollection<RemovableMessage>()).CAF();
			});
			_CloseHelpEntries = new ProcessQueue(1, async () =>
			{
				await HandleRemovableMessages(_Db.GetCollection<CloseHelpEntries>()).CAF();
			});
			_CloseQuotes = new ProcessQueue(1, async () =>
			{
				await HandleRemovableMessages(_Db.GetCollection<CloseQuotes>()).CAF();
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
				Filename = FileUtils.GetBaseBotDirectoryFile("TimedDatabase.db").FullName,
				Mode = FileMode.Exclusive,
			});
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
			await RemoveRemovableMessage(col, helpEntries.UserId).CAF();
			col.Insert(helpEntries);
		}
		/// <inheritdoc />
		public async Task AddAsync(CloseQuotes quotes)
		{
			var col = _Db.GetCollection<CloseQuotes>();
			await RemoveRemovableMessage(col, quotes.UserId).CAF();
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
		public async Task<RemovablePunishment> RemovePunishmentAsync(IGuild guild, ulong userId, Punishment punishment)
		{
			var col = _Db.GetCollection<RemovablePunishment>();
			var entry = col.FindOne(x => x.UserId == userId && x.GuildId == guild.Id && x.PunishmentType == punishment);
			if (entry != null)
			{
				col.Delete(entry.Id);
				await entry.RemoveAsync(_Client, _PunishmentRemover, _PunishmentReason).CAF();
			}
			return entry;
		}
		/// <inheritdoc />
		public async Task<CloseHelpEntries> RemoveActiveCloseHelpAsync(IUser user)
		{
			return await RemoveRemovableMessage(_Db.GetCollection<CloseHelpEntries>(), user.Id).CAF();
		}
		/// <inheritdoc />
		public async Task<CloseQuotes> RemoveActiveCloseQuoteAsync(IUser user)
		{
			return await RemoveRemovableMessage(_Db.GetCollection<CloseQuotes>(), user.Id).CAF();
		}

		private async Task HandleRemovableMessages<T>(LiteCollection<T> col) where T : RemovableMessage
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
		private async Task<T> RemoveRemovableMessage<T>(LiteCollection<T> col, ulong userId) where T : RemovableMessage
		{
			var entry = col.FindOne(x => x.UserId == userId);
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
		Task ITimersService.AddAsync(RemovableMessage message)
		{
			Add(message);
			return Task.FromResult(0);
		}
		Task ITimersService.AddAsync(TimedMessage message)
		{
			Add(message);
			return Task.FromResult(0);
		}
	}
}
