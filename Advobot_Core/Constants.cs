using Advobot.Actions;
using Advobot.Classes;
using Advobot.Enums;
using Discord;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Advobot
{
	public static class Constants
	{
		//Const for attributes/because they're very unlikely to change. 
		public const string ZERO_LENGTH_CHAR = "\u180E";
		public const string BOT_PREFIX = ZERO_LENGTH_CHAR + "PREFIX";
		public const string FAKE_DISCORD_LINK = "discord" + ZERO_LENGTH_CHAR + ".gg";
		public const string FAKE_EVERYONE = "@" + ZERO_LENGTH_CHAR + "everyone";
		public const string FAKE_TTS = "\\" + ZERO_LENGTH_CHAR + "tts";
		public const string BYPASS_STRING = "Bypass100";
		public const int MIN_BITRATE = 8;
		public const int MAX_BITRATE = 96;
		public const int VIP_BITRATE = 128;
		public const int MAX_MESSAGE_LENGTH_LONG = 1900; //Gives a little margin of error.
		public const int MAX_MESSAGE_LENGTH_SHORT = 750;
		public const int MAX_VOICE_CHANNEL_USER_LIMIT = 99;
		public const int MAX_STREAM_LENGTH = 25; //Source: https://www.reddit.com/r/Twitch/comments/32w5b2/username_requirements/cqf8yh0/
		public const int MIN_STREAM_LENGTH = 4;
		public const int MAX_GAME_LENGTH = 128; //Yes, I know it CAN go past that, but it won't show for others.
		public const int MIN_GAME_LENGTH = -1;
		public const int MAX_TOPIC_LENGTH = 1024;
		public const int MIN_TOPIC_LENGTH = -1;
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

		//Static readonly because they may change and I've heard using const means any assembly referencing it has to be recompiled each time the value gets manually changed.
		public static readonly string BOT_VERSION = "0.31.0";
		public static readonly string API_VERSION = "Discord.Net v2.0.0-alpha-build-00823";
		public static readonly string PROGRAM_NAME = "Advobot";
		public static readonly string IGNORE_ERROR = "Cx";
		public static readonly string DISCORD_INV = "https://discord.gg/MBXypxb"; //Switched from /xd to this invite since no matter what this inv will link to my server and never someone else's server
		public static readonly string TWITCH_URL = "https://www.twitch.tv/";
		public static readonly string REPO = "https://github.com/advorange/Advobot";
		public static readonly string VIP_REGIONS = "VIP_REGIONS";
		public static readonly string VANITY_URL = "VANITY_URL";
		public static readonly string INVITE_SPLASH = "INVITE_SPLASH";
		public static readonly string NO_NN = "No nickname";
		public static readonly string MUTE_ROLE_NAME = "Advobot_Mute";
		public static readonly string CHANNEL_INSTRUCTIONS = "#Channel|\"Channel Name\"";
		public static readonly string USER_INSTRUCTIONS = "@User|\"Username\"";
		public static readonly string ROLE_INSTRUCTIONS = "@Role|\"Role Name\"";
		public static readonly string VOICE_TYPE = "voice";
		public static readonly string TEXT_TYPE = "text";
		public static readonly string BASIC_TYPE_USER = "user";
		public static readonly string BASIC_TYPE_ROLE = "role";
		public static readonly string BASIC_TYPE_CHANNEL = "channel";
		public static readonly string BASIC_TYPE_GUILD = "guild";
		public static readonly string ERROR_MESSAGE = "**ERROR:** ";
		public static readonly string PATH_ERROR = "The bot does not have a valid path to save to/read from.";
		public static readonly string SERVER_FOLDER = "Discord_Servers";
		public static readonly string SETTING_FILE_EXTENSION = ".json";
		public static readonly string GENERAL_FILE_EXTENSION = ".txt";
		public static readonly string GUILD_SETTINGS_LOCATION = "GuildSettings" + SETTING_FILE_EXTENSION;
		public static readonly string BOT_SETTINGS_LOCATION = "BotSettings" + SETTING_FILE_EXTENSION;
		public static readonly string UI_INFO_LOCATION = "UISettings" + SETTING_FILE_EXTENSION;
		public static readonly string CRASH_LOG_LOCATION = "CrashLog" + GENERAL_FILE_EXTENSION;
		public static readonly string BOT_ICON_LOCATION = "BotIcon";
		public static readonly string GUILD_ICON_LOCATION = "GuildIcon";
		public static readonly int SECONDS_DEFAULT = 3;
		public static readonly int SECONDS_ACTIVE_CLOSE = 5;
		public static readonly int TICKS_REGEX_TIMEOUT = 1000000;
		public static readonly int MEMBER_LIMIT = 0;
		public static readonly int MAX_LENGTH_FOR_REGEX = 100;
		public static readonly int MAX_LENGTH_FOR_REASON = 512;
		public static readonly int MAX_SA_GROUPS = 10;
		public static readonly int MAX_QUOTES = 50;
		public static readonly int MAX_BANNED_STRINGS = 50;
		public static readonly int MAX_BANNED_REGEX = 25;
		public static readonly int MAX_BANNED_NAMES = 25;
		public static readonly int MAX_ICON_FILE_SIZE = 2500000;
		public static readonly int MAX_UTF16_VAL_FOR_NAMES = 1000;
		public static readonly int AMT_OF_DMS_TO_GATHER = 500;
		public static readonly int MIN_PREFIX_LENGTH = 1;
		public static readonly int MAX_PREFIX_LENGTH = 10;

		private static ReadOnlyCollection<string> _VALID_IMAGE_EXTENSIONS;
		public static ReadOnlyCollection<string> VALID_IMAGE_EXTENSIONS => _VALID_IMAGE_EXTENSIONS ?? (_VALID_IMAGE_EXTENSIONS = new ReadOnlyCollection<string>(new List<string>
		{
			".jpeg",
			".jpg",
			".png",
		}));
		private static ReadOnlyCollection<string> _VALID_GIF_EXTENSIONS;
		public static ReadOnlyCollection<string> VALID_GIF_EXTENTIONS => _VALID_GIF_EXTENSIONS ?? (_VALID_GIF_EXTENSIONS = new ReadOnlyCollection<string>(new List<string>
		{
			".gif",
			".gifv",
		}));
		private static ReadOnlyCollection<string> _TEST_PHRASES;
		public static ReadOnlyCollection<string> TEST_PHRASES => _TEST_PHRASES ?? (_TEST_PHRASES = new ReadOnlyCollection<string>(new List<string>
		{
			"Ӽ1(",
			"Ϯ3|",
			"⁊a~",
			"[&r",
		}));
		private static ReadOnlyCollection<HelpEntry> _HELP_ENTRIES;
		public static ReadOnlyCollection<HelpEntry> HELP_ENTRIES => _HELP_ENTRIES ?? (_HELP_ENTRIES = new ReadOnlyCollection<HelpEntry>(SavingAndLoadingActions.LoadHelpList()));
		private static ReadOnlyCollection<string> _COMMAND_NAMES;
		public static ReadOnlyCollection<string> COMMAND_NAMES => _COMMAND_NAMES ?? (_COMMAND_NAMES = new ReadOnlyCollection<string>(SavingAndLoadingActions.LoadCommandNames(HELP_ENTRIES)));
		private static ReadOnlyCollection<BotGuildPermission> _GUILD_PERMISSIONS; //Stuff has to be in this notation because lol static initializers
		public static ReadOnlyCollection<BotGuildPermission> GUILD_PERMISSIONS => _GUILD_PERMISSIONS ?? (_GUILD_PERMISSIONS = new ReadOnlyCollection<BotGuildPermission>(SavingAndLoadingActions.LoadGuildPermissions()));
		private static ReadOnlyCollection<BotChannelPermission> _CHANNEL_PERMISSIONS;
		public static ReadOnlyCollection<BotChannelPermission> CHANNEL_PERMISSIONS => _CHANNEL_PERMISSIONS ?? (_CHANNEL_PERMISSIONS = new ReadOnlyCollection<BotChannelPermission>(SavingAndLoadingActions.LoadChannelPermissions()));

		//Because the enum values might change in the future. These are never saved in JSON so these can be modified
		private static ReadOnlyDictionary<PunishmentType, int> _PUNISHMENT_SEVERITY;
		public static ReadOnlyDictionary<PunishmentType, int> PUNISHMENT_SEVERITY => _PUNISHMENT_SEVERITY ?? (_PUNISHMENT_SEVERITY = new ReadOnlyDictionary<PunishmentType, int>(new Dictionary<PunishmentType, int>
		{
			{ PunishmentType.Deafen, 0 },
			{ PunishmentType.VoiceMute, 100 },
			{ PunishmentType.RoleMute, 250 },
			{ PunishmentType.Kick, 500 },
			{ PunishmentType.KickThenBan, 750 },
			{ PunishmentType.Ban, 1000 },
		}));
		private static ReadOnlyDictionary<string, Color> _COLORS;
		public static ReadOnlyDictionary<string, Color> COLORS => _COLORS ?? (_COLORS = new ReadOnlyDictionary<string, Color>(GetActions.GetColorDictionary()));

		public static Color BASE { get; } = new Color(255, 100, 000);
		public static Color JOIN { get; } = new Color(000, 255, 000);
		public static Color LEAV { get; } = new Color(255, 000, 000);
		public static Color UEDT { get; } = new Color(051, 051, 255);
		public static Color ATCH { get; } = new Color(000, 204, 204);
		public static Color MEDT { get; } = new Color(000, 000, 255);
		public static Color MDEL { get; } = new Color(255, 051, 051);

		//Redefine these to whatever type you want for guild settings and global settings (they must inherit their respective setting interfaces)
		public static Type GUILDS_SETTINGS_TYPE { get; } = typeof(MyGuildSettings); //IGuildSettings
		public static Type GLOBAL_SETTINGS_TYPE { get; } = typeof(MyBotSettings); //IBotSettings
	}
}