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

			HandleBotID(Variables.Client.GetCurrentUser().Id);				//Give the variable Bot_ID the id of the bot
			Variables.BotName = Variables.Client.GetCurrentUser().Username; //Give the variable Bot_Name the username of the bot

			LoadPermissionNames();											//Gets the names of the permission bits in Discord
			LoadCommandInformation();                                       //Gets the information of a command (name, aliases, usage, summary). Has to go after LPN

			await LoadGuilds();												//Loads the guilds that attempted to load before the Bot_ID was gotten.
			await UpdateGame();												//Have the bot display its game and stream

			HourTimer(null);												//Start the hourly timer
			MinuteTimer(null);												//Start the minutely timer
			OneFourthSecondTimer(null);                                     //Start the one fourth second timer

			StartupMessages();
			Variables.Loaded = true;
		}

		public static async Task LoadGuilds()
		{
			await Variables.GuildsToBeLoaded.ForEachAsync(async x =>
			{
				Variables.Guilds.Add(x.Id, await CreateOrGetGuildInfo(x));
			});
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
				Variables.HelpList.Where(x => !cmdSwitches.Select(y => y.Name).CaseInsContains(x.Name)).ToList().ForEach(x => cmdSwitches.Add(new CommandSwitch(x.Name, x.DefaultEnabled)));

				var invites = ((List<BotInvite>)guildInfo.GetSetting(SettingOnGuild.Invites));
				invites.AddRange((await guild.GetInvitesAsync()).ToList().Select(x => new BotInvite(x.GuildId, x.Code, x.Uses)).ToList());
				guildInfo.PostDeserialize(guild.Id);
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
				WriteLine("The bot information file could not be found; using default.");
			}
			botInfo = botInfo ?? new BotGlobalInfo();

			botInfo.PostDeserialize();
			return botInfo;
		}

		public static string Serialize(dynamic obj)
		{
			return JsonConvert.SerializeObject(obj, Formatting.Indented);
		}

		public static void MaybeStartBot()
		{
			if (Variables.GotPath && Variables.GotKey && !Variables.Loaded)
			{
				new Program().Start(Variables.Client).GetAwaiter().GetResult();
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
			HandleBotID(Properties.Settings.Default.BotID);
			Variables.Windows = GetWindowsOrNot();
			Variables.Console = GetConsoleOrGUI();
			Variables.BotInfo = CreateBotInfo();
		}

		public static void LoadCommandInformation()
		{
			foreach (var classType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsSubclassOf(typeof(ModuleBase))))
			{
				var className = ((NameAttribute)classType.GetCustomAttribute(typeof(NameAttribute)))?.Text;
				if (className == null)
					continue;

				if (!Enum.TryParse(className, true, out CommandCategory category))
				{
					WriteLine(className + " is not currently in the CommandCategory enum.");
					continue;
				}

				foreach (var method in classType.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
				{
					//Get the name
					var name = "N/A";
					{
						var attr = (CommandAttribute)method.GetCustomAttribute(typeof(CommandAttribute));
						if (attr != null)
						{
							name = attr.Text;
						}
						else
							continue;
					}
					//Get the aliases
					string[] aliases = { "N/A" };
					{
						var attr = (AliasAttribute)method.GetCustomAttribute(typeof(AliasAttribute));
						if (attr != null)
						{
							aliases = attr.Aliases;
						}
					}
					//Get the usage
					var usage = "N/A";
					{
						var attr = (UsageAttribute)method.GetCustomAttribute(typeof(UsageAttribute));
						if (attr != null)
						{
							usage = name + " " + attr.Usage;
						}
					}
					//Get the base permissions
					var basePerm = "N/A";
					{
						basePerm = FormatAttribute((dynamic)method.GetCustomAttribute(typeof(PermissionRequirementAttribute)));
					}
					//Get the description
					var text = "N/A";
					{
						var attr = (SummaryAttribute)method.GetCustomAttribute(typeof(SummaryAttribute));
						if (attr != null)
						{
							text = attr.Text;
						}
					}
					//Get the default enabled
					var defaultEnabled = false;
					{
						var attr = (DefaultEnabledAttribute)method.GetCustomAttribute(typeof(DefaultEnabledAttribute));
						if (attr != null)
						{
							defaultEnabled = attr.Enabled;
						}
						else
						{
							WriteLine("Command does not have a default enabled value set: " + name);
						}
					}
					var simCmds = Variables.HelpList.Where(x =>
					{
						return CaseInsEquals(x.Name, name) || (x.Aliases[0] != "N/A" && x.Aliases.Intersect(aliases, StringComparer.OrdinalIgnoreCase).Any());
					});
					if (simCmds.Any())
					{
						WriteLine(String.Format("The following commands have conflicts: {0}\n{1}", String.Join(" + ", simCmds.Select(x => x.Name)), name));
					}
					//Add it to the helplist
					Variables.HelpList.Add(new HelpEntry(name, aliases, usage, basePerm, text, category, defaultEnabled));
				}
			}
			Variables.HelpList.ForEach(x => Variables.CommandNames.Add(x.Name));
		}

		public static void LoadPermissionNames()
		{
			for (int i = 0; i < 32; ++i)
			{
				var name = "";
				try
				{
					name = Enum.GetName(typeof(GuildPermission), i);
					if (name == null)
						continue;
				}
				catch (Exception)
				{
					WriteLine("Bad enum for GuildPermission: " + i);
					continue;
				}
				Variables.GuildPermissions.Add(new BotGuildPermissionType(name, i));
			}
			//Load all special cases
			LoadChannelPermissionNames();
		}

		public static void LoadChannelPermissionNames()
		{
			const uint GENERAL_BITS = 0
				| (1U << (int)GuildPermission.CreateInstantInvite)
				| (1U << (int)GuildPermission.ManageChannels)
				| (1U << (int)GuildPermission.ManageRoles)
				| (1U << (int)GuildPermission.ManageWebhooks);

			const uint TEXT_BITS = 0
				| (1U << (int)GuildPermission.ReadMessages)
				| (1U << (int)GuildPermission.SendMessages)
				| (1U << (int)GuildPermission.SendTTSMessages)
				| (1U << (int)GuildPermission.ManageMessages)
				| (1U << (int)GuildPermission.EmbedLinks)
				| (1U << (int)GuildPermission.AttachFiles)
				| (1U << (int)GuildPermission.ReadMessageHistory)
				| (1U << (int)GuildPermission.MentionEveryone)
				| (1U << (int)GuildPermission.UseExternalEmojis)
				| (1U << (int)GuildPermission.AddReactions);

			const uint VOICE_BITS = 0
				| (1U << (int)GuildPermission.Connect)
				| (1U << (int)GuildPermission.Speak)
				| (1U << (int)GuildPermission.MuteMembers)
				| (1U << (int)GuildPermission.DeafenMembers)
				| (1U << (int)GuildPermission.MoveMembers)
				| (1U << (int)GuildPermission.UseVAD);

			for (int i = 0; i < 32; i++)
			{
				var name = "";
				try
				{
					name = Enum.GetName(typeof(ChannelPermission), i);
					if (name == null)
						continue;
				}
				catch (Exception)
				{
					WriteLine("Bad enum for ChannelPermission: " + i);
					continue;
				}
				if ((GENERAL_BITS & (1U << i)) != 0)
				{
					Variables.ChannelPermissions.Add(new BotChannelPermissionType(name, i, gen: true));
				}
				if ((TEXT_BITS & (1U << i)) != 0)
				{
					Variables.ChannelPermissions.Add(new BotChannelPermissionType(name, i, text: true));
				}
				if ((VOICE_BITS & (1U << i)) != 0)
				{
					Variables.ChannelPermissions.Add(new BotChannelPermissionType(name, i, voice: true));
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
					WriteLine(String.Format("Unable to get the global setting for {0}.", Enum.GetName(typeof(SettingOnBot), e)));
				}
			}
			foreach (var e in Enum.GetValues(typeof(SettingOnGuild)).Cast<SettingOnGuild>())
			{
				if (BotGuildInfo.GetField(e) == null)
				{
					WriteLine(String.Format("Unable to get the guild setting for {0}.", Enum.GetName(typeof(SettingOnBot), e)));
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
			 * This list can keep all args separate or make the list a certain length. E.G. [ a, b, c, d ] (shortenTo = 3) => [ a, b, c d ]
			 * Specified arguments get left in a dictionary.
			 * Mentioned objects get left in respective lists of their ulong IDs.
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
					return new ReturnedArguments(list, ArgFailureReason.NotFailure);
				}
				else
				{
					return new ReturnedArguments(list, ArgFailureReason.TooFewArgs);
				}
			}

			var args = SplitByCharExceptInQuotes(input, ' ').ToList();
			if (min > max)
			{
				return new ReturnedArguments(args, ArgFailureReason.MaxLessThanMin);
			}
			else if (args.Count < min)
			{
				return new ReturnedArguments(args, ArgFailureReason.TooFewArgs);
			}
			else if (args.Count > max)
			{
				return new ReturnedArguments(args, ArgFailureReason.TooManyArgs);
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

		public static ReturnedType<T> GetType<T>(string input, IEnumerable<T> validTypes, IEnumerable<T> invalidTypes = null) where T : struct
		{
			if (!Enum.TryParse<T>(input, true, out T type))
			{
				return new ReturnedType<T>(type, TypeFailureReason.NotFound);
			}
			else if (invalidTypes != null && invalidTypes.Contains(type))
			{
				return new ReturnedType<T>(type, TypeFailureReason.InvalidType);
			}
			else if (!validTypes.Contains(type))
			{
				return new ReturnedType<T>(type, TypeFailureReason.InvalidType);
			}

			return new ReturnedType<T>(type, TypeFailureReason.NotFailure);
		}

		public static CommandSwitch GetCommand(BotGuildInfo guildInfo, string input)
		{
			return ((List<CommandSwitch>)guildInfo.GetSetting(SettingOnGuild.CommandSwitches)).FirstOrDefault(x =>
			{
				if (CaseInsEquals(x.Name, input))
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

		public static List<string> GetPermissionNames(uint flags)
		{
			var result = new List<string>();
			for (int i = 0; i < 32; ++i)
			{
				if ((flags & (1 << i)) != 0)
				{
					var name = Variables.GuildPermissions.FirstOrDefault(x => x.Position == i).Name;
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
			if (typeof(IGuildUser) == type)
			{
				return Constants.BASIC_TYPE_USER;
			}
			else if (typeof(IGuildChannel) == type)
			{
				return Constants.BASIC_TYPE_CHANNEL;
			}
			else if (typeof(IRole) == type)
			{
				return Constants.BASIC_TYPE_ROLE;
			}
			else if (typeof(IGuild) == type)
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
			var botFolder = String.Format("{0}_{1}", Constants.SERVER_FOLDER, Variables.BotID);

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
			var first = inputList?.Where(x => CaseInsEquals(x.Substring(0, Math.Max(x.IndexOf(':'), 1)), searchTerm)).FirstOrDefault();
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
			var first = inputArray?.Where(x => CaseInsEquals(x.Substring(0, Math.Max(x.IndexOf(':'), 1)), searchTerm)).FirstOrDefault();
			return first?.Substring(first.IndexOf(':') + 1);
		}

		public static string GetVariable(string inputString, string searchTerm)
		{
			var input = inputString?.Substring(0, Math.Max(inputString.IndexOf(':'), 1));
			return (inputString != null && CaseInsEquals(input, searchTerm) ? inputString.Substring(inputString.IndexOf(':') + 1) : null);
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
			return String.Format("**Uptime:** {0}:{1}:{2}:{3}", span.Days, span.Hours.ToString("00"), span.Minutes.ToString("00"), span.Seconds.ToString("00"));
		}

		public static bool GetIfValidUnicode(string str, int upperLimit)
		{
			if (String.IsNullOrWhiteSpace(str))
				return false;

			foreach (var c in str.ToCharArray())
			{
				if (c > upperLimit)
					return false;
			}
			return true;
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
			return CaseInsEquals(str, Constants.BYPASS_STRING);
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

		public static ulong GetUlong(string inputString)
		{
			return ulong.TryParse(inputString, out ulong number) ? number : 0;
		}

		public static uint GetBit(ICommandContext context, string permission, uint changeValue)
		{
			try
			{
				var bit = Variables.GuildPermissions.FirstOrDefault(x => CaseInsEquals(x.Name, permission)).Position;
				changeValue |= (1U << bit);
			}
			catch (Exception e)
			{
				ExceptionToConsole(e);
			}
			return changeValue;
		}

		public static int GetInteger(string inputString)
		{
			return Int32.TryParse(inputString, out int number) ? number : -1;
		}

		public static int GetMaxNumOfUsersToGather(ICommandContext context, IEnumerable<string> inputArray)
		{
			var ownerID = ((ulong)Variables.BotInfo.GetSetting(SettingOnBot.BotOwnerID));
			var gatherCount = ((int)Variables.BotInfo.GetSetting(SettingOnBot.MaxUserGatherCount));

			if (inputArray.CaseInsContains(Constants.BYPASS_STRING) && context.User.Id == ownerID)
			{
				return int.MaxValue;
			}
			else
			{
				return gatherCount;
			}
		}

		public static int GetLineBreaks(string input)
		{
			return input.Count(y => y == '\n' || y == '\r');
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

		public static int GetMinFromMultipleNumbers(params int[] nums)
		{
			var min = int.MaxValue;
			foreach (var num in nums)
			{
				min = Math.Min(min, num);
			}
			return min;
		}

		public static int GetMaxFromMultipleNumbers(params int[] nums)
		{
			var max = int.MinValue;
			foreach (var num in nums)
			{
				max = Math.Max(max, num);
			}
			return max;
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

			//Get all the channels that aren't the input channel
			var channels = CaseInsEquals(GetChannelType(channel), Constants.TEXT_TYPE)
				? (await channel.Guild.GetTextChannelsAsync()).Where(x => x != channel).OrderBy(x => x.Position).Cast<IGuildChannel>().ToList()
				: (await channel.Guild.GetVoiceChannelsAsync()).Where(x => x != channel).OrderBy(x => x.Position).Cast<IGuildChannel>().ToList();
			//Add the input channel into the given spot
			channels.Insert(Math.Max(Math.Min(channels.Count(), position), 0), channel);
			//Convert into reorder properties and use to reorder
			await channel.Guild.ReorderChannelsAsync(channels.Select(x => new ReorderChannelProperties(x.Id, channels.IndexOf(x))));
		}

		public static ReturnedDiscordObject<IGuildChannel> GetChannel(ICommandContext context, ChannelCheck[] checkingTypes, bool mentions, string input)
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
					var channels = (context.Guild as SocketGuild).VoiceChannels.Where(x => CaseInsEquals(x.Name, input));
					if (channels.Count() == 1)
					{
						channel = channels.First();
					}
					else if (channels.Count() > 1)
					{
						return new ReturnedDiscordObject<IGuildChannel>(channel, FailureReason.TooMany);
					}
				}
			}

			if (channel == null)
			{
				if (mentions)
				{
					var channelMentions = context.Message.MentionedChannelIds;
					if (channelMentions.Count() == 1)
					{
						channel = GetChannel(context.Guild, channelMentions.First());
					}
					else if (channelMentions.Count() > 1)
					{
						return new ReturnedDiscordObject<IGuildChannel>(channel, FailureReason.TooMany);
					}
				}
			}

			return GetChannel(context, checkingTypes, channel);
		}

		public static ReturnedDiscordObject<IGuildChannel> GetChannel(ICommandContext context, ChannelCheck[] checkingTypes, ulong inputID)
		{
			IGuildChannel channel = GetChannel(context.Guild, inputID);
			if (channel == null)
			{
				return new ReturnedDiscordObject<IGuildChannel>(channel, FailureReason.NotFound);
			}

			return GetChannel(context, checkingTypes, channel);
		}

		public static ReturnedDiscordObject<IGuildChannel> GetChannel(ICommandContext context, ChannelCheck[] checkingTypes, IGuildChannel channel)
		{
			if (channel == null)
			{
				return new ReturnedDiscordObject<IGuildChannel>(channel, FailureReason.NotFound);
			}

			var bot = GetBot(context.Guild);
			var user = context.User as IGuildUser;
			foreach (var type in checkingTypes)
			{
				if (!GetIfUserCanDoActionOnChannel(context, channel, user, type))
				{
					return new ReturnedDiscordObject<IGuildChannel>(channel, FailureReason.UserInability);
				}
				else if (!GetIfUserCanDoActionOnChannel(context, channel, bot, type))
				{
					return new ReturnedDiscordObject<IGuildChannel>(channel, FailureReason.BotInability);
				}
				else if (!GetIfChannelIsCorrectType(context, channel, user, type))
				{
					return new ReturnedDiscordObject<IGuildChannel>(channel, FailureReason.IncorrectChannelType);
				}
			}

			return new ReturnedDiscordObject<IGuildChannel>(channel, FailureReason.NotFailure);
		}

		public static EditableDiscordObject<IGuildChannel>? GetValidEditChannels(ICommandContext context)
		{
			//Gather the users
			var input = context.Message.MentionedChannelIds.ToList();
			var success = new List<IGuildChannel>();
			var failure = new List<string>();
			if (!input.Any())
			{
				return null;
			}
			else
			{
				var bot = GetBot(context.Guild);
				var user = context.User as IGuildUser;
				input.ForEach(x =>
				{
					var channel = GetChannel(context.Guild, x);
					if (GetIfUserCanDoActionOnChannel(context, channel, user, ChannelCheck.CanModifyPermissions) && GetIfUserCanDoActionOnChannel(context, channel, bot, ChannelCheck.CanModifyPermissions))
					{
						success.Add(channel);
					}
					else
					{
						failure.Add(channel.FormatChannel());
					}
				});
			}
			return new EditableDiscordObject<IGuildChannel>(success, failure);
		}

		public static IGuildChannel GetChannel(IGuild guild, ulong ID)
		{
			return (guild as SocketGuild).GetChannel(ID);
		}

		public static bool GetIfUserCanDoActionOnChannel(ICommandContext context, IGuildChannel target, IGuildUser user, ChannelCheck type)
		{
			if (target == null || user == null)
				return false;

			var channelPerms = user.GetPermissions(target);
			var guildPerms = user.GuildPermissions;
			if (target is ITextChannel)
			{
				switch (type)
				{
					case ChannelCheck.CanBeManaged:
					{
						return channelPerms.ReadMessages && channelPerms.ManageChannel;
					}
					case ChannelCheck.CanModifyPermissions:
					{
						return channelPerms.ReadMessages && channelPerms.ManageChannel && channelPerms.ManagePermissions;
					}
					case ChannelCheck.CanBeReordered:
					{
						return channelPerms.ReadMessages && guildPerms.ManageChannels;
					}
					case ChannelCheck.CanDeleteMessages:
					{
						return channelPerms.ReadMessages && channelPerms.ManageMessages;
					}
				}
			}
			else
			{
				switch (type)
				{
					case ChannelCheck.CanBeManaged:
					{
						return channelPerms.ManageChannel;
					}
					case ChannelCheck.CanModifyPermissions:
					{
						return channelPerms.ManageChannel && channelPerms.ManagePermissions;
					}
					case ChannelCheck.CanBeReordered:
					{
						return guildPerms.ManageChannels;
					}
					case ChannelCheck.CanMoveUsers:
					{
						return channelPerms.MoveMembers;
					}
				}
			}
			return true;
		}

		public static bool GetIfChannelIsCorrectType(ICommandContext context, IGuildChannel target, IGuildUser user, ChannelCheck type)
		{
			switch (type)
			{
				case ChannelCheck.IsText:
				{
					return GetChannelType(target) == Constants.TEXT_TYPE;
				}
				case ChannelCheck.IsVoice:
				{
					return GetChannelType(target) == Constants.VOICE_TYPE;
				}
			}
			return true;
		}
		#endregion

		#region Roles
		public static async Task<IRole> GetMuteRole(ICommandContext context, BotGuildInfo guildInfo)
		{
			var returnedMuteRole = GetRole(context, new[] { RoleCheck.CanBeEdited, RoleCheck.IsManaged }, ((DiscordObjectWithID<IRole>)guildInfo.GetSetting(SettingOnGuild.MuteRole))?.Object);
			var muteRole = returnedMuteRole.Object;
			if (returnedMuteRole.Reason != FailureReason.NotFailure)
			{
				muteRole = await CreateMuteRoleIfNotFound(guildInfo, context.Guild, muteRole);
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

		public static async Task ModifyRolePosition(IRole role, int position)
		{
			if (role == null)
				return;

			//Get all the roles that aren't the input role
			var roles = role.Guild.Roles.Where(x => x != role).OrderBy(x => x.Position).ToList();
			//Add in the input role into the given spot
			roles.Insert(Math.Max(Math.Min(roles.Count(), position), 0), role);
			//Convert into reorder properties and use to reorder
			await role.Guild.ReorderRolesAsync(roles.Select(x => new ReorderRoleProperties(x.Id, roles.IndexOf(x))));
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

		public static ReturnedDiscordObject<IRole> GetRole(ICommandContext context, RoleCheck[] checkingTypes, bool mentions, string input)
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
					var roles = context.Guild.Roles.Where(x => CaseInsEquals(x.Name, input));
					if (roles.Count() == 1)
					{
						role = roles.First();
					}
					else if (roles.Count() > 1)
					{
						return new ReturnedDiscordObject<IRole>(role, FailureReason.TooMany);
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
						return new ReturnedDiscordObject<IRole>(role, FailureReason.TooMany);
					}
				}
			}

			return GetRole(context, checkingTypes, role);
		}

		public static ReturnedDiscordObject<IRole> GetRole(ICommandContext context, RoleCheck[] checkingTypes, ulong inputID)
		{
			IRole role = GetRole(context.Guild, inputID);
			if (role == null)
			{
				return new ReturnedDiscordObject<IRole>(role, FailureReason.NotFound);
			}

			return GetRole(context, checkingTypes, role);
		}

		public static ReturnedDiscordObject<IRole> GetRole(ICommandContext context, RoleCheck[] checkingTypes, IRole role)
		{
			if (role == null)
			{
				return new ReturnedDiscordObject<IRole>(role, FailureReason.NotFound);
			}

			var bot = GetBot(context.Guild);
			var user = context.User as IGuildUser;
			foreach (var type in checkingTypes)
			{
				if (!GetIfUserCanDoActionOnRole(context, role, user, type))
				{
					return new ReturnedDiscordObject<IRole>(role, FailureReason.UserInability);
				}
				else if (!GetIfUserCanDoActionOnRole(context, role, bot, type))
				{
					return new ReturnedDiscordObject<IRole>(role, FailureReason.BotInability);
				}

				switch (type)
				{
					case RoleCheck.IsEveryone:
					{
						if (context.Guild.EveryoneRole.Id == role.Id)
						{
							return new ReturnedDiscordObject<IRole>(role, FailureReason.EveryoneRole);
						}
						break;
					}
					case RoleCheck.IsManaged:
					{
						if (role.IsManaged)
						{
							return new ReturnedDiscordObject<IRole>(role, FailureReason.ManagedRole);
						}
						break;
					}
				}
			}

			return new ReturnedDiscordObject<IRole>(role, FailureReason.NotFailure);
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
					var returnedRole = GetRole(context, new[] { RoleCheck.CanBeEdited, RoleCheck.IsEveryone, RoleCheck.IsManaged }, false, x);
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

		public static bool GetIfUserCanDoActionOnRole(ICommandContext context, IRole target, IGuildUser user, RoleCheck type)
		{
			if (target == null || user == null)
				return false;

			switch (type)
			{
				case RoleCheck.CanBeEdited:
				{
					return target.Position < GetUserPosition(context.Guild, user);
				}
			}
			return true;
		}
		#endregion

		#region Users
		public static async Task<List<IGuildUser>> GetUsers(ICommandContext context)
		{
			return (await context.Guild.GetUsersAsync()).ToList();
		}

		public static async Task<List<IGuildUser>> GetUsersTheBotAndUserCanEdit(ICommandContext context, Func<IGuildUser, bool> predicate = null)
		{
			var users = await GetUsers(context);
			if (predicate != null)
			{
				users = users.Where(predicate).ToList();
			}

			return users.Where(x => GetIfUserCanBeModifiedByUser(context, x) && GetIfUserCanBeModifiedByBot(context, x)).ToList();
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

		public static ReturnedDiscordObject<IGuildUser> GetGuildUser(ICommandContext context, UserCheck[] checkingTypes, bool mentions, string input)
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
					var users = (context.Guild as SocketGuild).Users.Where(x => CaseInsEquals(x.Username, input));
					if (users.Count() == 1)
					{
						user = users.First();
					}
					else if (users.Count() > 1)
					{
						return new ReturnedDiscordObject<IGuildUser>(user, FailureReason.TooMany);
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
						return new ReturnedDiscordObject<IGuildUser>(user, FailureReason.TooMany);
					}
				}
			}

			return GetGuildUser(context, checkingTypes, user);
		}

		public static ReturnedDiscordObject<IGuildUser> GetGuildUser(ICommandContext context, UserCheck[] checkingTypes, ulong inputID)
		{
			var user = GetGuildUser(context.Guild, inputID);
			return GetGuildUser(context, checkingTypes, user);
		}

		public static ReturnedDiscordObject<IGuildUser> GetGuildUser(ICommandContext context, UserCheck[] checkingTypes, IGuildUser user)
		{
			if (user == null)
			{
				return new ReturnedDiscordObject<IGuildUser>(user, FailureReason.NotFound);
			}

			var bot = GetBot(context.Guild);
			var currUser = context.User as IGuildUser;
			foreach (var type in checkingTypes)
			{
				if (!GetIfUserCanDoActionOnUser(context, user, currUser, type))
				{
					return new ReturnedDiscordObject<IGuildUser>(user, FailureReason.UserInability);
				}
				else if (!GetIfUserCanDoActionOnUser(context, user, bot, type))
				{
					return new ReturnedDiscordObject<IGuildUser>(user, FailureReason.BotInability);
				}
			}

			return new ReturnedDiscordObject<IGuildUser>(user, FailureReason.NotFailure);
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
					var user = GetGuildUser(context.Guild, x);
					if (GetIfUserCanBeModifiedByUser(context, user) && GetIfUserCanBeModifiedByBot(context, user))
					{
						success.Add(user);
					}
					else
					{
						failure.Add(user.FormatUser());
					}
				});
			}
			return new EditableDiscordObject<IGuildUser>(success, failure);
		}

		public static ReturnedBannedUser GetBannedUser(ICommandContext context, List<IBan> bans, string username, string discriminator, string userID)
		{
			if (!String.IsNullOrWhiteSpace(userID))
			{
				if (ulong.TryParse(userID, out ulong inputUserID))
				{
					return new ReturnedBannedUser(bans.FirstOrDefault(x => x.User.Id == inputUserID), BannedUserFailureReason.NotFailure);
				}
				else
				{
					return new ReturnedBannedUser(null, BannedUserFailureReason.InvalidID);
				}
			}
			else if (!String.IsNullOrWhiteSpace(username))
			{
				//Find users with the given username then the given discriminator if provided
				var users = bans.Where(x => CaseInsEquals(x.User.Username, username)).ToList();
				if (!String.IsNullOrWhiteSpace(discriminator))
				{
					if (ushort.TryParse(discriminator, out ushort disc))
					{
						users = users.Where(x => x.User.Discriminator.Equals(disc)).ToList();
					}
					else
					{
						return new ReturnedBannedUser(null, BannedUserFailureReason.InvalidDiscriminator);
					}
				}

				//Return a message saying if there are multiple users
				if (users.Count == 0)
				{
					return new ReturnedBannedUser(null, BannedUserFailureReason.NoMatch);
				}
				else if (users.Count == 1)
				{
					return new ReturnedBannedUser(users.First(), BannedUserFailureReason.NotFailure);
				}
				else
				{
					return new ReturnedBannedUser(null, BannedUserFailureReason.TooManyMatches, users);
				}
			}
			else
			{
				return new ReturnedBannedUser(null, BannedUserFailureReason.NoUsernameOrID);
			}
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

		public static bool GetIfUserCanDoActionOnUser(ICommandContext context, IGuildUser target, IGuildUser user, UserCheck type)
		{
			if (target == null || user == null)
				return false;

			switch (type)
			{
				case UserCheck.CanBeMovedFromChannel:
				{
					var channel = target.VoiceChannel;
					return GetIfUserCanDoActionOnChannel(context, channel, user, ChannelCheck.CanMoveUsers);
				}
				case UserCheck.CanBeEdited:
				{
					return GetIfUserCanBeModifiedByUser(context, target) || GetIfUserCanBeModifiedByBot(context, user);
				}
			}
			return true;
		}

		public static bool GetIfUserCanBeModifiedByUser(ICommandContext context, IGuildUser user)
		{
			var bannerPosition = GetUserPosition(context.Guild, context.User);
			var banneePosition = GetUserPosition(context.Guild, user);
			return bannerPosition > banneePosition;
		}

		public static bool GetIfUserCanBeModifiedByBot(ICommandContext context, IGuildUser user)
		{
			var bot = GetBot(context.Guild);
			var botPosition = GetUserPosition(context.Guild, bot);
			var userPosition = GetUserPosition(context.Guild, user);
			return botPosition > userPosition || user.Id == bot.Id;
		}

		public static int GetUserPosition(IGuild guild, IUser user)
		{
			//Make sure they're a SocketGuildUser
			var tempUser = user as SocketGuildUser;
			if (user == null)
				return -1;

			return tempUser.Hierarchy;
		}
		#endregion

		#region Emotes
		public static ReturnedDiscordObject<Emote> GetEmote(ICommandContext context, bool usage, string input)
		{
			Emote emote = null;
			if (!String.IsNullOrWhiteSpace(input))
			{
				if (Emote.TryParse(input, out emote))
				{
					return new ReturnedDiscordObject<Emote>(emote, FailureReason.NotFailure);
				}
				else if (ulong.TryParse(input, out ulong emoteID))
				{
					emote = context.Guild.Emotes.FirstOrDefault(x => x.Id == emoteID);
				}
				else
				{
					var emotes = context.Guild.Emotes.Where(x => CaseInsEquals(x.Name, input));
					if (emotes.Count() == 1)
					{
						emote = emotes.First();
					}
					else if (emotes.Count() > 1)
					{
						return new ReturnedDiscordObject<Emote>(emote, FailureReason.TooMany);
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
						return new ReturnedDiscordObject<Emote>(emote, FailureReason.TooMany);
					}
				}
			}

			return new ReturnedDiscordObject<Emote>(emote, FailureReason.NotFailure);
		}
		#endregion

		#region Messages
		public static async Task SendEmbedMessage(IMessageChannel channel, EmbedBuilder embed, string content = null)
		{
			var guild = GetGuild(channel);
			if (guild == null)
				return;
			var guildInfo = await CreateOrGetGuildInfo(guild);

			//Descriptions can only be 2048 characters max and mobile can only show up to 20 line breaks
			var description = embed.Description;
			var badDesc = false;
			if (!String.IsNullOrWhiteSpace(description))
			{
				if (description.Length > Constants.MAX_EMBED_LENGTH_LONG)
				{
					embed.WithDescription(String.Format("The description is over `{0}` characters and will be sent as a text file instead.", Constants.MAX_EMBED_LENGTH_LONG));
					badDesc = true;
				}
				else if (GetLineBreaks(description) > Constants.MAX_DESCRIPTION_LINES)
				{
					embed.WithDescription(String.Format("The description is over `{0}` lines and will be sent as a text file instead.", Constants.MAX_DESCRIPTION_LINES));
					badDesc = true;
				}
			}

			//Embeds can only be 1024 characters max and mobile can only show up to 5 line breaks
			var fields = embed.Fields;
			var badFields = new List<Tuple<int, string>>();
			for (int i = 0; i < fields.Count; i++)
			{
				var field = fields[i];
				var val = field.Value.ToString();
				if (!String.IsNullOrWhiteSpace(val))
				{
					if (val.Length > Constants.MAX_EMBED_LENGTH_SHORT)
					{
						field.WithValue(String.Format("This field is over `{0}` characters and will be sent as a text file instead.", Constants.MAX_EMBED_LENGTH_SHORT));
						badFields.Add(new Tuple<int, string>(i, val));
					}
					else if (GetLineBreaks(val) > Constants.MAX_FIELD_LINES)
					{
						field.WithValue(String.Format("This field is over `{0}` lines and will be sent as a text file instead.", Constants.MAX_FIELD_LINES));
						badFields.Add(new Tuple<int, string>(i, val));
					}
				}
			}

			try
			{
				await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + (content ?? ""), false, embed.WithCurrentTimestamp());
			}
			catch (Exception e)
			{
				ExceptionToConsole(e);
				await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + ERROR(e.Message));
			}

			//Go send the description/fields that had an error
			if (badDesc)
			{
				await WriteAndUploadTextFile(guild, channel, description, "Description_");
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

			message = CaseInsReplace(message, guild.EveryoneRole.Mention, Constants.FAKE_EVERYONE);
			message = CaseInsReplace(message, "@everyone", Constants.FAKE_EVERYONE);
			message = CaseInsReplace(message, "\tts", Constants.FAKE_TTS);

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

		public static async Task<int> RemoveMessages(IMessageChannel channel, int requestCount)
		{
			var guildChannel = channel as ITextChannel;
			if (guildChannel == null)
				return 0;

			var msg = (await channel.GetMessagesAsync(1).ToList()).SelectMany(x => x).FirstOrDefault();
			if (msg == null)
				return 0;

			var deletedCount = 0;
			while (requestCount > 0)
			{
				//Get the current messages and ones that aren't null
				var newNum = Math.Min(requestCount, 100);
				var messages = (await channel.GetMessagesAsync(msg, Direction.Before, newNum).ToList()).SelectMany(x => x);
				var msgAmt = messages.Count();
				if (msgAmt == 0)
					break;

				//Set the from message as the last of the currently grabbed ones
				msg = messages.Last();

				//Delete them in a try catch due to potential errors
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

				//Leave if the message count gathered implies that the channel is out of messages
				if (msgAmt < newNum)
					break;

				//Lower the request count
				requestCount -= msgAmt;
			}
			return deletedCount;
		}

		public static async Task<int> RemoveMessages(IMessageChannel channel, IUser user, int requestCount)
		{
			var guildChannel = channel as ITextChannel;
			if (guildChannel == null)
				return 0;

			if (user == null)
			{
				return await RemoveMessages(channel, requestCount);
			}

			var msg = (await channel.GetMessagesAsync(1).ToList()).SelectMany(x => x).FirstOrDefault();
			if (msg == null)
				return 0;

			var deletedCount = 0;
			while (requestCount > 0)
			{
				//Get the current messages and ones that aren't null
				var messages = (await channel.GetMessagesAsync(msg, Direction.Before, 100).ToList()).SelectMany(x => x);
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

				//Lower the request count
				requestCount -= msgAmt;
			}
			return deletedCount;
		}

		public static async Task<List<IMessage>> GetMessages(IMessageChannel channel, int requestCount)
		{
			var list = new List<IMessage>();

			var msg = (await channel.GetMessagesAsync(1).ToList()).SelectMany(x => x).FirstOrDefault();
			if (msg == null)
			{
				return list;
			}

			while (requestCount > 0)
			{
				var newNum = Math.Min(requestCount, 100);
				var messages = (await channel.GetMessagesAsync(msg, Direction.Before, newNum).ToList()).SelectMany(x => x);
				var msgAmt = messages.Count();
				if (msgAmt == 0)
				{
					break;
				}

				msg = messages.Last();
				list.AddRange(messages);

				//Leave if the message count gathered implies that the channel is out of messages
				if (msgAmt < newNum)
					break;

				requestCount -= msgAmt;
			}
			return list;
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
			if (guildChannel == null)
				return;
			if (messages == null || !messages.Any())
				return;

			//Delete them in a try catch due to potential errors
			try
			{
				await channel.DeleteMessagesAsync(messages.Where(x => DateTime.UtcNow.Subtract(x.CreatedAt.UtcDateTime).TotalDays < 14).Distinct());
			}
			catch
			{
				WriteLine(String.Format("Unable to delete {0} messages on the guild {1} on channel {2}.", messages.Count(), guildChannel.Guild.FormatGuild(), guildChannel.FormatChannel()));
			}
		}

		public static async Task DeleteMessage(IMessage message)
		{
			if (message == null || DateTime.UtcNow.Subtract(message.CreatedAt.UtcDateTime).TotalDays >= 14)
				return;
			var guildChannel = message.Channel as ITextChannel;
			if (guildChannel == null)
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
			content = CaseInsReplace(content, "{UserMention}", user != null ? user.Mention : "Invalid User");
			content = CaseInsReplace(content, "{User}", user != null ? user.FormatUser() : "Invalid User");
			//Put a zero length character in between invite links for names so the invite links will no longer embed
			content = CaseInsReplace(content, "discord.gg", String.Format("discord{0}.gg", Constants.ZERO_LENGTH_CHAR));

			if (notification.Embed != null)
			{
				await SendEmbedMessage(notification.Channel, notification.Embed, content);
			}
			else
			{
				await SendChannelMessage(notification.Channel, content);
			}
		}

		public static async Task HandleObjectGettingErrors<T>(ICommandContext context, ReturnedDiscordObject<T> returnedObject)
		{
			var objType = "";
			if (returnedObject.Object == null)
			{
				objType = GetObjectStringBasic(typeof(T));
			}
			else
			{
				objType = GetObjectStringBasic((dynamic)returnedObject.Object);
			}
			switch (returnedObject.Reason)
			{
				case FailureReason.NotFound:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("Unable to find the {0}.", objType)));
					return;
				}
				case FailureReason.UserInability:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("You are unable to make the given changes to the {0}: `{1}`.", objType, FormatObject((dynamic)returnedObject.Object))));
					return;
				}
				case FailureReason.BotInability:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("I am unable to make the given changes to the {0}: `{1}`.", objType, FormatObject((dynamic)returnedObject.Object))));
					return;
				}
				case FailureReason.TooMany:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("There are too many {0}s with the same name.", objType)));
					return;
				}
				case FailureReason.IncorrectChannelType:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("Invalid channel type for the given variable requirement.")));
					return;
				}
				case FailureReason.EveryoneRole:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("The everyone role cannot be modified in that way.")));
					return;
				}
				case FailureReason.ManagedRole:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("Managed roles cannot be modified in that way.")));
					return;
				}
			}
		}

		public static async Task HandleArgsGettingErrors(ICommandContext context, ReturnedArguments returnedArgs)
		{
			switch (returnedArgs.Reason)
			{
				case ArgFailureReason.TooManyArgs:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("Too many arguments."));
					return;
				}
				case ArgFailureReason.TooFewArgs:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("Too few arguments."));
					return;
				}
				case ArgFailureReason.MissingCriticalArgs:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("Missing critical arguments."));
					return;
				}
				case ArgFailureReason.MaxLessThanMin:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("NOT USER ERROR: Max less than min."));
					return;
				}
			}
		}

		public static async Task HandleTypeGettingErrors<T>(ICommandContext context, ReturnedType<T> returnedType)
		{
			switch (returnedType.Reason)
			{
				case TypeFailureReason.NotFound:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("Unable to find the type for the given input."));
					return;
				}
				case TypeFailureReason.InvalidType:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("The type `{0}` is not accepted in this instance.", returnedType.Type)));
					return;
				}
			}
		}

		public static async Task HandleBannedUserErrors(ICommandContext context, ReturnedBannedUser returnedBannedUser)
		{
			switch (returnedBannedUser.Reason)
			{
				case BannedUserFailureReason.NoBans:
				{
					await MakeAndDeleteSecondaryMessage(context, "The guild has no bans.");
					return;
				}
				case BannedUserFailureReason.NoMatch:
				{
					await MakeAndDeleteSecondaryMessage(context, "No ban was found which matched the given criteria.");
					return;
				}
				case BannedUserFailureReason.TooManyMatches:
				{
					var msg = String.Join("`, `", returnedBannedUser.MatchedBans.Select(x => x.User.FormatUser()));
					await SendChannelMessage(context, String.Format("The following users have that name: `{0}`.", msg));
					return;
				}
				case BannedUserFailureReason.InvalidDiscriminator:
				{
					await MakeAndDeleteSecondaryMessage(context, "The given discriminator is invalid.");
					return;
				}
				case BannedUserFailureReason.InvalidID:
				{
					await MakeAndDeleteSecondaryMessage(context, "The given ID is invalid.");
					return;
				}
				case BannedUserFailureReason.NoUsernameOrID:
				{
					await MakeAndDeleteSecondaryMessage(context, "A username or ID must be provided");
					return;
				}
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
			return await GetMessages(channel, 500);
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

		public static EmbedBuilder AddAuthor(EmbedBuilder embed, string name = null, string iconURL = null, string URL = null)
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

			return embed.WithAuthor(author);
		}

		public static EmbedBuilder AddFooter(EmbedBuilder embed, [CallerMemberName] string text = null, string iconURL = null)
		{
			//Make the footer builder
			var footer = new EmbedFooterBuilder();

			//Verify the URL
			iconURL = ValidateURL(iconURL) ? iconURL : null;

			//Add in the properties
			if (text != null)
			{
				footer.WithText(text.Substring(0, Math.Min(Constants.MAX_EMBED_LENGTH_LONG, text.Length)));
			}
			if (iconURL != null)
			{
				footer.WithIconUrl(iconURL);
			}

			return embed.WithFooter(footer);
		}

		public static EmbedBuilder AddField(EmbedBuilder embed, string name, string value, bool isInline = true)
		{
			if (embed.Build().Fields.Count() >= Constants.MAX_FIELDS)
				return embed;

			//Get the name and value
			name = String.IsNullOrWhiteSpace(name) ? "Placeholder" : name.Substring(0, Math.Min(Constants.MAX_TITLE_LENGTH, name.Length));
			value = String.IsNullOrWhiteSpace(name) ? "Placeholder" : value.Substring(0, Math.Min(Constants.MAX_LENGTH_FOR_FIELD_VALUE, value.Length));

			embed.AddField(x =>
			{
				x.Name = name;
				x.Value = value;
				x.IsInline = isInline;
			});

			return embed;
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

		public static EmbedBuilder FormatEmoteInfo(BotGuildInfo guildInfo, Emote emote)
		{
			//Try to find the emoji if global
			var guilds = Variables.Client.GetGuilds().Where(x =>
			{
				var placeholder = x.Emotes.FirstOrDefault(y => y.Id == emote.Id);
				if (placeholder == null)
				{
					return false;
				}
				return placeholder.IsManaged && placeholder.RequireColons;
			});

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
			var online = String.Format("**Online Since:** {0}", Variables.StartupTime);
			var uptime = GetUptime();
			var guildCount = String.Format("**Guild Count:** {0}", Variables.TotalGuilds);
			var memberCount = String.Format("**Cumulative Member Count:** {0}", Variables.TotalUsers);
			var currShard = String.Format("**Current Shard:** {0}", Variables.Client.GetShardFor(guild).ShardId);
			var description = String.Join("\n", new[] { online, uptime, guildCount, memberCount, currShard });

			var embed = MakeNewEmbed(null, description);
			AddAuthor(embed, Variables.BotName, Variables.Client.GetCurrentUser().GetAvatarUrl());
			AddFooter(embed, "Version " + Constants.BOT_VERSION);

			var firstField = FormatLoggedThings();
			AddField(embed, "Logged Actions", firstField);

			var attempt = String.Format("**Attempted Commands:** {0}", Variables.AttemptedCommands);
			var successful = String.Format("**Successful Commands:** {0}", Variables.AttemptedCommands - Variables.FailedCommands);
			var failed = String.Format("**Failed Commands:** {0}", Variables.FailedCommands);
			var secondField = String.Join("\n", new[] { attempt, successful, failed });
			AddField(embed, "Commands", secondField);

			var latency = String.Format("**Latency:** {0}ms", Variables.Client.GetLatency());
			var memory = String.Format("**Memory Usage:** {0}MB", GetMemory().ToString("0.00"));
			var threads = String.Format("**Thread Count:** {0}", System.Diagnostics.Process.GetCurrentProcess().Threads.Count);
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

				return String.Format("`{0}` **IN** `{1}` **SENT AT** `[{2}]`\n```\n{3}```",
					msg.Author.FormatUser(),
					msg.Channel.FormatChannel(),
					msg.CreatedAt.ToString("HH:mm:ss"),
					ReplaceMarkdownChars(content + "\n" + formattedDescriptions, true));
			}
			else if (msg.Attachments.Any())
			{
				return String.Format("`{0}` **IN** `{1}` **AT** `[{2}]`\n```\n{3}```",
					msg.Author.FormatUser(),
					msg.Channel.FormatChannel(),
					msg.CreatedAt.ToString("HH:mm:ss"),
					ReplaceMarkdownChars(content + " + " + String.Join(" + ", msg.Attachments.Select(y => y.Filename)), true));
			}
			else
			{
				return String.Format("`{0}` **IN** `{1}` **AT** `[{2}]`\n```\n{3}```",
					msg.Author.FormatUser(),
					msg.Channel.FormatChannel(),
					msg.CreatedAt.ToString("HH:mm:ss"),
					ReplaceMarkdownChars(content, true));
			}
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

		public static string ERROR(string message)
		{
			++Variables.FailedCommands;

			return Constants.ZERO_LENGTH_CHAR + Constants.ERROR_MESSAGE + message;
		}
		
		public static string ReplaceMarkdownChars(string input, bool replaceNewLines)
		{
			if (String.IsNullOrWhiteSpace(input))
				return "";

			while (replaceNewLines && input.Contains("\n\n"))
			{
				input = input.Replace("\n\n", "\n");
			}

			return new Regex("[*`]", RegexOptions.Compiled).Replace(input, "");
		}

		public static string ConcatStringsWithNewLinesBetween(params string[] strs)
		{
			return String.Join("\n", strs.Where(x => !String.IsNullOrWhiteSpace(x)));
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
			var basePerm = "";
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
				var perms = (attr.Requirements & (1U << (int)Precondition.UserHasAPerm)) != 0;
				var guild = (attr.Requirements & (1U << (int)Precondition.GuildOwner)) != 0;
				var trust = (attr.Requirements & (1U << (int)Precondition.TrustedUser)) != 0;
				var owner = (attr.Requirements & (1U << (int)Precondition.BotOwner)) != 0;

				basePerm = "[";
				if (perms)
				{
					basePerm += "Administrator | Any perm ending with 'Members' | Any perm starting with 'Manage'";
				}
				if (guild)
				{
					if (perms)
					{
						basePerm += " | ";
					}
					basePerm += "Guild Owner";
				}
				if (trust)
				{
					if (perms || guild)
					{
						basePerm += " | ";
					}
					basePerm += "Trusted User";
				}
				if (owner)
				{
					if (perms || guild || trust)
					{
						basePerm += " | ";
					}
					basePerm += "Bot Owner";
				}
				basePerm += "]";
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
					str += String.Format("**{0}**:\n{1}\n\n", Enum.GetName(typeof(SettingOnBot), e), formatted);
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
					str += String.Format("**{0}**:\n{1}\n\n", Enum.GetName(typeof(SettingOnGuild), e), formatted);
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
			//Make sure the guild exists
			if (guild == null)
				return null;
			//Get the invites
			var invs = await guild.GetInvitesAsync();
			//If no invites return null
			return invs.Any() ? invs : null;
		}

		public static async Task<BotInvite> GetInviteUserJoinedOn(BotGuildInfo guildInfo, IGuild guild)
		{
			//Get the current invites
			var curInvs = await GetInvites(guild);
			if (curInvs == null)
				return null;
			//Get the bot's stored invites
			var botInvs = ((List<BotInvite>)guildInfo.GetSetting(SettingOnGuild.Invites));
			if (!botInvs.Any())
				return null;

			//Set an invite to hold the current invite the user joined on
			BotInvite joinInv = null;
			//Find the first invite where the bot invite has the same code as the current invite but different use counts
			joinInv = botInvs.FirstOrDefault(bI => curInvs.Any(cI => cI.Code == bI.Code && cI.Uses != bI.Uses));
			//If the invite is null, take that as meaning there are new invites on the guild
			if (joinInv == null)
			{
				//Get the new invites on the guild by finding which guild invites aren't on the bot invites list
				var botInvCodes = botInvs.Select(y => y.Code);
				var newInvs = curInvs.Where(x => !botInvCodes.Contains(x.Code));
				//If there's only one, then use that as the current inv. If there's more than one then there's no way to know what invite it was on
				if (guild.Features.CaseInsContains(Constants.VANITY_URL) && (!newInvs.Any() || (newInvs.Count() == 1 && newInvs.First().Uses == 0)))
				{
					joinInv = new BotInvite(guild.Id, "Vanity URL", 0);
				}
				else if (newInvs.Count() == 1)
				{
					joinInv = new BotInvite(newInvs.First().GuildId, newInvs.First().Code, newInvs.First().Uses);
				}
				//Add all of the invites to the bot invites list
				botInvs.AddRange(newInvs.Select(x => new BotInvite(x.GuildId, x.Code, x.Uses)).ToList());
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
		public static async Task<IMessage> WriteAndUploadTextFile(IGuild guild, IMessageChannel channel, string text, string fileName, string contentToSayOnTopOfMessage = null)
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

			var msg = await channel.SendFileAsync(path, String.IsNullOrWhiteSpace(contentToSayOnTopOfMessage) ? "" : String.Format("**{0}:**", contentToSayOnTopOfMessage));
			File.Delete(path);
			return msg;
		}

		public static async Task UploadFile(IMessageChannel channel, string path, string text = null)
		{
			await channel.SendFileAsync(path, text);
		}

		public static async Task SetPicture(ICommandContext context, string input, bool user)
		{
			//See if the user wants to remove the icon
			if (CaseInsEquals(input, "remove"))
			{
				if (!user)
				{
					await context.Guild.ModifyAsync(x => x.Icon = new Image());
					await SendChannelMessage(context, "Successfully removed the guild's icon.");
				}
				else
				{
					await context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image());
					await SendChannelMessage(context, "Successfully removed the bot's icon.");
				}
				return;
			}

			//Check if there are even any attachments or embeds
			if (context.Message.Attachments.Count + context.Message.Embeds.Count == 0)
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR("No attached or embedded image."));
				return;
			}
			//Check if there are too many
			else if (context.Message.Attachments.Count + context.Message.Embeds.Count > 1)
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR("Too many attached or embedded images."));
				return;
			}

			//Get the URL of the image
			var imageURL = context.Message.Embeds.Count == 1 ? context.Message.Embeds.First().Thumbnail.ToString() : context.Message.Attachments.First().Url;

			//Run separate due to the time it takes
			Actions.DontWaitForResultOfBigUnimportantFunction(context.Channel, async () =>
			{
				//Check the image's file size first
				var req = HttpWebRequest.Create(imageURL);
				req.Method = "HEAD";
				using (var resp = req.GetResponse())
				{
					if (int.TryParse(resp.Headers.Get("Content-Length"), out int ContentLength))
					{
						//Check if valid content type
						if (!Constants.VALID_IMAGE_EXTENSIONS.Contains("." + resp.Headers.Get("Content-Type").Split('/').Last()))
						{
							await MakeAndDeleteSecondaryMessage(context, ERROR("Image must be a png or jpg."));
							return;
						}
						else
						{
							if (ContentLength > 2500000)
							{
								//Check if bigger than 2.5MB
								await MakeAndDeleteSecondaryMessage(context, ERROR("Image is bigger than 2.5MB. Please manually upload instead."));
								return;
							}
							else if (ContentLength == 0)
							{
								//Check if nothing was gotten
								await MakeAndDeleteSecondaryMessage(context, ERROR("Unable to get the image's file size."));
								return;
							}
						}
					}
					else
					{
						await MakeAndDeleteSecondaryMessage(context, ERROR("Unable to get the image's file size."));
						return;
					}
				}

				//Send a message saying how it's progressing
				var msg = await SendChannelMessage(context, "Attempting to download the file...");

				//Set the name of the file to prevent typos between the three places that use it
				var path = GetServerFilePath(context.Guild.Id, (user ? "boticon" : "guildicon") + Path.GetExtension(imageURL).ToLower());

				//Download the image
				using (var webclient = new WebClient())
				{
					webclient.DownloadFile(imageURL, path);
				}

				//Create a filestream to check the image's size if trying to set a guild icon
				if (!user)
				{
					using (var imgStream = new FileStream(path, FileMode.Open, FileAccess.Read))
					{
						var img = System.Drawing.Image.FromStream(imgStream);
						if (img.Width < 128 || img.Height < 128)
						{
							await MakeAndDeleteSecondaryMessage(context, ERROR("Images must be at least 128x128 pixels."));
							return;
						}
					}
				}

				//Create a second filestream to upload the image
				using (var imgStream = new FileStream(path, FileMode.Open, FileAccess.Read))
				{
					if (!user)
					{
						await context.Guild.ModifyAsync(x => x.Icon = new Image(imgStream));
					}
					else
					{
						await context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(imgStream));
					}
				}

				//Delete the file and send a success message
				File.Delete(path);
				await DeleteMessage(msg);
				await SendChannelMessage(context, String.Format("Successfully changed the {0} icon.", user ? "bot" : "guild"));
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

		public static bool VerifyServerLoggingAction(SocketGuildUser user, LogActions logAction, out VerifiedLoggingAction verifLoggingAction)
		{
			return VerifyServerLoggingAction(user.Guild, logAction, out verifLoggingAction);
		}

		public static bool VerifyServerLoggingAction(ISocketMessageChannel channel, LogActions logAction, out VerifiedLoggingAction verifLoggingAction)
		{
			return VerifyServerLoggingAction(GetGuild(channel) as SocketGuild, logAction, out verifLoggingAction) && !((List<ulong>)verifLoggingAction.GuildInfo.GetSetting(SettingOnGuild.IgnoredLogChannels)).Contains(channel.Id);
		}

		public static bool VerifyServerLoggingAction(SocketGuild guild, LogActions logAction, out VerifiedLoggingAction verifLoggingAction)
		{
			verifLoggingAction = new VerifiedLoggingAction(null, null, null);
			if (Variables.Pause)
				return false;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return false;

			var logChannel = ((DiscordObjectWithID<ITextChannel>)guildInfo.GetSetting(SettingOnGuild.ServerLog)).Object;
			verifLoggingAction = new VerifiedLoggingAction(guild, guildInfo, logChannel);
			return logChannel != null && ((List<LogActions>)guildInfo.GetSetting(SettingOnGuild.LogActions)).Contains(logAction);
		}
		#endregion

		#region Preferences/Settings
		public static async Task<bool> ValidateBotKey(BotClient client, string input, bool startup = false)
		{
			input = input.Trim();

			if (startup)
			{
				//Check if the bot already has a key
				if (!String.IsNullOrWhiteSpace(input))
				{
					try
					{
						await client.LoginAsync(TokenType.Bot, input);
						return true;
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
				return false;
			}

			//Login and connect to Discord.
			if (input.Length > 59)
			{
				//If the length isn't the normal length of a key make it retry
				WriteLine("The given key is too long. Please enter a regular length key:");
			}
			else if (input.Length < 59)
			{
				WriteLine("The given key is too short. Please enter a regular length key:");
			}
			else
			{
				try
				{
					//Try to login with the given key
					await client.LoginAsync(TokenType.Bot, input);

					//If the key works then save it within the settings
					WriteLine("Succesfully logged in via the given bot key.");
					Properties.Settings.Default.BotKey = input;
					Properties.Settings.Default.Save();
					return true;
				}
				catch (Exception)
				{
					//If the key doesn't work then retry
					WriteLine("The given key is invalid. Please enter a valid key:");
				}
			}
			return false;
		}

		public static bool ValidatePath(string input, bool startup = false)
		{
			var path = input.Trim();

			if (startup)
			{
				//Check if a path is already input
				if (!String.IsNullOrWhiteSpace(path) && Directory.Exists(path))
				{
					Properties.Settings.Default.Path = path;
					Properties.Settings.Default.Save();
					return true;
				}

				//Send the initial message
				if (Variables.Windows)
				{
					WriteLine("Please enter a valid directory path in which to save files or say 'AppData':");
				}
				else
				{
					WriteLine("Please enter a valid directory path in which to save files:");
				}

				return false;
			}

			if (Variables.Windows && CaseInsEquals(path, "appdata"))
			{
				path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			}

			if (!Directory.Exists(path))
			{
				WriteLine("Invalid directory. Please enter a valid directory:");
				return false;
			}
			else
			{
				Properties.Settings.Default.Path = path;
				Properties.Settings.Default.Save();
				return true;
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
		public static async Task SpamCheck(BotGuildInfo guildInfo, IGuild guild, IGuildUser author, IMessage msg)
		{
			var spamPrevUsers = ((List<SpamPreventionUser>)guildInfo.GetSetting(SettingOnGuild.SpamPreventionUsers));
			var spamUser = spamPrevUsers.FirstOrDefault(x => x.User.Id == author.Id);
			if (spamUser == null)
			{
				spamUser = new SpamPreventionUser(author);
				spamPrevUsers.ThreadSafeAdd(spamUser);
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

					//Make sure they have the lowest vote count required to kick
					spamUser.ChangeVotesRequired(spamPrev.VotesForKick);
					spamUser.EnablePotentialKick();

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

		public static async Task HandleBannedPhrases(BotGuildInfo guildInfo, IMessage message)
		{
			//TODO: Better bool here
			if (guildInfo == null || (message.Author as IGuildUser).GuildPermissions.Administrator)
				return;

			var phrase = ((List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseStrings)).FirstOrDefault(x =>
			{
				return CaseInsIndexOf(message.Content, x.Phrase);
			});
			if (phrase != null)
			{
				await HandleBannedPhrasePunishments(guildInfo, message, phrase);
			}

			var regex = ((List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseRegex)).FirstOrDefault(x =>
			{
				return Regex.IsMatch(message.Content, x.Phrase, RegexOptions.IgnoreCase, new TimeSpan(Constants.REGEX_TIMEOUT));
			});
			if (regex != null)
			{
				await HandleBannedPhrasePunishments(guildInfo, message, regex);
			}
		}

		public static async Task HandleBannedPhrasePunishments(BotGuildInfo guildInfo, IMessage message, BannedPhrase phrase)
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
					if (GetUserPosition(user.Guild, user) > GetUserPosition(user.Guild, GetBot(user.Guild)))
						return;

					await user.KickAsync();
					bpUser.ResetKickCount();

					if (logChannel != null)
					{
						var embed = AddFooter(MakeNewEmbed(null, "**ID:** " + user.Id, Constants.LEAV), "Banned Phrases Leave");
						await SendEmbedMessage(logChannel, AddAuthor(embed, String.Format("{0} in #{1}", user.FormatUser(), message.Channel), user.GetAvatarUrl()));
					}
					break;
				}
				case PunishmentType.Ban:
				{
					//Check if can ban them
					if (GetUserPosition(user.Guild, user) > GetUserPosition(user.Guild, GetBot(user.Guild)))
						return;

					await user.Guild.AddBanAsync(user);
					bpUser.ResetBanCount();

					if (logChannel != null)
					{
						var embed = AddFooter(MakeNewEmbed(null, "**ID:** " + user.Id, Constants.BANN), "Banned Phrases Ban");
						await SendEmbedMessage(logChannel, AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl()));
					}
					break;
				}
				case PunishmentType.Role:
				{
					await GiveRole(user, punishment.Role);
					bpUser.ResetRoleCount();

					//If a time is specified, run through the time then remove the role
					if (punishment.PunishmentTime != null)
					{
						Variables.PunishedUsers.Add(new RemovablePunishment(((DiscordObjectWithID<SocketGuild>)guildInfo.GetSetting(SettingOnGuild.Guild)).Object, user.Id, punishment.Role, DateTime.UtcNow.AddMinutes((int)punishment.PunishmentTime)));
					}

					if (logChannel != null)
					{
						var embed = AddFooter(MakeNewEmbed(null, "**Role Gained:** " + punishment.Role.Name, Constants.UEDT), "Banned Phrases Role");
						await SendEmbedMessage(logChannel, AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl()));
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
			bannedRegex = ((List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseRegex)).FirstOrDefault(x => CaseInsEquals(x.Phrase, searchPhrase));
			return bannedRegex != null;
		}

		public static bool TryGetBannedString(BotGuildInfo guildInfo, string searchPhrase, out BannedPhrase bannedString)
		{
			bannedString = ((List<BannedPhrase>)guildInfo.GetSetting(SettingOnGuild.BannedPhraseStrings)).FirstOrDefault(x => CaseInsEquals(x.Phrase, searchPhrase));
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
					if (!bannedStrings.Any(x => CaseInsEquals(x.Phrase, str)))
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
		public static List<CloseWord> GetRemindsWithSimilarNames(List<Remind> reminds, string input)
		{
			var closeWords = new List<CloseWord>();
			reminds.ToList().ForEach(x =>
			{
				//Check how close the word is to the input
				var closeness = FindCloseName(x.Name, input);
				//Ignore all closewords greater than a difference of five
				if (closeness > 5)
					return;
				//If no words in the list already, add it
				if (closeWords.Count < 3)
				{
					closeWords.Add(new CloseWord(x.Name, closeness));
				}
				else
				{
					//If three words in the list, check closeness value now
					foreach (var help in closeWords)
					{
						if (closeness < help.Closeness)
						{
							closeWords.Insert(closeWords.IndexOf(help), new CloseWord(x.Name, closeness));
							break;
						}
					}

					//Remove all words that are now after the third item
					closeWords.RemoveRange(3, closeWords.Count - 3);
				}
				closeWords.OrderBy(y => y.Closeness);
			});

			return GetRemindsWithInputInName(closeWords, reminds, input);
		}

		public static List<CloseWord> GetRemindsWithInputInName(List<CloseWord> closeReminds, List<Remind> reminds, string input)
		{
			//Check if any were gotten
			if (!reminds.Any())
				return new List<CloseWord>();

			reminds.ForEach(x =>
			{
				if (closeReminds.Count >= 5)
				{
					return;
				}
				else if (!closeReminds.Any(y => x.Name == y.Name))
				{
					closeReminds.Add(new CloseWord(x.Name, 0));
				}
			});

			//Remove all words that are now after the fifth item
			if (closeReminds.Count >= 5)
			{
				closeReminds.RemoveRange(5, closeReminds.Count - 5);
			}

			return closeReminds;
		}

		public static List<CloseHelp> GetCommandsWithSimilarName(string input)
		{
			var closeHelps = new List<CloseHelp>();
			Variables.HelpList.ForEach(HelpEntry =>
			{
				//Check how close the word is to the input
				var closeness = FindCloseName(HelpEntry.Name, input);
				//Ignore all closewords greater than a difference of five
				if (closeness > 5)
					return;
				//If no words in the list already, add it
				if (closeHelps.Count < 3)
				{
					closeHelps.Add(new CloseHelp(HelpEntry, closeness));
				}
				else
				{
					//If three words in the list, check closeness value now
					foreach (var help in closeHelps)
					{
						if (closeness < help.Closeness)
						{
							closeHelps.Insert(closeHelps.IndexOf(help), new CloseHelp(HelpEntry, closeness));
							break;
						}
					}

					//Remove all words that are now after the third item
					closeHelps.RemoveRange(3, closeHelps.Count - 3);
				}
				closeHelps.OrderBy(y => y.Closeness);
			});

			return GetCommandsWithInputInName(closeHelps, input);
		}

		public static List<CloseHelp> GetCommandsWithInputInName(List<CloseHelp> list, string input)
		{
			//Find commands with the input in their name
			var commands = Variables.HelpList.Where(x => CaseInsIndexOf(x.Name, input)).ToList();

			//Check if any were gotten
			if (!commands.Any())
				return null;

			var closeHelps = new List<CloseHelp>();
			commands.ForEach(x =>
			{
				if (closeHelps.Count >= 5)
				{
					return;
				}
				else if (!closeHelps.Any(y => x.Name == y.Help.Name))
				{
					closeHelps.Add(new CloseHelp(x, 0));
				}
			});

			//Remove all words that are now after the fifth item
			if (closeHelps.Count >= 5)
			{
				closeHelps.RemoveRange(5, closeHelps.Count - 5);
			}

			return closeHelps;
		}

		public static int FindCloseName(string s, string t)
		{
			//Levenshtein Distance
			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			//Step 1
			if (n == 0)
			{
				return m;
			}

			if (m == 0)
			{
				return n;
			}

			//Step 2
			for (int i = 0; i <= n; d[i, 0] = i++)
			{
			}

			for (int j = 0; j <= m; d[0, j] = j++)
			{
			}

			//Step 3
			for (int i = 1; i <= n; i++)
			{
				//Step 4
				for (int j = 1; j <= m; j++)
				{
					//Step 5
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

					//Step 6
					d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
				}
			}
			//Step 7
			return d[n, m];
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

		#region Case Insensitive Searches
		public static bool CaseInsEquals(string str1, string str2)
		{
			if (String.IsNullOrWhiteSpace(str1))
			{
				return String.IsNullOrWhiteSpace(str2) ? true : false;
			}
			else if (str1 == null || str2 == null)
			{
				return false;
			}
			else
			{
				return str1.Equals(str2, StringComparison.OrdinalIgnoreCase);
			}
		}

		public static bool CaseInsIndexOf(string source, string search)
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

		public static bool CaseInsIndexOf(string source, string search, out int position)
		{
			position = -1;
			if (source != null && search != null)
			{
				position = source.IndexOf(search, StringComparison.OrdinalIgnoreCase);
				if (position != -1)
				{
					return true;
				}
			}
			return false;
		}

		public static bool CaseInsStartsWith(string source, string search)
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

		public static bool CaseInsEndsWith(string source, string search)
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

		public static string CaseInsReplace(string str, string oldValue, string newValue)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			int previousIndex = 0;
			int index = str.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
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
				await Variables.Client.SetGameAsync(game, Constants.STREAM_URL + stream, StreamType.Twitch);
			}
		}

		public static FAWRType ClarifyFAWRType(FAWRType type)
		{
			switch (type)
			{
				case FAWRType.GR:
				{
					return FAWRType.GiveRole;
				}
				case FAWRType.TR:
				{
					return FAWRType.TakeRole;
				}
				case FAWRType.GNN:
				{
					return FAWRType.GiveNickname;
				}
				case FAWRType.TNN:
				{
					return FAWRType.TakeNickname;
				}
			}
			return type;
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
				IDisposable typing = null;
				if (channel != null)
				{
					typing = channel.EnterTypingState();
				}

				func.Invoke();
				if (typing != null)
				{
					typing.Dispose();
				}
			}).Forget();
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
			return list.GetRange(0, Math.Max(0, Math.Min(list.Count, Actions.GetMinFromMultipleNumbers(x))));
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
				return String.Format("'{0}#{1}' ({2})", Actions.EscapeMarkdown(user.Username, true), user.Discriminator, user.Id);
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
				if (!Actions.CaseInsEquals(array[i - 1], array[i]))
					return false;
			}
			return true;
		}

		public static bool CaseInsContains(this IEnumerable<string> enumerable, string search)
		{
			if (!enumerable.Any())
			{
				return false;
			}
			else
			{
				return enumerable.Contains(search, StringComparer.OrdinalIgnoreCase);
			}
		}
	}
}
