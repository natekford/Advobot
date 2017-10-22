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
		void AddRemovablePunishment(RemovablePunishment punishment);
		void AddRemovableMessage(RemovableMessage message);
		Task AddActiveCloseHelp(IGuildUser user, IUserMessage msg, CloseWords<HelpEntry> help);
		Task AddActiveCloseQuote(IGuildUser user, IUserMessage msg, CloseWords<Quote> quote);
		void AddSpamPreventionUser(SpamPreventionUserInformation user);
		void AddSlowmodeUser(SlowmodeUserInformation user);

		int RemovePunishments(ulong id, PunishmentType punishment);
		Task<CloseWords<HelpEntry>> GetOutActiveCloseHelp(IUser user);
		Task<CloseWords<Quote>> GetOutActiveCloseQuote(IUser user);

		SpamPreventionUserInformation GetSpamPreventionUser(IGuildUser user);
		IEnumerable<SpamPreventionUserInformation> GetSpamPreventionUsers(IGuild guild);
		SlowmodeUserInformation GetSlowmodeUser(IGuildUser user);
	}
}
