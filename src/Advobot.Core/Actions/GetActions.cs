using Advobot.Core.Actions.Formatting;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Advobot.Core.Actions
{
	public static class GetActions
	{
		/// <summary>
		/// Returns the assembly in the appdomain which is marked with <see cref="CommandAssemblyAttribute"/.>
		/// </summary>
		/// <returns></returns>
		public static Assembly GetCommandAssembly()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetCustomAttribute<CommandAssemblyAttribute>() != null);
			if (!assemblies.Any())
			{
				ConsoleActions.WriteLine($"Unable to find any command assemblies. Press any key to close the program.");
				Console.ReadKey();
				throw new DllNotFoundException("Unable to find any command assemblies.");
			}
			else if (assemblies.Count() > 1)
			{
				ConsoleActions.WriteLine("Too many command assemblies found. Press any key to close the program.");
				Console.ReadKey();
				throw new InvalidOperationException("Too many command assemblies found.");
			}

			return assemblies.Single();
		}
		/// <summary>
		/// Returns the fields in <see cref="Color"/> so colors can be gotten easily through name.
		/// </summary>
		/// <returns></returns>
		public static ImmutableDictionary<string, Color> GetColorDictionary()
		{
			return typeof(Color).GetFields(BindingFlags.Public | BindingFlags.Static).ToDictionary(
				x => x.Name,
				x => (Color)x.GetValue(new Color()),
				StringComparer.OrdinalIgnoreCase).ToImmutableDictionary();
		}
		/// <summary>
		/// Returns all names of commands that are in specific category.
		/// </summary>
		/// <param name="category"></param>
		/// <returns></returns>
		public static string[] GetCommandNames(CommandCategory category)
		{
			return Constants.HELP_ENTRIES[category].Select(x => x.Name).ToArray();
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
		/// Returns the guild's prefix if one is set. Returns the bot prefix if not.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public static string GetPrefix(IAdvobotCommandContext context)
			=> GetPrefix(context.BotSettings, context.GuildSettings);
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
			using (var process = Process.GetCurrentProcess())
			{
				process.Refresh();
				return process.PrivateMemorySize64 / (1024.0 * 1024.0);
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
			return bypass ? int.MaxValue : botSettings.MaxUserGatherCount;
		}
		/// <summary>
		/// Returns objects where the function does not return null and is either equal to, less than, or greater than a specified number.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="objects"></param>
		/// <param name="target"></param>
		/// <param name="count"></param>
		/// <param name="f"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetObjectsInListBasedOffOfCount<T>(IEnumerable<T> objects, CountTarget target, uint? count, Func<T, int?> f)
		{
			switch (target)
			{
				case CountTarget.Equal:
				{
					objects = objects.Where(x => f(x) != null && f(x) == count);
					break;
				}
				case CountTarget.Below:
				{
					objects = objects.Where(x => f(x) != null && f(x) < count);
					break;
				}
				case CountTarget.Above:
				{
					objects = objects.Where(x => f(x) != null && f(x) > count);
					break;
				}
			}
			return objects;
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
			var path = Path.Combine(Config.Configuration[ConfigKey.SavePath], $"{Constants.SERVER_FOLDER}_{Config.Configuration[ConfigKey.BotId]}");
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