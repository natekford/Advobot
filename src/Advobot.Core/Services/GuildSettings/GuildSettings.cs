using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Settings;
using Advobot.Classes.UserInformation;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Services.GuildSettings
{
	/// <summary>
	/// Holds settings for a guild.
	/// </summary>
	internal sealed class GuildSettings : SettingsBase, IGuildSettings
	{
		private static readonly ImmutableArray<LogAction> _DefaultLogActions = new List<LogAction>
		{
			LogAction.UserJoined,
			LogAction.UserLeft,
			LogAction.MessageReceived,
			LogAction.MessageUpdated,
			LogAction.MessageDeleted,
		}.ToImmutableArray();

		/// <inheritdoc />
		[JsonProperty("WelcomeMessage")]
		public GuildNotification WelcomeMessage { get; set; }
		/// <inheritdoc />
		[JsonProperty("GoodbyeMessage")]
		public GuildNotification GoodbyeMessage { get; set; }
		/// <inheritdoc />
		[JsonProperty("Slowmode")]
		public Slowmode Slowmode { get; set; }
		/// <inheritdoc />
		[JsonProperty("Rules")]
		public RuleHolder Rules { get; private set; } = new RuleHolder();
		/// <inheritdoc />
		[JsonProperty("Prefix")]
		public string Prefix { get; set; }
		/// <inheritdoc />
		[JsonProperty("ServerLog")]
		public ulong ServerLogId { get; set; }
		/// <inheritdoc />
		[JsonProperty("ModLog")]
		public ulong ModLogId { get; set; }
		/// <inheritdoc />
		[JsonProperty("ImageLog")]
		public ulong ImageLogId { get; set; }
		/// <inheritdoc />
		[JsonProperty("MuteRole")]
		public ulong MuteRoleId { get; set; }
		/// <inheritdoc />
		[JsonProperty("NonVerboseErrors")]
		public bool NonVerboseErrors { get; set; }
		/// <inheritdoc />
		[JsonProperty("SpamPrevention")]
		public IDictionary<SpamType, SpamPreventionInfo> SpamPreventionDictionary { get; private set; } = new Dictionary<SpamType, SpamPreventionInfo>
		{
			{ SpamType.Message, null },
			{ SpamType.LongMessage, null },
			{ SpamType.Link, null },
			{ SpamType.Image, null },
			{ SpamType.Mention, null }
		};
		/// <inheritdoc />
		[JsonProperty("RaidPrevention")]
		public IDictionary<RaidType, RaidPreventionInfo> RaidPreventionDictionary { get; private set; } = new Dictionary<RaidType, RaidPreventionInfo>
		{
			{ RaidType.Regular, null },
			{ RaidType.RapidJoins, null }
		};
		/// <inheritdoc />
		[JsonProperty("PersistentRoles")]
		public IList<PersistentRole> PersistentRoles { get; private set; } = new List<PersistentRole>();
		/// <inheritdoc />
		[JsonProperty("BotUsers")]
		public IList<BotUser> BotUsers { get; private set; } = new List<BotUser>();
		/// <inheritdoc />
		[JsonProperty("SelfAssignableGroups")]
		public IList<SelfAssignableRoles> SelfAssignableGroups { get; private set; } = new List<SelfAssignableRoles>();
		/// <inheritdoc />
		[JsonProperty("Quotes")]
		public IList<Quote> Quotes { get; private set; } = new List<Quote>();
		/// <inheritdoc />
		[JsonProperty("LogActions")]
		public IList<LogAction> LogActions { get; private set; } = new List<LogAction>(_DefaultLogActions);
		/// <inheritdoc />
		[JsonProperty("IgnoredCommandChannels")]
		public IList<ulong> IgnoredCommandChannels { get; private set; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("IgnoredLogChannels")]
		public IList<ulong> IgnoredLogChannels { get; private set; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("IgnoredXpChannels")]
		public IList<ulong> IgnoredXpChannels { get; private set; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("ImageOnlyChannels")]
		public IList<ulong> ImageOnlyChannels { get; private set; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("BannedPhraseStrings")]
		public IList<BannedPhrase> BannedPhraseStrings { get; private set; } = new List<BannedPhrase>();
		/// <inheritdoc />
		[JsonProperty("BannedPhraseRegex")]
		public IList<BannedPhrase> BannedPhraseRegex { get; private set; } = new List<BannedPhrase>();
		/// <inheritdoc />
		[JsonProperty("BannedPhraseNames")]
		public IList<BannedPhrase> BannedPhraseNames { get; private set; } = new List<BannedPhrase>();
		/// <inheritdoc />
		[JsonProperty("BannedPhrasePunishments")]
		public IList<BannedPhrasePunishment> BannedPhrasePunishments { get; private set; } = new List<BannedPhrasePunishment>();
		/// <inheritdoc />
		[JsonProperty("CommandSettings")]
		public CommandSettings CommandSettings { get; private set; } = new CommandSettings();
		/// <inheritdoc />
		[JsonIgnore]
		public IList<SpamPreventionUserInfo> SpamPreventionUsers { get; private set; } = new List<SpamPreventionUserInfo>();
		/// <inheritdoc />
		[JsonIgnore]
		public IList<SlowmodeUserInfo> SlowmodeUsers { get; private set; } = new List<SlowmodeUserInfo>();
		/// <inheritdoc />
		[JsonIgnore]
		public IList<BannedPhraseUserInfo> BannedPhraseUsers { get; private set; } = new List<BannedPhraseUserInfo>();
		/// <inheritdoc />
		[JsonIgnore]
		public IList<CachedInvite> Invites { get; private set; } = new List<CachedInvite>();
		/// <inheritdoc />
		[JsonIgnore]
		public IList<string> EvaluatedRegex { get; private set; } = new List<string>();
		/// <inheritdoc />
		[JsonIgnore]
		public MessageDeletion MessageDeletion { get; private set; } = new MessageDeletion();
		/// <inheritdoc />
		[JsonIgnore]
		public ulong GuildId { get; private set; }
		/// <inheritdoc />
		[JsonIgnore]
		public bool Loaded { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="GuildSettings"/>.
		/// </summary>
		public GuildSettings()
		{
			RegisterSetting(() => WelcomeMessage, x => null, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => GoodbyeMessage, x => null, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => Slowmode, x => null, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => Rules, x => new RuleHolder(), AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => Prefix, x => null);
			RegisterSetting(() => ServerLogId, x => 0);
			RegisterSetting(() => ModLogId, x => 0);
			RegisterSetting(() => ImageLogId, x => 0);
			RegisterSetting(() => MuteRoleId, x => 0);
			RegisterSetting(() => NonVerboseErrors, x => false);
			RegisterSetting(() => SpamPreventionDictionary, ResetDictionary, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => RaidPreventionDictionary, ResetDictionary, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => PersistentRoles, ClearList, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => BotUsers, ClearList, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => SelfAssignableGroups, ClearList, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => Quotes, ClearList, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => LogActions, v => { v.Clear(); v.AddRange(_DefaultLogActions); return v; }, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => IgnoredCommandChannels, ClearList, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => IgnoredLogChannels, ClearList, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => IgnoredXpChannels, ClearList, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => ImageOnlyChannels, ClearList, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => BannedPhraseStrings, ClearList, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => BannedPhraseRegex, ClearList, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => BannedPhraseNames, ClearList, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => BannedPhrasePunishments, ClearList, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => CommandSettings, x => new CommandSettings(), AdvobotUtils.EmptyTryParse);
		}

		/// <inheritdoc />
		public async Task PostDeserializeAsync(SocketGuild guild)
		{
			GuildId = guild.Id;
			Invites.AddRange((await DiscordUtils.GetInvitesAsync(guild).CAF()).Select(x => new CachedInvite(x)));
			foreach (var group in SelfAssignableGroups ?? Enumerable.Empty<SelfAssignableRoles>())
			{
				group.PostDeserialize(guild);
			}
			Loaded = true;
		}
		/// <inheritdoc />
		public override FileInfo GetFile(IBotDirectoryAccessor accessor)
			=> StaticGetPath(accessor, GuildId);
		/// <summary>
		/// Creates an instance of <see cref="GuildSettings"/> from file.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="guildId"></param>
		/// <returns></returns>
		public static GuildSettings Load(IBotDirectoryAccessor accessor, ulong guildId)
			=> IOUtils.DeserializeFromFile<GuildSettings>(StaticGetPath(accessor, guildId)) ?? new GuildSettings();
		private static FileInfo StaticGetPath(IBotDirectoryAccessor accessor, ulong guildId)
			=> accessor.GetBaseBotDirectoryFile(Path.Combine("GuildSettings", $"{guildId}.json"));
	}
}
