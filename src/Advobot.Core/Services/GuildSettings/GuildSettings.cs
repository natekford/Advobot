using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.EqualityComparers;
using Advobot.Classes.Settings;
using Advobot.Classes.UserInformation;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesSettingParser.Implementation.Instance;
using AdvorangesSettingParser.Utils;
using AdvorangesUtils;
using Discord.Rest;
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
		public IList<SpamPrev> SpamPrevention { get; } = new List<SpamPrev>();
		/// <inheritdoc />
		[JsonProperty("RaidPrevention")]
		public IList<RaidPrev> RaidPrevention { get; } = new List<RaidPrev>();
		/// <inheritdoc />
		[JsonProperty("PersistentRoles")]
		public IList<PersistentRole> PersistentRoles { get; } = new List<PersistentRole>();
		/// <inheritdoc />
		[JsonProperty("BotUsers")]
		public IList<BotUser> BotUsers { get; } = new List<BotUser>();
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
		[JsonProperty("Rules")]
		public RuleHolder Rules { get; private set; } = new RuleHolder();
		/// <inheritdoc />
		[JsonProperty("CommandSettings")]
		public CommandSettings CommandSettings { get; private set; } = new CommandSettings();
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
			SettingParser.Add(new Setting<GuildNotification>(() => WelcomeMessage)
			{
				ResetValueFactory = x => null,
			});
			SettingParser.Add(new Setting<GuildNotification>(() => GoodbyeMessage)
			{
				ResetValueFactory = x => null,
			});
			SettingParser.Add(new Setting<Slowmode>(() => Slowmode)
			{
				ResetValueFactory = x => null,
			});
			SettingParser.Add(new Setting<RuleHolder>(() => Rules, parser: TryParseUtils.TryParseTemporary)
			{
				ResetValueFactory = x => new RuleHolder(),
			});
			SettingParser.Add(new Setting<CommandSettings>(() => CommandSettings, parser: TryParseUtils.TryParseTemporary)
			{
				ResetValueFactory = x => new CommandSettings(),
			});
			SettingParser.Add(new Setting<string>(() => Prefix)
			{
				ResetValueFactory = x => null,
			});
			SettingParser.Add(new Setting<ulong>(() => ServerLogId)
			{
				ResetValueFactory = x => 0,
			});
			SettingParser.Add(new Setting<ulong>(() => ModLogId)
			{
				ResetValueFactory = x => 0,
			});
			SettingParser.Add(new Setting<ulong>(() => ImageLogId)
			{
				ResetValueFactory = x => 0,
			});
			SettingParser.Add(new Setting<ulong>(() => MuteRoleId)
			{
				ResetValueFactory = x => 0,
			});
			SettingParser.Add(new Setting<bool>(() => NonVerboseErrors)
			{
				ResetValueFactory = x => false,
			});
			SettingParser.Add(new CollectionSetting<SpamPrev>(() => SpamPrevention));
			SettingParser.Add(new CollectionSetting<RaidPrev>(() => RaidPrevention));
			SettingParser.Add(new CollectionSetting<PersistentRole>(() => PersistentRoles));
			SettingParser.Add(new CollectionSetting<BotUser>(() => BotUsers));
			SettingParser.Add(new CollectionSetting<SelfAssignableRoles>(() => SelfAssignableGroups));
			SettingParser.Add(new CollectionSetting<Quote>(() => Quotes)
			{
				EqualityComparer = NameableEqualityComparer.Default,
			});
			SettingParser.Add(new CollectionSetting<LogAction>(() => LogActions)
			{
				ResetValueFactory = x =>
				{
					x.Clear();
					foreach (var value in _DefaultLogActions)
					{
						x.Add(value);
					}
					return x;
				},
			});
			SettingParser.Add(new CollectionSetting<ulong>(() => IgnoredCommandChannels));
			SettingParser.Add(new CollectionSetting<ulong>(() => IgnoredLogChannels));
			SettingParser.Add(new CollectionSetting<ulong>(() => IgnoredXpChannels));
			SettingParser.Add(new CollectionSetting<ulong>(() => ImageOnlyChannels));
			SettingParser.Add(new CollectionSetting<BannedPhrase>(() => BannedPhraseStrings));
			SettingParser.Add(new CollectionSetting<BannedPhrase>(() => BannedPhraseRegex));
			SettingParser.Add(new CollectionSetting<BannedPhrase>(() => BannedPhraseNames));
			SettingParser.Add(new CollectionSetting<BannedPhrasePunishment>(() => BannedPhrasePunishments));
		}

		/// <inheritdoc />
		public SpamPrev this[SpamType type]
		{
			get => SpamPrevention.SingleOrDefault(x => x.Type == type);
			set
			{
				SpamPrevention.RemoveAll(x => x.Type == type);
				SpamPrevention.Add(value);
			}
		}
		/// <inheritdoc />
		public RaidPrev this[RaidType type]
		{
			get => RaidPrevention.SingleOrDefault(x => x.Type == type);
			set
			{
				RaidPrevention.RemoveAll(x => x.Type == type);
				RaidPrevention.Add(value);
			}
		}

		/// <inheritdoc />
		public async Task PostDeserializeAsync(SocketGuild guild)
		{
			Loaded = true;
			GuildId = guild.Id;
			foreach (var invite in await guild.SafeGetInvitesAsync().CAF() ?? Enumerable.Empty<RestInviteMetadata>())
			{
				Invites.Add(new CachedInvite(invite));
			}
			foreach (var group in SelfAssignableGroups ?? Enumerable.Empty<SelfAssignableRoles>())
			{
				group.PostDeserialize(guild);
			}
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
