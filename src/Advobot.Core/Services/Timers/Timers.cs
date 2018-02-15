using Advobot.Core.Classes;
using Advobot.Core.Classes.CloseWords;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Classes.Settings;
using Advobot.Core.Classes.UserInformation;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Advobot.Core.Services.Timers
{
	//I have absolutely no idea if this class works as intended under stress.
	internal sealed class TimersService : ITimersService
	{
		private IDiscordClient _Client;

		private Timer _HourTimer = new Timer(60 * 60 * 1000);
		private Timer _MinuteTimer = new Timer(60 * 1000);
		private Timer _SecondTimer = new Timer(1000);
		private PunishmentRemover _PunishmentRemover;
		private RequestOptions _PunishmentReason = ClientUtils.CreateRequestOptions("automatic punishment removal.");
		private RequestOptions _MessageReason = ClientUtils.CreateRequestOptions("automatic message deletion.");
		private RequestOptions _CloseHelpReason = ClientUtils.CreateRequestOptions("removing active close help");
		private RequestOptions _CloseQuotesReason = ClientUtils.CreateRequestOptions("removing active close quotes");

		//Guild specific
		private ConcurrentDoubleKeyDictionary<ulong, MultiKey<ulong, Punishment>, RemovablePunishment> _RemovablePunishments =
			new ConcurrentDoubleKeyDictionary<ulong, MultiKey<ulong, Punishment>, RemovablePunishment>();
		private ConcurrentDoubleKeyDictionary<ulong, ulong, SpamPreventionUserInfo> _SpamPreventionUsers =
			new ConcurrentDoubleKeyDictionary<ulong, ulong, SpamPreventionUserInfo>();
		private ConcurrentDoubleKeyDictionary<ulong, ulong, SlowmodeUserInfo> _SlowmodeUsers =
			new ConcurrentDoubleKeyDictionary<ulong, ulong, SlowmodeUserInfo>();
		private ConcurrentDoubleKeyDictionary<ulong, ulong, BannedPhraseUserInfo> _BannedPhraseUsers =
			new ConcurrentDoubleKeyDictionary<ulong, ulong, BannedPhraseUserInfo>();
		//Not guild specific
		private ConcurrentDictionary<MultiKey<ulong, long>, RemovableMessage> _RemovableMessages =
			new ConcurrentDictionary<MultiKey<ulong, long>, RemovableMessage>();
		private ConcurrentDictionary<ulong, TimedMessage> _TimedMessages =
			new ConcurrentDictionary<ulong, TimedMessage>();
		private ConcurrentDictionary<ulong, CloseWords<HelpEntry>> _ActiveCloseHelp =
			new ConcurrentDictionary<ulong, CloseWords<HelpEntry>>();
		private ConcurrentDictionary<ulong, CloseWords<Quote>> _ActiveCloseQuotes =
			new ConcurrentDictionary<ulong, CloseWords<Quote>>();

		public TimersService(IServiceProvider provider)
		{
			_Client = provider.GetRequiredService<IDiscordClient>();
			_PunishmentRemover = new PunishmentRemover(this);

			_HourTimer.Elapsed += (sender, e) =>
			{
				Task.Run(() => _SpamPreventionUsers.Clear());
			};
			_HourTimer.Enabled = true;

			_MinuteTimer.Elapsed += (sender, e) =>
			{
				Task.Run(async () =>
				{
					foreach (var punishment in _RemovablePunishments.RemoveValues(DateTime.UtcNow))
					{
						await punishment.RemoveAsync(_Client, _PunishmentRemover, _PunishmentReason).CAF();
					}
				});
				Task.Run(async () =>
				{
					foreach (var timedMessage in RemoveItemsByTime(_TimedMessages, DateTime.UtcNow))
					{
						if (!(await _Client.GetUserAsync(timedMessage.UserId).CAF() is IUser user))
						{
							continue;
						}

						await user.SendMessageAsync(timedMessage.Text).CAF();
					}
				});
			};
			_MinuteTimer.Enabled = true;

			_SecondTimer.Elapsed += (sender, e) =>
			{
				Task.Run(async () =>
				{
					foreach (var guildGroup in RemoveItemsByTime(_RemovableMessages, DateTime.UtcNow).GroupBy(x => x.GuildId))
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
					}
				});
				Task.Run(async () =>
				{
					foreach (var guildGroup in RemoveItemsByTime(_ActiveCloseHelp, DateTime.UtcNow).GroupBy(x => x.GuildId))
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
					}
				});
				Task.Run(async () =>
				{
					foreach (var guildGroup in RemoveItemsByTime(_ActiveCloseQuotes, DateTime.UtcNow).GroupBy(x => x.GuildId))
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
					}
				});
				Task.Run(() => _SlowmodeUsers.RemoveValues(DateTime.UtcNow));
			};
			_SecondTimer.Enabled = true;
		}

		/// <summary>
		/// Removes all older instances, undoes their current punishment, and stores <paramref name="punishment"/>.
		/// </summary>
		/// <param name="punishment"></param>
		/// <returns></returns>
		public async Task AddAsync(RemovablePunishment punishment)
		{          
			var doubleKey = new MultiKey<ulong, Punishment>(punishment.UserId, punishment.PunishmentType);
			if (_RemovablePunishments.TryRemove(punishment.GuildId, doubleKey, out var value))
			{
				await value.RemoveAsync(_Client, _PunishmentRemover, _PunishmentReason).CAF();
			}
			_RemovablePunishments.TryAdd(punishment.GuildId, doubleKey, punishment);
		}
		/// <summary>
		/// Removes all older instances, deletes the bot's message, and stores <paramref name="helpEntries"/>.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="message"></param>
		/// <param name="helpEntries"></param>
		/// <returns></returns>
		public async Task AddAsync(CloseWords<HelpEntry> helpEntries)
		{
			if (_ActiveCloseHelp.TryRemove(helpEntries.UserId, out var value)
				&& await _Client.GetGuildAsync(value.GuildId).CAF() is IGuild guild
				&& await guild.GetTextChannelAsync(value.ChannelId).CAF() is ITextChannel channel
				&& await channel.GetMessageAsync(value.MessageId).CAF() is IMessage msg)
			{
				await MessageUtils.DeleteMessageAsync(msg, _CloseHelpReason).CAF();
			}
			_ActiveCloseHelp.TryAdd(helpEntries.UserId, helpEntries);
		}
		/// <summary>
		/// Removes all older instances, delete's the bot's message, and stores <paramref name="quotes"/>.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="message"></param>
		/// <param name="quotes"></param>
		/// <returns></returns>
		public async Task AddAsync(CloseWords<Quote> quotes)
		{
			if (_ActiveCloseHelp.TryRemove(quotes.UserId, out var value)
				&& await _Client.GetGuildAsync(value.GuildId).CAF() is IGuild guild
				&& await guild.GetTextChannelAsync(value.ChannelId).CAF() is ITextChannel channel
				&& await channel.GetMessageAsync(value.MessageId).CAF() is IMessage msg)
			{
				await MessageUtils.DeleteMessageAsync(msg, _CloseQuotesReason).CAF();
			}
			_ActiveCloseQuotes.TryAdd(quotes.UserId, quotes);
		}
		public void Add(RemovableMessage message)
		{
			_RemovableMessages.TryAdd(new MultiKey<ulong, long>(message.ChannelId, message.Time.Ticks), message);
		}
		public void Add(TimedMessage message)
		{
			_TimedMessages.AddOrUpdate(message.UserId, message, (key, value) => message);
		}
		public void Add(SpamPreventionUserInfo user)
		{
			_SpamPreventionUsers.AddOrUpdate(user.GuildId, user.UserId, user);
		}
		public void Add(SlowmodeUserInfo user)
		{
			_SlowmodeUsers.AddOrUpdate(user.GuildId, user.UserId, user);
		}
		public void Add(BannedPhraseUserInfo user)
		{
			_BannedPhraseUsers.AddOrUpdate(user.GuildId, user.UserId, user);
		}

		public async Task<RemovablePunishment> RemovePunishmentAsync(IGuild guild, ulong userId, Punishment punishment)
		{
			if (_RemovablePunishments.TryRemove(guild.Id, new MultiKey<ulong, Punishment>(userId, punishment), out var value))
			{
				await value.RemoveAsync(_Client, _PunishmentRemover, _PunishmentReason).CAF();
			}
			return value;
		}
		public async Task<CloseWords<HelpEntry>> RemoveActiveCloseHelpAsync(IUser user)
		{
			if (_ActiveCloseHelp.TryRemove(user.Id, out var value) 
				&& await _Client.GetGuildAsync(value.GuildId).CAF() is IGuild guild
				&& await guild.GetTextChannelAsync(value.ChannelId).CAF() is ITextChannel channel
				&& await channel.GetMessageAsync(value.MessageId).CAF() is IMessage msg)
			{
				await MessageUtils.DeleteMessageAsync(msg, _CloseHelpReason).CAF();
			}
			return value;
		}
		public async Task<CloseWords<Quote>> RemoveActiveCloseQuoteAsync(IUser user)
		{
			if (_ActiveCloseQuotes.TryRemove(user.Id, out var value)
				&& await _Client.GetGuildAsync(value.GuildId).CAF() is IGuild guild
				&& await guild.GetTextChannelAsync(value.ChannelId).CAF() is ITextChannel channel
				&& await channel.GetMessageAsync(value.MessageId).CAF() is IMessage msg)
			{
				await MessageUtils.DeleteMessageAsync(msg, _CloseQuotesReason).CAF();
			}
			return value;
		}
		public IEnumerable<SpamPreventionUserInfo> GetSpamPreventionUsers(IGuild guild)
		{
			return _SpamPreventionUsers.GetValues(guild.Id);
		}
		public IEnumerable<SlowmodeUserInfo> GetSlowmodeUsers(IGuild guild)
		{
			return _SlowmodeUsers.GetValues(guild.Id);
		}
		public IEnumerable<BannedPhraseUserInfo> GetBannedPhraseUsers(IGuild guild)
		{
			return _BannedPhraseUsers.GetValues(guild.Id);
		}
		public SpamPreventionUserInfo GetSpamPreventionUser(IGuildUser user)
		{
			_SpamPreventionUsers.TryGetValue(user.Guild.Id, user.Id, out var spamPrevention);
			return spamPrevention;
		}
		public SlowmodeUserInfo GetSlowmodeUser(IGuildUser user)
		{
			_SlowmodeUsers.TryGetValue(user.Guild.Id, user.Id, out var slowmode);
			return slowmode;
		}
		public BannedPhraseUserInfo GetBannedPhraseUser(IGuildUser user)
		{
			_BannedPhraseUsers.TryGetValue(user.Guild.Id, user.Id, out var bannedPhrases);
			return bannedPhrases;
		}

		/// <summary>
		/// Gets and removes items older than <paramref name="time"/>.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		public static IEnumerable<TValue> RemoveItemsByTime<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dictionary, DateTime time) where TValue : ITime
		{
			//Loop through every value in the dictionary, remove if too old
			foreach (var kvp in dictionary)
			{
				if (kvp.Value.Time.Ticks < time.Ticks && dictionary.TryRemove(kvp.Key, out var value))
				{
					yield return value;
				}
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
