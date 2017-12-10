using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Interfaces;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Settings
{
	/// <summary>
	/// Holds settings for the bot. Settings are saved through property setters or calling <see cref="SaveSettings()"/>.
	/// </summary>
	public class BotSettings : IBotSettings, INotifyPropertyChanged
	{
		private static FileInfo LOC => GetActions.GetBaseBotDirectoryFile(Constants.BOT_SETTINGS_LOCATION);
		private const string MY_BOT_PREFIX = "&&";

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
			get => _Prefix ?? (_Prefix = MY_BOT_PREFIX);
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

		public BotSettings()
		{
			PropertyChanged += this.SaveSettings;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged([CallerMemberName] string propertyName = "")
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		private void SaveSettings(object sender, PropertyChangedEventArgs e)
		{
			ConsoleActions.WriteLine($"Successfully saved: {e.PropertyName}");
			SaveSettings();
		}
		public void SaveSettings()
			=> IOActions.OverWriteFile(LOC, IOActions.Serialize(this));
		public void TogglePause() => this.Pause = !this.Pause;

		public async Task<string> Format(IDiscordClient client)
		{
			var sb = new StringBuilder();
			foreach (var property in this.GetType().GetProperties())
			{
				//Only get public editable properties
				if (property.GetGetMethod() == null || property.GetSetMethod() == null)
				{
					continue;
				}

				var formatted = await Format(client, property).CAF();
				if (String.IsNullOrWhiteSpace(formatted))
				{
					continue;
				}

				sb.AppendLineFeed($"**{property.Name}**:");
				sb.AppendLineFeed($"{formatted}");
				sb.AppendLineFeed("");
			}
			return sb.ToString();
		}
		public async Task<string> Format(IDiscordClient client, PropertyInfo property)
			=> await FormatObjectAsync(client, property.GetValue(this)).CAF();
		private async Task<string> FormatObjectAsync(IDiscordClient client, object value)
		{
			if (value == null)
			{
				return "`Nothing`";
			}
			else if (value is ulong tempUlong)
			{
				var user = await client.GetUserAsync(tempUlong).CAF();
				if (user != null)
				{
					return $"`{user.FormatUser()}`";
				}

				var guild = await client.GetGuildAsync(tempUlong).CAF();
				if (guild != null)
				{
					return $"`{guild.FormatGuild()}`";
				}

				return tempUlong.ToString();
			}
			//Because strings are char[] this pointless else if has to be here so it doesn't go into the else if directly below
			else if (value is string tempStr)
			{
				return String.IsNullOrWhiteSpace(tempStr) ? "`Nothing`" : $"`{tempStr}`";
			}
			else if (value is IEnumerable tempIEnumerable)
			{
				var text = await Task.WhenAll(tempIEnumerable.Cast<object>().Select(async x => await FormatObjectAsync(client, x).CAF()));
				return String.Join("\n", text);
			}
			else
			{
				return $"`{value.ToString()}`";
			}
		}
	}
}
