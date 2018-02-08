using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.Rules;
using Advobot.Core.Classes.Settings;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Holds settings for a guild. Settings are only saved by calling <see cref="SaveSettings"/>.
	/// </summary>
	public class GuildSettings : SettingsBase, IGuildSettings
	{
		[JsonProperty("WelcomeMessage"), Setting(null)]
		private GuildNotification _WelcomeMessage = null;
		[JsonProperty("GoodbyeMessage"), Setting(null)]
		private GuildNotification _GoodbyeMessage = null;
		[JsonProperty("ListedInvite"), Setting(null)]
		private ListedInvite _ListedInvite = null;
		[JsonProperty("Slowmode"), Setting(null)]
		private Slowmode _Slowmode = null;
		[JsonProperty("ServerLog"), Setting(0)]
		private ulong _ServerLogId = 0;
		[JsonProperty("ModLog"), Setting(0)]
		private ulong _ModLogId = 0;
		[JsonProperty("ImageLog"), Setting(0)]
		private ulong _ImageLogId = 0;
		[JsonProperty("MuteRole"), Setting(0)]
		private ulong _MuteRoleId = 0;
		[JsonProperty("Prefix"), Setting(null)]
		private string _Prefix = null;
		[JsonProperty("NonVerboseErrors"), Setting(false)]
		private bool _NonVerboseErrors = false;
		[JsonProperty("SpamPrevention"), Setting(NonCompileTimeDefaultValue.ClearDictionaryValues)]
		private Dictionary<SpamType, SpamPreventionInfo> _SpamPrevention = new Dictionary<SpamType, SpamPreventionInfo>
		{
			{ SpamType.Message, null },
			{ SpamType.LongMessage, null },
			{ SpamType.Link, null },
			{ SpamType.Image, null },
			{ SpamType.Mention, null }
		};
		[JsonProperty("RaidPrevention"), Setting(NonCompileTimeDefaultValue.ClearDictionaryValues)]
		private Dictionary<RaidType, RaidPreventionInfo> _RaidPrevention = new Dictionary<RaidType, RaidPreventionInfo>
		{
			{ RaidType.Regular, null },
			{ RaidType.RapidJoins, null }
		};
		[JsonProperty("PersistentRoles"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<PersistentRole> _PersistentRoles = new List<PersistentRole>();
		[JsonProperty("BotUsers"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<BotImplementedPermissions> _BotUsers = new List<BotImplementedPermissions>();
		[JsonProperty("SelfAssignableGroups"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<SelfAssignableRoles> _SelfAssignableGroups = new List<SelfAssignableRoles>();
		[JsonProperty("Quotes"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<Quote> _Quotes = new List<Quote>();
		[JsonProperty("LogActions"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<LogAction> _LogActions = new List<LogAction>();
		[JsonProperty("IgnoredCommandChannels"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<ulong> _IgnoredCommandChannels = new List<ulong>();
		[JsonProperty("IgnoredLogChannels"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<ulong> _IgnoredLogChannels = new List<ulong>();
		[JsonProperty("ImageOnlyChannels"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<ulong> _ImageOnlyChannels = new List<ulong>();
		[JsonProperty("BannedPhraseStrings"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<BannedPhrase> _BannedPhraseStrings = new List<BannedPhrase>();
		[JsonProperty("BannedPhraseRegex"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<BannedPhrase> _BannedPhraseRegex = new List<BannedPhrase>();
		[JsonProperty("BannedPhraseNames"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<BannedPhrase> _BannedPhraseNames = new List<BannedPhrase>();
		[JsonProperty("BannedPhrasePunishments"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<BannedPhrasePunishment> _BannedPhrasePunishments = new List<BannedPhrasePunishment>();
		[JsonProperty("Rules"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private RuleHolder _Rules = new RuleHolder();
		[JsonProperty("CommandSettings"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private CommandSettings _CommandSettings = new CommandSettings();

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
		public RuleHolder Rules => _Rules;
		[JsonIgnore]
		public string Prefix
		{
			get => _Prefix;
			set => _Prefix = value;
		}
		[JsonIgnore]
		public ulong ServerLogId
		{
			get => _ServerLogId;
			set => _ServerLogId = value;
		}
		[JsonIgnore]
		public ulong ModLogId
		{
			get => _ModLogId;
			set => _ModLogId = value;
		}
		[JsonIgnore]
		public ulong ImageLogId
		{
			get => _ImageLogId;
			set => _ImageLogId = value;
		}
		[JsonIgnore]
		public ulong MuteRoleId
		{
			get => _MuteRoleId;
			set => _MuteRoleId = value;
		}
		[JsonIgnore]
		public bool NonVerboseErrors
		{
			get => _NonVerboseErrors;
			set => _NonVerboseErrors = value;
		}
		[JsonIgnore]
		public Dictionary<SpamType, SpamPreventionInfo> SpamPreventionDictionary => _SpamPrevention;
		[JsonIgnore]
		public Dictionary<RaidType, RaidPreventionInfo> RaidPreventionDictionary => _RaidPrevention;
		[JsonIgnore]
		public List<PersistentRole> PersistentRoles => _PersistentRoles;
		[JsonIgnore]
		public List<BotImplementedPermissions> BotUsers => _BotUsers;
		[JsonIgnore]
		public List<SelfAssignableRoles> SelfAssignableGroups => _SelfAssignableGroups;
		[JsonIgnore]
		public List<Quote> Quotes => _Quotes;
		[JsonIgnore]
		public List<LogAction> LogActions => _LogActions;
		[JsonIgnore]
		public List<ulong> IgnoredCommandChannels => _IgnoredCommandChannels;
		[JsonIgnore]
		public List<ulong> IgnoredLogChannels => _IgnoredLogChannels;
		[JsonIgnore]
		public List<ulong> ImageOnlyChannels => _ImageOnlyChannels;
		[JsonIgnore]
		public List<BannedPhrase> BannedPhraseStrings => _BannedPhraseStrings;
		[JsonIgnore]
		public List<BannedPhrase> BannedPhraseRegex => _BannedPhraseRegex;
		[JsonIgnore]
		public List<BannedPhrase> BannedPhraseNames => _BannedPhraseNames;
		[JsonIgnore]
		public List<BannedPhrasePunishment> BannedPhrasePunishments => _BannedPhrasePunishments;
		[JsonIgnore]
		public CommandSettings CommandSettings => _CommandSettings;

		[JsonIgnore]
		public List<CachedInvite> Invites { get; } = new List<CachedInvite>();
		[JsonIgnore]
		public List<string> EvaluatedRegex { get; } = new List<string>();
		[JsonIgnore]
		public MessageDeletion MessageDeletion { get; } = new MessageDeletion();
		[JsonIgnore]
		public override FileInfo FileLocation => IOUtils.GetServerDirectoryFile(GuildId, Constants.GUILD_SETTINGS_LOC);
		[JsonIgnore]
		public ulong GuildId { get; private set; }
		[JsonIgnore]
		public bool Loaded { get; private set; }

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			if (!(context.Context is SocketGuild guild))
			{
				throw new InvalidOperationException("The additional streaming context must be a socketguild when deserializing.");
			}

			GuildId = guild.Id;
			Task.Run(async () =>
			{
				Invites.AddRange((await DiscordUtils.GetInvitesAsync(guild).CAF()).Select(x => new CachedInvite(x.Code, x.Uses)));
			});
			foreach (var group in _SelfAssignableGroups ?? Enumerable.Empty<SelfAssignableRoles>())
			{
				group.PostDeserialize(guild);
			}
			_ListedInvite?.PostDeserialize(guild);

			Loaded = true;
		}
	}
}
