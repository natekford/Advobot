using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Punishments;
using Advobot.Classes.UserInformation;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Advobot.Services.Timers
{
	//I have absolutely no idea if this class works as intended under stress.
	public sealed class Timers : ITimersService
	{
		private readonly PunishmentRemover _PunishmentRemover;
		private readonly Timer _HourTimer = new Timer(60 * 60 * 1000);
		private readonly Timer _MinuteTimer = new Timer(60 * 1000);
		private readonly Timer _HalfSecondTimer = new Timer(1000 / 2);

		private readonly ConcurrentDictionary<UserKey, RemovablePunishment> _RemovablePunishments = new ConcurrentDictionary<UserKey, RemovablePunishment>();
		private readonly ConcurrentDictionary<ChannelKey, RemovableMessage> _RemovableMessages = new ConcurrentDictionary<ChannelKey, RemovableMessage>();
		private readonly ConcurrentDictionary<UserKey, CloseWords<HelpEntry>> _ActiveCloseHelp = new ConcurrentDictionary<UserKey, CloseWords<HelpEntry>>();
		private readonly ConcurrentDictionary<UserKey, CloseWords<Quote>> _ActiveCloseQuotes = new ConcurrentDictionary<UserKey, CloseWords<Quote>>();
		private readonly ConcurrentDictionary<UserKey, SpamPreventionUserInformation> _SpamPreventionUsers = new ConcurrentDictionary<UserKey, SpamPreventionUserInformation>();
		private readonly ConcurrentDictionary<UserKey, SlowmodeUserInformation> _SlowmodeUsers = new ConcurrentDictionary<UserKey, SlowmodeUserInformation>();

		public Timers(IServiceProvider provider)
		{
			_PunishmentRemover = new PunishmentRemover(this);

			_HourTimer.Elapsed += OnHourEvent;
			_HourTimer.Enabled = true;

			_MinuteTimer.Elapsed += OnMinuteEvent;
			_MinuteTimer.Enabled = true;

			_HalfSecondTimer.Elapsed += OnOneHalfSecondEvent;
			_HalfSecondTimer.Enabled = true;
		}

		private void OnHourEvent(object source, ElapsedEventArgs e)
		{
			Task.Run(() => ClearSpamPreventionUsers());
		}
		private void ClearSpamPreventionUsers()
		{
			_SpamPreventionUsers.Clear();
		}

		private void OnMinuteEvent(object source, ElapsedEventArgs e)
		{
			Task.Run(async () => { await RemovePunishments(); });
		}
		private async Task RemovePunishments()
		{
			foreach (var punishment in GetOutTimedObjects(_RemovablePunishments))
			{
				var punishmentType = punishment.PunishmentType;
				var reason = new AutomaticModerationReason($"automatic un{punishmentType.EnumName().FormatTitle().Replace(' ', '-')}");

				switch (punishmentType)
				{
					case PunishmentType.Ban:
					{
						await _PunishmentRemover.UnbanAsync(punishment.Guild, punishment.User.Id, reason);
						continue;
					}
					case PunishmentType.Deafen:
					{
						await _PunishmentRemover.UndeafenAsync(punishment.User as IGuildUser, reason);
						continue;
					}
					case PunishmentType.VoiceMute:
					{
						await _PunishmentRemover.UnvoicemuteAsync(punishment.User as IGuildUser, reason);
						continue;
					}
					case PunishmentType.RoleMute:
					{
						await _PunishmentRemover.UnrolemuteAsync(punishment.User as IGuildUser, punishment.Role, reason);
						continue;
					}
				}
			}
		}

		private void OnOneHalfSecondEvent(object source, ElapsedEventArgs e)
		{
			Task.Run(async () => { await DeleteTargettedMessages(); });
			Task.Run(() => RemoveActiveCloseHelp());
			Task.Run(() => RemoveActiveCloseQuotes());
			Task.Run(() => RemoveSlowmodeUsers());
		}
		private async Task DeleteTargettedMessages()
		{
			foreach (var group in GetOutTimedObjects(_RemovableMessages).GroupBy(x => x.Channel.Id))
			{
				var messages = group.SelectMany(x => x.Messages);
				if (messages.Count() == 1)
				{
					await MessageActions.DeleteMessage(messages.Single());
				}
				else
				{
					var channel = group.First().Channel;
					await MessageActions.DeleteMessages(channel, messages, new AutomaticModerationReason("automatic message deletion."));
				}
			}
		}
		private void RemoveActiveCloseHelp()
		{
			RemoveTimedObjects(_ActiveCloseHelp);
		}
		private void RemoveActiveCloseQuotes()
		{
			RemoveTimedObjects(_ActiveCloseQuotes);
		}
		private void RemoveSlowmodeUsers()
		{
			RemoveTimedObjects(_SlowmodeUsers);
		}

		//Adds
		public void AddRemovablePunishment(RemovablePunishment punishment)
		{
			Add(_RemovablePunishments, new UserKey(punishment.User as IGuildUser, punishment.GetTime().Ticks), punishment);
		}
		public void AddRemovableMessage(RemovableMessage message)
		{
			Add(_RemovableMessages, new ChannelKey(message.Channel, message.GetTime().Ticks), message);
		}
		public void AddActiveCloseHelp(CloseWords<HelpEntry> helpEntry)
		{
			//Remove all older ones; only one can be active at a given time.
			foreach (var kvp in _ActiveCloseHelp.Where(x => x.Key.UserId == helpEntry.User.Id))
			{
				Remove(_ActiveCloseHelp, kvp.Key);
			}

			Add(_ActiveCloseHelp, new UserKey(helpEntry.User as IGuildUser, helpEntry.GetTime().Ticks), helpEntry);
		}
		public void AddActiveCloseQuote(CloseWords<Quote> quote)
		{
			///Remove all older ones; only one can be active at a given time.
			foreach (var kvp in _ActiveCloseQuotes.Where(x => x.Key.UserId == quote.User.Id))
			{
				Remove(_ActiveCloseQuotes, kvp.Key);
			}

			Add(_ActiveCloseQuotes, new UserKey(quote.User as IGuildUser, quote.GetTime().Ticks), quote);
		}
		public void AddSpamPreventionUser(SpamPreventionUserInformation user)
		{
			Add(_SpamPreventionUsers, new UserKey(user.User as IGuildUser, user.GetTime().Ticks), user);
		}
		public void AddSlowmodeUser(SlowmodeUserInformation user)
		{
			Add(_SlowmodeUsers, new UserKey(user.User as IGuildUser, user.GetTime().Ticks), user);
		}

		//Removes
		public int RemovePunishments(ulong userId, PunishmentType punishment)
		{
			//Has to be made into a new list otherwise the concurrent modification also effects it.
			var punishments = _RemovablePunishments.Where(x => x.Key.UserId == userId && x.Value.PunishmentType == punishment).ToList();
			foreach (var kvp in punishments)
			{
				Remove(_RemovablePunishments, kvp.Key);
			}
			return punishments.Count();
		}
		public CloseWords<HelpEntry> GetOutActiveCloseHelp(IUser user)
		{
			//Should only ever have one for each user at a time.
			var kvp = _ActiveCloseHelp.SingleOrDefault(x => x.Key.UserId == user.Id);
			if (kvp.Equals(default))
			{
				return null;
			}

			return Remove(_ActiveCloseHelp, kvp.Key);
		}
		public CloseWords<Quote> GetOutActiveCloseQuote(IUser user)
		{
			//Should only ever have one for each user at a time.
			var kvp = _ActiveCloseQuotes.SingleOrDefault(x => x.Key.UserId == user.Id);
			if (kvp.Equals(default))
			{
				return null;
			}

			return Remove(_ActiveCloseQuotes, kvp.Key);
		}

		//Gets
		public SpamPreventionUserInformation GetSpamPreventionUser(IGuildUser user)
		{
			var kvp = _SpamPreventionUsers.SingleOrDefault(x => x.Key.GuildId == user.Guild.Id && x.Key.UserId == user.Id);
			if (kvp.Equals(default))
			{
				return null;
			}

			return kvp.Value;
		}
		public IEnumerable<SpamPreventionUserInformation> GetSpamPreventionUsers(IGuild guild)
		{
			return new List<SpamPreventionUserInformation>(_SpamPreventionUsers.Where(x => x.Key.GuildId == guild.Id).Select(x => x.Value));
		}
		public SlowmodeUserInformation GetSlowmodeUser(IGuildUser user)
		{
			var kvp = _SlowmodeUsers.SingleOrDefault(x => x.Key.GuildId == user.Guild.Id && x.Key.UserId == user.Id);
			if (kvp.Equals(default))
			{
				return null;
			}

			return kvp.Value;
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
		/// is the default value of its type.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="concDic"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		private void Add<TKey, TValue>(ConcurrentDictionary<TKey, TValue> concDic, TKey key, TValue value)
		{
			//Don't allow a null/default value to be set
			if (EqualityComparer<TValue>.Default.Equals(value, default))
			{
				return;
			}

			if (!concDic.TryAdd(key, value))
			{
				ConsoleActions.WriteLine($"Failed to add the object at {key} in {GetDictionaryTValueName(concDic)}.", color: ConsoleColor.Red);
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
		private TValue Remove<TKey, TValue>(ConcurrentDictionary<TKey, TValue> concDic, TKey key)
		{
			//Don't allow null/default keys to be used
			if (EqualityComparer<TKey>.Default.Equals(key, default))
			{
				return default;
			}

			if (!concDic.TryRemove(key, out var value))
			{
				ConsoleActions.WriteLine($"Failed to remove the object at {key} in {GetDictionaryTValueName(concDic)}.", color: ConsoleColor.Red);
			}
			return value;
		}
		/// <summary>
		/// Returns a readable name of the <paramref name="concDic"/> TValue.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="concDic"></param>
		/// <returns></returns>
		private string GetDictionaryTValueName<TKey, TValue>(ConcurrentDictionary<TKey, TValue> concDic)
		{
			var tValue = typeof(TValue);
			var tName = tValue.Name.Trim('1'); //Has `1 at the end of its name
			return (tValue.IsGenericType ? tName + tValue.GetGenericArguments()[0].Name : tName);
		}
	}
}
