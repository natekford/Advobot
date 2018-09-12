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
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Classes
{
	/// <summary>
	/// Abstract class for settings.
	/// </summary>
	public abstract class SettingsBase : ISettingsBase, INotifyPropertyChanged
	{
		/// <summary>
		/// Parses settings, but in this use case mainly doesn't handle direct strings.
		/// </summary>
		protected SettingParser Parser = new SettingParser();

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <inheritdoc />
		public virtual IReadOnlyDictionary<string, ICompleteSetting> GetSettings()
			=> Parser.GetSettings().ToDictionary(x => x.MainName, x => (ICompleteSetting)x, StringComparer.OrdinalIgnoreCase);
		/// <inheritdoc />
		public virtual string ToString(BaseSocketClient client, SocketGuild guild)
		{
			var sb = new StringBuilder();
			foreach (var kvp in GetSettings())
			{
				var formatted = Format(client, guild, kvp.Value.GetValue());
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
		public virtual string FormatSetting(BaseSocketClient client, SocketGuild guild, string name)
			=> Format(client, guild, GetSettings()[name].GetValue());
		/// <inheritdoc />
		public virtual string FormatValue(BaseSocketClient client, SocketGuild guild, object value)
			=> Format(client, guild, value);
		/// <inheritdoc />
		public virtual void ResetSettings()
		{
			foreach (var kvp in GetSettings())
			{
				ResetSetting(kvp.Value);
			}
		}
		/// <inheritdoc />
		public virtual void ResetSetting(string name)
			=> ResetSetting(GetSettings()[name]);
		/// <inheritdoc />
		public virtual void SetSetting<T>(string name, T value)
			=> SetSetting(GetSettings()[name], value);
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
		/// Adds the setting.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="selector"></param>
		/// <param name="reset"></param>
		/// <param name="parser"></param>
		protected void RegisterSetting<T>(Expression<Func<T>> selector, Func<T, T> reset, TryParseDelegate<T> parser = default)
			=> Parser.Add(new Setting<T>(selector, parser: parser) { ResetValueFactory = reset, });
		/// <summary>
		/// Clears the list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="x"></param>
		/// <returns></returns>
		protected IList<T> ClearList<T>(IList<T> x)
		{
			x.Clear();
			return x;
		}
		/// <summary>
		/// Resets the values in a dictionary.
		/// </summary>
		/// <typeparam name="TK"></typeparam>
		/// <typeparam name="TV"></typeparam>
		/// <param name="x"></param>
		/// <returns></returns>
		protected IDictionary<TK, TV> ResetDictionary<TK, TV>(IDictionary<TK, TV> x)
		{
			x.Keys.ToList().ForEach(k => x[k] = default);
			return x;
		}
		/// <summary>
		/// Sets the property to the specified value.
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private object SetSetting(ICompleteSetting setting, object value)
		{
			setting.Set(value);
			RaisePropertyChanged(setting.MainName);
			return setting.GetValue();
		}
		/// <summary>
		/// Sets the property to the specified default value.
		/// </summary>
		/// <param name="setting"></param>
		/// <returns></returns>
		private object ResetSetting(ICompleteSetting setting)
		{
			setting.Reset();
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
				case IGuildSetting setting:
					return setting.ToString(guild);
				case IDictionary dict: //Has to be above IEnumerable too
					var keys = dict.Keys.Cast<object>().Where(x => dict[x] != null);
					return string.Join("\n", keys.Select(x => $"{Format(client, guild, x)}: {Format(client, guild, dict[x])}"));
				case IEnumerable enumerable:
					return string.Join("\n", enumerable.Cast<object>().Select(x => Format(client, guild, x)));
				default:
					return $"`{value}`";
			}
		}
	}
}