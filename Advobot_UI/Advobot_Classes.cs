using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Advobot
{
	#region Attributes
	//If the user has all the perms required for the first arg then success, any of the second arg then success. Nothing means only Administrator works.
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class PermissionRequirementsAttribute : PreconditionAttribute
	{
		public PermissionRequirementsAttribute(uint anyOfTheListedPerms = 0, uint allOfTheListedPerms = 0)
		{
			mAllFlags = allOfTheListedPerms;
			mAnyFlags = anyOfTheListedPerms | (1U << (int)GuildPermission.Administrator);
		}

		public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
		{
			if (context.Guild != null)
			{
				var user = await context.Guild.GetUserAsync(context.User.Id);
				var botBits = Variables.BotUsers.FirstOrDefault(x => x.User == user)?.Permissions;
				if (botBits != null)
				{
					var perms = user.GuildPermissions.RawValue | botBits;
					if (mAllFlags != 0 && (perms & mAllFlags) == mAllFlags || (perms & mAnyFlags) != 0)
						return PreconditionResult.FromSuccess();
				}
				else
				{
					var perms = user.GuildPermissions.RawValue;
					if (mAllFlags != 0 && (perms & mAllFlags) == mAllFlags || (perms & mAnyFlags) != 0)
						return PreconditionResult.FromSuccess();
				}
			}
			return PreconditionResult.FromError(Constants.IGNORE_ERROR);
		}

		public string AllText
		{
			get { return String.Join(" & ", Actions.getPermissionNames(mAllFlags)); }
		}

		public string AnyText
		{
			get { return String.Join("|", Actions.getPermissionNames(mAnyFlags)); }
		}

		private uint mAllFlags;
		private uint mAnyFlags;
	}

	//Testing if the user is the bot owner or the guild owner
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class BotOwnerOrGuildOwnerRequirementAttribute : PreconditionAttribute
	{
		public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
		{
			return (await Actions.userHasOwner(context.Guild, context.User)) || Actions.userHasBotOwner(context.Guild, context.User) ?
				PreconditionResult.FromSuccess() : PreconditionResult.FromError(Constants.IGNORE_ERROR);
		}
	}

	//Use for testing if the person is the bot owner
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class BotOwnerRequirementAttribute : PreconditionAttribute
	{
		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
		{
			return Task.Run(() =>
			{
				return Actions.userHasBotOwner(context.Guild, context.User) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(Constants.IGNORE_ERROR);
			});
		}
	}

	//Testing if the user if the guild owner
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class GuildOwnerRequirementAttribute : PreconditionAttribute
	{
		public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
		{
			return await Actions.userHasOwner(context.Guild, context.User) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(Constants.IGNORE_ERROR);
		}
	}

	//Check if the user has any permission that would allow them to use the bot regularly
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class UserHasAPermissionAttribute : PreconditionAttribute
	{
		private const UInt32 PERMISSIONBITS = 0
			| (1U<<(int)GuildPermission.Administrator)
			| (1U << (int)GuildPermission.BanMembers)
			| (1U << (int)GuildPermission.DeafenMembers)
			| (1U << (int)GuildPermission.KickMembers)
			| (1U << (int)GuildPermission.ManageChannels)
			| (1U << (int)GuildPermission.ManageEmojis)
			| (1U << (int)GuildPermission.ManageGuild)
			| (1U << (int)GuildPermission.ManageMessages)
			| (1U << (int)GuildPermission.ManageNicknames)
			| (1U << (int)GuildPermission.ManageRoles)
			| (1U << (int)GuildPermission.ManageWebhooks)
			| (1U << (int)GuildPermission.MoveMembers)
			| (1U << (int)GuildPermission.MuteMembers);

		public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
		{
			if (context.Guild != null)
			{
				var user = await context.Guild.GetUserAsync(context.User.Id);
				var botBits = Variables.BotUsers.FirstOrDefault(x => x.User == user)?.Permissions;
				if (botBits != null)
				{
					if (((user.GuildPermissions.RawValue | botBits) & PERMISSIONBITS) != 0)
					{
						return PreconditionResult.FromSuccess();
					}
				}
				else
				{
					if ((user.GuildPermissions.RawValue & PERMISSIONBITS) != 0)
					{
						return PreconditionResult.FromSuccess();
					}
				}
			}
			return PreconditionResult.FromError(Constants.IGNORE_ERROR);
		}
	}
	#endregion

	#region Classes
	public class UsageAttribute : Attribute
	{
		public UsageAttribute(string usage)
		{
			mUsage = usage;
		}

		private string mUsage;

		public string Text
		{
			get { return mUsage; }
		}
	}

	public class HelpEntry
	{
		public HelpEntry(string name, string[] aliases, string usage, string basePerm, string text)
		{
			mName = name;
			mAliases = aliases;
			mUsage = usage;
			mBasePerm = basePerm;
			mText = text;
		}

		public string Name
		{
			get { return mName; }
		}
		public string[] Aliases
		{
			get { return mAliases; }
		}
		public string Usage
		{
			get { return Properties.Settings.Default.Prefix + mUsage; }
		}
		public string basePerm
		{
			get { return mBasePerm; }
		}
		public string Text
		{
			get { return mText.Replace(Constants.BOT_PREFIX, Properties.Settings.Default.Prefix); }
		}

		private string mName;
		private string[] mAliases;
		private string mUsage;
		private string mBasePerm;
		private string mText;
	}

	public class CommandSwitch
	{
		public CommandSwitch(string name, string value, CommandCategory category = CommandCategory.Miscellaneous, string[] aliases = null)
		{
			mName = name;
			mValue = value;
			mCategory = category;
			mAliases = aliases;
		}

		private string mName;
		private string mValue;
		private CommandCategory mCategory;
		private string[] mAliases;

		//Return the name
		public string Name
		{
			get { return mName; }
		}

		//Return the category
		public string CategoryName
		{
			get { return Enum.GetName(typeof(CommandCategory), (int)mCategory); }
		}

		//Return the category's value
		public int CategoryValue
		{
			get { return (int)mCategory; }
		}

		//Return the category's enum
		public CommandCategory CategoryEnum
		{
			get { return mCategory; }
		}

		//Return the value as a boolean
		public bool valAsBoolean
		{
			get
			{
				string[] trueMatches = { "true", "on", "yes", "1" };
				return trueMatches.Any(x => String.Equals(mValue.Trim(), x, StringComparison.OrdinalIgnoreCase));
			}
		}

		//Return the value as a string
		public string valAsString
		{
			get { return mValue.Trim(new char[] { '\n', '\r' }); }
		}

		//Return the value as an int
		public int valAsInteger
		{
			get
			{
				int value;
				if (Int32.TryParse(mValue, out value))
				{
					return value;
				}
				return -1;
			}
		}

		//Disable a command
		public void disable()
		{
			mValue = "OFF";
		}

		//Enable a command
		public void enable()
		{
			mValue = "ON";
		}

		//Return the aliases
		public string[] Aliases
		{
			get { return mAliases; }
		}
	}

	public class SlowmodeUser
	{
		public SlowmodeUser(IGuildUser user = null, int currentMessagesLeft = 1, int baseMessages = 1, int time = 5)
		{
			User = user;
			CurrentMessagesLeft = currentMessagesLeft;
			BaseMessages = baseMessages;
			Time = time;
		}

		public IGuildUser User;
		public int CurrentMessagesLeft;
		public int BaseMessages;
		public int Time;
	}

	public class SlowmodeChannel
	{
		public SlowmodeChannel(ulong channelID, ulong guildID)
		{
			ChannelID = channelID;
			GuildID = guildID;
		}

		public ulong ChannelID;
		public ulong GuildID;
	}

	public class BannedPhrasePunishment
	{
		public BannedPhrasePunishment(int number, PunishmentType punishment, IRole role = null, int? punishmentTime = null)
		{
			Number_Of_Removes = number;
			Punishment = punishment;
			Role = role;
			PunishmentTime = punishmentTime;
		}

		public int Number_Of_Removes;
		public PunishmentType Punishment;
		public IRole Role;
		public int? PunishmentTime;
	}

	public class BannedPhraseUser
	{
		public BannedPhraseUser(IGuildUser user, int amountOfRemovedMessages = 1)
		{
			User = user;
			AmountOfRemovedMessages = amountOfRemovedMessages;
		}

		public IGuildUser User;
		public int AmountOfRemovedMessages;
	}

	public class SelfAssignableRole
	{
		public SelfAssignableRole(IRole role, int group)
		{
			Role = role;
			Group = group;
		}

		public IRole Role;
		public int Group;
	}

	public class SelfAssignableRole2
	{
		public SelfAssignableRole2(string role, int group)
		{
			Role = role;
			Group = group;
		}

		public string Role;
		public int Group;
	}

	public class SelfAssignableGroup
	{
		public SelfAssignableGroup(List<SelfAssignableRole> roles, int group, ulong guildID)
		{
			Roles = roles;
			Group = group;
			GuildID = guildID;
		}

		public List<SelfAssignableRole> Roles;
		public int Group;
		public ulong GuildID;
	}

	public class BotInvite
	{
		public BotInvite(ulong guildID, string code, int uses)
		{
			GuildID = guildID;
			Code = code;
			Uses = uses;
		}

		public ulong GuildID;
		public string Code;
		public int Uses;
	}

	public class BotGuildInfo
	{
		public BotGuildInfo(IGuild guild)
		{
			Guild = guild;
		}

		public List<BannedPhrasePunishment> BannedPhrasesPunishments = new List<BannedPhrasePunishment>();
		public List<string> BannedPhrases = new List<string>();
		public List<Regex> BannedRegex = new List<Regex>();

		public List<CommandSwitch> CommandSettings = new List<CommandSwitch>();
		public List<LogActions> LogActions = new List<LogActions>();
		public List<ulong> IgnoredChannels = new List<ulong>();

		public List<Remind> Reminds = new List<Remind>();
		public List<BotInvite> Invites;

		public bool DefaultPrefs;
		public IGuild Guild;
		public string Prefix;
	}

	public class BotImplementedPermissions
	{
		public BotImplementedPermissions(IGuildUser user, uint permissions)
		{
			User = user;
			Permissions = permissions;
		}

		public IGuildUser User;
		public uint Permissions;
	}
	#endregion

	#region Structs
	public struct ChannelAndPosition
	{
		public ChannelAndPosition(IGuildChannel channel, int position)
		{
			Channel = channel;
			Position = position;
		}

		public IGuildChannel Channel;
		public int Position;
	}

	public struct BotGuildPermissionType
	{
		public BotGuildPermissionType(string name, int position)
		{
			mName = name;
			mPosition = position;
		}

		private string mName;
		private int mPosition;

		public string Name
		{
			get { return mName; }
		}

		public int Position
		{
			get { return mPosition; }
		}
	}

	public struct BotChannelPermissionType
	{
		public BotChannelPermissionType(string name, int position, bool gen = false, bool text = false, bool voice = false)
		{
			mName = name;
			mPosition = position;
			mGeneral = gen;
			mText = text;
			mVoice = voice;
		}

		private string mName;
		private int mPosition;
		private bool mGeneral;
		private bool mText;
		private bool mVoice;

		public string Name
		{
			get { return mName; }
		}

		public int Position
		{
			get { return mPosition; }
		}

		public bool General
		{
			get { return mGeneral; }
		}

		public bool Text
		{
			get { return mText; }
		}

		public bool Voice
		{
			get { return mVoice; }
		}
	}

	public struct Remind
	{
		public Remind(string name, string text)
		{
			Name = name;
			Text = text;
		}

		public string Name;
		public string Text;
	}

	public struct CloseWord
	{
		public CloseWord(string name, int closeness)
		{
			Name = name;
			Closeness = closeness;
		}

		public string Name;
		public int Closeness;
	}

	public struct ActiveCloseWords
	{
		public ActiveCloseWords(IGuildUser user, List<CloseWord> list)
		{
			User = user;
			List = list;
		}

		public IGuildUser User;
		public List<CloseWord> List;
	}

	public struct CloseHelp
	{
		public CloseHelp(HelpEntry help, int closeness)
		{
			Help = help;
			Closeness = closeness;
		}

		public HelpEntry Help;
		public int Closeness;
	}

	public struct ActiveCloseHelp
	{
		public ActiveCloseHelp(IGuildUser user, List<CloseHelp> list)
		{
			User = user;
			List = list;
		}

		public IGuildUser User;
		public List<CloseHelp> List;
	}
	#endregion

	#region Enums
	public enum CommandCategory
	{
		Global_Settings = 1,
		Guild_Settings = 2,
		Logs = 3,
		Ban_Phrases = 4,
		Self_Roles = 5,
		User_Moderation = 6,
		Role_Moderation = 7,
		Channel_Moderation = 8,
		Guild_Moderation = 9,
		Miscellaneous = 10
	}

	public enum PunishmentType
	{
		Kick = 1,
		Ban = 2,
		Role = 3
	}

	public enum SAGAction
	{
		Create = 1,
		Add = 2,
		Remove = 3,
		Delete = 4
	}

	public enum DeleteInvAction
	{
		User = 1,
		Channel = 2,
		Uses = 3,
		Expiry = 4
	}

	public enum LogActions
	{
		UserJoined = 1,
		UserLeft = 2,
		UserUnbanned = 3,
		UserBanned = 4,
		UserUpdated = 5,
		GuildMemberUpdated = 6,
		MessageReceived = 7,
		MessageUpdated = 8,
		MessageDeleted = 9,
		RoleCreated = 10,
		RoleUpdated = 11,
		RoleDeleted = 12,
		ChannelCreated = 12,
		ChannelUpdated = 13,
		ChannelDeleted = 14,
		ImageLog = 15
	}
	#endregion
}