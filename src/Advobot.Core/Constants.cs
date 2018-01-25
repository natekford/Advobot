using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Utilities;
using Discord;

namespace Advobot.Core
{
	public static class Constants
	{
		//Const for attributes/because they're very unlikely to change.
		public const string ZERO_LENGTH_CHAR = "\u180E";
		public const string PLACEHOLDER_PREFIX = ZERO_LENGTH_CHAR + "%PREFIX%";
		public const string DEFAULT_PREFIX = "&&";
		public const string FAKE_DISCORD_LINK = "discord" + ZERO_LENGTH_CHAR + ".gg";
		public const string FAKE_EVERYONE = "@" + ZERO_LENGTH_CHAR + "everyone";
		public const string FAKE_HERE = "@" + ZERO_LENGTH_CHAR + "here";
		public const string FAKE_TTS = "\\" + ZERO_LENGTH_CHAR + "tts";
		public const int MIN_BITRATE = 8;
		public const int MAX_BITRATE = 96;
		public const int VIP_BITRATE = 128;
		public const int MAX_MESSAGE_LENGTH_LONG = 2000; //Gives a little margin of error.
		public const int MAX_VOICE_CHANNEL_USER_LIMIT = 99;
		public const int MAX_STREAM_LENGTH = 25; //Source: https://www.reddit.com/r/Twitch/comments/32w5b2/username_requirements/cqf8yh0/
		public const int MIN_STREAM_LENGTH = 4;
		public const int MAX_GAME_LENGTH = 128; //Yes, I know it CAN go past that, but it won't show for others.
		public const int MIN_GAME_LENGTH = 0;
		public const int MAX_TOPIC_LENGTH = 1024;
		public const int MIN_TOPIC_LENGTH = 0;
		public const int MAX_GUILD_NAME_LENGTH = 100;
		public const int MIN_GUILD_NAME_LENGTH = 2;
		public const int MAX_CHANNEL_NAME_LENGTH = 100;
		public const int MIN_CHANNEL_NAME_LENGTH = 2;
		public const int MAX_ROLE_NAME_LENGTH = 100;
		public const int MIN_ROLE_NAME_LENGTH = 1;
		public const int MAX_NICKNAME_LENGTH = 32;
		public const int MIN_NICKNAME_LENGTH = 1;
		public const int MAX_USERNAME_LENGTH = 32;
		public const int MIN_USERNAME_LENGTH = 2;

		/*
		private const ChannelPermission GENERAL_BITS = 0
			| ChannelPermission.ViewChannel
			| ChannelPermission.CreateInstantInvite
			| ChannelPermission.ManageChannels
			| ChannelPermission.ManageRoles
			| ChannelPermission.ManageWebhooks;
		private const ChannelPermission TEXT_BITS = 0
			| ChannelPermission.SendMessages
			| ChannelPermission.SendTTSMessages
			| ChannelPermission.ManageMessages
			| ChannelPermission.EmbedLinks
			| ChannelPermission.AttachFiles
			| ChannelPermission.ReadMessageHistory
			| ChannelPermission.MentionEveryone
			| ChannelPermission.UseExternalEmojis
			| ChannelPermission.AddReactions;
		private const ChannelPermission VOICE_BITS = 0
			| ChannelPermission.Connect
			| ChannelPermission.Speak
			| ChannelPermission.MuteMembers
			| ChannelPermission.DeafenMembers
			| ChannelPermission.MoveMembers
			| ChannelPermission.UseVAD;*/

		public const ChannelPermission MUTE_ROLE_TEXT_PERMS = 0
			| ChannelPermission.CreateInstantInvite
			| ChannelPermission.ManageChannels
			| ChannelPermission.ManageRoles
			| ChannelPermission.ManageWebhooks
			| ChannelPermission.SendMessages
			| ChannelPermission.ManageMessages
			| ChannelPermission.AddReactions;
		public const ChannelPermission MUTE_ROLE_VOICE_PERMS = 0
			| ChannelPermission.CreateInstantInvite
			| ChannelPermission.ManageChannels
			| ChannelPermission.ManageRoles
			| ChannelPermission.ManageWebhooks
			| ChannelPermission.Speak
			| ChannelPermission.MuteMembers
			| ChannelPermission.DeafenMembers
			| ChannelPermission.MoveMembers;

		public const GuildPermission USER_HAS_A_PERMISSION_PERMS = 0
			| GuildPermission.Administrator
			| GuildPermission.BanMembers
			| GuildPermission.DeafenMembers
			| GuildPermission.KickMembers
			| GuildPermission.ManageChannels
			| GuildPermission.ManageEmojis
			| GuildPermission.ManageGuild
			| GuildPermission.ManageMessages
			| GuildPermission.ManageNicknames
			| GuildPermission.ManageRoles
			| GuildPermission.ManageWebhooks
			| GuildPermission.MoveMembers
			| GuildPermission.MuteMembers;

		//Regex for checking any awaits are non ConfigureAwait(false): ^(?!.*CAF\(\)).*await.*$
		public const string PROGRAM_NAME = "Advobot";
		public const string IGNORE_ERROR = "xd";
		public const string DISCORD_INV = "https://discord.gg/MBXypxb"; //Switched from /xd to this invite since no matter what this inv will link to my server and never someone else's server
		public const string TWITCH_URL = "https://www.twitch.tv/";
		public const string REPO = "https://github.com/advorange/Advobot";
		public const string VIP_REGIONS = "VIP_REGIONS";
		public const string VANITY_URL = "VANITY_URL";
		public const string INVITE_SPLASH = "INVITE_SPLASH";
		public const string MUTE_ROLE_NAME = "Advobot_Mute";
		public const string SERVER_FOLDER = "Discord_Servers";
		public const string SETTING_FILE_EXTENSION = ".json";
		public const string GENERAL_FILE_EXTENSION = ".txt";
		public const string IMAGE_FILE_EXTENSION = ".png";
		public const string GUILD_SETTINGS_LOC = "GuildSettings" + SETTING_FILE_EXTENSION;
		public const string BOT_SETTINGS_LOC = "BotSettings" + SETTING_FILE_EXTENSION;
		public const string UI_INFO_LOC = "UISettings" + SETTING_FILE_EXTENSION;
		public const string CRASH_LOG_LOC = "CrashLog" + GENERAL_FILE_EXTENSION;
		public const string BOT_ICON_LOC = "BotIcon" + IMAGE_FILE_EXTENSION;
		public const string GUILD_ICON_LOC = "GuildIcon" + IMAGE_FILE_EXTENSION;
		public const int SECONDS_DEFAULT = 3;
		public const int TICKS_REGEX_TIMEOUT = 1000000;
		public const int MIN_REGEX_LENGTH = 1;
		public const int MAX_REGEX_LENGTH = 100;
		public const int MIN_PREFIX_LENGTH = 1;
		public const int MAX_PREFIX_LENGTH = 10;
		public const int MIN_RULE_CATEGORY_LENGTH = 1;
		public const int MAX_RULE_CATEGORY_LENGTH = 250;
		public const int MIN_RULE_LENGTH = 1;
		public const int MAX_RULE_LENGTH = 150;
		public const int MAX_CATEGORIES = 20;
		public const int MAX_RULES_PER_CATEGORY = 20;
		public const int MAX_SA_GROUPS = 10;
		public const int MAX_QUOTES = 50;
		public const int MAX_BANNED_STRINGS = 50;
		public const int MAX_BANNED_REGEX = 25;
		public const int MAX_BANNED_NAMES = 25;
		public const int MAX_BANNED_PUNISHMENTS = 10;
		public const int MAX_ICON_FILE_SIZE = 2500000;
		public const int MAX_UTF16_VAL_FOR_NAMES = 1000;

		public static string BotVersion => Version.VERSION_NUMBER;
		private static string _ApiVersion;
		public static string ApiVersion => _ApiVersion ?? (_ApiVersion = Assembly.GetAssembly(typeof(IDiscordClient)).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
		private static ImmutableList<string> _ValidImageExtensions;
		public static ImmutableList<string> ValidImageExtensions => _ValidImageExtensions ?? (_ValidImageExtensions = ImmutableList.Create(".jpeg", ".jpg", ".png"));
		private static ImmutableList<string> _ValidGifExtentions;
		public static ImmutableList<string> ValidGifExtentions => _ValidGifExtentions ?? (_ValidGifExtentions = ImmutableList.Create(".gif", ".gifv"));
		private static ImmutableList<Assembly> _CommandAssemblies;
		public static ImmutableList<Assembly> CommandAssemblies => _CommandAssemblies ?? (_CommandAssemblies = GetCommandAssemblies());
		private static ImmutableDictionary<string, Color> _Colors;
		public static ImmutableDictionary<string, Color> Colors => _Colors ?? (_Colors = GetColorDictionary());
		private static HelpEntryHolder _HelpEntries;
		public static HelpEntryHolder HelpEntries => _HelpEntries ?? (_HelpEntries = new HelpEntryHolder());

		//Colors for logging embeds
		public static Color Base { get; } = new Color(255, 100, 000);
		public static Color Join { get; } = new Color(000, 255, 000);
		public static Color Leave { get; } = new Color(255, 000, 000);
		public static Color UserEdit { get; } = new Color(051, 051, 255);
		public static Color Attachment { get; } = new Color(000, 204, 204);
		public static Color MessageEdit { get; } = new Color(000, 000, 255);
		public static Color MessageDelete { get; } = new Color(255, 051, 051);

		private static ImmutableList<Assembly> GetCommandAssemblies()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetCustomAttribute<CommandAssemblyAttribute>() != null).ToList();
			if (assemblies.Any())
			{
				return assemblies.ToImmutableList();
			}

			ConsoleUtils.WriteLine($"Unable to find any command assemblies. Press any key to close the program.");
			Console.ReadKey();
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