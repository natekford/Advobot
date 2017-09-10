using System;

//I know enums don't need "= x," but I like it.
namespace Advobot.Enums
{
	/// <summary>
	/// Allows certain guild events to be logged when these are in <see cref="Interfaces.IGuildSettings.LogActions"/>.
	/// </summary>
	[Flags]
	public enum LogAction : uint
	{
		UserJoined						= (1U << 0),
		UserLeft						= (1U << 1),
		UserUpdated						= (1U << 2),
		MessageReceived					= (1U << 3),
		MessageUpdated					= (1U << 4),
		MessageDeleted					= (1U << 5),
	}

	/// <summary>
	/// Specifies which type of notification a guild notification should be.
	/// </summary>
	[Flags]
	public enum GuildNotificationType : uint
	{
		Welcome							= (1U << 0),
		Goodbye							= (1U << 1),
	}

	/// <summary>
	/// Specifies which category a command is in.
	/// </summary>
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
		Quotes							= (1U << 14),
		Rules							= (1U << 15),
	}

	/// <summary>
	/// Specifies which log channel to modify.
	/// </summary>
	[Flags]
	public enum LogChannelType : uint
	{
		Server							= (1U << 0),
		Mod								= (1U << 1),
		Image							= (1U << 2),
	}

	/// <summary>
	/// For use in <see cref="Attributes.VerifyUserAttribute"/> to determine what to check.
	/// </summary>
	[Flags]
	public enum UserVerification : uint
	{
		None							= (1U << 0),
		CanBeEdited						= (1U << 1),
		CanBeMovedFromChannel			= (1U << 2),
	}

	/// <summary>
	/// For use in <see cref="Attributes.VerifyChannelAttribute"/> to determine what to check.
	/// </summary>
	[Flags]
	public enum ChannelVerification : uint
	{
		None							= (1U << 0),
		CanBeEdited						= (1U << 1),
		//IsDefault						= (1U << 2), Not needed anymore since default channels removed
		//IsVoice						= (1U << 3), Not needed anymore since using Discord.Net arg parsing.
		//IsText						= (1U << 4), Not needed anymore for same reason as above.
		CanBeReordered					= (1U << 5),
		CanModifyPermissions			= (1U << 6),
		CanBeManaged					= (1U << 7),
		CanMoveUsers					= (1U << 8),
		CanDeleteMessages				= (1U << 9),
		CanBeRead						= (1U << 10),
		CanCreateInstantInvite			= (1U << 11),
	}

	/// <summary>
	/// For us in <see cref="Attributes.VerifyRoleAttribute"/> to determine what to check.
	/// </summary>
	[Flags]
	public enum RoleVerification : uint
	{
		None							= (1U << 0),
		CanBeEdited						= (1U << 1),
		IsEveryone						= (1U << 2),
		IsManaged						= (1U << 3),
	}

	/// <summary>
	/// Results from various checks on Discord entities.
	/// To be used in order to get a formatted error reason from <see cref="Actions.FormattingActions.FormatErrorString(Discord.IGuild, FailureReason, object)"/>.
	/// </summary>
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
		//ChannelType					= (1U << 6), Not needed anymore since using Discord.Net arg parsing.
		//DefaultChannel				= (1U << 7), Not needed anymore since default channels can be deleted/modified fully now

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

	/// <summary>
	/// Specify what punishment should be given.
	/// </summary>
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

	/// <summary>
	/// Specifies what spam prevention to modify/set up.
	/// </summary>
	[Flags]
	public enum SpamType : uint
	{
		Message							= (1U << 0),
		LongMessage						= (1U << 1),
		Link							= (1U << 2),
		Image							= (1U << 3),
		Mention							= (1U << 4),
	}

	/// <summary>
	/// Specifies what raid prevention to modify/set up.
	/// </summary>
	[Flags]
	public enum RaidType : uint
	{
		Regular							= (1U << 0),
		RapidJoins						= (1U << 1),
	}

	/// <summary>
	/// Specifies what action to do.
	/// </summary>
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
		Default							= (1U << 13),
		On								= (1U << 14),
		Off								= (1U << 15),
	}

	/// <summary>
	/// Used in <see cref="Attributes.OtherRequirementAttribute"/> to perform various checks.
	/// </summary>
	[Flags]
	public enum Precondition : uint
	{
		UserHasAPerm					= (1U << 0),
		GuildOwner						= (1U << 1),
		TrustedUser						= (1U << 2),
		BotOwner						= (1U << 3),
	}

	/// <summary>
	/// Specifies what external settings to use on a channel.
	/// </summary>
	[Flags]
	public enum ChannelSetting : uint
	{
		ImageOnly						= (1U << 0),
	}

	/// <summary>
	/// Specifies what to target with a command. Discord entities, names, prefix, etc.
	/// </summary>
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
		Prefix							= (1U << 12),
	}
}
