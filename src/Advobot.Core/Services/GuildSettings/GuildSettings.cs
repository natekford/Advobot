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
using AdvorangesUtils;
using Discord;
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
		public ulong GuildId => _Guild?.Id ?? 0;

		private IGuild? _Guild;

		/// <inheritdoc />
		public async Task PostDeserializeAsync(IGuild guild)
		{
			_Guild = guild;
			foreach (var invite in await guild.SafeGetInvitesAsync().CAF())
			{
				CachedInvites.Add(new CachedInvite(invite));
			}
			foreach (var group in SelfAssignableGroups)
			{
				group.RemoveRoles(group.Roles.Where(x => guild.GetRole(x) == null));
			}
		}
		/// <inheritdoc />
		public override FileInfo GetFile(IBotDirectoryAccessor accessor)
			=> StaticGetFile(accessor, GuildId);
		/// <summary>
		/// Creates an instance of <see cref="GuildSettings"/> from file.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static GuildSettings? Load(IBotDirectoryAccessor accessor, IGuild guild)
			=> IOUtils.DeserializeFromFile<GuildSettings>(StaticGetFile(accessor, guild.Id));
		private static FileInfo StaticGetFile(IBotDirectoryAccessor accessor, ulong guildId)
			=> accessor.GetBaseBotDirectoryFile(Path.Combine("GuildSettings", $"{guildId}.json"));
	}
}
