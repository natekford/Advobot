using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace Advobot
{
	public class Actions
	{
		#region Loads
		//Loading in all necessary information at bot start up
		public static void LoadInformation()
		{
			Variables.Bot_ID = Variables.Client.GetCurrentUser().Id;				//Give the variable Bot_ID the actual ID
			Variables.Bot_Name = Variables.Client.GetCurrentUser().Username;		//Give the variable Bot_Name the username of the bot
			Variables.Bot_Channel = Variables.Bot_Name.ToLower();					//Give the variable Bot_Channel a lowered version of the bot's name

			LoadPermissionNames();													//Gets the names of the permission bits in Discord
			LoadCommandInformation();												//Gets the information of a command (name, aliases, usage, summary). Has to go after LPN
			Variables.HelpList.ForEach(x => Variables.CommandNames.Add(x.Name));	//Gets all the active command names. Has to go after LCI

			LoadGuilds();															//Loads the guilds that attempted to load before the Bot_ID was gotten.

			Variables.Loaded = true;												//Set a bool stating that everything is done loading.
			ResetSpamPrevention(null);												//Start the hourly timer to restart spam prevention
			StartUpMessages();														//Say all of the start up messages
		}

		//Text said during the startup of the bot
		public static void StartUpMessages()
		{
			WriteLine("The current bot prefix is: " + Properties.Settings.Default.Prefix);
			WriteLine("Bot took " + String.Format("{0:n}", TimeSpan.FromTicks(DateTime.UtcNow.ToUniversalTime().Ticks - Variables.StartupTime.Ticks).TotalMilliseconds) + " milliseconds to load everything.");
		}

		//Load the information from the commands
		public static void LoadCommandInformation()
		{
			foreach (var classType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsSubclassOf(typeof(ModuleBase))))
			{
				foreach (var method in classType.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
				{
					//Get the name
					var name = "N/A";
					{
						CommandAttribute attr = (CommandAttribute)method.GetCustomAttribute(typeof(CommandAttribute));
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
						AliasAttribute attr = (AliasAttribute)method.GetCustomAttribute(typeof(AliasAttribute));
						if (attr != null)
						{
							aliases = attr.Aliases;
						}
					}
					//Get the usage
					var usage = "N/A";
					{
						UsageAttribute attr = (UsageAttribute)method.GetCustomAttribute(typeof(UsageAttribute));
						if (attr != null)
						{
							usage = attr.Text;
						}
					}
					//Get the base permissions
					var basePerm = "N/A";
					{
						PermissionRequirementAttribute attr = (PermissionRequirementAttribute)method.GetCustomAttribute(typeof(PermissionRequirementAttribute));
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
						SummaryAttribute attr = (SummaryAttribute)method.GetCustomAttribute(typeof(SummaryAttribute));
						if (attr != null)
						{
							text = attr.Text;
						}
					}
					//Add it to the helplist
					Variables.HelpList.Add(new HelpEntry(name, aliases, usage, basePerm, text));
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
		public static void LoadGuilds()
		{
			Variables.GuildsToBeLoaded.ForEach(async x => await LoadGuild(x));
		}

		//Load a guild's info
		public static async Task LoadGuild(IGuild guild)
		{
			//I know I am using .txt docs instead of .json; fite me.
			LoadCommandPreferences(guild);
			LoadBannedPhrasesAndPunishments(guild);
			LoadSelfAssignableRoles(guild);
			LoadGuildMiscInfo(guild);
			await LoadBotUsers(guild);
			LoadReminds(guild);
		}

		//Load preferences
		public static void LoadCommandPreferences(IGuild guild)
		{
			Variables.Guilds[guild.Id].CommandSettings = new List<CommandSwitch>();

			//Check if this server has any preferences
			var path = GetServerFilePath(guild.Id, Constants.PREFERENCES_FILE);
			if (!File.Exists(path))
			{
				//If not, go to the defaults
				var defaultPreferences = Properties.Resources.DefaultCommandPreferences;
				//Get the command category
				CommandCategory commandCategory = CommandCategory.Miscellaneous;
				//Split by new lines
				defaultPreferences.Split('\n').ToList().ForEach(x =>
				{
					//If the line is empty, do nothing
					if (String.IsNullOrWhiteSpace(x))
						return;
					//If the line starts with an @ then it's a category
					else if (x.StartsWith("@"))
					{
						if (!Enum.TryParse(x.Substring(1), out commandCategory))
							return;
					}
					//Anything else and it's a setting
					else
					{
						//Split before and after the colon, before is the setting name, after is the value
						var values = x.Split(new char[] { ':' }, 2);
						if (values.Length == 2)
						{
							var aliases = Variables.HelpList.FirstOrDefault(cmd => cmd.Name.Equals(values[0], StringComparison.OrdinalIgnoreCase))?.Aliases;
							Variables.Guilds[guild.Id].CommandSettings.Add(new CommandSwitch(values[0], values[1], commandCategory, aliases));
						}
						else
						{
							WriteLine("ERROR: " + x);
						}
					}
				});
				WriteLoadDone(guild, MethodBase.GetCurrentMethod().Name, "Command Preferences");
				Variables.Guilds[guild.Id].DefaultPrefs = true;
			}
			else
			{
				using (StreamReader file = new StreamReader(path))
				{
					//Get the command category
					CommandCategory commandCategory = CommandCategory.Miscellaneous;
					//Read the preferences document for information
					string line;
					while ((line = file.ReadLine()) != null)
					{
						//If the line is empty, do nothing
						if (String.IsNullOrWhiteSpace(line))
							continue;
						//If the line starts with an @ then it's a category
						if (line.StartsWith("@"))
						{
							if (!Enum.TryParse(line.Substring(1), out commandCategory))
								continue;
						}
						//Anything else and it's a setting
						else
						{
							//Split before and after the colon, before is the setting name, after is the value
							var values = line.Split(new char[] { ':' }, 2);
							if (values.Length == 2)
							{
								var aliases = Variables.HelpList.FirstOrDefault(x => x.Name.Equals(values[0], StringComparison.OrdinalIgnoreCase))?.Aliases;
								Variables.Guilds[guild.Id].CommandSettings.Add(new CommandSwitch(values[0], values[1], commandCategory, aliases));
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
		}

		//Load banned words/regex/punishments
		public static void LoadBannedPhrasesAndPunishments(IGuild guild)
		{
			//Check if the file exists
			var path = GetServerFilePath(guild.Id, Constants.BANNED_PHRASES);
			if (!File.Exists(path))
				return;

			//Get the banned phrases and regex
			using (StreamReader file = new StreamReader(path))
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
							var phrases = line.Substring(index + 1);
							if (!String.IsNullOrWhiteSpace(phrases))
							{
								Variables.Guilds[guild.Id].BannedPhrases = phrases.Split('/').Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().ToList();
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
								Variables.Guilds[guild.Id].BannedRegex = regex.Split('/').Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().Select(x => new Regex(x)).ToList();
							}
						}
					}
					//Punishments
					else if (line.StartsWith(Constants.BANNED_PHRASES_PUNISHMENTS))
					{
						int index = line.IndexOf(':');
						if (index >= 0 && index < line.Length - 1)
						{
							line.Substring(index + 1).Split('/').Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().ToList().ForEach(x =>
							{
								//Split the information in the file
								var args = x.Split(' ');

								//All need to be ifs to check each value

								//Number of removes to activate
								int number = 0;
								if (!int.TryParse(args[0], out number))
									return;

								//The type of punishment
								int punishment = 0;
								if (!int.TryParse(args[1], out punishment))
									return;

								//The role ID if a role punishment type
								ulong roleID = 0;
								IRole role = null;
								if (punishment == 3 && !ulong.TryParse(args[2], out roleID))
									return;
								else if (roleID != 0)
									role = guild.GetRole(roleID);

								//The time if a time is input
								int time = 0;
								if (role != null && !int.TryParse(args[3], out time))
									return;

								Variables.Guilds[guild.Id].BannedPhrasesPunishments.Add(new BannedPhrasePunishment(number, (PunishmentType)punishment, role, time));
							});
						}
					}
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
			using (StreamReader file = new StreamReader(path))
			{
				string line;
				while ((line = file.ReadLine()) != null)
				{
					//If the line is empty, do nothing
					if (String.IsNullOrWhiteSpace(line))
						continue;

					var inputArray = line.Split(' ');

					//Test if valid role
					ulong ID = 0;
					if (!ulong.TryParse(inputArray[0], out ID))
						return;
					IRole role = guild.GetRole(ID);
					if (role == null)
						return;

					//Test if valid group
					int group = 0;
					if (!int.TryParse(inputArray[1], out group))
						return;

					//Check if it's already in any list
					if (Variables.SelfAssignableGroups.Where(x => x.GuildID == guild.Id).Any(x => x.Roles.Any(y => y.Role.Id == ID)))
						return;

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
						Variables.SelfAssignableGroups.FirstOrDefault(x => x.Group == group).Roles.Add(SARole);
					}
				}
			}

			WriteLoadDone(guild, MethodBase.GetCurrentMethod().Name, "Self Assignable Roles/Groups");
		}

		//Load the prefix and logActions
		public static void LoadGuildMiscInfo(IGuild guild)
		{
			//Check if the file exists
			var path = GetServerFilePath(guild.Id, Constants.MISCGUILDINFO);
			if (!File.Exists(path))
				return;

			//Find the prefix line
			using (StreamReader reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (String.IsNullOrWhiteSpace(line))
						continue;

					if (line.Contains(Constants.GUILD_PREFIX))
					{
						Variables.Guilds[guild.Id].Prefix = line.Substring(line.IndexOf(':') + 1);
					}
					else if (line.Contains(Constants.LOG_ACTIONS))
					{
						var logActions = new List<LogActions>();
						line.Substring(line.IndexOf(':') + 1).Split('/').ToList().ForEach(x =>
						{
							LogActions temp;
							if (Enum.TryParse(x, out temp))
							{
								logActions.Add(temp);
							}
						});
						Variables.Guilds[guild.Id].LogActions = logActions.Distinct().OrderBy(x => (int)x).ToList();
					}
					else if (line.Contains(Constants.IGNORED_LOG_CHANNELS))
					{
						var IDs = new List<ulong>();
						line.Substring(line.IndexOf(':') + 1).Split('/').ToList().ForEach(x =>
						{
							ulong temp;
							if (ulong.TryParse(x, out temp))
							{
								IDs.Add(temp);
							}
						});
						Variables.Guilds[guild.Id].IgnoredLogChannels = IDs.Distinct().ToList();
					}
					else if (line.Contains(Constants.SPAM_PREVENTION))
					{
						var variableNumbers = line.Substring(line.IndexOf(':') + 1).Split('/').ToList();
						if (variableNumbers.Count != 3)
							return;

						var messagesRequired = 0;
						if (!int.TryParse(variableNumbers[0], out messagesRequired))
							return;

						var mentionsRequired = 0;
						if (!int.TryParse(variableNumbers[1], out mentionsRequired))
							return;

						var votesRequired = 0;
						if (!int.TryParse(variableNumbers[2], out votesRequired))
							return;

						Variables.Guilds[guild.Id].SpamPrevention = new SpamPreventionInformation(messagesRequired, mentionsRequired, votesRequired);
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
			var validBotUsers = new List<string>();
			using (StreamReader reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (String.IsNullOrWhiteSpace(line))
						continue;

					//Split input
					var inputArray = line.Split(':');
					if (inputArray.Length != 2)
						continue;

					//Check if valid ID
					ulong ID;
					if (!ulong.TryParse(inputArray[0], out ID))
						continue;

					//Check if valid perms
					uint perms;
					if (!uint.TryParse(inputArray[1], out perms))
						continue;

					var user = await guild.GetUserAsync(ID);
					if (user == null)
						continue;

					//If valid user then add to botusers and keep the line
					validBotUsers.Add(line);
					Variables.BotUsers.Add(new BotImplementedPermissions(user, perms));
				}
			}

			//Remove all bot users who are not in the server anymore
			using (StreamWriter writer = new StreamWriter(path))
			{
				writer.WriteLine(String.Join("\n", validBotUsers));
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
			using (StreamReader reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (String.IsNullOrWhiteSpace(line))
						continue;

					var inputArray = line.Split(new char[] { '/' }, 2);
					if (inputArray.Length != 2)
						return;

					var name = inputArray[0].Substring(1);
					var text = inputArray[1].Substring(0, inputArray[1].Length - 1);

					Variables.Guilds[guild.Id].Reminds.Add(new Remind(name, text.Replace("\\\\", "\\").Replace("\\n", "\n").Replace("\\'", "\'").Replace("\\\"", "\"")));
				}
			}

			WriteLoadDone(guild, MethodBase.GetCurrentMethod().Name, "Reminds");
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
			roleName = roleName.Trim();

			if (roleName.StartsWith("<@"))
			{
				roleName = roleName.Trim(new char[] { '<', '@', '&', '>' });
				ulong roleID = 0;
				if (UInt64.TryParse(roleName, out roleID))
				{
					return context.Guild.GetRole(roleID);
				}
			}
			var roles = context.Guild.Roles.Where(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).ToList();
			if (roles.Count > 1)
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR("Multiple roles with the same name. Please specify by mentioning the role or changing their names."));
			}
			else if (roles.Count == 1)
			{
				return roles.First();
			}
			return null;
		}
		
		//Simple get a role on the guild
		public static IRole GetRole(IGuild guild, string roleName)
		{
			//Order them by position (puts everyone first) then reverse so it sorts from the top down
			return guild.Roles.ToList().OrderBy(x => x.Position).Reverse().FirstOrDefault(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
		}
		
		//Get top position of a user
		public static int GetPosition(IGuild guild, IGuildUser user)
		{
			if (user.Id == guild.OwnerId)
				return Constants.OWNER_POSITION;

			int position = 0;
			user.RoleIds.ToList().ForEach(x => position = Math.Max(position, guild.GetRole(x).Position));

			return position;
		}
		
		//Get a user
		public static async Task<IGuildUser> GetUser(IGuild guild, string userName)
		{
			return await guild.GetUserAsync(GetUlong(userName.Trim(new char[] { '<', '>', '@', '!' })));
		}
		
		//Get the input to a ulong
		public static ulong GetUlong(string inputString)
		{
			ulong number = 0;
			if (UInt64.TryParse(inputString, out number))
			{
				return number;
			}
			return 0;
		}
		
		//Get if the user/bot can edit the role
		public static async Task<IRole> GetRoleEditAbility(CommandContext context, string input = null, bool ignore_Errors = false, IRole role = null)
		{
			//Check if valid role
			var inputRole = role == null ? await GetRole(context, input) : role;
			if (inputRole == null)
			{
				if (!ignore_Errors)
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(Constants.ROLE_ERROR));
				}
				return null;
			}

			//Determine if the user can edit the role
			if (inputRole.Position >= GetPosition(context.Guild, context.User as IGuildUser))
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
		
		//Get if the user can edit the channel
		public static async Task<IGuildChannel> GetChannelEditAbility(IGuildChannel channel, IGuildUser user)
		{
			if (GetChannelType(channel) == Constants.TEXT_TYPE)
			{
				using (var channelUsers = channel.GetUsersAsync().GetEnumerator())
				{
					while (await channelUsers.MoveNext())
					{
						if (channelUsers.Current.Contains(user))
						{
							return channel;
						}
					}
				}
			}
			else
			{
				if (user == null)
				{
					return null;
				}
				else if (user.GetPermissions(channel).Connect)
				{
					return channel;
				}
			}
			return null;
		}
		
		//Get if the user can edit this channel
		public static async Task<IGuildChannel> GetChannelEditAbility(CommandContext context, string input, bool ignoreErrors = false)
		{
			IGuildChannel channel = await GetChannel(context, input);
			if (channel == null)
			{
				return null;
			}
			if (await GetChannelEditAbility(channel, await context.Guild.GetUserAsync(context.User.Id)) == null)
			{
				if (!ignoreErrors)
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("You do not have the ability to edit `{0}`.", channel.Name)));
				}
				return null;
			}
			return channel;
		}
		
		//Get a channel ID
		public static async Task<IMessageChannel> GetChannelID(IGuild guild, string channelName)
		{
			IMessageChannel channel = null;
			ulong channelID = 0;
			if (UInt64.TryParse(channelName.Trim(new char[] { '<', '>', '#' }), out channelID))
			{
				channel = (IMessageChannel)await guild.GetChannelAsync(channelID);
			}
			return channel;
		}
		
		//Get a channel
		public static async Task<IGuildChannel> GetChannel(CommandContext context, string input)
		{
			return await GetChannel(context.Guild, context.Channel, context.Message, input);
		}
		
		//Get a channel without context
		public static async Task<IGuildChannel> GetChannel(IGuild guild, IMessageChannel channel, IUserMessage message, string input)
		{
			if (input.Contains("<#"))
			{
				input = input.Substring(input.IndexOf("<#"));
			}
			if (input.Contains(' '))
			{
				input = input.Substring(0, input.IndexOf(' '));
			}
			var values = input.Split(new char[] { '/' }, 2);

			//Get input channel type
			var channelType = values.Length == 2 ? values[1] : null;
			if (channelType != null && !(Constants.TEXT_TYPE.Equals(channelType, StringComparison.OrdinalIgnoreCase) || Constants.VOICE_TYPE.Equals(channelType, StringComparison.OrdinalIgnoreCase)))
			{
				return null;
			}

			//If a channel mention
			ulong channelID = 0;
			if (UInt64.TryParse(values[0].Trim(new char[] { '<', '#', '>' }), out channelID))
			{
				return await guild.GetChannelAsync(channelID);
			}

			//If a name and type
			else if (channelType != null)
			{
				//Get the channels from the guild
				var gottenChannels = await guild.GetChannelsAsync();
				//See which match the name and type given
				var channels = gottenChannels.Where(x => x.Name.Equals(values[0], StringComparison.OrdinalIgnoreCase) && x.GetType().Name.IndexOf(channelType, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

				if (channels.Count == 0)
					await MakeAndDeleteSecondaryMessage(channel, message, ERROR(String.Format("`{0}` does not exist as a channel on this guild.", input.Substring(0, input.IndexOf('/')))));
				if (channels.Count == 1)
					return channels[0];
				if (channels.Count > 1)
					await MakeAndDeleteSecondaryMessage(channel, message, ERROR("More than one channel exists with the same name."));
			}

			return null;
		}
		
		//Get the log channel
		public static async Task<ITextChannel> GetLogChannel(IGuild guild)
		{
			//Get the channels from the guild
			var gottenChannels = await guild.GetTextChannelsAsync();
			//See which match the name and type given
			return gottenChannels.FirstOrDefault(x => x.Name.Equals(Variables.Bot_Channel, StringComparison.OrdinalIgnoreCase));
		}
		
		//Get integer
		public static int GetInteger(string inputString)
		{
			int number = 0;
			if (Int32.TryParse(inputString, out number))
			{
				return number;
			}
			return -1;
		}
		
		//Get bits
		public static async Task<uint> GetBit(CommandContext context, string permission, uint changeValue)
		{
			try
			{
				int bit = Variables.GuildPermissions.FirstOrDefault(x => x.Name.Equals(permission, StringComparison.OrdinalIgnoreCase)).Position;
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
			Dictionary<String, String> channelPerms = new Dictionary<String, String>();

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
			if (!Variables.Guilds.ContainsKey(guild.Id))
				return null;

			var wut2 = Variables.Guilds[guild.Id].DefaultPrefs;
			var wut = Variables.Guilds[guild.Id].CommandSettings;
			return wut.Where(x => x.CategoryValue == number).Select(x => x.Name).ToArray();
		}
		
		//Get file paths
		public static string GetServerFilePath(ulong serverId, string fileName, bool global = false)
		{
			var folder = Properties.Settings.Default.Path;
			//If not a valid directory then give null
			if (!Directory.Exists(folder))
				return null;
			//Combine the path for the folders
			string directory;
			if (global)
			{
				directory = Path.Combine(folder, Constants.SERVER_FOLDER + "_" + Variables.Bot_ID);
			}
			else
			{
				directory = Path.Combine(folder, Constants.SERVER_FOLDER + "_" + Variables.Bot_ID, serverId.ToString());
			}
			//This string will be similar to C:\Users\User\AppData\Roaming\Discord_Servers_... if on using appdata. If not then it can be anything
			return Path.Combine(directory, fileName);
		}
		
		//Get if a channel is a text or voice channel
		public static string GetChannelType(IGuildChannel channel)
		{
			return channel.GetType().Name.IndexOf(Constants.TEXT_TYPE, StringComparison.OrdinalIgnoreCase) >= 0 ? Constants.TEXT_TYPE : Constants.VOICE_TYPE;
		}
		
		//Get if a bot channel already exists
		public static async Task<bool> GetDuplicateBotChan(IGuild guild)
		{
			//Get a list of text channels
			var tChans = await guild.GetTextChannelsAsync();
			//Return a bool stating if there's more than one or not
			return tChans.Where(x => x.Name == Variables.Bot_Channel).Count() > 1;
		}
		
		//Get what the serverlog is
		public static async Task<ITextChannel> GetLogChannel(IGuild guild, string serverOrMod, bool bypassBool = false)
		{
			var path = GetServerFilePath(guild.Id, Constants.MISCGUILDINFO);
			//Check if the file exists
			if (!File.Exists(path) || bypassBool)
			{
				//Default to the bot channel if it doesn't exist
				var logChannel = GetLogChannel(guild) as ITextChannel;
				if (logChannel != null && !await PermissionCheck(logChannel))
					return logChannel;
			}
			else
			{
				//Read the text document and find the serverlog 
				using (StreamReader reader = new StreamReader(path))
				{
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						if (line.Contains(serverOrMod))
						{
							var logChannelArray = line.Split(new Char[] { ':' }, 2);

							if (logChannelArray.Length < 2)
								return await GetLogChannel(guild, serverOrMod, true);

							var logChannel = (await guild.GetChannelAsync(Convert.ToUInt64(logChannelArray[1]))) as ITextChannel;
							if (logChannel == null || !await PermissionCheck(logChannel))
								return await GetLogChannel(guild, serverOrMod, true);
							return logChannel;
						}
					}
				}
			}
			return null;
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

		//Get the variables for slowmode
		public static string GetVariable(string[] inputArray, string searchTerm)
		{
			if (inputArray != null && inputArray.Any(x => x.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase)))
			{
				var first = inputArray.Where(x => x.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
				return first.Substring(first.IndexOf(':') + 1);
			}
			return null;
		}

		//Get the variable out of a string
		public static string GetVariable(string inputString, string searchTerm)
		{
			if (inputString != null && inputString.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase))
			{
				return inputString.Substring(inputString.IndexOf(':') + 1);
			}
			return null;
		}

		//Get the OS
		public static void GetOS()
		{
			var windir = Environment.GetEnvironmentVariable("windir");
			if (!string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir))
			{
				Variables.Windows = true;
			}
			else
			{
				Variables.Windows = false;
			}
		}

		//Get if it's a console or WPF
		public static void GetConsoleOrGUI()
		{
			try
			{
				int window_height = Console.WindowHeight;
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
			int groupNumber;
			if (!int.TryParse(input, out groupNumber))
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
				if (x.Name.Equals(input, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				else if (x.Aliases != null && x.Aliases.Contains(input, StringComparer.OrdinalIgnoreCase))
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
		public static async Task<bool> GetIfCommandIsValid(CommandContext context)
		{
			//Check to make sure everything is loaded
			if (!Variables.Loaded)
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR("Please wait until everything is loaded."));
				return false;
			}
			//Check if a command is disabled
			else if (!CheckCommandEnabled(context))
				return false;
			//Check if the bot still has admin
			else if (!(await context.Guild.GetCurrentUserAsync()).GuildPermissions.Administrator)
			{
				//If the server has been told already, ignore future commands fully
				if (Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts.Contains(context.Guild))
					return false;

				//Tell the guild that the bot needs admin (because I cba to code in checks if the bot has the permissions required for a lot of things)
				await SendChannelMessage(context, "This bot will not function without the `Administrator` permission, sorry.");

				//Add the guild to the list
				Variables.GuildsThatHaveBeenToldTheBotDoesNotWorkWithoutAdministratorAndWillBeIgnoredThuslyUntilTheyGiveTheBotAdministratorOrTheBotRestarts.Add(context.Guild);
				return false;
			}
			else
			{
				return true;
			}
		}

		//Get a list of lines in a text doc that aren't the targetted ones
		public static List<string> GetValidLines(string path, string checkString, bool getCheckString = false)
		{
			CreateFile(path);

			var validLines = new List<string>();
			using (StreamReader reader = new StreamReader(path))
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
					else if (!(line.IndexOf(checkString, StringComparison.OrdinalIgnoreCase) >= 0))
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
				help.basePerm,
				help.Text);
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
			RemoveCommandMessages(context.Channel, new IUserMessage[] { secondMsg, context.Message }, time);
		}
		
		//Remove secondary messages without context
		public static async Task MakeAndDeleteSecondaryMessage(IMessageChannel channel, IUserMessage message, string secondStr, Int32 time = Constants.WAIT_TIME)
		{
			var secondMsg = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + secondStr);
			RemoveCommandMessages(channel, new IUserMessage[] { secondMsg, message }, time);
		}
		
		//Remove messages
		public static async Task RemoveMessages(IMessageChannel channel, int requestCount)
		{
			//To remove the command itself
			++requestCount;

			while (requestCount > 0)
			{
				using (var enumerator = channel.GetMessagesAsync(requestCount).GetEnumerator())
				{
					while (await enumerator.MoveNext())
					{
						var messages = enumerator.Current;

						//If no more messages, leave
						if (messages.Count == 0)
							return;

						try
						{
							await channel.DeleteMessagesAsync(messages);
						}
						catch
						{
						}

						requestCount -= messages.Count;
					}
				}
			}
		}
		
		//Remove messages given a user id
		public static async Task RemoveMessages(ITextChannel channel, int requestCount, IUser user)
		{
			//Make sure there's a user id
			if (user == null)
			{
				await RemoveMessages(channel, requestCount);
				return;
			}

			WriteLine(String.Format("Deleting {0} messages from {1} in channel {2} in guild {3}.", requestCount, user.Id, channel.Name, channel.GuildId));
			var allMessages = new List<IMessage>();
			using (var enumerator = channel.GetMessagesAsync(Constants.MESSAGES_TO_GATHER).GetEnumerator())
			{
				while (await enumerator.MoveNext())
				{
					var messages = enumerator.Current;
					if (messages.Count == 0)
						continue;
					allMessages.AddRange(messages);
				}
			}

			//Get valid amount of messages to delete
			var userMessages = allMessages.Where(x => user == x.Author).ToList();
			if (requestCount > userMessages.Count)
			{
				requestCount = userMessages.Count;
			}
			else if (requestCount < userMessages.Count)
			{
				userMessages.RemoveRange(requestCount, userMessages.Count - requestCount);
			}
			userMessages.Insert(0, allMessages[0]); //Remove the initial command message

			WriteLine(String.Format("Found {0} messages; deleting {1} from user {2}", allMessages.Count, userMessages.Count - 1, user.Username));
			await channel.DeleteMessagesAsync(userMessages.ToArray());
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
			await SendChannelMessage(logChannel, String.Format("{0} **EDIT:** `{1}` **IN** `#{2}`\n**FROM:** ```\n{3}```\n**TO:** ```\n{4}```",
				time, Actions.FormatUser(user), channel.Name, before, after));
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
			return String.Format("{0}#{1} ({2})", user.Username, user.Discriminator, user.Id);
		}
		#endregion

		#region Uploads
		//Upload various text to a text uploader with a list of messages
		public static string UploadToHastebin(List<string> textList)
		{
			//Messages in the format to upload
			var text = ReplaceMarkdownChars(String.Join("\n-----\n", textList));
			return UploadToHastebin(text);
		}
		
		//Upload various text to a text uploader with a string
		public static string UploadToHastebin(string text)
		{
			//Regex for Getting the key out
			Regex hasteKeyRegex = new Regex(@"{""key"":""(?<key>[a-z].*)""}", RegexOptions.Compiled);

			//Upload the messages
			using (var client = new WebClient())
			{
				var response = client.UploadString("https://hastebin.com/documents", text);
				var match = hasteKeyRegex.Match(response);

				//Send the url back
				return String.Concat("https://hastebin.com/raw/", match.Groups["key"]);
			}
		}
		
		//Upload a text file with a list of messages
		public static async Task UploadTextFile(IGuild guild, IMessageChannel channel, List<string> textList, string fileName, string messageHeader)
		{
			//Messages in the format to upload
			var text = ReplaceMarkdownChars(String.Join("\n-----\n", textList));
			await UploadTextFile(guild, channel, text, fileName, messageHeader);
		}
		
		//Upload a text file with a string
		public static async Task UploadTextFile(IGuild guild, IMessageChannel channel, string text, string fileName, string messageHeader)
		{
			//Get the file path
			var deletedMessagesFile = fileName + DateTime.UtcNow.ToString("MM-dd_HH-mm-ss") + ".txt";
			var path = GetServerFilePath(guild.Id, deletedMessagesFile);
			if (path == null)
				return;

			//Create the temporary file
			if (!File.Exists(GetServerFilePath(guild.Id, deletedMessagesFile)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
			}

			//Write to the temporary file
			using (StreamWriter writer = new StreamWriter(path, true))
			{
				writer.WriteLine(text);
			}

			//Upload the file
			await channel.SendFileAsync(path, "**" + messageHeader + ":**");

			//Delete the file
			File.Delete(path);
		}

		//Upload a guild icon or bot icon
		public static async Task SetPicture(CommandContext context, string input, bool user)
		{
			//See if the user wants to remove the icon
			if (input != null && input.Equals("remove", StringComparison.OrdinalIgnoreCase))
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
			string imageURL = null;
			if (context.Message.Embeds.Count == 1)
			{
				imageURL = context.Message.Embeds.First().Thumbnail.ToString();
			}
			else
			{
				imageURL = context.Message.Attachments.First().Url;
			}

			//Run separate due to the time it takes
			var downloadUploadAndDelete = Task.Run(async () =>
			{
				//Check the image's file size first
				WebRequest req = HttpWebRequest.Create(imageURL);
				req.Method = "HEAD";
				using (WebResponse resp = req.GetResponse())
				{
					int ContentLength = 0;
					if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
					{
						//Check if valid content type
						if (!Constants.VALIDIMAGEEXTENSIONS.Contains("." + resp.Headers.Get("Content-Type").Split('/').Last()))
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
				context.Channel.EnterTypingState();

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
					using (FileStream imgStream = new FileStream(path, FileMode.Open, FileAccess.Read))
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
				using (FileStream imgStream = new FileStream(path, FileMode.Open, FileAccess.Read))
				{
					//Change the guild's icon to the downloaded image
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
				await msg.DeleteAsync();
				await SendChannelMessage(context, String.Format("Successfully changed the {0} icon.", user ? "bot" : "guild"));
			});
		}

		//Validate URL
		public static bool ValidateURL(string input)
		{
			Uri uriResult;
			var wut = Uri.TryCreate(input, UriKind.Absolute, out uriResult);
			return wut && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
		}
		#endregion

		#region Embeds
		//Send a message with an embedded object
		public static async Task<IMessage> SendEmbedMessage(IMessageChannel channel, string message, EmbedBuilder embed)
		{
			if (channel == null || !Variables.Guilds.ContainsKey(((channel as ITextChannel).Guild.Id)))
				return null;

			return await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + message, embed: embed);
		}
		
		//Send an embedded object
		public static async Task<IMessage> SendEmbedMessage(IMessageChannel channel, EmbedBuilder embed)
		{
			var guild = (channel as ITextChannel)?.Guild;
			if (channel == null || guild == null || !Variables.Guilds.ContainsKey(guild.Id))
				return null;

			//Replace all instances of the base prefix with the guild's prefix
			var guildPrefix = Variables.Guilds[guild.Id].Prefix;
			if (!String.IsNullOrWhiteSpace(guildPrefix))
			{
				embed.Description.Replace(Properties.Settings.Default.Prefix, guildPrefix);
			}

			var remadeEmbed = MakeNewEmbed();
			if (embed.Build().Fields.Any())
			{
				//Find out what the fields looks like
				var fields = String.Join("\n", embed.Build().Fields.Select(x => x.Name + "\n" + x.Value));

				if (fields.Length > Constants.LENGTH_CHECK)
				{
					remadeEmbed.Url = UploadToHastebin(ReplaceMarkdownChars(fields));
					remadeEmbed.Description = "Content is past 2,000 characters. Click the link to see it.";
				}
				else if (fields.Count(x => x == '\n' || x == '\r') >= 20 && embed.Build().Fields.Any(x => !x.Inline))
				{
					//Mobile can only show up to 20 or so lines per embed I think (at least on Android)
					remadeEmbed.Url = UploadToHastebin(ReplaceMarkdownChars(fields));
					remadeEmbed.Description = "Content is longer than 20 lines and has many fields. Click the link to see it.";
				}
				else
				{
					remadeEmbed.Url = embed.Url;
					remadeEmbed.Description = embed.Description;
					embed.Build().Fields.ToList().ForEach(x => AddField(remadeEmbed, x.Name, x.Value, x.Inline));
				}

				//Add everything else back to the new embed
				remadeEmbed.Author = embed.Author;
				remadeEmbed.Color = embed.Color;
				remadeEmbed.Footer = embed.Footer;
				remadeEmbed.ImageUrl = embed.ImageUrl;
				remadeEmbed.ThumbnailUrl = embed.ThumbnailUrl;
				remadeEmbed.Timestamp = embed.Timestamp;
				remadeEmbed.Title = embed.Title;
			}
			else
			{
				remadeEmbed = embed;
				if (!String.IsNullOrWhiteSpace(remadeEmbed.Description) && remadeEmbed.Description.Count(x => x == '\n' || x == '\r') >= 20)
				{
					remadeEmbed.Url = UploadToHastebin(ReplaceMarkdownChars(remadeEmbed.Description));
					remadeEmbed.Description = "Content is longer than 20 lines. Click the link to see it.";
				}
			}

			try
			{
				return await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR, embed: remadeEmbed);
			}
			//Embeds fail every now and then and I haven't been able to find the exact problem yet (I know fields are a problem, but not in this case)
			catch (Exception e)
			{
				ExceptionToConsole(MethodBase.GetCurrentMethod().Name, e);
				return null;
			}
		}
		
		//Make a new embed builder
		public static EmbedBuilder MakeNewEmbed(string title = null, string description = null, Color? color = null, string imageURL = null)
		{
			var embed = new EmbedBuilder().WithColor(Constants.BASE).WithCurrentTimestamp();
			
			if (title != null)
			{
				embed.Title = title;
			}
			if (description != null)
			{
				if (description.Length > Constants.LENGTH_CHECK)
				{
					embed.Url = UploadToHastebin(ReplaceMarkdownChars(description));
					embed.Description = "Content is past 2,000 characters. Click the link to see it.";
				}
				else
				{
					embed.Description = description;
					//Mobile can only show up to 20 or so lines per embed I think (at least on Android) 
					if (description.Count(x => x == '\n' || x == '\r') >= 20)
					{
						embed.Url = UploadToHastebin(ReplaceMarkdownChars(description));
					}
				}
			}
			if (color != null)
			{
				embed.Color = color.Value;
			}
			if (imageURL != null)
			{
				embed.ImageUrl = imageURL;
			}

			return embed;
		}
		
		//Make a new author for an embed
		public static EmbedBuilder AddAuthor(EmbedBuilder embed, string name = null, string iconURL = null, string URL = null)
		{
			EmbedAuthorBuilder author = new EmbedAuthorBuilder().WithIconUrl("https://discordapp.com/assets/322c936a8c8be1b803cd94861bdfa868.png");

			if (name != null)
			{
				author.Name = name;
			}
			if (iconURL != null)
			{
				author.IconUrl = iconURL;
			}
			if (URL != null)
			{
				author.Url = URL;
			}

			return embed.WithAuthor(author);
		}
		
		//Make a new footer for an embed
		public static EmbedBuilder AddFooter(EmbedBuilder embed, string text = null, string iconURL = null)
		{
			EmbedFooterBuilder footer = new EmbedFooterBuilder();

			if (text != null)
			{
				footer.Text = text;
			}
			if (iconURL != null)
			{
				footer.IconUrl = iconURL;
			}

			return embed.WithFooter(footer);
		}
		
		//Add a field to an embed
		public static EmbedBuilder AddField(EmbedBuilder embed, string name = "null", string value = "null", bool isInline = true)
		{
			if (name == null || value == null)
				return embed;

			embed.AddField(x =>
			{
				x.Name = name;
				x.Value = value;
				x.IsInline = isInline;
			});

			return embed;
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
			WriteLine(method + " EXCEPTION: " + e.ToString());
		}

		//Write when a load is done
		public static void WriteLoadDone(IGuild guild, string method, string name)
		{
			Variables.Guilds[guild.Id].DefaultPrefs = false;
			WriteLine(String.Format("{0}: {1} for the guild {2} have been loaded.", method, name, FormatGuild(guild)));
		}
		#endregion

		#region Server/Mod Log
		//Check if the bot can type in a logchannel
		public static async Task<bool> PermissionCheck(ITextChannel channel)
		{
			if (channel == null)
				return false;

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
			ITextChannel logChannel = null;
			//See if not null
			if (String.IsNullOrWhiteSpace(input))
			{
				await MakeAndDeleteSecondaryMessage(channel, message, ERROR("No channel specified."));
				return null;
			}
			else if (input.Equals("off", StringComparison.OrdinalIgnoreCase))
			{
				return await SetServerOrModLog(guild, channel, message, null as ITextChannel, serverOrMod);
			}

			//Get the channel with its ID
			logChannel = await GetChannel(guild, channel, message, input) as ITextChannel;
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

			//Find the lines that aren't the current serverlog line
			var validLines = new List<string>();
			using (StreamReader reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (line.Contains(serverOrMod))
					{
						if ((inputChannel != null) && (line.Contains(inputChannel.Id.ToString())))
						{
							await MakeAndDeleteSecondaryMessage(channel, message, "Channel is already the current " + serverOrMod + ".");
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
			using (StreamWriter writer = new StreamWriter(path))
			{
				if (inputChannel == null)
				{
					writer.WriteLine(serverOrMod + ":" + null + "\n" + String.Join("\n", validLines));
					await MakeAndDeleteSecondaryMessage(channel, message, "Disabled the " + serverOrMod + ".");
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
		public static async Task ImageLog(IMessageChannel channel, SocketMessage message, bool embeds)
		{
			if (!Variables.Guilds[(channel as ITextChannel).Guild.Id].LogActions.Any(x => "ImageLog".Equals(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase)))
				return;

			//Get the links
			var attachmentURLs = new List<string>();
			var embedURLs = new List<string>();
			var videoEmbeds = new List<Embed>();
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
							embedURLs.AddRange(message.Embeds.Select(y => y.Thumbnail.Value.Url).Distinct());
						}
						else if (x.Image.HasValue && !String.IsNullOrEmpty(x.Image.Value.Url))
						{
							embedURLs.AddRange(message.Embeds.Select(y => y.Image.Value.Url).Distinct());
						}
					}
					else
					{
						//Add the video URL and the thumbnail URL
						videoEmbeds.Add(x);
					}
				});
			}
			var user = message.Author;
			//Attached files
			foreach (string URL in attachmentURLs)
			{
				//Image attachment
				if (Constants.VALIDIMAGEEXTENSIONS.Contains(Path.GetExtension(URL), StringComparer.OrdinalIgnoreCase))
				{
					++Variables.LoggedImages;
					var embed = MakeNewEmbed(null, null, Constants.ATCH, URL);
					AddFooter(embed, "Attached Image");
					AddAuthor(embed, String.Format("{0} in #{1}", Actions.FormatUser(user), message.Channel), user.AvatarUrl, URL);
					await SendEmbedMessage(channel, embed);
				}
				//Gif attachment
				else if (Constants.VALIDGIFEXTENTIONS.Contains(Path.GetExtension(URL), StringComparer.OrdinalIgnoreCase))
				{
					++Variables.LoggedGifs;
					var embed = MakeNewEmbed(null, null, Constants.ATCH, URL);
					AddFooter(embed, "Attached Gif");
					AddAuthor(embed, String.Format("{0} in #{1}", Actions.FormatUser(user), message.Channel), user.AvatarUrl, URL);
					await SendEmbedMessage(channel, embed);
				}
				//Random file attachment
				else
				{
					++Variables.LoggedFiles;
					var embed = MakeNewEmbed(null, null, Constants.ATCH, URL);
					AddFooter(embed, "Attached File");
					AddAuthor(embed, String.Format("{0} in #{1}", Actions.FormatUser(user), message.Channel), user.AvatarUrl, URL);
					await SendEmbedMessage(channel, embed);
				}
			}
			//Embedded images
			foreach (string URL in embedURLs.Distinct())
			{
				++Variables.LoggedImages;
				var embed = MakeNewEmbed(null, null, Constants.ATCH, URL);
				AddFooter(embed, "Embedded Image");
				AddAuthor(embed, String.Format("{0} in #{1}", Actions.FormatUser(user), message.Channel), user.AvatarUrl, URL);
				await SendEmbedMessage(channel, embed);
			}
			//Embedded videos/gifs
			foreach (Embed embedObject in videoEmbeds.Distinct())
			{
				++Variables.LoggedGifs;
				var embed = MakeNewEmbed(null, null, Constants.ATCH, embedObject.Thumbnail.Value.Url);
				AddFooter(embed, "Embedded " + (Constants.VALIDGIFEXTENTIONS.Contains(Path.GetExtension(embedObject.Thumbnail.Value.Url), StringComparer.OrdinalIgnoreCase) ? "Gif" : "Video"));
				AddAuthor(embed, String.Format("{0} in #{1}", Actions.FormatUser(user), message.Channel), user.AvatarUrl, embedObject.Url);
				await SendEmbedMessage(channel, embed);
			}
		}

		//Check if the serverlog exists and if the bot can use it
		public static async Task<ITextChannel> VerifyLogChannel(IGuild guild, string checkString = Constants.SERVER_LOG_CHECK_STRING)
		{
			if (guild == null)
				return null;
			var logChannel = await GetLogChannel(guild, checkString);
			if (!await PermissionCheck(logChannel))
				return null;
			return logChannel;
		}

		//Check from channel
		public static async Task<ITextChannel> VerifyLogChannel(SocketChannel channel)
		{
			var tempChan = channel as IGuildChannel;
			if (tempChan == null || tempChan.Guild == null)
				return null;
			return await VerifyLogChannel(tempChan.Guild);
		}

		//Check from message
		public static async Task<ITextChannel> VerifyLogChannel(SocketMessage message)
		{
			var tempMsg = message.Channel as IGuildChannel;
			if (tempMsg == null || tempMsg.Guild == null)
				return null;
			return await VerifyLogChannel(tempMsg.Guild);
		}

		//Check from message
		public static async Task<ITextChannel> VerifyLogChannel(SocketUser user)
		{
			var tempUser = user as IGuildUser;
			if (tempUser == null || tempUser.Guild == null)
				return null;
			return await VerifyLogChannel(tempUser.Guild);
		}
		#endregion

		#region Preferences
		//Save preferences
		public static void SavePreferences(TextWriter writer, ulong guildID, string input = null)
		{
			//Check if any preferences exist
			if (!Variables.Guilds.ContainsKey(guildID))
				return;

			if (input != null)
			{
				writer.WriteLine(input);
				return;
			}

			//Variable for each category
			int categories = 0;
			foreach (var cmd in Variables.Guilds[guildID].CommandSettings.OrderBy(x => x.CategoryValue))
			{
				if (categories != cmd.CategoryValue)
				{
					if (categories != 0)
					{
						writer.WriteLine();
					}
					writer.WriteLine("@" + cmd.CategoryName);
					categories = cmd.CategoryValue;
				}
				writer.WriteLine(cmd.Name + ":" + cmd.valAsString);
			}
		}
		
		//Save preferences by server
		public static void SavePreferences(ulong serverID, string input = null)
		{
			var path = GetServerFilePath(serverID, Constants.PREFERENCES_FILE);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			using (StreamWriter writer = new StreamWriter(path, false))
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
				SavePreferences(guild.Id, Properties.Resources.DefaultCommandPreferences);
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
			Variables.Guilds[guild.Id].DefaultPrefs = false;

			//Send a success message
			await SendChannelMessage(message.Channel, "Successfully created the preferences for this guild.");
		}
		
		//Read out the preferences
		public static async Task ReadPreferences(IMessageChannel channel, string serverpath)
		{
			//Make the embed
			var embed = MakeNewEmbed("Preferences");

			//Make the information into separate fields
			var text = File.ReadAllText(serverpath).Replace("@", "").Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

			//Get the category name and the commands in it
			foreach (string category in text)
			{
				var titleAndCommands = category.Split(new char[] { '\r' }, 2);
				var title = titleAndCommands[0];
				var commands = titleAndCommands[1].TrimStart('\n');

				//Add the field
				if (!String.IsNullOrEmpty(title) && !String.IsNullOrEmpty(commands))
				{
					AddField(embed, title, commands, false);
				}
			}

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
		public static bool CheckCommandEnabled(ICommandContext context)
		{
			if (context.Guild == null)
				return false;

			//Get the command
			var cmd = GetCommand(context.Guild.Id, context.Message.Content.Substring(Properties.Settings.Default.Prefix.Length).Split(' ').FirstOrDefault());
			//Check if the command is on or off
			if (cmd != null && !cmd.valAsBoolean)
				return false;
			else
				return true;
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
			using (StreamWriter writer = new StreamWriter(path))
			{
				writer.WriteLine(Constants.LOG_ACTIONS + ":" + String.Join("/", logActions.Select(x => (int)x)) + "\n" + String.Join("\n", validLines));
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
				using (StreamWriter writer = new StreamWriter(path))
				{
					writer.WriteLine(target + ":" + input + "\n" + String.Join("\n", validLines));
				}
			}
			else
			{
				using (StreamWriter writer = new StreamWriter(path))
				{
					if (literal)
					{
						writer.WriteLine(String.Join("\n", validLines.Select(x => ToLiteral(x))));
					}
					else
					{
						writer.WriteLine(String.Join("\n", validLines));
					}
				}
			}
		}

		//Save literally
		public static string ToLiteral(string input)
		{
			using (StringWriter writer = new StringWriter())
			{
				using (var provider = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("CSharp"))
				{
					provider.GenerateCodeFromExpression(new System.CodeDom.CodePrimitiveExpression(input), writer, null);
					return Constants.FORMATREGEX.Replace(writer.ToString(), "");
				}
			}
		}
		#endregion

		#region Slowmode
		//Slowmode
		public static async Task Slowmode(SocketMessage message)
		{
			//Make a new SlowmodeUser
			var smUser = new SlowmodeUser();

			//Get SlowmodeUser from the guild ID
			if (Variables.SlowmodeGuilds.ContainsKey((message.Channel as IGuildChannel).GuildId))
			{
				smUser = Variables.SlowmodeGuilds[(message.Channel as IGuildChannel).GuildId].FirstOrDefault(x => x.User.Id == message.Author.Id);
			}
			//If that fails, try to get it from the channel ID
			else if (Variables.SlowmodeChannels.ContainsKey(message.Channel as IGuildChannel))
			{
				//Find a channel slowmode where the channel ID is the same as the message channel ID then get the user
				smUser = Variables.SlowmodeChannels[message.Channel as IGuildChannel].FirstOrDefault(x => x.User.Id == message.Author.Id);
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
					--smUser.CurrentMessagesLeft;
				}
				//Else delete the message
				else
				{
					await message.DeleteAsync();
				}
			}
		}

		//Add a new user who joined into the slowmode users list
		public static void AddSlowmodeUser(SocketGuildUser user)
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
			var guildChannelIDList = new List<ulong>();
			user.Guild.TextChannels.ToList().ForEach(x => guildChannelIDList.Add(x.Id));
			//Find if any of them are a slowmode channel
			var smChannels = Variables.SlowmodeChannels.Where(kvp => guildChannelIDList.Contains(kvp.Key.Id)).ToList();
			//If greater than zero, add the user to each one
			if (smChannels.Any())
			{
				foreach (var kvp in smChannels)
				{
					//Get the variables out of a different user
					int messages = kvp.Value.FirstOrDefault().BaseMessages;
					int time = kvp.Value.FirstOrDefault().Time;

					//Add them to the list for the slowmode in this guild
					kvp.Value.Add(new SlowmodeUser(user, messages, messages, time));
				}
			}
		}
		#endregion

		#region Banned Phrases
		//Banned phrases
		public static async Task BannedPhrases(SocketMessage message)
		{
			//Get the guild
			var guild = (message.Channel as IGuildChannel).Guild;
			if (guild == null)
				return;
			var guildLoaded = Variables.Guilds[guild.Id];

			//Check if it has any banned words
			if (guildLoaded.BannedPhrases.Any(x => message.Content.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0))
			{
				await BannedPhrasesPunishments(message);
			}
			//Check if it has any banned regex
			if (guildLoaded.BannedRegex.Any(x => x.IsMatch(message.Content)))
			{
				await BannedPhrasesPunishments(message);
			}
		}

		//Banned phrase punishments on a user
		public static async Task BannedPhrasesPunishments(SocketMessage message)
		{
			//Get rid of the message
			await message.DeleteAsync();

			//Check if the guild has any punishments set up
			if (!Variables.Guilds.ContainsKey((message.Channel as IGuildChannel).Guild.Id))
				return;

			//Get the user
			var user = message.Author as IGuildUser;

			//Check if the user is on the list already for saying a banned phrase
			BannedPhraseUser bpUser;
			if (Variables.BannedPhraseUserList.Any(x => x.User == user))
			{
				//Grab the user and add 1 onto his messages removed count
				bpUser = Variables.BannedPhraseUserList.FirstOrDefault(x => x.User == user);
				++bpUser.AmountOfRemovedMessages;
			}
			else
			{
				//Add in the user and give 1 onto his messages removed count
				bpUser = new BannedPhraseUser(user);
				Variables.BannedPhraseUserList.Add(bpUser);
			}

			//Get the banned phrases punishments from the guild
			var punishments = Variables.Guilds[user.Guild.Id].BannedPhrasesPunishments;

			//Check if any punishments have the messages count which the user has
			if (!punishments.Any(x => x.Number_Of_Removes == bpUser.AmountOfRemovedMessages))
				return;

			//Grab the punishment with the same number
			BannedPhrasePunishment punishment = punishments.FirstOrDefault(x => x.Number_Of_Removes == bpUser.AmountOfRemovedMessages);

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
					await SendEmbedMessage(logChannel, AddAuthor(embed, String.Format("{0} in #{1}", Actions.FormatUser(user), message.Channel), user.AvatarUrl));
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
					await SendEmbedMessage(logChannel, AddAuthor(embed, Actions.FormatUser(user), user.AvatarUrl));
				}
			}
			//Role
			else
			{
				//Give them the role
				await GiveRole(user, punishment.Role);

				//If a time is specified, run through the time then remove the role
				if (punishment.PunishmentTime != null)
				{
					BannedPhrasesPunishmentTimer(user, punishment.Role, (int)punishment.PunishmentTime);
				}

				//Send a message to the logchannel
				ITextChannel logChannel = await GetLogChannel(user.Guild, Constants.SERVER_LOG_CHECK_STRING);
				if (logChannel != null)
				{
					var embed = AddFooter(MakeNewEmbed(null, "**Gained:** " + punishment.Role.Name, Constants.UEDT), "Banned Phrases Role");
					await SendEmbedMessage(logChannel, AddAuthor(embed, Actions.FormatUser(user), user.AvatarUrl));
				}
			}
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

			if (Variables.Windows && path.Equals("appdata", StringComparison.OrdinalIgnoreCase))
			{
				path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			}

			if (!Directory.Exists(path))
			{
				WriteLine("Invalid directory. Please enter a valid directory:");
			}
			else
			{
				Properties.Settings.Default.Path = path;
				Properties.Settings.Default.Save();
				return true;
			}
			return false;
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
			var game = Variables.Client.GetCurrentUser().Game.HasValue ? Variables.Client.GetCurrentUser().Game.Value.Name : "type \"" + Properties.Settings.Default.Prefix + "help\" for help.";

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

		//Get the commands that have the given input in their name
		public static List<CloseHelp> GetCommandsWithInputInName(List<CloseHelp> list, string input)
		{
			//Find commands with the input in their name
			var commands = Variables.HelpList.Where(x => x.Name.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

			//Check if any were gotten
			if (!commands.Any())
				return null;

			var closeHelps = new List<CloseHelp>();
			commands.ForEach(x =>
			{
				if (closeHelps.Count < 5)
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
		public static void RemoveCommandMessages(IMessageChannel channel, IUserMessage[] messages, Int32 time)
		{
			Task t = Task.Run(async () =>
			{
				await Task.Delay(time);
				await channel.DeleteMessagesAsync(messages);
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
				smUser.CurrentMessagesLeft = smUser.BaseMessages;
			});
		}

		//Wait them remove the role on a user when they got it from a banned phrase punishment
		public static void BannedPhrasesPunishmentTimer(IGuildUser user, IRole role, int time)
		{
			Task.Run(async () =>
			{
				//Sleep for the given amount of seconds
				await Task.Delay(Math.Abs(time) * 60000);

				//Check if the user still has the role
				if (!user.RoleIds.Contains(role.Id))
					return;

				//Remove the role
				await user.RemoveRolesAsync(role);
			});
		}

		//Remove the role on a user after given amount of seconds
		public static void RemoveRoleAfterTime(IGuildUser user, IRole role, int time)
		{
			Task.Run(async () =>
			{
				//Sleep for the given amount of seconds
				await Task.Delay(Math.Abs(time) * 1000);
				//Add back their ability to send messages
				await user.RemoveRolesAsync(role);
			});
		}

		//Remove the mute on a user after given amount of seconds
		public static void UnmuteVoiceAfterTime(IGuildUser user, int time)
		{
			Task.Run(async () =>
			{
				//Sleep for the given amount of seconds
				await Task.Delay(Math.Abs(time) * 1000);
				//Add back their ability to send messages
				await user.ModifyAsync(x => x.Mute = false);
			});
		}

		//Remove the deafen on a user after given amount of seconds
		public static void UndeafenAfterTime(IGuildUser user, int time)
		{
			Task.Run(async () =>
			{
				//Sleep for the given amount of seconds
				await Task.Delay(Math.Abs(time) * 1000);
				//Add back their ability to send messages
				await user.ModifyAsync(x => x.Deaf = false);
			});
		}
		#endregion

		#region Spam Prevention
		//Reset the mention spam prevention hourly
		public static void ResetSpamPrevention(object obj)
		{
			//Get the period
			const long PERIOD = 60 * 60 * 1000;

			//Reset the spam prevention user list
			Variables.Guilds.ToList().ForEach(x => x.Value.SpamPreventionUsers = new List<SpamPreventionUser>());

			//Determine how long to wait until firing
			var time = PERIOD;
			if ((DateTime.UtcNow.Subtract(Variables.StartupTime)).TotalHours < 1)
			{
				time -= (long)DateTime.UtcNow.TimeOfDay.TotalMilliseconds % PERIOD;
			}

			//Wait until the next firing
			Variables.Timer = new Timer(ResetSpamPrevention, null, time, Timeout.Infinite);
		}
		#endregion
	}
}