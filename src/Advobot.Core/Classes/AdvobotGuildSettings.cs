using Advobot.Core.Classes.GuildSettings;
using Advobot.Core.Classes.Rules;
using Advobot.Core.Classes.UserInformation;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Holds settings for a guild. Settings are only saved by calling <see cref="SaveSettings"/>.
	/// </summary>
	[JsonConverter(typeof(JSONBreakingChangeFixer))]
	public sealed class AdvobotGuildSettings : IGuildSettings
	{
		#region Fields and Properties
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
		private List<SelfAssignableRoles> _SelfAssignableGroups;
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
		[JsonProperty("CommandSettings")]
		private CommandSettings _CommandSettings;

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
		public List<SelfAssignableRoles> SelfAssignableGroups
		{
			get => _SelfAssignableGroups ?? (_SelfAssignableGroups = new List<SelfAssignableRoles>());
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
		public CommandSettings CommandSettings
		{
			get => _CommandSettings ?? (_CommandSettings = new CommandSettings());
			set => _CommandSettings = value;
		}

		[JsonIgnore]
		public List<BannedPhraseUserInfo> BannedPhraseUsers { get; } = new List<BannedPhraseUserInfo>();
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
		#endregion

		public string Format()
		{
			var sb = new StringBuilder();
			foreach (var property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
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
			return Format(property.GetValue(this));
		}
		public bool SetLogChannel(LogChannelType logChannelType, ITextChannel channel)
		{
			switch (logChannelType)
			{
				case LogChannelType.Server:
				{
					if (_ServerLogId == (channel?.Id ?? 0))
					{ return false; }
					ServerLog = channel;
					return true;
				}
				case LogChannelType.Mod:
				{
					if (_ModLogId == (channel?.Id ?? 0))
					{ return false; }
					ModLog = channel;
					return true;
				}
				case LogChannelType.Image:
				{
					if (_ImageLogId == (channel?.Id ?? 0))
					{ return false; }
					ImageLog = channel;
					return true;
				}
				default:
				{
					throw new ArgumentException("invalid type", nameof(channel));
				}
			}
		}
		public void SaveSettings()
		{
			IOUtils.OverwriteFile(IOUtils.GetServerDirectoryFile(Guild?.Id ?? 0, Constants.GUILD_SETTINGS_LOC), IOUtils.Serialize(this));
		}
		public void PostDeserialize(SocketGuild guild)
		{
			Guild = guild;

			Task.Run(async () =>
			{
				var invites = await InviteUtils.GetInvitesAsync(guild).CAF();
				var cached = invites.Select(x => new CachedInvite(x.Code, x.Uses));
				lock (Invites)
				{
					Invites.AddRange(cached);
				}
#if false
				ConsoleUtils.WriteLine($"Invites for {guild.Name} have been gotten.");
#endif
			});

			if (_ListedInvite != null)
			{
				_ListedInvite.PostDeserialize(Guild);
			}
			if (_SelfAssignableGroups != null)
			{
				foreach (var group in _SelfAssignableGroups)
				{
					group.PostDeserialize(Guild);
				}
			}

			Loaded = true;
		}

		private string Format(object value)
		{
			if (value == null)
			{
				return "`Nothing`";
			}
			else if (value is IGuildSetting setting)
			{
				return setting.ToString();
			}
			else if (value is ulong ul)
			{
				var chan = Guild.GetChannel(ul);
				if (chan != null)
				{
					return $"`{chan.Format()}`";
				}
				var role = Guild.GetRole(ul);
				if (role != null)
				{
					return $"`{role.Format()}`";
				}
				var user = Guild.GetUser(ul);
				if (user != null)
				{
					return $"`{user.Format()}`";
				}
				return ul.ToString();
			}
			//Because strings are char[] this has to be here so it doesn't go into IEnumerable
			else if (value is string str)
			{
				return String.IsNullOrWhiteSpace(str) ? "`Nothing`" : $"`{str}`";
			}
			//Has to be above IEnumerable too
			else if (value is IDictionary dict)
			{
				var keys = dict.Keys.Cast<object>().Where(x => dict[x] != null);
				return String.Join("\n", keys.Select(x => $"{Format(x)}: {Format(dict[x])}"));
			}
			else if (value is IEnumerable enumarble)
			{
				return String.Join("\n", enumarble.Cast<object>().Select(x => Format(x)));
			}
			return $"`{value.ToString()}`";
		}
	}
}
