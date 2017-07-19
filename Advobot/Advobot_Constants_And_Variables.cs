using Advobot.Actions;
using Discord;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Advobot
{
	internal static class Constants
	{
		internal const string BOT_VERSION = "0.11.0";
		internal const string API_VERSION = "Discord.Net v1.0.2-build-00800";
		internal const string PROGRAM_NAME = "Advobot";
		internal const string BOT_PREFIX = "&&";
		internal const string ZERO_LENGTH_CHAR = "\u180E";
		internal const string IGNORE_ERROR = "Cx";
		internal const string DISCORD_INV = "https://discord.gg/MBXypxb"; //Switched from /xd to this invite since no matter what this inv will link to my server and never someone else's server
		internal const string TWITCH_URL = "https://www.twitch.tv/";
		internal const string REPO = "https://github.com/advorange/Advobot";
		internal const string VIP_REGIONS = "VIP_REGIONS";
		internal const string VANITY_URL = "VANITY_URL";
		internal const string INVITE_SPLASH = "INVITE_SPLASH";
		internal const string NO_NN = "NO NICKNAME";
		internal const string FAKE_DISCORD_LINK = "discord" + ZERO_LENGTH_CHAR + ".gg";
		internal const string FAKE_EVERYONE = "@" + ZERO_LENGTH_CHAR + "everyone";
		internal const string FAKE_TTS = "\\" + ZERO_LENGTH_CHAR + "tts";
		internal const string BYPASS_STRING = "Bypass100";
		internal const string MUTE_ROLE_NAME = "Advobot_Mute";
		internal const string CHANNEL_INSTRUCTIONS = "#Channel|\"Channel Name\"";
		internal const string USER_INSTRUCTIONS = "@User|\"Username\"";
		internal const string ROLE_INSTRUCTIONS = "@Role|\"Role Name\"";
		internal const string VOICE_TYPE = "voice";
		internal const string TEXT_TYPE = "text";
		internal const string BASIC_TYPE_USER = "user";
		internal const string BASIC_TYPE_ROLE = "role";
		internal const string BASIC_TYPE_CHANNEL = "channel";
		internal const string BASIC_TYPE_GUILD = "guild";
		internal const string ERROR_MESSAGE = "**ERROR:** ";
		internal const string PATH_ERROR = "The bot does not have a valid path to save to/read from.";
		internal const string SERVER_FOLDER = "Discord_Servers";
		internal const string SETTING_FILE_EXTENSION = ".json";
		internal const string GENERAL_FILE_EXTENSION = ".txt";
		internal const string GUILD_SETTINGS_LOCATION = "GuildSettings" + SETTING_FILE_EXTENSION;
		internal const string BOT_SETTINGS_LOCATION = "BotSettings" + SETTING_FILE_EXTENSION;
		internal const string UI_INFO_LOCATION = "UISettings" + SETTING_FILE_EXTENSION;
		internal const string BOT_ICON_LOCATION = "BotIcon";
		internal const string GUILD_ICON_LOCATION = "GuildIcon";

		internal const int SECONDS_DEFAULT = 3;
		internal const int SECONDS_ACTIVE_CLOSE = 5;
		internal const int TICKS_REGEX_TIMEOUT = 1000000;
		internal const int MEMBER_LIMIT = 0;
		internal const int MAX_LENGTH_FOR_REGEX = 100;
		internal const int MAX_LENGTH_FOR_REASON = 512;
		internal const int MAX_SA_GROUPS = 10;
		internal const int MAX_QUOTES = 50;
		internal const int MAX_BANNED_STRINGS = 50;
		internal const int MAX_BANNED_REGEX = 25;
		internal const int MAX_BANNED_NAMES = 25;
		internal const int MAX_ICON_FILE_SIZE = 2500000;
		internal const int MAX_UTF16_VAL_FOR_NAMES = 1000;
		internal const int AMT_OF_DMS_TO_GATHER = 500;
		internal const int VALID_KEY_LENGTH = 59; //This probably shouldn't be hardcoded in tbh
		internal const int MIN_BITRATE = 8;
		internal const int MAX_BITRATE = 96;
		internal const int VIP_BITRATE = 128;
		internal const int MAX_MESSAGE_LENGTH_LONG = 1900; //Gives a little margin of error.
		internal const int MAX_MESSAGE_LENGTH_SHORT = 750;
		internal const int MAX_VOICE_CHANNEL_USER_LIMIT = 99;
		internal const int MAX_STREAM_LENGTH = 25; //Source: https://www.reddit.com/r/Twitch/comments/32w5b2/username_requirements/cqf8yh0/
		internal const int MIN_STREAM_LENGTH = 4;
		internal const int MAX_GAME_LENGTH = 128; //Yes, I know it CAN go past that, but it won't show for others.
		internal const int MIN_GAME_LENGTH = -1;
		internal const int MAX_TOPIC_LENGTH = 1024;
		internal const int MIN_TOPIC_LENGTH = -1;
		internal const int MAX_GUILD_NAME_LENGTH = 100;
		internal const int MIN_GUILD_NAME_LENGTH = 2;
		internal const int MAX_CHANNEL_NAME_LENGTH = 100;
		internal const int MIN_CHANNEL_NAME_LENGTH = 2;
		internal const int MAX_ROLE_NAME_LENGTH = 100;
		internal const int MIN_ROLE_NAME_LENGTH = 1;
		internal const int MAX_NICKNAME_LENGTH = 32;
		internal const int MIN_NICKNAME_LENGTH = 1;
		internal const int MAX_USERNAME_LENGTH = 32;
		internal const int MIN_USERNAME_LENGTH = 2;
		internal const int MAX_EMBED_TOTAL_LENGTH = 6000;
		internal const int MAX_TITLE_LENGTH = 256;
		internal const int MAX_FOOTER_LENGTH = 2048;
		internal const int MAX_DESCRIPTION_LINES = 20;
		internal const int MAX_DESCRIPTION_LENGTH = 2048;
		internal const int MAX_FIELDS = 25;
		internal const int MAX_FIELD_LINES = 5;
		internal const int MAX_FIELD_NAME_LENGTH = 256;
		internal const int MAX_FIELD_VALUE_LENGTH = 1024;

		private static ReadOnlyCollection<string> _VALID_IMAGE_EXTENSIONS;
		internal static ReadOnlyCollection<string> VALID_IMAGE_EXTENSIONS => _VALID_IMAGE_EXTENSIONS ?? (_VALID_IMAGE_EXTENSIONS = new ReadOnlyCollection<string>(new List<string>
		{
			".jpeg",
			".jpg",
			".png",
		}));
		private static ReadOnlyCollection<string> _VALID_GIF_EXTENSIONS;
		internal static ReadOnlyCollection<string> VALID_GIF_EXTENTIONS => _VALID_GIF_EXTENSIONS ?? (_VALID_GIF_EXTENSIONS = new ReadOnlyCollection<string>(new List<string>
		{
			".gif",
			".gifv",
		}));
		private static ReadOnlyCollection<string> _COMMANDS_UNABLE_TO_BE_TURNED_OFF;
		internal static ReadOnlyCollection<string> COMMANDS_UNABLE_TO_BE_TURNED_OFF => _COMMANDS_UNABLE_TO_BE_TURNED_OFF ?? (_COMMANDS_UNABLE_TO_BE_TURNED_OFF = new ReadOnlyCollection<string>(new List<string>
		{
			"configurecommands",
			"help",
		}));
		private static ReadOnlyCollection<string> _TEST_PHRASES;
		internal static ReadOnlyCollection<string> TEST_PHRASES => _TEST_PHRASES ?? (_TEST_PHRASES = new ReadOnlyCollection<string>(new List<string>
		{
			"Ӽ1(",
			"Ϯ3|",
			"⁊a~",
			"[&r",
		}));
		private static ReadOnlyCollection<LogAction> _DEFAULT_LOG_ACTIONS;
		internal static ReadOnlyCollection<LogAction> DEFAULT_LOG_ACTIONS => _DEFAULT_LOG_ACTIONS ?? (_DEFAULT_LOG_ACTIONS = new ReadOnlyCollection<LogAction>(new List<LogAction>
		{
			LogAction.UserJoined,
			LogAction.UserLeft,
			LogAction.MessageReceived,
			LogAction.MessageUpdated,
			LogAction.MessageDeleted,
		}));
		private static ReadOnlyCollection<HelpEntry> _HELP_ENTRIES;
		internal static ReadOnlyCollection<HelpEntry> HELP_ENTRIES => _HELP_ENTRIES ?? (_HELP_ENTRIES = new ReadOnlyCollection<HelpEntry>(SavingAndLoading.LoadHelpList()));
		private static ReadOnlyCollection<string> _COMMAND_NAMES;
		internal static ReadOnlyCollection<string> COMMAND_NAMES => _COMMAND_NAMES ?? (_COMMAND_NAMES = new ReadOnlyCollection<string>(SavingAndLoading.LoadCommandNames(HELP_ENTRIES)));
		private static ReadOnlyCollection<BotGuildPermission> _GUILD_PERMISSIONS; //Stuff has to be in this notation because lol static initializers
		internal static ReadOnlyCollection<BotGuildPermission> GUILD_PERMISSIONS => _GUILD_PERMISSIONS ?? (_GUILD_PERMISSIONS = new ReadOnlyCollection<BotGuildPermission>(SavingAndLoading.LoadGuildPermissions()));
		private static ReadOnlyCollection<BotChannelPermission> _CHANNEL_PERMISSIONS;
		internal static ReadOnlyCollection<BotChannelPermission> CHANNEL_PERMISSIONS => _CHANNEL_PERMISSIONS ?? (_CHANNEL_PERMISSIONS = new ReadOnlyCollection<BotChannelPermission>(SavingAndLoading.LoadChannelPermissions()));

		//Because the enum values might change in the future. These are never saved in JSON so these can be modified
		private static ReadOnlyDictionary<PunishmentType, int> _PUNISHMENT_SEVERITY;
		internal static ReadOnlyDictionary<PunishmentType, int> PUNISHMENT_SEVERITY => _PUNISHMENT_SEVERITY ?? (_PUNISHMENT_SEVERITY = new ReadOnlyDictionary<PunishmentType, int>(new Dictionary<PunishmentType, int>
		{
			{ PunishmentType.Deafen, 0 },
			{ PunishmentType.VoiceMute, 100 },
			{ PunishmentType.RoleMute, 250 },
			{ PunishmentType.Kick, 500 },
			{ PunishmentType.KickThenBan, 750 },
			{ PunishmentType.Ban, 1000 },
		}));
		private static ReadOnlyDictionary<string, Color> _COLORS;
		internal static ReadOnlyDictionary<string, Color> COLORS => _COLORS ?? (_COLORS = new ReadOnlyDictionary<string, Color>(Gets.GetColorDictionary()));

		internal static Color BASE { get; } = new Color(255, 100, 000);
		internal static Color JOIN { get; } = new Color(000, 255, 000);
		internal static Color LEAV { get; } = new Color(255, 000, 000);
		internal static Color UEDT { get; } = new Color(051, 051, 255);
		internal static Color ATCH { get; } = new Color(000, 204, 204);
		internal static Color MEDT { get; } = new Color(000, 000, 255);
		internal static Color MDEL { get; } = new Color(255, 051, 051);

		//Redefine these to whatever type you want for guild settings and global settings (they must inherit their respective setting interfaces)
		internal static Type GUILDS_SETTINGS_TYPE { get; } = typeof(MyGuildSettings);
		internal static Type GLOBAL_SETTINGS_TYPE { get; } = typeof(MyBotSettings);
	}

	public static class Variables
	{
		//TODO: invite module? not very important tbh
		public readonly static List<ListedInvite> InviteList = new List<ListedInvite>();

	}
}