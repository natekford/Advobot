using Advobot.Core.Classes.Attributes;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Holds settings for the bot. Settings are saved through property setters or calling <see cref="SaveSettings()"/>.
	/// </summary>
	public class BotSettings : SettingsBase, IBotSettings
	{
		[JsonProperty("LogLevel"), Setting(LogSeverity.Warning)]
		private LogSeverity _LogLevel = LogSeverity.Warning;
		[JsonProperty("TrustedUsers"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<ulong> _TrustedUsers = new List<ulong>();
		[JsonProperty("UsersUnableToDmOwner"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<ulong> _UsersUnableToDmOwner = new List<ulong>();
		[JsonProperty("UsersIgnoredFromCommands"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<ulong> _UsersIgnoredFromCommands = new List<ulong>();
		[JsonProperty("Prefix"), Setting("&&")]
		private string _Prefix = "&&";
		[JsonProperty("Game"), Setting(null)]
		private string _Game = null;
		[JsonProperty("Stream"), Setting(null)]
		private string _Stream = null;
		[JsonProperty("AlwaysDownloadUsers"), Setting(true)]
		private bool _AlwaysDownloadUsers = true;
		[JsonProperty("ShardCount"), Setting(1)]
		private int _ShardCount = 1;
		[JsonProperty("MessageCacheCount"), Setting(1000)]
		private int _MessageCacheCount = 1000;
		[JsonProperty("MaxUserGatherCount"), Setting(100)]
		private int _MaxUserGatherCount = 100;
		[JsonProperty("MaxMessageGatherSize"), Setting(500000)]
		private int _MaxMessageGatherSize = 500000;
		[JsonProperty("MaxRuleCategories"), Setting(20)]
		private int _MaxRuleCategories = 20;
		[JsonProperty("MaxRulesPerCategory"), Setting(20)]
		private int _MaxRulesPerCategory = 20;
		[JsonProperty("MaxSelfAssignableRoleGroups"), Setting(10)]
		private int _MaxSelfAssignableRoleGroups = 10;
		[JsonProperty("MaxQuotes"), Setting(500)]
		private int _MaxQuotes = 500;
		[JsonProperty("MaxBannedStrings"), Setting(50)]
		private int _MaxBannedStrings = 50;
		[JsonProperty("MaxBannedRegex"), Setting(25)]
		private int _MaxBannedRegex = 25;
		[JsonProperty("MaxBannedNames"), Setting(25)]
		private int _MaxBannedNames = 25;
		[JsonProperty("MaxBannedPunishments"), Setting(10)]
		private int _MaxBannedPunishments = 10;

		[JsonIgnore]
		public LogSeverity LogLevel
		{
			get => _LogLevel;
			set => _LogLevel = value;
		}
		[JsonIgnore]
		public List<ulong> TrustedUsers => _TrustedUsers;
		[JsonIgnore]
		public List<ulong> UsersUnableToDmOwner => _UsersUnableToDmOwner;
		[JsonIgnore]
		public List<ulong> UsersIgnoredFromCommands => _UsersIgnoredFromCommands;
		[JsonIgnore]
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
		[JsonIgnore]
		public string Game
		{
			get => _Game;
			set => _Game = value;
		}
		[JsonIgnore]
		public string Stream
		{
			get => _Stream;
			set => _Stream = value;
		}
		[JsonIgnore]
		public bool AlwaysDownloadUsers
		{
			get => _AlwaysDownloadUsers;
			set => _AlwaysDownloadUsers = value;
		}
		[JsonIgnore]
		public int ShardCount
		{
			get => _ShardCount;
			set
			{
				if (value < 1)
				{
					throw new ArgumentException("Must be greater than 0.", nameof(value));
				}
				_ShardCount = value;
			}
		}
		[JsonIgnore]
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
		[JsonIgnore]
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
		[JsonIgnore]
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
		[JsonIgnore]
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
		[JsonIgnore]
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
		[JsonIgnore]
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
		[JsonIgnore]
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
		[JsonIgnore]
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
		[JsonIgnore]
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
		[JsonIgnore]
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
		[JsonIgnore]
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
		[JsonIgnore]
		public bool Pause { get; set; }

		public override FileInfo GetFileLocation()
		{
			return IOUtils.GetBaseBotDirectoryFile(Constants.BOT_SETTINGS_LOC);
		}
	}
}