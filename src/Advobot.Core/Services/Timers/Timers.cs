using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Advobot.Core.Classes;
using Advobot.Core.Classes.CloseWords;
using Advobot.Core.Classes.GuildSettings;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Classes.UserInformation;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;

namespace Advobot.Core.Services.Timers
{
	//I have absolutely no idea if this class works as intended under stress.
	internal sealed class TimersSservice : ITimersService
	{
		private Timer _HourTimer = new Timer(60 * 60 * 1000);
		private Timer _MinuteTimer = new Timer(60 * 1000);
		private Timer _SecondTimer = new Timer(1000);
		private PunishmentRemover _PunishmentRemover;
		private ModerationReason _PunishmentReason = new ModerationReason("automatic punishment removal.");
		private ModerationReason _MessageReason = new ModerationReason("automatic message deletion.");
		private ModerationReason _CloseHelpReason = new ModerationReason("removing active close help");
		private ModerationReason _CloseQuotesReason = new ModerationReason("removing active close quotes");

		//Guild specific
		private ConcurrentDoubleKeyDictionary<IGuild, MultiKey<ulong, PunishmentType>, RemovablePunishment> _RemovablePunishments =
			new ConcurrentDoubleKeyDictionary<IGuild, MultiKey<ulong, PunishmentType>, RemovablePunishment>();
		private ConcurrentDoubleKeyDictionary<IGuild, ulong, SpamPreventionUserInfo> _SpamPreventionUsers =
			new ConcurrentDoubleKeyDictionary<IGuild, ulong, SpamPreventionUserInfo>();
		private ConcurrentDoubleKeyDictionary<IGuild, ulong, SlowmodeUserInfo> _SlowmodeUsers =
			new ConcurrentDoubleKeyDictionary<IGuild, ulong, SlowmodeUserInfo>();
		private ConcurrentDoubleKeyDictionary<IGuild, ulong, BannedPhraseUserInfo> _BannedPhraseUsers =
			new ConcurrentDoubleKeyDictionary<IGuild, ulong, BannedPhraseUserInfo>();
		//Not guild specific
		private ConcurrentDictionary<MultiKey<ulong, long>, RemovableMessage> _RemovableMessages =
			new ConcurrentDictionary<MultiKey<ulong, long>, RemovableMessage>();
		private ConcurrentDictionary<ulong, TimedMessage> _TimedMessages =
			new ConcurrentDictionary<ulong, TimedMessage>();
		private ConcurrentDictionary<ulong, CloseWordsWrapper<HelpEntry>> _ActiveCloseHelp =
			new ConcurrentDictionary<ulong, CloseWordsWrapper<HelpEntry>>();
		private ConcurrentDictionary<ulong, CloseWordsWrapper<Quote>> _ActiveCloseQuotes =
			new ConcurrentDictionary<ulong, CloseWordsWrapper<Quote>>();

		public TimersSservice(IServiceProvider provider)
		{
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
					foreach (var punishment in _RemovablePunishments.GetValues(DateTime.UtcNow))
					{
						await punishment.RemoveAsync(_PunishmentRemover, _PunishmentReason).CAF();
					}
				});
				Task.Run(async () =>
				{
					foreach (var timedMessage in Utils.GetItemsByTime(_TimedMessages, DateTime.UtcNow))
					{
						await timedMessage.SendAsync().CAF();
					}
				});
			};
			_MinuteTimer.Enabled = true;

			_SecondTimer.Elapsed += (sender, e) =>
			{
				Task.Run(async () =>
				{
					foreach (var group in Utils.GetItemsByTime(_RemovableMessages, DateTime.UtcNow).GroupBy(x => x.Channel?.Id ?? 0))
					{
						if (group.Key == 0)
						{
							continue;
						}

						var groupMsgs = group.SelectMany(x => x.Messages).ToList();
						if (groupMsgs.Count() == 1)
						{
							await MessageUtils.DeleteMessageAsync(groupMsgs.First(), _MessageReason).CAF();
						}
						else
						{
							var channel = group.First().Channel;
							await MessageUtils.DeleteMessagesAsync(channel, groupMsgs, _MessageReason).CAF();
						}
					}
				});
				Task.Run(async () =>
				{
					foreach (var helpEntries in Utils.GetItemsByTime(_ActiveCloseHelp, DateTime.UtcNow))
					{
						await MessageUtils.DeleteMessageAsync(helpEntries.Message, _CloseHelpReason).CAF();
					}
				});
				Task.Run(async () =>
				{
					foreach (var quotes in Utils.GetItemsByTime(_ActiveCloseQuotes, DateTime.UtcNow))
					{
						await MessageUtils.DeleteMessageAsync(quotes.Message, _CloseQuotesReason).CAF();
					}
				});
				Task.Run(() => _SlowmodeUsers.GetValues(DateTime.UtcNow));
			};
			_SecondTimer.Enabled = true;
		}

		/// <summary>
		/// Removes all older instances, undoes their current punishment, and stores <paramref name="punishment"/>.
		/// </summary>
		/// <param name="punishment"></param>
		/// <returns></returns>
		public async Task Add(RemovablePunishment punishment)
		{          
			var doubleKey = new MultiKey<ulong, PunishmentType>(punishment.UserId, punishment.PunishmentType);
			if (_RemovablePunishments.TryRemove(punishment.Guild, doubleKey, out var value))
			{
				await value.RemoveAsync(_PunishmentRemover, _PunishmentReason).CAF();
			}
			_RemovablePunishments.TryAdd(punishment.Guild, doubleKey, punishment);
		}
		/// <summary>
		/// Removes all older instances, deletes the bot's message, and stores <paramref name="helpEntries"/>.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="message"></param>
		/// <param name="helpEntries"></param>
		/// <returns></returns>
		public async Task Add(IUser user, IUserMessage message, CloseWords<HelpEntry> helpEntries)
		{
			if (_ActiveCloseHelp.TryRemove(user.Id, out var value))
			{
				await MessageUtils.DeleteMessageAsync(value.Message, _CloseHelpReason).CAF();
			}
			_ActiveCloseHelp.TryAdd(user.Id, new CloseWordsWrapper<HelpEntry>(helpEntries, message));
		}
		/// <summary>
		/// Removes all older instances, delete's the bot's message, and stores <paramref name="quotes"/>.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="message"></param>
		/// <param name="quotes"></param>
		/// <returns></returns>
		public async Task Add(IUser user, IUserMessage message, CloseWords<Quote> quotes)
		{
			if (_ActiveCloseQuotes.TryRemove(user.Id, out var value))
			{
				await MessageUtils.DeleteMessageAsync(value.Message, _CloseQuotesReason).CAF();
			}
			_ActiveCloseQuotes.TryAdd(user.Id, new CloseWordsWrapper<Quote>(quotes, message));
		}
		public void Add(RemovableMessage message)
		{
			_RemovableMessages.TryAdd(new MultiKey<ulong, long>(message.Channel.Id, message.Time.Ticks), message);
		}
		public void Add(TimedMessage message)
		{
			_TimedMessages.AddOrUpdate(message.Author.Id, message, (key, value) => message);
		}
		public void Add(SpamPreventionUserInfo user)
		{
			_SpamPreventionUsers.AddOrUpdate(user.User.Guild, user.User.Id, user);
		}
		public void Add(SlowmodeUserInfo user)
		{
			_SlowmodeUsers.AddOrUpdate(user.User.Guild, user.User.Id, user);
		}
		public void Add(BannedPhraseUserInfo user)
		{
			_BannedPhraseUsers.AddOrUpdate(user.User.Guild, user.User.Id, user);
		}

		public async Task<RemovablePunishment> RemovePunishment(IGuild guild, ulong userId, PunishmentType punishment)
		{
			if (_RemovablePunishments.TryRemove(guild, new MultiKey<ulong, PunishmentType>(userId, punishment), out var value))
			{
				await value.RemoveAsync(_PunishmentRemover, _PunishmentReason).CAF();
			}
			return value;
		}
		public async Task<CloseWords<HelpEntry>> RemoveActiveCloseHelp(IUser user)
		{
			if (_ActiveCloseHelp.TryRemove(user.Id, out var wrapper))
			{
				await MessageUtils.DeleteMessageAsync(wrapper.Message, _CloseHelpReason).CAF();
			}
			return wrapper.CloseWords;
		}
		public async Task<CloseWords<Quote>> RemoveActiveCloseQuote(IUser user)
		{
			if (_ActiveCloseQuotes.TryRemove(user.Id, out var wrapper))
			{
				await MessageUtils.DeleteMessageAsync(wrapper.Message, _CloseQuotesReason).CAF();
			}
			return wrapper.CloseWords;
		}
		public IEnumerable<SpamPreventionUserInfo> GetSpamPreventionUsers(IGuild guild)
		{
			return _SpamPreventionUsers.GetValues(guild);
		}
		public IEnumerable<SlowmodeUserInfo> GetSlowmodeUsers(IGuild guild)
		{
			return _SlowmodeUsers.GetValues(guild);
		}
		public IEnumerable<BannedPhraseUserInfo> GetBannedPhraseUsers(IGuild guild)
		{
			return _BannedPhraseUsers.GetValues(guild);
		}
		public SpamPreventionUserInfo GetSpamPreventionUser(IGuildUser user)
		{
			_SpamPreventionUsers.TryGetValue(user.Guild, user.Id, out var spamPrevention);
			return spamPrevention;
		}
		public SlowmodeUserInfo GetSlowmodeUser(IGuildUser user)
		{
			_SlowmodeUsers.TryGetValue(user.Guild, user.Id, out var slowmode);
			return slowmode;
		}
		public BannedPhraseUserInfo GetBannedPhraseUser(IGuildUser user)
		{
			_BannedPhraseUsers.TryGetValue(user.Guild, user.Id, out var bannedPhrases);
			return bannedPhrases;
		}
	}

	internal struct MultiKey<TFirst, TSecond>
	{
		public readonly TFirst FirstValue;
		public readonly TSecond SecondValue;

		public MultiKey(TFirst firstValue, TSecond secondValue)
		{
			FirstValue = firstValue;
			SecondValue = secondValue;
		}

		public override bool Equals(object obj)
		{
			return obj is MultiKey<TFirst, TSecond> key && key.FirstValue.Equals(FirstValue) && key.SecondValue.Equals(SecondValue);
		}
		public override int GetHashCode()
		{
			//Source: https://stackoverflow.com/a/263416
			unchecked // Overflow is fine, just wrap
			{
				var hash = (int)2166136261;
				// Suitable nullity checks etc, of course :)
				hash = (hash * 16777619) ^ FirstValue.GetHashCode();
				hash = (hash * 16777619) ^ SecondValue.GetHashCode();
				return hash;
			}
		}

		public static bool operator ==(MultiKey<TFirst, TSecond> left, MultiKey<TFirst, TSecond> right)
		{
			return left.Equals(right);
		}
		public static bool operator !=(MultiKey<TFirst, TSecond> left, MultiKey<TFirst, TSecond> right)
		{
			return !left.Equals(right);
		}
	}

	internal sealed class ConcurrentDoubleKeyDictionary<TFirstKey, TSecondKey, TValue> where TValue : ITime
	{
		private ConcurrentDictionary<TFirstKey, ConcurrentDictionary<TSecondKey, TValue>> _Values = new ConcurrentDictionary<TFirstKey, ConcurrentDictionary<TSecondKey, TValue>>();

		public void Clear()
		{
			_Values.Clear();
		}
		public void AddOrUpdate(TFirstKey firstKey, TSecondKey secondKey, TValue value)
		{
			_Values.GetOrAdd(firstKey, new ConcurrentDictionary<TSecondKey, TValue>()).AddOrUpdate(secondKey, value, (k, v) => value);
		}
		public bool TryGetValue(TFirstKey firstKey, TSecondKey secondKey, out TValue value)
		{
			return !_Values.GetOrAdd(firstKey, new ConcurrentDictionary<TSecondKey, TValue>()).TryGetValue(secondKey, out value);
		}
		public bool TryAdd(TFirstKey firstKey, TSecondKey secondKey, TValue value)
		{
			return !_Values.GetOrAdd(firstKey, new ConcurrentDictionary<TSecondKey, TValue>()).TryAdd(secondKey, value);
		}
		public bool TryRemove(TFirstKey firstKey, TSecondKey secondKey, out TValue value)
		{
			return !_Values.GetOrAdd(firstKey, new ConcurrentDictionary<TSecondKey, TValue>()).TryRemove(secondKey, out value);
		}
		public IEnumerable<TValue> GetValues(TFirstKey firstKey)
		{
			return !_Values.TryGetValue(firstKey, out var innerDict) ? innerDict.Values : Enumerable.Empty<TValue>();
		}
		public IEnumerable<TValue> GetValues(DateTime time)
		{
			//Loop through each inner dictionary inside the outer dictionary
			foreach (var outerKvp in _Values)
			{
				foreach (var value in Utils.GetItemsByTime(outerKvp.Value, time))
				{
					yield return value;
				}
			}
		}
	}
}
