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
		public class MyGuildSettings : IGuildSettings
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
			[JsonProperty("PersistentRoles")]
			private List<PersistentRole> _PersistentRoles = new List<PersistentRole>();
			[JsonProperty("SpamPrevention")]
			private Dictionary<SpamType, SpamPreventionInfo> _SpamPrevention = null;
			[JsonProperty("RaidPrevention")]
			private Dictionary<RaidType, RaidPreventionInfo> _RaidPrevention = null;
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

			private void OnPropertyChanged([CallerMemberName] string propertyName = "")
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
			private void SaveSettings(object sender, PropertyChangedEventArgs e)
			{
				ConsoleActions.WriteLine($"Successfully saved: {e.PropertyName}");
				SavingAndLoadingActions.OverWriteFile(GetActions.GetBaseBotDirectoryFile(Constants.BOT_SETTINGS_LOCATION), SavingAndLoadingActions.Serialize(this));
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

			public CommandOverride(string name, ulong id)
			{
				Name = name;
				Id = id;
			}

			public override string ToString()
			{
				return $"**Command:** `{Name}`\n**ID:** `{Id}`";
			}
			public string ToString(SocketGuild guild)
			{
				return ToString();
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
			public string ValAsString { get => Value ? "ON" : "OFF"; }
			[JsonIgnore]
			public int ValAsInteger { get => Value ? 1 : -1; }
			[JsonIgnore]
			public bool ValAsBoolean { get => Value; }
			[JsonProperty]
			public CommandCategory Category { get; }
			[JsonIgnore]
			public string CategoryName { get => Category.EnumName(); }
			[JsonIgnore]
			public int CategoryValue { get => (int)Category; }
			[JsonIgnore]
			private HelpEntry _HelpEntry;

			public CommandSwitch(string name, bool value)
			{
				_HelpEntry = Constants.HELP_ENTRIES.FirstOrDefault(x => x.Name.Equals(name));
				if (_HelpEntry == null)
				{
					//TODO: uncomment this when all commands have been put back in
					//throw new ArgumentException("Command name does not have a help entry.");
					return;
				}

				Name = name;
				Value = value;
				Category = _HelpEntry.Category;
				Aliases = _HelpEntry.Aliases;
			}

			public void ToggleEnabled()
			{
				Value = !Value;
			}

			public override string ToString()
			{
				return $"`{ValAsString.PadRight(3)}` `{Name}`";
			}
			public string ToString(SocketGuild guild)
			{
				return ToString();
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
						Punishment = default(PunishmentType);
						return;
					}
				}
			}

			public override string ToString()
			{
				var punishmentChar = Punishment == default(PunishmentType) ? "N" : Punishment.EnumName().Substring(0, 1);
				return $"`{punishmentChar}` `{Phrase}`";
			}
			public string ToString(SocketGuild guild)
			{
				return ToString();
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

			public override string ToString()
			{
				var punishment = RoleId == 0 ? Punishment.EnumName() : RoleId.ToString();
				var time = PunishmentTime == 0 ? "" : " `" + PunishmentTime + " minutes`";
				return $"`{NumberOfRemoves.ToString("00")}.` `{punishment}`{time}";
			}
			public string ToString(SocketGuild guild)
			{
				var punishment = RoleId == 0 ? Punishment.EnumName() : guild.GetRole(RoleId).Name;
				var time = PunishmentTime == 0 ? "" : " `" + PunishmentTime + " minutes`";
				return $"`{NumberOfRemoves.ToString("00")}.` `{punishment}`{time}";
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

			public void AddRoles(IEnumerable<SelfAssignableRole> roles)
			{
				Roles.AddRange(roles);
			}
			public void RemoveRoles(IEnumerable<ulong> roleIDs)
			{
				Roles.RemoveAll(x => roleIDs.Contains(x.RoleId));
			}

			public override string ToString()
			{
				return $"`Group: {Group}`\n{String.Join("\n", Roles.Select(x => x.ToString()))}";
			}
			public string ToString(SocketGuild guild)
			{
				return ToString();
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

			public override string ToString()
			{
				return $"**Role:** `{Role.FormatRole()}`";
			}
			public string ToString(SocketGuild guild)
			{
				return ToString();
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

			public void AddPermissions(ulong flags)
			{
				Permissions |= flags;
			}
			public void RemovePermissions(ulong flags)
			{
				Permissions &= ~flags;
			}

			public override string ToString()
			{
				return $"**User:** `{UserId}`\n**Permissions:** `{Permissions}`";
			}
			public string ToString(SocketGuild guild)
			{
				return $"**User:** `{guild.GetUser(UserId).FormatUser()}`\n**Permissions:** `{Permissions}`";
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

			public override string ToString()
			{
				return $"**Channel:** `{Channel.FormatChannel()}`\n**Content:** `{Content}`\n**Title:** `{Title}`\n**Description:** `{Description}`\n**Thumbnail:** `{ThumbURL}`";
			}
			public string ToString(SocketGuild guild)
			{
				return ToString();
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
				Url = "https://www.discord.gg/" + Code;
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
				Url = "https://www.discord.gg/" + Code;
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

			public override string ToString()
			{
				if (String.IsNullOrWhiteSpace(Code))
				{
					return null;
				}

				var codeStr = $"**Code:** `{Code}`\n";
				var keywordStr = "";
				if (Keywords.Any())
				{
					keywordStr = $"**Keywords:**\n`{String.Join("`, `", Keywords)}`";
				}
				return codeStr + keywordStr;
			}
			public string ToString(SocketGuild guild)
			{
				return ToString();
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

			public override string ToString()
			{
				return $"`{Name}`";
			}
			public string ToString(SocketGuild guild)
			{
				return ToString();
			}
		}

		public class SpamPreventionInfo : ISetting
		{
			[JsonProperty]
			public PunishmentType PunishmentType { get; }
			[JsonProperty]
			public int RequiredSpamInstances { get; }
			[JsonProperty]
			public int RequiredSpamPerMessageOrTimeInterval { get; }
			[JsonProperty]
			public int VotesForKick { get; }
			[JsonIgnore]
			public bool Enabled { get; private set; }

			public SpamPreventionInfo(PunishmentType punishmentType, int requiredSpamInstances, int requiredSpamPerMessageOrTimeInterval, int votesForKick)
			{
				PunishmentType = punishmentType;
				RequiredSpamInstances = requiredSpamInstances;
				RequiredSpamPerMessageOrTimeInterval = requiredSpamPerMessageOrTimeInterval;
				VotesForKick = votesForKick;
				Enabled = false;
			}

			public void Enable()
			{
				Enabled = true;
			}
			public void Disable()
			{
				Enabled = false;
			}

			public override string ToString()
			{
				return  $"**Punishment:** `{PunishmentType.EnumName()}`\n" +
						$"**Spam Instances:** `{RequiredSpamInstances}`\n" +
						$"**Votes For Punishment:** `{VotesForKick}`\n" +
						$"**Spam Amt/Time Interval:** `{RequiredSpamPerMessageOrTimeInterval}`";
			}
			public string ToString(SocketGuild guild)
			{
				return ToString();
			}
		}

		public class RaidPreventionInfo : ISetting
		{
			[JsonProperty]
			public PunishmentType PunishmentType { get; }
			[JsonProperty]
			public int Interval { get; }
			[JsonProperty]
			public int UserCount { get; }
			[JsonProperty]
			public bool Enabled { get; private set; }
			[JsonIgnore]
			public List<BasicTimeInterface> TimeList { get; }

			public RaidPreventionInfo(PunishmentType punishmentType, int userCount, int interval)
			{
				PunishmentType = punishmentType;
				UserCount = userCount;
				Interval = interval;
				TimeList = new List<BasicTimeInterface>();
				Enabled = true;
			}

			public int GetSpamCount()
			{
				return TimeList.GetCountOfItemsInTimeFrame(Interval);
			}
			public void Add(DateTime time)
			{
				TimeList.ThreadSafeAdd(new BasicTimeInterface(time));
			}
			public void Remove(DateTime time)
			{
				TimeList.ThreadSafeRemoveAll(x => x.GetTime().Equals(time));
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

			public void Enable()
			{
				Enabled = true;
			}
			public void Disable()
			{
				Enabled = false;
			}

			public override string ToString()
			{
				return  $"**Enabled:** `{Enabled}`\n" +
						$"**Users:** `{UserCount}`\n" +
						$"**Time Interval:** `{Interval}`\n" +
						$"**Punishment:** `{PunishmentType.EnumName()}`";
			}
			public string ToString(SocketGuild guild)
			{
				return ToString();
			}
		}

		public class Slowmode : ISetting
		{
			[JsonProperty]
			public int BaseMessages { get; }
			[JsonProperty]
			public int Interval { get; }
			[JsonProperty]
			public ulong[] ImmuneRoleIds { get; }
			[JsonIgnore]
			public List<SlowmodeUser> Users { get; }
			[JsonIgnore]
			public bool Enabled { get; private set; }

			public Slowmode(int baseMessages, int interval, IRole[] immuneRoles)
			{
				BaseMessages = baseMessages;
				Interval = interval;
				Users = new List<SlowmodeUser>();
				ImmuneRoleIds = immuneRoles.Select(x => x.Id).Distinct().ToArray();
				Enabled = false;
			}

			public void Disable()
			{
				Enabled = false;
			}
			public void Enable()
			{
				Enabled = true;
			}

			public override string ToString()
			{
				return  $"**Base messages:** `{BaseMessages}`\n" +
						$"**Time interval:** `{Interval}`\n" +
						$"**Immune Role Ids:** `{String.Join("`, `", ImmuneRoleIds)}`";
			}
			public string ToString(SocketGuild guild)
			{
				return ToString();
			}
		}

		public class PersistentRole : ISetting
		{
			[JsonProperty]
			public ulong UserId { get; }
			[JsonProperty]
			public ulong RoleId { get; }

			public PersistentRole(IUser user, IRole role)
			{
				UserId = user.Id;
				RoleId = role.Id;
			}

			public override string ToString()
			{
				return $"**User Id:** `{UserId}`\n**Role Id:&& `{RoleId}`";
			}
			public string ToString(SocketGuild guild)
			{
				var user = guild.GetUser(UserId).FormatUser() ?? UserId.ToString();
				var role = guild.GetRole(RoleId).FormatRole() ?? RoleId.ToString();
				return $"**User:** `{user}`\n**Role:&& `{role}`";
			}
		}
	}

	namespace NonSavedClasses
	{
		/// <summary>
		/// Same as MyModuleBase except saves guild settings afterwards.
		/// </summary>
		public class MySavingModuleBase : MyModuleBase
		{
			protected override void AfterExecute(CommandInfo command)
			{
				Context.GuildSettings.SaveSettings();
				base.AfterExecute(command);
			}
		}

		/// <summary>
		/// Shorter way to write ModuleBase<MyCommandContext> and also has every command go through the command requirements attribute first.
		/// </summary>
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
				return _guildSettings.TryGetValue(guild?.Id ?? 0, out settings);
			}

			private async Task<IGuildSettings> CreateGuildSettings(Type guildSettingsType, IGuild guild)
			{
				if (_guildSettings.TryGetValue(guild.Id, out IGuildSettings guildSettings))
				{
					return guildSettings;
				}

				var fileInfo = GetActions.GetServerDirectoryFile(guild.Id, Constants.GUILD_SETTINGS_LOCATION);
				if (fileInfo.Exists)
				{
					try
					{
						using (var reader = new StreamReader(fileInfo.FullName))
						{
							guildSettings = (IGuildSettings)JsonConvert.DeserializeObject(reader.ReadToEnd(), guildSettingsType);
						}
						ConsoleActions.WriteLine($"The guild information for {guild.FormatGuild()} has successfully been loaded.");
					}
					catch (Exception e)
					{
						ConsoleActions.ExceptionToConsole(e);
					}
				}
				else
				{
					ConsoleActions.WriteLine($"The guild information file for {guild.FormatGuild()} could not be found; using default.");
				}
				guildSettings = guildSettings ?? (IGuildSettings)Activator.CreateInstance(guildSettingsType);

				var unsetCmdSwitches = Constants.HELP_ENTRIES.Where(x => !guildSettings.CommandSwitches.Select(y => y.Name).CaseInsContains(x.Name)).Select(x => new CommandSwitch(x.Name, x.DefaultEnabled));
				guildSettings.CommandSwitches.AddRange(unsetCmdSwitches);
				guildSettings.CommandsDisabledOnUser.RemoveAll(x => !String.IsNullOrWhiteSpace(x.Name));
				guildSettings.CommandsDisabledOnRole.RemoveAll(x => !String.IsNullOrWhiteSpace(x.Name));
				guildSettings.CommandsDisabledOnChannel.RemoveAll(x => !String.IsNullOrWhiteSpace(x.Name));
				guildSettings.Invites.AddRange((await InviteActions.GetInvites(guild)).Select(x => new BotInvite(x.GuildId, x.Code, x.Uses)));

				if (guildSettings is MyGuildSettings)
				{
					(guildSettings as MyGuildSettings).PostDeserialize(guild);
				}

				return guildSettings;
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
				var aliasStr = $"**Aliases:** {String.Join(", ", Aliases)}";
				var usageStr = $"**Usage:** {Usage}";
				var permStr = $"\n**Base Permission(s):**\n{BasePerm}";
				var descStr = $"\n**Description:**\n{Text}";
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

			public void IncrementUses()
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

		public class SpamPreventionUser
		{
			public IGuildUser User { get; }
			public List<ulong> UsersWhoHaveAlreadyVoted { get; } = new List<ulong>();
			public Dictionary<SpamType, List<BasicTimeInterface>> SpamLists { get; } = new Dictionary<SpamType, List<BasicTimeInterface>>();

			public int VotesRequired { get; private set; } = int.MaxValue;
			public bool PotentialPunishment { get; private set; } = false;
			public bool AlreadyKicked { get; private set; } = false;
			public PunishmentType Punishment { get; private set; } = default(PunishmentType);

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
				Punishment = default(PunishmentType);
			}
			public bool CheckIfAllowedToPunish(SpamPreventionInfo spamPrev, SpamType spamType)
			{
				return SpamLists[spamType].GetCountOfItemsInTimeFrame(spamPrev.RequiredSpamPerMessageOrTimeInterval) >= spamPrev.RequiredSpamInstances;
			}
			public async Task SpamPreventionPunishment(IGuildSettings guildSettings, ITimersModule timers = null)
			{
				//TODO: make this not 0
				await PunishmentActions.AutomaticPunishments(guildSettings, User, Punishment, AlreadyKicked, 0, timers);
			}
		}
	}
}