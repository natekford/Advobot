using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
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
				var botBits = Variables.Guilds[context.Guild.Id].BotUsers.FirstOrDefault(x => x.User == user)?.Permissions;
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
			return (await Actions.GetIfUserIsOwner(context.Guild, context.User)) || Actions.GetIfUserIsBotOwner(context.User) ?
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
				return Actions.GetIfUserIsBotOwner(context.User) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(Constants.IGNORE_ERROR);
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
				var botBits = Variables.Guilds[context.Guild.Id].BotUsers.FirstOrDefault(x => x.User == user)?.Permissions;
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

	[AttributeUsage(AttributeTargets.Method)]
	public class DefaultEnabledAttribute : Attribute
	{
		public DefaultEnabledAttribute(bool enabled)
		{
			Enabled = enabled;
		}

		public bool Enabled { get; private set; }
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class UsageAttribute : Attribute
	{
		public UsageAttribute(string usage)
		{
			Usage = usage;
		}

		public string Usage { get; private set; }
	}
	#endregion

	#region Saved Classes
	public class BotGuildInfo
	{
		public BotGuildInfo(ulong guildID)
		{
			GuildID = guildID;

			CommandSettings = new List<CommandSwitch>();
			CommandsDisabledOnChannel = new List<CommandDisabledOnChannel>();
			BotUsers = new List<BotImplementedPermissions>();
			SelfAssignableGroups = new List<SelfAssignableGroup>();
			Reminds = new List<Remind>();
			IgnoredCommandChannels = new List<ulong>();
			IgnoredLogChannels = new List<ulong>();
			LogActions = new List<LogActions>();
			ActiveCloseHelp = new List<ActiveCloseHelp>();
			ActiveCloseWords = new List<ActiveCloseWords>();
			SlowmodeChannels = new List<SlowmodeChannel>();
			Invites = new List<BotInvite>();
			EvaluatedRegex = new List<Regex>();
			BannedPhraseUsers = new List<BannedPhraseUser>();
			FAWRNicknames = new List<string>();
			FAWRRoles = new List<IRole>();

			BannedPhrases = new BannedPhrases();
			GlobalSpamPrevention = new GlobalSpamPrevention();
			RoleLoss = new RoleLoss();
			MessageDeletion = new MessageDeletion();

			Prefix = null;
			DefaultPrefs = true;
			Loaded = false;
			EnablingPrefs = false;
			DeletingPrefs = false;
		}

		[JsonProperty]
		public List<CommandSwitch> CommandSettings { get; private set; }
		[JsonProperty]
		public List<CommandDisabledOnChannel> CommandsDisabledOnChannel { get; private set; }
		[JsonProperty]
		public List<BotImplementedPermissions> BotUsers { get; private set; }
		[JsonProperty]
		public List<SelfAssignableGroup> SelfAssignableGroups { get; private set; }
		[JsonProperty]
		public List<Remind> Reminds { get; private set; }
		[JsonProperty]
		public List<ulong> IgnoredCommandChannels { get; private set; }
		[JsonProperty]
		public List<ulong> IgnoredLogChannels { get; private set; }
		[JsonProperty]
		public List<LogActions> LogActions { get; private set; }
		[JsonIgnore]
		public List<ActiveCloseHelp> ActiveCloseHelp { get; private set; }
		[JsonIgnore]
		public List<ActiveCloseWords> ActiveCloseWords { get; private set; }
		[JsonIgnore]
		public List<SlowmodeChannel> SlowmodeChannels { get; private set; }
		[JsonIgnore]
		public List<BotInvite> Invites { get; private set; }
		[JsonIgnore]
		public List<Regex> EvaluatedRegex { get; private set; }
		[JsonIgnore]
		public List<BannedPhraseUser> BannedPhraseUsers { get; private set; }
		[JsonIgnore]
		public List<string> FAWRNicknames { get; private set; }
		[JsonIgnore]
		public List<IRole> FAWRRoles { get; private set; }

		[JsonProperty]
		public BannedPhrases BannedPhrases { get; private set; }
		[JsonProperty]
		public GlobalSpamPrevention GlobalSpamPrevention { get; private set; }
		[JsonProperty]
		public WelcomeMessage WelcomeMessage { get; private set; }
		[JsonProperty]
		public GoodbyeMessage GoodbyeMessage { get; private set; }
		[JsonProperty]
		public string Prefix { get; private set; }
		[JsonProperty]
		public ulong GuildID { get; private set; }
		[JsonProperty]
		public ulong ServerLogID { get; private set; }
		[JsonProperty]
		public ulong ModLogID { get; private set; }
		[JsonIgnore]
		public SlowmodeGuild SlowmodeGuild { get; private set; }
		[JsonIgnore]
		public AntiRaid AntiRaid { get; private set; }
		[JsonIgnore]
		public RoleLoss RoleLoss { get; private set; }
		[JsonIgnore]
		public MessageDeletion MessageDeletion { get; private set; }
		[JsonIgnore]
		public bool DefaultPrefs { get; private set; }
		[JsonIgnore]
		public bool Loaded { get; private set; }
		[JsonIgnore]
		public bool EnablingPrefs { get; private set; }
		[JsonIgnore]
		public bool DeletingPrefs { get; private set; }
		[JsonIgnore]
		public SocketGuild Guild { get; private set; }
		[JsonIgnore]
		public ITextChannel ServerLog { get; private set; }
		[JsonIgnore]
		public ITextChannel ModLog { get; private set; }

		public void TurnDefaultPrefsOff()
		{
			DefaultPrefs = false;
		}
		public void TurnDefaultPrefsOn()
		{
			DefaultPrefs = true;
		}
		public void TurnLoadedOn()
		{
			Loaded = true;
		}
		public void SwitchEnablingPrefs()
		{
			EnablingPrefs = !EnablingPrefs;
		}
		public void SwitchDeletingPrefs()
		{
			DeletingPrefs = !DeletingPrefs;
		}
		public void SetBannedPhrases(BannedPhrases bannedPhrases)
		{
			BannedPhrases = bannedPhrases;
		}
		public void SetAntiRaid(AntiRaid antiRaid)
		{
			AntiRaid = antiRaid;
		}
		public void SetSlowmodeGuild(SlowmodeGuild slowmodeGuild)
		{
			SlowmodeGuild = slowmodeGuild;
		}
		public void SetWelcomeMessage(WelcomeMessage welcomeMessage)
		{
			WelcomeMessage = welcomeMessage;
		}
		public void SetGoodbyeMessage(GoodbyeMessage goodbyeMessage)
		{
			GoodbyeMessage = goodbyeMessage;
		}
		public void SetPrefix(string prefix)
		{
			Prefix = prefix;
		}
		public void SetServerLog(ITextChannel channel)
		{
			ServerLogID = channel.Id;
			ServerLog = channel;
		}
		public void SetModLog(ITextChannel channel)
		{
			ModLogID = channel.Id;
			ModLog = channel;
		}
		public void SetLogActions(List<LogActions> logActions)
		{
			LogActions = logActions;
		}
		public void ClearSMChannels()
		{
			SlowmodeChannels = new List<SlowmodeChannel>();
		}
		public void PostDeserialize()
		{
			DefaultPrefs = false;

			Guild = Variables.Client.GetGuild(GuildID);
			if (Guild == null)
				return;

			ModLog = Guild.GetChannel(ModLogID) as ITextChannel;
			ServerLog = Guild.GetChannel(ServerLogID) as ITextChannel;
		}
	}

	public class CommandSwitch
	{
		public CommandSwitch(string name, bool value)
		{
			mHelpEntry = Variables.HelpList.FirstOrDefault(x => x.Name.Equals(name));
			Name = name;
			Value = value;
			Category = mHelpEntry.Category;
			Aliases = mHelpEntry.Aliases;
		}

		[JsonIgnore]
		private HelpEntry mHelpEntry;
		[JsonIgnore]
		private readonly string[] mTrueMatches = { "true", "on", "yes", "1" };
		[JsonProperty]
		public string Name { get; private set; }
		[JsonProperty]
		public bool Value { get; private set; }
		[JsonIgnore]
		public CommandCategory Category { get; private set; }
		[JsonIgnore]
		public string[] Aliases { get; private set; }

		[JsonIgnore]
		public string CategoryName
		{
			get { return Enum.GetName(typeof(CommandCategory), (int)Category); }
		}
		[JsonIgnore]
		public int CategoryValue
		{
			get { return (int)Category; }
		}
		[JsonProperty]
		public CommandCategory CategoryEnum
		{
			get { return Category; }
		}
		[JsonIgnore]
		public string ValAsString
		{
			get { return Value ? "ON" : "OFF"; }
		}
		[JsonIgnore]
		public int ValAsInteger
		{
			get { return Value ? 1 : -1; }
		}
		[JsonIgnore]
		public bool ValAsBoolean
		{
			get { return Value; }
		}

		public void Disable()
		{
			Value = false;
		}
		public void Enable()
		{
			Value = true;
		}
	}

	public class BannedPhrases
	{
		public BannedPhrases()
		{
			Strings = new List<BannedPhrase<string>>();
			Regex = new List<BannedPhrase<Regex>>();
			Punishments = new List<BannedPhrasePunishment>();
		}

		[JsonProperty]
		public List<BannedPhrase<string>> Strings { get; private set; }
		[JsonProperty]
		public List<BannedPhrase<Regex>> Regex { get; private set; }
		[JsonProperty]
		public List<BannedPhrasePunishment> Punishments { get; private set; }

		public void MakeAllDistinct()
		{
			Strings = Strings.Distinct().ToList();
			Regex = Regex.Distinct().ToList();
			Punishments = Punishments.Distinct().ToList();
		}
	}

	public class BannedPhrase<T>
	{
		public BannedPhrase(T phrase, PunishmentType punishment)
		{
			Phrase = phrase;
			Punishment = (punishment == PunishmentType.Deafen || punishment == PunishmentType.Mute) ? PunishmentType.Nothing : punishment;
		}

		[JsonProperty]
		public T Phrase { get; private set; }
		[JsonProperty]
		public PunishmentType Punishment { get; private set; }

		public void ChangePunishment(PunishmentType type)
		{
			Punishment = (type == PunishmentType.Deafen || type == PunishmentType.Mute) ? PunishmentType.Nothing : type;
		}
	}

	public class BannedPhrasePunishment
	{
		public BannedPhrasePunishment(int number, PunishmentType punishment, ulong? guildID = null, ulong? roleID = null, int? punishmentTime = null)
		{
			NumberOfRemoves = number;
			Punishment = punishment;
			RoleID = roleID;
			GuildID = guildID;
			Role = RoleID != null && GuildID != null ? Variables.Client.GetGuild((ulong)GuildID)?.GetRole((ulong)RoleID) : null;
			PunishmentTime = punishmentTime;
		}

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
	}

	public class SelfAssignableRole
	{
		public SelfAssignableRole(ulong guildID, ulong roleID, int group)
		{
			GuildID = guildID;
			RoleID = roleID;
			Group = group;
			Role = Variables.Client.GetGuild(guildID).GetRole(roleID);
		}

		[JsonProperty]
		public ulong GuildID { get; private set; }
		[JsonProperty]
		public ulong RoleID { get; private set; }
		[JsonIgnore]
		public int Group { get; private set; }
		[JsonIgnore]
		public IRole Role { get; private set; }
	}

	public class SelfAssignableGroup
	{
		public SelfAssignableGroup(List<SelfAssignableRole> roles, int group)
		{
			mRoles = roles;
			Group = group;
		}

		[JsonProperty(PropertyName = "Roles")]
		private List<SelfAssignableRole> mRoles;
		[JsonProperty]
		public int Group { get; private set; }

		[JsonIgnore]
		public ReadOnlyCollection<SelfAssignableRole> Roles
		{
			get { return mRoles.AsReadOnly(); }
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

	public class BotImplementedPermissions
	{
		public BotImplementedPermissions(ulong guildID, ulong userID, uint permissions)
		{
			GuildID = guildID;
			UserID = userID;
			Permissions = permissions;
			User = Variables.Client.GetGuild(guildID).GetUser(userID);
			Variables.Guilds[guildID].BotUsers.Add(this);
		}

		[JsonProperty]
		public ulong GuildID { get; private set; }
		[JsonProperty]
		public ulong UserID { get; private set; }
		[JsonProperty]
		public uint Permissions { get; private set; }
		[JsonIgnore]
		public IGuildUser User { get; private set; }

		public void AddPermission(int add)
		{
			Permissions |= (1U << add);
		}
		public void RemovePermission(int remove)
		{
			Permissions &= ~(1U << remove);
		}
	}

	public class GuildNotification
	{
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

		public void ChangeChannel(ITextChannel channel)
		{
			Channel = channel;
		}
	}

	public class WelcomeMessage : GuildNotification
	{
		public WelcomeMessage(string content, string title, string description, string thumbURL, ulong guildID, ulong channelID) : base(content, title, description, thumbURL, guildID, channelID) { }
	}

	public class GoodbyeMessage : GuildNotification
	{
		public GoodbyeMessage(string content, string title, string description, string thumbURL, ulong guildID, ulong channelID) : base(content, title, description, thumbURL, guildID, channelID) { }
	}

	public class GlobalSpamPrevention
	{
		public GlobalSpamPrevention()
		{
			SpamPreventionUsers = new List<SpamPreventionUser>();
		}

		[JsonIgnore]
		public List<SpamPreventionUser> SpamPreventionUsers { get; private set; }
		[JsonProperty]
		public MessageSpamPrevention MessageSpamPrevention { get; private set; }
		[JsonProperty]
		public LongMessageSpamPrevention LongMessageSpamPrevention { get; private set; }
		[JsonProperty]
		public LinkSpamPrevention LinkSpamPrevention { get; private set; }
		[JsonProperty]
		public ImageSpamPrevention ImageSpamPrevention { get; private set; }
		[JsonProperty]
		public MentionSpamPrevention MentionSpamPrevention { get; private set; }

		public BaseSpamPrevention GetSpamPrevention(SpamType type)
		{
			switch (type)
			{
				case SpamType.Message:
				{
					return MessageSpamPrevention;
				}
				case SpamType.Long_Message:
				{
					return LongMessageSpamPrevention;
				}
				case SpamType.Link:
				{
					return LinkSpamPrevention;
				}
				case SpamType.Image:
				{
					return ImageSpamPrevention;
				}
				case SpamType.Mention:
				{
					return MentionSpamPrevention;
				}
			}
			return null;
		}
		public void SetMessageSpamPrevention(MessageSpamPrevention spamPrevention)
		{
			MessageSpamPrevention = spamPrevention;
		}
		public void SetLongMessageSpamPrevention(LongMessageSpamPrevention spamPrevention)
		{
			LongMessageSpamPrevention = spamPrevention;
		}
		public void SetLinkSpamPrevention(LinkSpamPrevention spamPrevention)
		{
			LinkSpamPrevention = spamPrevention;
		}
		public void SetImageSpamPrevention(ImageSpamPrevention spamPrevention)
		{
			ImageSpamPrevention = spamPrevention;
		}
		public void SetMentionSpamPrevention(MentionSpamPrevention spamPrevention)
		{
			MentionSpamPrevention = spamPrevention;
		}
	}

	public class BaseSpamPrevention
	{
		public BaseSpamPrevention(int amountOfMessages, int votesNeededForKick, int amountOfSpam, SpamType spamType)
		{
			AmountOfMessages = amountOfMessages;
			VotesNeededForKick = votesNeededForKick;
			AmountOfSpam = amountOfSpam;
			SpamType = spamType;
			Enabled = true;
		}

		[JsonProperty]
		public int AmountOfMessages { get; private set; }
		[JsonProperty]
		public int VotesNeededForKick { get; private set; }
		[JsonProperty]
		public int AmountOfSpam { get; private set; }
		[JsonProperty]
		public SpamType SpamType { get; private set; }
		[JsonProperty]
		public bool Enabled { get; private set; }

		public void SwitchEnabled(bool newVal)
		{
			Enabled = newVal;
		}
	}
	#endregion

	#region Non-saved Classes
	public class HelpEntry
	{
		public HelpEntry(string name, string[] aliases, string usage, string basePerm, string text, CommandCategory category, bool defaultEnabled)
		{
			Name = name;
			Aliases = aliases;
			Usage = usage;
			BasePerm = basePerm;
			Text = text;
			Category = category;
			DefaultEnabled = defaultEnabled;
		}

		public string Name { get; private set; }
		public string[] Aliases { get; private set; }
		public string Usage { get; private set; }
		public string BasePerm { get; private set; }
		public string Text { get; private set; }
		public CommandCategory Category { get; private set; }
		public bool DefaultEnabled { get; private set; }
	}

	public class BotInvite
	{
		public BotInvite(ulong guildID, string code, int uses)
		{
			GuildID = guildID;
			Code = code;
			Uses = uses;
		}

		public ulong GuildID { get; private set; }
		public string Code { get; private set; }
		public int Uses { get; private set; }

		public void IncreaseUses()
		{
			++Uses;
		}
	}

	public abstract class BotClient
	{
		public abstract void AddMessageReceivedHandler(Command_Handler handler);
		public abstract void AddConnectedHandler(Command_Handler handler);
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
	}

	public class SocketClient : BotClient
	{
		private DiscordSocketClient mSocketClient;
		public SocketClient(DiscordSocketClient client) { mSocketClient = client; }

		public override void AddMessageReceivedHandler(Command_Handler handler) { mSocketClient.MessageReceived += handler.HandleCommand; }
		public override void AddConnectedHandler(Command_Handler handler) { mSocketClient.Connected += Actions.LoadInformation; }
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
	}

	public class ShardedClient : BotClient
	{
		private DiscordShardedClient mShardedClient;
		public ShardedClient(DiscordShardedClient client) { mShardedClient = client; }

		public override void AddMessageReceivedHandler(Command_Handler handler) { mShardedClient.MessageReceived += handler.HandleCommand; }
		public override void AddConnectedHandler(Command_Handler handler) { mShardedClient.Shards.FirstOrDefault().Connected += Actions.LoadInformation; }
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

		public IGuildUser User { get; private set; }
		public int CurrentMessagesLeft { get; private set; }
		public int BaseMessages { get; private set; }
		public int Time { get; private set; }

		public void LowerMessagesLeft()
		{
			--CurrentMessagesLeft;
		}
		public void ResetMessagesLeft()
		{
			CurrentMessagesLeft = BaseMessages;
		}
	}

	public class BannedPhraseUser
	{
		public BannedPhraseUser(IGuildUser user)
		{
			User = user;
			Variables.Guilds[user.Guild.Id].BannedPhraseUsers.Add(this);
		}

		public IGuildUser User { get; private set; }
		public int MessagesForRole { get; private set; }
		public int MessagesForKick { get; private set; }
		public int MessagesForBan { get; private set; }

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

	public class SpamPreventionUser
	{
		public SpamPreventionUser(GlobalSpamPrevention global, IGuildUser user)
		{
			User = user;
			VotesRequired = int.MaxValue;
			PotentialKick = false;
			AlreadyKicked = false;
			UsersWhoHaveAlreadyVoted = new List<ulong>();
			global.SpamPreventionUsers.Add(this);
		}

		public IGuildUser User { get; private set; }
		public int VotesToKick { get; private set; }
		public int VotesRequired { get; private set; }
		public bool PotentialKick { get; private set; }
		public bool AlreadyKicked { get; private set; }
		public List<ulong> UsersWhoHaveAlreadyVoted { get; private set; }
		public int MessageSpamAmount { get; private set; }
		public int LongMessageSpamAmount { get; private set; }
		public int LinkSpamAmount { get; private set; }
		public int ImageSpamAmount { get; private set; }
		public int MentionSpamAmount { get; private set; }

		public void IncreaseVotesToKick()
		{
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
		public void AddUserToVotedList(ulong ID)
		{
			UsersWhoHaveAlreadyVoted.Add(ID);
		}
		public void ResetSpamUser()
		{
			MessageSpamAmount = 0;
			LongMessageSpamAmount = 0;
			LinkSpamAmount = 0;
			ImageSpamAmount = 0;
			MentionSpamAmount = 0;
			UsersWhoHaveAlreadyVoted = new List<ulong>();
		}
		public async Task CheckIfShouldKick(BaseSpamPrevention spamPrev, IMessage msg)
		{
			var spamAmount = 0;
			switch (spamPrev.SpamType)
			{
				case SpamType.Message:
				{
					spamAmount = ++MessageSpamAmount;
					break;
				}
				case SpamType.Long_Message:
				{
					spamAmount = ++LongMessageSpamAmount;
					break;
				}
				case SpamType.Link:
				{
					spamAmount = ++LinkSpamAmount;
					break;
				}
				case SpamType.Image:
				{
					spamAmount = ++ImageSpamAmount;
					break;
				}
				case SpamType.Mention:
				{
					spamAmount = ++MentionSpamAmount;
					break;
				}
			}

			if (spamAmount >= spamPrev.AmountOfMessages)
			{
				await Actions.VotesHigherThanRequiredAmount(spamPrev, this, msg);
			}
		}
	}

	public class MessageSpamPrevention : BaseSpamPrevention
	{
		public MessageSpamPrevention(int amountOfMessages, int votesNeededForKick, int placeholder) : base(amountOfMessages, votesNeededForKick, placeholder, SpamType.Message) { }
	}

	public class LongMessageSpamPrevention : BaseSpamPrevention
	{
		public LongMessageSpamPrevention(int amountOfMessages, int votesNeededForKick, int lengthOfMessage) : base(amountOfMessages, votesNeededForKick, lengthOfMessage, SpamType.Long_Message) {}
	}

	public class LinkSpamPrevention : BaseSpamPrevention
	{
		public LinkSpamPrevention(int amountOfMessages, int votesNeededForKick, int amountOfLinks) : base(amountOfMessages, votesNeededForKick, amountOfLinks, SpamType.Link) {}
	}

	public class ImageSpamPrevention : BaseSpamPrevention
	{
		public ImageSpamPrevention(int amountOfMessages, int votesNeededForKick, int amountOfImages) : base(amountOfMessages, votesNeededForKick, amountOfImages, SpamType.Image) { }
	}

	public class MentionSpamPrevention : BaseSpamPrevention
	{
		public MentionSpamPrevention(int amountOfMessages, int votesNeededForKick, int amountOfMentions) : base(amountOfMessages, votesNeededForKick, amountOfMentions, SpamType.Mention) { }
	}

	public abstract class DeletionSpamProtection
	{
		public CancellationTokenSource CancelToken { get; private set; }

		public void SetCancelToken(CancellationTokenSource cancelToken)
		{
			CancelToken = cancelToken;
		}
		public abstract List<ISnowflakeEntity> GetList();
		public abstract void SetList(List<ISnowflakeEntity> InList);
		public abstract void AddToList(ISnowflakeEntity Item);
		public abstract void ClearList();
	}

	public class RoleLoss : DeletionSpamProtection
	{
		private List<IGuildUser> mUsers = new List<IGuildUser>();

		public override List<ISnowflakeEntity> GetList()
		{
			return mUsers.Select(x => x as ISnowflakeEntity).ToList();
		}
		public override void SetList(List<ISnowflakeEntity> InList)
		{
			mUsers = InList.Select(x => x as IGuildUser).ToList();
		}
		public override void AddToList(ISnowflakeEntity Item)
		{
			mUsers.Add(Item as IGuildUser);
		}
		public override void ClearList()
		{
			mUsers.Clear();
		}
	}

	public class MessageDeletion : DeletionSpamProtection
	{
		private List<IMessage> mMessages = new List<IMessage>();

		public override List<ISnowflakeEntity> GetList()
		{
			return mMessages.Select(x => x as ISnowflakeEntity).ToList();
		}
		public override void SetList(List<ISnowflakeEntity> InList)
		{
			mMessages = InList.Select(x => x as IMessage).ToList();
		}
		public override void AddToList(ISnowflakeEntity Item)
		{
			mMessages.Add(Item as IMessage);
		}
		public override void ClearList()
		{
			mMessages.Clear();
		}
	}

	public class AntiRaid
	{
		public AntiRaid(IRole muteRole)
		{
			MuteRole = muteRole;
			UsersWhoHaveBeenMuted = new List<IGuildUser>();
		}

		public IRole MuteRole { get; private set; }
		public List<IGuildUser> UsersWhoHaveBeenMuted { get; private set; }

		public void DisableAntiRaid()
		{
			MuteRole = null;
		}
		public void AddUserToMutedList(IGuildUser user)
		{
			UsersWhoHaveBeenMuted.Add(user);
		}
	}

	public class SlowmodeGuild
	{
		public SlowmodeGuild(List<SlowmodeUser> users)
		{
			Users = users;
		}

		public List<SlowmodeUser> Users { get; private set; }
	}

	public class SlowmodeChannel
	{
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

		public ulong ChannelID { get; private set; }
		public List<SlowmodeUser> Users { get; private set; }

		public void SetUserList(List<SlowmodeUser> users)
		{
			Users = users;
		}
	}
	#endregion

	#region Structs
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

		[JsonProperty]
		public string Name { get; private set; }
		[JsonProperty]
		public string Text { get; private set; }
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
		public ReadOnlyCollection<CloseWord> List
		{
			get { return mList.AsReadOnly(); }
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
		public ReadOnlyCollection<CloseHelp> List
		{
			get { return mList.AsReadOnly(); }
		}
	}

	public struct UICommandNames
	{
		private static ReadOnlyDictionary<UICommandEnum, string[]> NamesAndAliases = new ReadOnlyDictionary<UICommandEnum, string[]>(new Dictionary<UICommandEnum, string[]>()
		{
			{ UICommandEnum.Pause, new string[] { SharedCommands.CPAUSE, SharedCommands.APAUSE } },
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
			{ UICommandEnum.Shards, new string[] { SharedCommands.CSHARDS, SharedCommands.ASHARDS } },
		});

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

	public struct CommandDisabledOnChannel
	{
		public CommandDisabledOnChannel(ulong channelID, string commandName)
		{
			mChannelID = channelID;
			mCommandName = commandName;
		}

		private ulong mChannelID;
		private string mCommandName;

		public ulong ChannelID
		{
			get { return mChannelID; }
		}
		public string CommandName
		{
			get { return mCommandName; }
		}
	}

	public struct RemovablePunishment
	{
		public RemovablePunishment(IGuild guild, IUser user, PunishmentType type, DateTime time)
		{
			mGuild = guild;
			mUser = user;
			mType = type;
			mTime = time;
			mRole = null;
		}

		public RemovablePunishment(IGuild guild, IUser user, IRole role, DateTime time)
		{
			mGuild = guild;
			mUser = user;
			mType = PunishmentType.Role;
			mTime = time;
			mRole = role;
		}

		private IGuild mGuild;
		private IUser mUser;
		private PunishmentType mType;
		private IRole mRole;
		private DateTime mTime;

		public IGuild Guild
		{
			get { return mGuild; }
		}
		public IUser User
		{
			get { return mUser; }
		}
		public PunishmentType Type
		{
			get { return mType; }
		}
		public IRole Role
		{
			get { return mRole; }
		}
		public DateTime Time
		{
			get { return mTime; }
		}
	}

	public struct RemovableMessage
	{
		public RemovableMessage(IMessage message, DateTime time)
		{
			Message = message;
			Time = time;
		}

		public IMessage Message { get; private set; }
		public DateTime Time { get; private set; }
	}

	public struct ReturnedChannel
	{
		public ReturnedChannel(IGuildChannel channel, FailureReason reason)
		{
			mChannel = channel;
			mReason = reason;
		}

		private IGuildChannel mChannel;
		private FailureReason mReason;

		public IGuildChannel Channel
		{
			get { return mChannel; }
		}
		public FailureReason Reason
		{
			get { return mReason; }
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
		Spam_Prevention = 11,
	}

	public enum PunishmentType
	{
		Nothing = 0,
		Kick = 1,
		Ban = 2,
		Role = 3,
		Deafen = 4,
		Mute = 5,
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

	public enum SpamType
	{
		Message = 1,
		Long_Message = 2,
		Link = 3,
		Image = 4,
		Mention = 5, 
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

	public enum CHPType
	{
		Show = 1,
		Allow = 2,
		Inherit = 3,
		Deny = 4,
	}

	public enum FailureReason
	{
		Not_Failure = 0,
		Not_Found = 1,
		User_Inability = 2,
		Bot_Inability = 3,
	}

	public enum BUMType
	{
		Add = 1,
		Remove = 2,
		Show = 3,
	}
	#endregion
}