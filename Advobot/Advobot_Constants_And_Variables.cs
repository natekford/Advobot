using Discord;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Advobot
{
	public static class Constants
	{
		public const string BOT_VERSION = "0.11.0";
		public const string API_VERSION = "Discord.Net v1.0.2-build-00795";
		public const string BOT_PREFIX = "&&";
		public const string ZERO_LENGTH_CHAR = "\u180E";
		public const string IGNORE_ERROR = "Cx";
		public const string DISCORD_INV = "https://discord.gg/MBXypxb"; //Switched from /xd to this invite since no matter what this inv will link to my server and never someone else's server
		public const string TWITCH_URL = "https://www.twitch.tv/";
		public const string REPO = "https://github.com/advorange/Advobot";
		public const string VIP_REGIONS = "VIP_REGIONS";
		public const string VANITY_URL = "VANITY_URL";
		public const string INVITE_SPLASH = "INVITE_SPLASH";
		public const string NO_NN = "NO NICKNAME";
		public const string FAKE_DISCORD_LINK = "discord" + ZERO_LENGTH_CHAR + ".gg";
		public const string FAKE_EVERYONE = "@" + ZERO_LENGTH_CHAR + "everyone";
		public const string FAKE_TTS = "\\" + ZERO_LENGTH_CHAR + "tts";
		public const string BYPASS_STRING = "Bypass100";

		public const string ERROR_MESSAGE = "**ERROR:** ";
		public const string PATH_ERROR = "The bot does not have a valid path to save to/read from.";
		public const string ROLE_ERROR = "TODO: remove";
		public const string CHANNEL_ERROR = ROLE_ERROR;
		public const string ARGUMENTS_ERROR = CHANNEL_ERROR;

		public const string SERVER_FOLDER = "Discord_Servers";
		public const string SAVING_FILE_EXTENSION = ".json";
		public const string GENERAL_FILE_EXTENSION = ".txt";
		public const string GUILD_INFO_LOCATION = "GuildInfo" + SAVING_FILE_EXTENSION;
		public const string BOT_INFO_LOCATION = "BotInfo" + SAVING_FILE_EXTENSION;
		public const string UI_INFO_LOCATION = "BotUIInfo" + SAVING_FILE_EXTENSION;
		public const string BOT_ICON_LOCATION = "BotIcon";
		public const string GUILD_ICON_LOCATION = "GuildIcon";

		public const string VOICE_TYPE = "voice";
		public const string TEXT_TYPE = "text";
		public const string BASIC_TYPE_USER = "user";
		public const string BASIC_TYPE_ROLE = "role";
		public const string BASIC_TYPE_CHANNEL = "channel";
		public const string BASIC_TYPE_GUILD = "guild";

		public const string CHANNEL_INSTRUCTIONS = "#Channel|\"Channel Name\"";
		public const string USER_INSTRUCTIONS = "@User|\"Username\"";
		public const string ROLE_INSTRUCTIONS = "@Role|\"Role Name\"";

		public const int TIME_TO_WAIT_BEFORE_MESSAGE_PRINT_TO_THE_SERVER_LOG = 3;
		public const int WAIT_TIME = 3000;
		public const int ACTIVE_CLOSE = 5000;
		public const int REGEX_TIMEOUT = 1000000;

		public const double PERCENT_AVERAGE = .75;
		public const int MEMBER_LIMIT = 0;
		public const int MAX_LENGTH_FOR_REGEX = 100;
		public const int MAX_LENGTH_FOR_REASON = 512;
		public const int MAX_SA_GROUPS = 10;
		public const int MAX_QUOTES = 50;
		public const int MAX_BANNED_STRINGS = 50;
		public const int MAX_BANNED_REGEX = 25;
		public const int MAX_BANNED_NAMES = 25;
		public const int MAX_ICON_FILE_SIZE = 2500000;
		public const int MAX_UTF16_VAL_FOR_NAMES = 1000;

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

		public static ReadOnlyCollection<string> VALID_IMAGE_EXTENSIONS = new ReadOnlyCollection<string>(new List<string>
		{
			".jpeg", ".jpg", ".png",
		});
		public static ReadOnlyCollection<string> VALID_GIF_EXTENTIONS = new ReadOnlyCollection<string>(new List<string>
		{
			".gif", ".gifv",
		});
		public static ReadOnlyCollection<string> VALID_REGION_IDS = new ReadOnlyCollection<string>(new List<string>
		{
			"brazil", "eu-central", "eu-west", "hongkong", "russia", "singapore", "sydney", "us-east", "us-central", "us-south", "us-west",
		});
		public static ReadOnlyCollection<string> VIP_REGIONIDS = new ReadOnlyCollection<string>(new List<string>
		{
			"vip-amsterdam", "vip-us-east", "vip-us-west",
		});
		public static ReadOnlyCollection<string> COMMANDS_UNABLE_TO_BE_TURNED_OFF = new ReadOnlyCollection<string>(new List<string>
		{
			"configurecommands", "help",
		});
		public static ReadOnlyCollection<string> TEST_PHRASES = new ReadOnlyCollection<string>(new List<string>
		{
			"Ӽ1(", "Ϯ3|", "⁊a~", "[&r",
		});
		public static ReadOnlyCollection<uint> VALID_AFK_TIMES = new ReadOnlyCollection<uint>(new List<uint>
		{
			60, 300, 900, 1800, 3600,
		});
		public static ReadOnlyCollection<LogAction> DEFAULT_LOG_ACTIONS = new ReadOnlyCollection<LogAction>(new List<LogAction>
		{
			LogAction.UserJoined,
			LogAction.UserLeft,
			LogAction.MessageReceived,
			LogAction.MessageUpdated,
			LogAction.MessageDeleted,
		});

		//Because the enum values might change in the future. These are never saved in JSON so these can be modified
		public static ReadOnlyDictionary<PunishmentType, int> Severity = new ReadOnlyDictionary<PunishmentType, int>(new Dictionary<PunishmentType, int>
		{
			{ PunishmentType.Deafen, 0 },
			{ PunishmentType.Mute, 100 },
			{ PunishmentType.Role, 250 },
			{ PunishmentType.Kick, 500 },
			{ PunishmentType.KickThenBan, 750 },
			{ PunishmentType.Ban, 1000 },
		});
		public static ReadOnlyDictionary<string, Color> Colors = new ReadOnlyDictionary<string, Color>(Actions.CreateColorDictionary());

		public static readonly Color BASE = new Color(255, 100, 000);
		public static readonly Color JOIN = new Color(000, 255, 000);
		public static readonly Color LEAV = new Color(255, 000, 000);
		public static readonly Color UNBN = new Color(000, 153, 000);
		public static readonly Color BANN = new Color(153, 000, 000);
		public static readonly Color UEDT = new Color(051, 051, 255);
		public static readonly Color ATCH = new Color(000, 204, 204);
		public static readonly Color MEDT = new Color(000, 000, 255);
		public static readonly Color MDEL = new Color(255, 051, 051);
		public static readonly Color RCRE = new Color(000, 175, 000);
		public static readonly Color REDT = new Color(000, 000, 204);
		public static readonly Color RDEL = new Color(175, 000, 000);
		public static readonly Color CCRE = new Color(000, 204, 000);
		public static readonly Color CEDT = new Color(000, 000, 153);
		public static readonly Color CDEL = new Color(204, 000, 000);
	}

	public static class Variables
	{
		public static ulong BotID = 0;
		public static string BotName;
		public static BotClient Client;
		public static BotGlobalInfo BotInfo;

		public static bool Windows = true;
		public static bool Loaded = false;
		public static bool Console = true;
		public static bool GotPath = false;
		public static bool GotKey = false;
		public static bool Pause = false;
		public static bool FirstInstanceOfBotStartingUpWithCurrentKey = true;

		public static DateTime StartupTime = DateTime.UtcNow;
		public static Timer HourTimer;
		public static Timer MinuteTimer;
		public static Timer OneFourthSecondTimer;

		public static int TotalUsers = 0;
		public static int TotalGuilds = 0;
		public static int AttemptedCommands = 0;
		public static int FailedCommands = 0;
		public static int LoggedJoins = 0;
		public static int LoggedLeaves = 0;
		public static int LoggedBans = 0;
		public static int LoggedUnbans = 0;
		public static int LoggedUserChanges = 0;
		public static int LoggedEdits = 0;
		public static int LoggedDeletes = 0;
		public static int LoggedMessages = 0;
		public static int LoggedImages = 0;
		public static int LoggedGifs = 0;
		public static int LoggedFiles = 0;

		public static Dictionary<ulong, BotGuildInfo> Guilds = new Dictionary<ulong, BotGuildInfo>();
		public static SortedDictionary<string, List<string>> WrittenLines = new SortedDictionary<string, List<string>>();

		//Lists that can only be modified through code for the most part
		public readonly static List<string> CommandNames = new List<string>();
		public readonly static List<string> RegionIDs = new List<string>();
		public readonly static List<HelpEntry> HelpList = new List<HelpEntry>();
		public readonly static List<BotGuildPermission> GuildPermissions = new List<BotGuildPermission>();
		public readonly static List<BotChannelPermission> ChannelPermissions = new List<BotChannelPermission>();

		//Lists that change as the bot is used
		public readonly static List<ListedInvite> InviteList = new List<ListedInvite>();
		public readonly static List<RemovablePunishment> PunishedUsers = new List<RemovablePunishment>();
		public readonly static List<RemovableMessage> TimedMessages = new List<RemovableMessage>();
		public readonly static List<ActiveCloseWord<HelpEntry>> ActiveCloseHelp = new List<ActiveCloseWord<HelpEntry>>();
		public readonly static List<ActiveCloseWord<Quote>> ActiveCloseWords = new List<ActiveCloseWord<Quote>>();
		public readonly static List<SlowmodeUser> SlowmodeUsers = new List<SlowmodeUser>();
		public readonly static List<IGuild> GuildsToBeLoaded = new List<IGuild>();
		public readonly static List<ulong> GuildsToldBotDoesntWorkWithoutAdmin = new List<ulong>();
	}
}