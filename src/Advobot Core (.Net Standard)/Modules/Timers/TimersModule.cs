﻿using Advobot.Actions;
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

namespace Advobot.Modules.Timers
{
	//I have absolutely no idea if this class works as intended under stress.
	public sealed class Timers : ITimersModule, IDisposable
	{
		private const long HOUR						= 60 * 60 * 1000;
		private const long MINUTE					= 60 * 1000;
		private const long ONE_HALF_SECOND			= 500;

		private readonly Timer _HourTimer			= new Timer(HOUR);
		private readonly Timer _MinuteTimer			= new Timer(MINUTE);
		private readonly Timer _OneHalfSecondTimer	= new Timer(ONE_HALF_SECOND);

		private readonly ConcurrentDictionary<ulong, List<RemovablePunishment>> _RemovablePunishments	= new ConcurrentDictionary<ulong, List<RemovablePunishment>>();
		private readonly ConcurrentDictionary<ulong, List<RemovableMessage>> _RemovableMessages			= new ConcurrentDictionary<ulong, List<RemovableMessage>>();
		private readonly ConcurrentDictionary<ulong, List<CloseWords<HelpEntry>>> _ActiveCloseHelp		= new ConcurrentDictionary<ulong, List<CloseWords<HelpEntry>>>();
		private readonly ConcurrentDictionary<ulong, List<CloseWords<Quote>>> _ActiveCloseQuotes		= new ConcurrentDictionary<ulong, List<CloseWords<Quote>>>();

		private readonly IGuildSettingsModule _GuildSettings;
		private readonly PunishmentRemover _PunishmentRemover;

		public Timers(IServiceProvider provider)
		{
			_GuildSettings = provider.GetService<IGuildSettingsModule>();
			_PunishmentRemover = new PunishmentRemover(this);

			_HourTimer.Elapsed += OnHourEvent;
			_HourTimer.Enabled = true;

			_MinuteTimer.Elapsed += OnMinuteEvent;
			_MinuteTimer.Enabled = true;

			_OneHalfSecondTimer.Elapsed += OnOneHalfSecondEvent;
			_OneHalfSecondTimer.Enabled = true;
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
				var reason = GeneralFormatting.FormatBotReason($"automatic un{punishment.PunishmentType.EnumName().FormatTitle()}");
				switch (punishment.PunishmentType)
				{
					case PunishmentType.Ban:
					{
						await _PunishmentRemover.UnbanAsync(punishment.Guild, punishment.UserId, reason);
						return;
					}
				}

				var guildUser = await punishment.Guild.GetUserAsync(punishment.UserId);
				if (guildUser == null)
				{
					return;
				}

				switch (punishment.PunishmentType)
				{
					case PunishmentType.Deafen:
					{
						await _PunishmentRemover.UndeafenAsync(guildUser, reason);
						return;
					}
					case PunishmentType.VoiceMute:
					{
						await _PunishmentRemover.VoiceUnmuteAsync(guildUser, reason);
						return;
					}
					case PunishmentType.RoleMute:
					{
						await _PunishmentRemover.RoleUnmuteAsync(guildUser, punishment.Role, reason);
						return;
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
			foreach (var message in GetOutTimedObjects(_RemovableMessages))
			{
				if (message.Messages.Count() == 1)
				{
					await MessageActions.DeleteMessage(message.Messages.FirstOrDefault());
				}
				else
				{
					await MessageActions.DeleteMessages(message.Channel, message.Messages, GeneralFormatting.FormatBotReason("automatic message deletion."));
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
			foreach (var slowmode in _GuildSettings.GetAllSettings().Where(x => x.Slowmode != null && x.Slowmode.Enabled).Select(x => x.Slowmode))
			{
				slowmode.ResetUsers();
			}
		}

		public void AddRemovablePunishments(params RemovablePunishment[] punishments)
		{
			foreach (var group in punishments.GroupBy(x => x.UserId))
			{
				AddObjects(_RemovablePunishments, group.Key, group);
			}
		}
		public void AddRemovableMessages(params RemovableMessage[] messages)
		{
			foreach (var group in messages.GroupBy(x => x.Channel.Id))
			{
				AddObjects(_RemovableMessages, group.Key, group);
			}
		}
		public void AddActiveCloseHelp(params CloseWords<HelpEntry>[] helpEntries)
		{
			foreach (var group in helpEntries.GroupBy(x => x.UserId))
			{
				AddObjects(_ActiveCloseHelp, group.Key, group);
			}
		}
		public void AddActiveCloseQuotes(params CloseWords<Quote>[] quotes)
		{
			foreach (var group in quotes.GroupBy(x => x.UserId))
			{
				AddObjects(_ActiveCloseQuotes, group.Key, group);
			}
		}

		public int RemovePunishments(ulong userId, PunishmentType punishment)
		{
			if (!_RemovablePunishments.TryGetValue(userId, out var value))
			{
				return 0;
			}

			lock (value)
			{
				return value.RemoveAll(x => x.PunishmentType == punishment);
			}
		}
		public CloseWords<HelpEntry> GetOutActiveCloseHelp(ulong userId)
		{
			return _ActiveCloseHelp.TryRemove(userId, out var removed) ? removed?.FirstOrDefault() : null;
		}
		public CloseWords<Quote> GetOutActiveCloseQuote(ulong userId)
		{
			return _ActiveCloseQuotes.TryRemove(userId, out var removed) ? removed?.FirstOrDefault() : null;
		}

		private void AddObjects<T>(ConcurrentDictionary<ulong, List<T>> concDic, ulong key, IEnumerable<T> addVal)
		{
			//I don't know if this is fully thread safe.
			if (!concDic.TryGetValue(key, out var value))
			{
				value = new List<T>();
				concDic.AddOrUpdate(key, value, (oldKey, oldVal) => value);
			}

			lock (value)
			{
				value.AddRange(addVal);
			}
		}
		private IEnumerable<T> GetOutTimedObjects<T>(ConcurrentDictionary<ulong, List<T>> concDic) where T : IHasTime
		{
			//I don't know exactly what the yield syntax does but I think it's applicable here.
			foreach (var list in concDic.Values)
			{
				foreach (var obj in list.Where(x => x.GetTime() < DateTime.UtcNow).ToList())
				{
					list.Remove(obj);
					yield return obj;
				}
			}
		}
		private void RemoveTimedObjects<T>(ConcurrentDictionary<ulong, List<T>> concDic) where T : IHasTime
		{
			foreach (var list in concDic.Values)
			{
				foreach (var obj in list.Where(x => x.GetTime() < DateTime.UtcNow).ToList())
				{
					list.Remove(obj);
				}
			}
		}

		public void Dispose()
		{
			_HourTimer.Dispose();
			_MinuteTimer.Dispose();
			_OneHalfSecondTimer.Dispose();

			_RemovablePunishments.Clear();
			_RemovableMessages.Clear();
			_ActiveCloseHelp.Clear();
			_ActiveCloseQuotes.Clear();
		}
	}
}