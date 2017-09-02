using Advobot.Enums;
using Advobot.NonSavedClasses;
using Advobot.RemovablePunishments;
using Advobot.SavedClasses;
using Advobot.Structs;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Advobot
{
	namespace Interfaces
	{
		public interface ITimeInterface
		{
			DateTime GetTime();
		}

		public interface IPermission
		{
			string Name { get; }
			ulong Bit { get; }
		}

		public interface INameAndText
		{
			string Name { get; }
			string Text { get; }
		}

		public interface ISetting
		{
			string ToString();
			string ToString(SocketGuild guild);
		}

		public interface IMyCommandContext : ICommandContext
		{
			IBotSettings BotSettings { get; }
			IGuildSettings GuildSettings { get; }
			ILogModule Logging { get; }
			ITimersModule Timers { get; }
		}

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
			Task RemoveGuild(IGuild guild);
			IGuildSettings GetSettings(IGuild guild);
			IEnumerable<IGuildSettings> GetAllSettings();
			bool TryGetSettings(IGuild guild, out IGuildSettings settings);
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

		public interface IBotSettings
		{
			IReadOnlyList<ulong> TrustedUsers { get; set; }
			IReadOnlyList<ulong> UsersUnableToDMOwner { get; set; }
			IReadOnlyList<ulong> UsersIgnoredFromCommands { get; set; }
			uint ShardCount { get; set; }
			uint MessageCacheCount { get; set; }
			uint MaxUserGatherCount { get; set; }
			uint MaxMessageGatherSize { get; set; }
			string Prefix { get; set; }
			string Game { get; set; }
			string Stream { get; set; }
			bool AlwaysDownloadUsers { get; set; }
			LogSeverity LogLevel { get; set; }

			bool Windows { get; }
			bool Console { get; }
			bool FirstInstanceOfBotStartingUpWithCurrentKey { get; }
			bool GotPath { get; }
			bool GotKey { get; }
			bool Loaded { get; }
			bool Pause { get; }
			DateTime StartupTime { get; }

			void TogglePause();
			void SetLoaded();
			void SetGotKey();
			void SetGotPath();
		}

		public interface IGuildSettings
		{
			List<BotImplementedPermissions> BotUsers { get; set; }
			List<SelfAssignableGroup> SelfAssignableGroups { get; set; }
			List<Quote> Quotes { get; set; }
			List<LogAction> LogActions { get; set; }
			List<ulong> IgnoredCommandChannels { get; set; }
			List<ulong> IgnoredLogChannels { get; set; }
			List<ulong> ImageOnlyChannels { get; set; }
			List<BannedPhrase> BannedPhraseStrings { get; set; }
			List<BannedPhrase> BannedPhraseRegex { get; set; }
			List<BannedPhrase> BannedNamesForJoiningUsers { get; set; }
			List<BannedPhrasePunishment> BannedPhrasePunishments { get; set; }
			List<CommandSwitch> CommandSwitches { get; set; }
			List<CommandOverride> CommandsDisabledOnUser { get; set; }
			List<CommandOverride> CommandsDisabledOnRole { get; set; }
			List<CommandOverride> CommandsDisabledOnChannel { get; set; }
			List<PersistentRole> PersistentRoles { get; set; }
			ITextChannel ServerLog { get; set; }
			ITextChannel ModLog { get; set; }
			ITextChannel ImageLog { get; set; }
			IRole MuteRole { get; set; }
			Dictionary<SpamType, SpamPreventionInfo> SpamPreventionDictionary { get; set; }
			Dictionary<RaidType, RaidPreventionInfo> RaidPreventionDictionary { get; set; }
			GuildNotification WelcomeMessage { get; set; }
			GuildNotification GoodbyeMessage { get; set; }
			ListedInvite ListedInvite { get; set; }
			Slowmode Slowmode { get; set; }
			string Prefix { get; set; }
			bool VerboseErrors { get; set; }

			List<BannedPhraseUser> BannedPhraseUsers { get; }
			List<SpamPreventionUser> SpamPreventionUsers { get; }
			List<BotInvite> Invites { get; }
			List<string> EvaluatedRegex { get; }
			MessageDeletion MessageDeletion { get; }
			SocketGuild Guild { get; }
			bool Loaded { get; }

			void SaveSettings();
		}
	}
}
