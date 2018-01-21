using Advobot.Core.Classes;
using Advobot.Core.Classes.CloseWords;
using Advobot.Core.Classes.GuildSettings;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Classes.UserInformation;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
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

		private ConcurrentDictionary<UserKey, RemovablePunishment> _RemovablePunishments = new ConcurrentDictionary<UserKey, RemovablePunishment>();
		private ConcurrentDictionary<ChannelKey, RemovableMessage> _RemovableMessages = new ConcurrentDictionary<ChannelKey, RemovableMessage>();
		private ConcurrentDictionary<UserKey, CloseWordsWrapper<HelpEntry>> _ActiveCloseHelp = new ConcurrentDictionary<UserKey, CloseWordsWrapper<HelpEntry>>();
		private ConcurrentDictionary<UserKey, CloseWordsWrapper<Quote>> _ActiveCloseQuotes = new ConcurrentDictionary<UserKey, CloseWordsWrapper<Quote>>();
		private ConcurrentDictionary<UserKey, SpamPreventionUserInfo> _SpamPreventionUsers = new ConcurrentDictionary<UserKey, SpamPreventionUserInfo>();
		private ConcurrentDictionary<UserKey, SlowmodeUserInfo> _SlowmodeUsers = new ConcurrentDictionary<UserKey, SlowmodeUserInfo>();
		private ConcurrentDictionary<UserKey, TimedMessage> _TimedMessages = new ConcurrentDictionary<UserKey, TimedMessage>();

		public TimersSservice(IServiceProvider provider)
		{
			_PunishmentRemover = new PunishmentRemover(this);

			_HourTimer.Elapsed += (sender, e) =>
			{
				Task.Run(() => HandleSpamPreventionUsers());
			};
			_HourTimer.Enabled = true;

			_MinuteTimer.Elapsed += (sender, e) =>
			{
				Task.Run(async () => await HandlePunishmentsAsync().CAF());
				Task.Run(async () => await HandleTimedMessages().CAF());
			};
			_MinuteTimer.Enabled = true;

			_SecondTimer.Elapsed += (sender, e) =>
			{
				Task.Run(async () => await HandleRemovableMessages().CAF());
				Task.Run(async () => await HandleActiveCloseHelp().CAF());
				Task.Run(async () => await HandleActiveCloseQuotes().CAF());
				Task.Run(() => HandleSlowmodeUsers());
			};
			_SecondTimer.Enabled = true;
		}

		private async Task HandlePunishmentsAsync()
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
		private async Task HandleRemovableMessages()
		{
			foreach (var group in GetOutTimedObjects(_RemovableMessages).Where(x => x.Channel != null && x.Messages != null).GroupBy(x => x.Channel.Id))
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
		private async Task HandleActiveCloseHelp()
		{
			foreach (var helpEntries in GetOutTimedObjects(_ActiveCloseHelp))
			{
				await MessageUtils.DeleteMessageAsync(helpEntries.Message, _CloseHelpReason).CAF();
			}
		}
		private async Task HandleActiveCloseQuotes()
		{
			foreach (var quotes in GetOutTimedObjects(_ActiveCloseQuotes))
			{
				await MessageUtils.DeleteMessageAsync(quotes.Message, _CloseQuotesReason).CAF();
			}
		}
		private void HandleSpamPreventionUsers()
		{
			_SpamPreventionUsers.Clear();
		}
		private void HandleSlowmodeUsers()
		{
			GetOutTimedObjects(_SlowmodeUsers);
		}
		private async Task HandleTimedMessages()
		{
			foreach (var timedMessage in GetOutTimedObjects(_TimedMessages))
			{
				await timedMessage.SendAsync().CAF();
			}
		}

		public void Add(RemovablePunishment punishment)
		{
			Add(_RemovablePunishments, new UserKey(punishment.Guild, punishment.User, punishment.Time.Ticks), punishment);
		}
		public void Add(RemovableMessage message)
		{
			Add(_RemovableMessages, new ChannelKey(message.Channel, message.Time.Ticks), message);
		}
		public async Task Add(IGuildUser author, IUserMessage botMessage, CloseWords<HelpEntry> helpEntries)
		{
			//Remove all older ones; only one can be active at a given time.
			foreach (var kvp in _ActiveCloseHelp.Where(x => x.Key.UserId == author.Id))
			{
				await MessageUtils.DeleteMessageAsync(Remove(_ActiveCloseHelp, kvp.Key).Message, _CloseHelpReason).CAF();
			}
			Add(_ActiveCloseHelp, new UserKey(author, helpEntries.Time.Ticks), new CloseWordsWrapper<HelpEntry>(helpEntries, botMessage));
		}
		public async Task Add(IGuildUser author, IUserMessage botMessage, CloseWords<Quote> quotes)
		{
			///Remove all older ones; only one can be active at a given time.
			foreach (var kvp in _ActiveCloseQuotes.Where(x => x.Key.UserId == author.Id))
			{
				await MessageUtils.DeleteMessageAsync(Remove(_ActiveCloseQuotes, kvp.Key).Message, _CloseQuotesReason).CAF();
			}
			Add(_ActiveCloseQuotes, new UserKey(author, quotes.Time.Ticks), new CloseWordsWrapper<Quote>(quotes, botMessage));
		}
		public void Add(SpamPreventionUserInfo user)
		{
			Add(_SpamPreventionUsers, new UserKey(user), user);
		}
		public void Add(SlowmodeUserInfo user)
		{
			Add(_SlowmodeUsers, new UserKey(user), user);
		}
		public void Add(TimedMessage message)
		{
			foreach (var kvp in _TimedMessages.Where(x => x.Key.UserId == message.Author.Id))
			{
				Remove(_TimedMessages, kvp.Key);
			}
			Add(_TimedMessages, new UserKey(message.Author, message.Time.Ticks), message);
		}

		public int RemovePunishments(ulong userId, PunishmentType punishment)
		{
			//Has to be made into a new list otherwise the concurrent modification also effects it.
			var kvps = _RemovablePunishments.ToList().Where(x => x.Key.UserId == userId && x.Value.PunishmentType == punishment);
			foreach (var kvp in kvps)
			{
				Remove(_RemovablePunishments, kvp.Key);
			}
			return kvps.Count();
		}
		public async Task<CloseWords<HelpEntry>> GetOutActiveCloseHelp(IGuildUser user)
		{
			//Should only ever have one for each user at a time.
			var kvp = _ActiveCloseHelp.ToList().FirstOrDefault(x => x.Key.GuildId == user.GuildId && x.Key.UserId == user.Id);
			if (kvp.Key != null)
			{
				var value = Remove(_ActiveCloseHelp, kvp.Key);
				await MessageUtils.DeleteMessageAsync(value.Message, _CloseHelpReason).CAF();
				return value.CloseWords;
			}
			return null;
		}
		public async Task<CloseWords<Quote>> GetOutActiveCloseQuote(IGuildUser user)
		{
			//Should only ever have one for each user at a time.
			var kvp = _ActiveCloseQuotes.ToList().FirstOrDefault(x => x.Key.GuildId == user.GuildId && x.Key.UserId == user.Id);
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
			var kvp = _SpamPreventionUsers.ToList().FirstOrDefault(x => x.Key.GuildId == user.GuildId && x.Key.UserId == user.Id);
			return kvp.Equals(default) ? null : kvp.Value;
		}
		public IEnumerable<SpamPreventionUserInfo> GetSpamPreventionUsers(IGuild guild)
		{
			return _SpamPreventionUsers.Where(x => x.Key.GuildId == guild.Id).Select(x => x.Value).ToList();
		}
		public SlowmodeUserInfo GetSlowmodeUser(IGuildUser user)
		{
			var kvp = _SlowmodeUsers.ToList().FirstOrDefault(x => x.Key.GuildId == user.GuildId && x.Key.UserId == user.Id);
			return kvp.Equals(default) ? null : kvp.Value;
		}

		/// <summary>
		/// Remove old entries then do something with them.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dict"></param>
		/// <returns></returns>
		private IEnumerable<TValue> GetOutTimedObjects<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict) where TKey : DictKey where TValue : ITime
		{
			var currentTicks = DateTime.UtcNow.Ticks;
			foreach (var kvp in dict)
			{
				if (kvp.Key.Ticks < currentTicks)
				{
					yield return Remove(dict, kvp.Key);
				}
			}
		}
		/// <summary>
		/// Adds <paramref name="value"/> to <paramref name="dict"/> with the given <paramref name="key"/> unless <paramref name="value"/>
		/// or <paramref name="key"/> are the default values of their type.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		private void Add<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict, TKey key, TValue value) where TKey : DictKey
		{
			if (EqualityComparer<TKey>.Default.Equals(key, default) || EqualityComparer<TValue>.Default.Equals(value, default) || !dict.TryAdd(key, value))
			{
				ConsoleUtils.WriteLine($"Failed to add the object at {key} in {typeof(TValue).Name}.", color: ConsoleColor.Red);
			}
		}
		/// <summary>
		/// Removes whatever value in <paramref name="dict"/> has the key <paramref name="key"/>.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		private TValue Remove<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict, TKey key) where TKey : DictKey
		{
			TValue value = default;
			if (EqualityComparer<TKey>.Default.Equals(key, default) || !dict.TryRemove(key, out value))
			{
				ConsoleUtils.WriteLine($"Failed to remove the object at {key} in {typeof(TValue).Name}.", color: ConsoleColor.Red);
			}
			return value;
		}
	}
}
