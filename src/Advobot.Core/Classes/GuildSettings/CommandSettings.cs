using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Advobot.Core.Classes.GuildSettings
{
	public class CommandSettings : IGuildSetting
	{
		[JsonProperty("DisabledCommands")]
		private List<string> _DisabledCommands = new List<string>();
		[JsonProperty("ChannelOverrides")]
		private Dictionary<ulong, List<string>> _ChannelOverrides = new Dictionary<ulong, List<string>>();
		[JsonProperty("RoleOverrides")]
		private Dictionary<ulong, List<string>> _RoleOverrides = new Dictionary<ulong, List<string>>();
		[JsonProperty("UserOverrides")]
		private Dictionary<ulong, List<string>> _UserOverrides = new Dictionary<ulong, List<string>>();

		[JsonConstructor]
		internal CommandSettings()
		{
			//Add in the default values for commands that aren't set
			_DisabledCommands.AddRange(Constants.HELP_ENTRIES.GetUnsetCommands(_DisabledCommands).Where(x => !x.DefaultEnabled).Select(x => x.Name));
			//Remove all that have no name/aren't commands anymore
			_DisabledCommands.RemoveAll(x => String.IsNullOrWhiteSpace(x));
			ClearInvalidValues(_UserOverrides);
			ClearInvalidValues(_RoleOverrides);
			ClearInvalidValues(_ChannelOverrides);
		}

		public string[] ModifyCommands(IEnumerable<HelpEntry> helpEntries, bool enable)
		{
			var names = new List<string>();
			foreach (var helpEntry in helpEntries)
			{
				if (ModifyCommand(helpEntry, enable))
				{
					names.Add(helpEntry.Name);
				}
			}
			return names.ToArray();
		}
		public string[] ModifyCommands<T>(IEnumerable<HelpEntry> helpEntries, bool enable, CommandOverrideTarget target, T obj) where T : ISnowflakeEntity
		{
			var names = new List<string>();
			foreach (var helpEntry in helpEntries)
			{
				if (ModifyCommand(helpEntry, enable, target, obj))
				{
					names.Add(helpEntry.Name);
				}
			}
			return names.ToArray();
		}
		public bool ModifyCommand(HelpEntry helpEntry, bool enable)
		{
			return (enable && ModifyCommand(_DisabledCommands, helpEntry, enable))
				|| (!enable && ModifyCommand(_DisabledCommands, helpEntry, enable));
		}
		public bool ModifyCommand<T>(HelpEntry helpEntry, bool enable, CommandOverrideTarget target, T obj) where T : ISnowflakeEntity
		{
			return ModifyCommand(GetList(target, obj.Id), helpEntry, enable);
		}
		public bool IsCommandEnabled(ICommandContext context, CommandInfo command)
		{
			return !(_DisabledCommands.Any(x => x.CaseInsEquals(command.Name))
				|| _ChannelOverrides.Any(o => o.Key == context.Channel.Id && o.Value.Any(n => n.CaseInsEquals(command.Name)))
				|| _RoleOverrides.Any(o => o.Key == context.Channel.Id && o.Value.Any(n => n.CaseInsEquals(command.Name)))
				|| _UserOverrides.Any(o => o.Key == context.Channel.Id && o.Value.Any(n => n.CaseInsEquals(command.Name))));
		}

		private List<string> GetList(CommandOverrideTarget target, ulong id)
		{
			Dictionary<ulong, List<string>> dict;
			switch (target)
			{
				case CommandOverrideTarget.Channel:
				{
					dict = _ChannelOverrides;
					break;
				}
				case CommandOverrideTarget.Role:
				{
					dict = _RoleOverrides;
					break;
				}
				case CommandOverrideTarget.User:
				{
					dict = _UserOverrides;
					break;
				}
				default:
				{
					throw new ArgumentException("Invalid type supplied.", nameof(target));
				}
			}
			return dict.TryGetValue(id, out var list) ? list : dict[id] = new List<string>();
		}
		private string FormatDict(Dictionary<ulong, List<string>> dict, string type, SocketGuild guild = null)
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

				var overrides = kvp.Value.Any() ? $"\tCommands: `{String.Join("`, `", kvp.Value)}`\n" : null;
				if (!String.IsNullOrWhiteSpace(overrides))
				{
					sb.AppendLine($"{title}\n{overrides}");
				}
			}
			return sb.ToString();
		}
		private bool ModifyCommand(List<string> list, HelpEntry helpEntry, bool enable)
		{
			if (enable)
			{
				return helpEntry.AbleToBeTurnedOff && list.RemoveAll(x => x == helpEntry.Name) > 0;
			}
			else
			{
				var success = helpEntry.AbleToBeTurnedOff && !list.Any(x => x == helpEntry.Name);
				if (success)
				{
					_DisabledCommands.Add(helpEntry.Name);
				}
				return success;
			}
		}
		private void ClearInvalidValues(Dictionary<ulong, List<string>> dict)
		{
			foreach (var key in dict.Keys)
			{
				dict[key].RemoveAll(x => String.IsNullOrWhiteSpace(x) || Constants.HELP_ENTRIES[x] == null);
				//If there are no valid commands/categories being disabled then remove the value
				if (!dict[key].Any())
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
			return $"**Disabled Commands:**\n{String.Join("\n", _DisabledCommands.Select(x => $"`{x}`"))}\n\n" +
				$"{FormatDict(_ChannelOverrides, "Channel", guild)}\n" +
				$"{FormatDict(_RoleOverrides, "Role", guild)}\n" +
				$"{FormatDict(_UserOverrides, "User", guild)}";
		}
	}

	public enum CommandOverrideTarget
	{
		Channel,
		Role,
		User,
	}
}
