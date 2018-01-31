using Advobot.Core.Classes.Attributes;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Advobot.Core.Classes
{
	public abstract class SettingsBase : ISettingsBase
    {
		public abstract FileInfo FileLocation { get; }
		private Dictionary<string, FieldInfo> _Settings;

		/// <summary>
		/// Returns all non-public instance fields with <see cref="SettingAttribute"/>.
		/// </summary>
		/// <returns></returns>
		public virtual IReadOnlyDictionary<string, FieldInfo> GetSettings()
		{
			return _Settings ?? (_Settings = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(x => x.GetCustomAttribute<SettingAttribute>() != null)
				.ToDictionary(x => x.Name.Trim('_'), x => x, StringComparer.OrdinalIgnoreCase));
		}
		public virtual string Format(IDiscordClient client, IGuild guild)
		{
			var sb = new StringBuilder();
			foreach (var kvp in GetSettings())
			{
				var formatted = Format(client, guild, kvp.Value);
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
		public virtual string Format(IDiscordClient client, IGuild guild, FieldInfo field)
		{
			return Format(client, guild, field.GetValue(this));
		}
		public virtual string Format(IDiscordClient client, IGuild guild, string name)
		{
			return Format(client, guild, GetField(name));
		}
		public virtual void ResetSettings()
		{
			foreach (var field in GetSettings())
			{
				ResetSetting(field.Value);
			}
		}
		public virtual object ResetSetting(FieldInfo field)
		{
			var settingAttr = field.GetCustomAttribute<SettingAttribute>();
			if (settingAttr.NonCompileTime)
			{
				object nonCompileTimeValue;
				switch (settingAttr.NonCompileTimeDefaultValue)
				{
					case NonCompileTimeDefaultValue.InstantiateDefaultParameterless:
						nonCompileTimeValue = Activator.CreateInstance(field.FieldType);
						break;
					case NonCompileTimeDefaultValue.ClearDictionaryValues:
						var dict = (IDictionary)field.GetValue(this);
						dict.Keys.Cast<object>().ToList().ForEach(x => dict[x] = null);
						return dict;
					default:
						throw new InvalidOperationException("Invalid non compile time default value provided.");
				}
				field.SetValue(this, nonCompileTimeValue);
				return field.GetValue(this);
			}
			else
			{
				field.SetValue(this, settingAttr.DefaultValue);
				return field.GetValue(this);
			}
		}
		public virtual object ResetSetting(string name)
		{
			return ResetSetting(GetField(name));
		}
		public virtual void SaveSettings()
		{
			IOUtils.OverwriteFile(FileLocation, IOUtils.Serialize(this));
		}

		private FieldInfo GetField(string name)
		{
			return GetSettings()[name] ?? throw new ArgumentException("Invalid field name provided.", nameof(name));
		}
		private string Format(IDiscordClient client, IGuild guild, object value)
		{
			switch (value)
			{
				case null:
					return "`Nothing`";
				case ulong id:
				{
					if (guild is SocketGuild sg)
					{
						if (sg?.GetChannel(id) is IChannel c)
						{
							return $"`{c.Format()}`";
						}
						if (sg?.GetRole(id) is IRole r)
						{
							return $"`{r.Format()}`";
						}
						if (sg?.GetUser(id) is IUser u)
						{
							return $"`{u.Format()}`";
						}
					}
					if (client != null)
					{
						if (ClientUtils.GetUser(client, id) is IUser u)
						{
							return $"`{u.Format()}`";
						}
						if (ClientUtils.GetGuild(client, id) is IGuild g)
						{
							return $"`{g.Format()}`";
						}
					}
					return id.ToString();
				}
				case string str: //Strings are char[], so this case needs to be above ienumerable
					return String.IsNullOrWhiteSpace(str) ? "`Nothing`" : $"`{str}`";
				case IGuildSetting setting:
					return setting.ToString();
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