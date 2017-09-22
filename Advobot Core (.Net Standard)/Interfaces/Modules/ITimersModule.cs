using Advobot.Classes;
using Advobot.Enums;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for a timers module. Handles timed punishments, close words, and slowmode.
	/// </summary>
	public interface ITimersModule
	{
		void AddRemovablePunishments(params RemovablePunishment[] punishments);
		void AddRemovableMessages(params RemovableMessage[] messages);
		void AddActiveCloseHelp(params CloseWords<HelpEntry>[] help);
		void AddActiveCloseQuotes(params CloseWords<Quote>[] quotes);
		void RemovePunishments(ulong id, PunishmentType punishment);

		CloseWords<HelpEntry> GetOutActiveCloseHelp(ulong id);
		CloseWords<Quote> GetOutActiveCloseQuote(ulong id);
	}
}
