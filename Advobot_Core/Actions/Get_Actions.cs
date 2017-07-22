using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.SavedClasses;
using Advobot.Structs;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Advobot
{
	namespace Actions
	{
		public static class GetActions
		{
			public static Dictionary<string, Color> GetColorDictionary()
			{
				var dict = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);
				foreach (var color in typeof(Color).GetFields().Where(x => x.IsPublic))
				{
					dict.Add(color.Name, (Color)color.GetValue(new Color()));
				}
				return dict;
			}

			public static string GetPlural(double i)
			{
				//Double allows most, if not all, number types in. https://stackoverflow.com/a/828963
				return i == 1 ? "" : "s";
			}
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

			public static Dictionary<string, string> GetChannelOverwritePermissions(Overwrite overwrite)
			{
				//Create a dictionary to hold the allow/deny/inherit values
				var channelPerms = new Dictionary<String, String>();

				//Make a copy of the channel perm list to check off perms as they go by
				var genericChannelPerms = Constants.CHANNEL_PERMISSIONS.Select(x => x.Name).ToList();

				//Add allow perms to the dictionary and remove them from the checklist
				overwrite.Permissions.ToAllowList().ForEach(x =>
				{
					channelPerms.Add(x.ToString(), "Allow");
					genericChannelPerms.Remove(x.ToString());
				});

				//Add deny perms to the dictionary and remove them from the checklist
				overwrite.Permissions.ToDenyList().ForEach(x =>
				{
					channelPerms.Add(x.ToString(), "Deny");
					genericChannelPerms.Remove(x.ToString());
				});

				//Add the remaining perms as inherit after removing all null values
				genericChannelPerms.ForEach(x => channelPerms.Add(x, "Inherit"));

				//Remove these random values that exist for some reason
				channelPerms.Remove("1");
				channelPerms.Remove("3");

				return channelPerms;
			}
			public static Dictionary<string, string> GetFilteredChannelOverwritePermissions(Overwrite overwrite, IGuildChannel channel)
			{
				var dictionary = GetChannelOverwritePermissions(overwrite);
				if (channel is ITextChannel)
				{
					Constants.CHANNEL_PERMISSIONS.Where(x => x.Voice).ToList().ForEach(x => dictionary.Remove(x.Name));
				}
				else
				{
					Constants.CHANNEL_PERMISSIONS.Where(x => x.Text).ToList().ForEach(x => dictionary.Remove(x.Name));
				}
				return dictionary;
			}
			public static List<string> GetPermissionNames(ulong flags)
			{
				var result = new List<string>();
				for (int i = 0; i < 64; ++i)
				{
					ulong bit = 1U << i;
					if ((flags & bit) != 0)
					{
						var name = Constants.GUILD_PERMISSIONS.FirstOrDefault(x => x.Bit == bit).Name;
						if (!String.IsNullOrWhiteSpace(name))
						{
							result.Add(name);
						}
					}
				}
				return result;
			}

			public static ReturnedArguments GetArgs(ICommandContext context, string input, int min, int max, string[] argsToSearchFor = null)
			{
				/* Non specified arguments get left in a list of args going left to right (mentions are not included in this if the bool is true).
					* Specified arguments get left in a dictionary.
					*/

				if (input == null)
				{
					var list = new List<string>();
					for (int i = 0; i < max; ++i)
					{
						list.Add(null);
					}
					if (min == 0)
					{
						return new ReturnedArguments(list, FailureReason.NotFailure);
					}
					else
					{
						return new ReturnedArguments(list, FailureReason.TooFew);
					}
				}

				var args = input.SplitByCharExceptInQuotes(' ').ToList();
				if (args.Count < min)
				{
					return new ReturnedArguments(args, FailureReason.TooFew);
				}
				else if (args.Count > max)
				{
					return new ReturnedArguments(args, FailureReason.TooMany);
				}

				//Finding the wanted arguments
				var specifiedArgs = new Dictionary<string, string>();
				if (argsToSearchFor != null)
				{
					foreach (var searchArg in argsToSearchFor)
					{
						var arg = GetVariableAndRemove(args, searchArg);
						if (arg != null)
						{
							specifiedArgs.Add(searchArg, arg);
						}
					}
				}

				for (int i = args.Count; i < max; ++i)
				{
					args.Add(null);
				}

				return new ReturnedArguments(args, specifiedArgs, context.Message);
			}
			public static ReturnedObject<T> GetEnum<T>(string input, IEnumerable<T> validEnums, IEnumerable<T> invalidEnums = null) where T : struct
			{
				if (!Enum.TryParse<T>(input, true, out T tempEnum))
				{
					return new ReturnedObject<T>(tempEnum, FailureReason.TooFew);
				}
				else if (!validEnums.Contains(tempEnum) || (invalidEnums != null && invalidEnums.Contains(tempEnum)))
				{
					return new ReturnedObject<T>(tempEnum, FailureReason.InvalidEnum);
				}

				return new ReturnedObject<T>(tempEnum, FailureReason.NotFailure);
			}

			public static List<CommandSwitch> GetMultipleCommands(IGuildSettings guildSettings, CommandCategory category)
			{
				return guildSettings.CommandSwitches.Where(x => x.Category == category).ToList();
			}
			public static CommandSwitch GetCommand(IGuildSettings guildSettings, string input)
			{
				return guildSettings.CommandSwitches.FirstOrDefault(x =>
				{
					if (x.Name.CaseInsEquals(input))
					{
						return true;
					}
					else if (x.Aliases != null && x.Aliases.CaseInsContains(input))
					{
						return true;
					}
					else
					{
						return false;
					}
				});
			}
			public static string[] GetCommands(CommandCategory category)
			{
				return Constants.HELP_ENTRIES.Where(x => x.Category == category).Select(x => x.Name).ToArray();
			}

			public static string GetObjectStringBasic(Type type)
			{
				if (type.Equals(typeof(IGuildUser)))
				{
					return Constants.BASIC_TYPE_USER;
				}
				else if (type.Equals(typeof(IGuildChannel)))
				{
					return Constants.BASIC_TYPE_CHANNEL;
				}
				else if (type.Equals(typeof(IRole)))
				{
					return Constants.BASIC_TYPE_ROLE;
				}
				else if (type.Equals(typeof(IGuild)))
				{
					return Constants.BASIC_TYPE_GUILD;
				}
				else
				{
					return "GetObjectStringBasic Error";
				}
			}

			public static string GetServerFilePath(ulong guildId, string fileName)
			{
				//Make sure the bot's directory exists
				var directory = GetBaseBotDirectory();
				Directory.CreateDirectory(directory);

				//This string will be similar to C:\Users\User\AppData\Roaming\Discord_Servers_... if using appdata. If not then it can be anything;
				return Path.Combine(directory, guildId.ToString(), fileName);
			}
			public static string GetBaseBotDirectory(string nonGuildFileName = null)
			{
				//Make sure a save path exists
				var folder = Properties.Settings.Default.Path;
				if (!Directory.Exists(folder))
					return null;

				//Get the bot's folder
				var botFolder = String.Format("{0}_{1}", Constants.SERVER_FOLDER, Properties.Settings.Default.BotID);

				//Send back the directory
				return String.IsNullOrWhiteSpace(nonGuildFileName) ? Path.Combine(folder, botFolder) : Path.Combine(folder, botFolder, nonGuildFileName);
			}
			public static FileType? GetFileType(string file)
			{
				if (Enum.TryParse(file, true, out FileType type))
				{
					return type;
				}
				return null;
			}

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
			public static string GetVariable(IEnumerable<string> inputArray, string searchTerm)
			{
				//Get the item
				var first = inputArray?.FirstOrDefault(x => x.Substring(0, Math.Max(x.IndexOf(':'), 1)).CaseInsEquals(searchTerm));
				return first?.Substring(first.IndexOf(':') + 1);
			}

			public static string GetUptime(IBotSettings botSettings)
			{
				var span = DateTime.UtcNow.Subtract(botSettings.StartupTime);
				return String.Format("{0}:{1}:{2}:{3}", span.Days, span.Hours.ToString("00"), span.Minutes.ToString("00"), span.Seconds.ToString("00"));
			}
			public static double GetMemory(bool windows)
			{
				if (windows)
				{
					using (var PC = new System.Diagnostics.PerformanceCounter("Process", "Working Set - Private", System.Diagnostics.Process.GetCurrentProcess().ProcessName))
					{
						return Convert.ToInt32(PC.NextValue()) / (1024.0 * 1024.0);
					}
				}
				else
				{
					using (var process = System.Diagnostics.Process.GetCurrentProcess())
					{
						return Convert.ToInt32(process.WorkingSet64) / (1024.0 * 1024.0);
					}
				}
			}

			public static int GetMaxAmountOfUsersToGather(IBotSettings botSettings, bool bypass)
			{
				return bypass ? int.MaxValue : (int)botSettings.MaxUserGatherCount;
			}
		}
	}
}