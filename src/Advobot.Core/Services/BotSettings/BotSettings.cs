using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Advobot.Classes;
using Advobot.Interfaces;
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
		[JsonProperty("LogLevel")]
		public LogSeverity LogLevel { get; set; } = LogSeverity.Warning;
		/// <inheritdoc />
		[JsonProperty("Prefix")]
		public string Prefix
		{
			get => _Prefix;
			set => ThrowIfElseSet(ref _Prefix, value, v => string.IsNullOrWhiteSpace(v), "Must not be null or whitespace.");
		}
		private string _Prefix = "&&";
		/// <inheritdoc />
		[JsonProperty("Game")]
		public string? Game
		{
			get => _Game;
			set => ThrowIfElseSet(ref _Game, value, v => v?.Length > 128, $"Must not be longer than 128 characters.");
		}
		private string? _Game;
		/// <inheritdoc />
		[JsonProperty("Stream")]
		public string? Stream
		{
			get => _Stream;
			set => ThrowIfElseSet(ref _Stream, value, v => !RegexUtils.IsValidTwitchName(v), "Invalid Twitch stream name supplied.");
		}
		private string? _Stream;
		/// <inheritdoc />
		[JsonProperty("AlwaysDownloadUsers")]
		public bool AlwaysDownloadUsers { get; set; } = true;
		/// <inheritdoc />
		[JsonProperty("MessageCacheSize")]
		public int MessageCacheSize
		{
			get => _MessageCacheSize;
			set => ThrowIfElseSet(ref _MessageCacheSize, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MessageCacheSize = 1000;
		/// <inheritdoc />
		[JsonProperty("MaxUserGatherCount")]
		public int MaxUserGatherCount
		{
			get => _MaxUserGatherCount;
			set => ThrowIfElseSet(ref _MaxUserGatherCount, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxUserGatherCount = 100;
		/// <inheritdoc />
		[JsonProperty("MaxMessageGatherSize")]
		public int MaxMessageGatherSize
		{
			get => _MaxMessageGatherSize;
			set => ThrowIfElseSet(ref _MaxMessageGatherSize, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxMessageGatherSize = 500000;
		/// <inheritdoc />
		[JsonProperty("MaxRuleCategories")]
		public int MaxRuleCategories
		{
			get => _MaxRuleCategories;
			set => ThrowIfElseSet(ref _MaxRuleCategories, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxRuleCategories = 20;
		/// <inheritdoc />
		[JsonProperty("MaxRulesPerCategory")]
		public int MaxRulesPerCategory
		{
			get => _MaxRulesPerCategory;
			set => ThrowIfElseSet(ref _MaxRulesPerCategory, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxRulesPerCategory = 20;
		/// <inheritdoc />
		[JsonProperty("MaxSelfAssignableRoleGroups")]
		public int MaxSelfAssignableRoleGroups
		{
			get => _MaxSelfAssignableRoleGroups;
			set => ThrowIfElseSet(ref _MaxSelfAssignableRoleGroups, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxSelfAssignableRoleGroups = 10;
		/// <inheritdoc />
		[JsonProperty("MaxQuotes")]
		public int MaxQuotes
		{
			get => _MaxQuotes;
			set => ThrowIfElseSet(ref _MaxQuotes, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxQuotes = 500;
		/// <inheritdoc />
		[JsonProperty("MaxBannedStrings")]
		public int MaxBannedStrings
		{
			get => _MaxBannedStrings;
			set => ThrowIfElseSet(ref _MaxBannedStrings, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxBannedStrings = 50;
		/// <inheritdoc />
		[JsonProperty("MaxBannedRegex")]
		public int MaxBannedRegex
		{
			get => _MaxBannedRegex;
			set => ThrowIfElseSet(ref _MaxBannedRegex, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxBannedRegex = 25;
		/// <inheritdoc />
		[JsonProperty("MaxBannedNames")]
		public int MaxBannedNames
		{
			get => _MaxBannedNames;
			set => ThrowIfElseSet(ref _MaxBannedNames, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxBannedNames = 25;
		/// <inheritdoc />
		[JsonProperty("MaxBannedPunishments")]
		public int MaxBannedPunishments
		{
			get => _MaxBannedPunishments;
			set => ThrowIfElseSet(ref _MaxBannedPunishments, value, v => v < 1, "Must be greater than 0.");
		}
		private int _MaxBannedPunishments = 10;
		/// <inheritdoc />
		[JsonProperty("TrustedUsers")]
		public IList<ulong> TrustedUsers { get; } = new ObservableCollection<ulong>();
		/// <inheritdoc />
		[JsonProperty("UsersUnableToDmOwner")]
		public IList<ulong> UsersUnableToDmOwner { get; } = new ObservableCollection<ulong>();
		/// <inheritdoc />
		[JsonProperty("UsersIgnoredFromCommands")]
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


		/// <summary>
		/// Creates an instance of <see cref="BotSettings"/>.
		/// </summary>
		private BotSettings()
		{
			/*
			SettingParser.Add(new Setting<LogSeverity>(() => LogLevel)
			{
				ResetValueFactory = x => LogSeverity.Warning,
			});
			SettingParser.Add(new Setting<string>(() => Prefix)
			{
				ResetValueFactory = x => "&&",
			});
			SettingParser.Add(new Setting<string?>(() => Game)
			{
				ResetValueFactory = x => null,
			});
			SettingParser.Add(new Setting<string?>(() => Stream)
			{
				ResetValueFactory = x => null,
			});
			SettingParser.Add(new Setting<bool>(() => AlwaysDownloadUsers)
			{
				ResetValueFactory = x => true,
			});
			SettingParser.Add(new Setting<int>(() => MessageCacheSize)
			{
				ResetValueFactory = x => 1000,
			});
			SettingParser.Add(new Setting<int>(() => MaxUserGatherCount)
			{
				ResetValueFactory = x => 100,
			});
			SettingParser.Add(new Setting<int>(() => MaxMessageGatherSize)
			{
				ResetValueFactory = x => 500000,
			});
			SettingParser.Add(new Setting<int>(() => MaxRuleCategories)
			{
				ResetValueFactory = x => 20,
			});
			SettingParser.Add(new Setting<int>(() => MaxRulesPerCategory)
			{
				ResetValueFactory = x => 20,
			});
			SettingParser.Add(new Setting<int>(() => MaxSelfAssignableRoleGroups)
			{
				ResetValueFactory = x => 10,
			});
			SettingParser.Add(new Setting<int>(() => MaxQuotes)
			{
				ResetValueFactory = x => 500,
			});
			SettingParser.Add(new Setting<int>(() => MaxBannedStrings)
			{
				ResetValueFactory = x => 50,
			});
			SettingParser.Add(new Setting<int>(() => MaxBannedRegex)
			{
				ResetValueFactory = x => 25,
			});
			SettingParser.Add(new Setting<int>(() => MaxBannedNames)
			{
				ResetValueFactory = x => 25,
			});
			SettingParser.Add(new Setting<int>(() => MaxBannedPunishments)
			{
				ResetValueFactory = x => 10,
			});
			SettingParser.Add(new CollectionSetting<ulong>(() => TrustedUsers));
			SettingParser.Add(new CollectionSetting<ulong>(() => UsersUnableToDmOwner));
			SettingParser.Add(new CollectionSetting<ulong>(() => UsersIgnoredFromCommands));*/
		}

		/// <inheritdoc />
		public override FileInfo GetFile(IBotDirectoryAccessor accessor)
			=> StaticGetPath(accessor);
		/// <summary>
		/// Creates an instance of <see cref="BotSettings"/> from file.
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		public static BotSettings Load(ILowLevelConfig config)
		{
			var settings = IOUtils.DeserializeFromFile<BotSettings>(StaticGetPath(config)) ?? new BotSettings();
			settings.BaseBotDirectory = config.BaseBotDirectory;
			settings.RestartArguments = config.RestartArguments;
			return settings;
		}
		private static FileInfo StaticGetPath(IBotDirectoryAccessor accessor)
			=> accessor.GetBaseBotDirectoryFile("BotSettings.json");
	}
}
