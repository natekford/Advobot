using Advobot.Core.Interfaces;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Advobot.Core.Classes.Settings
{
	/// <summary>
	/// Holds the settings for commands on a guild in the bot.
	/// </summary>
	public class CommandSettings : IGuildSetting
	{
		[JsonProperty("CommandValues")]
		private Dictionary<string, bool> _CommandValues = new Dictionary<string, bool>();
		[JsonProperty("ChannelOverrides")]
		private Dictionary<ulong, Dictionary<string, bool>> _ChannelOverrides = new Dictionary<ulong, Dictionary<string, bool>>();
		[JsonProperty("RoleOverrides")]
		private Dictionary<ulong, Dictionary<string, bool>> _RoleOverrides = new Dictionary<ulong, Dictionary<string, bool>>();
		[JsonProperty("UserOverrides")]
		private Dictionary<ulong, Dictionary<string, bool>> _UserOverrides = new Dictionary<ulong, Dictionary<string, bool>>();
		[JsonIgnore]
		private Dictionary<CommandOverrideTarget, Dictionary<ulong, Dictionary<string, bool>>> _OverrideDict = new Dictionary<CommandOverrideTarget, Dictionary<ulong, Dictionary<string, bool>>>();

		public CommandSettings()
		{
			_OverrideDict.Add(CommandOverrideTarget.Channel, _ChannelOverrides);
			_OverrideDict.Add(CommandOverrideTarget.Role, _RoleOverrides);
			_OverrideDict.Add(CommandOverrideTarget.User, _UserOverrides);

			OnDeserialized(new StreamingContext());
		}

		/// <summary>
		/// Changes the value for whether or not the commands are enabled on a guild.
		/// </summary>
		/// <param name="helpEntries">The commands to change.</param>
		/// <param name="enable">The value to give the commands.</param>
		/// <returns>The names of the commands which were successfully changed.</returns>
		public string[] ModifyCommandValues(IEnumerable<HelpEntry> helpEntries, bool enable)
		{
			var names = new List<string>();
			foreach (var helpEntry in helpEntries)
			{
				if (ModifyCommandValue(helpEntry, enable))
				{
					names.Add(helpEntry.Name);
				}
			}
			return names.ToArray();
		}
		/// <summary>
		/// Changes the values for whether or not a command is enabled on a guild.
		/// </summary>
		/// <param name="helpEntry">The command to change.</param>
		/// <param name="enable">The value to give the command.</param>
		/// <returns>Whether or not the method was successful. Failure indicates an untoggleable command or the command was already set to the passed in value.</returns>
		public bool ModifyCommandValue(HelpEntry helpEntry, bool enable)
		{
			return ModifyCommand(_CommandValues, helpEntry, enable);
		}
		/// <summary>
		/// Enabled/disables/removes overrides on specified commands for a specified object. Object can be channel, role, or user.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="helpEntries">The commands to override.</param>
		/// <param name="enable">The value to give them. null means the values will be removed.</param>
		/// <param name="target">The type of object that is being targetted.</param>
		/// <param name="obj">The object to target.</param>
		/// <returns>The names of the commands which were successfully changed.</returns>
		public string[] ModifyOverrides<T>(IEnumerable<HelpEntry> helpEntries, bool? enable, CommandOverrideTarget target, T obj) where T : ISnowflakeEntity
		{
			var names = new List<string>();
			foreach (var helpEntry in helpEntries)
			{
				if (ModifyOverride(helpEntry, enable, target, obj))
				{
					names.Add(helpEntry.Name);
				}
			}
			return names.ToArray();
		}
		/// <summary>
		/// Enables/disables/removes an override on a specified command for a specified object. Object can be channel, role, or user.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="helpEntry">The command to override.</param>
		/// <param name="enable">The value to give it. null means the value will be removed.</param>
		/// <param name="target">The type of object that is being targetted.</param>
		/// <param name="obj">The object to target.</param>
		/// <returns>Whether or not the method was successful. Failure indicates an untoggleable command or the command was already set to the passed in value.</returns>
		public bool ModifyOverride<T>(HelpEntry helpEntry, bool? enable, CommandOverrideTarget target, T obj) where T : ISnowflakeEntity
		{
			var outerDict = _OverrideDict[target];
			var innerDict = outerDict.TryGetValue(obj.Id, out var inner) ? inner : outerDict[obj.Id] = new Dictionary<string, bool>();
			return ModifyCommand(innerDict, helpEntry, enable);
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

			var name = Constants.HelpEntries[command.Aliases[0].Split(' ')[0]].Name;
			if (_UserOverrides.TryGetValue(context.User.Id, out var uDict) && uDict.TryGetValue(name, out var uValue))
			{
				return uValue;
			}

			foreach (var role in ((SocketGuildUser)context.User).Roles.OrderByDescending(x => x.Position))
			{
				if (_RoleOverrides.TryGetValue(role.Id, out var rDict) && rDict.TryGetValue(name, out var rValue))
				{
					return rValue;
				}
			}

			if (_ChannelOverrides.TryGetValue(context.Channel.Id, out var cDict) && cDict.TryGetValue(name, out var cValue))
			{
				return cValue;
			}

			return _CommandValues[name];
		}

		private static bool ModifyCommand(IDictionary<string, bool> dict, HelpEntry helpEntry, bool? enable)
		{
			if (!helpEntry.AbleToBeTurnedOff)
			{
				return false;
			}
			if (enable == null)
			{
				if (!dict.ContainsKey(helpEntry.Name))
				{
					return false;
				}
				dict.Remove(helpEntry.Name);
				return true;
			}
			if (dict.TryGetValue(helpEntry.Name, out var value) && value == enable)
			{
				return false;
			}

			dict[helpEntry.Name] = enable.Value;
			return true;
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			//Add in the default values for commands that aren't set
			foreach (var helpEntry in Constants.HelpEntries.GetUnsetCommands(_CommandValues.Keys))
			{
				_CommandValues.Add(helpEntry.Name, helpEntry.DefaultEnabled);
			}
			//Remove all that aren't commands anymore
			ClearInvalidValues(_CommandValues);
			ClearInvalidValues(_UserOverrides);
			ClearInvalidValues(_RoleOverrides);
			ClearInvalidValues(_ChannelOverrides);
		}
		private static void ClearInvalidValues(Dictionary<ulong, Dictionary<string, bool>> dict)
		{
			foreach (var outerKey in dict.Keys)
			{
				//If there are no valid commands then remove the dict
				ClearInvalidValues(dict[outerKey]);
				if (!dict[outerKey].Any())
				{
					dict.Remove(outerKey);
				}
			}
		}
		private static void ClearInvalidValues(Dictionary<string, bool> dict)
		{
			foreach (var key in dict.Keys)
			{
				if (Constants.HelpEntries[key] == null)
				{
					dict.Remove(key);
				}
			}
		}

		public override string ToString()
		{
			return ToString(null);
		}
		public string ToString(SocketGuild guild)
		{
			return $"{String.Join("\n", _CommandValues.Select(x => $"`{x.Key}:` `{x.Value}`"))}\n\n" +
				$"{ToString(_ChannelOverrides, "Channel", guild)}\n" +
				$"{ToString(_RoleOverrides, "Role", guild)}\n" +
				$"{ToString(_UserOverrides, "User", guild)}".TrimEnd();
		}
		private static string ToString(Dictionary<ulong, Dictionary<string, bool>> dict, string type, SocketGuild guild = null)
		{
			var sb = new StringBuilder();
			foreach (var kvp in dict)
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
					title = $"**{type}:** `{kvp.Key}`";
				}

				var overrides = "";
				var enabledKvps = kvp.Value.Where(x => x.Value);
				if (enabledKvps.Any())
				{
					overrides += $"\t**Enabled:** `{String.Join("`, `", kvp.Value)}`\n";
				}
				var disabledKvps = kvp.Value.Where(x => !x.Value);
				if (disabledKvps.Any())
				{
					overrides += $"\t**Disabled:** `{String.Join("`, `", kvp.Value)}`\n";
				}

				if (!String.IsNullOrWhiteSpace(overrides))
				{
					sb.AppendLine($"{title}\n{overrides}");
				}
			}
			return sb.ToString();
		}
	}

	public enum CommandOverrideTarget
	{
		Channel,
		Role,
		User
	}
}
