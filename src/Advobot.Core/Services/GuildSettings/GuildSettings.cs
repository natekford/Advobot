using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Settings;
using Advobot.Classes.UserInformation;
using Advobot.Databases.Abstract;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Advobot.Services.GuildSettings
{
	/// <summary>
	/// Holds settings for a guild.
	/// </summary>
	internal sealed class GuildSettings : SettingsBase, IGuildSettings, IDatabaseEntry
	{
		/// <inheritdoc />
		[JsonIgnore]
		public ulong GuildId { get; set; }
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
		[Setting(nameof(GuildSettingNames.GuildCulture)), JsonProperty("Culture")]
		public string Culture
		{
			get => _Culture;
			set => ThrowIfElseSet(ref _Culture, value, x => CultureInfo.GetCultureInfo(x) == null, "Invalid culture provided.");
		}
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
		public IList<SpamPrev> SpamPrevention { get; set; } = new ObservableCollection<SpamPrev>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.RaidPrevention)), JsonProperty("RaidPrevention")]
		public IList<RaidPrev> RaidPrevention { get; set; } = new ObservableCollection<RaidPrev>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.PersistentRoles)), JsonProperty("PersistentRoles")]
		public IList<PersistentRole> PersistentRoles { get; set; } = new ObservableCollection<PersistentRole>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.BotUsers)), JsonProperty("BotUsers")]
		public IList<BotUser> BotUsers { get; set; } = new ObservableCollection<BotUser>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.SelfAssignableGroups)), JsonProperty("SelfAssignableGroups")]
		public IList<SelfAssignableRoles> SelfAssignableGroups { get; set; } = new ObservableCollection<SelfAssignableRoles>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.Quotes)), JsonProperty("Quotes")]
		public IList<Quote> Quotes { get; set; } = new ObservableCollection<Quote>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.LogActions)), JsonProperty("LogActions")]
		public IList<LogAction> LogActions { get; set; } = new ObservableSet<LogAction>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.IgnoredCommandChannels)), JsonProperty("IgnoredCommandChannels")]
		public IList<ulong> IgnoredCommandChannels { get; set; } = new ObservableSet<ulong>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.IgnoredLogChannels)), JsonProperty("IgnoredLogChannels")]
		public IList<ulong> IgnoredLogChannels { get; set; } = new ObservableSet<ulong>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.IgnoredXpChannels)), JsonProperty("IgnoredXpChannels")]
		public IList<ulong> IgnoredXpChannels { get; set; } = new ObservableSet<ulong>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.ImageOnlyChannels)), JsonProperty("ImageOnlyChannels")]
		public IList<ulong> ImageOnlyChannels { get; set; } = new ObservableSet<ulong>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.BannedPhraseStrings)), JsonProperty("BannedPhraseStrings")]
		public IList<BannedPhrase> BannedPhraseStrings { get; set; } = new ObservableCollection<BannedPhrase>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.BannedPhraseRegex)), JsonProperty("BannedPhraseRegex")]
		public IList<BannedPhrase> BannedPhraseRegex { get; set; } = new ObservableCollection<BannedPhrase>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.BannedPhraseNames)), JsonProperty("BannedPhraseNames")]
		public IList<BannedPhrase> BannedPhraseNames { get; set; } = new ObservableCollection<BannedPhrase>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.BannedPhrasePunishments)), JsonProperty("BannedPhrasePunishments")]
		public IList<BannedPhrasePunishment> BannedPhrasePunishments { get; set; } = new ObservableCollection<BannedPhrasePunishment>();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.Rules)), JsonProperty("Rules")]
		public RuleHolder Rules { get; set; } = new RuleHolder();
		/// <inheritdoc />
		[Setting(nameof(GuildSettingNames.CommandSettings)), JsonProperty("CommandSettings")]
		public CommandSettings CommandSettings { get; set; } = new CommandSettings();

		/// <inheritdoc />
		[JsonIgnore]
		public IList<BannedPhraseUserInfo> BannedPhraseUsers { get; } = new List<BannedPhraseUserInfo>();
		/// <inheritdoc />
		[JsonIgnore]
		public IList<string> EvaluatedRegex { get; } = new List<string>();

		private GuildSettingsFactory? _Parent;
		private readonly InviteCache _InviteCache = new InviteCache();
		private readonly DeletedMessageCache _DeletedMessageCache = new DeletedMessageCache();

		public GuildSettings() { }

		/// <inheritdoc />
		protected override string GetLocalizedName(SettingAttribute attr)
			=> GuildSettingNames.ResourceManager.GetString(attr.UnlocalizedName);
		/// <inheritdoc />
		public override void Save()
		{
			if (_Parent == null)
			{
				throw new InvalidOperationException("Unable to save due to parent not being set.");
			}
			_Parent.Save(this);
		}
		/// <inheritdoc />
		public InviteCache GetInviteCache()
			=> _InviteCache;
		/// <inheritdoc />
		public DeletedMessageCache GetDeletedMessageCache()
			=> _DeletedMessageCache;
		/// <summary>
		/// Stores the factory so <see cref="Save"/> can be called solely from this object.
		/// </summary>
		/// <param name="parent"></param>
		public void StoreGuildSettingsFactory(GuildSettingsFactory parent)
			=> _Parent = parent;

		//IDatabseEntry
		object IDatabaseEntry.Id { get => GuildId; set => GuildId = (ulong)value; }
	}
}
