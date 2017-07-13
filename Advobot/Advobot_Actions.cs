using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Advobot
{
	public static class Actions
	{
		#region Saving and Loading
		public static async Task LoadInformation()
		{
			if (Variables.Loaded)
				return;

			HandleBotID(Variables.Client.GetCurrentUser().Id);              //Give the variable Bot_ID the id of the bot
			if (Variables.FirstInstanceOfBotStartingUpWithCurrentKey)
			{
				RestartBot();                                               //Restart so the bot can get the correct botInfo loaded
			}
			Variables.BotName = Variables.Client.GetCurrentUser().Username; //Give the variable Bot_Name the username of the bot

			LoadPermissionNames();                                          //Gets the names of the permission bits in Discord
			LoadCommandInformation();                                       //Gets the information of a command (name, aliases, usage, summary). Has to go after LPN

			await LoadGuilds();                                             //Loads the guilds that attempted to load before the Bot_ID was gotten.
			await UpdateGame();                                             //Have the bot display its game and stream

			HourTimer(null);                                                //Start the hourly timer
			MinuteTimer(null);                                              //Start the minutely timer
			OneFourthSecondTimer(null);                                     //Start the one fourth second timer

			StartupMessages();
			Variables.Loaded = true;
		}

		public static async Task LoadGuilds()
		{
			await Variables.GuildsToBeLoaded.ForEachAsync(async x => await CreateOrGetGuildInfo(x));
		}

		public static async Task<BotGuildInfo> CreateOrGetGuildInfo(IGuild guild)
		{
			if (guild == null)
			{
				return null;
			}

			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
			{
				var path = GetServerFilePath(guild.Id, Constants.GUILD_INFO_LOCATION);
				if (File.Exists(path))
				{
					try
					{
						using (var reader = new StreamReader(path))
						{
							guildInfo = JsonConvert.DeserializeObject<BotGuildInfo>(reader.ReadToEnd());
						}
						WriteLine(String.Format("The guild information for {0} has successfully been loaded.", guild.FormatGuild()));
					}
					catch (Exception e)
					{
						ExceptionToConsole(e);
					}
				}
				else
				{
					WriteLine(String.Format("The guild information file for {0} could not be found; using default.", guild.FormatGuild()));
				}
				guildInfo = guildInfo ?? new BotGuildInfo(guild.Id);

				var cmdUsers = ((List<CommandOverride>)guildInfo.GetSetting(SettingOnGuild.CommandsDisabledOnUser));
				cmdUsers.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
				var cmdRoles = ((List<CommandOverride>)guildInfo.GetSetting(SettingOnGuild.CommandsDisabledOnRole));
				cmdRoles.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
				var cmdChans = ((List<CommandOverride>)guildInfo.GetSetting(SettingOnGuild.CommandsDisabledOnChannel));
				cmdChans.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
				var cmdSwitches = ((List<CommandSwitch>)guildInfo.GetSetting(SettingOnGuild.CommandSwitches));
				cmdSwitches.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));

				foreach (var cmd in Variables.HelpList.Where(x => !cmdSwitches.Select(y => y.Name).CaseInsContains(x.Name)))
				{
					cmdSwitches.Add(new CommandSwitch(cmd.Name, cmd.DefaultEnabled));
				}

				var guildInvites = ((List<BotInvite>)guildInfo.GetSetting(SettingOnGuild.Invites));
				guildInvites.AddRange((await GetInvites(guild)).Select(x => new BotInvite(x.GuildId, x.Code, x.Uses)));
				guildInfo.PostDeserialize(guild.Id);

				Variables.Guilds.Add(guild.Id, guildInfo);
			}
			return guildInfo;
		}

		public static BotGlobalInfo CreateBotInfo()
		{
			BotGlobalInfo botInfo = null;
			var path = GetBaseBotDirectory(Constants.BOT_INFO_LOCATION);
			if (File.Exists(path))
			{
				try
				{
					using (var reader = new StreamReader(path))
					{
						botInfo = JsonConvert.DeserializeObject<BotGlobalInfo>(reader.ReadToEnd());
					}
					WriteLine("The bot information has successfully been loaded.");
				}
				catch (Exception e)
				{
					ExceptionToConsole(e);
				}
			}
			else
			{
				if (!Variables.FirstInstanceOfBotStartingUpWithCurrentKey)
				{
					WriteLine("The bot information file could not be found; using default.");
				}
			}
			botInfo = botInfo ?? new BotGlobalInfo();

			botInfo.PostDeserialize();
			return botInfo;
		}

		public static string Serialize(dynamic obj)
		{
			return JsonConvert.SerializeObject(obj, Formatting.Indented);
		}

		public static async Task MaybeStartBot()
		{
			if (Variables.GotPath && Variables.GotKey && !Variables.Loaded)
			{
				await new Program().Start(Variables.Client);
			}
		}

		public static void HandleBotID(ulong ID)
		{
			Variables.BotID = ID;
			Properties.Settings.Default.BotID = ID;
			Properties.Settings.Default.Save();
		}

		public static void LoadCriticalInformation()
		{
			BotGuildInfo.CreateFieldDictionary();
			BotGlobalInfo.CreateFieldDictionary();
			HandleBotID(Properties.Settings.Default.BotID);
			Variables.FirstInstanceOfBotStartingUpWithCurrentKey = Properties.Settings.Default.BotID == 0;
			Variables.Windows = GetWindowsOrNot();
			Variables.Console = GetConsoleOrGUI();
			Variables.BotInfo = CreateBotInfo();
		}

		public static void LoadCommandInformation()
		{
			foreach (var classType in Assembly.GetCallingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(MyModuleBase))))
			{
				var innerMostNameSpace = classType.Namespace.Substring(classType.Namespace.LastIndexOf('.') + 1);
				if (!Enum.TryParse(innerMostNameSpace, true, out CommandCategory category))
				{
					throw new InvalidOperationException(innerMostNameSpace + " is not currently in the CommandCategory enum.");
				}
				else if (classType.IsNotPublic)
				{
					throw new InvalidOperationException(classType.Name + " is not public and commands will not execute from it.");
				}
				else if (classType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).Any(x => x.GetCustomAttribute(typeof(CommandAttribute)) == null))
				{
					throw new InvalidOperationException(classType.Name + " has a command missing the command attribute.");
				}

				var groupAttr = (GroupAttribute)classType.GetCustomAttribute(typeof(GroupAttribute));
				var name = groupAttr?.Prefix;

				var aliasAttr = (AliasAttribute)classType.GetCustomAttribute(typeof(AliasAttribute));
				var aliases = aliasAttr?.Aliases;

				var summaryAttr = (SummaryAttribute)classType.GetCustomAttribute(typeof(SummaryAttribute));
				var summary = summaryAttr?.Text;

				var usageAttr = (UsageAttribute)classType.GetCustomAttribute(typeof(UsageAttribute));
				var usage = usageAttr == null ? null : name + " " + usageAttr.Usage;

				var permReqsAttr = (PermissionRequirementAttribute)classType.GetCustomAttribute(typeof(PermissionRequirementAttribute));
				var permReqs = permReqsAttr == null ? null : FormatAttribute(permReqsAttr);

				var otherReqsAttr = (OtherRequirementAttribute)classType.GetCustomAttribute(typeof(OtherRequirementAttribute));
				var otherReqs = otherReqsAttr == null ? null : FormatAttribute(otherReqsAttr);

				var defaultEnabledAttr = (DefaultEnabledAttribute)classType.GetCustomAttribute(typeof(DefaultEnabledAttribute));
				var defaultEnabled = defaultEnabledAttr == null ? false : defaultEnabledAttr.Enabled;
				if (defaultEnabledAttr == null)
				{
					throw new InvalidOperationException("Command does not have a default enabled value set: " + name);
				}

				var similarCmds = Variables.HelpList.Where(x => x.Name.CaseInsEquals(name) || (x.Aliases != null && aliases != null && x.Aliases.Intersect(aliases, StringComparer.OrdinalIgnoreCase).Any()));
				if (similarCmds.Any())
				{
					throw new ArgumentException(String.Format("The following commands have conflicts: {0} + {1}", String.Join(" + ", similarCmds.Select(x => x.Name)), name));
				}

				Variables.HelpList.Add(new HelpEntry(name, aliases, usage, JoinNonNullStrings(" | ", new[] { permReqs, otherReqs }), summary, category, defaultEnabled));
			}
			Variables.CommandNames.AddRange(Variables.HelpList.Select(x => x.Name));
		}

		public static void LoadPermissionNames()
		{
			LoadGuildPermissionNames();
			LoadChannelPermissionNames();
		}

		public static void LoadGuildPermissionNames()
		{
			for (int i = 0; i < 64; ++i)
			{
				var name = Enum.GetName(typeof(GuildPermission), i);
				if (name == null)
					continue;

				Variables.GuildPermissions.Add(new BotGuildPermission(name, i));
			}
		}

		public static void LoadChannelPermissionNames()
		{
			const ulong GENERAL_BITS = 0
				| (1U << (int)ChannelPermission.CreateInstantInvite)
				| (1U << (int)ChannelPermission.ManageChannel)
				| (1U << (int)ChannelPermission.ManagePermissions)
				| (1U << (int)ChannelPermission.ManageWebhooks);

			const ulong TEXT_BITS = 0
				| (1U << (int)ChannelPermission.ReadMessages)
				| (1U << (int)ChannelPermission.SendMessages)
				| (1U << (int)ChannelPermission.SendTTSMessages)
				| (1U << (int)ChannelPermission.ManageMessages)
				| (1U << (int)ChannelPermission.EmbedLinks)
				| (1U << (int)ChannelPermission.AttachFiles)
				| (1U << (int)ChannelPermission.ReadMessageHistory)
				| (1U << (int)ChannelPermission.MentionEveryone)
				| (1U << (int)ChannelPermission.UseExternalEmojis)
				| (1U << (int)ChannelPermission.AddReactions);

			const ulong VOICE_BITS = 0
				| (1U << (int)ChannelPermission.Connect)
				| (1U << (int)ChannelPermission.Speak)
				| (1U << (int)ChannelPermission.MuteMembers)
				| (1U << (int)ChannelPermission.DeafenMembers)
				| (1U << (int)ChannelPermission.MoveMembers)
				| (1U << (int)ChannelPermission.UseVAD);

			for (int i = 0; i < 64; i++)
			{
				var name = Enum.GetName(typeof(ChannelPermission), i);
				if (name == null)
					continue;

				if ((GENERAL_BITS & (1U << i)) != 0)
				{
					Variables.ChannelPermissions.Add(new BotChannelPermission(name, i, gen: true));
				}
				if ((TEXT_BITS & (1U << i)) != 0)
				{
					Variables.ChannelPermissions.Add(new BotChannelPermission(name, i, text: true));
				}
				if ((VOICE_BITS & (1U << i)) != 0)
				{
					Variables.ChannelPermissions.Add(new BotChannelPermission(name, i, voice: true));
				}
			}
		}

		public static void CreateFile(string path)
		{
			if (!File.Exists(path))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.Create(path).Close();
			}
		}

		public static void OverWriteFile(string path, string toSave)
		{
			CreateFile(path);
			using (var writer = new StreamWriter(path))
			{
				writer.Write(toSave);
			}
		}

		public static void StartupMessages()
		{
			WriteLine("The current bot prefix is: " + ((string)Variables.BotInfo.GetSetting(SettingOnBot.Prefix)));
			WriteLine(String.Format("Bot took {0:n} milliseconds to load everything.", TimeSpan.FromTicks(DateTime.UtcNow.ToUniversalTime().Ticks - Variables.StartupTime.Ticks).TotalMilliseconds));
			foreach (var e in Enum.GetValues(typeof(SettingOnBot)).Cast<SettingOnBot>().Except(new List<SettingOnBot> { SettingOnBot.SavePath }))
			{
				if (BotGlobalInfo.GetField(e) == null)
				{
					WriteLine(String.Format("Unable to get the global setting for {0}.", e.EnumName()));
				}
			}
			foreach (var e in Enum.GetValues(typeof(SettingOnGuild)).Cast<SettingOnGuild>())
			{
				if (BotGuildInfo.GetField(e) == null)
				{
					WriteLine(String.Format("Unable to get the guild setting for {0}.", e.EnumName()));
				}
			}
		}
		#endregion

		#region Basic Gets
		public static Dictionary<string, string> GetChannelOverwritePermissions(Overwrite overwrite)
		{
			//Create a dictionary to hold the allow/deny/inherit values
			var channelPerms = new Dictionary<String, String>();

			//Make a copy of the channel perm list to check off perms as they go by
			var genericChannelPerms = Variables.ChannelPermissions.Select(x => x.Name).ToList();

			//Add allow perms to the dictionary and remove them from the checklist
			overwrite.Permissions.ToAllowList().ForEach(x =>
			{
				channelPerms.Add(x.ToString(), "Allow");
				genericChannelPerms.Remove(x.ToString());
			});

			//Add deny perms to the dictionary and remove them from the checklist
			overwrite.Permissions.ToDenyList().ForEach(x =>
			{
				channelPerms.Add(x.ToString(), "Deny");
				genericChannelPerms.Remove(x.ToString());
			});

			//Add the remaining perms as inherit after removing all null values
			genericChannelPerms.ForEach(x => channelPerms.Add(x, "Inherit"));

			//Remove these random values that exist for some reason
			channelPerms.Remove("1");
			channelPerms.Remove("3");

			return channelPerms;
		}

		public static Dictionary<string, string> GetFilteredChannelOverwritePermissions(Overwrite overwrite, IGuildChannel channel)
		{
			var dictionary = GetChannelOverwritePermissions(overwrite);
			if (channel is ITextChannel)
			{
				Variables.ChannelPermissions.Where(x => x.Voice).ToList().ForEach(x => dictionary.Remove(x.Name));
			}
			else
			{
				Variables.ChannelPermissions.Where(x => x.Text).ToList().ForEach(x => dictionary.Remove(x.Name));
			}
			return dictionary;
		}

		public static List<CommandSwitch> GetMultipleCommands(BotGuildInfo guildInfo, CommandCategory category)
		{
			return ((List<CommandSwitch>)guildInfo.GetSetting(SettingOnGuild.CommandSwitches)).Where(x => x.Category == category).ToList();
		}

		public static ReturnedArguments GetArgs(ICommandContext context, string input, ArgNumbers argNums, string[] argsToSearchFor = null)
		{
			/* Non specified arguments get left in a list of args going left to right (mentions are not included in this if the bool is true).
			 * Specified arguments get left in a dictionary.
			 */

			var min = argNums.Min;
			var max = argNums.Max;

			if (input == null)
			{
				var list = new List<string>();
				for (int i = 0; i < max; i++)
				{
					list.Add(null);
				}
				if (min == 0)
				{
					return new ReturnedArguments(list, FailureReason.NotFailure);
				}
				else
				{
					return new ReturnedArguments(list, FailureReason.TooFew);
				}
			}

			var args = SplitByCharExceptInQuotes(input, ' ').ToList();
			if (args.Count < min)
			{
				return new ReturnedArguments(args, FailureReason.TooFew);
			}
			else if (args.Count > max)
			{
				return new ReturnedArguments(args, FailureReason.TooMany);
			}

			//Finding the wanted arguments
			var specifiedArgs = new Dictionary<string, string>();
			if (argsToSearchFor != null)
			{
				foreach (var searchArg in argsToSearchFor)
				{
					var arg = GetVariableAndRemove(args, searchArg);
					if (arg != null)
					{
						specifiedArgs.Add(searchArg, arg);
					}
				}
			}

			for (int i = args.Count; i < max; i++)
			{
				args.Add(null);
			}

			return new ReturnedArguments(args, specifiedArgs, context.Message);
		}

		public static ReturnedObject<T> GetEnum<T>(string input, IEnumerable<T> validEnums, IEnumerable<T> invalidEnums = null) where T : struct
		{
			if (!Enum.TryParse<T>(input, true, out T tempEnum))
			{
				return new ReturnedObject<T>(tempEnum, FailureReason.TooFew);
			}
			else if (!validEnums.Contains(tempEnum) || (invalidEnums != null && invalidEnums.Contains(tempEnum)))
			{
				return new ReturnedObject<T>(tempEnum, FailureReason.InvalidEnum);
			}

			return new ReturnedObject<T>(tempEnum, FailureReason.NotFailure);
		}

		public static CommandSwitch GetCommand(BotGuildInfo guildInfo, string input)
		{
			return ((List<CommandSwitch>)guildInfo.GetSetting(SettingOnGuild.CommandSwitches)).FirstOrDefault(x =>
			{
				if (x.Name.CaseInsEquals(input))
				{
					return true;
				}
				else if (x.Aliases != null && x.Aliases.CaseInsContains(input))
				{
					return true;
				}
				else
				{
					return false;
				}
			});
		}

		public static List<string> GetPermissionNames(ulong flags)
		{
			var result = new List<string>();
			for (int i = 0; i < 64; i++)
			{
				ulong bit = 1U << i;
				if ((flags & bit) != 0)
				{
					var name = Variables.GuildPermissions.FirstOrDefault(x => x.Bit == bit).Name;
					if (!String.IsNullOrWhiteSpace(name))
					{
						result.Add(name);
					}
				}
			}
			return result;
		}

		public static FileType? GetFileType(string file)
		{
			if (Enum.TryParse(file, true, out FileType type))
			{
				return type;
			}
			return null;
		}

		public static string[] GetCommands(CommandCategory category)
		{
			return Variables.HelpList.Where(x => x.Category == category).Select(x => x.Name).ToArray();
		}

		public static string GetObjectStringBasic(IGuildUser user)
		{
			return Constants.BASIC_TYPE_USER;
		}

		public static string GetObjectStringBasic(IGuildChannel channel)
		{
			return Constants.BASIC_TYPE_CHANNEL;
		}

		public static string GetObjectStringBasic(IRole role)
		{
			return Constants.BASIC_TYPE_ROLE;
		}

		public static string GetObjectStringBasic(IGuild guild)
		{
			return Constants.BASIC_TYPE_GUILD;
		}

		public static string GetObjectStringBasic(Type type)
		{
			if (type.Equals(typeof(IGuildUser)))
			{
				return Constants.BASIC_TYPE_USER;
			}
			else if (type.Equals(typeof(IGuildChannel)))
			{
				return Constants.BASIC_TYPE_CHANNEL;
			}
			else if (type.Equals(typeof(IRole)))
			{
				return Constants.BASIC_TYPE_ROLE;
			}
			else if (type.Equals(typeof(IGuild)))
			{
				return Constants.BASIC_TYPE_GUILD;
			}
			else
			{
				return "GetObjectStringBasic Error";
			}
		}

		public static string GetHelpString(HelpEntry help, string prefix)
		{
			var aliasStr = String.Format("**Aliases:** {0}", String.Join(", ", help.Aliases));
			var usageStr = String.Format("**Usage:** {0}", help.Usage);
			var permStr = String.Format("\n**Base Permission(s):**\n{0}", help.BasePerm);
			var descStr = String.Format("\n**Description:**\n{0}", help.Text);
			var fullStr = String.Join("\n", new[] { aliasStr, usageStr, permStr, descStr });
			return fullStr.Replace(((string)Variables.BotInfo.GetSetting(SettingOnBot.Prefix)), prefix);
		}

		public static string GetPlural(int i)
		{
			return i == 1 ? "" : "s";
		}

		public static string GetServerFilePath(ulong guildId, string fileName)
		{
			//Make sure the bot's directory exists
			var directory = GetBaseBotDirectory();
			Directory.CreateDirectory(directory);

			//This string will be similar to C:\Users\User\AppData\Roaming\Discord_Servers_... if on using appdata. If not then it can be anything
			return Path.Combine(directory, guildId.ToString(), fileName);
		}

		public static string GetBaseBotDirectory(string nonGuildFileName = null)
		{
			//Make sure a save path exists
			var folder = Properties.Settings.Default.Path;
			if (!Directory.Exists(folder))
				return null;

			//Get the bot's folder
			var botFolder = String.Format("{0}_{1}", Constants.SERVER_FOLDER, Properties.Settings.Default.BotID);

			//Send back the directory
			return String.IsNullOrWhiteSpace(nonGuildFileName) ? Path.Combine(folder, botFolder) : Path.Combine(folder, botFolder, nonGuildFileName);
		}

		public static string GetChannelType(IChannel channel)
		{
			if (channel is ITextChannel)
			{
				return Constants.TEXT_TYPE;
			}
			else if (channel is IVoiceChannel)
			{
				return Constants.VOICE_TYPE;
			}
			else
			{
				return "GetChannelType Error";
			}
		}

		public static string GetVariableAndRemove(List<string> inputList, string searchTerm)
		{
			var first = inputList?.FirstOrDefault(x => x.Substring(0, Math.Max(x.IndexOf(':'), 1)).CaseInsEquals(searchTerm));
			if (first != null)
			{
				inputList.ThreadSafeRemove(first);
				//Return everything after the first colon (the keyword)
				return first.Substring(first.IndexOf(':') + 1);
			}
			return null;
		}

		public static string GetVariable(IEnumerable<string> inputArray, string searchTerm)
		{
			//Get the item
			var first = inputArray?.FirstOrDefault(x => x.Substring(0, Math.Max(x.IndexOf(':'), 1)).CaseInsEquals(searchTerm));
			return first?.Substring(first.IndexOf(':') + 1);
		}

		public static string GetPrefix(BotGuildInfo guildInfo)
		{
			var prefix = ((string)guildInfo.GetSetting(SettingOnGuild.Prefix));
			if (String.IsNullOrWhiteSpace(prefix))
			{
				return ((string)Variables.BotInfo.GetSetting(SettingOnBot.Prefix));
			}
			else
			{
				return prefix;
			}
		}

		public static string GetUptime()
		{
			var span = DateTime.UtcNow.Subtract(Variables.StartupTime);
			return String.Format("{0}:{1}:{2}:{3}", span.Days, span.Hours.ToString("00"), span.Minutes.ToString("00"), span.Seconds.ToString("00"));
		}

		public static bool GetIfUserIsOwner(IGuild guild, IUser user)
		{
			if (guild == null || user == null)
				return false;

			return guild.OwnerId == user.Id || guild.OwnerId == Variables.BotID;
		}

		public static bool GetIfUserIsBotOwner(IUser user)
		{
			return user.Id == ((ulong)Variables.BotInfo.GetSetting(SettingOnBot.BotOwnerID));
		}

		public static bool GetIfUserIsTrustedUser(IUser user)
		{
			return ((List<ulong>)Variables.BotInfo.GetSetting(SettingOnBot.TrustedUsers)).Contains(user.Id);
		}

		public static bool GetIfBypass(string str)
		{
			return Constants.BYPASS_STRING.CaseInsEquals(str);
		}

		public static bool GetWindowsOrNot()
		{
			var windir = Environment.GetEnvironmentVariable("windir");
			return !String.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir);
		}

		public static bool GetConsoleOrGUI()
		{
			try
			{
				var window_height = Console.WindowHeight;
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static double GetMemory()
		{
			if (Variables.Windows)
			{
				using (var PC = new System.Diagnostics.PerformanceCounter("Process", "Working Set - Private", System.Diagnostics.Process.GetCurrentProcess().ProcessName))
				{
					return Convert.ToInt32(PC.NextValue()) / (1024.0 * 1024.0);
				}
			}
			else
			{
				using (var process = System.Diagnostics.Process.GetCurrentProcess())
				{
					return Convert.ToInt32(process.WorkingSet64) / (1024.0 * 1024.0);
				}
			}
		}

		public static int GetLineBreaks(string input)
		{
			return input == null ? 0 : input.Count(x => x == '\n' || x == '\r');
		}

		public static int GetCountOfItemsInTimeFrame<T>(List<T> timeList, int timeFrame = 0) where T : ITimeInterface
		{
			lock (timeList)
			{
				//No timeFrame given means that it's a spam prevention that doesn't check against time, like longmessage or mentions
				var listLength = timeList.Count;
				if (timeFrame <= 0 || listLength < 2)
					return listLength;

				//If there is a timeFrame then that means to gather the highest amount of messages that are in the time frame
				var count = 0;
				for (int i = 0; i < listLength; ++i)
				{
					for (int j = i + 1; j < listLength; ++j)
					{
						if ((int)timeList[j].GetTime().Subtract(timeList[i].GetTime()).TotalSeconds >= timeFrame)
						{
							//Optimization by checking if the time difference between two numbers is too high to bother starting at j - 1
							if ((int)timeList[j].GetTime().Subtract(timeList[j - 1].GetTime()).TotalSeconds > timeFrame)
								i = j;
							break;
						}
					}
				}

				//Remove all that are older than the given timeframe (with an added 1 second margin)
				var nowTime = DateTime.UtcNow;
				for (int i = listLength - 1; i >= 0; --i)
				{
					if ((int)nowTime.Subtract(timeList[i].GetTime()).TotalSeconds > timeFrame + 1)
					{
						timeList.RemoveRange(0, i + 1);
						break;
					}
				}

				return count;
			}
		}

		public static int GetMaxAmountOfUsersToGather(bool bypass)
		{
			return bypass ? int.MaxValue : ((int)Variables.BotInfo.GetSetting(SettingOnBot.MaxUserGatherCount));
		}
		#endregion

		#region Guilds
		public static IGuild GetGuild(IMessage message)
		{
			//Check if the guild can be gotten from the message's channel or author
			return message != null ? (message.Channel as IGuildChannel)?.Guild ?? (message.Author as IGuildUser)?.Guild : null;
		}

		public static IGuild GetGuild(IUser user)
		{
			return (user as IGuildUser)?.Guild;
		}

		public static IGuild GetGuild(IChannel channel)
		{
			return (channel as IGuildChannel)?.Guild;
		}

		public static IGuild GetGuild(IRole role)
		{
			return role?.Guild;
		}

		public static IGuild GetGuild(ulong id)
		{
			return Variables.Client.GetGuild(id);
		}
		#endregion

		#region Channels
		public static async Task ModifyChannelPosition(IGuildChannel channel, int position)
		{
			if (channel == null)
				return;

			IGuildChannel[] channels;
			if (channel is ITextChannel)
			{
				channels = (await channel.Guild.GetTextChannelsAsync()).Where(x => x.Id != channel.Id).OrderBy(x => x.Position).Cast<IGuildChannel>().ToArray();
			}
			else
			{
				channels = (await channel.Guild.GetVoiceChannelsAsync()).Where(x => x.Id != channel.Id).OrderBy(x => x.Position).Cast<IGuildChannel>().ToArray();
			}
			position = Math.Max(0, Math.Min(position, channels.Length));

			var reorderProperties = new ReorderChannelProperties[channels.Length];
			for (int i = 0; i < channels.Length; i++)
			{
				if (i < position)
				{
					reorderProperties[i] = new ReorderChannelProperties(channels[i].Id, i);
				}
				else if (i > position)
				{
					reorderProperties[i] = new ReorderChannelProperties(channels[i - 1].Id, i);
				}
				else
				{
					reorderProperties[i] = new ReorderChannelProperties(channel.Id, i);
				}
			}

			await channel.Guild.ReorderChannelsAsync(reorderProperties);
		}

		public static ReturnedObject<IGuildChannel> GetChannel(SocketCommandContext context, ObjectVerification[] checkingTypes, bool mentions, string input)
		{
			IGuildChannel channel = null;
			if (!String.IsNullOrWhiteSpace(input))
			{
				if (ulong.TryParse(input, out ulong channelID))
				{
					channel = GetChannel(context.Guild, channelID);
				}
				else if (MentionUtils.TryParseChannel(input, out channelID))
				{
					channel = GetChannel(context.Guild, channelID);
				}
				else
				{
					var channels = (context.Guild as SocketGuild).VoiceChannels.Where(x => x.Name.CaseInsEquals(input));
					if (channels.Count() == 1)
					{
						channel = channels.First();
					}
					else if (channels.Count() > 1)
					{
						return new ReturnedObject<IGuildChannel>(channel, FailureReason.TooMany);
					}
				}
			}

			if (channel == null)
			{
				if (mentions)
				{
					var channelMentions = context.Message.MentionedChannels.Select(x => x.Id);
					if (channelMentions.Count() == 1)
					{
						channel = GetChannel(context.Guild, channelMentions.First());
					}
					else if (channelMentions.Count() > 1)
					{
						return new ReturnedObject<IGuildChannel>(channel, FailureReason.TooMany);
					}
				}
			}

			return GetChannel(context, checkingTypes, channel);
		}

		public static ReturnedObject<IGuildChannel> GetChannel(SocketCommandContext context, ObjectVerification[] checkingTypes, ulong inputID)
		{
			var channel = GetChannel(context.Guild, inputID);
			if (channel == null)
			{
				return new ReturnedObject<IGuildChannel>(channel, FailureReason.TooFew);
			}

			return GetChannel(context, checkingTypes, channel);
		}

		public static ReturnedObject<IGuildChannel> GetChannel(SocketCommandContext context, ObjectVerification[] checkingTypes, IGuildChannel channel)
		{
			return GetChannel(context.Guild, context.User as IGuildUser, checkingTypes, channel);
		}

		public static ReturnedObject<T> GetChannel<T>(IGuild guild, IGuildUser user, ObjectVerification[] checkingTypes, T channel) where T : IGuildChannel
		{
			if (channel == null)
			{
				return new ReturnedObject<T>(channel, FailureReason.TooFew);
			}

			var bot = GetBot(guild);
			foreach (var type in checkingTypes)
			{
				if (!GetIfUserCanDoActionOnChannel(channel, user, type))
				{
					return new ReturnedObject<T>(channel, FailureReason.UserInability);
				}
				else if (!GetIfUserCanDoActionOnChannel(channel, bot, type))
				{
					return new ReturnedObject<T>(channel, FailureReason.BotInability);
				}

				switch (type)
				{
					case ObjectVerification.IsDefault:
					{
						if (channel.Id == guild.DefaultChannelId)
						{
							return new ReturnedObject<T>(channel, FailureReason.DefaultChannel);
						}
						break;
					}
					case ObjectVerification.IsText:
					{
						if (!(channel is ITextChannel))
						{
							return new ReturnedObject<T>(channel, FailureReason.ChannelType);
						}
						break;
					}
					case ObjectVerification.IsVoice:
					{
						if (!(channel is IVoiceChannel))
						{
							return new ReturnedObject<T>(channel, FailureReason.ChannelType);
						}
						break;
					}
				}
			}

			return new ReturnedObject<T>(channel, FailureReason.NotFailure);
		}

		public static IGuildChannel GetChannel(IGuild guild, ulong ID)
		{
			return (guild as SocketGuild).GetChannel(ID);
		}

		public static bool GetIfUserCanDoActionOnChannel(IGuildChannel target, IGuildUser user, ObjectVerification type)
		{
			if (target == null || user == null)
				return false;

			var channelPerms = user.GetPermissions(target);
			var guildPerms = user.GuildPermissions;

			var dontCheckReadPerms = target is IVoiceChannel;
			switch (type)
			{
				case ObjectVerification.CanBeRead:
				{
					return (dontCheckReadPerms || channelPerms.ReadMessages);
				}
				case ObjectVerification.CanCreateInstantInvite:
				{
					return (dontCheckReadPerms || channelPerms.ReadMessages) && channelPerms.CreateInstantInvite;
				}
				case ObjectVerification.CanBeManaged:
				{
					return (dontCheckReadPerms || channelPerms.ReadMessages) && channelPerms.ManageChannel;
				}
				case ObjectVerification.CanModifyPermissions:
				{
					return (dontCheckReadPerms || channelPerms.ReadMessages) && channelPerms.ManageChannel && channelPerms.ManagePermissions;
				}
				case ObjectVerification.CanBeReordered:
				{
					return (dontCheckReadPerms || channelPerms.ReadMessages) && guildPerms.ManageChannels;
				}
				case ObjectVerification.CanDeleteMessages:
				{
					return (dontCheckReadPerms || channelPerms.ReadMessages) && channelPerms.ManageMessages;
				}
				case ObjectVerification.CanMoveUsers:
				{
					return dontCheckReadPerms && channelPerms.MoveMembers;
				}
				default:
				{
					return true;
				}
			}
		}

		public static ulong GetOverwriteAllowBits(IGuildChannel channel, IRole role)
		{
			return channel.GetPermissionOverwrite(role)?.AllowValue ?? 0;
		}

		public static ulong GetOverwriteAllowBits(IGuildChannel channel, IUser user)
		{
			return channel.GetPermissionOverwrite(user)?.AllowValue ?? 0;
		}

		public static ulong GetOverwriteDenyBits(IGuildChannel channel, IRole role)
		{
			return channel.GetPermissionOverwrite(role)?.DenyValue ?? 0;
		}

		public static ulong GetOverwriteDenyBits(IGuildChannel channel, IUser user)
		{
			return channel.GetPermissionOverwrite(user)?.DenyValue ?? 0;
		}

		public static ulong AddChannelPermissions(ulong startBits, params ChannelPermission[] permissions)
		{
			foreach (var permission in permissions)
			{
				startBits = startBits & ~(1U << (int)permission);
			}
			return startBits;
		}

		public static ulong RemoveChannelPermissions(ulong startBits, params ChannelPermission[] permissions)
		{
			foreach (var permission in permissions)
			{
				startBits = startBits | (1U << (int)permission);
			}
			return startBits;
		}

		public static async Task ModifyOverwrite(IGuildChannel channel, IRole role, ulong allowBits, ulong denyBits)
		{
			await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(allowBits, denyBits));
		}

		public static async Task ModifyOverwrite(IGuildChannel channel, IUser user, ulong allowBits, ulong denyBits)
		{
			await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(allowBits, denyBits));
		}
		#endregion

		#region Roles
		public static async Task<IRole> GetMuteRole(IGuild guild, BotGuildInfo guildInfo)
		{
			//Even though GetRole requires an IGuildUser to check against, I can just throw in the bot a second time and it will be fine for the most part. Failures from this will be UserInability instead.
			var returnedMuteRole = GetRole(guild, GetBot(guild), new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsManaged }, ((DiscordObjectWithID<IRole>)guildInfo.GetSetting(SettingOnGuild.MuteRole))?.Object);
			var muteRole = returnedMuteRole.Object;
			if (returnedMuteRole.Reason != FailureReason.NotFailure)
			{
				muteRole = await CreateMuteRoleIfNotFound(guildInfo, guild, muteRole);
			}
			return muteRole;
		}

		public static async Task<IRole> CreateMuteRoleIfNotFound(BotGuildInfo guildInfo, IGuild guild, IRole muteRole)
		{
			if (muteRole == null)
			{
				muteRole = await guild.CreateRoleAsync("Advobot_Mute", new GuildPermissions(0));
				guildInfo.SetSetting(SettingOnGuild.MuteRole, new DiscordObjectWithID<IRole>(muteRole));
			}

			const uint TEXT_PERMS = 0
			| (1U << (int)ChannelPermission.CreateInstantInvite)
			| (1U << (int)ChannelPermission.ManageChannel)
			| (1U << (int)ChannelPermission.ManagePermissions)
			| (1U << (int)ChannelPermission.ManageWebhooks)
			| (1U << (int)ChannelPermission.SendMessages)
			| (1U << (int)ChannelPermission.ManageMessages)
			| (1U << (int)ChannelPermission.AddReactions);
			(await guild.GetTextChannelsAsync()).ToList().ForEach(x =>
			{
				x.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(0, TEXT_PERMS));
			});

			const uint VOICE_PERMS = 0
			| (1U << (int)ChannelPermission.CreateInstantInvite)
			| (1U << (int)ChannelPermission.ManageChannel)
			| (1U << (int)ChannelPermission.ManagePermissions)
			| (1U << (int)ChannelPermission.ManageWebhooks)
			| (1U << (int)ChannelPermission.Speak)
			| (1U << (int)ChannelPermission.MuteMembers)
			| (1U << (int)ChannelPermission.DeafenMembers)
			| (1U << (int)ChannelPermission.MoveMembers);
			(await guild.GetVoiceChannelsAsync()).ToList().ForEach(x =>
			{
				x.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(0, VOICE_PERMS));
			});

			return muteRole;
		}

		public static async Task<int> ModifyRolePosition(IRole role, int position)
		{
			if (role == null)
				return -1;

			var roles = role.Guild.Roles.Where(x => x.Id != role.Id && x.Position < GetUserPosition(GetBot(role.Guild))).OrderBy(x => x.Position).ToArray();
			position = Math.Max(1, Math.Min(position, roles.Length));

			var reorderProperties = new ReorderRoleProperties[roles.Length + 1];
			for (int i = 0; i < reorderProperties.Length; i++)
			{
				if (i == position)
				{
					reorderProperties[i] = new ReorderRoleProperties(role.Id, i);
				}
				else if (i > position)
				{
					reorderProperties[i] = new ReorderRoleProperties(roles[i - 1].Id, i);
				}
				else if (i < position)
				{
					reorderProperties[i] = new ReorderRoleProperties(roles[i].Id, i);
				}
			}

			await role.Guild.ReorderRolesAsync(reorderProperties);
			return reorderProperties.FirstOrDefault(x => x.Id == role.Id)?.Position ?? -1;
		}

		public static async Task GiveRole(IGuildUser user, IRole role)
		{
			if (role == null)
				return;
			if (user.RoleIds.Contains(role.Id))
				return;
			await user.AddRoleAsync(role);
		}

		public static async Task GiveRoles(IGuildUser user, IEnumerable<IRole> roles)
		{
			if (!roles.Any())
				return;

			await user.AddRolesAsync(roles);
		}

		public static async Task TakeRole(IGuildUser user, IRole role)
		{
			if (role == null)
				return;
			if (!user.RoleIds.Contains(role.Id))
				return;
			await user.RemoveRoleAsync(role);
		}

		public static async Task TakeRoles(IGuildUser user, IEnumerable<IRole> roles)
		{
			if (!roles.Any())
				return;

			await user.RemoveRolesAsync(roles);
		}

		public static async Task MuteUser(BotGuildInfo guildInfo, IGuildUser user, int time = -1)
		{
			var muteRole = await GetMuteRole(user.Guild, guildInfo);
			await GiveRole(user, muteRole);
			if (time > 0)
			{
				Variables.PunishedUsers.Add(new RemovablePunishment(user.Guild, user.Id, muteRole, DateTime.UtcNow.AddMinutes(time)));
			}
		}

		public static ReturnedObject<IRole> GetRole(ICommandContext context, ObjectVerification[] checkingTypes, bool mentions, string input)
		{
			IRole role = null;
			if (!String.IsNullOrWhiteSpace(input))
			{
				if (ulong.TryParse(input, out ulong roleID))
				{
					role = GetRole(context.Guild, roleID);
				}
				else if (MentionUtils.TryParseRole(input, out roleID))
				{
					role = GetRole(context.Guild, roleID);
				}
				else
				{
					var roles = context.Guild.Roles.Where(x => x.Name.CaseInsEquals(input));
					if (roles.Count() == 1)
					{
						role = roles.First();
					}
					else if (roles.Count() > 1)
					{
						return new ReturnedObject<IRole>(role, FailureReason.TooMany);
					}
				}
			}

			if (role == null)
			{
				if (mentions)
				{
					var roleMentions = context.Message.MentionedRoleIds;
					if (roleMentions.Count() == 1)
					{
						role = GetRole(context.Guild, roleMentions.First());
					}
					else if (roleMentions.Count() > 1)
					{
						return new ReturnedObject<IRole>(role, FailureReason.TooMany);
					}
				}
			}

			return GetRole(context, checkingTypes, role);
		}

		public static ReturnedObject<IRole> GetRole(ICommandContext context, ObjectVerification[] checkingTypes, ulong inputID)
		{
			var role = GetRole(context.Guild, inputID);
			if (role == null)
			{
				return new ReturnedObject<IRole>(role, FailureReason.TooFew);
			}

			return GetRole(context, checkingTypes, role);
		}

		public static ReturnedObject<IRole> GetRole(ICommandContext context, ObjectVerification[] checkingTypes, IRole role)
		{
			return GetRole(context.Guild, context.User as IGuildUser, checkingTypes, role);
		}

		public static ReturnedObject<T> GetRole<T>(IGuild guild, IGuildUser user, ObjectVerification[] checkingTypes, T role) where T : IRole
		{
			if (role == null)
			{
				return new ReturnedObject<T>(role, FailureReason.TooFew);
			}

			var bot = GetBot(guild);
			foreach (var type in checkingTypes)
			{
				if (!GetIfUserCanDoActionOnRole(role, user, type))
				{
					return new ReturnedObject<T>(role, FailureReason.UserInability);
				}
				else if (!GetIfUserCanDoActionOnRole(role, bot, type))
				{
					return new ReturnedObject<T>(role, FailureReason.BotInability);
				}

				switch (type)
				{
					case ObjectVerification.IsEveryone:
					{
						if (guild.EveryoneRole.Id == role.Id)
						{
							return new ReturnedObject<T>(role, FailureReason.EveryoneRole);
						}
						break;
					}
					case ObjectVerification.IsManaged:
					{
						if (role.IsManaged)
						{
							return new ReturnedObject<T>(role, FailureReason.ManagedRole);
						}
						break;
					}
				}
			}

			return new ReturnedObject<T>(role, FailureReason.NotFailure);
		}

		public static EditableDiscordObject<IRole>? GetValidEditRoles(ICommandContext context, IEnumerable<string> input)
		{
			//Gather the users
			var success = new List<IRole>();
			var failure = new List<string>();
			if (input == null || !input.Any())
			{
				return null;
			}
			else
			{
				var bot = GetBot(context.Guild);
				input.ToList().ForEach(x =>
				{
					var returnedRole = GetRole(context, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged }, false, x);
					if (returnedRole.Reason == FailureReason.NotFailure)
					{
						success.Add(returnedRole.Object);
					}
					else
					{
						failure.Add(x);
					}
				});
			}
			return new EditableDiscordObject<IRole>(success, failure);
		}

		public static IRole GetRole(IGuild guild, ulong ID)
		{
			return guild.GetRole(ID);
		}

		public static bool GetIfUserCanDoActionOnRole(IRole target, IGuildUser user, ObjectVerification type)
		{
			if (target == null || user == null)
				return false;

			switch (type)
			{
				case ObjectVerification.CanBeEdited:
				{
					return target.Position < GetUserPosition(user);
				}
				default:
				{
					return true;
				}
			}
		}
		#endregion

		#region Users
		public static IEnumerable<SocketGuildUser> GetUsersTheBotAndUserCanEdit(SocketCommandContext context)
		{
			return context.Guild.Users.Where(x => GetIfUserCanBeModifiedByUser(context.User, x) && GetIfUserCanBeModifiedByUser(GetBot(context.Guild), x));
		}

		public static async Task ChangeNickname(IGuildUser user, string newNN)
		{
			await user.ModifyAsync(x => x.Nickname = newNN ?? user.Username);
		}

		public static async Task RenicknameALotOfPeople(ICommandContext context, List<IGuildUser> validUsers, string newNickname)
		{
			//User count checking and stuff
			var userCount = validUsers.Count;
			if (userCount == 0)
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR("Unable to find any users matching the search criteria which are able to be edited by the user and bot."));
				return;
			}

			//Have the bot stay in the typing state and have a message that can be updated 
			var msg = await SendChannelMessage(context, String.Format("Attempting to change the nickname of `{0}` user{1}.", userCount, GetPlural(userCount))) as IUserMessage;

			//Actually rename them all
			var count = 0;
			await validUsers.ForEachAsync(async x =>
			{
				++count;
				if (count % 10 == 0)
				{
					await msg.ModifyAsync(y => y.Content = String.Format("ETA on completion: `{0}` seconds.", (int)((userCount - count) * 1.2)));
				}

				await ChangeNickname(x, newNickname);
			});

			//Get rid of stuff and send a success message
			await DeleteMessage(msg);
			await MakeAndDeleteSecondaryMessage(context, String.Format("Successfully changed the nicknames of `{0}` user{1}.", count, GetPlural(count)));
		}

		public static async Task BotBanUser(IGuild guild, ulong userID, int days = 1, string reason = null)
		{
			await guild.AddBanAsync(userID, days, FormatBotReason(reason));
		}

		public static async Task UserBanUser(ICommandContext context, ulong userID, int days = 1, string reason = null)
		{
			await context.Guild.AddBanAsync(userID, days, FormatUserReason(context.User, reason));
		}

		public static async Task BotKickUser(IGuildUser user, string reason = null)
		{
			await user.KickAsync(FormatBotReason(reason));
		}

		public static async Task UserKickUser(ICommandContext context, IGuildUser user, string reason = null)
		{
			await user.KickAsync(FormatUserReason(context.User, reason));
		}

		public static ReturnedObject<IGuildUser> GetGuildUser(ICommandContext context, ObjectVerification[] checkingTypes, bool mentions, string input)
		{
			IGuildUser user = null;
			if (!String.IsNullOrWhiteSpace(input))
			{
				if (ulong.TryParse(input, out ulong userID))
				{
					user = GetGuildUser(context.Guild, userID);
				}
				else if (MentionUtils.TryParseUser(input, out userID))
				{
					user = GetGuildUser(context.Guild, userID);
				}
				else
				{
					var users = (context.Guild as SocketGuild).Users.Where(x => x.Username.CaseInsEquals(input));
					if (users.Count() == 1)
					{
						user = users.First();
					}
					else if (users.Count() > 1)
					{
						return new ReturnedObject<IGuildUser>(user, FailureReason.TooMany);
					}
				}
			}

			if (user == null)
			{
				if (mentions)
				{
					var userMentions = context.Message.MentionedUserIds;
					if (userMentions.Count() == 1)
					{
						user = GetGuildUser(context.Guild, userMentions.First());
					}
					else if (userMentions.Count() > 1)
					{
						return new ReturnedObject<IGuildUser>(user, FailureReason.TooMany);
					}
				}
			}

			return GetGuildUser(context, checkingTypes, user);
		}

		public static ReturnedObject<IGuildUser> GetGuildUser(ICommandContext context, ObjectVerification[] checkingTypes, ulong inputID)
		{
			var user = GetGuildUser(context.Guild, inputID);
			return GetGuildUser(context, checkingTypes, user);
		}

		public static ReturnedObject<IGuildUser> GetGuildUser(ICommandContext context, ObjectVerification[] checkingTypes, IGuildUser user)
		{
			return GetGuildUser(context.Guild, context.User as IGuildUser, checkingTypes, user);
		}

		public static ReturnedObject<T> GetGuildUser<T>(IGuild guild, IGuildUser currUser, ObjectVerification[] checkingTypes, T user) where T : IGuildUser
		{
			if (user == null)
			{
				return new ReturnedObject<T>(user, FailureReason.TooFew);
			}

			var bot = GetBot(guild);
			foreach (var type in checkingTypes)
			{
				if (!GetIfUserCanDoActionOnUser(currUser, type, user))
				{
					return new ReturnedObject<T>(user, FailureReason.UserInability);
				}
				else if (!GetIfUserCanDoActionOnUser(bot, type, user))
				{
					return new ReturnedObject<T>(user, FailureReason.BotInability);
				}
			}

			return new ReturnedObject<T>(user, FailureReason.NotFailure);
		}

		public static EditableDiscordObject<IGuildUser>? GetValidEditUsers(ICommandContext context)
		{
			//Gather the users
			var input = context.Message.MentionedUserIds.ToList();
			var success = new List<IGuildUser>();
			var failure = new List<string>();
			if (!input.Any())
			{
				return null;
			}
			else
			{
				var bot = GetBot(context.Guild);
				input.ForEach(x =>
				{
					var targetUser = GetGuildUser(context.Guild, x);
					if (GetIfUserCanBeModifiedByUser(context.User as IGuildUser, targetUser) && GetIfUserCanBeModifiedByUser(bot, targetUser))
					{
						success.Add(targetUser);
					}
					else
					{
						failure.Add(targetUser.FormatUser());
					}
				});
			}
			return new EditableDiscordObject<IGuildUser>(success, failure);
		}

		public static IGuildUser GetGuildUser(IGuild guild, ulong ID)
		{
			return (guild as SocketGuild).GetUser(ID);
		}

		public static IGuildUser GetBot(IGuild guild)
		{
			return (guild as SocketGuild).CurrentUser;
		}

		public static IUser GetGlobalUser(ulong ID)
		{
			return Variables.Client.GetUser(ID);
		}

		public static IUser GetGlobalUser(string idStr)
		{
			if (ulong.TryParse(idStr, out ulong ID))
			{
				return GetGlobalUser(ID);
			}
			return null;
		}

		public static IUser GetBotOwner()
		{
			return Variables.Client.GetUser(((ulong)Variables.BotInfo.GetSetting(SettingOnBot.BotOwnerID)));
		}

		public static bool GetIfUserCanDoActionOnUser(IGuildUser currUser, ObjectVerification type, IGuildUser targetUser)
		{
			if (targetUser == null || currUser == null)
				return false;

			switch (type)
			{
				case ObjectVerification.CanBeMovedFromChannel:
				{
					return GetIfUserCanDoActionOnChannel(targetUser.VoiceChannel, currUser, ObjectVerification.CanMoveUsers);
				}
				case ObjectVerification.CanBeEdited:
				{
					return GetIfUserCanBeModifiedByUser(currUser, targetUser);
				}
				default:
				{
					return true;
				}
			}
		}

		public static bool GetIfUserCanBeModifiedByUser(IUser currUser, IUser targetUser)
		{
			return GetIfUserCanBeModifiedByUser(currUser as IGuildUser, targetUser as IGuildUser);
		}

		public static bool GetIfUserCanBeModifiedByUser(IGuildUser currUser, IGuildUser targetUser)
		{
			if (currUser.Id == Variables.BotID && targetUser.Id == Variables.BotID)
			{
				return true;
			}

			var bannerPosition = GetUserPosition(currUser);
			var banneePosition = GetUserPosition(targetUser);
			return bannerPosition > banneePosition;
		}

		public static int GetUserPosition(IUser user)
		{
			//Make sure they're a SocketGuildUser
			var tempUser = user as SocketGuildUser;
			if (user == null)
				return -1;

			return tempUser.Hierarchy;
		}
		#endregion

		#region Emotes
		public static ReturnedObject<Emote> GetEmote(ICommandContext context, bool usage, string input)
		{
			Emote emote = null;
			if (!String.IsNullOrWhiteSpace(input))
			{
				if (Emote.TryParse(input, out emote))
				{
					return new ReturnedObject<Emote>(emote, FailureReason.NotFailure);
				}
				else if (ulong.TryParse(input, out ulong emoteID))
				{
					emote = context.Guild.Emotes.FirstOrDefault(x => x.Id == emoteID);
				}
				else
				{
					var emotes = context.Guild.Emotes.Where(x => x.Name.CaseInsEquals(input));
					if (emotes.Count() == 1)
					{
						emote = emotes.First();
					}
					else if (emotes.Count() > 1)
					{
						return new ReturnedObject<Emote>(emote, FailureReason.TooMany);
					}
				}
			}

			if (emote == null)
			{
				if (usage)
				{
					var emoteMentions = context.Message.Tags.Where(x => x.Type == TagType.Emoji);
					if (emoteMentions.Count() == 1)
					{
						emote = emoteMentions.First().Value as Emote;
					}
					else if (emoteMentions.Count() > 1)
					{
						return new ReturnedObject<Emote>(emote, FailureReason.TooMany);
					}
				}
			}

			return new ReturnedObject<Emote>(emote, FailureReason.NotFailure);
		}
		#endregion

		#region Messages
		private static readonly string DESC_LEN_ERROR = String.Format("The description is over `{0}` characters and will be sent as a text file instead.", Constants.MAX_DESCRIPTION_LENGTH);
		private static readonly string DESC_LINE_ERROR = String.Format("The description is over `{0}` lines and will be sent as a text file instead.", Constants.MAX_DESCRIPTION_LINES);
		private static readonly string FIELD_LEN_ERROR = String.Format("This field is over `{0}` characters and will be sent as a text file instead.", Constants.MAX_FIELD_VALUE_LENGTH);
		private static readonly string FIELD_LINE_ERROR = String.Format("This field is over `{0}` lines and will be sent as a text file instead.", Constants.MAX_FIELD_LINES);
		private static readonly string TOTAL_CHAR_ERROR = String.Format("`{0}` char limit close.", Constants.MAX_EMBED_TOTAL_LENGTH);
		public static async Task SendEmbedMessage(IMessageChannel channel, EmbedBuilder embed, string content = null)
		{
			var guild = GetGuild(channel);
			if (guild == null)
				return;

			//Embeds have a global limit of 6000 characters
			var totalChars = 0
				+ embed?.Author?.Name?.Length
				+ embed?.Title?.Length
				+ embed?.Footer?.Text?.Length;

			//Descriptions can only be 2048 characters max and mobile can only show up to 20 line breaks
			string badDesc = null;
			if (embed.Description?.Length > Constants.MAX_DESCRIPTION_LENGTH)
			{
				badDesc = embed.Description;
				embed.WithDescription(DESC_LEN_ERROR);
			}
			else if (GetLineBreaks(embed.Description) > Constants.MAX_DESCRIPTION_LINES)
			{
				badDesc = embed.Description;
				embed.WithDescription(DESC_LINE_ERROR);
			}
			totalChars += embed.Description?.Length ?? 0;

			//Embeds can only be 1024 characters max and mobile can only show up to 5 line breaks
			var badFields = new List<Tuple<int, string>>();
			for (int i = 0; i < embed.Fields.Count; i++)
			{
				var field = embed.Fields[i];
				var value = field.Value.ToString();
				if (totalChars > Constants.MAX_EMBED_TOTAL_LENGTH - 1500)
				{
					badFields.Add(new Tuple<int, string>(i, value));
					field.WithName(i.ToString());
					field.WithValue(TOTAL_CHAR_ERROR);
				}
				else if (value?.Length > Constants.MAX_FIELD_VALUE_LENGTH)
				{
					badFields.Add(new Tuple<int, string>(i, value));
					field.WithValue(FIELD_LEN_ERROR);
				}
				else if (GetLineBreaks(value) > Constants.MAX_FIELD_LINES)
				{
					badFields.Add(new Tuple<int, string>(i, value));
					field.WithValue(FIELD_LINE_ERROR);
				}
				totalChars += value?.Length ?? 0;
				totalChars += field.Name?.Length ?? 0;
			}

			try
			{
				await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + content ?? "", false, embed.WithCurrentTimestamp());
			}
			catch (Exception e)
			{
				ExceptionToConsole(e);
				await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + ERROR(e.Message));
				return;
			}

			//Go send the description/fields that had an error
			if (badDesc != null)
			{
				await WriteAndUploadTextFile(guild, channel, badDesc, "Description_");
			}
			foreach (var tuple in badFields)
			{
				var num = tuple.Item1;
				var val = tuple.Item2;
				await WriteAndUploadTextFile(guild, channel, val, String.Format("Field_{0}_", num));
			}
		}

		public static async Task<IMessage> SendChannelMessage(ICommandContext context, string message)
		{
			return await SendChannelMessage(context.Channel, message);
		}

		public static async Task<IMessage> SendChannelMessage(IMessageChannel channel, string message)
		{
			return await SendChannelMessage(channel as ITextChannel, message);
		}

		public static async Task<IMessage> SendChannelMessage(ITextChannel channel, string message)
		{
			var guild = GetGuild(channel);
			if (channel == null || guild == null)
				return null;

			message = message.CaseInsReplace(guild.EveryoneRole.Mention, Constants.FAKE_EVERYONE);
			message = message.CaseInsReplace("@everyone", Constants.FAKE_EVERYONE);
			message = message.CaseInsReplace("\tts", Constants.FAKE_TTS);

			IMessage msg = null;
			if (message.Length >= Constants.MAX_MESSAGE_LENGTH_LONG)
			{
				msg = await WriteAndUploadTextFile(guild, channel, message, "Long_Message_", "The response is a long message and was sent as a text file instead");
			}
			else
			{
				msg = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + message);
			}
			return msg;
		}

		public static async Task<IMessage> SendDMMessage(IDMChannel channel, string message)
		{
			if (channel == null)
				return null;

			return await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + message);
		}

		public static async Task<int> RemoveMessages(IMessageChannel channel, IMessage fromMessage, int requestCount)
		{
			var guildChannel = channel as ITextChannel;
			if (guildChannel == null)
				return 0;

			var messages = await channel.GetMessagesAsync(fromMessage, Direction.Before, requestCount).Flatten();
			await DeleteMessages(channel, messages);
			return messages.Count();
		}

		public static async Task<int> RemoveMessages(IMessageChannel channel, IMessage msg, IUser user, int requestCount)
		{
			var guildChannel = channel as ITextChannel;
			if (guildChannel == null)
				return 0;

			if (user == null)
			{
				return await RemoveMessages(channel, msg, requestCount);
			}

			var deletedCount = 0;
			while (requestCount > 0)
			{
				//Get the current messages and ones that aren't null
				var messages = await channel.GetMessagesAsync(msg, Direction.Before, 100).Flatten();
				if (!messages.Any())
					break;

				//Set the from message as the last of the currently grabbed ones
				msg = messages.Last();

				//Check for messages of the targetted user
				messages = messages.Where(x => x.Author.Id == user.Id);
				if (!messages.Any())
					break;

				var gatheredForUserAmt = messages.Count();
				messages = messages.ToList().GetUpToAndIncludingMinNum(requestCount, gatheredForUserAmt, 100);

				//Delete them in a try catch due to potential errors
				var msgAmt = messages.Count();
				try
				{
					await DeleteMessages(channel, messages);
					deletedCount += msgAmt;
				}
				catch
				{
					WriteLine(String.Format("Unable to delete {0} messages on the guild {1} on channel {2}.", msgAmt, guildChannel.Guild.FormatGuild(), guildChannel.FormatChannel()));
					break;
				}

				//Leave if the message count gathered implies that enough user messages have been deleted 
				if (msgAmt < gatheredForUserAmt)
					break;

				requestCount -= msgAmt;
			}
			return deletedCount;
		}

		public static async Task<List<IMessage>> GetMessages(IMessageChannel channel, int requestCount)
		{
			return (await channel.GetMessagesAsync(++requestCount).Flatten()).ToList();
		}

		public static async Task MakeAndDeleteSecondaryMessage(ICommandContext context, string secondStr, int time = Constants.WAIT_TIME)
		{
			await MakeAndDeleteSecondaryMessage(context.Channel, context.Message, secondStr, time);
		}
		
		public static async Task MakeAndDeleteSecondaryMessage(IMessageChannel channel, IUserMessage message, string secondStr, int time = Constants.WAIT_TIME)
		{
			var secondMsg = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + secondStr);
			var messages = new List<IMessage> { secondMsg, message };

			if (message == null)
			{
				RemoveCommandMessage(secondMsg, time);
			}
			else
			{
				RemoveCommandMessages(messages, time);
			}
		}

		public static async Task MakeAndDeleteSecondaryMessage(IMessageChannel channel, string secondStr, int time = Constants.WAIT_TIME)
		{
			await MakeAndDeleteSecondaryMessage(channel, null, secondStr, time);
		}

		public static async Task DeleteMessages(IMessageChannel channel, IEnumerable<IMessage> messages)
		{
			var guildChannel = channel as ITextChannel;
			if (guildChannel == null || messages == null || !messages.Any())
				return;

			try
			{
				await guildChannel.DeleteMessagesAsync(messages.Where(x => DateTime.UtcNow.Subtract(x.CreatedAt.UtcDateTime).TotalDays < 14).Distinct());
			}
			catch
			{
				WriteLine(String.Format("Unable to delete {0} messages on the guild {1} on channel {2}.", messages.Count(), guildChannel.Guild.FormatGuild(), guildChannel.FormatChannel()));
			}
		}

		public static async Task DeleteMessage(IMessage message)
		{
			var guildChannel = message.Channel as ITextChannel;
			if (guildChannel == null || message == null || DateTime.UtcNow.Subtract(message.CreatedAt.UtcDateTime).TotalDays >= 14)
				return;

			try
			{
				await message.DeleteAsync();
			}
			catch
			{
				WriteLine(String.Format("Unable to delete the message {0} on channel {1}.", message.Id, guildChannel.FormatChannel()));
			}
		}

		public static async Task SendDeleteMessage(IGuild guild, ITextChannel channel, List<string> inputList)
		{
			//Get the character count
			int characterCount = 0;
			inputList.ForEach(x => characterCount += (x.Length + 100));

			if (!inputList.Any())
			{
				return;
			}
			else if (inputList.Count <= 5 && characterCount < Constants.MAX_MESSAGE_LENGTH_LONG)
			{
				//If there aren't many messages send the small amount in a message instead of a file or link
				var embed = MakeNewEmbed("Deleted Messages", String.Join("\n", inputList), Constants.MDEL);
				AddFooter(embed, "Deleted Messages");
				await SendEmbedMessage(channel, embed);
			}
			else
			{
				await WriteAndUploadTextFile(guild, channel, ReplaceMarkdownChars(String.Join("\n-----\n", inputList), true), "Deleted_Messages_", String.Format("{0} Deleted Messages", inputList.Count));
			}
		}

		public static async Task SendGuildNotification(IUser user, GuildNotification notification)
		{
			if (notification == null)
				return;

			var content = notification.Content;
			content = content.CaseInsReplace("{UserMention}", user != null ? user.Mention : "Invalid User");
			content = content.CaseInsReplace("{User}", user != null ? user.FormatUser() : "Invalid User");
			//Put a zero length character in between invite links for names so the invite links will no longer embed

			if (notification.Embed != null)
			{
				await SendEmbedMessage(notification.Channel, notification.Embed, content);
			}
			else
			{
				await SendChannelMessage(notification.Channel, content);
			}
		}

		public static async Task HandleObjectGettingErrors<T>(ICommandContext context, ReturnedObject<T> returnedObject)
		{
			await MakeAndDeleteSecondaryMessage(context, FormatErrorString(context.Guild, returnedObject));
		}

		public static async Task HandleArgsGettingErrors(ICommandContext context, ReturnedArguments returnedArgs)
		{
			//TODO: Remove my own arg parsing.
			switch (returnedArgs.Reason)
			{
				case FailureReason.TooMany:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("Too many arguments."));
					return;
				}
				case FailureReason.TooFew:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("Too few arguments."));
					return;
				}
				/*
				case FailureReason.MissingCriticalArgs:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("Missing critical arguments."));
					return;
				}
				case FailureReason.MaxLessThanMin:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("NOT USER ERROR: Max less than min."));
					return;
				}*/
			}
		}

		public static async Task<Dictionary<IUser, IMessageChannel>> GetAllBotDMs()
		{
			var dict = new Dictionary<IUser, IMessageChannel>();
			foreach (var channel in await Variables.Client.GetDMChannelsAsync())
			{
				var recep = channel.Recipient;
				if (recep != null)
				{
					dict.Add(recep, channel);
				}
			}
			return dict;
		}

		public static async Task<List<IMessage>> GetBotDMs(IDMChannel channel)
		{
			return (await GetMessages(channel, 500)).OrderBy(x => x.CreatedAt).ToList();
		}

		public static EmbedBuilder MakeNewEmbed(string title = null, string description = null, Color? color = null, string imageURL = null, string URL = null, string thumbnailURL = null)
		{
			//Make the embed builder
			var embed = new EmbedBuilder().WithColor(Constants.BASE);

			//Validate the URLs
			imageURL = ValidateURL(imageURL) ? imageURL : null;
			URL = ValidateURL(URL) ? URL : null;
			thumbnailURL = ValidateURL(thumbnailURL) ? thumbnailURL : null;

			//Add in the properties
			if (title != null)
			{
				embed.WithTitle(title.Substring(0, Math.Min(Constants.MAX_TITLE_LENGTH, title.Length)));
			}
			if (description != null)
			{
				embed.WithDescription(description);
			}
			if (color != null)
			{
				embed.WithColor(color.Value);
			}
			if (imageURL != null)
			{
				embed.WithImageUrl(imageURL);
			}
			if (URL != null)
			{
				embed.WithUrl(URL);
			}
			if (thumbnailURL != null)
			{
				embed.WithThumbnailUrl(thumbnailURL);
			}

			return embed;
		}

		public static void AddAuthor(EmbedBuilder embed, string name = null, string iconURL = null, string URL = null)
		{
			//Create the author builder
			var author = new EmbedAuthorBuilder();

			//Verify the URLs
			iconURL = ValidateURL(iconURL) ? iconURL : null;
			URL = ValidateURL(URL) ? URL : null;

			//Add in the properties
			if (name != null)
			{
				author.WithName(name.Substring(0, Math.Min(Constants.MAX_TITLE_LENGTH, name.Length)));
			}
			if (iconURL != null)
			{
				author.WithIconUrl(iconURL);
			}
			if (URL != null)
			{
				author.WithUrl(URL);
			}

			embed.WithAuthor(author);
		}

		public static void AddFooter(EmbedBuilder embed, [CallerMemberName] string text = null, string iconURL = null)
		{
			//Make the footer builder
			var footer = new EmbedFooterBuilder();

			//Verify the URL
			iconURL = ValidateURL(iconURL) ? iconURL : null;

			//Add in the properties
			if (text != null)
			{
				footer.WithText(text.Substring(0, Math.Min(Constants.MAX_FOOTER_LENGTH, text.Length)));
			}
			if (iconURL != null)
			{
				footer.WithIconUrl(iconURL);
			}

			embed.WithFooter(footer);
		}

		public static void AddField(EmbedBuilder embed, string name, string value, bool isInline = true)
		{
			if (embed.Build().Fields.Count() >= Constants.MAX_FIELDS)
				return;

			//Get the name and value
			name = String.IsNullOrWhiteSpace(name) ? "Placeholder" : name.Substring(0, Math.Min(Constants.MAX_FIELD_NAME_LENGTH, name.Length));
			value = String.IsNullOrWhiteSpace(name) ? "Placeholder" : value.Substring(0, Math.Min(Constants.MAX_FIELD_VALUE_LENGTH, value.Length));

			embed.AddField(x =>
			{
				x.Name = name;
				x.Value = value;
				x.IsInline = isInline;
			});
		}

		public static EmbedBuilder FormatUserInfo(BotGuildInfo guildInfo, SocketGuild guild, SocketGuildUser user)
		{
			var guildUser = user as SocketGuildUser;
			var roles = guildUser.Roles.OrderBy(x => x.Position).Where(x => !x.IsEveryone);
			var channels = new List<string>();
			guild.TextChannels.OrderBy(x => x.Position).ToList().ForEach(x =>
			{
				if (guildUser.GetPermissions(x).ReadMessages)
				{
					channels.Add(x.Name);
				}
			});
			guild.VoiceChannels.OrderBy(x => x.Position).ToList().ForEach(x =>
			{
				if (guildUser.GetPermissions(x).Connect)
				{
					channels.Add(x.Name + " (Voice)");
				}
			});
			var users = guild.Users.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();
			var created = guildUser.CreatedAt.UtcDateTime;
			var joined = guildUser.JoinedAt.Value.UtcDateTime;

			var IDstr = String.Format("**ID:** `{0}`", guildUser.Id);
			var nicknameStr = String.Format("**Nickname:** `{0}`", String.IsNullOrWhiteSpace(guildUser.Nickname) ? "NO NICKNAME" : EscapeMarkdown(guildUser.Nickname, true));
			var createdStr = String.Format("\n**Created:** `{0}`", FormatDateTime(guildUser.CreatedAt.UtcDateTime));
			var joinedStr = String.Format("**Joined:** `{0}` (`{1}` to join the guild)\n", FormatDateTime(guildUser.JoinedAt.Value.UtcDateTime), users.IndexOf(guildUser) + 1);
			var gameStr = FormatGameStr(guildUser);
			var statusStr = String.Format("**Online status:** `{0}`", guildUser.Status);
			var description = String.Join("\n", new[] { IDstr, nicknameStr, createdStr, joinedStr, gameStr, statusStr });

			var color = roles.OrderBy(x => x.Position).LastOrDefault(x => x.Color.RawValue != 0)?.Color;
			var embed = MakeNewEmbed(null, description, color, thumbnailURL: user.GetAvatarUrl());
			if (channels.Count() != 0)
			{
				AddField(embed, "Channels", String.Join(", ", channels));
			}
			if (roles.Count() != 0)
			{
				AddField(embed, "Roles", String.Join(", ", roles.Select(x => x.Name)));
			}
			if (user.VoiceChannel != null)
			{
				var desc = String.Format("Server mute: `{0}`\nServer deafen: `{1}`\nSelf mute: `{2}`\nSelf deafen: `{3}`", user.IsMuted, user.IsDeafened, user.IsSelfMuted, user.IsSelfDeafened);
				AddField(embed, "Voice Channel: " + user.VoiceChannel.Name, desc);
			}
			AddAuthor(embed, guildUser.FormatUser(), guildUser.GetAvatarUrl(), guildUser.GetAvatarUrl());
			AddFooter(embed, "User Info");
			return embed;
		}

		public static EmbedBuilder FormatUserInfo(BotGuildInfo guildInfo, SocketGuild guild, SocketUser user)
		{
			var ageStr		= String.Format("**Created:** `{0}`\n", FormatDateTime(user.CreatedAt.UtcDateTime));
			var gameStr		= FormatGameStr(user);
			var statusStr	= String.Format("**Online status:** `{0}`", user.Status);
			var description	= String.Join("\n", new[] { ageStr, gameStr, statusStr });

			var embed = MakeNewEmbed(null, description, null, thumbnailURL: user.GetAvatarUrl());
			AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl(), user.GetAvatarUrl());
			AddFooter(embed, "User Info");
			return embed;
		}

		public static EmbedBuilder FormatRoleInfo(BotGuildInfo guildInfo, SocketGuild guild, SocketRole role)
		{
			var ageStr		= String.Format("**Created:** `{0}` (`{1}` days ago)", FormatDateTime(role.CreatedAt.UtcDateTime), DateTime.UtcNow.Subtract(role.CreatedAt.UtcDateTime).Days);
			var positionStr	= String.Format("**Position:** `{0}`", role.Position);
			var usersStr	= String.Format("**User Count:** `{0}`", guild.Users.Where(x => x.Roles.Any(y => y.Id == role.Id)).Count());
			var description	= String.Join("\n", new[] { ageStr, positionStr, usersStr });

			var color = role.Color;
			var embed = MakeNewEmbed(null, description, color);
			AddAuthor(embed, role.FormatRole());
			AddFooter(embed, "Role Info");
			return embed;
		}

		public static EmbedBuilder FormatChannelInfo(BotGuildInfo guildInfo, SocketGuild guild, SocketChannel channel)
		{
			var ignoredFromLog		= ((List<ulong>)guildInfo.GetSetting(SettingOnGuild.IgnoredLogChannels)).Contains(channel.Id);
			var ignoredFromCmd		= ((List<ulong>)guildInfo.GetSetting(SettingOnGuild.IgnoredCommandChannels)).Contains(channel.Id);
			var imageOnly			= ((List<ulong>)guildInfo.GetSetting(SettingOnGuild.ImageOnlyChannels)).Contains(channel.Id);
			var sanitary			= ((List<ulong>)guildInfo.GetSetting(SettingOnGuild.SanitaryChannels)).Contains(channel.Id);
			var slowmode			= ((List<SlowmodeChannel>)guildInfo.GetSetting(SettingOnGuild.SlowmodeChannels)).Any(x => x.ChannelID == channel.Id);
			var serverLog			= ((DiscordObjectWithID<ITextChannel>)guildInfo.GetSetting(SettingOnGuild.ServerLog)).ID == channel.Id;
			var modLog				= ((DiscordObjectWithID<ITextChannel>)guildInfo.GetSetting(SettingOnGuild.ModLog)).ID == channel.Id;
			var imageLog			= ((DiscordObjectWithID<ITextChannel>)guildInfo.GetSetting(SettingOnGuild.ImageLog)).ID == channel.Id;

			var ageStr				= String.Format("**Created:** `{0}` (`{1}` days ago)", FormatDateTime(channel.CreatedAt.UtcDateTime), DateTime.UtcNow.Subtract(channel.CreatedAt.UtcDateTime).Days);
			var userCountStr		= String.Format("**User Count:** `{0}`", channel.Users.Count);
			var ignoredFromLogStr	= String.Format("\n**Ignored From Log:** `{0}`", ignoredFromLog ? "Yes" : "No");
			var ignoredFromCmdStr	= String.Format("**Ignored From Commands:** `{0}`", ignoredFromCmd ? "Yes" : "No");
			var imageOnlyStr		= String.Format("**Image Only:** `{0}`", imageOnly ? "Yes" : "No");
			var sanitaryStr			= String.Format("**Sanitary:** `{0}`", sanitary ? "Yes" : "No");
			var slowmodeStr			= String.Format("**Slowmode:** `{0}`", slowmode ? "Yes" : "No");
			var serverLogStr		= String.Format("\n**Serverlog:** `{0}`", serverLog ? "Yes" : "No");
			var modLogStr			= String.Format("**Modlog:** `{0}`", modLog ? "Yes" : "No");
			var imageLogStr			= String.Format("**Imagelog:** `{0}`", imageLog ? "Yes" : "No");
			var description			= String.Join("\n", new[] { ageStr, userCountStr, ignoredFromLogStr, ignoredFromCmdStr, imageOnlyStr, sanitaryStr, slowmodeStr, serverLogStr, modLogStr, imageLogStr });

			var embed = MakeNewEmbed(null, description);
			AddAuthor(embed, channel.FormatChannel());
			AddFooter(embed, "Channel Info");
			return embed;
		}

		public static EmbedBuilder FormatGuildInfo(BotGuildInfo guildInfo, SocketGuild guild)
		{
			var owner			= guild.Owner;
			var onlineCount		= guild.Users.Where(x => x.Status != UserStatus.Offline).Count();
			var nicknameCount	= guild.Users.Where(x => x.Nickname != null).Count();
			var gameCount		= guild.Users.Where(x => x.Game.HasValue).Count();
			var botCount		= guild.Users.Where(x => x.IsBot).Count();
			var voiceCount		= guild.Users.Where(x => x.VoiceChannel != null).Count();
			var localECount		= guild.Emotes.Where(x => !x.IsManaged).Count();
			var globalECount	= guild.Emotes.Where(x => x.IsManaged).Count();

			var ageStr			= String.Format("**Created:** `{0}` (`{1}` days ago)", FormatDateTime(guild.CreatedAt.UtcDateTime), DateTime.UtcNow.Subtract(guild.CreatedAt.UtcDateTime).Days);
			var ownerStr		= String.Format("**Owner:** `{0}`", owner.FormatUser());
			var regionStr		= String.Format("**Region:** `{0}`", guild.VoiceRegionId);
			var emoteStr		= String.Format("**Emotes:** `{0}` (`{1}` local, `{2}` global)\n", localECount + globalECount, localECount, globalECount);
			var userStr			= String.Format("**User Count:** `{0}` (`{1}` online, `{2}` bots)", guild.MemberCount, onlineCount, botCount);
			var nickStr			= String.Format("**Users With Nickname:** `{0}`", nicknameCount);
			var gameStr			= String.Format("**Users Playing Games:** `{0}`", gameCount);
			var voiceStr		= String.Format("**Users In Voice:** `{0}`\n", voiceCount);
			var roleStr			= String.Format("**Role Count:** `{0}`", guild.Roles.Count);
			var channelStr		= String.Format("**Channel Count:** `{0}` (`{1}` text, `{2}` voice)", guild.Channels.Count, guild.TextChannels.Count, guild.VoiceChannels.Count);
			var afkChanStr		= String.Format("**AFK Channel:** `{0}` (`{1}` minute{2})", guild.AFKChannel.FormatChannel(), guild.AFKTimeout / 60, GetPlural(guild.AFKTimeout / 60));
			var description		= String.Join("\n", new List<string>() { ageStr, ownerStr, regionStr, emoteStr, userStr, nickStr, gameStr, voiceStr, roleStr, channelStr, afkChanStr });

			var color = owner.Roles.FirstOrDefault(x => x.Color.RawValue != 0)?.Color;
			var embed = MakeNewEmbed(null, description, color, thumbnailURL: guild.IconUrl);
			AddAuthor(embed, guild.FormatGuild());
			AddFooter(embed, "Guild Info");
			return embed;
		}

		public static EmbedBuilder FormatEmoteInfo(BotGuildInfo guildInfo, DiscordSocketClient client, Emote emote)
		{
			//Try to find the emoji if global
			var guilds = client.Guilds.Where(x => x.Emotes.Any(y => y.Id == emote.Id && y.IsManaged && y.RequireColons));

			var description = String.Format("**ID:** `{0}`\n", emote.Id);
			if (guilds.Any())
			{
				description += String.Format("**From:** `{0}`", String.Join("`, `", guilds.Select(x => x.FormatGuild())));
			}

			var embed = MakeNewEmbed(null, description, thumbnailURL: emote.Url);
			AddAuthor(embed, emote.Name);
			AddFooter(embed, "Emoji Info");
			return embed;
		}

		public static EmbedBuilder FormatInviteInfo(BotGuildInfo guildInfo, SocketGuild guild, IInviteMetadata invite)
		{
			var inviterStr = String.Format("**Inviter:** `{0}`", invite.Inviter.FormatUser());
			var channelStr = String.Format("**Channel:** `{0}`", guild.Channels.FirstOrDefault(x => x.Id == invite.ChannelId).FormatChannel());
			var usesStr = String.Format("**Uses:** `{0}`", invite.Uses);
			var createdStr = String.Format("**Created At:** `{0}`", FormatDateTime(invite.CreatedAt.UtcDateTime));
			var description = String.Join("\n", new[] { inviterStr, channelStr, usesStr, createdStr });

			var embed = MakeNewEmbed(null, description);
			AddAuthor(embed, invite.Code);
			AddFooter(embed, "Emote Info");
			return embed;
		}

		public static EmbedBuilder FormatBotInfo(SocketGuild guild)
		{
			var online = String.Format("**Online Since:** `{0}`", FormatDateTime(Variables.StartupTime));
			var uptime = String.Format("**Uptime:** `{0}`", GetUptime());
			var guildCount = String.Format("**Guild Count:** `{0}`", Variables.TotalGuilds);
			var memberCount = String.Format("**Cumulative Member Count:** `{0}`", Variables.TotalUsers);
			var currShard = String.Format("**Current Shard:** `{0}`", Variables.Client.GetShardFor(guild).ShardId);
			var description = String.Join("\n", new[] { online, uptime, guildCount, memberCount, currShard });

			var embed = MakeNewEmbed(null, description);
			AddAuthor(embed, Variables.BotName, Variables.Client.GetCurrentUser().GetAvatarUrl());
			AddFooter(embed, "Version " + Constants.BOT_VERSION);

			var firstField = FormatLoggedThings();
			AddField(embed, "Logged Actions", firstField);

			var attempt = String.Format("**Attempted Commands:** `{0}`", Variables.AttemptedCommands);
			var successful = String.Format("**Successful Commands:** `{0}`", Variables.AttemptedCommands - Variables.FailedCommands);
			var failed = String.Format("**Failed Commands:** `{0}`", Variables.FailedCommands);
			var secondField = String.Join("\n", new[] { attempt, successful, failed });
			AddField(embed, "Commands", secondField);

			var latency = String.Format("**Latency:** `{0}ms`", Variables.Client.GetLatency());
			var memory = String.Format("**Memory Usage:** `{0}MB`", GetMemory().ToString("0.00"));
			var threads = String.Format("**Thread Count:** `{0}`", System.Diagnostics.Process.GetCurrentProcess().Threads.Count);
			var thirdField = String.Join("\n", new[] { latency, memory, threads });
			AddField(embed, "Technical", thirdField);

			return embed;
		}

		public static List<string> FormatMessages(IEnumerable<IMessage> list)
		{
			return list.Select(x => FormatMessage(x)).ToList();
		}

		public static List<string> FormatDMs(IEnumerable<IMessage> list)
		{
			return list.Select(x => FormatDM(x)).ToList();
		}

		public static string FormatErrorString<T>(IGuild guild, ReturnedObject<T> returnedObject)
		{
			var objType = returnedObject.Object == null ? GetObjectStringBasic(typeof(T)) : GetObjectStringBasic((dynamic)returnedObject.Object);
			switch (returnedObject.Reason)
			{
				case FailureReason.TooFew:
				{
					return String.Format("Unable to find the {0}.", objType);
				}
				case FailureReason.UserInability:
				{
					return String.Format("You are unable to make the given changes to the {0}: `{1}`.", objType, FormatObject((dynamic)returnedObject.Object));
				}
				case FailureReason.BotInability:
				{
					return String.Format("I am unable to make the given changes to the {0}: `{1}`.", objType, FormatObject((dynamic)returnedObject.Object));
				}
				case FailureReason.TooMany:
				{
					return String.Format("There are too many {0}s with the same name.", objType);
				}
				case FailureReason.ChannelType:
				{
					return "Invalid channel type for the given variable requirement.";
				}
				case FailureReason.DefaultChannel:
				{
					return "The default channel cannot be deleted.";
				}
				case FailureReason.EveryoneRole:
				{
					return "The everyone role cannot be modified in that way.";
				}
				case FailureReason.ManagedRole:
				{
					return "Managed roles cannot be modified in that way.";
				}
				case FailureReason.InvalidEnum:
				{
					return String.Format("The type `{0}` is not accepted in this instance.", returnedObject.Object);
				}
				default:
				{
					return "This shouldn't be seen. - Advobot";
				}
			}
		}

		public static string FormatMessage(IMessage msg)
		{
			var content = String.IsNullOrEmpty(msg.Content) ? "EMPTY MESSAGE CONTENT" : msg.Content;
			if (msg.Embeds.Any())
			{
				var descriptions = msg.Embeds.Where(x =>
				{
					return false
					|| x.Description != null
					|| x.Url != null
					|| x.Image.HasValue;
				}).Select(x =>
				{
					if (x.Url != null)
					{
						return String.Format("{0} URL: {1}", x.Description, x.Url);
					}
					if (x.Image.HasValue)
					{
						return String.Format("{0} IURL: {1}", x.Description, x.Image.Value.Url);
					}
					else
					{
						return x.Description;
					}
				}).ToArray();

				var formattedDescriptions = "";
				for (int i = 0; i < descriptions.Count(); i++)
				{
					formattedDescriptions += String.Format("Embed {0}: {1}", i + 1, descriptions[i]);
				}

				content = String.Format("{0}\n{1}", content, formattedDescriptions);
			}
			if (msg.Attachments.Any())
			{
				content = String.Format("{0} + {1}", content, String.Join(" + ", msg.Attachments.Select(y => y.Filename)));
			}

			return FormatMessage(msg, content);
		}

		public static string FormatMessage(IMessage message, string text)
		{
			return String.Format("`[{2}]` `{0}` **IN** `{1}`\n```\n{3}```",
					message.Author.FormatUser(),
					message.Channel.FormatChannel(),
					message.CreatedAt.ToString("HH:mm:ss"),
					ReplaceMarkdownChars(text, true));
		}

		public static string FormatDM(IMessage msg)
		{
			var content = String.IsNullOrEmpty(msg.Content) ? "EMPTY MESSAGE CONTENT" : msg.Content;
			if (msg.Embeds.Any())
			{
				var descriptions = msg.Embeds.Where(x =>
				{
					return false
					|| x.Description != null
					|| x.Url != null
					|| x.Image.HasValue;
				}).Select(x =>
				{
					if (x.Url != null)
					{
						return String.Format("{0} URL: {1}", x.Description, x.Url);
					}
					if (x.Image.HasValue)
					{
						return String.Format("{0} IURL: {1}", x.Description, x.Image.Value.Url);
					}
					else
					{
						return x.Description;
					}
				}).ToArray();

				var formattedDescriptions = "";
				for (int i = 0; i < descriptions.Count(); i++)
				{
					formattedDescriptions += String.Format("Embed {0}: {1}", i + 1, descriptions[i]);
				}

				return String.Format("`{0}` **SENT AT** `[{1}]`\n```\n{2}```",
					msg.Author.FormatUser(),
					FormatDateTime(msg.CreatedAt),
					ReplaceMarkdownChars(content + "\n" + formattedDescriptions, true));
			}
			else if (msg.Attachments.Any())
			{
				return String.Format("`{0}` **SENT AT** `[{1}]`\n```\n{2}```",
					msg.Author.FormatUser(),
					FormatDateTime(msg.CreatedAt),
					ReplaceMarkdownChars(content + " + " + String.Join(" + ", msg.Attachments.Select(y => y.Filename)), true));
			}
			else
			{
				return String.Format("`{0}` **SENT AT** `[{1}]`\n```\n{2}```",
					msg.Author.FormatUser(),
					FormatDateTime(msg.CreatedAt),
					ReplaceMarkdownChars(content, true));
			}
		}

		public static string FormatDateTime(DateTime? dt)
		{
			if (!dt.HasValue)
			{
				return "N/A";
			}

			var ndt = dt.Value.ToUniversalTime();
			return String.Format("{0} {1}, {2} at {3}",
				System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(ndt.Month),
				ndt.Day,
				ndt.Year,
				ndt.ToLongTimeString());
		}

		public static string FormatDateTime(DateTimeOffset? dt)
		{
			if (!dt.HasValue)
			{
				return "N/A";
			}

			return FormatDateTime(dt.Value.UtcDateTime);
		}

		public static string FormatGameStr(IUser user)
		{
			if (user.Game.HasValue)
			{
				var game = user.Game.Value;
				if (game.StreamType == StreamType.Twitch)
				{
					return String.Format("**Current Stream:** [{0}]({1})", EscapeMarkdown(game.Name, true), game.StreamUrl);
				}
				else
				{
					return String.Format("**Current Game:** `{0}`", EscapeMarkdown(game.Name, true));
				}
			}
			return "**Current Game:** `N/A`";
		}

		public static string ERROR(string message, [CallerMemberName] string name = "")
		{
			++Variables.FailedCommands;

			return Constants.ZERO_LENGTH_CHAR + Constants.ERROR_MESSAGE + message;
		}
		
		public static string ReplaceMarkdownChars(string input, bool replaceNewLines)
		{
			if (String.IsNullOrWhiteSpace(input))
				return "";

			input = new Regex("[*`]", RegexOptions.Compiled).Replace(input, "");

			while (replaceNewLines)
			{
				if (input.Contains("\n\n"))
				{
					input = input.Replace("\n\n", "\n");
				}
				else
				{
					break;
				}
			}

			return input;
		}

		public static string FormatObject(IUser user)
		{
			return user.FormatUser();
		}

		public static string FormatObject(IGuildChannel channel)
		{
			return channel.FormatChannel();
		}

		public static string FormatObject(IRole role)
		{
			return role.FormatRole();
		}

		public static string FormatObject(IGuild guild)
		{
			return guild.FormatGuild();
		}

		public static string FormatObject(SocketGuild guild)
		{
			return guild.FormatGuild();
		}

		public static string FormatObject(string str)
		{
			return str;
		}

		public static string FormatObject(object obj)
		{
			return "FormatObject Error";
		}

		public static string RemoveNewLines(string input)
		{
			return input.Replace(Environment.NewLine, "").Replace("\r", "").Replace("\n", "");
		}

		public static string FormatLoggedThings()
		{
			var j = Variables.LoggedJoins;
			var l = Variables.LoggedLeaves;
			var u = Variables.LoggedUserChanges;
			var e = Variables.LoggedEdits;
			var d = Variables.LoggedDeletes;
			var i = Variables.LoggedImages;
			var g = Variables.LoggedGifs;
			var f = Variables.LoggedFiles;
			var leftSpacing = new[] { j, l, u, e, d, i, g, f }.Max().ToString().Length;

			const string jTitle = "**Joins:**";
			const string lTitle = "**Leaves:**";
			const string uTitle = "**User Changes:**";
			const string eTitle = "**Edits:**";
			const string dTitle = "**Deletes:**";
			const string iTitle = "**Images:**";
			const string gTitle = "**Gifs:**";
			const string fTitle = "**Files:**";
			var rightSpacing = new[] { jTitle, lTitle, uTitle, eTitle, dTitle, iTitle, gTitle, fTitle }.Max(x => x.Length) + 1;

			var joins = FormatStringsWithLength(jTitle, j, rightSpacing, leftSpacing);
			var leaves = FormatStringsWithLength(lTitle, l, rightSpacing, leftSpacing);
			var userChanges = FormatStringsWithLength(uTitle, u, rightSpacing, leftSpacing);
			var edits = FormatStringsWithLength(eTitle, e, rightSpacing, leftSpacing);
			var deletes = FormatStringsWithLength(dTitle, d, rightSpacing, leftSpacing);
			var images = FormatStringsWithLength(iTitle, i, rightSpacing, leftSpacing);
			var gifs = FormatStringsWithLength(gTitle, g, rightSpacing, leftSpacing);
			var files = FormatStringsWithLength(fTitle, f, rightSpacing, leftSpacing);
			return String.Join("\n", new[] { joins, leaves, userChanges, edits, deletes, images, gifs, files });
		}

		public static string FormatLoggedCommands()
		{
			var a = Variables.AttemptedCommands;
			var s = Variables.AttemptedCommands - Variables.FailedCommands;
			var f = Variables.FailedCommands;
			var maxNumLen = new[] { a, s, f }.Max().ToString().Length;

			var aStr = "**Attempted:**";
			var sStr = "**Successful:**";
			var fStr = "**Failed:**";
			var maxStrLen = new[] { aStr, sStr, fStr }.Max(x => x.Length);

			var leftSpacing = maxNumLen;
			var rightSpacing = maxStrLen + 1;

			var attempted = FormatStringsWithLength(aStr, a, rightSpacing, leftSpacing);
			var successful = FormatStringsWithLength(sStr, s, rightSpacing, leftSpacing);
			var failed = FormatStringsWithLength(fStr, f, rightSpacing, leftSpacing);
			return String.Join("\n", new[] { attempted, successful, failed });
		}

		public static string FormatStringsWithLength(object obj1, object obj2, int len)
		{
			var str1 = obj1.ToString();
			var str2 = obj2.ToString();
			return String.Format("{0}{1}", str1.PadRight(len - str2.Length), str2);
		}

		public static string FormatStringsWithLength(object obj1, object obj2, int right, int left)
		{
			var str1 = obj1.ToString().PadRight(right);
			var str2 = obj2.ToString().PadLeft(left);
			return String.Format("{0}{1}", str1, str2);
		}

		public static string FormatResponseMessagesForCmdsOnLotsOfObjects<T>(IEnumerable<T> success, IEnumerable<string> failure, string objType, string successAction, string failureAction)
		{
			var succOutput = "";
			if (success.Any())
			{
				var c = success.Count();
				succOutput = String.Format("Successfully {0} `{1}` {2}{3}: `{4}`. ",
					successAction,
					c,
					objType,
					GetPlural(c),
					String.Join("`, `", success.Select(x => FormatObject((dynamic)x))));
			}
			var failOutput = "";
			if (failure.Any())
			{
				var c = failure.Count();
				failOutput = String.Format("Failed to {0} `{1}` {2}{3}: `{4}`.",
					failureAction,
					c,
					objType,
					GetPlural(c),
					String.Join("`, `", failure));
			}
			return succOutput + failOutput;
		}

		public static string FormatAttribute(PermissionRequirementAttribute attr)
		{
			var basePerm = "N/A";
			if (attr != null)
			{
				var all = !String.IsNullOrWhiteSpace(attr.AllText);
				var any = !String.IsNullOrWhiteSpace(attr.AnyText);

				basePerm = "[";
				if (all)
				{
					basePerm += attr.AllText;
				}
				if (any)
				{
					if (all)
					{
						basePerm += " | ";
					}
					basePerm += attr.AnyText;
				}
				basePerm += "]";
			}
			return basePerm;
		}

		public static string FormatAttribute(OtherRequirementAttribute attr)
		{
			var basePerm = "N/A";
			if (attr != null)
			{
				var perms = (attr.Requirements & Precondition.UserHasAPerm) != 0;
				var guild = (attr.Requirements & Precondition.GuildOwner) != 0;
				var trust = (attr.Requirements & Precondition.TrustedUser) != 0;
				var owner = (attr.Requirements & Precondition.BotOwner) != 0;

				var text = new List<string>();
				if (perms)
				{
					text.Add("Administrator | Any perm ending with 'Members' | Any perm starting with 'Manage'");
				}
				if (guild)
				{
					text.Add("Guild Owner");
				}
				if (trust)
				{
					text.Add("Trusted User");
				}
				if (owner)
				{
					text.Add("Bot Owner");
				}
				basePerm = String.Format("[{0}]", String.Join(" | ", text));
			}
			return basePerm;
		}

		public static string FormatAllSettings(BotGlobalInfo globalInfo)
		{
			var str = "";
			foreach (var e in Enum.GetValues(typeof(SettingOnBot)).Cast<SettingOnBot>())
			{
				var formatted = FormatSettingInfo(globalInfo, e);
				if (!String.IsNullOrWhiteSpace(formatted))
				{
					str += String.Format("**{0}**:\n{1}\n\n", e.EnumName(), formatted);
				}
			}
			return str;
		}

		public static string FormatSettingInfo(BotGlobalInfo globalInfo, SettingOnBot setting)
		{
			var field = BotGlobalInfo.GetField(setting);
			if (field != null)
			{
				var notSaved = (JsonIgnoreAttribute)field.GetCustomAttribute(typeof(JsonIgnoreAttribute));
				if (notSaved != null)
				{
					return null;
				}

				var set = globalInfo.GetSetting(field);
				if (set != null)
				{
					return FormatSettingInfo(set);
				}
			}
			return null;
		}

		public static string FormatSettingInfo(System.Collections.IEnumerable setting)
		{
			return String.Join("\n", setting.Cast<dynamic>().Select(x => FormatSettingInfo(x)));
		}

		public static string FormatSettingInfo(ulong setting)
		{
			var user = GetGlobalUser(setting);
			if (user != null)
			{
				return user.FormatUser();
			}

			var guild = GetGuild(setting);
			if (guild != null)
			{
				return guild.FormatGuild();
			}

			return null;
		}

		public static string FormatSettingInfo(object setting)
		{
			return String.Format("`{0}`", setting.ToString());
		}

		public static string FormatAllSettings(SocketGuild guild, BotGuildInfo guildInfo)
		{
			var str = "";
			foreach (var e in Enum.GetValues(typeof(SettingOnGuild)).Cast<SettingOnGuild>())
			{
				var formatted = FormatSettingInfo(guild, guildInfo, e);
				if (!String.IsNullOrWhiteSpace(formatted))
				{
					str += String.Format("**{0}**:\n{1}\n\n", e.EnumName(), formatted);
				}
			}
			return str;
		}

		public static string FormatSettingInfo(SocketGuild guild, BotGuildInfo guildInfo, SettingOnGuild setting)
		{
			var field = BotGuildInfo.GetField(setting);
			if (field != null)
			{
				var notSaved = (JsonIgnoreAttribute)field.GetCustomAttribute(typeof(JsonIgnoreAttribute));
				if (notSaved != null)
				{
					return null;
				}

				var set = guildInfo.GetSetting(field);
				if (set != null)
				{
					return FormatSettingInfo(guild, set);
				}
			}
			return null;
		}

		public static string FormatSettingInfo(SocketGuild guild, Setting setting)
		{
			return setting.SettingToString();
		}

		public static string FormatSettingInfo(SocketGuild guild, System.Collections.IEnumerable setting)
		{
			return String.Join("\n", setting.Cast<dynamic>().Select(x => FormatSettingInfo(guild, x)));
		}

		public static string FormatSettingInfo(SocketGuild guild, ulong setting)
		{
			var chan = guild.GetChannel(setting);
			if (chan != null)
			{
				return chan.FormatChannel();
			}

			var role = guild.GetRole(setting);
			if (role != null)
			{
				return role.FormatRole();
			}

			var user = guild.GetUser(setting);
			if (user != null)
			{
				return user.FormatUser();
			}

			return null;
		}

		public static string FormatSettingInfo(SocketGuild guild, object setting)
		{
			return String.Format("`{0}`", setting.ToString());
		}

		public static string EscapeMarkdown(string str, bool onlyAccentGrave)
		{
			str = str.Replace("`", "\\`");
			return onlyAccentGrave ? str : str.Replace("*", "\\*").Replace("_", "\\_");
		}

		public static string FormatUserReason(IUser user, string reason)
		{
			if (!String.IsNullOrWhiteSpace(reason))
			{
				reason = String.Format("Action by {0}. Reason is {1}.", user.FormatUser(), reason.TrimEnd('.'));
				reason = reason.Substring(0, Math.Min(reason.Length, Constants.MAX_LENGTH_FOR_REASON));
			}
			else
			{
				reason = String.Format("Action by {0}.", user.FormatUser());
			}

			return reason;
		}

		public static string FormatBotReason(string reason)
		{
			if (!String.IsNullOrWhiteSpace(reason))
			{
				reason = String.Format("Automated action. User triggered {0}.", reason.TrimEnd('.'));
				reason = reason.Substring(0, Math.Min(reason.Length, Constants.MAX_LENGTH_FOR_REASON));
			}
			else
			{
				reason = "Automated action. User triggered something.";
			}

			return reason;
		}

		public static string JoinNonNullStrings(string joining, params string[] toJoin)
		{
			return String.Join(joining, toJoin.Where(x => !String.IsNullOrWhiteSpace(x)));
		}

		public static void WriteLine(string text, [CallerMemberName] string name = "")
		{
			var line = String.Format("[{0}] [{1}]: {2}", DateTime.Now.ToString("HH:mm:ss"), name, ReplaceMarkdownChars(text, true));

			if (!Variables.Console)
			{
				if (!Variables.WrittenLines.TryGetValue(name, out List<string> list))
				{
					Variables.WrittenLines.Add(name, list = new List<string>());
				}
				list.Add(line);
			}

			Console.WriteLine(line);
		}

		public static void ExceptionToConsole(Exception e, [CallerMemberName] string name = "")
		{
			if (e == null)
				return;

			WriteLine("EXCEPTION: " + e, name);
		}

		public static void WriteLoadDone(IGuild guild, string method, string name)
		{
			WriteLine(String.Format("{0}: {1} for the guild {2} have been loaded.", method, name, guild.FormatGuild()));
		}
		#endregion

		#region Invites
		public static async Task<IReadOnlyCollection<IInviteMetadata>> GetInvites(IGuild guild)
		{
			if (guild == null)
				return new List<IInviteMetadata>();

			var currUser = await guild.GetCurrentUserAsync();
			if (!currUser.GuildPermissions.ManageGuild)
				return new List<IInviteMetadata>();

			return await guild.GetInvitesAsync();
		}

		public static async Task<BotInvite> GetInviteUserJoinedOn(BotGuildInfo guildInfo, IGuild guild)
		{
			var curInvs = await GetInvites(guild);
			if (!curInvs.Any())
				return null;
			var botInvs = ((List<BotInvite>)guildInfo.GetSetting(SettingOnGuild.Invites));

			//Find the first invite where the bot invite has the same code as the current invite but different use counts
			var joinInv = botInvs.FirstOrDefault(bI => curInvs.Any(cI => cI.Code == bI.Code && cI.Uses != bI.Uses));
			//If the invite is null, take that as meaning there are new invites on the guild
			if (joinInv == null)
			{
				//Get the new invites on the guild by finding which guild invites aren't on the bot invites list
				var newInvs = curInvs.Where(cI => !botInvs.Select(bI => bI.Code).Contains(cI.Code));
				//If there's only one, then use that as the current inv. If there's more than one then there's no way to know what invite it was on
				if (guild.Features.CaseInsContains(Constants.VANITY_URL) && (!newInvs.Any() || newInvs.All(x => x.Uses == 0)))
				{
					joinInv = new BotInvite(guild.Id, "Vanity URL", 0);
				}
				else if (newInvs.Count() == 1)
				{
					var newInv = newInvs.First();
					joinInv = new BotInvite(newInv.GuildId, newInv.Code, newInv.Uses);
				}
				botInvs.AddRange(newInvs.Select(x => new BotInvite(x.GuildId, x.Code, x.Uses)));
			}
			else
			{
				//Increment the invite the bot is holding if a curInv was found so as to match with the current invite uses count
				joinInv.IncreaseUses();
			}
			return joinInv;
		}

		public static List<ListedInvite> GetMatchingInvites(List<ListedInvite> curMatches, List<ListedInvite> matches, bool inBool, out bool outBool)
		{
			outBool = inBool;
			if (!outBool)
			{
				curMatches = curMatches.Intersect(matches).ToList();
			}
			else
			{
				curMatches.AddRange(matches);
				outBool = false;
			}
			return curMatches;
		}
		#endregion

		#region Uploads
		public static async Task<IMessage> WriteAndUploadTextFile(IGuild guild, IMessageChannel channel, string text, string fileName, string content = null)
		{
			//Get the file path
			if (!fileName.EndsWith("_"))
			{
				fileName += "_";
			}

			var file = fileName + DateTime.UtcNow.ToString("MM-dd_HH-mm-ss") + Constants.GENERAL_FILE_EXTENSION;
			var path = GetServerFilePath(guild.Id, file);
			if (path == null)
				return null;

			using (var writer = new StreamWriter(path))
			{
				writer.WriteLine(ReplaceMarkdownChars(text, false));
			}

			var textOnTop = String.IsNullOrWhiteSpace(content) ? "" : String.Format("**{0}:**", content);
			var msg = await channel.SendFileAsync(path, textOnTop);
			File.Delete(path);
			return msg;
		}

		public static async Task UploadFile(IMessageChannel channel, string path, string text = null)
		{
			await channel.SendFileAsync(path, text);
		}

		public static async Task SetBotIcon(SocketCommandContext context, string imageURL)
		{
			if (imageURL == null)
			{
				await context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image());
				await MakeAndDeleteSecondaryMessage(context, "Successfully removed the bot's icon.");
				return;
			}

			var fileType = await GetFileTypeOrSayErrors(context, imageURL);
			if (fileType == null)
				return;

			var path = GetServerFilePath(context.Guild.Id, Constants.BOT_ICON_LOCATION + fileType);
			using (var webclient = new WebClient())
			{
				webclient.DownloadFileAsync(new Uri(imageURL), path);
				webclient.DownloadFileCompleted += (sender, e) => SetIcon(sender, e, context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(path)), context, path);
			}
		}

		public static async Task<string> GetFileTypeOrSayErrors(SocketCommandContext context, string imageURL)
		{
			string fileType;
			var req = WebRequest.Create(imageURL);
			req.Method = WebRequestMethods.Http.Head;
			using (var resp = req.GetResponse())
			{
				if (!Constants.VALID_IMAGE_EXTENSIONS.Contains(fileType = "." + resp.Headers.Get("Content-Type").Split('/').Last()))
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("Image must be a png or jpg."));
					return null;
				}
				else if (!int.TryParse(resp.Headers.Get("Content-Length"), out int ContentLength))
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("Unable to get the image's file size."));
					return null;
				}
				else if (ContentLength > Constants.MAX_ICON_FILE_SIZE)
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("Image is bigger than {0:0.0}MB. Manually upload instead.", (double)Constants.MAX_ICON_FILE_SIZE / 1000000)));
					return null;
				}
			}
			return fileType;
		}

		public static void SetIcon(object sender, System.ComponentModel.AsyncCompletedEventArgs e, Task iconSetter, SocketCommandContext context, string path)
		{
			iconSetter.ContinueWith(async prevTask =>
			{
				if (prevTask?.Exception?.InnerExceptions?.Any() ?? false)
				{
					var exceptionMessages = new List<string>();
					foreach (var exception in prevTask.Exception.InnerExceptions)
					{
						ExceptionToConsole(exception);
						exceptionMessages.Add(exception.Message);
					}
					await SendChannelMessage(context, String.Format("Failed to change the bot icon. Following exceptions occurred:\n{0}.", String.Join("\n", exceptionMessages)));
				}
				else
				{
					await MakeAndDeleteSecondaryMessage(context, "Successfully changed the bot icon.");
				}

				File.Delete(path);
			});
		}

		public static bool ValidateURL(string input)
		{
			if (input == null)
				return false;

			return Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
		}
		#endregion

		#region Server/Mod Log
		public static async Task<ITextChannel> VerifyLogChannel(IGuild guild, ITextChannel channel)
		{
			//Check to make sure the bot can post to there
			return await PermissionCheck(channel) ? channel : null;
		}

		public static async Task<bool> PermissionCheck(ITextChannel channel)
		{
			//Return false if the channel doesn't exist
			if (channel == null)
				return false;

			//Get the bot
			var bot = GetBot(channel.Guild);

			//Check if the bot can send messages
			if (!bot.GetPermissions(channel).SendMessages)
				return false;

			//Check if the bot can embed
			if (!bot.GetPermissions(channel).EmbedLinks)
			{
				await SendChannelMessage(channel, "Bot is unable to use message embeds on this channel.");
				return false;
			}

			return true;
		}

		public static async Task LogImage(ITextChannel channel, IMessage message, bool embeds)
		{
			//Get the user
			var user = message.Author;

			//Get the links
			var attachmentURLs = new List<string>();
			var embedURLs = new List<string>();
			var videoEmbeds = new List<IEmbed>();
			if (!embeds && message.Attachments.Any())
			{
				//If attachment, the file is hosted on discord which has a concrete URL name for files (cdn.discordapp.com/attachments/.../x.png)
				attachmentURLs = message.Attachments.Select(x => x.Url).Distinct().ToList();
			}
			else if (embeds && message.Embeds.Any())
			{
				//If embed this is slightly trickier, but only images/videos can embed (AFAIK)
				message.Embeds.ToList().ForEach(x =>
				{
					if (x.Video == null)
					{
						//If no video then it has to be just an image
						if (x.Thumbnail.HasValue && !String.IsNullOrEmpty(x.Thumbnail.Value.Url))
						{
							embedURLs.Add(x.Thumbnail.Value.Url);
						}
						if (x.Image.HasValue && !String.IsNullOrEmpty(x.Image.Value.Url))
						{
							embedURLs.Add(x.Image.Value.Url);
						}
					}
					else
					{
						//Add the video URL and the thumbnail URL
						videoEmbeds.Add(x);
					}
				});
			}
			//Attached files
			await attachmentURLs.ForEachAsync(async x =>
			{
				//Image attachment
				if (Constants.VALID_IMAGE_EXTENSIONS.CaseInsContains(Path.GetExtension(x)))
				{
					var desc = String.Format("**Channel:** `{0}`\n**Message ID:** `{1}`", message.Channel.FormatChannel(), message.Id);
					var embed = MakeNewEmbed(null, desc, Constants.ATCH, x);
					AddFooter(embed, "Attached Image");
					AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl(), x);
					await SendEmbedMessage(channel, embed);

					++Variables.LoggedImages;
				}
				//Gif attachment
				else if (Constants.VALID_GIF_EXTENTIONS.CaseInsContains(Path.GetExtension(x)))
				{
					var desc = String.Format("**Channel:** `{0}`\n**Message ID:** `{1}`", message.Channel.FormatChannel(), message.Id);
					var embed = MakeNewEmbed(null, desc, Constants.ATCH, x);
					AddFooter(embed, "Attached Gif");
					AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl(), x);
					await SendEmbedMessage(channel, embed);

					++Variables.LoggedGifs;
				}
				//Random file attachment
				else
				{
					var desc = String.Format("**Channel:** `{0}`\n**Message ID:** `{1}`", message.Channel.FormatChannel(), message.Id);
					var embed = MakeNewEmbed(null, desc, Constants.ATCH, x);
					AddFooter(embed, "Attached File");
					AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl(), x);
					await SendEmbedMessage(channel, embed);

					++Variables.LoggedFiles;
				}
			});
			//Embedded images
			await embedURLs.Distinct().ToList().ForEachAsync(async x =>
			{
				var desc = String.Format("**Channel:** `{0}`\n**Message ID:** `{1}`", message.Channel.FormatChannel(), message.Id);
				var embed = MakeNewEmbed(null, desc, Constants.ATCH, x);
				AddFooter(embed, "Embedded Image");
				AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl(), x);
				await SendEmbedMessage(channel, embed);

				++Variables.LoggedImages;
			});
			//Embedded videos/gifs
			await videoEmbeds.GroupBy(x => x.Url).Select(x => x.First()).ToList().ForEachAsync(async x =>
			{
				var desc = String.Format("**Channel:** `{0}`\n**Message ID:** `{1}`", message.Channel.FormatChannel(), message.Id);
				var embed = MakeNewEmbed(null, desc, Constants.ATCH, x.Thumbnail.Value.Url);
				AddFooter(embed, "Embedded " + (Constants.VALID_GIF_EXTENTIONS.CaseInsContains(Path.GetExtension(x.Thumbnail.Value.Url)) ? "Gif" : "Video"));
				AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl(), x.Url);
				await SendEmbedMessage(channel, embed);

				++Variables.LoggedGifs;
			});
		}

		public static async Task HandleJoiningUsers(BotGuildInfo guildInfo, IGuildUser user)
		{
			//Slowmode
			{
				var smGuild = ((SlowmodeGuild)guildInfo.GetSetting(SettingOnGuild.SlowmodeGuild));
				if (smGuild != null)
				{
					smGuild.Users.ThreadSafeAdd(new SlowmodeUser(user, smGuild.BaseMessages, smGuild.Interval));
				}
				var smChannels = ((List<SlowmodeChannel>)guildInfo.GetSetting(SettingOnGuild.SlowmodeChannels));
				if (smChannels.Any())
				{
					smChannels.Where(x => (user.Guild as SocketGuild).TextChannels.Select(y => y.Id).Contains(x.ChannelID)).ToList().ForEach(smChan =>
					{
						smChan.Users.ThreadSafeAdd(new SlowmodeUser(user, smChan.BaseMessages, smChan.Interval));
					});
				}
			}

			//Raid Prevention
			{
				var antiRaid = guildInfo.GetRaidPrevention(RaidType.Regular);
				if (antiRaid != null && antiRaid.Enabled)
				{
					await antiRaid.PunishUser(user);
				}
				var antiJoin = guildInfo.GetRaidPrevention(RaidType.RapidJoins);
				if (antiJoin != null && antiJoin.Enabled)
				{
					antiJoin.Add(user.JoinedAt.Value.UtcDateTime);
					if (antiJoin.GetSpamCount() >= antiJoin.RequiredCount)
					{
						await antiJoin.PunishUser(user);
						var serverLog = ((DiscordObjectWithID<ITextChannel>)guildInfo.GetSetting(SettingOnGuild.ServerLog)).Object;
						if (serverLog != null)
						{
							await SendEmbedMessage(serverLog, MakeNewEmbed("Anti Rapid Join Mute", String.Format("**User:** {0}", user.FormatUser())));
						}
					}
				}
			}	
		}

		public static bool VerifyLoggingIsEnabledOnThisChannel(BotGuildInfo guildInfo, IMessage message)
		{
			return !((List<ulong>)guildInfo.GetSetting(SettingOnGuild.IgnoredLogChannels)).Contains(message.Channel.Id);
		}

		public static bool VerifyMessageShouldBeLogged(BotGuildInfo guildInfo, IMessage message)
		{
			//Ignore null messages
			if (message == null)
				return false;
			//Ignore webhook messages
			else if (message.Author.IsWebhook)
				return false;
			//Ignore bot messgaes
			else if (message.Author.IsBot && message.Author.Id != Variables.BotID)
				return false;
			//Ignore commands on channels that shouldn't be logged
			else if (!VerifyLoggingIsEnabledOnThisChannel(guildInfo, message))
				return false;
			return true;
		}

		public static bool VerifyServerLoggingAction(SocketGuildUser user, LogAction logAction, out VerifiedLoggingAction verifLoggingAction)
		{
			return VerifyServerLoggingAction(user.Guild, logAction, out verifLoggingAction);
		}

		public static bool VerifyServerLoggingAction(ISocketMessageChannel channel, LogAction logAction, out VerifiedLoggingAction verifLoggingAction)
		{
			return VerifyServerLoggingAction(GetGuild(channel) as SocketGuild, logAction, out verifLoggingAction) && !((List<ulong>)verifLoggingAction.GuildInfo.GetSetting(SettingOnGuild.IgnoredLogChannels)).Contains(channel.Id);
		}

		public static bool VerifyServerLoggingAction(SocketGuild guild, LogAction logAction, out VerifiedLoggingAction verifLoggingAction)
		{
			verifLoggingAction = new VerifiedLoggingAction(null, null, null);
			if (Variables.Pause)
				return false;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return false;

			var logChannel = ((DiscordObjectWithID<ITextChannel>)guildInfo.GetSetting(SettingOnGuild.ServerLog)).Object;
			verifLoggingAction = new VerifiedLoggingAction(guild, guildInfo, logChannel);
			return logChannel != null && ((List<LogAction>)guildInfo.GetSetting(SettingOnGuild.LogActions)).Contains(logAction);
		}
		#endregion

		#region Preferences/Settings
		public static async Task ValidateBotKey(BotClient client, string input, bool startup = false)
		{
			var key = input.Trim();

			if (startup)
			{
				//Check if the bot already has a key
				if (!String.IsNullOrWhiteSpace(input))
				{
					try
					{
						await client.LoginAsync(TokenType.Bot, key);
						Variables.GotKey = true;
					}
					catch (Exception)
					{
						//If the key doesn't work then retry
						WriteLine("The given key is no longer valid. Please enter a new valid key:");
					}
				}
				else
				{
					WriteLine("Please enter the bot's key:");
				}
				return;
			}

			//Login and connect to Discord.
			if (key.Length > 59)
			{
				//If the length isn't the normal length of a key make it retry
				WriteLine("The given key is too long. Please enter a regular length key:");
			}
			else if (key.Length < 59)
			{
				WriteLine("The given key is too short. Please enter a regular length key:");
			}
			else
			{
				try
				{
					//Try to login with the given key
					await client.LoginAsync(TokenType.Bot, key);

					//If the key works then save it within the settings
					WriteLine("Succesfully logged in via the given bot key.");
					Properties.Settings.Default.BotKey = key;
					Properties.Settings.Default.Save();
					Variables.GotKey = true;
				}
				catch (Exception)
				{
					//If the key doesn't work then retry
					WriteLine("The given key is invalid. Please enter a valid key:");
				}
			}
		}

		public static void ValidatePath(string input, bool startup = false)
		{
			var path = input.Trim();

			if (startup)
			{
				//Check if a path is already input
				if (!String.IsNullOrWhiteSpace(path) && Directory.Exists(path))
				{
					Properties.Settings.Default.Path = path;
					Properties.Settings.Default.Save();
					Variables.GotPath = true;
				}
				else
				{
					//Send the initial message
					if (Variables.Windows)
					{
						WriteLine("Please enter a valid directory path in which to save files or say 'AppData':");
					}
					else
					{
						WriteLine("Please enter a valid directory path in which to save files:");
					}
				}
				return;
			}

			if (Variables.Windows && "appdata".CaseInsEquals(path))
			{
				path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			}

			if (Directory.Exists(path))
			{
				Properties.Settings.Default.Path = path;
				Properties.Settings.Default.Save();
				Variables.GotPath = true;
			}
			else
			{
				WriteLine("Invalid directory. Please enter a valid directory:");
			}
		}

		public static void ResetSettings()
		{
			var botInfo = Variables.BotInfo;
			botInfo.ResetAll();
			Variables.BotInfo.SaveInfo();

			Properties.Settings.Default.Reset();
			Properties.Settings.Default.Save();
		}
		#endregion

		#region Slowmode/Banned Phrases/Spam Prevention
		public static async Task SpamCheck(BotGuildInfo guildInfo, IGuild guild, IUser author, IMessage msg)
		{
			var spamPrevUsers = ((List<SpamPreventionUser>)guildInfo.GetSetting(SettingOnGuild.SpamPreventionUsers));
			var spamUser = spamPrevUsers.FirstOrDefault(x => x.User.Id == author.Id);
			if (spamUser == null)
			{
				spamPrevUsers.ThreadSafeAdd(spamUser = new SpamPreventionUser(author as IGuildUser));
			}

			//TODO: Make sure this works
			var spam = false;
			foreach (var spamType in Enum.GetValues(typeof(SpamType)).Cast<SpamType>())
			{
				var spamPrev = guildInfo.GetSpamPrevention(spamType);
				if (spamPrev == null || !spamPrev.Enabled)
					return;

				var userSpamList = spamUser.SpamLists[spamType];

				var spamAmt = 0;
				switch (spamType)
				{
					case SpamType.Message:
					{
						spamAmt = int.MaxValue;
						break;
					}
					case SpamType.LongMessage:
					{
						spamAmt = msg.Content?.Length ?? 0;
						break;
					}
					case SpamType.Link:
					{
						spamAmt = msg.Content?.Split(' ')?.Count(x => Uri.IsWellFormedUriString(x, UriKind.Absolute)) ?? 0;
						break;
					}
					case SpamType.Image:
					{
						var attachCount = msg.Attachments.Where(x =>
						{
							return false
							|| x.Height != null
							|| x.Width != null;
						}).Count();

						var embedCount = msg.Embeds.Where(x =>
						{
							return false
							|| x.Image != null
							|| x.Video != null;
						}).Count();

						spamAmt = attachCount + embedCount;
						break;
					}
					case SpamType.Mention:
					{
						spamAmt = msg.MentionedUserIds.Distinct().Count();
						break;
					}
				}

				if (spamAmt >= spamPrev.RequiredSpamPerMessage)
				{
					//Ticks should be small enough that this will not allow duplicates of the same message, but can still allow rapidly spammed messages
					if (!userSpamList.Any(x => x.GetTime().Ticks == msg.CreatedAt.UtcTicks))
					{
						userSpamList.ThreadSafeAdd(new BasicTimeInterface(msg.CreatedAt.UtcDateTime));
					}
				}

				if (spamUser.CheckIfAllowedToPunish(spamPrev, spamType, msg))
				{
					await DeleteMessage(msg);

					//Make sure they have the lowest vote count required to kick and the most severe punishment type
					spamUser.ChangeVotesRequired(spamPrev.VotesForKick);
					spamUser.ChangePunishmentType(spamPrev.PunishmentType);
					spamUser.EnablePunishable();

					spam = true;
				}
			}

			if (spam)
			{
				await MakeAndDeleteSecondaryMessage(msg.Channel, String.Format("The user `{0}` needs `{1}` votes to be kicked. Vote to kick them by mentioning them.",
					author.FormatUser(), spamUser.VotesRequired - spamUser.VotesToKick));
			}
		}

		public static async Task HandleSlowmode(SlowmodeGuild smGuild, SlowmodeChannel smChannel, IMessage message)
		{
			if (smGuild != null)
			{
				await HandleSlowmodeUser(smGuild.Users.FirstOrDefault(x => x.User.Id == message.Author.Id), message);
			}
			if (smChannel != null)
			{
				await HandleSlowmodeUser(smChannel.Users.FirstOrDefault(x => x.User.Id == message.Author.Id), message);
			}
		}

		public static async Task HandleSlowmodeUser(SlowmodeUser user, IMessage message)
		{
			if (user != null)
			{
				//If the user still has messages left, check if this is the first of their interval. Start a countdown if it is. Else lower by one or delete the message.
				if (user.CurrentMessagesLeft > 0)
				{
					if (user.CurrentMessagesLeft == user.BaseMessages)
					{
						user.SetNewTime(DateTime.UtcNow.AddSeconds(user.Interval));
						Variables.SlowmodeUsers.ThreadSafeAdd(user);
					}

					user.LowerMessagesLeft();
				}
				else
				{
					await DeleteMessage(message);
				}
			}
		}

		public static async Task HandleBannedPhrases(BotGuildInfo guildInfo, SocketGuild guild, IMessage message)
		{
			//Ignore admins and messages older than an hour. (Accidentally deleted something important once due to not having these checks in place, but this should stop most accidental deletions)
			if ((message.Author as IGuildUser).GuildPermissions.Administrator || (int)DateTime.UtcNow.Subtract(message.CreatedAt.UtcDateTime).TotalHours > 0)
				return;

			var phrase = ((List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseStrings)).FirstOrDefault(x =>
			{
				return message.Content.CaseInsContains(x.Phrase);
			});
			if (phrase != null)
			{
				await HandleBannedPhrasePunishments(guildInfo, guild, message, phrase);
			}

			var regex = ((List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseRegex)).FirstOrDefault(x =>
			{
				return Regex.IsMatch(message.Content, x.Phrase, RegexOptions.IgnoreCase, new TimeSpan(Constants.REGEX_TIMEOUT));
			});
			if (regex != null)
			{
				await HandleBannedPhrasePunishments(guildInfo, guild, message, regex);
			}
		}

		public static async Task HandleBannedPhrasePunishments(BotGuildInfo guildInfo, SocketGuild guild, IMessage message, BannedPhrase phrase)
		{
			await DeleteMessage(message);
			var user = message.Author as IGuildUser;
			var bpUser = ((List<BannedPhraseUser>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseUsers)).FirstOrDefault(x => x.User == user) ?? new BannedPhraseUser(user, guildInfo);

			var amountOfMsgs = 0;
			switch (phrase.Punishment)
			{
				case PunishmentType.Role:
				{
					bpUser.IncreaseRoleCount();
					amountOfMsgs = bpUser.MessagesForRole;
					break;
				}
				case PunishmentType.Kick:
				{
					bpUser.IncreaseKickCount();
					amountOfMsgs = bpUser.MessagesForKick;
					break;
				}
				case PunishmentType.Ban:
				{
					bpUser.IncreaseBanCount();
					amountOfMsgs = bpUser.MessagesForBan;
					break;
				}
			}

			//Get the banned phrases punishments from the guild
			if (!TryGetPunishment(guildInfo, phrase.Punishment, amountOfMsgs, out BannedPhrasePunishment punishment))
				return;

			var logChannel = ((DiscordObjectWithID<ITextChannel>)guildInfo.GetSetting(SettingOnGuild.ServerLog)).Object;
			switch (punishment.Punishment)
			{
				case PunishmentType.Kick:
				{
					//Check if can kick them
					if (GetUserPosition(user) > GetUserPosition(GetBot(user.Guild)))
						return;

					await BotKickUser(user, "banned phrases");
					bpUser.ResetKickCount();
					break;
				}
				case PunishmentType.Ban:
				{
					//Check if can ban them
					if (GetUserPosition(user) > GetUserPosition(GetBot(user.Guild)))
						return;

					await BotBanUser(user.Guild, user.Id, 1, "banned phrases");
					bpUser.ResetBanCount();
					break;
				}
				case PunishmentType.Role:
				{
					await GiveRole(user, punishment.Role);
					bpUser.ResetRoleCount();

					//If a time is specified, run through the time then remove the role
					if (punishment.PunishmentTime != null)
					{
						Variables.PunishedUsers.Add(new RemovablePunishment(guild, user.Id, punishment.Role, DateTime.UtcNow.AddMinutes((int)punishment.PunishmentTime)));
					}

					if (logChannel != null)
					{
						var embed = MakeNewEmbed(null, "**Role Gained:** " + punishment.Role.Name, Constants.UEDT);
						AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl());
						AddFooter(embed, "Banned Phrases Role");
						await SendEmbedMessage(logChannel, embed);
					}
					break;
				}
			}
		}

		public static bool TryGetPunishment(BotGuildInfo guildInfo, PunishmentType type, int msgs, out BannedPhrasePunishment punishment)
		{
			punishment = ((List<BannedPhrasePunishment>)guildInfo.GetSetting(SettingOnGuild.BannedPhrasePunishments)).FirstOrDefault(x => x.Punishment == type && x.NumberOfRemoves == msgs);
			return punishment != null;
		}

		public static bool TryGetBannedRegex(BotGuildInfo guildInfo, string searchPhrase, out BannedPhrase bannedRegex)
		{
			bannedRegex = ((List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseRegex)).FirstOrDefault(x => x.Phrase.CaseInsEquals(searchPhrase));
			return bannedRegex != null;
		}

		public static bool TryGetBannedString(BotGuildInfo guildInfo, string searchPhrase, out BannedPhrase bannedString)
		{
			bannedString = ((List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseStrings)).FirstOrDefault(x => x.Phrase.CaseInsEquals(searchPhrase));
			return bannedString != null;
		}

		public static bool TryCreateRegex(string input, out Regex regexOutput, out string stringOutput)
		{
			regexOutput = null;
			stringOutput = null;
			try
			{
				regexOutput = new Regex(input);
				return true;
			}
			catch (Exception e)
			{
				stringOutput = e.Message;
				return false;
			}
		}

		public static void AddSlowmodeUser(SlowmodeGuild smGuild, List<SlowmodeChannel> smChannels, IGuildUser user)
		{
			if (smGuild != null)
			{
				smGuild.Users.ThreadSafeAdd(new SlowmodeUser(user, smGuild.BaseMessages, smGuild.Interval));
			}

			smChannels.Where(x => (user.Guild as SocketGuild).TextChannels.Select(y => y.Id).Contains(x.ChannelID)).ToList().ForEach(smChan =>
			{
				smChan.Users.ThreadSafeAdd(new SlowmodeUser(user, smChan.BaseMessages, smChan.Interval));
			});
		}

		public static void HandleBannedPhraseModification(List<BannedPhrase> bannedStrings, IEnumerable<string> inputPhrases, bool add, out List<string> success, out List<string> failure)
		{
			success = new List<string>();
			failure = new List<string>();
			if (add)
			{
				//Don't add duplicate words
				foreach (var str in inputPhrases)
				{
					if (!bannedStrings.Any(x => x.Phrase.CaseInsEquals(str)))
					{
						bannedStrings.Add(new BannedPhrase(str, PunishmentType.Nothing));
						success.Add(str);
					}
					else
					{
						failure.Add(str);
					}
				}
			}
			else
			{
				var positions = new List<int>();
				foreach (var potentialPosition in inputPhrases)
				{
					if (int.TryParse(potentialPosition, out int temp) && temp < bannedStrings.Count)
					{
						positions.Add(temp);
					}
				}

				//Removing by phrase
				if (!positions.Any())
				{
					foreach (var str in inputPhrases)
					{
						var temp = bannedStrings.FirstOrDefault(x => x.Phrase.Equals(str));
						if (temp != null)
						{
							success.Add(str);
							bannedStrings.Remove(temp);
						}
						else
						{
							failure.Add(str);
						}
					}
				}
				//Removing by index
				else
				{
					//Put them in descending order so as to not delete low values before high ones
					foreach (var position in positions.OrderByDescending(x => x))
					{
						if (bannedStrings.Count - 1 <= position)
						{
							var temp = bannedStrings[position];
							bannedStrings.Remove(temp);
							success.Add(temp?.Phrase ?? "null");
							continue;
						}
						failure.Add("String at position " + position);
					}
				}
			}

			return;
		}
		#endregion

		#region Close Words
		public static List<CloseWord<T>> GetObjectsWithSimilarNames<T>(List<T> suppliedObjects, string input) where T : INameAndText
		{
			var closeWords = new List<CloseWord<T>>();
			foreach (var word in suppliedObjects)
			{
				var closeness = FindCloseName(word.Name, input);
				if (closeness > 3)
					continue;

				closeWords.Add(new CloseWord<T>(word, closeness));
				if (closeWords.Count > 5)
				{
					closeWords.OrderBy(x => x.Closeness);
					closeWords.RemoveRange(4, closeWords.Count - 4);
				}
			}

			foreach (var word in suppliedObjects.Where(x => x.Name.CaseInsContains(input)))
			{
				if (closeWords.Count >= 5)
				{
					break;
				}
				else if (!closeWords.Any(x => x.Word.Name.CaseInsEquals(word.Name)))
				{
					closeWords.Add(new CloseWord<T>(word, 5));
				}
			}

			return closeWords;
		}

		public static int FindCloseName(string source, string target, int threshold = 10)
		{
			/* Damerau Levenshtein Distance: https://en.wikipedia.org/wiki/Damerau–Levenshtein_distance
			 * Copied verbatim from: https://stackoverflow.com/a/9454016 
			 */
			int length1 = source.Length;
			int length2 = target.Length;

			// Return trivial case - difference in string lengths exceeds threshhold
			if (Math.Abs(length1 - length2) > threshold) { return int.MaxValue; }

			// Ensure arrays [i] / length1 use shorter length 
			if (length1 > length2)
			{
				Swap(ref target, ref source);
				Swap(ref length1, ref length2);
			}

			int maxi = length1;
			int maxj = length2;

			int[] dCurrent = new int[maxi + 1];
			int[] dMinus1 = new int[maxi + 1];
			int[] dMinus2 = new int[maxi + 1];
			int[] dSwap;

			for (int i = 0; i <= maxi; i++) { dCurrent[i] = i; }

			int jm1 = 0, im1 = 0, im2 = -1;

			for (int j = 1; j <= maxj; j++)
			{

				// Rotate
				dSwap = dMinus2;
				dMinus2 = dMinus1;
				dMinus1 = dCurrent;
				dCurrent = dSwap;

				// Initialize
				int minDistance = int.MaxValue;
				dCurrent[0] = j;
				im1 = 0;
				im2 = -1;

				for (int i = 1; i <= maxi; i++)
				{

					int cost = source[im1] == target[jm1] ? 0 : 1;

					int del = dCurrent[im1] + 1;
					int ins = dMinus1[i] + 1;
					int sub = dMinus1[im1] + cost;

					//Fastest execution for min value of 3 integers
					int min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

					if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
						min = Math.Min(min, dMinus2[im2] + cost);

					dCurrent[i] = min;
					if (min < minDistance) { minDistance = min; }
					im1++;
					im2++;
				}
				jm1++;
				if (minDistance > threshold) { return int.MaxValue; }
			}

			int result = dCurrent[maxi];
			return (result > threshold) ? int.MaxValue : result;
		}

		public static void Swap<T>(ref T arg1, ref T arg2)
		{
			T temp = arg1;
			arg1 = arg2;
			arg2 = temp;
		}
		#endregion

		#region Timers
		public static List<T> GetOutTimedObject<T>(List<T> inputList) where T : ITimeInterface
		{
			var eligibleToBeGotten = inputList.Where(x => x.GetTime() <= DateTime.UtcNow).ToList();
			inputList.ThreadSafeRemoveAll(x => eligibleToBeGotten.Contains(x));
			return eligibleToBeGotten;
		}

		public static void RemoveCommandMessages(List<IMessage> messages, int time)
		{
			lock (Variables.TimedMessages)
			{
				Variables.TimedMessages.Add(new RemovableMessage(messages, DateTime.UtcNow.AddMilliseconds(time)));
			}
		}

		public static void RemoveCommandMessage(IMessage message, int time)
		{
			lock (Variables.TimedMessages)
			{
				Variables.TimedMessages.Add(new RemovableMessage(message, DateTime.UtcNow.AddMilliseconds(time)));
			}
		}

		public static void HourTimer(object obj)
		{
			ClearPunishedUsersList();

			const long PERIOD = 60 * 60 * 1000;
			var time = PERIOD;
			if ((DateTime.UtcNow.Subtract(Variables.StartupTime)).TotalHours < 1)
			{
				time -= (long)DateTime.UtcNow.TimeOfDay.TotalMilliseconds % PERIOD;
			}
			Variables.HourTimer = new Timer(HourTimer, null, time, Timeout.Infinite);
		}

		public static void MinuteTimer(object obj)
		{
			RemovePunishments();

			const long PERIOD = 60 * 1000;
			var time = PERIOD;
			if ((DateTime.UtcNow.Subtract(Variables.StartupTime)).TotalMinutes < 1)
			{
				time -= (long)DateTime.UtcNow.TimeOfDay.TotalMilliseconds % PERIOD;
			}
			Variables.MinuteTimer = new Timer(MinuteTimer, null, time, Timeout.Infinite);
		}

		public static void OneFourthSecondTimer(object obj)
		{
			DeleteTargettedMessages();
			RemoveActiveCloseHelpAndWords();
			ResetSlowModeUserMessages();

			const long PERIOD = 250;
			var time = PERIOD;
			if ((DateTime.UtcNow.Subtract(Variables.StartupTime)).TotalSeconds < 1)
			{
				time -= (long)DateTime.UtcNow.TimeOfDay.TotalMilliseconds % PERIOD;
			}
			Variables.OneFourthSecondTimer = new Timer(OneFourthSecondTimer, null, time, Timeout.Infinite);
		}

		public static void ClearPunishedUsersList()
		{
			Variables.Guilds.Values.ToList().ForEach(x => ((List<SpamPreventionUser>)x.GetSetting(SettingOnGuild.SpamPreventionUsers)).Clear());
		}

		public static void RemovePunishments()
		{
			//The reason this is not a foreachasync is 1) it doesn't work well with a timer and 2) the results of this are unimportant
			GetOutTimedObject(Variables.PunishedUsers).ForEach(async punishment =>
			{
				//Things that can be done with an IUser
				var userID = punishment.UserID;
				if (punishment.Type == PunishmentType.Ban)
				{
					await punishment.Guild.RemoveBanAsync(userID);
					return;
				}

				//Things that need an IGuildUser
				var guildUser = await punishment.Guild.GetUserAsync(userID);
				if (guildUser == null)
					return;

				switch (punishment.Type)
				{
					case PunishmentType.Role:
					{
						await guildUser.RemoveRoleAsync(punishment.Role);
						return;
					}
					case PunishmentType.Deafen:
					{
						await guildUser.ModifyAsync(x => x.Deaf = false);
						return;
					}
					case PunishmentType.Mute:
					{
						await guildUser.ModifyAsync(x => x.Mute = false);
						return;
					}
				}
			});
		}

		public static void DeleteTargettedMessages()
		{
			//The reason this is not a foreachasync is 1) it doesn't work well with a timer and 2) the results of this are unimportant
			GetOutTimedObject(Variables.TimedMessages).ForEach(async timed =>
			{
				if (timed.Messages.Count() == 1)
				{
					await DeleteMessage(timed.Messages.FirstOrDefault());
				}
				else
				{
					await DeleteMessages(timed.Channel, timed.Messages);
				}
			});
		}

		public static void RemoveActiveCloseHelpAndWords()
		{
			GetOutTimedObject(Variables.ActiveCloseHelp);
			GetOutTimedObject(Variables.ActiveCloseWords);
		}

		public static void ResetSlowModeUserMessages()
		{
			GetOutTimedObject(Variables.SlowmodeUsers).ForEach(x =>
			{
				x.ResetMessagesLeft();
			});
		}
		#endregion

		#region Miscellaneous
		public static async Task UpdateGame()
		{
			var game = ((string)Variables.BotInfo.GetSetting(SettingOnBot.Game));
			var stream = ((string)Variables.BotInfo.GetSetting(SettingOnBot.Stream));
			var prefix = ((string)Variables.BotInfo.GetSetting(SettingOnBot.Prefix));

			if (String.IsNullOrWhiteSpace(game))
			{
				game = String.Format("type \"{0}help\" for help.", prefix);
			}

			if (String.IsNullOrWhiteSpace(stream))
			{
				await Variables.Client.SetGameAsync(game, stream, StreamType.NotStreaming);
			}
			else
			{
				await Variables.Client.SetGameAsync(game, Constants.TWITCH_URL + stream, StreamType.Twitch);
			}
		}

		public static string[] SplitByCharExceptInQuotes(string inputString, char inputChar)
		{
			if (String.IsNullOrWhiteSpace(inputString))
				return null;

			return inputString.Split('"').Select((element, index) =>
			{
				if (index % 2 == 0)
				{
					return element.Split(new[] { inputChar }, StringSplitOptions.RemoveEmptyEntries);
				}
				else
				{
					return new[] { element };
				}
			}).SelectMany(x => x).Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
		}

		public static bool CheckIfRegMatch(string msg, string pattern)
		{
			return Regex.IsMatch(msg, pattern, RegexOptions.IgnoreCase, new TimeSpan(Constants.REGEX_TIMEOUT));
		}

		public static void RestartBot()
		{
			try
			{
				//Create a new instance of the bot and close the old one
				System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
				Environment.Exit(0);
			}
			catch (Exception e)
			{
				ExceptionToConsole(e);
			}
		}

		public static void DisconnectBot()
		{
			Environment.Exit(0);
		}

		public static void DontWaitForResultOfBigUnimportantFunction(IMessageChannel channel, Action func)
		{
			Task.Run(() =>
			{
				if (channel != null)
				{
					using (var typing = channel.EnterTypingState())
					{
						func.Invoke();
					}
				}
				else
				{
					func.Invoke();
				}
			}).Forget();
		}

		public static ReturnedObject<T> GetDiscordObject<T>(IGuild guild, IGuildUser user, ObjectVerification[] verif, T obj) where T : ISnowflakeEntity
		{
			if (obj is IGuildChannel)
			{
				return GetChannel(guild, user, verif, (dynamic)obj);
			}
			else if (obj is IRole)
			{
				return GetRole(guild, user, verif, (dynamic)obj);
			}
			else if (obj is IGuildUser)
			{
				return GetGuildUser(guild, user, verif, (dynamic)obj);
			}
			else
			{
				return new ReturnedObject<T>(obj, FailureReason.NotFailure);
			}
		}

		public static ulong AddGuildPermissionBit(string permissionName, ulong inputValue)
		{
			var permission = Variables.GuildPermissions.FirstOrDefault(x => x.Name.CaseInsEquals(permissionName));
			if (!permission.Equals(default(BotGuildPermission)))
			{
				inputValue |= permission.Bit;
			}
			return inputValue;
		}

		public static ulong AddChannelPermissionBit(string permissionName, ulong inputValue)
		{
			var permission = Variables.ChannelPermissions.FirstOrDefault(x => x.Name.CaseInsEquals(permissionName));
			if (!permission.Equals(default(BotChannelPermission)))
			{
				inputValue |= permission.Bit;
			}
			return inputValue;
		}

		private static readonly Regex TWITCH_USERNAME_REGEX = new Regex("^[a-zA-Z0-9_]{4,25}$", RegexOptions.Compiled); //Source: https://www.reddit.com/r/Twitch/comments/32w5b2/username_requirements/cqf8yh0/
		public static bool MakeSureInputIsValidTwitchAccountName(string input)
		{
			return TWITCH_USERNAME_REGEX.IsMatch(input);
		}

		public static Dictionary<string, Color> CreateColorDictionary()
		{
			var dict = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);
			foreach (var color in typeof(Color).GetFields().Where(x => x.IsPublic))
			{
				dict.Add(color.Name, (Color)color.GetValue(new Color()));
			}
			return dict;
		}
		#endregion
	}

	public static class ExtendedMethods
	{
		public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
		{
			foreach (var value in list)
			{
				await func(value);
			}
		}

		public static async void Forget(this Task task)
		{
			try
			{
				await task.ConfigureAwait(false);
			}
			catch (Exception e)
			{
				Actions.ExceptionToConsole(e);
			}
		}

		public static List<T> GetUpToAndIncludingMinNum<T>(this List<T> list, params int[] x)
		{
			return list.GetRange(0, Math.Max(0, Math.Min(list.Count, x.Min())));
		}

		public static void ThreadSafeAdd<T>(this List<T> list, T obj)
		{
			lock (list)
			{
				list.Add(obj);
			}
		}

		public static void ThreadSafeRemove<T>(this List<T> list, T obj)
		{
			lock (list)
			{
				list.Remove(obj);
			}
		}

		public static void ThreadSafeRemoveAll<T>(this List<T> list, Predicate<T> match)
		{
			lock (list)
			{
				list.RemoveAll(match);
			}
		}

		public static string FormatNumberedList<T>(this IEnumerable<T> list, string format, params Func<T, object>[] args)
		{
			var count = 1;
			var maxLen = list.Count().ToString().Length;
			//.ToArray() must be used or else String.Format tries to use an overload accepting object as a parameter instead of object[] thus causing an exception
			return String.Join("\n", list.Select(x => String.Format("`{0}.` ", count++.ToString().PadLeft(maxLen, '0')) + String.Format(@format, args.Select(y => y(x)).ToArray())));
		}

		public static string FormatUser(this IUser user, ulong? userID = 0)
		{
			user = user ?? Variables.Client.GetUser((ulong)userID);
			if (user != null)
			{
				return String.Format("'{0}#{1}' ({2})",
					Actions.EscapeMarkdown(user.Username, true).CaseInsReplace("discord.gg", Constants.FAKE_DISCORD_LINK),
					user.Discriminator,
					user.Id);
			}
			else
			{
				return String.Format("Irretrievable User ({0})", userID);
			}
		}

		public static string FormatRole(this IRole role)
		{
			if (role != null)
			{
				return String.Format("'{0}' ({1})", Actions.EscapeMarkdown(role.Name, true), role.Id);
			}
			else
			{
				return "Irretrievable Role";
			}
		}

		public static string FormatChannel(this IChannel channel)
		{
			if (channel != null)
			{
				return String.Format("'{0}' ({1}) ({2})", Actions.EscapeMarkdown(channel.Name, true), Actions.GetChannelType(channel), channel.Id);
			}
			else
			{
				return "Irretrievable Channel";
			}
		}

		public static string FormatGuild(this IGuild guild, ulong? guildID = 0)
		{
			guild = guild ?? Variables.Client.GetGuild((ulong)guildID);
			if (guild != null)
			{
				return String.Format("'{0}' ({1})", Actions.EscapeMarkdown(guild.Name, true), guild.Id);
			}
			else
			{
				return String.Format("Irretrievable Guild ({0})", guildID); 
			}
		}

		public static bool CaseInsEverythingSame(this IEnumerable<string> enumerable)
		{
			var array = enumerable.ToArray();
			for (int i = 1; i < array.Length; i++)
			{
				if (!array[i - 1].CaseInsEquals(array[i]))
					return false;
			}
			return true;
		}

		public static bool CaseInsContains(this IEnumerable<string> enumerable, string search)
		{
			if (enumerable.Any())
			{
				return enumerable.Contains(search, StringComparer.OrdinalIgnoreCase);
			}
			return false;
		}

		public static string EnumName(this Enum e)
		{
			return Enum.GetName(e.GetType(), e);
		}

		public static bool AllCharactersAreWithinUpperLimit(this string str, int upperLimit)
		{
			if (String.IsNullOrWhiteSpace(str))
				return false;

			foreach (var c in str)
			{
				if (c > upperLimit)
					return false;
			}
			return true;
		}

		public static bool CaseInsEquals(this string str1, string str2)
		{
			if (str1 == null)
			{
				return str2 == null;
			}
			else if (str2 == null)
			{
				return false;
			}
			else
			{
				return str1.Equals(str2, StringComparison.OrdinalIgnoreCase);
			}
		}

		public static bool CaseInsContains(this string source, string search)
		{
			if (source == null || search == null)
			{
				return false;
			}
			else
			{
				return source.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
			}
		}

		public static bool CaseInsIndexOf(this string source, string search, out int position)
		{
			position = -1;
			if (source == null || search == null)
			{
				return false;
			}
			else
			{
				return (position = source.IndexOf(search, StringComparison.OrdinalIgnoreCase)) >= 0;
			}
		}

		public static bool CaseInsStartsWith(this string source, string search)
		{
			if (source == null || search == null)
			{
				return false;
			}
			else
			{
				return source.StartsWith(search, StringComparison.OrdinalIgnoreCase);
			}
		}

		public static bool CaseInsEndsWith(this string source, string search)
		{
			if (source == null || search == null)
			{
				return false;
			}
			else
			{
				return source.EndsWith(search, StringComparison.OrdinalIgnoreCase);
			}
		}

		public static string CaseInsReplace(this string str, string oldValue, string newValue)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			var previousIndex = 0;
			var index = str.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
			while (index != -1)
			{
				sb.Append(str.Substring(previousIndex, index - previousIndex));
				sb.Append(newValue);
				index += oldValue.Length;

				previousIndex = index;
				index = str.IndexOf(oldValue, index, StringComparison.OrdinalIgnoreCase);
			}
			sb.Append(str.Substring(previousIndex));

			return sb.ToString();
		}
	}
}
