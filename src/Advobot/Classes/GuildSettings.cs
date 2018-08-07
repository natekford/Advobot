using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Classes.Settings;
using Advobot.Classes.UserInformation;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes
{
	/// <summary>
	/// Holds settings for a guild.
	/// </summary>
	public class GuildSettings : SettingsBase, IGuildSettings
	{
		/// <inheritdoc />
		[JsonProperty("WelcomeMessage"), Setting(null)]
		public GuildNotification WelcomeMessage { get; set; }
		/// <inheritdoc />
		[JsonProperty("GoodbyeMessage"), Setting(null)]
		public GuildNotification GoodbyeMessage { get; set; }
		/// <inheritdoc />
		[JsonProperty("ListedInvite"), Setting(null)]
		public ListedInvite ListedInvite { get; set; }
		/// <inheritdoc />
		[JsonProperty("Slowmode"), Setting(null)]
		public Slowmode Slowmode { get; set; }
		/// <inheritdoc />
		[JsonProperty("Rules"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
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
		[JsonProperty("SpamPrevention"), Setting(NonCompileTimeDefaultValue.ClearDictionaryValues)]
		public Dictionary<SpamType, SpamPreventionInfo> SpamPreventionDictionary { get; } = new Dictionary<SpamType, SpamPreventionInfo>
		{
			{ SpamType.Message, null },
			{ SpamType.LongMessage, null },
			{ SpamType.Link, null },
			{ SpamType.Image, null },
			{ SpamType.Mention, null }
		};
		/// <inheritdoc />
		[JsonProperty("RaidPrevention"), Setting(NonCompileTimeDefaultValue.ClearDictionaryValues)]
		public Dictionary<RaidType, RaidPreventionInfo> RaidPreventionDictionary { get; } = new Dictionary<RaidType, RaidPreventionInfo>
		{
			{ RaidType.Regular, null },
			{ RaidType.RapidJoins, null }
		};
		/// <inheritdoc />
		[JsonProperty("PersistentRoles"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public List<PersistentRole> PersistentRoles { get; } = new List<PersistentRole>();
		/// <inheritdoc />
		[JsonProperty("BotUsers"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public List<BotImplementedPermissions> BotUsers { get; } = new List<BotImplementedPermissions>();
		/// <inheritdoc />
		[JsonProperty("SelfAssignableGroups"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public List<SelfAssignableRoles> SelfAssignableGroups { get; } = new List<SelfAssignableRoles>();
		/// <inheritdoc />
		[JsonProperty("Quotes"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public List<Quote> Quotes { get; } = new List<Quote>();
		/// <inheritdoc />
		[JsonProperty("LogActions"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public List<LogAction> LogActions { get; } = new List<LogAction>();
		/// <inheritdoc />
		[JsonProperty("IgnoredCommandChannels"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public List<ulong> IgnoredCommandChannels { get; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("IgnoredLogChannels"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public List<ulong> IgnoredLogChannels { get; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("IgnoredXpChannels"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public List<ulong> IgnoredXpChannels { get; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("ImageOnlyChannels"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public List<ulong> ImageOnlyChannels { get; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("BannedPhraseStrings"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public List<BannedPhrase> BannedPhraseStrings { get; } = new List<BannedPhrase>();
		/// <inheritdoc />
		[JsonProperty("BannedPhraseRegex"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public List<BannedPhrase> BannedPhraseRegex { get; } = new List<BannedPhrase>();
		/// <inheritdoc />
		[JsonProperty("BannedPhraseNames"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public List<BannedPhrase> BannedPhraseNames { get; } = new List<BannedPhrase>();
		/// <inheritdoc />
		[JsonProperty("BannedPhrasePunishments"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public List<BannedPhrasePunishment> BannedPhrasePunishments { get; } = new List<BannedPhrasePunishment>();
		/// <inheritdoc />
		[JsonProperty("CommandSettings"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public CommandSettings CommandSettings { get; } = new CommandSettings();
		/// <inheritdoc />
		[JsonIgnore]
		public List<SpamPreventionUserInfo> SpamPreventionUsers { get; } = new List<SpamPreventionUserInfo>();
		/// <inheritdoc />
		[JsonIgnore]
		public List<SlowmodeUserInfo> SlowmodeUsers { get; } = new List<SlowmodeUserInfo>();
		/// <inheritdoc />
		[JsonIgnore]
		public List<BannedPhraseUserInfo> BannedPhraseUsers { get; } = new List<BannedPhraseUserInfo>();
		/// <inheritdoc />
		[JsonIgnore]
		public List<CachedInvite> Invites { get; } = new List<CachedInvite>();
		/// <inheritdoc />
		[JsonIgnore]
		public List<string> EvaluatedRegex { get; } = new List<string>();
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
			ListedInvite?.PostDeserialize(guild);

			Loaded = true;
		}
		/// <inheritdoc />
		public override FileInfo GetPath(ILowLevelConfig config)
		{
			return StaticGetPath(config, GuildId);
		}
		/// <summary>
		/// Creates an instance of <typeparamref name="T"/> from file.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="config"></param>
		/// <param name="guildId"></param>
		/// <returns></returns>
		public static IGuildSettings Load<T>(ILowLevelConfig config, ulong guildId) where T : class, IGuildSettings, new()
		{
			return IOUtils.DeserializeFromFile<T>(StaticGetPath(config, guildId)) ?? new T();
		}
		private static FileInfo StaticGetPath(ILowLevelConfig config, ulong guildId)
		{
			return config.GetBaseBotDirectoryFile(Path.Combine("GuildSettings", $"{guildId}.json"));
		}
	}
}
