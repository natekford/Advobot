using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes.BannedPhrases;
using Advobot.Classes.Permissions;
using Advobot.Classes.SpamPrevention;
using Advobot.Classes.UserInformation;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Advobot.Classes.Rules;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Holds settings for a guild. Settings are only saved by calling <see cref="SaveSettings"/>.
	/// </summary>
	public class GuildSettings : IGuildSettings
	{
		[JsonProperty("WelcomeMessage")]
		private GuildNotification _WelcomeMessage;
		[JsonProperty("GoodbyeMessage")]
		private GuildNotification _GoodbyeMessage;
		[JsonProperty("ListedInvite")]
		private ListedInvite _ListedInvite;
		[JsonProperty("Slowmode")]
		private Slowmode _Slowmode;
		[JsonProperty("Rules")]
		private RuleHolder _Rules;
		[JsonProperty("Prefix")]
		private string _Prefix;
		[JsonProperty("NonVerboseErrors")]
		private bool _NonVerboseErrors;
		[JsonProperty("ServerLog")]
		private ulong _ServerLogId;
		[JsonProperty("ModLog")]
		private ulong _ModLogId;
		[JsonProperty("ImageLog")]
		private ulong _ImageLogId;
		[JsonProperty("MuteRole")]
		private ulong _MuteRoleId;
		[JsonIgnore]
		private ITextChannel _ServerLog;
		[JsonIgnore]
		private ITextChannel _ModLog;
		[JsonIgnore]
		private ITextChannel _ImageLog;
		[JsonIgnore]
		private IRole _MuteRole;
		[JsonProperty("SpamPrevention")]
		private Dictionary<SpamType, SpamPreventionInfo> _SpamPrevention;
		[JsonProperty("RaidPrevention")]
		private Dictionary<RaidType, RaidPreventionInfo> _RaidPrevention;
		[JsonProperty("PersistentRoles")]
		private List<PersistentRole> _PersistentRoles;
		[JsonProperty("BotUsers")]
		private List<BotImplementedPermissions> _BotUsers;
		[JsonProperty("SelfAssignableGroups")]
		private List<SelfAssignableGroup> _SelfAssignableGroups;
		[JsonProperty("Quotes")]
		private List<Quote> _Quotes;
		[JsonProperty("LogActions")]
		private List<LogAction> _LogActions;
		[JsonProperty("IgnoredCommandChannels")]
		private List<ulong> _IgnoredCommandChannels;
		[JsonProperty("IgnoredLogChannels")]
		private List<ulong> _IgnoredLogChannels;
		[JsonProperty("ImageOnlyChannels")]
		private List<ulong> _ImageOnlyChannels;
		[JsonProperty("BannedPhraseStrings")]
		private List<BannedPhrase> _BannedPhraseStrings;
		[JsonProperty("BannedPhraseRegex")]
		private List<BannedPhrase> _BannedPhraseRegex;
		[JsonProperty("BannedPhraseNames")]
		private List<BannedPhrase> _BannedPhraseNames;
		[JsonProperty("BannedPhrasePunishments")]
		private List<BannedPhrasePunishment> _BannedPhrasePunishments;
		[JsonProperty("CommandsDisabledOnUser")]
		private List<CommandOverride> _CommandsDisabledOnUser;
		[JsonProperty("CommandsDisabledOnRole")]
		private List<CommandOverride> _CommandsDisabledOnRole;
		[JsonProperty("CommandsDisabledOnChannel")]
		private List<CommandOverride> _CommandsDisabledOnChannel;
		[JsonProperty("CommandSwitches")]
		private List<CommandSwitch> _CommandSwitches;

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
		public RuleHolder Rules
		{
			get => _Rules ?? (_Rules = new RuleHolder());
			set => _Rules = value;
		}
		[JsonIgnore]
		public string Prefix
		{
			get => _Prefix;
			set => _Prefix = value;
		}
		[JsonIgnore]
		public bool NonVerboseErrors
		{
			get => _NonVerboseErrors;
			set => _NonVerboseErrors = value;
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
		public List<PersistentRole> PersistentRoles
		{
			get => _PersistentRoles ?? (_PersistentRoles = new List<PersistentRole>());
			set => _PersistentRoles = value;
		}
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
		public List<BannedPhrase> BannedPhraseNames
		{
			get => _BannedPhraseNames ?? (_BannedPhraseNames = new List<BannedPhrase>());
			set => _BannedPhraseNames = value;
		}
		[JsonIgnore]
		public List<BannedPhrasePunishment> BannedPhrasePunishments
		{
			get => _BannedPhrasePunishments ?? (_BannedPhrasePunishments = new List<BannedPhrasePunishment>());
			set => _BannedPhrasePunishments = value;
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
		public List<CommandSwitch> CommandSwitches
		{
			get => _CommandSwitches ?? (_CommandSwitches = new List<CommandSwitch>());
			set => _CommandSwitches = value;
		}

		[JsonIgnore]
		public List<BannedPhraseUserInformation> BannedPhraseUsers { get; } = new List<BannedPhraseUserInformation>();
		[JsonIgnore]
		public List<CachedInvite> Invites { get; } = new List<CachedInvite>();
		[JsonIgnore]
		public List<string> EvaluatedRegex { get; } = new List<string>();
		[JsonIgnore]
		public MessageDeletion MessageDeletion { get; } = new MessageDeletion();
		[JsonIgnore]
		public SocketGuild Guild { get; private set; } = null;
		[JsonIgnore]
		public bool Loaded { get; private set; } = false;

		public CommandSwitch[] GetCommands(CommandCategory category)
		{
			return CommandSwitches.Where(x => x.Category == category).ToArray();
		}
		public CommandSwitch GetCommand(string commandNameOrAlias)
		{
			return CommandSwitches.FirstOrDefault(x =>
			{
				if (x.Name.CaseInsEquals(commandNameOrAlias))
				{
					return true;
				}
				else if (x.Aliases != null && x.Aliases.CaseInsContains(commandNameOrAlias))
				{
					return true;
				}
				else
				{
					return false;
				}
			});
		}
		public bool SetLogChannel(LogChannelType logChannelType, ITextChannel channel)
		{
			switch (logChannelType)
			{
				case LogChannelType.Server:
				{
					if (_ServerLogId == channel.Id)
					{
						return false;
					}

					ServerLog = channel;
					return true;
				}
				case LogChannelType.Mod:
				{
					if (_ModLogId == channel.Id)
					{
						return false;
					}

					ModLog = channel;
					return true;
				}
				case LogChannelType.Image:
				{
					if (_ImageLogId == channel.Id)
					{
						return false;
					}

					ImageLog = channel;
					return true;
				}
				default:
				{
					throw new ArgumentException("Invalid channel type supplied.");
				}
			}
		}
		public bool RemoveLogChannel(LogChannelType logChannelType)
		{
			switch (logChannelType)
			{
				case LogChannelType.Server:
				{
					if (_ServerLogId == 0)
					{
						return false;
					}

					ServerLog = null;
					return true;
				}
				case LogChannelType.Mod:
				{
					if (_ModLogId == 0)
					{
						return false;
					}

					ModLog = null;
					return true;
				}
				case LogChannelType.Image:
				{
					if (_ImageLogId == 0)
					{
						return false;
					}

					ImageLog = null;
					return true;
				}
				default:
				{
					throw new ArgumentException("Invalid channel type supplied.");
				}
			}

		}

		public void SaveSettings()
		{
			if (Guild != null)
			{
				SavingAndLoadingActions.OverWriteFile(GetActions.GetServerDirectoryFile(Guild.Id, Constants.GUILD_SETTINGS_LOCATION), SavingAndLoadingActions.Serialize(this));
			}
		}
		public async Task<IGuildSettings> PostDeserialize(IGuild guild)
		{
			Guild = guild as SocketGuild;

			//Add in the default values for commands that aren't set
			var unsetCmds = Constants.HELP_ENTRIES.GetUnsetCommands(CommandSwitches.Select(x => x.Name));
			CommandSwitches.AddRange(unsetCmds.Select(x => new CommandSwitch(x.Name, x.DefaultEnabled)));
			//Remove all that have no name/aren't commands anymore
			CommandSwitches.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name) || Constants.HELP_ENTRIES[x.Name] == null);
			CommandsDisabledOnUser.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
			CommandsDisabledOnRole.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
			CommandsDisabledOnChannel.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
			Invites.AddRange((await InviteActions.GetInvitesAsync(guild)).Select(x => new CachedInvite(x.Code, x.Uses)));

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
			if (_SelfAssignableGroups != null)
			{
				foreach (var group in _SelfAssignableGroups)
				{
					group.Roles.RemoveAll(x => x == null || x.GetRole(Guild) == null);
				}
			}
			if (_PersistentRoles != null)
			{
				_PersistentRoles.RemoveAll(x => x.GetRole(Guild) == null);
			}

			Loaded = true;
			return this;
		}

		public string Format()
		{
			var sb = new StringBuilder();
			foreach (var property in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				//Only get public editable properties
				if (property.GetGetMethod() == null || property.GetSetMethod() == null)
				{
					continue;
				}

				var formatted = Format(property);
				if (String.IsNullOrWhiteSpace(formatted))
				{
					continue;
				}

				sb.AppendLineFeed($"**{property.Name}**:");
				sb.AppendLineFeed($"{formatted}");
				sb.AppendLineFeed("");
			}
			return sb.ToString();
		}
		public string Format(PropertyInfo property)
		{
			return FormatObject(property.GetValue(this));
		}
		private string FormatObject(object value)
		{
			if (value == null)
			{
				return "`Nothing`";
			}
			else if (value is ISetting tempISetting)
			{
				return tempISetting.ToString();
			}
			else if (value is ulong tempUlong)
			{
				var chan = Guild.GetChannel(tempUlong);
				if (chan != null)
				{
					return $"`{chan.FormatChannel()}`";
				}

				var role = Guild.GetRole(tempUlong);
				if (role != null)
				{
					return $"`{role.FormatRole()}`";
				}

				var user = Guild.GetUser(tempUlong);
				if (user != null)
				{
					return $"`{user.FormatUser()}`";
				}

				return tempUlong.ToString();
			}
			//Because strings are char[] this has to be here so it doesn't go into IEnumerable
			else if (value is string tempStr)
			{
				return String.IsNullOrWhiteSpace(tempStr) ? "`Nothing`" : $"`{tempStr}`";
			}
			//Has to be above IEnumerable too
			else if (value is IDictionary tempIDictionary)
			{
				var validKeys = tempIDictionary.Keys.Cast<object>().Where(x => tempIDictionary[x] != null);
				return String.Join("\n", validKeys.Select(x =>
				{
					return $"{FormatObject(x)}: {FormatObject(tempIDictionary[x])}";
				}));
			}
			else if (value is IEnumerable tempIEnumerable)
			{
				return String.Join("\n", tempIEnumerable.Cast<object>().Select(x => FormatObject(x)));
			}
			else
			{
				return $"`{value.ToString()}`";
			}
		}
	}
}
