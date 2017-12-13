using Advobot.Core.Interfaces;
using Discord;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Advobot.Core.Classes.Settings
{
	/// <summary>
	/// Holds settings for the bot. Settings are saved through property setters or calling <see cref="SaveSettings()"/>.
	/// </summary>
	public partial class BotSettings : IBotSettings, INotifyPropertyChanged
	{
		[JsonProperty("TrustedUsers")]
		private List<ulong> _TrustedUsers;
		[JsonProperty("UsersUnableToDMOwner")]
		private List<ulong> _UsersUnableToDMOwner;
		[JsonProperty("UsersIgnoredFromCommands")]
		private List<ulong> _UsersIgnoredFromCommands;
		[JsonProperty("ShardCount")]
		private int _ShardCount;
		[JsonProperty("MessageCacheCount")]
		private int _MessageCacheCount;
		[JsonProperty("MaxUserGatherCount")]
		private int _MaxUserGatherCount;
		[JsonProperty("MaxMessageGatherSize")]
		private int _MaxMessageGatherSize;
		[JsonProperty("Prefix")]
		private string _Prefix;
		[JsonProperty("Game")]
		private string _Game;
		[JsonProperty("Stream")]
		private string _Stream;
		[JsonProperty("AlwaysDownloadUsers")]
		private bool _AlwaysDownloadUsers = true;
		[JsonProperty("LogLevel")]
		private LogSeverity _LogLevel = LogSeverity.Warning;

		[JsonIgnore]
		public IReadOnlyList<ulong> TrustedUsers
		{
			get => _TrustedUsers.AsReadOnly() ?? (_TrustedUsers = new List<ulong>()).AsReadOnly();
			set
			{
				this._TrustedUsers = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<ulong> UsersUnableToDMOwner
		{
			get => _UsersUnableToDMOwner.AsReadOnly() ?? (_UsersUnableToDMOwner = new List<ulong>()).AsReadOnly();
			set
			{
				this._UsersUnableToDMOwner = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<ulong> UsersIgnoredFromCommands
		{
			get => _UsersIgnoredFromCommands.AsReadOnly() ?? (_UsersIgnoredFromCommands = new List<ulong>()).AsReadOnly();
			set
			{
				this._UsersIgnoredFromCommands = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public int ShardCount
		{
			get => _ShardCount > 1 ? _ShardCount : (_ShardCount = 1);
			set
			{
				this._ShardCount = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public int MessageCacheCount
		{
			get => _MessageCacheCount > 0 ? _MessageCacheCount : (_MessageCacheCount = 1000);
			set
			{
				this._MessageCacheCount = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public int MaxUserGatherCount
		{
			get => _MaxUserGatherCount > 0 ? _MaxUserGatherCount : (_MaxUserGatherCount = 100);
			set
			{
				this._MaxUserGatherCount = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public int MaxMessageGatherSize
		{
			get => _MaxMessageGatherSize > 0 ? _MaxMessageGatherSize : (_MaxMessageGatherSize = 500000);
			set
			{
				this._MaxMessageGatherSize = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public string Prefix
		{
			get => _Prefix ?? (_Prefix = DEFAULT_PREFIX);
			set
			{
				this._Prefix = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public string Game
		{
			get => _Game ?? (_Game = $"type \"{Prefix}help\" for help.");
			set
			{
				this._Game = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public string Stream
		{
			get => _Stream;
			set
			{
				this._Stream = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public bool AlwaysDownloadUsers
		{
			get => _AlwaysDownloadUsers;
			set
			{
				this._AlwaysDownloadUsers = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public LogSeverity LogLevel
		{
			get => _LogLevel;
			set
			{
				this._LogLevel = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public bool Pause { get; private set; }
	}
}
