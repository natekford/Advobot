using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Interfaces;
using Discord;
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
	public partial class BotSettings : IBotSettings, INotifyPropertyChanged
	{
		private const string DEFAULT_PREFIX = "&&";

		public BotSettings()
		{
			PropertyChanged += SaveSettings;
		}

		/// <summary>
		/// Returns all public properties that have a set method. Will not return SavePath and BotKey since those
		/// are saved via <see cref="Properties.Settings.Default"/>.
		/// </summary>
		/// <returns></returns>
		public static PropertyInfo[] GetSettings()
		{
			return typeof(IBotSettings)
.GetProperties(BindingFlags.Public | BindingFlags.Instance)
.Where(x => x.CanWrite && x.GetSetMethod(true).IsPublic).ToArray();
		}

		/// <summary>
		/// Returns the values of <see cref="GetBotSettings"/> which either are strings or do not implement the generic IEnumerable.
		/// </summary>
		/// <returns></returns>
		public static PropertyInfo[] GetNonEnumerableSettings()
		{
			return GetSettings()
.Where(x =>
{
return x.PropertyType == typeof(string)
|| !x.PropertyType.GetInterfaces().Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(IEnumerable<>));
}).ToArray();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void SaveSettings(object sender, PropertyChangedEventArgs e)
		{
			ConsoleUtils.WriteLine($"Successfully saved: {e.PropertyName}");
			SaveSettings();
		}
		public void SaveSettings()
		{
			IOUtils.OverWriteFile(IOUtils.GetBaseBotDirectoryFile(Constants.BOT_SETTINGS_LOC), IOUtils.Serialize(this));
		}

		public void TogglePause()
		{
			Pause = !Pause;
		}

		public int GetMaxAmountOfUsersToGather(bool bypass)
		{
			return bypass ? int.MaxValue : MaxUserGatherCount;
		}

		public async Task<string> Format(IDiscordClient client)
		{
			var sb = new StringBuilder();
			foreach (var property in GetType().GetProperties())
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
		{
			return await FormatObjectAsync(client, property.GetValue(this)).CAF();
		}

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
					return $"`{user.Format()}`";
				}

				var guild = await client.GetGuildAsync(tempUlong).CAF();
				if (guild != null)
				{
					return $"`{guild.Format()}`";
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
