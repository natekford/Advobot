using Advobot.Classes;
using Advobot.Classes.CloseWords;
using Advobot.Classes.Punishments;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace Advobot.Services.Timers
{
	//I have absolutely no idea if this class works as intended under stress.
	internal sealed class TimersService : ITimersService, IDisposable
	{
		private LiteDatabase _Db;
		private IDiscordClient _Client;

		private Timer _MinuteTimer = new Timer(60 * 1000);
		private Timer _SecondTimer = new Timer(1000);
		private PunishmentRemover _PunishmentRemover;
		private RequestOptions _PunishmentReason = ClientUtils.CreateRequestOptions("automatic punishment removal.");
		private RequestOptions _MessageReason = ClientUtils.CreateRequestOptions("automatic message deletion.");

		private ProcessQueue _RemovablePunishments;
		private ProcessQueue _TimedMessages;
		private ProcessQueue _RemovableMessages;
		private ProcessQueue _CloseHelpEntries;
		private ProcessQueue _CloseQuotes;

		public TimersService(IServiceProvider provider)
		{
			_Client = provider.GetRequiredService<IDiscordClient>();
			_PunishmentRemover = new PunishmentRemover(this);

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
					if (!(await _Client.GetUserAsync(timedMessage.UserId).CAF() is IUser user))
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

		public void Start()
		{
			//Use mode=exclusive to not have ioexceptions
			_Db = new LiteDatabase($"filename={FileUtils.GetBaseBotDirectoryFile("TimedDatabase.db")};mode=exclusive;");
			_MinuteTimer.Enabled = true;
			_SecondTimer.Enabled = true;
		}
		public void Dispose()
		{
			_MinuteTimer.Stop();
			_SecondTimer.Stop();
			_Db.Dispose();
		}

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
		public async Task AddAsync(CloseHelpEntries helpEntries)
		{
			var col = _Db.GetCollection<CloseHelpEntries>();
			await RemoveRemovableMessage(col, helpEntries.UserId).CAF();
			col.Insert(helpEntries);
		}
		public async Task AddAsync(CloseQuotes quotes)
		{
			var col = _Db.GetCollection<CloseQuotes>();
			await RemoveRemovableMessage(col, quotes.UserId).CAF();
			col.Insert(quotes);
		}
		public void Add(RemovableMessage message)
		{
			_Db.GetCollection<RemovableMessage>().Insert(message);
		}
		public void Add(TimedMessage message)
		{
			var col = _Db.GetCollection<TimedMessage>();
			//Only allow one timed message per user at a time.
			col.Delete(x => x.UserId == message.UserId);
			col.Insert(message);
		}

		public bool Update(RemovablePunishment punishment)
		{
			return _Db.GetCollection<RemovablePunishment>().Upsert(punishment);
		}
		public bool Update(CloseHelpEntries help)
		{
			return _Db.GetCollection<CloseHelpEntries>().Upsert(help);
		}
		public bool Update(CloseQuotes quote)
		{
			return _Db.GetCollection<CloseQuotes>().Upsert(quote);
		}
		public bool Update(RemovableMessage message)
		{
			return _Db.GetCollection<RemovableMessage>().Upsert(message);
		}
		public bool Update(TimedMessage message)
		{
			return _Db.GetCollection<TimedMessage>().Upsert(message);
		}

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
		public async Task<CloseHelpEntries> RemoveActiveCloseHelpAsync(IUser user)
		{
			return await RemoveRemovableMessage(_Db.GetCollection<CloseHelpEntries>(), user.Id).CAF();
		}
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
				if (!(await _Client.GetGuildAsync(guildGroup.Key).CAF() is IGuild guild))
				{
					continue;
				}
				foreach (var channelGroup in guildGroup.GroupBy(x => x.ChannelId))
				{
					if (!(await guild.GetTextChannelAsync(channelGroup.Key).CAF() is ITextChannel channel))
					{
						continue;
					}

					var tasks = channelGroup
						.SelectMany(g => g.MessageIds)
						.Select(async m => m == 0 ? null : await channel.GetMessageAsync(m).CAF());
					var messages = (await Task.WhenAll(tasks).CAF()).Where(x => x != null).ToList();
					if (!messages.Any())
					{
						continue;
					}
					else if (messages.Count == 1)
					{
						await MessageUtils.DeleteMessageAsync(messages.First(), _MessageReason).CAF();
					}
					else
					{
						await MessageUtils.DeleteMessagesAsync(channel, messages, _MessageReason).CAF();
					}
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
			if (await _Client.GetGuildAsync(entry.GuildId).CAF() is IGuild guild &&
				await guild.GetTextChannelAsync(entry.ChannelId).CAF() is ITextChannel channel)
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

		private class ProcessQueue
		{
			Func<Task> _T;
			SemaphoreSlim _Semaphore;

			public ProcessQueue(int threads, Func<Task> t)
			{
				_Semaphore = new SemaphoreSlim(threads);
				_T = t;
			}

			public void Process()
			{
				if (_Semaphore.CurrentCount <= 0)
				{
					return;
				}

				Task.Run(async () =>
				{
					await _Semaphore.WaitAsync().CAF();
					await _T().CAF();
					_Semaphore.Release();
				});
			}
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
