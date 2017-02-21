using System;
using System.Collections.Generic;

namespace Advobot
{
	public static class Constants
	{
		//Client config
		public const bool ALWAYS_DOWNLOAD_USERS = true;
		public const int CACHED_MESSAGE_COUNT = 10000;
		public const Discord.LogSeverity LOG_LEVEL = Discord.LogSeverity.Warning;

		public const string BOT_VERSION = "0.9.12";
		public const string API_VERSION = "Discord.Net v1.0.0-rc-00595";
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
		public const string PREFERENCES_FILE = "CommandPreferences.txt";
		public const string MISCGUILDINFO = "MiscGuildInfo.txt";
		public const string BANNED_PHRASES = "BannedPhrases.txt";
		public const string SA_ROLES = "SelfAssignableRoles.txt";
		public const string PERMISSIONS = "BotPermissions.txt";
		public const string REMINDS = "Reminds.txt";
		public const string SERVER_LOG_CHECK_STRING = "serverlog";
		public const string MOD_LOG_CHECK_STRING = "modlog";
		public const string BANNED_PHRASES_CHECK_STRING = "BannedPhrases";
		public const string BANNED_REGEX_CHECK_STRING = "bannedregex";
		public const string BANNED_PHRASES_PUNISHMENTS = "punishments";
		public const string GUILD_PREFIX = "guildprefix";
		public const string LOG_ACTIONS = "logactions";
		public const string IGNORED_LOG_CHANNELS = "ignoredlogchannels";
		public const string SPAM_PREVENTION = "spamprevention";
		public const string IGNORED_COMMAND_CHANNELS = "ignoredcommandchannels";
		public const string CHANNEL_INSTRUCTIONS = "[#Channel|[Channel/Text|Voice]]";
		public const string OPTIONAL_CHANNEL_INSTRUCTIONS = "<#Channel|[Channel/Text|Voice]>";
		public const string VOICE_TYPE = "voice";
		public const string TEXT_TYPE = "text";
		public const string BYPASS_STRING = "Badoodle123";
		public const string DENY_WITHOUT_PREFERENCES = "This guild does not have preferences enabled and thus cannot use this command. Please run the `comconfigmodify` command to enable them.";
		public const string STREAM_URL = "https://www.twitch.tv/";
		public const string VIP_REGIONS = "VIP_REGIONS";
		public const string VANITY_URL = "VANITY_URL";
		public const string INVITE_SPLASH = "INVITE_SPLASH";
		public static readonly string DEFAULT_GAME = "type \"" + Properties.Settings.Default.Prefix + "help\" for help.";

		public const double PERCENT_AVERAGE = .75;
		public const int WAIT_TIME = 3000;
		public const int MEMBER_LIMIT = 0;
		public const int MAX_SA_GROUPS = 10;
		public const int MAX_REMINDS = 50;
		public const int LENGTH_CHECK = 1900;
		public const int SHORT_LENGTH_CHECK = 750;
		public const int OWNER_POSITION = 9001;
		public const int MESSAGES_TO_GATHER = 100;
		public const int TIME_FOR_WAIT_BETWEEN_DELETING_MESSAGES_UNTIL_THEY_PRINT_TO_THE_SERVER_LOG = 3;
		public const int NICKNAME_MIN_LENGTH = 2;
		public const int NICKNAME_MAX_LENGTH = 32;
		public const int TOPIC_LENGTH = 1024;
		public const int ROLE_NAME_LENGTH = 32;
		public const int CHANNEL_NAME_MAX_LENGTH = 100;
		public const int CHANNEL_NAME_MIN_LENGTH = 2;
		public const int GAME_MAX_LENGTH = 128; //Yes, I know it CAN go past that, but it won't show for others.
		public const int MIN_BITRATE = 8;
		public const int MAX_BITRATE = 96;
		public const int VIP_BITRATE = 128;

		public static readonly string[] VALIDIMAGEEXTENSIONS = { ".jpeg", ".jpg", ".png" };
		public static readonly string[] VALIDGIFEXTENTIONS = { ".gif", ".gifv" };
		public static readonly string[] VALIDREGIONIDS = { "brazil", "eu-central", "eu-west", "hongkong", "singapore", "sydney", "us-east", "us-central", "us-south", "us-west" };
		public static readonly string[] VIPREGIONIDS = { "vip-amsterdam", "vip-us-east", "vip-us-west" };
		public static readonly string[] COMMANDSUNABLETOBETURNEDOFF = { "comconfigtoggle", "comconfigcurrent", "comconfigmodify", "help" };
		public static readonly string[] CLOSEWORDSPOSITIONS = { "1", "2", "3", "4", "5" };

		public static readonly LogActions[] DEFAULTLOGACTIONS =
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
		};

		public static readonly bool DISCONNECT = false;
		public static readonly bool NEWEST_DELETED_MESSAGES_AT_TOP = false;
		public static readonly bool TEXT_FILE = false;

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

		public static readonly System.Text.RegularExpressions.Regex FORMATREGEX = new System.Text.RegularExpressions.Regex("\\\"[ ]+[+|\r|\n]{0,3}[ ]+\\\"");
	}

	public static class Variables
	{
		public static BotClient Client;

		public static UInt64 Bot_ID = 0;
		public static string Bot_Name;
		public static string Bot_Channel;

		public static bool Windows = true;
		public static bool Loaded = false;
		public static bool Console = true;
		public static bool GotPath = false;
		public static bool GotKey = false;
		public static bool Pause = false;

		public static DateTime StartupTime = DateTime.UtcNow;
		public static System.Threading.Timer Timer;

		public static int TotalUsers = 0;
		public static int TotalGuilds = 0;
		public static int FailedCommands = 0;
		public static int AttemptedCommands = 0;
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

		public static Dictionary<ulong, List<Discord.WebSocket.SocketMessage>> DeletedMessages = new Dictionary<ulong, List<Discord.WebSocket.SocketMessage>>();
		public static Dictionary<ulong, System.Threading.CancellationTokenSource> CancelTokens = new Dictionary<ulong, System.Threading.CancellationTokenSource>();
		public static Dictionary<ulong, Discord.IUser> UnbannedUsers = new Dictionary<ulong, Discord.IUser>();
		public static Dictionary<ulong, BotGuildInfo> Guilds = new Dictionary<ulong, BotGuildInfo>();
		public static Dictionary<ulong, List<SlowmodeUser>> SlowmodeGuilds = new Dictionary<ulong, List<SlowmodeUser>>();
		public static Dictionary<Discord.IGuildChannel, List<SlowmodeUser>> SlowmodeChannels = new Dictionary<Discord.IGuildChannel, List<SlowmodeUser>>();

		public static List<string> CommandNames = new List<string>();
		public static List<string> RegionIDs = new List<string>();
		public static List<ulong> PotentialBotOwners = new List<ulong>();
		public static List<ulong> DeletedRoles = new List<ulong>();
		public static List<HelpEntry> HelpList = new List<HelpEntry>();
		public static List<ActiveCloseHelp> ActiveCloseHelp = new List<ActiveCloseHelp>();
		public static List<ActiveCloseWords> ActiveCloseWords = new List<ActiveCloseWords>();
		public static List<BannedPhraseUser> BannedPhraseUserList = new List<BannedPhraseUser>();
		public static List<SelfAssignableGroup> SelfAssignableGroups = new List<SelfAssignableGroup>();
		public static List<BotGuildPermissionType> GuildPermissions = new List<BotGuildPermissionType>();
		public static List<BotChannelPermissionType> ChannelPermissions = new List<BotChannelPermissionType>();
		public static List<BotImplementedPermissions> BotUsers = new List<BotImplementedPermissions>();
		public static List<Discord.IGuild> GuildsToBeLoaded = new List<Discord.IGuild>();
		public static List<Discord.IGuild> GuildsEnablingPreferences = new List<Discord.IGuild>();
		public static List<Discord.IGuild> GuildsDeletingPreferences = new List<Discord.IGuild>();
		public static List<Discord.IGuild> GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts = new List<Discord.IGuild>();
	}

	public static class SharedCommands
	{
		public const string CPAUSE = "pause";

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
	}
}