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
using System.Threading.Tasks;
using System.Timers;

namespace Advobot.Core.Services.Timers
{
	//I have absolutely no idea if this class works as intended under stress.
	internal sealed class TimersService : ITimersService, IDisposable
	{
		private static string _DBLoc = IOUtils.GetBaseBotDirectoryFile("TimedDatabase.db").ToString();
		private LiteDatabase _DB;
		private IDiscordClient _Client;

		private Timer _HourTimer = new Timer(60 * 60 * 1000);
		private Timer _MinuteTimer = new Timer(60 * 1000);
		private Timer _SecondTimer = new Timer(1000);
		private PunishmentRemover _PunishmentRemover;
		private RequestOptions _PunishmentReason = ClientUtils.CreateRequestOptions("automatic punishment removal.");
		private RequestOptions _MessageReason = ClientUtils.CreateRequestOptions("automatic message deletion.");
		private RequestOptions _CloseHelpReason = ClientUtils.CreateRequestOptions("removing active close help");
		private RequestOptions _CloseQuotesReason = ClientUtils.CreateRequestOptions("removing active close quotes");

		public TimersService(IServiceProvider provider)
		{
			_Client = provider.GetRequiredService<IDiscordClient>();
			_PunishmentRemover = new PunishmentRemover(this);

			_HourTimer.Elapsed += (sender, e) =>
			{
				Task.Run(() =>
				{
					//TODO: does check if collection needs to exist before doing this
					_DB.DropCollection(typeof(SpamPreventionUserInfo).Name);
				});
			};
			_MinuteTimer.Elapsed += (sender, e) =>
			{
				Task.Run(async () =>
				{
					var col = _DB.GetCollection<RemovablePunishment>();
					foreach (var punishment in col.Find(x => x.Time < DateTime.UtcNow))
					{
						await punishment.RemoveAsync(_Client, _PunishmentRemover, _PunishmentReason).CAF();
						col.Delete(punishment.Id);
					}
				});
				Task.Run(async () =>
				{
					var col = _DB.GetCollection<TimedMessage>();
					foreach (var timedMessage in col.Find(x => x.Time < DateTime.UtcNow))
					{
						if (!(await _Client.GetUserAsync(timedMessage.UserId).CAF() is IUser user))
						{
							col.Delete(timedMessage.Id);
							continue;
						}

						await user.SendMessageAsync(timedMessage.Text).CAF();
						col.Delete(timedMessage.Id);
					}
				});
			};
			_SecondTimer.Elapsed += (sender, e) =>
			{
				Task.Run(async () =>
				{
					var col = _DB.GetCollection<RemovableMessage>();
					foreach (var guildGroup in col.Find(x => x.Time < DateTime.UtcNow).GroupBy(x => x.GuildId))
					{
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
						//Remove them from the database. After instead of above because it modifies the 
						foreach (var g in guildGroup)
						{
							col.Delete(g.Id);
						}
					}
				});
				Task.Run(async () =>
				{
					var col = _DB.GetCollection<CloseHelpEntries>();
					foreach (var guildGroup in col.Find(x => x.Time < DateTime.UtcNow).GroupBy(x => x.GuildId))
					{
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
								await MessageUtils.DeleteMessageAsync(messages.First(), _CloseHelpReason).CAF();
							}
							else
							{
								await MessageUtils.DeleteMessagesAsync(channel, messages, _CloseHelpReason).CAF();
							}
						}
						//Remove them from the database. After instead of above because it modifies the 
						foreach (var g in guildGroup)
						{
							col.Delete(g.Id);
						}
					}
				});
				Task.Run(async () =>
				{
					var col = _DB.GetCollection<CloseQuotes>();
					foreach (var guildGroup in col.Find(x => x.Time < DateTime.UtcNow).GroupBy(x => x.GuildId))
					{
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
						//Remove them from the database. After instead of above because it modifies the foreach
						foreach (var g in guildGroup)
						{
							col.Delete(g.Id);
						}
					}
				});
				Task.Run(() =>
				{
					_DB.GetCollection<SlowmodeUserInfo>().Delete(x => x.Time < DateTime.UtcNow);
				});
			};
		}

		/// <summary>
		/// Starts the second, minute, and hour timers.
		/// </summary>
		public void Start()
		{
			//TODO: make correctly deserialization so it actually does something
			_DB = new LiteDatabase(_DBLoc);
			_HourTimer.Enabled = true;
			_MinuteTimer.Enabled = true;
			_SecondTimer.Enabled = true;
		}
		/// <summary>
		/// Stops the timers and disposes the database connection.
		/// </summary>
		public void Dispose()
		{
			_HourTimer.Stop();
			_MinuteTimer.Stop();
			_SecondTimer.Stop();
			_DB.Dispose();
		}

		/// <summary>
		/// Removes all older instances, undoes their current punishment, and stores <paramref name="punishment"/>.
		/// </summary>
		/// <param name="punishment"></param>
		/// <returns></returns>
		public async Task AddAsync(RemovablePunishment punishment)
		{          
			var col = _DB.GetCollection<RemovablePunishment>();
			var entry = col.FindOne(x => x.UserId == punishment.UserId && x.GuildId == punishment.GuildId && x.PunishmentType == punishment.PunishmentType);
			if (entry != null)
			{
				await entry.RemoveAsync(_Client, _PunishmentRemover, _PunishmentReason).CAF();
				col.Delete(entry.Id);
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
			var col = _DB.GetCollection<CloseHelpEntries>();
			var entry = col.FindOne(x => x.UserId == helpEntries.UserId);
			if (entry != null
				&& await _Client.GetGuildAsync(entry.GuildId).CAF() is IGuild guild
				&& await guild.GetTextChannelAsync(entry.ChannelId).CAF() is ITextChannel channel
				&& await channel.GetMessageAsync(entry.MessageId).CAF() is IMessage msg)
			{
				await MessageUtils.DeleteMessageAsync(msg, _CloseHelpReason).CAF();
				col.Delete(entry.Id);
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
			var col = _DB.GetCollection<CloseQuotes>();
			var entry = col.FindOne(x => x.UserId == quotes.UserId);
			if (entry != null
				&& await _Client.GetGuildAsync(entry.GuildId).CAF() is IGuild guild
				&& await guild.GetTextChannelAsync(entry.ChannelId).CAF() is ITextChannel channel
				&& await channel.GetMessageAsync(entry.MessageId).CAF() is IMessage msg)
			{
				await MessageUtils.DeleteMessageAsync(msg, _CloseHelpReason).CAF();
				col.Delete(entry.Id);
			}
			col.Insert(quotes);
		}
		public void Add(RemovableMessage message)
		{
			_DB.GetCollection<RemovableMessage>().Insert(message);
		}
		public void Add(TimedMessage message)
		{
			var col = _DB.GetCollection<TimedMessage>();
			//Only allow one timed message per user at a time.
			col.Delete(x => x.UserId == message.UserId);
			col.Insert(message);
		}
		public void Add(SpamPreventionUserInfo user)
		{
			var col = _DB.GetCollection<SpamPreventionUserInfo>();
			//Only allow one spam prevention user at a time
			col.Delete(x => x.UserId == user.UserId && x.GuildId == user.GuildId);
			col.Insert(user);
		}
		public void Add(SlowmodeUserInfo user)
		{
			var col = _DB.GetCollection<SlowmodeUserInfo>();
			//Only allow one spam prevention user at a time
			col.Delete(x => x.UserId == user.UserId && x.GuildId == user.GuildId);
			col.Insert(user);
		}
		public void Add(BannedPhraseUserInfo user)
		{
			var col = _DB.GetCollection<BannedPhraseUserInfo>();
			//Only allow one spam prevention user at a time
			col.Delete(x => x.UserId == user.UserId && x.GuildId == user.GuildId);
			col.Insert(user);
		}

		public async Task<RemovablePunishment> RemovePunishmentAsync(IGuild guild, ulong userId, Punishment punishment)
		{
			var col = _DB.GetCollection<RemovablePunishment>();
			var entry = col.FindOne(x => x.UserId == userId && x.GuildId == guild.Id && x.PunishmentType == punishment);
			if (entry != null)
			{
				await entry.RemoveAsync(_Client, _PunishmentRemover, _PunishmentReason).CAF();
				col.Delete(entry.Id);
			}
			return entry;
		}
		public async Task<CloseHelpEntries> RemoveActiveCloseHelpAsync(IUser user)
		{
			var col = _DB.GetCollection<CloseHelpEntries>();
			var entry = col.FindOne(x => x.UserId == user.Id);
			if (entry != null
				&& await _Client.GetGuildAsync(entry.GuildId).CAF() is IGuild guild
				&& await guild.GetTextChannelAsync(entry.ChannelId).CAF() is ITextChannel channel
				&& await channel.GetMessageAsync(entry.MessageId).CAF() is IMessage msg)
			{
				await MessageUtils.DeleteMessageAsync(msg, _CloseHelpReason).CAF();
				col.Delete(entry.Id);
			}
			return entry;
		}
		public async Task<CloseQuotes> RemoveActiveCloseQuoteAsync(IUser user)
		{
			var col = _DB.GetCollection<CloseQuotes>();
			var entry = col.FindOne(x => x.UserId == user.Id);
			if (entry != null
				&& await _Client.GetGuildAsync(entry.GuildId).CAF() is IGuild guild
				&& await guild.GetTextChannelAsync(entry.ChannelId).CAF() is ITextChannel channel
				&& await channel.GetMessageAsync(entry.MessageId).CAF() is IMessage msg)
			{
				await MessageUtils.DeleteMessageAsync(msg, _CloseHelpReason).CAF();
				col.Delete(entry.Id);
			}
			return entry;
		}
		public IEnumerable<SpamPreventionUserInfo> GetSpamPreventionUsers(IGuild guild)
		{
			return _DB.GetCollection<SpamPreventionUserInfo>().Find(x => x.GuildId == guild.Id);
		}
		public IEnumerable<SlowmodeUserInfo> GetSlowmodeUsers(IGuild guild)
		{
			return _DB.GetCollection<SlowmodeUserInfo>().Find(x => x.GuildId == guild.Id);
		}
		public IEnumerable<BannedPhraseUserInfo> GetBannedPhraseUsers(IGuild guild)
		{
			return _DB.GetCollection<BannedPhraseUserInfo>().Find(x => x.GuildId == guild.Id);
		}
		public SpamPreventionUserInfo GetSpamPreventionUser(IGuildUser user)
		{
			return _DB.GetCollection<SpamPreventionUserInfo>().FindOne(x => x.UserId == user.Id && x.GuildId == user.GuildId);
		}
		public SlowmodeUserInfo GetSlowmodeUser(IGuildUser user)
		{
			return _DB.GetCollection<SlowmodeUserInfo>().FindOne(x => x.UserId == user.Id && x.GuildId == user.GuildId);
		}
		public BannedPhraseUserInfo GetBannedPhraseUser(IGuildUser user)
		{
			return _DB.GetCollection<BannedPhraseUserInfo>().FindOne(x => x.UserId == user.Id && x.GuildId == user.GuildId);
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
		Task ITimersService.AddAsync(SpamPreventionUserInfo user)
		{
			Add(user);
			return Task.FromResult(0);
		}
		Task ITimersService.AddAsync(SlowmodeUserInfo user)
		{
			Add(user);
			return Task.FromResult(0);
		}
		Task ITimersService.AddAsync(BannedPhraseUserInfo user)
		{
			Add(user);
			return Task.FromResult(0);
		}
	}
}
