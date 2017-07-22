using Advobot.Actions;
using Advobot.Attributes;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.NonSavedClasses;
using Advobot.SavedClasses;
using Advobot.Structs;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot
{
	namespace SavedClasses
	{
		public class MyGuildSettings : IGuildSettings, INotifyPropertyChanged
		{
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
			[JsonProperty("SanitaryChannels")]
			private List<ulong> _SanitaryChannels = new List<ulong>();
			[JsonProperty("BannedPhraseStrings")]
			private List<BannedPhrase> _BannedPhraseStrings = new List<BannedPhrase>();
			[JsonProperty("BannedPhraseRegex")]
			private List<BannedPhrase> _BannedPhraseRegex = new List<BannedPhrase>();
			[JsonProperty("BannedNamesForJoiningUsers")]
			private List<BannedPhrase> _BannedNamesForJoiningUsers = new List<BannedPhrase>();
			[JsonProperty("BannedPhrasePunishments")]
			private List<BannedPhrasePunishment> _BannedPhrasePunishments = new List<BannedPhrasePunishment>();
			[JsonProperty("CommandSwitches")]
			private List<CommandSwitch> _CommandSwitches = new List<CommandSwitch>();
			[JsonProperty("CommandsDisabledOnUser")]
			private List<CommandOverride> _CommandsDisabledOnUser = new List<CommandOverride>();
			[JsonProperty("CommandsDisabledOnRole")]
			private List<CommandOverride> _CommandsDisabledOnRole = new List<CommandOverride>();
			[JsonProperty("CommandsDisabledOnChannel")]
			private List<CommandOverride> _CommandsDisabledOnChannel = new List<CommandOverride>();
			[JsonProperty("ServerLog")]
			private DiscordObjectWithId<ITextChannel> _ServerLog = new DiscordObjectWithId<ITextChannel>(null);
			[JsonProperty("ModLog")]
			private DiscordObjectWithId<ITextChannel> _ModLog = new DiscordObjectWithId<ITextChannel>(null);
			[JsonProperty("ImageLog")]
			private DiscordObjectWithId<ITextChannel> _ImageLog = new DiscordObjectWithId<ITextChannel>(null);
			[JsonProperty("MuteRole")]
			private DiscordObjectWithId<IRole> _MuteRole = new DiscordObjectWithId<IRole>(null);
			[JsonProperty("MessageSpamPrevention")]
			private SpamPrevention _MessageSpamPrevention = null;
			[JsonProperty("LongMessageSpamPrevention")]
			private SpamPrevention _LongMessageSpamPrevention = null;
			[JsonProperty("LinkSpamPrevention")]
			private SpamPrevention _LinkSpamPrevention = null;
			[JsonProperty("ImageSpamPrevention")]
			private SpamPrevention _ImageSpamPrevention = null;
			[JsonProperty("MentionSpamPrevention")]
			private SpamPrevention _MentionSpamPrevention = null;
			[JsonProperty("RaidPrevention")]
			private RaidPrevention _RaidPrevention = null;
			[JsonProperty("RapidJoinPrevention")]
			private RaidPrevention _RapidJoinPrevention = null;
			[JsonProperty("WelcomeMessage")]
			private GuildNotification _WelcomeMessage = null;
			[JsonProperty("GoodbyeMessage")]
			private GuildNotification _GoodbyeMessage = null;
			[JsonProperty("ListedInvite")]
			private ListedInvite _ListedInvite = null;
			[JsonProperty("Prefix")]
			private string _Prefix = null;
			[JsonProperty("VerboseErrors")]
			private bool _VerboseErrors = true;

			[JsonIgnore]
			public IReadOnlyList<BotImplementedPermissions> BotUsers
			{
				get => _BotUsers.AsReadOnly() ?? (_BotUsers = new List<BotImplementedPermissions>()).AsReadOnly();
				set
				{
					_BotUsers = value.ToList();
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyList<SelfAssignableGroup> SelfAssignableGroups
			{
				get => _SelfAssignableGroups.AsReadOnly() ?? (_SelfAssignableGroups = new List<SelfAssignableGroup>()).AsReadOnly();
				set
				{
					_SelfAssignableGroups = value.ToList();
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyList<Quote> Quotes
			{
				get => _Quotes.AsReadOnly() ?? (_Quotes = new List<Quote>()).AsReadOnly();
				set
				{
					_Quotes = value.ToList();
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyList<LogAction> LogActions
			{
				get => _LogActions.AsReadOnly() ?? (_LogActions = new List<LogAction>()).AsReadOnly();
				set
				{
					_LogActions = value.ToList();
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyList<ulong> IgnoredCommandChannels
			{
				get => _IgnoredCommandChannels.AsReadOnly() ?? (_IgnoredCommandChannels = new List<ulong>()).AsReadOnly();
				set
				{
					_IgnoredCommandChannels = value.ToList();
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyList<ulong> IgnoredLogChannels
			{
				get => _IgnoredLogChannels.AsReadOnly() ?? (_IgnoredLogChannels = new List<ulong>()).AsReadOnly();
				set
				{
					_IgnoredLogChannels = value.ToList();
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyList<ulong> ImageOnlyChannels
			{
				get => _ImageOnlyChannels.AsReadOnly() ?? (_ImageOnlyChannels = new List<ulong>()).AsReadOnly();
				set
				{
					_ImageOnlyChannels = value.ToList();
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyList<ulong> SanitaryChannels
			{
				get => _SanitaryChannels.AsReadOnly() ?? (_SanitaryChannels = new List<ulong>()).AsReadOnly();
				set
				{
					_SanitaryChannels = value.ToList();
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyList<BannedPhrase> BannedPhraseStrings
			{
				get => _BannedPhraseStrings.AsReadOnly() ?? (_BannedPhraseStrings = new List<BannedPhrase>()).AsReadOnly();
				set
				{
					_BannedPhraseStrings = value.ToList();
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyList<BannedPhrase> BannedPhraseRegex
			{
				get => _BannedPhraseRegex.AsReadOnly() ?? (_BannedPhraseRegex = new List<BannedPhrase>()).AsReadOnly();
				set
				{
					_BannedPhraseRegex = value.ToList();
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyList<BannedPhrase> BannedNamesForJoiningUsers
			{
				get => _BannedNamesForJoiningUsers.AsReadOnly() ?? (_BannedNamesForJoiningUsers = new List<BannedPhrase>()).AsReadOnly();
				set
				{
					_BannedNamesForJoiningUsers = value.ToList();
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyList<BannedPhrasePunishment> BannedPhrasePunishments
			{
				get => _BannedPhrasePunishments.AsReadOnly() ?? (_BannedPhrasePunishments = new List<BannedPhrasePunishment>()).AsReadOnly();
				set
				{
					_BannedPhrasePunishments = value.ToList();
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyList<CommandSwitch> CommandSwitches
			{
				get => _CommandSwitches.AsReadOnly() ?? (_CommandSwitches = new List<CommandSwitch>()).AsReadOnly();
				set
				{
					_CommandSwitches = value.ToList();
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyList<CommandOverride> CommandsDisabledOnUser
			{
				get => _CommandsDisabledOnUser.AsReadOnly() ?? (_CommandsDisabledOnUser = new List<CommandOverride>()).AsReadOnly();
				set
				{
					_CommandsDisabledOnUser = value.ToList();
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyList<CommandOverride> CommandsDisabledOnRole
			{
				get => _CommandsDisabledOnRole.AsReadOnly() ?? (_CommandsDisabledOnRole = new List<CommandOverride>()).AsReadOnly();
				set
				{
					_CommandsDisabledOnRole = value.ToList();
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyList<CommandOverride> CommandsDisabledOnChannel
			{
				get => _CommandsDisabledOnChannel.AsReadOnly() ?? (_CommandsDisabledOnChannel = new List<CommandOverride>()).AsReadOnly();
				set
				{
					_CommandsDisabledOnChannel = value.ToList();
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public ITextChannel ServerLog
			{
				get => (_ServerLog ?? (_ServerLog = new DiscordObjectWithId<ITextChannel>(null))).Object;
				set
				{
					_ServerLog = new DiscordObjectWithId<ITextChannel>(value);
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public ITextChannel ModLog
			{
				get => (_ModLog ?? (_ModLog = new DiscordObjectWithId<ITextChannel>(null))).Object;
				set
				{
					_ModLog = new DiscordObjectWithId<ITextChannel>(value);
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public ITextChannel ImageLog
			{
				get => (_ImageLog ?? (_ImageLog = new DiscordObjectWithId<ITextChannel>(null))).Object;
				set
				{
					_ImageLog = new DiscordObjectWithId<ITextChannel>(value);
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IRole MuteRole
			{
				get => (_MuteRole ?? (_MuteRole = new DiscordObjectWithId<IRole>(null))).Object;
				set
				{
					_MuteRole = new DiscordObjectWithId<IRole>(value);
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyDictionary<SpamType, SpamPrevention> SpamPreventionDictionary
			{
				get => new ReadOnlyDictionary<SpamType, SpamPrevention>(new Dictionary<SpamType, SpamPrevention>
			{
				{ SpamType.Message, _MessageSpamPrevention },
				{ SpamType.LongMessage, _LongMessageSpamPrevention },
				{ SpamType.Link, _LinkSpamPrevention },
				{ SpamType.Image, _ImageSpamPrevention },
				{ SpamType.Mention, _MentionSpamPrevention },
			});
				set
				{
					foreach (var kvp in value)
					{
						switch (kvp.Key)
						{
							case SpamType.Message:
							{
								_MessageSpamPrevention = kvp.Value;
								break;
							}
							case SpamType.LongMessage:
							{
								_LongMessageSpamPrevention = kvp.Value;
								break;
							}
							case SpamType.Link:
							{
								_LinkSpamPrevention = kvp.Value;
								break;
							}
							case SpamType.Image:
							{
								_ImageSpamPrevention = kvp.Value;
								break;
							}
							case SpamType.Mention:
							{
								_MentionSpamPrevention = kvp.Value;
								break;
							}
						}
					}
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public IReadOnlyDictionary<RaidType, RaidPrevention> RaidPreventionDictionary
			{
				get => new ReadOnlyDictionary<RaidType, RaidPrevention>(new Dictionary<RaidType, RaidPrevention>
			{
				{ RaidType.Regular, _RaidPrevention },
				{ RaidType.RapidJoins, _RapidJoinPrevention },
			});
				set
				{
					foreach (var kvp in value)
					{
						switch (kvp.Key)
						{
							case RaidType.Regular:
							{
								_RaidPrevention = kvp.Value;
								break;
							}
							case RaidType.RapidJoins:
							{
								_RapidJoinPrevention = kvp.Value;
								break;
							}
						}
					}
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public GuildNotification WelcomeMessage
			{
				get => _WelcomeMessage;
				set
				{
					_WelcomeMessage = value;
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public GuildNotification GoodbyeMessage
			{
				get => _GoodbyeMessage;
				set
				{
					_GoodbyeMessage = value;
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public ListedInvite ListedInvite
			{
				get => _ListedInvite;
				set
				{
					_ListedInvite = value;
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public string Prefix
			{
				get => _Prefix;
				set
				{
					_Prefix = value;
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public bool VerboseErrors
			{
				get => _VerboseErrors;
				set
				{
					_VerboseErrors = value;
					OnPropertyChanged();
				}
			}

			[JsonIgnore]
			public List<BannedPhraseUser> BannedPhraseUsers { get; } = new List<BannedPhraseUser>();
			[JsonIgnore]
			public List<SpamPreventionUser> SpamPreventionUsers { get; } = new List<SpamPreventionUser>();
			[JsonIgnore]
			public List<SlowmodeChannel> SlowmodeChannels { get; } = new List<SlowmodeChannel>();
			[JsonIgnore]
			public List<BotInvite> Invites { get; } = new List<BotInvite>();
			[JsonIgnore]
			public List<string> EvaluatedRegex { get; } = new List<string>();
			[JsonIgnore]
			public SlowmodeGuild SlowmodeGuild { get; } = null;
			[JsonIgnore]
			public MessageDeletion MessageDeletion { get; } = new MessageDeletion();
			[JsonIgnore]
			public IGuild Guild { get; private set; } = null;
			[JsonIgnore]
			public bool Loaded { get; private set; } = false;

			public event PropertyChangedEventHandler PropertyChanged;
			public MyGuildSettings()
			{
				PropertyChanged += SaveSettings;
			}

			private void OnPropertyChanged([CallerMemberName] string propertyName = "")
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
			private void SaveSettings(object sender, PropertyChangedEventArgs e)
			{
				//ConsoleActions.WriteLine(String.Format("Successfully saved: {0}", e.PropertyName));
				if (Guild != null)
				{
					SavingAndLoadingActions.OverWriteFile(GetActions.GetServerFilePath(Guild.Id, Constants.GUILD_SETTINGS_LOCATION), SavingAndLoadingActions.Serialize(this));
				}
			}
			public void PostDeserialize(IGuild guild)
			{
				Guild = guild;
				var tempGuild = guild as SocketGuild;

				if (_ModLog != null)
				{
					_ModLog.PostDeserialize(tempGuild);
				}
				if (_ServerLog != null)
				{
					_ServerLog.PostDeserialize(tempGuild);
				}
				if (_ImageLog != null)
				{
					_ImageLog.PostDeserialize(tempGuild);
				}
				if (_MuteRole != null)
				{
					_MuteRole.PostDeserialize(tempGuild);
				}

				if (_ListedInvite != null)
				{
					_ListedInvite.PostDeserialize(tempGuild);
				}
				if (_WelcomeMessage != null)
				{
					_WelcomeMessage.PostDeserialize(tempGuild);
				}
				if (_GoodbyeMessage != null)
				{
					_GoodbyeMessage.PostDeserialize(tempGuild);
				}

				foreach (var bannedPhrasePunishment in _BannedPhrasePunishments)
				{
					bannedPhrasePunishment.PostDeserialize(tempGuild);
				}
				foreach (var group in _SelfAssignableGroups)
				{
					group.Roles.ForEach(x => x.PostDeserialize(tempGuild));
					group.Roles.RemoveAll(x => x == null || x.Role == null);
				}

				Loaded = true;
			}
		}

		public class MyBotSettings : IBotSettings, INotifyPropertyChanged
		{
			[JsonProperty("TrustedUsers")]
			private List<ulong> _TrustedUsers = new List<ulong>();
			[JsonProperty("UsersUnableToDMOwner")]
			private List<ulong> _UsersUnableToDMOwner = new List<ulong>();
			[JsonProperty("UsersIgnoredFromCommands")]
			private List<ulong> _UsersIgnoredFromCommands = new List<ulong>();
			[JsonProperty("BotOwnerID")]
			private ulong _BotOwnerId = 0;
			[JsonProperty("ShardCount")]
			private uint _ShardCount = 1;
			[JsonProperty("MessageCacheCount")]
			private uint _MessageCacheCount = 1000;
			[JsonProperty("MaxUserGatherCount")]
			private uint _MaxUserGatherCount = 100;
			[JsonProperty("MaxMessageGatherSize")]
			private uint _MaxMessageGatherSize = 500000;
			[JsonProperty("Prefix")]
			private string _Prefix = Constants.BOT_PREFIX;
			[JsonProperty("Game")]
			private string _Game = String.Format("type \"{0}help\" for help.", Constants.BOT_PREFIX);
			[JsonProperty("Stream")]
			private string _Stream = null;
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
			public ulong BotOwnerId
			{
				get => _BotOwnerId;
				set
				{
					_BotOwnerId = value;
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
				get => _MessageCacheCount;
				set
				{
					_MessageCacheCount = value;
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public uint MaxUserGatherCount
			{
				get => _MaxUserGatherCount;
				set
				{
					_MaxUserGatherCount = value;
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public uint MaxMessageGatherSize
			{
				get => _MaxMessageGatherSize;
				set
				{
					_MaxMessageGatherSize = value;
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public string Prefix
			{
				get => _Prefix ?? (_Prefix = Constants.BOT_PREFIX);
				set
				{
					_Prefix = value;
					OnPropertyChanged();
				}
			}
			[JsonIgnore]
			public string Game
			{
				get => _Game;
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
			public bool Windows { get; private set; }
			[JsonIgnore]
			public bool Console { get; private set; }
			[JsonIgnore]
			public bool FirstInstanceOfBotStartingUpWithCurrentKey { get; private set; }
			[JsonIgnore]
			public bool GotPath { get; private set; }
			[JsonIgnore]
			public bool GotKey { get; private set; }
			[JsonIgnore]
			public bool Loaded { get; private set; }
			[JsonIgnore]
			public bool Pause { get; private set; }
			[JsonIgnore]
			public DateTime StartupTime { get; } = DateTime.UtcNow;

			public event PropertyChangedEventHandler PropertyChanged;
			public MyBotSettings()
			{
				PropertyChanged += SaveSettings;
			}

			//TODO: put a wait like on message deletion
			private void OnPropertyChanged([CallerMemberName] string propertyName = "")
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
			private void SaveSettings(object sender, PropertyChangedEventArgs e)
			{
				ConsoleActions.WriteLine(String.Format("Successfully saved: {0}", e.PropertyName));
				SavingAndLoadingActions.OverWriteFile(GetActions.GetBaseBotDirectory(Constants.BOT_SETTINGS_LOCATION), SavingAndLoadingActions.Serialize(this));
			}
			public void PostDeserialize(bool windows, bool console, bool firstInstance)
			{
				Windows = windows;
				Console = console;
				FirstInstanceOfBotStartingUpWithCurrentKey = firstInstance;
			}

			public void TogglePause()
			{
				Pause = !Pause;
			}
			public void SetLoaded()
			{
				Loaded = true;
			}
			public void SetGotKey()
			{
				GotKey = true;
			}
			public void SetGotPath()
			{
				GotPath = true;
			}
		}

		public class CommandOverride : ISetting
		{
			[JsonProperty]
			public string Name { get; }
			[JsonProperty]
			public ulong Id { get; }
			[JsonProperty]
			public bool Enabled { get; private set; }

			public CommandOverride(string name, ulong id, bool enabled)
			{
				Name = name;
				Id = id;
				Enabled = enabled;
			}

			public void ToggleEnabled()
			{
				Enabled = !Enabled;
			}

			public string SettingToString()
			{
				return String.Format("**Command:** `{0}`\n**ID:** `{1}`\n**Enabled:** `{2}`", Name, Id, Enabled);
			}
			public string SettingToString(SocketGuild guild)
			{
				return SettingToString();
			}
		}

		public class CommandSwitch : ISetting
		{
			[JsonProperty]
			public string Name { get; }
			[JsonIgnore]
			public string[] Aliases { get; }
			[JsonProperty]
			public bool Value { get; private set; }
			[JsonIgnore]
			public string ValAsString { get { return Value ? "ON" : "OFF"; } }
			[JsonIgnore]
			public int ValAsInteger { get { return Value ? 1 : -1; } }
			[JsonIgnore]
			public bool ValAsBoolean { get { return Value; } }
			[JsonProperty]
			public CommandCategory Category { get; }
			[JsonIgnore]
			public string CategoryName { get { return Category.EnumName(); } }
			[JsonIgnore]
			public int CategoryValue { get { return (int)Category; } }
			[JsonIgnore]
			private HelpEntry _HelpEntry;

			public CommandSwitch(string name, bool value)
			{
				_HelpEntry = Constants.HELP_ENTRIES.FirstOrDefault(x => x.Name.Equals(name));
				if (_HelpEntry == null)
					return;

				Name = name;
				Value = value;
				Category = _HelpEntry.Category;
				Aliases = _HelpEntry.Aliases;
			}

			public void ToggleEnabled()
			{
				Value = !Value;
			}

			public string SettingToString()
			{
				return String.Format("`{0}` `{1}`", ValAsString.PadRight(3), Name);
			}
			public string SettingToString(SocketGuild guild)
			{
				return SettingToString();
			}
		}

		public class BannedPhrase : ISetting
		{
			[JsonProperty]
			public string Phrase { get; }
			[JsonProperty]
			public PunishmentType Punishment { get; private set; }

			public BannedPhrase(string phrase, PunishmentType punishment)
			{
				Phrase = phrase;
				ChangePunishment(punishment);
			}

			public void ChangePunishment(PunishmentType punishment)
			{
				switch (punishment)
				{
					case PunishmentType.RoleMute:
					case PunishmentType.Kick:
					case PunishmentType.KickThenBan:
					case PunishmentType.Ban:
					{
						Punishment = punishment;
						return;
					}
					default:
					{
						Punishment = PunishmentType.Nothing;
						return;
					}
				}
			}

			public string SettingToString()
			{
				return String.Format("`{0}` `{1}`", Punishment.EnumName().Substring(0, 1), Phrase);
			}
			public string SettingToString(SocketGuild guild)
			{
				return SettingToString();
			}
		}

		public class BannedPhrasePunishment : ISetting
		{
			[JsonProperty]
			public int NumberOfRemoves { get; }
			[JsonProperty]
			public PunishmentType Punishment { get; }
			[JsonProperty]
			public ulong RoleId { get; }
			[JsonProperty]
			public ulong GuildId { get; }
			[JsonProperty]
			public uint PunishmentTime { get; }
			[JsonIgnore]
			public IRole Role { get; private set; }

			[JsonConstructor]
			public BannedPhrasePunishment(int number, PunishmentType punishment, ulong guildId = 0, ulong roleId = 0, uint punishmentTime = 0)
			{
				NumberOfRemoves = number;
				Punishment = punishment;
				RoleId = roleId;
				GuildId = guildId;
				PunishmentTime = punishmentTime;
			}
			public BannedPhrasePunishment(int number, PunishmentType punishment, ulong guildId = 0, ulong roleId = 0, uint punishmentTime = 0, IRole role = null) : this(number, punishment, guildId, roleId, punishmentTime)
			{
				Role = role;
			}

			public void PostDeserialize(SocketGuild guild)
			{
				Role = guild.GetRole(RoleId);
			}

			public string SettingToString()
			{
				return String.Format("`{0}.` `{1}`{2}",
					NumberOfRemoves.ToString("00"),
					RoleId == 0 ? Punishment.EnumName() : RoleId.ToString(),
					PunishmentTime == 0 ? "" : " `" + PunishmentTime + " minutes`");
			}
			public string SettingToString(SocketGuild guild)
			{
				return String.Format("`{0}.` `{1}`{2}",
					NumberOfRemoves.ToString("00"),
					RoleId == 0 ? Punishment.EnumName() : guild.GetRole(RoleId).Name,
					PunishmentTime == 0 ? "" : " `" + PunishmentTime + " minutes`");
			}
		}

		public class SelfAssignableGroup : ISetting
		{
			[JsonProperty]
			public List<SelfAssignableRole> Roles { get; private set; }
			[JsonProperty]
			public int Group { get; private set; }

			public SelfAssignableGroup(int group)
			{
				Roles = new List<SelfAssignableRole>();
				Group = group;
			}

			public void AddRole(SelfAssignableRole role)
			{
				Roles.Add(role);
			}
			public void AddRoles(IEnumerable<SelfAssignableRole> roles)
			{
				Roles.AddRange(roles);
			}
			public void RemoveRoles(IEnumerable<ulong> roleIDs)
			{
				Roles.RemoveAll(x => roleIDs.Contains(x.RoleId));
			}

			public string SettingToString()
			{
				return String.Format("`Group: {0}`\n{1}", Group, String.Join("\n", Roles.Select(x => x.SettingToString())));
			}
			public string SettingToString(SocketGuild guild)
			{
				return SettingToString();
			}
		}

		public class SelfAssignableRole : ISetting
		{
			[JsonProperty]
			public ulong RoleId { get; }
			[JsonIgnore]
			public IRole Role { get; private set; }

			[JsonConstructor]
			public SelfAssignableRole(ulong roleID)
			{
				RoleId = roleID;
			}
			public SelfAssignableRole(IRole role)
			{
				RoleId = role.Id;
				Role = role;
			}

			public void PostDeserialize(SocketGuild guild)
			{
				Role = guild.GetRole(RoleId);
			}

			public string SettingToString()
			{
				return String.Format("**Role:** `{0}`", Role.FormatRole());
			}
			public string SettingToString(SocketGuild guild)
			{
				return SettingToString();
			}
		}

		public class BotImplementedPermissions : ISetting
		{
			[JsonProperty]
			public ulong UserId { get; }
			[JsonProperty]
			public ulong Permissions { get; private set; }

			public BotImplementedPermissions(ulong userID, ulong permissions)
			{
				UserId = userID;
				Permissions = permissions;
			}

			public void AddPermission(ulong bit)
			{
				Permissions |= bit;
			}
			public void RemovePermission(ulong bit)
			{
				Permissions &= ~bit;
			}

			public string SettingToString()
			{
				return String.Format("**User:** `{0}`\n**Permissions:** `{1}`", UserId, Permissions);
			}
			public string SettingToString(SocketGuild guild)
			{
				return String.Format("**User:** `{0}`\n**Permissions:** `{1}`", guild.GetUser(UserId).FormatUser(), Permissions);
			}
		}

		public class GuildNotification : ISetting
		{
			[JsonProperty]
			public string Content { get; }
			[JsonProperty]
			public string Title { get; }
			[JsonProperty]
			public string Description { get; }
			[JsonProperty]
			public string ThumbURL { get; }
			[JsonProperty]
			public ulong ChannelId { get; }
			[JsonIgnore]
			public EmbedBuilder Embed { get; }
			[JsonIgnore]
			public ITextChannel Channel { get; private set; }

			[JsonConstructor]
			public GuildNotification(string content, string title, string description, string thumbURL, ulong channelID)
			{
				Content = content;
				Title = title;
				Description = description;
				ThumbURL = thumbURL;
				ChannelId = channelID;
				if (!(String.IsNullOrWhiteSpace(title) && String.IsNullOrWhiteSpace(description) && String.IsNullOrWhiteSpace(thumbURL)))
				{
					Embed = EmbedActions.MakeNewEmbed(title, description, null, null, null, thumbURL);
				}
			}
			public GuildNotification(string content, string title, string description, string thumbURL, ITextChannel channel) : this(content, title, description, thumbURL, channel.Id)
			{
				Channel = channel;
			}

			public void ChangeChannel(ITextChannel channel)
			{
				Channel = channel;
			}
			public void PostDeserialize(SocketGuild guild)
			{
				Channel = guild.GetTextChannel(ChannelId);
			}

			public string SettingToString()
			{
				return String.Format("**Channel:** `{0}`\n**Content:** `{1}`\n**Title:** `{2}`\n**Description:** `{3}`\n**Thumbnail:** `{4}`",
					Channel.FormatChannel(),
					Content,
					Title,
					Description,
					ThumbURL);
			}
			public string SettingToString(SocketGuild guild)
			{
				return SettingToString();
			}
		}

		public class ListedInvite : ISetting
		{
			[JsonProperty]
			public string Code { get; private set; }
			[JsonProperty]
			public string[] Keywords { get; private set; }
			[JsonProperty]
			public bool HasGlobalEmotes { get; private set; }
			[JsonIgnore]
			public DateTime LastBumped { get; private set; }
			[JsonIgnore]
			public string Url { get; private set; }
			[JsonIgnore]
			public SocketGuild Guild { get; private set; }

			[JsonConstructor]
			public ListedInvite(string code, string[] keywords)
			{
				LastBumped = DateTime.UtcNow;
				Code = code;
				Url = String.Format("https://www.discord.gg/{0}", Code);
				Keywords = keywords ?? new string[0];
			}
			public ListedInvite(SocketGuild guild, string code, string[] keywords) : this(code, keywords)
			{
				Guild = guild;
				HasGlobalEmotes = Guild.HasGlobalEmotes();
			}

			public void UpdateCode(string code)
			{
				Code = code;
				Url = String.Format("https://www.discord.gg/{0}", Code);
			}
			public void UpdateKeywords(string[] keywords)
			{
				Keywords = keywords;
			}
			public void UpdateLastBumped()
			{
				LastBumped = DateTime.UtcNow;
			}
			public void PostDeserialize(SocketGuild guild)
			{
				Guild = guild;
				HasGlobalEmotes = Guild.HasGlobalEmotes();
			}

			public string SettingToString()
			{
				if (String.IsNullOrWhiteSpace(Code))
				{
					return null;
				}

				var codeStr = String.Format("**Code:** `{0}`\n", Code);
				var keywordStr = "";
				if (Keywords.Any())
				{
					keywordStr = String.Format("**Keywords:**\n`{0}`", String.Join("`, `", Keywords));
				}
				return codeStr + keywordStr;
			}
			public string SettingToString(SocketGuild guild)
			{
				return SettingToString();
			}
		}

		public class Quote : ISetting, INameAndText
		{
			[JsonProperty]
			public string Name { get; }
			[JsonProperty]
			public string Text { get; }

			public Quote(string name, string text)
			{
				Name = name;
				Text = text;
			}

			public string SettingToString()
			{
				return String.Format("`{0}`", Name);
			}
			public string SettingToString(SocketGuild guild)
			{
				return SettingToString();
			}
		}

		public class DiscordObjectWithId<T> : ISetting where T : ISnowflakeEntity
		{
			[JsonIgnore]
			private readonly ReadOnlyDictionary<Type, Func<SocketGuild, ulong, object>> inits = new ReadOnlyDictionary<Type, Func<SocketGuild, ulong, object>>(new Dictionary<Type, Func<SocketGuild, ulong, object>>
			{
				{ typeof(IRole), (guild, id) => guild.GetRole(id) },
				{ typeof(ITextChannel), (guild, id) => guild.GetTextChannel(id) },
			});
			[JsonProperty]
			public ulong Id { get; }
			[JsonIgnore]
			public T Object { get; private set; }

			[JsonConstructor]
			public DiscordObjectWithId(ulong id)
			{
				Id = id;
				Object = default(T);
			}
			public DiscordObjectWithId(T obj)
			{
				Id = obj?.Id ?? 0;
				Object = obj;
			}

			public void PostDeserialize(SocketGuild guild)
			{
				if (inits.TryGetValue(typeof(T), out var method))
				{
					Object = (T)method(guild, Id);
				}
			}

			public string SettingToString()
			{
				return Object != null ? FormattingActions.FormatObject(Object) : null;
			}
			public string SettingToString(SocketGuild guild)
			{
				return SettingToString();
			}
		}

		public class SpamPrevention : ISetting
		{
			[JsonProperty]
			public PunishmentType PunishmentType { get; }
			[JsonProperty]
			public int TimeInterval { get; }
			[JsonProperty]
			public int RequiredSpamInstances { get; }
			[JsonProperty]
			public int RequiredSpamPerMessage { get; }
			[JsonProperty]
			public int VotesForKick { get; }
			[JsonProperty]
			public bool Enabled { get; private set; }

			public SpamPrevention(PunishmentType punishmentType, int timeInterval, int requiredSpamInstances, int requiredSpamPerMessage, int votesForKick)
			{
				PunishmentType = punishmentType;
				TimeInterval = timeInterval;
				RequiredSpamInstances = requiredSpamInstances;
				RequiredSpamPerMessage = requiredSpamPerMessage;
				VotesForKick = votesForKick;
				Enabled = true;
			}

			public void ToggleEnabled()
			{
				Enabled = !Enabled;
			}

			public string SettingToString()
			{
				return String.Format("**Enabled:** `{0}`\n**Spam Instances:** `{1}`\n**Spam Amount/Time Interval:** `{2}`\n**Votes Needed For Kick:** `{3}`\n**Punishment:** `{4}`",
					Enabled,
					RequiredSpamInstances,
					RequiredSpamPerMessage,
					VotesForKick,
					PunishmentType.EnumName());
			}
			public string SettingToString(SocketGuild guild)
			{
				return SettingToString();
			}
		}

		public class RaidPrevention : ISetting
		{
			[JsonProperty]
			public PunishmentType PunishmentType { get; }
			[JsonProperty]
			public int TimeInterval { get; }
			[JsonProperty]
			public int RequiredCount { get; }
			[JsonProperty]
			public bool Enabled { get; private set; }
			[JsonIgnore]
			public List<BasicTimeInterface> TimeList { get; }

			public RaidPrevention(PunishmentType punishmentType, int timeInterval, int requiredCount)
			{
				PunishmentType = punishmentType;
				TimeInterval = timeInterval;
				RequiredCount = requiredCount;
				TimeList = new List<BasicTimeInterface>();
				Enabled = true;
			}

			public int GetSpamCount()
			{
				return TimeList.GetCountOfItemsInTimeFrame(TimeInterval);
			}
			public void Add(DateTime time)
			{
				TimeList.ThreadSafeAdd(new BasicTimeInterface(time));
			}
			public void Remove(DateTime time)
			{
				TimeList.ThreadSafeRemoveAll(x => x.GetTime().Equals(time));
			}
			public void ToggleEnabled()
			{
				Enabled = !Enabled;
			}
			public void Reset()
			{
				TimeList.Clear();
			}
			public async Task RaidPreventionPunishment(IGuildSettings guildSettings, IGuildUser user, ITimersModule timers = null)
			{
				//TODO: make this not 0
				await PunishmentActions.AutomaticPunishments(guildSettings, user, PunishmentType, false, 0, timers);
			}

			public string SettingToString()
			{
				return String.Format("**Enabled:** `{0}`\n**Users:** `{1}`\n**Time Interval:** `{2}`\n**Punishment:** `{3}`",
					Enabled,
					RequiredCount,
					TimeInterval,
					PunishmentType.EnumName());
			}
			public string SettingToString(SocketGuild guild)
			{
				return SettingToString();
			}
		}
	}

	namespace NonSavedClasses
	{
		[CommandRequirements]
		public class MyModuleBase : ModuleBase<MyCommandContext>
		{
		}

		public class MyCommandContext : CommandContext, IMyCommandContext
		{
			public IBotSettings BotSettings { get; }
			public IGuildSettings GuildSettings { get; }
			public ILogModule Logging { get; }
			public ITimersModule Timers { get; }

			public MyCommandContext(IBotSettings botSettings, IGuildSettings guildSettings, ILogModule logging, ITimersModule timers, IDiscordClient client, IUserMessage msg) : base(client, msg)
			{
				BotSettings = botSettings;
				GuildSettings = guildSettings;
				Logging = logging;
				Timers = timers;
			}
		}

		public class MyGuildSettingsModule : IGuildSettingsModule
		{
			private readonly Dictionary<ulong, IGuildSettings> _guildSettings = new Dictionary<ulong, IGuildSettings>();
			private readonly Type _guildSettingsType;

			public MyGuildSettingsModule(Type guildSettingsType)
			{
				if (guildSettingsType == null || !guildSettingsType.GetInterfaces().Contains(typeof(IGuildSettings)))
				{
					throw new ArgumentException("Invalid type for guild settings provided.");
				}

				_guildSettingsType = guildSettingsType;
			}

			public async Task AddGuild(IGuild guild)
			{
				if (!_guildSettings.ContainsKey(guild.Id))
				{
					_guildSettings.Add(guild.Id, await CreateGuildSettings(_guildSettingsType, guild));
				}
			}
			public Task RemoveGuild(IGuild guild)
			{
				if (_guildSettings.ContainsKey(guild.Id))
				{
					_guildSettings.Remove(guild.Id);
				}
				return Task.FromResult(0);
			}
			public IGuildSettings GetSettings(IGuild guild)
			{
				return _guildSettings[guild.Id];
			}
			public IEnumerable<IGuildSettings> GetAllSettings()
			{
				return _guildSettings.Values;
			}
			public bool TryGetSettings(IGuild guild, out IGuildSettings settings)
			{
				return _guildSettings.TryGetValue(guild.Id, out settings);
			}

			private async Task<IGuildSettings> CreateGuildSettings(Type guildSettingsType, IGuild guild)
			{
				if (_guildSettings.TryGetValue(guild.Id, out IGuildSettings guildSettings))
				{
					return guildSettings;
				}

				var path = GetActions.GetServerFilePath(guild.Id, Constants.GUILD_SETTINGS_LOCATION);
				if (File.Exists(path))
				{
					try
					{
						using (var reader = new StreamReader(path))
						{
							guildSettings = (IGuildSettings)JsonConvert.DeserializeObject(reader.ReadToEnd(), guildSettingsType);
						}
						ConsoleActions.WriteLine(String.Format("The guild information for {0} has successfully been loaded.", guild.FormatGuild()));
					}
					catch (Exception e)
					{
						ConsoleActions.ExceptionToConsole(e);
					}
				}
				else
				{
					ConsoleActions.WriteLine(String.Format("The guild information file for {0} could not be found; using default.", guild.FormatGuild()));
				}
				guildSettings = guildSettings ?? (IGuildSettings)Activator.CreateInstance(guildSettingsType);

				guildSettings.CommandsDisabledOnUser = guildSettings.CommandsDisabledOnUser.Where(x => !String.IsNullOrWhiteSpace(x.Name)).ToList().AsReadOnly();
				guildSettings.CommandsDisabledOnRole = guildSettings.CommandsDisabledOnRole.Where(x => !String.IsNullOrWhiteSpace(x.Name)).ToList().AsReadOnly();
				guildSettings.CommandsDisabledOnChannel = guildSettings.CommandsDisabledOnChannel.Where(x => !String.IsNullOrWhiteSpace(x.Name)).ToList().AsReadOnly();

				var tempCommandSwitches = guildSettings.CommandSwitches.Where(x => !String.IsNullOrWhiteSpace(x.Name)).ToList();
				foreach (var cmd in Constants.HELP_ENTRIES.Where(x => !guildSettings.CommandSwitches.Select(y => y.Name).CaseInsContains(x.Name)))
				{
					tempCommandSwitches.Add(new CommandSwitch(cmd.Name, cmd.DefaultEnabled));
				}
				guildSettings.CommandSwitches = tempCommandSwitches;

				guildSettings.Invites.AddRange((await InviteActions.GetInvites(guild)).Select(x => new BotInvite(x.GuildId, x.Code, x.Uses)));

				var myGuildSettings = guildSettings as MyGuildSettings;
				if (myGuildSettings == null)
				{
					return guildSettings;
				}

				myGuildSettings.PostDeserialize(guild);
				return myGuildSettings;
			}
		}

		public class MyInviteListModule : IInviteListModule
		{
			private List<ListedInvite> _ListedInvites;
			public List<ListedInvite> ListedInvites => _ListedInvites ?? (_ListedInvites = new List<ListedInvite>());

			public void AddInvite(ListedInvite invite)
			{
				ListedInvites.ThreadSafeAdd(invite);
			}
			public void RemoveInvite(ListedInvite invite)
			{
				ListedInvites.ThreadSafeRemove(invite);
			}
			public void RemoveInvite(IGuild guild)
			{
				ListedInvites.ThreadSafeRemoveAll(x => x.Guild.Id == guild.Id);
			}
			public void BumpInvite(ListedInvite invite)
			{
				RemoveInvite(invite);
				AddInvite(invite);
				invite.UpdateLastBumped();
			}
		}

		public class HelpEntry : INameAndText
		{
			public string Name { get; }
			public string[] Aliases { get; }
			public string Usage { get; }
			public string BasePerm { get; }
			public string Text { get; }
			public CommandCategory Category { get; }
			public bool DefaultEnabled { get; }
			private const string PLACE_HOLDER_STR = "N/A";

			public HelpEntry(string name, string[] aliases, string usage, string basePerm, string text, CommandCategory category, bool defaultEnabled)
			{
				Name = String.IsNullOrWhiteSpace(name) ? PLACE_HOLDER_STR : name;
				Aliases = aliases ?? new[] { PLACE_HOLDER_STR };
				Usage = String.IsNullOrWhiteSpace(usage) ? PLACE_HOLDER_STR : Constants.BOT_PREFIX + usage;
				BasePerm = String.IsNullOrWhiteSpace(basePerm) ? PLACE_HOLDER_STR : basePerm;
				Text = String.IsNullOrWhiteSpace(text) ? PLACE_HOLDER_STR : text;
				Category = category;
				DefaultEnabled = defaultEnabled;
			}

			public override string ToString()
			{
				var aliasStr = String.Format("**Aliases:** {0}", String.Join(", ", Aliases));
				var usageStr = String.Format("**Usage:** {0}", Usage);
				var permStr = String.Format("\n**Base Permission(s):**\n{0}", BasePerm);
				var descStr = String.Format("\n**Description:**\n{0}", Text);
				return String.Join("\n", new[] { aliasStr, usageStr, permStr, descStr });
			}
		}

		public class BotInvite
		{
			public ulong GuildId { get; }
			public string Code { get; }
			public int Uses { get; private set; }

			public BotInvite(ulong guildId, string code, int uses)
			{
				GuildId = guildId;
				Code = code;
				Uses = uses;
			}

			public void IncreaseUses()
			{
				++Uses;
			}
		}

		public class SlowmodeUser : ITimeInterface
		{
			public IGuildUser User { get; }
			public int BaseMessages { get; }
			public int Interval { get; }
			public int CurrentMessagesLeft { get; private set; }
			public DateTime Time { get; private set; }

			public SlowmodeUser(IGuildUser user, int baseMessages, int interval)
			{
				User = user;
				BaseMessages = baseMessages;
				Interval = interval;
				CurrentMessagesLeft = baseMessages;
			}

			public void LowerMessagesLeft()
			{
				--CurrentMessagesLeft;
			}
			public void ResetMessagesLeft()
			{
				CurrentMessagesLeft = BaseMessages;
			}
			public void SetNewTime()
			{
				Time = DateTime.UtcNow.AddSeconds(Interval);
			}
			public DateTime GetTime()
			{
				return Time;
			}
		}

		public class BannedPhraseUser
		{
			public IGuildUser User { get; }
			public int MessagesForRole { get; private set; }
			public int MessagesForKick { get; private set; }
			public int MessagesForBan { get; private set; }

			public BannedPhraseUser(IGuildUser user)
			{
				User = user;
			}

			public void IncreaseRoleCount()
			{
				++MessagesForRole;
			}
			public void ResetRoleCount()
			{
				MessagesForRole = 0;
			}
			public void IncreaseKickCount()
			{
				++MessagesForKick;
			}
			public void ResetKickCount()
			{
				MessagesForKick = 0;
			}
			public void IncreaseBanCount()
			{
				++MessagesForBan;
			}
			public void ResetBanCount()
			{
				MessagesForBan = 0;
			}
		}

		public class MessageDeletion
		{
			public CancellationTokenSource CancelToken { get; private set; }
			private List<IMessage> _Messages = new List<IMessage>();

			public void SetCancelToken(CancellationTokenSource cancelToken)
			{
				CancelToken = cancelToken;
			}
			public List<IMessage> GetList()
			{
				return _Messages.ToList();
			}
			public void SetList(List<IMessage> InList)
			{
				_Messages = InList.ToList();
			}
			public void AddToList(IMessage Item)
			{
				_Messages.Add(Item);
			}
			public void ClearList()
			{
				_Messages.Clear();
			}
		}

		public class SlowmodeGuild
		{
			public int BaseMessages { get; }
			public int Interval { get; }
			public List<SlowmodeUser> Users { get; }

			public SlowmodeGuild(int baseMessages, int interval)
			{
				BaseMessages = baseMessages;
				Interval = interval;
				Users = new List<SlowmodeUser>();
			}
			public SlowmodeGuild(int baseMessages, int interval, List<SlowmodeUser> users)
			{
				BaseMessages = baseMessages;
				Interval = interval;
				Users = users;
			}
		}

		public class SlowmodeChannel
		{
			public ulong ChannelId { get; }
			public int BaseMessages { get; }
			public int Interval { get; }
			public List<SlowmodeUser> Users { get; }

			public SlowmodeChannel(ulong channelId, int baseMessages, int interval)
			{
				ChannelId = channelId;
				BaseMessages = baseMessages;
				Interval = interval;
				Users = new List<SlowmodeUser>();
			}
			public SlowmodeChannel(ulong channelId, int baseMessages, int interval, List<SlowmodeUser> users)
			{
				ChannelId = channelId;
				BaseMessages = baseMessages;
				Interval = interval;
				Users = users;
			}
		}

		public class SpamPreventionUser
		{
			public IGuildUser User { get; }
			public List<ulong> UsersWhoHaveAlreadyVoted { get; } = new List<ulong>();
			public Dictionary<SpamType, List<BasicTimeInterface>> SpamLists { get; } = new Dictionary<SpamType, List<BasicTimeInterface>>();

			public int VotesRequired { get; private set; } = int.MaxValue;
			public bool PotentialPunishment { get; private set; } = false;
			public bool AlreadyKicked { get; private set; } = false;
			public PunishmentType Punishment { get; private set; } = PunishmentType.Nothing;

			public SpamPreventionUser(IGuildUser user)
			{
				User = user;
				foreach (var spamType in Enum.GetValues(typeof(SpamType)).Cast<SpamType>())
				{
					SpamLists.Add(spamType, new List<BasicTimeInterface>());
				}
			}

			public void IncreaseVotesToKick(ulong Id)
			{
				UsersWhoHaveAlreadyVoted.ThreadSafeAdd(Id);
			}
			public void ChangeVotesRequired(int newVotesRequired)
			{
				VotesRequired = Math.Min(newVotesRequired, VotesRequired);
			}
			public void ChangePunishmentType(PunishmentType newPunishment)
			{
				if (Constants.PUNISHMENT_SEVERITY[newPunishment] > Constants.PUNISHMENT_SEVERITY[Punishment])
				{
					Punishment = newPunishment;
				}
			}
			public void EnablePunishable()
			{
				PotentialPunishment = true;
			}
			public void ResetSpamUser()
			{
				//Don't reset already kicked since KickThenBan requires it
				UsersWhoHaveAlreadyVoted.Clear();
				foreach (var spamList in SpamLists.Values)
				{
					spamList.Clear();
				}

				VotesRequired = int.MaxValue;
				PotentialPunishment = false;
				Punishment = PunishmentType.Nothing;
			}
			public bool CheckIfAllowedToPunish(SpamPrevention spamPrev, SpamType spamType)
			{
				return SpamLists[spamType].GetCountOfItemsInTimeFrame(spamPrev.TimeInterval) >= spamPrev.RequiredSpamInstances;
			}
			public async Task SpamPreventionPunishment(IGuildSettings guildSettings, ITimersModule timers = null)
			{
				//TODO: make this not 0
				await PunishmentActions.AutomaticPunishments(guildSettings, User, Punishment, AlreadyKicked, 0, timers);
			}
		}
	}
}