using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Settings
{
	/// <summary>
	/// Holds settings for a guild. Settings are only saved by calling <see cref="SaveSettings"/>.
	/// </summary>
	public partial class GuildSettings : IGuildSettings
	{
		/// <summary>
		/// Returns all public properties that have a set method.
		/// </summary>
		/// <returns></returns>
		public static PropertyInfo[] GetSettings()
		{
			return typeof(IGuildSettings)
.GetProperties(BindingFlags.Public | BindingFlags.Instance)
.Where(x => x.CanWrite && x.GetSetMethod(true).IsPublic).ToArray();
		}

		public CommandSwitch[] GetCommands(CommandCategory category)
		{
			return CommandSwitches.Where(x => x.Category == category).ToArray();
		}

		public CommandSwitch GetCommand(string commandNameOrAlias)
		{
			return CommandSwitches.FirstOrDefault(x =>
						{
							return x.Name.CaseInsEquals(commandNameOrAlias) || x.Aliases != null && x.Aliases.CaseInsContains(commandNameOrAlias);
						});
		}

		public bool SetLogChannel(LogChannelType logChannelType, ITextChannel channel)
		{
			switch (logChannelType)
			{
				case LogChannelType.Server:
				{
					if (_ServerLogId == (channel?.Id ?? 0))
					{
						return false;
					}

					ServerLog = channel;
					return true;
				}
				case LogChannelType.Mod:
				{
					if (_ModLogId == (channel?.Id ?? 0))
					{
						return false;
					}

					ModLog = channel;
					return true;
				}
				case LogChannelType.Image:
				{
					if (_ImageLogId == (channel?.Id ?? 0))
					{
						return false;
					}

					ImageLog = channel;
					return true;
				}
				default:
				{
					throw new ArgumentException("invalid type", nameof(channel));
				}
			}
		}
		public string GetPrefix(IBotSettings botSettings)
		{
			return String.IsNullOrWhiteSpace(Prefix) ? botSettings.Prefix : Prefix;
		}

		public void SaveSettings()
		{
			if (Guild == null)
			{
				return;
			}

			IOUtils.OverWriteFile(IOUtils.GetServerDirectoryFile(Guild.Id, Constants.GUILD_SETTINGS_LOC), IOUtils.Serialize(this));
		}
		//TODO: refactor this method and command settings
		public void PostDeserialize(SocketGuild guild)
		{
			Guild = guild;

			//Add in the default values for commands that aren't set
			var unsetCmds = Constants.HELP_ENTRIES.GetUnsetCommands(CommandSwitches.Select(x => x.Name));
			CommandSwitches.AddRange(unsetCmds.Select(x => new CommandSwitch(x.Name, x.DefaultEnabled)));
			//Remove all that have no name/aren't commands anymore
			CommandSwitches.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name) || Constants.HELP_ENTRIES[x.Name].Equals(default));
			CommandsDisabledOnUser.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
			CommandsDisabledOnRole.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
			CommandsDisabledOnChannel.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
			Task.Run(async () =>
			{
				var invites = await InviteUtils.GetInvitesAsync(guild).CAF();
				var cached = invites.Select(x => new CachedInvite(x.Code, x.Uses));
				lock (Invites)
				{
					Invites.AddRange(cached);
				}
#if DEBUG
				ConsoleUtils.WriteLine($"Invites for {guild.Name} have been gotten.");
#endif
			});

			if (_ListedInvite != null)
			{
				_ListedInvite.PostDeserialize(Guild);
			}
			if (_WelcomeMessage != null)
			{
				_WelcomeMessage.PostDeserialize(Guild);
			}
			if (_GoodbyeMessage != null)
			{
				_GoodbyeMessage.PostDeserialize(Guild);
			}
			if (_SelfAssignableGroups != null)
			{
				foreach (var group in _SelfAssignableGroups)
				{
					group.PostDeserialize(Guild);
				}
			}
			if (_PersistentRoles != null)
			{
				_PersistentRoles.RemoveAll(x => x.GetRole(Guild) == null);
			}

			Loaded = true;
		}
		public string Format()
		{
			var sb = new StringBuilder();
			foreach (var property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				//Only get public editable properties
				if (property.GetGetMethod() == null || property.GetSetMethod() == null)
				{
					continue;
				}

				var formatted = Format(property);
				if (String.IsNullOrWhiteSpace(formatted))
				{
					continue;
				}

				sb.AppendLineFeed($"**{property.Name}**:");
				sb.AppendLineFeed($"{formatted}");
				sb.AppendLineFeed("");
			}
			return sb.ToString();
		}
		public string Format(PropertyInfo property)
		{
			return FormatObject(property.GetValue(this));
		}

		private string FormatObject(object value)
		{
			if (value == null)
			{
				return "`Nothing`";
			}
			else if (value is ISetting tempISetting)
			{
				return tempISetting.ToString();
			}
			else if (value is ulong tempUlong)
			{
				var chan = Guild.GetChannel(tempUlong);
				if (chan != null)
				{
					return $"`{chan.Format()}`";
				}

				var role = Guild.GetRole(tempUlong);
				if (role != null)
				{
					return $"`{role.Format()}`";
				}

				var user = Guild.GetUser(tempUlong);
				if (user != null)
				{
					return $"`{user.Format()}`";
				}

				return tempUlong.ToString();
			}
			//Because strings are char[] this has to be here so it doesn't go into IEnumerable
			else if (value is string tempStr)
			{
				return String.IsNullOrWhiteSpace(tempStr) ? "`Nothing`" : $"`{tempStr}`";
			}
			//Has to be above IEnumerable too
			else if (value is IDictionary tempIDictionary)
			{
				var validKeys = tempIDictionary.Keys.Cast<object>().Where(x => tempIDictionary[x] != null);
				return String.Join("\n", validKeys.Select(x =>
				{
					return $"{FormatObject(x)}: {FormatObject(tempIDictionary[x])}";
				}));
			}
			else if (value is IEnumerable tempIEnumerable)
			{
				return String.Join("\n", tempIEnumerable.Cast<object>().Select(x => FormatObject(x)));
			}
			else
			{
				return $"`{value.ToString()}`";
			}
		}
	}
}
