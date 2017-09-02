using Advobot.Actions;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.NonSavedClasses;
using Advobot.RemovablePunishments;
using Advobot.SavedClasses;
using Advobot.Structs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System;

namespace Advobot
{
	namespace Timers
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
			private readonly List<ActiveCloseWord<HelpEntry>> _ActiveCloseHelp = new List<ActiveCloseWord<HelpEntry>>();
			private readonly List<ActiveCloseWord<Quote>> _ActiveCloseQuotes = new List<ActiveCloseWord<Quote>>();
			private readonly List<SlowmodeUser> _SlowmodeUsers = new List<SlowmodeUser>();

			public MyTimersModule(IServiceProvider provider)
			{
				_GuildSettings = (IGuildSettingsModule)provider.GetService(typeof(IGuildSettingsModule));

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
							await PunishmentActions.AutomaticUnbanUser(punishment.Guild, punishment.UserId);
							return;
						}
					}

					var guildUser = await punishment.Guild.GetUserAsync(punishment.UserId);
					if (guildUser == null)
						return;

					switch (punishment.PunishmentType)
					{
						case PunishmentType.Deafen:
						{
							await PunishmentActions.AutomaticUndeafenUser(guildUser);
							return;
						}
						case PunishmentType.VoiceMute:
						{
							await PunishmentActions.AutomaticVoiceUnmuteUser(guildUser);
							return;
						}
						case PunishmentType.RoleMute:
						{
							await PunishmentActions.AutomaticRoleUnmuteUser(guildUser, (punishment as RemovableRoleMute)?.Role);
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
						await MessageActions.DeleteMessages(message.Channel, message.Messages, FormattingActions.FormatBotReason("automatic message deletion."));
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
				foreach (var slowModeUser in _SlowmodeUsers.GetOutTimedObjects())
				{
					slowModeUser.ResetMessagesLeft();
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
			public void AddActiveCloseHelp(params ActiveCloseWord<HelpEntry>[] help)
			{
				_ActiveCloseHelp.ThreadSafeAddRange(help);
			}
			public void AddActiveCloseQuotes(params ActiveCloseWord<Quote>[] quotes)
			{
				_ActiveCloseQuotes.ThreadSafeAddRange(quotes);
			}
			public void AddSlowModeUsers(params SlowmodeUser[] users)
			{
				_SlowmodeUsers.ThreadSafeAddRange(users);
			}

			public void RemovePunishments(ulong userId, PunishmentType punishment)
			{
				_RemovablePunishments.Where(x => x.UserId == userId && x.PunishmentType == punishment);
			}

			public ActiveCloseWord<HelpEntry> GetOutActiveCloseHelp(ulong userId)
			{
				var help = _ActiveCloseHelp.FirstOrDefault(x => x.UserId == userId);
				_ActiveCloseHelp.ThreadSafeRemoveAll((x => x.UserId == userId));
				return help;
			}
			public ActiveCloseWord<Quote> GetOutActiveCloseQuote(ulong userId)
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
				_SlowmodeUsers.Clear();
			}
		}
	}
}
