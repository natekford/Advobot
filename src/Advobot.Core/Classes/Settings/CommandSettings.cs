using System.Collections.Generic;
using System.Linq;
using System.Text;
using Advobot.Classes.Attributes;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Holds the settings for commands on a guild in the bot.
	/// </summary>
	public class CommandSettings : IGuildSetting
	{
		[JsonProperty("CommandValues")]
		private Dictionary<string, bool> _CommandValues = new Dictionary<string, bool>();
		[JsonProperty("Overrides")]
		private Dictionary<ulong, Dictionary<string, bool>> _Overrides = new Dictionary<ulong, Dictionary<string, bool>>();

		/// <summary>
		/// Changes the value for whether or not the commands are enabled on a guild.
		/// </summary>
		/// <param name="values">The commands to change.</param>
		/// <returns>The names of the commands which were successfully changed.</returns>
		public string[] ModifyCommandValues(IEnumerable<ValueToModify> values)
		{
			var changed = new List<string>();
			foreach (var value in values)
			{
				if (ModifyCommandValue(value))
				{
					changed.Add(value.Name);
				}
			}
			return changed.ToArray();
		}
		/// <summary>
		/// Changes the values for whether or not a command is enabled on a guild.
		/// </summary>
		/// <param name="value">The command to change.</param>
		/// <returns>Whether or not the method was successful. Failure indicates an untoggleable command or the command was already set to the passed in value.</returns>
		public bool ModifyCommandValue(ValueToModify value)
		{
			return ModifyOverride(_CommandValues, value);
		}
		/// <summary>
		/// Enabled/disables/removes overrides on specified commands for a specified object. Object can be channel, role, or user.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="values">The commands to override.</param>
		/// <param name="obj">The object to target.</param>
		/// <returns>The names of the commands which were successfully changed.</returns>
		public string[] ModifyOverrides<T>(IEnumerable<ValueToModify> values, T obj) where T : ISnowflakeEntity
		{
			var changed = new List<string>();
			foreach (var value in values)
			{
				if (ModifyOverride(value, obj))
				{
					changed.Add(value.Name);
				}
			}
			return changed.ToArray();
		}
		/// <summary>
		/// Enables/disables/removes an override on a specified command for a specified object. Object can be channel, role, or user.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value">The command to override.</param>
		/// <param name="obj">The object to target.</param>
		/// <returns>Whether or not the method was successful. Failure indicates an untoggleable command or the command was already set to the passed in value.</returns>
		public bool ModifyOverride<T>(ValueToModify value, T obj) where T : ISnowflakeEntity
		{
			var innerDict = _Overrides.TryGetValue(obj.Id, out var inner) ? inner : _Overrides[obj.Id] = new Dictionary<string, bool>();
			return ModifyOverride(innerDict, value);
		}
		/// <summary>
		/// Returns a value indicating whether or not the command is enabled in the current context.
		/// Checks user, then roles ordered by descending hierarchy, then channel, then finally the default guild setting.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <returns></returns>
		public bool IsCommandEnabled(ICommandContext context, CommandInfo command)
		{
			//Hierarchy:
			//User
			//Role -> Ordered by position
			//Channel
			//Guild

			var topModule = command.Module;
			while (topModule.Parent != null)
			{
				topModule = topModule.Parent;
			}
			var name = topModule.Name;
			if (_Overrides.TryGetValue(context.User.Id, out var uDict) && uDict.TryGetValue(name, out var uValue))
			{
				return uValue;
			}
			foreach (var role in ((SocketGuildUser)context.User).Roles.OrderByDescending(x => x.Position))
			{
				if (_Overrides.TryGetValue(role.Id, out var rDict) && rDict.TryGetValue(name, out var rValue))
				{
					return rValue;
				}
			}
			if (_Overrides.TryGetValue(context.Channel.Id, out var cDict) && cDict.TryGetValue(name, out var cValue))
			{
				return cValue;
			}
			if (_CommandValues.TryGetValue(name, out var value))
			{
				return value;
			}

			//If they get here it means they're not in the command values currently so they should just use the default value.
			var defaultEnabled = topModule.Attributes.GetAttribute<DefaultEnabledAttribute>().Enabled;
			_CommandValues.Add(name, defaultEnabled);
			return defaultEnabled;
		}
		/// <summary>
		/// Checks whether the command is enabled on the guild.
		/// Returns true if set to true, returns false it set to false, returns null if not set.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool? IsCommandEnabled(string name)
		{
			return _CommandValues.TryGetValue(name, out var val) ? val : (bool?)null;
		}
		/// <inheritdoc />
		public override string ToString()
		{
			return ToString(null);
		}
		/// <inheritdoc />
		public string ToString(SocketGuild guild)
		{
			var sb = new StringBuilder();
			foreach (var kvp in _Overrides)
			{
				string title;
				if (guild?.GetChannel(kvp.Key) is IGuildChannel channel)
				{
					title = $"**Channel:** `{channel.Format()}`";
				}
				else if (guild?.GetRole(kvp.Key) is IRole role)
				{
					title = $"**Role:** `{role.Format()}`";
				}
				else if (guild?.GetUser(kvp.Key) is IUser user)
				{
					title = $"**User:** `{user.Format()}`";
				}
				else
				{
					title = $"**Unspecified:** `{kvp.Key}`";
				}

				var overrides = "";
				var enabledKvps = kvp.Value.Where(x => x.Value);
				if (enabledKvps.Any())
				{
					overrides += $"\t**Enabled:** `{string.Join("`, `", kvp.Value)}`\n";
				}
				var disabledKvps = kvp.Value.Where(x => !x.Value);
				if (disabledKvps.Any())
				{
					overrides += $"\t**Disabled:** `{string.Join("`, `", kvp.Value)}`\n";
				}

				if (!string.IsNullOrWhiteSpace(overrides))
				{
					sb.AppendLine($"{title}\n{overrides}");
				}
			}
			return $"{string.Join("\n", _CommandValues.Select(x => $"`{x.Key}:` `{x.Value}`"))}\n\n{sb}".TrimEnd();
		}
		private static bool ModifyOverride(IDictionary<string, bool> dict, ValueToModify newValue)
		{
			if (!newValue.CanModify)
			{
				return false;
			}
			if (newValue.Value == null)
			{
				if (!dict.ContainsKey(newValue.Name))
				{
					return false;
				}
				dict.Remove(newValue.Name);
				return true;
			}
			if (dict.TryGetValue(newValue.Name, out var currentValue) && currentValue == newValue.Value)
			{
				return false;
			}
			dict[newValue.Name] = (bool)newValue.Value;
			return true;
		}
	}
}
