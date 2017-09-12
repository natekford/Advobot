using Advobot.Attributes;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Modules.GuildSettings;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class SavingAndLoadingActions
	{
		public static async Task LoadInformation(IDiscordClient client, IBotSettings botSettings)
		{
			if (botSettings.Loaded)
			{
				return;
			}

			if (Config.Configuration[ConfigKeys.Bot_Id] != client.CurrentUser.Id.ToString())
			{
				Config.Configuration[ConfigKeys.Bot_Id] = client.CurrentUser.Id.ToString();
				Config.Save();
				ConsoleActions.WriteLine("The bot needs to be restarted in order for the config to be loaded correctly.");
				ClientActions.DisconnectBot();
			}

			await ClientActions.UpdateGame(client, botSettings);

			ConsoleActions.WriteLine("The current bot prefix is: " + botSettings.Prefix);
			ConsoleActions.WriteLine($"Bot took {DateTime.UtcNow.Subtract(Process.GetCurrentProcess().StartTime).TotalMilliseconds:n} milliseconds to load everything.");
			botSettings.SetLoaded();
		}

		public static IBotSettings CreateBotSettings(Type botSettingsType)
		{
			if (botSettingsType == null || !botSettingsType.GetInterfaces().Contains(typeof(IBotSettings)))
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
						botSettings = (IBotSettings)JsonConvert.DeserializeObject(reader.ReadToEnd(), botSettingsType);
					}
					ConsoleActions.WriteLine("The bot information has successfully been loaded.");
				}
				catch (Exception e)
				{
					ConsoleActions.ExceptionToConsole(e);
				}
			}
			else
			{
				ConsoleActions.WriteLine("The bot information file could not be found; using default.");
			}
			botSettings = botSettings ?? (IBotSettings)Activator.CreateInstance(botSettingsType);

			return botSettings;
		}
		public static IGuildSettingsModule CreateGuildSettingsModule(Type guildSettingType)
		{
			if (guildSettingType == null || !guildSettingType.GetInterfaces().Contains(typeof(IGuildSettings)))
			{
				throw new ArgumentException("Invalid type for guild settings provided.");
			}

			return new MyGuildSettingsModule(guildSettingType);
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
				if (defaultEnabledAttr == null)
				{
					throw new InvalidOperationException(name + " does not have a default enabled value set.");
				}

				var similarCmds = temp.Where(x => x.Name.CaseInsEquals(name) || (x.Aliases != null && aliases != null && x.Aliases.Intersect(aliases, StringComparer.OrdinalIgnoreCase).Any()));
				if (similarCmds.Any())
				{
					throw new ArgumentException($"The following commands have conflicts: {String.Join(" + ", similarCmds.Select(x => x.Name))} + {name}");
				}

				temp.Add(new HelpEntry(name, aliases, usage, FormattingActions.JoinNonNullStrings(" | ", new[] { permReqs, otherReqs }), summary, category, defaultEnabledAttr.Enabled));
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
			return JsonConvert.SerializeObject(obj, Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
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

		public static void LogUncaughtException(object sender, UnhandledExceptionEventArgs e)
		{
			var crashLogPath = GetActions.GetBaseBotDirectoryFile(Constants.CRASH_LOG_LOCATION);
			CreateFile(crashLogPath);
			//Use File.AppendText instead of new StreamWriter so the text doesn't get overwritten.
			using (var writer = crashLogPath.AppendText())
			{
				writer.WriteLine($"{FormattingActions.FormatDateTime(DateTime.UtcNow)}: {e.ExceptionObject.ToString()}\n");
			}
		}
	}
}