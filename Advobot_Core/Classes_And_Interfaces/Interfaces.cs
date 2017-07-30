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
			uint TotalUsers { get; }
			uint TotalGuilds { get; }
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

			ILog Log { get; }

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

		public interface ILog
		{
			Task Log(LogMessage msg);
			Task OnGuildAvailable(SocketGuild guild);
			Task OnGuildUnavailable(SocketGuild guild);
			Task OnJoinedGuild(SocketGuild guild);
			Task OnLeftGuild(SocketGuild guild);
			Task OnUserJoined(SocketGuildUser user);
			Task OnUserLeft(SocketGuildUser user);
			Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser);
			Task OnMessageReceived(SocketMessage message);
			Task OnMessageUpdated(Cacheable<IMessage, ulong> cached, SocketMessage afterMessage, ISocketMessageChannel channel);
			Task OnMessageDeleted(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel);
			Task LogCommand(IMyCommandContext context);
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
			IReadOnlyList<BotImplementedPermissions> BotUsers { get; set; }
			IReadOnlyList<SelfAssignableGroup> SelfAssignableGroups { get; set; }
			IReadOnlyList<Quote> Quotes { get; set; }
			IReadOnlyList<LogAction> LogActions { get; set; }
			IReadOnlyList<ulong> IgnoredCommandChannels { get; set; }
			IReadOnlyList<ulong> IgnoredLogChannels { get; set; }
			IReadOnlyList<ulong> ImageOnlyChannels { get; set; }
			IReadOnlyList<ulong> SanitaryChannels { get; set; }
			IReadOnlyList<BannedPhrase> BannedPhraseStrings { get; set; }
			IReadOnlyList<BannedPhrase> BannedPhraseRegex { get; set; }
			IReadOnlyList<BannedPhrase> BannedNamesForJoiningUsers { get; set; }
			IReadOnlyList<BannedPhrasePunishment> BannedPhrasePunishments { get; set; }
			IReadOnlyList<CommandSwitch> CommandSwitches { get; set; }
			IReadOnlyList<CommandOverride> CommandsDisabledOnUser { get; set; }
			IReadOnlyList<CommandOverride> CommandsDisabledOnRole { get; set; }
			IReadOnlyList<CommandOverride> CommandsDisabledOnChannel { get; set; }
			ITextChannel ServerLog { get; set; }
			ITextChannel ModLog { get; set; }
			ITextChannel ImageLog { get; set; }
			IRole MuteRole { get; set; }
			IReadOnlyDictionary<SpamType, SpamPreventionInfo> SpamPreventionDictionary { get; set; }
			IReadOnlyDictionary<RaidType, RaidPreventionInfo> RaidPreventionDictionary { get; set; }
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
			IGuild Guild { get; }
			bool Loaded { get; }
		}
	}
}
