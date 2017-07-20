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
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Advobot
{
	namespace Actions
	{
		public static class SavingAndLoading
		{
			public static async Task LoadInformation(IDiscordClient client, IBotSettings botSettings, IGuildSettingsModule guildSettingsModule)
			{
				if (botSettings.Loaded)
					return;

				HandleBotID(client.CurrentUser.Id);
				HandleBotName(client.CurrentUser.Username);
				if (botSettings.FirstInstanceOfBotStartingUpWithCurrentKey)
				{
					Misc.RestartBot(); //Restart so the bot can get the correct globalInfo loaded
				}

				await ClientActions.SetGame(client, botSettings);

				ConsoleActions.WriteLine("The current bot prefix is: " + botSettings.Prefix);
				ConsoleActions.WriteLine(String.Format("Bot took {0:n} milliseconds to load everything.", TimeSpan.FromTicks(DateTime.UtcNow.ToUniversalTime().Ticks - botSettings.StartupTime.Ticks).TotalMilliseconds));
				botSettings.SetLoaded();
			}
			public static void HandleBotID(ulong ID)
			{
				Properties.Settings.Default.BotID = ID;
				Properties.Settings.Default.Save();
			}
			public static void HandleBotName(string name)
			{
				Properties.Settings.Default.BotName = name;
				Properties.Settings.Default.Save();
			}

			public static async Task<bool> ValidateBotKey(IDiscordClient client, string input, bool startup = false)
			{
				var key = input?.Trim();

				if (startup)
				{
					if (!String.IsNullOrWhiteSpace(input))
					{
						try
						{
							await ClientActions.Login(client, key);
							return true;
						}
						catch (Exception)
						{
							ConsoleActions.WriteLine("The given key is no longer valid. Please enter a new valid key:");
						}
					}
					else
					{
						ConsoleActions.WriteLine("Please enter the bot's key:");
					}
					return false;
				}

				if (key.Length > Constants.VALID_KEY_LENGTH)
				{
					ConsoleActions.WriteLine("The given key is too long. Please enter a regular length key:");
				}
				else if (key.Length < Constants.VALID_KEY_LENGTH)
				{
					ConsoleActions.WriteLine("The given key is too short. Please enter a regular length key:");
				}
				else
				{
					try
					{
						await ClientActions.Login(client, key);

						ConsoleActions.WriteLine("Succesfully logged in via the given bot key.");
						Properties.Settings.Default.BotKey = key;
						Properties.Settings.Default.Save();
						return true;
					}
					catch (Exception)
					{
						ConsoleActions.WriteLine("The given key is invalid. Please enter a valid key:");
					}
				}

				return false;
			}
			public static bool ValidatePath(string input, bool windows, bool startup = false)
			{
				var path = input?.Trim();

				if (startup)
				{
					if (!String.IsNullOrWhiteSpace(path) && Directory.Exists(path))
					{
						Properties.Settings.Default.Path = path;
						Properties.Settings.Default.Save();
						return true;
					}

					if (windows)
					{
						ConsoleActions.WriteLine("Please enter a valid directory path in which to save files or say 'AppData':");
					}
					else
					{
						ConsoleActions.WriteLine("Please enter a valid directory path in which to save files:");
					}
					return false;
				}

				if (windows && "appdata".CaseInsEquals(path))
				{
					path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				}

				if (Directory.Exists(path))
				{
					ConsoleActions.WriteLine("Successfully set the save path as " + path);
					Properties.Settings.Default.Path = path;
					Properties.Settings.Default.Save();
					return true;
				}

				ConsoleActions.WriteLine("Invalid directory. Please enter a valid directory:");
				return false;
			}

			public static IBotSettings CreateBotSettings(Type globalSettingType, bool windows, bool console, bool firstInstance)
			{
				if (globalSettingType == null || !globalSettingType.GetInterfaces().Contains(typeof(IBotSettings)))
				{
					throw new ArgumentException("Invalid type for global settings provided.");
				}

				IBotSettings botSettings = null;
				var path = Gets.GetBaseBotDirectory(Constants.BOT_SETTINGS_LOCATION);
				if (File.Exists(path))
				{
					try
					{
						using (var reader = new StreamReader(path))
						{
							botSettings = (IBotSettings)JsonConvert.DeserializeObject(reader.ReadToEnd(), globalSettingType);
						}
						ConsoleActions.WriteLine("The bot information has successfully been loaded.");
					}
					catch (Exception e)
					{
						ConsoleActions.ExceptionToConsole(e);
					}
				}
				else if (!firstInstance)
				{
					ConsoleActions.WriteLine("The bot information file could not be found; using default.");
				}
				botSettings = botSettings ?? (IBotSettings)Activator.CreateInstance(globalSettingType);

				if (botSettings is MyBotSettings)
				{
					(botSettings as MyBotSettings).PostDeserialize(windows, console, firstInstance);
				}

				return botSettings;
			}
			public static CriticalInformation LoadCriticalInformation()
			{
				HandleBotID(Properties.Settings.Default.BotID);

				bool windows;
				{
					var windir = Environment.GetEnvironmentVariable("windir");
					windows = !String.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir);
				}
				bool console;
				{
					try
					{
						var window_height = Console.WindowHeight;
						console = true;
					}
					catch
					{
						console = false;
					}
				}
				bool firstInstance = Properties.Settings.Default.BotID == 0;

				return new CriticalInformation(windows, console, firstInstance);
			}

			public static List<HelpEntry> LoadHelpList()
			{
				var temp = new List<HelpEntry>();
				foreach (var classType in Assembly.GetCallingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(MyModuleBase))))
				{
					var innerMostNameSpace = classType.Namespace.Substring(classType.Namespace.LastIndexOf('.') + 1);
					if (!Enum.TryParse(innerMostNameSpace, true, out CommandCategory category))
					{
#if FALSE
						Messages.WriteLine(innerMostNameSpace + " is not currently in the CommandCategory enum.");
#endif
						continue;
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
					var permReqs = permReqsAttr == null ? null : Formatting.FormatAttribute(permReqsAttr);

					var otherReqsAttr = (OtherRequirementAttribute)classType.GetCustomAttribute(typeof(OtherRequirementAttribute));
					var otherReqs = otherReqsAttr == null ? null : Formatting.FormatAttribute(otherReqsAttr);

					var defaultEnabledAttr = (DefaultEnabledAttribute)classType.GetCustomAttribute(typeof(DefaultEnabledAttribute));
					var defaultEnabled = defaultEnabledAttr == null ? false : defaultEnabledAttr.Enabled;
					if (defaultEnabledAttr == null)
					{
						throw new InvalidOperationException("Command does not have a default enabled value set: " + name);
					}

					var similarCmds = temp.Where(x => x.Name.CaseInsEquals(name) || (x.Aliases != null && aliases != null && x.Aliases.Intersect(aliases, StringComparer.OrdinalIgnoreCase).Any()));
					if (similarCmds.Any())
					{
						throw new ArgumentException(String.Format("The following commands have conflicts: {0} + {1}", String.Join(" + ", similarCmds.Select(x => x.Name)), name));
					}

					temp.Add(new HelpEntry(name, aliases, usage, Formatting.JoinNonNullStrings(" | ", new[] { permReqs, otherReqs }), summary, category, defaultEnabled));
				}
				return temp;
			}
			public static List<string> LoadCommandNames(IEnumerable<HelpEntry> helpEntries)
			{
				return helpEntries.Select(x => x.Name).ToList();
			}
			public static List<BotGuildPermission> LoadGuildPermissions()
			{
				var temp = new List<BotGuildPermission>();
				for (int i = 0; i < 64; ++i)
				{
					var name = Enum.GetName(typeof(GuildPermission), i);
					if (name == null)
						continue;

					temp.Add(new BotGuildPermission(name, i));
				}
				return temp;
			}
			public static List<BotChannelPermission> LoadChannelPermissions()
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

				var temp = new List<BotChannelPermission>();
				for (int i = 0; i < 64; ++i)
				{
					var name = Enum.GetName(typeof(ChannelPermission), i);
					if (name == null)
						continue;

					if ((GENERAL_BITS & (1U << i)) != 0)
					{
						temp.Add(new BotChannelPermission(name, i, gen: true));
					}
					if ((TEXT_BITS & (1U << i)) != 0)
					{
						temp.Add(new BotChannelPermission(name, i, text: true));
					}
					if ((VOICE_BITS & (1U << i)) != 0)
					{
						temp.Add(new BotChannelPermission(name, i, voice: true));
					}
				}
				return temp;
			}

			public static string Serialize(object obj)
			{
				return JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
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
		}

		public static class ClientActions
		{
			public static async Task MaybeStartBot(IDiscordClient client, IBotSettings botSettings)
			{
				if (botSettings.GotPath && botSettings.GotKey && !botSettings.Loaded)
				{
					ConsoleActions.WriteLine("Connecting the client...");

					try
					{
						await client.StartAsync();
						ConsoleActions.WriteLine("Successfully connected the client.");
					}
					catch (Exception e)
					{
						ConsoleActions.ExceptionToConsole(e);
					}

					await Task.Delay(-1);
				}
			}

			public static IDiscordClient CreateBotClient(IBotSettings botSettings)
			{
				IDiscordClient client;
				if (botSettings.ShardCount > 1)
				{
					client = CreateShardedClient(botSettings);
				}
				else
				{
					client = CreateSocketClient(botSettings);
				}
				return client;
			}
			public static DiscordShardedClient CreateShardedClient(IBotSettings botSettings)
			{
				return new DiscordShardedClient(new DiscordSocketConfig
				{
					AlwaysDownloadUsers = botSettings.AlwaysDownloadUsers,
					MessageCacheSize = (int)botSettings.MessageCacheCount,
					LogLevel = botSettings.LogLevel,
					TotalShards = (int)botSettings.ShardCount,
				});
			}
			public static DiscordSocketClient CreateSocketClient(IBotSettings botSettings)
			{
				return new DiscordSocketClient(new DiscordSocketConfig
				{
					AlwaysDownloadUsers = botSettings.AlwaysDownloadUsers,
					MessageCacheSize = (int)botSettings.MaxUserGatherCount,
					LogLevel = botSettings.LogLevel,
				});
			}

			public static async Task Login(IDiscordClient client, string key)
			{
				if (client is DiscordSocketClient)
				{
					await (client as DiscordSocketClient).LoginAsync(TokenType.Bot, key);
				}
				else if (client is DiscordShardedClient)
				{
					await (client as DiscordShardedClient).LoginAsync(TokenType.Bot, key);
				}
			}
			public static async Task SetGame(IDiscordClient client, IBotSettings botSettings)
			{
				var game = botSettings.Game;
				var stream = botSettings.Stream;
				var prefix = botSettings.Prefix;

				var streamType = StreamType.NotStreaming;
				if (!String.IsNullOrWhiteSpace(stream))
				{
					stream = Constants.TWITCH_URL + stream.Substring(stream.LastIndexOf('/') + 1);
					streamType = StreamType.Twitch;
				}

				if (client is DiscordSocketClient)
				{
					await (client as DiscordSocketClient).SetGameAsync(game ?? String.Format("type \"{0}help\" for help.", prefix), stream, streamType);
				}
				else if (client is DiscordShardedClient)
				{
					await (client as DiscordShardedClient).SetGameAsync(game ?? String.Format("type \"{0}help\" for help.", prefix), stream, streamType);
				}
			}
			public static int GetShardID(IDiscordClient client)
			{
				if (client is DiscordSocketClient)
				{
					return (client as DiscordSocketClient).ShardId;
				}
				else
				{
					return -1;
				}
			}
			public static int GetLatency(IDiscordClient client)
			{
				if (client is DiscordSocketClient)
				{
					return (client as DiscordSocketClient).Latency;
				}
				else if (client is DiscordShardedClient)
				{
					return (client as DiscordShardedClient).Latency;
				}
				else
				{
					return -1;
				}
			}
			public static int GetShardCount(IDiscordClient client)
			{
				if (client is DiscordSocketClient)
				{
					return 1;
				}
				else if (client is DiscordShardedClient)
				{
					return (client as DiscordShardedClient).Shards.Count;
				}
				else
				{
					return -1;
				}
			}
			public static int GetShardIdFor(IDiscordClient client, IGuild guild)
			{
				if (client is DiscordSocketClient)
				{
					return (client as DiscordSocketClient).ShardId;
				}
				else if (client is DiscordShardedClient)
				{
					return (client as DiscordShardedClient).GetShardIdFor(guild);
				}
				else
				{
					return -1;
				}
			}
		}

		public static class Gets
		{
			public static Dictionary<string, Color> GetColorDictionary()
			{
				var dict = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);
				foreach (var color in typeof(Color).GetFields().Where(x => x.IsPublic))
				{
					dict.Add(color.Name, (Color)color.GetValue(new Color()));
				}
				return dict;
			}

			public static string GetPlural(double i)
			{
				//Double allows most, if not all, number types in. https://stackoverflow.com/a/828963
				return i == 1 ? "" : "s";
			}

			public static Dictionary<string, string> GetChannelOverwritePermissions(Overwrite overwrite)
			{
				//Create a dictionary to hold the allow/deny/inherit values
				var channelPerms = new Dictionary<String, String>();

				//Make a copy of the channel perm list to check off perms as they go by
				var genericChannelPerms = Constants.CHANNEL_PERMISSIONS.Select(x => x.Name).ToList();

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
					Constants.CHANNEL_PERMISSIONS.Where(x => x.Voice).ToList().ForEach(x => dictionary.Remove(x.Name));
				}
				else
				{
					Constants.CHANNEL_PERMISSIONS.Where(x => x.Text).ToList().ForEach(x => dictionary.Remove(x.Name));
				}
				return dictionary;
			}
			public static List<string> GetPermissionNames(ulong flags)
			{
				var result = new List<string>();
				for (int i = 0; i < 64; ++i)
				{
					ulong bit = 1U << i;
					if ((flags & bit) != 0)
					{
						var name = Constants.GUILD_PERMISSIONS.FirstOrDefault(x => x.Bit == bit).Name;
						if (!String.IsNullOrWhiteSpace(name))
						{
							result.Add(name);
						}
					}
				}
				return result;
			}

			public static ReturnedArguments GetArgs(ICommandContext context, string input, int min, int max, string[] argsToSearchFor = null)
			{
				/* Non specified arguments get left in a list of args going left to right (mentions are not included in this if the bool is true).
					* Specified arguments get left in a dictionary.
					*/

				if (input == null)
				{
					var list = new List<string>();
					for (int i = 0; i < max; ++i)
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

				var args = input.SplitByCharExceptInQuotes(' ').ToList();
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

				for (int i = args.Count; i < max; ++i)
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

			public static List<CommandSwitch> GetMultipleCommands(IGuildSettings guildSettings, CommandCategory category)
			{
				return guildSettings.CommandSwitches.Where(x => x.Category == category).ToList();
			}
			public static CommandSwitch GetCommand(IGuildSettings guildSettings, string input)
			{
				return guildSettings.CommandSwitches.FirstOrDefault(x =>
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
			public static string[] GetCommands(CommandCategory category)
			{
				return Constants.HELP_ENTRIES.Where(x => x.Category == category).Select(x => x.Name).ToArray();
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

			public static string GetServerFilePath(ulong guildId, string fileName)
			{
				//Make sure the bot's directory exists
				var directory = GetBaseBotDirectory();
				Directory.CreateDirectory(directory);

				//This string will be similar to C:\Users\User\AppData\Roaming\Discord_Servers_... if using appdata. If not then it can be anything;
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
			public static FileType? GetFileType(string file)
			{
				if (Enum.TryParse(file, true, out FileType type))
				{
					return type;
				}
				return null;
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

			public static string GetUptime(IBotSettings botSettings)
			{
				var span = DateTime.UtcNow.Subtract(botSettings.StartupTime);
				return String.Format("{0}:{1}:{2}:{3}", span.Days, span.Hours.ToString("00"), span.Minutes.ToString("00"), span.Seconds.ToString("00"));
			}
			public static double GetMemory(bool windows)
			{
				if (windows)
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

			public static int GetMaxAmountOfUsersToGather(IBotSettings botSettings, bool bypass)
			{
				return bypass ? int.MaxValue : (int)botSettings.MaxUserGatherCount;
			}
		}

		public static class Guilds
		{
			public static async Task<IGuild> GetGuild(IDiscordClient client, ulong id)
			{
				return await client.GetGuildAsync(id);
			}

			public static ulong AddGuildPermissionBit(string permissionName, ulong inputValue)
			{
				var permission = Constants.GUILD_PERMISSIONS.FirstOrDefault(x => x.Name.CaseInsEquals(permissionName));
				if (!permission.Equals(default(BotGuildPermission)))
				{
					inputValue |= permission.Bit;
				}
				return inputValue;
			}
			public static ulong RemoveGuildPermissionBit(string permissionName, ulong inputValue)
			{
				var permission = Constants.GUILD_PERMISSIONS.FirstOrDefault(x => x.Name.CaseInsEquals(permissionName));
				if (!permission.Equals(default(BotGuildPermission)))
				{
					inputValue &= ~permission.Bit;
				}
				return inputValue;
			}
		}

		public static class Channels
		{
			public static ReturnedObject<IGuildChannel> GetChannel(ICommandContext context, ObjectVerification[] checkingTypes, bool mentions, string input)
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
						var channels = (context.Guild as SocketGuild).Channels.Where(x => x.Name.CaseInsEquals(input));
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

				if (channel == null && mentions)
				{
					var channelMentions = context.Message.MentionedChannelIds;
					if (channelMentions.Count() == 1)
					{
						channel = GetChannel(context.Guild, channelMentions.First());
					}
					else if (channelMentions.Count() > 1)
					{
						return new ReturnedObject<IGuildChannel>(channel, FailureReason.TooMany);
					}
				}

				return GetChannel(context, checkingTypes, channel);
			}
			public static ReturnedObject<IGuildChannel> GetChannel(ICommandContext context, ObjectVerification[] checkingTypes, ulong inputID)
			{
				return GetChannel(context, checkingTypes, GetChannel(context.Guild, inputID));
			}
			public static ReturnedObject<IGuildChannel> GetChannel(ICommandContext context, ObjectVerification[] checkingTypes, IGuildChannel channel)
			{
				return GetChannel(context.Guild, context.User as IGuildUser, checkingTypes, channel);
			}
			public static ReturnedObject<T> GetChannel<T>(IGuild guild, IGuildUser currUser, ObjectVerification[] checkingTypes, T channel) where T : IGuildChannel
			{
				checkingTypes.AssertEnumsAreAllCorrectTargetType(channel);
				if (channel == null)
				{
					return new ReturnedObject<T>(channel, FailureReason.TooFew);
				}

				var bot = Users.GetBot(guild);
				foreach (var type in checkingTypes)
				{
					if (!GetIfUserCanDoActionOnChannel(channel, currUser, type))
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

			public static async Task<int> ModifyChannelPosition(IGuildChannel channel, int position)
			{
				if (channel == null)
					return -1;

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
				for (int i = 0; i < channels.Length; ++i)
				{
					if (i > position)
					{
						reorderProperties[i] = new ReorderChannelProperties(channels[i - 1].Id, i);
					}
					else if (i < position)
					{
						reorderProperties[i] = new ReorderChannelProperties(channels[i].Id, i);
					}
					else
					{
						reorderProperties[i] = new ReorderChannelProperties(channel.Id, i);
					}
				}

				await channel.Guild.ReorderChannelsAsync(reorderProperties);
				return reorderProperties.FirstOrDefault(x => x.Id == channel.Id)?.Position ?? -1;
			}

			public static async Task ModifyOverwrite(IGuildChannel channel, object obj, ulong allowBits, ulong denyBits)
			{
				if (obj is IRole)
				{
					await channel.AddPermissionOverwriteAsync(obj as IRole, new OverwritePermissions(allowBits, denyBits));
				}
				else if (obj is IUser)
				{
					await channel.AddPermissionOverwriteAsync(obj as IUser, new OverwritePermissions(allowBits, denyBits));
				}
				else
				{
					throw new ArgumentException("Invalid object passed in. Must either be a role or a user.");
				}
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
			public static OverwritePermissions? GetOverwrite(IGuildChannel channel, object obj)
			{
				if (obj is IRole)
				{
					return channel.GetPermissionOverwrite(obj as IRole);
				}
				else if (obj is IUser)
				{
					return channel.GetPermissionOverwrite(obj as IUser);
				}
				else
				{
					throw new ArgumentException("Invalid object passed in. Must either be a role or a user.");
				}
			}
			public static ulong GetOverwriteAllowBits(IGuildChannel channel, object obj)
			{
				if (obj is IRole)
				{
					return channel.GetPermissionOverwrite(obj as IRole)?.AllowValue ?? 0;
				}
				else if (obj is IUser)
				{
					return channel.GetPermissionOverwrite(obj as IUser)?.AllowValue ?? 0;
				}
				else
				{
					throw new ArgumentException("Invalid object passed in. Must either be a role or a user.");
				}
			}
			public static ulong GetOverwriteDenyBits(IGuildChannel channel, object obj)
			{
				if (obj is IRole)
				{
					return channel.GetPermissionOverwrite(obj as IRole)?.DenyValue ?? 0;
				}
				else if (obj is IUser)
				{
					return channel.GetPermissionOverwrite(obj as IUser)?.DenyValue ?? 0;
				}
				else
				{
					throw new ArgumentException("Invalid object passed in. Must either be a role or a user.");
				}
			}
		}

		public static class Roles
		{
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

				if (role == null && mentions)
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

				return GetRole(context, checkingTypes, role);
			}
			public static ReturnedObject<IRole> GetRole(ICommandContext context, ObjectVerification[] checkingTypes, ulong inputID)
			{
				return GetRole(context, checkingTypes, GetRole(context.Guild, inputID));
			}
			public static ReturnedObject<IRole> GetRole(ICommandContext context, ObjectVerification[] checkingTypes, IRole role)
			{
				return GetRole(context.Guild, context.User as IGuildUser, checkingTypes, role);
			}
			public static ReturnedObject<T> GetRole<T>(IGuild guild, IGuildUser currUser, ObjectVerification[] checkingTypes, T role) where T : IRole
			{
				checkingTypes.AssertEnumsAreAllCorrectTargetType(role);
				if (role == null)
				{
					return new ReturnedObject<T>(role, FailureReason.TooFew);
				}

				var bot = Users.GetBot(guild);
				foreach (var type in checkingTypes)
				{
					if (!GetIfUserCanDoActionOnRole(role, currUser, type))
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
						return target.Position < Users.GetUserPosition(user);
					}
					default:
					{
						return true;
					}
				}
			}

			public static async Task<int> ModifyRolePosition(IRole role, int position)
			{
				if (role == null)
					return -1;

				var roles = role.Guild.Roles.Where(x => x.Id != role.Id && x.Position < Users.GetUserPosition(Users.GetBot(role.Guild))).OrderBy(x => x.Position).ToArray();
				position = Math.Max(1, Math.Min(position, roles.Length));

				var reorderProperties = new ReorderRoleProperties[roles.Length + 1];
				for (int i = 0; i < reorderProperties.Length; ++i)
				{
					if (i > position)
					{
						reorderProperties[i] = new ReorderRoleProperties(roles[i - 1].Id, i);
					}
					else if (i < position)
					{
						reorderProperties[i] = new ReorderRoleProperties(roles[i].Id, i);
					}
					else
					{
						reorderProperties[i] = new ReorderRoleProperties(role.Id, i);
					}
				}

				await role.Guild.ReorderRolesAsync(reorderProperties);
				return reorderProperties.FirstOrDefault(x => x.Id == role.Id)?.Position ?? -1;
			}

			public static async Task<IRole> GetMuteRole(IGuildSettings guildSettings, IGuild guild, IGuildUser user)
			{
				var returnedMuteRole = GetRole(guild, user, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsManaged }, guildSettings.MuteRole);
				var muteRole = returnedMuteRole.Object;
				if (muteRole == null)
				{
					muteRole = await guild.CreateRoleAsync(Constants.MUTE_ROLE_NAME, new GuildPermissions(0));
					//TODO: guildSettings.SetSetting(SettingOnGuild.MuteRole, new DiscordObjectWithID<IRole>(muteRole));
				}

				const uint TEXT_PERMS = 0
					| (1U << (int)ChannelPermission.CreateInstantInvite)
					| (1U << (int)ChannelPermission.ManageChannel)
					| (1U << (int)ChannelPermission.ManagePermissions)
					| (1U << (int)ChannelPermission.ManageWebhooks)
					| (1U << (int)ChannelPermission.SendMessages)
					| (1U << (int)ChannelPermission.ManageMessages)
					| (1U << (int)ChannelPermission.AddReactions);
				foreach (var textChannel in await guild.GetTextChannelsAsync())
				{
					if (textChannel.GetPermissionOverwrite(muteRole) == null)
					{
						await textChannel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(0, TEXT_PERMS));
					}
				}

				const uint VOICE_PERMS = 0
					| (1U << (int)ChannelPermission.CreateInstantInvite)
					| (1U << (int)ChannelPermission.ManageChannel)
					| (1U << (int)ChannelPermission.ManagePermissions)
					| (1U << (int)ChannelPermission.ManageWebhooks)
					| (1U << (int)ChannelPermission.Speak)
					| (1U << (int)ChannelPermission.MuteMembers)
					| (1U << (int)ChannelPermission.DeafenMembers)
					| (1U << (int)ChannelPermission.MoveMembers);
				foreach (var voiceChannel in await guild.GetVoiceChannelsAsync())
				{
					if (voiceChannel.GetPermissionOverwrite(muteRole) == null)
					{
						await voiceChannel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(0, VOICE_PERMS));
					}
				}

				return muteRole;
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
		}

		public static class Users
		{
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

				if (user == null && mentions)
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

				return GetGuildUser(context, checkingTypes, user);
			}
			public static ReturnedObject<IGuildUser> GetGuildUser(ICommandContext context, ObjectVerification[] checkingTypes, ulong inputID)
			{
				return GetGuildUser(context, checkingTypes, GetGuildUser(context.Guild, inputID));
			}
			public static ReturnedObject<IGuildUser> GetGuildUser(ICommandContext context, ObjectVerification[] checkingTypes, IGuildUser user)
			{
				return GetGuildUser(context.Guild, context.User as IGuildUser, checkingTypes, user);
			}
			public static ReturnedObject<T> GetGuildUser<T>(IGuild guild, IGuildUser currUser, ObjectVerification[] checkingTypes, T user) where T : IGuildUser
			{
				checkingTypes.AssertEnumsAreAllCorrectTargetType(user);
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
			public static IGuildUser GetGuildUser(IGuild guild, ulong ID)
			{
				return (guild as SocketGuild).GetUser(ID);
			}
			public static bool GetIfUserCanDoActionOnUser(IGuildUser currUser, ObjectVerification type, IGuildUser targetUser)
			{
				if (targetUser == null || currUser == null)
					return false;

				switch (type)
				{
					case ObjectVerification.CanBeMovedFromChannel:
					{
						return Channels.GetIfUserCanDoActionOnChannel(targetUser.VoiceChannel, currUser, ObjectVerification.CanMoveUsers);
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

			public static IGuildUser GetBot(IGuild guild)
			{
				return (guild as SocketGuild).CurrentUser;
			}
			public static async Task<IUser> GetGlobalUser(IDiscordClient client, ulong ID)
			{
				return await client.GetUserAsync(ID);
			}
			public static async Task<IUser> GetBotOwner(IDiscordClient client, IBotSettings botSettings)
			{
				return await client.GetUserAsync(botSettings.BotOwnerID);
			}

			public static bool GetIfUserCanBeModifiedByUser(IUser currUser, IUser targetUser)
			{
				if (currUser.Id == Properties.Settings.Default.BotID && targetUser.Id == Properties.Settings.Default.BotID)
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

			public static async Task<IEnumerable<IGuildUser>> GetUsersTheBotAndUserCanEdit(ICommandContext context)
			{
				return (await context.Guild.GetUsersAsync()).Where(x => GetIfUserCanBeModifiedByUser(context.User, x) && GetIfUserCanBeModifiedByUser(GetBot(context.Guild), x));
			}

			public static async Task ChangeNickname(IGuildUser user, string newNN)
			{
				await user.ModifyAsync(x => x.Nickname = newNN ?? user.Username);
			}
			public static async Task NicknameManyUsers(IMyCommandContext context, List<IGuildUser> users, string replace)
			{
				var msg = await Messages.SendChannelMessage(context, String.Format("Attempting to rename `{0}` people.", users.Count));
				for (int i = 0; i < users.Count; ++i)
				{
					if (i % 10 == 0)
					{
						await msg.ModifyAsync(x => x.Content = String.Format("Attempting to rename `{0}` people. ETA on completion: `{1}`.", 
							users.Count - i,
							(int)((users.Count - i) * 1.2)));
					}

					await ChangeNickname(users[i], replace);
				}

				await Messages.DeleteMessage(msg);
				await Messages.MakeAndDeleteSecondaryMessage(context, String.Format("Successfully renamed `{0}` people.", users.Count));
			}
			public static async Task MoveUser(IGuildUser user, IVoiceChannel channel)
			{
				await user.ModifyAsync(x => x.Channel = Optional.Create(channel));
			}
			public static async Task MoveManyUsers(IMyCommandContext context, List<IGuildUser> users, IVoiceChannel outputChannel)
			{
				var msg = await Messages.SendChannelMessage(context, String.Format("Attempting to move `{0}` people.", users.Count));
				for (int i = 0; i < users.Count; ++i)
				{
					if (i % 10 == 0)
					{
						await msg.ModifyAsync(x => x.Content = String.Format("Attempting to move `{0}` people. ETA on completion: `{1}`.",
							users.Count - i,
							(int)((users.Count - i) * 1.2)));
					}

					await MoveUser(users[i], outputChannel);
				}

				await Messages.DeleteMessage(msg);
				await Messages.MakeAndDeleteSecondaryMessage(context, String.Format("Successfully moved `{0}` people.", users.Count));
			}
		}

		public static class Emotes
		{
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

				if (emote == null && usage)
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

				return new ReturnedObject<Emote>(emote, FailureReason.NotFailure);
			}
		}

		public static class Formatting
		{
			public static EmbedBuilder FormatUserInfo(IGuildSettings guildSettings, SocketGuild guild, SocketGuildUser user)
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
				var gameStr = FormatGame(guildUser);
				var statusStr = String.Format("**Online status:** `{0}`", guildUser.Status);
				var description = String.Join("\n", new[] { IDstr, nicknameStr, createdStr, joinedStr, gameStr, statusStr });

				var color = roles.OrderBy(x => x.Position).LastOrDefault(x => x.Color.RawValue != 0)?.Color;
				var embed = Embeds.MakeNewEmbed(null, description, color, thumbnailURL: user.GetAvatarUrl());
				if (channels.Count() != 0)
				{
					Embeds.AddField(embed, "Channels", String.Join(", ", channels));
				}
				if (roles.Count() != 0)
				{
					Embeds.AddField(embed, "Roles", String.Join(", ", roles.Select(x => x.Name)));
				}
				if (user.VoiceChannel != null)
				{
					var desc = String.Format("Server mute: `{0}`\nServer deafen: `{1}`\nSelf mute: `{2}`\nSelf deafen: `{3}`", user.IsMuted, user.IsDeafened, user.IsSelfMuted, user.IsSelfDeafened);
					Embeds.AddField(embed, "Voice Channel: " + user.VoiceChannel.Name, desc);
				}
				Embeds.AddAuthor(embed, guildUser);
				Embeds.AddFooter(embed, "User Info");
				return embed;
			}
			public static EmbedBuilder FormatUserInfo(IGuildSettings guildSettings, SocketGuild guild, SocketUser user)
			{
				var ageStr = String.Format("**Created:** `{0}`\n", FormatDateTime(user.CreatedAt.UtcDateTime));
				var gameStr = FormatGame(user);
				var statusStr = String.Format("**Online status:** `{0}`", user.Status);
				var description = String.Join("\n", new[] { ageStr, gameStr, statusStr });

				var embed = Embeds.MakeNewEmbed(null, description, null, thumbnailURL: user.GetAvatarUrl());
				Embeds.AddAuthor(embed, user.FormatUser(), user.GetAvatarUrl(), user.GetAvatarUrl());
				Embeds.AddFooter(embed, "User Info");
				return embed;
			}
			public static EmbedBuilder FormatRoleInfo(IGuildSettings guildSettings, SocketGuild guild, SocketRole role)
			{
				var ageStr = String.Format("**Created:** `{0}` (`{1}` days ago)", FormatDateTime(role.CreatedAt.UtcDateTime), DateTime.UtcNow.Subtract(role.CreatedAt.UtcDateTime).Days);
				var positionStr = String.Format("**Position:** `{0}`", role.Position);
				var usersStr = String.Format("**User Count:** `{0}`", guild.Users.Where(x => x.Roles.Any(y => y.Id == role.Id)).Count());
				var description = String.Join("\n", new[] { ageStr, positionStr, usersStr });

				var color = role.Color;
				var embed = Embeds.MakeNewEmbed(null, description, color);
				Embeds.AddAuthor(embed, role.FormatRole());
				Embeds.AddFooter(embed, "Role Info");
				return embed;
			}
			public static EmbedBuilder FormatChannelInfo(IGuildSettings guildSettings, SocketGuild guild, SocketChannel channel)
			{
				var ignoredFromLog = guildSettings.IgnoredLogChannels.Contains(channel.Id);
				var ignoredFromCmd = guildSettings.IgnoredCommandChannels.Contains(channel.Id);
				var imageOnly = guildSettings.ImageOnlyChannels.Contains(channel.Id);
				var sanitary = guildSettings.SanitaryChannels.Contains(channel.Id);
				var slowmode = guildSettings.SlowmodeChannels.Any(x => x.ChannelID == channel.Id);
				var serverLog = guildSettings.ServerLog?.Id == channel.Id;
				var modLog = guildSettings.ModLog?.Id == channel.Id;
				var imageLog = guildSettings.ImageLog?.Id == channel.Id;

				var ageStr = String.Format("**Created:** `{0}` (`{1}` days ago)", FormatDateTime(channel.CreatedAt.UtcDateTime), DateTime.UtcNow.Subtract(channel.CreatedAt.UtcDateTime).Days);
				var userCountStr = String.Format("**User Count:** `{0}`", channel.Users.Count);
				var ignoredFromLogStr = String.Format("\n**Ignored From Log:** `{0}`", ignoredFromLog ? "Yes" : "No");
				var ignoredFromCmdStr = String.Format("**Ignored From Commands:** `{0}`", ignoredFromCmd ? "Yes" : "No");
				var imageOnlyStr = String.Format("**Image Only:** `{0}`", imageOnly ? "Yes" : "No");
				var sanitaryStr = String.Format("**Sanitary:** `{0}`", sanitary ? "Yes" : "No");
				var slowmodeStr = String.Format("**Slowmode:** `{0}`", slowmode ? "Yes" : "No");
				var serverLogStr = String.Format("\n**Serverlog:** `{0}`", serverLog ? "Yes" : "No");
				var modLogStr = String.Format("**Modlog:** `{0}`", modLog ? "Yes" : "No");
				var imageLogStr = String.Format("**Imagelog:** `{0}`", imageLog ? "Yes" : "No");
				var description = String.Join("\n", new[] { ageStr, userCountStr, ignoredFromLogStr, ignoredFromCmdStr, imageOnlyStr, sanitaryStr, slowmodeStr, serverLogStr, modLogStr, imageLogStr });

				var embed = Embeds.MakeNewEmbed(null, description);
				Embeds.AddAuthor(embed, channel.FormatChannel());
				Embeds.AddFooter(embed, "Channel Info");
				return embed;
			}
			public static EmbedBuilder FormatGuildInfo(IGuildSettings guildSettings, SocketGuild guild)
			{
				var owner = guild.Owner;
				var onlineCount = guild.Users.Where(x => x.Status != UserStatus.Offline).Count();
				var nicknameCount = guild.Users.Where(x => x.Nickname != null).Count();
				var gameCount = guild.Users.Where(x => x.Game.HasValue).Count();
				var botCount = guild.Users.Where(x => x.IsBot).Count();
				var voiceCount = guild.Users.Where(x => x.VoiceChannel != null).Count();
				var localECount = guild.Emotes.Where(x => !x.IsManaged).Count();
				var globalECount = guild.Emotes.Where(x => x.IsManaged).Count();

				var ageStr = String.Format("**Created:** `{0}` (`{1}` days ago)", FormatDateTime(guild.CreatedAt.UtcDateTime), DateTime.UtcNow.Subtract(guild.CreatedAt.UtcDateTime).Days);
				var ownerStr = String.Format("**Owner:** `{0}`", owner.FormatUser());
				var regionStr = String.Format("**Region:** `{0}`", guild.VoiceRegionId);
				var emoteStr = String.Format("**Emotes:** `{0}` (`{1}` local, `{2}` global)\n", localECount + globalECount, localECount, globalECount);
				var userStr = String.Format("**User Count:** `{0}` (`{1}` online, `{2}` bots)", guild.MemberCount, onlineCount, botCount);
				var nickStr = String.Format("**Users With Nickname:** `{0}`", nicknameCount);
				var gameStr = String.Format("**Users Playing Games:** `{0}`", gameCount);
				var voiceStr = String.Format("**Users In Voice:** `{0}`\n", voiceCount);
				var roleStr = String.Format("**Role Count:** `{0}`", guild.Roles.Count);
				var channelStr = String.Format("**Channel Count:** `{0}` (`{1}` text, `{2}` voice)", guild.Channels.Count, guild.TextChannels.Count, guild.VoiceChannels.Count);
				var afkChanStr = String.Format("**AFK Channel:** `{0}` (`{1}` minute{2})", guild.AFKChannel.FormatChannel(), guild.AFKTimeout / 60, Gets.GetPlural(guild.AFKTimeout / 60));
				var description = String.Join("\n", new List<string>() { ageStr, ownerStr, regionStr, emoteStr, userStr, nickStr, gameStr, voiceStr, roleStr, channelStr, afkChanStr });

				var color = owner.Roles.FirstOrDefault(x => x.Color.RawValue != 0)?.Color;
				var embed = Embeds.MakeNewEmbed(null, description, color, thumbnailURL: guild.IconUrl);
				Embeds.AddAuthor(embed, guild.FormatGuild());
				Embeds.AddFooter(embed, "Guild Info");
				return embed;
			}
			public static EmbedBuilder FormatEmoteInfo(IGuildSettings guildSettings, IEnumerable<IGuild> guilds, Emote emote)
			{
				//Try to find the emoji if global
				var guildsWithEmote = guilds.Where(x => x.HasGlobalEmotes());

				var description = String.Format("**ID:** `{0}`\n", emote.Id);
				if (guildsWithEmote.Any())
				{
					description += String.Format("**From:** `{0}`", String.Join("`, `", guildsWithEmote.Select(x => x.FormatGuild())));
				}

				var embed = Embeds.MakeNewEmbed(null, description, thumbnailURL: emote.Url);
				Embeds.AddAuthor(embed, emote.Name);
				Embeds.AddFooter(embed, "Emoji Info");
				return embed;
			}
			public static EmbedBuilder FormatInviteInfo(IGuildSettings guildSettings, SocketGuild guild, IInviteMetadata invite)
			{
				var inviterStr = String.Format("**Inviter:** `{0}`", invite.Inviter.FormatUser());
				var channelStr = String.Format("**Channel:** `{0}`", guild.Channels.FirstOrDefault(x => x.Id == invite.ChannelId).FormatChannel());
				var usesStr = String.Format("**Uses:** `{0}`", invite.Uses);
				var createdStr = String.Format("**Created At:** `{0}`", FormatDateTime(invite.CreatedAt.UtcDateTime));
				var description = String.Join("\n", new[] { inviterStr, channelStr, usesStr, createdStr });

				var embed = Embeds.MakeNewEmbed(null, description);
				Embeds.AddAuthor(embed, invite.Code);
				Embeds.AddFooter(embed, "Emote Info");
				return embed;
			}
			public static EmbedBuilder FormatBotInfo(IBotSettings globalInfo, IDiscordClient client, ILogModule logModule, IGuild guild)
			{
				var online = String.Format("**Online Since:** `{0}`", FormatDateTime(globalInfo.StartupTime));
				var uptime = String.Format("**Uptime:** `{0}`", Gets.GetUptime(globalInfo));
				var guildCount = String.Format("**Guild Count:** `{0}`", logModule.TotalGuilds);
				var memberCount = String.Format("**Cumulative Member Count:** `{0}`", logModule.TotalUsers);
				var currShard = String.Format("**Current Shard:** `{0}`", ClientActions.GetShardIdFor(client, guild));
				var description = String.Join("\n", new[] { online, uptime, guildCount, memberCount, currShard });

				var embed = Embeds.MakeNewEmbed(null, description);
				Embeds.AddAuthor(embed, client.CurrentUser);
				Embeds.AddFooter(embed, "Version " + Constants.BOT_VERSION);

				var firstField = logModule.FormatLoggedActions();
				Embeds.AddField(embed, "Logged Actions", firstField);

				var secondField = logModule.FormatLoggedCommands();
				Embeds.AddField(embed, "Commands", secondField);

				var latency = String.Format("**Latency:** `{0}ms`", ClientActions.GetLatency(client));
				var memory = String.Format("**Memory Usage:** `{0}MB`", Gets.GetMemory(globalInfo.Windows).ToString("0.00"));
				var threads = String.Format("**Thread Count:** `{0}`", System.Diagnostics.Process.GetCurrentProcess().Threads.Count);
				var thirdField = String.Join("\n", new[] { latency, memory, threads });
				Embeds.AddField(embed, "Technical", thirdField);

				return embed;
			}

			public static List<string> FormatMessages(IEnumerable<IMessage> list)
			{
				return list.Select(x => FormatNonDM(x)).ToList();
			}
			public static List<string> FormatDMs(IEnumerable<IMessage> list)
			{
				return list.Select(x => FormatDM(x)).ToList();
			}
			public static string FormatNonDM(IMessage message)
			{
				return String.Format("`[{0}]` `{1}` **IN** `{2}`\n```\n{3}```",
					message.CreatedAt.ToString("HH:mm:ss"),
					message.Author.FormatUser(),
					message.Channel.FormatChannel(),
					RemoveMarkdownChars(FormatMessageContent(message), true));
			}
			public static string FormatDM(IMessage message)
			{
				return String.Format("`[{0}]` `{1}`\n```\n{2}```",
					FormatDateTime(message.CreatedAt),
					message.Author.FormatUser(),
					RemoveMarkdownChars(FormatMessageContent(message), true));
			}
			public static string FormatMessageContent(IMessage message)
			{
				var content = String.IsNullOrEmpty(message.Content) ? "Empty message content" : message.Content;
				if (message.Embeds.Any())
				{
					var descriptions = message.Embeds.Where(x => x.Description != null || x.Url != null || x.Image.HasValue).Select(x =>
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
					for (int i = 0; i < descriptions.Length; ++i)
					{
						formattedDescriptions += String.Format("Embed {0}: {1}", i + 1, descriptions[i]);
					}

					content += "\n" + formattedDescriptions;
				}
				if (message.Attachments.Any())
				{
					content += " + " + String.Join(" + ", message.Attachments.Select(x => x.Filename));
				}

				return content;
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
				return FormatDateTime(dt?.UtcDateTime);
			}

			public static string FormatGame(IUser user)
			{
				var game = user.Game;
				switch (game?.StreamType)
				{
					case StreamType.NotStreaming:
					{
						return String.Format("**Current Game:** `{0}`", EscapeMarkdown(game?.Name, true));
					}
					case StreamType.Twitch:
					{
						return String.Format("**Current Stream:** [{0}]({1})", EscapeMarkdown(game?.Name, true), game?.StreamUrl);
					}
					default:
					{
						return "**Current Game:** `N/A`";
					}
				}
			}

			public static string ERROR(string message)
			{
				return Constants.ZERO_LENGTH_CHAR + Constants.ERROR_MESSAGE + message;
			}

			public static string EscapeMarkdown(string str, bool onlyAccentGrave)
			{
				return onlyAccentGrave ? str.Replace("`", "\\`") : str.Replace("`", "\\`").Replace("*", "\\*").Replace("_", "\\_");
			}
			public static string RemoveMarkdownChars(string input, bool replaceNewLines)
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
			public static string RemoveNewLines(string input)
			{
				return input.Replace(Environment.NewLine, "").Replace("\r", "").Replace("\n", "");
			}

			public static string FormatErrorString(IGuild guild, FailureReason failureReason, object obj)
			{
				var objType = FormatObjectType(obj);
				switch (failureReason)
				{
					case FailureReason.TooFew:
					{
						return String.Format("Unable to find the {0}.", objType);
					}
					case FailureReason.UserInability:
					{
						return String.Format("You are unable to make the given changes to the {0}: `{1}`.", objType, FormatObject(obj));
					}
					case FailureReason.BotInability:
					{
						return String.Format("I am unable to make the given changes to the {0}: `{1}`.", objType, FormatObject(obj));
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
						return "The default channel cannot be modified in that way.";
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
						return String.Format("The option `{0}` is not accepted in this instance.", (obj as Enum).EnumName());
					}
					default:
					{
						return "This shouldn't be seen. - Advobot";
					}
				}
			}
			public static string FormatObjectType(object obj)
			{
				if (obj is IUser)
				{
					return Constants.BASIC_TYPE_USER;
				}
				else if (obj is IChannel)
				{
					return Constants.BASIC_TYPE_CHANNEL;
				}
				else if (obj is IRole)
				{
					return Constants.BASIC_TYPE_ROLE;
				}
				else if (obj is IGuild)
				{
					return Constants.BASIC_TYPE_GUILD;
				}
				else
				{
					return "Error fetching type";
				}
			}
			public static string FormatObject(object obj)
			{
				if (obj is IUser)
				{
					return (obj as IUser).FormatUser();
				}
				else if (obj is IChannel)
				{
					return (obj as IChannel).FormatChannel();
				}
				else if (obj is IRole)
				{
					return (obj as IRole).FormatRole();
				}
				else if (obj is IGuild)
				{
					return (obj as IGuild).FormatGuild();
				}
				else
				{
					return "Error formatting object";
				}
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

			public static string FormatAttribute(PermissionRequirementAttribute attr)
			{
				return attr != null ? String.Format("[{0}]", JoinNonNullStrings(" | ", attr.AllText, attr.AnyText)) : "N/A";
			}
			public static string FormatAttribute(OtherRequirementAttribute attr)
			{
				var basePerm = "N/A";
				if (attr != null)
				{
					var text = new List<string>();
					if ((attr.Requirements & Precondition.UserHasAPerm) != 0)
					{
						text.Add("Administrator | Any perm ending with 'Members' | Any perm starting with 'Manage'");
					}
					if ((attr.Requirements & Precondition.GuildOwner) != 0)
					{
						text.Add("Guild Owner");
					}
					if ((attr.Requirements & Precondition.TrustedUser) != 0)
					{
						text.Add("Trusted User");
					}
					if ((attr.Requirements & Precondition.BotOwner) != 0)
					{
						text.Add("Bot Owner");
					}
					basePerm = String.Format("[{0}]", String.Join(" | ", text));
				}
				return basePerm;
			}

			public static async Task<string> FormatAllBotSettings(IDiscordClient client, IBotSettings botSettings)
			{
				var str = "";
				foreach (var property in botSettings.GetType().GetProperties())
				{
					//Only get public editable properties
					if (property.GetGetMethod() != null && property.GetSetMethod() != null)
					{
						var formatted = await FormatBotSettingInfo(client, botSettings, property);
						if (!String.IsNullOrWhiteSpace(formatted))
						{
							str += String.Format("**{0}**:\n{1}\n\n", property.Name, formatted);
						}
					}
				}
				return str;
			}
			public static async Task<string> FormatBotSettingInfo(IDiscordClient client, IBotSettings botSettings, PropertyInfo property)
			{
				var value = property.GetValue(botSettings);
				return value != null ? await FormatBotSettingInfo(client, value) : null;
			}
			public static async Task<string> FormatBotSettingInfo(IDiscordClient client, object value)
			{
				if (value is ulong)
				{
					var user = await Users.GetGlobalUser(client, (ulong)value);
					if (user != null)
					{
						return String.Format("`{0}`", user.FormatUser());
					}

					var guild = await Guilds.GetGuild(client, (ulong)value);
					if (guild != null)
					{
						return String.Format("`{0}`", guild.FormatGuild());
					}

					return ((ulong)value).ToString();
				}
				//Because strings are char[] this pointless else if has to be here so it doesn't go into the else if directly below
				else if (value is string)
				{
					return String.IsNullOrWhiteSpace(value.ToString()) ? "`Nothing`" : String.Format("`{0}`", value.ToString());
				}
				else if (value is System.Collections.IEnumerable)
				{
					var temp = new List<string>();
					foreach (var tempSetting in ((System.Collections.IEnumerable)value).Cast<object>())
					{
						temp.Add(await FormatBotSettingInfo(client, tempSetting));
					}
					return String.Join("\n", temp);
				}
				else
				{
					return String.Format("`{0}`", value.ToString());
				}
			}

			public static string FormatAllGuildSettings(IGuild guild, IGuildSettings guildSettings)
			{
				var str = "";
				foreach (var property in guildSettings.GetType().GetProperties(BindingFlags.Public))
				{
					//Only get public editable properties
					if (property.GetGetMethod() != null && property.GetSetMethod() != null)
					{
						var formatted = FormatGuildSettingInfo(guild as SocketGuild, guildSettings, property);
						if (!String.IsNullOrWhiteSpace(formatted))
						{
							str += String.Format("**{0}**:\n{1}\n\n", property.Name, formatted);
						}
					}
				}
				return str;
			}
			public static string FormatGuildSettingInfo(SocketGuild guild, IGuildSettings guildSettings, PropertyInfo property)
			{
				var value = property.GetValue(guildSettings);
				if (value != null)
				{
					return FormatGuildSettingInfo(guild, value);
				}
				else
				{
					return null;
				}
			}
			public static string FormatGuildSettingInfo(SocketGuild guild, object value)
			{
				if (value is ISetting)
				{
					return ((ISetting)value).SettingToString();
				}
				else if (value is ulong)
				{
					var chan = guild.GetChannel((ulong)value);
					if (chan != null)
					{
						return String.Format("`{0}`", chan.FormatChannel());
					}

					var role = guild.GetRole((ulong)value);
					if (role != null)
					{
						return String.Format("`{0}`", role.FormatRole());
					}

					var user = guild.GetUser((ulong)value);
					if (user != null)
					{
						return String.Format("`{0}`", user.FormatUser());
					}

					return ((ulong)value).ToString();
				}
				//Because strings are char[] this pointless else if has to be here so it doesn't go into the else if directly below
				else if (value is string)
				{
					return String.IsNullOrWhiteSpace(value.ToString()) ? "`Nothing`" : String.Format("`{0}`", value.ToString());
				}
				else if (value is System.Collections.IEnumerable)
				{
					return String.Join("\n", ((System.Collections.IEnumerable)value).Cast<object>().Select(x => FormatGuildSettingInfo(guild, x)));
				}
				else
				{
					return String.Format("`{0}`", value.ToString());
				}
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
		}

		public static class Messages
		{
			public static async Task<IUserMessage> SendEmbedMessage(IMessageChannel channel, EmbedBuilder embed, string content = null)
			{
				var guild = channel.GetGuild();
				if (guild == null)
					return null;

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
					embed.WithDescription(String.Format("The description is over `{0}` characters and will be sent as a text file instead.", Constants.MAX_DESCRIPTION_LENGTH));
				}
				else if (embed.Description.GetLineBreaks() > Constants.MAX_DESCRIPTION_LINES)
				{
					badDesc = embed.Description;
					embed.WithDescription(String.Format("The description is over `{0}` lines and will be sent as a text file instead.", Constants.MAX_DESCRIPTION_LINES));
				}
				totalChars += embed.Description?.Length ?? 0;

				//Embeds can only be 1024 characters max and mobile can only show up to 5 line breaks
				var badFields = new List<Tuple<int, string>>();
				for (int i = 0; i < embed.Fields.Count; ++i)
				{
					var field = embed.Fields[i];
					var value = field.Value.ToString();
					if (totalChars > Constants.MAX_EMBED_TOTAL_LENGTH - 1500)
					{
						badFields.Add(new Tuple<int, string>(i, value));
						field.WithName(i.ToString());
						field.WithValue(String.Format("`{0}` char limit close.", Constants.MAX_EMBED_TOTAL_LENGTH));
					}
					else if (value?.Length > Constants.MAX_FIELD_VALUE_LENGTH)
					{
						badFields.Add(new Tuple<int, string>(i, value));
						field.WithValue(String.Format("This field is over `{0}` characters and will be sent as a text file instead.", Constants.MAX_FIELD_VALUE_LENGTH));
					}
					else if (value.GetLineBreaks() > Constants.MAX_FIELD_LINES)
					{
						badFields.Add(new Tuple<int, string>(i, value));
						field.WithValue(String.Format("This field is over `{0}` lines and will be sent as a text file instead.", Constants.MAX_FIELD_LINES));
					}
					totalChars += value?.Length ?? 0;
					totalChars += field.Name?.Length ?? 0;
				}

				IUserMessage msg;
				try
				{
					if (content != null)
					{
						content = content.CaseInsReplace(guild.EveryoneRole.Mention, Constants.FAKE_EVERYONE);
						content = content.CaseInsReplace("@everyone", Constants.FAKE_EVERYONE);
						content = content.CaseInsReplace("\tts", Constants.FAKE_TTS);
					}

					msg = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + content ?? "", false, embed.WithCurrentTimestamp());
				}
				catch (Exception e)
				{
					ConsoleActions.ExceptionToConsole(e);
					msg = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + Formatting.ERROR(e.Message));
					return null;
				}

				//Go send the description/fields that had an error
				if (badDesc != null)
				{
					await Uploads.WriteAndUploadTextFile(guild, channel, badDesc, "Description_");
				}
				foreach (var tuple in badFields)
				{
					var num = tuple.Item1;
					var val = tuple.Item2;
					await Uploads.WriteAndUploadTextFile(guild, channel, val, String.Format("Field_{0}_", num));
				}

				return msg;
			}
			public static async Task<IUserMessage> SendChannelMessage(ICommandContext context, string content)
			{
				return await SendChannelMessage(context.Channel, content);
			}
			public static async Task<IUserMessage> SendChannelMessage(IMessageChannel channel, string content)
			{
				var guild = (channel as ITextChannel)?.Guild;
				if (guild == null)
					return null;

				content = content.CaseInsReplace(guild.EveryoneRole.Mention, Constants.FAKE_EVERYONE);
				content = content.CaseInsReplace("@everyone", Constants.FAKE_EVERYONE);
				content = content.CaseInsReplace("\tts", Constants.FAKE_TTS);

				IUserMessage msg = null;
				if (content.Length >= Constants.MAX_MESSAGE_LENGTH_LONG)
				{
					msg = await Uploads.WriteAndUploadTextFile(guild, channel, content, "Long_Message_", "The response is a long message and was sent as a text file instead");
				}
				else
				{
					msg = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + content);
				}
				return msg;
			}
			public static async Task<IUserMessage> SendDMMessage(IDMChannel channel, string message)
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
			public static async Task<int> RemoveMessages(IMessageChannel channel, IMessage fromMessage, int requestCount, IUser user)
			{
				var guildChannel = channel as ITextChannel;
				if (guildChannel == null)
					return 0;

				if (user == null)
				{
					return await RemoveMessages(channel, fromMessage, requestCount);
				}

				var deletedCount = 0;
				while (requestCount > 0)
				{
					//Get the current messages and ones that aren't null
					var messages = await channel.GetMessagesAsync(fromMessage, Direction.Before, 100).Flatten();
					if (!messages.Any())
						break;

					//Set the from message as the last of the currently grabbed ones
					fromMessage = messages.Last();

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
						ConsoleActions.WriteLine(String.Format("Unable to delete {0} messages on the guild {1} on channel {2}.", msgAmt, guildChannel.Guild.FormatGuild(), guildChannel.FormatChannel()));
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

			public static async Task MakeAndDeleteSecondaryMessage(IMyCommandContext context, string secondStr, int time = Constants.SECONDS_DEFAULT)
			{
				await MakeAndDeleteSecondaryMessage(context.Timers, context.Channel, context.Message, secondStr, time);
			}
			public static async Task MakeAndDeleteSecondaryMessage(ITimersModule timers, IMessageChannel channel, IUserMessage message, string secondStr, int time = Constants.SECONDS_DEFAULT)
			{
				var secondMsg = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + secondStr);
				var messages = new List<IMessage> { secondMsg, message };

				if (message == null)
				{
					RemoveCommandMessage(timers, secondMsg, time);
				}
				else
				{
					RemoveCommandMessages(timers, messages, time);
				}
			}

			public static void RemoveCommandMessages(ITimersModule timers, IEnumerable<IMessage> messages, int time)
			{
				timers.RemovableMessages.Add(new RemovableMessage(messages, time));
			}
			public static void RemoveCommandMessage(ITimersModule timers, IMessage message, int time)
			{
				timers.RemovableMessages.Add(new RemovableMessage(message, time));
			}

			public static async Task DeleteMessages(IMessageChannel channel, IEnumerable<IMessage> messages)
			{
				if (messages == null || !messages.Any())
					return;

				try
				{
					await channel.DeleteMessagesAsync(messages.Where(x => DateTime.UtcNow.Subtract(x.CreatedAt.UtcDateTime).TotalDays < 14).Distinct());
				}
				catch
				{
					ConsoleActions.WriteLine(String.Format("Unable to delete {0} messages on the guild {1} on channel {2}.", messages.Count(), channel.GetGuild().FormatGuild(), channel.FormatChannel()));
				}
			}
			public static async Task DeleteMessage(IMessage message)
			{
				if (message == null || DateTime.UtcNow.Subtract(message.CreatedAt.UtcDateTime).TotalDays >= 14)
					return;

				try
				{
					await message.DeleteAsync();
				}
				catch
				{
					ConsoleActions.WriteLine(String.Format("Unable to delete the message {0} on channel {1}.", message.Id, message.Channel.FormatChannel()));
				}
			}
			public static async Task SendMessageContainingFormattedDeletedMessages(IGuild guild, ITextChannel channel, List<string> inputList)
			{
				if (!inputList.Any())
				{
					return;
				}

				var characterCount = 0;
				inputList.ForEach(x => characterCount += (x.Length + 100));

				if (inputList.Count <= 5 && characterCount < Constants.MAX_MESSAGE_LENGTH_LONG)
				{
					var embed = Embeds.MakeNewEmbed("Deleted Messages", String.Join("\n", inputList), Constants.MDEL);
					Embeds.AddFooter(embed, "Deleted Messages");
					await SendEmbedMessage(channel, embed);
				}
				else
				{
					var text = Formatting.RemoveMarkdownChars(String.Join("\n-----\n", inputList), true);
					var name = "Deleted_Messages_";
					var content = String.Format("{0} Deleted Messages", inputList.Count);
					await Uploads.WriteAndUploadTextFile(guild, channel, text, name, content);
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

			public static async Task HandleObjectGettingErrors<T>(IMyCommandContext context, ReturnedObject<T> returnedObject)
			{
				await MakeAndDeleteSecondaryMessage(context, Formatting.FormatErrorString(context.Guild, returnedObject.Reason, returnedObject.Object));
			}
			public static async Task HandleArgsGettingErrors(IMyCommandContext context, ReturnedArguments returnedArgs)
			{
				//TODO: Remove my own arg parsing.
				switch (returnedArgs.Reason)
				{
					case FailureReason.TooMany:
					{
						await MakeAndDeleteSecondaryMessage(context, Formatting.ERROR("Too many arguments."));
						return;
					}
					case FailureReason.TooFew:
					{
						await MakeAndDeleteSecondaryMessage(context, Formatting.ERROR("Too few arguments."));
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

			public static async Task<Dictionary<IUser, IMessageChannel>> GetAllBotDMs(IDiscordClient client)
			{
				var dict = new Dictionary<IUser, IMessageChannel>();
				foreach (var channel in await client.GetDMChannelsAsync())
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
				return (await GetMessages(channel, Constants.AMT_OF_DMS_TO_GATHER)).OrderBy(x => x?.CreatedAt).ToList();
			}
		}

		public static class ConsoleActions
		{
			public static SortedDictionary<string, List<string>> WrittenLines = new SortedDictionary<string, List<string>>();

			public static void WriteLine(string text, [CallerMemberName] string name = "")
			{
				var line = String.Format("[{0}] [{1}]: {2}", DateTime.Now.ToString("HH:mm:ss"), name, Formatting.RemoveMarkdownChars(text, true));

				if (!WrittenLines.TryGetValue(name, out List<string> list))
				{
					WrittenLines.Add(name, list = new List<string>());
				}
				list.Add(line);

				Console.WriteLine(line);
			}
			public static void ExceptionToConsole(Exception e, [CallerMemberName] string name = "")
			{
				WriteLine("EXCEPTION: " + e, name);
			}
		}

		public static class Embeds
		{
			public static EmbedBuilder MakeNewEmbed(string title = null, string description = null, Color? color = null, string imageURL = null, string URL = null, string thumbnailURL = null)
			{
				//Make the embed builder
				var embed = new EmbedBuilder().WithColor(Constants.BASE);

				//Validate the URLs
				imageURL = Uploads.ValidateURL(imageURL) ? imageURL : null;
				URL = Uploads.ValidateURL(URL) ? URL : null;
				thumbnailURL = Uploads.ValidateURL(thumbnailURL) ? thumbnailURL : null;

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
				iconURL = Uploads.ValidateURL(iconURL) ? iconURL : null;
				URL = Uploads.ValidateURL(URL) ? URL : null;

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
			public static void AddAuthor(EmbedBuilder embed, IUser user, string URL = null)
			{
				AddAuthor(embed, user.Username, user.GetAvatarUrl(), URL ?? user.GetAvatarUrl());
			}
			public static void AddFooter(EmbedBuilder embed, [CallerMemberName] string text = null, string iconURL = null)
			{
				//Make the footer builder
				var footer = new EmbedFooterBuilder();

				//Verify the URL
				iconURL = Uploads.ValidateURL(iconURL) ? iconURL : null;

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
		}

		public static class Invites
		{
			public static async Task<IReadOnlyCollection<IInviteMetadata>> GetInvites(IGuild guild)
			{
				if (guild == null)
					return new List<IInviteMetadata>();

				var currUser = await guild.GetCurrentUserAsync();
				if (!currUser.GuildPermissions.ManageGuild)
					return new List<IInviteMetadata>();

				return await guild.GetInvitesAsync();
			}
			public static async Task<BotInvite> GetInviteUserJoinedOn(IGuildSettings guildSettings, IGuild guild)
			{
				var curInvs = await GetInvites(guild);
				if (!curInvs.Any())
					return null;

				//Find the first invite where the bot invite has the same code as the current invite but different use counts
				var joinInv = guildSettings.Invites.FirstOrDefault(bI => curInvs.Any(cI => cI.Code == bI.Code && cI.Uses != bI.Uses));
				//If the invite is null, take that as meaning there are new invites on the guild
				if (joinInv == null)
				{
					//Get the new invites on the guild by finding which guild invites aren't on the bot invites list
					var newInvs = curInvs.Where(cI => !guildSettings.Invites.Select(bI => bI.Code).Contains(cI.Code));
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
					guildSettings.Invites.AddRange(newInvs.Select(x => new BotInvite(x.GuildId, x.Code, x.Uses)));
				}
				else
				{
					//Increment the invite the bot is holding if a curInv was found so as to match with the current invite uses count
					joinInv.IncreaseUses();
				}
				return joinInv;
			}
		}

		public static class Uploads
		{
			public static async Task<IUserMessage> WriteAndUploadTextFile(IGuild guild, IMessageChannel channel, string text, string fileName, string content = null)
			{
				//Get the file path
				if (!fileName.EndsWith("_"))
				{
					fileName += "_";
				}

				var file = fileName + DateTime.UtcNow.ToString("MM-dd_HH-mm-ss") + Constants.GENERAL_FILE_EXTENSION;
				var path = Gets.GetServerFilePath(guild.Id, file);
				if (path == null)
					return null;

				using (var writer = new StreamWriter(path))
				{
					writer.WriteLine(Formatting.RemoveMarkdownChars(text, false));
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

			public static async Task SetBotIcon(IMyCommandContext context, string imageURL)
			{
				if (imageURL == null)
				{
					await context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image());
					await Messages.MakeAndDeleteSecondaryMessage(context, "Successfully removed the bot's icon.");
					return;
				}

				var fileType = await GetFileTypeOrSayErrors(context, imageURL);
				if (fileType == null)
					return;

				var path = Gets.GetServerFilePath(context.Guild.Id, Constants.BOT_ICON_LOCATION + fileType);
				using (var webclient = new WebClient())
				{
					webclient.DownloadFileAsync(new Uri(imageURL), path);
					webclient.DownloadFileCompleted += (sender, e) => SetIcon(sender, e, context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(path)), context, path);
				}
			}
			public static async Task<string> GetFileTypeOrSayErrors(IMyCommandContext context, string imageURL)
			{
				string fileType;
				var req = WebRequest.Create(imageURL);
				req.Method = WebRequestMethods.Http.Head;
				using (var resp = req.GetResponse())
				{
					if (!Constants.VALID_IMAGE_EXTENSIONS.Contains(fileType = "." + resp.Headers.Get("Content-Type").Split('/').Last()))
					{
						await Messages.MakeAndDeleteSecondaryMessage(context, Formatting.ERROR("Image must be a png or jpg."));
						return null;
					}
					else if (!int.TryParse(resp.Headers.Get("Content-Length"), out int ContentLength))
					{
						await Messages.MakeAndDeleteSecondaryMessage(context, Formatting.ERROR("Unable to get the image's file size."));
						return null;
					}
					else if (ContentLength > Constants.MAX_ICON_FILE_SIZE)
					{
						await Messages.MakeAndDeleteSecondaryMessage(context, Formatting.ERROR(String.Format("Image is bigger than {0:0.0}MB. Manually upload instead.", (double)Constants.MAX_ICON_FILE_SIZE / 1000000)));
						return null;
					}
				}
				return fileType;
			}
			public static void SetIcon(object sender, System.ComponentModel.AsyncCompletedEventArgs e, Task iconSetter, IMyCommandContext context, string path)
			{
				iconSetter.ContinueWith(async prevTask =>
				{
					if (prevTask?.Exception?.InnerExceptions?.Any() ?? false)
					{
						var exceptionMessages = new List<string>();
						foreach (var exception in prevTask.Exception.InnerExceptions)
						{
							ConsoleActions.ExceptionToConsole(exception);
							exceptionMessages.Add(exception.Message);
						}
						await Messages.SendChannelMessage(context, String.Format("Failed to change the bot icon. Following exceptions occurred:\n{0}.", String.Join("\n", exceptionMessages)));
					}
					else
					{
						await Messages.MakeAndDeleteSecondaryMessage(context, "Successfully changed the bot icon.");
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
		}

		public static class Punishments
		{
			public static async Task RoleMuteUser(ITimersModule timers, IGuildUser user, IRole role, uint time = 0)
			{
				await Roles.GiveRole(user, role);

				if (time > 0)
				{
					timers.RemovablePunishments.ThreadSafeAdd(new RemovableRoleMute(user.Guild, user, time, role));
				}
			}
			public static async Task VoiceMuteUser(ITimersModule timers, IGuildUser user, uint time = 0)
			{
				await user.ModifyAsync(x => x.Mute = true);

				if (time > 0)
				{
					timers.RemovablePunishments.ThreadSafeAdd(new RemovableVoiceMute(user.Guild, user, time));
				}
			}
			public static async Task DeafenUser(ITimersModule timers, IGuildUser user, uint time = 0)
			{
				await user.ModifyAsync(x => x.Deaf = true);

				if (time > 0)
				{
					timers.RemovablePunishments.ThreadSafeAdd(new RemovableDeafen(user.Guild, user, time));
				}
			}

			public static async Task ManualRoleUnmuteUser(ITimersModule timers, IGuildUser user, IRole role)
			{
				await Roles.TakeRole(user, role);

				timers.RemovablePunishments.ThreadSafeRemoveAll(x => x.UserID == user.Id && x.PunishmentType == PunishmentType.RoleMute);
			}
			public static async Task ManualVoiceUnmuteUser(ITimersModule timers, IGuildUser user)
			{
				await user.ModifyAsync(x => x.Mute = false);

				timers.RemovablePunishments.ThreadSafeRemoveAll(x => x.UserID == user.Id && x.PunishmentType == PunishmentType.VoiceMute);
			}
			public static async Task ManualUndeafenUser(ITimersModule timers, IGuildUser user)
			{
				await user.ModifyAsync(x => x.Deaf = false);

				timers.RemovablePunishments.ThreadSafeRemoveAll(x => x.UserID == user.Id && x.PunishmentType == PunishmentType.Deafen);
			}
			public static async Task ManualBan(ITimersModule timers, ICommandContext context, ulong userID, int days = 0, uint time = 0, string reason = null)
			{
				await context.Guild.AddBanAsync(userID, days, Formatting.FormatUserReason(context.User, reason));

				if (time > 0)
				{
					timers.RemovablePunishments.ThreadSafeAdd(new RemovableBan(context.Guild, userID, time));
				}
			}
			public static async Task ManualSoftban(ICommandContext context, ulong userID, string reason = null)
			{
				await context.Guild.AddBanAsync(userID, 7, Formatting.FormatUserReason(context.User, reason));
				await context.Guild.RemoveBanAsync(userID);
			}
			public static async Task ManualKick(ICommandContext context, IGuildUser user, string reason = null)
			{
				await user.KickAsync(Formatting.FormatUserReason(context.User, reason));
			}

			public static async Task AutomaticRoleUnmuteUser(IGuildUser user, IRole role)
			{
				await Roles.TakeRole(user, role);
			}
			public static async Task AutomaticVoiceUnmuteUser(IGuildUser user)
			{
				await user.ModifyAsync(x => x.Mute = false);
			}
			public static async Task AutomaticUndeafenUser(IGuildUser user)
			{
				await user.ModifyAsync(x => x.Deaf = false);
			}
			public static async Task AutomaticBan(IGuild guild, ulong userID, [CallerMemberName] string reason = null)
			{
				await guild.AddBanAsync(userID, 7, Formatting.FormatBotReason(reason));
			}
			public static async Task AutomaticSoftban(IGuild guild, ulong userID, [CallerMemberName] string reason = null)
			{
				await guild.AddBanAsync(userID, 7, Formatting.FormatBotReason(reason));
				await guild.RemoveBanAsync(userID);
			}
			public static async Task AutomaticKick(IGuildUser user, [CallerMemberName] string reason = null)
			{
				await user.KickAsync(Formatting.FormatBotReason(reason));
			}

			public static async Task AutomaticPunishments(ITimersModule timers, IGuildSettings guildSettings, IGuildUser user, PunishmentType punishmentType, bool alreadyKicked = false, uint time = 0, [CallerMemberName] string caller = "")
			{
				//TODO: Rework the 4 big punishment things
				//Basically a consolidation of 4 separate big banning things into one. I still need to rework a lot of this.
				var guild = user.Guild;
				if (!Users.GetIfUserCanBeModifiedByUser(Users.GetBot(guild), user))
					return;

				switch (punishmentType)
				{
					case PunishmentType.Nothing:
					{
						return;
					}
					case PunishmentType.Deafen:
					{
						await DeafenUser(timers, user, time);
						return;
					}
					case PunishmentType.VoiceMute:
					{
						await VoiceMuteUser(timers, user, time);
						return;
					}
					case PunishmentType.RoleMute:
					{
						await RoleMuteUser(timers, user, guildSettings.MuteRole, time);
						return;
					}
					case PunishmentType.Kick:
					{
						await AutomaticKick(user, caller);
						return;
					}
					case PunishmentType.KickThenBan:
					{
						if (!alreadyKicked)
						{
							await AutomaticKick(user, caller);
						}
						else
						{
							await AutomaticBan(guild, user.Id, caller);

							if (time > 0)
							{
								timers.RemovablePunishments.ThreadSafeAdd(new RemovableBan(guild, user, time));
							}
						}
						return;
					}
					case PunishmentType.Ban:
					{
						await AutomaticBan(guild, user.Id, caller);
						
						if (time > 0)
						{
							timers.RemovablePunishments.ThreadSafeAdd(new RemovableBan(guild, user, time));
						}
						return;
					}
				}
			}
		}

		public static class Spam
		{
			public static async Task HandleSpamPrevention(ITimersModule timers, IGuildSettings guildSettings, IGuild guild, IUser author, IMessage msg)
			{
				var spamUser = guildSettings.SpamPreventionUsers.FirstOrDefault(x => x.User.Id == author.Id);
				if (spamUser == null)
				{
					guildSettings.SpamPreventionUsers.ThreadSafeAdd(spamUser = new SpamPreventionUser(author as IGuildUser));
				}

				//TODO: Make sure this works
				var spam = false;
				foreach (var spamType in Enum.GetValues(typeof(SpamType)).Cast<SpamType>())
				{
					var spamPrev = guildSettings.SpamPreventionDictionary[spamType];
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

					if (spamUser.CheckIfAllowedToPunish(spamPrev, spamType))
					{
						await Messages.DeleteMessage(msg);

						//Make sure they have the lowest vote count required to kick and the most severe punishment type
						spamUser.ChangeVotesRequired(spamPrev.VotesForKick);
						spamUser.ChangePunishmentType(spamPrev.PunishmentType);
						spamUser.EnablePunishable();

						spam = true;
					}
				}

				if (spam)
				{
					var content = String.Format("The user `{0}` needs `{1}` votes to be kicked. Vote by mentioning them.", author.FormatUser(), spamUser.VotesRequired - spamUser.UsersWhoHaveAlreadyVoted.Count);
					await Messages.MakeAndDeleteSecondaryMessage(timers, msg.Channel, null, content, 10);
				}
			}
			public static async Task HandleSlowmode(IGuildSettings guildSettings, IMessage message)
			{
				var smGuild = guildSettings.SlowmodeGuild;
				if (smGuild != null)
				{
					await HandleSlowmodeUser(smGuild.Users.FirstOrDefault(x => x.User.Id == message.Author.Id), message);
				}

				var smChannel = guildSettings.SlowmodeChannels.FirstOrDefault(x => x.ChannelID == message.Channel.Id);
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
							user.SetNewTime();
						}

						user.LowerMessagesLeft();
					}
					else
					{
						await Messages.DeleteMessage(message);
					}
				}
			}
			public static async Task HandleBannedPhrases(ITimersModule timers, IGuildSettings guildSettings, IGuild guild, IMessage message)
			{
				//Ignore admins and messages older than an hour. (Accidentally deleted something important once due to not having these checks in place, but this should stop most accidental deletions)
				if ((message.Author as IGuildUser).GuildPermissions.Administrator || (int)DateTime.UtcNow.Subtract(message.CreatedAt.UtcDateTime).TotalHours > 0)
					return;

				var str = guildSettings.BannedPhraseStrings.FirstOrDefault(x => message.Content.CaseInsContains(x.Phrase));
				if (str != null)
				{
					await HandleBannedPhrasePunishments(timers, guildSettings, guild, message, str);
				}

				var regex = guildSettings.BannedPhraseRegex.FirstOrDefault(x => CheckIfRegMatch(message.Content, x.Phrase));
				if (regex != null)
				{
					await HandleBannedPhrasePunishments(timers, guildSettings, guild, message, regex);
				}
			}
			public static async Task HandleBannedPhrasePunishments(ITimersModule timers, IGuildSettings guildSettings, IGuild guild, IMessage message, BannedPhrase phrase)
			{
				await Messages.DeleteMessage(message);

				var user = message.Author as IGuildUser;
				var bpUser = guildSettings.BannedPhraseUsers.FirstOrDefault(x => x.User == user);
				if (bpUser == null)
				{
					guildSettings.BannedPhraseUsers.Add(bpUser = new BannedPhraseUser(user));
				}
				var punishmentType = phrase.Punishment;

				var amountOfMsgs = 0;
				switch (punishmentType)
				{
					case PunishmentType.RoleMute:
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
				if (!TryGetPunishment(guildSettings, punishmentType, amountOfMsgs, out BannedPhrasePunishment punishment))
					return;

				//TODO: include all automatic punishments in this
				await Punishments.AutomaticPunishments(timers, guildSettings, user, punishmentType, false, punishment.PunishmentTime);
				switch (punishmentType)
				{
					case PunishmentType.Kick:
					{
						bpUser.ResetKickCount();
						return;
					}
					case PunishmentType.Ban:
					{
						bpUser.ResetBanCount();
						return;
					}
					case PunishmentType.RoleMute:
					{
						bpUser.ResetRoleCount();
						return;
					}
				}
			}

			public static bool TryGetPunishment(IGuildSettings guildSettings, PunishmentType type, int msgs, out BannedPhrasePunishment punishment)
			{
				punishment = guildSettings.BannedPhrasePunishments.FirstOrDefault(x => x.Punishment == type && x.NumberOfRemoves == msgs);
				return punishment != null;
			}
			public static bool TryGetBannedRegex(IGuildSettings guildSettings, string searchPhrase, out BannedPhrase bannedRegex)
			{
				bannedRegex = guildSettings.BannedPhraseRegex.FirstOrDefault(x => x.Phrase.CaseInsEquals(searchPhrase));
				return bannedRegex != null;
			}
			public static bool TryGetBannedString(IGuildSettings guildSettings, string searchPhrase, out BannedPhrase bannedString)
			{
				bannedString = guildSettings.BannedPhraseStrings.FirstOrDefault(x => x.Phrase.CaseInsEquals(searchPhrase));
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

			public static bool CheckIfRegMatch(string msg, string pattern)
			{
				return Regex.IsMatch(msg, pattern, RegexOptions.IgnoreCase, new TimeSpan(Constants.TICKS_REGEX_TIMEOUT));
			}

			public static void AddSlowmodeUser(SlowmodeGuild smGuild, List<SlowmodeChannel> smChannels, IGuildUser user)
			{
				if (smGuild != null)
				{
					smGuild.Users.ThreadSafeAdd(new SlowmodeUser(user, smGuild.BaseMessages, smGuild.Interval));
				}

				foreach (var smChannel in smChannels.Where(x => (user.Guild as SocketGuild).TextChannels.Select(y => y.Id).Contains(x.ChannelID)))
				{
					smChannel.Users.ThreadSafeAdd(new SlowmodeUser(user, smChannel.BaseMessages, smChannel.Interval));
				}
			}

			public static void HandleBannedPhraseModification(List<BannedPhrase> bannedPhrases, IEnumerable<string> inputPhrases, bool add, out List<string> success, out List<string> failure)
			{
				if (add)
				{
					AddBannedPhrases(bannedPhrases, inputPhrases, out success, out failure);
				}
				else
				{
					RemoveBannedPhrases(bannedPhrases, inputPhrases, out success, out failure);
				}
			}
			public static void AddBannedPhrases(List<BannedPhrase> bannedPhrases, IEnumerable<string> inputPhrases, out List<string> success, out List<string> failure)
			{
				success = new List<string>();
				failure = new List<string>();

				//Don't add duplicate words
				foreach (var str in inputPhrases)
				{
					if (!bannedPhrases.Any(x => x.Phrase.CaseInsEquals(str)))
					{
						success.Add(str);
						bannedPhrases.Add(new BannedPhrase(str, PunishmentType.Nothing));
					}
					else
					{
						failure.Add(str);
					}
				}
			}
			public static void RemoveBannedPhrases(List<BannedPhrase> bannedPhrases, IEnumerable<string> inputPhrases, out List<string> success, out List<string> failure)
			{
				success = new List<string>();
				failure = new List<string>();

				var positions = new List<int>();
				foreach (var potentialPosition in inputPhrases)
				{
					if (int.TryParse(potentialPosition, out int temp) && temp < bannedPhrases.Count)
					{
						positions.Add(temp);
					}
				}

				//Removing by phrase
				if (!positions.Any())
				{
					foreach (var str in inputPhrases)
					{
						var temp = bannedPhrases.FirstOrDefault(x => x.Phrase.Equals(str));
						if (temp != null)
						{
							success.Add(str);
							bannedPhrases.Remove(temp);
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
						if (bannedPhrases.Count - 1 <= position)
						{
							success.Add(bannedPhrases[position]?.Phrase ?? "null");
							bannedPhrases.RemoveAt(position);
						}
						else
						{
							failure.Add("String at position " + position);
						}
					}
				}
			}
		}

		public static class CloseWords
		{
			public static List<CloseWord<T>> GetObjectsWithSimilarNames<T>(IEnumerable<T> suppliedObjects, string input) where T : INameAndText
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
		}

		public static class Misc
		{
			public static string GetSavePath()
			{
				return Properties.Settings.Default.Path;
			}
			public static string GetBotKey()
			{
				return Properties.Settings.Default.BotKey;
			}
			public static void ResetSettings()
			{
				Properties.Settings.Default.BotKey = null;
				Properties.Settings.Default.Path = null;
				Properties.Settings.Default.BotName = null;
				Properties.Settings.Default.BotID = 0;
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
					ConsoleActions.ExceptionToConsole(e);
				}
			}
			public static void DisconnectBot()
			{
				Environment.Exit(0);
			}

			public static bool MakeSureInputIsValidTwitchAccountName(string input)
			{
				//In the bot's case if it's a null name then that just means to not show a stream
				if (String.IsNullOrWhiteSpace(input))
					return true;

				return new Regex("^[a-zA-Z0-9_]{4,25}$", RegexOptions.Compiled).IsMatch(input); //Source: https://www.reddit.com/r/Twitch/comments/32w5b2/username_requirements/cqf8yh0/
			}
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
					ConsoleActions.ExceptionToConsole(e);
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

			public static string FormatNumberedList<T>(this IEnumerable<T> list, string format, params Func<T, object>[] args)
			{
				var count = 0;
				var maxLen = list.Count().ToString().Length;
				//.ToArray() must be used or else String.Format tries to use an overload accepting object as a parameter instead of object[] thus causing an exception
				return String.Join("\n", list.Select(x => String.Format("`{0}.` ", (++count).ToString().PadLeft(maxLen, '0')) + String.Format(@format, args.Select(y => y(x)).ToArray())));
			}
			public static string FormatUser(this IUser user, ulong? userID = 0)
			{
				if (user != null)
				{
					return String.Format("'{0}#{1}' ({2})",
						Formatting.EscapeMarkdown(user.Username, true).CaseInsReplace("discord.gg", Constants.FAKE_DISCORD_LINK),
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
					return String.Format("'{0}' ({1})", Formatting.EscapeMarkdown(role.Name, true), role.Id);
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
					return String.Format("'{0}' ({1}) ({2})", Formatting.EscapeMarkdown(channel.Name, true), (channel is IMessageChannel ? "text" : "voice"), channel.Id);
				}
				else
				{
					return "Irretrievable Channel";
				}
			}
			public static string FormatGuild(this IGuild guild, ulong? guildID = 0)
			{
				if (guild != null)
				{
					return String.Format("'{0}' ({1})", Formatting.EscapeMarkdown(guild.Name, true), guild.Id);
				}
				else
				{
					return String.Format("Irretrievable Guild ({0})", guildID);
				}
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
			public static string CaseInsReplace(this string source, string oldValue, string newValue)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				var previousIndex = 0;
				var index = source.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
				while (index != -1)
				{
					sb.Append(source.Substring(previousIndex, index - previousIndex));
					sb.Append(newValue);
					index += oldValue.Length;

					previousIndex = index;
					index = source.IndexOf(oldValue, index, StringComparison.OrdinalIgnoreCase);
				}
				sb.Append(source.Substring(previousIndex));

				return sb.ToString();
			}
			public static bool CaseInsEverythingSame(this IEnumerable<string> enumerable)
			{
				var array = enumerable.ToArray();
				for (int i = 1; i < array.Length; ++i)
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
			public static bool HasGlobalEmotes(this IGuild guild)
			{
				return guild.Emotes.Any(x => x.IsManaged && x.RequireColons);
			}

			public static void AssertEnumsAreAllCorrectTargetType(this IEnumerable<ObjectVerification> enums, ISnowflakeEntity obj)
			{
				Target correctEnumTarget;
				if (obj is IUser)
				{
					correctEnumTarget = Target.User;
				}
				else if (obj is IChannel)
				{
					correctEnumTarget = Target.Channel;
				}
				else if (obj is IRole)
				{
					correctEnumTarget = Target.Role;
				}
				else
				{
					throw new ArgumentException("Provided object is not a user, channel, or role.");
				}

				foreach (var e in enums)
				{
					var eTarget = (uint)((DiscordObjectTargetAttribute)typeof(ObjectVerification).GetField(e.EnumName()).GetCustomAttribute(typeof(DiscordObjectTargetAttribute)))?.Target;
					if ((eTarget & (uint)correctEnumTarget) != eTarget)
					{
						throw new ArgumentException("Invalid object verification enum provided for a " + correctEnumTarget.EnumName());
					}
				}
			}

			public static int GetLineBreaks(this string str)
			{
				if (str == null)
					return 0;

				return str.Count(x => x == '\r' || x == '\n');
			}

			public static IGuild GetGuild(this IMessage message)
			{
				return (message?.Channel as IGuildChannel)?.Guild;
			}
			public static IGuild GetGuild(this IUser user)
			{
				return (user as IGuildUser)?.Guild;
			}
			public static IGuild GetGuild(this IChannel channel)
			{
				return (channel as IGuildChannel)?.Guild;
			}
			public static IGuild GetGuild(this IRole role)
			{
				return role?.Guild;
			}

			public static List<T> GetUpToAndIncludingMinNum<T>(this List<T> list, params int[] x)
			{
				return list.GetRange(0, Math.Max(0, Math.Min(list.Count, x.Min())));
			}
			public static List<T> GetOutTimedObjects<T>(this List<T> inputList) where T : ITimeInterface
			{
				if (inputList == null)
					return null;

				var eligibleToBeGotten = inputList.Where(x => x.GetTime() <= DateTime.UtcNow).ToList();
				inputList.ThreadSafeRemoveAll(x => eligibleToBeGotten.Contains(x));
				return eligibleToBeGotten;
			}
			public static int GetCountOfItemsInTimeFrame<T>(this List<T> timeList, int timeFrame = 0) where T : ITimeInterface
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

			public static string[] SplitByCharExceptInQuotes(this string inputString, char inputChar)
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
		}
	}
}
