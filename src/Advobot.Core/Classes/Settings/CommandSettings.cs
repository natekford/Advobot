using System.Collections.Generic;
using System.Linq;
using Advobot.Classes.Attributes;
using Advobot.Classes.Formatting;
using Advobot.Interfaces;
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
	public sealed class CommandSettings : IGuildFormattable
	{
		[JsonProperty("CommandValues")]
		private readonly Dictionary<string, bool> _CommandValues = new Dictionary<string, bool>();
		[JsonProperty("Overrides")]
		private readonly Dictionary<ulong, Dictionary<string, bool>> _Overrides = new Dictionary<ulong, Dictionary<string, bool>>();

		/// <summary>
		/// Changes the value for whether or not the commands are enabled on a guild.
		/// </summary>
		/// <param name="values">The commands to change.</param>
		/// <param name="enable"></param>
		/// <returns>The names of the commands which were successfully changed.</returns>
		public string[] ModifyCommandValues(IEnumerable<IHelpEntry> values, bool? enable)
		{
			var changed = new List<string>();
			foreach (var value in values)
			{
				if (ModifyCommandValue(value, enable))
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
		/// <param name="enable"></param>
		/// <returns>Whether or not the method was successful. Failure indicates an untoggleable command or the command was already set to the passed in value.</returns>
		public bool ModifyCommandValue(IHelpEntry value, bool? enable)
			=> ModifyOverride(_CommandValues, value, enable);
		/// <summary>
		/// Enabled/disables/removes overrides on specified commands for a specified object. Object can be channel, role, or user.
		/// </summary>
		/// <param name="values">The commands to override.</param>
		/// <param name="obj">The object to target.</param>
		/// <param name="enable"></param>
		/// <returns>The names of the commands which were successfully changed.</returns>
		public string[] ModifyOverrides(IEnumerable<IHelpEntry> values, ISnowflakeEntity obj, bool? enable)
		{
			var changed = new List<string>();
			foreach (var value in values)
			{
				if (ModifyOverride(value, obj, enable))
				{
					changed.Add(value.Name);
				}
			}
			return changed.ToArray();
		}
		/// <summary>
		/// Enables/disables/removes an override on a specified command for a specified object.
		/// </summary>
		/// <param name="value">The command to override.</param>
		/// <param name="obj">The object to target.</param>
		/// <param name="enable"></param>
		/// <returns>Whether or not the method was successful. Failure indicates an untoggleable command or the command was already set to the passed in value.</returns>
		public bool ModifyOverride(IHelpEntry value, ISnowflakeEntity obj, bool? enable)
		{
			var innerDict = _Overrides.TryGetValue(obj.Id, out var inner) ? inner : _Overrides[obj.Id] = new Dictionary<string, bool>();
			return ModifyOverride(innerDict, value, enable);
		}
		/// <summary>
		/// Returns a value indicating whether or not the command is enabled in the current context.
		/// Checks user, then roles ordered by descending hierarchy, then channel, then finally the default guild setting.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="channel"></param>
		/// <param name="command"></param>
		/// <returns></returns>
		public bool IsCommandEnabled(SocketGuildUser user, SocketTextChannel channel, CommandInfo command)
		{
			//Hierarchy:
			//	User
			//	Role -> Ordered by position
			//	Channel
			//	Guild

			var module = command.Module;
			while (module.Parent != null && module.Parent.IsSubmodule)
			{
				module = module.Parent;
			}
			var name = module.Name;

			if (_Overrides.TryGetValue(user.Id, out var uD) && uD.TryGetValue(name, out var u))
			{
				return u;
			}
			foreach (var role in user.Roles.OrderByDescending(x => x.Position))
			{
				if (_Overrides.TryGetValue(role.Id, out var rD) && rD.TryGetValue(name, out var r))
				{
					return r;
				}
			}
			if (_Overrides.TryGetValue(channel.Id, out var cD) && cD.TryGetValue(name, out var c))
			{
				return c;
			}
			if (_CommandValues.TryGetValue(name, out var value))
			{
				return value;
			}

			//If they get here it means they're not in the command values currently so they should just use the default value.
			var defaultEnabledAttr = module.Attributes.GetAttribute<EnabledByDefaultAttribute>();
			var defaultEnabled = defaultEnabledAttr?.Enabled ?? false;
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
			=> _CommandValues.TryGetValue(name, out var val) ? val : (bool?)null;
		private static bool ModifyOverride(IDictionary<string, bool> dict, IHelpEntry help, bool? enable)
		{
			if (!help.AbleToBeToggled)
			{
				return false;
			}
			if (enable == null)
			{
				return dict.Remove(help.Name);
			}
			if (dict.TryGetValue(help.Name, out var currentValue) && currentValue == enable)
			{
				return false;
			}
			dict[help.Name] = enable.Value;
			return true;
		}

		/// <inheritdoc />
		public IDiscordFormattableString GetFormattableString()
		{
			var formattable = new DiscordFormattableStringCollection();
			foreach (var kvp in _CommandValues)
			{
				formattable.Add($"{kvp.Key}: {kvp.Value}");
			}
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
			foreach (var (Id, Dict) in _Overrides)
			{
				foreach (var (CommandName, Enabled) in Dict)
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
				{
					formattable.Add($"{Id}: {CommandName} ({Enabled})");
				}
			}
			return formattable;
		}
	}
}
