using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes
{
	/// <summary>
	/// Abstract class for settings.
	/// </summary>
	internal abstract class SettingsBase : ISettingsBase
	{
		private Dictionary<string, PropertyInfo> _Settings { get; }

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		public SettingsBase()
		{
#warning make setting attribute?
			_Settings = GetType().GetProperties()
				.Where(x => x.GetCustomAttribute<JsonPropertyAttribute>() != null)
				.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);
		}

		/// <inheritdoc />
		public bool IsSetting(string name)
			=> _Settings.ContainsKey(name);
		/// <inheritdoc />
		public string[] GetSettingNames()
			=> _Settings.Keys.ToArray();
		/// <inheritdoc />
		public string Format(BaseSocketClient client, SocketGuild guild)
		{
			var sb = new StringBuilder();
			foreach (var kvp in _Settings)
			{
				var formatted = Format(client, guild, kvp.Value.GetValue(this));
				if (string.IsNullOrWhiteSpace(formatted))
				{
					continue;
				}

				sb.AppendLineFeed($"**{kvp.Key.FormatTitle()}**:");
				sb.AppendLineFeed($"{formatted}");
				sb.AppendLineFeed();
			}
			return sb.ToString();
		}
		/// <inheritdoc />
		public string FormatSetting(BaseSocketClient client, SocketGuild guild, string name)
			=> Format(client, guild, _Settings[name].GetValue(this));
		/// <inheritdoc />
		public string FormatValue(BaseSocketClient client, SocketGuild guild, object? value)
			=> Format(client, guild, value);
		/// <inheritdoc />
		public void Save(IBotDirectoryAccessor accessor)
			=> IOUtils.SafeWriteAllText(GetFile(accessor), IOUtils.Serialize(this));
		/// <inheritdoc />
		public abstract FileInfo GetFile(IBotDirectoryAccessor accessor);
		/// <inheritdoc />
		protected void RaisePropertyChanged([CallerMemberName] string caller = "")
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
		/// <summary>
		/// Sets the field and raises property changed.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="field"></param>
		/// <param name="value"></param>
		/// <param name="caller"></param>
		protected void SetValue<T>(ref T field, T value, [CallerMemberName] string caller = "")
		{
			field = value;
			RaisePropertyChanged(caller);
		}
		/// <summary>
		/// Throws an argument exception if the condition is true.
		/// </summary>
		/// <param name="backingField"></param>
		/// <param name="value"></param>
		/// <param name="condition"></param>
		/// <param name="msg"></param>
		/// <param name="caller"></param>
		protected void ThrowIfElseSet<T>(ref T backingField, T value, Func<T, bool> condition, string msg, [CallerMemberName] string caller = "")
		{
			if (condition(value))
			{
				throw new ArgumentException(msg, caller);
			}
			backingField = value;
			RaisePropertyChanged(caller);
		}
		/// <summary>
		/// Recursive function for formatting objects.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private string Format(BaseSocketClient client, SocketGuild guild, object? value)
		{
			switch (value)
			{
				case MemberInfo member:
					throw new InvalidOperationException("MemberInfo should not be passed directly into here.");
				case null:
					return "`Nothing`";
				case ulong id:
				{
					if (guild?.GetChannel(id) is IChannel c)
					{
						return $"`{c.Format()}`";
					}
					if (guild?.GetRole(id) is IRole r)
					{
						return $"`{r.Format()}`";
					}
					if (guild?.GetUser(id) is IUser u)
					{
						return $"`{u.Format()}`";
					}
					if (client?.GetUser(id) is IUser u2)
					{
						return $"`{u2.Format()}`";
					}
					if (client?.GetGuild(id) is IGuild g)
					{
						return $"`{g.Format()}`";
					}
					goto default;
				}
				case string str: //Strings are char[], so this case needs to be above ienumerable
					return string.IsNullOrWhiteSpace(str) ? "`Nothing`" : $"`{str}`";
				case IGuildFormattable formattable:
					return formattable.Format(guild);
				case IDictionary dict: //Has to be above IEnumerable too
					var keys = dict.Keys.Cast<object>().Where(x => dict[x] != null);
					return keys.Join("\n", x => $"{Format(client, guild, x)}: {Format(client, guild, dict[x])}");
				case IEnumerable enumerable:
					return enumerable.Cast<object>().Join("\n", x => Format(client, guild, x));
				default:
					return $"`{value}`";
			}
		}
	}
}