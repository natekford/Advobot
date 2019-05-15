using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Settings;
using Advobot.Classes.UserInformation;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
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
		/// <inheritdoc />
		[JsonProperty("WelcomeMessage")]
		public GuildNotification? WelcomeMessage { get => _WelcomeMessage; set => SetValue(ref _WelcomeMessage, value); }
		private GuildNotification? _WelcomeMessage;
		/// <inheritdoc />
		[JsonProperty("GoodbyeMessage")]
		public GuildNotification? GoodbyeMessage { get => _GoodbyeMessage; set => SetValue(ref _GoodbyeMessage, value); }
		private GuildNotification? _GoodbyeMessage;
		/// <inheritdoc />
		[JsonProperty("Prefix")]
		public string? Prefix { get => _Prefix; set => SetValue(ref _Prefix, value); }
		private string? _Prefix;
		/// <inheritdoc />
		[JsonProperty("ServerLog")]
		public ulong ServerLogId { get => _ServerLogId; set => SetValue(ref _ServerLogId, value); }
		private ulong _ServerLogId;
		/// <inheritdoc />
		[JsonProperty("ModLog")]
		public ulong ModLogId { get => _ModLogId; set => SetValue(ref _ModLogId, value); }
		private ulong _ModLogId;
		/// <inheritdoc />
		[JsonProperty("ImageLog")]
		public ulong ImageLogId { get => _ImageLogId; set => SetValue(ref _ImageLogId, value); }
		private ulong _ImageLogId;
		/// <inheritdoc />
		[JsonProperty("MuteRole")]
		public ulong MuteRoleId { get => _MuteRoleId; set => SetValue(ref _MuteRoleId, value); }
		private ulong _MuteRoleId;
		/// <inheritdoc />
		[JsonProperty("NonVerboseErrors")]
		public bool NonVerboseErrors { get => _NonVerboseErrors; set => SetValue(ref _NonVerboseErrors, value); }
		private bool _NonVerboseErrors;
		/// <inheritdoc />
		[JsonProperty("SpamPrevention")]
		public IList<SpamPrev> SpamPrevention { get; } = new ObservableCollection<SpamPrev>();
		/// <inheritdoc />
		[JsonProperty("RaidPrevention")]
		public IList<RaidPrev> RaidPrevention { get; } = new ObservableCollection<RaidPrev>();
		/// <inheritdoc />
		[JsonProperty("PersistentRoles")]
		public IList<PersistentRole> PersistentRoles { get; } = new ObservableCollection<PersistentRole>();
		/// <inheritdoc />
		[JsonProperty("BotUsers")]
		public IList<BotUser> BotUsers { get; } = new ObservableCollection<BotUser>();
		/// <inheritdoc />
		[JsonProperty("SelfAssignableGroups")]
		public IList<SelfAssignableRoles> SelfAssignableGroups { get; } = new ObservableCollection<SelfAssignableRoles>();
		/// <inheritdoc />
		[JsonProperty("Quotes")]
		public IList<Quote> Quotes { get; } = new ObservableCollection<Quote>();
		/// <inheritdoc />
		[JsonProperty("LogActions")]
		public IList<LogAction> LogActions { get; } = new ObservableCollection<LogAction>();
		/// <inheritdoc />
		[JsonProperty("IgnoredCommandChannels")]
		public IList<ulong> IgnoredCommandChannels { get; } = new ObservableCollection<ulong>();
		/// <inheritdoc />
		[JsonProperty("IgnoredLogChannels")]
		public IList<ulong> IgnoredLogChannels { get; } = new ObservableCollection<ulong>();
		/// <inheritdoc />
		[JsonProperty("IgnoredXpChannels")]
		public IList<ulong> IgnoredXpChannels { get; } = new ObservableCollection<ulong>();
		/// <inheritdoc />
		[JsonProperty("ImageOnlyChannels")]
		public IList<ulong> ImageOnlyChannels { get; } = new ObservableCollection<ulong>();
		/// <inheritdoc />
		[JsonProperty("BannedPhraseStrings")]
		public IList<BannedPhrase> BannedPhraseStrings { get; } = new ObservableCollection<BannedPhrase>();
		/// <inheritdoc />
		[JsonProperty("BannedPhraseRegex")]
		public IList<BannedPhrase> BannedPhraseRegex { get; } = new ObservableCollection<BannedPhrase>();
		/// <inheritdoc />
		[JsonProperty("BannedPhraseNames")]
		public IList<BannedPhrase> BannedPhraseNames { get; } = new ObservableCollection<BannedPhrase>();
		/// <inheritdoc />
		[JsonProperty("BannedPhrasePunishments")]
		public IList<BannedPhrasePunishment> BannedPhrasePunishments { get; } = new ObservableCollection<BannedPhrasePunishment>();
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
		public IList<BannedPhraseUserInfo> BannedPhraseUsers { get; } = new List<BannedPhraseUserInfo>();
		/// <inheritdoc />
		[JsonIgnore]
		public IList<CachedInvite> CachedInvites { get; } = new List<CachedInvite>();
		/// <inheritdoc />
		[JsonIgnore]
		public IList<string> EvaluatedRegex { get; } = new List<string>();
		/// <inheritdoc />
		[JsonIgnore]
		public MessageDeletion MessageDeletion { get; } = new MessageDeletion();
		/// <inheritdoc />
		[JsonIgnore]
		public ulong GuildId { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="GuildSettings"/>.
		/// </summary>
		public GuildSettings()
		{
			/*
			SettingParser.Add(new Setting<GuildNotification>(() => WelcomeMessage)
			{
				ResetValueFactory = x => default,
			});
			SettingParser.Add(new Setting<GuildNotification>(() => GoodbyeMessage)
			{
				ResetValueFactory = x => default,
			});
			SettingParser.Add(new Setting<RuleHolder>(() => Rules, parser: TryParseUtils.TryParseTemporary)
			{
				ResetValueFactory = x => new RuleHolder(),
			});
			SettingParser.Add(new Setting<CommandSettings>(() => CommandSettings, parser: TryParseUtils.TryParseTemporary)
			{
				ResetValueFactory = x => new CommandSettings(),
			});
			SettingParser.Add(new Setting<string?>(() => Prefix)
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
			SettingParser.Add(new CollectionSetting<BannedPhrasePunishment>(() => BannedPhrasePunishments));*/
		}

		/// <inheritdoc />
		public SpamPrev? this[SpamType type]
		{
			get => SpamPrevention.SingleOrDefault(x => x.Type == type);
			set
			{
				SpamPrevention.RemoveAll(x => x.Type == type);
				value.Type = type;
				SpamPrevention.Add(value);
				RaisePropertyChanged(nameof(SpamPrevention));
			}
		}
		/// <inheritdoc />
		public RaidPrev? this[RaidType type]
		{
			get => RaidPrevention.SingleOrDefault(x => x.Type == type);
			set
			{
				RaidPrevention.RemoveAll(x => x.Type == type);
				value.Type = type;
				RaidPrevention.Add(value);
				RaisePropertyChanged(nameof(RaidPrevention));
			}
		}

		/// <inheritdoc />
		public async Task PostDeserializeAsync(SocketGuild guild)
		{
			GuildId = guild.Id;
			foreach (var invite in await guild.SafeGetInvitesAsync().CAF() ?? Enumerable.Empty<RestInviteMetadata>())
			{
				CachedInvites.Add(new CachedInvite(invite));
			}
			foreach (var group in SelfAssignableGroups ?? Enumerable.Empty<SelfAssignableRoles>())
			{
				group.RemoveRoles(group.Roles.Where(x => guild.GetRole(x) == null));
			}
		}
		/// <inheritdoc />
		public override FileInfo GetFile(IBotDirectoryAccessor accessor)
			=> GetFile(accessor, GuildId);
		/// <summary>
		/// Creates an instance of <see cref="GuildSettings"/> from file.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="guildId"></param>
		/// <returns></returns>
		public static GuildSettings Load(IBotDirectoryAccessor accessor, ulong guildId)
			=> IOUtils.DeserializeFromFile<GuildSettings>(GetFile(accessor, guildId)) ?? new GuildSettings();
		private static FileInfo GetFile(IBotDirectoryAccessor accessor, ulong guildId)
			=> accessor.GetBaseBotDirectoryFile(Path.Combine("GuildSettings", $"{guildId}.json"));
	}
}
