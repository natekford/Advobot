using Advobot.Actions;
using Advobot.Classes;
using Advobot.Classes.Settings;
using Advobot.Enums;
using Discord;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;

namespace Advobot
{
	public static class Constants
	{
		//Const for attributes/because they're very unlikely to change. 
		public const string ZERO_LENGTH_CHAR = "\u180E";
		public const string PLACEHOLDER_PREFIX = ZERO_LENGTH_CHAR + "%PREFIX%";
		public const string USER_MENTION = "%USERMENTION%";
		public const string USER_STRING = "%USER%";
		public const string FAKE_DISCORD_LINK = "discord" + ZERO_LENGTH_CHAR + ".gg";
		public const string FAKE_EVERYONE = "@" + ZERO_LENGTH_CHAR + "everyone";
		public const string FAKE_HERE = "@" + ZERO_LENGTH_CHAR + "here";
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
		public static readonly string API_VERSION = "Discord.Net v2.0.0-alpha-build-00828";
		public static readonly string PROGRAM_NAME = "Advobot";
		public static readonly string IGNORE_ERROR = "Cx";
		public static readonly string DISCORD_INV = "https://discord.gg/MBXypxb"; //Switched from /xd to this invite since no matter what this inv will link to my server and never someone else's server
		public static readonly string TWITCH_URL = "https://www.twitch.tv/";
		public static readonly string REPO = "https://github.com/advorange/Advobot";
		public static readonly string VIP_REGIONS = "VIP_REGIONS";
		public static readonly string VANITY_URL = "VANITY_URL";
		public static readonly string INVITE_SPLASH = "INVITE_SPLASH";
		public static readonly string MUTE_ROLE_NAME = "Advobot_Mute";
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

		//Because the enum values might change in the future. These are never saved in JSON so these can be modified
		private static ImmutableDictionary<PunishmentType, int> _P_SEV;
		public static ImmutableDictionary<PunishmentType, int> PUNISHMENT_SEVERITY => _P_SEV ?? (_P_SEV = new Dictionary<PunishmentType, int>
		{
			{ PunishmentType.Deafen, 0 },
			{ PunishmentType.VoiceMute, 100 },
			{ PunishmentType.RoleMute, 250 },
			{ PunishmentType.Kick, 500 },
			{ PunishmentType.Softban, 750 },
			{ PunishmentType.Ban, 1000 },
		}.ToImmutableDictionary());

		//Redefine these to whatever type you want for guild settings and global settings (they must inherit their respective setting interfaces)
		public static Type GUILD_SETTINGS_TYPE { get; } = typeof(GuildSettings); //IGuildSettings
		public static Type BOT_SETTINGS_TYPE { get; } = typeof(BotSettings); //IBotSettings
	}

	public static class Colors
	{
		private static ImmutableDictionary<string, Color> _COLORS;
		public static ImmutableDictionary<string, Color> COLORS => _COLORS ?? (_COLORS = GetActions.GetColorDictionary().ToImmutableDictionary());

		public static Color BASE { get; } = new Color(255, 100, 000);
		public static Color JOIN { get; } = new Color(000, 255, 000);
		public static Color LEAV { get; } = new Color(255, 000, 000);
		public static Color UEDT { get; } = new Color(051, 051, 255);
		public static Color ATCH { get; } = new Color(000, 204, 204);
		public static Color MEDT { get; } = new Color(000, 000, 255);
		public static Color MDEL { get; } = new Color(255, 051, 051);
	}
}