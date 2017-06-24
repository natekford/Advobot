using Discord;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Advobot
{
	public static class Constants
	{
		public const string BOT_VERSION = "0.10.8";
		public const string API_VERSION = "Discord.Net v1.0.0-rc3-00755";
		public const string BOT_PREFIX = "&&";
		public const string ZERO_LENGTH_CHAR = "\u180E";
		public const string IGNORE_ERROR = "Cx";
		public const string DISCORD_INV = "https://discord.gg/MBXypxb"; //Switched from /xd to this invite since no matter what this inv will link to my server and never someone else's server
		public const string STREAM_URL = "https://www.twitch.tv/";
		public const string REPO = "https://github.com/advorange/Advobot";
		public const string VIP_REGIONS = "VIP_REGIONS";
		public const string VANITY_URL = "VANITY_URL";
		public const string INVITE_SPLASH = "INVITE_SPLASH";
		public const string NO_NN = "NO NICKNAME";
		public const string FAKE_EVERYONE = "@" + ZERO_LENGTH_CHAR + "everyone";
		public const string FAKE_TTS = "\\" + ZERO_LENGTH_CHAR + "tts";
		public const string BYPASS_STRING = "Bypass100";

		public const string ERROR_MESSAGE = "**ERROR:** ";
		public const string ROLE_ERROR = "None of the targetted roles were valid.";
		public const string CHANNEL_ERROR = "None of the targetted channels were valid.";
		public const string ARGUMENTS_ERROR = "An invalid number of arguments was supplied.";
		public const string CHANNEL_PERMISSIONS_ERROR = "You do not have the ability to edit that channel.";
		public const string PATH_ERROR = "The bot does not have a valid path to save to/read from.";
		public const string DENY_WITHOUT_PREFERENCES = "This guild does not have preferences enabled and thus cannot use this command. Please run the `comconfigmodify` command to enable them.";

		public const string SERVER_FOLDER = "Discord_Servers";
		public const string SAVING_FILE_EXTENSION = ".json";
		public const string GENERAL_FILE_EXTENSION = ".txt";
		public const string GUILD_INFO_LOCATION = "GuildInfo" + SAVING_FILE_EXTENSION;
		public const string BOT_INFO_LOCATION = "BotInfo" + SAVING_FILE_EXTENSION;
		public const string UI_INFO_LOCATION = "BotUIInfo" + SAVING_FILE_EXTENSION;

		public const string VOICE_TYPE = "voice";
		public const string TEXT_TYPE = "text";
		public const string BASIC_TYPE_USER = "user";
		public const string BASIC_TYPE_ROLE = "role";
		public const string BASIC_TYPE_CHANNEL = "channel";
		public const string BASIC_TYPE_GUILD = "guild";

		//In case I ever need to have a prefix for these to have better arg parsing
		public const string CHAN = "";
		public const string ROLE = "";
		public const string USER = "";
		public const string CHANNEL_INSTRUCTIONS = CHAN + "#Channel|\"Channel Name\"";
		public const string USER_INSTRUCTIONS = USER + "@User|\"Username\"";
		public const string ROLE_INSTRUCTIONS = ROLE + "@Role|\"Role Name\"";

		public const double PERCENT_AVERAGE = .75;
		public const int TIME_TO_WAIT_BEFORE_MESSAGE_PRINT_TO_THE_SERVER_LOG = 3;
		public const int WAIT_TIME = 3000;
		public const int MEMBER_LIMIT = 0;
		public const int MAX_SA_GROUPS = 10;
		public const int MAX_REMINDS = 50;
		public const int MAX_BANNED_STRINGS = 50;
		public const int MAX_BANNED_REGEX = 25;
		public const int MAX_BANNED_NAMES = 25;
		public const int MESSAGES_TO_GATHER = 100;
		public const int PAD_RIGHT = 20;
		public const int ACTIVE_CLOSE = 5000;
		public const int REGEX_TIMEOUT = 1000000;

		public const int MIN_BITRATE = 8;
		public const int MAX_BITRATE = 96;
		public const int VIP_BITRATE = 128;
		public const int MAX_MESSAGE_LENGTH_LONG = 1900;
		public const int MAX_MESSAGE_LENGTH_SHORT = 750;
		public const int MAX_NICKNAME_LENGTH = 32;
		public const int MIN_NICKNAME_LENGTH = 2;
		public const int MAX_CHANNEL_NAME_LENGTH = 100;
		public const int MIN_CHANNEL_NAME_LENGTH = 2;
		public const int MAX_ROLE_NAME_LENGTH = 100;
		public const int MIN_ROLE_NAME_LENGTH = 1;
		public const int MAX_TOPIC_LENGTH = 1024;
		public const int MAX_GAME_LENGTH = 128; //Yes, I know it CAN go past that, but it won't show for others.
		public const int MAX_EMBED_LENGTH_LONG = 2048;
		public const int MAX_EMBED_LENGTH_SHORT = 1024;
		public const int MAX_TITLE_LENGTH = 256;
		public const int MAX_FIELDS = 25;
		public const int MAX_DESCRIPTION_LINES = 20;
		public const int MAX_FIELD_LINES = 5;
		public const int MAX_LENGTH_FOR_FIELD_VALUE = 250000;
		public const int MAX_LENGTH_FOR_REGEX = 100;

		public static ReadOnlyCollection<string> VALID_IMAGE_EXTENSIONS = new ReadOnlyCollection<string>(new List<string>()
		{
			".jpeg", ".jpg", ".png",
		});
		public static ReadOnlyCollection<string> VALID_GIF_EXTENTIONS = new ReadOnlyCollection<string>(new List<string>()
		{
			".gif", ".gifv",
		});
		public static ReadOnlyCollection<string> VALID_REGION_IDS = new ReadOnlyCollection<string>(new List<string>()
		{
			"brazil", "eu-central", "eu-west", "hongkong", "russia", "singapore", "sydney", "us-east", "us-central", "us-south", "us-west",
		});
		public static ReadOnlyCollection<string> VIP_REGIONIDS = new ReadOnlyCollection<string>(new List<string>()
		{
			"vip-amsterdam", "vip-us-east", "vip-us-west",
		});
		public static ReadOnlyCollection<string> COMMANDS_UNABLE_TO_BE_TURNED_OFF = new ReadOnlyCollection<string>(new List<string>()
		{
			"configurecommands", "help",
		});
		public static ReadOnlyCollection<string> TEST_PHRASES = new ReadOnlyCollection<string>(new List<string>()
		{
			"Ӽ1(", "Ϯ3|", "⁊a~", "[&r",
		});

		public static ReadOnlyCollection<LogActions> DEFAULT_LOG_ACTIONS = new ReadOnlyCollection<LogActions>(new List<LogActions>()
		{
			LogActions.UserJoined,
			LogActions.UserLeft,
			LogActions.MessageReceived,
			LogActions.MessageUpdated,
			LogActions.MessageDeleted,
		});

		public const bool DISCONNECT = false;

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
		public readonly static List<BotGuildPermissionType> GuildPermissions = new List<BotGuildPermissionType>();
		public readonly static List<BotChannelPermissionType> ChannelPermissions = new List<BotChannelPermissionType>();

		//Lists that change as the bot is used
		public readonly static List<ListedInvite> InviteList = new List<ListedInvite>();
		public readonly static List<RemovablePunishment> PunishedUsers = new List<RemovablePunishment>();
		public readonly static List<RemovableMessage> TimedMessages = new List<RemovableMessage>();
		public readonly static List<ActiveCloseHelp> ActiveCloseHelp = new List<ActiveCloseHelp>();
		public readonly static List<ActiveCloseWords> ActiveCloseWords = new List<ActiveCloseWords>();
		public readonly static List<SlowmodeUser> SlowmodeUsers = new List<SlowmodeUser>();
		public readonly static List<IGuild> GuildsToBeLoaded = new List<IGuild>();
		public readonly static List<ulong> GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts = new List<ulong>();
	}
}