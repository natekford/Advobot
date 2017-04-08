using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
		#region Saving and Loading
		public static async Task LoadInformation()
		{
			Variables.Bot_ID = Variables.Client.GetCurrentUser().Id;				//Give the variable Bot_ID the id of the bot
			Variables.Bot_Name = Variables.Client.GetCurrentUser().Username;		//Give the variable Bot_Name the username of the bot

			LoadPermissionNames();													//Gets the names of the permission bits in Discord
			LoadCommandInformation();												//Gets the information of a command (name, aliases, usage, summary). Has to go after LPN
			Variables.HelpList.ForEach(x => Variables.CommandNames.Add(x.Name));	//Gets all the active command names. Has to go after LCI

			LoadGuilds();															//Loads the guilds that attempted to load before the Bot_ID was gotten.
			await SetGame();                                                        //Set up the game and/or stream

			HourTimer(null);														//Start the hourly timer
			MinuteTimer(null);														//Start the minutely timer
			OneFourthSecondTimer(null);												//Start the one fourth second timer
			StartUpMessages();														//Say all of the start up messages
			Variables.Loaded = true;												//Set a bool stating that everything is done loading.
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

			WriteLine("The current bot prefix is: " + Properties.Settings.Default.Prefix);
			WriteLine("Bot took " + String.Format("{0:n}", TimeSpan.FromTicks(DateTime.UtcNow.ToUniversalTime().Ticks - Variables.StartupTime.Ticks).TotalMilliseconds) + " milliseconds to load everything.");
		}

		public static void LoadBasicInformation()
		{
			//Checks if the OS is Windows or not
			GetOS();
			//Check if console or WPF app
			GetConsoleOrGUI();
		}

		public static void LoadCommandInformation()
		{
			foreach (var classType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsSubclassOf(typeof(ModuleBase))))
			{
				if (!Enum.TryParse(((NameAttribute)classType.GetCustomAttribute(typeof(NameAttribute)))?.Text, true, out CommandCategory category))
					continue;

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
						else
						{
							WriteLine("Command does not have a default enabled value set: " + name);
						}
					}
					//Add it to the helplist
					Variables.HelpList.Add(new HelpEntry(name, aliases, usage, basePerm, text, category, defaultEnabled));
				}
			}
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

		public static void LoadGuilds()
		{
			Variables.GuildsToBeLoaded.ForEach(x => LoadGuild(x));
		}

		public static void LoadGuild(IGuild guild)
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
			guildInfo.TurnLoadedOn();
			Task.Run(async () =>
			{
				//Get all of the invites and add their guildID, code, and current uses to the usage check list
				guildInfo.Invites.AddRange((await guild.GetInvitesAsync()).ToList().Select(x => new BotInvite(x.GuildId, x.Code, x.Uses)).ToList());
			});
			Variables.Guilds.Add(guild.Id, guildInfo);
		}

		public static BotGuildInfo LoadGuildInfo(IGuild guild)
		{
			BotGuildInfo guildInfo = null;

			var path = GetServerFilePath(guild.Id, Constants.GUILD_INFO_LOCATION);
			if (!File.Exists(path))
			{
				WriteLine(String.Format("The guild information for {0} does not exist.", FormatGuild(guild)));
				return guildInfo;
			}

			try
			{
				using (var reader = new StreamReader(path))
				{
					guildInfo = JsonConvert.DeserializeObject<BotGuildInfo>(reader.ReadToEnd());
				}
				WriteLine(String.Format("The guild information for {0} has successfully been loaded.", FormatGuild(guild)));
			}
			catch (Exception e)
			{
				ExceptionToConsole(String.Format("LoadGuildInfo for {0}", FormatGuild(guild)), e);
			}

			var cmds = guildInfo.CommandSettings.Select(x => x.Name).ToList();
			Variables.HelpList.Where(x => !CaseInsContains(cmds, x.Name)).ToList().ForEach(x => guildInfo.CommandSettings.Add(new CommandSwitch(x.Name, x.DefaultEnabled)));
			return guildInfo;
		}

		public static void SaveGuildInfo(BotGuildInfo guildInfo)
		{
			OverWriteFile(GetServerFilePath(guildInfo.GuildID, Constants.GUILD_INFO_LOCATION), Serialize(guildInfo));
		}

		public static string Serialize(dynamic obj)
		{
			return JsonConvert.SerializeObject(obj, Formatting.Indented);
		}

		public static void CreateFile(string path)
		{
			if (!File.Exists(path))
			{
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

		#region Gets
		public static async Task<IRole> GetRole(CommandContext context, string roleName)
		{
			roleName = roleName.Trim().Trim(new char[] { '<', '@', '&', '>' });

			if (UInt64.TryParse(roleName, out ulong roleID))
			{
				return context.Guild.GetRole(roleID);
			}

			var roles = context.Guild.Roles.Where(x => CaseInsEquals(x.Name, roleName)).ToList();
			if (roles.Count == 0)
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("No role has the name of `{0}`.", roleName)));
			}
			else if (roles.Count == 1)
			{
				return roles.First();
			}
			else if (roles.Count > 1)
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR("Multiple roles with the same name. Please specify by mentioning the role or changing their names."));
			}
			return null;
		}
		
		public static IRole GetRole(IGuild guild, string roleName)
		{
			//Trim it
			roleName = roleName.Trim();
			//Order them by position (puts everyone first) then reverse so it sorts from the top down
			return guild.Roles.ToList().OrderBy(x => x.Position).Reverse().FirstOrDefault(x => CaseInsEquals(x.Name, roleName));
		}
		
		public static int GetPosition(IGuild guild, IUser user)
		{
			//Make sure they're an IGuildUser
			var tempUser = user as IGuildUser;
			if (user == null)
				return -1;

			//Check if the user is the owner
			if (user.Id == guild.OwnerId)
				return Constants.OWNER_POSITION;

			//Check if any roles
			var roleIDs = tempUser.RoleIds;
			if (!roleIDs.Any())
				return -1;

			//Get the position off of their roles
			return roleIDs.Max(x => guild.GetRole(x).Position);
		}
		
		public static async Task<IGuildUser> GetUser(IGuild guild, string userName)
		{
			return userName == null ? null : await guild.GetUserAsync(GetUlong(userName.Trim(new char[] { '<', '>', '@', '!' })));
		}

		public static async Task<IGuildUser> GetUser(IGuild guild, ulong ID)
		{
			return await guild.GetUserAsync(ID);
		}
		
		public static ulong GetUlong(string inputString)
		{
			return UInt64.TryParse(inputString, out ulong number) ? number : 0;
		}

		public static async Task<IGuildUser> GetBot(IGuild guild)
		{
			return await guild.GetCurrentUserAsync();
		}
		
		public static async Task<IRole> GetRoleEditAbility(CommandContext context, string input = null, bool ignore_Errors = false, IRole role = null)
		{
			//Check if valid role
			var inputRole = role ?? await GetRole(context, input);
			if (inputRole == null)
				return null;

			if (inputRole.Position >= GetPosition(context.Guild, context.User))
			{
				if (!ignore_Errors)
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("`{0}` has a higher position than you are allowed to edit or use.", inputRole.Name)));
				}
			}
			else if (inputRole.Position >= GetPosition(context.Guild, await context.Guild.GetUserAsync(Variables.Bot_ID)))
			{
				if (!ignore_Errors)
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("`{0}` has a higher position than the bot is allowed to edit or use.", inputRole.Name)));
				}
			}
			else
			{
				return inputRole;
			}
			return null;
		}

		public static IVoiceChannel GetUserMovability(IVoiceChannel channel, IUser user)
		{
			var guildUser = user as IGuildUser;
			if (guildUser == null)
				return null;
			else if (guildUser.GuildPermissions.Administrator)
				return channel;

			var perms = guildUser.GetPermissions(channel);
			if ((perms.Connect && perms.MoveMembers) || (perms.ManagePermissions && perms.ManageChannel))
				return channel;
			return null;
		}
		
		public static dynamic GetChannelPermability(IGuildChannel channel, IUser user)
		{
			var guildUser = user as IGuildUser;
			if (guildUser == null)
				return null;
			else if (guildUser.GuildPermissions.Administrator)
				return channel;

			var perms = guildUser.GetPermissions(channel);
			if (channel is ITextChannel && perms.ReadMessages && perms.ManagePermissions && perms.ManageChannel)
				return channel as ITextChannel;
			else if (channel is IVoiceChannel && perms.ManagePermissions && perms.ManageChannel)
				return channel as IVoiceChannel;
			return null;
		}

		public static dynamic GetChannelManagability(IGuildChannel channel, IUser user)
		{
			var guildUser = user as IGuildUser;
			if (guildUser == null)
				return null;
			else if (guildUser.GuildPermissions.Administrator)
				return channel;

			var perms = guildUser.GetPermissions(channel);
			if (channel is ITextChannel && perms.ReadMessages && perms.ManageChannel)
				return channel as ITextChannel;
			else if (channel is IVoiceChannel && perms.ManageChannel)
				return channel as IVoiceChannel;
			return null;
		}

		public static dynamic GetChannelMovability(IGuildChannel channel, IUser user)
		{
			var guildUser = user as IGuildUser;
			if (guildUser == null)
				return null;

			var perms = guildUser.GuildPermissions;
			if (perms.Administrator)
				return channel;
			else if (channel is ITextChannel && guildUser.GetPermissions(channel).ReadMessages && perms.ManageChannels)
				return channel as ITextChannel;
			else if (channel is IVoiceChannel && perms.ManageChannels)
				return channel as IVoiceChannel;
			return null;
		}

		public static async Task<ReturnedChannel> GetChannelPermability(CommandContext context, string input, IGuildChannel channel = null)
		{
			channel = channel ?? await GetChannel(context, input);
			if (channel == null)
			{
				return new ReturnedChannel(null, FailureReason.Not_Found);
			}
			else if (GetChannelPermability(channel, context.User) == null)
			{
				return new ReturnedChannel(channel, FailureReason.User_Inability);
			}
			else if (GetChannelPermability(channel, await GetUser(context.Guild, Variables.Bot_ID)) == null)
			{
				return new ReturnedChannel(channel, FailureReason.Bot_Inability);
			}
			else
			{
				return new ReturnedChannel(channel, FailureReason.Not_Failure);
			}
		}

		public static async Task<ReturnedChannel> GetChannelManagability(CommandContext context, string input, IGuildChannel channel = null)
		{
			channel = channel ?? await GetChannel(context, input);
			if (channel == null)
			{
				return new ReturnedChannel(null, FailureReason.Not_Found);
			}
			else if (GetChannelManagability(channel, context.User) == null)
			{
				return new ReturnedChannel(channel, FailureReason.User_Inability);
			}
			else if (GetChannelManagability(channel, await GetUser(context.Guild, Variables.Bot_ID)) == null)
			{
				return new ReturnedChannel(channel, FailureReason.Bot_Inability);
			}
			else
			{
				return new ReturnedChannel(channel, FailureReason.Not_Failure);
			}
		}

		public static async Task<ReturnedChannel> GetChannelMovability(CommandContext context, string input, IGuildChannel channel = null)
		{
			channel = channel ?? await GetChannel(context, input);
			if (channel == null)
			{
				return new ReturnedChannel(null, FailureReason.Not_Found);
			}
			else if (GetChannelMovability(channel, context.User) == null)
			{
				return new ReturnedChannel(channel, FailureReason.User_Inability);
			}
			else if (GetChannelMovability(channel, await GetUser(context.Guild, Variables.Bot_ID)) == null)
			{
				return new ReturnedChannel(channel, FailureReason.Bot_Inability);
			}
			else
			{
				return new ReturnedChannel(channel, FailureReason.Not_Failure);
			}
		}

		public static async Task HandleChannelPermsLacked(CommandContext context, ReturnedChannel channel)
		{
			switch (channel.Reason)
			{
				case FailureReason.Not_Found:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR("Unable to find the channel."));
					break;
				}
				case FailureReason.User_Inability:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("You are unable to make the given changes to the channel: `{0}`.", FormatChannel(channel.Channel))));
					break;
				}
				case FailureReason.Bot_Inability:
				{
					await MakeAndDeleteSecondaryMessage(context, ERROR(String.Format("I am unable to make the given changes to the channel: `{0}`.", FormatChannel(channel.Channel))));
					break;
				}
			}
		}
		
		public static async Task<IGuildChannel> GetChannel(CommandContext context, string input)
		{
			return await GetChannel(context.Guild, context.Channel, context.Message, input);
		}
		
		public static async Task<IGuildChannel> GetChannel(IGuild guild, IMessageChannel channel, IUserMessage message, string input)
		{
			//Go off of mentions for text channel
			if (String.IsNullOrWhiteSpace(input) && message.MentionedChannelIds.Any())
			{
				return await guild.GetChannelAsync(message.MentionedChannelIds.FirstOrDefault());
			}
			//If input is given
			else if (ulong.TryParse(input.Trim('<', '#', '>'), out ulong ID))
			{
				return await guild.GetChannelAsync(ID);
			}
			//If name contains a space then it's a voice channel
			else
			{
				//See which match the name and type given
				var channels = (await guild.GetVoiceChannelsAsync()).Where(x => CaseInsEquals(x.Name, input)).ToList();

				//If zero then no channels have the name so return an error message
				if (channels.Count < 1)
				{
					await MakeAndDeleteSecondaryMessage(channel, message, ERROR(String.Format("`{0}` does not exist as a channel on this guild.", input)));
				}
				//If only one then return that channel
				if (channels.Count == 1)
				{
					return channels.FirstOrDefault();
				}
				//If more than one return an error message too because how are we supposed to know which one they want?
				else if (channels.Count > 1)
				{
					await MakeAndDeleteSecondaryMessage(channel, message, ERROR(String.Format("More than one channel exists with the name `{0}`.", input)));
				}
			}

			return null;
		}
		
		public static int GetInteger(string inputString)
		{
			return Int32.TryParse(inputString, out int number) ? number : -1;
		}
		
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
		
		public static Dictionary<String, String> GetTextChannelPermissions(Dictionary<String, String> dictionary)
		{
			Variables.ChannelPermissions.Where(x => x.Voice).ToList().ForEach(x => dictionary.Remove(x.Name));
			return dictionary;
		}
		
		public static Dictionary<String, String> GetVoiceChannelPermissions(Dictionary<String, String> dictionary)
		{
			Variables.ChannelPermissions.Where(x => x.Text).ToList().ForEach(x => dictionary.Remove(x.Name));
			return dictionary;
		}
		
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
		
		public static string[] GetCommands(IGuild guild, int number)
		{
			if (!Variables.Guilds.ContainsKey(guild.Id))
				return null;

			return Variables.Guilds[guild.Id].CommandSettings.Where(x => x.CategoryValue == number).Select(x => x.Name).ToArray();
		}
		
		public static string GetServerFilePath(ulong guildId, string fileName)
		{
			//Make sure the bot's directory exists
			var directory = GetDirectory();
			if (!Directory.Exists(directory))
				return null;

			//This string will be similar to C:\Users\User\AppData\Roaming\Discord_Servers_... if on using appdata. If not then it can be anything
			return Path.Combine(directory, guildId.ToString(), fileName);
		}

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
		
		public static string GetChannelType(IGuildChannel channel)
		{
			return CaseInsIndexOf(channel.GetType().Name, Constants.TEXT_TYPE) ? Constants.TEXT_TYPE : Constants.VOICE_TYPE;
		}

		public static ITextChannel GetServerLogChannel(IGuild guild)
		{
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return null;

			return guildInfo.ServerLog;
		}

		public static ITextChannel GetModLogChannel(IGuild guild)
		{
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return null;

			return guildInfo.ModLog;
		}
		
		public static async Task<bool> GetIfUserIsOwner(IGuild guild, IUser user)
		{
			if (guild == null || user == null)
				return false;

			//This is a .GetOwnerAsync instead of .OwnerID simply so it can have an await in the methot to make it async
			return (await guild.GetOwnerAsync()).Id == user.Id || GetIfUserIsOwnerButBotIsOwner(guild, user);
		}

		public static bool GetIfUserIsOwnerButBotIsOwner(IGuild guild, IUser user)
		{
			return guild.OwnerId == Variables.Bot_ID && GetPosition(guild, user) == guild.Roles.Max(x => x.Position) - 1;
		}

		public static bool GetIfUserIsBotOwner(IUser user)
		{
			return user.Id == Properties.Settings.Default.BotOwner;
		}

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

		public static string[] SplitByCharExceptInQuotes(string inputString, char inputChar)
		{
			return inputString.Split('"').Select((element, index) =>
			{
				if (index % 2 == 0)
				{
					return element.Split(new[] { inputChar }, StringSplitOptions.RemoveEmptyEntries);
				}
				else
				{
					return new string[] { element };
				}
			}).SelectMany(element => element).ToArray();
		}

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

		public static string GetVariable(string[] inputArray, string searchTerm)
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

		public static IGuildUser GetBotOwner(BotClient client)
		{
			return client.GetGuilds().SelectMany(x => x.Users).FirstOrDefault(x => x.Id == Properties.Settings.Default.BotOwner);
		}

		public static async Task<int> GetIfGroupIsValid(CommandContext context, string input)
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

		public static List<CommandSwitch> GetMultipleCommands(ulong id, CommandCategory category)
		{
			return Variables.Guilds[id].CommandSettings.Where(x => x.CategoryEnum == category).ToList();
		}

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
				return await Command_Handler.Commands.ExecuteAsync(context, argPos, map);
			}
		}

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
					else if (getCheckString && CaseInsIndexOf(line, checkString))
					{
						validLines.Add(line);
					}
				}
			}
			return validLines;
		}

		public static string GetHelpString(HelpEntry help)
		{
			return String.Format("**Aliases:** {0}\n**Usage:** {1}\n\n**Base Permission(s):**\n{2}\n\n**Description:**\n{3}",
				String.Join(", ", help.Aliases),
				help.Usage,
				help.BasePerm,
				help.Text);
		}

		public static int GetLineBreaks(string input)
		{
			return input.Count(y => y == '\n' || y == '\r');
		}

		public static bool UserCanBeModifiedByUser(CommandContext context, IGuildUser user)
		{
			var bannerPosition = GetPosition(context.Guild, context.User);
			var banneePosition = user == null ? -1 : GetPosition(context.Guild, user);
			return bannerPosition > banneePosition;
		}

		public static bool UserCanBeModifiedByBot(IGuild guild, IGuildUser targetUser, IGuildUser bot)
		{
			var botPosition = GetPosition(guild, bot);
			var userPosition = GetPosition(guild, targetUser);
			return botPosition > userPosition  || targetUser.Id == bot.Id;
		}

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
				if (CaseInsContains(guild.Features.ToList(), Constants.VANITY_URL) && (newInvs.Count() == 0 || (newInvs.Count() == 1 && newInvs.First().Uses == 0)))
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

		public static async Task<GuildNotification> GetGuildNotification(CommandContext context, string input)
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
			var channel = await GetChannel(context, channelStr);
			if (channel == null)
				return null;
			var tChannel = channel as ITextChannel;
			if (tChannel == null)
			{
				await MakeAndDeleteSecondaryMessage(context, ERROR("The welcome channel can only be set to a text channel."));
				return null;
			}

			return new GuildNotification(content, title, desc, thumb, context.Guild.Id, channel.Id);
		}
		#endregion

		#region Roles
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
		
		public static async Task GiveRole(IGuildUser user, IRole role)
		{
			if (role == null)
				return;
			if (user.RoleIds.Contains(role.Id))
				return;
			await user.AddRoleAsync(role);
		}
		
		public static async Task GiveRole(IGuildUser user, IRole[] roles)
		{
			await user.AddRolesAsync(roles);
		}
		
		public static async Task TakeRole(IGuildUser user, IRole[] roles)
		{
			if (roles.Count() == 0)
				return;
			await user.RemoveRolesAsync(roles);
		}
		
		public static async Task TakeRole(IGuildUser user, IRole role)
		{
			if (role == null)
				return;
			await user.RemoveRoleAsync(role);
		}
		#endregion

		#region Messages
		public static void WriteLine(string text)
		{
			Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + ReplaceMarkdownChars(text));
		}

		public static void ExceptionToConsole(string method, Exception e)
		{
			if (e == null)
				return;

			WriteLine(method + " EXCEPTION: " + e.ToString());
		}

		public static void WriteLoadDone(IGuild guild, string method, string name)
		{
			Variables.Guilds[guild.Id].TurnDefaultPrefsOff();
			WriteLine(String.Format("{0}: {1} for the guild {2} have been loaded.", method, name, FormatGuild(guild)));
		}

		public static async Task MakeAndDeleteSecondaryMessage(CommandContext context, string secondStr, Int32 time = Constants.WAIT_TIME)
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

		public static async Task RemoveMessages(IMessageChannel channel, int requestCount)
		{
			var guildChannel = channel as ITextChannel;
			if (guildChannel == null)
				return;

			while (requestCount > 0)
			{
				//Get the current messages and ones that aren't null
				var newNum = Math.Min(requestCount, 100);
				var messages = (await channel.GetMessagesAsync(newNum).ToList()).SelectMany(x => x).Where(x => x != null).ToList();
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
					break;
				}

				//Leave if the message count gathered implies that the channel is out of messages
				if (messages.Count < newNum)
					break;

				//Lower the request count
				requestCount -= messages.Count;
			}
		}
		
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
				var newNum = Math.Min(requestCount, 100);
				var messages = (await channel.GetMessagesAsync(newNum).ToList()).SelectMany(x => x).Where(x => x.Author == user && x != null).ToList();
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
					break;
				}

				//Leave if the message count gathered implies that the channel is out of messages
				if (messages.Count < newNum)
					break;

				//Lower the request count
				requestCount -= messages.Count;
			}
		}

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

		public static string ERROR(string message)
		{
			return Constants.ZERO_LENGTH_CHAR + Constants.ERROR_MESSAGE + message;
		}
		
		public static async Task<IMessage> SendChannelMessage(CommandContext context, string message)
		{
			if (context.Channel == null || !Variables.Guilds.ContainsKey(context.Guild.Id))
				return null;

			return await context.Channel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + message);
		}

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
		
		public static async Task FormatEditMessage(ITextChannel logChannel, string time, IGuildUser user, IMessageChannel channel, string before, string after)
		{
			await SendChannelMessage(logChannel, String.Format("{0} **EDIT:** `{1}` **IN** `#{2}`\n**FROM:** ```\n{3}```\n**TO:** ```\n{4}```", time, FormatUser(user), FormatChannel(channel), before, after));
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

		public static string FormatGuild(IGuild guild)
		{
			return String.Format("'{0}' ({1})", guild.Name, guild.Id);
		}

		public static string FormatUser(IUser user)
		{
			return String.Format("'{0}#{1}' ({2})", String.IsNullOrWhiteSpace(user.Username) ? "Irretrievable" : user.Username, user.Discriminator, user.Id);
		}

		public static string FormatChannel(IChannel channel)
		{
			var tempChan = channel as IGuildChannel;
			if (tempChan == null)
				return "Unable to get channel data.";
			return String.Format("'{0}' ({1}) ({2})", channel.Name, GetChannelType(tempChan), channel.Id);
		}

		public static string FormatRole(IRole role)
		{
			return String.Format("'{0}' ({1})", role.Name, role.Id);
		}

		public static string RemoveNewLines(string input)
		{
			return input.Replace(Environment.NewLine, "").Replace("\r", "").Replace("\n", "");
		}

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

		public static async Task SendDeleteMessage(IGuild guild, ITextChannel channel, List<string> inputList)
		{
			//Get the character count
			int characterCount = 0;
			inputList.ForEach(x => characterCount += (x.Length + 100));

			if (inputList.Count == 0)
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

		public static async Task<IMessage> SendEmbedMessage(IMessageChannel channel, EmbedBuilder embed, string content = null)
		{
			var guildChannel = channel as ITextChannel;
			if (guildChannel == null)
				return null;
			var guild = guildChannel.Guild;
			if (guild == null || !Variables.Guilds.ContainsKey(guild.Id))
				return null;

			content = content ?? "";

			//Replace all instances of the base prefix with the guild's prefix
			var guildPrefix = Variables.Guilds[guild.Id].Prefix;
			if (!String.IsNullOrWhiteSpace(guildPrefix))
			{
				embed.Description.Replace(Properties.Settings.Default.Prefix, guildPrefix);
			}

			try
			{
				//Generate the message
				return await guildChannel.SendMessageAsync(Constants.ZERO_LENGTH_CHAR + content, embed: embed);
			}
			//Embeds fail every now and then and I haven't been able to find the exact problem yet (I know fields are a problem, but not in this case)
			catch (Exception e)
			{
				ExceptionToConsole(MethodBase.GetCurrentMethod().Name, e);
				return null;
			}
		}
		
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
		
		public static EmbedBuilder AddFooter(EmbedBuilder embed, string text = null, string iconURL = null)
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

		public static async Task SendGuildNotification(IUser user, GuildNotification notification)
		{
			if (notification == null)
				return;

			var userMention = user != null ? user.Mention : "Invalid User";
			var content = notification.Content.Replace("@User", userMention);

			if (notification.Embed != null)
			{
				await SendEmbedMessage(notification.Channel, notification.Embed, content);
			}
			else
			{
				await SendChannelMessage(notification.Channel, content);
			}
		}
		#endregion

		#region Uploads
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

		public static async Task WriteAndUploadTextFile(IGuild guild, IMessageChannel channel, List<string> textList, string fileName, string messageHeader)
		{
			//Messages in the format to upload
			var text = ReplaceMarkdownChars(String.Join("\n-----\n", textList));
			await WriteAndUploadTextFile(guild, channel, text, fileName, messageHeader);
		}

		public static async Task WriteAndUploadTextFile(IGuild guild, IMessageChannel channel, string text, string fileName, string fileMessage = null)
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

		public static async Task UploadFile(IMessageChannel channel, string path, string text = null)
		{
			await channel.SendFileAsync(path, text);
		}

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

		public static bool ValidateURL(string input)
		{
			if (input == null)
				return false;

			return Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
		}
		#endregion

		#region Server/Mod Log
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

		public static IGuild VerifyLoggingAction(IGuild guild, LogActions logAction)
		{
			//If the guild is null send back null. If the logaction being tested isn't turned on send back null.
			return guild != null && Variables.Guilds[guild.Id].LogActions.Contains(logAction) ? guild : null;
		}

		public static IGuild VerifyGuild(IMessage message, LogActions logAction)
		{
			//Make sure the message wasn't sent by another bot, that channel isn't ignored, the logged action is turned on, and that the bot isn't paused
			return VerifyUnpaused(VerifyLoggingAction(VerifyLoggingIsEnabledOnThisChannel(VerifyMessage(message)), logAction));
		}

		public static IGuild VerifyGuild(IUser user, LogActions logAction)
		{
			return VerifyUnpaused(VerifyLoggingAction(GetGuildFromUser(user), logAction));
		}

		public static IGuild VerifyGuild(IGuild guild, LogActions logAction)
		{
			return VerifyUnpaused(VerifyLoggingAction(guild, logAction));
		}

		public static IGuild VerifyGuild(IGuildChannel channel, LogActions logAction)
		{
			return VerifyUnpaused(VerifyLoggingAction(GetGuildFromChannel(channel), logAction));
		}

		public static IGuild VerifyGuild(IRole role, LogActions logAction)
		{
			return VerifyUnpaused(VerifyLoggingAction(GetGuildFromRole(role), logAction));
		}

		public static IGuild VerifyUnpaused(IGuild guild)
		{
			return Variables.Pause ? null : guild;
		}
		#endregion

		#region Preferences/Settings
		public static async Task EnablePreferences(BotGuildInfo guildInfo, IGuild guild, IUserMessage message)
		{
			var path = GetServerFilePath(guild.Id, Constants.GUILD_INFO_LOCATION);
			if (path == null)
			{
				await MakeAndDeleteSecondaryMessage(message.Channel, message, ERROR(Constants.PATH_ERROR));
				return;
			}
			if (!File.Exists(path))
			{
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

		#region Slowmode/Banned Phrases/Spam Prevention
		public static async Task Slowmode(IMessage message)
		{
			//Get the guild and its info
			var guild = GetGuildFromMessage(message);
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return;

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
						Variables.SlowmodeUsers.Add(smUser);
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

		public static async Task BannedPhrases(IMessage message)
		{
			//Get the guild
			var guild = GetGuildFromMessage(message);
			if (guild == null)
				return;

			//Get the guild's bot data
			var bannedPhrases = Variables.Guilds[guild.Id].BannedPhrases;

			//Check if it has any banned words or regex
			var phrase = bannedPhrases.Strings.FirstOrDefault(x => CaseInsIndexOf(message.Content, x.Phrase));
			if (phrase != null)
			{
				await BannedStringPunishments(message, phrase);
			}
			var regex = bannedPhrases.Regex.FirstOrDefault(x => x.Phrase.IsMatch(message.Content));
			if (regex != null)
			{
				await BannedRegexPunishments(message, regex);
			}
		}

		public static async Task BannedStringPunishments(IMessage message, BannedPhrase<string> phrase)
		{
			await DeleteMessage(message);
			var guild = (message.Channel as IGuildChannel)?.Guild;
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return;
			var user = message.Author as IGuildUser;
			var bpUser = guildInfo.BannedPhraseUsers.FirstOrDefault(x => x.User == user) ?? new BannedPhraseUser(user);

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
					if (GetPosition(user.Guild, user) > GetPosition(user.Guild, await user.Guild.GetUserAsync(Variables.Bot_ID)))
						return;

					//Kick them
					await user.KickAsync();
					bpUser.ResetKickCount();

					//Send a message to the logchannel
					var logChannel = GetServerLogChannel(guild);
					if (logChannel != null)
					{
						var embed = AddFooter(MakeNewEmbed(null, "**ID:** " + user.Id, Constants.LEAV), "Banned Phrases Leave");
						await SendEmbedMessage(logChannel, AddAuthor(embed, String.Format("{0} in #{1}", FormatUser(user), message.Channel), user.GetAvatarUrl()));
					}
					break;
				}
				case PunishmentType.Ban:
				{
					//Check if can ban them
					if (GetPosition(user.Guild, user) > GetPosition(user.Guild, await user.Guild.GetUserAsync(Variables.Bot_ID)))
						return;

					//Ban them
					await user.Guild.AddBanAsync(user);
					bpUser.ResetBanCount();

					//Send a message to the logchannel
					var logChannel = GetServerLogChannel(guild);
					if (logChannel != null)
					{
						var embed = AddFooter(MakeNewEmbed(null, "**ID:** " + user.Id, Constants.BANN), "Banned Phrases Ban");
						await SendEmbedMessage(logChannel, AddAuthor(embed, FormatUser(user), user.GetAvatarUrl()));
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
						Variables.PunishedUsers.Add(new RemovablePunishment(guild, user, punishment.Role, DateTime.UtcNow.AddMinutes((int)punishment.PunishmentTime)));
					}

					//Send a message to the logchannel
					var logChannel = GetServerLogChannel(guild);
					if (logChannel != null)
					{
						var embed = AddFooter(MakeNewEmbed(null, "**Role Gained:** " + punishment.Role.Name, Constants.UEDT), "Banned Phrases Role");
						await SendEmbedMessage(logChannel, AddAuthor(embed, FormatUser(user), user.GetAvatarUrl()));
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

		public static async Task BannedRegexPunishments(IMessage message, BannedPhrase<Regex> regex)
		{
			await DeleteMessage(message);
			var guild = (message.Channel as IGuildChannel)?.Guild;
			if (guild == null)
				return;
			if (!Variables.Guilds.TryGetValue(guild.Id, out BotGuildInfo guildInfo))
				return;
			var user = message.Author as IGuildUser;
			var bpUser = guildInfo.BannedPhraseUsers.FirstOrDefault(x => x.User == user) ?? new BannedPhraseUser(user);

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
					if (GetPosition(user.Guild, user) > GetPosition(user.Guild, await user.Guild.GetUserAsync(Variables.Bot_ID)))
						return;

					//Kick them
					await user.KickAsync();

					//Send a message to the logchannel
					var logChannel = GetServerLogChannel(guild);
					if (logChannel != null)
					{
						var embed = AddFooter(MakeNewEmbed(null, "**ID:** " + user.Id, Constants.LEAV), "Banned Phrases Leave");
						await SendEmbedMessage(logChannel, AddAuthor(embed, String.Format("{0} in #{1}", FormatUser(user), message.Channel), user.GetAvatarUrl()));
					}
					break;
				}
				case PunishmentType.Ban:
				{
					//Check if can ban them
					if (GetPosition(user.Guild, user) > GetPosition(user.Guild, await user.Guild.GetUserAsync(Variables.Bot_ID)))
						return;

					//Ban them
					await user.Guild.AddBanAsync(message.Author);

					//Send a message to the logchannel
					var logChannel = GetServerLogChannel(guild);
					if (logChannel != null)
					{
						var embed = AddFooter(MakeNewEmbed(null, "**ID:** " + user.Id, Constants.BANN), "Banned Phrases Ban");
						await SendEmbedMessage(logChannel, AddAuthor(embed, FormatUser(user), user.GetAvatarUrl()));
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
						Variables.PunishedUsers.Add(new RemovablePunishment(guild, user, punishment.Role, DateTime.UtcNow.AddMinutes((int)punishment.PunishmentTime)));
					}

					//Send a message to the logchannel
					var logChannel = GetServerLogChannel(guild);
					if (logChannel != null)
					{
						var embed = AddFooter(MakeNewEmbed(null, "**Gained:** " + punishment.Role.Name, Constants.UEDT), "Banned Phrases Role");
						await SendEmbedMessage(logChannel, AddAuthor(embed, FormatUser(user), user.GetAvatarUrl()));
					}
					break;
				}
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

		public static bool TryGetBannedRegex(BotGuildInfo guildInfo, string searchPhrase, out BannedPhrase<Regex> bannedRegex)
		{
			bannedRegex = guildInfo.BannedPhrases.Regex.FirstOrDefault(x => CaseInsEquals(x.Phrase.ToString(), searchPhrase));
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

		public static async Task<bool> HandleSpamPrevention(GlobalSpamPrevention global, BaseSpamPrevention spamPrev, IGuild guild, IGuildUser user, IMessage msg)
		{
			if (spamPrev == null || !spamPrev.Enabled)
				return false;

			//Get the user from the list or, if not found, create a new one
			var spUser = Variables.Guilds[guild.Id].GlobalSpamPrevention.SpamPreventionUsers.FirstOrDefault(x => x.User == user) ?? new SpamPreventionUser(global, user);
			//Add one to the count of the spam type they triggered and check if the user should be kicked/banned
			await spUser.CheckIfShouldKick(spamPrev, msg);
			return true;
		}

		public static async Task VotesHigherThanRequiredAmount(BaseSpamPrevention spamPrev, SpamPreventionUser spUser, IMessage msg)
		{
			//Make sure they have the lowest vote count required to kick
			spUser.ChangeVotesRequired(spamPrev.VotesNeededForKick);
			//Turn on their ability to be kicked so they can be kicked
			spUser.EnablePotentialKick();
			//Send this message updating the amount of votes the user needs
			await MakeAndDeleteSecondaryMessage(msg.Channel, String.Format("The user `{0}` needs `{1}` votes to be kicked. Vote to kick them by mentioning them.",
				FormatUser(msg.Author), spUser.VotesRequired - spUser.VotesToKick));
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

		#region Close Words
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
		#endregion

		#region Timers
		public static void RemoveCommandMessages(List<IMessage> messages, Int32 time)
		{
			Variables.TimedMessages.Add(new RemovableMessage(messages, DateTime.UtcNow.AddMilliseconds(time)));
		}

		public static void RemoveCommandMessage(IMessage message, Int32 time)
		{
			Variables.TimedMessages.Add(new RemovableMessage(message, DateTime.UtcNow.AddMilliseconds(time)));
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

		public static bool CaseInsContains(List<string> list, string str)
		{
			if (!list.Any())
			{
				return false;
			}
			else
			{
				return list.Contains(str, StringComparer.OrdinalIgnoreCase);
			}
		}

		public static bool CaseInsContains(string[] array, string str)
		{
			if (!array.Any())
			{
				return false;
			}
			else
			{
				return array.Contains(str, StringComparer.OrdinalIgnoreCase);
			}
		}

		public static bool CaseInsContains(ReadOnlyCollection<string> readonlycollection, string str)
		{
			if (!readonlycollection.Any())
			{
				return false;
			}
			else
			{
				return readonlycollection.Contains(str, StringComparer.OrdinalIgnoreCase);
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
		public static async Task ModifyChannelPosition(IGuildChannel channel, int position)
		{
			if (channel == null)
				return;

			//Get all the channels that aren't the input channel
			var channels = GetChannelType(channel).Equals(Constants.TEXT_TYPE)
				? (await channel.Guild.GetTextChannelsAsync()).Where(x => x != channel).OrderBy(x => x.Position).Cast<IGuildChannel>().ToList()
				: (await channel.Guild.GetVoiceChannelsAsync()).Where(x => x != channel).OrderBy(x => x.Position).Cast<IGuildChannel>().ToList();
			//Add the input channel into the given spot
			channels.Insert(Math.Max(Math.Min(channels.Count(), position), 0), channel);
			//Convert into reorder properties and use to reorder
			await channel.Guild.ReorderChannelsAsync(channels.Select(x => new ReorderChannelProperties(x.Id, channels.IndexOf(x))));
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

		public static async Task ChangeNickname(IGuildUser user, string newNN)
		{
			await user.ModifyAsync(x => x.Nickname = newNN ?? user.Username);
		}

		public static void ClearPunishedUsersList()
		{
			Variables.Guilds.ToList().ForEach(x => x.Value.GlobalSpamPrevention.SpamPreventionUsers.Clear());
		}

		public static void RemovePunishments()
		{
			var eligibleToLosePunishment = Variables.PunishedUsers.Where(x => x.Time <= DateTime.UtcNow).ToList();
			//The reason this is not a foreachasync is 1) it doesn't work well with a timer and 2) the results of this are unimportant
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
			var eligibleToBeDeleted = Variables.TimedMessages.Where(x => x.Time <= DateTime.UtcNow).ToList();
			//The reason this is not a foreachasync is 1) it doesn't work well with a timer and 2) the results of this are unimportant
			eligibleToBeDeleted.ForEach(async timed =>
			{
				Variables.TimedMessages.Remove(timed);

				//Delete a single message
				if (timed.Message != null)
				{
					await DeleteMessage(timed.Message);
				}
				//Multiple
				else
				{
					await DeleteMessages(timed.Messages.FirstOrDefault().Channel, timed.Messages);
				}
			});
		}

		public static void RemoveActiveCloseHelpAndWords()
		{
			var inactiveHelp = Variables.ActiveCloseHelp.Where(x => x.DeleteTime <= DateTime.UtcNow).ToList();
			inactiveHelp.ForEach(x =>
			{
				Variables.ActiveCloseHelp.Remove(x);
			});
			var inactiveWords = Variables.ActiveCloseWords.Where(x => x.DeleteTime <= DateTime.UtcNow).ToList();
			inactiveWords.ForEach(x =>
			{
				Variables.ActiveCloseWords.Remove(x);
			});
		}

		public static void ActivateGuildToggles()
		{
			var eligibleToBeToggled = Variables.GuildToggles.Where(x => x.Time <= DateTime.UtcNow).ToList();
			eligibleToBeToggled.ForEach(x =>
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
			var eligibleForReset = Variables.SlowmodeUsers.Where(x => x.Time <= DateTime.UtcNow).ToList();
			eligibleForReset.ForEach(x =>
			{
				Variables.SlowmodeUsers.Remove(x);
				x.ResetMessagesLeft();
			});
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