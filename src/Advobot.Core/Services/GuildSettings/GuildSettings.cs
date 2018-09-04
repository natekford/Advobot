using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
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
		/// <inheritdoc />
		[JsonProperty("WelcomeMessage"), Setting(null)]
		public GuildNotification WelcomeMessage { get; set; }
		/// <inheritdoc />
		[JsonProperty("GoodbyeMessage"), Setting(null)]
		public GuildNotification GoodbyeMessage { get; set; }
		/// <inheritdoc />
		[JsonProperty("Slowmode"), Setting(null)]
		public Slowmode Slowmode { get; set; }
		/// <inheritdoc />
		[JsonProperty("Rules"), Setting(NonCompileTimeDefaultValue.Default)]
		public RuleHolder Rules { get; } = new RuleHolder();
		/// <inheritdoc />
		[JsonProperty("Prefix"), Setting(null)]
		public string Prefix { get; set; }
		/// <inheritdoc />
		[JsonProperty("ServerLog"), Setting(0)]
		public ulong ServerLogId { get; set; }
		/// <inheritdoc />
		[JsonProperty("ModLog"), Setting(0)]
		public ulong ModLogId { get; set; }
		/// <inheritdoc />
		[JsonProperty("ImageLog"), Setting(0)]
		public ulong ImageLogId { get; set; }
		/// <inheritdoc />
		[JsonProperty("MuteRole"), Setting(0)]
		public ulong MuteRoleId { get; set; }
		/// <inheritdoc />
		[JsonProperty("NonVerboseErrors"), Setting(false)]
		public bool NonVerboseErrors { get; set; }
		/// <inheritdoc />
		[JsonProperty("SpamPrevention"), Setting(NonCompileTimeDefaultValue.ResetDictionaryValues)]
		public IDictionary<SpamType, SpamPreventionInfo> SpamPreventionDictionary { get; } = new Dictionary<SpamType, SpamPreventionInfo>
		{
			{ SpamType.Message, null },
			{ SpamType.LongMessage, null },
			{ SpamType.Link, null },
			{ SpamType.Image, null },
			{ SpamType.Mention, null }
		};
		/// <inheritdoc />
		[JsonProperty("RaidPrevention"), Setting(NonCompileTimeDefaultValue.ResetDictionaryValues)]
		public IDictionary<RaidType, RaidPreventionInfo> RaidPreventionDictionary { get; } = new Dictionary<RaidType, RaidPreventionInfo>
		{
			{ RaidType.Regular, null },
			{ RaidType.RapidJoins, null }
		};
		/// <inheritdoc />
		[JsonProperty("PersistentRoles"), Setting(NonCompileTimeDefaultValue.Default)]
		public IList<PersistentRole> PersistentRoles { get; } = new List<PersistentRole>();
		/// <inheritdoc />
		[JsonProperty("BotUsers"), Setting(NonCompileTimeDefaultValue.Default)]
		public IList<BotImplementedPermissions> BotUsers { get; } = new List<BotImplementedPermissions>();
		/// <inheritdoc />
		[JsonProperty("SelfAssignableGroups"), Setting(NonCompileTimeDefaultValue.Default)]
		public IList<SelfAssignableRoles> SelfAssignableGroups { get; } = new List<SelfAssignableRoles>();
		/// <inheritdoc />
		[JsonProperty("Quotes"), Setting(NonCompileTimeDefaultValue.Default)]
		public IList<Quote> Quotes { get; } = new List<Quote>();
		/// <inheritdoc />
		[JsonProperty("LogActions"), Setting(NonCompileTimeDefaultValue.Default)]
		public IList<LogAction> LogActions { get; } = new List<LogAction>();
		/// <inheritdoc />
		[JsonProperty("IgnoredCommandChannels"), Setting(NonCompileTimeDefaultValue.Default)]
		public IList<ulong> IgnoredCommandChannels { get; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("IgnoredLogChannels"), Setting(NonCompileTimeDefaultValue.Default)]
		public IList<ulong> IgnoredLogChannels { get; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("IgnoredXpChannels"), Setting(NonCompileTimeDefaultValue.Default)]
		public IList<ulong> IgnoredXpChannels { get; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("ImageOnlyChannels"), Setting(NonCompileTimeDefaultValue.Default)]
		public IList<ulong> ImageOnlyChannels { get; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("BannedPhraseStrings"), Setting(NonCompileTimeDefaultValue.Default)]
		public IList<BannedPhrase> BannedPhraseStrings { get; } = new List<BannedPhrase>();
		/// <inheritdoc />
		[JsonProperty("BannedPhraseRegex"), Setting(NonCompileTimeDefaultValue.Default)]
		public IList<BannedPhrase> BannedPhraseRegex { get; } = new List<BannedPhrase>();
		/// <inheritdoc />
		[JsonProperty("BannedPhraseNames"), Setting(NonCompileTimeDefaultValue.Default)]
		public IList<BannedPhrase> BannedPhraseNames { get; } = new List<BannedPhrase>();
		/// <inheritdoc />
		[JsonProperty("BannedPhrasePunishments"), Setting(NonCompileTimeDefaultValue.Default)]
		public IList<BannedPhrasePunishment> BannedPhrasePunishments { get; } = new List<BannedPhrasePunishment>();
		/// <inheritdoc />
		[JsonProperty("CommandSettings"), Setting(NonCompileTimeDefaultValue.Default)]
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
