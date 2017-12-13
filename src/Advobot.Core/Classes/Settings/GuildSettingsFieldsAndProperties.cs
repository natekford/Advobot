using Advobot.Core.Classes.BannedPhrases;
using Advobot.Core.Classes.Permissions;
using Advobot.Core.Classes.Rules;
using Advobot.Core.Classes.SpamPrevention;
using Advobot.Core.Classes.UserInformation;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Advobot.Core.Classes.Settings
{
	/// <summary>
	/// Holds settings for a guild. Settings are only saved by calling <see cref="SaveSettings"/>.
	/// </summary>
	public partial class GuildSettings : IGuildSettings
	{
		[JsonProperty("WelcomeMessage")]
		private GuildNotification _WelcomeMessage;
		[JsonProperty("GoodbyeMessage")]
		private GuildNotification _GoodbyeMessage;
		[JsonProperty("ListedInvite")]
		private ListedInvite _ListedInvite;
		[JsonProperty("Slowmode")]
		private Slowmode _Slowmode;
		[JsonProperty("Rules")]
		private RuleHolder _Rules;
		[JsonProperty("Prefix")]
		private string _Prefix;
		[JsonProperty("NonVerboseErrors")]
		private bool _NonVerboseErrors;
		[JsonProperty("ServerLog")]
		private ulong _ServerLogId;
		[JsonProperty("ModLog")]
		private ulong _ModLogId;
		[JsonProperty("ImageLog")]
		private ulong _ImageLogId;
		[JsonProperty("MuteRole")]
		private ulong _MuteRoleId;
		[JsonIgnore]
		private ITextChannel _ServerLog;
		[JsonIgnore]
		private ITextChannel _ModLog;
		[JsonIgnore]
		private ITextChannel _ImageLog;
		[JsonIgnore]
		private IRole _MuteRole;
		[JsonProperty("SpamPrevention")]
		private Dictionary<SpamType, SpamPreventionInfo> _SpamPrevention;
		[JsonProperty("RaidPrevention")]
		private Dictionary<RaidType, RaidPreventionInfo> _RaidPrevention;
		[JsonProperty("PersistentRoles")]
		private List<PersistentRole> _PersistentRoles;
		[JsonProperty("BotUsers")]
		private List<BotImplementedPermissions> _BotUsers;
		[JsonProperty("SelfAssignableGroups")]
		private List<SelfAssignableGroup> _SelfAssignableGroups;
		[JsonProperty("Quotes")]
		private List<Quote> _Quotes;
		[JsonProperty("LogActions")]
		private List<LogAction> _LogActions;
		[JsonProperty("IgnoredCommandChannels")]
		private List<ulong> _IgnoredCommandChannels;
		[JsonProperty("IgnoredLogChannels")]
		private List<ulong> _IgnoredLogChannels;
		[JsonProperty("ImageOnlyChannels")]
		private List<ulong> _ImageOnlyChannels;
		[JsonProperty("BannedPhraseStrings")]
		private List<BannedPhrase> _BannedPhraseStrings;
		[JsonProperty("BannedPhraseRegex")]
		private List<BannedPhrase> _BannedPhraseRegex;
		[JsonProperty("BannedPhraseNames")]
		private List<BannedPhrase> _BannedPhraseNames;
		[JsonProperty("BannedPhrasePunishments")]
		private List<BannedPhrasePunishment> _BannedPhrasePunishments;
		[JsonProperty("CommandsDisabledOnUser")]
		private List<CommandOverride> _CommandsDisabledOnUser;
		[JsonProperty("CommandsDisabledOnRole")]
		private List<CommandOverride> _CommandsDisabledOnRole;
		[JsonProperty("CommandsDisabledOnChannel")]
		private List<CommandOverride> _CommandsDisabledOnChannel;
		[JsonProperty("CommandSwitches")]
		private List<CommandSwitch> _CommandSwitches;

		[JsonIgnore]
		public GuildNotification WelcomeMessage
		{
			get => _WelcomeMessage;
			set => _WelcomeMessage = value;
		}
		[JsonIgnore]
		public GuildNotification GoodbyeMessage
		{
			get => _GoodbyeMessage;
			set => _GoodbyeMessage = value;
		}
		[JsonIgnore]
		public ListedInvite ListedInvite
		{
			get => _ListedInvite;
			set => _ListedInvite = value;
		}
		[JsonIgnore]
		public Slowmode Slowmode
		{
			get => _Slowmode;
			set => _Slowmode = value;
		}
		[JsonIgnore]
		public RuleHolder Rules
		{
			get => _Rules ?? (_Rules = new RuleHolder());
			set => _Rules = value;
		}
		[JsonIgnore]
		public string Prefix
		{
			get => _Prefix;
			set => _Prefix = value;
		}
		[JsonIgnore]
		public bool NonVerboseErrors
		{
			get => _NonVerboseErrors;
			set => _NonVerboseErrors = value;
		}
		[JsonIgnore]
		public ITextChannel ServerLog
		{
			get => _ServerLog ?? (_ServerLog = Guild.GetTextChannel(_ServerLogId));
			set
			{
				this._ServerLogId = value?.Id ?? 0;
				this._ServerLog = value;
			}
		}
		[JsonIgnore]
		public ITextChannel ModLog
		{
			get => _ModLog ?? (_ModLog = Guild.GetTextChannel(_ModLogId));
			set
			{
				this._ModLogId = value?.Id ?? 0;
				this._ModLog = value;
			}
		}
		[JsonIgnore]
		public ITextChannel ImageLog
		{
			get => _ImageLog ?? (_ImageLog = Guild.GetTextChannel(_ImageLogId));
			set
			{
				this._ImageLogId = value?.Id ?? 0;
				this._ImageLog = value;
			}
		}
		[JsonIgnore]
		public IRole MuteRole
		{
			get => _MuteRole ?? (_MuteRole = Guild.GetRole(_MuteRoleId));
			set
			{
				this._MuteRoleId = value?.Id ?? 0;
				this._MuteRole = value;
			}
		}
		[JsonIgnore]
		public Dictionary<SpamType, SpamPreventionInfo> SpamPreventionDictionary
		{
			get => _SpamPrevention ?? (_SpamPrevention = new Dictionary<SpamType, SpamPreventionInfo>
			{
				{ SpamType.Message, null },
				{ SpamType.LongMessage, null },
				{ SpamType.Link, null },
				{ SpamType.Image, null },
				{ SpamType.Mention, null },
			});
			set => _SpamPrevention = value;
		}
		[JsonIgnore]
		public Dictionary<RaidType, RaidPreventionInfo> RaidPreventionDictionary
		{
			get => _RaidPrevention ?? (_RaidPrevention = new Dictionary<RaidType, RaidPreventionInfo>
			{
				{ RaidType.Regular, null },
				{ RaidType.RapidJoins, null },
			});
			set => _RaidPrevention = value;
		}
		[JsonIgnore]
		public List<PersistentRole> PersistentRoles
		{
			get => _PersistentRoles ?? (_PersistentRoles = new List<PersistentRole>());
			set => _PersistentRoles = value;
		}
		[JsonIgnore]
		public List<BotImplementedPermissions> BotUsers
		{
			get => _BotUsers ?? (_BotUsers = new List<BotImplementedPermissions>());
			set => _BotUsers = value;
		}
		[JsonIgnore]
		public List<SelfAssignableGroup> SelfAssignableGroups
		{
			get => _SelfAssignableGroups ?? (_SelfAssignableGroups = new List<SelfAssignableGroup>());
			set => _SelfAssignableGroups = value;
		}
		[JsonIgnore]
		public List<Quote> Quotes
		{
			get => _Quotes ?? (_Quotes = new List<Quote>());
			set => _Quotes = value;
		}
		[JsonIgnore]
		public List<LogAction> LogActions
		{
			get => _LogActions ?? (_LogActions = new List<LogAction>());
			set => _LogActions = value;
		}
		[JsonIgnore]
		public List<ulong> IgnoredCommandChannels
		{
			get => _IgnoredCommandChannels ?? (_IgnoredCommandChannels = new List<ulong>());
			set => _IgnoredCommandChannels = value;
		}
		[JsonIgnore]
		public List<ulong> IgnoredLogChannels
		{
			get => _IgnoredLogChannels ?? (_IgnoredLogChannels = new List<ulong>());
			set => _IgnoredLogChannels = value;
		}
		[JsonIgnore]
		public List<ulong> ImageOnlyChannels
		{
			get => _ImageOnlyChannels ?? (_ImageOnlyChannels = new List<ulong>());
			set => _ImageOnlyChannels = value;
		}
		[JsonIgnore]
		public List<BannedPhrase> BannedPhraseStrings
		{
			get => _BannedPhraseStrings ?? (_BannedPhraseStrings = new List<BannedPhrase>());
			set => _BannedPhraseStrings = value;
		}
		[JsonIgnore]
		public List<BannedPhrase> BannedPhraseRegex
		{
			get => _BannedPhraseRegex ?? (_BannedPhraseRegex = new List<BannedPhrase>());
			set => _BannedPhraseRegex = value;
		}
		[JsonIgnore]
		public List<BannedPhrase> BannedPhraseNames
		{
			get => _BannedPhraseNames ?? (_BannedPhraseNames = new List<BannedPhrase>());
			set => _BannedPhraseNames = value;
		}
		[JsonIgnore]
		public List<BannedPhrasePunishment> BannedPhrasePunishments
		{
			get => _BannedPhrasePunishments ?? (_BannedPhrasePunishments = new List<BannedPhrasePunishment>());
			set => _BannedPhrasePunishments = value;
		}
		[JsonIgnore]
		public List<CommandOverride> CommandsDisabledOnUser
		{
			get => _CommandsDisabledOnUser ?? (_CommandsDisabledOnUser = new List<CommandOverride>());
			set => _CommandsDisabledOnUser = value;
		}
		[JsonIgnore]
		public List<CommandOverride> CommandsDisabledOnRole
		{
			get => _CommandsDisabledOnRole ?? (_CommandsDisabledOnRole = new List<CommandOverride>());
			set => _CommandsDisabledOnRole = value;
		}
		[JsonIgnore]
		public List<CommandOverride> CommandsDisabledOnChannel
		{
			get => _CommandsDisabledOnChannel ?? (_CommandsDisabledOnChannel = new List<CommandOverride>());
			set => _CommandsDisabledOnChannel = value;
		}
		[JsonIgnore]
		public List<CommandSwitch> CommandSwitches
		{
			get => _CommandSwitches ?? (_CommandSwitches = new List<CommandSwitch>());
			set => _CommandSwitches = value;
		}

		[JsonIgnore]
		public List<BannedPhraseUserInformation> BannedPhraseUsers { get; } = new List<BannedPhraseUserInformation>();
		[JsonIgnore]
		public List<CachedInvite> Invites { get; } = new List<CachedInvite>();
		[JsonIgnore]
		public List<string> EvaluatedRegex { get; } = new List<string>();
		[JsonIgnore]
		public MessageDeletion MessageDeletion { get; } = new MessageDeletion();
		[JsonIgnore]
		public SocketGuild Guild { get; private set; } = null;
		[JsonIgnore]
		public bool Loaded { get; private set; } = false;
	}
}
