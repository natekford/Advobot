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
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Holds settings for a guild. Settings are only saved by calling <see cref="SaveSettings"/>.
	/// </summary>
	//[JsonConverter(typeof(AdvobotGuildSettingsFixer))] Uncomment if something needs fixing. MAKE SURE TO UPDATE THE FIXES
	public sealed class AdvobotGuildSettings : IGuildSettings, IPostDeserialize
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

		public CommandSwitch[] GetCommands(CommandCategory category)
		{
			return CommandSwitches.Where(x => x.Category == category).ToArray();
		}
		public CommandSwitch GetCommand(string commandNameOrAlias)
		{
			return CommandSwitches.FirstOrDefault(x =>
			{
				return x.Name.CaseInsEquals(commandNameOrAlias) || x.Aliases != null && x.Aliases.CaseInsContains(commandNameOrAlias);
			});
		}
		public string GetPrefix(IBotSettings botSettings)
		{
			return String.IsNullOrWhiteSpace(Prefix) ? botSettings.Prefix : Prefix;
		}
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
		public string Format(object value)
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
				var validKeys = dict.Keys.Cast<object>().Where(x => dict[x] != null);
				return String.Join("\n", validKeys.Select(x =>
				{
					return $"{Format(x)}: {Format(dict[x])}";
				}));
			}
			else if (value is IEnumerable enumarble)
			{
				return String.Join("\n", enumarble.Cast<object>().Select(x => Format(x)));
			}
			else
			{
				return $"`{value.ToString()}`";
			}
		}
		public bool SetLogChannel(LogChannelType logChannelType, ITextChannel channel)
		{
			switch (logChannelType)
			{
				case LogChannelType.Server:
				{
					if (_ServerLogId == (channel?.Id ?? 0))
					{
						return false;
					}

					ServerLog = channel;
					return true;
				}
				case LogChannelType.Mod:
				{
					if (_ModLogId == (channel?.Id ?? 0))
					{
						return false;
					}

					ModLog = channel;
					return true;
				}
				case LogChannelType.Image:
				{
					if (_ImageLogId == (channel?.Id ?? 0))
					{
						return false;
					}

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
			if (Guild == null)
			{
				return;
			}

			IOUtils.OverwriteFile(IOUtils.GetServerDirectoryFile(Guild.Id, Constants.GUILD_SETTINGS_LOC), IOUtils.Serialize(this));
		}
		public void PostDeserialize(SocketGuild guild)
		{
			Guild = guild;

			//Add in the default values for commands that aren't set
			var unsetCmds = Constants.HELP_ENTRIES.GetUnsetCommands(CommandSwitches.Select(x => x.Name));
			CommandSwitches.AddRange(unsetCmds.Select(x => new CommandSwitch(x.Name, x.DefaultEnabled)));
			//Remove all that have no name/aren't commands anymore
			CommandSwitches.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name) || Constants.HELP_ENTRIES[x.Name] == null);
			CommandsDisabledOnUser.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
			CommandsDisabledOnRole.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
			CommandsDisabledOnChannel.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
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
					group.PostDeserialize(Guild);
				}
			}
			if (_PersistentRoles != null)
			{
				_PersistentRoles.RemoveAll(x => x.GetRole(Guild) == null);
			}

			Loaded = true;
		}
	}

	/// <summary>
	/// A converter to help manually fix guild settings if they get broken by a new change.
	/// </summary>
	internal class AdvobotGuildSettingsFixer : JsonConverter
	{
		private const BindingFlags FLAGS = 0
			| BindingFlags.Instance
			| BindingFlags.NonPublic
			| BindingFlags.Public;

		//Values to replace when building
		//Has to be manually set, but that shouldn't be a problem since the break would have been manually created anyways
		private List<Fix> _Fixes = new List<Fix>
		{
			new Fix
			{
				Path = "WelcomeMessage.Title",
				ErrorValues = new List<string> { "[]" },
				NewValue = null,
			}
		};

		public override bool CanRead => true;
		public override bool CanWrite => false;
		public override bool CanConvert(Type objectType)
		{
			return typeof(IGuildSettings).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			//Fixing the JSON
			var jObj = JObject.Load(reader);
			foreach (var fix in _Fixes)
			{
				if (!(jObj.SelectToken(fix.Path)?.Parent is JProperty jProp))
				{
					continue;
				}
				else if (fix.ErrorValues.Any(x => x.CaseInsEquals(jProp.Value.ToString())))
				{
					jProp.Value = fix.NewValue;
				}
			}

			//Actually creating the object with the JSON
			var value = Activator.CreateInstance(objectType);
			foreach (var setting in Config.GuildSettingsType.GetMembers(FLAGS))
			{
				if (setting is EventInfo || setting is MethodInfo)
				{
					continue;
				}
				if (!(setting.GetCustomAttributes(typeof(JsonPropertyAttribute), false).SingleOrDefault() is JsonPropertyAttribute attr))
				{
					continue;
				}

				var settingName = attr?.PropertyName ?? setting.Name;
				if (String.IsNullOrWhiteSpace(settingName))
				{
					continue;
				}

				if (setting is FieldInfo field)
				{
					field.SetValue(value, jObj[settingName].ToObject(field.FieldType, serializer));
				}
				else if (setting is PropertyInfo prop)
				{
					prop.SetValue(value, jObj[settingName].ToObject(prop.PropertyType, serializer));
				}
			}
			return value;
		}
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		private class Fix
		{
			public string Path;
			public List<string> ErrorValues;
			public string NewValue;
		}
	}
}
