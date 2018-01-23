using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Newtonsoft.Json;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Holds settings for the bot. Settings are saved through property setters or calling <see cref="SaveSettings()"/>.
	/// </summary>
	public sealed class AdvobotBotSettings : IBotSettings, INotifyPropertyChanged
	{
		#region Fields and Properties
		[JsonProperty("TrustedUsers")]
		private List<ulong> _TrustedUsers;
		[JsonProperty("UsersUnableToDmOwner")]
		private List<ulong> _UsersUnableToDmOwner;
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
				_TrustedUsers = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<ulong> UsersUnableToDmOwner
		{
			get => _UsersUnableToDmOwner.AsReadOnly() ?? (_UsersUnableToDmOwner = new List<ulong>()).AsReadOnly();
			set
			{
				_UsersUnableToDmOwner = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<ulong> UsersIgnoredFromCommands
		{
			get => _UsersIgnoredFromCommands.AsReadOnly() ?? (_UsersIgnoredFromCommands = new List<ulong>()).AsReadOnly();
			set
			{
				_UsersIgnoredFromCommands = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public int ShardCount
		{
			get => _ShardCount > 1 ? _ShardCount : (_ShardCount = 1);
			set
			{
				_ShardCount = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public int MessageCacheCount
		{
			get => _MessageCacheCount > 0 ? _MessageCacheCount : (_MessageCacheCount = 1000);
			set
			{
				_MessageCacheCount = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public int MaxUserGatherCount
		{
			get => _MaxUserGatherCount > 0 ? _MaxUserGatherCount : (_MaxUserGatherCount = 100);
			set
			{
				_MaxUserGatherCount = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public int MaxMessageGatherSize
		{
			get => _MaxMessageGatherSize > 0 ? _MaxMessageGatherSize : (_MaxMessageGatherSize = 500000);
			set
			{
				_MaxMessageGatherSize = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public string Prefix
		{
			get => _Prefix ?? (_Prefix = Constants.DEFAULT_PREFIX);
			set
			{
				_Prefix = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public string Game
		{
			get => _Game ?? (_Game = $"type \"{Prefix}help\" for help.");
			set
			{
				_Game = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public string Stream
		{
			get => _Stream;
			set
			{
				_Stream = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public bool AlwaysDownloadUsers
		{
			get => _AlwaysDownloadUsers;
			set
			{
				_AlwaysDownloadUsers = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public LogSeverity LogLevel
		{
			get => _LogLevel;
			set
			{
				_LogLevel = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public bool Pause { get; set; }
		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		public AdvobotBotSettings()
		{
			PropertyChanged += SaveSettings;
		}

		public async Task<string> FormatAsync(IDiscordClient client)
		{
			var sb = new StringBuilder();
			foreach (var property in GetType().GetProperties())
			{
				//Only get public editable properties
				if (property.GetGetMethod() == null || property.GetSetMethod() == null)
				{
					continue;
				}

				var formatted = await FormatAsync(client, property).CAF();
				if (String.IsNullOrWhiteSpace(formatted))
				{
					continue;
				}

				sb.AppendLineFeed($"**{property.Name}**:");
				sb.AppendLineFeed($"{formatted}");
				sb.AppendLineFeed();
			}
			return sb.ToString();
		}
		public async Task<string> FormatAsync(IDiscordClient client, PropertyInfo property)
		{
			return await FormatObjectAsync(client, property.GetValue(this)).CAF();
		}
		public void SaveSettings()
		{
			IOUtils.OverwriteFile(IOUtils.GetBaseBotDirectoryFile(Constants.BOT_SETTINGS_LOC), IOUtils.Serialize(this));
		}

		private async Task<string> FormatObjectAsync(IDiscordClient client, object value)
		{
			if (value == null)
			{
				return "`Nothing`";
			}

			if (value is ulong ul)
			{
				var user = await client.GetUserAsync(ul).CAF();
				if (user != null)
				{
					return $"`{user.Format()}`";
				}
				var guild = await client.GetGuildAsync(ul).CAF();
				if (guild != null)
				{
					return $"`{guild.Format()}`";
				}
				return ul.ToString();
			}
			//Because strings are char[] this pointless else if has to be here so it doesn't go into the else if directly below

			if (value is string str)
			{
				return String.IsNullOrWhiteSpace(str) ? "`Nothing`" : $"`{str}`";
			}

			if (value is IEnumerable enumerable)
			{
				var text = await Task.WhenAll(enumerable.Cast<object>().Select(async x => await FormatObjectAsync(client, x).CAF()));
				return String.Join("\n", text);
			}
			return $"`{value}`";
		}
		private void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		private void SaveSettings(object sender, PropertyChangedEventArgs e)
		{
			ConsoleUtils.WriteLine($"Successfully saved: {e.PropertyName}");
			SaveSettings();
		}
	}
}
