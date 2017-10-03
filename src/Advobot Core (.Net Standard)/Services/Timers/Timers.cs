using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Punishments;
using Advobot.Enums;
using Advobot.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.Concurrent;
using Advobot.Classes.SpamPrevention;
using Discord;

namespace Advobot.Services.Timers
{
	//I have absolutely no idea if this class works as intended under stress.
	public sealed class Timers : ITimersService
	{
		private readonly Timer _HourTimer = new Timer(60 * 60 * 1000);
		private readonly Timer _MinuteTimer = new Timer(60 * 1000);
		private readonly Timer _HalfSecondTimer = new Timer(1000 / 2);

		private readonly ConcurrentDictionary<TimeKey, RemovablePunishment> _RemovablePunishments = new ConcurrentDictionary<TimeKey, RemovablePunishment>();
		private readonly ConcurrentDictionary<TimeKey, RemovableMessage> _RemovableMessages = new ConcurrentDictionary<TimeKey, RemovableMessage>();
		private readonly ConcurrentDictionary<TimeKey, CloseWords<HelpEntry>> _ActiveCloseHelp = new ConcurrentDictionary<TimeKey, CloseWords<HelpEntry>>();
		private readonly ConcurrentDictionary<TimeKey, CloseWords<Quote>> _ActiveCloseQuotes = new ConcurrentDictionary<TimeKey, CloseWords<Quote>>();
		private readonly ConcurrentDictionary<UserKey, SpamPreventionUser> _SpamPreventionUsers = new ConcurrentDictionary<UserKey, SpamPreventionUser>();

		private readonly IGuildSettingsService _GuildSettings;
		private readonly PunishmentRemover _PunishmentRemover;

		public Timers(IServiceProvider provider)
		{
			_GuildSettings = provider.GetService<IGuildSettingsService>();
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
						await _PunishmentRemover.UnbanAsync(punishment.Guild, punishment.UserId, reason);
						continue;
					}
				}

				var guildUser = await punishment.Guild.GetUserAsync(punishment.UserId);
				if (guildUser == null)
				{
					continue;
				}

				switch (punishmentType)
				{
					case PunishmentType.Deafen:
					{
						await _PunishmentRemover.UndeafenAsync(guildUser, reason);
						continue;
					}
					case PunishmentType.VoiceMute:
					{
						await _PunishmentRemover.UnvoicemuteAsync(guildUser, reason);
						continue;
					}
					case PunishmentType.RoleMute:
					{
						await _PunishmentRemover.UnrolemuteAsync(guildUser, punishment.Role, reason);
						continue;
					}
				}
			}
		}

		private void OnOneHalfSecondEvent(object source, ElapsedEventArgs e)
		{
			Task.Run(async () => { await DeleteTargettedMessages(); });
			Task.Run(() => RemoveActiveCloseHelpAndWords());
			Task.Run(() => ResetSlowModeUserMessages());
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
		private void RemoveActiveCloseHelpAndWords()
		{
			RemoveTimedObjects(_ActiveCloseHelp);
			RemoveTimedObjects(_ActiveCloseQuotes);
		}
		private void ResetSlowModeUserMessages()
		{
			foreach (var slowmode in _GuildSettings.GetAllSettings().Where(x => x.Slowmode?.Enabled ?? false).Select(x => x.Slowmode))
			{
				slowmode.ResetUsers();
			}
		}

		//Adds
		public void AddRemovablePunishment(RemovablePunishment punishment)
		{
			Add(_RemovablePunishments, new TimeKey(punishment.UserId, punishment.GetTime().Ticks), punishment);
		}
		public void AddRemovableMessage(RemovableMessage message)
		{
			Add(_RemovableMessages, new TimeKey(message.Channel.Id, message.GetTime().Ticks), message);
		}
		public void AddActiveCloseHelp(CloseWords<HelpEntry> helpEntry)
		{
			//Remove all older ones; only one can be active at a given time.
			foreach (var kvp in _ActiveCloseHelp.Where(x => x.Key.Id == helpEntry.UserId))
			{
				Remove(_ActiveCloseHelp, kvp.Key);
			}

			Add(_ActiveCloseHelp, new TimeKey(helpEntry.UserId, helpEntry.GetTime().Ticks), helpEntry);
		}
		public void AddActiveCloseQuote(CloseWords<Quote> quote)
		{
			///Remove all older ones; only one can be active at a given time.
			foreach (var kvp in _ActiveCloseQuotes.Where(x => x.Key.Id == quote.UserId))
			{
				Remove(_ActiveCloseQuotes, kvp.Key);
			}

			Add(_ActiveCloseQuotes, new TimeKey(quote.UserId, quote.GetTime().Ticks), quote);
		}
		public void AddSpamPreventionUser(SpamPreventionUser user)
		{
			Add(_SpamPreventionUsers, new UserKey(user.User.GuildId, user.User.Id), user);
		}

		//Removes
		public int RemovePunishments(ulong userId, PunishmentType punishment)
		{
			//Has to be made into a new list otherwise the concurrent modification also effects it.
			var punishments = _RemovablePunishments.Where(x => x.Key.Id == userId && x.Value.PunishmentType == punishment).ToList();
			foreach (var kvp in punishments)
			{
				Remove(_RemovablePunishments, kvp.Key);
			}
			return punishments.Count();
		}
		public CloseWords<HelpEntry> GetOutActiveCloseHelp(IUser user)
		{
			//Should only ever have one for each user at a time.
			var kvp = _ActiveCloseHelp.SingleOrDefault(x => x.Key.Id == user.Id);
			if (kvp.Equals(default))
			{
				return null;
			}

			return Remove(_ActiveCloseHelp, kvp.Key);
		}
		public CloseWords<Quote> GetOutActiveCloseQuote(IUser user)
		{
			//Should only ever have one for each user at a time.
			var kvp = _ActiveCloseQuotes.SingleOrDefault(x => x.Key.Id == user.Id);
			if (kvp.Equals(default))
			{
				return null;
			}

			return Remove(_ActiveCloseQuotes, kvp.Key);
		}

		//Gets
		public SpamPreventionUser GetSpamPreventionUser(IGuildUser user)
		{
			var kvp = _SpamPreventionUsers.SingleOrDefault(x => x.Key.GuildId == user.Guild.Id && x.Key.UserId == user.Id);
			if (kvp.Equals(default))
			{
				return null;
			}

			return kvp.Value;
		}
		public IEnumerable<SpamPreventionUser> GetSpamPreventionUsers(IGuild guild)
		{
			return new List<SpamPreventionUser>(_SpamPreventionUsers.Where(x => x.Key.GuildId == guild.Id).Select(x => x.Value));
		}

		/// <summary>
		/// Remove old entries then do something with them.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="concDic"></param>
		/// <returns></returns>
		private IEnumerable<T> GetOutTimedObjects<T>(ConcurrentDictionary<TimeKey, T> concDic) where T : IHasTime
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
		/// <typeparam name="T"></typeparam>
		/// <param name="concDic"></param>
		private void RemoveTimedObjects<T>(ConcurrentDictionary<TimeKey, T> concDic) where T : IHasTime
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
		/// <typeparam name="T"></typeparam>
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
		/// <typeparam name="T"></typeparam>
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
		/// <typeparam name="T"></typeparam>
		/// <param name="concDic"></param>
		/// <returns></returns>
		private string GetDictionaryTValueName<TKey, TValue>(ConcurrentDictionary<TKey, TValue> concDic)
		{
			var tValue = typeof(TValue);
			var tName = tValue.Name.Trim('1'); //Has `1 at the end of its name
			return (tValue.IsGenericType ? tName + tValue.GetGenericArguments()[0].Name : tName);
		}

		private struct TimeKey
		{
			/// <summary>
			/// An identifying key. Can be a channel id, a user id, etc.
			/// </summary>
			public readonly ulong Id;
			/// <summary>
			/// The time at which to remove the entry.
			/// </summary>
			public readonly long Ticks;

			public TimeKey(ulong id, long ticks)
			{
				Id = id;
				Ticks = ticks;
			}

			public override string ToString()
			{
				return $"{Id}:{Ticks}";
			}
		}

		private struct UserKey
		{
			/// <summary>
			/// The guild a user belongs to.
			/// </summary>
			public readonly ulong GuildId;
			/// <summary>
			/// The user's id.
			/// </summary>
			public readonly ulong UserId;

			public UserKey(ulong guildId, ulong userId)
			{
				GuildId = guildId;
				UserId = userId;
			}

			public override string ToString()
			{
				return $"{GuildId}:{UserId}";
			}
		}
	}
}
