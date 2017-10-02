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

namespace Advobot.Services.Timers
{
	//I have absolutely no idea if this class works as intended under stress.
	public sealed class Timers : ITimersService
	{
		private readonly Timer _HourTimer = new Timer(60 * 60 * 1000);
		private readonly Timer _MinuteTimer	= new Timer(60 * 1000);
		private readonly Timer _HalfSecondTimer	= new Timer(1000 / 2);

		private readonly ConcurrentDictionary<KeyForDict, RemovablePunishment>	_RemovablePunishments = new ConcurrentDictionary<KeyForDict, RemovablePunishment>();
		private readonly ConcurrentDictionary<KeyForDict, RemovableMessage> _RemovableMessages = new ConcurrentDictionary<KeyForDict, RemovableMessage>();
		private readonly ConcurrentDictionary<KeyForDict, CloseWords<HelpEntry>> _ActiveCloseHelp = new ConcurrentDictionary<KeyForDict, CloseWords<HelpEntry>>();
		private readonly ConcurrentDictionary<KeyForDict, CloseWords<Quote>> _ActiveCloseQuotes = new ConcurrentDictionary<KeyForDict, CloseWords<Quote>>();

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
			Task.Run(() => ClearPunishedUsersList());
		}
		private void ClearPunishedUsersList()
		{
			foreach (var guildSettings in _GuildSettings.GetAllSettings())
			{
				guildSettings.SpamPreventionUsers.Clear();
			}
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

		public void AddRemovablePunishment(RemovablePunishment punishment)
		{
			Add(_RemovablePunishments, new KeyForDict(punishment.UserId, punishment.GetTime().Ticks), punishment);
		}
		public void AddRemovableMessage(RemovableMessage message)
		{
			Add(_RemovableMessages, new KeyForDict(message.Channel.Id, message.GetTime().Ticks), message);
		}
		public void AddActiveCloseHelp(CloseWords<HelpEntry> helpEntry)
		{
			//Remove all older ones; only one can be active at a given time.
			foreach (var kvp in _ActiveCloseHelp.Where(x => x.Key.Id == helpEntry.UserId))
			{
				Remove(_ActiveCloseHelp, kvp.Key);
			}

			Add(_ActiveCloseHelp, new KeyForDict(helpEntry.UserId, helpEntry.GetTime().Ticks), helpEntry);
		}
		public void AddActiveCloseQuote(CloseWords<Quote> quote)
		{
			///Remove all older ones; only one can be active at a given time.
			foreach (var kvp in _ActiveCloseQuotes.Where(x => x.Key.Id == quote.UserId))
			{
				Remove(_ActiveCloseQuotes, kvp.Key);
			}

			Add(_ActiveCloseQuotes, new KeyForDict(quote.UserId, quote.GetTime().Ticks), quote);
		}

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
		public CloseWords<HelpEntry> GetOutActiveCloseHelp(ulong userId)
		{
			//Should only ever have one for each user at a time.
			var kvp = _ActiveCloseHelp.SingleOrDefault(x => x.Key.Id == userId);
			if (kvp.Equals(default))
			{
				return null;
			}

			return Remove(_ActiveCloseHelp, kvp.Key);
		}
		public CloseWords<Quote> GetOutActiveCloseQuote(ulong userId)
		{
			//Should only ever have one for each user at a time.
			var kvp = _ActiveCloseQuotes.SingleOrDefault(x => x.Key.Id == userId);
			if (kvp.Equals(default))
			{
				return null;
			}

			return Remove(_ActiveCloseQuotes, kvp.Key);
		}

		/// <summary>
		/// Remove old entries then do something with them.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="concDic"></param>
		/// <returns></returns>
		private IEnumerable<T> GetOutTimedObjects<T>(ConcurrentDictionary<KeyForDict, T> concDic) where T : IHasTime
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
		private void RemoveTimedObjects<T>(ConcurrentDictionary<KeyForDict, T> concDic) where T : IHasTime
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
		private void Add<T>(ConcurrentDictionary<KeyForDict, T> concDic, KeyForDict key, T value) where T : IHasTime
		{
			//Don't allow a null/default value to be set
			if (EqualityComparer<T>.Default.Equals(value, default))
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
		private T Remove<T>(ConcurrentDictionary<KeyForDict, T> concDic, KeyForDict key) where T : IHasTime
		{
			//Don't allow null/default keys to be used
			if (EqualityComparer<KeyForDict>.Default.Equals(key, default))
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
		private string GetDictionaryTValueName<T>(ConcurrentDictionary<KeyForDict, T> concDic)
		{
			var tValue = typeof(T);
			var tName = tValue.Name.Trim('1'); //Has `1 at the end of its name
			return (tValue.IsGenericType ? tName + tValue.GetGenericArguments()[0].Name : tName);
		}

		private struct KeyForDict
		{
			public readonly ulong Id;
			public readonly long Ticks;

			public KeyForDict(ulong id, long ticks)
			{
				Id = id;
				Ticks = ticks;
			}

			public override string ToString()
			{
				return $"{Id}:{Ticks}";
			}
		}
	}
}
