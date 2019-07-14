using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Settings;
using Advobot.Classes.UserInformation;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Resources;
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
		[Setting(nameof(GuildSettingNames.WelcomeMessage)), JsonProperty("WelcomeMessage")]
		public GuildNotification? WelcomeMessage
		{
			get => _WelcomeMessage;
			set => SetValue(ref _WelcomeMessage, value);
		}
		private GuildNotification? _WelcomeMessage;
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.GoodbyeMessage)), JsonProperty("GoodbyeMessage")]
		public GuildNotification? GoodbyeMessage
		{
			get => _GoodbyeMessage;
			set => SetValue(ref _GoodbyeMessage, value);
		}
		private GuildNotification? _GoodbyeMessage;
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.GuildCulture)), JsonIgnore]
		public CultureInfo Culture
		{
			get => CultureInfo.GetCultureInfo(_Culture);
			set => SetValue(ref _Culture, value.Name);
		}
		[JsonProperty("Culture")]
		private string _Culture = "en-US";
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.Prefix)), JsonProperty("Prefix")]
		public string? Prefix
		{
			get => _Prefix;
			set => SetValue(ref _Prefix, value);
		}
		private string? _Prefix;
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.ServerLog)), JsonProperty("ServerLog")]
		public ulong ServerLogId
		{
			get => _ServerLogId;
			set => SetValue(ref _ServerLogId, value);
		}
		private ulong _ServerLogId;
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.ModLog)), JsonProperty("ModLog")]
		public ulong ModLogId
		{
			get => _ModLogId;
			set => SetValue(ref _ModLogId, value);
		}
		private ulong _ModLogId;
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.ImageLog)), JsonProperty("ImageLog")]
		public ulong ImageLogId
		{
			get => _ImageLogId;
			set => SetValue(ref _ImageLogId, value);
		}
		private ulong _ImageLogId;
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.MuteRole)), JsonProperty("MuteRole")]
		public ulong MuteRoleId
		{
			get => _MuteRoleId;
			set => SetValue(ref _MuteRoleId, value);
		}
		private ulong _MuteRoleId;
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.NonVerboseErrors)), JsonProperty("NonVerboseErrors")]
		public bool NonVerboseErrors
		{
			get => _NonVerboseErrors;
			set => SetValue(ref _NonVerboseErrors, value);
		}
		private bool _NonVerboseErrors;
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.SpamPrevention)), JsonProperty("SpamPrevention")]
		public IList<SpamPrev> SpamPrevention { get; } = new ObservableCollection<SpamPrev>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.RaidPrevention)), JsonProperty("RaidPrevention")]
		public IList<RaidPrev> RaidPrevention { get; } = new ObservableCollection<RaidPrev>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.PersistentRoles)), JsonProperty("PersistentRoles")]
		public IList<PersistentRole> PersistentRoles { get; } = new ObservableCollection<PersistentRole>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.BotUsers)), JsonProperty("BotUsers")]
		public IList<BotUser> BotUsers { get; } = new ObservableCollection<BotUser>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.SelfAssignableGroups)), JsonProperty("SelfAssignableGroups")]
		public IList<SelfAssignableRoles> SelfAssignableGroups { get; } = new ObservableCollection<SelfAssignableRoles>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.Quotes)), JsonProperty("Quotes")]
		public IList<Quote> Quotes { get; } = new ObservableCollection<Quote>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.LogActions)), JsonProperty("LogActions")]
		public IList<LogAction> LogActions { get; } = new ObservableCollection<LogAction>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.IgnoredCommandChannels)), JsonProperty("IgnoredCommandChannels")]
		public IList<ulong> IgnoredCommandChannels { get; } = new ObservableCollection<ulong>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.IgnoredLogChannels)), JsonProperty("IgnoredLogChannels")]
		public IList<ulong> IgnoredLogChannels { get; } = new ObservableCollection<ulong>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.IgnoredXpChannels)), JsonProperty("IgnoredXpChannels")]
		public IList<ulong> IgnoredXpChannels { get; } = new ObservableCollection<ulong>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.ImageOnlyChannels)), JsonProperty("ImageOnlyChannels")]
		public IList<ulong> ImageOnlyChannels { get; } = new ObservableCollection<ulong>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.BannedPhraseStrings)), JsonProperty("BannedPhraseStrings")]
		public IList<BannedPhrase> BannedPhraseStrings { get; } = new ObservableCollection<BannedPhrase>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.BannedPhraseRegex)), JsonProperty("BannedPhraseRegex")]
		public IList<BannedPhrase> BannedPhraseRegex { get; } = new ObservableCollection<BannedPhrase>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.BannedPhraseNames)), JsonProperty("BannedPhraseNames")]
		public IList<BannedPhrase> BannedPhraseNames { get; } = new ObservableCollection<BannedPhrase>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.BannedPhrasePunishments)), JsonProperty("BannedPhrasePunishments")]
		public IList<BannedPhrasePunishment> BannedPhrasePunishments { get; } = new ObservableCollection<BannedPhrasePunishment>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.Rules)), JsonProperty("Rules")]
		public RuleHolder Rules { get; private set; } = new RuleHolder();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.CommandSettings)), JsonProperty("CommandSettings")]
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
		private IBotSettings? _BotSettings;

		private GuildSettings() { }

		/// <inheritdoc />
		public string GetPrefix()
			=> Prefix ?? _BotSettings?.Prefix ?? throw new InvalidOperationException("Unable to find a prefix.");
		/// <inheritdoc />
		protected override string GetLocalizedName(SettingAttribute attr)
			=> GuildSettingNames.ResourceManager.GetString(attr.UnlocalizedName);

		#region Saving and Loading
		/// <inheritdoc />
		public override void Save()
		{
			if (_BotSettings == null)
			{
				throw new InvalidOperationException("Unable to save.");
			}

			var path = StaticGetFile(_BotSettings, GuildId);
			IOUtils.SafeWriteAllText(path, IOUtils.Serialize(this));
		}
		/// <summary>
		/// Creates an instance of <see cref="GuildSettings"/> from file.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static async Task<GuildSettings> CreateOrLoadAsync(IBotSettings settings, IGuild guild)
		{
			var path = StaticGetFile(settings, guild.Id);
			var instance = IOUtils.DeserializeFromFile<GuildSettings>(path);
			if (instance == null)
			{
				instance = new GuildSettings();
				instance.Save();
			}

			instance._BotSettings = settings;
			instance._Guild = guild;

			foreach (var invite in await guild.SafeGetInvitesAsync().CAF())
			{
				instance.CachedInvites.Add(new CachedInvite(invite));
			}
			foreach (var group in instance.SelfAssignableGroups)
			{
				group.RemoveRoles(group.Roles.Where(x => guild.GetRole(x) == null));
			}

			return instance;
		}
		private static FileInfo StaticGetFile(IBotDirectoryAccessor accessor, ulong guildId)
			=> accessor.GetBaseBotDirectoryFile(Path.Combine("GuildSettings", $"{guildId}.json"));
		#endregion
	}
}
