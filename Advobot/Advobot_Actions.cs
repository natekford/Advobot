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
		#region Loads
		//Loading in all necessary information at bot start up
		public static void loadInformation()
		{
			Variables.Bot_ID = CommandHandler.Client.CurrentUser.Id;				//Give the variable Bot_ID the actual ID
			Variables.Bot_Name = CommandHandler.Client.CurrentUser.Username;		//Give the variable Bot_Name the username of the bot
			Variables.Bot_Channel = Variables.Bot_Name.ToLower();					//Give the variable Bot_Channel a lowered version of the bot's name

			loadPermissionNames();													//Gets the names of the permission bits in Discord
			loadCommandInformation();												//Gets the information of a command (name, aliases, usage, summary). Has to go after LPN
			Variables.HelpList.ForEach(x => Variables.CommandNames.Add(x.Name));	//Gets all the active command names. Has to go after LCI
		}

		//Load the information from the commands
		public static void loadCommandInformation()
		{
			var classTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsSubclassOf(typeof(ModuleBase)));
			foreach (var classType in classTypes)
			{
				List<MethodInfo> methods = classType.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic).ToList();
				foreach (var method in methods)
				{
					string name = "N/A";
					string[] aliases = { "N/A" };
					string usage = "N/A";
					string basePerm = "N/A";
					string text = "N/A";
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

		//Load the permission names
		public static void loadPermissionNames()
		{
			for (int i = 0; i < 32; ++i)
			{
				string name = "";
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

		//Load the channel permission names
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
				string name = "";
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

		//Load preferences
		public static void loadPreferences(IGuild guild)
		{
			List<PreferenceCategory> categories;
			if (Variables.CommandPreferences.TryGetValue(guild.Id, out categories))
				return;

			categories = new List<PreferenceCategory>();
			Variables.CommandPreferences[guild.Id] = categories;

			//Check if this server has any preferences
			string path = getServerFilePath(guild.Id, Constants.PREFERENCES_FILE);
			if (!File.Exists(path))
			{
				//If not, go to the defaults
				string defaultPreferences = Properties.Resources.DefaultCommandPreferences;
				//Split by new lines
				defaultPreferences.Split('\n').ToList().ForEach(x =>
				{
					//If the line is empty, do nothing
					if (String.IsNullOrWhiteSpace(x))
					{
					}
					//If the line starts with an @ then it's a category
					else if (x.StartsWith("@"))
					{
						categories.Add(new PreferenceCategory(x.Substring(1)));
					}
					//Anything else and it's a setting
					else
					{
						//Split before and after the colon, before is the setting name, after is the value
						string[] values = x.Split(new char[] { ':' }, 2);
						if (values.Length == 2)
						{
							categories[categories.Count - 1].Settings.Add(new PreferenceSetting(values[0], values[1]));
						}
						else
						{
							Actions.writeLine("ERROR: " + x);
						}
					}
				});
				Actions.writeLine(MethodBase.GetCurrentMethod().Name + ": preferences for the server " + guild.Name + " have been loaded.");
				return;
			}

			using (StreamReader file = new StreamReader(path))
			{
				//Read the preferences document for information
				string line;
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
						string[] values = line.Split(new char[] { ':' }, 2);
						if (values.Length == 2)
						{
							categories[categories.Count - 1].Settings.Add(new PreferenceSetting(values[0], values[1]));
						}
						else
						{
							Actions.writeLine("ERROR: " + line);
						}
					}
				}
				Actions.writeLine(MethodBase.GetCurrentMethod().Name + ": preferences for the server " + guild.Name + " have been loaded.");
			}
		}

		//Load banned words/regex/punishments
		public static void loadBannedPhrasesAndPunishments(IGuild guild)
		{
			//Check if the file even exists
			string path = getServerFilePath(guild.Id, Constants.BANNED_PHRASES);
			if (!File.Exists(path))
				return;

			//Get the banned phrases and regex
			var bannedPhrases = new List<string>();
			var bannedRegex = new List<Regex>();
			var bannedPhrasesPunishments = new List<BannedPhrasePunishment>();
			using (StreamReader file = new StreamReader(path))
			{
				Actions.writeLine(MethodBase.GetCurrentMethod().Name + ": banned phrases/regex/punishments for the server " + guild.Name + " have been loaded.");
				string line;
				while ((line = file.ReadLine()) != null)
				{
					//If the line is empty, do nothing
					if (String.IsNullOrWhiteSpace(line))
					{
						continue;
					}
					//Banned phrases
					if (line.StartsWith(Constants.BANNED_PHRASES_CHECK_STRING))
					{
						int index = line.IndexOf(':');
						if (index >= 0 && index < line.Length - 1)
						{
							string phrases = line.Substring(index + 1);
							if (!String.IsNullOrWhiteSpace(phrases))
							{
								bannedPhrases = phrases.Split('/').Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().ToList();
							}
						}
						continue;
					}
					//Banned regex
					if (line.StartsWith(Constants.BANNED_REGEX_CHECK_STRING))
					{
						int index = line.IndexOf(':');
						if (index >= 0 && index < line.Length - 1)
						{
							string regex = line.Substring(index + 1);
							if (!String.IsNullOrWhiteSpace(regex))
							{
								bannedRegex = regex.Split('/').Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().Select(x => new Regex(x)).ToList();
							}
						}
						continue;
					}
					//Punishments
					if (line.StartsWith(Constants.BANNED_PHRASES_PUNISHMENTS))
					{
						int index = line.IndexOf(':');
						if (index >= 0 && index < line.Length - 1)
						{
							string punishments = line.Substring(index + 1);
							punishments.Split('/').Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().ToList().ForEach(x =>
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
								int givenTime = 0;
								int? time = null;
								if (role != null && !int.TryParse(args[3], out givenTime))
									return;
								else if (givenTime != 0)
									time = givenTime;

								bannedPhrasesPunishments.Add(new BannedPhrasePunishment(number, (PunishmentType)punishment, role, time));
							});
						}
						continue;
					}
				}
			}

			//Add them to the dictionary with the guild
			if (!Variables.BannedPhrases.ContainsKey(guild.Id) && bannedPhrases.Any())
			{
				Variables.BannedPhrases.Add(guild.Id, bannedPhrases);
			}
			if (!Variables.BannedRegex.ContainsKey(guild.Id) && bannedRegex.Any())
			{
				Variables.BannedRegex.Add(guild.Id, bannedRegex);
			}
			if (!Variables.BannedPhrasesPunishments.ContainsKey(guild.Id) && bannedPhrasesPunishments.Any())
			{
				Variables.BannedPhrasesPunishments.Add(guild.Id, bannedPhrasesPunishments);
			}
		}

		//Load the self assignable roles
		public static void loadSelfAssignableRoles(IGuild guild)
		{
			//Check if the file even exists
			string path = getServerFilePath(guild.Id, Constants.SA_ROLES);
			if (!File.Exists(path))
				return;

			//Read the file
			using (StreamReader file = new StreamReader(path))
			{
				Actions.writeLine(MethodBase.GetCurrentMethod().Name + ": self assignable roles for the server " + guild.Name + " have been loaded.");
				string line;
				while ((line = file.ReadLine()) != null)
				{
					//If the line is empty, do nothing
					if (String.IsNullOrWhiteSpace(line))
					{
						continue;
					}
					else
					{
						string[] inputArray = line.Split(' ');
						ulong ID = 0;
						int group = 0;

						//Test if valid role
						if (!ulong.TryParse(inputArray[0], out ID))
							return;
						IRole role = guild.GetRole(ID);
						if (role == null)
							return;

						//Test if valid group
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
					continue;
				}
			}
		}
		#endregion

		#region Gets
		//Complex get a role on the server
		public static async Task<IRole> getRole(CommandContext context, string roleName)
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
		
		//Simple get a role on the server
		public static IRole getRole(IGuild guild, string roleName)
		{
			//Order them by position (puts everyone first) then reverse so it sorts from the top down
			return guild.Roles.ToList().OrderBy(x => x.Position).Reverse().FirstOrDefault(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
		}
		
		//Get top position of a user
		public static int getPosition(IGuild guild, IGuildUser user)
		{
			if (user.Id == guild.OwnerId)
				return Constants.OWNER_POSITION;

			int position = 0;
			user.RoleIds.ToList().ForEach(x => position = Math.Max(position, guild.GetRole(x).Position));

			return position;
		}
		
		//Get a user
		public static async Task<IGuildUser> getUser(IGuild guild, string userName)
		{
			IGuildUser user = await guild.GetUserAsync(getUlong(userName.Trim(new char[] { '<', '>', '@', '!' })));
			return user;
		}
		
		//Get the input to a ulong
		public static ulong getUlong(string inputString)
		{
			ulong number = 0;
			if (UInt64.TryParse(inputString, out number))
			{
				return number;
			}
			return 0;
		}
		
		//Get if the user/bot can edit the role
		public static async Task<IRole> getRoleEditAbility(CommandContext context, string input = null, bool ignore_Errors = false, IRole role = null)
		{
			//Check if valid role
			IRole inputRole = role == null ? await getRole(context, input) : role;
			if (inputRole == null)
			{
				if (!ignore_Errors)
				{
					await makeAndDeleteSecondaryMessage(context, ERROR(Constants.ROLE_ERROR));
				}
				return null;
			}

			//Determine if the user can edit the role
			if (inputRole.Position >= getPosition(context.Guild, context.User as IGuildUser))
			{
				if (!ignore_Errors)
				{
					await makeAndDeleteSecondaryMessage(context,
						ERROR(String.Format("`{0}` has a higher position than you are allowed to edit or use.", inputRole.Name)));
				}
				return null;
			}

			//Determine if the bot can edit the role
			if (inputRole.Position >= getPosition(context.Guild, await context.Guild.GetUserAsync(Variables.Bot_ID)))
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
		
		//Get if the user can edit the channel
		public static async Task<IGuildChannel> getChannelEditAbility(IGuildChannel channel, IGuildUser user)
		{
			if (Actions.getChannelType(channel) == Constants.TEXT_TYPE)
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
		public static async Task<IGuildChannel> getChannelEditAbility(CommandContext context, string input)
		{
			IGuildChannel channel = await getChannel(context, input);
			if (channel == null)
			{
				return null;
			}
			if (await getChannelEditAbility(channel, await context.Guild.GetUserAsync(context.User.Id)) == null)
			{
				await makeAndDeleteSecondaryMessage(context, ERROR(String.Format("You do not have the ability to edit `{0}`.", channel.Name)));
				return null;
			}
			return channel;
		}
		
		//Get a channel ID
		public static async Task<IMessageChannel> getChannelID(IGuild guild, string channelName)
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
		public static async Task<IGuildChannel> getChannel(CommandContext context, string input)
		{
			return await getChannel(context.Guild, context.Channel, context.Message, input);
		}
		
		//Get a channel without context
		public static async Task<IGuildChannel> getChannel(IGuild guild, IMessageChannel channel, IUserMessage message, string input)
		{
			if (input.Contains("<#"))
			{
				input = input.Substring(input.IndexOf("<#"));
			}
			if (input.Contains(' '))
			{
				input = input.Substring(0, input.IndexOf(' '));
			}
			string[] values = input.Split(new char[] { '/' }, 2);

			//Get input channel type
			string channelType = values.Length == 2 ? values[1].ToLower() : null;
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
					await makeAndDeleteSecondaryMessage(channel, message, ERROR(String.Format("`{0}` does not exist as a channel on this guild.", input.Substring(0, input.IndexOf('/')))));
				if (channels.Count == 1)
					return channels[0];
				if (channels.Count > 1)
					await makeAndDeleteSecondaryMessage(channel, message, ERROR("More than one channel exists with the same name."));
			}

			return null;
		}
		
		//Get the log channel
		public static async Task<IGuildChannel> getLogChannel(IGuild guild)
		{
			//Get the channels from the guild
			var gottenChannels = await guild.GetTextChannelsAsync();
			//See which match the name and type given
			return gottenChannels.FirstOrDefault(x => x.Name.Equals(Variables.Bot_Channel, StringComparison.OrdinalIgnoreCase));
		}
		
		//Get integer
		public static int getInteger(string inputString)
		{
			int number = 0;
			if (Int32.TryParse(inputString, out number))
			{
				return number;
			}
			return -1;
		}
		
		//Get bits
		public static async Task<uint> getBit(CommandContext context, string permission, uint changeValue)
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
			List<string> genericChannelPerms = Variables.ChannelPermissionNames.Values.ToList();

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
		public static Dictionary<String, String> getTextChannelPermissions(Dictionary<String, String> dictionary)
		{
			Variables.VoiceChannelPermissionValues.Keys.ToList().ForEach(x => dictionary.Remove(x));
			return dictionary;
		}
		
		//Get voice channel perms
		public static Dictionary<String, String> getVoiceChannelPermissions(Dictionary<String, String> dictionary)
		{
			Variables.TextChannelPermissionValues.Keys.ToList().ForEach(x => dictionary.Remove(x));
			return dictionary;
		}
		
		//Get a dictionary with the correct perms
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
		public static bool getStringAndPermissions(string input, out string output, out List<string> permissions)
		{
			output = null;
			permissions = null;
			string[] values = input.Split(new char[] { ' ' });
			if (values.Length == 1)
				return false;

			permissions = values.Last().Split('/').ToList();
			output = String.Join(" ", values.Take(values.Length - 1));

			return output != null && permissions != null;
		}
		
		//Get server commands
		public static string[] getCommands(IGuild guild, int number)
		{
			List<PreferenceCategory> categories;
			if (!Variables.CommandPreferences.TryGetValue(guild.Id, out categories))
			{
				return null;
			}

			List<string> commands = new List<string>();
			foreach (PreferenceSetting command in categories[number].Settings)
			{
				commands.Add(command.mName.ToString());
			}
			return commands.ToArray();
		}
		
		//Get file paths
		public static string getServerFilePath(ulong serverId, string fileName)
		{
			string folder;
			if (String.IsNullOrWhiteSpace(Properties.Settings.Default.Path))
			{
				if (Variables.Windows)
				{
					//Get the appdata folder
					folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				}
				else
					return null;
			}
			else
			{
				folder = Properties.Settings.Default.Path;
				//If not a valid directory then give null
				if (!Directory.Exists(folder))
					return null;
			}
			//Combine the path for the folders
			string directory = Path.Combine(folder, Constants.SERVER_FOLDER + "_" + Variables.Bot_Name, serverId.ToString());
			//This string will be similar to C:\Users\User\AppData\Roaming\ServerID if on Windows. If not then it can be anything
			string path = Path.Combine(directory, fileName);
			return path;
		}
		
		//Get if a channel is a text or voice channel
		public static string getChannelType(IGuildChannel channel)
		{
			return channel.GetType().Name.ToLower().Contains(Constants.TEXT_TYPE) ? Constants.TEXT_TYPE : Constants.VOICE_TYPE;
		}
		
		//Get if a bot channel already exists
		public static async Task<bool> getDuplicateBotChan(IGuild guild)
		{
			//Get a list of text channels
			var tChans = await guild.GetTextChannelsAsync();
			//Return a bool stating if there's more than one or not
			return tChans.Where(x => x.Name == Variables.Bot_Channel).Count() > 1;
		}
		
		//Get what the serverlog is
		public static async Task<ITextChannel> logChannelCheck(IGuild guild, string serverOrMod)
		{
			string path = getServerFilePath(guild.Id, Constants.SERVERLOG_AND_MODLOG);
			ITextChannel logChannel = null;
			//Check if the file exists
			if (!File.Exists(path))
			{
				//Default to 'advobot' if it doesn't exist
				logChannel = getLogChannel(guild) as ITextChannel;
				if (logChannel != null)
				{
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
							string[] logChannelArray = line.Split(new Char[] { ':' }, 2);

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
		
		//Get if the user is the owner of the server
		public static async Task<bool> userHasOwner(IGuild guild, IUser user)
		{
			if (guild == null)
				return false;

			return (await guild.GetOwnerAsync()).Id == user.Id;
		}
		
		//Get if the user if the bot owner
		public static bool userHasBotOwner(IGuild guild, IUser user)
		{
			if (guild == null)
				return false;

			return user.Id == Properties.Settings.Default.BotOwner;
		}

		//Get the permission names to an array
		public static string[] getPermissionNames(uint flags)
		{
			List<string> result = new List<string>();
			for (int i = 0; i < 32; ++i)
			{
				if ((flags & (1 << i)) != 0)
				{
					result.Add(Variables.PermissionNames[i]);
				}
			}
			return result.ToArray();
		}

		//Get the variables for slowmode
		public static string getVariable(string[] inputArray, string searchTerm)
		{
			if (inputArray != null && inputArray.Any(x => x.ToLower().StartsWith(searchTerm)))
			{
				string first = inputArray.Where(x => x.ToLower().StartsWith(searchTerm)).FirstOrDefault();
				return first.Substring(first.IndexOf(':') + 1);
			}
			return null;
		}

		//Get the variable out of a string
		public static string getVariable(string inputString, string searchTerm)
		{
			if (inputString != null && inputString.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase))
			{
				return inputString.Substring(inputString.IndexOf(':') + 1);
			}
			return null;
		}

		//Get the OS
		public static void getOS()
		{
			string windir = Environment.GetEnvironmentVariable("windir");
			if (!string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir))
			{
				Variables.Windows = true;
			}
			else
			{
				Variables.Windows = false;
			}
		}

		//Get the bot owner
		public static async Task<IGuildUser> getBotOwner(IDiscordClient client)
		{
			foreach (var guild in await client.GetGuildsAsync())
			{
				var user = (await guild.GetUsersAsync()).FirstOrDefault(x => x.Id == Properties.Settings.Default.BotOwner);
				if (user != null)
				{
					return user;
				}
			}
			return null;
		}
		#endregion

		#region Roles
		//Create a role on the server if it's not found
		public static async Task<IRole> createRoleIfNotFound(IGuild guild, string roleName)
		{
			IRole role = getRole(guild, roleName);
			if (role == null)
			{
				role = await guild.CreateRoleAsync(roleName);
			}
			return role;
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
		#endregion

		#region Message Removal
		//Remove secondary messages
		public static async Task makeAndDeleteSecondaryMessage(CommandContext context, string secondStr, Int32 time = Constants.WAIT_TIME)
		{
			IUserMessage secondMsg = await context.Channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + secondStr);
			removeCommandMessages(context.Channel, new IUserMessage[] { secondMsg, context.Message }, time);
		}
		
		//Remove secondary messages without context
		public static async Task makeAndDeleteSecondaryMessage(IMessageChannel channel, IUserMessage message, string secondStr, Int32 time = Constants.WAIT_TIME)
		{
			IUserMessage secondMsg = await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + secondStr);
			removeCommandMessages(channel, new IUserMessage[] { secondMsg, message }, time);
		}
		
		//Remove commands
		public static void removeCommandMessages(IMessageChannel channel, IUserMessage[] messages, Int32 time)
		{
			Task t = Task.Run(async () =>
			{
				await Task.Delay(time);
				await channel.DeleteMessagesAsync(messages);
			});
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

						//Try due to 404 errors
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
		#endregion

		#region Message Formatting
		//Format the error message
		public static string ERROR(string message)
		{
			return Constants.ZERO_LENGTH_CHAR + Constants.ERROR_MESSAGE + message;
		}
		
		//Send a message with a zero length char at the front
		public static async Task<IMessage> sendChannelMessage(IMessageChannel channel, string message)
		{
			if (channel == null || !Variables.Guilds.Contains((channel as ITextChannel).Guild))
				return null;

			return await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + message);
		}

		public static async Task<IMessage> sendDMMessage(IDMChannel channel, string message)
		{
			if (channel == null)
				return null;

			return await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + message);
		}
		
		//Edit message log message
		public static async Task editMessage(ITextChannel logChannel, string time, IGuildUser user, IMessageChannel channel, string before, string after)
		{
			await sendChannelMessage(logChannel, String.Format("{0} **EDIT:** `{1}#{2}` **IN** `#{3}`\n**FROM:** ```\n{4}```\n**TO:** ```\n{5}```",
				time, user.Username, user.Discriminator, channel.Name, before, after));
		}
		
		//Get rid of certain elements to make messages look neater
		public static string replaceMessageCharacters(string input)
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
		#endregion

		#region Uploads
		//Upload various text to a text uploader with a list of messages
		public static string uploadToHastebin(List<string> textList)
		{
			//Messages in the format to upload
			string text = replaceMessageCharacters(String.Join("\n-----\n", textList));
			return uploadToHastebin(text);
		}
		
		//Upload various text to a text uploader with a string
		public static string uploadToHastebin(string text)
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
		public static async Task uploadTextFile(IGuild guild, IMessageChannel channel, List<string> textList, string fileName, string messageHeader)
		{
			//Messages in the format to upload
			string text = replaceMessageCharacters(String.Join("\n-----\n", textList));
			await uploadTextFile(guild, channel, text, fileName, messageHeader);
		}
		
		//Upload a text file with a string
		public static async Task uploadTextFile(IGuild guild, IMessageChannel channel, string text, string fileName, string messageHeader)
		{
			//Get the file path
			string deletedMessagesFile = fileName + DateTime.UtcNow.ToString("MM-dd_HH-mm-ss") + ".txt";
			string path = Actions.getServerFilePath(guild.Id, deletedMessagesFile);
			if (path == null)
				return;

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
		#endregion

		#region Embeds
		//Send a message with an embedded object
		public static async Task<IMessage> sendEmbedMessage(IMessageChannel channel, string message, EmbedBuilder embed)
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

			try
			{
				return await channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR, embed: embed);
			}
			//Embeds fail every now and then and I haven't been able to find the problem yet (I know fields are a problem, but not in this case)
			catch (Exception e)
			{
				exceptionToConsole(MethodBase.GetCurrentMethod().Name, e);
				return null;
			}
		}
		
		//Make a new embed builder
		public static EmbedBuilder makeNewEmbed(Color? color = null, string title = null, string description = null, string imageURL = null)
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
		public static EmbedBuilder addAuthor(EmbedBuilder embed, string name = null, string iconURL = null, string URL = null)
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
		public static EmbedBuilder addFooter(EmbedBuilder embed, string text = null, string iconURL = null)
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
		public static EmbedBuilder addField(EmbedBuilder embed, string name = "null", string value = "null", bool isInline = true)
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
		public static void writeLine(string text)
		{
			Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + text);
		}
		
		//Send an exception message to the console
		public static void exceptionToConsole(string method, Exception e)
		{
			Actions.writeLine(method + " EXCEPTION: " + e.ToString());
		}
		#endregion

		#region Server/Mod Log
		//Check if the bot can type in a logchannel
		public static async Task<bool> permissionCheck(ITextChannel channel)
		{
			IGuildUser bot = await channel.Guild.GetUserAsync(Variables.Bot_ID);

			//Check if the bot can send messages
			if (!bot.GetPermissions(channel).SendMessages)
				return false;

			//Check if the bot can embed
			if (!bot.GetPermissions(channel).EmbedLinks)
			{
				await Actions.sendChannelMessage(channel, "Bot is unable to use message embeds on this channel.");
				return false;
			}

			return true;
		}
		//Set the server or mod log
		public static async Task<ITextChannel> setServerOrModLog(CommandContext context, string input, string serverOrMod)
		{
			return await setServerOrModLog(context.Guild, context.Channel, context.Message, input, serverOrMod);
		}
		
		//Set the server or mod log without context
		public static async Task<ITextChannel> setServerOrModLog(IGuild guild, IMessageChannel channel, IUserMessage message, string input, string serverOrMod)
		{
			ITextChannel logChannel = null;
			//See if not null
			if (String.IsNullOrWhiteSpace(input))
			{
				await makeAndDeleteSecondaryMessage(channel, message, ERROR("No channel specified."));
				return null;
			}
			else if (input.ToLower().Equals("off"))
			{
				return await setServerOrModLog(guild, channel, message, null as ITextChannel, serverOrMod);
			}

			//Get the channel with its ID
			logChannel = await getChannel(guild, channel, message, input) as ITextChannel;
			if (logChannel == null)
			{
				await makeAndDeleteSecondaryMessage(channel, message, ERROR(String.Format("Unable to set the logchannel on `{0}`.", input)));
				return null;
			}

			return await setServerOrModLog(guild, channel, message, logChannel, serverOrMod);
		}
		
		//Set the server and mod log with an already gotten channel
		public static async Task<ITextChannel> setServerOrModLog(IGuild guild, IMessageChannel channel, IUserMessage message, ITextChannel inputChannel, string serverOrMod)
		{
			//Create the file if it doesn't exist
			string path = getServerFilePath(guild.Id, Constants.SERVERLOG_AND_MODLOG);
			if (!File.Exists(path))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				var newFile = File.Create(path);
				newFile.Close();
			}

			//Find the lines that aren't the current serverlog line
			List<string> validLines = new List<string>();
			using (StreamReader reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (line.Contains(serverOrMod))
					{
						if ((inputChannel != null) && (line.Contains(inputChannel.Id.ToString())))
						{
							await makeAndDeleteSecondaryMessage(channel, message, "Channel is already the current " + serverOrMod + ".");
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
					await makeAndDeleteSecondaryMessage(channel, message, "Disabled the " + serverOrMod + ".");
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
		public static async Task imageLog(IMessageChannel channel, SocketMessage message, bool embeds)
		{
			//Get the links
			List<string> attachmentURLs = new List<string>();
			List<string> embedURLs = new List<string>();
			List<Embed> videoEmbeds = new List<Embed>();
			if (!embeds && message.Attachments.Any())
			{
				//If attachment, the file is hosted on discord which has a concrete URL name for files (cdn.discordapp.com/attachments/.../x.png)
				attachmentURLs = message.Attachments.Select(x => x.Url).ToList();
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
						else if (x.Image.HasValue && !String.IsNullOrEmpty(x.Image.Value.Url))
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
			IUser user = message.Author;
			foreach (string URL in attachmentURLs.Distinct())
			{
				if (Constants.VALIDIMAGEEXTENSIONS.Contains(Path.GetExtension(URL).ToLower()))
				{
					++Variables.LoggedImages;
					//Image attachment
					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.ATTACH, "Image", imageURL: URL), "Attached Image");
					Actions.addAuthor(embed, String.Format("{0}#{1} in #{2}", user.Username, user.Discriminator, message.Channel), user.AvatarUrl);
					await Actions.sendEmbedMessage(channel, embed);
				}
				else if (Constants.VALIDGIFEXTENTIONS.Contains(Path.GetExtension(URL).ToLower()))
				{
					++Variables.LoggedGifs;
					//Gif attachment
					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.ATTACH, "Gif", imageURL: URL), "Attached Gif");
					Actions.addAuthor(embed, String.Format("{0}#{1} in #{2}", user.Username, user.Discriminator, message.Channel), user.AvatarUrl);
					await Actions.sendEmbedMessage(channel, embed);
				}
				else
				{
					++Variables.LoggedFiles;
					//Random file attachment
					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.ATTACH, "File"), "Attached File");
					Actions.addAuthor(embed, String.Format("{0}#{1} in #{2}", user.Username, user.Discriminator, message.Channel), user.AvatarUrl);
					await Actions.sendEmbedMessage(channel, embed.WithDescription(URL));
				}
			}
			foreach (string URL in embedURLs.Distinct())
			{
				++Variables.LoggedImages;
				//Embed image
				EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.ATTACH, "Image", imageURL: URL), "Embedded Image");
				Actions.addAuthor(embed, String.Format("{0}#{1} in #{2}", user.Username, user.Discriminator, message.Channel), user.AvatarUrl);
				await Actions.sendEmbedMessage(channel, embed);
			}
			foreach (Embed embedObject in videoEmbeds.Distinct())
			{
				++Variables.LoggedGifs;
				//Check if video or gif
				string title = Constants.VALIDGIFEXTENTIONS.Contains(Path.GetExtension(embedObject.Thumbnail.Value.Url).ToLower()) ? "Gif" : "Video";

				EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.ATTACH, title, embedObject.Url, embedObject.Thumbnail.Value.Url), "Embedded " + title);
				Actions.addAuthor(embed, String.Format("{0}#{1} in #{2}", user.Username, user.Discriminator, message.Channel), user.AvatarUrl);
				await Actions.sendEmbedMessage(channel, embed);
			}
		}
		#endregion

		#region Preferences
		//Save preferences
		public static void savePreferences(TextWriter writer, ulong serverID)
		{
			//Test if the categories exist
			List<PreferenceCategory> categories;
			if (!Variables.CommandPreferences.TryGetValue(serverID, out categories))
			{
				return;
			}

			//If they exist, actually overwrite the new preferences file with the base preferences
			foreach (PreferenceCategory category in categories)
			{
				writer.WriteLine("@" + category.Name);
				foreach (PreferenceSetting setting in category.Settings)
				{
					writer.WriteLine(setting.mName + ":" + setting.asString());
				}
				writer.Write("\n");
			}
		}
		
		//Save preferences by server
		public static void savePreferences(ulong serverID)
		{
			string path = getServerFilePath(serverID, Constants.PREFERENCES_FILE);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			using (StreamWriter writer = new StreamWriter(path, false))
			{
				savePreferences(writer, serverID);
			}
		}
		
		//Remove the option to say yes for preferences after ten seconds
		public static void turnOffEnableYes(IGuild guild)
		{
			Task t = Task.Run(() =>
			{
				Thread.Sleep(10000);
				Variables.GuildsEnablingPreferences.Remove(guild);
			});
		}
		
		//Remove the option to say yes for preferences after ten seconds
		public static void turnOffDeleteYes(IGuild guild)
		{
			Task t = Task.Run(() =>
			{
				Thread.Sleep(10000);
				Variables.GuildsDeletingPreferences.Remove(guild);
			});
		}
		
		//Enable preferences
		public static async Task enablePreferences(IGuild guild, IUserMessage message)
		{
			//Set up the preferences file(s) location(s) on the computer
			string path = Actions.getServerFilePath(guild.Id, Constants.PREFERENCES_FILE);
			if (path == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(message.Channel, message, Actions.ERROR(Constants.PATH_ERROR));
				return;
			}
			if (!File.Exists(path))
			{
				Actions.savePreferences(guild.Id);
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(message.Channel, message, "Preferences are already turned on.");
				Variables.GuildsEnablingPreferences.Remove(guild);
				return;
			}
			//Create bot channel if not on the server
			ITextChannel channel = await logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
			if (channel == null)
			{
				channel = await guild.CreateTextChannelAsync(Variables.Bot_Channel);
				await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(readMessages: PermValue.Deny));
				await Actions.setServerOrModLog(guild, message.Channel, message, channel, Constants.SERVER_LOG_CHECK_STRING);
				await Actions.setServerOrModLog(guild, message.Channel, message, channel, Constants.MOD_LOG_CHECK_STRING);
			}
			else
			{
				channel = (await guild.GetTextChannelsAsync()).FirstOrDefault(x => x.Name == Variables.Bot_Channel);
			}

			//Remove them from the emable list
			Variables.GuildsEnablingPreferences.Remove(guild);

			//Send a success message
			await Actions.sendChannelMessage(message.Channel, "Successfully created the preferences for this guild.");
		}
		
		//Read out the preferences
		public static async Task readPreferences(IMessageChannel channel, string serverpath)
		{
			//Make the embed
			EmbedBuilder embed = Actions.makeNewEmbed(null, "Preferences");

			//Make the information into separate fields
			string[] text = File.ReadAllText(serverpath).Replace("@", "").Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

			//Get the category name and the commands in it
			foreach (string category in text)
			{
				string[] titleAndCommands = category.Split(new char[] { '\r' }, 2);
				string title = titleAndCommands[0];
				string commands = titleAndCommands[1].TrimStart('\n');

				//Add the field
				if (!String.IsNullOrEmpty(title) && !String.IsNullOrEmpty(commands))
				{
					Actions.addField(embed, title, commands, false);
				}
			}

			//Send the preferences message
			await Actions.sendEmbedMessage(channel, embed);
		}
		
		//Delete preferences
		public static async Task deletePreferences(IGuild guild, IUserMessage message)
		{
			//Check if valid path
			string path = Actions.getServerFilePath(guild.Id, Constants.PREFERENCES_FILE);
			if (path == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(message.Channel, message, Actions.ERROR(Constants.PATH_ERROR));
				return;
			}

			//Delete the preferences file
			File.Delete(path);

			//Remove them from the emable list
			Variables.GuildsDeletingPreferences.Remove(guild);

			//Send a success message
			await Actions.sendChannelMessage(message.Channel, "Successfully deleted the stored preferences for this guild.");
		}
		#endregion

		#region Slowmode
		//Slowmode
		public static async Task slowmode(SocketMessage message)
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
						slowmodeInterval(smUser);
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

		//Time interval for slowmode
		public static void slowmodeInterval(SlowmodeUser smUser)
		{
			Task t = Task.Run(() =>
			{
				//Sleep for the given amount of seconds
				Thread.Sleep(smUser.Time * 1000);
				//Add back their ability to send messages
				smUser.CurrentMessagesLeft = smUser.BaseMessages;
			});
		}

		//Add a new user who joined into the slowmode users list
		public static async Task slowmodeAddUser(SocketGuildUser user)
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
			(await user.Guild.GetTextChannelsAsync()).ToList().ForEach(x => guildChannelIDList.Add(x.Id));
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
		public static async Task bannedPhrases(SocketMessage message)
		{
			//Get the guild
			IGuild guild = (message.Channel as IGuildChannel).Guild;

			//Check if it has any banned words
			if (Variables.BannedPhrases.ContainsKey(guild.Id))
			{
				foreach (string phrase in Variables.BannedPhrases[guild.Id])
				{
					if (message.Content.IndexOf(phrase, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						await bannedPhrasesPunishments(message);
						return;
					}
				}
			}
			//Check if it has any banned regex
			if (Variables.BannedRegex.ContainsKey(guild.Id))
			{
				foreach (var regex in Variables.BannedRegex[guild.Id])
				{
					//See if any matches
					if (regex.IsMatch(message.Content))
					{
						await bannedPhrasesPunishments(message);
						return;
					}
				}
			}
		}

		//Banned phrase punishments on a user
		public static async Task bannedPhrasesPunishments(SocketMessage message)
		{
			//Get rid of the message
			await message.DeleteAsync();

			//Check if the guild has any punishments set up
			if (!Variables.BannedPhrasesPunishments.ContainsKey((message.Channel as IGuildChannel).GuildId))
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
			var punishments = Variables.BannedPhrasesPunishments[(message.Channel as IGuildChannel).GuildId];

			//Check if any punishments have the messages count which the user has
			if (!punishments.Any(x => x.Number_Of_Removes == bpUser.AmountOfRemovedMessages))
				return;

			//Grab the punishment with the same number
			BannedPhrasePunishment punishment = punishments.FirstOrDefault(x => x.Number_Of_Removes == bpUser.AmountOfRemovedMessages);

			//Kick
			if (punishment.Punishment == PunishmentType.Kick)
			{
				//Check if can kick them
				if (Actions.getPosition(user.Guild, user) > Actions.getPosition(user.Guild, await user.Guild.GetUserAsync(Variables.Bot_ID)))
					return;

				//Kick them
				await user.KickAsync();

				//Send a message to the logchannel
				ITextChannel logChannel = await Actions.logChannelCheck(user.Guild, Constants.SERVER_LOG_CHECK_STRING);
				if (logChannel != null)
				{
					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.LEAVE, description: "**ID:** " + user.Id.ToString()), "Banned Phrases Leave");
					await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl));
				}
			}
			//Ban
			else if (punishment.Punishment == PunishmentType.Ban)
			{
				//Check if can ban them
				if (Actions.getPosition(user.Guild, user) > Actions.getPosition(user.Guild, await user.Guild.GetUserAsync(Variables.Bot_ID)))
					return;

				//Ban them
				await user.Guild.AddBanAsync(message.Author);

				//Send a message to the logchannel
				ITextChannel logChannel = await Actions.logChannelCheck(user.Guild, Constants.SERVER_LOG_CHECK_STRING);
				if (logChannel != null)
				{
					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.BAN, description: "**ID:** " + user.Id.ToString()), "Banned Phrases Ban");
					await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl));
				}
			}
			//Role
			else
			{
				//Give them the role
				await Actions.giveRole(user, punishment.Role);

				//If a time is specified, run through the time then remove the role
				if (punishment.PunishmentTime != null)
				{
					bannedPhrasesPunishmentTimer(user, punishment.Role, (int)punishment.PunishmentTime);
				}

				//Send a message to the logchannel
				ITextChannel logChannel = await Actions.logChannelCheck(user.Guild, Constants.SERVER_LOG_CHECK_STRING);
				if (logChannel != null)
				{
					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.UEDIT, description: "**Gained:** " + punishment.Role.Name), "Banned Phrases Role");
					await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl));
				}
			}
		}

		//Wait them remove the role on a user when they got it from a banned phrase punishment
		public static void bannedPhrasesPunishmentTimer(IGuildUser user, IRole role, int time)
		{
			Task t = Task.Run(async () =>
			{
				//Sleep for the given amount of seconds
				Thread.Sleep(time * 60000);

				//Check if the user still has the role
				if (!user.RoleIds.Contains(role.Id))
					return;

				//Remove the role
				await user.RemoveRolesAsync(role);
			});
		}
		#endregion
	}
}