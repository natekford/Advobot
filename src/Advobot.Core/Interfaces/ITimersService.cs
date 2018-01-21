using Advobot.Core.Classes;
using Advobot.Core.Classes.CloseWords;
using Advobot.Core.Classes.GuildSettings;
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
		void Add(RemovablePunishment punishment);
		void Add(RemovableMessage message);
		Task Add(IGuildUser author, IUserMessage botMessage, CloseWords<HelpEntry> help);
		Task Add(IGuildUser author, IUserMessage botMessage, CloseWords<Quote> quote);
		void Add(SpamPreventionUserInfo user);
		void Add(SlowmodeUserInfo user);
		void Add(TimedMessage message);

		int RemovePunishments(ulong id, PunishmentType punishment);
		Task<CloseWords<HelpEntry>> GetOutActiveCloseHelp(IGuildUser user);
		Task<CloseWords<Quote>> GetOutActiveCloseQuote(IGuildUser user);
		SpamPreventionUserInfo GetSpamPreventionUser(IGuildUser user);
		IEnumerable<SpamPreventionUserInfo> GetSpamPreventionUsers(IGuild guild);
		SlowmodeUserInfo GetSlowmodeUser(IGuildUser user);
	}
}
