using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
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
	public class PermissionRequirementAttribute : PreconditionAttribute
	{
		public PermissionRequirementAttribute(uint anyOfTheListedPerms = 0, uint allOfTheListedPerms = 0)
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
			get { return String.Join(" & ", Actions.GetPermissionNames(mAllFlags)); }
		}

		public string AnyText
		{
			get { return String.Join("|", Actions.GetPermissionNames(mAnyFlags)); }
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
			return (await Actions.GetIfUserIsOwner(context.Guild, context.User)) || Actions.GetIfUserIsBotOwner(context.Guild, context.User) ?
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
				return Actions.GetIfUserIsBotOwner(context.Guild, context.User) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(Constants.IGNORE_ERROR);
			});
		}
	}

	//Testing if the user if the guild owner
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class GuildOwnerRequirementAttribute : PreconditionAttribute
	{
		public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
		{
			return await Actions.GetIfUserIsOwner(context.Guild, context.User) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(Constants.IGNORE_ERROR);
		}
	}

	//Check if the user has any permission that would allow them to use the bot regularly
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class UserHasAPermissionAttribute : PreconditionAttribute
	{
		private const UInt32 PERMISSIONBITS = 0
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

		public string Usage
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
			get { return Properties.Settings.Default.Prefix + mName + " " + mUsage; }
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

		public string Name
		{
			get { return mName; }
		}
		public string CategoryName
		{
			get { return Enum.GetName(typeof(CommandCategory), (int)mCategory); }
		}
		public int CategoryValue
		{
			get { return (int)mCategory; }
		}
		public CommandCategory CategoryEnum
		{
			get { return mCategory; }
		}
		public string[] Aliases
		{
			get { return mAliases; }
		}

		public bool ValAsBoolean
		{
			get
			{
				string[] trueMatches = { "true", "on", "yes", "1" };
				return trueMatches.Any(x => String.Equals(mValue.Trim(), x, StringComparison.OrdinalIgnoreCase));
			}
		}
		public string ValAsString
		{
			get { return mValue.Trim(new char[] { '\n', '\r' }); }
		}
		public int ValAsInteger
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

		public void Disable()
		{
			mValue = "OFF";
		}
		public void Enable()
		{
			mValue = "ON";
		}
	}

	public class SlowmodeUser
	{
		public SlowmodeUser(IGuildUser user = null, int currentMessagesLeft = 1, int baseMessages = 1, int time = 5)
		{
			mUser = user;
			mCurrentMessagesLeft = currentMessagesLeft;
			mBaseMessages = baseMessages;
			mTime = time;
		}

		private IGuildUser mUser;
		private int mCurrentMessagesLeft;
		private int mBaseMessages;
		private int mTime;

		public IGuildUser User
		{
			get { return mUser; }
		}
		public int CurrentMessagesLeft
		{
			get { return mCurrentMessagesLeft; }
		}
		public int BaseMessages
		{
			get { return mBaseMessages; }
		}
		public int Time
		{
			get { return mTime; }
		}

		public void LowerMessagesLeft()
		{
			--mCurrentMessagesLeft;
		}
		public void ResetMessagesLeft()
		{
			mCurrentMessagesLeft = mBaseMessages;
		}
	}

	public class BannedPhrasePunishment
	{
		public BannedPhrasePunishment(int number, PunishmentType punishment, IRole role = null, int? punishmentTime = null)
		{
			mNumberOfRemoves = number;
			mPunishment = punishment;
			mRole = role;
			mPunishmentTime = punishmentTime;
		}

		private int mNumberOfRemoves;
		private PunishmentType mPunishment;
		private IRole mRole;
		private int? mPunishmentTime;

		public int NumberOfRemoves
		{
			get { return mNumberOfRemoves; }
		}
		public PunishmentType Punishment
		{
			get { return mPunishment; }
		}
		public IRole Role
		{
			get { return mRole; }
		}
		public int? PunishmentTime
		{
			get { return mPunishmentTime; }
		}
	}

	public class BannedPhraseUser
	{
		public BannedPhraseUser(IGuildUser user, int amountOfRemovedMessages = 1)
		{
			mUser = user;
			mAmountOfRemovedMessages = amountOfRemovedMessages;
		}

		private IGuildUser mUser;
		private int mAmountOfRemovedMessages;

		public IGuildUser User
		{
			get { return mUser; }
		}
		public int AmountOfRemovedMessages
		{
			get { return mAmountOfRemovedMessages; }
		}

		public void IncreaseAmountOfRemovedMessages()
		{
			++mAmountOfRemovedMessages;
		}
		public void ResetAmountOfRemovesMessages()
		{
			mAmountOfRemovedMessages = 0;
		}
	}

	public class SelfAssignableRole
	{
		public SelfAssignableRole(IRole role, int group)
		{
			mRole = role;
			mGroup = group;
		}

		private IRole mRole;
		private int mGroup;

		public IRole Role
		{
			get { return mRole; }
		}
		public int Group
		{
			get { return mGroup; }
		}
	}

	public class SelfAssignableGroup
	{
		public SelfAssignableGroup(List<SelfAssignableRole> roles, int group, ulong guildID)
		{
			mRoles = roles;
			mGroup = group;
			mGuildID = guildID;
		}

		private List<SelfAssignableRole> mRoles;
		private int mGroup;
		private ulong mGuildID;

		public IReadOnlyCollection<SelfAssignableRole> Roles
		{
			get { return mRoles.AsReadOnly(); }
		}
		public int Group
		{
			get { return mGroup; }
		}
		public ulong GuildID
		{
			get { return mGuildID; }
		}

		public void AddRole(SelfAssignableRole role)
		{
			mRoles.Add(role);
		}
		public void AddRoles(List<SelfAssignableRole> roles)
		{
			mRoles.AddRange(roles);
		}
		public void RemoveRoles(List<ulong> roleIDs)
		{
			mRoles.RemoveAll(x => roleIDs.Contains(x.Role.Id));
		}
		public string FormatSaveString()
		{
			return String.Join("\n", mRoles.Select(y => String.Format("{0} {1}", y.Role.Id, y.Group)).ToList());
		}
	}

	public class BotInvite
	{
		public BotInvite(ulong guildID, string code, int uses)
		{
			mGuildID = guildID;
			mCode = code;
			mUses = uses;
		}

		private ulong mGuildID;
		private string mCode;
		private int mUses;

		public ulong GuildID
		{
			get { return mGuildID; }
		}
		public string Code
		{
			get { return mCode; }
		}
		public int Uses
		{
			get { return mUses; }
		}

		public void IncreaseUses()
		{
			++mUses;
		}
	}

	public class BotGuildInfo
	{
		public BotGuildInfo(IGuild guild)
		{
			Guild = guild;
		}

		//Banned phrases/regex/punishment
		public List<string> BannedPhrases = new List<string>();
		public List<Regex> BannedRegex = new List<Regex>();
		public List<BannedPhrasePunishment> BannedPhrasesPunishments = new List<BannedPhrasePunishment>();

		//Commands and logging
		public List<CommandSwitch> CommandSettings = new List<CommandSwitch>();
		public List<ulong> IgnoredCommandChannels = new List<ulong>();
		public List<LogActions> LogActions = new List<LogActions>();
		public List<ulong> IgnoredLogChannels = new List<ulong>();

		//Spam prevention
		public List<SpamPreventionUser> SpamPreventionUsers = new List<SpamPreventionUser>();
		public MentionSpamPrevention MentionSpamPrevention;
		public LongMessageSpamPrevention LongMessageSpamPrevention;

		//Raid prevention
		public bool RaidPrevention = false;
		public IRole MuteRole;
		public List<IGuildUser> UsersWhoHaveBeenMuted = new List<IGuildUser>();

		//Misc
		public List<Remind> Reminds = new List<Remind>();
		public List<BotInvite> Invites = new List<BotInvite>();
		public bool DefaultPrefs = true;
		public string Prefix;
		public IGuild Guild;
	}

	public class BotImplementedPermissions
	{
		public BotImplementedPermissions(IGuildUser user, uint permissions)
		{
			mUser = user;
			mPermissions = permissions;
		}

		private IGuildUser mUser;
		private uint mPermissions;

		public IGuildUser User
		{
			get { return mUser; }
		}
		public uint Permissions
		{
			get { return mPermissions; }
		}

		public void AddPermission(int add)
		{
			mPermissions |= (1U << add);
		}
		public void RemovePermission(int remove)
		{
			mPermissions &= ~(1U << remove);
		}
	}

	public class SpamPreventionUser
	{
		public SpamPreventionUser(IGuildUser user, int currentSpamAmount)
		{
			mUser = user;
			mCurrentSpamAmount = currentSpamAmount;
		}

		private IGuildUser mUser;
		private int mVotesToKick;
		private int mCurrentSpamAmount;
		private bool mAlreadyKicked = false;
		private bool mPotentialKick = false;
		private List<ulong> mUsersWhoHaveAlreadyVoted = new List<ulong>();

		public IGuildUser User
		{
			get { return mUser; }
		}
		public int VotesToKick
		{
			get { return mVotesToKick; }
		}
		public int CurrentSpamAmount
		{
			get { return mCurrentSpamAmount; }
		}
		public bool AlreadyKicked
		{
			get { return mAlreadyKicked; }
		}
		public bool PotentialKick
		{
			get { return mPotentialKick; }
		}
		public IReadOnlyCollection<ulong> UsersWhoHaveAlreadyVoted
		{
			get { return mUsersWhoHaveAlreadyVoted.AsReadOnly(); }
		}

		public void IncreaseVotesToKick()
		{
			++mVotesToKick;
		}
		public void IncreaseCurrentSpamAmount()
		{
			++mCurrentSpamAmount;
		}
		public void EnablePotentialKick()
		{
			mPotentialKick = true;
		}
		public void AddUserToVotedList(ulong ID)
		{
			mUsersWhoHaveAlreadyVoted.Add(ID);
		}
	}

	public abstract class BotClient
	{
		public abstract void AddMessageReceivedHandler(CommandHandler handler);
		public abstract void AddConnectedHandler(CommandHandler handler);
		public abstract BaseDiscordClient GetClient();
		public abstract SocketSelfUser GetCurrentUser();
		public abstract IReadOnlyCollection<SocketGuild> GetGuilds();
		public abstract SocketGuild GetGuild(ulong id);
		public abstract IReadOnlyCollection<DiscordSocketClient> GetShards();
		public abstract DiscordSocketClient GetShardFor(IGuild guild);
		public abstract int GetLatency();
		public abstract Task StartAsync();
		public abstract Task LoginAsync(TokenType tokenType, string token);
		public abstract Task LogoutAsync();
		public abstract Task WaitForGuildsAsync();
		public abstract Task SetGameAsync(string game, string stream, StreamType streamType);
	}

	public class SocketClient : BotClient
	{
		private DiscordSocketClient mSocketClient;
		public SocketClient(DiscordSocketClient client) { mSocketClient = client; }

		public override void AddMessageReceivedHandler(CommandHandler handler) { mSocketClient.MessageReceived += handler.HandleCommand; }
		public override void AddConnectedHandler(CommandHandler handler) { mSocketClient.Connected += Actions.LoadInformation; }
		public override BaseDiscordClient GetClient() { return mSocketClient; }
		public override SocketSelfUser GetCurrentUser() { return mSocketClient.CurrentUser; }
		public override IReadOnlyCollection<SocketGuild> GetGuilds() { return mSocketClient.Guilds; }
		public override SocketGuild GetGuild(ulong id) { return mSocketClient.GetGuild(id); }
		public override IReadOnlyCollection<DiscordSocketClient> GetShards() { return new[] { mSocketClient }; }
		public override DiscordSocketClient GetShardFor(IGuild guild) { return mSocketClient; }
		public override int GetLatency() { return mSocketClient.Latency; }
		public override async Task StartAsync() { await mSocketClient.StartAsync(); }
		public override async Task LoginAsync(TokenType tokenType, string token) { await mSocketClient.LoginAsync(tokenType, token); }
		public override async Task LogoutAsync() { await mSocketClient.LogoutAsync(); }
		public override async Task WaitForGuildsAsync() { await mSocketClient.WaitForGuildsAsync(); }
		public override async Task SetGameAsync(string game, string stream, StreamType streamType) { await mSocketClient.SetGameAsync(game, stream, streamType); }
	}

	public class ShardedClient : BotClient
	{
		private DiscordShardedClient mShardedClient;
		public ShardedClient(DiscordShardedClient client) { mShardedClient = client; }

		public override void AddMessageReceivedHandler(CommandHandler handler) { mShardedClient.MessageReceived += handler.HandleCommand; }
		public override void AddConnectedHandler(CommandHandler handler) { mShardedClient.Shards.FirstOrDefault().Connected += Actions.LoadInformation; }
		public override BaseDiscordClient GetClient() { return mShardedClient; }
		public override SocketSelfUser GetCurrentUser() { return mShardedClient.Shards.FirstOrDefault().CurrentUser; }
		public override IReadOnlyCollection<SocketGuild> GetGuilds() { return mShardedClient.Guilds; }
		public override SocketGuild GetGuild(ulong id) { return mShardedClient.GetGuild(id); }
		public override IReadOnlyCollection<DiscordSocketClient> GetShards() { return mShardedClient.Shards; }
		public override DiscordSocketClient GetShardFor(IGuild guild) { return mShardedClient.GetShardFor(guild); }
		public override int GetLatency() { return mShardedClient.Latency; }
		public override async Task StartAsync() { await mShardedClient.StartAsync(); }
		public override async Task LoginAsync(TokenType tokenType, string token) { await mShardedClient.LoginAsync(tokenType, token); }
		public override async Task LogoutAsync() { await mShardedClient.LogoutAsync(); }
		public override async Task WaitForGuildsAsync() { await mShardedClient.Shards.ToList().ForEachAsync(async x => await x.WaitForGuildsAsync()); }
		public override async Task SetGameAsync(string game, string stream, StreamType streamType) { await mShardedClient.SetGameAsync(game, stream, streamType); }
	}

	public class BaseSpamPrevention
	{
		public BaseSpamPrevention(int amountOfMessages, int votesNeededForKick, int amountOfSpam)
		{
			mAmountOfMessages = amountOfMessages;
			mVotesNeededForKick = votesNeededForKick;
			mAmountOfSpam = amountOfSpam;
			mEnabled = true;
		}

		private int mAmountOfMessages;
		private int mVotesNeededForKick;
		private int mAmountOfSpam;
		private bool mEnabled;

		public int AmountOfMentionsPerMsg
		{
			get { return mAmountOfSpam; }
		}
		public int AmountOfMessages
		{
			get { return mAmountOfMessages; }
		}
		public int VotesNeededForKick
		{
			get { return mVotesNeededForKick; }
		}
		public bool Enabled
		{
			get { return mEnabled; }
		}

		public void SwitchEnabled(bool newVal)
		{
			mEnabled = newVal;
		}
	}

	public class MessageSpamPrevention : BaseSpamPrevention
	{
		public MessageSpamPrevention(int amountOfMessages, int votesNeededForKick, int placeholder) : base(amountOfMessages, votesNeededForKick, placeholder) { }
	}

	public class MentionSpamPrevention : BaseSpamPrevention
	{
		public MentionSpamPrevention(int amountOfMessages, int votesNeededForKick, int amountOfMentions) : base(amountOfMessages, votesNeededForKick, amountOfMentions) {}
	}

	public class LongMessageSpamPrevention : BaseSpamPrevention
	{
		public LongMessageSpamPrevention(int amountOfMessages, int votesNeededForKick, int lengthOfMessage) : base(amountOfMessages, votesNeededForKick, lengthOfMessage) {}
	}

	public class LinkSpamPrevention : BaseSpamPrevention
	{
		public LinkSpamPrevention(int amountOfMessages, int votesNeededForKick, int amountOfLinks) : base(amountOfMessages, votesNeededForKick, amountOfLinks) {}
	}

	public class ImageSpamPrevention : BaseSpamPrevention
	{
		public ImageSpamPrevention(int amountOfMessages, int votesNeededForKick, int amountOfImages) : base(amountOfMessages, votesNeededForKick, amountOfImages) { }
	}
	#endregion

	#region Structs
	public struct ChannelAndPosition
	{
		public ChannelAndPosition(IGuildChannel channel, int position)
		{
			mChannel = channel;
			mPosition = position;
		}

		private IGuildChannel mChannel;
		private int mPosition;

		public IGuildChannel Channel
		{
			get { return mChannel; }
		}
		public int Position
		{
			get { return Position; }
		}
	}

	public struct SlowmodeChannel
	{
		public SlowmodeChannel(ulong channelID, ulong guildID)
		{
			mChannelID = channelID;
			mGuildID = guildID;
		}

		private ulong mChannelID;
		private ulong mGuildID;

		public ulong ChannelID
		{
			get { return mChannelID; }
		}
		public ulong GuildID
		{
			get { return mGuildID; }
		}
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
			mName = name;
			mText = text;
		}

		private string mName;
		private string mText;

		public string Name
		{
			get { return mName; }
		}
		public string Text
		{
			get { return mText; }
		}
	}

	public struct CloseWord
	{
		public CloseWord(string name, int closeness)
		{
			mName = name;
			mCloseness = closeness;
		}

		private string mName;
		private int mCloseness;

		public string Name
		{
			get { return mName; }
		}
		public int Closeness
		{
			get { return mCloseness; }
		}
	}

	public struct ActiveCloseWords
	{
		public ActiveCloseWords(IGuildUser user, List<CloseWord> list)
		{
			mUser = user;
			mList = list;
		}

		private IGuildUser mUser;
		private List<CloseWord> mList;

		public IGuildUser User
		{
			get { return mUser; }
		}
		public List<CloseWord> List
		{
			get { return mList; }
		}
	}

	public struct CloseHelp
	{
		public CloseHelp(HelpEntry help, int closeness)
		{
			mHelp = help;
			mCloseness = closeness;
		}

		private HelpEntry mHelp;
		private int mCloseness;

		public HelpEntry Help
		{
			get { return mHelp; }
		}
		public int Closeness
		{
			get { return mCloseness; }
		}
	}

	public struct ActiveCloseHelp
	{
		public ActiveCloseHelp(IGuildUser user, List<CloseHelp> list)
		{
			mUser = user;
			mList = list;
		}

		private IGuildUser mUser;
		private List<CloseHelp> mList;

		public IGuildUser User
		{
			get { return mUser; }
		}
		public List<CloseHelp> List
		{
			get { return mList; }
		}
	}

	public struct UICommandNames
	{
		private static readonly Dictionary<UICommandEnum, string[]> NamesAndAliases = new Dictionary<UICommandEnum, string[]>
		{
			{ UICommandEnum.Pause, new string[] { SharedCommands.CPAUSE } },
			{ UICommandEnum.BotOwner, new string[] { SharedCommands.COWNER, SharedCommands.AOWNER } },
			{ UICommandEnum.SavePath, new string[] { SharedCommands.CPATH, SharedCommands.APATH } },
			{ UICommandEnum.Prefix, new string[] { SharedCommands.CPREFIX, SharedCommands.APREFIX } },
			{ UICommandEnum.Settings, new string[] { SharedCommands.CSETTINGS, SharedCommands.ASETTINGS } },
			{ UICommandEnum.BotIcon, new string[] { SharedCommands.CICON, SharedCommands.AICON } },
			{ UICommandEnum.BotGame, new string[] { SharedCommands.CGAME, SharedCommands.AGAME } },
			{ UICommandEnum.BotStream, new string[] { SharedCommands.CSTREAM, SharedCommands.ASTREAM } },
			{ UICommandEnum.BotName, new string[] { SharedCommands.CNAME, SharedCommands.ANAME } },
			{ UICommandEnum.Disconnect, new string[] { SharedCommands.CDISC, SharedCommands.ADISC_1, SharedCommands.ADISC_2 } },
			{ UICommandEnum.Restart, new string[] { SharedCommands.CRESTART, SharedCommands.ARESTART } },
			{ UICommandEnum.ListGuilds, new string[] { SharedCommands.CGUILDS, SharedCommands.AGUILDS } },
			{ UICommandEnum.Shards, new string[] { SharedCommands.CSHARDS } },
		};

		public static string[] GetNameAndAliases(UICommandEnum cmd)
		{
			return NamesAndAliases.ContainsKey(cmd) ? NamesAndAliases[cmd] : new string[] { };
		}
		public static string GetName(UICommandEnum cmd)
		{
			return NamesAndAliases.ContainsKey(cmd) ? NamesAndAliases[cmd][0] : null;
		}
		public static string[] GetAliases(UICommandEnum cmd)
		{
			return NamesAndAliases.ContainsKey(cmd) ? NamesAndAliases[cmd].Skip(1).ToArray() : new string[] { };
		}

		public static string FormatStringForUse()
		{
			return String.Join("\n", Enum.GetValues(typeof(UICommandEnum)).Cast<UICommandEnum>().ToList().Select(x =>
			{
				var aliases = String.Join(", ", GetAliases(x));
				return String.Format("{0,-20}{1}", GetName(x), String.IsNullOrWhiteSpace(aliases) ? "This command has no aliases." : aliases);
			}));
		}
	}
	#endregion

	#region Enums
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
		ImageLog = 15,
		CommandLog = 16,
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
	}

	public enum PunishmentType
	{
		Kick = 1,
		Ban = 2,
		Role = 3,
	}

	public enum SAGAction
	{
		Create = 1,
		Add = 2,
		Remove = 3,
		Delete = 4,
	}

	public enum DeleteInvAction
	{
		User = 1,
		Channel = 2,
		Uses = 3,
		Expiry = 4,
	}

	public enum SpamPreventionAction
	{
		Enable = 1,
		Disable = 2,
		Current = 3,
		Setup = 4,
	}

	public enum UICommandEnum
	{
		Pause = 0,
		BotOwner = 1,
		SavePath = 2,
		Prefix = 3,
		Settings = 4,
		BotIcon = 5,
		BotGame = 6,
		BotStream = 7,
		BotName = 8,
		Disconnect = 9,
		Restart = 10,
		ListGuilds = 11,
		Shards = 12,
	};
	#endregion
}