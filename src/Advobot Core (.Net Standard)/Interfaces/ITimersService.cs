using Advobot.Classes;
using Advobot.Classes.Punishments;
using Advobot.Classes.SpamPrevention;
using Advobot.Enums;
using Discord;
using System.Collections.Generic;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for a timers module. Handles timed punishments, close words, and slowmode.
	/// </summary>
	public interface ITimersService
	{
		void AddRemovablePunishment(RemovablePunishment punishment);
		void AddRemovableMessage(RemovableMessage message);
		void AddActiveCloseHelp(CloseWords<HelpEntry> help);
		void AddActiveCloseQuote(CloseWords<Quote> quote);
		void AddSpamPreventionUser(SpamPreventionUser user);

		int RemovePunishments(ulong id, PunishmentType punishment);
		CloseWords<HelpEntry> GetOutActiveCloseHelp(IUser user);
		CloseWords<Quote> GetOutActiveCloseQuote(IUser user);

		SpamPreventionUser GetSpamPreventionUser(IGuildUser user);
		IEnumerable<SpamPreventionUser> GetSpamPreventionUsers(IGuild guild);
	}
}
