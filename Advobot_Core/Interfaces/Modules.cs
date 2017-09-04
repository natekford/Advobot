using Advobot.Enums;
using Advobot.Classes;
using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Advobot.Interfaces
{
	public interface ITimersModule
	{
		void AddRemovablePunishments(params RemovablePunishment[] punishments);
		void AddRemovableMessages(params RemovableMessage[] messages);
		void AddActiveCloseHelp(params ActiveCloseWord<HelpEntry>[] help);
		void AddActiveCloseQuotes(params ActiveCloseWord<Quote>[] quotes);
		void AddSlowModeUsers(params SlowmodeUser[] users);
		void RemovePunishments(ulong id, PunishmentType punishment);
		ActiveCloseWord<HelpEntry> GetOutActiveCloseHelp(ulong id);
		ActiveCloseWord<Quote> GetOutActiveCloseQuote(ulong id);
	}

	public interface IInviteListModule
	{
		List<ListedInvite> ListedInvites { get; }

		void BumpInvite(ListedInvite invite);
		void AddInvite(ListedInvite invite);
		void RemoveInvite(ListedInvite invite);
		void RemoveInvite(IGuild guild);
	}

	public interface IGuildSettingsModule
	{
		Task AddGuild(IGuild guild);
		Task RemoveGuild(ulong guildId);
		IGuildSettings GetSettings(ulong guildId);
		IEnumerable<IGuildSettings> GetAllSettings();
		bool TryGetSettings(ulong guildId, out IGuildSettings settings);
		bool ContainsGuild(ulong guildId);
	}

	public interface ILogModule
	{
		List<LoggedCommand> RanCommands { get; }

		uint TotalUsers { get; }
		uint TotalGuilds { get; }
		uint AttemptedCommands { get; }
		uint SuccessfulCommands { get; }
		uint FailedCommands { get; }
		uint LoggedJoins { get; }
		uint LoggedLeaves { get; }
		uint LoggedUserChanges { get; }
		uint LoggedEdits { get; }
		uint LoggedDeletes { get; }
		uint LoggedMessages { get; }
		uint LoggedImages { get; }
		uint LoggedGifs { get; }
		uint LoggedFiles { get; }

		void AddUsers(int users);
		void RemoveUsers(int users);
		void IncrementUsers();
		void DecrementUsers();
		void IncrementGuilds();
		void DecrementGuilds();
		void IncrementSuccessfulCommands();
		void IncrementFailedCommands();
		void IncrementJoins();
		void IncrementLeaves();
		void IncrementUserChanges();
		void IncrementEdits();
		void IncrementDeletes();
		void IncrementMessages();
		void IncrementImages();
		void IncrementGifs();
		void IncrementFiles();

		string FormatLoggedCommands();
		string FormatLoggedActions();
	}
}
