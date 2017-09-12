using Advobot.Actions;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Advobot.Classes
{
	/// <summary>
	/// Holds settings for a guild. Settings are only saved by calling <see cref="SaveSettings"/>.
	/// </summary>
	public class MyGuildSettings : IGuildSettings
	{
		[JsonProperty("WelcomeMessage")]
		private GuildNotification _WelcomeMessage = null;
		[JsonProperty("GoodbyeMessage")]
		private GuildNotification _GoodbyeMessage = null;
		[JsonProperty("ListedInvite")]
		private ListedInvite _ListedInvite = null;
		[JsonProperty("Slowmode")]
		private Slowmode _Slowmode = null;
		[JsonProperty("Prefix")]
		private string _Prefix = null;
		[JsonProperty("VerboseErrors")]
		private bool _VerboseErrors = true;
		[JsonProperty("ServerLog")]
		private ulong _ServerLogId = 0;
		[JsonProperty("ModLog")]
		private ulong _ModLogId = 0;
		[JsonProperty("ImageLog")]
		private ulong _ImageLogId = 0;
		[JsonProperty("MuteRole")]
		private ulong _MuteRoleId = 0;
		[JsonProperty("SpamPrevention")]
		private Dictionary<SpamType, SpamPreventionInfo> _SpamPrevention = null;
		[JsonProperty("RaidPrevention")]
		private Dictionary<RaidType, RaidPreventionInfo> _RaidPrevention = null;
		[JsonProperty("BotUsers")]
		private List<BotImplementedPermissions> _BotUsers = new List<BotImplementedPermissions>();
		[JsonProperty("SelfAssignableGroups")]
		private List<SelfAssignableGroup> _SelfAssignableGroups = new List<SelfAssignableGroup>();
		[JsonProperty("Quotes")]
		private List<Quote> _Quotes = new List<Quote>();
		[JsonProperty("LogActions")]
		private List<LogAction> _LogActions = new List<LogAction>();
		[JsonProperty("IgnoredCommandChannels")]
		private List<ulong> _IgnoredCommandChannels = new List<ulong>();
		[JsonProperty("IgnoredLogChannels")]
		private List<ulong> _IgnoredLogChannels = new List<ulong>();
		[JsonProperty("ImageOnlyChannels")]
		private List<ulong> _ImageOnlyChannels = new List<ulong>();
		[JsonProperty("BannedPhraseStrings")]
		private List<BannedPhrase> _BannedPhraseStrings = new List<BannedPhrase>();
		[JsonProperty("BannedPhraseRegex")]
		private List<BannedPhrase> _BannedPhraseRegex = new List<BannedPhrase>();
		[JsonProperty("BannedNamesForJoiningUsers")]
		private List<BannedPhrase> _BannedNamesForJoiningUsers = new List<BannedPhrase>();
		[JsonProperty("BannedPhrasePunishments")]
		private List<BannedPhrasePunishment> _BannedPhrasePunishments = new List<BannedPhrasePunishment>();
		[JsonProperty("CommandsDisabledOnUser")]
		private List<CommandOverride> _CommandsDisabledOnUser = new List<CommandOverride>();
		[JsonProperty("CommandsDisabledOnRole")]
		private List<CommandOverride> _CommandsDisabledOnRole = new List<CommandOverride>();
		[JsonProperty("CommandsDisabledOnChannel")]
		private List<CommandOverride> _CommandsDisabledOnChannel = new List<CommandOverride>();
		[JsonProperty("PersistentRoles")]
		private List<PersistentRole> _PersistentRoles = new List<PersistentRole>();
		[JsonProperty("CommandSwitches")]
		private List<CommandSwitch> _CommandSwitches = new List<CommandSwitch>();
		[JsonIgnore]
		private ITextChannel _ServerLog = null;
		[JsonIgnore]
		private ITextChannel _ModLog = null;
		[JsonIgnore]
		private ITextChannel _ImageLog = null;
		[JsonIgnore]
		private IRole _MuteRole = null;

		[JsonIgnore]
		public List<BotImplementedPermissions> BotUsers
		{
			get => _BotUsers ?? (_BotUsers = new List<BotImplementedPermissions>());
			set => _BotUsers = value;
		}
		[JsonIgnore]
		public List<SelfAssignableGroup> SelfAssignableGroups
		{
			get => _SelfAssignableGroups ?? (_SelfAssignableGroups = new List<SelfAssignableGroup>());
			set => _SelfAssignableGroups = value;
		}
		[JsonIgnore]
		public List<Quote> Quotes
		{
			get => _Quotes ?? (_Quotes = new List<Quote>());
			set => _Quotes = value;
		}
		[JsonIgnore]
		public List<LogAction> LogActions
		{
			get => _LogActions ?? (_LogActions = new List<LogAction>());
			set => _LogActions = value;
		}
		[JsonIgnore]
		public List<ulong> IgnoredCommandChannels
		{
			get => _IgnoredCommandChannels ?? (_IgnoredCommandChannels = new List<ulong>());
			set => _IgnoredCommandChannels = value;
		}
		[JsonIgnore]
		public List<ulong> IgnoredLogChannels
		{
			get => _IgnoredLogChannels ?? (_IgnoredLogChannels = new List<ulong>());
			set => _IgnoredLogChannels = value;
		}
		[JsonIgnore]
		public List<ulong> ImageOnlyChannels
		{
			get => _ImageOnlyChannels ?? (_ImageOnlyChannels = new List<ulong>());
			set => _ImageOnlyChannels = value;
		}
		[JsonIgnore]
		public List<BannedPhrase> BannedPhraseStrings
		{
			get => _BannedPhraseStrings ?? (_BannedPhraseStrings = new List<BannedPhrase>());
			set => _BannedPhraseStrings = value;
		}
		[JsonIgnore]
		public List<BannedPhrase> BannedPhraseRegex
		{
			get => _BannedPhraseRegex ?? (_BannedPhraseRegex = new List<BannedPhrase>());
			set => _BannedPhraseRegex = value;
		}
		[JsonIgnore]
		public List<BannedPhrase> BannedNamesForJoiningUsers
		{
			get => _BannedNamesForJoiningUsers ?? (_BannedNamesForJoiningUsers = new List<BannedPhrase>());
			set => _BannedNamesForJoiningUsers = value;
		}
		[JsonIgnore]
		public List<BannedPhrasePunishment> BannedPhrasePunishments
		{
			get => _BannedPhrasePunishments ?? (_BannedPhrasePunishments = new List<BannedPhrasePunishment>());
			set => _BannedPhrasePunishments = value;
		}
		[JsonIgnore]
		public List<CommandSwitch> CommandSwitches
		{
			get => _CommandSwitches ?? (_CommandSwitches = new List<CommandSwitch>());
			set => _CommandSwitches = value;
		}
		[JsonIgnore]
		public List<CommandOverride> CommandsDisabledOnUser
		{
			get => _CommandsDisabledOnUser ?? (_CommandsDisabledOnUser = new List<CommandOverride>());
			set => _CommandsDisabledOnUser = value;
		}
		[JsonIgnore]
		public List<CommandOverride> CommandsDisabledOnRole
		{
			get => _CommandsDisabledOnRole ?? (_CommandsDisabledOnRole = new List<CommandOverride>());
			set => _CommandsDisabledOnRole = value;
		}
		[JsonIgnore]
		public List<CommandOverride> CommandsDisabledOnChannel
		{
			get => _CommandsDisabledOnChannel ?? (_CommandsDisabledOnChannel = new List<CommandOverride>());
			set => _CommandsDisabledOnChannel = value;
		}
		[JsonIgnore]
		public List<PersistentRole> PersistentRoles
		{
			get => _PersistentRoles ?? (_PersistentRoles = new List<PersistentRole>());
			set => _PersistentRoles = value;
		}
		[JsonIgnore]
		public Dictionary<SpamType, SpamPreventionInfo> SpamPreventionDictionary
		{
			get => _SpamPrevention ?? (_SpamPrevention = new Dictionary<SpamType, SpamPreventionInfo>
			{
				{ SpamType.Message, null },
				{ SpamType.LongMessage, null },
				{ SpamType.Link, null },
				{ SpamType.Image, null },
				{ SpamType.Mention, null },
			});
			set => _SpamPrevention = value;
		}
		[JsonIgnore]
		public Dictionary<RaidType, RaidPreventionInfo> RaidPreventionDictionary
		{
			get => _RaidPrevention ?? (_RaidPrevention = new Dictionary<RaidType, RaidPreventionInfo>
			{
				{ RaidType.Regular, null },
				{ RaidType.RapidJoins, null },
			});
			set => _RaidPrevention = value;
		}
		[JsonIgnore]
		public GuildNotification WelcomeMessage
		{
			get => _WelcomeMessage;
			set => _WelcomeMessage = value;
		}
		[JsonIgnore]
		public GuildNotification GoodbyeMessage
		{
			get => _GoodbyeMessage;
			set => _GoodbyeMessage = value;
		}
		[JsonIgnore]
		public ListedInvite ListedInvite
		{
			get => _ListedInvite;
			set => _ListedInvite = value;
		}
		[JsonIgnore]
		public Slowmode Slowmode
		{
			get => _Slowmode;
			set => _Slowmode = value;
		}
		[JsonIgnore]
		public string Prefix
		{
			get => _Prefix;
			set => _Prefix = value;
		}
		[JsonIgnore]
		public bool VerboseErrors
		{
			get => _VerboseErrors;
			set => _VerboseErrors = value;
		}
		[JsonIgnore]
		public ITextChannel ServerLog
		{
			get => _ServerLog ?? (_ServerLog = Guild.GetTextChannel(_ServerLogId));
			set
			{
				_ServerLogId = value?.Id ?? 0;
				_ServerLog = value;
			}
		}
		[JsonIgnore]
		public ITextChannel ModLog
		{
			get => _ModLog ?? (_ModLog = Guild.GetTextChannel(_ModLogId));
			set
			{
				_ModLogId = value?.Id ?? 0;
				_ModLog = value;
			}
		}
		[JsonIgnore]
		public ITextChannel ImageLog
		{
			get => _ImageLog ?? (_ImageLog = Guild.GetTextChannel(_ImageLogId));
			set
			{
				_ImageLogId = value?.Id ?? 0;
				_ImageLog = value;
			}
		}
		[JsonIgnore]
		public IRole MuteRole
		{
			get => _MuteRole ?? (_MuteRole = Guild.GetRole(_MuteRoleId));
			set
			{
				_MuteRoleId = value?.Id ?? 0;
				_MuteRole = value;
			}
		}

		[JsonIgnore]
		public List<BannedPhraseUser> BannedPhraseUsers { get; } = new List<BannedPhraseUser>();
		[JsonIgnore]
		public List<SpamPreventionUser> SpamPreventionUsers { get; } = new List<SpamPreventionUser>();
		[JsonIgnore]
		public List<BotInvite> Invites { get; } = new List<BotInvite>();
		[JsonIgnore]
		public List<string> EvaluatedRegex { get; } = new List<string>();
		[JsonIgnore]
		public MessageDeletion MessageDeletion { get; } = new MessageDeletion();
		[JsonIgnore]
		public SocketGuild Guild { get; private set; } = null;
		[JsonIgnore]
		public bool Loaded { get; private set; } = false;

		public void SaveSettings()
		{
			if (Guild != null)
			{
				SavingAndLoadingActions.OverWriteFile(GetActions.GetServerDirectoryFile(Guild.Id, Constants.GUILD_SETTINGS_LOCATION), SavingAndLoadingActions.Serialize(this));
			}
		}
		public void PostDeserialize(IGuild guild)
		{
			Guild = guild as SocketGuild;

			if (_ListedInvite != null)
			{
				_ListedInvite.PostDeserialize(Guild);
			}
			if (_WelcomeMessage != null)
			{
				_WelcomeMessage.PostDeserialize(Guild);
			}
			if (_GoodbyeMessage != null)
			{
				_GoodbyeMessage.PostDeserialize(Guild);
			}

			foreach (var bannedPhrasePunishment in _BannedPhrasePunishments)
			{
				bannedPhrasePunishment.PostDeserialize(Guild);
			}
			foreach (var group in _SelfAssignableGroups)
			{
				group.Roles.ForEach(x => x.PostDeserialize(Guild));
				group.Roles.RemoveAll(x => x == null || x.Role == null);
			}

			Loaded = true;
		}
	}

	/// <summary>
	/// Holds settings for the bot. Settings are saved through property setters or calling <see cref="SaveSettings()"/>.
	/// </summary>
	public class MyBotSettings : IBotSettings, INotifyPropertyChanged
	{
		private const string MY_BOT_PREFIX = "&&";

		[JsonProperty("TrustedUsers")]
		private List<ulong> _TrustedUsers;
		[JsonProperty("UsersUnableToDMOwner")]
		private List<ulong> _UsersUnableToDMOwner;
		[JsonProperty("UsersIgnoredFromCommands")]
		private List<ulong> _UsersIgnoredFromCommands;
		[JsonProperty("ShardCount")]
		private uint _ShardCount;
		[JsonProperty("MessageCacheCount")]
		private uint _MessageCacheCount;
		[JsonProperty("MaxUserGatherCount")]
		private uint _MaxUserGatherCount;
		[JsonProperty("MaxMessageGatherSize")]
		private uint _MaxMessageGatherSize;
		[JsonProperty("Prefix")]
		private string _Prefix;
		[JsonProperty("Game")]
		private string _Game;
		[JsonProperty("Stream")]
		private string _Stream;
		[JsonProperty("AlwaysDownloadUsers")]
		private bool _AlwaysDownloadUsers = true;
		[JsonProperty("LogLevel")]
		private LogSeverity _LogLevel = LogSeverity.Warning;

		[JsonIgnore]
		public IReadOnlyList<ulong> TrustedUsers
		{
			get => _TrustedUsers.AsReadOnly() ?? (_TrustedUsers = new List<ulong>()).AsReadOnly();
			set
			{
				_TrustedUsers = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<ulong> UsersUnableToDMOwner
		{
			get => _UsersUnableToDMOwner.AsReadOnly() ?? (_UsersUnableToDMOwner = new List<ulong>()).AsReadOnly();
			set
			{
				_UsersUnableToDMOwner = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<ulong> UsersIgnoredFromCommands
		{
			get => _UsersIgnoredFromCommands.AsReadOnly() ?? (_UsersIgnoredFromCommands = new List<ulong>()).AsReadOnly();
			set
			{
				_UsersIgnoredFromCommands = value.ToList();
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public uint ShardCount
		{
			get => _ShardCount > 1 ? _ShardCount : (_ShardCount = 1);
			set
			{
				_ShardCount = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public uint MessageCacheCount
		{
			get => _MessageCacheCount > 0 ? _MessageCacheCount : (_MessageCacheCount = 1000);
			set
			{
				_MessageCacheCount = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public uint MaxUserGatherCount
		{
			get => _MaxUserGatherCount > 0 ? _MaxUserGatherCount : (_MaxUserGatherCount = 100);
			set
			{
				_MaxUserGatherCount = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public uint MaxMessageGatherSize
		{
			get => _MaxMessageGatherSize > 0 ? _MaxMessageGatherSize : (_MaxMessageGatherSize = 500000);
			set
			{
				_MaxMessageGatherSize = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public string Prefix
		{
			get => _Prefix ?? (_Prefix = MY_BOT_PREFIX);
			set
			{
				_Prefix = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public string Game
		{
			get => _Game ?? (_Game = $"type \"{Prefix}help\" for help.");
			set
			{
				_Game = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public string Stream
		{
			get => _Stream;
			set
			{
				_Stream = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public bool AlwaysDownloadUsers
		{
			get => _AlwaysDownloadUsers;
			set
			{
				_AlwaysDownloadUsers = value;
				OnPropertyChanged();
			}
		}
		[JsonIgnore]
		public LogSeverity LogLevel
		{
			get => _LogLevel;
			set
			{
				_LogLevel = value;
				OnPropertyChanged();
			}
		}

		[JsonIgnore]
		public bool IsWindows { get; private set; }
		[JsonIgnore]
		public bool IsConsole { get; private set; }
		[JsonIgnore]
		public bool Loaded { get; private set; }
		[JsonIgnore]
		public bool Pause { get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;
		public MyBotSettings()
		{
			PropertyChanged += SaveSettings;

			{
				var windir = Environment.GetEnvironmentVariable("windir");
				IsWindows = !String.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir);
			}
			{
				try
				{
					var window_height = System.Console.WindowHeight;
					IsConsole = true;
				}
				catch
				{
					IsConsole = false;
				}
			}
		}

		private void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		private void SaveSettings(object sender, PropertyChangedEventArgs e)
		{
			ConsoleActions.WriteLine($"Successfully saved: {e.PropertyName}");
			SaveSettings();
		}
		public void SaveSettings()
		{
			SavingAndLoadingActions.OverWriteFile(GetActions.GetBaseBotDirectoryFile(Constants.BOT_SETTINGS_LOCATION), SavingAndLoadingActions.Serialize(this));
		}

		public void TogglePause()
		{
			Pause = !Pause;
		}
		public void SetLoaded()
		{
			Loaded = true;
		}
	}
}