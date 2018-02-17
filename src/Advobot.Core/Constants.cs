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
		public const string BOT_SETTINGS_LOC = "BotSettings.json";
		public const int MAX_MESSAGE_LENGTH = 2000;

		public static readonly string API_VERSION = Assembly.GetAssembly(typeof(IDiscordClient)).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
		public static readonly TimeSpan DEFAULT_WAIT_TIME = TimeSpan.FromSeconds(3);
	}
}