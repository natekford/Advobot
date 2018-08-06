using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
	public abstract class SettingsBase : ISettingsBase
	{
		/// <inheritdoc />
		public abstract string FileName { get; }

		private Dictionary<string, PropertyInfo> _Settings;

		/// <inheritdoc />
		public virtual IReadOnlyDictionary<string, PropertyInfo> GetSettings()
		{
			if (_Settings != null)
			{
				return _Settings;
			}

			return _Settings = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(x => x.GetCustomAttribute<SettingAttribute>() != null)
				.ToDictionary(x => x.Name.Trim('_'), x => x, StringComparer.OrdinalIgnoreCase);
		}
		/// <inheritdoc />
		public virtual string ToString(DiscordShardedClient client, SocketGuild guild)
		{
			var sb = new StringBuilder();
			foreach (var kvp in GetSettings())
			{
				var formatted = Format(client, guild, kvp.Value.GetValue(this));
				if (String.IsNullOrWhiteSpace(formatted))
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
		public virtual string ToString(DiscordShardedClient client, SocketGuild guild, string name)
		{
			return Format(client, guild, GetMember(name).GetValue(this));
		}
		/// <inheritdoc />
		public virtual void ResetSettings()
		{
			foreach (var field in GetSettings())
			{
				ResetSetting(field.Value);
			}
		}
		/// <inheritdoc />
		public virtual object ResetSetting(string name)
		{
			return ResetSetting(GetMember(name));
		}
		/// <inheritdoc />
		public virtual void SaveSettings(ILowLevelConfig config)
		{
			FileUtils.SafeWriteAllText(config.GetBaseBotDirectoryFile(FileName), IOUtils.Serialize(this));
		}

		private PropertyInfo GetMember(string name)
		{
			return GetSettings()[name] ?? throw new ArgumentException($"Invalid member name provided: {name}.", nameof(name));
		}
		private object ResetSetting(PropertyInfo property)
		{
			var settingAttr = property.GetCustomAttribute<SettingAttribute>();
			if (settingAttr.NonCompileTimeDefaultValue != default)
			{
				object nonCompileTimeValue;
				switch (settingAttr.NonCompileTimeDefaultValue)
				{
					case NonCompileTimeDefaultValue.InstantiateDefaultParameterless:
						nonCompileTimeValue = Activator.CreateInstance(property.GetUnderlyingType());
						break;
					case NonCompileTimeDefaultValue.ClearDictionaryValues:
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
		private string Format(DiscordShardedClient client, SocketGuild guild, object value)
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
					return String.IsNullOrWhiteSpace(str) ? "`Nothing`" : $"`{str}`";
				case IGuildSetting setting:
					return setting.ToString(guild);
				case IDictionary dict: //Has to be above IEnumerable too
					var keys = dict.Keys.Cast<object>().Where(x => dict[x] != null);
					return String.Join("\n", keys.Select(x => $"{Format(client, guild, x)}: {Format(client, guild, dict[x])}"));
				case IEnumerable enumerable:
					return String.Join("\n", enumerable.Cast<object>().Select(x => Format(client, guild, x)));
				default:
					return $"`{value}`";
			}
		}
	}
}