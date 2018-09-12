using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Enums;
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
		/// <inheritdoc />
		[JsonProperty("Game")]
		public string Game
		{
			get => _Game;
			set
			{
				var max = ValidateStringAttribute.MinsMaxesAndErrors[Target.Game].Max;
				ThrowIfElseSet(ref _Game, value, v => v?.Length > max, $"Must not be longer than {max} characters.");
			}
		}
		/// <inheritdoc />
		[JsonProperty("Stream")]
		public string Stream
		{
			get => _Stream;
			set => ThrowIfElseSet(ref _Stream, value, v => !RegexUtils.IsValidTwitchName(v), "Invalid Twitch stream name supplied.");
		}
		/// <inheritdoc />
		[JsonProperty("AlwaysDownloadUsers")]
		public bool AlwaysDownloadUsers { get; set; } = true;
		/// <inheritdoc />
		[JsonProperty("MessageCacheSize")]
		public int MessageCacheSize
		{
			get => _MessageCacheCount;
			set => ThrowIfElseSet(ref _MessageCacheCount, value, v => v < 1, "Must be greater than 0.");
		}
		/// <inheritdoc />
		[JsonProperty("MaxUserGatherCount")]
		public int MaxUserGatherCount
		{
			get => _MaxUserGatherCount;
			set => ThrowIfElseSet(ref _MaxUserGatherCount, value, v => v < 1, "Must be greater than 0.");
		}
		/// <inheritdoc />
		[JsonProperty("MaxMessageGatherSize")]
		public int MaxMessageGatherSize
		{
			get => _MaxMessageGatherSize;
			set => ThrowIfElseSet(ref _MaxMessageGatherSize, value, v => v < 1, "Must be greater than 0.");
		}
		/// <inheritdoc />
		[JsonProperty("MaxRuleCategories")]
		public int MaxRuleCategories
		{
			get => _MaxRuleCategories;
			set => ThrowIfElseSet(ref _MaxRuleCategories, value, v => v < 1, "Must be greater than 0.");
		}
		/// <inheritdoc />
		[JsonProperty("MaxRulesPerCategory")]
		public int MaxRulesPerCategory
		{
			get => _MaxRulesPerCategory;
			set => ThrowIfElseSet(ref _MaxRulesPerCategory, value, v => v < 1, "Must be greater than 0.");
		}
		/// <inheritdoc />
		[JsonProperty("MaxSelfAssignableRoleGroups")]
		public int MaxSelfAssignableRoleGroups
		{
			get => _MaxSelfAssignableRoleGroups;
			set => ThrowIfElseSet(ref _MaxSelfAssignableRoleGroups, value, v => v < 1, "Must be greater than 0.");
		}
		/// <inheritdoc />
		[JsonProperty("MaxQuotes")]
		public int MaxQuotes
		{
			get => _MaxQuotes;
			set => ThrowIfElseSet(ref _MaxQuotes, value, v => v < 1, "Must be greater than 0.");
		}
		/// <inheritdoc />
		[JsonProperty("MaxBannedStrings")]
		public int MaxBannedStrings
		{
			get => _MaxBannedStrings;
			set => ThrowIfElseSet(ref _MaxBannedStrings, value, v => v < 1, "Must be greater than 0.");
		}
		/// <inheritdoc />
		[JsonProperty("MaxBannedRegex")]
		public int MaxBannedRegex
		{
			get => _MaxBannedRegex;
			set => ThrowIfElseSet(ref _MaxBannedRegex, value, v => v < 1, "Must be greater than 0.");
		}
		/// <inheritdoc />
		[JsonProperty("MaxBannedNames")]
		public int MaxBannedNames
		{
			get => _MaxBannedNames;
			set => ThrowIfElseSet(ref _MaxBannedNames, value, v => v < 1, "Must be greater than 0.");
		}
		/// <inheritdoc />
		[JsonProperty("MaxBannedPunishments")]
		public int MaxBannedPunishments
		{
			get => _MaxBannedPunishments;
			set => ThrowIfElseSet(ref _MaxBannedPunishments, value, v => v < 1, "Must be greater than 0.");
		}
		/// <inheritdoc />
		[JsonProperty("TrustedUsers")]
		public IList<ulong> TrustedUsers { get; private set; } = new ObservableCollection<ulong>();
		/// <inheritdoc />
		[JsonProperty("UsersUnableToDmOwner")]
		public IList<ulong> UsersUnableToDmOwner { get; private set; } = new ObservableCollection<ulong>();
		/// <inheritdoc />
		[JsonProperty("UsersIgnoredFromCommands")]
		public IList<ulong> UsersIgnoredFromCommands { get; private set; } = new ObservableCollection<ulong>();
		/// <inheritdoc />
		[JsonIgnore]
		public bool Pause { get; set; }
		/// <inheritdoc />
		[JsonIgnore]
		public DirectoryInfo BaseBotDirectory { get; private set; }
		/// <inheritdoc />
		[JsonIgnore]
		public string RestartArguments { get; private set; }

		[JsonIgnore]
		private string _Prefix = "&&";
		[JsonIgnore]
		private string _Stream;
		[JsonIgnore]
		private string _Game;
		[JsonIgnore]
		private int _MessageCacheCount = 1000;
		[JsonIgnore]
		private int _MaxUserGatherCount = 100;
		[JsonIgnore]
		private int _MaxMessageGatherSize = 500000;
		[JsonIgnore]
		private int _MaxRuleCategories = 20;
		[JsonIgnore]
		private int _MaxRulesPerCategory = 20;
		[JsonIgnore]
		private int _MaxSelfAssignableRoleGroups = 10;
		[JsonIgnore]
		private int _MaxQuotes = 500;
		[JsonIgnore]
		private int _MaxBannedStrings = 50;
		[JsonIgnore]
		private int _MaxBannedRegex = 25;
		[JsonIgnore]
		private int _MaxBannedNames = 25;
		[JsonIgnore]
		private int _MaxBannedPunishments = 10;

		/// <summary>
		/// Creates an instance of <see cref="BotSettings"/>.
		/// </summary>
		public BotSettings()
		{
			IList<T> ClearList<T>(IList<T> x) { x.Clear(); return x; }

			RegisterSetting(() => LogLevel, x => LogSeverity.Warning, AdvobotUtils.TryParseCaseIns);
			RegisterSetting(() => Prefix, x => "&&");
			RegisterSetting(() => Game, x => null);
			RegisterSetting(() => Stream, x => null);
			RegisterSetting(() => AlwaysDownloadUsers, x => true);
			RegisterSetting(() => MessageCacheSize, x => 1000);
			RegisterSetting(() => MaxUserGatherCount, x => 100);
			RegisterSetting(() => MaxMessageGatherSize, x => 500000);
			RegisterSetting(() => MaxRuleCategories, x => 20);
			RegisterSetting(() => MaxRulesPerCategory, x => 20);
			RegisterSetting(() => MaxSelfAssignableRoleGroups, x => 10);
			RegisterSetting(() => MaxQuotes, x => 500);
			RegisterSetting(() => MaxBannedStrings, x => 50);
			RegisterSetting(() => MaxBannedRegex, x => 25);
			RegisterSetting(() => MaxBannedNames, x => 25);
			RegisterSetting(() => MaxBannedPunishments, x => 10);
			RegisterSetting(() => TrustedUsers, ClearList, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => UsersUnableToDmOwner, ClearList, AdvobotUtils.EmptyTryParse);
			RegisterSetting(() => UsersIgnoredFromCommands, ClearList, AdvobotUtils.EmptyTryParse);
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
