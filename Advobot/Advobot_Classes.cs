using Advobot.Actions;
using Advobot.Logging;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Advobot
{
	#region Attributes
	[AttributeUsage(AttributeTargets.Class)]
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
		/* For when/if GuildPermission values get put as bits
		public PermissionRequirementAttribute(GuildPermission anyOfTheListedPerms, GuildPermission allOfTheListedPerms)
		{
			throw new NotImplementedException();
		}
		public PermissionRequirementAttribute(uint anyOfTheListedPerms = 0, uint allOfTheListedPerms = 0)
		{
			mAnyFlags = anyOfTheListedPerms | (1U << (int)GuildPermission.Administrator);
			mAllFlags = allOfTheListedPerms;
		}
		*/

		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
		{
			if (context is MyCommandContext)
			{
				var cont = context as MyCommandContext;
				var user = context.User as IGuildUser;

				var userBits = cont.GuildSettings.BotUsers.FirstOrDefault(x => x.UserID == user.Id)?.Permissions ?? 0;

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
			get { return String.Join(" & ", Gets.GetPermissionNames(mAllFlags)); }
		}
		public string AnyText
		{
			get { return String.Join(" | ", Gets.GetPermissionNames(mAnyFlags)); }
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
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
		public Precondition Requirements { get; }

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
					var userBits = cont.GuildSettings.BotUsers.FirstOrDefault(x => x.UserID == user.Id)?.Permissions ?? 0;
					if (((user.GuildPermissions.RawValue | userBits) & PERMISSION_BITS) != 0)
					{
						return Task.FromResult(PreconditionResult.FromSuccess());
					}
				}
				if (guildOwner && cont.Guild.OwnerId == user.Id)
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
				if (trustedUser && cont.GlobalInfo.TrustedUsers.Contains(user.Id))
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
				if (botOwner && cont.GlobalInfo.BotOwnerID == user.Id)
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
			}
			return Task.FromResult(PreconditionResult.FromError(Constants.IGNORE_ERROR));
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class CommandRequirementsAttribute : PreconditionAttribute
	{
		public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (context is MyCommandContext)
			{
				var cont = context as MyCommandContext;
				var user = context.User as IGuildUser;

				if (!(await cont.Guild.GetCurrentUserAsync()).GuildPermissions.Administrator)
				{
					return PreconditionResult.FromError("This bot will not function without the `Administrator` permission.");
				}
				else if (!cont.GlobalInfo.Loaded)
				{
					return PreconditionResult.FromError("Wait until the bot is loaded.");
				}
				if (!cont.GuildSettings.Loaded)
				{
					return PreconditionResult.FromError("Wait until the guild is loaded.");
				}
				else if (cont.GuildSettings.IgnoredCommandChannels.Contains(context.Channel.Id) || !CheckIfCommandIsEnabled(cont, command, user))
				{
					return PreconditionResult.FromError(Constants.IGNORE_ERROR);
				}
				else
				{
					return PreconditionResult.FromSuccess();
				}
			}
			return PreconditionResult.FromError(Constants.IGNORE_ERROR);
		}

		private bool CheckIfCommandIsEnabled(MyCommandContext context, CommandInfo command, IGuildUser user)
		{
			//Use the first alias since that's what group gets set as (could use any alias since GetCommand works for aliases too)
			//Doing a split since subcommands (in this bot's case) are simply easy to use options on a single command
			var cmd = Gets.GetCommand(context.GuildSettings, command.Aliases[0].Split(' ')[0]);
			if (!cmd.ValAsBoolean)
			{
				return false;
			}

			/* If user is set, use user setting
			 * Else if any roles are set, use the highest role setting
			 * Else if channel is set, use channel setting
			 */

			var userOverrides = context.GuildSettings.CommandsDisabledOnUser;
			var userOverride = userOverrides.FirstOrDefault(x => x.ID == context.User.Id && cmd.Name.CaseInsEquals(x.Name));
			if (userOverride != null)
			{
				return userOverride.Enabled;
			}

			var roleOverrides = context.GuildSettings.CommandsDisabledOnRole;
			var roleOverride = roleOverrides.Where(x => user.RoleIds.Contains(x.ID) && cmd.Name.CaseInsEquals(x.Name)).OrderBy(x => context.Guild.GetRole(x.ID).Position).LastOrDefault();
			if (roleOverride != null)
			{
				return roleOverride.Enabled;
			}

			var channelOverrides = context.GuildSettings.CommandsDisabledOnChannel;
			var channelOverride = channelOverrides.FirstOrDefault(x => x.ID == context.Channel.Id && cmd.Name.CaseInsEquals(x.Name));
			if (channelOverride != null)
			{
				return channelOverride.Enabled;
			}

			return true;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class DefaultEnabledAttribute : Attribute
	{
		public bool Enabled { get; }

		public DefaultEnabledAttribute(bool enabled)
		{
			Enabled = enabled;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class UsageAttribute : Attribute
	{
		public string Usage { get; }

		public UsageAttribute(string usage)
		{
			Usage = usage;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class DiscordObjectTargetAttribute : Attribute
	{
		public Target Target { get; }

		public DiscordObjectTargetAttribute(Target target)
		{
			Target = target;
		}
	}

	[AttributeUsage(AttributeTargets.Parameter)]
	public class VerifyObjectAttribute : ParameterPreconditionAttribute
	{
		private readonly bool mIfNullDrawFromContext;
		private readonly ObjectVerification[] mChecks;

		public VerifyObjectAttribute(bool ifNullDrawFromContext, params ObjectVerification[] checks)
		{
			mIfNullDrawFromContext = ifNullDrawFromContext;
			mChecks = checks;
		}

		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, Discord.Commands.ParameterInfo parameter, object value, IServiceProvider services)
		{
			//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
			if (value == null && !mIfNullDrawFromContext)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}

			return Task.FromResult(GetPreconditionResult(context, value));
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
		private PreconditionResult GetPreconditionResult(ICommandContext context, object value)
		{
			FailureReason failureReason = default(FailureReason);
			object obj = null;
			if (value is ITextChannel)
			{
				var returned = Channels.GetChannel(context.Guild, context.User as IGuildUser, mChecks, (value ?? context.Channel) as IGuildChannel);
				failureReason = returned.Reason;
				obj = returned.Object;
			}
			else if (value is IVoiceChannel)
			{
				var returned = Channels.GetChannel(context.Guild, context.User as IGuildUser, mChecks, (value ?? (context.User as IGuildUser).VoiceChannel) as IGuildChannel);
				failureReason = returned.Reason;
				obj = returned.Object;
			}
			else if (value is IGuildUser)
			{
				var returned = Users.GetGuildUser(context.Guild, context.User as IGuildUser, mChecks, (value ?? context.User) as IGuildUser);
				failureReason = returned.Reason;
				obj = returned.Object;
			}
			else if (value is IRole)
			{
				var returned = Roles.GetRole(context.Guild, context.User as IGuildUser, mChecks, value as IRole);
				failureReason = returned.Reason;
				obj = returned.Object;
			}

			if (failureReason != FailureReason.NotFailure)
			{
				return PreconditionResult.FromError(Actions.Formatting.FormatErrorString(context.Guild, failureReason, obj));
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
	
	[AttributeUsage(AttributeTargets.Parameter)]
	public class VerifyStringLengthAttribute : ParameterPreconditionAttribute
	{
		private readonly ReadOnlyDictionary<Target, Tuple<int, int, string>> mMinsAndMaxesAndErrors = new ReadOnlyDictionary<Target, Tuple<int, int, string>>(new Dictionary<Target, Tuple<int, int, string>>
		{
			{ Target.Guild, new Tuple<int, int, string>(Constants.MIN_GUILD_NAME_LENGTH, Constants.MAX_GUILD_NAME_LENGTH, "guild name") },
			{ Target.Channel, new Tuple<int, int, string>(Constants.MIN_CHANNEL_NAME_LENGTH, Constants.MAX_CHANNEL_NAME_LENGTH, "channel name") },
			{ Target.Role, new Tuple<int, int, string>(Constants.MIN_ROLE_NAME_LENGTH, Constants.MAX_ROLE_NAME_LENGTH, "role name") },
			{ Target.Name, new Tuple<int, int, string>(Constants.MIN_USERNAME_LENGTH, Constants.MAX_USERNAME_LENGTH, "username") },
			{ Target.Nickname, new Tuple<int, int, string>(Constants.MIN_NICKNAME_LENGTH, Constants.MAX_NICKNAME_LENGTH, "nickname") },
			{ Target.Game, new Tuple<int, int, string>(Constants.MIN_GAME_LENGTH, Constants.MAX_GAME_LENGTH, "game") },
			{ Target.Stream, new Tuple<int, int, string>(Constants.MIN_STREAM_LENGTH, Constants.MAX_STREAM_LENGTH, "stream name") },
			{ Target.Topic, new Tuple<int, int, string>(Constants.MIN_TOPIC_LENGTH, Constants.MAX_TOPIC_LENGTH, "channel topic") },
		});
		private int mMin;
		private int mMax;
		private string mTooShort;
		private string mTooLong;

		public VerifyStringLengthAttribute(Target target)
		{
			if (mMinsAndMaxesAndErrors.TryGetValue(target, out var minAndMaxAndError))
			{
				mMin = minAndMaxAndError.Item1;
				mMax = minAndMaxAndError.Item2;
				mTooShort = String.Format("A {0} must be at least `{1}` characters long.", minAndMaxAndError.Item3, mMin);
				mTooLong = String.Format("A {0} must be at most `{1}` characters long.", minAndMaxAndError.Item3, mMax);
			}
			else
			{
				throw new NotSupportedException("Inputted enum doesn't have a min and max or error output.");
			}
		}

		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, Discord.Commands.ParameterInfo parameter, object value, IServiceProvider services)
		{
			//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
			if (value == null)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}

			if (value.GetType() == typeof(string))
			{
				var str = value.ToString();
				if (str.Length < mMin)
				{
					return Task.FromResult(PreconditionResult.FromError(mTooShort));
				}
				else if (str.Length > mMax)
				{
					return Task.FromResult(PreconditionResult.FromError(mTooLong));
				}
				else
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
			}
			else
			{
				throw new NotSupportedException(String.Format("{0} only supports strings.", nameof(VerifyStringLengthAttribute)));
			}
		}
	}
	#endregion

	#region Typereaders
	public abstract class MyTypeReader : TypeReader
	{
		public bool TryParseMyCommandContext(ICommandContext context, out MyCommandContext myContext)
		{
			return (myContext = context as MyCommandContext) != null;
		}
	}

	public class IInviteTypeReader : MyTypeReader
	{
		public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			if (!TryParseMyCommandContext(context, out MyCommandContext myContext))
			{
				return TypeReaderResult.FromError(CommandError.Exception, "Invalid context provided.");
			}

			var invites = await myContext.Guild.GetInvitesAsync();
			var invite = invites.FirstOrDefault(x => x.Code.CaseInsEquals(input));
			return invite != null ? TypeReaderResult.FromSuccess(invite) : TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching invite.");
		}
	}

	public class IBanTypeReader : MyTypeReader
	{
		public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			if (!TryParseMyCommandContext(context, out MyCommandContext myContext))
			{
				return TypeReaderResult.FromError(CommandError.Exception, "Invalid context provided.");
			}

			IBan ban = null;
			var bans = await myContext.Guild.GetBansAsync();
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
					ban = bans.FirstOrDefault(x => x.User.DiscriminatorValue == discriminator && x.User.Username.CaseInsEquals(usernameAndDiscriminator[0]));
				}
			}

			if (ban == null)
			{
				var matchingUsernames = bans.Where(x => x.User.Username.CaseInsEquals(input));

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

	public class IEmoteTypeReader : MyTypeReader
	{
		public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			if (!TryParseMyCommandContext(context, out MyCommandContext myContext))
			{
				return Task.FromResult(TypeReaderResult.FromError(CommandError.Exception, "Invalid context provided."));
			}

			IEmote emote = null;
			if (Emote.TryParse(input, out Emote tempEmote))
			{
				emote = tempEmote;
			}
			else if (ulong.TryParse(input, out ulong emoteID))
			{
				emote = myContext.Guild.Emotes.FirstOrDefault(x => x.Id == emoteID);
			}

			if (emote == null)
			{
				var emotes = myContext.Guild.Emotes.Where(x => x.Name.CaseInsEquals(input));
				if (emotes.Count() == 1)
				{
					emote = emotes.First();
				}
				else if (emotes.Count() > 1)
				{
					return Task.FromResult(TypeReaderResult.FromError(CommandError.MultipleMatches, "Too many emotes have the provided name."));
				}
			}

			return emote != null ? Task.FromResult(TypeReaderResult.FromSuccess(emote)) : Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching emote."));
		}
	}

	public class ColorTypeReader : MyTypeReader
	{
		public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			if (!TryParseMyCommandContext(context, out MyCommandContext myContext))
			{
				return Task.FromResult(TypeReaderResult.FromError(CommandError.Exception, "Invalid context provided."));
			}

			Color? color = null;
			//By name
			if (Constants.COLORS.TryGetValue(input, out Color temp))
			{
				color = temp;
			}
			//By hex
			else if (uint.TryParse(input.TrimStart(new[] { '&', 'h', '#', '0', 'x' }), System.Globalization.NumberStyles.HexNumber, null, out uint hex))
			{
				color = new Color(hex);
			}
			//By RGB
			else if (input.Contains('/'))
			{
				var colorRGB = input.Split('/');
				if (colorRGB.Length == 3)
				{
					const byte MAX_VAL = 255;
					if (byte.TryParse(colorRGB[0], out byte r) && byte.TryParse(colorRGB[1], out byte g) && byte.TryParse(colorRGB[2], out byte b))
					{
						color = new Color(Math.Min(r, MAX_VAL), Math.Min(g, MAX_VAL), Math.Min(b, MAX_VAL));
					}
				}
			}

			return color != null ? Task.FromResult(TypeReaderResult.FromSuccess(color)) : Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching color."));
		}
	}

	public class BypassUserLimitTypeReader : MyTypeReader
	{
		public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			return Task.FromResult(TypeReaderResult.FromSuccess(Constants.BYPASS_STRING.CaseInsEquals(input)));
		}
	}

	public class BoolTypeReader : MyTypeReader
	{
		public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			if (bool.TryParse(input, out bool output))
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(output));
			}
			else
			{
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Failed to parse a bool."));
			}
		}
	}
	#endregion

	#region Saved Classes
	public class MyGuildSettings : IGuildSettings, INotifyPropertyChanged
	{
		//I wanted to go put all of the settings in a Dictionary<SettingOnGuild, object>
		//The problem with that was when deserializing I didn't know how to get JSON to deserialize to the correct types.
		[JsonProperty("BotUsers")]
		private List<BotImplementedPermissions> _BotUsers = new List<BotImplementedPermissions>();
		[JsonProperty("SelfAssignableGroups")]
		private List<SelfAssignableGroup> _SelfAssignableGroups = new List<SelfAssignableGroup>();
		[JsonProperty("Quotes")]
		private List<Quote> _Quotes = new List<Quote>();
		[JsonProperty("LogActions")]
		private List<LogAction> _LogActions = new List<LogAction>();
		[JsonProperty("IgnoredCommandChannels")]
		private List<ulong> _IgnoredCommandChannels = new List<ulong>();
		[JsonProperty("IgnoredLogChannels")]
		private List<ulong> _IgnoredLogChannels = new List<ulong>();
		[JsonProperty("ImageOnlyChannels")]
		private List<ulong> _ImageOnlyChannels = new List<ulong>();
		[JsonProperty("SanitaryChannels")]
		private List<ulong> _SanitaryChannels = new List<ulong>();
		[JsonProperty("BannedPhraseStrings")]
		private List<BannedPhrase> _BannedPhraseStrings = new List<BannedPhrase>();
		[JsonProperty("BannedPhraseRegex")]
		private List<BannedPhrase> _BannedPhraseRegex = new List<BannedPhrase>();
		[JsonProperty("BannedNamesForJoiningUsers")]
		private List<BannedPhrase> _BannedNamesForJoiningUsers = new List<BannedPhrase>();
		[JsonProperty("BannedPhrasePunishments")]
		private List<BannedPhrasePunishment> _BannedPhrasePunishments = new List<BannedPhrasePunishment>();
		[JsonProperty("CommandSwitches")]
		private List<CommandSwitch> _CommandSwitches = new List<CommandSwitch>();
		[JsonProperty("CommandsDisabledOnUser")]
		private List<CommandOverride> _CommandsDisabledOnUser = new List<CommandOverride>();
		[JsonProperty("CommandsDisabledOnRole")]
		private List<CommandOverride> _CommandsDisabledOnRole = new List<CommandOverride>();
		[JsonProperty("CommandsDisabledOnChannel")]
		private List<CommandOverride> _CommandsDisabledOnChannel = new List<CommandOverride>();
		[JsonProperty("ServerLog")]
		private DiscordObjectWithID<ITextChannel> _ServerLog = new DiscordObjectWithID<ITextChannel>(null);
		[JsonProperty("ModLog")]
		private DiscordObjectWithID<ITextChannel> _ModLog = new DiscordObjectWithID<ITextChannel>(null);
		[JsonProperty("ImageLog")]
		private DiscordObjectWithID<ITextChannel> _ImageLog = new DiscordObjectWithID<ITextChannel>(null);
		[JsonProperty("MuteRole")]
		private DiscordObjectWithID<IRole> _MuteRole = new DiscordObjectWithID<IRole>(null);
		[JsonProperty("MessageSpamPrevention")]
		private SpamPrevention _MessageSpamPrevention = null;
		[JsonProperty("LongMessageSpamPrevention")]
		private SpamPrevention _LongMessageSpamPrevention = null;
		[JsonProperty("LinkSpamPrevention")]
		private SpamPrevention _LinkSpamPrevention = null;
		[JsonProperty("ImageSpamPrevention")]
		private SpamPrevention _ImageSpamPrevention = null;
		[JsonProperty("MentionSpamPrevention")]
		private SpamPrevention _MentionSpamPrevention = null;
		[JsonProperty("RaidPrevention")]
		private RaidPrevention _RaidPrevention = null;
		[JsonProperty("RapidJoinPrevention")]
		private RaidPrevention _RapidJoinPrevention = null;
		[JsonProperty("WelcomeMessage")]
		private GuildNotification _WelcomeMessage = null;
		[JsonProperty("GoodbyeMessage")]
		private GuildNotification _GoodbyeMessage = null;
		[JsonProperty("ListedInvite")]
		private ListedInvite _ListedInvite = null;
		[JsonProperty("Prefix")]
		private string _Prefix = null;
		[JsonProperty("VerboseErrors")]
		private bool _VerboseErrors = true;

		[JsonIgnore]
		public IReadOnlyList<BotImplementedPermissions> BotUsers
		{
			get => _BotUsers.AsReadOnly() ?? (_BotUsers = new List<BotImplementedPermissions>()).AsReadOnly();
			set
			{
				_BotUsers = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<SelfAssignableGroup> SelfAssignableGroups
		{
			get => _SelfAssignableGroups.AsReadOnly() ?? (_SelfAssignableGroups = new List<SelfAssignableGroup>()).AsReadOnly();
			set
			{
				_SelfAssignableGroups = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<Quote> Quotes
		{
			get => _Quotes.AsReadOnly() ?? (_Quotes = new List<Quote>()).AsReadOnly();
			set
			{
				_Quotes = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<LogAction> LogActions
		{
			get => _LogActions.AsReadOnly() ?? (_LogActions = new List<LogAction>()).AsReadOnly();
			set
			{
				_LogActions = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<ulong> IgnoredCommandChannels
		{
			get => _IgnoredCommandChannels.AsReadOnly() ?? (_IgnoredCommandChannels = new List<ulong>()).AsReadOnly();
			set
			{
				_IgnoredCommandChannels = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<ulong> IgnoredLogChannels
		{
			get => _IgnoredLogChannels.AsReadOnly() ?? (_IgnoredLogChannels = new List<ulong>()).AsReadOnly();
			set
			{
				_IgnoredLogChannels = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<ulong> ImageOnlyChannels
		{
			get => _ImageOnlyChannels.AsReadOnly() ?? (_ImageOnlyChannels = new List<ulong>()).AsReadOnly();
			set
			{
				_ImageOnlyChannels = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<ulong> SanitaryChannels
		{
			get => _SanitaryChannels.AsReadOnly() ?? (_SanitaryChannels = new List<ulong>()).AsReadOnly();
			set
			{
				_SanitaryChannels = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<BannedPhrase> BannedPhraseStrings
		{
			get => _BannedPhraseStrings.AsReadOnly() ?? (_BannedPhraseStrings = new List<BannedPhrase>()).AsReadOnly();
			set
			{
				_BannedPhraseStrings = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<BannedPhrase> BannedPhraseRegex
		{
			get => _BannedPhraseRegex.AsReadOnly() ?? (_BannedPhraseRegex = new List<BannedPhrase>()).AsReadOnly();
			set
			{
				_BannedPhraseRegex = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<BannedPhrase> BannedNamesForJoiningUsers
		{
			get => _BannedNamesForJoiningUsers.AsReadOnly() ?? (_BannedNamesForJoiningUsers = new List<BannedPhrase>()).AsReadOnly();
			set
			{
				_BannedNamesForJoiningUsers = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<BannedPhrasePunishment> BannedPhrasePunishments
		{
			get => _BannedPhrasePunishments.AsReadOnly() ?? (_BannedPhrasePunishments = new List<BannedPhrasePunishment>()).AsReadOnly();
			set
			{
				_BannedPhrasePunishments = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<CommandSwitch> CommandSwitches
		{
			get => _CommandSwitches.AsReadOnly() ?? (_CommandSwitches = new List<CommandSwitch>()).AsReadOnly();
			set
			{
				_CommandSwitches = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<CommandOverride> CommandsDisabledOnUser
		{
			get => _CommandsDisabledOnUser.AsReadOnly() ?? (_CommandsDisabledOnUser = new List<CommandOverride>()).AsReadOnly();
			set
			{
				_CommandsDisabledOnUser = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<CommandOverride> CommandsDisabledOnRole
		{
			get => _CommandsDisabledOnRole.AsReadOnly() ?? (_CommandsDisabledOnRole = new List<CommandOverride>()).AsReadOnly();
			set
			{
				_CommandsDisabledOnRole = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<CommandOverride> CommandsDisabledOnChannel
		{
			get => _CommandsDisabledOnChannel.AsReadOnly() ?? (_CommandsDisabledOnChannel = new List<CommandOverride>()).AsReadOnly();
			set
			{
				_CommandsDisabledOnChannel = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public ITextChannel ServerLog
		{
			get => (_ServerLog ?? (_ServerLog = new DiscordObjectWithID<ITextChannel>(null))).Object;
			set
			{
				_ServerLog = new DiscordObjectWithID<ITextChannel>(value);
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public ITextChannel ModLog
		{
			get => (_ModLog ?? (_ModLog = new DiscordObjectWithID<ITextChannel>(null))).Object;
			set
			{
				_ModLog = new DiscordObjectWithID<ITextChannel>(value);
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public ITextChannel ImageLog
		{
			get => (_ImageLog ?? (_ImageLog = new DiscordObjectWithID<ITextChannel>(null))).Object;
			set
			{
				_ImageLog = new DiscordObjectWithID<ITextChannel>(value);
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IRole MuteRole
		{
			get => (_MuteRole ?? (_MuteRole = new DiscordObjectWithID<IRole>(null))).Object;
			set
			{
				_MuteRole = new DiscordObjectWithID<IRole>(value);
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyDictionary<SpamType, SpamPrevention> SpamPreventionDictionary
		{
			get => new ReadOnlyDictionary<SpamType, SpamPrevention>(new Dictionary<SpamType, SpamPrevention>
			{
				{ SpamType.Message, _MessageSpamPrevention },
				{ SpamType.LongMessage, _LongMessageSpamPrevention },
				{ SpamType.Link, _LinkSpamPrevention },
				{ SpamType.Image, _ImageSpamPrevention },
				{ SpamType.Mention, _MentionSpamPrevention },
			});
			set
			{
				foreach (var kvp in value)
				{
					switch (kvp.Key)
					{
						case SpamType.Message:
						{
							_MessageSpamPrevention = kvp.Value;
							break;
						}
						case SpamType.LongMessage:
						{
							_LongMessageSpamPrevention = kvp.Value;
							break;
						}
						case SpamType.Link:
						{
							_LinkSpamPrevention = kvp.Value;
							break;
						}
						case SpamType.Image:
						{
							_ImageSpamPrevention = kvp.Value;
							break;
						}
						case SpamType.Mention:
						{
							_MentionSpamPrevention = kvp.Value;
							break;
						}
					}
				}
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyDictionary<RaidType, RaidPrevention> RaidPreventionDictionary
		{
			get => new ReadOnlyDictionary<RaidType, RaidPrevention>(new Dictionary<RaidType, RaidPrevention>
			{
				{ RaidType.Regular, _RaidPrevention },
				{ RaidType.RapidJoins, _RapidJoinPrevention },
			});
			set
			{
				foreach (var kvp in value)
				{
					switch (kvp.Key)
					{
						case RaidType.Regular:
						{
							_RaidPrevention = kvp.Value;
							break;
						}
						case RaidType.RapidJoins:
						{
							_RapidJoinPrevention = kvp.Value;
							break;
						}
					}
				}
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public GuildNotification WelcomeMessage
		{
			get => _WelcomeMessage;
			set
			{
				_WelcomeMessage = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public GuildNotification GoodbyeMessage
		{
			get => _GoodbyeMessage;
			set
			{
				_GoodbyeMessage = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public ListedInvite ListedInvite
		{
			get => _ListedInvite;
			set
			{
				_ListedInvite = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public string Prefix
		{
			get => _Prefix;
			set
			{
				_Prefix = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public bool VerboseErrors
		{
			get => _VerboseErrors;
			set
			{
				_VerboseErrors = value;
				OnPropertyChanged();
			}
		}

		[JsonIgnore]
		public List<BannedPhraseUser> BannedPhraseUsers { get; private set; } = new List<BannedPhraseUser>();
		[JsonIgnore]
		public List<SpamPreventionUser> SpamPreventionUsers { get; private set; } = new List<SpamPreventionUser>();
		[JsonIgnore]
		public List<SlowmodeChannel> SlowmodeChannels { get; private set; } = new List<SlowmodeChannel>();
		[JsonIgnore]
		public List<BotInvite> Invites { get; private set; } = new List<BotInvite>();
		[JsonIgnore]
		public List<string> EvaluatedRegex { get; private set; } = new List<string>();
		[JsonIgnore]
		public SlowmodeGuild SlowmodeGuild { get; private set; } = null;
		[JsonIgnore]
		public MessageDeletion MessageDeletion { get; private set; } = new MessageDeletion();
		[JsonIgnore]
		public IGuild Guild { get; private set; } = null;
		[JsonIgnore]
		public bool Loaded { get; private set; } = false;

		public event PropertyChangedEventHandler PropertyChanged;
		public MyGuildSettings()
		{
			PropertyChanged += SaveInfo;
		}

		private void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		public void SaveInfo(object sender, PropertyChangedEventArgs e)
		{
			//ConsoleActions.WriteLine(String.Format("Successfully saved: {0}", e.PropertyName));
			if (Guild != null)
			{
				SavingAndLoading.OverWriteFile(Gets.GetServerFilePath(Guild.Id, Constants.GUILD_SETTINGS_LOCATION), SavingAndLoading.Serialize(this));
			}
		}
		public void PostDeserialize(IGuild guild)
		{
			Guild = guild;
			var tempGuild = guild as SocketGuild;

			if (_ModLog != null)
			{
				_ModLog.PostDeserialize(tempGuild);
			}
			if (_ServerLog != null)
			{
				_ServerLog.PostDeserialize(tempGuild);
			}
			if (_ImageLog != null)
			{
				_ImageLog.PostDeserialize(tempGuild);
			}
			if (_MuteRole != null)
			{
				_MuteRole.PostDeserialize(tempGuild);
			}

			if (_ListedInvite != null)
			{
				_ListedInvite.PostDeserialize(tempGuild);
				Variables.InviteList.ThreadSafeAdd(_ListedInvite);
			}
			if (_WelcomeMessage != null)
			{
				_WelcomeMessage.PostDeserialize(tempGuild);
			}
			if (_GoodbyeMessage != null)
			{
				_GoodbyeMessage.PostDeserialize(tempGuild);
			}

			foreach (var bannedPhrasePunishment in _BannedPhrasePunishments)
			{
				bannedPhrasePunishment.PostDeserialize(tempGuild);
			}
			foreach (var group in _SelfAssignableGroups)
			{
				group.Roles.ForEach(x => x.PostDeserialize(tempGuild));
				group.Roles.RemoveAll(x => x == null || x.Role == null);
			}

			Loaded = true;
		}
	}

	public class MyBotSettings : IBotSettings, INotifyPropertyChanged
	{
		[JsonProperty("TrustedUsers")]
		private List<ulong> _TrustedUsers = new List<ulong>();
		[JsonProperty("UsersUnableToDMOwner")]
		private List<ulong> _UsersUnableToDMOwner = new List<ulong>();
		[JsonProperty("UsersIgnoredFromCommands")]
		private List<ulong> _UsersIgnoredFromCommands = new List<ulong>();
		[JsonProperty("BotOwnerID")]
		private ulong _BotOwnerID = 0;
		[JsonProperty("ShardCount")]
		private uint _ShardCount = 1;
		[JsonProperty("MessageCacheCount")]
		private uint _MessageCacheCount = 1000;
		[JsonProperty("MaxUserGatherCount")]
		private uint _MaxUserGatherCount = 100;
		[JsonProperty("MaxMessageGatherSize")]
		private uint _MaxMessageGatherSize = 500000;
		[JsonProperty("Prefix")]
		private string _Prefix = Constants.BOT_PREFIX;
		[JsonProperty("Game")]
		private string _Game = String.Format("type \"{0}help\" for help.", Constants.BOT_PREFIX);
		[JsonProperty("Stream")]
		private string _Stream = null;
		[JsonProperty("AlwaysDownloadUsers")]
		private bool _AlwaysDownloadUsers = true;
		[JsonProperty("LogLevel")]
		private LogSeverity _LogLevel = LogSeverity.Warning;

		[JsonIgnore]
		public IReadOnlyList<ulong> TrustedUsers
		{
			get => _TrustedUsers.AsReadOnly() ?? (_TrustedUsers = new List<ulong>()).AsReadOnly();
			set
			{
				_TrustedUsers = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<ulong> UsersUnableToDMOwner
		{
			get => _UsersUnableToDMOwner.AsReadOnly() ?? (_UsersUnableToDMOwner = new List<ulong>()).AsReadOnly();
			set
			{
				_UsersUnableToDMOwner = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<ulong> UsersIgnoredFromCommands
		{
			get => _UsersIgnoredFromCommands.AsReadOnly() ?? (_UsersIgnoredFromCommands = new List<ulong>()).AsReadOnly();
			set
			{
				_UsersIgnoredFromCommands = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public ulong BotOwnerID
		{
			get => _BotOwnerID;
			set
			{
				_BotOwnerID = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public uint ShardCount
		{
			get => _ShardCount > 1 ? _ShardCount : (_ShardCount = 1);
			set
			{
				_ShardCount = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public uint MessageCacheCount
		{
			get => _MessageCacheCount;
			set
			{
				_MessageCacheCount = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public uint MaxUserGatherCount
		{
			get => _MaxUserGatherCount;
			set
			{
				_MaxUserGatherCount = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public uint MaxMessageGatherSize
		{
			get => _MaxMessageGatherSize;
			set
			{
				_MaxMessageGatherSize = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public string Prefix
		{
			get => _Prefix ?? (_Prefix = Constants.BOT_PREFIX);
			set
			{
				_Prefix = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public string Game
		{
			get => _Game;
			set
			{
				_Game = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public string Stream
		{
			get => _Stream;
			set
			{
				_Stream = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public bool AlwaysDownloadUsers
		{
			get => _AlwaysDownloadUsers;
			set
			{
				_AlwaysDownloadUsers = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public LogSeverity LogLevel
		{
			get => _LogLevel;
			set
			{
				_LogLevel = value;
				OnPropertyChanged();
			}
		}

		[JsonIgnore]
		public bool Windows { get; private set; }
		[JsonIgnore]
		public bool Console { get; private set; }
		[JsonIgnore]
		public bool FirstInstanceOfBotStartingUpWithCurrentKey { get; private set; }
		[JsonIgnore]
		public bool GotPath { get; private set; }
		[JsonIgnore]
		public bool GotKey { get; private set; }
		[JsonIgnore]
		public bool Loaded { get; private set; }
		[JsonIgnore]
		public bool Pause { get; private set; }
		[JsonIgnore]
		public DateTime StartupTime { get; } = DateTime.UtcNow;

		public event PropertyChangedEventHandler PropertyChanged;
		public MyBotSettings()
		{
			PropertyChanged += SaveInfo;
		}

		//TODO: put a wait like on message deletion 
		private void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		private void SaveInfo(object sender, PropertyChangedEventArgs e)
		{
			ConsoleActions.WriteLine(String.Format("Successfully saved: {0}", e.PropertyName));
			SavingAndLoading.OverWriteFile(Gets.GetBaseBotDirectory(Constants.BOT_SETTINGS_LOCATION), SavingAndLoading.Serialize(this));
		}
		public void PostDeserialize(bool windows, bool console, bool firstInstance)
		{
			Windows = windows;
			Console = console;
			FirstInstanceOfBotStartingUpWithCurrentKey = firstInstance;
		}

		public void TogglePause()
		{
			Pause = !Pause;
		}
		public void SetLoaded()
		{
			Loaded = true;
		}
		public void SetGotKey()
		{
			GotKey = true;
		}
		public void SetGotPath()
		{
			GotPath = true;
		}
	}

	public class CommandOverride : ISetting
	{
		[JsonProperty]
		public string Name { get; }
		[JsonProperty]
		public ulong ID { get; }
		[JsonProperty]
		public bool Enabled { get; private set; }

		public CommandOverride(string name, ulong id, bool enabled)
		{
			Name = name;
			ID = id;
			Enabled = enabled;
		}

		public void ToggleEnabled()
		{
			Enabled = !Enabled;
		}

		public string SettingToString()
		{
			return String.Format("**Command:** `{0}`\n**ID:** `{1}`\n**Enabled:** `{2}`", Name, ID, Enabled);
		}
		public string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class CommandSwitch : ISetting
	{
		[JsonProperty]
		public string Name { get; }
		[JsonIgnore]
		public string[] Aliases { get; }
		[JsonProperty]
		public bool Value { get; private set; }
		[JsonIgnore]
		public string ValAsString { get { return Value ? "ON" : "OFF"; } }
		[JsonIgnore]
		public int ValAsInteger { get { return Value ? 1 : -1; } }
		[JsonIgnore]
		public bool ValAsBoolean { get { return Value; } }
		[JsonProperty]
		public CommandCategory Category { get; }
		[JsonIgnore]
		public string CategoryName { get { return Category.EnumName(); } }
		[JsonIgnore]
		public int CategoryValue { get { return (int)Category; } }
		[JsonIgnore]
		private HelpEntry mHelpEntry;

		public CommandSwitch(string name, bool value)
		{
			mHelpEntry = Constants.HELP_ENTRIES.FirstOrDefault(x => x.Name.Equals(name));
			if (mHelpEntry == null)
				return;

			Name = name;
			Value = value;
			Category = mHelpEntry.Category;
			Aliases = mHelpEntry.Aliases;
		}

		public void ToggleEnabled()
		{
			Value = !Value;
		}

		public string SettingToString()
		{
			return String.Format("`{0}` `{1}`", ValAsString.PadRight(3), Name);
		}
		public string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class BannedPhrase : ISetting
	{
		[JsonProperty]
		public string Phrase { get; }
		[JsonProperty]
		public PunishmentType Punishment { get; private set; }

		public BannedPhrase(string phrase, PunishmentType punishment)
		{
			Phrase = phrase;
			ChangePunishment(punishment);
		}

		public void ChangePunishment(PunishmentType punishment)
		{
			switch (punishment)
			{
				case PunishmentType.RoleMute:
				case PunishmentType.Kick:
				case PunishmentType.KickThenBan:
				case PunishmentType.Ban:
				{
					Punishment = punishment;
					return;
				}
				default:
				{
					Punishment = PunishmentType.Nothing;
					return;
				}
			}
		}

		public string SettingToString()
		{
			return String.Format("`{0}` `{1}`", Punishment.EnumName().Substring(0, 1), Phrase);
		}
		public string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class BannedPhrasePunishment : ISetting
	{
		[JsonProperty]
		public int NumberOfRemoves { get; }
		[JsonProperty]
		public PunishmentType Punishment { get; }
		[JsonProperty]
		public ulong RoleID { get; }
		[JsonProperty]
		public ulong GuildID { get; }
		[JsonProperty]
		public uint PunishmentTime { get; }
		[JsonIgnore]
		public IRole Role { get; private set; }

		[JsonConstructor]
		public BannedPhrasePunishment(int number, PunishmentType punishment, ulong guildID = 0, ulong roleID = 0, uint punishmentTime = 0)
		{
			NumberOfRemoves = number;
			Punishment = punishment;
			RoleID = roleID;
			GuildID = guildID;
			PunishmentTime = punishmentTime;
		}
		public BannedPhrasePunishment(int number, PunishmentType punishment, ulong guildID = 0, ulong roleID = 0, uint punishmentTime = 0, IRole role = null) : this(number, punishment, guildID, roleID, punishmentTime)
		{
			Role = role;
		}

		public void PostDeserialize(SocketGuild guild)
		{
			Role = guild.GetRole(RoleID);
		}

		public string SettingToString()
		{
			return String.Format("`{0}.` `{1}`{2}",
				NumberOfRemoves.ToString("00"),
				RoleID == 0 ? Punishment.EnumName() : RoleID.ToString(),
				PunishmentTime == 0 ? "" : " `" + PunishmentTime + " minutes`");
		}
		public string SettingToString(SocketGuild guild)
		{
			return String.Format("`{0}.` `{1}`{2}",
				NumberOfRemoves.ToString("00"),
				RoleID == 0 ? Punishment.EnumName() : guild.GetRole(RoleID).Name,
				PunishmentTime == 0 ? "" : " `" + PunishmentTime + " minutes`");
		}
	}

	public class SelfAssignableGroup : ISetting
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
			Roles.Add(role);
		}
		public void AddRoles(IEnumerable<SelfAssignableRole> roles)
		{
			Roles.AddRange(roles);
		}
		public void RemoveRoles(IEnumerable<ulong> roleIDs)
		{
			Roles.RemoveAll(x => roleIDs.Contains(x.RoleID));
		}

		public string SettingToString()
		{
			return String.Format("`Group: {0}`\n{1}", Group, String.Join("\n", Roles.Select(x => x.SettingToString())));
		}
		public string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class SelfAssignableRole : ISetting
	{
		[JsonProperty]
		public ulong RoleID { get; }
		[JsonIgnore]
		public IRole Role { get; private set; }

		[JsonConstructor]
		public SelfAssignableRole(ulong roleID)
		{
			RoleID = roleID;
		}
		public SelfAssignableRole(IRole role)
		{
			RoleID = role.Id;
			Role = role;
		}

		public void PostDeserialize(SocketGuild guild)
		{
			Role = guild.GetRole(RoleID);
		}

		public string SettingToString()
		{
			return String.Format("**Role:** `{0}`", Role.FormatRole());
		}
		public string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class BotImplementedPermissions : ISetting
	{
		[JsonProperty]
		public ulong UserID { get; }
		[JsonProperty]
		public ulong Permissions { get; private set; }

		public BotImplementedPermissions(ulong userID, ulong permissions)
		{
			UserID = userID;
			Permissions = permissions;
		}

		public void AddPermission(ulong bit)
		{
			Permissions |= bit;
		}
		public void RemovePermission(ulong bit)
		{
			Permissions &= ~bit;
		}

		public string SettingToString()
		{
			return String.Format("**User:** `{0}`\n**Permissions:** `{1}`", UserID, Permissions);
		}
		public string SettingToString(SocketGuild guild)
		{
			return String.Format("**User:** `{0}`\n**Permissions:** `{1}`", guild.GetUser(UserID).FormatUser(), Permissions);
		}
	}

	public class GuildNotification : ISetting
	{
		[JsonProperty]
		public string Content { get; }
		[JsonProperty]
		public string Title { get; }
		[JsonProperty]
		public string Description { get; }
		[JsonProperty]
		public string ThumbURL { get; }
		[JsonProperty]
		public ulong ChannelID { get; }
		[JsonIgnore]
		public EmbedBuilder Embed { get; }
		[JsonIgnore]
		public ITextChannel Channel { get; private set; }

		[JsonConstructor]
		public GuildNotification(string content, string title, string description, string thumbURL, ulong channelID)
		{
			Content = content;
			Title = title;
			Description = description;
			ThumbURL = thumbURL;
			ChannelID = channelID;
			if (!(String.IsNullOrWhiteSpace(title) && String.IsNullOrWhiteSpace(description) && String.IsNullOrWhiteSpace(thumbURL)))
			{
				Embed = Embeds.MakeNewEmbed(title, description, null, null, null, thumbURL);
			}
		}
		public GuildNotification(string content, string title, string description, string thumbURL, ITextChannel channel) : this(content, title, description, thumbURL, channel.Id)
		{
			Channel = channel;
		}

		public void ChangeChannel(ITextChannel channel)
		{
			Channel = channel;
		}
		public void PostDeserialize(SocketGuild guild)
		{
			Channel = guild.GetTextChannel(ChannelID);
		}

		public string SettingToString()
		{
			return String.Format("**Channel:** `{0}`\n**Content:** `{1}`\n**Title:** `{2}`\n**Description:** `{3}`\n**Thumbnail:** `{4}`",
				Channel.FormatChannel(),
				Content,
				Title,
				Description,
				ThumbURL);
		}
		public string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class ListedInvite : ISetting
	{
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

		[JsonConstructor]
		public ListedInvite(string code, string[] keywords)
		{
			LastBumped = DateTime.UtcNow;
			Code = code;
			URL = String.Concat("https://www.discord.gg/", Code);
			Keywords = keywords ?? new string[0];
		}
		public ListedInvite(SocketGuild guild, string code, string[] keywords) : this(code, keywords)
		{
			Guild = guild;
			HasGlobalEmotes = Guild.HasGlobalEmotes();
		}

		public void UpdateCode(string code)
		{
			Code = code;
			URL = String.Concat("https://www.discord.gg/", Code);
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
		public void PostDeserialize(SocketGuild guild)
		{
			Guild = guild;
			HasGlobalEmotes = Guild.HasGlobalEmotes();
		}

		public string SettingToString()
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
		public string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class Quote : ISetting, INameAndText
	{
		[JsonProperty]
		public string Name { get; }
		[JsonProperty]
		public string Text { get; }

		public Quote(string name, string text)
		{
			Name = name;
			Text = text;
		}

		public string SettingToString()
		{
			return String.Format("`{0}`", Name);
		}
		public string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class DiscordObjectWithID<T> : ISetting where T : ISnowflakeEntity
	{
		[JsonIgnore]
		private ReadOnlyDictionary<Type, Func<SocketGuild, ulong, object>> inits = new ReadOnlyDictionary<Type, Func<SocketGuild, ulong, object>>(new Dictionary<Type, Func<SocketGuild, ulong, object>>
		{
			{ typeof(IRole), (SocketGuild guild, ulong ID) => { return guild.GetRole(ID); } },
			{ typeof(ITextChannel), (SocketGuild guild, ulong ID) => { return guild.GetTextChannel(ID); } },
		});
		[JsonProperty]
		public ulong ID { get; }
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
				Object = (T)method(guild, ID);
			}
		}

		public string SettingToString()
		{
			if (Object != null)
			{
				return Actions.Formatting.FormatObject(Object);
			}
			else
			{
				return null;
			}
		}
		public string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class SpamPrevention : ISetting
	{
		[JsonProperty]
		public PunishmentType PunishmentType { get; }
		[JsonProperty]
		public int TimeInterval { get; }
		[JsonProperty]
		public int RequiredSpamInstances { get; }
		[JsonProperty]
		public int RequiredSpamPerMessage { get; }
		[JsonProperty]
		public int VotesForKick { get; }
		[JsonProperty]
		public bool Enabled { get; private set; }

		public SpamPrevention(PunishmentType punishmentType, int timeInterval, int requiredSpamInstances, int requiredSpamPerMessage, int votesForKick)
		{
			PunishmentType = punishmentType;
			TimeInterval = timeInterval;
			RequiredSpamInstances = requiredSpamInstances;
			RequiredSpamPerMessage = requiredSpamPerMessage;
			VotesForKick = votesForKick;
			Enabled = true;
		}

		public void ToggleEnabled()
		{
			Enabled = !Enabled;
		}

		public string SettingToString()
		{
			return String.Format("**Enabled:** `{0}`\n**Spam Instances:** `{1}`\n**Spam Amount/Time Interval:** `{2}`\n**Votes Needed For Kick:** `{3}`\n**Punishment:** `{4}`",
				Enabled,
				RequiredSpamInstances,
				RequiredSpamPerMessage,
				VotesForKick,
				PunishmentType.EnumName());
		}
		public string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}

	public class RaidPrevention : ISetting
	{
		[JsonProperty]
		public PunishmentType PunishmentType { get; }
		[JsonProperty]
		public int TimeInterval { get; }
		[JsonProperty]
		public int RequiredCount { get; }
		[JsonProperty]
		public bool Enabled { get; private set; }
		[JsonIgnore]
		public List<BasicTimeInterface> TimeList { get; }

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
			return TimeList.GetCountOfItemsInTimeFrame(TimeInterval);
		}
		public void Add(DateTime time)
		{
			TimeList.ThreadSafeAdd(new BasicTimeInterface(time));
		}
		public void Remove(DateTime time)
		{
			TimeList.ThreadSafeRemoveAll(x => x.GetTime().Equals(time));
		}
		public void ToggleEnabled()
		{
			Enabled = !Enabled;
		}
		public void Reset()
		{
			TimeList.Clear();
		}
		public async Task RaidPreventionPunishment(IGuildSettings guildSettings, IGuildUser user)
		{
			//TODO: make this not 0
			await Punishments.AutomaticPunishments(guildSettings, user, PunishmentType, false, 0);
		}

		public string SettingToString()
		{
			return String.Format("**Enabled:** `{0}`\n**Users:** `{1}`\n**Time Interval:** `{2}`\n**Punishment:** `{3}`",
				Enabled,
				RequiredCount,
				TimeInterval,
				PunishmentType.EnumName());
		}
		public string SettingToString(SocketGuild guild)
		{
			return SettingToString();
		}
	}
	#endregion

	#region Non-saved Classes
	[CommandRequirements]
	public class MyModuleBase : ModuleBase<MyCommandContext>
	{
	}

	public class MyCommandContext : CommandContext
	{
		public IBotSettings GlobalInfo { get; }
		public IGuildSettings GuildSettings { get; }
		public ILogModule Logging { get; }

		public MyCommandContext(IBotSettings globalInfo, IGuildSettings guildSettings, ILogModule logging, IDiscordClient client, IUserMessage msg) : base(client, msg)
		{
			GlobalInfo = globalInfo;
			GuildSettings = guildSettings;
			Logging = logging;
		}
	}

	public class GuildSettingsModule : IGuildSettingsModule
	{
		private Dictionary<ulong, IGuildSettings> mGuildSettings = new Dictionary<ulong, IGuildSettings>();
		private Type mGuildSettingsType;

		public GuildSettingsModule(Type guildSettingsType)
		{
			if (guildSettingsType == null || !guildSettingsType.GetInterfaces().Contains(typeof(IGuildSettings)))
			{
				throw new ArgumentException("Invalid type for guild settings provided.");
			}

			mGuildSettingsType = guildSettingsType;
		}

		public async Task AddGuild(IGuild guild)
		{
			if (!mGuildSettings.ContainsKey(guild.Id))
			{
				mGuildSettings.Add(guild.Id, await CreateGuildSettings(mGuildSettingsType, guild));
			}
		}
		public Task RemoveGuild(IGuild guild)
		{
			if (mGuildSettings.ContainsKey(guild.Id))
			{
				mGuildSettings.Remove(guild.Id);
			}
			return Task.FromResult(0);
		}
		public IGuildSettings GetSettings(IGuild guild)
		{
			return mGuildSettings[guild.Id];
		}
		public IEnumerable<IGuildSettings> GetAllSettings()
		{
			return mGuildSettings.Values;
		}
		public bool TryGetSettings(IGuild guild, out IGuildSettings settings)
		{
			return mGuildSettings.TryGetValue(guild.Id, out settings);
		}

		private async Task<IGuildSettings> CreateGuildSettings(Type guildSettingsType, IGuild guild)
		{
			if (!mGuildSettings.TryGetValue(guild.Id, out IGuildSettings guildSettings))
			{
				var path = Gets.GetServerFilePath(guild.Id, Constants.GUILD_SETTINGS_LOCATION);
				if (File.Exists(path))
				{
					try
					{
						using (var reader = new StreamReader(path))
						{
							guildSettings = (IGuildSettings)JsonConvert.DeserializeObject(reader.ReadToEnd(), guildSettingsType);
						}
						ConsoleActions.WriteLine(String.Format("The guild information for {0} has successfully been loaded.", guild.FormatGuild()));
					}
					catch (Exception e)
					{
						ConsoleActions.ExceptionToConsole(e);
					}
				}
				else
				{
					ConsoleActions.WriteLine(String.Format("The guild information file for {0} could not be found; using default.", guild.FormatGuild()));
				}
				guildSettings = guildSettings ?? (IGuildSettings)Activator.CreateInstance(guildSettingsType);

				guildSettings.CommandsDisabledOnUser = guildSettings.CommandsDisabledOnUser.Where(x => !String.IsNullOrWhiteSpace(x.Name)).ToList().AsReadOnly();
				guildSettings.CommandsDisabledOnRole = guildSettings.CommandsDisabledOnRole.Where(x => !String.IsNullOrWhiteSpace(x.Name)).ToList().AsReadOnly();
				guildSettings.CommandsDisabledOnChannel = guildSettings.CommandsDisabledOnChannel.Where(x => !String.IsNullOrWhiteSpace(x.Name)).ToList().AsReadOnly();

				var tempCommandSwitches = guildSettings.CommandSwitches.Where(x => !String.IsNullOrWhiteSpace(x.Name)).ToList();
				foreach (var cmd in Constants.HELP_ENTRIES.Where(x => !guildSettings.CommandSwitches.Select(y => y.Name).CaseInsContains(x.Name)))
				{
					tempCommandSwitches.Add(new CommandSwitch(cmd.Name, cmd.DefaultEnabled));
				}
				guildSettings.CommandSwitches = tempCommandSwitches;

				guildSettings.Invites.AddRange((await Invites.GetInvites(guild)).Select(x => new BotInvite(x.GuildId, x.Code, x.Uses)));

				if (guildSettings is MyGuildSettings)
				{
					(guildSettings as MyGuildSettings).PostDeserialize(guild);
				}
			}

			return guildSettings;
		}
	}

	public class HelpEntry : INameAndText
	{
		public string Name { get; }
		public string[] Aliases { get; }
		public string Usage { get; }
		public string BasePerm { get; }
		public string Text { get; }
		public CommandCategory Category { get; }
		public bool DefaultEnabled { get; }
		private const string placeHolderStr = "N/A";

		public HelpEntry(string name, string[] aliases, string usage, string basePerm, string text, CommandCategory category, bool defaultEnabled)
		{
			Name = String.IsNullOrWhiteSpace(name) ? placeHolderStr : name;
			Aliases = aliases ?? new[] { placeHolderStr };
			Usage = String.IsNullOrWhiteSpace(usage) ? placeHolderStr : Constants.BOT_PREFIX + usage;
			BasePerm = String.IsNullOrWhiteSpace(basePerm) ? placeHolderStr : basePerm;
			Text = String.IsNullOrWhiteSpace(text) ? placeHolderStr : text;
			Category = category;
			DefaultEnabled = defaultEnabled;
		}

		public override string ToString()
		{
			var aliasStr = String.Format("**Aliases:** {0}", String.Join(", ", Aliases));
			var usageStr = String.Format("**Usage:** {0}", Usage);
			var permStr = String.Format("\n**Base Permission(s):**\n{0}", BasePerm);
			var descStr = String.Format("\n**Description:**\n{0}", Text);
			return String.Join("\n", new[] { aliasStr, usageStr, permStr, descStr });
		}
	}

	public class BotInvite
	{
		public ulong GuildID { get; }
		public string Code { get; }
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
		public IGuildUser User { get; }
		public int BaseMessages { get; }
		public int Interval { get; }
		public int CurrentMessagesLeft { get; private set; }
		public DateTime Time { get; private set; }

		public SlowmodeUser(IGuildUser user, int baseMessages, int interval)
		{
			User = user;
			BaseMessages = baseMessages;
			Interval = interval;
			CurrentMessagesLeft = baseMessages;
		}

		public void LowerMessagesLeft()
		{
			--CurrentMessagesLeft;
		}
		public void ResetMessagesLeft()
		{
			CurrentMessagesLeft = BaseMessages;
		}
		public void SetNewTime()
		{
			Time = DateTime.UtcNow.AddSeconds(Interval);
		}
		public DateTime GetTime()
		{
			return Time;
		}
	}

	public class BannedPhraseUser
	{
		public IGuildUser User { get; }
		public int MessagesForRole { get; private set; }
		public int MessagesForKick { get; private set; }
		public int MessagesForBan { get; private set; }

		public BannedPhraseUser(IGuildUser user)
		{
			User = user;
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
		public int BaseMessages { get; }
		public int Interval { get; }
		public List<SlowmodeUser> Users { get; }

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
		public ulong ChannelID { get; }
		public int BaseMessages { get; }
		public int Interval { get; }
		public List<SlowmodeUser> Users { get; }

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
	}

	public class SpamPreventionUser
	{
		public IGuildUser User { get; }
		public List<ulong> UsersWhoHaveAlreadyVoted { get; } = new List<ulong>();
		public Dictionary<SpamType, List<BasicTimeInterface>> SpamLists { get; } = new Dictionary<SpamType, List<BasicTimeInterface>>();

		public int VotesRequired { get; private set; } = int.MaxValue;
		public bool PotentialPunishment { get; private set; } = false;
		public bool AlreadyKicked { get; private set; } = false;
		public PunishmentType Punishment { get; private set; } = PunishmentType.Nothing;

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
		}
		public void ChangeVotesRequired(int newVotesRequired)
		{
			VotesRequired = Math.Min(newVotesRequired, VotesRequired);
		}
		public void ChangePunishmentType(PunishmentType newPunishment)
		{
			if (Constants.PUNISHMENT_SEVERITY[newPunishment] > Constants.PUNISHMENT_SEVERITY[Punishment])
			{
				Punishment = newPunishment;
			}
		}
		public void EnablePunishable()
		{
			PotentialPunishment = true;
		}
		public void ResetSpamUser()
		{
			//Don't reset already kicked since KickThenBan requires it
			UsersWhoHaveAlreadyVoted.Clear();
			foreach (var spamList in SpamLists.Values)
			{
				spamList.Clear();
			}

			VotesRequired = int.MaxValue;
			PotentialPunishment = false;
			Punishment = PunishmentType.Nothing;
		}
		public bool CheckIfAllowedToPunish(SpamPrevention spamPrev, SpamType spamType)
		{
			return SpamLists[spamType].GetCountOfItemsInTimeFrame(spamPrev.TimeInterval) >= spamPrev.RequiredSpamInstances;
		}
		public async Task SpamPreventionPunishment(IGuildSettings guildSettings)
		{
			//TODO: make this not 0
			await Punishments.AutomaticPunishments(guildSettings, User, Punishment, AlreadyKicked, 0);
		}
	}
	#endregion

	#region Punishments
	public class Punishment
	{
		public IGuild Guild { get; }
		public ulong UserID { get; }
		public PunishmentType PunishmentType { get; }

		public Punishment(IGuild guild, ulong userID, PunishmentType punishmentType)
		{
			Guild = guild;
			UserID = userID;
			PunishmentType = punishmentType;
		}
		public Punishment(IGuild guild, IUser user, PunishmentType punishmentType) : this(guild, user.Id, punishmentType)
		{
		}
	}

	public class RemovablePunishment : Punishment, ITimeInterface
	{
		private DateTime mTime;

		public RemovablePunishment(IGuild guild, ulong userID, PunishmentType punishmentType, uint minutes) : base(guild, userID, punishmentType)
		{
			mTime = DateTime.UtcNow.AddMinutes(minutes);
		}
		public RemovablePunishment(IGuild guild, IUser user, PunishmentType punishmentType, uint minutes) : this(guild, user.Id, punishmentType, minutes)
		{
		}

		public DateTime GetTime()
		{
			return mTime;
		}
	}

	public class RemovableRoleMute : RemovablePunishment
	{
		public IRole Role { get; }

		public RemovableRoleMute(IGuild guild, ulong userID, uint minutes, IRole role) : base(guild, userID, PunishmentType.RoleMute, minutes)
		{
		}
		public RemovableRoleMute(IGuild guild, IUser user, uint minutes, IRole role) : this(guild, user.Id, minutes, role)
		{
		}
	}

	public class RemovableVoiceMute : RemovablePunishment
	{
		public RemovableVoiceMute(IGuild guild, ulong userID, uint minutes) : base(guild, userID, PunishmentType.VoiceMute, minutes)
		{
		}
		public RemovableVoiceMute(IGuild guild, IUser user, uint minutes) : this(guild, user.Id, minutes)
		{
		}
	}

	public class RemovableDeafen : RemovablePunishment
	{
		public RemovableDeafen(IGuild guild, ulong userID, uint minutes) : base(guild, userID, PunishmentType.Deafen, minutes)
		{
		}
		public RemovableDeafen(IGuild guild, IUser user, uint minutes) : this(guild, user.Id, minutes)
		{
		}
	}

	public class RemovableBan : RemovablePunishment
	{
		public RemovableBan(IGuild guild, ulong userID, uint minutes) : base(guild, userID, PunishmentType.Ban, minutes)
		{
		}
		public RemovableBan(IGuild guild, IUser user, uint minutes) : this(guild, user.Id, minutes)
		{
		}
	}

	public class RemovableMessage : ITimeInterface
	{
		public IEnumerable<IMessage> Messages { get; }
		public IMessageChannel Channel { get; }
		private DateTime mTime;

		public RemovableMessage(IEnumerable<IMessage> messages, uint seconds)
		{
			Messages = messages;
			Channel = messages.FirstOrDefault().Channel;
			mTime = DateTime.UtcNow.AddSeconds(seconds);
		}
		public RemovableMessage(IMessage message, uint seconds) : this(new[] { message }, seconds)
		{
		}

		public DateTime GetTime()
		{
			return mTime;
		}
	}
	#endregion

	#region Structs
	public struct BotGuildPermission : IPermission
	{
		public string Name { get; }
		public ulong Bit { get; }

		public BotGuildPermission(string name, int position)
		{
			Name = name;
			Bit = (1U << position);
		}
	}

	public struct BotChannelPermission : IPermission
	{
		public string Name { get; }
		public ulong Bit { get; }
		public bool General { get; }
		public bool Text { get; }
		public bool Voice { get; }

		public BotChannelPermission(string name, int position, bool gen = false, bool text = false, bool voice = false)
		{
			Name = name;
			Bit = (1U << position);
			General = gen;
			Text = text;
			Voice = voice;
		}
	}

	public struct ActiveCloseWord<T> : ITimeInterface where T : INameAndText
	{
		public ulong UserID { get; }
		public List<CloseWord<T>> List { get; }
		private DateTime mTime;

		public ActiveCloseWord(ulong userID, IEnumerable<CloseWord<T>> list)
		{
			UserID = userID;
			List = list.ToList();
			mTime = DateTime.UtcNow.AddMilliseconds(Constants.SECONDS_ACTIVE_CLOSE);
		}

		public DateTime GetTime()
		{
			return mTime;
		}
	}

	public struct CloseWord<T> where T : INameAndText
	{
		public T Word { get; }
		public int Closeness { get; }

		public CloseWord(T word, int closeness)
		{
			Word = word;
			Closeness = closeness;
		}
	}

	public struct ReturnedObject<T>
	{
		public T Object { get; }
		public FailureReason Reason { get; }

		public ReturnedObject(T obj, FailureReason reason)
		{
			Object = obj;
			Reason = reason;
		}
	}

	public struct ReturnedArguments
	{
		public List<string> Arguments { get; }
		public int ArgCount { get; }
		public Dictionary<string, string> SpecifiedArguments { get; }
		public List<ulong> MentionedUsers { get; }
		public List<ulong> MentionedRoles { get; }
		public List<ulong> MentionedChannels { get; }
		public FailureReason Reason { get; }

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

	public struct GuildFileInformation
	{
		public ulong ID { get; }
		public string Name { get; }
		public int MemberCount { get; }

		public GuildFileInformation(ulong id, string name, int memberCount)
		{
			ID = id;
			Name = name;
			MemberCount = memberCount;
		}
	}

	public struct FileInformation
	{
		public FileType FileType { get; }
		public string FileLocation { get; }

		public FileInformation(FileType fileType, string fileLocation)
		{
			FileType = fileType;
			FileLocation = fileLocation;
		}
	}

	public struct VerifiedLoggingAction
	{
		public IGuild Guild { get; }
		public IGuildSettings GuildSettings { get; }
		public ITextChannel LoggingChannel { get; }

		public VerifiedLoggingAction(IGuild guild, IGuildSettings guildSettings, ITextChannel loggingChannel)
		{
			Guild = guild;
			GuildSettings = guildSettings;
			LoggingChannel = loggingChannel;
		}
	}

	public struct LoggedCommand
	{
		public string Guild { get; }
		public string Channel { get; }
		public string User { get; }
		public string Time { get; }
		public string Text { get; }

		public LoggedCommand(ICommandContext context)
		{
			Guild = context.Guild.FormatGuild();
			Channel = context.Channel.FormatChannel();
			User = context.User.FormatUser();
			Time = Actions.Formatting.FormatDateTime(context.Message.CreatedAt);
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

	public struct CriticalInformation
	{
		public bool Windows { get; }
		public bool Console { get; }
		public bool FirstInstance { get; }

		public CriticalInformation(bool windows, bool console, bool firstInstance)
		{
			Windows = windows;
			Console = console;
			FirstInstance = firstInstance;
		}
	}
	#endregion

	#region Interfaces
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

		BaseLog BotLog { get; }
		BaseLog ServerLog { get; }
		BaseLog ModLog { get; }

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

	public interface ISetting
	{
		string SettingToString();
		string SettingToString(SocketGuild guild);
	}

	public interface IBotSettings
	{
		IReadOnlyList<ulong> TrustedUsers { get; set; }
		IReadOnlyList<ulong> UsersUnableToDMOwner { get; set; }
		IReadOnlyList<ulong> UsersIgnoredFromCommands { get; set; }
		ulong BotOwnerID { get; set; }
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
		IReadOnlyDictionary<SpamType, SpamPrevention> SpamPreventionDictionary { get; set; }
		IReadOnlyDictionary<RaidType, RaidPrevention> RaidPreventionDictionary { get; set; }
		//TODO: give all of my own custom classes being saved in settings a custom interface
		GuildNotification WelcomeMessage { get; set; }
		GuildNotification GoodbyeMessage { get; set; }
		ListedInvite ListedInvite { get; set; }
		string Prefix { get; set; }
		bool VerboseErrors { get; set; }

		List<BannedPhraseUser> BannedPhraseUsers { get; }
		List<SpamPreventionUser> SpamPreventionUsers { get; }
		List<SlowmodeChannel> SlowmodeChannels { get; }
		List<BotInvite> Invites { get; }
		List<string> EvaluatedRegex { get; }
		SlowmodeGuild SlowmodeGuild { get; }
		MessageDeletion MessageDeletion { get; }
		IGuild Guild { get; }
		bool Loaded { get; }
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
	public enum ChannelType : uint
	{
		Text							= (1U << 0),
		Voice							= (1U << 1),
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
		CanBeMovedFromChannel			= (1U << 0),

		[DiscordObjectTarget(Target.Channel)]
		IsVoice							= (1U << 0),
		[DiscordObjectTarget(Target.Channel)]
		IsText							= (1U << 1),
		[DiscordObjectTarget(Target.Channel)]
		CanBeReordered					= (1U << 2),
		[DiscordObjectTarget(Target.Channel)]
		CanModifyPermissions			= (1U << 3),
		[DiscordObjectTarget(Target.Channel)]
		CanBeManaged					= (1U << 4),
		[DiscordObjectTarget(Target.Channel)]
		CanMoveUsers					= (1U << 5),
		[DiscordObjectTarget(Target.Channel)]
		CanDeleteMessages				= (1U << 6),
		[DiscordObjectTarget(Target.Channel)]
		CanBeRead						= (1U << 7),
		[DiscordObjectTarget(Target.Channel)]
		CanCreateInstantInvite			= (1U << 8),
		[DiscordObjectTarget(Target.Channel)]
		IsDefault						= (1U << 9),

		[DiscordObjectTarget(Target.Role)]
		IsEveryone						= (1U << 0),
		[DiscordObjectTarget(Target.Role)]
		IsManaged						= (1U << 1),
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
		Nothing							= (1U << 0),
		Kick							= (1U << 1),
		Ban								= (1U << 2),
		Deafen							= (1U << 3),
		VoiceMute						= (1U << 4),
		KickThenBan						= (1U << 5),
		RoleMute						= (1U << 6),
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
	#endregion
}