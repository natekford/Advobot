using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Classes;
using Advobot.Core.Classes.CloseWords;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Classes.UserInformation;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
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
		private readonly PunishmentRemover _PunishmentRemover;
		private readonly Timer _HourTimer = new Timer(60 * 60 * 1000);
		private readonly Timer _MinuteTimer = new Timer(60 * 1000);
		private readonly Timer _HalfSecondTimer = new Timer(1000 / 2);

		private readonly ConcurrentDictionary<UserKey, RemovablePunishment> _RemovablePunishments = new ConcurrentDictionary<UserKey, RemovablePunishment>();
		private readonly ConcurrentDictionary<ChannelKey, RemovableMessage> _RemovableMessages = new ConcurrentDictionary<ChannelKey, RemovableMessage>();
		private readonly ConcurrentDictionary<UserKey, CloseWordsWrapper<HelpEntryHolder.HelpEntry>> _ActiveCloseHelp = new ConcurrentDictionary<UserKey, CloseWordsWrapper<HelpEntryHolder.HelpEntry>>();
		private readonly ConcurrentDictionary<UserKey, CloseWordsWrapper<Quote>> _ActiveCloseQuotes = new ConcurrentDictionary<UserKey, CloseWordsWrapper<Quote>>();
		private readonly ConcurrentDictionary<UserKey, SpamPreventionUserInfo> _SpamPreventionUsers = new ConcurrentDictionary<UserKey, SpamPreventionUserInfo>();
		private readonly ConcurrentDictionary<UserKey, SlowmodeUserInfo> _SlowmodeUsers = new ConcurrentDictionary<UserKey, SlowmodeUserInfo>();

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
				var punishmentType = punishment.PunishmentType;
				var reason = new ModerationReason($"automatic un{punishmentType.EnumName().FormatTitle().Replace(' ', '-')}");
				switch (punishmentType)
				{
					case PunishmentType.Ban:
					{
						await _PunishmentRemover.UnbanAsync(punishment.Guild, punishment.User.Id, reason).CAF();
						continue;
					}
					case PunishmentType.Deafen:
					{
						var user = punishment.User as IGuildUser ?? await punishment.Guild.GetUserAsync(punishment.User.Id).CAF();
						await _PunishmentRemover.UndeafenAsync(user, reason).CAF();
						continue;
					}
					case PunishmentType.VoiceMute:
					{
						var user = punishment.User as IGuildUser ?? await punishment.Guild.GetUserAsync(punishment.User.Id).CAF();
						await _PunishmentRemover.UnvoicemuteAsync(user, reason).CAF();
						continue;
					}
					case PunishmentType.RoleMute:
					{
						var user = punishment.User as IGuildUser ?? await punishment.Guild.GetUserAsync(punishment.User.Id).CAF();
						await _PunishmentRemover.UnrolemuteAsync(user, punishment.Role, reason).CAF();
						continue;
					}
				}
			}
		}
		private async Task DeleteTargettedMessagesAsync()
		{
			foreach (var group in GetOutTimedObjects(_RemovableMessages).GroupBy(x => x.Channel.Id))
			{
				var reason = new ModerationReason("automatic message deletion.");
				var messages = group.SelectMany(x => x.Messages);
				if (messages.Count() == 1)
				{
					await MessageUtils.DeleteMessageAsync(messages.Single(), reason).CAF();
				}
				else
				{
					var channel = group.First().Channel;
					await MessageUtils.DeleteMessagesAsync(channel, messages, reason).CAF();
				}
			}
		}
		private async Task RemoveActiveCloseHelp()
		{
			foreach (var helpEntries in GetOutTimedObjects(_ActiveCloseHelp))
			{
				await MessageUtils.DeleteMessageAsync(helpEntries.Message, new ModerationReason("removing active close help")).CAF();
			}
		}
		private async Task RemoveActiveCloseQuotes()
		{
			foreach (var quotes in GetOutTimedObjects(_ActiveCloseQuotes))
			{
				await MessageUtils.DeleteMessageAsync(quotes.Message, new ModerationReason("removing active close quotes")).CAF();
			}
		}
		private void RemoveSlowmodeUsers()
		{
			RemoveTimedObjects(_SlowmodeUsers);
		}

		public void AddRemovablePunishment(RemovablePunishment punishment)
		{
			Add(_RemovablePunishments, new UserKey(punishment.Guild, punishment.User, punishment.GetTime().Ticks), punishment);
		}

		public void AddRemovableMessage(RemovableMessage message)
		{
			Add(_RemovableMessages, new ChannelKey(message.Channel, message.GetTime().Ticks), message);
		}

		public async Task AddActiveCloseHelp(IGuildUser user, IUserMessage msg, CloseWords<HelpEntryHolder.HelpEntry> helpEntries)
		{
			//Remove all older ones; only one can be active at a given time.
			foreach (var kvp in _ActiveCloseHelp.Where(x => x.Key.UserId == user.Id))
			{
				await MessageUtils.DeleteMessageAsync(Remove(_ActiveCloseHelp, kvp.Key).Message, new ModerationReason("removing active close help")).CAF();
			}
			Add(_ActiveCloseHelp, new UserKey(user, helpEntries.GetTime().Ticks), new CloseWordsWrapper<HelpEntryHolder.HelpEntry>(helpEntries, msg));
		}
		public async Task AddActiveCloseQuote(IGuildUser user, IUserMessage msg, CloseWords<Quote> quotes)
		{
			///Remove all older ones; only one can be active at a given time.
			foreach (var kvp in _ActiveCloseQuotes.Where(x => x.Key.UserId == user.Id))
			{
				await MessageUtils.DeleteMessageAsync(Remove(_ActiveCloseQuotes, kvp.Key).Message, new ModerationReason("removing active close quotes")).CAF();
			}
			Add(_ActiveCloseQuotes, new UserKey(user, quotes.GetTime().Ticks), new CloseWordsWrapper<Quote>(quotes, msg));
		}
		public void AddSpamPreventionUser(SpamPreventionUserInfo user)
		{
			Add(_SpamPreventionUsers, new UserKey(user), user);
		}

		public void AddSlowmodeUser(SlowmodeUserInfo user)
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

		public async Task<CloseWords<HelpEntryHolder.HelpEntry>> GetOutActiveCloseHelp(IUser user)
		{
			//Should only ever have one for each user at a time.
			var kvp = _ActiveCloseHelp.SingleOrDefault(x => x.Key.UserId == user.Id);
			if (kvp.Key != null)
			{
				var value = Remove(_ActiveCloseHelp, kvp.Key);
				await MessageUtils.DeleteMessageAsync(value.Message, new ModerationReason("removing active close help")).CAF();
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
				await MessageUtils.DeleteMessageAsync(value.Message, new ModerationReason("removing active close quotes")).CAF();
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
		private IEnumerable<TValue> GetOutTimedObjects<TKey, TValue>(ConcurrentDictionary<TKey, TValue> concDic) where TKey : DictKey where TValue : IHasTime
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
		/// Remove old entries completely.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="concDic"></param>
		private void RemoveTimedObjects<TKey, TValue>(ConcurrentDictionary<TKey, TValue> concDic) where TKey : DictKey where TValue : IHasTime
		{
			var currentTicks = DateTime.UtcNow.Ticks;
			foreach (var kvp in concDic)
			{
				if (kvp.Key.Ticks < currentTicks)
				{
					Remove(concDic, kvp.Key);
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
