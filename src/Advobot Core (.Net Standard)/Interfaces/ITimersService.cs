using Advobot.Classes;
using Advobot.Classes.Punishments;
using Advobot.Enums;

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

		int RemovePunishments(ulong id, PunishmentType punishment);
		CloseWords<HelpEntry> GetOutActiveCloseHelp(ulong id);
		CloseWords<Quote> GetOutActiveCloseQuote(ulong id);
	}
}
