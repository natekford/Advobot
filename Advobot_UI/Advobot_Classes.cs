using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot
{
	#region Attributes
	public class PermissionRequirementAttribute : PreconditionAttribute
	{
		private uint mAllFlags;
		private uint mAnyFlags;

		public PermissionRequirementAttribute(uint anyOfTheListedPerms = 0, uint allOfTheListedPerms = 0)
		{
			mAllFlags = allOfTheListedPerms;
			mAnyFlags = anyOfTheListedPerms | (1U << (int)GuildPermission.Administrator);
		}

		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
		{
			if (context.Guild != null && Variables.Guilds.TryGetValue(context.Guild.Id, out BotGuildInfo guildInfo))
			{
				var user = context.User as IGuildUser;
				var botBits = ((List<BotImplementedPermissions>)guildInfo.GetSetting(SettingOnGuild.BotUsers)).FirstOrDefault(x => x.User.Id == user.Id)?.Permissions;
				if (botBits != null)
				{
					var perms = user.GuildPermissions.RawValue | botBits;
					if (mAllFlags != 0 && (perms & mAllFlags) == mAllFlags || (perms & mAnyFlags) != 0)
						return Task.FromResult(PreconditionResult.FromSuccess());
				}
				else
				{
					var perms = user.GuildPermissions.RawValue;
					if (mAllFlags != 0 && (perms & mAllFlags) == mAllFlags || (perms & mAnyFlags) != 0)
						return Task.FromResult(PreconditionResult.FromSuccess());
				}
			}
			return Task.FromResult(PreconditionResult.FromError(Constants.IGNORE_ERROR));
		}

		public string AllText
		{
			get { return String.Join(" & ", Actions.GetPermissionNames(mAllFlags)); }
		}
		public string AnyText
		{
			get { return String.Join(" | ", Actions.GetPermissionNames(mAnyFlags)); }
		}
	}

	public class OtherRequirementAttribute : PermissionRequirementAttribute
	{
		private const uint PERMISSIONBITS = 0
			| (1U << (int)GuildPermission.Administrator)
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
		public uint Requirements { get; private set; }

		public OtherRequirementAttribute(uint preconditions)
		{
			Requirements = preconditions;
		}

		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
		{
			var user = context.User as IGuildUser;
			var guild = context.Guild;
			if (!Variables.Guilds.TryGetValue(context.Guild.Id, out BotGuildInfo guildInfo))
			{
				return Task.FromResult(PreconditionResult.FromError("Guild is not loaded correctly."));
			}

			var permissions = (Requirements & (1U << (int)Precondition.User_Has_A_Perm)) != 0;
			var guildOwner = (Requirements & (1U << (int)Precondition.Guild_Owner)) != 0;
			var trustedUser = (Requirements & (1U << (int)Precondition.Trusted_User)) != 0;
			var botOwner = (Requirements & (1U << (int)Precondition.Bot_Owner)) != 0;

			//Check if users has any permissions
			if (permissions)
			{
				var botBits = ((List<BotImplementedPermissions>)guildInfo.GetSetting(SettingOnGuild.BotUsers)).FirstOrDefault(x => x.User.Id == user.Id)?.Permissions;
				if (botBits != null)
				{
					if (((user.GuildPermissions.RawValue | botBits) & PERMISSIONBITS) != 0)
					{
						return Task.FromResult(PreconditionResult.FromSuccess());
					}
				}
				else
				{
					if ((user.GuildPermissions.RawValue & PERMISSIONBITS) != 0)
					{
						return Task.FromResult(PreconditionResult.FromSuccess());
					}
				}
			}
			//Check if the user is the guild owner
			if (guildOwner)
			{
				if (Actions.GetIfUserIsOwner(context.Guild, user))
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
			}
			//Check if the user is a trusted user
			if (trustedUser)
			{
				if (Actions.GetIfUserIsTrustedUser(user))
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
			}
			//Check if the user is the bot owner
			if (botOwner)
			{
				if (Actions.GetIfUserIsBotOwner(user))
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
			}

			//If they don't match any checks then return an error
			return Task.FromResult(PreconditionResult.FromError(Constants.IGNORE_ERROR));
		}
	}

	public class DefaultEnabledAttribute : Attribute
	{
		public bool Enabled { get; private set; }

		public DefaultEnabledAttribute(bool enabled)
		{
			Enabled = enabled;
		}
	}

	public class UsageAttribute : Attribute
	{
		public string Usage { get; private set; }

		public UsageAttribute(string usage)
		{
			Usage = usage;
		}
	}

	public class GuildSettingAttribute : Attribute
	{
		public List<SettingOnGuild> Settings { get; private set; }

		public GuildSettingAttribute(params SettingOnGuild[] settings)
		{
			Settings = settings.ToList();
		}
	}
	#endregion

	#region Saved Classes
	public class BotGuildInfo
	{
		[JsonProperty("Settings")]
		private Dictionary<SettingOnGuild, object> mSettings = new Dictionary<SettingOnGuild, object>
		{
			{ SettingOnGuild.CommandPreferences, new List<CommandSwitch>() },
			{ SettingOnGuild.CommandsDisabledOnUser, new List<CommandOverride<IGuildUser>>() },
			{ SettingOnGuild.CommandsDisabledOnRole, new List<CommandOverride<IRole>>() },
			{ SettingOnGuild.CommandsDisabledOnChannel, new List<CommandOverride<IGuildChannel>>() },

			{ SettingOnGuild.BotUsers, new List<BotImplementedPermissions>() },
			{ SettingOnGuild.SelfAssignableGroups, new List<SelfAssignableGroup>() },
			{ SettingOnGuild.Reminds, new List<Remind>() },
			{ SettingOnGuild.LogActions, new List<LogActions>() },
			{ SettingOnGuild.BannedPhraseStrings, new List<BannedPhrase>() },
			{ SettingOnGuild.BannedPhraseRegex, new List<BannedPhrase>() },
			{ SettingOnGuild.BannedPhrasePunishments, new List<BannedPhrasePunishment>() },

			{ SettingOnGuild.IgnoredCommandChannels, new List<ulong>() },
			{ SettingOnGuild.IgnoredLogChannels, new List<ulong>() },
			{ SettingOnGuild.ImageOnlyChannels, new List<ulong>() },
			{ SettingOnGuild.SanitaryChannels, new List<ulong>() },
			{ SettingOnGuild.BannedNamesForJoiningUsers, new List<string>() },

			{ SettingOnGuild.Guild, new DiscordObjectWithID<SocketGuild>(null) },
			{ SettingOnGuild.ServerLog, new DiscordObjectWithID<ITextChannel>(null) },
			{ SettingOnGuild.ModLog, new DiscordObjectWithID<ITextChannel>(null) },
			{ SettingOnGuild.ImageLog, new DiscordObjectWithID<ITextChannel>(null) },
			{ SettingOnGuild.MuteRole, new DiscordObjectWithID<IRole>(null) },

			{ SettingOnGuild.MessageSpamPrevention, null },
			{ SettingOnGuild.LongMessageSpamPrevention, null },
			{ SettingOnGuild.LinkSpamPrevention, null },
			{ SettingOnGuild.ImageSpamPrevention, null },
			{ SettingOnGuild.MentionSpamPrevention, null },
			{ SettingOnGuild.WelcomeMessage, null },
			{ SettingOnGuild.GoodbyeMessage, null },
			{ SettingOnGuild.Prefix, null },
			{ SettingOnGuild.ListedInvite, null },
			{ SettingOnGuild.RaidPrevention, null },
			{ SettingOnGuild.RapidJoinPrevention, null },
			{ SettingOnGuild.PyramidalRoleSystem, new PyramidalRoleSystem() },
		};

		[JsonIgnore]
		public List<BannedPhraseUser> BannedPhraseUsers = new List<BannedPhraseUser>();
		[JsonIgnore]
		public List<SpamPreventionUser> SpamPreventionUsers = new List<SpamPreventionUser>();
		[JsonIgnore]
		public List<SlowmodeChannel> SlowmodeChannels = new List<SlowmodeChannel>();
		[JsonIgnore]
		public List<BotInvite> Invites = new List<BotInvite>();
		[JsonIgnore]
		public List<string> EvaluatedRegex = new List<string>();
		[JsonIgnore]
		public SlowmodeGuild SlowmodeGuild = null;
		[JsonIgnore]
		public MessageDeletion MessageDeletion = new MessageDeletion();
		[JsonIgnore]
		public bool Loaded = false;

		[JsonConstructor]
		public BotGuildInfo(Dictionary<SettingOnGuild, object> settings)
		{
			mSettings = settings;
		}
		public BotGuildInfo(ulong guildID)
		{
			SetSetting(SettingOnGuild.Guild, new DiscordObjectWithID<SocketGuild>(guildID));
		}

		public dynamic GetSetting(SettingOnGuild setting)
		{
			try
			{
				return mSettings[setting];
			}
			catch (Exception e)
			{
				Actions.ExceptionToConsole(e);
				return null;
			}
		}
		public bool SetSetting(SettingOnGuild setting, dynamic obj)
		{
			try
			{
				mSettings[setting] = obj;
				SaveGuildSettings();
				return true;
			}
			catch (Exception e)
			{
				Actions.ExceptionToConsole(e);
				return false;
			}
		}
		public SpamPrevention GetSpamPrevention(SpamType spamType)
		{
			switch (spamType)
			{
				case SpamType.Message:
				{
					return (SpamPrevention)mSettings[SettingOnGuild.MessageSpamPrevention];
				}
				case SpamType.Long_Message:
				{
					return (SpamPrevention)mSettings[SettingOnGuild.LongMessageSpamPrevention];
				}
				case SpamType.Link:
				{
					return (SpamPrevention)mSettings[SettingOnGuild.LinkSpamPrevention];
				}
				case SpamType.Image:
				{
					return (SpamPrevention)mSettings[SettingOnGuild.ImageSpamPrevention];
				}
				case SpamType.Mention:
				{
					return (SpamPrevention)mSettings[SettingOnGuild.MentionSpamPrevention];
				}
				default:
				{
					return null;
				}
			}
		}
		public RaidPrevention GetRaidPrevention(RaidType raidType)
		{
			switch (raidType)
			{
				case RaidType.Regular:
				{
					return (RaidPrevention)mSettings[SettingOnGuild.RaidPrevention];
				}
				case RaidType.Rapid_Joins:
				{
					return (RaidPrevention)mSettings[SettingOnGuild.RapidJoinPrevention];
				}
				default:
				{
					return null;
				}
			}
		}
		public void SetSpamPrevention(SpamType spamType, SpamPrevention spamPrev)
		{
			switch (spamType)
			{
				case SpamType.Message:
				{
					mSettings[SettingOnGuild.MessageSpamPrevention] = spamPrev;
					return;
				}
				case SpamType.Long_Message:
				{
					mSettings[SettingOnGuild.LongMessageSpamPrevention] = spamPrev;
					return;
				}
				case SpamType.Link:
				{
					mSettings[SettingOnGuild.LinkSpamPrevention] = spamPrev;
					return;
				}
				case SpamType.Image:
				{
					mSettings[SettingOnGuild.ImageSpamPrevention] = spamPrev;
					return;
				}
				case SpamType.Mention:
				{
					mSettings[SettingOnGuild.MentionSpamPrevention] = spamPrev;
					return;
				}
			}
		}
		public void SetRaidPrevention(RaidType raidType, RaidPrevention raidPrev)
		{
			switch (raidType)
			{
				case RaidType.Regular:
				{
					mSettings[SettingOnGuild.RaidPrevention] = raidPrev;
					return;
				}
				case RaidType.Rapid_Joins:
				{
					mSettings[SettingOnGuild.RapidJoinPrevention] = raidPrev;
					return;
				}
			}
		}
		public void SaveGuildSettings()
		{
			var path = Actions.GetServerFilePath(((DiscordObjectWithID<SocketGuild>)GetSetting(SettingOnGuild.Guild)).ID, Constants.GUILD_INFO_LOCATION);
			var serializedSettings = Actions.Serialize(mSettings);
			Actions.OverWriteFile(path, serializedSettings);
		}
		public void PostDeserialize(ulong guildID)
		{
			var guild = ((DiscordObjectWithID<SocketGuild>)GetSetting(SettingOnGuild.Guild));
			if (guild.ID == 0)
			{
				SetSetting(SettingOnGuild.Guild, new DiscordObjectWithID<SocketGuild>(guildID));
			}
			guild.PostDeserialize(null);

			var modLog = ((DiscordObjectWithID<ITextChannel>)GetSetting(SettingOnGuild.ModLog));
			modLog.PostDeserialize(guild.Object);

			var serverLog = ((DiscordObjectWithID<ITextChannel>)GetSetting(SettingOnGuild.ServerLog));
			serverLog.PostDeserialize(guild.Object);

			var imageLog = ((DiscordObjectWithID<ITextChannel>)GetSetting(SettingOnGuild.ImageLog));
			imageLog.PostDeserialize(guild.Object);

			var muteRole = ((DiscordObjectWithID<IRole>)GetSetting(SettingOnGuild.MuteRole));
			muteRole.PostDeserialize(guild.Object);

			foreach (var group in ((List<SelfAssignableGroup>)GetSetting(SettingOnGuild.SelfAssignableGroups)))
			{
				group.Roles.RemoveAll(x => x == null || x.Role == null);
				group.Roles.ForEach(x => x.SetGroup(group.Group));
			}

			var listedInv = ((ListedInvite)GetSetting(SettingOnGuild.ListedInvite));
			if (listedInv != null)
			{
				Variables.InviteList.ThreadSafeAdd(listedInv);
			}

			Loaded = true;
		}
	}

	public class BotGlobalInfo
	{
		[JsonProperty]
		public ulong BotOwnerID { get; private set; }
		[JsonProperty]
		public List<ulong> TrustedUsers { get; private set; }
		[JsonProperty]
		public string Prefix { get; private set; }
		[JsonProperty]
		public string Game { get; private set; }
		[JsonProperty]
		public string Stream { get; private set; }
		[JsonProperty]
		public int ShardCount { get; private set; }
		[JsonProperty]
		public int MessageCacheSize { get; private set; }
		[JsonProperty]
		public bool AlwaysDownloadUsers { get; private set; }
		[JsonProperty]
		public LogSeverity LogLevel { get; private set; }
		[JsonProperty]
		public int MaxUserGatherCount { get; private set; }

		public BotGlobalInfo()
		{
			BotOwnerID = 0;
			TrustedUsers = new List<ulong>();
			Prefix = Constants.BOT_PREFIX;
			Game = String.Format("type \"{0}help\" for help.", Prefix);
			Stream = null;
			ShardCount = 1;
			MessageCacheSize = 1000;
			AlwaysDownloadUsers = true;
			LogLevel = LogSeverity.Warning;
			MaxUserGatherCount = 100;
		}

		public string GetSetting(SettingOnBot setting)
		{
			var text = "";
			switch (setting)
			{
				case SettingOnBot.BotOwner:
				{
					text = BotOwnerID.ToString();
					break;
				}
				case SettingOnBot.Prefix:
				{
					text = Prefix;
					break;
				}
				case SettingOnBot.Game:
				{
					text = Game;
					break;
				}
				case SettingOnBot.Stream:
				{
					text = Stream;
					break;
				}
				case SettingOnBot.ShardCount:
				{
					text = ShardCount.ToString();
					break;
				}
				case SettingOnBot.MessageCacheSize:
				{
					text = MessageCacheSize.ToString();
					break;
				}
				case SettingOnBot.MaxUserGatherCount:
				{
					text = MaxUserGatherCount.ToString();
					break;
				}
			}
			return text;
		}
		public void SetBotOwner(ulong ID)
		{
			BotOwnerID = ID;
		}
		public void ResetBotOwner()
		{
			BotOwnerID = 0;
		}
		public void AddTrustedUser(ulong ID)
		{
			TrustedUsers.ThreadSafeAdd(ID);
		}
		public void RemoveTrustedUser(ulong ID)
		{
			TrustedUsers.ThreadSafeRemove(ID);
		}
		public void SetTrustedUsers(List<ulong> IDs)
		{
			TrustedUsers = IDs;
		}
		public void ResetTrustedUsers()
		{
			TrustedUsers = new List<ulong>();
		}
		public void SetPrefix(string prefix)
		{
			Prefix = prefix;
		}
		public void ResetPrefix()
		{
			Prefix = Constants.BOT_PREFIX;
		}
		public void SetGame(string game)
		{
			Game = game;
		}
		public void ResetGame()
		{
			Game = null;
		}
		public void SetStream(string stream)
		{
			Stream = stream;
		}
		public void ResetStream()
		{
			Stream = null;
		}
		public void SetShardCount(int i)
		{
			ShardCount = i;
		}
		public void SetCacheSize(int i)
		{
			MessageCacheSize = i;
		}
		public void ResetCacheSize()
		{
			MessageCacheSize = 1000;
		}
		public void SetAlwaysDownloadUsers(bool dl)
		{
			AlwaysDownloadUsers = dl;
		}
		public void ResetAlwaysDownloadUsers()
		{
			AlwaysDownloadUsers = true;
		}
		public void SetLogLevel(LogSeverity logLevel)
		{
			LogLevel = logLevel;
		}
		public void ResetLogLevel()
		{
			LogLevel = LogSeverity.Warning;
		}
		public void SetMaxUserGatherCount(int count)
		{
			MaxUserGatherCount = count;
		}
		public void ResetMaxUserGatherCount()
		{
			MaxUserGatherCount = 100;
		}
		public void ResetAll()
		{
			ResetPrefix();
			ResetTrustedUsers();
			ResetBotOwner();
			ResetStream();
			ResetGame();
			ResetCacheSize();
			ResetAlwaysDownloadUsers();
			ResetLogLevel();
			ResetMaxUserGatherCount();
		}
		public void PostDeserialize()
		{
		}
	}

	public abstract class Setting
	{
		public abstract string SettingToString();
		public abstract string SettingToString(SocketGuild guild);
	}

	public class CommandOverride<T> : Setting
	{
		[JsonProperty]
		public string Name { get; private set; }
		[JsonProperty]
		public ulong ID { get; private set; }
		[JsonProperty]
		public bool Enabled { get; private set; }

		public CommandOverride(string name, ulong id, bool enabled)
		{
			Name = name;
			ID = id;
			Enabled = enabled;
		}

		public void Switch()
		{
			Enabled = !Enabled;
		}
		public override string SettingToString()
		{
			return String.Format("**Command:** `{0}`\n**ID:** `{1}`\n**Enabled:** `{2}`", Name, ID, Enabled);
		}
		public override string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class CommandSwitch : Setting
	{
		[JsonProperty]
		public string Name { get; private set; }
		[JsonIgnore]
		public string[] Aliases { get; private set; }

		[JsonProperty]
		public bool Value { get; private set; }
		[JsonIgnore]
		public string ValAsString { get { return Value ? "ON" : "OFF"; } }
		[JsonIgnore]
		public int ValAsInteger { get { return Value ? 1 : -1; } }
		[JsonIgnore]
		public bool ValAsBoolean { get { return Value; } }

		[JsonProperty]
		public CommandCategory Category { get; private set; }
		[JsonIgnore]
		public string CategoryName { get { return Enum.GetName(typeof(CommandCategory), (int)Category); } }
		[JsonIgnore]
		public int CategoryValue { get { return (int)Category; } }

		[JsonIgnore]
		private HelpEntry mHelpEntry;

		public CommandSwitch(string name, bool value)
		{
			mHelpEntry = Variables.HelpList.FirstOrDefault(x => x.Name.Equals(name));
			if (mHelpEntry == null)
				return;
			Name = name;
			Value = value;
			Category = mHelpEntry.Category;
			Aliases = mHelpEntry.Aliases;
		}


		public void Disable()
		{
			Value = false;
		}
		public void Enable()
		{
			Value = true;
		}
		public override string SettingToString()
		{
			return String.Format("`{0}` `{1}`", ValAsString.PadRight(3), Name);
		}
		public override string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class BannedPhrase : Setting
	{
		[JsonProperty]
		public string Phrase { get; private set; }
		[JsonProperty]
		public PunishmentType Punishment { get; private set; }

		public BannedPhrase(string phrase, PunishmentType punishment)
		{
			Phrase = phrase;
			Punishment = (punishment == PunishmentType.Deafen || punishment == PunishmentType.Mute) ? PunishmentType.Nothing : punishment;
		}

		public void ChangePunishment(PunishmentType type)
		{
			Punishment = (type == PunishmentType.Deafen || type == PunishmentType.Mute) ? PunishmentType.Nothing : type;
		}
		public override string SettingToString()
		{
			return String.Format("`{0}` `{1}`", Enum.GetName(typeof(PunishmentType), Punishment).Substring(0, 1), Phrase);
		}
		public override string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class BannedPhrasePunishment : Setting
	{
		[JsonProperty]
		public int NumberOfRemoves { get; private set; }
		[JsonProperty]
		public PunishmentType Punishment { get; private set; }
		[JsonProperty]
		public ulong? RoleID { get; private set; }
		[JsonProperty]
		public ulong? GuildID { get; private set; }
		[JsonIgnore]
		public IRole Role { get; private set; }
		[JsonProperty]
		public int? PunishmentTime { get; private set; }

		public BannedPhrasePunishment(int number, PunishmentType punishment, ulong? guildID = null, ulong? roleID = null, int? punishmentTime = null)
		{
			NumberOfRemoves = number;
			Punishment = punishment;
			RoleID = roleID;
			GuildID = guildID;
			Role = RoleID != null && GuildID != null ? Variables.Client.GetGuild((ulong)GuildID)?.GetRole((ulong)RoleID) : null;
			PunishmentTime = punishmentTime;
		}
		public override string SettingToString()
		{
			return String.Format("`{0}.` `{1}`{2}",
				NumberOfRemoves.ToString("00"),
				Role == null ? Enum.GetName(typeof(PunishmentType), Punishment) : Role.Name,
				PunishmentTime == null ? "" : " `" + PunishmentTime + " minutes`");
		}
		public override string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class SelfAssignableGroup : Setting
	{
		[JsonProperty]
		public List<SelfAssignableRole> Roles { get; private set; }
		[JsonProperty]
		public int Group { get; private set; }

		public SelfAssignableGroup(int group)
		{
			Roles = new List<SelfAssignableRole>();
			Group = group;
		}

		public void AddRole(SelfAssignableRole role)
		{
			role.SetGroup(Group);
			Roles.Add(role);
		}
		public void AddRoles(IEnumerable<SelfAssignableRole> roles)
		{
			foreach (var role in roles)
			{
				role.SetGroup(Group);
			}
			Roles.AddRange(roles);
		}
		public void RemoveRoles(IEnumerable<ulong> roleIDs)
		{
			Roles.RemoveAll(x => roleIDs.Contains(x.Role.Id));
		}
		public override string SettingToString()
		{
			return String.Format("`Group: {0}`\n{1}", Group, String.Join("\n", Roles.Select(x => String.Format("`{0}`", x.Role.FormatRole()))));
		}
		public override string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class SelfAssignableRole : Setting
	{
		[JsonProperty]
		public ulong GuildID { get; private set; }
		[JsonProperty]
		public ulong RoleID { get; private set; }
		[JsonIgnore]
		public int Group { get; private set; }
		[JsonIgnore]
		public IRole Role { get; private set; }

		public SelfAssignableRole(ulong guildID, ulong roleID)
		{
			GuildID = guildID;
			RoleID = roleID;
			Role = Variables.Client.GetGuild(guildID).GetRole(roleID);
		}

		public void SetGroup(int group)
		{
			Group = group;
		}
		public override string SettingToString()
		{
			return String.Format("**Group:** `{0}`\n**Role:** `{1}`", Group, Role.FormatRole());
		}
		public override string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class BotImplementedPermissions : Setting
	{
		//TODO: Remove GuildID field?
		[JsonProperty]
		public ulong GuildID { get; private set; }
		[JsonProperty]
		public ulong UserID { get; private set; }
		[JsonProperty]
		public uint Permissions { get; private set; }
		[JsonIgnore]
		public IGuildUser User { get; private set; }

		public BotImplementedPermissions(ulong guildID, ulong userID, uint permissions, BotGuildInfo guildInfo = null)
		{
			GuildID = guildID;
			UserID = userID;
			Permissions = permissions;
			User = Variables.Client.GetGuild(guildID).GetUser(userID);
			if (guildInfo != null)
			{
				((List<BotImplementedPermissions>)guildInfo.GetSetting(SettingOnGuild.BotUsers)).ThreadSafeAdd(this);
			}
		}

		public void AddPermission(int add)
		{
			Permissions |= (1U << add);
		}
		public void RemovePermission(int remove)
		{
			Permissions &= ~(1U << remove);
		}
		public override string SettingToString()
		{
			return String.Format("**User:** `{0}`\n**Permissions:** `{1}`", User.FormatUser(), Permissions);
		}
		public override string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class GuildNotification : Setting
	{
		[JsonProperty]
		public string Content { get; private set; }
		[JsonProperty]
		public string Title { get; private set; }
		[JsonProperty]
		public string Description { get; private set; }
		[JsonProperty]
		public string ThumbURL { get; private set; }
		[JsonProperty]
		public ulong GuildID { get; private set; }
		[JsonProperty]
		public ulong ChannelID { get; private set; }
		[JsonIgnore]
		public EmbedBuilder Embed { get; private set; }
		[JsonIgnore]
		public ITextChannel Channel { get; private set; }

		public GuildNotification(string content, string title, string description, string thumbURL, ulong guildID, ulong channelID)
		{
			Content = content;
			Title = title;
			Description = description;
			ThumbURL = thumbURL;
			GuildID = guildID;
			ChannelID = channelID;
			if (!(String.IsNullOrWhiteSpace(title) && String.IsNullOrWhiteSpace(description) && String.IsNullOrWhiteSpace(thumbURL)))
			{
				Embed = Actions.MakeNewEmbed(title, description, null, null, null, thumbURL);
			}
			Channel = Variables.Client.GetGuild(GuildID).GetChannel(channelID) as ITextChannel;
		}

		public void ChangeChannel(ITextChannel channel)
		{
			Channel = channel;
		}
		public override string SettingToString()
		{
			return String.Format("**Channel:** `{0}`\n**Content:** `{1}`\n**Title:** `{2}`\n**Description:** `{3}`\n**Thumbnail:** `{4}`",
				Channel.FormatChannel(),
				Content,
				Title,
				Description,
				ThumbURL);
		}
		public override string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class ListedInvite : Setting
	{
		[JsonProperty]
		public ulong GuildID { get; private set; }
		[JsonProperty]
		public string Code { get; private set; }
		[JsonProperty]
		public string[] Keywords { get; private set; }
		[JsonProperty]
		public bool HasGlobalEmotes { get; private set; }
		[JsonIgnore]
		public DateTime LastBumped { get; private set; }
		[JsonIgnore]
		public string URL { get; private set; }
		[JsonIgnore]
		public SocketGuild Guild { get; private set; }

		public ListedInvite(ulong guildID, string code, string[] keywords)
		{
			GuildID = guildID;
			Guild = Variables.Client.GetGuild(GuildID);
			HasGlobalEmotes = Guild.Emotes.Any(x => x.IsManaged);
			LastBumped = DateTime.UtcNow;
			Code = code;
			URL = String.Concat("https://www.discord.gg/", Code);
			Keywords = keywords ?? new string[0];
		}

		public void UpdateKeywords(string[] keywords)
		{
			Keywords = keywords;
		}
		public void Bump()
		{
			LastBumped = DateTime.UtcNow;
			Variables.InviteList.ThreadSafeRemove(this);
			Variables.InviteList.ThreadSafeAdd(this);
		}
		public override string SettingToString()
		{
			if (!String.IsNullOrWhiteSpace(Code))
			{
				return null;
			}

			var codeStr = String.Format("**Code:** `{0}`\n");
			var keywordStr = "";
			if (Keywords.Any())
			{
				keywordStr = String.Format("**Keywords:**\n`{0}`", String.Join("`, `", Keywords));
			}
			return codeStr + keywordStr;
		}
		public override string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class Remind : Setting
	{
		[JsonProperty]
		public string Name { get; private set; }
		[JsonProperty]
		public string Text { get; private set; }

		public Remind(string name, string text)
		{
			Name = name;
			Text = text;
		}

		public override string SettingToString()
		{
			return String.Format("`{0}`", Name);
		}
		public override string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class DiscordObjectWithID<T> : Setting where T : ISnowflakeEntity
	{
		private readonly Dictionary<Type, Func<SocketGuild, ulong, dynamic>> inits = new Dictionary<Type, Func<SocketGuild, ulong, dynamic>>
		{
			{ typeof(IRole), (SocketGuild guild, ulong ID) => { return guild.GetRole(ID); } },
			{ typeof(ITextChannel), (SocketGuild guild, ulong ID) => { return guild.GetTextChannel(ID); } },
			{ typeof(SocketGuild), (SocketGuild guild, ulong ID) => { return Variables.Client.GetGuild(ID); } },
		};
		[JsonProperty]
		public ulong ID { get; private set; }
		[JsonIgnore]
		public T Object { get; private set; }

		[JsonConstructor]
		public DiscordObjectWithID(ulong id)
		{
			ID = id;
			Object = default(T);
		}
		public DiscordObjectWithID(T obj)
		{
			ID = obj?.Id ?? 0;
			Object = obj;
		}

		public void PostDeserialize(SocketGuild guild)
		{
			if (inits.TryGetValue(typeof(T), out var val))
			{
				Object = val(guild, ID);
			}
		}
		public override string SettingToString()
		{
			return Actions.FormatObject((dynamic)Object);
		}
		public override string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class PyramidalRoleSystem : Setting
	{
		/*  ▲
		 * ▲ ▲
		 * I originally thought this should resemble a pyramid;
		 * At this point, it's more of just the current Discord system except multiple things can occupy the same slot-
		 * Pyramidal Role System sounds a lot better than Bot Role System though.
		 */
		[JsonProperty]
		public Dictionary<int, ulong> Users { get; private set; }
		[JsonProperty]
		public Dictionary<int, ulong> Roles { get; private set; }

		public PyramidalRoleSystem()
		{
			Users = new Dictionary<int, ulong>();
			Roles = new Dictionary<int, ulong>();
		}

		public override string SettingToString()
		{
			var userStr = "";
			var users = Users.Select(x => String.Format("`{0}`: `{1}`", x.Key, x.Value));
			if (users.Any())
			{
				userStr = String.Format("**Users:**\n{0}", String.Join("\n", users));
			}
			var roleStr = "";
			var roles = Roles.Select(x => String.Format("`{0}`: `{1}`", x.Key, x.Value));
			if (roles.Any())
			{
				roleStr = String.Format("**Roles:**\n{0}", String.Join("\n", roles));
			}
			var spaceBetween = "";
			if (users.Any() && roles.Any())
			{
				spaceBetween = "\n";
			}
			return userStr + spaceBetween + roleStr;
		}
		public override string SettingToString(SocketGuild guild)
		{
			var userStr = "";
			var users = Users.Select(x => String.Format("`{0}`: `{1}`", x.Key, guild.GetUser(x.Value).FormatUser()));
			if (users.Any())
			{
				userStr = String.Format("**Users:**\n{0}", String.Join("\n", users));
			}
			var roleStr = "";
			var roles = Roles.Select(x => String.Format("`{0}`: `{1}`", x.Key, guild.GetRole(x.Value).FormatRole()));
			if (roles.Any())
			{
				roleStr = String.Format("**Roles:**\n{0}", String.Join("\n", roles));
			}
			var spaceBetween = "";
			if (users.Any() && roles.Any())
			{
				spaceBetween = "\n";
			}
			return userStr + spaceBetween + roleStr;
		}
	}
	#endregion

	#region Non-saved Classes
	public abstract class BotClient
	{
		public abstract BaseDiscordClient GetClient();
		public abstract SocketSelfUser GetCurrentUser();
		public abstract IUser GetUser(ulong id);
		public abstract IReadOnlyCollection<SocketGuild> GetGuilds();
		public abstract SocketGuild GetGuild(ulong id);
		public abstract IReadOnlyCollection<DiscordSocketClient> GetShards();
		public abstract DiscordSocketClient GetShardFor(IGuild guild);
		public abstract int GetLatency();
		public abstract Task StartAsync();
		public abstract Task StopAsync();
		public abstract Task LoginAsync(TokenType tokenType, string token);
		public abstract Task LogoutAsync();
		public abstract Task SetGameAsync(string game, string stream, StreamType streamType);
		public abstract Task<RestGuild> CreateGuildAsync(string name, IVoiceRegion region);
		public abstract Task<IVoiceRegion> GetOptimalVoiceRegionAsync();
		public abstract Task<RestInvite> GetInviteAsync(string code);
		public abstract Task<IEnumerable<IDMChannel>> GetDMChannelsAsync();
	}

	public class SocketClient : BotClient
	{
		private DiscordSocketClient mSocketClient;

		public SocketClient(DiscordSocketClient client) { mSocketClient = client; }

		public override BaseDiscordClient GetClient() { return mSocketClient; }
		public override SocketSelfUser GetCurrentUser() { return mSocketClient.CurrentUser; }
		public override IUser GetUser(ulong id) { return mSocketClient.GetUser(id); }
		public override IReadOnlyCollection<SocketGuild> GetGuilds() { return mSocketClient.Guilds; }
		public override SocketGuild GetGuild(ulong id) { return mSocketClient.GetGuild(id); }
		public override IReadOnlyCollection<DiscordSocketClient> GetShards() { return new[] { mSocketClient }; }
		public override DiscordSocketClient GetShardFor(IGuild guild) { return mSocketClient; }
		public override int GetLatency() { return mSocketClient.Latency; }
		public override async Task StartAsync() { await mSocketClient.StartAsync(); }
		public override async Task StopAsync() { await mSocketClient.StopAsync(); }
		public override async Task LoginAsync(TokenType tokenType, string token) { await mSocketClient.LoginAsync(tokenType, token); }
		public override async Task LogoutAsync() { await mSocketClient.LogoutAsync(); }
		public override async Task SetGameAsync(string game, string stream, StreamType streamType) { await mSocketClient.SetGameAsync(game, stream, streamType); }
		public override async Task<RestGuild> CreateGuildAsync(string name, IVoiceRegion region) { return await mSocketClient.CreateGuildAsync(name, region); }
		public override async Task<IVoiceRegion> GetOptimalVoiceRegionAsync() { return await mSocketClient.GetOptimalVoiceRegionAsync(); }
		public override async Task<RestInvite> GetInviteAsync(string code) { return await mSocketClient.GetInviteAsync(code); }
		public override async Task<IEnumerable<IDMChannel>> GetDMChannelsAsync() { return await mSocketClient.GetDMChannelsAsync(); }
	}

	public class ShardedClient : BotClient
	{
		private DiscordShardedClient mShardedClient;

		public ShardedClient(DiscordShardedClient client) { mShardedClient = client; }

		public override BaseDiscordClient GetClient() { return mShardedClient; }
		public override SocketSelfUser GetCurrentUser() { return mShardedClient.Shards.FirstOrDefault().CurrentUser; }
		public override IUser GetUser(ulong id) { return mShardedClient.GetUser(id); }
		public override IReadOnlyCollection<SocketGuild> GetGuilds() { return mShardedClient.Guilds; }
		public override SocketGuild GetGuild(ulong id) { return mShardedClient.GetGuild(id); }
		public override IReadOnlyCollection<DiscordSocketClient> GetShards() { return mShardedClient.Shards; }
		public override DiscordSocketClient GetShardFor(IGuild guild) { return mShardedClient.GetShardFor(guild); }
		public override int GetLatency() { return mShardedClient.Latency; }
		public override async Task StartAsync() { await mShardedClient.StartAsync(); }
		public override async Task StopAsync() { await mShardedClient.StopAsync(); }
		public override async Task LoginAsync(TokenType tokenType, string token) { await mShardedClient.LoginAsync(tokenType, token); }
		public override async Task LogoutAsync() { await mShardedClient.LogoutAsync(); }
		public override async Task SetGameAsync(string game, string stream, StreamType streamType) { await mShardedClient.SetGameAsync(game, stream, streamType); }
		public override async Task<RestGuild> CreateGuildAsync(string name, IVoiceRegion region) { return await mShardedClient.CreateGuildAsync(name, region); }
		public override async Task<IVoiceRegion> GetOptimalVoiceRegionAsync() { return await mShardedClient.GetOptimalVoiceRegionAsync(); }
		public override async Task<RestInvite> GetInviteAsync(string code) { return await mShardedClient.GetInviteAsync(code); }
		public override async Task<IEnumerable<IDMChannel>> GetDMChannelsAsync() { return await mShardedClient.GetDMChannelsAsync(); }
	}

	public class HelpEntry
	{
		public string Name { get; private set; }
		public string[] Aliases { get; private set; }
		public string Usage { get; private set; }
		public string BasePerm { get; private set; }
		public string Text { get; private set; }
		public CommandCategory Category { get; private set; }
		public bool DefaultEnabled { get; private set; }

		public HelpEntry(string name, string[] aliases, string usage, string basePerm, string text, CommandCategory category, bool defaultEnabled)
		{
			Name = name;
			Aliases = aliases;
			Usage = Variables.BotInfo.Prefix + usage;
			BasePerm = basePerm;
			Text = text;
			Category = category;
			DefaultEnabled = defaultEnabled;
		}
	}

	public class BotInvite
	{
		public ulong GuildID { get; private set; }
		public string Code { get; private set; }
		public int Uses { get; private set; }

		public BotInvite(ulong guildID, string code, int uses)
		{
			GuildID = guildID;
			Code = code;
			Uses = uses;
		}

		public void IncreaseUses()
		{
			++Uses;
		}
	}

	public class SlowmodeUser : ITimeInterface
	{
		public IGuildUser User { get; private set; }
		public int CurrentMessagesLeft { get; private set; }
		public int BaseMessages { get; private set; }
		public int Interval { get; private set; }
		public DateTime Time { get; private set; }

		public SlowmodeUser(IGuildUser user = null, int currentMessagesLeft = 1, int baseMessages = 1, int interval = 5)
		{
			User = user;
			CurrentMessagesLeft = currentMessagesLeft;
			BaseMessages = baseMessages;
			Interval = interval;
		}

		public void LowerMessagesLeft()
		{
			--CurrentMessagesLeft;
		}
		public void ResetMessagesLeft()
		{
			CurrentMessagesLeft = BaseMessages;
		}
		public void SetNewTime(DateTime time)
		{
			Time = time;
		}
		public DateTime GetTime()
		{
			return Time;
		}
	}

	public class BannedPhraseUser
	{
		public IGuildUser User { get; private set; }
		public int MessagesForRole { get; private set; }
		public int MessagesForKick { get; private set; }
		public int MessagesForBan { get; private set; }

		public BannedPhraseUser(IGuildUser user, BotGuildInfo guildInfo = null)
		{
			User = user;
			if (guildInfo != null)
			{
				guildInfo.BannedPhraseUsers.Add(this);
			}
		}

		public void IncreaseRoleCount()
		{
			++MessagesForRole;
		}
		public void ResetRoleCount()
		{
			MessagesForRole = 0;
		}
		public void IncreaseKickCount()
		{
			++MessagesForKick;
		}
		public void ResetKickCount()
		{
			MessagesForKick = 0;
		}
		public void IncreaseBanCount()
		{
			++MessagesForBan;
		}
		public void ResetBanCount()
		{
			MessagesForBan = 0;
		}
	}

	public class MessageDeletion
	{
		public CancellationTokenSource CancelToken { get; private set; }
		private List<IMessage> mMessages = new List<IMessage>();

		public void SetCancelToken(CancellationTokenSource cancelToken)
		{
			CancelToken = cancelToken;
		}
		public List<ISnowflakeEntity> GetList()
		{
			return mMessages.Select(x => x as ISnowflakeEntity).ToList();
		}
		public void SetList(List<ISnowflakeEntity> InList)
		{
			mMessages = InList.Select(x => x as IMessage).ToList();
		}
		public void AddToList(ISnowflakeEntity Item)
		{
			mMessages.Add(Item as IMessage);
		}
		public void ClearList()
		{
			mMessages.Clear();
		}
	}

	public class SlowmodeGuild
	{
		public List<SlowmodeUser> Users { get; private set; }

		public SlowmodeGuild(List<SlowmodeUser> users)
		{
			Users = users;
		}
	}

	public class SlowmodeChannel
	{
		public ulong ChannelID { get; private set; }
		public List<SlowmodeUser> Users { get; private set; }

		public SlowmodeChannel(ulong channelID)
		{
			ChannelID = channelID;
			Users = new List<SlowmodeUser>();
		}
		public SlowmodeChannel(ulong channelID, List<SlowmodeUser> users)
		{
			ChannelID = channelID;
			Users = users;
		}

		public void SetUserList(List<SlowmodeUser> users)
		{
			Users = users;
		}
	}
	#endregion

	#region Spam Prevention
	public class SpamPreventionUser
	{
		public IGuildUser User { get; private set; }
		public int VotesToKick { get; private set; }
		public int VotesRequired { get; private set; }
		public bool PotentialKick { get; private set; }
		public bool AlreadyKicked { get; private set; }
		public List<ulong> UsersWhoHaveAlreadyVoted { get; private set; }
		public Dictionary<SpamType, List<BasicTimeInterface>> SpamLists { get; private set; }

		public SpamPreventionUser(IGuildUser user)
		{
			User = user;
			VotesToKick = 0;
			VotesRequired = int.MaxValue;
			PotentialKick = false;
			AlreadyKicked = false;
			UsersWhoHaveAlreadyVoted = new List<ulong>();
			SpamLists = new Dictionary<SpamType, List<BasicTimeInterface>>();
			foreach (var spamType in Enum.GetValues(typeof(SpamType)).Cast<SpamType>())
			{
				SpamLists.Add(spamType, new List<BasicTimeInterface>());
			}
		}

		public void IncreaseVotesToKick(ulong ID)
		{
			UsersWhoHaveAlreadyVoted.ThreadSafeAdd(ID);
			++VotesToKick;
		}
		public void ChangeVotesRequired(int input)
		{
			VotesRequired = Math.Min(input, VotesRequired);
		}
		public void EnablePotentialKick()
		{
			PotentialKick = true;
		}
		public void ResetSpamUser()
		{
			VotesToKick = 0;
			VotesRequired = int.MaxValue;
			PotentialKick = false;
			UsersWhoHaveAlreadyVoted = new List<ulong>();
			SpamLists.Values.ToList().ForEach(x =>
			{
				x.Clear();
			});
		}
		public bool CheckIfAllowedToPunish(SpamPrevention spamPrev, SpamType spamType, IMessage msg)
		{
			return Actions.GetCountOfItemsInTimeFrame(SpamLists[spamType], spamPrev.TimeInterval) >= spamPrev.RequiredSpamInstances;
		}
	}

	public class SpamPrevention : Setting
	{
		[JsonProperty]
		public PunishmentType PunishmentType { get; private set; }
		[JsonProperty]
		public int TimeInterval { get; private set; }
		[JsonProperty]
		public int RequiredSpamInstances { get; private set; }
		[JsonProperty]
		public int RequiredSpamPerMessage { get; private set; }
		[JsonProperty]
		public int VotesForKick { get; private set; }
		[JsonProperty]
		public bool Enabled { get; private set; }
		[JsonIgnore]
		public List<IGuildUser> PunishedUsers { get; private set; }

		public SpamPrevention(PunishmentType punishmentType, int timeInterval, int requiredSpamInstances, int requiredSpamPerMessage, int votesForKick)
		{
			PunishmentType = punishmentType;
			TimeInterval = timeInterval;
			RequiredSpamInstances = requiredSpamInstances;
			RequiredSpamPerMessage = requiredSpamPerMessage;
			VotesForKick = votesForKick;
			Enabled = true;
		}

		public void Disable()
		{
			Enabled = false;
		}
		public void Enable()
		{
			Enabled = true;
		}
		public async Task PunishUser(IGuildUser user)
		{
			var guild = user.Guild;
			var guildInfo = await Actions.GetGuildInfo(guild);
			switch (PunishmentType)
			{
				case PunishmentType.Ban:
				{
					await guild.AddBanAsync(user);
					break;
				}
				case PunishmentType.Kick:
				{
					await user.KickAsync();
					break;
				}
				case PunishmentType.Kick_Then_Ban:
				{
					await (guildInfo.SpamPreventionUsers.FirstOrDefault(x => x.User.Id == user.Id).AlreadyKicked ? guild.AddBanAsync(user) : user.KickAsync());
					break;
				}
				case PunishmentType.Role:
				{
					await Actions.GiveRole(user, ((DiscordObjectWithID<IRole>)guildInfo.GetSetting(SettingOnGuild.MuteRole))?.Object);
					break;
				}
			}
			PunishedUsers.ThreadSafeAdd(user);
		}
		public override string SettingToString()
		{
			return String.Format("**Enabled:** `{0}`\n**Spam Instances:** `{1}`\n**Spam Amount/Time Interval:** `{2}`\n**Votes Needed For Kick:** `{3}`\n**Punishment:** `{4}`",
				Enabled,
				RequiredSpamInstances,
				RequiredSpamPerMessage,
				VotesForKick,
				Enum.GetName(typeof(PunishmentType), PunishmentType));
		}
		public override string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class RaidPrevention : Setting
	{
		[JsonProperty]
		public PunishmentType PunishmentType { get; private set; }
		[JsonProperty]
		public int TimeInterval { get; private set; }
		[JsonProperty]
		public int RequiredCount { get; private set; }
		[JsonProperty]
		public bool Enabled { get; private set; }
		[JsonIgnore]
		public List<BasicTimeInterface> TimeList { get; private set; }
		[JsonIgnore]
		public List<IGuildUser> PunishedUsers { get; private set; }

		public RaidPrevention(PunishmentType punishmentType, int timeInterval, int requiredCount)
		{
			PunishmentType = punishmentType;
			TimeInterval = timeInterval;
			RequiredCount = requiredCount;
			TimeList = new List<BasicTimeInterface>();
			Enabled = true;
		}

		public int GetSpamCount()
		{
			return Actions.GetCountOfItemsInTimeFrame(TimeList, TimeInterval);
		}
		public void Add(DateTime time)
		{
			TimeList.ThreadSafeAdd(new BasicTimeInterface(time));
		}
		public void Remove(DateTime time)
		{
			TimeList.ThreadSafeRemoveAll(x =>
			{
				return x.GetTime().Equals(time);
			});
		}
		public void Disable()
		{
			Enabled = false;
		}
		public void Enable()
		{
			Enabled = true;
		}
		public void Reset()
		{
			TimeList = new List<BasicTimeInterface>();
		}
		public async Task PunishUser(IGuildUser user)
		{
			var guild = user.Guild;
			var guildInfo = await Actions.GetGuildInfo(guild);
			switch (PunishmentType)
			{
				case PunishmentType.Ban:
				{
					await guild.AddBanAsync(user);
					break;
				}
				case PunishmentType.Kick:
				{
					await user.KickAsync();
					break;
				}
				case PunishmentType.Kick_Then_Ban:
				{
					await (guildInfo.SpamPreventionUsers.FirstOrDefault(x => x.User.Id == user.Id).AlreadyKicked ? guild.AddBanAsync(user) : user.KickAsync());
					break;
				}
				case PunishmentType.Role:
				{
					await Actions.GiveRole(user, ((DiscordObjectWithID<IRole>)guildInfo.GetSetting(SettingOnGuild.MuteRole))?.Object);
					break;
				}
			}
			PunishedUsers.ThreadSafeAdd(user);
		}
		public override string SettingToString()
		{
			return String.Format("**Enabled:** `{0}`\n**Users:** `{1}`\n**Time Interval:** `{2}`\n**Punishment:** `{3}`",
				Enabled,
				RequiredCount,
				TimeInterval,
				Enum.GetName(typeof(PunishmentType), PunishmentType));
		}
		public override string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}
	#endregion

	#region Structs
	public struct BotGuildPermissionType
	{
		public string Name { get; private set; }
		public int Position { get; private set; }

		public BotGuildPermissionType(string name, int position)
		{
			Name = name;
			Position = position;
		}
	}

	public struct BotChannelPermissionType
	{
		public string Name { get; private set; }
		public int Position { get; private set; }
		public bool General { get; private set; }
		public bool Text { get; private set; }
		public bool Voice { get; private set; }

		public BotChannelPermissionType(string name, int position, bool gen = false, bool text = false, bool voice = false)
		{
			Name = name;
			Position = position;
			General = gen;
			Text = text;
			Voice = voice;
		}
	}

	public struct ActiveCloseWords : ITimeInterface
	{
		public ulong UserID { get; private set; }
		public List<CloseWord> List { get; private set; }
		public DateTime Time { get; private set; }

		public ActiveCloseWords(ulong userID, List<CloseWord> list)
		{
			UserID = userID;
			List = list;
			Time = DateTime.UtcNow.AddMilliseconds(Constants.ACTIVE_CLOSE);
		}

		public DateTime GetTime()
		{
			return Time;
		}
	}

	public struct CloseWord
	{
		public string Name { get; private set; }
		public int Closeness { get; private set; }

		public CloseWord(string name, int closeness)
		{
			Name = name;
			Closeness = closeness;
		}
	}

	public struct ActiveCloseHelp : ITimeInterface
	{
		public ulong UserID { get; private set; }
		public List<CloseHelp> List { get; private set; }
		public DateTime Time { get; private set; }

		public ActiveCloseHelp(ulong userID, List<CloseHelp> list)
		{
			UserID = userID;
			List = list;
			Time = DateTime.UtcNow.AddMilliseconds(Constants.ACTIVE_CLOSE);
		}

		public DateTime GetTime()
		{
			return Time;
		}
	}

	public struct CloseHelp
	{
		public HelpEntry Help { get; private set; }
		public int Closeness { get; private set; }

		public CloseHelp(HelpEntry help, int closeness)
		{
			Help = help;
			Closeness = closeness;
		}
	}

	public struct RemovablePunishment : ITimeInterface
	{
		public IGuild Guild { get; private set; }
		public ulong UserID { get; private set; }
		public PunishmentType Type { get; private set; }
		public IRole Role { get; private set; }
		public DateTime Time { get; private set; }

		public RemovablePunishment(IGuild guild, ulong userID, PunishmentType type, DateTime time)
		{
			Guild = guild;
			UserID = userID;
			Type = type;
			Time = time;
			Role = null;
		}
		public RemovablePunishment(IGuild guild, ulong userID, IRole role, DateTime time)
		{
			Guild = guild;
			UserID = userID;
			Type = PunishmentType.Role;
			Time = time;
			Role = role;
		}

		public DateTime GetTime()
		{
			return Time;
		}
	}

	public struct RemovableMessage : ITimeInterface
	{
		public IMessage Message { get; private set; }
		public List<IMessage> Messages { get; private set; }
		public DateTime Time { get; private set; }

		public RemovableMessage(IMessage message, DateTime time)
		{
			Message = message;
			Messages = null;
			Time = time;
		}
		public RemovableMessage(List<IMessage> messages, DateTime time)
		{
			Message = null;
			Messages = messages;
			Time = time;
		}

		public DateTime GetTime()
		{
			return Time;
		}
	}

	public struct EditableDiscordObject<T>
	{
		public List<T> Success { get; private set; }
		public List<string> Failure { get; private set; }

		public EditableDiscordObject(List<T> success, List<string> failure)
		{
			Success = success;
			Failure = failure;
		}
	}

	public struct ReturnedDiscordObject<T>
	{
		public T Object { get; private set; }
		public FailureReason Reason { get; private set; }

		public ReturnedDiscordObject(T obj, FailureReason reason)
		{
			Object = obj;
			Reason = reason;
		}
	}

	public struct ReturnedType<T>
	{
		public T Type { get; private set; }
		public TypeFailureReason Reason { get; private set; }

		public ReturnedType(T type, TypeFailureReason reason)
		{
			Type = type;
			Reason = reason;
		}
	}

	public struct ReturnedArguments
	{
		public List<string> Arguments { get; private set; }
		public int ArgCount { get; private set; }
		public Dictionary<string, string> SpecifiedArguments { get; private set; }
		public List<ulong> MentionedUsers { get; private set; }
		public List<ulong> MentionedRoles { get; private set; }
		public List<ulong> MentionedChannels { get; private set; }
		public ArgFailureReason Reason { get; private set; }

		public ReturnedArguments(List<string> args, ArgFailureReason reason)
		{
			Arguments = args;
			ArgCount = args.Where(x => !String.IsNullOrWhiteSpace(x)).Count();
			SpecifiedArguments = null;
			MentionedUsers = null;
			MentionedRoles = null;
			MentionedChannels = null;
			Reason = reason;
		}
		public ReturnedArguments(List<string> args, Dictionary<string, string> specifiedArgs, IMessage message)
		{
			Arguments = args;
			ArgCount = args.Where(x => !String.IsNullOrWhiteSpace(x)).Count();
			SpecifiedArguments = specifiedArgs;
			MentionedUsers = message.MentionedUserIds.ToList();
			MentionedRoles = message.MentionedRoleIds.ToList();
			MentionedChannels = message.MentionedChannelIds.ToList();
			Reason = ArgFailureReason.Not_Failure;
		}

		public string GetSpecifiedArg(string input)
		{
			if (SpecifiedArguments.TryGetValue(input, out string value))
			{
				return value;
			}
			else
			{
				return null;
			}
		}
	}

	public struct ReturnedBannedUser
	{
		public IBan Ban { get; private set; }
		public BannedUserFailureReason Reason { get; private set; }
		public List<IBan> MatchedBans { get; private set; }

		public ReturnedBannedUser(IBan ban, BannedUserFailureReason reason, List<IBan> matchedBans = null)
		{
			Ban = ban;
			Reason = reason;
			MatchedBans = matchedBans;
		}
	}

	public struct ReturnedSetting
	{
		public String Setting { get; private set; }
		public NSF Status { get; private set; }

		public ReturnedSetting(SettingOnBot setting, NSF status)
		{
			Setting = Enum.GetName(typeof(SettingOnBot), setting);
			Status = status;
		}
	}

	public struct BasicTimeInterface : ITimeInterface
	{
		private DateTime mTime;

		public BasicTimeInterface(DateTime time)
		{
			mTime = time.ToUniversalTime();
		}

		public DateTime GetTime()
		{
			return mTime;
		}
	}

	public struct ArgNumbers
	{
		public int Min { get; private set; }
		public int Max { get; private set; }

		public ArgNumbers(int min, int max)
		{
			Min = min;
			Max = max;
		}
	}

	public struct GuildFileInformation
	{
		public ulong ID { get; private set; }
		public string Name { get; private set; }
		public int MemberCount { get; private set; }

		public GuildFileInformation(ulong id, string name, int memberCount)
		{
			ID = id;
			Name = name;
			MemberCount = memberCount;
		}
	}

	public struct FileInformation
	{
		public FileType FileType { get; private set; }
		public string FileLocation { get; private set; }

		public FileInformation(FileType fileType, string fileLocation)
		{
			FileType = fileType;
			FileLocation = fileLocation;
		}
	}

	public struct VerifiedLoggingAction
	{
		public SocketGuild Guild { get; private set; }
		public BotGuildInfo GuildInfo { get; private set; }
		public ITextChannel LoggingChannel { get; private set; }

		public VerifiedLoggingAction(SocketGuild guild, BotGuildInfo guildInfo, ITextChannel loggingChannel)
		{
			Guild = guild;
			GuildInfo = guildInfo;
			LoggingChannel = loggingChannel;
		}
	}
	#endregion

	#region Interfaces
	public interface ITimeInterface
	{
		DateTime GetTime();
	}
	#endregion

	#region Enums
	public enum LogActions
	{
		//Legacy numbering due to deletion of certain enums because of audit logs
		UserJoined = 1,
		UserLeft = 2,
		UserUpdated = 5,
		MessageReceived = 7,
		MessageUpdated = 8,
		MessageDeleted = 9,
	}

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
		Miscellaneous = 10,
		Spam_Prevention = 11,
		Invite_Moderation = 15,
		Guild_List = 13,
		Nickname_Moderation = 14,
	}

	public enum PunishmentType
	{
		Nothing = 0,
		Kick = 1,
		Ban = 2,
		Role = 3,
		Deafen = 4,
		Mute = 5,
		Kick_Then_Ban = 6,
	}

	public enum DeleteInvAction
	{
		User = 1,
		Channel = 2,
		Uses = 3,
		Expiry = 4,
	}

	public enum SpamType
	{
		Message = 1,
		Long_Message = 2,
		Link = 3,
		Image = 4,
		Mention = 5,
	}

	public enum RaidType
	{
		Regular = 1,
		Rapid_Joins = 2,
	}

	public enum FAWRType
	{
		Give_Role = 1,
		GR = 2,
		Take_Role = 3,
		TR = 4,
		Give_Nickname = 5,
		GNN = 6,
		Take_Nickname = 7,
		TNN = 8,
	}

	public enum ActionType
	{
		Nothing = 0,
		Show = 1,
		Allow = 2,
		Inherit = 3,
		Deny = 4,
		Enable = 5,
		Disable = 6,
		Setup = 7,
		Create = 8,
		Add = 9,
		Remove = 10,
		Delete = 11,
		Clear = 12,
		Current = 13,
	}

	public enum FailureReason
	{
		Not_Failure = 0,
		Not_Found = 1,
		User_Inability = 2,
		Bot_Inability = 3,
		Too_Many = 4,
		Incorrect_Channel_Type = 5,
		Everyone_Role = 6,
		Managed_Role = 7,
	}

	public enum SettingOnGuild
	{
		Nothing							= 0,
		CommandPreferences				= 1,
		CommandsDisabledOnChannel		= 2,
		BotUsers						= 3,
		SelfAssignableGroups			= 4,
		Reminds							= 5,
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
		Guild							= 33,
	}

	public enum SettingOnBot
	{
		BotOwner = 0,
		TrustedUsers = 1,
		Prefix = 2,
		Game = 3,
		Stream = 4,
		ShardCount = 5,
		MessageCacheSize = 6,
		AlwaysDownloadUsers = 7,
		LogLevel = 8,
		SavePath = 9,
		MaxUserGatherCount = 11,
	}

	public enum GuildNotifications
	{
		Welcome = 0,
		Goodbye = 1,
	}

	public enum LogChannelTypes
	{
		Server = 0,
		Mod = 1,
		Image = 2,
	}

	public enum UserCheck
	{
		None = 0,
		Can_Be_Moved_From_Channel = 1,
		Can_Be_Edited = 2,
	}

	public enum RoleCheck
	{
		None = 0,
		Can_Be_Edited = 1,
		Is_Everyone = 2,
		Is_Managed = 3,
	}

	public enum ChannelCheck
	{
		None = 0,
		Can_Be_Reordered = 1,
		Can_Modify_Permissions = 2,
		Can_Be_Managed = 3,
		Is_Voice = 4,
		Is_Text = 5,
		Can_Move_Users = 6,
		Can_Delete_Messages = 7,
	}

	public enum ArgFailureReason
	{
		Not_Failure = 0,
		Too_Many_Args = 1,
		Too_Few_Args = 2,
		Missing_Critical_Args = 3,
		Max_Less_Than_Min = 4,
	}

	public enum TypeFailureReason
	{
		Not_Failure = 0,
		Not_Found = 1,
		Invalid_Type = 2,
	}

	public enum BannedUserFailureReason
	{
		Not_Failure = 0,
		No_Bans = 1,
		No_Match = 2,
		Too_Many_Matches = 3,
		Invalid_Discriminator = 4,
		Invalid_ID = 5,
		No_Username_Or_ID = 6,
	}

	public enum CCEnum
	{
		Clear = 0,
		Current = 1,
	}

	public enum NSF
	{
		Nothing = 0,
		Success = 1,
		Failure = 2,
	}

	public enum FileType
	{
		GuildInfo = 0,
	}

	[Flags]
	public enum Precondition
	{
		User_Has_A_Perm = 1,
		Guild_Owner = 2,
		Trusted_User = 4,
		Bot_Owner = 8,
	}

	public enum ChannelSettings
	{
		ImageOnly = 0,
		Sanitary = 1,
	}
	#endregion
}