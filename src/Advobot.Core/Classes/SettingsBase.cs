using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Advobot.Classes.Attributes;
using Advobot.Enums;
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
	public abstract class SettingsBase : ISettingsBase, INotifyPropertyChanged
	{
		private ImmutableDictionary<string, PropertyInfo> _Settings;

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <inheritdoc />
		public virtual IReadOnlyDictionary<string, PropertyInfo> GetSettings()
			=> _Settings ?? (_Settings = GetSettings(GetType()));
		/// <summary>
		/// Gets settings from a type statically.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ImmutableDictionary<string, PropertyInfo> GetSettings(Type type)
		{
			return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(x => x.GetCustomAttribute<SettingAttribute>() != null)
				.ToDictionary(x => x.Name, x => x)
				.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
		}
		/// <inheritdoc />
		public virtual string ToString(BaseSocketClient client, SocketGuild guild)
		{
			var sb = new StringBuilder();
			foreach (var kvp in GetSettings())
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
		public virtual string FormatSetting(BaseSocketClient client, SocketGuild guild, string name)
			=> Format(client, guild, GetSettings()[name].GetValue(this));
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
		/// Sets the property to the specified value.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private object SetSetting(PropertyInfo property, object value)
		{
			property.SetValue(this, Convert.ChangeType(value, property.PropertyType));
			RaisePropertyChanged(property.Name);
			return property.GetValue(this);
		}
		/// <summary>
		/// Sets the property to the specified default value.
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		private object ResetSetting(PropertyInfo property)
		{
			var settingAttr = property.GetCustomAttribute<SettingAttribute>();
			if (settingAttr.NonCompileTimeDefaultValue != default)
			{
				object nonCompileTimeValue;
				switch (settingAttr.NonCompileTimeDefaultValue)
				{
					case NonCompileTimeDefaultValue.Default:
						nonCompileTimeValue = Activator.CreateInstance(property.PropertyType);
						break;
					case NonCompileTimeDefaultValue.ResetDictionaryValues:
						var dict = (IDictionary)property.GetValue(this);
						dict.Keys.Cast<object>().ToList().ForEach(x => dict[x] = null);
						return dict;
					default:
						throw new InvalidOperationException("Invalid non compile time default value provided.");
				}
				property.SetValue(this, nonCompileTimeValue);
			}
			else
			{
				property.SetValue(this, settingAttr.DefaultValue);
			}
			RaisePropertyChanged(property.Name);
			return property.GetValue(this);
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