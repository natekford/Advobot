using System;
using System.Collections.Generic;
using System.IO;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Newtonsoft.Json;

namespace Advobot.Classes
{
	/// <summary>
	/// Holds settings for the bot.
	/// </summary>
	public class BotSettings : SettingsBase, IBotSettings
	{
		/// <inheritdoc />
		[JsonProperty("LogLevel"), Setting(LogSeverity.Warning)]
		public LogSeverity LogLevel { get; set; } = LogSeverity.Warning;
		/// <inheritdoc />
		[JsonProperty("Prefix"), Setting("&&")]
		public string Prefix
		{
			get => _Prefix;
			set
			{
				if (String.IsNullOrWhiteSpace(value))
				{
					throw new ArgumentException("Must not be null or whitespace.", nameof(value));
				}
				_Prefix = value;
			}
		}
		/// <inheritdoc />
		[JsonProperty("Game"), Setting(null)]
		public string Game { get; set; }
		/// <inheritdoc />
		[JsonProperty("Stream"), Setting(null)]
		public string Stream { get; set; }
		/// <inheritdoc />
		[JsonProperty("AlwaysDownloadUsers"), Setting(true)]
		public bool AlwaysDownloadUsers { get; set; } = true;
		/// <inheritdoc />
		[JsonProperty("MessageCacheCount"), Setting(1000)]
		public int MessageCacheCount
		{
			get => _MessageCacheCount;
			set
			{
				if (value < 1)
				{
					throw new ArgumentException("Must be greater than 0.", nameof(value));
				}
				_MessageCacheCount = value;
			}
		}
		/// <inheritdoc />
		[JsonProperty("MaxUserGatherCount"), Setting(100)]
		public int MaxUserGatherCount
		{
			get => _MaxUserGatherCount;
			set
			{
				if (value < 1)
				{
					throw new ArgumentException("Must be greater than 0.", nameof(value));
				}
				_MaxUserGatherCount = value;
			}
		}
		/// <inheritdoc />
		[JsonProperty("MaxMessageGatherSize"), Setting(500000)]
		public int MaxMessageGatherSize
		{
			get => _MaxMessageGatherSize;
			set
			{
				if (value < 1)
				{
					throw new ArgumentException("Must be greater than 0.", nameof(value));
				}
				_MaxMessageGatherSize = value;
			}
		}
		/// <inheritdoc />
		[JsonProperty("MaxRuleCategories"), Setting(20)]
		public int MaxRuleCategories
		{
			get => _MaxRuleCategories;
			set
			{
				if (value < 1)
				{
					throw new ArgumentException("Must be greater than 0.", nameof(value));
				}
				_MaxRuleCategories = value;
			}
		}
		/// <inheritdoc />
		[JsonProperty("MaxRulesPerCategory"), Setting(20)]
		public int MaxRulesPerCategory
		{
			get => _MaxRulesPerCategory;
			set
			{
				if (value < 1)
				{
					throw new ArgumentException("Must be greater than 0.", nameof(value));
				}
				_MaxRulesPerCategory = value;
			}
		}
		/// <inheritdoc />
		[JsonProperty("MaxSelfAssignableRoleGroups"), Setting(10)]
		public int MaxSelfAssignableRoleGroups
		{
			get => _MaxSelfAssignableRoleGroups;
			set
			{
				if (value < 1)
				{
					throw new ArgumentException("Must be greater than 0.", nameof(value));
				}
				_MaxSelfAssignableRoleGroups = value;
			}
		}
		/// <inheritdoc />
		[JsonProperty("MaxQuotes"), Setting(500)]
		public int MaxQuotes
		{
			get => _MaxQuotes;
			set
			{
				if (value < 1)
				{
					throw new ArgumentException("Must be greater than 0.", nameof(value));
				}
				_MaxQuotes = value;
			}
		}
		/// <inheritdoc />
		[JsonProperty("MaxBannedStrings"), Setting(50)]
		public int MaxBannedStrings
		{
			get => _MaxBannedStrings;
			set
			{
				if (value < 1)
				{
					throw new ArgumentException("Must be greater than 0.", nameof(value));
				}
				_MaxBannedStrings = value;
			}
		}
		/// <inheritdoc />
		[JsonProperty("MaxBannedRegex"), Setting(25)]
		public int MaxBannedRegex
		{
			get => _MaxBannedRegex;
			set
			{
				if (value < 1)
				{
					throw new ArgumentException("Must be greater than 0.", nameof(value));
				}
				_MaxBannedRegex = value;
			}
		}
		/// <inheritdoc />
		[JsonProperty("MaxBannedNames"), Setting(25)]
		public int MaxBannedNames
		{
			get => _MaxBannedNames;
			set
			{
				if (value < 1)
				{
					throw new ArgumentException("Must be greater than 0.", nameof(value));
				}
				_MaxBannedNames = value;
			}
		}
		/// <inheritdoc />
		[JsonProperty("MaxBannedPunishments"), Setting(10)]
		public int MaxBannedPunishments
		{
			get => _MaxBannedPunishments;
			set
			{
				if (value < 1)
				{
					throw new ArgumentException("Must be greater than 0.", nameof(value));
				}
				_MaxBannedPunishments = value;
			}
		}
		/// <inheritdoc />
		[JsonProperty("TrustedUsers"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public List<ulong> TrustedUsers { get; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("UsersUnableToDmOwner"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public List<ulong> UsersUnableToDmOwner { get; } = new List<ulong>();
		/// <inheritdoc />
		[JsonProperty("UsersIgnoredFromCommands"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		public List<ulong> UsersIgnoredFromCommands { get; } = new List<ulong>();
		/// <inheritdoc />
		[JsonIgnore]
		public bool Pause { get; set; }

		[JsonIgnore]
		private string _Prefix = "&&";
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

		/// <inheritdoc />
		protected override FileInfo GetPath(ILowLevelConfig config)
		{
			return StaticGetPath(config);
		}
		/// <summary>
		/// Creates an instance of <typeparamref name="T"/> from file.
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		public static IBotSettings Load<T>(ILowLevelConfig config) where T : class, IBotSettings, new()
		{
			return IOUtils.DeserializeFromFile<T>(StaticGetPath(config)) ?? new T();
		}
		private static FileInfo StaticGetPath(ILowLevelConfig config)
		{
			return config.GetBaseBotDirectoryFile("BotSettings.json");
		}
	}
}