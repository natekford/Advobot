using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Modules;
using Discord.WebSocket;
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Advobot
{
	public class Actions
	{
		//Loading in all necessary information at bot start up
		public static void loadInformation()
		{
			Variables.Bot = CommandHandler.client.CurrentUser;						//Give the variable Bot the bot as a SocketUser
			Variables.Bot_ID = CommandHandler.client.CurrentUser.Id;                //Give the variable Bot_ID the actual ID
			Variables.Bot_Name = CommandHandler.client.CurrentUser.Username;        //Give the variable Bot_Name the username of the bot
			Variables.Bot_Channel = Variables.Bot_Name.ToLower();					//Give the variable Bot_Channel a lowered version of the bot's name

			loadPermissionNames();													//Gets the names of the permission bits in Discord
			loadCommandInformation();												//Gets the information of a command (name, aliases, usage, summary). Has to go after LPN
			Variables.HelpList.ForEach(x => Variables.CommandNames.Add(x.Name));	//Gets all the active command names. Has to go after LCI
		}

		//Get the information from the commands
		public static void loadCommandInformation()
		{
			var classTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsSubclassOf(typeof(ModuleBase)));
			foreach (var classType in classTypes)
			{
				List<MethodInfo> methods = classType.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic).ToList();
				foreach (var method in methods)
				{
					String name = "N/A";
					String[] aliases = { "N/A" };
					String usage = "N/A";
					String basePerm = "N/A";
					String text = "N/A";
					//Actions.writeLine(classType.Name + "." + method.Name);
					{
						CommandAttribute attr = (CommandAttribute)method.GetCustomAttribute(typeof(CommandAttribute));
						if (attr != null)
						{
							//Actions.writeLine(classType.Name + "." + method.Name + ": " + attr.Text);
							name = attr.Text;
						}
						else
						{
							continue;
						}
					}
					{
						AliasAttribute attr = (AliasAttribute)method.GetCustomAttribute(typeof(AliasAttribute));
						if (attr != null)
						{
							//Actions.writeLine(classType.Name + "." + method.Name + ": " + attr.Text);
							aliases = attr.Aliases;
						}
					}
					{
						UsageAttribute attr = (UsageAttribute)method.GetCustomAttribute(typeof(UsageAttribute));
						if (attr != null)
						{
							//Actions.writeLine(classType.Name + "." + method.Name + ": " + attr.Text);
							usage = attr.Text;
						}
					}
					{
						PermissionRequirementsAttribute attr = (PermissionRequirementsAttribute)method.GetCustomAttribute(typeof(PermissionRequirementsAttribute));
						BotOwnerRequirementAttribute botowner = (BotOwnerRequirementAttribute)method.GetCustomAttribute(typeof(BotOwnerRequirementAttribute));
						UserHasAPermissionAttribute anyperm = (UserHasAPermissionAttribute)method.GetCustomAttribute(typeof(UserHasAPermissionAttribute));
						if (attr != null)
						{
							//Actions.writeLine(classType.Name + "." + method.Name + ": " + attr.Text);
							basePerm = String.IsNullOrWhiteSpace(attr.AllText) ? "" : "[" + attr.AllText + "]";
							if (!basePerm.Equals("[Administrator]"))
							{
								basePerm += basePerm.Contains('[') ? " or <" + attr.AnyText + ">" : "[" + attr.AnyText + "]";
							}
						}
						else if (botowner != null)
						{
							basePerm = "[Bot owner]";
						}
						else if (anyperm != null)
						{
							basePerm = "[Administrator or any perms starting with 'Manage' or ending with 'Members']";
						}
					}
					{
						SummaryAttribute attr = (SummaryAttribute)method.GetCustomAttribute(typeof(SummaryAttribute));
						if (attr != null)
						{
							//Actions.writeLine(classType.Name + "." + method.Name + ": " + attr.Text);
							text = attr.Text;
						}
					}
					Variables.HelpList.Add(new HelpEntry(name, aliases, usage, basePerm, text));
				}
			}
		}

		//Get the permission names to an array
		public static String[] getPermissionNames(uint flags)
		{
			List<String> result = new List<String>();
			for (int i = 0; i < 32; ++i)
			{
				if ((flags & (1 << i)) != 0)
				{
					result.Add(Variables.PermissionNames[i]);
				}
			}
			return result.ToArray();
		}

		//Find the permission names
		public static void loadPermissionNames()
		{
			for (int i = 0; i < 32; ++i)
			{
				String name = "";
				try
				{
					name = Enum.GetName(typeof(GuildPermission), (GuildPermission)i);
					if (name == null)
						continue;
				}
				catch (Exception)
				{
					Actions.writeLine("Bad enum for GuildPermission: " + i);
					continue;
				}
				Variables.PermissionNames.Add(i, name);
				Variables.PermissionValues.Add(name, i);
			}
			//Load all special cases
			loadAllChannelPermissionNames();
		}

		//Find the channel permission names
		public static void loadAllChannelPermissionNames()
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
				String name = "";
				try
				{
					name = Enum.GetName(typeof(ChannelPermission), (ChannelPermission)i);
					if (name == null)
						continue;
				}
				catch (Exception)
				{
					Actions.writeLine("Bad enum for ChannelPermission: " + i);
					continue;
				}
				Variables.ChannelPermissionNames.Add(i, name);
				if ((GENERAL_BITS & (1U << i)) != 0)
				{
					Variables.GeneralChannelPermissionValues.Add(name, i);
				}
				if ((TEXT_BITS & (1U << i)) != 0)
				{
					Variables.TextChannelPermissionValues.Add(name, i);
				}
				if ((VOICE_BITS & (1U << i)) != 0)
				{
					Variables.VoiceChannelPermissionValues.Add(name, i);
				}
			}
		}

		//Complex find a role on the server
		public static async Task<IRole> getRole(CommandContext context, String roleName)
		{
			if (roleName.StartsWith("<@"))
			{
				roleName = roleName.Trim(new char[] { '<', '@', '&', '>' });
				ulong roleID = 0;
				if (UInt64.TryParse(roleName, out roleID))
				{
					return context.Guild.GetRole(roleID);
				}
			}
			List<IRole> roles = context.Guild.Roles.ToList().Where(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)).ToList();
			if (roles.Count > 1)
			{
				await Actions.makeAndDeleteSecondaryMessage(context,
					ERROR("Multiple roles with the same name. Please specify by mentioning the role or changing their names."));
				return null;
			}
			if (roles.Count == 1)
			{
				return roles.First();
			}
			return null;
		}

		//Simple find a role on the server
		public static IRole getRole(IGuild guild, String roleName)
		{
			return guild.Roles.ToList().FirstOrDefault(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
		}

		//Create a role on the server if it's not found
		public static async Task<IRole> createRoleIfNotFound(IGuild guild, String roleName)
		{
			IRole role = getRole(guild, roleName);
			if (role == null)
			{
				role = await guild.CreateRoleAsync(roleName);
			}
			return role;
		} 

		//Get top position of a user
		public static int getPosition(IGuild guild, IGuildUser user)
		{
			int position = 0;
			user.RoleIds.ToList().ForEach(x => position = Math.Max(position, guild.GetRole(x).Position));
			return position;
		}

		//Get a user
		public static async Task<IGuildUser> getUser(IGuild guild, String userName)
		{
			IGuildUser user = await guild.GetUserAsync(getUlong(userName.Trim(new char[] { '<', '>', '@', '!' })));
			return user;
		}

		//Convert the input to a ulong
		public static ulong getUlong(String inputString)
		{
			ulong number = 0;
			if (UInt64.TryParse(inputString, out number))
			{
				return number;
			}
			return 0;
		}

		//Give the user the role
		public static async Task giveRole(IGuildUser user, IRole role)
		{
			if (role == null)
				return;
			if (user.RoleIds.Contains(role.Id))
				return;
			await user.AddRolesAsync(role);
		}

		//Give the user multiple roles
		public static async Task giveRole(IGuildUser user, IRole[] roles)
		{
			await user.AddRolesAsync(roles);
		}

		//Take multiple roles from a user
		public static async Task takeRole(IGuildUser user, IRole[] roles)
		{
			if (roles.Count() == 0)
				return;
			await user.RemoveRolesAsync(roles);
		}

		//Take a single role from a user
		public static async Task takeRole(IGuildUser user, IRole role)
		{
			if (role == null)
				return;
			await user.RemoveRolesAsync(role);
		}

		//See if the user/bot can edit the role
		public static async Task<IRole> getRoleEditAbility(CommandContext context, String input, bool ignore_Errors = false)
		{
			//Check if valid role
			IRole inputRole = await getRole(context, input);
			if (inputRole == null)
			{
				if (!ignore_Errors)
				{
					await makeAndDeleteSecondaryMessage(context, ERROR(Constants.ROLE_ERROR));
				}
				return null;
			}

			//Determine if the user can edit the role
			if ((context.Guild.OwnerId == context.User.Id ? Constants.OWNER_POSITION
				: getPosition(context.Guild, await context.Guild.GetUserAsync(context.User.Id))) <= inputRole.Position)
			{
				if (!ignore_Errors)
				{
					await makeAndDeleteSecondaryMessage(context, 
						ERROR(String.Format("`{0}` has a higher position than you are allowed to edit or use.", inputRole.Name)));
				}
				return null;
			}

			//Determine if the bot can edit the role
			if (getPosition(context.Guild, await context.Guild.GetUserAsync(Variables.Bot_ID)) <= inputRole.Position)
			{
				if (!ignore_Errors)
				{
					await makeAndDeleteSecondaryMessage(context,
						ERROR(String.Format("`{0}` has a higher position than the bot is allowed to edit or use.", inputRole.Name)));
				}
				return null;
			}

			return inputRole;
		}

		//See if the user can edit the channel
		public static async Task<bool> getChannelEditAbility(IGuildChannel channel, IGuildUser user)
		{
			if (Actions.getChannelType(channel) == Constants.TEXT_TYPE)
			{
				using (var channelUsers = channel.GetUsersAsync().GetEnumerator())
				{
					while (await channelUsers.MoveNext())
					{
						if (channelUsers.Current.Contains(user))
						{
							return true;
						}
					}
				}
			}
			else
			{
				if (user == null)
				{
					return false;
				}
				if (user.GetPermissions(channel).Connect)
				{
					return true;
				}
			}
			return false;
		}

		//See if the user can edit this channel
		public static async Task<IGuildChannel> getChannelEditAbility(CommandContext context, String input)
		{
			IGuildChannel channel = await getChannel(context.Guild, input);
			if (channel == null)
			{
				await makeAndDeleteSecondaryMessage(context, ERROR(String.Format("`{0}` does not exist as a channel on this guild.", input)));
				return null;
			}
			if (!await getChannelEditAbility(channel, await context.Guild.GetUserAsync(context.User.Id)))
			{
				await makeAndDeleteSecondaryMessage(context, ERROR(String.Format("You do not have the ability to edit `{0}`.", channel.Name)));
				return null;
			}
			return channel;
		}

		//Remove secondary messages
		public static async Task makeAndDeleteSecondaryMessage(CommandContext context, String secondStr, Int32 time = Constants.WAIT_TIME)
		{
			IUserMessage secondMsg = await context.Channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + secondStr);
			removeCommandMessages(context.Channel, new IUserMessage[] { secondMsg, context.Message }, time);
		}

		//Remove commands
		public static void removeCommandMessages(IMessageChannel channel, IUserMessage[] messages, Int32 time)
		{
			Task t = Task.Run(async () =>
			{
				Thread.Sleep(time);
				await channel.DeleteMessagesAsync(messages);
			});
		}

		//Format the error message
		public static String ERROR(String message)
		{
			return Constants.ZERO_LENGTH_CHAR + Constants.ERROR_MESSAGE + message;
		}

		//Send a message with a zero length char at the front
		public static async Task<IMessage> sendChannelMessage(IMessageChannel channel, String message)
		{
			if (channel == null || !Variables.Guilds.Contains((channel as ITextChannel).Guild))
				return null;

			return await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + message);
		}

		//Remove messages
		public static async Task removeMessages(IMessageChannel channel, int requestCount)
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

						////Try due to 404 errors
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
		public static async Task removeMessages(ITextChannel channel, int requestCount, IUser user)
		{
			//Make sure there's a user id
			if (user == null)
			{
				await removeMessages(channel, requestCount);
				return;
			}

			Actions.writeLine(String.Format("Deleting {0} messages from {1} in channel {2} in guild {3}.", requestCount, user.Id, channel.Name, channel.GuildId));
			List<IMessage> allMessages = new List<IMessage>();
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
			List<IMessage> userMessages = allMessages.Where(x => user == x.Author).ToList();
			if (requestCount > userMessages.Count)
			{
				requestCount = userMessages.Count;
			}
			else if (requestCount < userMessages.Count)
			{
				userMessages.RemoveRange(requestCount, userMessages.Count - requestCount);
			}
			userMessages.Insert(0, allMessages[0]); //Remove the initial command message

			Actions.writeLine(String.Format("Found {0} messages; deleting {1} from user {2}", allMessages.Count, userMessages.Count - 1, user.Username));
			await channel.DeleteMessagesAsync(userMessages.ToArray());
		}

		//Get a channel ID
		public static async Task<IMessageChannel> getChannelID(IGuild guild, String channelName)
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
		public static async Task<IGuildChannel> getChannel(IGuild guild, String input)
		{
			if (input.Contains("<#"))
			{
				input = input.Substring(input.IndexOf("<#"));
			}
			if (input.Contains(' '))
			{
				input = input.Substring(0, input.IndexOf(' '));
			}
			String[] values = input.Split(new char[] { '/' }, 2);

			//Get input channel type
			String channelType = values.Length == 2 ? values[1].ToLower() : null;
			if (channelType != null && !(channelType.Equals(Constants.TEXT_TYPE) || channelType.Equals(Constants.VOICE_TYPE)))
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
				var channels = gottenChannels.Where(x => x.Name.Equals(values[0], StringComparison.OrdinalIgnoreCase) && x.GetType().Name.ToLower().Contains(channelType)).ToList();

				if (channels.Count == 0)
					return null;
				if (channels.Count == 1)
					return channels[0];

			}

			return null;
		}

		//Get integer
		public static int getInteger(String inputString)
		{
			int number = 0;
			if (Int32.TryParse(inputString, out number))
			{
				return number;
			}
			return -1;
		}

		//Get server commands
		public static String[] getCommands(IGuild guild, int number)
		{
			List<PreferenceCategory> categories;
			if (!Variables.CommandPreferences.TryGetValue(guild.Id, out categories))
			{
				return null;
			}

			List<string> commands = new List<string>();
			foreach (PreferenceSetting command in categories[number].mSettings)
			{
				commands.Add(command.mName.ToString());
			}
			return commands.ToArray();
		}

		//Load preferences
		public static void loadPreferences(IGuild guild)
		{
			List<PreferenceCategory> categories;
			if (Variables.CommandPreferences.TryGetValue(guild.Id, out categories))
			{
				return;
			}

			categories = new List<PreferenceCategory>();
			Variables.CommandPreferences[guild.Id] = categories;

			String path = getServerFilePath(guild.Id, Constants.PREFERENCES_FILE);
			if (!File.Exists(path))
			{
				path = "DefaultCommandPreferences.txt";
			}

			using (StreamReader file = new StreamReader(path))
			{
				Actions.writeLine(MethodBase.GetCurrentMethod().Name + ": preferences for the server " + guild.Name + " have been loaded.");
				//Read the preferences document for information
				String line;
				while ((line = file.ReadLine()) != null)
				{
					//If the line is empty, do nothing
					if (String.IsNullOrWhiteSpace(line))
					{
						continue;
					}
					//If the line starts with an @ then it's a category
					if (line.StartsWith("@"))
					{
						categories.Add(new PreferenceCategory(line.Substring(1)));
					}
					//Anything else and it's a setting
					else
					{
						//Split before and after the colon, before is the setting name, after is the value
						String[] values = line.Split(new char[] { ':' }, 2);
						if (values.Length == 2)
						{
							categories[categories.Count - 1].mSettings.Add(new PreferenceSetting(values[0], values[1]));
						}
						else
						{
							Actions.writeLine("ERROR: " + line);
						}
					}
				}
			}
		}

		//Get file paths
		public static String getServerFilePath(ulong serverId, String fileName)
		{
			//Gets the appdata folder for usage, allowed to change
			String folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			//Combines the path for appdata and the preferences text file, allowed to change, but I'd recommend to keep the serverID part
			String directory = Path.Combine(folder, "Discord_Servers", serverId.ToString());
			//This string will be similar to C:\Users\User\AppData\Roaming\ServerID
			String path = Path.Combine(directory, fileName);
			return path;
		}

		//Checks what the serverlog is
		public static async Task<ITextChannel> logChannelCheck(IGuild guild, String serverOrMod)
		{
			String path = getServerFilePath(guild.Id, Constants.SERVERLOG_AND_MODLOG);
			ITextChannel logChannel = null;
			//Check if the file exists
			if (!File.Exists(path))
			{
				//Default to 'advobot' if it doesn't exist
				if (getChannel(guild, Variables.Bot_Channel) != null)
				{
					logChannel = getChannel(guild, Variables.Bot_Channel) as ITextChannel;
					return logChannel;
				}
				//If the file and the channel both don't exist then return null
				return null;
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
							String[] logChannelArray = line.Split(new Char[] { ':' }, 2);

							if (String.IsNullOrWhiteSpace(logChannelArray[1]) || (String.IsNullOrEmpty(logChannelArray[1])))
							{
								return null;
							}
							else
							{
								logChannel = (await guild.GetChannelAsync(Convert.ToUInt64(logChannelArray[1]))) as ITextChannel;
								return logChannel;
							}
						}
					}
				}
			}
			return null;
		}

		//Edit message log message
		public static async Task editMessage(ITextChannel logChannel, String time, IGuildUser user, IMessageChannel channel, String before, String after)
		{
			await sendChannelMessage(logChannel, String.Format("{0} **EDIT:** `{1}#{2}` **IN** `#{3}`\n**FROM:** ```\n{4}```\n**TO:** ```\n{5}```",
				time, user.Username, user.Discriminator, channel.Name, before, after));
		}

		//Check if the user is the owner of the server
		public static bool userHasOwner(IGuild guild, IUser user)
		{
			if (guild == null)
				return false;

			return guild.GetOwnerAsync().Result.Id == user.Id;
		}

		//Bheck if the user if the bot owner
		public static bool userHasBotOwner(IGuild guild, IUser user)
		{
			if (guild == null)
				return false;

			return user.Id == Constants.OWNER_ID;
		}

		//Send an exception message to the console
		public static void exceptionToConsole(String method, Exception e)
		{
			Actions.writeLine(method + " EXCEPTION: " + e.ToString());
		}

		//Upload various text to a text uploader with a list of messages
		public static String uploadToHastebin(List<String> textList)
		{
			//Messages in the format to upload
			string text = replaceMessageCharacters(String.Join("\n-----\n", textList));
			return uploadToHastebin(text);
		}

		//Upload various text to a text uploader with a string
		public static String uploadToHastebin(String text)
		{
			//Regex for getting the key out
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
		public static async Task uploadTextFile(IGuild guild, IMessageChannel channel, List<String> textList, String fileName, String messageHeader)
		{
			//Messages in the format to upload
			string text = replaceMessageCharacters(String.Join("\n-----\n", textList));
			await uploadTextFile(guild, channel, text, fileName, messageHeader);
		}
		
		//Upload a text file with a string
		public static async Task uploadTextFile(IGuild guild, IMessageChannel channel, String text, String fileName, String messageHeader)
		{
			//Get the file path
			String deletedMessagesFile = fileName + DateTime.UtcNow.ToString("MM-dd_HH-mm-ss") + ".txt";
			String path = Actions.getServerFilePath(guild.Id, deletedMessagesFile);

			//Create the temporary file
			if (!File.Exists(Actions.getServerFilePath(guild.Id, deletedMessagesFile)))
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

		//Get rid of certain elements to make messages look neater
		public static String replaceMessageCharacters(String input)
		{
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

		//Get bits
		public static async Task<uint> getBit(CommandContext context, String permission, uint changeValue)
		{
			try
			{
				int bit = Variables.PermissionValues[permission];
				changeValue |= (1U << bit);
				return changeValue;
			}
			catch (Exception)
			{
				await makeAndDeleteSecondaryMessage(context, ERROR(String.Format("Couldn't parse permission '{0}'", permission)));
				return 0;
			}
		}

		//Get the permissions something has on a channel
		public static Dictionary<String, String> getChannelPermissions(Overwrite overwrite)
		{
			//Create a dictionary to hold the allow/deny/inherit values
			Dictionary<String, String> channelPerms = new Dictionary<String, String>();

			//Make a copy of the channel perm list to check off perms as they go by
			List<String> genericChannelPerms = Variables.ChannelPermissionNames.Values.ToList();

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

		//Remove voice channel perms
		public static Dictionary<String, String> getTextChannelPermissions(Dictionary<String, String> dictionary)
		{
			Variables.VoiceChannelPermissionValues.Keys.ToList().ForEach(x => dictionary.Remove(x));
			return dictionary;
		}

		//Remove text channel perms 
		public static Dictionary<String, String> getVoiceChannelPermissions(Dictionary<String, String> dictionary)
		{
			Variables.TextChannelPermissionValues.Keys.ToList().ForEach(x => dictionary.Remove(x));
			return dictionary;
		}

		//Return a dictionary with the correct perms
		public static Dictionary<String, String> getPerms(Overwrite overwrite, IGuildChannel channel)
		{
			//Get the general perms from the overwrite given
			Dictionary<String, String> dictionary = Actions.getChannelPermissions(overwrite);

			//See if the channel is a text channel and remove voice channel perms
			if (Actions.getChannelType(channel) == Constants.TEXT_TYPE)
			{
				getTextChannelPermissions(dictionary);
			}
			//See if the channel is a voice channel and remove text channel perms
			else if (Actions.getChannelType(channel) == Constants.VOICE_TYPE)
			{
				getVoiceChannelPermissions(dictionary);
			}

			return dictionary;
		}

		//Get the input string and permissions
		public static bool getStringAndPermissions(String input, out String output, out List<String> permissions)
		{
			output = null;
			permissions = null;
			String[] values = input.Split(new char[] { ' ' });
			if (values.Length == 1)
				return false;

			permissions = values.Last().Split('/').ToList();
			output = String.Join(" ", values.Take(values.Length - 1));

			return output != null && permissions != null;
		}

		//Send a message with an embedded object
		public static async Task<IMessage> sendEmbedMessage(IMessageChannel channel, String message, EmbedBuilder embed)
		{
			if (channel == null || !Variables.Guilds.Contains((channel as ITextChannel).Guild))
				return null;

			return await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + message, embed: embed);
		}

		//Send an embedded object
		public static async Task<IMessage> sendEmbedMessage(IMessageChannel channel, EmbedBuilder embed)
		{
			if (channel == null || !Variables.Guilds.Contains((channel as ITextChannel).Guild))
				return null;

			return await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR, embed: embed);
		}

		//Make a new embed builder
		public static EmbedBuilder makeNewEmbed(Color? color = null, String title = null, String description = null, String imageURL = null)
		{
			//Timestamp is in UTC for simplicity and organization's sake
			EmbedBuilder embed = new EmbedBuilder().WithColor(Constants.BASE).WithCurrentTimestamp();
			
			if (color != null)
			{
				embed.Color = color.Value;
			}
			if (title != null)
			{
				embed.Title = title;
			}
			if (description != null)
			{
				embed.Description = description;
			}
			if (imageURL != null)
			{
				embed.ImageUrl = imageURL;
			}

			return embed;
		}

		//Make a new author for an embed
		public static EmbedBuilder addAuthor(EmbedBuilder embed, String name = null, String iconURL = null, String URL = null)
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
		public static EmbedBuilder addFooter(EmbedBuilder embed, String text = null, String iconURL = null)
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
		public static EmbedBuilder addField(EmbedBuilder embed, String name, String value, bool isInline = true)
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

		//Write to the console with a timestamp
		public static void writeLine(String text)
		{
			if (text != null)
			{
				Console.WriteLine(DateTime.UtcNow.ToString("HH:mm:ss") + " " + text);
			}
		}

		//Set the server or mod log
		public static async Task<ITextChannel> setServerOrModLog(CommandContext context, String input, String serverOrMod)
		{
			ITextChannel logChannel = null;
			//See if not null
			if (String.IsNullOrWhiteSpace(input))
			{
				await makeAndDeleteSecondaryMessage(context, ERROR("No channel specified."));
				return null;
			}
			else if (input.ToLower().Equals("off"))
			{
				logChannel = null;
			}

			//Get the channel with its ID
			var textChannels = context.Guild.GetTextChannelsAsync().Result.ToList().Where(x => input.Contains(x.Id.ToString())).ToList();
			if (textChannels.Count == 1)
			{
				logChannel = textChannels[0];
			}


			//Create the file if it doesn't exist
			String path = getServerFilePath(context.Guild.Id, Constants.SERVERLOG_AND_MODLOG);
			if (!File.Exists(path))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				var newFile = File.Create(path);
				newFile.Close();
			}

			//Find the lines that aren't the current serverlog line
			List<String> validLines = new List<String>();
			using (StreamReader reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (line.Contains(serverOrMod))
					{
						if ((logChannel != null) && (line.Contains(logChannel.Id.ToString())))
						{
							await makeAndDeleteSecondaryMessage(context, "Channel is already the current " + serverOrMod + ".");
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
				if (logChannel == null)
				{
					writer.WriteLine(serverOrMod + ":" + null + "\n" + String.Join("\n", validLines));
					await makeAndDeleteSecondaryMessage(context, "Disabled the " + serverOrMod + ".");
					return null;
				}
				else
				{
					writer.WriteLine(serverOrMod + ":" + logChannel.Id + "\n" + String.Join("\n", validLines));
				}
			}

			return logChannel;
		}

		//Get if a channel is a text or voice channel
		public static String getChannelType(IGuildChannel channel)
		{
			return channel.GetType().Name.ToLower().Contains(Constants.TEXT_TYPE) ? Constants.TEXT_TYPE : Constants.VOICE_TYPE;
		}
	}
}