using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Utilities;
using Discord;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Advobot.Core
{
	public static class Constants
	{
		//Regex for checking any awaits are non ConfigureAwait(false): ^(?!.*CAF\(\)).*await.*$
		public const string BOT_VERSION = Version.VERSION_NUMBER;
		public const string PLACEHOLDER_PREFIX = "%PREFIX%";
		public const string DISCORD_INV = "https://discord.gg/MBXypxb"; //Switched from /xd to this invite since no matter what this inv will link to my server and never someone else's server
		public const string REPO = "https://github.com/advorange/Advobot";
		public const string VIP_REGIONS = "VIP_REGIONS";
		public const string VANITY_URL = "VANITY_URL";
		public const string INVITE_SPLASH = "INVITE_SPLASH";
		public const string GUILD_SETTINGS_LOC = "GuildSettings.json";
		public const string BOT_SETTINGS_LOC = "BotSettings.json";
		public const int MAX_MESSAGE_LENGTH = 2000;

		public static readonly string API_VERSION = Assembly.GetAssembly(typeof(IDiscordClient)).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
		public static readonly TimeSpan DEFAULT_WAIT_TIME = TimeSpan.FromSeconds(3);
		public static readonly ImmutableList<Assembly> COMMAND_ASSEMBLIES = GetCommandAssemblies();
		public static readonly ImmutableDictionary<string, Color> COLORS = GetColorDictionary();
		public static readonly HelpEntryHolder HELP_ENTRIES = new HelpEntryHolder(COMMAND_ASSEMBLIES);

		private static ImmutableList<Assembly> GetCommandAssemblies()
		{
			var currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			var unloadedAssemblies = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly)
				.Where(f => Path.GetFileName(f).CaseInsContains("Commands"))
				.Select(f => Assembly.LoadFrom(f));
			var commandAssemblies = currentAssemblies.Concat(unloadedAssemblies).Where(x => x.GetCustomAttribute<CommandAssemblyAttribute>() != null).ToList();
			if (commandAssemblies.Any())
			{
				return commandAssemblies.ToImmutableList();
			}

			ConsoleUtils.WriteLine($"Unable to find any command assemblies.");
			Console.Read();
			throw new DllNotFoundException("Unable to find any command assemblies.");
		}
		private static ImmutableDictionary<string, Color> GetColorDictionary()
		{
			return typeof(Color).GetFields(BindingFlags.Public | BindingFlags.Static)
				.ToDictionary(x => x.Name, x => (Color)x.GetValue(new Color()), StringComparer.OrdinalIgnoreCase)
				.ToImmutableDictionary();
		}
	}
}