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
		public RuleHolder Rules { get; } = new RuleHolder();
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
		public IDictionary<SpamType, SpamPreventionInfo> SpamPreventionDictionary { get; } = new Dictionary<SpamType, SpamPreventionInfo>
		{
			{ SpamType.Message, null },
			{ SpamType.LongMessage, null },
			{ SpamType.Link, null },
			{ SpamType.Image, null },
			{ SpamType.Mention, null }
		};
		/// <inheritdoc />
		[JsonProperty("RaidPrevention")]
		public IDictionary<RaidType, RaidPreventionInfo> RaidPreventionDictionary { get; } = new Dictionary<RaidType, RaidPreventionInfo>
		{
			{ RaidType.Regular, null },
			{ RaidType.RapidJoins, null }
		};
		/// <inheritdoc />
		[JsonProperty("PersistentRoles")]
		public IList<PersistentRole> PersistentRoles { get; } = new List<PersistentRole>();
		/// <inheritdoc />
		[JsonProperty("BotUsers")]
		public IList<BotImplementedPermissions> BotUsers { get; } = new List<BotImplementedPermissions>();
		/// <inheritdoc />
		[JsonProperty("SelfAssignableGroups")]
		public IList<SelfAssignableRoles> SelfAssignableGroups { get; } = new List<SelfAssignableRoles>();
		/// <inheritdoc />
		[JsonProperty("Quotes")]
		public IList<Quote> Quotes { get; } = new List<Quote>();
		/// <inheritdoc />
		[JsonProperty("LogActions")]
		public IList<LogAction> LogActions { get; } = new List<LogAction>(_DefaultLogActions);
		/// <inheritdoc />
		[JsonProperty("IgnoredCommandChannels")]
		public IList<ulong> IgnoredCommandChannels { get; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("IgnoredLogChannels")]
		public IList<ulong> IgnoredLogChannels { get; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("IgnoredXpChannels")]
		public IList<ulong> IgnoredXpChannels { get; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("ImageOnlyChannels")]
		public IList<ulong> ImageOnlyChannels { get; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("BannedPhraseStrings")]
		public IList<BannedPhrase> BannedPhraseStrings { get; } = new List<BannedPhrase>();
		/// <inheritdoc />
		[JsonProperty("BannedPhraseRegex")]
		public IList<BannedPhrase> BannedPhraseRegex { get; } = new List<BannedPhrase>();
		/// <inheritdoc />
		[JsonProperty("BannedPhraseNames")]
		public IList<BannedPhrase> BannedPhraseNames { get; } = new List<BannedPhrase>();
		/// <inheritdoc />
		[JsonProperty("BannedPhrasePunishments")]
		public IList<BannedPhrasePunishment> BannedPhrasePunishments { get; } = new List<BannedPhrasePunishment>();
		/// <inheritdoc />
		[JsonProperty("CommandSettings")]
		public CommandSettings CommandSettings { get; } = new CommandSettings();
		/// <inheritdoc />
		[JsonIgnore]
		public IList<SpamPreventionUserInfo> SpamPreventionUsers { get; } = new List<SpamPreventionUserInfo>();
		/// <inheritdoc />
		[JsonIgnore]
		public IList<SlowmodeUserInfo> SlowmodeUsers { get; } = new List<SlowmodeUserInfo>();
		/// <inheritdoc />
		[JsonIgnore]
		public IList<BannedPhraseUserInfo> BannedPhraseUsers { get; } = new List<BannedPhraseUserInfo>();
		/// <inheritdoc />
		[JsonIgnore]
		public IList<CachedInvite> Invites { get; } = new List<CachedInvite>();
		/// <inheritdoc />
		[JsonIgnore]
		public IList<string> EvaluatedRegex { get; } = new List<string>();
		/// <inheritdoc />
		[JsonIgnore]
		public MessageDeletion MessageDeletion { get; } = new MessageDeletion();
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
			RegisterSetting(this, x => x.WelcomeMessage, (s, v) => null);
			RegisterSetting(this, x => x.GoodbyeMessage, (s, v) => null);
			RegisterSetting(this, x => x.Slowmode, (s, v) => null);
			RegisterSetting(this, x => x.Rules, (s, v) => new RuleHolder());
			RegisterSetting(this, x => x.Prefix, (s, v) => null);
			RegisterSetting(this, x => x.ServerLogId, (s, v) => 0);
			RegisterSetting(this, x => x.ModLogId, (s, v) => 0);
			RegisterSetting(this, x => x.ImageLogId, (s, v) => 0);
			RegisterSetting(this, x => x.MuteRoleId, (s, v) => 0);
			RegisterSetting(this, x => x.NonVerboseErrors, (s, v) => false);
			RegisterSetting(this, x => x.SpamPreventionDictionary, (s, v) => { v.Keys.ToList().ForEach(x => v[x] = null); return v; });
			RegisterSetting(this, x => x.RaidPreventionDictionary, (s, v) => { v.Keys.ToList().ForEach(x => v[x] = null); return v; });
			RegisterSetting(this, x => x.PersistentRoles, (s, v) => { v.Clear(); return v; });
			RegisterSetting(this, x => x.BotUsers, (s, v) => { v.Clear(); return v; });
			RegisterSetting(this, x => x.SelfAssignableGroups, (s, v) => { v.Clear(); return v; });
			RegisterSetting(this, x => x.Quotes, (s, v) => { v.Clear(); return v; });
			RegisterSetting(this, x => x.LogActions, (s, v) => { v.Clear(); v.AddRange(_DefaultLogActions); return v; });
			RegisterSetting(this, x => x.IgnoredCommandChannels, (s, v) => { v.Clear(); return v; });
			RegisterSetting(this, x => x.IgnoredLogChannels, (s, v) => { v.Clear(); return v; });
			RegisterSetting(this, x => x.IgnoredXpChannels, (s, v) => { v.Clear(); return v; });
			RegisterSetting(this, x => x.ImageOnlyChannels, (s, v) => { v.Clear(); return v; });
			RegisterSetting(this, x => x.BannedPhraseStrings, (s, v) => { v.Clear(); return v; });
			RegisterSetting(this, x => x.BannedPhraseRegex, (s, v) => { v.Clear(); return v; });
			RegisterSetting(this, x => x.BannedPhraseNames, (s, v) => { v.Clear(); return v; });
			RegisterSetting(this, x => x.BannedPhrasePunishments, (s, v) => { v.Clear(); return v; });
			RegisterSetting(this, x => x.CommandSettings, (s, v) => new CommandSettings());
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
