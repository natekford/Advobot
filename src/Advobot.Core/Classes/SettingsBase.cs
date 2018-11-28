using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesSettingParser;
using AdvorangesSettingParser.Implementation.Instance;
using AdvorangesSettingParser.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes
{
	/// <summary>
	/// Abstract class for settings.
	/// </summary>
	public abstract class SettingsBase : ISettingsBase
	{
		/// <inheritdoc />
		[JsonIgnore]
		public SettingParser SettingParser { get; } = new SettingParser();

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <inheritdoc />
		public virtual string ToString(BaseSocketClient client, SocketGuild guild)
		{
			var sb = new StringBuilder();
			foreach (var setting in SettingParser)
			{
				var formatted = Format(client, guild, setting.GetValue());
				if (string.IsNullOrWhiteSpace(formatted))
				{
					continue;
				}

				sb.AppendLineFeed($"**{setting.MainName.FormatTitle()}**:");
				sb.AppendLineFeed($"{formatted}");
				sb.AppendLineFeed();
			}
			return sb.ToString();
		}
		/// <inheritdoc />
		public virtual string FormatSetting(BaseSocketClient client, SocketGuild guild, string name)
			=> Format(client, guild, SettingParser.GetSetting(name, PrefixState.NotPrefixed).GetValue());
		/// <inheritdoc />
		public virtual string FormatValue(BaseSocketClient client, SocketGuild guild, object value)
			=> Format(client, guild, value);
		/// <inheritdoc />
		public virtual void ResetSettings()
		{
			foreach (var setting in SettingParser)
			{
				setting.ResetValue();
			}
		}
		/// <inheritdoc />
		public virtual void ResetSetting(string name)
			=> ResetSetting(SettingParser.GetSetting(name, PrefixState.NotPrefixed));
		/// <inheritdoc />
		public virtual void SetSetting<T>(string name, T value)
			=> SetSetting(SettingParser.GetSetting(name, PrefixState.NotPrefixed), value);
		/// <inheritdoc />
		public virtual void SaveSettings(IBotDirectoryAccessor accessor)
			=> IOUtils.SafeWriteAllText(GetFile(accessor), IOUtils.Serialize(this));
		/// <inheritdoc />
		public abstract FileInfo GetFile(IBotDirectoryAccessor accessor);
		/// <inheritdoc />
		public void RaisePropertyChanged([CallerMemberName] string name = "")
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
		/// Sets the property to the specified value.
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private object SetSetting(ISetting setting, object value)
		{
			setting.SetValue(value);
			RaisePropertyChanged(setting.MainName);
			return setting.GetValue();
		}
		/// <summary>
		/// Sets the property to the specified default value.
		/// </summary>
		/// <param name="setting"></param>
		/// <returns></returns>
		private object ResetSetting(ISetting setting)
		{
			setting.ResetValue();
			RaisePropertyChanged(setting.MainName);
			return setting.GetValue();
		}
		/// <summary>
		/// Recursive function for formatting objects.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private string Format(BaseSocketClient client, SocketGuild guild, object value)
		{
			switch (value)
			{
				case MemberInfo member:
					throw new InvalidOperationException("MemberInfo should not be passed directly into here.");
				case null:
					return "`Nothing`";
				case ulong id:
				{
					if (guild != null)
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
					}
					if (client != null)
					{
						if (client.GetUser(id) is IUser u)
						{
							return $"`{u.Format()}`";
						}
						if (client.GetGuild(id) is IGuild g)
						{
							return $"`{g.Format()}`";
						}
					}
					return id.ToString();
				}
				case string str: //Strings are char[], so this case needs to be above ienumerable
					return string.IsNullOrWhiteSpace(str) ? "`Nothing`" : $"`{str}`";
				case IGuildFormattable setting:
					return setting.Format(guild);
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