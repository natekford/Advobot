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

namespace Advobot.Classes
{
	/// <summary>
	/// Abstract class for settings.
	/// </summary>
	public abstract class SettingsBase : ISettingsBase, INotifyPropertyChanged
	{
		private ImmutableDictionary<string, PropertyInfo> _S;

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <inheritdoc />
		public virtual IReadOnlyDictionary<string, PropertyInfo> GetSettings()
			=> _S ?? (_S = GetSettings(GetType()));
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
		public virtual string ToString(BaseSocketClient client, SocketGuild guild, string name)
			=> Format(client, guild, GetProperty(name).GetValue(this));
		/// <inheritdoc />
		public virtual void ResetSettings()
		{
			foreach (var field in GetSettings())
			{
				ResetSetting(field.Value);
			}
		}
		/// <inheritdoc />
		public virtual void ResetSetting(string name)
		{
			var property = GetProperty(name);
			ResetSetting(property);
			NotifyPropertyChanged(property.Name);
		}
		/// <inheritdoc />
		public virtual void SetSetting(string name, object value)
		{
			var property = GetProperty(name);
			property.SetValue(this, Convert.ChangeType(value, property.PropertyType));
			NotifyPropertyChanged(property.Name);
		}
		/// <inheritdoc />
		public virtual void ModifyList(string name, object value, bool add, bool allowDuplicates = false)
		{
			var property = GetProperty(name);
			var list = (IList)property.GetValue(this);
			if (!add)
			{
				list.Remove(value);
			}
			else if (allowDuplicates || !list.Contains(value))
			{
				list.Add(value);
			}
			NotifyPropertyChanged(property.Name);
		}
		/// <inheritdoc />
		public virtual void SaveSettings(IBotDirectoryAccessor accessor)
			=> IOUtils.SafeWriteAllText(GetFile(accessor), IOUtils.Serialize(this));
		/// <inheritdoc />
		public abstract FileInfo GetFile(IBotDirectoryAccessor accessor);
		/// <summary>
		/// Fires the property changed event.
		/// </summary>
		/// <param name="name"></param>
		protected void NotifyPropertyChanged([CallerMemberName] string name = "")
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		/// <summary>
		/// Gets the property with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private PropertyInfo GetProperty(string name)
			=> GetSettings()[name] ?? throw new ArgumentException($"Invalid property name provided: {name}.", nameof(name));
		/// <summary>
		/// Sets the property to the specified default vaule.
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
				return property.GetValue(this);
			}
			else
			{
				property.SetValue(this, settingAttr.DefaultValue);
				return property.GetValue(this);
			}
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