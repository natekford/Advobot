using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Advobot
{
	public static class Constants
	{
		//Client config
		public const bool ALWAYS_DOWNLOAD_USERS = true;
		public const int CACHED_MESSAGE_COUNT = 10000;
		public const Discord.LogSeverity LOG_LEVEL = Discord.LogSeverity.Warning;

		public const string BOT_VERSION = "0.9.20";
		public const string API_VERSION = "Discord.Net v1.0.0-rc-00691";
		public const string BOT_PREFIX = "+=";
		public const string IGNORE_ERROR = "Cx";
		public const string ZERO_LENGTH_CHAR = "\u180E";
		public const string TEXT_HOST = "hastebin";
		public const string ERROR_MESSAGE = "**ERROR:** ";
		public const string ARGUMENTS_ERROR = "Invalid number of arguments.";
		public const string USER_ERROR = "Invalid user.";
		public const string ROLE_ERROR = "Invalid role.";
		public const string CHANNEL_ERROR = "Invalid channel.";
		public const string ACTION_ERROR = "Invalid action.";
		public const string CHANNEL_PERMISSIONS_ERROR = "You do not have the ability to edit that channel.";
		public const string PATH_ERROR = "The bot does not have a valid path to save to/read from.";
		public const string MUTE_ROLE_NAME = "Muted";
		public const string SERVER_FOLDER = "Discord_Servers";
		public const string FILE_EXTENSION = ".json";
		public const string GUILD_INFO_LOCATION = "GuildInfo" + FILE_EXTENSION;
		public const string CHANNEL_INSTRUCTIONS = "[#Channel|\"Channel Name\"]";
		public const string OPTIONAL_CHANNEL_INSTRUCTIONS = "<#Channel|\"Channel Name\">";
		public const string VOICE_TYPE = "voice";
		public const string TEXT_TYPE = "text";
		public const string BYPASS_STRING = "Badoodle123";
		public const string DENY_WITHOUT_PREFERENCES = "This guild does not have preferences enabled and thus cannot use this command. Please run the `comconfigmodify` command to enable them.";
		public const string STREAM_URL = "https://www.twitch.tv/";
		public const string VIP_REGIONS = "VIP_REGIONS";
		public const string VANITY_URL = "VANITY_URL";
		public const string INVITE_SPLASH = "INVITE_SPLASH";
		public static readonly string DEFAULT_GAME = "type \"" + Properties.Settings.Default.Prefix + "help\" for help.";
		public const string HASTEBIN_ERROR = "The length of the content is over 200,000 characters and will be sent in a few seconds as a text file.";
		public const string NO_NN = "NO NICKNAME";

		public const double PERCENT_AVERAGE = .75;
		public const int TIME_TO_WAIT_BEFORE_MESSAGE_PRINT_TO_THE_SERVER_LOG = 3;
		public const int WAIT_TIME = 3000;
		public const int MEMBER_LIMIT = 0;
		public const int MAX_SA_GROUPS = 10;
		public const int MAX_REMINDS = 50;
		public const int MAX_BANNED_STRINGS = 50;
		public const int MAX_BANNED_REGEX = 25;
		public const int OWNER_POSITION = int.MaxValue;
		public const int MESSAGES_TO_GATHER = 100;
		public const int PAD_RIGHT = 20;
		public const int ACTIVE_CLOSE = 5000;

		public const int MIN_BITRATE = 8;
		public const int MAX_BITRATE = 96;
		public const int VIP_BITRATE = 128;
		public const int MAX_MESSAGE_LENGTH_LONG = 1900;
		public const int MAX_MESSAGE_LENGTH_SHORT = 750;
		public const int MAX_NICKNAME_LENGTH = 32;
		public const int MIN_NICKNAME_LENGTH = 2;
		public const int MAX_CHANNEL_NAME_LENGTH = 100;
		public const int MIN_CHANNEL_NAME_LENGTH = 2;
		public const int MAX_ROLE_NAME_LENGTH = 32;
		public const int MIN_ROLE_NAME_LENGTH = 1;
		public const int MAX_TOPIC_LENGTH = 1024;
		public const int MAX_GAME_LENGTH = 128; //Yes, I know it CAN go past that, but it won't show for others.
		public const int MAX_EMBED_LENGTH_LONG = 2048;
		public const int MAX_EMBED_LENGTH_SHORT = 1024;
		public const int MAX_TITLE_LENGTH = 256;
		public const int MAX_FIELDS = 25;
		public const int MAX_DESCRIPTION_LINES = 20;
		public const int MAX_FIELD_LINES = 5;
		public const int MAX_LENGTH_FOR_HASTEBIN = 200000;
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
			"comconfig", "help",
		});
		public static ReadOnlyCollection<string> VALID_GUILD_FILES = new ReadOnlyCollection<string>(new List<string>()
		{
			GUILD_INFO_LOCATION,
		});

		public static ReadOnlyCollection<LogActions> DEFAULT_LOG_ACTIONS = new ReadOnlyCollection<LogActions>(new List<LogActions>()
		{
			LogActions.UserJoined,
			LogActions.UserLeft,
			LogActions.UserUnbanned,
			LogActions.UserBanned,
			LogActions.GuildMemberUpdated,
			LogActions.MessageReceived,
			LogActions.MessageUpdated,
			LogActions.MessageDeleted,
			LogActions.ImageLog
		});

		public static readonly bool DISCONNECT = false;

		public static readonly Discord.Color BASE = new Discord.Color(255, 100, 000);
		public static readonly Discord.Color JOIN = new Discord.Color(000, 255, 000);
		public static readonly Discord.Color LEAV = new Discord.Color(255, 000, 000);
		public static readonly Discord.Color UNBN = new Discord.Color(000, 153, 000);
		public static readonly Discord.Color BANN = new Discord.Color(153, 000, 000);
		public static readonly Discord.Color UEDT = new Discord.Color(051, 051, 255);
		public static readonly Discord.Color ATCH = new Discord.Color(000, 204, 204);
		public static readonly Discord.Color MEDT = new Discord.Color(000, 000, 255);
		public static readonly Discord.Color MDEL = new Discord.Color(255, 051, 051);
		public static readonly Discord.Color RCRE = new Discord.Color(000, 175, 000);
		public static readonly Discord.Color REDT = new Discord.Color(000, 000, 204);
		public static readonly Discord.Color RDEL = new Discord.Color(175, 000, 000);
		public static readonly Discord.Color CCRE = new Discord.Color(000, 204, 000);
		public static readonly Discord.Color CEDT = new Discord.Color(000, 000, 153);
		public static readonly Discord.Color CDEL = new Discord.Color(204, 000, 000);

		public static readonly Regex FORMATREGEX = new Regex("\\\"[ ]+[+|\r|\n]{0,3}[ ]+\\\"", RegexOptions.Compiled);
	}

	public static class Variables
	{
		public static BotClient Client;
		public static UInt64 Bot_ID = 0;
		public static string Bot_Name;

		public static bool Windows = true;
		public static bool Loaded = false;
		public static bool Console = true;
		public static bool GotPath = false;
		public static bool GotKey = false;
		public static bool Pause = false;

		public static DateTime StartupTime = DateTime.UtcNow;
		public static System.Threading.Timer SpamTimer;
		public static System.Threading.Timer RemovePunishmentTimer;

		public static int TotalUsers = 0;
		public static int TotalGuilds = 0;
		public static int SucceededCommands = 0;
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

		//Lists that can only be modified through code for the most part
		public readonly static List<string> CommandNames = new List<string>();
		public readonly static List<string> RegionIDs = new List<string>();
		public readonly static List<HelpEntry> HelpList = new List<HelpEntry>();
		public readonly static List<BotGuildPermissionType> GuildPermissions = new List<BotGuildPermissionType>();
		public readonly static List<BotChannelPermissionType> ChannelPermissions = new List<BotChannelPermissionType>();

		//Lists that change as the bot is used
		public readonly static List<ulong> PotentialBotOwners = new List<ulong>();
		public readonly static List<ulong> DeletedRoles = new List<ulong>();
		public readonly static List<RemovablePunishment> PunishedUsers = new List<RemovablePunishment>();
		public readonly static List<RemovableMessage> TimedMessages = new List<RemovableMessage>();
		public readonly static List<ActiveCloseHelp> ActiveCloseHelp = new List<ActiveCloseHelp>();
		public readonly static List<ActiveCloseWords> ActiveCloseWords = new List<ActiveCloseWords>();
		public readonly static List<GuildToggleAfterTime> GuildToggles = new List<GuildToggleAfterTime>();
		public readonly static List<SlowmodeUser> SlowmodeUsers = new List<SlowmodeUser>();
		public readonly static List<Discord.IGuild> GuildsToBeLoaded = new List<Discord.IGuild>();
		public readonly static List<Discord.IGuild> GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts = new List<Discord.IGuild>();
	}

	public static class SharedCommands
	{
		public const string CPAUSE = "pause";
		public const string APAUSE = "p";

		public const string COWNER = "globalbotowner";
		public const string AOWNER = "glbo";

		public const string CPATH = "globalsavepath";
		public const string APATH = "glsp";

		public const string CPREFIX = "globalprefix";
		public const string APREFIX = "glp";

		public const string CSETTINGS = "globalsettings";
		public const string ASETTINGS = "gls";

		public const string CICON = "boticon";
		public const string AICON = "bi";

		public const string CGAME = "botgame";
		public const string AGAME = "bg";

		public const string CSTREAM = "botstream";
		public const string ASTREAM = "bst";

		public const string CNAME = "botname";
		public const string ANAME = "bn";

		public const string CDISC = "disconnect";
		public const string ADISC_1 = "dc";
		public const string ADISC_2 = "runescapeservers";

		public const string CRESTART = "restart";
		public const string ARESTART = "res";

		public const string CGUILDS = "listguilds";
		public const string AGUILDS = "lgds";

		public const string CSHARDS = "modifyshards";
		public const string ASHARDS = "msh";
	}
}