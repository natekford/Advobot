using Advobot.Core.Actions;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Settings;
using Discord;
using System;
using System.Collections.Immutable;
using System.Reflection;

namespace Advobot
{
	public static class Constants
	{
		//Const for attributes/because they're very unlikely to change. 
		public const string ZERO_LENGTH_CHAR = "\u180E";
		public const string PLACEHOLDER_PREFIX = ZERO_LENGTH_CHAR + "%PREFIX%";
		public const string FAKE_DISCORD_LINK = "discord" + ZERO_LENGTH_CHAR + ".gg";
		public const string FAKE_EVERYONE = "@" + ZERO_LENGTH_CHAR + "everyone";
		public const string FAKE_HERE = "@" + ZERO_LENGTH_CHAR + "here";
		public const string FAKE_TTS = "\\" + ZERO_LENGTH_CHAR + "tts";
		public const int MIN_BITRATE = 8;
		public const int MAX_BITRATE = 96;
		public const int VIP_BITRATE = 128;
		public const int MAX_MESSAGE_LENGTH_LONG = 1900; //Gives a little margin of error.
		public const int MAX_MESSAGE_LENGTH_SHORT = 750;
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
		public const int MAX_EMBED_TOTAL_LENGTH = 6000;
		public const int MAX_TITLE_LENGTH = 256;
		public const int MAX_FOOTER_LENGTH = 2048;
		public const int MAX_DESCRIPTION_LINES = 20;
		public const int MAX_DESCRIPTION_LENGTH = 2048;
		public const int MAX_FIELDS = 25;
		public const int MAX_FIELD_LINES = 5;
		public const int MAX_FIELD_NAME_LENGTH = 256;
		public const int MAX_FIELD_VALUE_LENGTH = 1024;

		//Static because they may change and I've heard using const means any assembly referencing it has to be recompiled each time the value gets manually changed.
		//Regex for checking any awaits are non ConfigureAwait(false): ^(?!.*CAF\(\)).*await.*$
		public static string API_VERSION => Assembly.GetAssembly(typeof(IDiscordClient)).GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
		public static string PROGRAM_NAME => "Advobot";
		public static string IGNORE_ERROR => "Cx";
		public static string DISCORD_INV => "https://discord.gg/MBXypxb"; //Switched from /xd to this invite since no matter what this inv will link to my server and never someone else's server
		public static string TWITCH_URL => "https://www.twitch.tv/";
		public static string REPO => "https://github.com/advorange/Advobot";
		public static string VIP_REGIONS => "VIP_REGIONS";
		public static string VANITY_URL => "VANITY_URL";
		public static string INVITE_SPLASH => "INVITE_SPLASH";
		public static string MUTE_ROLE_NAME => "Advobot_Mute";
		public static string SERVER_FOLDER => "Discord_Servers";
		public static string SETTING_FILE_EXTENSION => ".json";
		public static string GENERAL_FILE_EXTENSION => ".txt";
		public static string GUILD_SETTINGS_LOCATION => "GuildSettings" + SETTING_FILE_EXTENSION;
		public static string BOT_SETTINGS_LOCATION => "BotSettings" + SETTING_FILE_EXTENSION;
		public static string UI_INFO_LOCATION => "UISettings" + SETTING_FILE_EXTENSION;
		public static string CRASH_LOG_LOCATION => "CrashLog" + GENERAL_FILE_EXTENSION;
		public static string BOT_ICON_LOCATION => "BotIcon";
		public static string GUILD_ICON_LOCATION => "GuildIcon";
		public static int SECONDS_DEFAULT => 3;
		public static int SECONDS_ACTIVE_CLOSE => 5;
		public static int TICKS_REGEX_TIMEOUT => 1000000;
		public static int MEMBER_LIMIT => 0;
		public static int MIN_REGEX_LENGTH => 1;
		public static int MAX_REGEX_LENGTH => 100;
		public static int MIN_REASON_LENGTH => 0;
		public static int MAX_REASON_LENGTH => 512;
		public static int MIN_PREFIX_LENGTH => 1;
		public static int MAX_PREFIX_LENGTH => 10;
		public static int MIN_RULE_CATEGORY_LENGTH => 1;
		public static int MAX_RULE_CATEGORY_LENGTH => 250;
		public static int MIN_RULE_LENGTH => 1;
		public static int MAX_RULE_LENGTH => 150;
		public static int MAX_CATEGORIES => 20;
		public static int MAX_RULES_PER_CATEGORY => 20;
		public static int MAX_SA_GROUPS => 10;
		public static int MAX_QUOTES => 50;
		public static int MAX_BANNED_STRINGS => 50;
		public static int MAX_BANNED_REGEX => 25;
		public static int MAX_BANNED_NAMES => 25;
		public static int MAX_BANNED_PUNISHMENTS => 10;
		public static int MAX_ICON_FILE_SIZE => 2500000;
		public static int MAX_UTF16_VAL_FOR_NAMES => 1000;
		public static int AMT_OF_DMS_TO_GATHER => 500;

		private static ImmutableList<string> _IMG;
		public static ImmutableList<string> VALID_IMAGE_EXTENSIONS => _IMG ?? (_IMG = ImmutableList.Create(new[]
		{
			".jpeg",
			".jpg",
			".png",
		}));
		private static ImmutableList<string> _GIF;
		public static ImmutableList<string> VALID_GIF_EXTENTIONS => _GIF ?? (_GIF = ImmutableList.Create(new[]
		{
			".gif",
			".gifv",
		}));
		private static HelpEntryHolder _HELP;
		public static HelpEntryHolder HELP_ENTRIES => _HELP ?? (_HELP = new HelpEntryHolder());
		private static Assembly _CMD_ASSEMBLY;
		public static Assembly COMMAND_ASSEMBLY => _CMD_ASSEMBLY ?? (_CMD_ASSEMBLY = GetActions.GetCommandAssembly());
		private static ImmutableDictionary<string, Color> _COLORS;
		public static ImmutableDictionary<string, Color> COLORS => _COLORS ?? (_COLORS = GetActions.GetColorDictionary());

		//Colors for logging embeds
		public static Color BASE => new Color(255, 100, 000);
		public static Color JOIN => new Color(000, 255, 000);
		public static Color LEAV => new Color(255, 000, 000);
		public static Color UEDT => new Color(051, 051, 255);
		public static Color ATCH => new Color(000, 204, 204);
		public static Color MEDT => new Color(000, 000, 255);
		public static Color MDEL => new Color(255, 051, 051);

		//Redefine these to whatever type you want for guild settings and global settings (they must inherit their respective setting interfaces)
		public static Type GUILD_SETTINGS_TYPE { get; } = typeof(GuildSettings); //IGuildSettings
		public static Type BOT_SETTINGS_TYPE { get; } = typeof(BotSettings); //IBotSettings
	}
}