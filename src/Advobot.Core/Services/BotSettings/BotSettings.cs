using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Advobot.Interfaces;
using Advobot.Resources;
using Advobot.Settings;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Newtonsoft.Json;

namespace Advobot.Services.BotSettings
{
	/// <summary>
	/// Holds settings for the bot.
	/// </summary>
	internal sealed class BotSettings : SettingsBase, IBotSettings
	{
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.LogLevel)), JsonProperty("LogLevel")]
		public LogSeverity LogLevel { get; set; } = LogSeverity.Warning;
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.Prefix)), JsonProperty("Prefix")]
		public string Prefix
		{
			get => _Prefix;
			set => ThrowIfElseSet(ref _Prefix, value, v => string.IsNullOrWhiteSpace(v), "Must not be null or whitespace.");
		}
		private string _Prefix = "&&";
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.Game)), JsonProperty("Game")]
		public string? Game
		{
			get => _Game;
			set => ThrowIfElseSet(ref _Game, value, v => v?.Length > 128, $"Must not be longer than 128 characters.");
		}
		private string? _Game;
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.Stream)), JsonProperty("Stream")]
		public string? Stream
		{
			get => _Stream;
			set => ThrowIfElseSet(ref _Stream, value, v => !RegexUtils.IsValidTwitchName(v), "Invalid Twitch stream name supplied.");
		}
		private string? _Stream;
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.AlwaysDownloadUsers)), JsonProperty("AlwaysDownloadUsers")]
		public bool AlwaysDownloadUsers { get; set; } = true;
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.MessageCacheSize)), JsonProperty("MessageCacheSize")]
		public int MessageCacheSize
		{
			get => _MessageCacheSize;
			set => ThrowIfElseSet(ref _MessageCacheSize, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MessageCacheSize = 1000;
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.MaxUserGatherCount)), JsonProperty("MaxUserGatherCount")]
		public int MaxUserGatherCount
		{
			get => _MaxUserGatherCount;
			set => ThrowIfElseSet(ref _MaxUserGatherCount, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxUserGatherCount = 100;
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.MaxMessageGatherSize)), JsonProperty("MaxMessageGatherSize")]
		public int MaxMessageGatherSize
		{
			get => _MaxMessageGatherSize;
			set => ThrowIfElseSet(ref _MaxMessageGatherSize, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxMessageGatherSize = 500000;
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.MaxRuleCategories)), JsonProperty("MaxRuleCategories")]
		public int MaxRuleCategories
		{
			get => _MaxRuleCategories;
			set => ThrowIfElseSet(ref _MaxRuleCategories, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxRuleCategories = 20;
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.MaxRulesPerCategory)), JsonProperty("MaxRulesPerCategory")]
		public int MaxRulesPerCategory
		{
			get => _MaxRulesPerCategory;
			set => ThrowIfElseSet(ref _MaxRulesPerCategory, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxRulesPerCategory = 20;
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.MaxSelfAssignableRoleGroups)), JsonProperty("MaxSelfAssignableRoleGroups")]
		public int MaxSelfAssignableRoleGroups
		{
			get => _MaxSelfAssignableRoleGroups;
			set => ThrowIfElseSet(ref _MaxSelfAssignableRoleGroups, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxSelfAssignableRoleGroups = 10;
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.MaxQuotes)), JsonProperty("MaxQuotes")]
		public int MaxQuotes
		{
			get => _MaxQuotes;
			set => ThrowIfElseSet(ref _MaxQuotes, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxQuotes = 500;
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.MaxBannedStrings)), JsonProperty("MaxBannedStrings")]
		public int MaxBannedStrings
		{
			get => _MaxBannedStrings;
			set => ThrowIfElseSet(ref _MaxBannedStrings, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxBannedStrings = 50;
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.MaxBannedRegex)), JsonProperty("MaxBannedRegex")]
		public int MaxBannedRegex
		{
			get => _MaxBannedRegex;
			set => ThrowIfElseSet(ref _MaxBannedRegex, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxBannedRegex = 25;
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.MaxBannedNames)), JsonProperty("MaxBannedNames")]
		public int MaxBannedNames
		{
			get => _MaxBannedNames;
			set => ThrowIfElseSet(ref _MaxBannedNames, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxBannedNames = 25;
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.MaxBannedPunishments)), JsonProperty("MaxBannedPunishments")]
		public int MaxBannedPunishments
		{
			get => _MaxBannedPunishments;
			set => ThrowIfElseSet(ref _MaxBannedPunishments, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxBannedPunishments = 10;
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.TrustedUsers)), JsonProperty("TrustedUsers")]
		public IList<ulong> TrustedUsers { get; } = new ObservableCollection<ulong>();
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.UsersUnableToDmOwner)), JsonProperty("UsersUnableToDmOwner")]
		public IList<ulong> UsersUnableToDmOwner { get; } = new ObservableCollection<ulong>();
		/// <inheritdoc />
		[Setting(nameof(BotSettingNames.UsersIgnoredFromCommands)), JsonProperty("UsersIgnoredFromCommands")]
		public IList<ulong> UsersIgnoredFromCommands { get; } = new ObservableCollection<ulong>();
		/// <inheritdoc />
		[JsonIgnore]
		public bool Pause { get; set; }
		/// <inheritdoc />
		[JsonIgnore]
		public DirectoryInfo BaseBotDirectory { get; private set; } = new DirectoryInfo(Directory.GetCurrentDirectory());
		/// <inheritdoc />
		[JsonIgnore]
		public string RestartArguments { get; private set; } = "";

		public BotSettings() { }

		/// <inheritdoc />
		protected override string GetLocalizedName(SettingAttribute attr)
			=> BotSettingNames.ResourceManager.GetString(attr.UnlocalizedName);

		#region Saving and Loading
		/// <inheritdoc />
		public override void Save()
			=> IOUtils.SafeWriteAllText(StaticGetPath(this), IOUtils.Serialize(this));
		/// <summary>
		/// Creates an instance of <see cref="BotSettings"/> from file.
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		public static BotSettings CreateOrLoad(ILowLevelConfig config)
		{
			var settings = IOUtils.DeserializeFromFile<BotSettings>(StaticGetPath(config)) ?? new BotSettings();
			settings.BaseBotDirectory = config.BaseBotDirectory;
			settings.RestartArguments = config.RestartArguments;
			return settings;
		}
		private static FileInfo StaticGetPath(IBotDirectoryAccessor accessor)
			=> accessor.GetBaseBotDirectoryFile("BotSettings.json");
		#endregion
	}
}
