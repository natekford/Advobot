﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Core.Classes;
using Advobot.Core.Classes.CloseWords;
using Advobot.Core.Classes.GuildSettings;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Classes.UserInformation;
using Advobot.Core.Enums;
using Discord;

namespace Advobot.Core.Interfaces
{
	/// <summary>
	/// Abstraction for a timers module. Handles timed punishments, close words, and slowmode.
	/// </summary>
	public interface ITimersService
	{
		Task Add(RemovablePunishment punishment);
		Task Add(IUser author, IUserMessage botMessage, CloseWords<HelpEntry> help);
		Task Add(IUser author, IUserMessage botMessage, CloseWords<Quote> quote);
		void Add(RemovableMessage message);
		void Add(TimedMessage message);
		void Add(SpamPreventionUserInfo user);
		void Add(SlowmodeUserInfo user);
		void Add(BannedPhraseUserInfo user);

		Task<RemovablePunishment> RemovePunishment(IGuild guild, ulong id, PunishmentType punishment);
		Task<CloseWords<HelpEntry>> RemoveActiveCloseHelp(IUser user);
		Task<CloseWords<Quote>> RemoveActiveCloseQuote(IUser user);
		IEnumerable<SpamPreventionUserInfo> GetSpamPreventionUsers(IGuild guild);
		IEnumerable<SlowmodeUserInfo> GetSlowmodeUsers(IGuild guild);
		IEnumerable<BannedPhraseUserInfo> GetBannedPhraseUsers(IGuild guild);
		SpamPreventionUserInfo GetSpamPreventionUser(IGuildUser user);
		SlowmodeUserInfo GetSlowmodeUser(IGuildUser user);
		BannedPhraseUserInfo GetBannedPhraseUser(IGuildUser user);
	}
}
