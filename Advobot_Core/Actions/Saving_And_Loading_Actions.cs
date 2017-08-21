using Advobot.Attributes;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.NonSavedClasses;
using Advobot.SavedClasses;
using Advobot.Structs;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot
{
	namespace Actions
	{
		public static class SavingAndLoadingActions
		{
			public static async Task LoadInformation(IDiscordClient client, IBotSettings botSettings, IGuildSettingsModule guildSettingsModule)
			{
				if (botSettings.Loaded)
					return;

				HandleBotID(client.CurrentUser.Id);
				HandleBotName(client.CurrentUser.Username);
				if (botSettings.FirstInstanceOfBotStartingUpWithCurrentKey)
				{
					MiscActions.RestartBot(); //Restart so the bot can get the correct globalInfo loaded
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
				var fileInfo = GetActions.GetBaseBotDirectoryFile(Constants.BOT_SETTINGS_LOCATION);
				if (fileInfo.Exists)
				{
					try
					{
						using (var reader = new StreamReader(fileInfo.FullName))
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
#if DEBUG
						ConsoleActions.WriteLine(innerMostNameSpace + " is not currently in the CommandCategory enum.");
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
					else if (classType.IsNested)
					{
						//Nested commands don't really need to be added since they're added under the class they're nested in
						continue;
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
					var permReqs = permReqsAttr == null ? null : FormattingActions.FormatAttribute(permReqsAttr);

					var otherReqsAttr = (OtherRequirementAttribute)classType.GetCustomAttribute(typeof(OtherRequirementAttribute));
					var otherReqs = otherReqsAttr == null ? null : FormattingActions.FormatAttribute(otherReqsAttr);

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

					temp.Add(new HelpEntry(name, aliases, usage, FormattingActions.JoinNonNullStrings(" | ", new[] { permReqs, otherReqs }), summary, category, defaultEnabled));
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
				return JsonConvert.SerializeObject(obj, Formatting.Indented);
			}
			public static void CreateFile(FileInfo fileInfo)
			{
				if (!fileInfo.Exists)
				{
					Directory.CreateDirectory(fileInfo.DirectoryName);
					fileInfo.Create().Close();
				}
			}
			public static void OverWriteFile(FileInfo fileInfo, string toSave)
			{
				CreateFile(fileInfo);
				using (var writer = new StreamWriter(fileInfo.FullName))
				{
					writer.Write(toSave);
				}
			}
			public static void DeleteFile(FileInfo fileInfo)
			{
				try
				{
					fileInfo.Delete();
				}
				catch (Exception e)
				{
					ConsoleActions.ExceptionToConsole(e);
				}
			}

			public static void LogUncaughtException(object sender, UnhandledExceptionEventArgs e, ILogModule logging)
			{
				var exception = (Exception)e.ExceptionObject;
				var lastRanCommand = logging.RanCommands.LastOrDefault();

				string line;
				if (lastRanCommand.Equals(default(LoggedCommand)))
				{
					line = String.Format("{0}: {1}\n", FormattingActions.FormatDateTime(DateTime.UtcNow), exception.ToString());
				}
				else
				{
					line = String.Format("{0}: {1}\nLast ran command: {2}\n", FormattingActions.FormatDateTime(DateTime.UtcNow), exception.ToString(), lastRanCommand.ToString());
				}

				var crashLogPath = GetActions.GetBaseBotDirectoryFile(Constants.CRASH_LOG_LOCATION);
				CreateFile(crashLogPath);
				//Use File.AppendText instead of new StreamWriter so the text doesn't get overwritten.
				using (var writer = crashLogPath.AppendText())
				{
					writer.WriteLine(line);
				}
			}
		}
	}
}