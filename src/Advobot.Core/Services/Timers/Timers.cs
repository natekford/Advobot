using Advobot.Core.Classes;
using Advobot.Core.Classes.CloseWords;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Classes.UserInformation;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace Advobot.Core.Services.Timers
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
		private RequestOptions _CloseHelpReason = ClientUtils.CreateRequestOptions("removing active close help");
		private RequestOptions _CloseQuotesReason = ClientUtils.CreateRequestOptions("removing active close quotes");

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
				var col = _Db.GetCollection<RemovableMessage>();
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

						var tasks = channelGroup.SelectMany(x => x.MessageIds).Select(async x => await channel.GetMessageAsync(x).CAF());
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
			});
			_CloseHelpEntries = new ProcessQueue(1, async () =>
			{
				var col = _Db.GetCollection<CloseHelpEntries>();
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

						var tasks = channelGroup.Where(x => x.MessageId != 0).Select(async x => await channel.GetMessageAsync(x.MessageId).CAF());
						var messages = (await Task.WhenAll(tasks).CAF()).Where(x => x != null).ToList();
						if (!messages.Any())
						{
							continue;
						}
						else if (messages.Count == 1)
						{
							await MessageUtils.DeleteMessageAsync(messages.First(), _CloseHelpReason).CAF();
						}
						else
						{
							await MessageUtils.DeleteMessagesAsync(channel, messages, _CloseHelpReason).CAF();
						}
					}
				}
			});
			_CloseQuotes = new ProcessQueue(1, async () =>
			{
				var col = _Db.GetCollection<CloseQuotes>();
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

						var tasks = channelGroup.Select(async x => await channel.GetMessageAsync(x.MessageId).CAF());
						var messages = (await Task.WhenAll(tasks).CAF()).Where(x => x != null).ToList();
						if (!messages.Any())
						{
							continue;
						}
						else if (messages.Count == 1)
						{
							await MessageUtils.DeleteMessageAsync(messages.First(), _CloseQuotesReason).CAF();
						}
						else
						{
							await MessageUtils.DeleteMessagesAsync(channel, messages, _CloseQuotesReason).CAF();
						}
					}
				}
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

		/// <summary>
		/// Starts the second, minute, and hour timers.
		/// </summary>
		public void Start()
		{
			//TODO: make correctly deserialization so it actually does something
			var loc = IOUtils.GetBaseBotDirectoryFile("TimedDatabase.db").ToString();
			_Db = new LiteDatabase($"filename={loc};mode=exclusive;");
			_MinuteTimer.Enabled = true;
			_SecondTimer.Enabled = true;
		}
		/// <summary>
		/// Stops the timers and disposes the database connection.
		/// </summary>
		public void Dispose()
		{
			_MinuteTimer.Stop();
			_SecondTimer.Stop();
			_Db.Dispose();
		}

		/// <summary>
		/// Removes all older instances, undoes their current punishment, and stores <paramref name="punishment"/>.
		/// </summary>
		/// <param name="punishment"></param>
		/// <returns></returns>
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
		/// <summary>
		/// Removes all older instances, deletes the bot's message, and stores <paramref name="helpEntries"/>.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="message"></param>
		/// <param name="helpEntries"></param>
		/// <returns></returns>
		public async Task AddAsync(CloseHelpEntries helpEntries)
		{
			var col = _Db.GetCollection<CloseHelpEntries>();
			var entry = col.FindOne(x => x.UserId == helpEntries.UserId);
			if (entry != null
				&& await _Client.GetGuildAsync(entry.GuildId).CAF() is IGuild guild
				&& await guild.GetTextChannelAsync(entry.ChannelId).CAF() is ITextChannel channel
				&& await channel.GetMessageAsync(entry.MessageId).CAF() is IMessage msg)
			{
				col.Delete(entry.Id);
				await MessageUtils.DeleteMessageAsync(msg, _CloseHelpReason).CAF();
			}
			col.Insert(helpEntries);
		}
		/// <summary>
		/// Removes all older instances, delete's the bot's message, and stores <paramref name="quotes"/>.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="message"></param>
		/// <param name="quotes"></param>
		/// <returns></returns>
		public async Task AddAsync(CloseQuotes quotes)
		{
			var col = _Db.GetCollection<CloseQuotes>();
			var entry = col.FindOne(x => x.UserId == quotes.UserId);
			if (entry != null
				&& await _Client.GetGuildAsync(entry.GuildId).CAF() is IGuild guild
				&& await guild.GetTextChannelAsync(entry.ChannelId).CAF() is ITextChannel channel
				&& await channel.GetMessageAsync(entry.MessageId).CAF() is IMessage msg)
			{
				col.Delete(entry.Id);
				await MessageUtils.DeleteMessageAsync(msg, _CloseHelpReason).CAF();
			}
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
			var col = _Db.GetCollection<CloseHelpEntries>();
			var entry = col.FindOne(x => x.UserId == user.Id);
			if (entry != null
				&& await _Client.GetGuildAsync(entry.GuildId).CAF() is IGuild guild
				&& await guild.GetTextChannelAsync(entry.ChannelId).CAF() is ITextChannel channel
				&& await channel.GetMessageAsync(entry.MessageId).CAF() is IMessage msg)
			{
				col.Delete(entry.Id);
				await MessageUtils.DeleteMessageAsync(msg, _CloseHelpReason).CAF();
			}
			return entry;
		}
		public async Task<CloseQuotes> RemoveActiveCloseQuoteAsync(IUser user)
		{
			var col = _Db.GetCollection<CloseQuotes>();
			var entry = col.FindOne(x => x.UserId == user.Id);
			if (entry != null
				&& await _Client.GetGuildAsync(entry.GuildId).CAF() is IGuild guild
				&& await guild.GetTextChannelAsync(entry.ChannelId).CAF() is ITextChannel channel
				&& await channel.GetMessageAsync(entry.MessageId).CAF() is IMessage msg)
			{
				col.Delete(entry.Id);
				await MessageUtils.DeleteMessageAsync(msg, _CloseHelpReason).CAF();
			}
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
					await _T.Invoke().CAF();
					_Semaphore.Release();
				});
			}
		}
	}
}
