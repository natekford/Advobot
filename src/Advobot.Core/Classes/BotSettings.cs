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
	/// Holds settings for the bot.
	/// </summary>
	public class BotSettings : SettingsBase, IBotSettings
	{
		/// <inheritdoc />
		[JsonIgnore]
		public LogSeverity LogLevel
		{
			get => _LogLevel;
			set => _LogLevel = value;
		}
		/// <inheritdoc />
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
		/// <inheritdoc />
		[JsonIgnore]
		public string Game
		{
			get => _Game;
			set => _Game = value;
		}
		/// <inheritdoc />
		[JsonIgnore]
		public string Stream
		{
			get => _Stream;
			set => _Stream = value;
		}
		/// <inheritdoc />
		[JsonIgnore]
		public bool AlwaysDownloadUsers
		{
			get => _AlwaysDownloadUsers;
			set => _AlwaysDownloadUsers = value;
		}
		/// <inheritdoc />
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
		/// <inheritdoc />
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
		/// <inheritdoc />
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
		/// <inheritdoc />
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
		/// <inheritdoc />
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
		/// <inheritdoc />
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
		/// <inheritdoc />
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
		/// <inheritdoc />
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
		/// <inheritdoc />
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
		/// <inheritdoc />
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
		/// <inheritdoc />
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
		/// <inheritdoc />
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
		/// <inheritdoc />
		[JsonIgnore]
		public List<ulong> TrustedUsers => _TrustedUsers;
		/// <inheritdoc />
		[JsonIgnore]
		public List<ulong> UsersUnableToDmOwner => _UsersUnableToDmOwner;
		/// <inheritdoc />
		[JsonIgnore]
		public List<ulong> UsersIgnoredFromCommands => _UsersIgnoredFromCommands;
		/// <inheritdoc />
		[JsonIgnore]
		public bool Pause { get; set; }
		/// <inheritdoc />
		[JsonIgnore]
		public override FileInfo FileLocation => FileUtils.GetBotSettingsFile();

		[JsonProperty("LogLevel"), Setting(LogSeverity.Warning)]
		private LogSeverity _LogLevel = LogSeverity.Warning;
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
		[JsonProperty("TrustedUsers"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<ulong> _TrustedUsers = new List<ulong>();
		[JsonProperty("UsersUnableToDmOwner"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<ulong> _UsersUnableToDmOwner = new List<ulong>();
		[JsonProperty("UsersIgnoredFromCommands"), Setting(NonCompileTimeDefaultValue.InstantiateDefaultParameterless)]
		private List<ulong> _UsersIgnoredFromCommands = new List<ulong>();
	}
}