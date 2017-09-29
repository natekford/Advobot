using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Advobot.Actions
{
	public static class GetActions
	{
		/// <summary>
		/// Returns the public fields in the Discord.Color struct as a name to color dictionary.
		/// </summary>
		/// <returns></returns>
		public static ReadOnlyDictionary<string, Color> GetColorDictionary()
		{
			return new ReadOnlyDictionary<string, Color>(typeof(Color).GetFields().Where(x => x.IsPublic).ToDictionary(
				x => x.Name, 
				x => (Color)x.GetValue(new Color()), 
				StringComparer.OrdinalIgnoreCase));
		}
		/// <summary>
		/// Returns a list of every command's help entry.
		/// </summary>
		/// <returns></returns>
		public static ReadOnlyCollection<HelpEntry> GetHelpList()
		{
			var temp = new List<HelpEntry>();

			var types = Assembly.GetExecutingAssembly().GetTypes();
			var cmds = types.Where(x => x.IsSubclassOf(typeof(MyModuleBase)) && x.GetCustomAttribute<GroupAttribute>() != null);
			foreach (var classType in cmds)
			{
				var innerMostNameSpace = classType.Namespace.Substring(classType.Namespace.LastIndexOf('.') + 1);
				if (!Enum.TryParse(innerMostNameSpace, true, out CommandCategory category))
				{
					throw new InvalidOperationException(innerMostNameSpace + " is not currently in the CommandCategory enum.");
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

				temp.Add(new HelpEntry(name, aliases, usage, GeneralFormatting.JoinNonNullStrings(" | ", new[] { permReqs, otherReqs }), summary, category, defaultEnabledAttr.Enabled));
			}
			return temp.AsReadOnly();
		}
		/// <summary>
		/// Returns a list of every command's name.
		/// </summary>
		/// <returns></returns>
		public static ReadOnlyCollection<string> GetCommandNames()
		{
			return Constants.HELP_ENTRIES.Select(x => x.Name).ToList().AsReadOnly();
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
			return String.IsNullOrWhiteSpace(guildSettings.Prefix) ? botSettings.Prefix : guildSettings.Prefix;
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
			var path = Path.Combine(Config.Configuration[Config.ConfigKeys.Save_Path], $"{Constants.SERVER_FOLDER}_{Config.Configuration[Config.ConfigKeys.Bot_Id]}");
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
		/// Returns true if there is a valid error reason. Returns false if the command executed without errors.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="errorReason"></param>
		/// <returns></returns>
		public static bool TryGetErrorReason(IResult result, out string errorReason)
		{
			errorReason = result.ErrorReason;
			if (result.IsSuccess || Constants.IGNORE_ERROR.CaseInsEquals(result.ErrorReason))
			{
				return false;
			}

			switch (result.Error)
			{
				case null:
				//Ignore commands with the unknown command error because it's annoying
				case CommandError.UnknownCommand:
				{
					return false;
				}
				default:
				{
					return true;
				}
			}
		}
		/// <summary>
		/// Returns true if a valid file type was gotten and the image is smaller than 2.5MB.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="imageUrl"></param>
		/// <param name="fileType"></param>
		/// <param name="errorReason"></param>
		/// <returns></returns>
		public static bool TryGetFileType(IMyCommandContext context, string imageUrl, out string fileType, out string errorReason)
		{
			fileType = null;
			errorReason = null;

			var req = WebRequest.Create(imageUrl);
			req.Method = WebRequestMethods.Http.Head;
			using (var resp = req.GetResponse())
			{
				if (!Constants.VALID_IMAGE_EXTENSIONS.Contains(fileType = "." + resp.Headers.Get("Content-Type").Split('/').Last()))
				{
					errorReason = "Image must be a png or jpg.";
				}
				else if (!int.TryParse(resp.Headers.Get("Content-Length"), out int ContentLength))
				{
					errorReason = "Unable to get the image's file size.";
				}
				else if (ContentLength > Constants.MAX_ICON_FILE_SIZE)
				{
					var maxSize = (double)Constants.MAX_ICON_FILE_SIZE / 1000 * 1000;
					errorReason = $"Image is bigger than {maxSize:0.0}MB. Manually upload instead.";
				}
				else
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Returns true if the passed in string is a valid Url.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static bool GetIfStringIsValidUrl(string input)
		{
			if (String.IsNullOrWhiteSpace(input))
			{
				return false;
			}

			return Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
		}

		/// <summary>
		/// Returns the <see cref="Process.WorkingSet64"/> value divided by a MB.
		/// </summary>
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