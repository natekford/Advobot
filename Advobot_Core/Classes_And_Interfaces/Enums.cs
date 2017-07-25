using Advobot.Attributes;
using System;

namespace Advobot
{
	namespace Enums
	{
		//I know enums don't need "= x," but I like it.
		public enum LogAction
		{
			UserJoined						= 0,
			UserLeft						= 1,
			UserUpdated						= 2,
			MessageReceived					= 3,
			MessageUpdated					= 4,
			MessageDeleted					= 5,
		}

		public enum SettingOnGuild
		{
			//Nonsaved settings
			Loaded							= -8,
			MessageDeletion					= -7,
			SlowmodeGuild					= -6,
			EvaluatedRegex					= -5,
			Invites							= -4,
			SlowmodeChannels				= -3,
			SpamPreventionUsers				= -2,
			BannedPhraseUsers				= -1,

			//Saved settings
			CommandSwitches					= 1,
			CommandsDisabledOnChannel		= 2,
			BotUsers						= 3,
			SelfAssignableGroups			= 4,
			Quotes							= 5,
			IgnoredLogChannels				= 6,
			LogActions						= 7,
			BannedPhraseStrings				= 8,
			BannedPhraseRegex				= 9,
			BannedPhrasePunishments			= 10,
			MessageSpamPrevention			= 11,
			LongMessageSpamPrevention		= 12,
			LinkSpamPrevention				= 13,
			ImageSpamPrevention				= 14,
			MentionSpamPrevention			= 15,
			WelcomeMessage					= 16,
			GoodbyeMessage					= 17,
			Prefix							= 18,
			ServerLog						= 19,
			ModLog							= 20,
			ImageOnlyChannels				= 21,
			IgnoredCommandChannels			= 22,
			CommandsDisabledOnUser			= 23,
			CommandsDisabledOnRole			= 24,
			ImageLog						= 25,
			ListedInvite					= 26,
			BannedNamesForJoiningUsers		= 27,
			RaidPrevention					= 28,
			RapidJoinPrevention				= 29,
			PyramidalRoleSystem				= 30,
			MuteRole						= 31,
			SanitaryChannels				= 32,
			VerboseErrors					= 33,
			Guild							= 34,
		}

		public enum SettingOnBot
		{
			//Saved in Properties.Settings
			SavePath						= -1,

			//Saved in JSON
			TrustedUsers					= 2,
			Prefix							= 3,
			Game							= 4,
			Stream							= 5,
			ShardCount						= 6,
			MessageCacheCount				= 7,
			AlwaysDownloadUsers				= 8,
			LogLevel						= 9,
			MaxUserGatherCount				= 10,
			UnableToDMOwnerUsers			= 11,
			IgnoredCommandUsers				= 12,
			MaxMessageGatherSize			= 13,
		}

		public enum GuildNotificationType
		{
			Welcome							= 1,
			Goodbye							= 2,
		}

		[Flags]
		public enum CommandCategory : uint
		{
			BotSettings						= (1U << 0),
			GuildSettings					= (1U << 1),
			Logs							= (1U << 2),
			BanPhrases						= (1U << 3),
			SelfRoles						= (1U << 4),
			UserModeration					= (1U << 5),
			RoleModeration					= (1U << 6),
			ChannelModeration				= (1U << 7),
			GuildModeration					= (1U << 8),
			Miscellaneous					= (1U << 9),
			SpamPrevention					= (1U << 10),
			InviteModeration				= (1U << 11),
			GuildList						= (1U << 12),
			NicknameModeration				= (1U << 13),
		}

		[Flags]
		public enum EmoteType : uint
		{
			Global							= (1U << 0),
			Guild							= (1U << 1),
		}

		[Flags]
		public enum LogChannelType : uint
		{
			Server							= (1U << 0),
			Mod								= (1U << 1),
			Image							= (1U << 2),
		}

		[Flags]
		public enum ObjectVerification : uint
		{
			[DiscordObjectTarget(0)]
			None							= (1U << 0),
			[DiscordObjectTarget(0)]
			CanBeEdited						= (1U << 1),

			[DiscordObjectTarget(Target.User)]
			CanBeMovedFromChannel			= (1U << 2),

			[DiscordObjectTarget(Target.Channel)]
			IsVoice							= (1U << 3),
			[DiscordObjectTarget(Target.Channel)]
			IsText							= (1U << 4),
			[DiscordObjectTarget(Target.Channel)]
			CanBeReordered					= (1U << 5),
			[DiscordObjectTarget(Target.Channel)]
			CanModifyPermissions			= (1U << 6),
			[DiscordObjectTarget(Target.Channel)]
			CanBeManaged					= (1U << 7),
			[DiscordObjectTarget(Target.Channel)]
			CanMoveUsers					= (1U << 8),
			[DiscordObjectTarget(Target.Channel)]
			CanDeleteMessages				= (1U << 9),
			[DiscordObjectTarget(Target.Channel)]
			CanBeRead						= (1U << 10),
			[DiscordObjectTarget(Target.Channel)]
			CanCreateInstantInvite			= (1U << 11),
			[DiscordObjectTarget(Target.Channel)]
			IsDefault						= (1U << 12),

			[DiscordObjectTarget(Target.Role)]
			IsEveryone						= (1U << 13),
			[DiscordObjectTarget(Target.Role)]
			IsManaged						= (1U << 14),
		}

		[Flags]
		public enum FailureReason : uint
		{
			//Generic
			NotFailure						= (1U << 1),
			TooFew							= (1U << 2),
			TooMany							= (1U << 3),

			//User
			UserInability					= (1U << 4),
			BotInability					= (1U << 5),
		
			//Channels
			ChannelType						= (1U << 6),
			DefaultChannel					= (1U << 7),

			//Roles
			EveryoneRole					= (1U << 8),
			ManagedRole						= (1U << 9),

			//Enums
			InvalidEnum						= (1U << 10),

			//Bans
			NoBans							= (1U << 11),
			InvalidDiscriminator			= (1U << 12),
			InvalidID						= (1U << 13),
			NoUsernameOrID					= (1U << 14),
		}

		[Flags]
		public enum FileType : uint
		{
			GuildSettings					= (1U << 0),
		}

		[Flags]
		public enum PunishmentType : uint
		{
			Kick							= (1U << 0),
			Ban								= (1U << 1),
			Deafen							= (1U << 2),
			VoiceMute						= (1U << 3),
			KickThenBan						= (1U << 4),
			RoleMute						= (1U << 5),
		}

		[Flags]
		public enum DeleteInvAction : uint
		{
			User							= (1U << 0),
			Channel							= (1U << 1),
			Uses							= (1U << 2),
			Expiry							= (1U << 3),
		}

		[Flags]
		public enum SpamType : uint
		{
			Message							= (1U << 0),
			LongMessage						= (1U << 1),
			Link							= (1U << 2),
			Image							= (1U << 3),
			Mention							= (1U << 4),
		}

		[Flags]
		public enum RaidType : uint
		{
			Regular							= (1U << 0),
			RapidJoins						= (1U << 1),
		}

		[Flags]
		public enum ActionType : uint
		{
			Show							= (1U << 0),
			Allow							= (1U << 1),
			Inherit							= (1U << 2),
			Deny							= (1U << 3),
			Enable							= (1U << 4),
			Disable							= (1U << 5),
			Setup							= (1U << 6),
			Create							= (1U << 7),
			Add								= (1U << 8),
			Remove							= (1U << 9),
			Delete							= (1U << 10),
			Clear							= (1U << 11),
			Current							= (1U << 12),
		}

		[Flags]
		public enum Precondition : uint
		{
			UserHasAPerm					= (1U << 0),
			GuildOwner						= (1U << 1),
			TrustedUser						= (1U << 2),
			BotOwner						= (1U << 3),
		}

		[Flags]
		public enum ChannelSetting : uint
		{
			ImageOnly						= (1U << 0),
			Sanitary						= (1U << 1),
		}

		[Flags]
		public enum Target : uint
		{
			Guild							= (1U << 0),
			Channel							= (1U << 1),
			Role							= (1U << 2),
			User							= (1U << 3),
			Emote							= (1U << 4),
			Invite							= (1U << 5),
			Bot								= (1U << 6),
			Name							= (1U << 7),
			Nickname						= (1U << 8),
			Game							= (1U << 9),
			Stream							= (1U << 10),
			Topic							= (1U << 11),
		}
	}
}
