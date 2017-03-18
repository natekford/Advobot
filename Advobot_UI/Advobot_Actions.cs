using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Advobot
{
	public class Actions
	{
		#region Loads
		//Loading in all necessary information at bot start up
		public static async Task LoadInformation()
		{
			Variables.Bot_ID = Variables.Client.GetCurrentUser().Id;				//Give the variable Bot_ID the id of the bot
			Variables.Bot_Name = Variables.Client.GetCurrentUser().Username;		//Give the variable Bot_Name the username of the bot
			Variables.Bot_Channel = Variables.Bot_Name.ToLower();					//Give the variable Bot_Channel a lowered version of the bot's name

			LoadPermissionNames();													//Gets the names of the permission bits in Discord
			LoadCommandInformation();												//Gets the information of a command (name, aliases, usage, summary). Has to go after LPN
			Variables.HelpList.ForEach(x => Variables.CommandNames.Add(x.Name));	//Gets all the active command names. Has to go after LCI

			await LoadGuilds();														//Loads the guilds that attempted to load before the Bot_ID was gotten.
			await SetGame();														//Set up the game and/or stream

			ResetSpamPrevention(null);												//Start the hourly timer to restart spam prevention
			RemovePunishments(null);												//Start the minutely timer to remove punishments on a user
			StartUpMessages();														//Say all of the start up messages
			Variables.Loaded = true;												//Set a bool stating that everything is done loading.
		}

		//Text said during the startup of the bot
		public static void StartUpMessages()
		{
			if (Variables.Loaded)
				return;

			WriteLine("The current bot prefix is: " + Properties.Settings.Default.Prefix);
			WriteLine("Bot took " + String.Format("{0:n}", TimeSpan.FromTicks(DateTime.UtcNow.ToUniversalTime().Ticks - Variables.StartupTime.Ticks).TotalMilliseconds) + " milliseconds to load everything.");
		}

		//Load the information from the commands
		public static void LoadCommandInformation()
		{
			foreach (var classType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsSubclassOf(typeof(ModuleBase))))
			{
				var className = ((NameAttribute)classType.GetCustomAttribute(typeof(NameAttribute)))?.Text;
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
							usage = attr.Usage;
						}
					}
					//Get the base permissions
					var basePerm = "N/A";
					{
						var attr = (PermissionRequirementAttribute)method.GetCustomAttribute(typeof(PermissionRequirementAttribute));
						if (attr != null)
						{
							basePerm = String.IsNullOrWhiteSpace(attr.AllText) ? "" : "[" + attr.AllText;
							if (!basePerm.Equals("[Administrator"))
							{
								basePerm += basePerm.Contains('[') ? "|" + attr.AnyText + "]" : "[" + attr.AnyText + "]";
							}
							else
							{
								basePerm += "]";
							}
						}
						else if ((BotOwnerRequirementAttribute)method.GetCustomAttribute(typeof(BotOwnerRequirementAttribute)) != null)
						{
							basePerm = "[Bot Owner]";
						}
						else if ((BotOwnerOrGuildOwnerRequirementAttribute)method.GetCustomAttribute(typeof(BotOwnerOrGuildOwnerRequirementAttribute)) != null)
						{
							basePerm = "[Bot Owner|Guild Owner]";
						}
						else if ((GuildOwnerRequirementAttribute)method.GetCustomAttribute(typeof(GuildOwnerRequirementAttribute)) != null)
						{
							basePerm = "[Guild Owner]";
						}
						else if ((UserHasAPermissionAttribute)method.GetCustomAttribute(typeof(UserHasAPermissionAttribute)) != null)
						{
							basePerm = "[Administrator|Any perm starting with 'Manage'|Any perm ending with 'Members']";
						}
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
					}
					//Add it to the helplist
					Variables.HelpList.Add(new HelpEntry(name, aliases, usage, basePerm, text, className, defaultEnabled));
				}
			}
		}

		//Load the permission names
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
			LoadAllChannelPermissionNames();
		}

		//Load the channel permission names
		public static void LoadAllChannelPermissionNames()
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

		//Load the guilds gotten before the Bot_ID is set
		public static async Task LoadGuilds()
		{
			await Variables.GuildsToBeLoaded.ForEachAsync(async x => await LoadGuild(x));
		}

		//Load a guild's info
		public static async Task LoadGuild(IGuild guild)
		{
			//I know I am using .txt docs instead of .json; fite me.
			LoadCommandPreferences(guild);
			LoadBannedPhrasesAndPunishments(guild);
			LoadSelfAssignableRoles(guild);
			await LoadGuildMiscInfo(guild);
			await LoadBotUsers(guild);
			LoadReminds(guild);
			LoadCommandsDisabledByChannel(guild);
		}

		//Load preferences
		public static void LoadCommandPreferences(IGuild guild)
		{
			//Check if this server has any preferences
			var path = GetServerFilePath(guild.Id, Constants.PREFERENCES_FILE);
			var defaultCategory = CommandCategory.Miscellaneous;
			if (!File.Exists(path))
			{
				Variables.HelpList.ForEach(x =>
				{
					var category = Enum.TryParse(x.ClassName, out CommandCategory temp) ? temp : defaultCategory;
					Variables.Guilds[guild.Id].CommandSettings.Add(new CommandSwitch(x.Name, x.DefaultEnabled, category, x?.Aliases));
				});
			}
			else
			{
				using (var file = new StreamReader(path))
				{
					string line;
					while ((line = file.ReadLine()) != null)
					{
						//If the line is empty, do nothing
						if (String.IsNullOrWhiteSpace(line))
							continue;

						//Split before and after the colon, before is the setting name, after is the value
						var values = line.Split(new char[] { ':' }, 2);
						if (values.Length == 2)
						{
							var helpEntry = Variables.HelpList.FirstOrDefault(cmd => CaseInsEquals(cmd.Name, values[0]));
							var category = Enum.TryParse(helpEntry?.ClassName, out CommandCategory temp) ? temp : defaultCategory;
							Variables.Guilds[guild.Id].CommandSettings.Add(new CommandSwitch(values[0], values[1], category, helpEntry?.Aliases));
						}
						else
						{
							WriteLine("ERROR: " + line);
						}
					}
				}
			}
			WriteLoadDone(guild, MethodBase.GetCurrentMethod().Name, "Command Preferences");
		}

		//Load banned words/regex/punishments
		public static void LoadBannedPhrasesAndPunishments(IGuild guild)
		{
			//Check if the file exists
			var path = GetServerFilePath(guild.Id, Constants.BANNED_PHRASES);
			if (!File.Exists(path))
				return;

			//Get the guild info
			var guildInfo = Variables.Guilds[guild.Id];

			//Get the banned phrases and regex
			using (var file = new StreamReader(path))
			{
				string line;
				while ((line = file.ReadLine()) != null)
				{
					//If the line is empty, do nothing
					if (String.IsNullOrWhiteSpace(line))
						continue;
					//Banned phrases
					if (line.StartsWith(Constants.BANNED_PHRASES_CHECK_STRING))
					{
						int index = line.IndexOf(':');
						if (index >= 0 && index < line.Length - 1)
						{
							var phrase = line.Substring(index + 1);
							if (!String.IsNullOrWhiteSpace(phrase))
							{
								guildInfo.BannedStrings.Add(phrase);
							}
						}
					}
					//Banned regex
					else if (line.StartsWith(Constants.BANNED_REGEX_CHECK_STRING))
					{
						int index = line.IndexOf(':');
						if (index >= 0 && index < line.Length - 1)
						{
							var regex = line.Substring(index + 1);
							if (!String.IsNullOrWhiteSpace(regex))
							{
								guildInfo.BannedRegex.Add(new Regex(regex));
							}
						}
					}
					//Punishments
					else if (line.StartsWith(Constants.BANNED_PHRASES_PUNISHMENTS))
					{
						int index = line.IndexOf(':');
						if (index >= 0 && index < line.Length - 1)
						{
							//Split the information in the file
							var args = line.Substring(index + 1).Split(' ');

							//All need to be ifs to check each value

							//Number of removes to activate
							if (!int.TryParse(args[0], out int number))
								continue;

							//The type of punishment
							if (!int.TryParse(args[1], out int punishment))
								continue;

							//The role ID if a role punishment type
							ulong roleID = 0;
							if (punishment == 3 && !ulong.TryParse(args[2], out roleID))
								continue;

							//Get the role
							var role = roleID != 0 ? guild.GetRole(roleID) : null;

							//The time if a time is input
							int time = 0;
							if (role != null && !int.TryParse(args[3], out time))
								continue;

							guildInfo.BannedPhrasesPunishments.Add(new BannedPhrasePunishment(number, (PunishmentType)punishment, role, time));
						}
					}

					//Remove all duplicates
					guildInfo.BannedStrings = guildInfo.BannedStrings.Distinct().ToList();
					guildInfo.BannedRegex = guildInfo.BannedRegex.Distinct().ToList();
					guildInfo.BannedPhrasesPunishments = guildInfo.BannedPhrasesPunishments.Distinct().ToList();
				}
			}

			WriteLoadDone(guild, MethodBase.GetCurrentMethod().Name, "Banned Phrases/Regex/Punishments");
		}

		//Load the self assignable roles
		public static void LoadSelfAssignableRoles(IGuild guild)
		{
			//Check if the file exists
			var path = GetServerFilePath(guild.Id, Constants.SA_ROLES);
			if (!File.Exists(path))
				return;

			//Read the file
			using (var file = new StreamReader(path))
			{
				string line;
				while ((line = file.ReadLine()) != null)
				{
					//If the line is empty, do nothing
					if (String.IsNullOrWhiteSpace(line))
						continue;

					var inputArray = line.Split(' ');

					//Test if ID
					if (!ulong.TryParse(inputArray[0], out ulong ID))
						continue;

					//Test if valid role
					var role = guild.GetRole(ID);
					if (role == null)
						continue;

					//Test if valid group
					if (!int.TryParse(inputArray[1], out int group))
						continue;

					//Check if it's already in any list
					if (Variables.SelfAssignableGroups.Where(x => x.GuildID == guild.Id).Any(x => x.Roles.Any(y => y.Role.Id == ID)))
						continue;

					//Remake the SARole
					var SARole = new SelfAssignableRole(role, group);

					//Check if that group exists already
					if (!Variables.SelfAssignableGroups.Any(x => x.Group == group))
					{
						Variables.SelfAssignableGroups.Add(new SelfAssignableGroup(new List<SelfAssignableRole> { SARole }, group, guild.Id));
					}
					//Add it to the list if it already does exist
					else
					{
						Variables.SelfAssignableGroups.FirstOrDefault(x => x.Group == group).AddRole(SARole);
					}
				}
			}

			WriteLoadDone(guild, MethodBase.GetCurrentMethod().Name, "Self Assignable Roles/Groups");
		}

		//Load the prefix and logActions
		public static async Task LoadGuildMiscInfo(IGuild guild)
		{
			//Check if the file exists
			var path = GetServerFilePath(guild.Id, Constants.MISCGUILDINFO);
			if (!File.Exists(path))
				return;

			var GuildInfo = Variables.Guilds[guild.Id];

			//Find the prefix line
			using (var reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					//Ignore empty lines
					if (String.IsNullOrWhiteSpace(line))
						continue;

					//Guild prefix
					if (line.Contains(Constants.GUILD_PREFIX))
					{
						GuildInfo.SetPrefix(line.Substring(line.IndexOf(':') + 1));
					}
					//Log channel
					else if (line.Contains(Constants.SERVER_LOG_CHECK_STRING))
					{
						var logChannelArray = line.Split(new Char[] { ':' }, 2);
						GuildInfo.SetServerLog((await guild.GetChannelAsync(Convert.ToUInt64(logChannelArray[1]))) as ITextChannel);
					}
					else if (line.Contains(Constants.MOD_LOG_CHECK_STRING))
					{
						var logChannelArray = line.Split(new Char[] { ':' }, 2);
						GuildInfo.SetModLog((await guild.GetChannelAsync(Convert.ToUInt64(logChannelArray[1]))) as ITextChannel);
					}
					//Log actions
					else if (line.Contains(Constants.LOG_ACTIONS))
					{
						var logActions = new List<LogActions>();
						line.Substring(line.IndexOf(':') + 1).Split('/').ToList().ForEach(x =>
						{
							if (Enum.TryParse(x, out LogActions temp))
							{
								logActions.Add(temp);
							}
						});
						GuildInfo.LogActions.AddRange(logActions.Distinct().OrderBy(x => (int)x).ToList());
					}
					//Ignored log channels
					else if (line.Contains(Constants.IGNORED_LOG_CHANNELS))
					{
						var IDs = new List<ulong>();
						line.Substring(line.IndexOf(':') + 1).Split('/').ToList().ForEach(x =>
						{
							if (ulong.TryParse(x, out ulong temp))
							{
								IDs.Add(temp);
							}
						});
						GuildInfo.IgnoredLogChannels.AddRange(IDs.Distinct().ToList());
					}
					//Ignored command channels
					else if (line.Contains(Constants.IGNORED_COMMAND_CHANNELS))
					{
						var IDs = new List<ulong>();
						line.Substring(line.IndexOf(':') + 1).Split('/').ToList().ForEach(x =>
						{
							if (ulong.TryParse(x, out ulong temp))
							{
								IDs.Add(temp);
							}
						});
						GuildInfo.IgnoredCommandChannels.AddRange(IDs.Distinct().ToList());
					}
					//Spam prevention
					else if (line.Contains(Constants.SPAM_PREVENTION))
					{
						var variableNumbers = line.Substring(line.IndexOf(':') + 1).Split('/').ToList();
						if (variableNumbers.Count != 3)
							continue;

						if (!int.TryParse(variableNumbers[0], out int messagesRequired))
							continue;
						if (!int.TryParse(variableNumbers[1], out int mentionsRequired))
							continue;
						if (!int.TryParse(variableNumbers[2], out int votesRequired))
							continue;

						GuildInfo.GlobalSpamPrevention.SetMentionSpamPrevention(new MentionSpamPrevention(messagesRequired, votesRequired, mentionsRequired));
					}
				}
			}

			WriteLoadDone(guild, MethodBase.GetCurrentMethod().Name, "Misc Info");
		}

		//Load the bot users
		public static async Task LoadBotUsers(IGuild guild)
		{
			//Check if the file exists
			var path = GetServerFilePath(guild.Id, Constants.PERMISSIONS);
			if (!File.Exists(path))
				return;

			//Go through each line checking for the users
			var counter = 0;
			var validBotUsers = new List<string>();
			using (var reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (String.IsNullOrWhiteSpace(line))
						continue;

					//Increment the counter
					++counter;

					//Split input
					var inputArray = line.Split(':');
					if (inputArray.Length != 2)
						continue;

					//Check if valid ID
					if (!ulong.TryParse(inputArray[0], out ulong ID))
						continue;

					//Check if valid perms
					if (!uint.TryParse(inputArray[1], out uint perms))
						continue;

					//Get the user
					var user = await guild.GetUserAsync(ID);
					if (user == null)
						continue;

					//If valid user then add to botusers and keep the line
					validBotUsers.Add(line);
					Variables.BotUsers.Add(new BotImplementedPermissions(user, perms));

					//Decrement the counter
					--counter;
				}
			}

			//Remove all bot users who are not in the server anymore
			if (counter != 0)
			{
				using (var writer = new StreamWriter(path))
				{
					writer.WriteLine(String.Join("\n", validBotUsers));
				}
			}

			WriteLoadDone(guild, "LoadBotUsers", "Bot Users");
		}

		//Load the reminds the guild has
		public static void LoadReminds(IGuild guild)
		{
			//Check if the file exists
			var path = GetServerFilePath(guild.Id, Constants.REMINDS);
			if (!File.Exists(path))
				return;

			//Find the prefix line
			using (var reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (String.IsNullOrWhiteSpace(line))
						continue;

					var inputArray = line.Split(new char[] { ':' }, 2);
					if (inputArray.Length != 2)
						continue;

					var name = inputArray[0].Substring(1);
					var text = inputArray[1].Substring(0, inputArray[1].Length - 1);

					Variables.Guilds[guild.Id].Reminds.Add(new Remind(name, text.Replace("\\\\", "\\").Replace("\\n", "\n").Replace("\\'", "\'").Replace("\\\"", "\"")));
				}
			}

			WriteLoadDone(guild, MethodBase.GetCurrentMethod().Name, "Reminds");
		}

		//Load all of the commands that are disabled by channel on the guild
		public static void LoadCommandsDisabledByChannel(IGuild guild)
		{
			//Check if the file exists
			var path = GetServerFilePath(guild.Id, Constants.COMMANDS_DISABLED_BY_CHANNEL);
			if (!File.Exists(path))
				return;

			//Find the prefix line
			using (var reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (String.IsNullOrWhiteSpace(line))
						continue;

					var inputArray = line.Split(new char[] { ' ' });
					if (inputArray.Length != 2)
						continue;

					if (!ulong.TryParse(inputArray[0], out ulong channelID))
						continue;
					var cmd = inputArray[1];

					Variables.Guilds[guild.Id].CommandsDisabledOnChannel.Add(new CommandDisabledOnChannel(channelID, cmd));
				}
			}
			WriteLoadDone(guild, MethodBase.GetCurrentMethod().Name, "Commands Disabled By Channel");
		}

		//Load the most basic information
		public static void LoadBasicInformation()
		{
			//Checks if the OS is Windows or not
			GetOS();
			//Check if console or WPF app
			GetConsoleOrGUI();
		}

		//Possibly load the bot, I don't know.
		public static void MaybeStartBot()
		{
			if (Variables.GotPath && Variables.GotKey && !Variables.Loaded)
			{
				new Program().Start(Variables.Client).GetAwaiter().GetResult();
			}
		}
		#endregion

		#region Gets
		//Complex get a role on the guild
		public static async Task<IRole> GetRole(CommandContext context, string roleName)
		{
			//Remove spaces
			roleName = roleName.Trim().Trim(new char[] { '<', '@', '&', '>' });

			if (UInt64.TryParse(roleName, out ulong roleID))
			{
				return context.Guild.GetRole(roleID);
			}

			var roles = context.Guild.Roles.Where(x => CaseInsEquals(x.Name, roleName)).ToList();
			if (roles.Count == 1)
			{
				return roles.First();
			}
			else if (roles.Count > 1)
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR("Multiple roles with the same name. Please specify by mentioning the role or changing their names."));
			}
			return null;
		}
		
		//Simple get a role on the guild
		public static IRole GetRole(IGuild guild, string roleName)
		{
			//Trim it
			roleName = roleName.Trim();
			//Order them by position (puts everyone first) then reverse so it sorts from the top down
			return guild.Roles.ToList().OrderBy(x => x.Position).Reverse().FirstOrDefault(x => CaseInsEquals(x.Name, roleName));
		}
		
		//Get top position of a user
		public static int GetPosition(IGuild guild, IUser user)
		{
			//Check if the user is the owner
			if (user.Id == guild.OwnerId)
				return Constants.OWNER_POSITION;

			//Make sure they're an IGuildUser
			var tempUser = user as IGuildUser;
			if (user == null)
				return -1;

			//Get the position off of their roles
			return tempUser.RoleIds.ToList().Max(x => guild.GetRole(x).Position);
		}
		
		//Get a user
		public static async Task<IGuildUser> GetUser(IGuild guild, string userName)
		{
			return userName == null ? null : await guild.GetUserAsync(GetUlong(userName.Trim(new char[] { '<', '>', '@', '!' })));
		}
		
		//Get the input to a ulong
		public static ulong GetUlong(string inputString)
		{
			return UInt64.TryParse(inputString, out ulong number) ? number : 0;
		}
		
		//Get if the user/bot can edit the role
		public static async Task<IRole> GetRoleEditAbility(CommandContext context, string input = null, bool ignore_Errors = false, IRole role = null)
		{
			//Check if valid role
			var inputRole = role ?? await GetRole(context, input);
			if (inputRole == null)
			{
				if (!ignore_Errors)
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(Constants.ROLE_ERROR));
				}
				return null;
			}

			//Determine if the user can edit the role
			if (inputRole.Position >= GetPosition(context.Guild, context.User))
			{
				if (!ignore_Errors)
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("`{0}` has a higher position than you are allowed to edit or use.", inputRole.Name)));
				}
				return null;
			}

			//Determine if the bot can edit the role
			if (inputRole.Position >= GetPosition(context.Guild, await context.Guild.GetUserAsync(Variables.Bot_ID)))
			{
				if (!ignore_Errors)
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("`{0}` has a higher position than the bot is allowed to edit or use.", inputRole.Name)));
				}
				return null;
			}

			return inputRole;
		}

		//Get if the user can move people from the channel
		public static IVoiceChannel GetChannelMovability(IVoiceChannel channel, IUser user)
		{
			var guildUser = user as IGuildUser;
			if (guildUser == null)
				return null;

			var perms = guildUser.GetPermissions(channel);
			return (perms.ManagePermissions || perms.MoveMembers) ? channel : null;
		}
		
		//Get if the user can edit the channel
		public static ITextChannel GetChannelEditAbility(ITextChannel channel, IUser user)
		{
			var guildUser = user as IGuildUser;
			if (guildUser == null)
				return null;

			var perms = guildUser.GetPermissions(channel);
			return (perms.ManagePermissions || perms.ManageChannel) ? channel : null;
		}

		public static IVoiceChannel GetChannelEditAbility(IVoiceChannel channel, IUser user)
		{
			var guildUser = user as IGuildUser;
			if (guildUser == null)
				return null;

			var perms = guildUser.GetPermissions(channel);
			return (perms.ManagePermissions || perms.ManageChannel) ? channel : null;
		}

		public static IGuildChannel GetChannelEditAbility(IGuildChannel channel, IUser user)
		{
			var guildUser = user as IGuildUser;
			if (guildUser == null)
				return null;

			var perms = guildUser.GetPermissions(channel);
			return (perms.ManagePermissions || perms.ManageChannel) ? channel : null;
		}

		//Get if the user can edit this channel
		public static async Task<IGuildChannel> GetChannelEditAbility(CommandContext context, string input, bool ignoreErrors = false)
		{
			var channel = await GetChannel(context, input);
			if (channel == null)
			{
				if (!ignoreErrors)
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("No channel was able to be gotten."));
				}
			}
			else if (GetChannelEditAbility(channel, context.User) == null)
			{
				if (!ignoreErrors)
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("You do not have the ability to edit `{0}`.", FormatChannel(channel))));
				}
			}
			else
			{
				return channel;
			}
			return null;
		}
		
		//Get a channel
		public static async Task<IGuildChannel> GetChannel(CommandContext context, string input)
		{
			return await GetChannel(context.Guild, context.Channel, context.Message, input);
		}
		
		//Get a channel without context
		public static async Task<IGuildChannel> GetChannel(IGuild guild, IMessageChannel channel, IUserMessage message, string input)
		{
			//Ignore everything after a space
			if (input.Contains(' '))
			{
				input = input.Substring(0, input.IndexOf(' '));
			}

			//Split at the first forward slash
			var values = input.Split(new char[] { '/' }, 2);
			var channelName = values[0];

			//If a channel mention
			if (ulong.TryParse(channelName.Trim(new char[] { '<', '#', '>' }), out ulong channelID))
			{
				return await guild.GetChannelAsync(channelID);
			}

			//If a name and type
			var channelType = values.Length == 2 ? values[1] : null;
			if (channelType != null && !(CaseInsEquals(channelType, Constants.TEXT_TYPE) || CaseInsEquals(channelType, Constants.VOICE_TYPE)))
			{
				//See which match the name and type given
				var channels = (await guild.GetChannelsAsync()).Where(x => CaseInsEquals(x.Name, channelName) && CaseInsIndexOf(x.GetType().Name, channelType)).ToList();

				//If zero then no channels have the name so return an error message
				if (channels.Count < 1)
				{
					await MakeAndDeleteSecondaryMessage(channel, message, ERROR(String.Format("`{0}` does not exist as a channel on this guild.", channelName)));
				}
				//If only one then return that channel
				else if (channels.Count == 1)
				{
					return channels[0];
				}
				//If more than one return an error message too because how are we supposed to know which one they want?
				else if (channels.Count > 1)
				{
					await MakeAndDeleteSecondaryMessage(channel, message, ERROR("More than one channel exists with the same name."));
				}
			}
			return null;
		}
		
		//Get integer
		public static int GetInteger(string inputString)
		{
			return Int32.TryParse(inputString, out int number) ? number : -1;
		}
		
		//Get bits
		public static async Task<uint> GetBit(CommandContext context, string permission, uint changeValue)
		{
			try
			{
				int bit = Variables.GuildPermissions.FirstOrDefault(x => CaseInsEquals(x.Name, permission)).Position;
				changeValue |= (1U << bit);
				return changeValue;
			}
			catch (Exception)
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("Couldn't parse permission '{0}'", permission)));
				return 0;
			}
		}
		
		//Get the permissions something has on a channel
		public static Dictionary<String, String> GetChannelPermissions(Overwrite overwrite)
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
		
		//Get text channel perms
		public static Dictionary<String, String> GetTextChannelPermissions(Dictionary<String, String> dictionary)
		{
			Variables.ChannelPermissions.Where(x => x.Voice).ToList().ForEach(x => dictionary.Remove(x.Name));
			return dictionary;
		}
		
		//Get voice channel perms
		public static Dictionary<String, String> GetVoiceChannelPermissions(Dictionary<String, String> dictionary)
		{
			Variables.ChannelPermissions.Where(x => x.Text).ToList().ForEach(x => dictionary.Remove(x.Name));
			return dictionary;
		}
		
		//Get a dictionary with the correct perms
		public static Dictionary<String, String> GetPerms(Overwrite overwrite, IGuildChannel channel)
		{
			//Get the general perms from the overwrite given
			Dictionary<String, String> dictionary = GetChannelPermissions(overwrite);

			//See if the channel is a text channel and remove voice channel perms
			if (GetChannelType(channel) == Constants.TEXT_TYPE)
			{
				GetTextChannelPermissions(dictionary);
			}
			//See if the channel is a voice channel and remove text channel perms
			else if (GetChannelType(channel) == Constants.VOICE_TYPE)
			{
				GetVoiceChannelPermissions(dictionary);
			}

			return dictionary;
		}
		
		//Get the input string and permissions
		public static bool GetStringAndPermissions(string input, out string output, out List<string> permissions)
		{
			output = null;
			permissions = null;
			var values = input.Split(new char[] { ' ' });
			if (values.Length == 1)
				return false;

			permissions = values.Last().Split('/').ToList();
			output = String.Join(" ", values.Take(values.Length - 1));

			return output != null && permissions != null;
		}
		
		//Get guild commands
		public static string[] GetCommands(IGuild guild, int number)
		{
			if (!Variables.Guilds.ContainsKey(guild.Id) || Variables.Guilds[guild.Id].DefaultPrefs)
				return null;

			return Variables.Guilds[guild.Id].CommandSettings.Where(x => x.CategoryValue == number).Select(x => x.Name).ToArray();
		}
		
		//Get file paths
		public static string GetServerFilePath(ulong guildId, string fileName)
		{
			//Make sure the bot's directory exists
			var directory = GetDirectory();
			if (!Directory.Exists(directory))
				return null;

			//This string will be similar to C:\Users\User\AppData\Roaming\Discord_Servers_... if on using appdata. If not then it can be anything
			return Path.Combine(directory, guildId.ToString(), fileName);
		}

		//Get the bot's directory
		public static string GetDirectory(string nonGuildFileName = null)
		{
			//Make sure a save path exists
			var folder = Properties.Settings.Default.Path;
			if (!Directory.Exists(folder))
				return null;

			//Get the bot's folder
			var botFolder = String.Format("{0}_{1}", Constants.SERVER_FOLDER, Variables.Bot_ID);

			//Send back the directory
			return String.IsNullOrWhiteSpace(nonGuildFileName) ? Path.Combine(folder, botFolder) : Path.Combine(folder, botFolder, nonGuildFileName);
		}
		
		//Get if a channel is a text or voice channel
		public static string GetChannelType(IGuildChannel channel)
		{
			return CaseInsIndexOf(channel.GetType().Name, Constants.TEXT_TYPE) ? Constants.TEXT_TYPE : Constants.VOICE_TYPE;
		}

		//Get a voice channel by add in a string
		public static async Task<IVoiceChannel> GetVoiceChannel(CommandContext context, string input)
		{
			const string voice = "/voice";
			if (!CaseInsEndsWith(input, voice))
			{
				input += voice;
			}

			return await GetChannel(context, input) as IVoiceChannel;
		}
		
		//Get what the serverlog is
		public static async Task<ITextChannel> GetLogChannel(IGuild guild, string serverOrMod)
		{
			//Get the guild info
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return null;

			//Make sure the channel still exists
			var channel = serverOrMod == Constants.SERVER_LOG_CHECK_STRING ? guildInfo.ServerLog : guildInfo.ModLog;
			if (channel != null)
			{
				channel = await guild.GetTextChannelAsync(channel.Id);
			}

			return channel;
		}
		
		//Get if the user is the owner of the server
		public static async Task<bool> GetIfUserIsOwner(IGuild guild, IUser user)
		{
			if (guild == null)
				return false;

			return (await guild.GetOwnerAsync()).Id == user.Id;
		}
		
		//Get if the user if the bot owner
		public static bool GetIfUserIsBotOwner(IGuild guild, IUser user)
		{
			if (guild == null)
				return false;

			return user.Id == Properties.Settings.Default.BotOwner;
		}

		//Get the permission names to an array
		public static List<string> GetPermissionNames(uint flags)
		{
			var result = new List<string>();
			for (int i = 0; i < 32; ++i)
			{
				if ((flags & (1 << i)) != 0)
				{
					result.Add(Variables.GuildPermissions.FirstOrDefault(x => x.Position == i).Name);
				}
			}
			return result;
		}

		//Get the split input of an input char except when in quotes
		public static string[] SplitByCharExceptInQuotes(string inputString, char inputChar)
		{
			return inputString.Split('"').Select((element, index) => index % 2 == 0 ? element.Split(new[] { inputChar }, StringSplitOptions.RemoveEmptyEntries)
										   : new string[] { element }).SelectMany(element => element).ToArray();
		}

		//Get the variables out of a list
		public static string GetVariableAndRemove(List<string> inputList, string searchTerm)
		{
			//Get the item
			var first = inputList?.Where(x => CaseInsEquals(x.Substring(0, Math.Max(x.IndexOf(':'), 1)), searchTerm)).FirstOrDefault();
			if (first != null)
			{
				//Remove it from the list
				inputList.Remove(first);
				//Return everything after the first colon (the keyword)
				return first.Substring(first.IndexOf(':') + 1);
			}
			return null;
		}

		//Get the variables out of an array
		public static string GetVariable(string[] inputArray, string searchTerm)
		{
			//Get the item
			var first = inputArray?.Where(x => CaseInsEquals(x.Substring(0, Math.Max(x.IndexOf(':'), 1)), searchTerm)).FirstOrDefault();
			return first?.Substring(first.IndexOf(':') + 1);
		}

		//Get the variable out of a string
		public static string GetVariable(string inputString, string searchTerm)
		{
			var input = inputString?.Substring(0, Math.Max(inputString.IndexOf(':'), 1));
			return (inputString != null && CaseInsEquals(input, searchTerm) ? inputString.Substring(inputString.IndexOf(':') + 1) : null);
		}

		//Get the OS
		public static void GetOS()
		{
			var windir = Environment.GetEnvironmentVariable("windir");
			Variables.Windows = !String.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir);
		}

		//Get if it's a console or WPF
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

		//Get the bot owner
		public static IGuildUser GetBotOwner(BotClient client)
		{
			return client.GetGuilds().SelectMany(x => x.Users).FirstOrDefault(x => x.Id == Properties.Settings.Default.BotOwner);
		}

		//Get a group number
		public static async Task<int> GetGroup(string input, CommandContext context)
		{
			return await GetIfGroupIsValid(GetVariable(input, "group"), context);
		}

		//Get a group number
		public static async Task<int> GetGroup(string[] inputArray, CommandContext context)
		{
			return await GetIfGroupIsValid(GetVariable(inputArray, "group"), context);
		}

		//Validate the group
		public static async Task<int> GetIfGroupIsValid(string input, CommandContext context)
		{
			if (String.IsNullOrWhiteSpace(input))
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR("Invalid input for group."));
				return -1;
			}
			//Check if valid number
			if (!int.TryParse(input, out int groupNumber))
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR("Invalid group number."));
				return -1;
			}
			if (groupNumber < 0)
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR("Group number must be positive."));
				return -1;
			}

			return groupNumber;
		}

		//Get a command
		public static CommandSwitch GetCommand(ulong id, string input)
		{
			return Variables.Guilds[id].CommandSettings.FirstOrDefault(x =>
			{
				if (CaseInsEquals(x.Name, input))
				{
					return true;
				}
				else if (x.Aliases != null && CaseInsContains(x.Aliases, input))
				{
					return true;
				}
				else
				{
					return false;
				}
			});
		}

		//Get multiple commands
		public static List<CommandSwitch> GetMultipleCommands(ulong id, CommandCategory category)
		{
			return Variables.Guilds[id].CommandSettings.Where(x => x.CategoryEnum == category).ToList();
		}

		//Get if a command is valid
		public static async Task<IResult> GetIfCommandIsValidAndExecute(CommandContext context, int argPos, IDependencyMap map)
		{
			//Check to make sure everything is loaded
			if (!Variables.Loaded)
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR("Please wait until everything is loaded."));
				return null;
			}
			//Check if a command is disabled
			else if (!CheckCommandEnabled(context, argPos))
			{
				return null;
			}
			//Check if the bot still has admin
			else if (!(await context.Guild.GetCurrentUserAsync()).GuildPermissions.Administrator)
			{
				//If the server has been told already, ignore future commands fully
				if (Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts.Contains(context.Guild))
					return null;

				//Tell the guild that the bot needs admin (because I cba to code in checks if the bot has the permissions required for a lot of things)
				await SendChannelMessage(context, "This bot will not function without the `Administrator` permission, sorry.");

				//Add the guild to the list
				Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts.Add(context.Guild);
				return null;
			}
			else
			{
				return await CommandHandler.Commands.ExecuteAsync(context, argPos, map);
			}
		}

		//Get a list of lines in a text doc that aren't the targetted ones
		public static List<string> GetValidLines(string path, string checkString, bool getCheckString = false)
		{
			CreateFile(path);

			var validLines = new List<string>();
			using (var reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (String.IsNullOrWhiteSpace(line))
					{
						continue;
					}
					else if (checkString == null)
					{
						validLines.Add(line);
					}
					else if (!CaseInsIndexOf(line, checkString))
					{
						validLines.Add(line);
					}
					else if (getCheckString)
					{
						return new List<string>() { line };
					}
				}
			}
			return validLines;
		}

		//Get the help entry string
		public static string GetHelpString(HelpEntry help)
		{
			return String.Format("**Aliases:** {0}\n**Usage:** {1}\n\n**Base Permission(s):**\n{2}\n\n**Description:**\n{3}",
				String.Join(", ", help.Aliases),
				help.Usage,
				help.BasePerm,
				help.Text);
		}

		//Get the amount of line breaks a string has
		public static int GetLineBreaks(string input)
		{
			return input.Count(y => y == '\n' || y == '\r');
		}

		//Get if a user can be modified by another user
		public static bool UserCanBeModifiedByUser(CommandContext context, IGuildUser user)
		{
			var bannerPosition = GetPosition(context.Guild, context.User);
			var banneePosition = user == null ? -1 : GetPosition(context.Guild, user);
			return bannerPosition > banneePosition;
		}

		//Get if the bot can modify a user
		public static async Task<bool> UserCanBeModifiedByBot(CommandContext context, IGuildUser user)
		{
			var bannerPosition = GetPosition(context.Guild, await context.Guild.GetUserAsync(Variables.Bot_ID));
			var banneePosition = user == null ? -1 : GetPosition(context.Guild, user);
			return bannerPosition > banneePosition;
		}

		//Get the invites on a guild
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

		//Get the invite a user joined on
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
				if (newInvs.Count() == 0 && CaseInsContains(guild.Features.ToList(), Constants.VANITY_URL))
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
		#endregion

		#region Roles
		//Create a role on the server if it's not found
		public static async Task<IRole> CreateMuteRoleIfNotFound(IGuild guild, string roleName)
		{
			//Create the role if not found
			var role = GetRole(guild, roleName);
			if (role == null)
			{
				role = await guild.CreateRoleAsync(roleName);
			}
			//Change its guild perms
			await role.ModifyAsync(x => x.Permissions = new GuildPermissions(0));
			//Change the perms it has on every single text channel
			(await guild.GetTextChannelsAsync()).ToList().ForEach(x =>
			{
				x.AddPermissionOverwriteAsync(role, new OverwritePermissions(0, 805316689));
			});
			//Give the role back
			return role;
		} 
		
		//Give the user the role
		public static async Task GiveRole(IGuildUser user, IRole role)
		{
			if (role == null)
				return;
			if (user.RoleIds.Contains(role.Id))
				return;
			await user.AddRolesAsync(role);
		}
		
		//Give the user multiple roles
		public static async Task GiveRole(IGuildUser user, IRole[] roles)
		{
			await user.AddRolesAsync(roles);
		}
		
		//Take multiple roles from a user
		public static async Task TakeRole(IGuildUser user, IRole[] roles)
		{
			if (roles.Count() == 0)
				return;
			await user.RemoveRolesAsync(roles);
		}
		
		//Take a single role from a user
		public static async Task TakeRole(IGuildUser user, IRole role)
		{
			if (role == null)
				return;
			await user.RemoveRolesAsync(role);
		}
		#endregion

		#region Message Removal
		//Remove secondary messages
		public static async Task MakeAndDeleteSecondaryMessage(CommandContext context, string secondStr, Int32 time = Constants.WAIT_TIME)
		{
			var secondMsg = await context.Channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + secondStr);
			RemoveCommandMessages(context.Channel, new List<IMessage> { secondMsg, context.Message }, time);
		}
		
		//Remove secondary messages without context
		public static async Task MakeAndDeleteSecondaryMessage(IMessageChannel channel, IUserMessage message, string secondStr, Int32 time = Constants.WAIT_TIME)
		{
			var secondMsg = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + secondStr);
			RemoveCommandMessages(channel as ITextChannel, new List<IMessage> { secondMsg, message }, time);
		}
		
		//Remove messages
		public static async Task RemoveMessages(IMessageChannel channel, int requestCount)
		{
			var guildChannel = channel as ITextChannel;
			if (guildChannel == null)
				return;

			while (requestCount > 0)
			{
				//Get the current messages and ones that aren't null
				var messages = (await channel.GetMessagesAsync(requestCount).ToList()).SelectMany(x => x).ToList();
				if (messages.Count == 0)
					break;

				//Delete them in a try catch due to potential errors
				try
				{
					await DeleteMessages(channel, messages);
				}
				catch
				{
					WriteLine(String.Format("Unable to delete {0} messages on the guild {1} on channel {2}.", messages.Count, FormatGuild(guildChannel.Guild), FormatChannel(channel)));
				}

				//Lower the request count
				requestCount -= messages.Count;
			}
		}
		
		//Remove messages given a user id
		public static async Task RemoveMessages(IMessageChannel channel, IUser user, int requestCount)
		{
			var guildChannel = channel as ITextChannel;
			if (guildChannel == null)
				return;

			//Make sure there's a user id
			if (user == null)
			{
				await RemoveMessages(channel, requestCount);
				return;
			}

			while (requestCount > 0)
			{
				//Get the current messages and ones that aren't null
				var messages = (await channel.GetMessagesAsync(requestCount).ToList()).SelectMany(x => x).Where(x => x.Author == user).ToList();
				if (messages.Count == 0)
					break;

				//Delete them in a try catch due to potential errors
				try
				{
					await DeleteMessages(channel, messages);
				}
				catch
				{
					WriteLine(String.Format("Unable to delete {0} messages on the guild {1} on channel {2}.", messages.Count, FormatGuild(guildChannel.Guild), FormatChannel(channel)));
				}

				//Lower the request count
				requestCount -= messages.Count;
			}
		}

		//Delete messages that aren't null
		public static async Task DeleteMessages(IMessageChannel channel, List<IMessage> messages)
		{
			var guildChannel = channel as ITextChannel;
			if (guildChannel == null)
				return;

			//Delete them in a try catch due to potential errors
			try
			{
				await channel.DeleteMessagesAsync(messages.Where(x => x != null).Distinct());
			}
			catch
			{
				WriteLine(String.Format("Unable to delete {0} messages on the guild {1} on channel {2}.", messages.Count, FormatGuild(guildChannel.Guild), FormatChannel(channel)));
			}
		}

		//Delete a message that isn't null
		public static async Task DeleteMessage(IMessage message)
		{
			if (message == null)
				return;

			try
			{
				await message.DeleteAsync();
			}
			catch
			{
				WriteLine(String.Format("Unable to delete the message {0} on channel {1}.", message.Id, FormatChannel(message.Channel)));
			}
		}
		#endregion

		#region Message Formatting
		//Format the error message
		public static string ERROR(string message)
		{
			return Constants.ZERO_LENGTH_CHAR + Constants.ERROR_MESSAGE + message;
		}
		
		//Send a message with a zero length char at the front
		public static async Task<IMessage> SendChannelMessage(CommandContext context, string message)
		{
			if (context.Channel == null || !Variables.Guilds.ContainsKey(context.Guild.Id))
				return null;

			return await context.Channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + message);
		}

		//Send a message with a zero length char at the front
		public static async Task<IMessage> SendChannelMessage(IMessageChannel channel, string message)
		{
			if (channel == null || !Variables.Guilds.ContainsKey((channel as ITextChannel).GuildId))
				return null;

			return await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + message);
		}

		public static async Task<IMessage> SendDMMessage(IDMChannel channel, string message)
		{
			if (channel == null)
				return null;

			return await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + message);
		}
		
		//Edit message log message
		public static async Task FormatEditMessage(ITextChannel logChannel, string time, IGuildUser user, IMessageChannel channel, string before, string after)
		{
			await SendChannelMessage(logChannel, String.Format("{0} **EDIT:** `{1}` **IN** `#{2}`\n**FROM:** ```\n{3}```\n**TO:** ```\n{4}```", time, FormatUser(user), FormatChannel(channel), before, after));
		}
		
		//Get rid of certain elements to make messages look neater
		public static string ReplaceMarkdownChars(string input)
		{
			if (String.IsNullOrWhiteSpace(input))
				return "";

			//Matching
			Regex empty = new Regex("[*`]");
			Regex newLines = new Regex("[\n]{2}");

			//Actually removing
			input = empty.Replace(input, "");
			while (input.Contains("\n\n"))
			{
				input = newLines.Replace(input, "\n");
			}

			return input;
		}

		//Format the guild string for use in the console
		public static string FormatGuild(IGuild guild)
		{
			return String.Format("'{0}' ({1})", guild.Name, guild.Id);
		}

		//Format the user string
		public static string FormatUser(IUser user)
		{
			return String.Format("{0}#{1} ({2})", String.IsNullOrWhiteSpace(user.Username) ? "Irretrievable" : user.Username, user.Discriminator, user.Id);
		}

		//Format the channel string
		public static string FormatChannel(IChannel channel)
		{
			var tempChan = channel as IGuildChannel;
			return String.Format("{0} ({1}) ({2})", channel.Name, GetChannelType(tempChan), channel.Id);
		}

		//Format the role string
		public static string FormatRole(IRole role)
		{
			return String.Format("{0} ({1})", role.Name, role.Id);
		}

		//Remove all new lines
		public static string RemoveNewLines(string input)
		{
			return input.Replace(Environment.NewLine, "").Replace("\r", "").Replace("\n", "");
		}

		//Format all things that have been logged
		public static string FormatLoggedThings()
		{
			const int spacing = Constants.PAD_RIGHT;
			return String.Format("{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}\n{8}\n{9}\n{10}",
				String.Format("{0}{1}", "Logged Actions:".PadRight(spacing), "Count:"),
				String.Format("{0}{1}", "Joins:".PadRight(spacing), Variables.LoggedJoins),
				String.Format("{0}{1}", "Leaves:".PadRight(spacing), Variables.LoggedLeaves),
				String.Format("{0}{1}", "Bans:".PadRight(spacing), Variables.LoggedBans),
				String.Format("{0}{1}", "Unbans:".PadRight(spacing), Variables.LoggedUnbans),
				String.Format("{0}{1}", "User changes:".PadRight(spacing), Variables.LoggedUserChanges),
				String.Format("{0}{1}", "Edits:".PadRight(spacing), Variables.LoggedEdits),
				String.Format("{0}{1}", "Deletes:".PadRight(spacing), Variables.LoggedDeletes),
				String.Format("{0}{1}", "Images:".PadRight(spacing), Variables.LoggedImages),
				String.Format("{0}{1}", "Gifs:".PadRight(spacing), Variables.LoggedGifs),
				String.Format("{0}{1}", "Files:".PadRight(spacing), Variables.LoggedFiles));
		}

		//Format the content of messages that were deleted
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
							FormatUser(x.Author),
							FormatChannel(x.Channel),
							x.CreatedAt.ToString("HH:mm:ss"),
							ReplaceMarkdownChars((String.IsNullOrEmpty(msgContent) ? msgContent : msgContent + "\n") + description)));
					}
					else
					{
						deletedMessagesContent.Add(String.Format("`{0}` **IN** `{1}` **SENT AT** `[{2}]`\n```\n{3}```",
							FormatUser(x.Author),
							FormatChannel(x.Channel),
							x.CreatedAt.ToString("HH:mm:ss"),
							"An embed which was unable to be gotten."));
					}
				}
				//See if any attachments were put in
				else if (x.Attachments.Any())
				{
					var content = String.IsNullOrEmpty(x.Content) ? "EMPTY MESSAGE" : x.Content;
					deletedMessagesContent.Add(String.Format("`{0}` **IN** `{1}` **SENT AT** `[{2}]`\n```\n{3}```",
						FormatUser(x.Author),
						FormatChannel(x.Channel),
						x.CreatedAt.ToString("HH:mm:ss"),
						ReplaceMarkdownChars(content + " + " + x.Attachments.ToList().First().Filename)));
				}
				//Else add the message in normally
				else
				{
					var content = String.IsNullOrEmpty(x.Content) ? "EMPTY MESSAGE" : x.Content;
					deletedMessagesContent.Add(String.Format("`{0}` **IN** `{1}` **SENT AT** `[{2}]`\n```\n{3}```",
						FormatUser(x.Author),
						FormatChannel(x.Channel),
						x.CreatedAt.ToString("HH:mm:ss"),
						ReplaceMarkdownChars(content)));
				}
			});
			return deletedMessagesContent;
		}

		//Send the messages for deleting messages
		public static async Task SendDeleteMessage(IGuild guild, ITextChannel channel, List<string> inputList)
		{
			//Get the character count
			int characterCount = 0;
			inputList.ForEach(x => characterCount += (x.Length + 100));

			if (inputList.Count == 0)
			{
				return;
			}
			else if (inputList.Count <= 5 && characterCount < Constants.LENGTH_CHECK)
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
					await UploadTextFile(guild, channel, content, "Deleted_Messages_", "Deleted Messages");
				}
			}
		}
		#endregion

		#region Uploads
		//Upload various text to a text uploader with a string
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
					//Double check that mark down characters have been removed
					var response = client.UploadString("https://hastebin.com/documents", ReplaceMarkdownChars(text));

					//Send the url back
					output = String.Concat("https://hastebin.com/raw/", hasteKeyRegex.Match(response).Groups["key"]);
				}
				catch (Exception e)
				{
					output = e.Message;
					return false;
				}
			}
			return true;
		}
		
		//Upload a text file with a list of messages
		public static async Task UploadTextFile(IGuild guild, IMessageChannel channel, List<string> textList, string fileName, string messageHeader)
		{
			//Messages in the format to upload
			var text = ReplaceMarkdownChars(String.Join("\n-----\n", textList));
			await UploadTextFile(guild, channel, text, fileName, messageHeader);
		}
		
		//Upload a text file with a string
		public static async Task UploadTextFile(IGuild guild, IMessageChannel channel, string text, string fileName, string fileMessage = null)
		{
			//Get the file path
			var file = fileName + DateTime.UtcNow.ToString("MM-dd_HH-mm-ss") + Constants.FILE_EXTENSION;
			var path = GetServerFilePath(guild.Id, file);
			if (path == null)
				return;

			//Double check that all markdown characters are removed
			text = ReplaceMarkdownChars(text);
			//Make sure a file message exists
			fileMessage = String.IsNullOrWhiteSpace(fileMessage) ? "" : String.Format("**{0}:**", fileMessage);

			//Create the temporary file
			if (!File.Exists(GetServerFilePath(guild.Id, file)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
			}
			//Write to the temporary file
			using (var writer = new StreamWriter(path, true))
			{
				writer.WriteLine(text);
			}
			//Upload the file
			await channel.SendFileAsync(path, fileMessage);
			//Delete the file
			File.Delete(path);
		}

		//Upload a guild icon or bot icon
		public static async Task SetPicture(CommandContext context, string input, bool user)
		{
			//See if the user wants to remove the icon
			if (input != null && CaseInsEquals(input, "remove"))
			{
				if (!user)
				{
					await context.Guild.ModifyAsync(x => x.Icon = new Image());
				}
				else
				{
					await context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image());
				}
				await SendChannelMessage(context, String.Format("Successfully removed the {0}'s icon.", user ? "bot" : "guild"));
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
			var downloadUploadAndDelete = Task.Run(async () =>
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
					//Change the guild's icon to the downloaded image
					await (user ? context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(imgStream)) : context.Guild.ModifyAsync(x => x.Icon = new Image(imgStream)));
				}

				//Delete the file and send a success message
				File.Delete(path);
				typing.Dispose();
				await DeleteMessage(msg);
				await SendChannelMessage(context, String.Format("Successfully changed the {0} icon.", user ? "bot" : "guild"));
			});
		}

		//Validate URL
		public static bool ValidateURL(string input)
		{
			if (input == null)
				return false;

			return Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
		}
		#endregion

		#region Embeds
		//Send an embedded object
		public static async Task<IMessage> SendEmbedMessage(IMessageChannel channel, EmbedBuilder embed)
		{
			var guildChannel = channel as ITextChannel;
			if (guildChannel == null)
				return null;
			var guild = guildChannel.Guild;
			if (guild == null || !Variables.Guilds.ContainsKey(guild.Id))
				return null;

			//Replace all instances of the base prefix with the guild's prefix
			var guildPrefix = Variables.Guilds[guild.Id].Prefix;
			if (!String.IsNullOrWhiteSpace(guildPrefix))
			{
				embed.Description.Replace(Properties.Settings.Default.Prefix, guildPrefix);
			}

			try
			{
				//Generate the message
				return await guildChannel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR, embed: embed);
			}
			//Embeds fail every now and then and I haven't been able to find the exact problem yet (I know fields are a problem, but not in this case)
			catch (Exception e)
			{
				ExceptionToConsole(MethodBase.GetCurrentMethod().Name, e);
				return null;
			}
		}
		
		//Make a new embed builder
		public static EmbedBuilder MakeNewEmbed(string title = null, string description = null, Color? color = null, string imageURL = null, string URL = null, string thumbnailURL = null)
		{
			//Make the embed builder
			var embed = new EmbedBuilder().WithColor(Constants.BASE).WithCurrentTimestamp();

			//Validate the URLs
			imageURL = ValidateURL(imageURL) ? imageURL : null;
			URL = ValidateURL(URL) ? URL : null;
			thumbnailURL = ValidateURL(thumbnailURL) ? thumbnailURL : null;

			//Add in the properties
			if (title != null)
			{
				embed.WithTitle(title.Substring(0, Math.Min(Constants.TITLE_MAX_LENGTH, title.Length)));
			}
			if (description != null)
			{
				var output = description;
				//Descriptions can only be 2048 characters max
				if (description.Length > Constants.EMBED_MAX_LENGTH_LONG)
				{
					if (TryToUploadToHastebin(description, out output))
					{
						output = String.Format("Content is past {0} characters. Click [here]({1}) to see it.", Constants.EMBED_MAX_LENGTH_LONG, output);
					}
				}
				//Mobile can only show up to 20 or so lines on the description part of an embed
				else if (GetLineBreaks(description) > Constants.DESCRIPTION_MAX_LINES)
				{
					if (TryToUploadToHastebin(description, out output))
					{
						output = String.Format("Content is past {0} new lines. Click [here]({1}) to see it.", Constants.DESCRIPTION_MAX_LINES, output);
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
		
		//Make a new author for an embed
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
				author.WithName(name.Substring(0, Math.Min(Constants.TITLE_MAX_LENGTH, name.Length)));
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
		
		//Make a new footer for an embed
		public static EmbedBuilder AddFooter(EmbedBuilder embed, string text = null, string iconURL = null)
		{
			//Make the footer builder
			var footer = new EmbedFooterBuilder();

			//Verify the URL
			iconURL = ValidateURL(iconURL) ? iconURL : null;

			//Add in the properties
			if (text != null)
			{
				footer.WithText(text.Substring(0, Math.Min(Constants.EMBED_MAX_LENGTH_LONG, text.Length)));
			}
			if (iconURL != null)
			{
				footer.WithIconUrl(iconURL);
			}

			return embed.WithFooter(footer);
		}
		
		//Add a field to an embed
		public static EmbedBuilder AddField(EmbedBuilder embed, string name, string value, bool isInline = true)
		{
			if ((String.IsNullOrWhiteSpace(name) && String.IsNullOrWhiteSpace(value)) || embed.Build().Fields.Count() >= Constants.FIELDS_MAX)
				return embed;

			//Get the name and value
			name = String.IsNullOrWhiteSpace(name) ? "Placeholder" : name.Substring(0, Math.Min(Constants.TITLE_MAX_LENGTH, name.Length));
			value = String.IsNullOrWhiteSpace(name) ? "Placeholder" : value.Substring(0, Math.Min(value.Length, Constants.MAX_LENGTH_FOR_HASTEBIN));

			embed.AddField(x =>
			{
				var outputValue = value;
				//Embeds can only show up to 1024 chars per field
				if (value.Length > Constants.EMBED_MAX_LENGTH_SHORT)
				{
					if (TryToUploadToHastebin(value, out outputValue))
					{
						outputValue = String.Format("Field has more than {0} characters; please click [here]({1}) to see the content.", Constants.EMBED_MAX_LENGTH_SHORT, outputValue);
					}
				}
				//Fields can only show up to five lines on mobile
				else if (GetLineBreaks(value) > Constants.FIELD_MAX_LINES)
				{
					if (TryToUploadToHastebin(value, out outputValue))
					{
						outputValue = String.Format("Field has more than {0} new lines; please click [here]({1}) to see the content.", Constants.FIELD_MAX_LINES, outputValue);
					}
				}

				//Actually add in the values
				x.Name = name;
				x.Value = outputValue;
				x.IsInline = isInline;
			});

			return embed;
		}

		//Send an embed that potentially has a really big description
		public static async Task SendPotentiallyBigEmbed(IGuild guild, IMessageChannel channel, EmbedBuilder embed, string input, string fileName)
		{
			//Send the embed
			await SendEmbedMessage(channel, embed);

			//If the description is the too long message then upload the string
			if (embed.Description == Constants.HASTEBIN_ERROR)
			{
				//Send the file
				await UploadTextFile(guild, channel, input, fileName);
			}
		}
		#endregion

		#region Console
		//Write to the console with a timestamp
		public static void WriteLine(string text)
		{
			Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + ReplaceMarkdownChars(text));
		}
		
		//Send an exception message to the console
		public static void ExceptionToConsole(string method, Exception e)
		{
			if (e == null)
				return;

			WriteLine(method + " EXCEPTION: " + e.ToString());
		}

		//Write when a load is done
		public static void WriteLoadDone(IGuild guild, string method, string name)
		{
			Variables.Guilds[guild.Id].TurnDefaultPrefsOff();
			WriteLine(String.Format("{0}: {1} for the guild {2} have been loaded.", method, name, FormatGuild(guild)));
		}
		#endregion

		#region Server/Mod Log
		//Check if the bot can type in a logchannel
		public static async Task<bool> PermissionCheck(ITextChannel channel)
		{
			//Return false if the channel doesn't exist
			if (channel == null)
				return false;

			//Get the bot
			var bot = await channel.Guild.GetUserAsync(Variables.Bot_ID);

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

		//Set the server or mod log
		public static async Task<ITextChannel> SetServerOrModLog(CommandContext context, string input, string serverOrMod)
		{
			return await SetServerOrModLog(context.Guild, context.Channel, context.Message, input, serverOrMod);
		}
		
		//Set the server or mod log without context
		public static async Task<ITextChannel> SetServerOrModLog(IGuild guild, IMessageChannel channel, IUserMessage message, string input, string serverOrMod)
		{
			//See if not null
			if (String.IsNullOrWhiteSpace(input))
			{
				await MakeAndDeleteSecondaryMessage(channel, message, ERROR("No channel specified."));
				return null;
			}
			else if (CaseInsEquals(input, "off"))
			{
				return await SetServerOrModLog(guild, channel, message, null as ITextChannel, serverOrMod);
			}

			//Get the channel with its ID
			var logChannel = await GetChannel(guild, channel, message, input) as ITextChannel;
			if (logChannel == null)
			{
				await MakeAndDeleteSecondaryMessage(channel, message, ERROR(String.Format("Unable to set the logchannel on `{0}`.", input)));
				return null;
			}

			return await SetServerOrModLog(guild, channel, message, logChannel, serverOrMod);
		}
		
		//Set the server and mod log with an already gotten channel
		public static async Task<ITextChannel> SetServerOrModLog(IGuild guild, IMessageChannel channel, IUserMessage message, ITextChannel inputChannel, string serverOrMod)
		{
			//Create the file if it doesn't exist
			var path = GetServerFilePath(guild.Id, Constants.MISCGUILDINFO);
			if (!File.Exists(path))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				var newFile = File.Create(path);
				newFile.Close();
			}

			//Set it on the bot's info
			if (serverOrMod == Constants.SERVER_LOG_CHECK_STRING)
			{
				Variables.Guilds[guild.Id].SetServerLog(inputChannel);
			}
			else if (serverOrMod == Constants.MOD_LOG_CHECK_STRING)
			{
				Variables.Guilds[guild.Id].SetModLog(inputChannel);
			}

			//Find the lines that aren't the current serverlog line
			var validLines = new List<string>();
			using (var reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (line.Contains(serverOrMod))
					{
						if ((inputChannel != null) && (line.Contains(inputChannel.Id.ToString())))
						{
							await MakeAndDeleteSecondaryMessage(channel, message, String.Format("Channel is already the current {0}.", serverOrMod));
							return null;
						}
					}
					else if (!line.Contains(serverOrMod))
					{
						validLines.Add(line);
					}
				}
			}

			//Add the lines that do not include serverlog and  the new serverlog line
			using (var writer = new StreamWriter(path))
			{
				if (inputChannel == null)
				{
					writer.WriteLine(serverOrMod + ":" + null + "\n" + String.Join("\n", validLines));
					await MakeAndDeleteSecondaryMessage(channel, message, String.Format("Disabled the {0}.", serverOrMod));
					return null;
				}
				else
				{
					writer.WriteLine(serverOrMod + ":" + inputChannel.Id + "\n" + String.Join("\n", validLines));
				}
			}

			return inputChannel;
		}

		//Logging images
		public static async Task ImageLog(ITextChannel channel, IMessage message, bool embeds)
		{
			//Check if the guild has image logging enabled
			if (!Variables.Guilds[channel.Guild.Id].LogActions.Contains(LogActions.ImageLog))
				return;

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
				if (CaseInsContains(Constants.VALID_IMAGE_EXTENSIONS, Path.GetExtension(x)))
				{
					var embed = MakeNewEmbed(null, null, Constants.ATCH, x);
					AddFooter(embed, "Attached Image");
					AddAuthor(embed, String.Format("{0} in #{1}", FormatUser(user), message.Channel), user.GetAvatarUrl(), x);
					await SendEmbedMessage(channel, embed);

					++Variables.LoggedImages;
				}
				//Gif attachment
				else if (CaseInsContains(Constants.VALID_GIF_EXTENTIONS, Path.GetExtension(x)))
				{
					var embed = MakeNewEmbed(null, null, Constants.ATCH, x);
					AddFooter(embed, "Attached Gif");
					AddAuthor(embed, String.Format("{0} in #{1}", FormatUser(user), message.Channel), user.GetAvatarUrl(), x);
					await SendEmbedMessage(channel, embed);

					++Variables.LoggedGifs;
				}
				//Random file attachment
				else
				{
					var embed = MakeNewEmbed(null, null, Constants.ATCH, x);
					AddFooter(embed, "Attached File");
					AddAuthor(embed, String.Format("{0} in #{1}", FormatUser(user), message.Channel), user.GetAvatarUrl(), x);
					await SendEmbedMessage(channel, embed);

					++Variables.LoggedFiles;
				}
			});
			//Embedded images
			await embedURLs.Distinct().ToList().ForEachAsync(async x =>
			{
				var embed = MakeNewEmbed(null, null, Constants.ATCH, x);
				AddFooter(embed, "Embedded Image");
				AddAuthor(embed, String.Format("{0} in #{1}", FormatUser(user), message.Channel), user.GetAvatarUrl(), x);
				await SendEmbedMessage(channel, embed);

				++Variables.LoggedImages;
			});
			//Embedded videos/gifs
			await videoEmbeds.GroupBy(x => x.Url).Select(x => x.First()).ToList().ForEachAsync(async x =>
			{
				var embed = MakeNewEmbed(null, null, Constants.ATCH, x.Thumbnail.Value.Url);
				AddFooter(embed, "Embedded " + (CaseInsContains(Constants.VALID_GIF_EXTENTIONS, Path.GetExtension(x.Thumbnail.Value.Url)) ? "Gif" : "Video"));
				AddAuthor(embed, String.Format("{0} in #{1}", FormatUser(user), message.Channel), user.GetAvatarUrl(), x.Url);
				await SendEmbedMessage(channel, embed);

				++Variables.LoggedGifs;
			});
		}

		//Check if the serverlog exists and if the bot can use it
		public static async Task<ITextChannel> VerifyLogChannel(IGuild guild, ITextChannel channel)
		{
			//Check to make sure the bot can post to there
			return await PermissionCheck(channel) ? channel : null;
		}

		public static IMessage VerifyMessage(IMessage message)
		{
			//Make sure the message doesn't come from a bot
			return !(message == null || message.Author.IsBot && message.Author.Id != Variables.Bot_ID) ? message : null;
		}

		public static IGuild GetGuildFromMessage(IMessage message)
		{
			//Check if the guild can be gotten from the message's channel or author
			return message != null ? (message.Channel as IGuildChannel)?.Guild ?? (message.Author as IGuildUser)?.Guild : null;
		}

		public static IGuild GetGuildFromUser(IUser user)
		{
			return (user as IGuildUser)?.Guild;
		}

		public static IGuild GetGuildFromChannel(IChannel channel)
		{
			return (channel as IGuildChannel)?.Guild;
		}

		public static IGuild GetGuildFromRole(IRole role)
		{
			return role?.Guild;
		}

		public static IGuild VerifyLoggingIsEnabledOnThisChannel(IMessage message)
		{
			var guild = GetGuildFromMessage(message);
			//Check if the message was sent on an ignored channel. If not give back the guild, if so send back null.
			return guild != null && !Variables.Guilds[guild.Id].IgnoredLogChannels.Contains(message.Channel.Id) ? guild : null;
		}

		//Verify if the given action is being logged
		public static IGuild VerifyLoggingAction(IGuild guild, LogActions logAction)
		{
			//If the guild is null send back null. If the logaction being tested isn't turned on send back null.
			return guild != null && Variables.Guilds[guild.Id].LogActions.Contains(logAction) ? guild : null;
		}

		//Verify the guild and log channels with a message
		public static IGuild VerifyGuild(IMessage message, LogActions logAction)
		{
			//Make sure the message wasn't sent by another bot, that channel isn't ignored, the logged action is turned on, and that the bot isn't paused
			return VerifyUnpaused(VerifyLoggingAction(VerifyLoggingIsEnabledOnThisChannel(VerifyMessage(message)), logAction));
		}

		//Verify the guild and log channels with a user
		public static IGuild VerifyGuild(IUser user, LogActions logAction)
		{
			return VerifyUnpaused(VerifyLoggingAction(GetGuildFromUser(user), logAction));
		}

		//Verify the guild and log channels with a guild
		public static IGuild VerifyGuild(IGuild guild, LogActions logAction)
		{
			return VerifyUnpaused(VerifyLoggingAction(guild, logAction));
		}

		//Verify the guild and log channels with a channel
		public static IGuild VerifyGuild(IGuildChannel channel, LogActions logAction)
		{
			return VerifyUnpaused(VerifyLoggingAction(GetGuildFromChannel(channel), logAction));
		}

		//Verify the guild and log channels with a role
		public static IGuild VerifyGuild(IRole role, LogActions logAction)
		{
			return VerifyUnpaused(VerifyLoggingAction(GetGuildFromRole(role), logAction));
		}

		//Make sure the bot's not paused
		public static IGuild VerifyUnpaused(IGuild guild)
		{
			return Variables.Pause ? null : guild;
		}
		#endregion

		#region Preferences
		//Save preferences
		public static void SavePreferences(TextWriter writer, ulong guildID, string input = null)
		{
			//Check if any preferences exist
			if (!Variables.Guilds.ContainsKey(guildID))
				return;

			//Write the input
			if (input != null)
			{
				writer.WriteLine(input);
				return;
			}

			//Variable for each category
			Variables.Guilds[guildID].CommandSettings.OrderBy(x => x.CategoryValue).ToList().ForEach(cmd => writer.WriteLine(cmd.Name + ":" + cmd.ValAsString));
		}
		
		//Save preferences by server
		public static void SavePreferences(ulong serverID, string input = null)
		{
			var path = GetServerFilePath(serverID, Constants.PREFERENCES_FILE);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			using (var writer = new StreamWriter(path, false))
			{
				SavePreferences(writer, serverID, input);
			}
		}
		
		//Enable preferences
		public static async Task EnablePreferences(IGuild guild, IUserMessage message)
		{
			//Set up the preferences file(s) location(s) on the computer
			var path = GetServerFilePath(guild.Id, Constants.PREFERENCES_FILE);
			if (path == null)
			{
				await MakeAndDeleteSecondaryMessage(message.Channel, message, ERROR(Constants.PATH_ERROR));
				return;
			}
			if (!File.Exists(path))
			{
				SavePreferences(guild.Id);
			}
			else
			{
				await MakeAndDeleteSecondaryMessage(message.Channel, message, "Preferences are already turned on.");
				Variables.GuildsEnablingPreferences.Remove(guild);
				return;
			}
			//Create bot channel if not on the server
			var channel = await GetLogChannel(guild, Constants.SERVER_LOG_CHECK_STRING);
			if (channel == null)
			{
				channel = await guild.CreateTextChannelAsync(Variables.Bot_Channel);
				await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(readMessages: PermValue.Deny));
				await SetServerOrModLog(guild, message.Channel, message, channel, Constants.SERVER_LOG_CHECK_STRING);
				await SetServerOrModLog(guild, message.Channel, message, channel, Constants.MOD_LOG_CHECK_STRING);
			}
			else
			{
				channel = (await guild.GetTextChannelsAsync()).FirstOrDefault(x => x.Name == Variables.Bot_Channel);
			}

			//Remove them from the emable list
			Variables.GuildsEnablingPreferences.Remove(guild);

			//Set the default prefs bool to false
			Variables.Guilds[guild.Id].TurnDefaultPrefsOff();

			//Send a success message
			await SendChannelMessage(message.Channel, "Successfully created the preferences for this guild.");
		}
		
		//Read out the preferences
		public static async Task ReadPreferences(IMessageChannel channel, string serverpath)
		{
			//Make the embed
			var embed = MakeNewEmbed("Preferences");

			//Make the information into separate fields
			var text = File.ReadAllText(serverpath).Replace("@", "").Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

			//Get the category name and the commands in it
			text.ForEach(category =>
			{
				var titleAndCommands = category.Split(new char[] { '\r' }, 2);
				var title = titleAndCommands[0];
				var commands = titleAndCommands[1].TrimStart('\n');

				//Add the field
				if (!String.IsNullOrEmpty(title) && !String.IsNullOrEmpty(commands))
				{
					AddField(embed, title, commands, false);
				}
			});

			//Send the preferences message
			await SendEmbedMessage(channel, embed);
		}
		
		//Delete preferences
		public static async Task DeletePreferences(IGuild guild, IUserMessage message)
		{
			//Check if valid path
			var path = GetServerFilePath(guild.Id, Constants.PREFERENCES_FILE);
			if (path == null)
			{
				await MakeAndDeleteSecondaryMessage(message.Channel, message, ERROR(Constants.PATH_ERROR));
				return;
			}

			//Delete the preferences file
			File.Delete(path);

			//Remove them from the emable list
			Variables.GuildsDeletingPreferences.Remove(guild);

			//Send a success message
			await SendChannelMessage(message.Channel, "Successfully deleted the stored preferences for this guild.");
		}

		//Check if a command is enabled
		public static bool CheckCommandEnabled(ICommandContext context, int argPos)
		{
			if (context.Guild == null)
				return false;

			//Get the command
			var cmd = GetCommand(context.Guild.Id, context.Message.Content.Substring(argPos).Split(' ').FirstOrDefault());
			//Check if the command is on or off
			if (cmd != null && !cmd.ValAsBoolean)
			{
				return false;
			}
			//Get the commands that are disabled on specific channels
			if (Variables.Guilds[context.Guild.Id].CommandsDisabledOnChannel.Any(x => CaseInsEquals(cmd.Name, x.CommandName) && context.Channel.Id == x.ChannelID))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		//Save the log actions
		public static void SaveLogActions(CommandContext context, List<LogActions> logActions)
		{
			//Create the file if it doesn't exist
			var path = GetServerFilePath(context.Guild.Id, Constants.MISCGUILDINFO);
			CreateFile(path);

			//Find the lines that aren't the current log action line
			var validLines = GetValidLines(path, Constants.LOG_ACTIONS);

			//Add all the lines back
			using (var writer = new StreamWriter(path))
			{
				var output = String.Join("\n", new string[] { String.Format("{0}:{1}", Constants.LOG_ACTIONS, String.Join("/", logActions.Select(x => (int)x))), String.Join("\n", validLines) } );
				writer.WriteLine(output);
			}

			Variables.Guilds[context.Guild.Id].LogActions = logActions.OrderBy(x => (int)x).ToList();
		}

		//Create file
		public static void CreateFile(string path)
		{
			if (!File.Exists(path))
			{
				File.Create(path).Close();
			}
		}

		//Add back in the lines
		public static void SaveLines(string path, string target, string input, List<string> validLines, bool literal = false)
		{
			if (target != null && input != null)
			{
				//Add all the lines back
				using (var writer = new StreamWriter(path))
				{
					writer.WriteLine(target + ":" + input + "\n" + String.Join("\n", validLines));
				}
			}
			else
			{
				if (literal)
				{
					SaveLines(path, validLines.Select(x => ToLiteral(x)).ToList());
				}
				else
				{
					SaveLines(path, validLines);
				}
			}
		}

		public static void SaveLines(string path, List<string> validLines)
		{
			using (var writer = new StreamWriter(path))
			{
				writer.WriteLine(String.Join("\n", validLines));
			}
		}

		//Save literally
		public static string ToLiteral(string input)
		{
			using (var writer = new StringWriter())
			{
				using (var provider = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("CSharp"))
				{
					provider.GenerateCodeFromExpression(new System.CodeDom.CodePrimitiveExpression(input), writer, null);
					return Constants.FORMATREGEX.Replace(writer.ToString(), "");
				}
			}
		}

		//Save multiple lines back in at a time
		public static void SaveLines(string path, string target, List<string> inputLines, List<string> validLines)
		{
			if (target != null)
			{
				//Add all the lines back
				using (var writer = new StreamWriter(path))
				{
					var output = String.Join("\n", new string[] { String.Join("\n", inputLines.Select(x => String.Format("{0}:{1}", target, x))), String.Join("\n", validLines) });
					writer.WriteLine(output);
				}
			}
		}
		#endregion

		#region Slowmode
		//Slowmode
		public static async Task Slowmode(IMessage message)
		{
			//Get the guild
			var guild = GetGuildFromMessage(message);

			//Make a new SlowmodeUser
			var smUser = new SlowmodeUser();

			//Get SlowmodeUser from the guild ID
			if (Variables.SlowmodeGuilds.ContainsKey(guild.Id))
			{
				smUser = Variables.SlowmodeGuilds[guild.Id].FirstOrDefault(x => x.User.Id == message.Author.Id);
			}
			//If that fails, try to get it from the channel ID
			else if (Variables.SlowmodeChannels.ContainsKey(message.Channel.Id))
			{
				//Find a channel slowmode where the channel ID is the same as the message channel ID then get the user
				smUser = Variables.SlowmodeChannels[message.Channel.Id].FirstOrDefault(x => x.User.Id == message.Author.Id);
			}

			//Once the user within the SlowmodeUser class isn't null then go through with slowmode
			if (smUser != null)
			{
				//Check if their messages allowed is above 0
				if (smUser.CurrentMessagesLeft > 0)
				{
					if (smUser.CurrentMessagesLeft == smUser.BaseMessages)
					{
						//Start the interval
						SlowmodeInterval(smUser);
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

		//Add a new user who joined into the slowmode users list
		public static async Task AddSlowmodeUser(IGuildUser user)
		{
			//Check if the guild has slowmode enabled
			if (Variables.SlowmodeGuilds.ContainsKey(user.Guild.Id))
			{
				//Get the variables out of a different user
				int messages = Variables.SlowmodeGuilds[user.Guild.Id].FirstOrDefault().BaseMessages;
				int time = Variables.SlowmodeGuilds[user.Guild.Id].FirstOrDefault().Time;

				//Add them to the list for the slowmode in this guild
				Variables.SlowmodeGuilds[user.Guild.Id].Add(new SlowmodeUser(user, messages, messages, time));
			}

			//Get a list of the IDs of the guild's channels
			var guildChannelIDList = (await user.Guild.GetTextChannelsAsync()).Select(x => x.Id);
			//Find if any of them are a slowmode channel
			var smChannels = Variables.SlowmodeChannels.Where(kvp => guildChannelIDList.Contains(kvp.Key)).ToList();
			//If greater than zero, add the user to each one
			if (smChannels.Any())
			{
				smChannels.ForEach(kvp =>
				{
					//Get the variables out of a different user
					int messages = kvp.Value.FirstOrDefault().BaseMessages;
					int time = kvp.Value.FirstOrDefault().Time;

					//Add them to the list for the slowmode in this guild
					kvp.Value.Add(new SlowmodeUser(user, messages, messages, time));
				});
			}
		}
		#endregion

		#region Banned Phrases
		//Banned phrases
		public static async Task BannedPhrases(IMessage message)
		{
			//Get the guild
			var guild = GetGuildFromMessage(message);
			if (guild == null)
				return;

			//Get the guild's bot data
			var guildLoaded = Variables.Guilds[guild.Id];

			//Check if it has any banned words or regex
			if (guildLoaded.BannedStrings.Any(x => CaseInsIndexOf(message.Content, x)) || guildLoaded.BannedRegex.Any(x => x.IsMatch(message.Content)))
			{
				await BannedPhrasesPunishments(message);
			}
		}

		//Banned phrase punishments on a user
		public static async Task BannedPhrasesPunishments(IMessage message)
		{
			//Get rid of the message
			await DeleteMessage(message);

			//Check if the guild has any punishments set up
			var guild = (message.Channel as IGuildChannel)?.Guild;
			if (guild == null || !Variables.Guilds.ContainsKey(guild.Id))
				return;

			//Get the user
			var user = message.Author as IGuildUser;

			//Check if the user is on the list already for saying a banned phrase
			var bpUser = Variables.BannedPhraseUserList.FirstOrDefault(x => x.User == user);
			if (bpUser != null)
			{
				bpUser.IncreaseAmountOfRemovedMessages();
			}
			else
			{
				//Add in the user and give 1 onto his messages removed count
				bpUser = new BannedPhraseUser(user);
				Variables.BannedPhraseUserList.Add(bpUser);
			}

			//Get the banned phrases punishments from the guild
			var punishment = Variables.Guilds[user.Guild.Id].BannedPhrasesPunishments.FirstOrDefault(x => x.NumberOfRemoves == bpUser.AmountOfRemovedMessages);
			if (punishment == null)
				return;

			//Kick
			if (punishment.Punishment == PunishmentType.Kick)
			{
				//Check if can kick them
				if (GetPosition(user.Guild, user) > GetPosition(user.Guild, await user.Guild.GetUserAsync(Variables.Bot_ID)))
					return;

				//Kick them
				await user.KickAsync();

				//Send a message to the logchannel
				var logChannel = await GetLogChannel(user.Guild, Constants.SERVER_LOG_CHECK_STRING);
				if (logChannel != null)
				{
					var embed = AddFooter(MakeNewEmbed(null, "**ID:** " + user.Id, Constants.LEAV), "Banned Phrases Leave");
					await SendEmbedMessage(logChannel, AddAuthor(embed, String.Format("{0} in #{1}", FormatUser(user), message.Channel), user.GetAvatarUrl()));
				}
			}
			//Ban
			else if (punishment.Punishment == PunishmentType.Ban)
			{
				//Check if can ban them
				if (GetPosition(user.Guild, user) > GetPosition(user.Guild, await user.Guild.GetUserAsync(Variables.Bot_ID)))
					return;

				//Ban them
				await user.Guild.AddBanAsync(message.Author);

				//Send a message to the logchannel
				var logChannel = await GetLogChannel(user.Guild, Constants.SERVER_LOG_CHECK_STRING);
				if (logChannel != null)
				{
					var embed = AddFooter(MakeNewEmbed(null, "**ID:** " + user.Id, Constants.BANN), "Banned Phrases Ban");
					await SendEmbedMessage(logChannel, AddAuthor(embed, FormatUser(user), user.GetAvatarUrl()));
				}
			}
			//Role
			else if (punishment.Punishment == PunishmentType.Role)
			{
				//Give them the role
				await GiveRole(user, punishment.Role);

				//If a time is specified, run through the time then remove the role
				if (punishment.PunishmentTime != null)
				{
					Variables.PunishedUsers.Add(new RemovablePunishment(guild, user, punishment.Role, DateTime.UtcNow.AddMinutes((int)punishment.PunishmentTime)));
				}

				//Send a message to the logchannel
				var logChannel = await GetLogChannel(user.Guild, Constants.SERVER_LOG_CHECK_STRING);
				if (logChannel != null)
				{
					var embed = AddFooter(MakeNewEmbed(null, "**Gained:** " + punishment.Role.Name, Constants.UEDT), "Banned Phrases Role");
					await SendEmbedMessage(logChannel, AddAuthor(embed, FormatUser(user), user.GetAvatarUrl()));
				}
			}
		}

		public static List<string> HandleBannedRegexModification(List<Regex> bannedRegex, List<string> inputPhrases, bool add)
		{
			if (add)
			{
				inputPhrases.ForEach(x => bannedRegex.Add(new Regex(x)));
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
					inputPhrases.ForEach(x => bannedRegex.Remove(bannedRegex.FirstOrDefault(y => y.ToString() == x)));
				}
				else
				{
					//Put them in descending order so as to not delete low values before high ones
					positions.OrderByDescending(x => x).ToList().ForEach(x => bannedRegex.RemoveAt(x));
				}
			}

			return bannedRegex.Select(x => x.ToString()).ToList();
		}

		public static List<string> HandleBannedStringModification(List<string> bannedStrings, List<string> inputPhrases, bool add)
		{
			if (add)
			{
				inputPhrases.ForEach(x => bannedStrings.Add(x));
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
					inputPhrases.ForEach(x => bannedStrings.Remove(bannedStrings.FirstOrDefault(y => y.ToString() == x)));
				}
				else
				{
					//Put them in descending order so as to not delete low values before high ones
					positions.OrderByDescending(x => x).ToList().ForEach(x => bannedStrings.RemoveAt(x));
				}
			}

			return bannedStrings.ToList();
		}
		#endregion

		#region Settings
		//Make sure the path is valid
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

		//Save the bot key
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

		//Set the game at start and whenever the prefix is changed
		public static async Task SetGame(string prefix = null)
		{
			//Get the game
			var game = Variables.Client.GetCurrentUser().Game.HasValue ? Variables.Client.GetCurrentUser().Game.Value.Name : Constants.DEFAULT_GAME;

			//Check if there's a game in the settings
			if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.Game))
			{
				game = Properties.Settings.Default.Game;
			}

			//Replace all instances of the prefix with the new current global prefix
			if (prefix != null)
			{
				game.Replace(prefix, Properties.Settings.Default.Prefix);
			}

			//Check if there's a stream to set
			if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.Stream))
			{
				await Variables.Client.SetGameAsync(game, Properties.Settings.Default.Stream, StreamType.Twitch);
			}
			else
			{
				await Variables.Client.SetGameAsync(game, Properties.Settings.Default.Stream, StreamType.NotStreaming);
			}
		}

		//Reset the settings
		public static void ResetSettings()
		{
			//Save the amount of shards that currently exist
			var shards = Properties.Settings.Default.ShardCount;
			//Reset the settings
			Properties.Settings.Default.Reset();
			//Add back in the shards
			Properties.Settings.Default.ShardCount = shards;
			//Save the settings
			Properties.Settings.Default.Save();
		}
		#endregion

		#region Close Words
		//Get the words close to a target word
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
				return null;

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
		#endregion

		#region Timers
		//Remove commands
		public static void RemoveCommandMessages(IMessageChannel channel, List<IMessage> messages, Int32 time)
		{
			Task.Run(async () =>
			{
				await Task.Delay(time);
				await DeleteMessages(channel, messages);
			});
		}

		//Remove active close word list
		public static void RemoveActiveCloseWords(ActiveCloseWords list)
		{
			Task.Run(async () =>
			{
				await Task.Delay(5000);
				Variables.ActiveCloseWords.Remove(list);
			});
		}

		//Remove active close help list
		public static void RemoveActiveCloseHelp(ActiveCloseHelp list)
		{
			Task.Run(async () =>
			{
				await Task.Delay(5000);
				Variables.ActiveCloseHelp.Remove(list);
			});
		}

		//Remove the option to say yes for preferences after ten seconds
		public static void RemovePrefEnable(IGuild guild)
		{
			Task.Run(async () =>
			{
				await Task.Delay(5000);
				Variables.GuildsEnablingPreferences.Remove(guild);
			});
		}

		//Remove the option to say yes for preferences after ten seconds
		public static void RemovePrefDelete(IGuild guild)
		{
			Task.Run(async () =>
			{
				await Task.Delay(5000);
				Variables.GuildsDeletingPreferences.Remove(guild);
			});
		}

		//Time interval for slowmode
		public static void SlowmodeInterval(SlowmodeUser smUser)
		{
			Task.Run(async () =>
			{
				//Sleep for the given amount of seconds
				await Task.Delay(Math.Abs(smUser.Time) * 1000);
				//Add back their ability to send messages
				smUser.ResetMessagesLeft();
			});
		}

		//Reset the mention spam prevention hourly
		public static void ResetSpamPrevention(object obj)
		{
			//Get the period
			const long PERIOD = 60 * 60 * 1000;

			//Reset the spam prevention user list
			Variables.Guilds.ToList().ForEach(x => x.Value.GlobalSpamPrevention.SpamPreventionUsers.Clear());

			//Determine how long to wait until firing
			var time = PERIOD;
			if ((DateTime.UtcNow.Subtract(Variables.StartupTime)).TotalHours < 1)
			{
				time -= (long)DateTime.UtcNow.TimeOfDay.TotalMilliseconds % PERIOD;
			}

			//Wait until the next firing
			Variables.SpamTimer = new Timer(ResetSpamPrevention, null, time, Timeout.Infinite);
		}

		//Reset the mention spam prevention hourly
		public static void RemovePunishments(object obj)
		{
			//Get the period
			const long PERIOD = 60 * 1000;

			//Go through and remove the punishment on each user
			var eligibleToLosePunishment = Variables.PunishedUsers.Where(x => x.Time <= DateTime.UtcNow).ToList();
			eligibleToLosePunishment.ForEach(async punishment =>
			{
				Variables.PunishedUsers.Remove(punishment);

				//Things that can be done with an IUser
				var user = punishment.User;
				if (punishment.Type == PunishmentType.Ban)
				{
					await punishment.Guild.RemoveBanAsync(user.Id);
					return;
				}

				//Things that need an IGuildUser
				var guildUser = await punishment.Guild.GetUserAsync(user.Id);
				switch (punishment.Type)
				{
					case PunishmentType.Role:
					{
						await guildUser.RemoveRolesAsync(punishment.Role);
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

			//Determine how long to wait until firing
			var time = PERIOD;
			if ((DateTime.UtcNow.Subtract(Variables.StartupTime)).TotalMinutes < 1)
			{
				time -= (long)DateTime.UtcNow.TimeOfDay.TotalMilliseconds % PERIOD;
			}

			//Wait until the next firing
			Variables.RemovePunishmentTimer = new Timer(RemovePunishments, null, time, Timeout.Infinite);
		}
		#endregion

		#region Spam Prevention
		//Going through with the spam prevention
		public static async Task<bool> HandleSpamPrevention(GlobalSpamPrevention global, BaseSpamPrevention spamPrev, IGuild guild, IMessage message)
		{
			if (spamPrev == null || !spamPrev.Enabled)
				return false;

			//Check if the bot can even kick/ban this user
			var author = message.Author as IGuildUser;
			if (GetPosition(guild, author) >= GetPosition(guild, await guild.GetUserAsync(Variables.Bot_ID)))
				return true;

			var votes = spamPrev.VotesNeededForKick;
			var spUser = Variables.Guilds[guild.Id].GlobalSpamPrevention.SpamPreventionUsers.FirstOrDefault(x => x.User == author) ?? new SpamPreventionUser(global, author, 0, votes);

			spUser.IncreaseCurrentSpamAmount();
			if (spUser.CurrentSpamAmount < spamPrev.AmountOfMessages)
				return true;

			//Send a message telling the users of the guild they can vote to ban this person
			await SendChannelMessage(message.Channel, String.Format("The user `{0}` needs `{1}` votes to be kicked. Vote to kick them by mentioning them.", FormatUser(author), votes));
			//Make sure they have the lowest vote count required to kick
			spUser.ChangeVotesRequired(votes);
			spUser.EnablePotentialKick();
			return true;
		}

		public static bool SpamCheck(MessageSpamPrevention spamPrev, IMessage message)
		{
			if (spamPrev == null)
				return false;

			return false; //TODO: message.MentionedUserIds.Distinct().Count() > spamPrev.AmountOfSpam;
		}

		public static bool SpamCheck(LongMessageSpamPrevention spamPrev, IMessage message)
		{
			if (spamPrev == null)
				return false;

			return message.Content.Length > spamPrev.AmountOfSpam;
		}

		public static bool SpamCheck(LinkSpamPrevention spamPrev, IMessage message)
		{
			if (spamPrev == null)
				return false;

			return false; //TODO: message.MentionedUserIds.Distinct().Count() > spamPrev.AmountOfSpam;
		}

		public static bool SpamCheck(ImageSpamPrevention spamPrev, IMessage message)
		{
			if (spamPrev == null)
				return false;

			return false; //TODO: message.MentionedUserIds.Distinct().Count() > spamPrev.AmountOfSpam;
		}

		public static bool SpamCheck(MentionSpamPrevention spamPrev, IMessage message)
		{
			if (spamPrev == null)
				return false;

			return message.MentionedUserIds.Distinct().Count() > spamPrev.AmountOfSpam;
		}
		#endregion

		#region Case Insensitive Searches
		public static bool CaseInsEquals(string str1, string str2)
		{
			return str1.Equals(str2, StringComparison.OrdinalIgnoreCase);
		}

		public static bool CaseInsIndexOf(string source, string search)
		{
			return source.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static bool CaseInsStartsWith(string source, string search)
		{
			return source.StartsWith(search, StringComparison.OrdinalIgnoreCase);
		}

		public static bool CaseInsEndsWith(string source, string search)
		{
			return source.EndsWith(search, StringComparison.OrdinalIgnoreCase);
		}

		public static bool CaseInsContains(List<string> list, string str)
		{
			return list.Contains(str, StringComparer.OrdinalIgnoreCase);
		}

		public static bool CaseInsContains(string[] array, string str)
		{
			return array.Contains(str, StringComparer.OrdinalIgnoreCase);
		}

		public static bool CaseInsContains(ReadOnlyCollection<string> readonlycollection, string str)
		{
			return readonlycollection.Contains(str, StringComparer.OrdinalIgnoreCase);
		}
		#endregion
	}

	public static class AsyncForEach
	{
		public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
		{
			foreach (var value in list)
			{
				await func(value);
			}
		}
	}
}