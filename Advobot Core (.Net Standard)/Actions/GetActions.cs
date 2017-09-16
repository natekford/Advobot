using Advobot.Attributes;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Advobot.Actions
{
	public static class GetActions
	{
		/// <summary>
		/// Returns the public fields in the Discord.Color struct as a name to color dictionary.
		/// </summary>
		/// <returns></returns>
		public static Dictionary<string, Color> GetColorDictionary()
		{
			var dict = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);
			foreach (var color in typeof(Color).GetFields().Where(x => x.IsPublic))
			{
				dict.Add(color.Name, (Color)color.GetValue(new Color()));
			}
			return dict;
		}
		/// <summary>
		/// Returns a list of every command's help entry.
		/// </summary>
		/// <returns></returns>
		public static List<HelpEntry> GetHelpList()
		{
			var temp = new List<HelpEntry>();
			foreach (var classType in Assembly.GetCallingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(MyModuleBase))))
			{
				var innerMostNameSpace = classType.Namespace.Substring(classType.Namespace.LastIndexOf('.') + 1);
				if (!Enum.TryParse(innerMostNameSpace, true, out CommandCategory category))
				{
#if DEBUG
					ConsoleActions.WriteLine(innerMostNameSpace + " is not currently in the CommandCategory enum.");
#endif
					continue;
				}
				else if (classType.IsNotPublic)
				{
					throw new InvalidOperationException(classType.Name + " is not public and commands will not execute from it.");
				}
				else if (classType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).Any(x => x.GetCustomAttribute(typeof(CommandAttribute)) == null))
				{
					throw new InvalidOperationException(classType.Name + " has a command missing the command attribute.");
				}
				else if (classType.IsNested)
				{
					//Nested commands don't really need to be added since they're added under the class they're nested in
					continue;
				}

				var name = classType.GetCustomAttribute<GroupAttribute>()?.Prefix;
				var aliases = classType.GetCustomAttribute<AliasAttribute>()?.Aliases;
				var summary = classType.GetCustomAttribute<SummaryAttribute>()?.Text;
				var usage = classType.GetCustomAttribute<UsageAttribute>()?.ToString(name);
				var permReqs = classType.GetCustomAttribute<PermissionRequirementAttribute>()?.ToString();
				var otherReqs = classType.GetCustomAttribute<OtherRequirementAttribute>()?.ToString();

				var defaultEnabledAttr = classType.GetCustomAttribute<DefaultEnabledAttribute>();
				if (defaultEnabledAttr == null)
				{
					throw new InvalidOperationException(name + " does not have a default enabled value set.");
				}

				var similarCmds = temp.Where(x => x.Name.CaseInsEquals(name) || (x.Aliases != null && aliases != null && x.Aliases.Intersect(aliases, StringComparer.OrdinalIgnoreCase).Any()));
				if (similarCmds.Any())
				{
					throw new ArgumentException($"The following commands have conflicts: {String.Join(" + ", similarCmds.Select(x => x.Name))} + {name}");
				}

				temp.Add(new HelpEntry(name, aliases, usage, FormattingActions.JoinNonNullStrings(" | ", new[] { permReqs, otherReqs }), summary, category, defaultEnabledAttr.Enabled));
			}
			return temp;
		}
		/// <summary>
		/// Returns a list of every command's name.
		/// </summary>
		/// <param name="helpEntries"></param>
		/// <returns></returns>
		public static List<string> GetCommandNames()
		{
			return Constants.HELP_ENTRIES.Select(x => x.Name).ToList();
		}
		/// <summary>
		/// Returns a list of every guild permission.
		/// </summary>
		/// <returns></returns>
		public static List<BotGuildPermission> GetGuildPermissions()
		{
			var temp = new List<BotGuildPermission>();
			for (int i = 0; i < 64; ++i)
			{
				var name = Enum.GetName(typeof(GuildPermission), i);
				if (name == null)
					continue;

				temp.Add(new BotGuildPermission(name, i));
			}
			return temp;
		}
		/// <summary>
		/// Returns a list of every channel permission.
		/// </summary>
		/// <returns></returns>
		public static List<BotChannelPermission> GetChannelPermissions()
		{
			const ulong GENERAL_BITS = 0
				| (1U << (int)ChannelPermission.CreateInstantInvite)
				| (1U << (int)ChannelPermission.ManageChannel)
				| (1U << (int)ChannelPermission.ManagePermissions)
				| (1U << (int)ChannelPermission.ManageWebhooks);

			const ulong TEXT_BITS = 0
				| (1U << (int)ChannelPermission.ReadMessages)
				| (1U << (int)ChannelPermission.SendMessages)
				| (1U << (int)ChannelPermission.SendTTSMessages)
				| (1U << (int)ChannelPermission.ManageMessages)
				| (1U << (int)ChannelPermission.EmbedLinks)
				| (1U << (int)ChannelPermission.AttachFiles)
				| (1U << (int)ChannelPermission.ReadMessageHistory)
				| (1U << (int)ChannelPermission.MentionEveryone)
				| (1U << (int)ChannelPermission.UseExternalEmojis)
				| (1U << (int)ChannelPermission.AddReactions);

			const ulong VOICE_BITS = 0
				| (1U << (int)ChannelPermission.Connect)
				| (1U << (int)ChannelPermission.Speak)
				| (1U << (int)ChannelPermission.MuteMembers)
				| (1U << (int)ChannelPermission.DeafenMembers)
				| (1U << (int)ChannelPermission.MoveMembers)
				| (1U << (int)ChannelPermission.UseVAD);

			var temp = new List<BotChannelPermission>();
			for (int i = 0; i < 64; ++i)
			{
				var name = Enum.GetName(typeof(ChannelPermission), i);
				if (name == null)
					continue;

				if ((GENERAL_BITS & (1U << i)) != 0)
				{
					temp.Add(new BotChannelPermission(name, i, gen: true));
				}
				if ((TEXT_BITS & (1U << i)) != 0)
				{
					temp.Add(new BotChannelPermission(name, i, text: true));
				}
				if ((VOICE_BITS & (1U << i)) != 0)
				{
					temp.Add(new BotChannelPermission(name, i, voice: true));
				}
			}
			return temp;
		}

		/// <summary>
		/// Returns nothing if equal to 1. Returns "s" if not. Double allows most, if not all, number types in: https://stackoverflow.com/a/828963.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public static string GetPlural(double i)
		{
			return i == 1 ? "" : "s";
		}
		/// <summary>
		/// Returns the guild prefix if one is set. Returns the bot prefix if not.
		/// </summary>
		/// <param name="botSettings"></param>
		/// <param name="guildSettings"></param>
		/// <returns></returns>
		public static string GetPrefix(IBotSettings botSettings, IGuildSettings guildSettings)
		{
			var guildPrefix = guildSettings.Prefix;
			if (!String.IsNullOrWhiteSpace(guildPrefix))
			{
				return guildPrefix;
			}
			else
			{
				return botSettings.Prefix;
			}
		}

		/// <summary>
		/// Returns a dictionary of channel permissions and their values (allow, deny, inherit). Non filtered so incorrect channel type permissions will be in it.
		/// </summary>
		/// <param name="overwrite"></param>
		/// <returns></returns>
		public static Dictionary<string, string> GetChannelOverwritePermissions(Overwrite overwrite)
		{
			var channelPerms = new Dictionary<string, string>();
			//Make a copy of the channel perm list to check off perms as they go by
			var genericChannelPerms = Constants.CHANNEL_PERMISSIONS.Select(x => x.Name).ToList();
			//Add allow perms to the dictionary and remove them from the checklist
			foreach (var perm in overwrite.Permissions.ToAllowList())
			{
				channelPerms.Add(perm.ToString(), nameof(PermValue.Allow));
				genericChannelPerms.Remove(perm.ToString());
			}
			//Add deny perms to the dictionary and remove them from the checklist
			foreach (var perm in overwrite.Permissions.ToDenyList())
			{
				channelPerms.Add(perm.ToString(), nameof(PermValue.Deny));
				genericChannelPerms.Remove(perm.ToString());
			}
			//Add the remaining perms as inherit
			genericChannelPerms.ForEach(x => channelPerms.Add(x, nameof(PermValue.Inherit)));

			//Remove these random values that exist for some reason
			//Not sure these still exist, but leaving this in. TODO: make sure these aren't here?
			channelPerms.Remove("1");
			channelPerms.Remove("3");

			return channelPerms;
		}
		/// <summary>
		/// Returns a similar dictionary to <see cref="GetChannelOverwritePermissions"/> except this method has voice permissions filtered out of text channels and vice versa.
		/// </summary>
		/// <param name="overwrite"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public static Dictionary<string, string> GetFilteredChannelOverwritePermissions(Overwrite overwrite, IGuildChannel channel)
		{
			var dictionary = GetChannelOverwritePermissions(overwrite);
			if (channel is ITextChannel)
			{
				foreach (var perm in Constants.CHANNEL_PERMISSIONS.Where(x => x.Voice))
				{
					dictionary.Remove(perm.Name);
				}
			}
			else
			{
				foreach (var perm in Constants.CHANNEL_PERMISSIONS.Where(x => x.Text))
				{
					dictionary.Remove(perm.Name);
				}
			}
			return dictionary;
		}
		/// <summary>
		/// Returns the guild permission bits that are set within the passed in ulong.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static string[] GetGuildPermissionNames(ulong flags)
		{
			var result = new List<string>();
			//Using 64 has this return duplicated permissions.
			for (int i = 0; i < 32; ++i)
			{
				ulong value = 1U << i;
				if ((flags & value) == 0)
				{
					continue;
				}

				var name = Constants.GUILD_PERMISSIONS.FirstOrDefault(x => x.Value == value).Name;
				if (String.IsNullOrWhiteSpace(name))
				{
					continue;
				}

				result.Add(name);
			}
			return result.ToArray();
		}
		/// <summary>
		/// Returns the channel permission bits that are set within the passed in ulong.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static string[] GetChannelPermissionNames(ulong flags)
		{
			var result = new List<string>();
			//Using 64 has this return duplicated permissions.
			for (int i = 0; i < 32; ++i)
			{
				ulong value = 1U << i;
				if ((flags & value) == 0)
				{
					continue;
				}

				var name = Constants.CHANNEL_PERMISSIONS.FirstOrDefault(x => x.Value == value).Name;
				if (String.IsNullOrWhiteSpace(name))
				{
					continue;
				}

				result.Add(name);
			}
			return result.ToArray();
		}
		/// <summary>
		/// Returns the channel perms gotten from <see cref="GetFilteredChannelOverwritePermissions"/> formatted with their perm value in front of the perm name.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channel"></param>
		/// <param name="overwriteObj"></param>
		/// <returns></returns>
		public static string[] GetFormattedPermsFromOverwrite<T>(IGuildChannel channel, T overwriteObj) where T : ISnowflakeEntity
		{
			var perms = GetFilteredChannelOverwritePermissions(channel.PermissionOverwrites.FirstOrDefault(x => overwriteObj.Id == x.TargetId), channel);
			var maxLen = perms.Keys.Max(x => x.Length);
			return perms.Select(x => $"{x.Key.PadRight(maxLen)} {x.Value}").ToArray();
		}
		/// <summary>
		/// Returns a bool indicating true if all perms are valid. Out values of valid perms and invalid perms.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="validPerms"></param>
		/// <param name="invalidPerms"></param>
		/// <returns>Boolean representing true if all permissions are valid, false if any are invalid.</returns>
		public static bool TryGetValidGuildPermissionNamesFromInputString(string input, out IEnumerable<string> validPerms, out IEnumerable<string> invalidPerms)
		{
			var permissions = input.Split('/', ' ').Select(x => x.Trim(','));
			validPerms = permissions.Where(x => Constants.GUILD_PERMISSIONS.Select(y => y.Name).CaseInsContains(x));
			invalidPerms = permissions.Where(x => !Constants.GUILD_PERMISSIONS.Select(y => y.Name).CaseInsContains(x));
			return !invalidPerms.Any();
		}
		/// <summary>
		/// Returns a bool indicating true if all perms are valid. Out values of valid perms and invalid perms.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="validPerms"></param>
		/// <param name="invalidPerms"></param>
		/// <returns>Boolean representing true if all permissions are valid, false if any are invalid.</returns>
		public static bool TryGetValidChannelPermissionNamesFromInputString(string input, out IEnumerable<string> validPerms, out IEnumerable<string> invalidPerms)
		{
			var permissions = input.Split('/', ' ').Select(x => x.Trim(','));
			validPerms = permissions.Where(x => Constants.CHANNEL_PERMISSIONS.Select(y => y.Name).CaseInsContains(x));
			invalidPerms = permissions.Where(x => !Constants.CHANNEL_PERMISSIONS.Select(y => y.Name).CaseInsContains(x));
			return !invalidPerms.Any();
		}

		/// <summary>
		/// Returns commands from guildsettings that are in a specific category.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="category"></param>
		/// <returns></returns>
		public static CommandSwitch[] GetMultipleCommands(IGuildSettings guildSettings, CommandCategory category)
		{
			return guildSettings.CommandSwitches.Where(x => x.Category == category).ToArray();
		}
		/// <summary>
		/// Returns a command from guildsettings with the passed in command name/alias.
		/// </summary>
		/// <param name="guildSettings"></param>
		/// <param name="commandNameOrAlias"></param>
		/// <returns></returns>
		public static CommandSwitch GetCommand(IGuildSettings guildSettings, string commandNameOrAlias)
		{
			return guildSettings.CommandSwitches.FirstOrDefault(x =>
			{
				if (x.Name.CaseInsEquals(commandNameOrAlias))
				{
					return true;
				}
				else if (x.Aliases != null && x.Aliases.CaseInsContains(commandNameOrAlias))
				{
					return true;
				}
				else
				{
					return false;
				}
			});
		}
		/// <summary>
		/// Returns all names of commands that are in specific category.
		/// </summary>
		/// <param name="category"></param>
		/// <returns></returns>
		public static string[] GetCommandNames(CommandCategory category)
		{
			return Constants.HELP_ENTRIES.Where(x => x.Category == category).Select(x => x.Name).ToArray();
		}

		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId\ServerId
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		public static DirectoryInfo GetServerDirectory(ulong guildId)
		{
			var path = Path.Combine(GetBaseBotDirectory().FullName, guildId.ToString());
			return Directory.CreateDirectory(path);
		}
		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId\ServerId\File
		/// </summary>
		/// <param name="guildId"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static FileInfo GetServerDirectoryFile(ulong guildId, string fileName)
		{
			var path = Path.Combine(GetServerDirectory(guildId).FullName, fileName);
			return new FileInfo(path);
		}
		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId
		/// </summary>
		/// <returns></returns>
		public static DirectoryInfo GetBaseBotDirectory()
		{
			var path = Path.Combine(Config.Configuration[ConfigKeys.Save_Path], $"{Constants.SERVER_FOLDER}_{Config.Configuration[ConfigKeys.Bot_Id]}");
			return Directory.CreateDirectory(path);
		}
		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId\File
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static FileInfo GetBaseBotDirectoryFile(string fileName)
		{
			var path = Path.Combine(GetBaseBotDirectory().FullName, fileName);
			return new FileInfo(path);
		}

		/// <summary>
		/// Returns a variable from a list of arguments and removes it from the list of arguments.
		/// </summary>
		/// <param name="inputList"></param>
		/// <param name="searchTerm"></param>
		/// <returns></returns>
		public static string GetVariableAndRemove(List<string> inputList, string searchTerm)
		{
			var first = inputList?.FirstOrDefault(x => x.Substring(0, Math.Max(x.IndexOf(':'), 1)).CaseInsEquals(searchTerm));
			if (first != null)
			{
				inputList.ThreadSafeRemove(first);
				//Return everything after the first colon (the keyword)
				return first.Substring(first.IndexOf(':') + 1);
			}
			return null;
		}
		/// <summary>
		/// Returns a variable from a list of arguments.
		/// </summary>
		/// <param name="inputArray"></param>
		/// <param name="searchTerm"></param>
		/// <returns></returns>
		public static string GetVariable(IEnumerable<string> inputArray, string searchTerm)
		{
			var first = inputArray?.FirstOrDefault(x => x.Substring(0, Math.Max(x.IndexOf(':'), 1)).CaseInsEquals(searchTerm));
			return first?.Substring(first.IndexOf(':') + 1);
		}

		/// <summary>
		/// On windows, returns the task manager value. On other systems, returns the WorkingSet64 value.
		/// </summary>
		/// <param name="windows"></param>
		/// <returns></returns>
		public static double GetMemory()
		{
			const double _MB = 1024.0 * 1024.0;

			using (var process = Process.GetCurrentProcess())
			{
				return Convert.ToInt32(process.WorkingSet64) / _MB;
			}
		}
		/// <summary>
		/// Returns int.MaxValue is bypass is true, otherwise returns whatever botSettings has for MaxUserGatherCount.
		/// </summary>
		/// <param name="botSettings"></param>
		/// <param name="bypass"></param>
		/// <returns></returns>
		public static int GetMaxAmountOfUsersToGather(IBotSettings botSettings, bool bypass)
		{
			return bypass ? int.MaxValue : (int)botSettings.MaxUserGatherCount;
		}

		/// <summary>
		/// Returns all public properties from IGuildSettings that have a set method.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<PropertyInfo> GetGuildSettings()
		{
			return GetSettings(typeof(IGuildSettings));
		}
		/// <summary>
		/// Returns all public properties from IBotSettings that have a set method. Will not return SavePath and BotKey since those
		/// are saved via <see cref="Properties.Settings.Default"/>.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<PropertyInfo> GetBotSettings()
		{
			return GetSettings(typeof(IBotSettings));
		}
		/// <summary>
		/// Returns the values of <see cref="GetBotSettings"/> which either are strings or do not implement the generic IEnumerable.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<PropertyInfo> GetBotSettingsThatArentIEnumerables()
		{
			return GetBotSettings().Where(x =>
			{
				return x.PropertyType == typeof(string) || !x.PropertyType.GetInterfaces().Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(IEnumerable<>));
			});
		}
		/// <summary>
		/// Returns all public properties 
		/// </summary>
		/// <param name="settingHolderType"></param>
		/// <returns></returns>
		public static IEnumerable<PropertyInfo> GetSettings(Type settingHolderType)
		{
			var properties = settingHolderType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			return properties.Where(x => x.CanWrite && x.GetSetMethod(true).IsPublic);
		}
	}
}