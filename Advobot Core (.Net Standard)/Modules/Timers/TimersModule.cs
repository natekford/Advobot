using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Advobot.Modules.Timers
{
	public sealed class MyTimersModule : ITimersModule, IDisposable
	{
		private const long HOUR = 60 * 60 * 1000;
		private const long MINUTE = 60 * 1000;
		private const long ONE_HALF_SECOND = 500;

		private readonly Timer _HourTimer = new Timer(HOUR);
		private readonly Timer _MinuteTimer = new Timer(MINUTE);
		private readonly Timer _OneHalfSecondTimer = new Timer(ONE_HALF_SECOND);

		private readonly IGuildSettingsModule _GuildSettings;

		private readonly List<RemovablePunishment> _RemovablePunishments = new List<RemovablePunishment>();
		private readonly List<RemovableMessage> _RemovableMessages = new List<RemovableMessage>();
		private readonly List<CloseWords<HelpEntry>> _ActiveCloseHelp = new List<CloseWords<HelpEntry>>();
		private readonly List<CloseWords<Quote>> _ActiveCloseQuotes = new List<CloseWords<Quote>>();

		public MyTimersModule(IGuildSettingsModule guildSettings)
		{
			_GuildSettings = guildSettings;

			_HourTimer.Elapsed += OnHourEvent;
			_HourTimer.Enabled = true;

			_MinuteTimer.Elapsed += OnMinuteEvent;
			_MinuteTimer.Enabled = true;

			_OneHalfSecondTimer.Elapsed += OnOneHalfSecondEvent;
			_OneHalfSecondTimer.Enabled = true;
		}

		private void OnHourEvent(object source, ElapsedEventArgs e)
		{
			ClearPunishedUsersList();
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
			foreach (var punishment in _RemovablePunishments.GetOutTimedObjects())
			{
				switch (punishment.PunishmentType)
				{
					case PunishmentType.Ban:
					{
						await Punishments.Unban(punishment.Guild, punishment.UserId, GeneralFormatting.FormatBotReason("automatic unban."));
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
						await Punishments.Undeafen(guildUser, GeneralFormatting.FormatBotReason("automatic undeafen."));
						return;
					}
					case PunishmentType.VoiceMute:
					{
						await Punishments.VoiceUnmute(guildUser, GeneralFormatting.FormatBotReason("automatic voice unmute."));
						return;
					}
					case PunishmentType.RoleMute:
					{
						await Punishments.RoleUnmute(guildUser, punishment.Role, GeneralFormatting.FormatBotReason("automatic role unmute."));
						return;
					}
				}
			}
		}

		private void OnOneHalfSecondEvent(object source, ElapsedEventArgs e)
		{
			Task.Run(async () => { await DeleteTargettedMessages(); });
			RemoveActiveCloseHelpAndWords();
			ResetSlowModeUserMessages();
		}
		private async Task DeleteTargettedMessages()
		{
			foreach (var message in _RemovableMessages.GetOutTimedObjects())
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
			_ActiveCloseHelp.GetOutTimedObjects();
			_ActiveCloseQuotes.GetOutTimedObjects();
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
			_RemovablePunishments.ThreadSafeAddRange(punishments);
		}
		public void AddRemovableMessages(params RemovableMessage[] messages)
		{
			_RemovableMessages.ThreadSafeAddRange(messages);
		}
		public void AddActiveCloseHelp(params CloseWords<HelpEntry>[] help)
		{
			_ActiveCloseHelp.ThreadSafeAddRange(help);
		}
		public void AddActiveCloseQuotes(params CloseWords<Quote>[] quotes)
		{
			_ActiveCloseQuotes.ThreadSafeAddRange(quotes);
		}

		public void RemovePunishments(ulong userId, PunishmentType punishment)
		{
			_RemovablePunishments.Where(x => x.UserId == userId && x.PunishmentType == punishment);
		}

		public CloseWords<HelpEntry> GetOutActiveCloseHelp(ulong userId)
		{
			var help = _ActiveCloseHelp.FirstOrDefault(x => x.UserId == userId);
			_ActiveCloseHelp.ThreadSafeRemoveAll((x => x.UserId == userId));
			return help;
		}
		public CloseWords<Quote> GetOutActiveCloseQuote(ulong userId)
		{
			var quote = _ActiveCloseQuotes.FirstOrDefault(x => x.UserId == userId);
			_ActiveCloseQuotes.ThreadSafeRemoveAll(x => x.UserId == userId);
			return quote;
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
