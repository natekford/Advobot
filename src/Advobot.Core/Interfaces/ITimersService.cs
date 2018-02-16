using Advobot.Core.Classes;
using Advobot.Core.Classes.CloseWords;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Classes.UserInformation;
using Advobot.Core.Enums;
using Discord;
using System.Collections.Generic;
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
		Task AddAsync(SpamPreventionUserInfo user);
		Task AddAsync(SlowmodeUserInfo user);
		Task AddAsync(BannedPhraseUserInfo user);

		Task<RemovablePunishment> RemovePunishmentAsync(IGuild guild, ulong userId, Punishment punishment);
		Task<CloseHelpEntries> RemoveActiveCloseHelpAsync(IUser user);
		Task<CloseQuotes> RemoveActiveCloseQuoteAsync(IUser user);
		IEnumerable<SpamPreventionUserInfo> GetSpamPreventionUsers(IGuild guild);
		IEnumerable<SlowmodeUserInfo> GetSlowmodeUsers(IGuild guild);
		IEnumerable<BannedPhraseUserInfo> GetBannedPhraseUsers(IGuild guild);
		SpamPreventionUserInfo GetSpamPreventionUser(IGuildUser user);
		SlowmodeUserInfo GetSlowmodeUser(IGuildUser user);
		BannedPhraseUserInfo GetBannedPhraseUser(IGuildUser user);
	}
}
