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
	//[AttributeUsage(AttributeTargets.Class)]
	public class PermissionRequirementAttribute : PreconditionAttribute
	{
		private uint mAllFlags;
		private uint mAnyFlags;

		//This doesn't have default values for the parameters since that makes it harder to potentially provide the wrong permissions
		public PermissionRequirementAttribute(GuildPermission[] anyOfTheListedPerms, GuildPermission[] allOfTheListedPerms)
		{
			mAnyFlags |= (1U << (int)GuildPermission.Administrator);
			foreach (var perm in anyOfTheListedPerms ?? Enumerable.Empty<GuildPermission>())
			{
				mAnyFlags |= (1U << (int)perm);
			}
			foreach (var perm in allOfTheListedPerms ?? Enumerable.Empty<GuildPermission>())
			{
				mAllFlags |= (1U << (int)perm);
			}
		}
		//For when/if GuildPermission values get put as bits
		public PermissionRequirementAttribute(GuildPermission anyOfTheListedPerms, GuildPermission allOfTheListedPerms)
		{
			throw new NotImplementedException();
		}
		/*
		public PermissionRequirementAttribute(uint anyOfTheListedPerms = 0, uint allOfTheListedPerms = 0)
		{
			mAnyFlags = anyOfTheListedPerms | (1U << (int)GuildPermission.Administrator);
			mAllFlags = allOfTheListedPerms;
		}*/

		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
		{
			if (context is MyCommandContext)
			{
				var cont = context as MyCommandContext;
				var user = context.User as IGuildUser;

				var botBits = ((List<BotImplementedPermissions>)cont.GuildInfo.GetSetting(SettingOnGuild.BotUsers));
				var userBits = botBits.FirstOrDefault(x => x.UserID == user.Id)?.Permissions ?? 0;

				var perms = user.GuildPermissions.RawValue | userBits;
				if ((perms & mAllFlags) == mAllFlags || (perms & mAnyFlags) != 0)
				{
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

	//[AttributeUsage(AttributeTargets.Class)]
	public class OtherRequirementAttribute : PreconditionAttribute
	{
		private const uint PERMISSION_BITS = 0
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
		public Precondition Requirements { get; private set; }

		public OtherRequirementAttribute(Precondition requirements)
		{
			Requirements = requirements;
		}

		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
		{
			if (context is MyCommandContext)
			{
				var cont = context as MyCommandContext;
				var user = context.User as IGuildUser;

				var permissions = (Requirements & Precondition.UserHasAPerm) != 0;
				var guildOwner = (Requirements & Precondition.GuildOwner) != 0;
				var trustedUser = (Requirements & Precondition.TrustedUser) != 0;
				var botOwner = (Requirements & Precondition.BotOwner) != 0;

				if (permissions)
				{
					var botBits = ((List<BotImplementedPermissions>)cont.GuildInfo.GetSetting(SettingOnGuild.BotUsers));
					var userBits = botBits.FirstOrDefault(x => x.UserID == user.Id)?.Permissions ?? 0;

					if (((user.GuildPermissions.RawValue | userBits) & PERMISSION_BITS) != 0)
					{
						return Task.FromResult(PreconditionResult.FromSuccess());
					}
				}
				if (guildOwner && Actions.GetIfUserIsOwner(context.Guild, user))
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
				if (trustedUser && Actions.GetIfUserIsTrustedUser(user))
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
				if (botOwner && Actions.GetIfUserIsBotOwner(user))
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
			}
			return Task.FromResult(PreconditionResult.FromError(Constants.IGNORE_ERROR));
		}
	}

	//[AttributeUsage(AttributeTargets.Class)]
	public class DefaultEnabledAttribute : Attribute
	{
		public bool Enabled { get; private set; }

		public DefaultEnabledAttribute(bool enabled)
		{
			Enabled = enabled;
		}
	}

	//[AttributeUsage(AttributeTargets.Class)]
	public class UsageAttribute : Attribute
	{
		public string Usage { get; private set; }

		public UsageAttribute(string usage)
		{
			Usage = usage;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class GuildSettingAttribute : Attribute
	{
		public ReadOnlyCollection<SettingOnGuild> Settings { get; private set; }

		public GuildSettingAttribute(params SettingOnGuild[] settings)
		{
			Settings = new ReadOnlyCollection<SettingOnGuild>(settings);
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class BotSettingAttribute : Attribute
	{
		public ReadOnlyCollection<SettingOnBot> Settings { get; private set; }

		public BotSettingAttribute(params SettingOnBot[] settings)
		{
			Settings = new ReadOnlyCollection<SettingOnBot>(settings);
		}
	}

	[AttributeUsage(AttributeTargets.Parameter)]
	public class VerifyObjectAttribute : ParameterPreconditionAttribute
	{
		private readonly ObjectVerification[] mChecks;

		public VerifyObjectAttribute(params ObjectVerification[] checks)
		{
			mChecks = checks;
		}

		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, Discord.Commands.ParameterInfo parameter, object value, IServiceProvider services)
		{
			//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
			if (value == null)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}

			return Task.FromResult(GetPreconditionResult(context, (dynamic)value));
		}

		private PreconditionResult GetPreconditionResult(ICommandContext context, System.Collections.IEnumerable list)
		{
			foreach (var item in list)
			{
				var preconditionResult = GetPreconditionResult(context, item);
				if (!preconditionResult.IsSuccess)
				{
					return preconditionResult;
				}
			}

			return PreconditionResult.FromSuccess();
		}
		private PreconditionResult GetPreconditionResult(ICommandContext context, dynamic value)
		{
			var returnedObject = Actions.GetDiscordObject(context.Guild, context.User as IGuildUser, mChecks, value);
			if (returnedObject.Reason != FailureReason.NotFailure)
			{
				return PreconditionResult.FromError(Actions.FormatErrorString(context.Guild, returnedObject));
			}
			else
			{
				return PreconditionResult.FromSuccess();
			}
		}
	}

	[AttributeUsage(AttributeTargets.Parameter)]
	public class VerifyEnumAttribute : ParameterPreconditionAttribute
	{
		private readonly uint mAllowed;
		private readonly uint mDisallowed;

		public VerifyEnumAttribute(uint allowed = 0, uint disallowed = 0)
		{
			mAllowed = allowed;
			mDisallowed = disallowed;
		}

		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, Discord.Commands.ParameterInfo parameter, object value, IServiceProvider services)
		{
			var enumVal = (uint)value;
			if (mAllowed != 0 && ((mAllowed & enumVal) == 0))
			{
				return Task.FromResult(PreconditionResult.FromError(String.Format("The option `{0}` is not allowed for the current command overload.", value)));
			}
			else if (mDisallowed != 0 && ((mDisallowed & enumVal) != 0))
			{
				return Task.FromResult(PreconditionResult.FromError(String.Format("The option `{0}` is not allowed for the current command overload.", value)));
			}
			else
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
		}
	}

	[AttributeUsage(AttributeTargets.Parameter)]
	public class VerifyStringAttribute : ParameterPreconditionAttribute
	{
		private readonly string[] mValidStrings;

		public VerifyStringAttribute(params string[] validStrings)
		{
			mValidStrings = validStrings;
		}

		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, Discord.Commands.ParameterInfo parameter, object value, IServiceProvider services)
		{
			//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
			if (value == null)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}

			return mValidStrings.CaseInsContains(value.ToString()) ? Task.FromResult(PreconditionResult.FromSuccess()) : Task.FromResult(PreconditionResult.FromError("Invalid string provided."));
		}
	}
	#endregion

	#region Typereaders
	public class IInviteTypeReader : TypeReader
	{
		public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			var invite = (await context.Guild.GetInvitesAsync()).FirstOrDefault(x => Actions.CaseInsEquals(x.Code, input));
			return invite != null ? TypeReaderResult.FromSuccess(invite) : TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching invite.");
		}
	}

	public class IBanTypeReader : TypeReader
	{
		public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			IBan ban = null;
			var bans = await context.Guild.GetBansAsync();
			if (MentionUtils.TryParseUser(input, out ulong userID))
			{
				ban = bans.FirstOrDefault(x => x.User.Id == userID);
			}
			else if (ulong.TryParse(input, out userID))
			{
				ban = bans.FirstOrDefault(x => x.User.Id == userID);
			}
			else if (input.Contains('#'))
			{
				var usernameAndDiscriminator = input.Split('#');
				if (usernameAndDiscriminator.Length == 2 && ushort.TryParse(usernameAndDiscriminator[1], out ushort discriminator))
				{
					ban = bans.FirstOrDefault(x => x.User.DiscriminatorValue == discriminator && Actions.CaseInsEquals(x.User.Username, usernameAndDiscriminator[0]));
				}
			}

			if (ban == null)
			{
				var matchingUsernames = bans.Where(x => Actions.CaseInsEquals(x.User.Username, input));

				if (matchingUsernames.Count() == 1)
				{
					ban = matchingUsernames.FirstOrDefault();
				}
				else if (matchingUsernames.Count() > 1)
				{
					return TypeReaderResult.FromError(CommandError.MultipleMatches, "Too many bans found with the same username.");
				}
			}

			return ban != null ? TypeReaderResult.FromSuccess(ban) : TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching ban.");
		}
	}
	#endregion

	#region Saved Classes
	public abstract class SettingHolder<T>
	{
		public abstract dynamic GetSetting(T setting);
		public abstract dynamic GetSetting(FieldInfo field);
		public abstract bool SetSetting(T setting, dynamic val, bool save = true);
		public abstract bool ResetSetting(T setting);
		public abstract void ResetAll();
		public virtual void PostDeserialize(ulong id) { throw new NotImplementedException(); }
		public virtual void PostDeserialize() { throw new NotImplementedException(); }
		public abstract void SaveInfo();
	}

	public class BotGuildInfo : SettingHolder<SettingOnGuild>
	{
		/* I wanted to go put all of the settings in a Dictionary<SettingOnGuild, object>;
		 * The problem with that was when deserializing JSON wouldn't deserialize to the correct type.
		 * I guess I kind of have all the settings in a dictionary, but I don't think this is nearly as efficient.
		 * Disabled warning 414 since these fields are accessed via reflection.
		 */
#pragma warning disable 414
		[JsonIgnore]
		private static ReadOnlyDictionary<SettingOnGuild, dynamic> mDefaultSettings = new ReadOnlyDictionary<SettingOnGuild, dynamic>(new Dictionary<SettingOnGuild, dynamic>
		{
			//These settings are in a jumbled order since they are mostly in order of created
			//{ SettingOnGuild.Guild, new DiscordObjectWithID<SocketGuild>(null) }, Shouldn't reset the guild setting since that's kinda barely a setting
			{ SettingOnGuild.CommandSwitches, new List<CommandSwitch>() },
			{ SettingOnGuild.CommandsDisabledOnChannel, new List<CommandOverride>() },
			{ SettingOnGuild.BotUsers, new List<BotImplementedPermissions>() },
			{ SettingOnGuild.SelfAssignableGroups, new List<SelfAssignableGroup>() },
			{ SettingOnGuild.Quotes, new List<Quote>() },
			{ SettingOnGuild.IgnoredLogChannels, new List<ulong>() },
			{ SettingOnGuild.LogActions, new List<LogAction>() },
			{ SettingOnGuild.BannedPhraseStrings, new List<BannedPhrase>() },
			{ SettingOnGuild.BannedPhraseRegex, new List<BannedPhrase>() },
			{ SettingOnGuild.BannedPhrasePunishments, new List<BannedPhrasePunishment>() },
			{ SettingOnGuild.MessageSpamPrevention, null },
			{ SettingOnGuild.LongMessageSpamPrevention, null },
			{ SettingOnGuild.LinkSpamPrevention, null },
			{ SettingOnGuild.ImageSpamPrevention, null },
			{ SettingOnGuild.MentionSpamPrevention, null },
			{ SettingOnGuild.WelcomeMessage, null },
			{ SettingOnGuild.GoodbyeMessage, null },
			{ SettingOnGuild.Prefix, null },
			{ SettingOnGuild.ServerLog, new DiscordObjectWithID<ITextChannel>(null) },
			{ SettingOnGuild.ModLog, new DiscordObjectWithID<ITextChannel>(null) },
			{ SettingOnGuild.ImageOnlyChannels, new List<ulong>() },
			{ SettingOnGuild.IgnoredCommandChannels, new List<ulong>() },
			{ SettingOnGuild.CommandsDisabledOnUser, new List<CommandOverride>() },
			{ SettingOnGuild.CommandsDisabledOnRole, new List<CommandOverride>() },
			{ SettingOnGuild.ImageLog, new DiscordObjectWithID<ITextChannel>(null) },
			{ SettingOnGuild.ListedInvite, null },
			{ SettingOnGuild.BannedNamesForJoiningUsers, new List<BannedPhrase>() },
			{ SettingOnGuild.RaidPrevention, null },
			{ SettingOnGuild.RapidJoinPrevention, null },
			{ SettingOnGuild.PyramidalRoleSystem, new PyramidalRoleSystem() },
			{ SettingOnGuild.MuteRole, new DiscordObjectWithID<IRole>(null) },
			{ SettingOnGuild.SanitaryChannels, new List<ulong>() },
		});
		[JsonIgnore]
		private static Dictionary<SettingOnGuild, FieldInfo> mFieldInfo = new Dictionary<SettingOnGuild, FieldInfo>();

		[GuildSetting(SettingOnGuild.BotUsers)]
		[JsonProperty("BotUsers")]
		private List<BotImplementedPermissions> mBotUsers = new List<BotImplementedPermissions>();
		[GuildSetting(SettingOnGuild.SelfAssignableGroups)]
		[JsonProperty("SelfAssignableGroups")]
		private List<SelfAssignableGroup> mSelfAssignableGroups = new List<SelfAssignableGroup>();
		[GuildSetting(SettingOnGuild.Quotes)]
		[JsonProperty("Quotes")]
		private List<Quote> mQuotes = new List<Quote>();
		[GuildSetting(SettingOnGuild.LogActions)]
		[JsonProperty("LogActions")]
		private List<LogAction> mLogActions = new List<LogAction>();

		[GuildSetting(SettingOnGuild.IgnoredCommandChannels)]
		[JsonProperty("IgnoredCommandChannels")]
		private List<ulong> mIgnoredCommandChannels = new List<ulong>();
		[GuildSetting(SettingOnGuild.IgnoredLogChannels)]
		[JsonProperty("IgnoredLogChannels")]
		private List<ulong> mIgnoredLogChannels = new List<ulong>();
		[GuildSetting(SettingOnGuild.ImageOnlyChannels)]
		[JsonProperty("ImageOnlyChannels")]
		private List<ulong> mImageOnlyChannels = new List<ulong>();
		[GuildSetting(SettingOnGuild.SanitaryChannels)]
		[JsonProperty("SanitaryChannels")]
		private List<ulong> mSanitaryChannels = new List<ulong>();

		[GuildSetting(SettingOnGuild.BannedPhraseStrings)]
		[JsonProperty("BannedPhraseStrings")]
		private List<BannedPhrase> mBannedPhraseStrings = new List<BannedPhrase>();
		[GuildSetting(SettingOnGuild.BannedPhraseRegex)]
		[JsonProperty("BannedPhraseRegex")]
		private List<BannedPhrase> mBannedPhraseRegex = new List<BannedPhrase>();
		[GuildSetting(SettingOnGuild.BannedNamesForJoiningUsers)]
		[JsonProperty("BannedNamesForJoiningUsers")]
		private List<BannedPhrase> mBannedNamesForJoiningUsers = new List<BannedPhrase>();
		[GuildSetting(SettingOnGuild.BannedPhrasePunishments)]
		[JsonProperty("BannedPhrasePunishments")]
		private List<BannedPhrasePunishment> mBannedPhrasePunishments = new List<BannedPhrasePunishment>();

		[GuildSetting(SettingOnGuild.CommandSwitches)]
		[JsonProperty("CommandSwitches")]
		private List<CommandSwitch> mCommandSwitches = new List<CommandSwitch>();
		[GuildSetting(SettingOnGuild.CommandsDisabledOnUser)]
		[JsonProperty("CommandsDisabledOnUser")]
		private List<CommandOverride> mCommandsDisabledOnUser = new List<CommandOverride>();
		[GuildSetting(SettingOnGuild.CommandsDisabledOnRole)]
		[JsonProperty("CommandsDisabledOnRole")]
		private List<CommandOverride> mCommandsDisabledOnRole = new List<CommandOverride>();
		[GuildSetting(SettingOnGuild.CommandsDisabledOnChannel)]
		[JsonProperty("CommandsDisabledOnChannel")]
		private List<CommandOverride> mCommandsDisabledOnChannel = new List<CommandOverride>();

		[GuildSetting(SettingOnGuild.Guild)]
		[JsonProperty("Guild")]
		private DiscordObjectWithID<SocketGuild> mGuild = new DiscordObjectWithID<SocketGuild>(null);
		[GuildSetting(SettingOnGuild.ServerLog)]
		[JsonProperty("ServerLog")]
		private DiscordObjectWithID<ITextChannel> mServerLog = new DiscordObjectWithID<ITextChannel>(null);
		[GuildSetting(SettingOnGuild.ModLog)]
		[JsonProperty("ModLog")]
		private DiscordObjectWithID<ITextChannel> mModLog = new DiscordObjectWithID<ITextChannel>(null);
		[GuildSetting(SettingOnGuild.ImageLog)]
		[JsonProperty("ImageLog")]
		private DiscordObjectWithID<ITextChannel> mImageLog = new DiscordObjectWithID<ITextChannel>(null);
		[GuildSetting(SettingOnGuild.MuteRole)]
		[JsonProperty("MuteRole")]
		private DiscordObjectWithID<IRole> mMuteRole = new DiscordObjectWithID<IRole>(null);

		[GuildSetting(SettingOnGuild.MessageSpamPrevention)]
		[JsonProperty("MessageSpamPrevention")]
		private SpamPrevention mMessageSpamPrevention = null;
		[GuildSetting(SettingOnGuild.LongMessageSpamPrevention)]
		[JsonProperty("LongMessageSpamPrevention")]
		private SpamPrevention mLongMessageSpamPrevention = null;
		[GuildSetting(SettingOnGuild.LinkSpamPrevention)]
		[JsonProperty("LinkSpamPrevention")]
		private SpamPrevention mLinkSpamPrevention = null;
		[GuildSetting(SettingOnGuild.ImageSpamPrevention)]
		[JsonProperty("ImageSpamPrevention")]
		private SpamPrevention mImageSpamPrevention = null;
		[GuildSetting(SettingOnGuild.MentionSpamPrevention)]
		[JsonProperty("MentionSpamPrevention")]
		private SpamPrevention mMentionSpamPrevention = null;
		[GuildSetting(SettingOnGuild.RaidPrevention)]
		[JsonProperty("RaidPrevention")]
		private RaidPrevention mRaidPrevention = null;
		[GuildSetting(SettingOnGuild.RapidJoinPrevention)]
		[JsonProperty("RapidJoinPrevention")]
		private RaidPrevention mRapidJoinPrevention = null;

		[GuildSetting(SettingOnGuild.PyramidalRoleSystem)]
		[JsonProperty("PyramidalRoleSystem")]
		private PyramidalRoleSystem mPyramidalRoleSystem = new PyramidalRoleSystem();
		[GuildSetting(SettingOnGuild.WelcomeMessage)]
		[JsonProperty("WelcomeMessage")]
		private GuildNotification mWelcomeMessage = null;
		[GuildSetting(SettingOnGuild.GoodbyeMessage)]
		[JsonProperty("GoodbyeMessage")]
		private GuildNotification mGoodbyeMessage = null;
		[GuildSetting(SettingOnGuild.ListedInvite)]
		[JsonProperty("ListedInvite")]
		private ListedInvite mListedInvite = null;
		[GuildSetting(SettingOnGuild.Prefix)]
		[JsonProperty("Prefix")]
		private string mPrefix = null;
		[GuildSetting(SettingOnGuild.VerboseErrors)]
		[JsonProperty("VerboseErrors")]
		private bool mVerboseErrors = true;

		[GuildSetting(SettingOnGuild.BannedPhraseUsers)]
		[JsonIgnore]
		private List<BannedPhraseUser> mBannedPhraseUsers = new List<BannedPhraseUser>();
		[GuildSetting(SettingOnGuild.SpamPreventionUsers)]
		[JsonIgnore]
		private List<SpamPreventionUser> mSpamPreventionUsers = new List<SpamPreventionUser>();
		[GuildSetting(SettingOnGuild.SlowmodeChannels)]
		[JsonIgnore]
		private List<SlowmodeChannel> mSlowmodeChannels = new List<SlowmodeChannel>();
		[GuildSetting(SettingOnGuild.Invites)]
		[JsonIgnore]
		private List<BotInvite> mInvites = new List<BotInvite>();
		[GuildSetting(SettingOnGuild.EvaluatedRegex)]
		[JsonIgnore]
		private List<string> mEvaluatedRegex = new List<string>();
		[GuildSetting(SettingOnGuild.SlowmodeGuild)]
		[JsonIgnore]
		private SlowmodeGuild mSlowmodeGuild = null;
		[GuildSetting(SettingOnGuild.MessageDeletion)]
		[JsonIgnore]
		private MessageDeletion mMessageDeletion = new MessageDeletion();
		[GuildSetting(SettingOnGuild.Loaded)]
		[JsonIgnore]
		private bool mLoaded = false;
#pragma warning restore 414

		public BotGuildInfo(ulong guildID)
		{
			mGuild = new DiscordObjectWithID<SocketGuild>(guildID);
		}

		private static FieldInfo CreateFieldDictionaryItem(SettingOnGuild setting)
		{
			foreach (var field in typeof(BotGuildInfo).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				var attr = (GuildSettingAttribute)field.GetCustomAttribute(typeof(GuildSettingAttribute));
				if (attr != null)
				{
					if (attr.Settings.Contains(setting))
					{
						return field;
					}
				}
			}
			return null;
		}
		public static void CreateFieldDictionary()
		{
			foreach (var setting in Enum.GetValues(typeof(SettingOnGuild)).Cast<SettingOnGuild>())
			{
				if (mFieldInfo.ContainsKey(setting))
					break;

				mFieldInfo.Add(setting, CreateFieldDictionaryItem(setting));
			}
		}
		public static FieldInfo GetField(SettingOnGuild setting)
		{
			if (mFieldInfo.TryGetValue(setting, out FieldInfo value))
			{
				return value;
			}
			else
			{
				return null;
			}
		}
		public override object GetSetting(SettingOnGuild setting)
		{
			var field = GetField(setting);
			if (field != null)
			{
				return field.GetValue(this);
			}
			else
			{
				return null;
			}
		}
		public override object GetSetting(FieldInfo field)
		{
			try
			{
				return field.GetValue(this);
			}
			catch (Exception e)
			{
				Actions.ExceptionToConsole(e);
				return null;
			}
		}
		public override bool SetSetting(SettingOnGuild setting, dynamic val, bool save = true)
		{
			var field = GetField(setting);
			if (field != null)
			{
				try
				{
					field.SetValue(this, val);
					if (save)
					{
						SaveInfo();
					}
					return true;
				}
				catch (Exception e)
				{
					Actions.ExceptionToConsole(e);
					return false;
				}
			}
			else
			{
				return false;
			}
		}
		public override bool ResetSetting(SettingOnGuild setting)
		{
			var field = GetField(setting);
			if (field != null && mDefaultSettings.TryGetValue(setting, out dynamic val))
			{
				try
				{
					field.SetValue(this, val);
					SaveInfo();
					return true;
				}
				catch (Exception e)
				{
					Actions.ExceptionToConsole(e);
					return false;
				}
			}
			else
			{
				return false;
			}
		}
		public override void ResetAll()
		{
			foreach (var setting in Enum.GetValues(typeof(SettingOnGuild)).Cast<SettingOnGuild>())
			{
				ResetSetting(setting);
			}
		}
		public override void PostDeserialize(ulong guildID)
		{
			var guild = ((DiscordObjectWithID<SocketGuild>)GetSetting(SettingOnGuild.Guild));
			if (guild.ID == 0)
			{
				SetSetting(SettingOnGuild.Guild, new DiscordObjectWithID<SocketGuild>(guildID));
			}
			guild.PostDeserialize(null);

			var modLog = ((DiscordObjectWithID<ITextChannel>)GetSetting(SettingOnGuild.ModLog));
			if (modLog != null)
			{
				modLog.PostDeserialize(guild.Object);
			}

			var serverLog = ((DiscordObjectWithID<ITextChannel>)GetSetting(SettingOnGuild.ServerLog));
			if (serverLog != null)
			{
				serverLog.PostDeserialize(guild.Object);
			}

			var imageLog = ((DiscordObjectWithID<ITextChannel>)GetSetting(SettingOnGuild.ImageLog));
			if (imageLog != null)
			{
				imageLog.PostDeserialize(guild.Object);
			}

			var muteRole = ((DiscordObjectWithID<IRole>)GetSetting(SettingOnGuild.MuteRole));
			if (muteRole != null)
			{
				muteRole.PostDeserialize(guild.Object);
			}

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

			mLoaded = true;
		}
		public override void SaveInfo()
		{
			var guildID = ((DiscordObjectWithID<SocketGuild>)GetSetting(SettingOnGuild.Guild)).ID;
			if (guildID != 0)
			{
				Actions.OverWriteFile(Actions.GetServerFilePath(guildID, Constants.GUILD_INFO_LOCATION), Actions.Serialize(this));
			}
		}

		public SpamPrevention GetSpamPrevention(SpamType spamType)
		{
			switch (spamType)
			{
				case SpamType.Message:
				{
					return (SpamPrevention)GetSetting(SettingOnGuild.MessageSpamPrevention);
				}
				case SpamType.LongMessage:
				{
					return (SpamPrevention)GetSetting(SettingOnGuild.LongMessageSpamPrevention);
				}
				case SpamType.Link:
				{
					return (SpamPrevention)GetSetting(SettingOnGuild.LinkSpamPrevention);
				}
				case SpamType.Image:
				{
					return (SpamPrevention)GetSetting(SettingOnGuild.ImageSpamPrevention);
				}
				case SpamType.Mention:
				{
					return (SpamPrevention)GetSetting(SettingOnGuild.MentionSpamPrevention);
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
					SetSetting(SettingOnGuild.MessageSpamPrevention, spamPrev);
					return;
				}
				case SpamType.LongMessage:
				{
					SetSetting(SettingOnGuild.LongMessageSpamPrevention, spamPrev);
					return;
				}
				case SpamType.Link:
				{
					SetSetting(SettingOnGuild.LinkSpamPrevention, spamPrev);
					return;
				}
				case SpamType.Image:
				{
					SetSetting(SettingOnGuild.ImageSpamPrevention, spamPrev);
					return;
				}
				case SpamType.Mention:
				{
					SetSetting(SettingOnGuild.MentionSpamPrevention, spamPrev);
					return;
				}
			}

			SaveInfo();
		}
		public RaidPrevention GetRaidPrevention(RaidType raidType)
		{
			switch (raidType)
			{
				case RaidType.Regular:
				{
					return (RaidPrevention)GetSetting(SettingOnGuild.RaidPrevention);
				}
				case RaidType.RapidJoins:
				{
					return (RaidPrevention)GetSetting(SettingOnGuild.RapidJoinPrevention);
				}
				default:
				{
					return null;
				}
			}
		}
		public void SetRaidPrevention(RaidType raidType, RaidPrevention raidPrev)
		{
			switch (raidType)
			{
				case RaidType.Regular:
				{
					SetSetting(SettingOnGuild.RaidPrevention, raidPrev);
					return;
				}
				case RaidType.RapidJoins:
				{
					SetSetting(SettingOnGuild.RaidPrevention, raidPrev);
					return;
				}
			}

			SaveInfo();
		}
	}

	public class BotGlobalInfo : SettingHolder<SettingOnBot>
	{
		//Disabling for same reason as BotGuildInfo
#pragma warning disable 414
		[JsonIgnore]
		private static ReadOnlyDictionary<SettingOnBot, dynamic> mDefaultSettings = new ReadOnlyDictionary<SettingOnBot, dynamic>(new Dictionary<SettingOnBot, dynamic>
		{
			{ SettingOnBot.BotOwnerID, (ulong)0 }, //Needs to be cast as a ulong or gets an exception when trying to set it
			{ SettingOnBot.TrustedUsers, new List<ulong>() },
			{ SettingOnBot.Prefix, Constants.BOT_PREFIX },
			{ SettingOnBot.Game, String.Format("type \"{0}help\" for help.", Constants.BOT_PREFIX) },
			{ SettingOnBot.Stream, null },
			//{ SettingOnBot.ShardCount, 1 }, Leaving this one out since shard count shouldn't be reset without checking guild count
			{ SettingOnBot.MessageCacheCount, 1000 },
			{ SettingOnBot.AlwaysDownloadUsers, true },
			{ SettingOnBot.LogLevel, LogSeverity.Warning },
			{ SettingOnBot.MaxUserGatherCount, 100 },
			{ SettingOnBot.MaxMessageGatherSize, 500000 },
			{ SettingOnBot.UnableToDMOwnerUsers, new List<ulong>() },
			{ SettingOnBot.IgnoredCommandUsers, new List<ulong>() },
		});
		[JsonIgnore]
		private static Dictionary<SettingOnBot, FieldInfo> mFieldInfo = new Dictionary<SettingOnBot, FieldInfo>();

		[BotSetting(SettingOnBot.BotOwnerID)]
		[JsonProperty("BotOwnerID")]
		private ulong mBotOwnerID = 0;
		[BotSetting(SettingOnBot.TrustedUsers)]
		[JsonProperty("TrustedUsers")]
		private List<ulong> mTrustedUsers = new List<ulong>();
		[BotSetting(SettingOnBot.Prefix)]
		[JsonProperty("Prefix")]
		private string mPrefix = Constants.BOT_PREFIX;
		[BotSetting(SettingOnBot.Game)]
		[JsonProperty("Game")]
		private string mGame = String.Format("type \"{0}help\" for help.", Constants.BOT_PREFIX);
		[BotSetting(SettingOnBot.Stream)]
		[JsonProperty("Stream")]
		private string mStream = null;
		[BotSetting(SettingOnBot.ShardCount)]
		[JsonProperty("ShardCount")]
		private int mShardCount = 1;
		[BotSetting(SettingOnBot.MessageCacheCount)]
		[JsonProperty("MessageCacheCount")]
		private int mMessageCacheCount = 1000;
		[BotSetting(SettingOnBot.AlwaysDownloadUsers)]
		[JsonProperty("AlwaysDownloadUsers")]
		private bool mAlwaysDownloadUsers = true;
		[BotSetting(SettingOnBot.LogLevel)]
		[JsonProperty("LogLevel")]
		private LogSeverity mLogLevel = LogSeverity.Warning;
		[BotSetting(SettingOnBot.MaxUserGatherCount)]
		[JsonProperty("MaxUserGatherCount")]
		private int mMaxUserGatherCount = 100;
		[BotSetting(SettingOnBot.MaxMessageGatherSize)]
		[JsonProperty("MaxMessageGatherSize")]
		private int mMaxMessageGatherSize = 500000;
		[BotSetting(SettingOnBot.UnableToDMOwnerUsers)]
		[JsonProperty("UnableToDMOwnerUsers")]
		private List<ulong> mUnableToDMOwnerUsers = new List<ulong>();
		[BotSetting(SettingOnBot.IgnoredCommandUsers)]
		[JsonProperty("IgnoredCommandUsers")]
		private List<ulong> mIgnoredCommandUsers = new List<ulong>();
#pragma warning restore 414

		private static FieldInfo CreateFieldDictionaryItem(SettingOnBot setting)
		{
			foreach (var field in typeof(BotGlobalInfo).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				var attr = (BotSettingAttribute)field.GetCustomAttribute(typeof(BotSettingAttribute));
				if (attr != null)
				{
					if (attr.Settings.Contains(setting))
					{
						return field;
					}
				}
			}
			return null;
		}
		public static void CreateFieldDictionary()
		{
			foreach (var setting in Enum.GetValues(typeof(SettingOnBot)).Cast<SettingOnBot>())
			{
				mFieldInfo.Add(setting, CreateFieldDictionaryItem(setting));
			}
		}
		public static FieldInfo GetField(SettingOnBot setting)
		{
			if (mFieldInfo.TryGetValue(setting, out FieldInfo value))
			{
				return value;
			}
			else
			{
				return null;
			}
		}
		public override object GetSetting(SettingOnBot setting)
		{
			var field = GetField(setting);
			if (field != null)
			{
				return field.GetValue(this);
			}
			else
			{
				return null;
			}
		}
		public override object GetSetting(FieldInfo field)
		{
			try
			{
				return field.GetValue(this);
			}
			catch (Exception e)
			{
				Actions.ExceptionToConsole(e);
				return null;
			}
		}
		public override bool SetSetting(SettingOnBot setting, dynamic val, bool save = true)
		{
			var field = GetField(setting);
			if (field != null)
			{
				try
				{
					field.SetValue(this, val);
					if (save)
					{
						SaveInfo();
					}
					return true;
				}
				catch (Exception e)
				{
					Actions.ExceptionToConsole(e);
					return false;
				}
			}
			else
			{
				return false;
			}
		}
		public override bool ResetSetting(SettingOnBot setting)
		{
			var field = GetField(setting);
			if (field != null && mDefaultSettings.TryGetValue(setting, out dynamic val))
			{
				try
				{
					field.SetValue(this, val);
					SaveInfo();
					return true;
				}
				catch (Exception e)
				{
					Actions.ExceptionToConsole(e);
					return false;
				}
			}
			else
			{
				return false;
			}
		}
		public override void ResetAll()
		{
			foreach (var setting in Enum.GetValues(typeof(SettingOnBot)).Cast<SettingOnBot>())
			{
				if (setting == SettingOnBot.ShardCount)
				{
					//Don't reset shards to 1. Reset it to enough to allow the current amount of guilds + some buffer
					SetSetting(setting, Variables.Client.GetGuilds().Count / 2500 + 1);
				}
				else
				{
					ResetSetting(setting);
				}
			}
		}
		public override void PostDeserialize()
		{
			//Probably will be needed in the future, but for now it's just an empty method. I think it's being called in a few spots too, hmm.
		}
		public override void SaveInfo()
		{
			Actions.OverWriteFile(Actions.GetBaseBotDirectory(Constants.BOT_INFO_LOCATION), Actions.Serialize(this));
		}
	}

	public abstract class Setting
	{
		public abstract string SettingToString();
		public abstract string SettingToString(SocketGuild guild);
	}

	public class CommandOverride : Setting
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
		public string CategoryName { get { return Category.EnumName(); } }
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

		//TODO: Fix this weird shit that's using ternary operators
		public void ChangePunishment(PunishmentType type)
		{
			Punishment = (type == PunishmentType.Deafen || type == PunishmentType.Mute) ? PunishmentType.Nothing : type;
		}
		public override string SettingToString()
		{
			return String.Format("`{0}` `{1}`", Punishment.EnumName().Substring(0, 1), Phrase);
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
				Role == null ? Punishment.EnumName() : Role.Name,
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
		[JsonProperty]
		public ulong UserID { get; private set; }
		[JsonProperty]
		public uint Permissions { get; private set; }

		public BotImplementedPermissions(ulong userID, uint permissions, BotGuildInfo guildInfo = null)
		{
			UserID = userID;
			Permissions = permissions;
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
			return String.Format("**User:** `{0}`\n**Permissions:** `{1}`", UserID, Permissions);
		}
		public override string SettingToString(SocketGuild guild)
		{
			return String.Format("**User:** `{0}`\n**Permissions:** `{1}`", guild.GetUser(UserID).FormatUser(), Permissions);
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
			if (String.IsNullOrWhiteSpace(Code))
			{
				return null;
			}

			var codeStr = String.Format("**Code:** `{0}`\n", Code);
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

	public class Quote : Setting
	{
		[JsonProperty]
		public string Name { get; private set; }
		[JsonProperty]
		public string Text { get; private set; }

		public Quote(string name, string text)
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
		[JsonIgnore]
		private ReadOnlyDictionary<Type, Func<SocketGuild, ulong, dynamic>> inits = new ReadOnlyDictionary<Type, Func<SocketGuild, ulong, dynamic>>(new Dictionary<Type, Func<SocketGuild, ulong, dynamic>>
		{
			{ typeof(IRole), (SocketGuild guild, ulong ID) => { return guild.GetRole(ID); } },
			{ typeof(ITextChannel), (SocketGuild guild, ulong ID) => { return guild.GetTextChannel(ID); } },
			{ typeof(SocketGuild), (SocketGuild guild, ulong ID) => { return Variables.Client.GetGuild(ID); } },
		});

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
			if (inits.TryGetValue(typeof(T), out var method))
			{
				Object = method(guild, ID);
			}
		}
		public override string SettingToString()
		{
			if (Object != null)
			{
				return Actions.FormatObject((dynamic)Object);
			}
			else
			{
				return null;
			}
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
			var guildInfo = await Actions.CreateOrGetGuildInfo(guild);
			switch (PunishmentType)
			{
				case PunishmentType.Ban:
				{
					await Actions.BotBanUser(guild, user.Id, 1, "spam prevention.");
					break;
				}
				case PunishmentType.Kick:
				{
					await Actions.BotKickUser(user, "spam prevention");
					break;
				}
				case PunishmentType.KickThenBan:
				{
					if (((List<SpamPreventionUser>)guildInfo.GetSetting(SettingOnGuild.SpamPreventionUsers)).FirstOrDefault(x => x.User.Id == user.Id).AlreadyKicked)
					{
						await Actions.BotBanUser(guild, user.Id, 1, "spam prevention");
					}
					else
					{
						await Actions.BotKickUser(user, "spam prevention");
					}
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
				PunishmentType.EnumName());
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
			var guildInfo = await Actions.CreateOrGetGuildInfo(guild);
			switch (PunishmentType)
			{
				case PunishmentType.Ban:
				{
					await Actions.BotBanUser(guild, user.Id, 1, "raid prevention");
					break;
				}
				case PunishmentType.Kick:
				{
					await Actions.BotKickUser(user, "raid prevention");
					break;
				}
				case PunishmentType.KickThenBan:
				{
					if (((List<SpamPreventionUser>)guildInfo.GetSetting(SettingOnGuild.SpamPreventionUsers)).FirstOrDefault(x => x.User.Id == user.Id).AlreadyKicked)
					{
						await Actions.BotBanUser(guild, user.Id, 1, "raid prevention");
					}
					else
					{
						await Actions.BotKickUser(user, "raid prevention");
					}
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
				PunishmentType.EnumName());
		}
		public override string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}
	#endregion

	#region Non-saved Classes
	public class MyCommandContext : CommandContext
	{
		public BotGuildInfo GuildInfo { get; private set; }

		public MyCommandContext(BotGuildInfo guildInfo, IDiscordClient client, IUserMessage msg) : base(client, msg)
		{
			GuildInfo = guildInfo;
		}
	}

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
		private const string placeHolderStr = "N/A";

		public HelpEntry(string name, string[] aliases, string usage, string basePerm, string text, CommandCategory category, bool defaultEnabled)
		{
			Name = String.IsNullOrWhiteSpace(name) ? placeHolderStr : name;
			Aliases = aliases ?? new[] { placeHolderStr };
			Usage = String.IsNullOrWhiteSpace(usage) ? placeHolderStr : ((string)Variables.BotInfo.GetSetting(SettingOnBot.Prefix)) + usage;
			BasePerm = String.IsNullOrWhiteSpace(basePerm) ? placeHolderStr : basePerm;
			Text = String.IsNullOrWhiteSpace(text) ? placeHolderStr : text;
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

		public SlowmodeUser(IGuildUser user = null, int baseMessages = 1, int interval = 5)
		{
			User = user;
			CurrentMessagesLeft = baseMessages;
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
				((List<BannedPhraseUser>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseUsers)).ThreadSafeAdd(this);
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
		public List<IMessage> GetList()
		{
			return mMessages.ToList();
		}
		public void SetList(List<IMessage> InList)
		{
			mMessages = InList.ToList();
		}
		public void AddToList(IMessage Item)
		{
			mMessages.Add(Item);
		}
		public void ClearList()
		{
			mMessages.Clear();
		}
	}

	public class SlowmodeGuild
	{
		public int BaseMessages { get; private set; }
		public int Interval { get; private set; }
		public List<SlowmodeUser> Users { get; private set; }

		public SlowmodeGuild(int baseMessages, int interval)
		{
			BaseMessages = baseMessages;
			Interval = interval;
			Users = new List<SlowmodeUser>();
		}
		public SlowmodeGuild(int baseMessages, int interval, List<SlowmodeUser> users)
		{
			BaseMessages = baseMessages;
			Interval = interval;
			Users = users;
		}
	}

	public class SlowmodeChannel
	{
		public ulong ChannelID { get; private set; }
		public int BaseMessages { get; private set; }
		public int Interval { get; private set; }
		public List<SlowmodeUser> Users { get; private set; }

		public SlowmodeChannel(ulong channelID, int baseMessages, int interval)
		{
			ChannelID = channelID;
			BaseMessages = baseMessages;
			Interval = interval;
			Users = new List<SlowmodeUser>();
		}
		public SlowmodeChannel(ulong channelID, int baseMessages, int interval, List<SlowmodeUser> users)
		{
			ChannelID = channelID;
			BaseMessages = baseMessages;
			Interval = interval;
			Users = users;
		}

		public void SetUserList(List<SlowmodeUser> users)
		{
			Users = users;
		}
	}

	public class SpamPreventionUser
	{
		public IGuildUser User { get; private set; }
		public int VotesToKick { get; private set; } = 0;
		public int VotesRequired { get; private set; } = int.MaxValue;
		public bool PotentialPunishment { get; private set; } = false;
		public bool AlreadyKicked { get; private set; } = false;
		public List<ulong> UsersWhoHaveAlreadyVoted { get; private set; } = new List<ulong>();
		public PunishmentType Punishment { get; private set; } = PunishmentType.Nothing;
		public Dictionary<SpamType, List<BasicTimeInterface>> SpamLists { get; private set; } = new Dictionary<SpamType, List<BasicTimeInterface>>();

		public SpamPreventionUser(IGuildUser user)
		{
			User = user;
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
		public void ChangePunishmentType(PunishmentType punishmentType)
		{
			if (Constants.Severity[punishmentType] > Constants.Severity[Punishment])
			{
				Punishment = punishmentType;
			}
		}
		public void EnablePunishable()
		{
			PotentialPunishment = true;
		}
		public void ResetSpamUser()
		{
			//Don't reset already kicked since KickThenBan requires it
			VotesToKick = 0;
			VotesRequired = int.MaxValue;
			PotentialPunishment = false;
			UsersWhoHaveAlreadyVoted.Clear();
			Punishment = PunishmentType.Nothing;
			foreach (var spamList in SpamLists.Values)
			{
				spamList.Clear();
			}
		}
		public bool CheckIfAllowedToPunish(SpamPrevention spamPrev, SpamType spamType, IMessage msg)
		{
			return Actions.GetCountOfItemsInTimeFrame(SpamLists[spamType], spamPrev.TimeInterval) >= spamPrev.RequiredSpamInstances;
		}
		public async Task Punish(BotGuildInfo guildInfo, IGuild guild)
		{
			switch (Punishment)
			{
				case PunishmentType.Role:
				{
					await Actions.MuteUser(guildInfo, User);
					return;
				}
				case PunishmentType.Kick:
				{
					await Actions.BotKickUser(User, "voted spam prevention");
					return;
				}
				case PunishmentType.KickThenBan:
				{
					//Check if they've already been kicked to determine if they should be banned or kicked
					if (AlreadyKicked)
					{
						await Actions.BotBanUser(guild, User.Id, 1, "voted spam prevention");
					}
					else
					{
						await Actions.BotKickUser(User, "voted spam prevention");
					}
					return;
				}
				case PunishmentType.Ban:
				{
					await Actions.BotBanUser(guild, User.Id, 1, "voted spam prevention");
					return;
				}
				default:
				{
					return;
				}
			}
		}
	}
	#endregion

	#region Structs
	public struct BotGuildPermission
	{
		public string Name { get; private set; }
		public int Position { get; private set; }

		public BotGuildPermission(string name, int position)
		{
			Name = name;
			Position = position;
		}
	}

	public struct BotChannelPermission
	{
		public string Name { get; private set; }
		public int Position { get; private set; }
		public bool General { get; private set; }
		public bool Text { get; private set; }
		public bool Voice { get; private set; }

		public BotChannelPermission(string name, int position, bool gen = false, bool text = false, bool voice = false)
		{
			Name = name;
			Position = position;
			General = gen;
			Text = text;
			Voice = voice;
		}
	}

	public struct ActiveCloseWord<T> : ITimeInterface
	{
		public ulong UserID { get; private set; }
		public List<CloseWord<T>> List { get; private set; }
		public DateTime Time { get; private set; }

		public ActiveCloseWord(ulong userID, IEnumerable<CloseWord<T>> list)
		{
			UserID = userID;
			List = list.ToList();
			Time = DateTime.UtcNow.AddMilliseconds(Constants.ACTIVE_CLOSE);
		}

		public DateTime GetTime()
		{
			return Time;
		}
	}

	public struct CloseWord<T>
	{
		public T Word { get; private set; }
		public int Closeness { get; private set; }

		public CloseWord(T word, int closeness)
		{
			Word = word;
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
		public IEnumerable<IMessage> Messages { get; private set; }
		public IMessageChannel Channel { get; private set; }
		public DateTime Time { get; private set; }

		public RemovableMessage(IMessage message, DateTime time)
		{
			Messages = new[] { message };
			Channel = message.Channel;
			Time = time;
		}
		public RemovableMessage(IEnumerable<IMessage> messages, DateTime time)
		{
			Messages = messages;
			Channel = messages.FirstOrDefault().Channel;
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

	public struct ReturnedObject<T>
	{
		public T Object { get; private set; }
		public FailureReason Reason { get; private set; }

		public ReturnedObject(T obj, FailureReason reason)
		{
			Object = obj;
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
		public FailureReason Reason { get; private set; }

		public ReturnedArguments(List<string> args, FailureReason reason)
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
			Reason = FailureReason.NotFailure;
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
		public FailureReason Reason { get; private set; }
		public List<IBan> MatchedBans { get; private set; }

		public ReturnedBannedUser(IBan ban, FailureReason reason, List<IBan> matchedBans = null)
		{
			Ban = ban;
			Reason = reason;
			MatchedBans = matchedBans;
		}
	}

	public struct ReturnedSetting
	{
		public string Setting { get; private set; }
		public NSF Status { get; private set; }

		public ReturnedSetting(SettingOnBot setting, NSF status)
		{
			Setting = setting.EnumName();
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

	public struct LoggedCommand
	{
		public string Guild { get; private set; }
		public string Channel { get; private set; }
		public string User { get; private set; }
		public string Time { get; private set; }
		public string Text { get; private set; }

		public LoggedCommand(ICommandContext context)
		{
			Guild = context.Guild.FormatGuild();
			Channel = context.Channel.FormatChannel();
			User = context.User.FormatUser();
			Time = Actions.FormatDateTime(context.Message.CreatedAt);
			Text = context.Message.Content;
		}

		public override string ToString()
		{
			var guild = String.Format("Guild: {0}", Guild);
			var channel = String.Format("Channel: {0}", Channel);
			var user = String.Format("User: {0}", User);
			var time = String.Format("Time: {0}", Time);
			var text = String.Format("Text: {0}", Text);
			return String.Join(Environment.NewLine + new string(' ', 25), new[] { guild, channel, user, time, text });
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

	public enum CommandCategory
	{
		GlobalSettings					= 1,
		GuildSettings					= 2,
		Logs							= 3,
		BanPhrases						= 4,
		SelfRoles						= 5,
		UserModeration					= 6,
		RoleModeration					= 7,
		ChannelModeration				= 8,
		GuildModeration					= 9,
		Miscellaneous					= 10,
		SpamPrevention					= 11,
		InviteModeration				= 12,
		GuildList						= 13,
		NicknameModeration				= 14,
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
		BotOwnerID						= 1,
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

	public enum EmoteType
	{
		Global							= 1,
		Guild							= 2,
	}

	public enum ChannelType
	{
		Text							= 1,
		Voice							= 2,
	}

	public enum LogChannelType
	{
		Server							= 1,
		Mod								= 2,
		Image							= 3,
	}

	public enum ObjectVerification
	{
		//Generic
		None							= 0,
		CanBeEdited						= 1,

		//User
		CanBeMovedFromChannel			= 100,

		//Channels
		IsVoice							= 200,
		IsText							= 201,
		CanBeReordered					= 202,
		CanModifyPermissions			= 203,
		CanBeManaged					= 204,
		CanMoveUsers					= 205,
		CanDeleteMessages				= 206,
		CanBeRead						= 207,
		CanCreateInstantInvite			= 208,
		IsDefault						= 209,

		//Roles
		IsEveryone						= 300,
		IsManaged						= 301,
	}

	public enum FailureReason
	{
		//Generic
		NotFailure						= 0,
		TooFew							= 1,
		TooMany							= 2,

		//User
		UserInability					= 100,
		BotInability					= 101,
		
		//Channels
		ChannelType						= 200,
		DefaultChannel					= 201,

		//Roles
		EveryoneRole					= 300,
		ManagedRole						= 301,

		//Enums
		InvalidEnum						= 400,

		//Bans
		NoBans							= 500,
		InvalidDiscriminator			= 501,
		InvalidID						= 502,
		NoUsernameOrID					= 503,
	}

	public enum NSF
	{
		Nothing							= 0,
		Success							= 1,
		Failure							= 2,
	}

	public enum FileType
	{
		GuildInfo						= 0,
	}

	[Flags]
	public enum PunishmentType : uint
	{
		Nothing							= (1U << 0),
		Kick							= (1U << 1),
		Ban								= (1U << 2),
		Role							= (1U << 3),
		Deafen							= (1U << 4),
		Mute							= (1U << 5),
		KickThenBan						= (1U << 6),
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
	public enum GetUsersWithReasonTarget : uint
	{
		Role							= (1U << 0),
		Name							= (1U << 1),
		Game							= (1U << 2),
		Stream							= (1U << 3),
	}

	[Flags]
	public enum GetIDInfoType : uint
	{
		Guild							= (1U << 0),
		Channel							= (1U << 1),
		Role							= (1U << 2),
		User							= (1U << 3),
		Emote							= (1U << 4),
		Invite							= (1U << 5),
		Bot								= (1U << 6),
	}
	#endregion
}