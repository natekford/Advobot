using System;
using System.Collections.Generic;

namespace Advobot
{
	public static class Constants
	{
		public const String BOT_VERSION = "0.8.0";
		public const String API_VERSION = "Discord.Net by RogueException v1.0.0-rc-00544";

		public const String BOT_PREFIX = "++";
		public const String IGNORE_ERROR = "Cx";
		public const String ZERO_LENGTH_CHAR = "\u180E";
		public const String TEXT_HOST = "hastebin";
		public const String STARTUP_GAME = "type \"" + Constants.BOT_PREFIX + "help\" for help.";
		public const String ERROR_MESSAGE = "**ERROR:** ";
		public const String ARGUMENTS_ERROR = "Invalid number of arguments.";
		public const String USER_ERROR = "Invalid user.";
		public const String ROLE_ERROR = "Invalid role.";
		public const String CHANNEL_ERROR = "Invalid channel.";
		public const String CHANNEL_PERMISSIONS_ERROR = "You do not have the ability to edit that channel.";
		public const String MUTE_ROLE_NAME = "Muted";
		public const String PREFERENCES_FILE = "commandPreferences.txt";
		public const String SERVERLOG_AND_MODLOG = "serverlogAndModlog.txt";
		public const String SERVER_LOG_CHECK_STRING = "serverlog";
		public const String MOD_LOG_CHECK_STRING = "modlog";
		public const String CHANNEL_INSTRUCTIONS = "[#Channel|[Channel/Text|Voice]]";
		public const String OPTIONAL_CHANNEL_INSTRUCTIONS = "<#Channel|[Channel/Text|Voice]>";
		public const String VOICE_TYPE = "voice";
		public const String TEXT_TYPE = "text";
		public const String BYPASS_STRING = "Badoodle123";

		public const UInt64 OWNER_ID = 172138437246320640;
		public const double PERCENT_AVERAGE = .75;
		public const int WAIT_TIME = 3000;
		public const int MEMBER_LIMIT = 0;
		public const int LENGTH_CHECK = 1900;
		public const int SHORT_LENGTH_CHECK = 750;
		public const int OWNER_POSITION = 9001;
		public const int MESSAGES_TO_GATHER = 100;
		public const int TIME_FOR_WAIT_BETWEEN_DELETING_MESSAGES_UNTIL_THEY_PRINT_TO_THE_SERVER_LOG = 3;

		public const int NICKNAME_LENGTH = 32;
		public const int TOPIC_LENGTH = 1024;
		public const int ROLE_NAME_LENGTH = 32;
		public const int CHANNEL_NAME_MAX_LENGTH = 100;
		public const int CHANNEL_NAME_MIN_LENGTH = 2;

		public static readonly String[] VALIDIMAGEEXTENSIONS = { ".jpeg", ".jpg", ".png" };
		public static readonly String[] VALIDGIFEXTENTIONS = { ".gif", ".gifv" };
		public static readonly String[] VALIDREGIONIDS = { "brazil", "eu-central", "eu-west", "singapore", "sydney", "us-east", "us-central", "us-south", "us-west" };

		public static readonly bool DISCONNECT = false;
		public static readonly bool NEWEST_DELETED_MESSAGES_AT_TOP = false;
		public static readonly bool TEXT_FILE = false;

		public static readonly Discord.Color BASE = new Discord.Color(255, 100, 0);
		public static readonly Discord.Color JOIN = new Discord.Color(0, 153, 0);
		public static readonly Discord.Color LEAVE = new Discord.Color(153, 0, 0);
		public static readonly Discord.Color UNBAN = new Discord.Color(0, 102, 0);
		public static readonly Discord.Color BAN = new Discord.Color(102, 0, 0);
		public static readonly Discord.Color UEDIT = new Discord.Color(255, 215, 0);
		public static readonly Discord.Color MEDIT = new Discord.Color(0, 0, 153);
		public static readonly Discord.Color MDEL = new Discord.Color(204, 0, 0);
		public static readonly Discord.Color ATTACH = new Discord.Color(0, 204, 204);
	}

	public static class Variables
	{
		public static UInt64 Bot_ID = 0;
		public static String Bot_Name = null;
		public static String Bot_Channel = null;

		public static DateTime StartupTime = DateTime.UtcNow.ToUniversalTime();
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
		public static int LoggedCommands = 0;

		public static Dictionary<ulong, List<PreferenceCategory>> CommandPreferences = new Dictionary<ulong, List<PreferenceCategory>>();
		public static Dictionary<ulong, List<Discord.WebSocket.SocketMessage>> DeletedMessages = new Dictionary<ulong, List<Discord.WebSocket.SocketMessage>>();
		public static Dictionary<ulong, System.Threading.CancellationTokenSource> CancelTokens = new Dictionary<ulong, System.Threading.CancellationTokenSource>();
		public static Dictionary<int, String> PermissionNames = new Dictionary<int, String>();
		public static Dictionary<int, String> ChannelPermissionNames = new Dictionary<int, String>();
		public static Dictionary<String, int> PermissionValues = new Dictionary<String, int>(StringComparer.OrdinalIgnoreCase);
		public static Dictionary<String, int> GeneralChannelPermissionValues = new Dictionary<String, int>(StringComparer.OrdinalIgnoreCase);
		public static Dictionary<String, int> TextChannelPermissionValues = new Dictionary<String, int>(StringComparer.OrdinalIgnoreCase);
		public static Dictionary<String, int> VoiceChannelPermissionValues = new Dictionary<String, int>(StringComparer.OrdinalIgnoreCase);
		public static Dictionary<ulong, Discord.IUser> UnbannedUsers = new Dictionary<ulong, Discord.IUser>();

		public static List<Discord.IGuild> Guilds = new List<Discord.IGuild>();
		public static List<Discord.IGuild> GuildsEnablingPreferences = new List<Discord.IGuild>();
		public static List<Discord.IGuild> GuildsDeletingPreferences = new List<Discord.IGuild>();
		public static List<Discord.IGuild> GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministrator = new List<Discord.IGuild>();
		public static List<HelpEntry> HelpList = new List<HelpEntry>();
		public static List<String> CommandNames = new List<String>();
		public static List<String> RegionIDs = new List<String>();
	}
}