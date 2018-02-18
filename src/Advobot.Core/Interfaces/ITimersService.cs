using Advobot.Core.Classes;
using Advobot.Core.Classes.CloseWords;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Enums;
using Discord;
using System.Threading.Tasks;

namespace Advobot.Core.Interfaces
{
	/// <summary>
	/// Abstraction for a timers module. Handles timed punishments, close words, and slowmode.
	/// </summary>
	public interface ITimersService
	{
		void Start();

		Task AddAsync(RemovablePunishment punishment);
		Task AddAsync(CloseHelpEntries help);
		Task AddAsync(CloseQuotes quote);
		Task AddAsync(RemovableMessage message);
		Task AddAsync(TimedMessage message);

		Task<RemovablePunishment> RemovePunishmentAsync(IGuild guild, ulong userId, Punishment punishment);
		Task<CloseHelpEntries> RemoveActiveCloseHelpAsync(IUser user);
		Task<CloseQuotes> RemoveActiveCloseQuoteAsync(IUser user);
	}
}
