using Advobot.Core.Classes;
using Advobot.Core.Classes.CloseWords;
using Advobot.Core.Classes.GuildSettings;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Classes.UserInformation;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;

namespace Advobot.Core.Services.Timers
{
	//I have absolutely no idea if this class works as intended under stress.
	internal sealed class Timers : ITimersService
	{
		private PunishmentRemover _PunishmentRemover;
		private Timer _HourTimer = new Timer(60 * 60 * 1000);
		private Timer _MinuteTimer = new Timer(60 * 1000);
		private Timer _HalfSecondTimer = new Timer(1000 / 2);
		private ModerationReason _PunishmentReason = new ModerationReason("automatic punishment removal.");
		private ModerationReason _MessageReason = new ModerationReason("automatic message deletion.");
		private ModerationReason _CloseHelpReason = new ModerationReason("removing active close help");
		private ModerationReason _CloseQuotesReason = new ModerationReason("removing active close quotes");

		private ConcurrentDictionary<UserKey, RemovablePunishment> _RemovablePunishments = new ConcurrentDictionary<UserKey, RemovablePunishment>();
		private ConcurrentDictionary<ChannelKey, RemovableMessage> _RemovableMessages = new ConcurrentDictionary<ChannelKey, RemovableMessage>();
		private ConcurrentDictionary<UserKey, CloseWordsWrapper<HelpEntry>> _ActiveCloseHelp = new ConcurrentDictionary<UserKey, CloseWordsWrapper<HelpEntry>>();
		private ConcurrentDictionary<UserKey, CloseWordsWrapper<Quote>> _ActiveCloseQuotes = new ConcurrentDictionary<UserKey, CloseWordsWrapper<Quote>>();
		private ConcurrentDictionary<UserKey, SpamPreventionUserInfo> _SpamPreventionUsers = new ConcurrentDictionary<UserKey, SpamPreventionUserInfo>();
		private ConcurrentDictionary<UserKey, SlowmodeUserInfo> _SlowmodeUsers = new ConcurrentDictionary<UserKey, SlowmodeUserInfo>();

		public Timers(IServiceProvider provider)
		{
			_PunishmentRemover = new PunishmentRemover(this);

			_HourTimer.Elapsed += (sender, e) =>
			{
				Task.Run(() => ClearSpamPreventionUsers());
			};
			_HourTimer.Enabled = true;

			_MinuteTimer.Elapsed += (sender, e) =>
			{
				Task.Run(async () => await RemovePunishmentsAsync().CAF());
			};
			_MinuteTimer.Enabled = true;

			_HalfSecondTimer.Elapsed += (sender, e) =>
			{
				Task.Run(async () => await DeleteTargettedMessagesAsync().CAF());
				Task.Run(async () => await RemoveActiveCloseHelp().CAF());
				Task.Run(async () => await RemoveActiveCloseQuotes().CAF());
				Task.Run(() => RemoveSlowmodeUsers());
			};
			_HalfSecondTimer.Enabled = true;
		}

		private void ClearSpamPreventionUsers()
		{
			_SpamPreventionUsers.Clear();
		}
		private async Task RemovePunishmentsAsync()
		{
			foreach (var punishment in GetOutTimedObjects(_RemovablePunishments))
			{
				switch (punishment.PunishmentType)
				{
					case PunishmentType.Ban:
					{
						await _PunishmentRemover.UnbanAsync(punishment.Guild, punishment.User.Id, _PunishmentReason).CAF();
						continue;
					}
					case PunishmentType.Deafen:
					{
						var user = punishment.User as IGuildUser ?? await punishment.Guild.GetUserAsync(punishment.User.Id).CAF();
						await _PunishmentRemover.UndeafenAsync(user, _PunishmentReason).CAF();
						continue;
					}
					case PunishmentType.VoiceMute:
					{
						var user = punishment.User as IGuildUser ?? await punishment.Guild.GetUserAsync(punishment.User.Id).CAF();
						await _PunishmentRemover.UnvoicemuteAsync(user, _PunishmentReason).CAF();
						continue;
					}
					case PunishmentType.RoleMute:
					{
						var user = punishment.User as IGuildUser ?? await punishment.Guild.GetUserAsync(punishment.User.Id).CAF();
						await _PunishmentRemover.UnrolemuteAsync(user, punishment.Role, _PunishmentReason).CAF();
						continue;
					}
				}
			}
		}
		private async Task DeleteTargettedMessagesAsync()
		{
			var messages = GetOutTimedObjects(_RemovableMessages).Where(x => x.Channel != null && x.Messages != null);
			foreach (var group in messages.GroupBy(x => x.Channel.Id))
			{
				var groupMsgs = group.SelectMany(x => x.Messages);
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
		}
		private async Task RemoveActiveCloseHelp()
		{
			foreach (var helpEntries in GetOutTimedObjects(_ActiveCloseHelp))
			{
				await MessageUtils.DeleteMessageAsync(helpEntries.Message, _CloseHelpReason).CAF();
			}
		}
		private async Task RemoveActiveCloseQuotes()
		{
			foreach (var quotes in GetOutTimedObjects(_ActiveCloseQuotes))
			{
				await MessageUtils.DeleteMessageAsync(quotes.Message, _CloseQuotesReason).CAF();
			}
		}
		private void RemoveSlowmodeUsers()
		{
			GetOutTimedObjects(_SlowmodeUsers);
		}

		public void Add(RemovablePunishment punishment)
		{
			Add(_RemovablePunishments, new UserKey(punishment.Guild, punishment.User, punishment.Time.Ticks), punishment);
		}
		public void Add(RemovableMessage message)
		{
			Add(_RemovableMessages, new ChannelKey(message.Channel, message.Time.Ticks), message);
		}
		public async Task Add(IGuildUser user, IUserMessage msg, CloseWords<HelpEntry> helpEntries)
		{
			//Remove all older ones; only one can be active at a given time.
			foreach (var kvp in _ActiveCloseHelp.Where(x => x.Key.UserId == user.Id))
			{
				await MessageUtils.DeleteMessageAsync(Remove(_ActiveCloseHelp, kvp.Key).Message, new ModerationReason("removing active close help")).CAF();
			}
			Add(_ActiveCloseHelp, new UserKey(user, helpEntries.Time.Ticks), new CloseWordsWrapper<HelpEntry>(helpEntries, msg));
		}
		public async Task Add(IGuildUser user, IUserMessage msg, CloseWords<Quote> quotes)
		{
			///Remove all older ones; only one can be active at a given time.
			foreach (var kvp in _ActiveCloseQuotes.Where(x => x.Key.UserId == user.Id))
			{
				await MessageUtils.DeleteMessageAsync(Remove(_ActiveCloseQuotes, kvp.Key).Message, new ModerationReason("removing active close quotes")).CAF();
			}
			Add(_ActiveCloseQuotes, new UserKey(user, quotes.Time.Ticks), new CloseWordsWrapper<Quote>(quotes, msg));
		}
		public void Add(SpamPreventionUserInfo user)
		{
			Add(_SpamPreventionUsers, new UserKey(user), user);
		}
		public void Add(SlowmodeUserInfo user)
		{
			Add(_SlowmodeUsers, new UserKey(user), user);
		}

		public int RemovePunishments(ulong userId, PunishmentType punishment)
		{
			//Has to be made into a new list otherwise the concurrent modification also effects it.
			var kvps = _RemovablePunishments.Where(x => x.Key.UserId == userId && x.Value.PunishmentType == punishment).ToList();
			foreach (var kvp in kvps)
			{
				Remove(_RemovablePunishments, kvp.Key);
			}
			return kvps.Count();
		}
		public async Task<CloseWords<HelpEntry>> GetOutActiveCloseHelp(IUser user)
		{
			//Should only ever have one for each user at a time.
			var kvp = _ActiveCloseHelp.SingleOrDefault(x => x.Key.UserId == user.Id);
			if (kvp.Key != null)
			{
				var value = Remove(_ActiveCloseHelp, kvp.Key);
				await MessageUtils.DeleteMessageAsync(value.Message, _CloseHelpReason).CAF();
				return value.CloseWords;
			}
			return null;
		}
		public async Task<CloseWords<Quote>> GetOutActiveCloseQuote(IUser user)
		{
			//Should only ever have one for each user at a time.
			var kvp = _ActiveCloseQuotes.SingleOrDefault(x => x.Key.UserId == user.Id);
			if (kvp.Key != null)
			{
				var value = Remove(_ActiveCloseQuotes, kvp.Key);
				await MessageUtils.DeleteMessageAsync(value.Message, _CloseQuotesReason).CAF();
				return value.CloseWords;
			}
			return null;
		}
		public SpamPreventionUserInfo GetSpamPreventionUser(IGuildUser user)
		{
			var kvp = _SpamPreventionUsers.SingleOrDefault(x => x.Key.GuildId == user.Guild.Id && x.Key.UserId == user.Id);
			return kvp.Equals(default) ? null : kvp.Value;
		}
		public IEnumerable<SpamPreventionUserInfo> GetSpamPreventionUsers(IGuild guild)
		{
			return new List<SpamPreventionUserInfo>(_SpamPreventionUsers.Where(x => x.Key.GuildId == guild.Id).Select(x => x.Value));
		}
		public SlowmodeUserInfo GetSlowmodeUser(IGuildUser user)
		{
			var kvp = _SlowmodeUsers.SingleOrDefault(x => x.Key.GuildId == user.Guild.Id && x.Key.UserId == user.Id);
			return kvp.Equals(default) ? null : kvp.Value;
		}

		/// <summary>
		/// Remove old entries then do something with them.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="concDic"></param>
		/// <returns></returns>
		private IEnumerable<TValue> GetOutTimedObjects<TKey, TValue>(ConcurrentDictionary<TKey, TValue> concDic) where TKey : DictKey where TValue : ITime
		{
			var currentTicks = DateTime.UtcNow.Ticks;
			foreach (var kvp in concDic)
			{
				if (kvp.Key.Ticks < currentTicks)
				{
					yield return Remove(concDic, kvp.Key);
				}
			}
		}
		/// <summary>
		/// Adds <paramref name="value"/> to <paramref name="concDic"/> with the given <paramref name="key"/> unless <paramref name="value"/>
		/// or <paramref name="key"/> are the default values of their type.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="concDic"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		private void Add<TKey, TValue>(ConcurrentDictionary<TKey, TValue> concDic, TKey key, TValue value, [CallerMemberName] string caller = "")
		{
			if (EqualityComparer<TKey>.Default.Equals(key, default) || EqualityComparer<TValue>.Default.Equals(value, default) || !concDic.TryAdd(key, value))
			{
				ConsoleUtils.WriteLine($"Failed to add the object at {key} in {caller}.", color: ConsoleColor.Red);
			}
		}
		/// <summary>
		/// Removes whatever value in <paramref name="concDic"/> has the key <paramref name="key"/>.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="concDic"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		private TValue Remove<TKey, TValue>(ConcurrentDictionary<TKey, TValue> concDic, TKey key, [CallerMemberName] string caller = "")
		{
			TValue value = default;
			if (EqualityComparer<TKey>.Default.Equals(key, default) || !concDic.TryRemove(key, out value))
			{
				ConsoleUtils.WriteLine($"Failed to remove the object at {key} in {caller}.", color: ConsoleColor.Red);
			}
			return value;
		}
	}
}
