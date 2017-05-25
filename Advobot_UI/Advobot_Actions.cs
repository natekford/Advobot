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
	public class Actions
	{
		#region Saving and Loading
		public static async Task LoadInformation()
		{
			if (Variables.Loaded)
				return;

			HandleBotID(Variables.Client.GetCurrentUser().Id);						//Give the variable Bot_ID the id of the bot
			Variables.BotName = Variables.Client.GetCurrentUser().Username;			//Give the variable Bot_Name the username of the bot

			LoadPermissionNames();													//Gets the names of the permission bits in Discord
			LoadCommandInformation();												//Gets the information of a command (name, aliases, usage, summary). Has to go after LPN

			await LoadGuilds();														//Loads the guilds that attempted to load before the Bot_ID was gotten.
			await UpdateGame();														//Have the bot display its game and stream

			HourTimer(null);														//Start the hourly timer
			MinuteTimer(null);														//Start the minutely timer
			OneFourthSecondTimer(null);												//Start the one fourth second timer

			StartUpMessages();														//Say all of the start up messages
			Variables.Loaded = true;                                                //Set a bool stating that everything is done loading.
		}

		public static async Task LoadGuilds()
		{
			await Variables.GuildsToBeLoaded.ForEachAsync(async x => await LoadGuild(x));
		}

		public static async Task LoadGuild(IGuild guild)
		{
			var guildInfo = LoadGuildInfo(guild);
			if (guildInfo != null)
			{
				guildInfo.PostDeserialize();
			}
			else
			{
				guildInfo = new BotGuildInfo(guild.Id);
			}

			if (guildInfo.CommandOverrides != null)
			{
				guildInfo.CommandOverrides.Users.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
				guildInfo.CommandOverrides.Roles.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
				guildInfo.CommandOverrides.Channels.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));
				guildInfo.CommandOverrides.Commands.RemoveAll(x => String.IsNullOrWhiteSpace(x.Name));

				var cmds = guildInfo.CommandOverrides.Commands.Select(x => x.Name).ToList();
				Variables.HelpList.Where(x => !cmds.CaseInsContains(x.Name)).ToList().ForEach(x => guildInfo.CommandOverrides.Commands.Add(new CommandSwitch(x.Name, x.DefaultEnabled)));
			}
			else
			{
				Variables.HelpList.ForEach(x => guildInfo.CommandOverrides.Commands.Add(new CommandSwitch(x.Name, x.DefaultEnabled)));
			}

			guildInfo.TurnLoadedOn();
			guildInfo.Invites.AddRange((await guild.GetInvitesAsync()).ToList().Select(x => new BotInvite(x.GuildId, x.Code, x.Uses)).ToList());
			Variables.Guilds.Add(guild.Id, guildInfo);
		}

		public static BotGlobalInfo LoadBotInfo()
		{
			BotGlobalInfo botInfo = null;
			var path = GetBaseBotDirectory(Constants.BOT_INFO_LOCATION);
			if (!File.Exists(path))
			{
				WriteLine("The bot information file does not exist.");
				return botInfo;
			}

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
			return botInfo;
		}

		public static BotGuildInfo LoadGuildInfo(IGuild guild)
		{
			BotGuildInfo guildInfo = null;
			var path = GetServerFilePath(guild.Id, Constants.GUILD_INFO_LOCATION);
			if (!File.Exists(path))
			{
				WriteLine(String.Format("The guild information file for {0} does not exist.", guild.FormatGuild()));
				return guildInfo;
			}

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
			return guildInfo;
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

		public static void StartUpMessages()
		{
			if (Variables.Loaded)
				return;

			WriteLine("The current bot prefix is: " + Variables.BotInfo.Prefix);
			WriteLine("Bot took " + String.Format("{0:n}", TimeSpan.FromTicks(DateTime.UtcNow.ToUniversalTime().Ticks - Variables.StartupTime.Ticks).TotalMilliseconds) + " milliseconds to load everything.");
		}

		public static void LoadBasicInformation()
		{
			//Get the bot's ID
			HandleBotID(Properties.Settings.Default.BotID);
			//Checks if the OS is Windows or not
			GetOS();
			//Check if console or WPF app
			GetConsoleOrGUI();
			//Loads up stuff like global prefix, game, shard count
			LoadBot();
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
						WriteLine(String.Format("The following commands have conflicts: {0}\n{1}", String.Join("\n", simCmds.Select(x => x.Name)), name));
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
					name = Enum.GetName(typeof(GuildPermission), (GuildPermission)i);
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
			const UInt32 GENERAL_BITS = 0
				| (1U << (int)GuildPermission.CreateInstantInvite)
				| (1U << (int)GuildPermission.ManageChannels)
				| (1U << (int)GuildPermission.ManageRoles)
				| (1U << (int)GuildPermission.ManageWebhooks);

			const UInt32 TEXT_BITS = 0
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

			const UInt32 VOICE_BITS = 0
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
					name = Enum.GetName(typeof(ChannelPermission), (ChannelPermission)i);
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

		public static void SaveGuildInfo(BotGuildInfo guildInfo)
		{
			OverWriteFile(GetServerFilePath(guildInfo.GuildID, Constants.GUILD_INFO_LOCATION), Serialize(guildInfo));
		}

		public static void LoadBot()
		{
			var botInfo = LoadBotInfo();
			if (botInfo != null)
			{
				botInfo.PostDeserialize();
			}
			else
			{
				botInfo = new BotGlobalInfo();
			}

			Variables.BotInfo = botInfo;
		}

		public static void SaveBotInfo()
		{
			OverWriteFile(GetBaseBotDirectory(Constants.BOT_INFO_LOCATION), Serialize(Variables.BotInfo));
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
		#endregion

		#region Basic Gets
		public static Dictionary<String, String> GetChannelOverwritePermissions(Overwrite overwrite)
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

		public static Dictionary<String, String> GetFilteredChannelOverwritePermissions(Overwrite overwrite, IGuildChannel channel)
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
			return guildInfo.CommandOverrides.Commands.Where(x => x.CategoryEnum == category).ToList();
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
				for (int i = 0; i < min; i++)
				{
					list.Add(null);
				}
				if (min == 0)
				{
					return new ReturnedArguments(list, ArgFailureReason.Not_Failure);
				}
				else
				{
					return new ReturnedArguments(list, ArgFailureReason.Too_Few_Args);
				}
			}

			var args = SplitByCharExceptInQuotes(input, ' ').ToList();
			if (min > max)
			{
				return new ReturnedArguments(args, ArgFailureReason.Max_Less_Than_Min);
			}
			else if (args.Count < min)
			{
				return new ReturnedArguments(args, ArgFailureReason.Too_Few_Args);
			}
			else if (args.Count > max)
			{
				return new ReturnedArguments(args, ArgFailureReason.Too_Many_Args);
			}

			//Remove all mentions
			/*
			if (removeMentions)
			{
				var tempList = new List<string>();
				foreach (var arg in args)
				{
					if (MentionUtils.TryParseChannel(arg, out ulong ID))
					{
					}
					else if (MentionUtils.TryParseRole(arg, out ID))
					{
					}
					else if (MentionUtils.TryParseUser(arg, out ID))
					{
					}
					else
					{
						tempList.Add(arg);
					}
				}
				args = tempList;
			}

			//Limiting the amount of args that are kept separate
			if (shortenTo != int.MaxValue && args.Count > 1)
			{
				//[ a, b, c, d ]; args.Length = 4
				//shortenToIndex = 3; [ a, b, c, d ] => [ a, b, c d ] <-- Used example
				//shortenToIndex = 1; [ a, b, c, d ] => [ a b c d ]
				var count = Math.Min(shortenTo, args.Count) - 1; //Min of (3, 4) which is 3 minus 1 is 2
				var tempList = new List<string>(); //2 + 1 is 3 so an array of 3 objects
				for (int i = 0; i <= count; i++)
				{
					tempList.Add(args[i]); //Set the first two objects. [ a, b, empty ]
				}
				if (shortenTo < args.Count)
				{
					tempList[count] = String.Join(" ", args.GetRange(count, args.Count - count)); //Grab the remaining objects (a b) and stuff them into the array
				}
				args = tempList;
			}
			*/

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
				return new ReturnedType<T>(type, TypeFailureReason.Not_Found);
			}
			else if (invalidTypes != null)
			{
				if (invalidTypes.Contains(type))
				{
					return new ReturnedType<T>(type, TypeFailureReason.Invalid_Type);
				}
			}
			else if (!validTypes.ToList().Contains(type))
			{
				return new ReturnedType<T>(type, TypeFailureReason.Invalid_Type);
			}

			return new ReturnedType<T>(type, TypeFailureReason.Not_Failure);
		}

		public static CommandSwitch GetCommand(BotGuildInfo guildInfo, string input)
		{
			return guildInfo.CommandOverrides.Commands.FirstOrDefault(x =>
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
			return fullStr.Replace(Variables.BotInfo.Prefix, prefix);
		}

		public static string GetPlural(int i)
		{
			return i == 1 ? "" : "s";
		}

		public static string GetServerFilePath(ulong guildId, string fileName)
		{
			//Make sure the bot's directory exists
			var directory = GetBaseBotDirectory();
			if (!Directory.Exists(directory))
				return null;

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

		public static string GetPrefix(IGuild guild)
		{
			var prefix = "";
			if (Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
			{
				prefix = guildInfo.Prefix;
			}
			if (String.IsNullOrWhiteSpace(prefix))
			{
				prefix = Variables.BotInfo.Prefix;
			}
			return prefix;
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

			return guild.OwnerId == user.Id || GetIfUserIsOwnerButBotIsOwner(guild, user);
		}

		public static bool GetIfUserIsOwnerButBotIsOwner(IGuild guild, IUser user)
		{
			return guild.OwnerId == Variables.BotID && GetUserPosition(guild, user) == guild.Roles.Max(x => x.Position) - 1;
		}

		public static bool GetIfUserIsBotOwner(IUser user)
		{
			return user.Id == Variables.BotInfo.BotOwnerID;
		}

		public static bool GetIfUserIsTrustedUser(IUser user)
		{
			return Variables.BotInfo.TrustedUsers.Contains(user.Id);
		}

		public static bool GetIfBypass(string str)
		{
			return CaseInsEquals(str, Constants.BYPASS_STRING);
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
			return inputArray.CaseInsContains(Constants.BYPASS_STRING) && context.User.Id == Variables.BotInfo.BotOwnerID ? int.MaxValue : Variables.BotInfo.MaxUserGatherCount;
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
				if (timeFrame == 0 || listLength < 2)
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

		public static void GetOS()
		{
			var windir = Environment.GetEnvironmentVariable("windir");
			Variables.Windows = !String.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir);
		}

		public static void GetConsoleOrGUI()
		{
			try
			{
				var window_height = Console.WindowHeight;
			}
			catch
			{
				Variables.Console = false;
			}
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
						return new ReturnedDiscordObject<IGuildChannel>(channel, FailureReason.Too_Many);
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
						return new ReturnedDiscordObject<IGuildChannel>(channel, FailureReason.Too_Many);
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
				return new ReturnedDiscordObject<IGuildChannel>(channel, FailureReason.Not_Found);
			}

			return GetChannel(context, checkingTypes, channel);
		}

		public static ReturnedDiscordObject<IGuildChannel> GetChannel(ICommandContext context, ChannelCheck[] checkingTypes, IGuildChannel channel)
		{
			if (channel == null)
			{
				return new ReturnedDiscordObject<IGuildChannel>(channel, FailureReason.Not_Found);
			}

			var bot = GetBot(context.Guild);
			var user = context.User as IGuildUser;
			foreach (var type in checkingTypes)
			{
				if (!GetIfUserCanDoActionOnChannel(context, channel, user, type))
				{
					return new ReturnedDiscordObject<IGuildChannel>(channel, FailureReason.User_Inability);
				}
				else if (!GetIfUserCanDoActionOnChannel(context, channel, bot, type))
				{
					return new ReturnedDiscordObject<IGuildChannel>(channel, FailureReason.Bot_Inability);
				}
				else if (!GetIfChannelIsCorrectType(context, channel, user, type))
				{
					return new ReturnedDiscordObject<IGuildChannel>(channel, FailureReason.Incorrect_Channel_Type);
				}
			}

			return new ReturnedDiscordObject<IGuildChannel>(channel, FailureReason.Not_Failure);
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
					if (GetIfUserCanDoActionOnChannel(context, channel, user, ChannelCheck.Can_Modify_Permissions) && GetIfUserCanDoActionOnChannel(context, channel, bot, ChannelCheck.Can_Modify_Permissions))
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
					case ChannelCheck.Can_Be_Managed:
					{
						return channelPerms.ReadMessages && channelPerms.ManageChannel;
					}
					case ChannelCheck.Can_Modify_Permissions:
					{
						return channelPerms.ReadMessages && channelPerms.ManageChannel && channelPerms.ManagePermissions;
					}
					case ChannelCheck.Can_Be_Reordered:
					{
						return channelPerms.ReadMessages && guildPerms.ManageChannels;
					}
					case ChannelCheck.Can_Delete_Messages:
					{
						return channelPerms.ReadMessages && channelPerms.ManageMessages;
					}
				}
			}
			else
			{
				switch (type)
				{
					case ChannelCheck.Can_Be_Managed:
					{
						return channelPerms.ManageChannel;
					}
					case ChannelCheck.Can_Modify_Permissions:
					{
						return channelPerms.ManageChannel && channelPerms.ManagePermissions;
					}
					case ChannelCheck.Can_Be_Reordered:
					{
						return guildPerms.ManageChannels;
					}
					case ChannelCheck.Can_Move_Users:
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
				case ChannelCheck.Is_Text:
				{
					return GetChannelType(target) == Constants.TEXT_TYPE;
				}
				case ChannelCheck.Is_Voice:
				{
					return GetChannelType(target) == Constants.VOICE_TYPE;
				}
			}
			return true;
		}
		#endregion

		#region Roles
		public static async Task<IRole> CreateMuteRoleIfNotFound(IGuild guild, IRole muteRole)
		{
			//Create the role if not found
			if (muteRole == null)
			{
				muteRole = await guild.CreateRoleAsync(Constants.MUTE_ROLE_NAME);
			}
			//Change its guild perms
			await muteRole.ModifyAsync(x => x.Permissions = new GuildPermissions(0));
			//Change the perms it has on every single text channel
			(await guild.GetTextChannelsAsync()).ToList().ForEach(x =>
			{
				x.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(0, 805316689));
			});
			return muteRole;
		}

		public static async Task<int> GetIfGroupIsValid(ICommandContext context, string input)
		{
			if (!int.TryParse(input, out int groupNumber))
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR("Invalid group number."));
				return -1;
			}
			else if (groupNumber < 0)
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR("Group number must be positive."));
				return -1;
			}

			return groupNumber;
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
						return new ReturnedDiscordObject<IRole>(role, FailureReason.Too_Many);
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
						return new ReturnedDiscordObject<IRole>(role, FailureReason.Too_Many);
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
				return new ReturnedDiscordObject<IRole>(role, FailureReason.Not_Found);
			}

			return GetRole(context, checkingTypes, role);
		}

		public static ReturnedDiscordObject<IRole> GetRole(ICommandContext context, RoleCheck[] checkingTypes, IRole role)
		{
			if (role == null)
			{
				return new ReturnedDiscordObject<IRole>(role, FailureReason.Not_Found);
			}

			var bot = GetBot(context.Guild);
			var user = context.User as IGuildUser;
			foreach (var type in checkingTypes)
			{
				if (!GetIfUserCanDoActionOnRole(context, role, user, type))
				{
					return new ReturnedDiscordObject<IRole>(role, FailureReason.User_Inability);
				}
				else if (!GetIfUserCanDoActionOnRole(context, role, bot, type))
				{
					return new ReturnedDiscordObject<IRole>(role, FailureReason.Bot_Inability);
				}

				switch (type)
				{
					case RoleCheck.Is_Everyone:
					{
						if (context.Guild.EveryoneRole.Id == role.Id)
						{
							return new ReturnedDiscordObject<IRole>(role, FailureReason.Everyone_Role);
						}
						break;
					}
					case RoleCheck.Is_Managed:
					{
						if (role.IsManaged)
						{
							return new ReturnedDiscordObject<IRole>(role, FailureReason.Managed_Role);
						}
						break;
					}
				}
			}

			return new ReturnedDiscordObject<IRole>(role, FailureReason.Not_Failure);
		}

		public static EditableDiscordObject<IRole>? GetValidEditRoles(ICommandContext context, List<string> input)
		{
			//Gather the users
			var success = new List<IRole>();
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
					var returnedRole = GetRole(context, new[] { RoleCheck.Can_Be_Edited, RoleCheck.Is_Everyone, RoleCheck.Is_Managed }, false, x);
					if (returnedRole.Reason == FailureReason.Not_Failure)
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
				case RoleCheck.Can_Be_Edited:
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
			var typing = context.Channel.EnterTypingState();

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
			typing.Dispose();
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
						return new ReturnedDiscordObject<IGuildUser>(user, FailureReason.Too_Many);
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
						return new ReturnedDiscordObject<IGuildUser>(user, FailureReason.Too_Many);
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
				return new ReturnedDiscordObject<IGuildUser>(user, FailureReason.Not_Found);
			}

			var bot = GetBot(context.Guild);
			var currUser = context.User as IGuildUser;
			foreach (var type in checkingTypes)
			{
				if (!GetIfUserCanDoActionOnUser(context, user, currUser, type))
				{
					return new ReturnedDiscordObject<IGuildUser>(user, FailureReason.User_Inability);
				}
				else if (!GetIfUserCanDoActionOnUser(context, user, bot, type))
				{
					return new ReturnedDiscordObject<IGuildUser>(user, FailureReason.Bot_Inability);
				}
			}

			return new ReturnedDiscordObject<IGuildUser>(user, FailureReason.Not_Failure);
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
					return new ReturnedBannedUser(bans.FirstOrDefault(x => x.User.Id == inputUserID), BannedUserFailureReason.Not_Failure);
				}
				else
				{
					return new ReturnedBannedUser(null, BannedUserFailureReason.Invalid_ID);
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
						return new ReturnedBannedUser(null, BannedUserFailureReason.Invalid_Discriminator);
					}
				}

				//Return a message saying if there are multiple users
				if (users.Count == 0)
				{
					return new ReturnedBannedUser(null, BannedUserFailureReason.No_Match);
				}
				else if (users.Count == 1)
				{
					return new ReturnedBannedUser(users.First(), BannedUserFailureReason.Not_Failure);
				}
				else
				{
					return new ReturnedBannedUser(null, BannedUserFailureReason.Too_Many_Matches, users);
				}
			}
			else
			{
				return new ReturnedBannedUser(null, BannedUserFailureReason.No_Username_Or_ID);
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

		public static IUser GetBotOwner()
		{
			return Variables.Client.GetUser(Variables.BotInfo.BotOwnerID);
		}

		public static bool GetIfUserCanDoActionOnUser(ICommandContext context, IGuildUser target, IGuildUser user, UserCheck type)
		{
			if (target == null || user == null)
				return false;

			switch (type)
			{
				case UserCheck.Can_Be_Moved_From_Channel:
				{
					var channel = target.VoiceChannel;
					return GetIfUserCanDoActionOnChannel(context, channel, user, ChannelCheck.Can_Move_Users);
				}
				case UserCheck.Can_Be_Edited:
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

		#region Messages
		public static async Task<EmbedBuilder> FormatSettingInfo(ICommandContext context, BotGuildInfo guildInfo, SettingOnGuild setting, string targetStr, string extraStr)
		{
			var user = GetGuildUser(context, new[] { UserCheck.None }, true, targetStr).Object;
			var role = GetRole(context, new[] { RoleCheck.None }, true, targetStr).Object;
			var channel = GetChannel(context, new[] { ChannelCheck.None }, true, targetStr).Object;

			var title = Enum.GetName(typeof(SettingOnGuild), setting);
			var str = "";
			switch (setting)
			{
				case SettingOnGuild.CommandPreferences:
				{
					str = String.Join("\n", guildInfo.CommandOverrides.Commands.Select(x => String.Format("`{0}` `{1}`", x.Name, x.ValAsString)));
					break;
				}
				case SettingOnGuild.CommandsDisabledOnChannel:
				{
					if (!String.IsNullOrWhiteSpace(extraStr))
					{
						var cmd = Variables.CommandNames.FirstOrDefault(x => CaseInsEquals(x, extraStr));
						if (cmd == null)
						{
							str = String.Format("The given input `{0}` is not a valid command.", extraStr);
						}
						else
						{
							var cmds = guildInfo.CommandOverrides.Channels.Where(x => CaseInsEquals(x.Name, cmd));
							str = String.Join("\n", cmds.Select(x => String.Format("`{0}` `{1}`", x.ID, x.Name)));
							title = String.Format("Channels `{0}` is unable to be used on", cmd);
						}
					}
					else
					{
						str = String.Join("\n", guildInfo.CommandOverrides.Channels.Select(x => String.Format("`{0}` `{1}`", x.ID, x.Name)));
					}
					break;
				}
				case SettingOnGuild.BotUsers:
				{
					if (user != null)
					{
						var botUser = guildInfo.BotUsers.FirstOrDefault(x => x.User.Id == user.Id);
						var perms = GetPermissionNames(botUser.Permissions);
						if (botUser == null || !perms.Any())
						{
							str = ERROR("That user has no bot permissions.");
						}
						else
						{
							str = String.Format("The user `{0}` has the following permission(s): `{1}`.", user.FormatUser(), String.Join("`, `", perms));
						}
					}
					else
					{
						str = String.Join("\n", guildInfo.BotUsers.Select(x => String.Format("`{0}` `{1}`", x.UserID, x.Permissions)));
					}
					break;
				}
				case SettingOnGuild.SelfAssignableGroups:
				{
					if (!String.IsNullOrWhiteSpace(extraStr))
					{
						var num = await GetIfGroupIsValid(context, extraStr);
						if (num == -1)
							return null;

						var group = guildInfo.SelfAssignableGroups.FirstOrDefault(x => x.Group == num);
						if (group == null)
						{
							str = "There is no group with that number.";
						}
						else
						{
							str = String.Format("`{0}`", String.Join("`\n`", group.Roles.Select(x => x.Role.Name))); ;
						}
					}
					else
					{
						str = String.Join("\n", guildInfo.SelfAssignableGroups.SelectMany(x => x.Roles).OrderBy(x => x.Group).Select(x => String.Format("`{0}`: `{1}`", x.Group, x.RoleID)));
					}
					break;
				}
				case SettingOnGuild.Reminds:
				{
					str = String.Join("\n", guildInfo.Reminds.Select(x => String.Format("`{0}`", x.Name)));
					break;
				}
				case SettingOnGuild.IgnoredLogChannels:
				{
					str = String.Join("\n", guildInfo.IgnoredLogChannels.Select(x => String.Format("`{0}`", (context.Guild as SocketGuild).GetChannel(x).FormatChannel())));
					break;
				}
				case SettingOnGuild.LogActions:
				{
					str = String.Join("\n", guildInfo.LogActions.Select(x => String.Format("`{0}`", Enum.GetName(typeof(LogActions), x))));
					break;
				}
				case SettingOnGuild.BannedPhraseStrings:
				{
					str = String.Join("\n", guildInfo.BannedPhrases.Strings.Select(x =>
						String.Format("`{0}` `{1}`", Enum.GetName(typeof(PunishmentType), x.Punishment).Substring(0, 1), x.Phrase)));
					break;
				}
				case SettingOnGuild.BannedPhraseRegex:
				{
					str = String.Join("\n", guildInfo.BannedPhrases.Regex.Select(x =>
						String.Format("`{0}` `{1}`", Enum.GetName(typeof(PunishmentType), x.Punishment).Substring(0, 1), x.Phrase.ToString())));
					break;
				}
				case SettingOnGuild.BannedPhrasePunishments:
				{
					str = String.Join("\n", guildInfo.BannedPhrases.Punishments.Select(x =>
					{
						return String.Format("`{0}.` `{1}`{2}",
							x.NumberOfRemoves.ToString("00"),
							x.Role == null ? Enum.GetName(typeof(PunishmentType), x.Punishment) : x.Role.Name,
							x.PunishmentTime == null ? "" : " `" + x.PunishmentTime + " minutes`");
					}));
					break;
				}
				case SettingOnGuild.MessageSpamPrevention:
				{
					var spamPrev = guildInfo.GlobalSpamPrevention.GetSpamPrevention(SpamType.Message);
					if (spamPrev != null)
					{
						str = String.Format("**Enabled:** `{0}`\n**Amount Of Messages:** `{1}`\n**Timeframe:** `{2}`\n**Votes Needed For Kick:** `{3}`",
							spamPrev.Enabled, spamPrev.AmountOfMessages, spamPrev.AmountOfSpam, spamPrev.VotesNeededForKick);
					}
					break;
				}
				case SettingOnGuild.LongMessageSpamPrevention:
				{
					var spamPrev = guildInfo.GlobalSpamPrevention.GetSpamPrevention(SpamType.Long_Message);
					if (spamPrev != null)
					{
						str = String.Format("**Enabled:** `{0}`\n**Amount Of Messages:** `{1}`\n**Length:** `{2}`\n**Votes Needed For Kick:** `{3}`",
							spamPrev.Enabled, spamPrev.AmountOfMessages, spamPrev.AmountOfSpam, spamPrev.VotesNeededForKick);
					}
					break;
				}
				case SettingOnGuild.LinkSpamPrevention:
				{
					var spamPrev = guildInfo.GlobalSpamPrevention.GetSpamPrevention(SpamType.Link);
					if (spamPrev != null)
					{
						str = String.Format("**Enabled:** `{0}`\n**Amount Of Messages:** `{1}`\n**Link Count:** `{2}`\n**Votes Needed For Kick:** `{3}`",
							spamPrev.Enabled, spamPrev.AmountOfMessages, spamPrev.AmountOfSpam, spamPrev.VotesNeededForKick);
					}
					break;
				}
				case SettingOnGuild.ImageSpamPrevention:
				{
					var spamPrev = guildInfo.GlobalSpamPrevention.GetSpamPrevention(SpamType.Image);
					if (spamPrev != null)
					{
						str = String.Format("**Enabled:** `{0}`\n**Amount Of Messages:** `{1}`\n**Timeframe:** `{2}`\n**Votes Needed For Kick:** `{3}`",
							spamPrev.Enabled, spamPrev.AmountOfMessages, spamPrev.AmountOfSpam, spamPrev.VotesNeededForKick);
					}
					break;
				}
				case SettingOnGuild.MentionSpamPrevention:
				{
					var spamPrev = guildInfo.GlobalSpamPrevention.GetSpamPrevention(SpamType.Mention);
					if (spamPrev != null)
					{
						str = String.Format("**Enabled:** `{0}`\n**Amount Of Messages:** `{1}`\n**Mentions:** `{2}`\n**Votes Needed For Kick:** `{3}`",
							spamPrev.Enabled, spamPrev.AmountOfMessages, spamPrev.AmountOfSpam, spamPrev.VotesNeededForKick);
					}
					break;
				}
				case SettingOnGuild.WelcomeMessage:
				{
					var wm = guildInfo.WelcomeMessage;
					if (wm != null)
					{
						str = String.Format("**Content:** `{0}`\n**Title:** `{1}`\n**Description:** `{2}`\n**Thumbnail:** `{3}`", wm.Content, wm.Title, wm.Description, wm.ThumbURL);
					}
					break;
				}
				case SettingOnGuild.GoodbyeMessage:
				{
					var gb = guildInfo.GoodbyeMessage;
					if (gb != null)
					{
						str = String.Format("**Content:** `{0}`\n**Title:** `{1}`\n**Description:** `{2}`\n**Thumbnail:** `{3}`", gb.Content, gb.Title, gb.Description, gb.ThumbURL);
					}
					break;
				}
				case SettingOnGuild.Prefix:
				{
					if (!String.IsNullOrWhiteSpace(guildInfo.Prefix))
					{
						str = String.Format("`{0}`", guildInfo.Prefix);
					}
					break;
				}
				case SettingOnGuild.Serverlog:
				{
					if (guildInfo.ServerLog != null)
					{
						str = String.Format("`{0}`", guildInfo.ServerLog.FormatChannel());
					}
					break;
				}
				case SettingOnGuild.Modlog:
				{
					if (guildInfo.ModLog != null)
					{
						str = String.Format("`{0}`", guildInfo.ModLog.FormatChannel());
					}
					break;
				}
				case SettingOnGuild.ImageOnlyChannels:
				{
					str = String.Join("\n", guildInfo.ImageOnlyChannels.Select(x => String.Format("`{0}`", (context.Guild as SocketGuild).GetChannel(x).FormatChannel())));
					break;
				}
				case SettingOnGuild.IgnoredCommandChannels:
				{
					str = String.Join("\n", guildInfo.IgnoredCommandChannels.Select(x => String.Format("`{0}`", (context.Guild as SocketGuild).GetChannel(x).FormatChannel())));
					break;
				}
				case SettingOnGuild.CommandsDisabledOnUser:
				{
					if (!String.IsNullOrWhiteSpace(extraStr))
					{
						var cmd = Variables.CommandNames.FirstOrDefault(x => CaseInsEquals(x, extraStr));
						if (cmd == null)
						{
							str = String.Format("The given input `{0}` is not a valid command.", extraStr);
						}
						else
						{
							var cmds = guildInfo.CommandOverrides.Users.Where(x => CaseInsEquals(x.Name, cmd));
							str = String.Join("\n", cmds.Select(x => String.Format("`{0}` `{1}`", x.ID, x.Name)));
							title = String.Format("Users unable to use the command `{0}`", cmd);
						}
					}
					else
					{
						str = String.Join("\n", guildInfo.CommandOverrides.Users.Select(x => String.Format("`{0}` `{1}`", x.ID, x.Name)));
					}
					break;
				}
				case SettingOnGuild.CommandsDisabledOnRole:
				{
					if (!String.IsNullOrWhiteSpace(extraStr))
					{
						var cmd = Variables.CommandNames.FirstOrDefault(x => CaseInsEquals(x, extraStr));
						if (cmd == null)
						{
							str = String.Format("The given input `{0}` is not a valid command.", extraStr);
						}
						else
						{
							var cmds = guildInfo.CommandOverrides.Roles.Where(x => CaseInsEquals(x.Name, cmd));
							str = String.Join("\n", cmds.Select(x => String.Format("`{0}` `{1}`", x.ID, x.Name)));
							title = String.Format("Roles unable to use the command `{0}`", cmd);
						}
					}
					else
					{
						str = String.Join("\n", guildInfo.CommandOverrides.Roles.Select(x => String.Format("`{0}` `{1}`", x.ID, x.Name)));
					}
					break;
				}
			}

			return MakeNewEmbed(title, String.IsNullOrWhiteSpace(str) ? "`NOTHING`" : str);
		}

		public static async Task<IMessage> SendEmbedMessage(IMessageChannel channel, EmbedBuilder embed, string content = null)
		{
			var guildChannel = channel as ITextChannel;
			if (guildChannel == null)
				return null;
			var guild = guildChannel.Guild;
			if (guild == null)
				return null;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return null;

			content = content ?? "";

			//Replace all instances of the base prefix with the guild's prefix
			var guildPrefix = guildInfo.Prefix;
			if (!String.IsNullOrWhiteSpace(guildPrefix))
			{
				embed.Description.Replace(Variables.BotInfo.Prefix, guildPrefix);
			}

			try
			{
				return await guildChannel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + content, embed: embed.WithCurrentTimestamp());
			}
			catch (Exception e)
			{
				ExceptionToConsole(e);
				return null;
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
			if (channel == null || guild == null || !Variables.Guilds.ContainsKey(guild.Id))
				return null;

			var everyoneMention = guild.EveryoneRole.Mention;
			message = message.Replace(everyoneMention, Constants.FAKE_EVERYONE);
			message = CaseInsReplace(message, "@everyone", Constants.FAKE_EVERYONE);

			if (message.Length >= Constants.MAX_MESSAGE_LENGTH_LONG)
			{
				if (TryToUploadToHastebin(message, out string urlOrError))
				{
					message = String.Format("The given content for this message is over 2,000 characters and can be found at {0}.", urlOrError);
				}
				else
				{
					await SendPotentiallyBigMessage(channel.Guild, channel, message, urlOrError, "Very_Long_Message_");
					return null;
				}
			}

			return await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + message);
		}

		public static async Task<IMessage> SendDMMessage(IDMChannel channel, string message)
		{
			if (channel == null)
				return null;

			return await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + message);
		}

		public static async Task MakeAndDeleteSecondaryMessage(ICommandContext context, string secondStr, Int32 time = Constants.WAIT_TIME)
		{
			await MakeAndDeleteSecondaryMessage(context.Channel, context.Message, secondStr, time);
		}
		
		public static async Task MakeAndDeleteSecondaryMessage(IMessageChannel channel, IUserMessage message, string secondStr, Int32 time = Constants.WAIT_TIME)
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

		public static async Task MakeAndDeleteSecondaryMessage(IMessageChannel channel, string secondStr, Int32 time = Constants.WAIT_TIME)
		{
			await MakeAndDeleteSecondaryMessage(channel, null, secondStr, time);
		}

		public static async Task<int> RemoveMessages(IMessageChannel channel, int requestCount)
		{
			var guildChannel = channel as ITextChannel;
			if (guildChannel == null)
				return 0;

			var deletedCount = 0;
			while (requestCount > 0)
			{
				//Get the current messages and ones that aren't null
				var newNum = Math.Min(requestCount, 100);
				var messages = (await channel.GetMessagesAsync(newNum).ToList()).SelectMany(x => x).Where(x => x != null);
				if (!messages.Any())
					break;

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

			//Make sure there's a user id
			if (user == null)
			{
				return await RemoveMessages(channel, requestCount);
			}

			var deletedCount = 0;
			while (requestCount > 0)
			{
				//Get the current messages and ones that aren't null
				var newNum = Math.Min(requestCount, 100);
				var messages = (await channel.GetMessagesAsync(newNum).ToList()).SelectMany(x => x).Where(x => x != null && x.Author.Id == user.Id);
				if (!messages.Any())
					break;

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

				//Leave if the message count gathered implies that the channel is out of messages
				if (msgAmt < newNum)
					break;

				//Lower the request count
				requestCount -= msgAmt;
			}
			return deletedCount;
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
				await channel.DeleteMessagesAsync(messages.Where(x => x != null && DateTime.UtcNow.Subtract(x.CreatedAt.UtcDateTime).TotalDays < 14).Distinct());
			}
			catch
			{
				WriteLine(String.Format("Unable to delete {0} messages on the guild {1} on channel {2}.", messages.Count(), guildChannel.Guild.FormatGuild(), guildChannel.FormatChannel()));
			}
		}

		public static async Task DeleteMessage(IMessage message)
		{
			if (message == null)
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
				var content = ReplaceMarkdownChars(String.Join("\n-----\n", inputList));
				if (TryToUploadToHastebin(content, out string url))
				{
					//Upload the embed with the Hastebin link
					var embed = MakeNewEmbed("Deleted Messages", String.Format("Click [here]({0}) to see the messages.", url), Constants.MDEL);
					AddFooter(embed, "Deleted Messages");
					await SendEmbedMessage(channel, embed);
				}
				else
				{
					await WriteAndUploadTextFile(guild, channel, content, "Deleted_Messages_", "Deleted Messages");
				}
			}
		}

		public static async Task SendGuildNotification(IUser user, GuildNotification notification)
		{
			if (notification == null)
				return;

			var content = notification.Content;
			content = CaseInsReplace(content, "{UserMention}", user != null ? user.Mention : "Invalid User");
			content = CaseInsReplace(content, "{User}", user != null ? user.FormatUser() : "Invalid User");

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
				case FailureReason.Not_Found:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("Unable to find the {0}.", objType)));
					return;
				}
				case FailureReason.User_Inability:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("You are unable to make the given changes to the {0}: `{1}`.", objType, FormatObject((dynamic)returnedObject.Object))));
					return;
				}
				case FailureReason.Bot_Inability:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("I am unable to make the given changes to the {0}: `{1}`.", objType, FormatObject((dynamic)returnedObject.Object))));
					return;
				}
				case FailureReason.Too_Many:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("There are too many {0}s with the same name.", objType)));
					return;
				}
				case FailureReason.Incorrect_Channel_Type:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("Invalid channel type for the given variable requirement.")));
					return;
				}
				case FailureReason.Everyone_Role:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("The everyone role cannot be modified in that way.")));
					return;
				}
				case FailureReason.Managed_Role:
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
				case ArgFailureReason.Too_Many_Args:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("Too many arguments."));
					return;
				}
				case ArgFailureReason.Too_Few_Args:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("Too few arguments."));
					return;
				}
				case ArgFailureReason.Missing_Critical_Args:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("Missing critical arguments."));
					return;
				}
				case ArgFailureReason.Max_Less_Than_Min:
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
				case TypeFailureReason.Not_Found:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("Unable to find the type for the given input."));
					return;
				}
				case TypeFailureReason.Invalid_Type:
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
				case BannedUserFailureReason.No_Bans:
				{
					await MakeAndDeleteSecondaryMessage(context, "The guild has no bans.");
					return;
				}
				case BannedUserFailureReason.No_Match:
				{
					await MakeAndDeleteSecondaryMessage(context, "No ban was found which matched the given criteria.");
					return;
				}
				case BannedUserFailureReason.Too_Many_Matches:
				{
					var msg = String.Join("`, `", returnedBannedUser.MatchedBans.Select(x => x.User.FormatUser()));
					await SendChannelMessage(context, String.Format("The following users have that name: `{0}`.", msg));
					return;
				}
				case BannedUserFailureReason.Invalid_Discriminator:
				{
					await MakeAndDeleteSecondaryMessage(context, "The given discriminator is invalid.");
					return;
				}
				case BannedUserFailureReason.Invalid_ID:
				{
					await MakeAndDeleteSecondaryMessage(context, "The given ID is invalid.");
					return;
				}
				case BannedUserFailureReason.No_Username_Or_ID:
				{
					await MakeAndDeleteSecondaryMessage(context, "A username or ID must be provided");
					return;
				}
			}
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
				var output = description;
				//Descriptions can only be 2048 characters max
				if (description.Length > Constants.MAX_EMBED_LENGTH_LONG)
				{
					if (TryToUploadToHastebin(description, out output))
					{
						output = String.Format("Content is past {0} characters. Click [here]({1}) to see it.", Constants.MAX_EMBED_LENGTH_LONG, output);
					}
				}
				//Mobile can only show up to 20 or so lines on the description part of an embed
				else if (GetLineBreaks(description) > Constants.MAX_DESCRIPTION_LINES)
				{
					if (TryToUploadToHastebin(description, out output))
					{
						output = String.Format("Content is past {0} new lines. Click [here]({1}) to see it.", Constants.MAX_DESCRIPTION_LINES, output);
					}
				}
				embed.WithDescription(output);
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
			var author = new EmbedAuthorBuilder().WithIconUrl("https://discordapp.com/assets/322c936a8c8be1b803cd94861bdfa868.png");

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
			if ((String.IsNullOrWhiteSpace(name) && String.IsNullOrWhiteSpace(value)) || embed.Build().Fields.Count() >= Constants.MAX_FIELDS)
				return embed;

			//Get the name and value
			name = String.IsNullOrWhiteSpace(name) ? "Placeholder" : name.Substring(0, Math.Min(Constants.MAX_TITLE_LENGTH, name.Length));
			value = String.IsNullOrWhiteSpace(name) ? "Placeholder" : value.Substring(0, Math.Min(value.Length, Constants.MAX_LENGTH_FOR_HASTEBIN));

			embed.AddField(x =>
			{
				var outputValue = value;
				//Embeds can only show up to 1024 chars per field
				if (value.Length > Constants.MAX_EMBED_LENGTH_SHORT)
				{
					if (TryToUploadToHastebin(value, out outputValue))
					{
						outputValue = String.Format("Field has more than {0} characters; please click [here]({1}) to see the content.", Constants.MAX_EMBED_LENGTH_SHORT, outputValue);
					}
				}
				//Fields can only show up to five lines on mobile
				else if (GetLineBreaks(value) > Constants.MAX_FIELD_LINES)
				{
					if (TryToUploadToHastebin(value, out outputValue))
					{
						outputValue = String.Format("Field has more than {0} new lines; please click [here]({1}) to see the content.", Constants.MAX_FIELD_LINES, outputValue);
					}
				}

				//Actually add in the values
				x.Name = name;
				x.Value = outputValue;
				x.IsInline = isInline;
			});

			return embed;
		}

		public static EmbedBuilder FormatUserInfo(SocketGuild guild, IGuildUser user)
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

			//Make the description
			var IDstr = String.Format("**ID:** `{0}`", guildUser.Id);
			var nicknameStr = String.Format("**Nickname:** `{0}`", String.IsNullOrWhiteSpace(guildUser.Nickname) ? "NO NICKNAME" : guildUser.Nickname);
			var createdStr = String.Format("\n**Created:** `{0} {1}, {2} at {3}`",
				System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(created.Month),
				created.Day,
				created.Year,
				created.ToLongTimeString());
			var joinedStr = String.Format("**Joined:** `{0} {1}, {2} at {3}` (`{4}` to join the guild)\n",
				System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(joined.Month),
				joined.Day,
				joined.Year,
				joined.ToLongTimeString(),
				users.IndexOf(guildUser) + 1);
			var gameStr = FormatGameStr(guildUser);
			var statusStr = String.Format("**Online status:** `{0}`", guildUser.Status);
			var description = String.Join("\n", new[] { IDstr, nicknameStr, createdStr, joinedStr, gameStr, statusStr });

			var embed = MakeNewEmbed(null, description, roles.FirstOrDefault(x => x.Color.RawValue != 0)?.Color, thumbnailURL: user.GetAvatarUrl());
			AddAuthor(embed, guildUser.FormatUser(), guildUser.GetAvatarUrl(), guildUser.GetAvatarUrl());
			AddFooter(embed, "Userinfo");
			//Add the channels the user can access
			if (channels.Count() != 0)
			{
				AddField(embed, "Channels", String.Join(", ", channels));
			}
			//Add the roles the user has
			if (roles.Count() != 0)
			{
				AddField(embed, "Roles", String.Join(", ", roles.Select(x => x.Name)));
			}
			//Add the voice channel
			if (user.VoiceChannel != null)
			{
				var desc = String.Format("Server mute: `{0}`\nServer deafen: `{1}`\nSelf mute: `{2}`\nSelf deafen: `{3}`", user.IsMuted, user.IsDeafened, user.IsSelfMuted, user.IsSelfDeafened);
				AddField(embed, "Voice Channel: " + user.VoiceChannel.Name, desc);
			}
			return embed;
		}

		public static EmbedBuilder FormatUserInfo(SocketGuild guild, IUser user)
		{
			var created = user.CreatedAt.UtcDateTime;

			//Make the description
			var createdStr = String.Format("**Created:** `{0} {1}, {2} at {3}`\n",
				System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(created.Month),
				created.Day,
				created.Year,
				created.ToLongTimeString());
			var gameStr = FormatGameStr(user);
			var statusStr = String.Format("**Online status:** `{0}`", user.Status);
			var description = String.Join("\n", new[] { createdStr, gameStr, statusStr });

			var embed = MakeNewEmbed(null, description, null, thumbnailURL: user.GetAvatarUrl());
			AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl(), user.GetAvatarUrl());
			AddFooter(embed, "Userinfo");
			return embed;
		}

		public static EmbedBuilder FormatSettingInfo(ICommandContext context, BotGlobalInfo botInfo, SettingOnBot setting)
		{
			var title = Enum.GetName(typeof(SettingOnBot), setting);
			var str = "";
			switch (setting)
			{
				case SettingOnBot.BotOwner:
				{
					str = String.Format("`{0}`", Variables.Client.GetUser(botInfo.BotOwnerID).FormatUser());
					break;
				}
				case SettingOnBot.TrustedUsers:
				{
					str = String.Join("\n", botInfo.TrustedUsers.Select(x => String.Format("`{0}`", Variables.Client.GetUser(x).FormatUser())));
					break;
				}
				case SettingOnBot.Prefix:
				{
					if (!String.IsNullOrWhiteSpace(botInfo.Prefix))
					{
						str = "Then how did you use this command. :thinking:";
					}
					break;
				}
				case SettingOnBot.Game:
				{
					if (!String.IsNullOrWhiteSpace(botInfo.Game))
					{
						str = String.Format("`{0}`", botInfo.Game);
					}
					break;
				}
				case SettingOnBot.Stream:
				{
					if (!String.IsNullOrWhiteSpace(botInfo.Stream))
					{
						str = String.Format("`{0}`", botInfo.Stream);
					}
					break;
				}
				case SettingOnBot.ShardCount:
				{
					str = String.Format("`{0}`", botInfo.ShardCount);
					break;
				}
				case SettingOnBot.MessageCacheSize:
				{
					str = String.Format("`{0}`", botInfo.MessageCacheSize);
					break;
				}
				case SettingOnBot.AlwaysDownloadUsers:
				{
					str = String.Format("`{0}`", botInfo.AlwaysDownloadUsers);
					break;
				}
				case SettingOnBot.LogLevel:
				{
					str = String.Format("`{0}`", Enum.GetName(typeof(LogSeverity), botInfo.LogLevel));
					break;
				}
				case SettingOnBot.SavePath:
				{
					str = String.Format("`{0}`", Properties.Settings.Default.Path);
					break;
				}
			}

			return MakeNewEmbed(title, String.IsNullOrWhiteSpace(str) ? "`NOTHING`" : str);
		}

		public static List<string> FormatDeletedMessages(List<IMessage> list)
		{
			var deletedMessagesContent = new List<string>();
			list.ForEach(x =>
			{
				//See if any embeds deleted
				if (x.Embeds.Any())
				{
					//Get the first embed with a valid description, then URL, then image
					var embed = x.Embeds.FirstOrDefault(desc => desc.Description != null) ?? x.Embeds.FirstOrDefault(url => url.Url != null) ?? x.Embeds.FirstOrDefault(img => img.Image != null);

					if (embed != null)
					{
						var msgContent = String.IsNullOrWhiteSpace(x.Content) ? "" : "Message Content: " + x.Content;
						var description = String.IsNullOrWhiteSpace(embed.Description) ? "" : "Embed Description: " + embed.Description;
						deletedMessagesContent.Add(String.Format("`{0}` **IN** `{1}` **SENT AT** `[{2}]`\n```\n{3}```",
							x.Author.FormatUser(),
							x.Channel.FormatChannel(),
							x.CreatedAt.ToString("HH:mm:ss"),
							ReplaceMarkdownChars((String.IsNullOrEmpty(msgContent) ? msgContent : msgContent + "\n") + description)));
					}
					else
					{
						deletedMessagesContent.Add(String.Format("`{0}` **IN** `{1}` **SENT AT** `[{2}]`\n```\n{3}```",
							x.Author.FormatUser(),
							x.Channel.FormatChannel(),
							x.CreatedAt.ToString("HH:mm:ss"),
							"An embed which was unable to be gotten."));
					}
				}
				//See if any attachments were put in
				else if (x.Attachments.Any())
				{
					var content = String.IsNullOrEmpty(x.Content) ? "EMPTY MESSAGE" : x.Content;
					deletedMessagesContent.Add(String.Format("`{0}` **IN** `{1}` **SENT AT** `[{2}]`\n```\n{3}```",
						x.Author.FormatUser(),
						x.Channel.FormatChannel(),
						x.CreatedAt.ToString("HH:mm:ss"),
						ReplaceMarkdownChars(content + " + " + x.Attachments.ToList().First().Filename)));
				}
				//Else add the message in normally
				else
				{
					var content = String.IsNullOrEmpty(x.Content) ? "EMPTY MESSAGE" : x.Content;
					deletedMessagesContent.Add(String.Format("`{0}` **IN** `{1}` **SENT AT** `[{2}]`\n```\n{3}```",
						x.Author.FormatUser(),
						x.Channel.FormatChannel(),
						x.CreatedAt.ToString("HH:mm:ss"),
						ReplaceMarkdownChars(content)));
				}
			});
			return deletedMessagesContent;
		}

		public static string FormatGameStr(IUser user)
		{
			if (user.Game.HasValue)
			{
				var game = user.Game.Value;
				if (game.StreamType == StreamType.Twitch)
				{
					return String.Format("**Current Stream:** [{0}]({1})", game.Name, game.StreamUrl);
				}
				else
				{
					return String.Format("**Current Game:** `{0}`", game.Name);
				}
			}
			return "**Current Game:** `N/A`";
		}

		public static string ERROR(string message)
		{
			++Variables.FailedCommands;

			return Constants.ZERO_LENGTH_CHAR + Constants.ERROR_MESSAGE + message;
		}
		
		public static string ReplaceMarkdownChars(string input)
		{
			if (String.IsNullOrWhiteSpace(input))
				return "";

			//Matching
			var empty = new Regex("[*`]", RegexOptions.Compiled);

			//Actually removing
			input = empty.Replace(input, "");
			while (input.Contains("\n\n"))
			{
				input = input.Replace("\n\n", "\n");
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
			return FormatRole(role);
		}

		public static string FormatObject(IGuild guild)
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

		public static string FormatRole(IRole role)
		{
			if (role == null)
				return "Unable to get role data.";
			return String.Format("{0} ({1})", role.Name, role.Id);
		}

		public static string RemoveNewLines(string input)
		{
			return input.Replace(Environment.NewLine, "").Replace("\r", "").Replace("\n", "");
		}

		public static string FormatLoggedThings(int spacing)
		{
			var joins = String.Format("{0}{1}", "**Joins:**".PadRight(spacing), Variables.LoggedJoins);
			var leaves = String.Format("{0}{1}", "**Leaves:**".PadRight(spacing), Variables.LoggedLeaves);
			var userChanges = String.Format("{0}{1}", "**User changes:**".PadRight(spacing), Variables.LoggedUserChanges);
			var edits = String.Format("{0}{1}", "**Edits:**".PadRight(spacing), Variables.LoggedEdits);
			var deletes = String.Format("{0}{1}", "**Deletes:**".PadRight(spacing), Variables.LoggedDeletes);
			var images = String.Format("{0}{1}", "**Images:**".PadRight(spacing), Variables.LoggedImages);
			var gifs = String.Format("{0}{1}", "**Gifs:**".PadRight(spacing), Variables.LoggedGifs);
			var files = String.Format("{0}{1}", "**Files:**".PadRight(spacing), Variables.LoggedFiles);
			return String.Join("\n", new[] { joins, leaves, userChanges, edits, deletes, images, gifs, files });
		}

		public static string FormatLoggedCommands(int spacing)
		{
			var attempted = String.Format("{0}{1}", "**Attempted:**".PadRight(spacing), Variables.AttemptedCommands);
			var successful = String.Format("{0}{1}", "**Successful:**".PadRight(spacing), Variables.AttemptedCommands - Variables.FailedCommands);
			var failed = String.Format("{0}{1}", "**Failed:**".PadRight(spacing), Variables.FailedCommands);
			return String.Join("\n", new[] { attempted, successful, failed });
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

		public static string FormatAllSettings(BotGuildInfo guildInfo)
		{
			//Getting bools
			var commandPreferences = !guildInfo.DefaultPrefs;
			var commandsDisabledOnUser = guildInfo.CommandOverrides.Users.Any();
			var commandsDisabledOnRole = guildInfo.CommandOverrides.Roles.Any();
			var commandsDisabledOnChannel = guildInfo.CommandOverrides.Channels.Any();
			var botUsers = guildInfo.BotUsers.Any();
			var selfAssignableGroups = guildInfo.SelfAssignableGroups.Any();
			var reminds = guildInfo.Reminds.Any();
			var ignoredCommandChannels = guildInfo.IgnoredCommandChannels.Any();
			var ignoredLogChannels = guildInfo.IgnoredLogChannels.Any();
			var imageOnlyChannels = guildInfo.ImageOnlyChannels.Any();
			var logActions = guildInfo.LogActions.Any();
			var bannedPhraseStrings = guildInfo.BannedPhrases.Strings.Any();
			var bannedPhraseRegex = guildInfo.BannedPhrases.Regex.Any();
			var bannedPhrasePunishments = guildInfo.BannedPhrases.Punishments.Any();
			var messageSpamPrevention = guildInfo.GlobalSpamPrevention.GetSpamPrevention(SpamType.Message) != null;
			var longMessageSpamPrevention = guildInfo.GlobalSpamPrevention.GetSpamPrevention(SpamType.Long_Message) != null;
			var linkSpamPrevention = guildInfo.GlobalSpamPrevention.GetSpamPrevention(SpamType.Link) != null;
			var imageSpamPrevention = guildInfo.GlobalSpamPrevention.GetSpamPrevention(SpamType.Image) != null;
			var mentionSpamPrevention = guildInfo.GlobalSpamPrevention.GetSpamPrevention(SpamType.Mention) != null;
			var reactionSpamPrevention = guildInfo.GlobalSpamPrevention.GetSpamPrevention(SpamType.Reaction) != null;
			var welcomeMessage = guildInfo.WelcomeMessage != null;
			var goodbyeMessage = guildInfo.GoodbyeMessage != null;
			var prefix = !String.IsNullOrWhiteSpace(guildInfo.Prefix);
			var serverlog = guildInfo.ServerLog != null;
			var modlog = guildInfo.ModLog != null;
			var imagelog = guildInfo.ImageLog != null;

			//Formatting the description
			var description = "";
			description += String.Format("**Command Preferences:** `{0}`\n", commandPreferences ? "Yes" : "No");
			description += String.Format("**Commands Disabled On User:** `{0}`\n", commandsDisabledOnUser ? "Yes" : "No");
			description += String.Format("**Commands Disabled On Role:** `{0}`\n", commandsDisabledOnRole ? "Yes" : "No");
			description += String.Format("**Commands Disabled On Channel:** `{0}`\n", commandsDisabledOnChannel ? "Yes" : "No");
			description += String.Format("**Bot Users:** `{0}`\n", botUsers ? "Yes" : "No");
			description += String.Format("**Self Assignable Roles:** `{0}`\n", selfAssignableGroups ? "Yes" : "No");
			description += String.Format("**Reminds:** `{0}`\n", reminds ? "Yes" : "No");
			description += String.Format("**Ignored Command Channels:** `{0}`\n", ignoredCommandChannels ? "Yes" : "No");
			description += String.Format("**Ignored Log Channels:** `{0}`\n", ignoredLogChannels ? "Yes" : "No");
			description += String.Format("**Image Only Channels:** `{0}`\n", imageOnlyChannels ? "Yes" : "No");
			description += String.Format("**Log Actions:** `{0}`\n", logActions ? "Yes" : "No");
			description += String.Format("**Banned Phrase Strings:** `{0}`\n", bannedPhraseStrings ? "Yes" : "No");
			description += String.Format("**Banned Phrase Regex:** `{0}`\n", bannedPhraseRegex ? "Yes" : "No");
			description += String.Format("**Banned Phrase Punishments:** `{0}`\n", bannedPhrasePunishments ? "Yes" : "No");
			description += String.Format("**Message Spam Prevention:** `{0}`\n", messageSpamPrevention ? "Yes" : "No");
			description += String.Format("**Long Message Spam Prevention:** `{0}`\n", longMessageSpamPrevention ? "Yes" : "No");
			description += String.Format("**Link Spam Prevention:** `{0}`\n", linkSpamPrevention ? "Yes" : "No");
			description += String.Format("**Image Spam Prevention:** `{0}`\n", imageSpamPrevention ? "Yes" : "No");
			description += String.Format("**Mention Spam Prevention:** `{0}`\n", mentionSpamPrevention ? "Yes" : "No");
			description += String.Format("**Welcome Message:** `{0}`\n", welcomeMessage ? "Yes" : "No");
			description += String.Format("**Goodbye Message:** `{0}`\n", goodbyeMessage ? "Yes" : "No");
			description += String.Format("**Prefix:** `{0}`\n", prefix ? "Yes" : "No");
			description += String.Format("**Server Log:** `{0}`\n", serverlog ? "Yes" : "No");
			description += String.Format("**Mod Log:** `{0}`\n", modlog ? "Yes" : "No");
			description += String.Format("**Image Log:** `{0}`\n", imagelog ? "Yes" : "No");

			return description;
		}

		public static string FormatAllSettings(BotGlobalInfo botInfo)
		{
			//TODO: Put rest of stuff in here
			var prefStr = String.Format("**Prefix:** `{0}`", String.IsNullOrWhiteSpace(botInfo.Prefix) ? "N/A" : botInfo.Prefix);
			var shardStr = String.Format("**Shards:** `{0}`", botInfo.ShardCount);
			var saveStr = String.Format("**Save Path:** `{0}`", String.IsNullOrWhiteSpace(Properties.Settings.Default.Path) ? "N/A" : Properties.Settings.Default.Path);
			var ownerStr = String.Format("**Bot Owner ID:** `{0}`", String.IsNullOrWhiteSpace(botInfo.BotOwnerID.ToString()) ? "N/A" : botInfo.BotOwnerID.ToString());
			var streamStr = String.Format("**Stream:** `{0}`", String.IsNullOrWhiteSpace(botInfo.Stream) ? "N/A" : botInfo.Stream);
			return String.Join("\n", new[] { prefStr, shardStr, saveStr, ownerStr, streamStr });
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
				var perms = (attr.Requirements & (1U << (int)Precondition.User_Has_A_Perm)) != 0;
				var guild = (attr.Requirements & (1U << (int)Precondition.Guild_Owner)) != 0;
				var trust = (attr.Requirements & (1U << (int)Precondition.Trusted_User)) != 0;
				var owner = (attr.Requirements & (1U << (int)Precondition.Bot_Owner)) != 0;

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

		public static void WriteLine(string text, [CallerMemberName] string name = "")
		{
			Console.WriteLine(String.Format("[{0}] [{1}]: {2}", DateTime.Now.ToString("HH:mm:ss"), name, ReplaceMarkdownChars(text)));
		}

		public static void ExceptionToConsole(Exception e, [CallerMemberName] string name = "")
		{
			if (e == null)
				return;

			WriteLine("EXCEPTION: " + e, name);
		}

		public static void WriteLoadDone(IGuild guild, string method, string name)
		{
			Variables.Guilds[guild.Id].TurnDefaultPrefsOff();
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

		public static async Task<BotInvite> GetInviteUserJoinedOn(IGuild guild)
		{
			//Get the current invites
			var curInvs = await GetInvites(guild);
			if (curInvs == null)
				return null;
			//Get the bot's stored invites
			var botInvs = Variables.Guilds[guild.Id].Invites;
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
		public static async Task WriteAndUploadTextFile(IGuild guild, IMessageChannel channel, List<string> textList, string fileName, string messageHeader)
		{
			//Messages in the format to upload
			var text = ReplaceMarkdownChars(String.Join("\n-----\n", textList));
			await WriteAndUploadTextFile(guild, channel, text, fileName, messageHeader);
		}

		public static async Task WriteAndUploadTextFile(IGuild guild, IMessageChannel channel, string text, string fileName, string fileMessage = null)
		{
			//Get the file path
			var file = fileName + DateTime.UtcNow.ToString("MM-dd_HH-mm-ss") + Constants.GENERAL_FILE_EXTENSION;
			var path = GetServerFilePath(guild.Id, file);
			if (path == null)
				return;

			//Double check that all markdown characters are removed
			text = ReplaceMarkdownChars(text);
			fileMessage = String.IsNullOrWhiteSpace(fileMessage) ? "" : String.Format("**{0}:**", fileMessage);

			using (var writer = new StreamWriter(path))
			{
				writer.WriteLine(text);
			}
			await channel.SendFileAsync(path, fileMessage);
			File.Delete(path);
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
			Task.Run(async () =>
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
				var typing = context.Channel.EnterTypingState();

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
				typing.Dispose();
				await DeleteMessage(msg);
				await SendChannelMessage(context, String.Format("Successfully changed the {0} icon.", user ? "bot" : "guild"));
			}).Forget();
		}

		public static async Task SendPotentiallyBigEmbed(IGuild guild, IMessageChannel channel, EmbedBuilder embed, string input, string fileName)
		{
			//Send the embed
			await SendEmbedMessage(channel, embed);

			//If the description is the too long message then upload the string
			if (embed.Description == Constants.HASTEBIN_ERROR)
			{
				//Send the file
				await WriteAndUploadTextFile(guild, channel, input, fileName);
			}
		}

		public static async Task SendPotentiallyBigMessage(IGuild guild, IMessageChannel channel, string error, string input, string fileName)
		{
			await SendChannelMessage(channel, error);

			if (error == Constants.HASTEBIN_ERROR)
			{
				await WriteAndUploadTextFile(guild, channel, input, fileName);
			}
		}

		public static bool TryToUploadToHastebin(string text, out string output)
		{
			//Check its length
			if (text.Length > Constants.MAX_LENGTH_FOR_HASTEBIN)
			{
				output = Constants.HASTEBIN_ERROR;
				return false;
			}

			//Regex for Getting the key out
			var hasteKeyRegex = new Regex(@"{""key"":""(?<key>[a-z].*)""}", RegexOptions.Compiled);

			//Upload the messages
			using (var client = new WebClient())
			{
				try
				{
					output = String.Concat("https://hastebin.com/raw/", hasteKeyRegex.Match(client.UploadString("https://hastebin.com/documents", ReplaceMarkdownChars(text))).Groups["key"]);
				}
				catch (Exception e)
				{
					output = e.Message;
					return false;
				}
			}
			return true;
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

		public static bool VerifyLoggingIsEnabledOnThisChannel(BotGuildInfo guildInfo, IMessage message)
		{
			return !guildInfo.IgnoredLogChannels.Contains(message.Channel.Id);
		}

		public static bool VerifyLoggingAction(BotGuildInfo guildInfo, LogActions logAction)
		{
			return guildInfo.LogActions.Contains(logAction);
		}

		public static bool VerifyLogging(BotGuildInfo guildInfo, IMessage message, LogActions logAction)
		{
			var shouldBeLogged = VerifyMessageShouldBeLogged(guildInfo, message);
			var loggingAction = VerifyLoggingAction(guildInfo, logAction);
			var unpaused = VerifyUnpaused();
			return shouldBeLogged && loggingAction && unpaused;
		}

		public static bool VerifyLogging(BotGuildInfo guildInfo, LogActions logAction)
		{
			var loggingAction = VerifyLoggingAction(guildInfo, logAction);
			var unpaused = VerifyUnpaused();
			return loggingAction && unpaused;
		}

		public static bool VerifyUnpaused()
		{
			return !Variables.Pause;
		}

		public static bool VerifyMessageShouldBeLogged(BotGuildInfo guildInfo, IMessage message)
		{
			if (message == null)
				return false;
			else if (message.Author.IsWebhook)
				return false;
			else if (message.Author.IsBot && message.Author.Id != Variables.BotID)
				return false;
			else if (!VerifyLoggingIsEnabledOnThisChannel(guildInfo, message))
				return false;
			return true;
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

		public static async Task EnablePreferences(BotGuildInfo guildInfo, IGuild guild, IUserMessage message)
		{
			if (!guildInfo.EnablingPrefs)
				return;

			var path = GetServerFilePath(guild.Id, Constants.GUILD_INFO_LOCATION);
			if (path == null)
			{
				await MakeAndDeleteSecondaryMessage(message.Channel, message, ERROR(Constants.PATH_ERROR));
				return;
			}
			if (!File.Exists(path))
			{
				var cmds = guildInfo.CommandOverrides.Commands.Select(x => x.Name).ToList();
				Variables.HelpList.Where(x => !cmds.CaseInsContains(x.Name)).ToList().ForEach(x => guildInfo.CommandOverrides.Commands.Add(new CommandSwitch(x.Name, x.DefaultEnabled)));
				SaveGuildInfo(guildInfo);
			}
			else
			{
				await MakeAndDeleteSecondaryMessage(message.Channel, message, "Preferences are already turned on.");
				guildInfo.SwitchEnablingPrefs();
				return;
			}

			guildInfo.SwitchEnablingPrefs();
			guildInfo.TurnDefaultPrefsOff();
			await SendChannelMessage(message.Channel, "Successfully created the preferences for this guild.");
		}
		
		public static async Task DeletePreferences(BotGuildInfo guildInfo, IGuild guild, IUserMessage message)
		{
			if (!guildInfo.DeletingPrefs)
				return;

			var path = GetServerFilePath(guild.Id, Constants.GUILD_INFO_LOCATION);
			if (path == null)
			{
				await MakeAndDeleteSecondaryMessage(message.Channel, message, ERROR(Constants.PATH_ERROR));
				return;
			}
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			else
			{
				await MakeAndDeleteSecondaryMessage(message.Channel, message, "The preferences file has already been deleted.");
				guildInfo.SwitchEnablingPrefs();
				return;
			}

			guildInfo.SwitchDeletingPrefs();
			guildInfo.TurnDefaultPrefsOn();
			await SendChannelMessage(message.Channel, "Successfully deleted the stored preferences for this guild.");
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
			//Reset everything but shards since shards are pretty important
			var botInfo = Variables.BotInfo;
			botInfo.ResetAll();
			SaveBotInfo();

			Properties.Settings.Default.Reset();
			Properties.Settings.Default.Save();
		}
		#endregion

		#region Slowmode/Banned Phrases/Spam Prevention
		public static async Task<bool> HandleSpamPrevention(GlobalSpamPrevention global, BaseSpamPrevention spamPrev, IGuild guild, IGuildUser user, IMessage msg)
		{
			if (spamPrev == null || !spamPrev.Enabled)
				return false;

			//Get the user from the list or, if not found, create a new one
			var spUser = Variables.Guilds[guild.Id].GlobalSpamPrevention.SpamPreventionUsers.FirstOrDefault(x => x.User == user);
			if (spUser == null)
			{
				spUser = new SpamPreventionUser(user);
				global.SpamPreventionUsers.ThreadSafeAdd(spUser);
			}
			//Add one to the count of the spam type they triggered and check if the user should be kicked/banned
			await spUser.CheckIfShouldKick(spamPrev, msg);
			return true;
		}

		public static async Task<bool> SpamCheck(GlobalSpamPrevention global, IGuild guild, IGuildUser author, IMessage msg)
		{
			if (global == null)
				return false;

			var isSpam = false;
			await global.SpamPreventions.Values.ToList().ForEachAsync(async x =>
			{
				if (x?.SpamType == null)
					return;

				switch (x.SpamType)
				{
					case SpamType.Message:
					{
						var curSpamTest = false;
						isSpam = isSpam || curSpamTest;
						if (curSpamTest)
						{
							await HandleSpamPrevention(global, x, guild, author, msg);
						}
						break;
					}
					case SpamType.Long_Message:
					{
						var curSpamTest = msg.Content.Length > x.AmountOfSpam;
						isSpam = isSpam || curSpamTest;
						if (curSpamTest)
						{
							await HandleSpamPrevention(global, x, guild, author, msg);
						}
						break;
					}
					case SpamType.Link:
					{
						var curSpamTest = false;
						isSpam = isSpam || curSpamTest;
						if (curSpamTest)
						{
							await HandleSpamPrevention(global, x, guild, author, msg);
						}
						break;
					}
					case SpamType.Image:
					{
						var curSpamTest = false;
						isSpam = isSpam || curSpamTest;
						if (curSpamTest)
						{
							await HandleSpamPrevention(global, x, guild, author, msg);
						}
						break;
					}
					case SpamType.Mention:
					{
						var curSpamTest = msg.MentionedUserIds.Distinct().Count() > x.AmountOfSpam;
						isSpam = isSpam || curSpamTest;
						if (curSpamTest)
						{
							await HandleSpamPrevention(global, x, guild, author, msg);
						}
						break;
					}
					case SpamType.Reaction:
					{
						var curSpamTest = false;
						isSpam = isSpam || curSpamTest;
						if (curSpamTest)
						{
							await HandleSpamPrevention(global, x, guild, author, msg);
						}
						break;
					}
				}
			});

			return isSpam;
		}

		public static async Task VotesHigherThanRequiredAmount(BaseSpamPrevention spamPrev, SpamPreventionUser spUser, IMessage msg)
		{
			//Make sure they have the lowest vote count required to kick
			spUser.ChangeVotesRequired(spamPrev.VotesNeededForKick);
			//Turn on their ability to be kicked so they can be kicked
			spUser.EnablePotentialKick();
			//Send this message updating the amount of votes the user needs
			await MakeAndDeleteSecondaryMessage(msg.Channel, String.Format("The user `{0}` needs `{1}` votes to be kicked. Vote to kick them by mentioning them.",
				msg.Author.FormatUser(), spUser.VotesRequired - spUser.VotesToKick));
		}

		public static async Task Slowmode(BotGuildInfo guildInfo, IMessage message)
		{
			//Make a new SlowmodeUser
			var smUser = new SlowmodeUser();

			//Get SlowmodeUser from the guild ID
			if (guildInfo.SlowmodeGuild != null)
			{
				smUser = guildInfo.SlowmodeGuild.Users.FirstOrDefault(x => x.User.Id == message.Author.Id);
			}
			//If that fails, try to get it from the channel ID
			var channelSM = guildInfo.SlowmodeChannels.FirstOrDefault(x => x.ChannelID == message.Channel.Id);
			if (channelSM != null)
			{
				//Find a channel slowmode where the channel ID is the same as the message channel ID then get the user
				smUser = channelSM.Users.FirstOrDefault(x => x.User.Id == message.Author.Id);
			}

			//Once the user within the SlowmodeUser class isn't null then go through with slowmode
			if (smUser != null)
			{
				//Check if their messages allowed is above 0
				if (smUser.CurrentMessagesLeft > 0)
				{
					if (smUser.CurrentMessagesLeft == smUser.BaseMessages)
					{
						smUser.SetNewTime(DateTime.UtcNow.AddSeconds(smUser.Interval));
						lock (Variables.SlowmodeUsers)
						{
							Variables.SlowmodeUsers.Add(smUser);
						}
					}

					//Lower it by one
					smUser.LowerMessagesLeft();
				}
				//Else delete the message
				else
				{
					await DeleteMessage(message);
				}
			}
		}

		public static async Task AddSlowmodeUser(BotGuildInfo guildInfo, IGuildUser user)
		{
			//Check if the guild has slowmode enabled
			if (guildInfo.SlowmodeGuild != null)
			{
				//Get the variables out of a different user
				var messages = guildInfo.SlowmodeGuild.Users.FirstOrDefault().BaseMessages;
				var interval = guildInfo.SlowmodeGuild.Users.FirstOrDefault().Interval;

				//Add them to the list for the slowmode in this guild
				guildInfo.SlowmodeGuild.Users.Add(new SlowmodeUser(user, messages, messages, interval));
			}

			//Get a list of the IDs of the guild's channels
			var guildChannelIDList = (await user.Guild.GetTextChannelsAsync()).Select(x => x.Id);
			//Find if any of them are a slowmode channel
			var smChannels = guildInfo.SlowmodeChannels.Where(x => guildChannelIDList.Contains(x.ChannelID)).ToList();
			//If greater than zero, add the user to each one
			if (smChannels.Any())
			{
				smChannels.ForEach(x =>
				{
					//Get the variables out of a different user
					var messages = x.Users.FirstOrDefault().BaseMessages;
					var interval = x.Users.FirstOrDefault().Interval;

					//Add them to the list for the slowmode in this guild
					x.Users.Add(new SlowmodeUser(user, messages, messages, interval));
				});
			}
		}

		public static async Task BannedPhrases(BotGuildInfo guildInfo, IMessage message)
		{
			//TODO: Better bool here
			if ((message.Author as IGuildUser).GuildPermissions.Administrator)
				return;

			//Check if it has any banned words or regex
			var bannedPhrases = guildInfo.BannedPhrases;
			var phrase = bannedPhrases.Strings.FirstOrDefault(x => CaseInsIndexOf(message.Content, x.Phrase));
			if (phrase != null)
			{
				await BannedStringPunishments(guildInfo, message, phrase);
			}
			var regex = bannedPhrases.Regex.FirstOrDefault(x => Regex.IsMatch(message.Content, x.Phrase, RegexOptions.IgnoreCase, new TimeSpan(Constants.REGEX_TIMEOUT)));
			if (regex != null)
			{
				await BannedRegexPunishments(guildInfo, message, regex);
			}
		}

		public static async Task BannedStringPunishments(BotGuildInfo guildInfo, IMessage message, BannedPhrase<string> phrase)
		{
			await DeleteMessage(message);
			var user = message.Author as IGuildUser;
			var bpUser = guildInfo.BannedPhraseUsers.FirstOrDefault(x => x.User == user) ?? new BannedPhraseUser(user, guildInfo);

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

			switch (punishment.Punishment)
			{
				case PunishmentType.Kick:
				{
					//Check if can kick them
					if (GetUserPosition(user.Guild, user) > GetUserPosition(user.Guild, GetBot(user.Guild)))
						return;

					//Kick them
					await user.KickAsync();
					bpUser.ResetKickCount();

					//Send a message to the logchannel
					var logChannel = guildInfo.ServerLog;
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

					//Ban them
					await user.Guild.AddBanAsync(user);
					bpUser.ResetBanCount();

					//Send a message to the logchannel
					var logChannel = guildInfo.ServerLog;
					if (logChannel != null)
					{
						var embed = AddFooter(MakeNewEmbed(null, "**ID:** " + user.Id, Constants.BANN), "Banned Phrases Ban");
						await SendEmbedMessage(logChannel, AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl()));
					}
					break;
				}
				case PunishmentType.Role:
				{
					//Give them the role
					await GiveRole(user, punishment.Role);
					bpUser.ResetRoleCount();

					//If a time is specified, run through the time then remove the role
					if (punishment.PunishmentTime != null)
					{
						Variables.PunishedUsers.Add(new RemovablePunishment(guildInfo.Guild, user.Id, punishment.Role, DateTime.UtcNow.AddMinutes((int)punishment.PunishmentTime)));
					}

					//Send a message to the logchannel
					var logChannel = guildInfo.ServerLog;
					if (logChannel != null)
					{
						var embed = AddFooter(MakeNewEmbed(null, "**Role Gained:** " + punishment.Role.Name, Constants.UEDT), "Banned Phrases Role");
						await SendEmbedMessage(logChannel, AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl()));
					}
					break;
				}
			}
		}

		public static async Task BannedRegexPunishments(BotGuildInfo guildInfo, IMessage message, BannedPhrase<string> regex)
		{
			await DeleteMessage(message);
			var user = message.Author as IGuildUser;
			var bpUser = guildInfo.BannedPhraseUsers.FirstOrDefault(x => x.User == user) ?? new BannedPhraseUser(user, guildInfo);

			var amountOfMsgs = 0;
			switch (regex.Punishment)
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
			if (!TryGetPunishment(guildInfo, regex.Punishment, amountOfMsgs, out BannedPhrasePunishment punishment))
				return;

			switch (punishment.Punishment)
			{
				case PunishmentType.Kick:
				{
					//Check if can kick them
					if (GetUserPosition(user.Guild, user) > GetUserPosition(user.Guild, GetBot(user.Guild)))
						return;

					//Kick them
					await user.KickAsync();

					//Send a message to the logchannel
					var logChannel = guildInfo.ServerLog;
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

					//Ban them
					await user.Guild.AddBanAsync(message.Author);

					//Send a message to the logchannel
					var logChannel = guildInfo.ServerLog;
					if (logChannel != null)
					{
						var embed = AddFooter(MakeNewEmbed(null, "**ID:** " + user.Id, Constants.BANN), "Banned Phrases Ban");
						await SendEmbedMessage(logChannel, AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl()));
					}
					break;
				}
				case PunishmentType.Role:
				{
					//Give them the role
					await GiveRole(user, punishment.Role);

					//If a time is specified, run through the time then remove the role
					if (punishment.PunishmentTime != null)
					{
						Variables.PunishedUsers.Add(new RemovablePunishment(guildInfo.Guild, user.Id, punishment.Role, DateTime.UtcNow.AddMinutes((int)punishment.PunishmentTime)));
					}

					//Send a message to the logchannel
					var logChannel = guildInfo.ServerLog;
					if (logChannel != null)
					{
						var embed = AddFooter(MakeNewEmbed(null, "**Gained:** " + punishment.Role.Name, Constants.UEDT), "Banned Phrases Role");
						await SendEmbedMessage(logChannel, AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl()));
					}
					break;
				}
			}
		}

		public static bool TryGetPunishment(BotGuildInfo guildInfo, PunishmentType type, int msgs, out BannedPhrasePunishment punishment)
		{
			punishment = guildInfo.BannedPhrases.Punishments.Where(x => x.Punishment == type).FirstOrDefault(x => x.NumberOfRemoves == msgs);
			return punishment != null;
		}

		public static bool TryGetBannedRegex(BotGuildInfo guildInfo, string searchPhrase, out BannedPhrase<string> bannedRegex)
		{
			bannedRegex = guildInfo.BannedPhrases.Regex.FirstOrDefault(x => CaseInsEquals(x.Phrase, searchPhrase));
			return bannedRegex != null;
		}

		public static bool TryGetBannedString(BotGuildInfo guildInfo, string searchPhrase, out BannedPhrase<string> bannedString)
		{
			bannedString = guildInfo.BannedPhrases.Strings.FirstOrDefault(x => CaseInsEquals(x.Phrase, searchPhrase));
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

		public static void HandleBannedRegexModification(List<BannedPhrase<Regex>> bannedRegex, List<string> inputPhrases, bool add, out List<string> success, out List<string> failure)
		{
			success = new List<string>();
			failure = new List<string>();
			if (add)
			{
				foreach (var str in inputPhrases)
				{
					if (TryCreateRegex(str, out Regex regex, out string error))
					{
						bannedRegex.Add(new BannedPhrase<Regex>(regex, PunishmentType.Nothing));
						success.Add(str);
					}
					else
					{
						failure.Add(error);
					}
				}
			}
			else
			{
				var positions = new List<int>();
				inputPhrases.ForEach(potentialNumber =>
				{
					//Check if is a number and is less than the count of the list
					if (int.TryParse(potentialNumber, out int temp) && temp < bannedRegex.Count)
					{
						positions.Add(temp);
					}
				});

				if (!positions.Any())
				{
					foreach (var str in inputPhrases)
					{
						var tempRegex = bannedRegex.FirstOrDefault(y => y.Phrase.ToString() == str);
						if (tempRegex == null)
						{
							failure.Add(str);
						}
						else
						{
							success.Add(str);
							bannedRegex.Remove(tempRegex);
						}
					}
				}
				else
				{
					//Put them in descending order so as to not delete low values before high ones
					foreach (var position in positions.OrderByDescending(x => x))
					{
						if (bannedRegex.Count - 1 <= position)
						{
							var tempRegex = bannedRegex[position];
							if (tempRegex != null)
							{
								bannedRegex.Remove(tempRegex);
								success.Add(tempRegex.Phrase.ToString());
								continue;
							}
						}
						else
						{
							failure.Add("Regex at position " + position);
						}
					}
				}
			}

			return;
		}

		public static void HandleBannedStringModification(List<BannedPhrase<string>> bannedStrings, List<string> inputPhrases, bool add, out List<string> success, out List<string> failure)
		{
			success = new List<string>();
			failure = new List<string>();
			if (add)
			{
				foreach (var str in inputPhrases)
				{
					bannedStrings.Add(new BannedPhrase<string>(str, PunishmentType.Nothing));
					success.Add(str);
				}
			}
			else
			{
				var positions = new List<int>();
				inputPhrases.ForEach(potentialNumber =>
				{
					//Check if is a number and is less than the count of the list
					if (int.TryParse(potentialNumber, out int temp) && temp < bannedStrings.Count)
					{
						positions.Add(temp);
					}
				});

				if (!positions.Any())
				{
					foreach (var str in inputPhrases)
					{
						var tempString = bannedStrings.FirstOrDefault(y => y.Phrase.ToString() == str);
						if (tempString == null)
						{
							failure.Add(str);
						}
						else
						{
							success.Add(str);
							bannedStrings.Remove(tempString);
						}
					}
				}
				else
				{
					//Put them in descending order so as to not delete low values before high ones
					foreach (var position in positions.OrderByDescending(x => x))
					{
						if (bannedStrings.Count - 1 <= position)
						{
							var tempString = bannedStrings[position];
							if (tempString != null)
							{
								bannedStrings.Remove(tempString);
								success.Add(tempString.Phrase);
								continue;
							}
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

			return closeHelps;
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
			List<T> eligibleToBeGotten;
			lock (inputList)
			{
				eligibleToBeGotten = inputList.Where(x => x.GetTime() <= DateTime.UtcNow).ToList();
				inputList.RemoveAll(x => eligibleToBeGotten.Contains(x));
			}
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
			Variables.SpamTimer = new Timer(HourTimer, null, time, Timeout.Infinite);
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
			Variables.RemovePunishmentTimer = new Timer(MinuteTimer, null, time, Timeout.Infinite);
		}

		public static void OneFourthSecondTimer(object obj)
		{
			DeleteTargettedMessages();
			RemoveActiveCloseHelpAndWords();
			ActivateGuildToggles();
			ResetSMUserMessages();

			const long PERIOD = 250;
			var time = PERIOD;
			if ((DateTime.UtcNow.Subtract(Variables.StartupTime)).TotalSeconds < 1)
			{
				time -= (long)DateTime.UtcNow.TimeOfDay.TotalMilliseconds % PERIOD;
			}
			Variables.RemovePunishmentTimer = new Timer(OneFourthSecondTimer, null, time, Timeout.Infinite);
		}

		public static void ClearPunishedUsersList()
		{
			Variables.Guilds.Values.ToList().ForEach(x => x.GlobalSpamPrevention.SpamPreventionUsers.Clear());
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
				await DeleteMessage(timed.Message);
				await DeleteMessages(timed.Messages.FirstOrDefault().Channel, timed.Messages);
			});
		}

		public static void RemoveActiveCloseHelpAndWords()
		{
			GetOutTimedObject(Variables.ActiveCloseHelp);
			GetOutTimedObject(Variables.ActiveCloseWords);
		}

		public static void ActivateGuildToggles()
		{
			GetOutTimedObject(Variables.GuildToggles).ForEach(x =>
			{
				switch (x.Toggle)
				{
					case GuildToggle.EnablePrefs:
					{
						Variables.Guilds[x.GuildID].SwitchEnablingPrefs();
						break;
					}
					case GuildToggle.DeletePrefs:
					{
						Variables.Guilds[x.GuildID].SwitchDeletingPrefs();
						break;
					}
				}
			});
		}

		public static void ResetSMUserMessages()
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
		public static async Task<GuildNotification> MakeGuildNotification(ICommandContext context, string input)
		{
			//Get the variables out
			var inputArray = SplitByCharExceptInQuotes(input, ' ');
			var channelStr = inputArray[0];
			var content = GetVariable(inputArray, "content");
			var title = GetVariable(inputArray, "title");
			var desc = GetVariable(inputArray, "desc");
			var thumb = GetVariable(inputArray, "thumb");
			thumb = ValidateURL(thumb) ? thumb : null;

			//Check if everything is null
			var contentB = String.IsNullOrWhiteSpace(content);
			var titleB = String.IsNullOrWhiteSpace(title);
			var descB = String.IsNullOrWhiteSpace(desc);
			var thumbB = String.IsNullOrWhiteSpace(thumb);
			if (contentB && titleB && descB && thumbB)
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR("One of the variables has to be given."));
				return null;
			}

			//Make sure the channel mention is valid
			var returnedChannel = GetChannel(context, new[] { ChannelCheck.Can_Modify_Permissions, ChannelCheck.Is_Text }, true, channelStr);
			if (returnedChannel.Reason != FailureReason.Not_Failure)
			{
				await HandleObjectGettingErrors(context, returnedChannel);
				return null;
			}
			var channel = returnedChannel.Object as ITextChannel;

			return new GuildNotification(content, title, desc, thumb, context.Guild.Id, channel.Id);
		}

		public static async Task UpdateGame()
		{
			var botInfo = Variables.BotInfo;
			var gameStr = String.IsNullOrWhiteSpace(botInfo.Game) ? String.Format("type \"{0}help\" for help.", botInfo.Prefix) : botInfo.Game;
			var strmStr = botInfo.Stream;

			if (String.IsNullOrWhiteSpace(strmStr))
			{
				await Variables.Client.SetGameAsync(gameStr, strmStr, StreamType.NotStreaming);
			}
			else
			{
				await Variables.Client.SetGameAsync(gameStr, Constants.STREAM_URL + strmStr, StreamType.Twitch);
			}
		}

		public static FAWRType ClarifyFAWRType(FAWRType type)
		{
			switch (type)
			{
				case FAWRType.GR:
				{
					return FAWRType.Give_Role;
				}
				case FAWRType.TR:
				{
					return FAWRType.Take_Role;
				}
				case FAWRType.GNN:
				{
					return FAWRType.Give_Nickname;
				}
				case FAWRType.TNN:
				{
					return FAWRType.Take_Nickname;
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
			}).SelectMany(element => element).Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
		}

		public static bool CheckIfRegMatch(string msg, string pattern)
		{
			return Regex.IsMatch(msg, pattern, RegexOptions.IgnoreCase, new TimeSpan(Constants.REGEX_TIMEOUT));
		}

		public static void HandleBotID(ulong ID)
		{
			Variables.BotID = ID;
			Properties.Settings.Default.BotID = ID;
			Properties.Settings.Default.Save();
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
				Actions.ExceptionToConsole(e);
			}
		}

		public static void DisconnectBot()
		{
			Environment.Exit(0);
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

		public static List<T> GetUpToXElement<T>(this List<T> list, int x)
		{
			return list.GetRange(0, Math.Min(list.Count, x));
		}

		public static string FormatUser(this IUser user, ulong? userID = 0)
		{
			user = user ?? Variables.Client.GetUser((ulong)userID);
			if (user != null)
			{
				return String.Format("'{0}#{1}' ({2})", user.Username, user.Discriminator, user.Id);
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
				return String.Format("'{0}' ({1})", role.Name, role.Id);
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
				return String.Format("'{0}' ({1}) ({2})", channel.Name, Actions.GetChannelType(channel), channel.Id);
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
				return String.Format("'{0}' ({1})", guild.Name, guild.Id);
			}
			else
			{
				return String.Format("Irretrievable Guild ({0})", guildID); 
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