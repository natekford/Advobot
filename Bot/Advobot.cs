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

namespace Advobot
{
	class MyBot
	{
		public MyBot()
		{

			Client mDiscord = new DiscordSocketClient(config);
			{
				x.LogLevel = LogSeverity.Info;
				x.LogHandler = Log;
			});

			mDiscord.UsingCommands(x =>
			{
				x.PrefixChar = '>';
				x.AllowMentionPrefix = true;
			});

			mCommands = mDiscord.GetService<CommandService>();

			mDiscord.Ready += OnReady;
			mDiscord.JoinedServer += OnJoinedServer;
			mDiscord.ServerAvailable += OnServerAvailable;
			mDiscord.LeftServer += OnServerLeft;
			mDiscord.UserJoined += OnUserJoined;
			mDiscord.UserLeft += OnUserLeft;
			mDiscord.UserUpdated += OnUserUpdated;
			mDiscord.UserBanned += OnUserBanned;
			mDiscord.UserUnbanned += OnUserUnbanned;
			mDiscord.MessageUpdated += OnMessageUpdated;
			mDiscord.MessageDeleted += OnMessageDeleted;

			RegisterHelp();
			RegisterCommands();
			RegisterSetGame();
			RegisterDisconnect();
			RegisterLeaveServer();
			RegisterPreferences();
			RegisterServerLog();
			RegisterModLog();
			RegisterEnableCommand();
			RegisterDisableCommand();
			RegisterFullMute();
			RegisterFullUnmute();
			RegisterKick();
			RegisterSoftBan();
			RegisterBan();
			RegisterUnban();
			RegisterRemoveMessages();
			RegisterRemoveAllMessages();
			RegisterPruneMembers();
			RegisterGiveRole();
			RegisterTakeRole();
			RegisterCreateRole();
			RegisterSoftDeleteRole();
			RegisterDeleteRole();
			RegisterRolePerm();
			RegisterCopyRolePerm();
			RegisterClearRolePerm();
			RegisterChangeRoleName();
			RegisterChangeRoleColor();
			RegisterCreateChannel();
			RegisterSoftDeleteChannel();
			RegisterDeleteChannel();
			RegisterChannelPerm();
			RegisterCopyChannelPerms();
			RegisterClearChannelPerms();
			RegisterChangeChannelName();
			RegisterChangeChannelTopic();
			RegisterCreateInstantInvite();
			RegisterMoveUser();
			RegisterVoiceMuteUser();
			RegisterDeafenUser();
			RegisterNicknameUser();
			RegisterUsersWithRole();
			RegisterForUsersWithRole();
			RegisterServerID();
			RegisterChannelID();
			RegisterRoleID();
			RegisterUserID();
			RegisterUserInfo();
			RegisterBotInfo();
			RegisterCurrentPreferences();
			RegisterCurrentBanList();

			RegisterTest();

			mDiscord.ExecuteAndWait(async () =>
			{
				await mDiscord.Connect("Key", TokenType.Bot);
			});
		}

		static MyBot()
		{
			//Reflect the permission types so they are usable in this bot
			Type type = Type.GetType("Discord.PermissionBits,Discord.Net");
			if (type == null)
			{
				Console.WriteLine("!!!ERROR IN PERMISSIONS, BOT SHUTTING DOWN!!!");
				Thread.Sleep(5000);
				System.Environment.Exit(727);
			}
			foreach (var field in type.GetFields())
			{
				try
				{
					int bit = (byte)Enum.Parse(type, field.Name);
					mPermissionNames[bit] = field.Name.ToLower();
					mPermissionValues[field.Name.ToLower()] = bit;
					Console.WriteLine("Permission: " + field.Name + ": " + bit);
				}
				catch (Exception)
				{
				}
			}

			mPermissionValues["manageroles"] = mPermissionValues["managerolesorpermissions"];
			mPermissionValues["managepermissions"] = mPermissionValues["managerolesorpermissions"];

			//Get the names and bits of permissions not in API
			foreach (int value in Enum.GetValues(typeof(ChannelPermissionsNotInAPI)))
			{
				String permissionName = Enum.GetName(typeof(ChannelPermissionsNotInAPI), value);
				mPermissionNames[value] = permissionName;
				mPermissionValues[permissionName] = value;
			}

			uint voicePermissions = ChannelPermissions.VoiceOnly.RawValue;
			uint textPermissions = ChannelPermissions.TextOnly.RawValue;

			voicePermissions |= (1 << (int)ChannelPermissionsNotInAPI.managewebhooks);
			textPermissions |= (1 << (int)ChannelPermissionsNotInAPI.addreactions);
			textPermissions |= (1 << (int)ChannelPermissionsNotInAPI.useexternalemojis);
			textPermissions |= (1 << (int)ChannelPermissionsNotInAPI.managewebhooks);

			VOICE_PERMISSIONS = new ChannelPermissions(voicePermissions);
			TEXT_PERMISSIONS = new ChannelPermissions(textPermissions);
		}

		enum ChannelPermissionsNotInAPI : int
		{
			//Only 99% sure these are the correct bit values
			addreactions = 6,
			useexternalemojis = 18,
			managewebhooks = 29,
			manageemojis = 30
		}

		private static readonly ChannelPermissions VOICE_PERMISSIONS;
		private static readonly ChannelPermissions TEXT_PERMISSIONS;

		//----------TODO_List----------
		//RAM thing
		//Personalised join and leave messages
		//Enable/disable commands
		//Preferences stuff now?
		//--------TODO_List_End--------

		//----------Constants----------
		const UInt64 OWNER_ID = 172138437246320640;
		const Int32 WAIT_TIME = 3000;
		const int MEMBER_LIMIT = 0;
		const int MAX_MESSAGES_TO_GATHER = 100;
		const int TIME_FOR_WAIT_BETWEEN_DELETING_MESSAGES_UNTIL_THEY_PRINT_TO_THE_SERVER_LOG = 3;
		const int NICKNAME_LENGTH = 32;
		const int TOPIC_LENGTH = 1024;
		const int OWNER_POSITION = 9001;
		const bool DISCONNECT = false;
		const String ZERO_LENGTH_CHAR = "\u180E";
		const String ERROR_MESSAGE = "Unsuitable input.";
		const String ARGUMENTS_ERROR = "Invalid number of arguments.";
		const String USER_ERROR = "Invalid user.";
		const String ROLE_ERROR = "Invalid role.";
		const String CHANNEL_ERROR = "Invalid channel.";
		const String MUTE_ROLE_NAME = "Muted";
		const String BASE_CHANNEL_NAME = "advobot";
		const String PREFERENCES_FILE = "commandPreferences.txt";
		const String BAN_REFERENCE_FILE = "banReferences.txt";
		const String SERVERLOG_AND_MODLOG = "serverlogAndModlog.txt";
		const String SERVER_LOG_CHECKER = "Serverlog:";
		const String MOD_LOG_CHECKER = "Modlog:";
		const String CHANNEL_INSTRUCTIONS = "[#Channel|[Channel/[Text|Voice]]]";
		const String CHANNEL_PERMISSIONS_ERROR = "You do not have the ability to edit that channel.";
		//--------Constants_End--------

		//----------Other_Items----------
		private UInt64 Key = (ulong)new Random().Next(0, 10000000);
		private List<HelpEntry> mHelpList = new List<HelpEntry>();
		private DateTime startupTime = DateTime.UtcNow;
		private int totalUsers = 0;
		private int totalServers = 0;
		private int successfulCommands = 0;
		private int attemptedCommands = 0;
		private int loggedJoins = 0;
		private int loggedLeaves = 0;
		private int loggedUserChanged = 0;
		private int loggedBans = 0;
		private int loggedUnbans = 0;
		private int loggedEdits = 0;
		private int loggedDeletes = 0;
		//--------Other_Items_End--------

		//----------Dictionaries----------
		private Dictionary<ulong, List<PreferenceCategory>> mCommandPreferences = new Dictionary<ulong, List<PreferenceCategory>>();
		private Dictionary<ulong, Dictionary<ulong, String>> mBanList = new Dictionary<ulong, Dictionary<ulong, String>>();
		private Dictionary<ulong, List<String>> mDeletedMessages = new Dictionary<ulong, List<String>>();
		private Dictionary<ulong, CancellationTokenSource> mCancelTokens = new Dictionary<ulong, CancellationTokenSource>();
		private Dictionary<String, int> mInviteLinks = new Dictionary<String, int>();
		private static Dictionary<String, int> mPermissionValues = new Dictionary<String, int>();
		private static Dictionary<int, String> mPermissionNames = new Dictionary<int, String>();
		//--------Dictionaries_Statis_End--------

		//----------Autonomous_Actions----------
		//Log creator
		private void Log(object sender, LogMessageEventArgs e)
		{
			Console.WriteLine(e.Message);
		}
		//
		//When the bot is first turned on
		private void OnReady(object sender, EventArgs args)
		{
			Console.WriteLine(String.Format("{0}: Bot is online.", MethodBase.GetCurrentMethod().Name));

			//Set the 'playing' section
			mDiscord.SetGame("type \">help\" for help.");
		}
		//
		//When the bot joins a server
		private void OnJoinedServer(object sender, ServerEventArgs args)
		{
			Console.WriteLine(String.Format("{0}: Bot joined {1}#{2}.", MethodBase.GetCurrentMethod().Name, args.Server, args.Server.Id));
		}
		//
		//When the bot turns on and a server shows up
		private void OnServerAvailable(object sender, ServerEventArgs args)
		{
			Console.WriteLine(String.Format("{0}: {1}#{2} is online now.", MethodBase.GetCurrentMethod().Name, args.Server, args.Server.Id));
			loadPreferences(args.Server);
			loadBans(args.Server);

			//var t = Task.Run(async delegate
			//{
			//	IEnumerable<Invite> invs = await args.Server.GetInvites();
			//	List<Invite> invites = args.Server.GetInvites().Result.ToList();
			//	foreach (Invite inv in invites)
			//	{
			//		mInviteLinks[inv.Code] = inv.Uses;
			//	}
			//});

			totalUsers += args.Server.UserCount;
			totalServers++;
		}
		//
		//When the bot leaves a server
		private void OnServerLeft(object sender, ServerEventArgs args)
		{
			Console.WriteLine(String.Format("{0}: Bot has left {1}#{2}.", MethodBase.GetCurrentMethod().Name, args.Server, args.Server.Id));

			totalUsers -= (args.Server.UserCount + 1);
			totalServers--;
		}
		//
		//--------Server_Log_Stuff--------
		//
		//Tell when a user joins the server
		private void OnUserJoined(object sender, UserEventArgs args)
		{
			++loggedJoins;
			Channel logChannel = logChannelCheck(args.Server, SERVER_LOG_CHECKER);
			if (logChannel != null)
			{
				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				if (args.User.IsBot)
				{
					sendMessage(logChannel, String.Format("{0} **BOT JOIN:** `{1}#{2}` **ID** `{3}`",
						time, args.User.Name, args.User.Discriminator, args.User.Id));
					return;
				}

				sendMessage(logChannel, String.Format("{0} **JOIN:** `{1}#{2}` **ID** `{3}`",
					time, args.User.Name, args.User.Discriminator, args.User.Id));
			}
		}
		//
		//Tell when a user leaves the server
		private void OnUserLeft(object sender, UserEventArgs args)
		{
			++loggedLeaves;
			Channel logChannel = logChannelCheck(args.Server, SERVER_LOG_CHECKER);
			if (logChannel != null)
			{
				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				if (args.User.IsBot)
				{
					sendMessage(logChannel, String.Format("{0} **BOT LEAVE:** `{1}#{2}` **ID** `{3}`",
						time, args.User.Name, args.User.Discriminator, args.User.Id));
					return;
				}

				sendMessage(logChannel, String.Format("{0} **LEAVE:** `{1}#{2}` **ID** `{3}`",
					time, args.User.Name, args.User.Discriminator, args.User.Id));
			}
		}
		//
		//Tell when a user has their name, nickname, or roles changed
		private void OnUserUpdated(object sender, UserUpdatedEventArgs args)
		{
			++loggedUserChanged;
			Channel logChannel = logChannelCheck(args.Server, SERVER_LOG_CHECKER);
			if (logChannel != null)
			{
				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				//Name change
				if (!args.Before.Name.Equals(args.After.Name))
				{
					sendMessage(logChannel, String.Format("{0} **NAME:** `{1}#{2}` **TO** `{3}#{4}`",
						time, args.Before.Name, args.Before.Discriminator, args.After.Name, args.After.Discriminator));
				}

				//Nickname change
				if ((String.IsNullOrWhiteSpace(args.Before.Nickname) && !String.IsNullOrWhiteSpace(args.After.Nickname))
					 || (!String.IsNullOrWhiteSpace(args.Before.Nickname) && String.IsNullOrWhiteSpace(args.After.Nickname)))
				{
					String originalNickname = args.Before.Nickname;
					if (String.IsNullOrWhiteSpace(args.Before.Nickname))
					{
						originalNickname = "NO NICKNAME";
					}
					String nicknameChange = args.After.Nickname;
					if (String.IsNullOrWhiteSpace(args.After.Nickname))
					{
						nicknameChange = "NO NICKNAME";
					}
					sendMessage(logChannel, String.Format("{0} **NICKNAME:** `{1}#{2}` **FROM** `{3}` **TO** `{4}`",
						time, args.Before.Name, args.Before.Discriminator, originalNickname, nicknameChange));
				}
				else if (!(String.IsNullOrWhiteSpace(args.Before.Nickname) && String.IsNullOrWhiteSpace(args.After.Nickname)))
				{
					if (!args.Before.Nickname.Equals(args.After.Nickname))
					{
						sendMessage(logChannel, String.Format("{0} **NICKNAME:** `{1}#{2}` **FROM** `{3}` **TO** `{4}`",
							time, args.Before.Name, args.Before.Discriminator, args.Before.Nickname, args.After.Nickname));
					}
				}

				//Role change
				String roles = null;
				List<Role> firstNotSecond = args.Before.Roles.ToList().Except(args.After.Roles.ToList()).ToList();
				List<Role> secondNotFirst = args.After.Roles.ToList().Except(args.Before.Roles.ToList()).ToList();
				if (firstNotSecond.Count() > 0)
				{
					roles = String.Join(", ", firstNotSecond);
					sendMessage(logChannel, String.Format("{0} **LOSS:** `{1}#{2}` **LOST** `{3}`",
						time, args.Before.Name, args.Before.Discriminator, roles));
				}
				else if (secondNotFirst.Count() > 0)
				{
					roles = String.Join(", ", secondNotFirst);
					sendMessage(logChannel, String.Format("{0} **GAIN:** `{1}#{2}` **GAINED** `{3}`",
						time, args.Before.Name, args.Before.Discriminator, roles));
				}
			}
		}
		//
		//Tell when a user is banned
		private void OnUserBanned(object sender, UserEventArgs args)
		{
			++loggedBans;
			Channel logChannel = logChannelCheck(args.Server, SERVER_LOG_CHECKER);
			if (logChannel != null)
			{
				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				sendMessage(logChannel, String.Format("{0} **BAN:** `{1}#{2}` **ID** `{3}`",
					time, args.User.Name, args.User.Discriminator, args.User.Id));
			}
			//Add the user to the ban list document
			Dictionary<ulong, String> banList = mBanList[args.Server.Id];
			banList[args.User.Id] = args.User.Name + "#" + args.User.Discriminator;
			saveBans(args.Server.Id);
		}
		//
		//Tell when a user is unbanned
		private void OnUserUnbanned(object sender, UserEventArgs args)
		{
			++loggedUnbans;
			Channel logChannel = logChannelCheck(args.Server, SERVER_LOG_CHECKER);
			if (logChannel != null)
			{
				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				sendMessage(logChannel, String.Format("{0} **UNBAN:** `{1}#{2}` **ID** `{3}`",
					time, args.User.Name, args.User.Discriminator, args.User.Id));
			}
			//Remove the user from the ban list document
			Dictionary<ulong, String> banList = mBanList[args.Server.Id];
			banList.Remove(args.User.Id);
			saveBans(args.Server.Id);
		}
		//
		//Tell when a message is edited 
		private void OnMessageUpdated(object sender, MessageUpdatedEventArgs args)
		{
			++loggedEdits;
			Channel logChannel = logChannelCheck(args.Server, SERVER_LOG_CHECKER);
			if (logChannel != null)
			{
				String[] beforeInfo = args.Before.ToString().Split(new char[] { ' ' }, 2);
				String[] afterInfo = args.After.ToString().Split(new char[] { ' ' }, 2);
				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";

				//Bot cannot pick up messages from before it was started
				if (String.IsNullOrWhiteSpace(beforeInfo[1]))
				{
					beforeInfo[1] = "UNABLE TO BE GATHERED";
				}

				//Determine lengths for error checking
				if (beforeInfo[1].Length + afterInfo[1].Length < 1500)
				{
					editMessage(logChannel, time, args.User, args.Channel, beforeInfo[1], afterInfo[1]);
					return;
				}
				else
				{
					if (beforeInfo[1].Length > 750)
					{
						if (afterInfo[1].Length > 750)
						{
							editMessage(logChannel, time, args.User, args.Channel, "SPAM", "SPAM");
							return;
						}
						editMessage(logChannel, time, args.User, args.Channel, "SPAM", afterInfo[1]);
						return;
					}
					editMessage(logChannel, time, args.User, args.Channel, beforeInfo[1], "SPAM");
					return;
				}
			}
		}
		//
		//Tell when a message is deleted
		private void OnMessageDeleted(object sender, MessageEventArgs args)
		{
			++loggedDeletes;
			Channel logChannel = logChannelCheck(args.Server, SERVER_LOG_CHECKER);
			if (logChannelCheck(args.Server, SERVER_LOG_CHECKER) != null)
			{
				//Got an error once time due to a null user when spam testing, so this check is here
				if (args.User == null)
					return;

				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				String message = String.Format("{0} **DELETED:** `{1}#{2}` **IN** `#{3}`\n```{4}```",
					time, args.User.Name, args.User.Discriminator, args.Channel, args.Message.Text.Replace("`", "'"));

				//Get a list of the deleted messages per server
				List<String> mainMessages;
				if (!mDeletedMessages.TryGetValue(args.Server.Id, out mainMessages))
				{
					mainMessages = new List<String>();
					mDeletedMessages[args.Server.Id] = mainMessages;
				}
				lock (mainMessages)
				{
					mainMessages.Add(message);
					Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Maintask: " + mainMessages.Count());
				}

				//Use a token so the messages do not get sent prematurely
				CancellationTokenSource cancelToken;
				if (mCancelTokens.TryGetValue(args.Server.Id, out cancelToken))
				{
					cancelToken.Cancel();
				}
				cancelToken = new CancellationTokenSource();
				mCancelTokens[args.Server.Id] = cancelToken;

				//Make a separate task in order to not mess up the other commands
				var t = Task.Run(async delegate
				{
					try
					{
						//IGNORE THIS EXCEPTION OR ELSE THE BOT LOCKS EACH TIME MESSAGES ARE DELETED
						await Task.Delay(TimeSpan.FromSeconds(TIME_FOR_WAIT_BETWEEN_DELETING_MESSAGES_UNTIL_THEY_PRINT_TO_THE_SERVER_LOG), cancelToken.Token);
					}
					catch (TaskCanceledException)
					{
						Console.WriteLine("Expected exception occurred during deleting messages.");
						return;
					}
					int characterCounter = 0;
					List<String> deletedMessages;
					List<String> taskMessages = mDeletedMessages[args.Server.Id];
					lock (taskMessages)
					{
						deletedMessages = new List<String>(taskMessages);
						characterCounter += taskMessages[0].Length;
						taskMessages.Clear();
					}
					if (deletedMessages.Count() == 0)
						return;
					Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Deleting: " + deletedMessages.Count());
					characterCounter += deletedMessages.Count() * 100;

					if ((deletedMessages.Count() <= 3) && (characterCounter < 2000))
					{
						//If there aren't many messages send the small amount in a message instead of a file
						await sendMessage(logChannel, String.Join("\n", deletedMessages));
					}
					else
					{
						//Get the file path
						String deletedMessagesFile = "Deleted_Messages_" + DateTime.UtcNow.ToString("MM-dd_HH-mm-ss") + ".txt";
						String path = getServerFilePath(args.Server.Id, deletedMessagesFile);

						//Create the temporary file
						if (!File.Exists(getServerFilePath(args.Server.Id, deletedMessagesFile)))
						{
							System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
						}

						//Write to the temporary file
						using (StreamWriter writer = new StreamWriter(path, true))
						{
							writer.WriteLine(String.Join("\n-----\n", deletedMessages).Replace("*", "").Replace("`", ""));
						}

						//Upload the file
						Message msg = await sendMessage(logChannel, time + "**DELETED:**");
						while (msg.State == MessageState.Queued)
						{
							//Sleep is needed otherwise the message gets sent after the file
							Thread.Sleep(100);
						}
						await logChannel.SendFile(path);

						//Delete the file
						File.Delete(path);
					}
				});
			}
		}
		//
		//--------Mod_Log_Stuff--------
		//
		//--------Autonomous_Actions_End--------

		//----------Commands----------
		//Help
		private void RegisterHelp()
		{
			String COMMAND_NAME = "help";
			String[] COMMAND_ALIASES = { "h" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Command", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				//Check if the user has a permission that would even allow him to use any commands
				if (!userHasSomething(e.Server, e.User))
					return;
				attCount();

				//Get the input command, if nothing then link to documentation
				String command = e.GetArg(0).ToLower();
				String[] commandParts = command.Split(new char[] { '[' }, 2);
				//See if nothing was input
				if (String.IsNullOrWhiteSpace(command))
				{
					await sendMessage(e.Channel, "Type `>commands` for the list of commands." +
						"\nType `>help [Command]` for help with a command.\nLink to the documentation of this bot: https://gist.github.com/advorange/3da9140889b20009816e4c9629de51c9");
					return;
				}
				//Idiot proofing
				else if (command.IndexOf('[') == 0)
				{
					if (commandParts[1].ToLower().Equals("command"))
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, "If you do not know what commands this bot has, type `>commands` for a list of commands.", 10000);
						return;
					}
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, "[] means required information. <> means optional information. | means or.", 10000);
					return;
				}

				//Send the message for that command
				HelpEntry helpEntry = mHelpList.FirstOrDefault(x => x.Name.Equals(command));
				if (helpEntry == null)
				{
					foreach (HelpEntry commands in mHelpList)
					{
						if (commands.Aliases.Contains(command))
						{
							helpEntry = commands;
						}
					}
					if (helpEntry == null)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Nonexistent command."), WAIT_TIME);
						return;
					}
				}
				await sendMessage(e.Channel, String.Format("```Aliases: {0}\nUsage: {1}\nBase Permission(s): {2}\nDescription: {3}```",
					helpEntry.FormatAliases(), helpEntry.Usage, helpEntry.basePerm, helpEntry.Text));
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">help [Command]", "Any",
				"Prints out the aliases of the command, the usage of the command, and the description of the command. If left blank will print out a link to the documentation of this bot.");
		}
		//
		//Command list
		private void RegisterCommands()
		{
			String COMMAND_NAME = "commands";
			String[] COMMAND_ALIASES = { "cmds" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Section", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				//Check if the user has a permission that would even allow them to use any commands
				if (!userHasSomething(e.Server, e.User))
					return;
				attCount();
				bool allCommandsBool = true;

				String section = e.GetArg(0).ToLower();
				if (String.IsNullOrWhiteSpace(section))
				{
					await sendMessage(e.Channel, "The following categories exist: `Administration`, `Moderation`, `Votemute`, `Slowmode`, `Banphrase`, and `All`." +
						"\nType `>commands [Category]` for commands from that category.");
				}
				else if (section.Equals("administration"))
				{
					String[] commands = getCommands(e.Server, 0);
					await sendMessage(e.Channel, String.Format("**ADMINISTRATION:** ```\n{0}```", String.Join("\n", commands)));
				}
				else if (section.Equals("moderation"))
				{
					String[] commands = getCommands(e.Server, 1);
					await sendMessage(e.Channel, String.Format("**MODERATION:** ```\n{0}```", String.Join("\n", commands)));
				}
				else if (section.Equals("votemute"))
				{
					String[] commands = getCommands(e.Server, 2);
					await sendMessage(e.Channel, String.Format("**VOTEMUTE:** ```\n{0}```", String.Join("\n", commands)));
				}
				else if (section.Equals("slowmode"))
				{
					String[] commands = getCommands(e.Server, 3);
					await sendMessage(e.Channel, String.Format("**SLOWMODE:** ```\n{0}```", String.Join("\n", commands)));
				}
				else if (section.Equals("banphrase"))
				{
					String[] commands = getCommands(e.Server, 4);
					await sendMessage(e.Channel, String.Format("**BANPHRASE:** ```\n{0}```", String.Join("\n", commands)));
				}
				else if (section.Equals("all"))
				{
					if (allCommandsBool)
					{
						List<String> commands = new List<String>();
						foreach (Command command in mCommands.AllCommands)
						{
							commands.Add(command.Text);
						}
						await sendMessage(e.Channel, String.Format("**ALL:** ```\n{0}```", String.Join("\n", commands)));
					}
					else
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, "All is currently turned off.", WAIT_TIME);
					}
				}
				else
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Category does not exist."), WAIT_TIME);
				}
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">commands <Category|All>", "Any",
				"Prints out the commands in that section of the command list.");
		}
		//
		//Setgame
		private void RegisterSetGame()
		{
			String COMMAND_NAME = "setgame";
			String[] COMMAND_ALIASES = { "sg" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Game", ParameterType.Unparsed)
			.Do(e =>
			{
				if (!e.User.Id.Equals(OWNER_ID))
					return;
				attCount();

				//Check the game name length
				String game = e.GetArg(0);
				if (game.Length > 128)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Game name cannot be longer than 128 characters or else it doesn't show to other people."),
						10000);
					return;
				}

				mDiscord.SetGame(game);
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Game set to `{0}`.", game), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">serverlog [New Name]", "Bot owner",
				"Changes the game the bot is currently listed as playing. By default only the bot owner can change this.");
		}
		//
		//Disconnect
		private void RegisterDisconnect()
		{
			String COMMAND_NAME = "disconnect";
			String[] COMMAND_ALIASES = { "dc" };
			String[] IT_IS_TRUE = { "dc", "runescapeservers" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(IT_IS_TRUE)
			.Do(async (e) =>
			{
				//This command FULLY TURNS OFF THE BOT.
				if (!userHasOwner(e.Server, e.User))
					return;
				attCount();

				if ((e.User.Id == OWNER_ID) || (DISCONNECT == true))
				{
					String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
					List<Message> msgs = new List<Message>();
					mBanList.Keys.ToList().ForEach(async x =>
					{
						Channel channel = logChannelCheck(mDiscord.GetServer(x), SERVER_LOG_CHECKER);
						if (null != channel)
						{
							msgs.Add(await sendMessage(channel, String.Format("{0} Bot is disconnecting.", time)));
						}
					});
					while (msgs.Any(x => x.State == MessageState.Queued))
					{
						Thread.Sleep(100);
					}
					await mDiscord.Disconnect();
				}
				else
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, "Disconnection is turned off for everyone but the bot owner currently.", WAIT_TIME);
				}
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">disconnect", "Bot owner",
				"Turns the bot off. By default only the person hosting the bot can do this.");
		}
		//
		//Leave a server
		private void RegisterLeaveServer()
		{
			String COMMAND_NAME = "leaveserver";
			String[] COMMAND_ALIASES = { "leave" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("ServerID", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasOwner(e.Server, e.User))
					return;
				attCount();

				String serverName;
				Server server;
				ulong serverID = 0;
				if (UInt64.TryParse(e.GetArg(0), out serverID))
				{
					if (e.User.Id.Equals(OWNER_ID))
					{
						server = mDiscord.GetServer(serverID);
						if (server == null)
						{
							makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Invalid server supplied."), WAIT_TIME);
							return;
						}
						serverName = server.Name;
						await server.Leave();
						if (e.Server == server)
						{
							return;
						}
						await sendMessage(e.Channel, String.Format("Successfully left the server `{0}` with an ID `{2}`.", serverName, serverID));
					}
				}
				else if (e.GetArg(0) == null)
				{
					await sendMessage(e.Channel, "Goodbye.");
					await e.Server.Leave();
					return;
				}
				else
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Invalid server supplied."), WAIT_TIME);
					return;
				}
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">leaveserver", "Owner",
				"Makes the bot leave the server.");
		}
		//
		//Enable preferences to be stored
		private void RegisterPreferences()
		{
			String COMMAND_NAME = "enablepreferences";
			String[] COMMAND_ALIASES = { "eprefs" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Confirmation", ParameterType.Optional)
			.Do(async (e) =>
			{
				if (!userHasOwner(e.Server, e.User))
					return;
				attCount();

				//Member limit
				if ((e.Server.UserCount < MEMBER_LIMIT) && (e.User.Id != OWNER_ID))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message,
						String.Format("Sorry, but this server is too small to warrant preferences. {0} or more members are required.", MEMBER_LIMIT), 10000);
					return;
				}

				//Confirmation of agreement
				String response = e.GetArg(0).ToLower();
				if (String.IsNullOrWhiteSpace(response))
				{
					await sendMessage(e.Channel, "By turning preferences on you will be enabling the ability to toggle commands, change who can use commands, " +
						"and many more features. This data will be stored in a text file off of the server, and whoever is hosting the bot will most likely have " +
						"access to these. A new text channel will be automatically created to display preferences and the server/mod log. If you agree to this, redo " +
						"the original command but with a \'Yes\' at the end");
					return;
				}
				else if (!(response.Equals("yes") || response.Equals("ye") || response.Equals("y")))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, "'Yes' was not input correctly.", 10000);
					return;
				}

				//Set up the preferences file(s) location(s) on the computer
				if (!File.Exists(getServerFilePath(e.Server.Id, PREFERENCES_FILE)))
				{
					savePreferences(e.Server.Id);
				}
				//Recreate advobot channel if not on the server but preferences file exists
				else if (File.Exists(getServerFilePath(e.Server.Id, PREFERENCES_FILE)) && !e.Server.FindChannels(BASE_CHANNEL_NAME).Any())
				{
					await e.Server.CreateChannel(BASE_CHANNEL_NAME, ChannelType.Text);
					e.Server.FindChannels(BASE_CHANNEL_NAME).ToList().ForEach(x => x.AddPermissionsRule(e.Server.EveryoneRole,
						new ChannelPermissionOverrides(readMessages: PermValue.Deny)));
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, "Successfully recreated the Advobot channel.", WAIT_TIME);
					return;
				}
				else
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, "Preferences are already turned on.", WAIT_TIME);
					return;
				}

				//Create advobot channel
				if (!e.Server.FindChannels(BASE_CHANNEL_NAME).Any())
				{
					await e.Server.CreateChannel(BASE_CHANNEL_NAME, ChannelType.Text);
					e.Server.FindChannels(BASE_CHANNEL_NAME).ToList().ForEach(x => x.AddPermissionsRule(e.Server.EveryoneRole,
						new ChannelPermissionOverrides(readMessages: PermValue.Deny)));
				}

				//Send the preferences message
				await sendMessage(e.Server.FindChannels(BASE_CHANNEL_NAME).FirstOrDefault(),
					System.IO.File.ReadAllText(getServerFilePath(e.Server.Id, PREFERENCES_FILE)).Replace("@", ""));
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">enablepreferences <Yes>", "Owner",
				"Gives the server preferences which allows using self-assignable roles, toggling commands, and changing the permissions of commands. " +
				"Recreates the 'Advobot' channel if removed.");
		}
		//
		//Enable serverlog on channel other than advobot
		private void RegisterServerLog()
		{
			String COMMAND_NAME = "serverlog";
			String[] COMMAND_ALIASES = { "sl" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Channel", ParameterType.Optional)
			.Do(async (e) =>
			{
				if (!userHasAdmin(e.Server, e.User))
					return;
				attCount();

				String input = e.GetArg(0);
				if (setServerOrModlog(e.Server, e.Channel, e.Message, input, "serverlog"))
				{
					await sendMessage(e.Channel, String.Format("Serverlog has been set on channel {0} with the ID `{1}`.",
						input, e.Server.FindChannels(input).FirstOrDefault().Id));
				}
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">serverlog [#Channel]", "Administrator",
				"Puts the serverlog on the specified channel. Serverlog is a log of: users joining/leaving, editing messages, deleting messages, and bans/unbans." +
				"Type in \'>serverlog [Off]\' to turn off the serverlog completely.");
		}
		//
		//Enable modlog on channel other than advobot
		private void RegisterModLog()
		{
			String COMMAND_NAME = "modlog";
			String[] COMMAND_ALIASES = { "ml" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Channel", ParameterType.Optional)
			.Do(async (e) =>
			{
				if (!userHasAdmin(e.Server, e.User))
					return;
				attCount();

				String input = e.GetArg(0);
				if (setServerOrModlog(e.Server, e.Channel, e.Message, input, "modlog"))
				{
					await sendMessage(e.Channel, String.Format("Modlog has been set on channel {0} with the ID `{1}`.", input, e.Server.FindChannels(input).FirstOrDefault().Id));
				}
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">Modlog [#Channel]", "Administrator",
				"Puts the modlog on the specified channel. Modlog is a log of commands used from this bot." +
				"Type in \'>modlog [Off]\' to turn off the serverlog completely.");
		}
		//
		//Enable command
		private void RegisterEnableCommand()
		{

		}
		//
		//Disable command
		private void RegisterDisableCommand()
		{

		}
		//
		//Mute
		private void RegisterFullMute()
		{
			String COMMAND_NAME = "fullmute";
			String[] COMMAND_ALIASES = { "fm" };
			String[] STUPID_JOKE_THAT_DOESNT_EVEN_WORK_ALL_THAT_WELL_SINCE_THE_USER_CAN_STILL_SEE_MESSAGES_BUT_WHATEVER = { "fm", "hellenkeller" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(STUPID_JOKE_THAT_DOESNT_EVEN_WORK_ALL_THAT_WELL_SINCE_THE_USER_CAN_STILL_SEE_MESSAGES_BUT_WHATEVER)
			.Parameter("Username", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				//Either userHasManageMessages and userHasManageRoles because muting can be a proactive way of managing spam
				if (!userHasManageMessages(e.Server, e.User) && !userHasManageRoles(e.Server, e.User))
					return;
				attCount();

				//Check if role already exists, if not, create it
				Role muteRole = await createMuteRole(e.Server, e.User);
				if (muteRole == null)
					return;

				//Determine if the bot can use the mute role
				int position = getPosition(e.Server, e.Server.GetUser(mDiscord.CurrentUser.Id));
				if (position < muteRole.Position)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Mute role has a higher position than the bot can access."), WAIT_TIME);
					return;
				}

				//Test if valid user mention
				if (getUser(e.Server, e.GetArg(0)) == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(USER_ERROR), WAIT_TIME);
					return;
				}
				User user = getUser(e.Server, e.GetArg(0));

				//Give the targetted user the role
				await giveRole(user, muteRole);
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully muted {0}.", user.Mention), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">fullmute [@User]", "Manage messages or manage roles",
				"Removes the user's ability to speak or type via the 'Muted' role." +
				"If a 'Muted' role already exists, this command will use that instead of creating a new role.");
		}
		//
		//Unmute
		private void RegisterFullUnmute()
		{
			String COMMAND_NAME = "fullunmute";
			String[] COMMAND_ALIASES = { "fum" };
			String[] ANOTHER_BAD_JOKE = { "fum", "chum" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(ANOTHER_BAD_JOKE)
			.Parameter("Username", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				//Either userHasManageMessages and userHasManageRoles for same reason as above
				if (!userHasManageMessages(e.Server, e.User) && !userHasManageRoles(e.Server, e.User))
					return;
				attCount();

				//Test if valid user mention
				if (getUser(e.Server, e.GetArg(0)) == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(USER_ERROR), WAIT_TIME);
					return;
				}
				User user = getUser(e.Server, e.GetArg(0));

				//Remove the role
				await takeRole(user, getRole(e.Server, MUTE_ROLE_NAME));
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully unmuted {0}.", user.Mention), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">fullunmute [@User]", "Manage messages or manage roles",
				"Gives the user back the ability to speak or type via removing the 'Muted' role.");
		}
		//
		//Kick
		private void RegisterKick()
		{
			String COMMAND_NAME = "kick";
			String[] COMMAND_ALIASES = { "k" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Username", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasKickMembers(e.Server, e.User))
					return;
				attCount();
				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";

				//Test if valid user mention
				User inputUser = getUser(e.Server, e.GetArg(0));
				if (inputUser == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(USER_ERROR), WAIT_TIME);
					return;
				}

				//Determine if the user is allowed to kick this person
				int kickerPosition = userHasOwner(e.Server, e.User) ? OWNER_POSITION : getPosition(e.Server, e.User);
				int kickeePosition = getPosition(e.Server, inputUser);
				if (kickerPosition <= kickeePosition)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("User is unable to be kicked by you."), WAIT_TIME);
					return;
				}

				//Determine if the bot can kick this person
				if (getPosition(e.Server, e.Server.GetUser(mDiscord.CurrentUser.Id)) <= kickeePosition)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Bot is unable to kick user."), WAIT_TIME);
					return;
				}

				//Kick the targetted user
				await inputUser.Kick();
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully kicked {0}.", inputUser.Mention), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">kick [@User]", "Kick members",
				"Kicks the user from the server.");
		}
		//
		//Softban
		private void RegisterSoftBan()
		{
			String COMMAND_NAME = "softban";
			String[] COMMAND_ALIASES = { "sb" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Username", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasBanMembers(e.Server, e.User))
					return;
				attCount();

				//Test if valid user mention
				User inputUser = getUser(e.Server, e.GetArg(0));
				if (inputUser == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(USER_ERROR), WAIT_TIME);
					return;
				}

				//Determine if the user is allowed to softban this person
				int sberPosition = userHasOwner(e.Server, e.User) ? OWNER_POSITION : getPosition(e.Server, e.User);
				int sbeePosition = getPosition(e.Server, inputUser);
				if (sberPosition <= sbeePosition)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("User is unable to be soft-banned by you."), WAIT_TIME);
					return;
				}

				//Determine if the bot can softban this person
				if (getPosition(e.Server, e.Server.GetUser(mDiscord.CurrentUser.Id)) <= sbeePosition)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Bot is unable to soft-ban user."), WAIT_TIME);
					return;
				}

				//Softban the targetted user
				await e.Server.Ban(inputUser);
				await e.Server.Unban(inputUser);
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully banned and unbanned {0}.", inputUser.Mention), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">softban [@User]", "Ban members",
				"Bans then unbans a user from the server.");
		}
		//
		//Ban
		private void RegisterBan()
		{
			String COMMAND_NAME = "ban";
			String[] COMMAND_ALIASES = { "b" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Username or ID and Days", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasBanMembers(e.Server, e.User))
					return;
				attCount();

				//Test number of arguments
				String[] values = e.GetArg(0).Split(' ');
				if ((values.Length < 1) || (values.Length > 2))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ARGUMENTS_ERROR), WAIT_TIME);
					return;
				}

				//Test if valid user mention
				User inputUser = null;
				if (values[0].StartsWith("<@"))
				{
					inputUser = getUser(e.Server, values[0]);
				}
				else if (getUlong(values[0]) != 0)
				{
					inputUser = e.Server.GetUser(getUlong(values[0]));
				}

				if (null == inputUser)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(USER_ERROR), WAIT_TIME);
					return;
				}

				//Determine if the user is allowed to ban this person
				int bannerPosition = userHasOwner(e.Server, e.User) ? OWNER_POSITION : getPosition(e.Server, e.User);
				int banneePosition = getPosition(e.Server, inputUser);
				if (bannerPosition <= banneePosition)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("User is unable to be banned by you."), WAIT_TIME);
					return;
				}

				//Determine if the bot can ban this person
				if (getPosition(e.Server, e.Server.GetUser(mDiscord.CurrentUser.Id)) <= banneePosition)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Bot is unable to ban user."), WAIT_TIME);
					return;
				}

				//Checking for valid days requested
				int pruneDays = 0;
				if (values.Length == 2 && !Int32.TryParse(values[1], out pruneDays))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Incorrect input for days of messages to be deleted."), WAIT_TIME);
					return;
				}

				//Forming the second half of the string that prints out when a user is successfully banned
				String plurality = "days";
				String latterHalfOfString = null;
				if (pruneDays == 1)
				{
					plurality = "day";
				}
				else if (pruneDays > 0)
				{
					latterHalfOfString = String.Format(", and deleted {0} {1} worth of messages.", pruneDays, plurality);
				}
				else if (pruneDays == 0)
				{
					latterHalfOfString = ".";
				}

				//Ban the user
				await e.Server.Ban(inputUser, pruneDays);
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully banned {0} {1}", inputUser.Mention, latterHalfOfString), 10000);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">ban [@User]", "Ban members",
				"Bans the user from the server.");
		}
		//
		//Unban
		private void RegisterUnban()
		{
			String COMMAND_NAME = "unban";
			String[] COMMAND_ALIASES = { "ub" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Username", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasBanMembers(e.Server, e.User))
					return;
				attCount();

				//Cut the user mention into the username and the discriminator
				String input = e.GetArg(0);
				String[] values = input.Split('#');
				if (values.Length < 1 || values.Length > 2)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(USER_ERROR), WAIT_TIME);
					return;
				}

				//Get their name and discriminator or ulong
				ulong inputUserID = 0;
				String secondHalfOfTheSecondaryMessage = "";
				//Unban given a username and discriminator
				if (values.Length == 2)
				{
					ushort discriminator = (ushort)Convert.ToInt32(values[1]);
					String username = values[0].Replace("@", "");
					String discriminatedUserName = username + "#" + discriminator;
					inputUserID = mBanList[e.Server.Id].FirstOrDefault(x => String.Equals(x.Value, discriminatedUserName)).Key;
					secondHalfOfTheSecondaryMessage = String.Format("unbanned the user `{0}#{1}` with the ID `{2}`.", username, discriminator, inputUserID);
				}
				//Unban given just a username
				else if (values.Length == 1 && !(ulong.TryParse(input, out inputUserID)))
				{
					List<String> bannedUsersWithSameName = mBanList[e.Server.Id].Values.Where(x => x.StartsWith(input)).ToList();
					if (bannedUsersWithSameName.Count() > 1)
					{
						await sendMessage(e.Channel, String.Format("The following users have that name: `{0}`.", String.Join("`, `", bannedUsersWithSameName)));
						return;
					}
					else if (bannedUsersWithSameName.Count == 0)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("No user on the ban list has that username."), WAIT_TIME);
						return;
					}

					//If only one user is found, unban that user
					String[] user = bannedUsersWithSameName[0].Split('#');
					ushort discriminator = (ushort)Convert.ToInt32(user[1]);
					String username = user[0].Replace("@", "");
					String discriminatedUserName = username + "#" + discriminator;
					inputUserID = mBanList[e.Server.Id].FirstOrDefault(x => String.Equals(x.Value, discriminatedUserName)).Key;
					secondHalfOfTheSecondaryMessage = String.Format("unbanned the user `{0}#{1}` with the ID `{2}`.", username, discriminator, inputUserID);
				}
				//Unban given a user ID
				else
				{
					inputUserID = getUlong(input);
					if (inputUserID == 0)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Invalid userID."), WAIT_TIME);
						return;
					}
					secondHalfOfTheSecondaryMessage = String.Format("unbanned the user with the ID `{0}`.", inputUserID);
				}

				//Unban the targetted user
				await e.Server.Unban(inputUserID);
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully {0}", secondHalfOfTheSecondaryMessage), 10000);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">unban [User|User#Discriminator|User ID]", "Ban users",
				"Removes the user from the ban list.");
		}
		//
		//Remove messages
		private void RegisterRemoveMessages()
		{
			String COMMAND_NAME = "removemessages";
			String[] COMMAND_ALIASES = { "rm" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Username and Count", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageMessages(e.Server, e.User))
					return;
				attCount();

				//Test number of arguments
				String input = e.GetArg(0);
				String[] values = input.Split(' ');
				if ((values.Length < 1) || (values.Length > 3))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ARGUMENTS_ERROR), WAIT_TIME);
					return;
				}

				int argIndex = 0;
				int argCount = values.Length;

				//Testing if starts with user mention
				User inputUser = null;
				if (argIndex < argCount && values[argIndex].StartsWith("<@"))
				{
					inputUser = getUser(e.Server, values[argIndex]);
					if (null == inputUser)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(USER_ERROR), WAIT_TIME);
						return;
					}
					++argIndex;
				}

				//Testing if starts with channel mention
				Channel inputChannel = e.Channel;
				if (argIndex < argCount && values[argIndex].StartsWith("<#"))
				{
					inputChannel = getChannelID(e.Server, values[argIndex]);
					if (null == inputChannel)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_ERROR), WAIT_TIME);
						return;
					}
					++argIndex;
				}

				//Checking for valid request count
				int requestCount = (argIndex == argCount - 1) ? getInteger(values[argIndex]) : -1;
				if (requestCount < 1)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Incorrect input for number of messages to be removed."), WAIT_TIME);
					return;
				}

				//Removing the command message itself
				if (e.Channel != inputChannel)
				{
					await removeMessages(e.Channel, 0);
				}
				else if ((e.User == inputUser) && (e.Channel == inputChannel))
				{
					++requestCount;
				}

				await removeMessages(inputChannel, requestCount, inputUser);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">removemessages <@User> <#Channel> [Number of Messages]", "Manage messages",
				"Removes the selected number of messages from either the user, the channel, both, or, if neither is input, the channel the command is said on.");
		}
		//
		//Remove all messages
		private void RegisterRemoveAllMessages()
		{
			String COMMAND_NAME = "removeallmessages";
			String[] COMMAND_ALIASES = { "ram" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Key", ParameterType.Optional)
			.Do(async (e) =>
			{
				//userHasAdmin instead of userHasManageMessages due to how this command can delete EVERY SINGLE message from a channel
				if (!userHasAdmin(e.Server, e.User))
					return;
				attCount();

				//Extremely lowtech way of preventing accidental use
				UInt64 input;
				UInt64.TryParse(e.GetArg(0), out input);
				if (input == Key)
				{
					await removeMessages(e.Channel, Int32.MaxValue - 1, null);
					Key = (ulong)new Random().Next(0, 1000000000);
					Console.WriteLine(String.Format("RAM Key: @{0} on the server {1}#{2} generated a new key.", e.User, e.Server, e.Server.Id));
				}
				else
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("That is the wrong key, {0}.", e.User.Mention), WAIT_TIME);
					await e.User.SendMessage("The current key is: " + Key);
				}
				succCount();

				//Pseudo code
				//if (user is on banned from ram list)
				//{
				//	e.channel.sendmessage(e.user.mention + " did a silly thing!");
				//}
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">ram <Key>", "Administrator",
				"This command will effectively kill message removing until all messages are removed from the targetted channel. " +
				"Do not abuse this command.");
		}
		//
		//Prune members
		private void RegisterPruneMembers()
		{
			String COMMAND_NAME = "prunemembers";
			String[] COMMAND_ALIASES = { "prune" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Days", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasAdmin(e.Server, e.User))
					return;
				attCount();

				String input = e.GetArg(0);
				if (!(input.Equals("1") || input.Equals("7") || input.Equals("30")))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Invalid option for days."), WAIT_TIME);
					return;
				}
				int number = Convert.ToInt16(input);

				await e.Server.PruneUsers(number);
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully remove all users who have not been online in the past {0} {1}.",
					number, number == 1 ? "day" : "days"), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">prunemembers [1|7|30]", "Administrator",
				"Removes users from the server who haven't been seen online in the past 1, 7, or 30 days." +
				"Having your online status set to offline does not add you to these lists.");
		}
		//
		//Give role
		private void RegisterGiveRole()
		{
			String COMMAND_NAME = "giverole";
			String[] COMMAND_ALIASES = { "give", "gr" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Username and Role", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageRoles(e.Server, e.User))
					return;
				attCount();

				//Test number of arguments
				String input = e.GetArg(0);
				String[] values = input.Split(new char[] { ' ' }, 2);
				if (values.Length != 2)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ARGUMENTS_ERROR), WAIT_TIME);
					return;
				}

				//Determine if the role exists and if it is able to be edited by both the bot and the user
				String roleString = values[1];
				if (getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, roleString) == null)
				{
					return;
				}
				Role inputRole = getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, roleString);

				if (inputRole.IsManaged)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Role is managed and unable to be given."), WAIT_TIME);
					return;
				}

				//Test if valid user mention
				User inputUser = getUser(e.Server, values[0]);
				if (inputUser == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(USER_ERROR), WAIT_TIME);
					return;
				}

				await giveRole(inputUser, inputRole);
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully gave {0} the {1} role.", inputUser.Mention, inputRole), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">giverole [@User] [Role]", "Manage roles",
				"Gives the user the role (assuming the person using the command and bot both have the ability to give that role).");
		}
		//
		//Take role
		private void RegisterTakeRole()
		{
			String COMMAND_NAME = "takerole";
			String[] COMMAND_ALIASES = { "take", "tr" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Username and Role", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageRoles(e.Server, e.User))
					return;
				attCount();

				//Test number of arguments
				String input = e.GetArg(0);
				String[] values = input.Split(new char[] { ' ' }, 2);
				if (values.Length != 2)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ARGUMENTS_ERROR), WAIT_TIME);
					return;
				}

				//Determine if the role exists and if it is able to be edited by both the bot and the user
				String roleString = values[1];
				if (getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, roleString) == null)
				{
					return;
				}
				Role inputRole = getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, roleString);

				if (inputRole.IsManaged)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Role is managed and unable to be taken."), WAIT_TIME);
					return;
				}

				//Test if valid user mention
				User inputUser = getUser(e.Server, values[0]);
				if (inputUser == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(USER_ERROR), WAIT_TIME);
					return;
				}

				await takeRole(inputUser, inputRole);
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully took the {0} role from {1}.", inputRole, inputUser.Mention), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">takerole [@User] [Role]", "Manage roles",
				"Take the role from the user (assuming the person using the command and bot both have the ability to take that role).");
		}
		//
		//Create role
		private void RegisterCreateRole()
		{
			String COMMAND_NAME = "createrole";
			String[] COMMAND_ALIASES = { "cr" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Role", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageRoles(e.Server, e.User))
					return;
				attCount();

				String role = e.GetArg(0);
				await e.Server.CreateRole(role, new ServerPermissions(0));
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully created the `{0}` role.", role), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">createrole [Role]", "Manage roles",
				"Adds a role to the server with the chosen name.");
		}
		//
		//Softdelete role
		private void RegisterSoftDeleteRole()
		{
			String COMMAND_NAME = "softdeleterole";
			String[] COMMAND_ALIASES = { "sdrole", "sdr" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Role", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageRoles(e.Server, e.User))
					return;
				attCount();

				//Determine if the role exists and if it is able to be edited by both the bot and the user
				Role inputRole = getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, e.GetArg(0));
				if (inputRole == null)
				{
					return;
				}

				//Check if even removable
				if (inputRole.IsManaged)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Role is managed and unable to be softdeleted."), WAIT_TIME);
					return;
				}

				//Create a new role with the same attributes (including space) and no perms
				Role newRole = await e.Server.CreateRole(inputRole.Name, new ServerPermissions(0), inputRole.Color);
				await newRole.Edit(position: inputRole.Position);

				//Delete the old role
				await inputRole.Delete();

				makeAndDeleteSecondaryMessage(e.Channel, e.Message,
					String.Format("Successfully removed all permissions from `{0}` and removed the role from all users on the server.", inputRole.Name), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">softdeleterole [Role]", "Manage roles",
				"Removes all permissions from a role (and all channels the role had permissions on) and removes the role from everyone. Leaves the name and color behind.");
		}
		//
		//Delete role
		private void RegisterDeleteRole()
		{
			String COMMAND_NAME = "deleterole";
			String[] COMMAND_ALIASES = { "drole", "dr" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Role", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageRoles(e.Server, e.User))
					return;
				attCount();

				//Determine if the role exists and if it is able to be edited by both the bot and the user
				String input = e.GetArg(0);
				if (getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, input) == null)
				{
					return;
				}
				Role inputRole = getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, input);

				if (inputRole.IsManaged)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Role is managed and unable to be deleted."), WAIT_TIME);
					return;
				}

				await inputRole.Delete();
				makeAndDeleteSecondaryMessage(e.Channel, e.Message,
					String.Format("Successfully removed all permissions from `{0}` and removed the role from all users on the server.", input), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">deleterole [Role]", "Manage roles",
				"Deletes the role. 'Drole' is a pretty funny alias.");
		}
		//
		//Change permissions of a role
		private void RegisterRolePerm()
		{
			String COMMAND_NAME = "rolepermissions";
			String[] COMMAND_ALIASES = { "erp", "rp" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Action, Role, and Permission", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageRoles(e.Server, e.User))
					return;
				attCount();

				//Put the input into a string
				String input = e.GetArg(0).ToLower();

				//Set the permission types into a list to later check against
				List<String> permissionTypeStrings = getPermissionNames().ToList();

				String[] actionRolePerms = input.Split(new char[] { ' ' }, 2); //Separate the role and whether to add or remove from the permissions
				String perms = null; //Set placeholder perms variable
				String role = null; //Set placeholder role variable
				bool show = false; //Set show bool

				//If the user wants to see the permission types, print them out
				if (input.Equals("show"))
				{
					await sendMessage(e.Channel, "**ROLE PERMISSIONS:**```\n" + String.Join("\n", permissionTypeStrings) + "```");
					return;
				}
				//If something is said after show, take that as a role.
				else if (input.StartsWith("show"))
				{
					role = input.Substring("show".Length).Trim();
					show = true;
				}
				//If show is not input, take the stuff being said as a role and perms
				else
				{
					if (actionRolePerms.Length == 1)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ARGUMENTS_ERROR), WAIT_TIME);
						return;
					}
					int lastSpace = actionRolePerms[1].LastIndexOf(' ');
					if (lastSpace <= 0)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ARGUMENTS_ERROR), WAIT_TIME);
						return;
					}
					//Separate out the permissions
					perms = actionRolePerms[1].Substring(lastSpace).Trim();
					//Separate out the role
					role = actionRolePerms[1].Substring(0, lastSpace).Trim();
				}

				//Determine if the role exists and if it is able to be edited by both the bot and the user
				if (getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, role) == null)
				{
					return;
				}
				Role inputRole = getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, role);

				if (inputRole.IsManaged)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Role is managed and unable to have its permissions changed."), WAIT_TIME);
					return;
				}

				//Send a message of the permissions the targetted role has
				if (show)
				{
					ServerPermissions rolePerms = new ServerPermissions(e.Server.GetRole(inputRole.Id).Permissions);
					List<String> currentRolePerms = new List<String>();
					foreach (var permissionValue in getPermissionValues())
					{
						int bit = (int)permissionValue;
						if ((rolePerms.RawValue & (1 << bit)) != 0)
						{
							currentRolePerms.Add(getPermissionName(bit));
						}
					}
					String pluralityPerms = "permissions";
					if (currentRolePerms.Count == 1)
					{
						pluralityPerms = "permission";
					}
					await sendMessage(e.Channel, String.Format("{0}{1} has the following {2}: `{3}`.",
						role.Substring(0, 1).ToUpper(), role.Substring(1), pluralityPerms, String.Join("`, `", currentRolePerms).ToLower()));
					return;
				}

				//See if it's add or remove
				String addOrRemove = actionRolePerms[0];
				bool add;
				if (addOrRemove.Equals("add"))
				{
					add = true;
				}
				else if (addOrRemove.Equals("remove"))
				{
					add = false;
				}
				else
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Add or remove not specified."), WAIT_TIME);
					return;
				}

				//Get the permissions
				String[] permissions = perms.Split('/');
				//Check if valid permissions
				if (permissions.Intersect(permissionTypeStrings).Count() != permissions.Count())
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(String.Format("Invalid {0} supplied.",
						(permissions.Count() - permissions.Intersect(permissionTypeStrings).Count()) == 1 ? "permission" : "permissions")), WAIT_TIME);
					return;
				}

				//Determine the permissions being added
				uint rolePermissions = 0;
				foreach (String permission in permissions)
				{
					try
					{
						int bit = getPermissionValue(permission);
						rolePermissions |= (1U << bit);
					}
					catch (Exception)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(String.Format("Couldn't parse permission '{0}'", permission)), WAIT_TIME);
						return;
					}
				}

				//Determine if the user can give these perms
				if (!userHasOwner(e.Server, e.User))
				{
					rolePermissions &= ~(1U << getPermissionValue("administrator"));
					if (!userHasAdmin(e.Server, e.User))
					{
						rolePermissions &= e.User.ServerPermissions.RawValue;
					}
					//If the role has something, but the user is not allowed to edit a permissions
					if (rolePermissions == 0)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(String.Format("You do not have the ability to modify {0}.",
							permissions.Count() == 1 ? "that permission" : "those permissions")), WAIT_TIME);
						return;
					}
				}

				//Get a list of the permissions that were given
				List<String> givenPermissions = getPermissionNames(rolePermissions);
				//Get a list of the permissions that were not given
				List<String> skippedPermissions = permissions.Except(givenPermissions).ToList();

				//New perms
				uint currentBits = e.Server.GetRole(inputRole.Id).Permissions.RawValue;
				if (add)
				{
					currentBits |= rolePermissions;
				}
				else
				{
					currentBits &= ~rolePermissions;
				}

				await e.Server.GetRole(inputRole.Id).Edit(null, new ServerPermissions(currentBits));
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully {0} `{1}` {2} {3} `{4}`.",
					(add ? "added" : "removed"),
					String.Join("`, `", givenPermissions),
					(skippedPermissions.Count() > 0 ? " and failed to " + (add ? "add `" : "remove `") + String.Join("`, `", skippedPermissions) + "`" : ""),
					(add ? "to" : "from"), inputRole),
					7500);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">rolepermissions [Show|Add|Remove] [Role] [Permission/...]", "Manage roles",
				"Add/remove the selected permissions to/from the role. Permissions must be separated by a '/'!" +
				"Type '>rolepermissions [Show]' to see the available permissions. Type '>rolepermissions [Show] [Role]' to see the permissions of that role.");
		}
		//
		//Copy role permissions
		private void RegisterCopyRolePerm()
		{
			String COMMAND_NAME = "copyrolepermissions";
			String[] COMMAND_ALIASES = { "crp" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Role and Role", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageRoles(e.Server, e.User))
					return;
				attCount();

				//Put the input into a string
				String input = e.GetArg(0).ToLower();
				String[] roles = input.Split(new char[] { '/' }, 2);

				//Test if two roles were input
				if (roles.Length != 2)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ARGUMENTS_ERROR), WAIT_TIME);
					return;
				}

				//Determine if the input role exists
				Role inputRole = getRole(e.Server, roles[0]);
				if (inputRole == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ROLE_ERROR), WAIT_TIME);
					return;
				}

				//Determine if the role exists and if it is able to be edited by both the bot and the user
				if (getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, roles[1]) == null)
				{
					return;
				}
				Role outputRole = getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, roles[1]);

				if (outputRole.IsManaged)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Role is managed and unable to have its permissions changed."), WAIT_TIME);
					return;
				}

				//Get the permissions
				uint rolePermissions = inputRole.Permissions.RawValue;
				List<String> permissions = getPermissionNames(rolePermissions);
				if (rolePermissions != 0)
				{
					//Determine if the user can give these permissions
					if (!userHasOwner(e.Server, e.User))
					{
						rolePermissions &= ~(1U << getPermissionValue("administrator"));
						if (!userHasAdmin(e.Server, e.User))
						{
							rolePermissions &= e.User.ServerPermissions.RawValue;
						}
						//If the role has something, but the user is not allowed to edit a permissions
						if (rolePermissions == 0)
						{
							makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(String.Format("You do not have the ability to modify {0}.",
								permissions.Count() == 1 ? "that permission" : "those permissions")), WAIT_TIME);
							return;
						}
					}
				}

				//Get a list of the permissions that were given
				List<String> givenPermissions = getPermissionNames(rolePermissions);
				//Get a list of the permissions that were not given
				List<String> skippedPermissions = permissions.Except(givenPermissions).ToList();

				//Actually change the permissions
				await e.Server.GetRole(outputRole.Id).Edit(null, new ServerPermissions(rolePermissions));
				//Send the long ass message detailing what happened with the command
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully copied `{0}` {1} from `{2}` to `{3}`.",
					(givenPermissions.Count() == 0 ? "NOTHING" : givenPermissions.Count() == permissions.Count() ? "ALL" : String.Join("`, `", givenPermissions)),
					(skippedPermissions.Count() > 0 ? "and failed to copy `" + String.Join("`, `", skippedPermissions) + "`" : ""),
					inputRole, outputRole),
					7500);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">copyrolepermissions [Role]/[Role]", "Manage roles",
				"Copies the permissions from the first role to the second role. Will not copy roles that the user does not have access to." +
				"Will not overwrite roles that are above the user's top role.");
		}
		//
		//Clear role permissions
		private void RegisterClearRolePerm()
		{
			String COMMAND_NAME = "clearrolepermissions";
			String[] COMMAND_ALIASES = { "clrrole" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Role", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageRoles(e.Server, e.User))
					return;
				attCount();

				//Determine if the role exists and if it is able to be edited by both the bot and the user
				String input = e.GetArg(0);
				if (getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, input) == null)
				{
					return;
				}
				Role inputRole = getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, input);

				if (inputRole.IsManaged)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Role is managed and unable to have its permissions changed."), WAIT_TIME);
					return;
				}

				//Clear the role's perms
				await inputRole.Edit(null, new ServerPermissions(0));
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully removed all permissions from `{0}`.", input), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">clearrolepermissions [Role]", "Manage roles",
				"Removes all permissions from a role.");
		}
		//
		//Change name of a role
		private void RegisterChangeRoleName()
		{
			String COMMAND_NAME = "changerolename";
			String[] COMMAND_ALIASES = { "crn" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Role and New Name", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageRoles(e.Server, e.User))
					return;
				attCount();

				//Put the input into a string
				String input = e.GetArg(0);
				//Split at the current role name and the new role name
				String[] values = input.Split(new char[] { '/' }, 2);

				//Determine if the role exists and if it is able to be edited by both the bot and the user
				if (getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, values[0]) == null)
				{
					return;
				}
				Role inputRole = getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, values[0]);

				await e.Server.GetRole(inputRole.Id).Edit(values[1]);
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully changed the name of the role `{0}` to `{1}`.",
					values[0], values[1]), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">changerolename [Role]/[New Name]",
				"Manage roles", "Changes the name of the role.");
		}
		//
		//Change color of a role
		private void RegisterChangeRoleColor()
		{
			String COMMAND_NAME = "changerolecolor";
			String[] COMMAND_ALIASES = { "crc" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Role and New Color", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageRoles(e.Server, e.User))
					return;
				attCount();

				//Put the input into a string
				String input = e.GetArg(0);
				//Split at the current role name and the new role name
				String[] values = input.Split(new char[] { '/' }, 2);


				//Determine if the role exists and if it is able to be edited by both the bot and the user
				if (getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, values[0]) == null)
				{
					return;
				}
				Role inputRole = getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, values[0]);

				UInt32 colorID = 0;
				colorID = (UInt32)System.Drawing.Color.FromName(values[1]).ToArgb();
				if (colorID == 0)
				{
					//Couldn't get name
					String hexString = values[1];
					if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
					{
						hexString = hexString.Substring(2);
					}
					if (!UInt32.TryParse(hexString, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out colorID))
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Color is unable to be added."), WAIT_TIME);
						return;
					}
				}

				await e.Server.GetRole(inputRole.Id).Edit(null, null, new Color(colorID & 0xffffff));
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully changed the color of the role `{0}` to `{1}`.",
					values[0], values[1]), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">changerolecolor Role/[Hexadecimal|Color Name]", "Manage roles",
				"Changes the role's color. A color of '0' sets the role back to the default color. " +
				"Colors must either be in hexadecimal format or be a color listed in the System.Drawing namespace of C#." +
				"\nFor a list of acceptable color names: https://msdn.microsoft.com/en-us/library/system.drawing.color(v=vs.110).aspx");
		}
		//
		//Create channel
		private void RegisterCreateChannel()
		{
			String COMMAND_NAME = "createchannel";
			String[] COMMAND_ALIASES = { "cch" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Channel and Type", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageChannels(e.Server, e.User))
					return;
				attCount();

				String[] values = e.GetArg(0).Split('/');
				String type = values[1];

				if (values.Length != 2)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ARGUMENTS_ERROR), WAIT_TIME);
					return;
				}
				if (values[0].Contains(' '))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("No spaces allowed."), WAIT_TIME);
					return;
				}
				if (!(type.Equals(ChannelType.Text.ToString()) || type.Equals(ChannelType.Voice.ToString())))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Invalid channel type."), WAIT_TIME);
					return;
				}

				await e.Server.CreateChannel(values[0], type);
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully created `{0}` ({1}).", values[0], char.ToUpper(type[0]) + type.Substring(1)), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">createchannel [Name]/[Text|Voice]", "Manage channels",
				"Adds a channel to the server of the given type with the given name. The name CANNOT contain any spaces, use underscores or dashes instead.");
		}
		//
		//Softdelete channel
		private void RegisterSoftDeleteChannel()
		{
			String COMMAND_NAME = "softdeletechannel";
			String[] COMMAND_ALIASES = { "sdch" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Channel", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!(userHasManageChannels(e.Server, e.User)) && !(userHasManageRoles(e.Server, e.User)))
					return;
				attCount();

				//Get the input
				String input = e.GetArg(0).ToLower();
				if (input.Contains(' '))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_ERROR), WAIT_TIME);
					return;
				}

				//Get the channel
				Channel channel = getChannel(e.Server, input, null);
				if (channel == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_ERROR), WAIT_TIME);
					return;
				}

				//See if the user can see and thus edit that channel
				if (!getChannelEditAbility(channel, e.User))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_PERMISSIONS_ERROR), WAIT_TIME);
					return;
				}

				//See if attempted on a voice channel
				if (channel.Type == ChannelType.Voice)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Softdelete only works on text channels."), WAIT_TIME);
					return;
				}

				//Make it so only admins/the owner can read the channel
				foreach (Channel.PermissionOverwrite overwrite in channel.PermissionOverwrites)
				{
					if (overwrite.TargetType.Equals("role"))
					{
						Role role = e.Server.GetRole(overwrite.TargetId);
						uint allowBits = channel.GetPermissionsRule(role).AllowValue & ~getBit(e.Channel, e.Message, "readmessages", 0);
						uint denyBits = channel.GetPermissionsRule(role).DenyValue | getBit(e.Channel, e.Message, "readmessages", 0);
						await channel.AddPermissionsRule(role, new ChannelPermissionOverrides(allowBits, denyBits));
					}
					else
					{
						User user = e.Server.GetUser(overwrite.TargetId);
						uint allowBits = channel.GetPermissionsRule(user).AllowValue & ~getBit(e.Channel, e.Message, "readmessages", 0);
						uint denyBits = channel.GetPermissionsRule(user).DenyValue | getBit(e.Channel, e.Message, "readmessages", 0);
						await channel.AddPermissionsRule(user, new ChannelPermissionOverrides(allowBits, denyBits));
					}
				}

				//Determine the highest position (kind of backwards, the lower the closer to the top, the higher the closer to the bottom)
				int highestPosition = 0;
				foreach (Channel channelOnServer in e.Server.AllChannels)
				{
					if (channelOnServer.Position > highestPosition)
					{
						highestPosition = channelOnServer.Position;
					}
				}

				await channel.Edit(position: highestPosition);
				await sendMessage(channel, "Successfully softdeleted this channel. Only admins and the owner will be able to read anything on this channel.");
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">softdeletechannel [#Channel]", "Manage channels and manage roles",
				"Makes most roles unable to read the channel and moves it to the bottom of the channel list. Only works for text channels.");
		}
		//
		//Delete channel
		private void RegisterDeleteChannel()
		{
			String COMMAND_NAME = "deletechannel";
			String[] COMMAND_ALIASES = { "dch" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Channel and Type", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageChannels(e.Server, e.User))
					return;
				attCount();

				//Check if valid channel
				Channel channel = getChannel(e.Server, e.GetArg(0), null);
				if (channel == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_ERROR), WAIT_TIME);
					return;
				}

				//See if the user can see and thus edit that channel
				if (!getChannelEditAbility(channel, e.User))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_PERMISSIONS_ERROR), WAIT_TIME);
					return;
				}

				String channelString = channel.Name;
				await channel.Delete();
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully deleted `{0}` ({1}).", channelString, channel.Type), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, String.Format(">deletechannel {0}", CHANNEL_INSTRUCTIONS), "Manage channels",
				"Deletes the channel. Deleting a voice channel requires '[Channel] [Voice]' since it is not possible to mention a voice channel.");
		}
		//
		//Change permissions of a channel
		private void RegisterChannelPerm()
		{
			String COMMAND_NAME = "channelpermissions";
			String[] COMMAND_ALIASES = { "chp" };
			String[] BAD_JOKE = { "chp", "cheesepizza" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(BAD_JOKE)
			.Parameter("Input", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!(userHasManageChannels(e.Server, e.User) && userHasManageRoles(e.Server, e.User)))
					return;
				attCount();

				//Set the variables
				List<String> permissions = null;
				String overwriteString = "";
				String roleOrUser = null;
				Channel channel = null;
				User user = null;
				Role role = null;

				//Split the input
				String input = e.GetArg(0).ToLower().Trim();
				String[] values = input.Split(new char[] { ' ' }, 2);
				if (values.Length == 0)
				{
					return;
				}
				String actionName = values[0];
				List<String> permissionTypeStrings = getPermissionNames((VOICE_PERMISSIONS.RawValue | TEXT_PERMISSIONS.RawValue) & ~(1U << getPermissionValue("administrator"))).ToList();
				if (actionName.Equals("show"))
				{
					//If only show, take that as a person wanting to see the permission types
					if (values.Length == 1)
					{
						await sendMessage(e.Channel, String.Format("**CHANNEL PERMISSION TYPES:**```\n{0}```", String.Join("\n", permissionTypeStrings)));
						return;
					}

					//Check for valid channel
					values = values[1].Split(new char[] { ' ' }, 2);
					channel = getChannel(e.Server, values[0], null);
					if (channel == null)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_ERROR), WAIT_TIME);
						return;
					}

					//See if the user can see and thus edit that channel
					if (!getChannelEditAbility(channel, e.User))
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("You do not have the ability to see that channel's permissions"), WAIT_TIME);
						return;
					}

					//Say the overwrites on a channel
					if (values.Length == 1)
					{
						List<String> overwrites = new List<String>();
						foreach (Channel.PermissionOverwrite overwrite in channel.PermissionOverwrites)
						{
							if (overwrite.TargetType == PermissionTarget.Role)
							{
								overwrites.Add(e.Server.GetRole(overwrite.TargetId).Name);
							}
							else
							{
								overwrites.Add(e.Server.GetUser(overwrite.TargetId).Name);
							}
						}
						await sendMessage(e.Channel, String.Format("**OVERWRITES FOR `{0}` ({1}):**```\n{2}```",
							channel.Name, channel.Type.ToString().ToUpper(), String.Join("\n", overwrites.ToArray())));
						return;
					}

					//Check if valid role or user
					role = getRole(e.Server, values[1]);
					if (role == null)
					{
						user = getUser(e.Server, values[1]);
						if (user == null)
						{
							makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Invalid role or user supplied."), WAIT_TIME);
							return;
						}
					}

					//Say the permissions of an overwrite
					foreach (Channel.PermissionOverwrite overwrite in channel.PermissionOverwrites)
					{
						if (overwrite.TargetType.Equals("role"))
						{
							getChannelPermissions(overwrite, channel.Type).ToList().ForEach(kvp => overwriteString += kvp.Key + ": " + kvp.Value + '\n');
							await sendMessage(e.Channel, String.Format("**PERMISSIONS FOR `{0}` ON `{1}` ({2}):**```\n{3}```",
								role.Name, channel.Name, channel.Type.ToString().ToUpper(), overwriteString));
							return;
						}
						else
						{
							getChannelPermissions(overwrite, channel.Type).ToList().ForEach(kvp => overwriteString += kvp.Key + ": " + kvp.Value + '\n');
							await sendMessage(e.Channel, String.Format("**PERMISSIONS FOR `{0}` ON `{1}` ({2}):**```\n{3}```",
								user.Mention, channel.Name, channel.Type.ToString().ToUpper(), overwriteString));
							return;
						}
					}
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(String.Format("Unable to show permissions for `{0}` on `{1}` ({2}).",
						roleOrUser, channel.Name, channel.Type.ToString())), WAIT_TIME);
					return;
				}
				else if (actionName.Equals("allow") || actionName.Equals("deny") || actionName.Equals("inherit"))
				{
					values = values[1].Split(new char[] { ' ' }, 2);

					//Check if valid number of arguments
					if (values.Length == 1)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ARGUMENTS_ERROR), WAIT_TIME);
						return;
					}

					//Check if valid channel and/or type
					channel = getChannel(e.Server, values[0], null);
					if (channel == null)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_ERROR), WAIT_TIME);
						return;
					}

					//See if the user can see and thus edit that channel
					if (!getChannelEditAbility(channel, e.User))
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_PERMISSIONS_ERROR), WAIT_TIME);
						return;
					}

					//Check if valid role/user and permissions
					if ((!getRoleAndPermissions(e.Server, values[1], out role, out permissions, out roleOrUser))
						&& (!getUserAndPermissions(e.Server, values[1], out user, out permissions, out roleOrUser)))
					{
						if (permissions == null)
						{
							makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("No permissions supplied."), WAIT_TIME);
						}
						else
						{
							makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Invalid role or user supplied."), WAIT_TIME);
						}
						return;
					}
				}
				else
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Invalid action."), WAIT_TIME);
					return;
				}

				//Check if valid permissions supplied
				if (permissions.Intersect(permissionTypeStrings).Count() != permissions.Count())
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(String.Format("Invalid {0} supplied.",
						(permissions.Count() - permissions.Intersect(permissionTypeStrings).Count()) == 1 ? "permission" : "permissions")), WAIT_TIME);
					return;
				}

				//Remove any attempt to change readmessages on the base channel because nothing can change that
				if (channel == e.Server.DefaultChannel && permissions.Contains("readmessages"))
				{
					permissions.RemoveAll(x => x.StartsWith("readmessages"));
				}

				//Get the permissions
				uint changeValue = 0;
				uint allowBits = 0;
				uint denyBits = 0;
				if (role != null)
				{
					allowBits = channel.GetPermissionsRule(role).AllowValue;
					denyBits = channel.GetPermissionsRule(role).DenyValue;
				}
				else
				{
					allowBits = channel.GetPermissionsRule(user).AllowValue;
					denyBits = channel.GetPermissionsRule(user).DenyValue;
				}

				//Changing the bit values
				foreach (String permission in permissions)
				{
					changeValue = getBit(e.Channel, e.Message, permission, changeValue);
				}
				if (actionName.Equals("allow"))
				{
					allowBits |= changeValue;
					denyBits &= ~changeValue;
					actionName = "allowed";
				}
				else if (actionName.Equals("inherit"))
				{
					allowBits &= ~changeValue;
					denyBits &= ~changeValue;
					actionName = "inherited";
				}
				else
				{
					allowBits &= ~changeValue;
					denyBits |= changeValue;
					actionName = "denied";
				}

				//Change the permissions
				String roleNameOrUsername;
				if (role != null)
				{
					await channel.AddPermissionsRule(role, new ChannelPermissionOverrides(allowBits, denyBits));
					roleNameOrUsername = role.Name;
				}
				else
				{
					await channel.AddPermissionsRule(user, new ChannelPermissionOverrides(allowBits, denyBits));
					roleNameOrUsername = user.Name + "#" + user.Discriminator;
				}

				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully {0} `{1}` for `{2}` on `{3}` ({4})",
					actionName, String.Join("`, `", permissions), roleNameOrUsername, channel.Name, channel.Type), 7500);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">channelpermissions [Show|Allow|Inherit|Deny] [#Channel|Channel/[Voice|Text]] [Role|User] <Permission/...>",
				"Manage channels and manage roles", "Type '>chp [Show]' to see the available permissions. Permissions must be separated by a '/'! " +
				"Type '>chp [Show] [Channel]' to see all permissions on a channel. Type '>chp [Show] [Channel] [Role|User]' to see permissions a role/user has on a channel.");
		}
		//
		//Copy channel permissions
		private void RegisterCopyChannelPerms()
		{
			String COMMAND_NAME = "copychannelpermissions";
			String[] COMMAND_ALIASES = { "cchp" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Channels", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!(userHasManageChannels(e.Server, e.User) && userHasManageRoles(e.Server, e.User)))
					return;
				attCount();

				//Get arguments
				String[] input = e.GetArg(0).ToLower().Split(new char[] { ' ' }, 3);

				//Separating the channels
				Channel inputChannel = getChannel(e.Server, input[0], null);
				if (inputChannel == null)
				{
					return;
				}
				Channel outputChannel = getChannel(e.Server, input[1], null);
				if (outputChannel == null)
				{
					return;
				}

				//See if the user can see and thus edit that channel
				if (!getChannelEditAbility(outputChannel, e.User))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_PERMISSIONS_ERROR), WAIT_TIME);
					return;
				}

				//Copy the selected target
				String target;
				if (input[2].Equals("all"))
				{
					target = "ALL";
					foreach (Channel.PermissionOverwrite permissionOverwrite in inputChannel.PermissionOverwrites)
					{
						if (permissionOverwrite.TargetType.Equals("role"))
						{
							Role tempRole = e.Server.GetRole(permissionOverwrite.TargetId);
							await outputChannel.AddPermissionsRule(tempRole, new ChannelPermissionOverrides(inputChannel.GetPermissionsRule(tempRole)));
						}
						else
						{
							User tempUser = e.Server.GetUser(permissionOverwrite.TargetId);
							await outputChannel.AddPermissionsRule(tempUser, new ChannelPermissionOverrides(inputChannel.GetPermissionsRule(tempUser)));
						}
					}
				}
				else
				{
					Role role = getRole(e.Server, input[1]);
					if (role != null)
					{
						target = role.Name;
						await outputChannel.AddPermissionsRule(role, new ChannelPermissionOverrides(inputChannel.GetPermissionsRule(role)));
					}
					else
					{
						User user = getUser(e.Server, input[1]);
						if (user != null)
						{
							target = user.Name;
							await outputChannel.AddPermissionsRule(user, new ChannelPermissionOverrides(inputChannel.GetPermissionsRule(user)));
						}
						else
						{
							makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("No valid role/user or all input."), WAIT_TIME);
							return;
						}
					}
				}

				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully copied `{0}` from `{1}` ({2}) to `{3}` ({4})",
					target, inputChannel.Name, inputChannel.Type, outputChannel.Name, outputChannel.Type), 7500);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">copychannelpermissions [Channel]/[Channel] [Role|User|All]", "Manage channels and manage roles",
				"Copy permissions from one channel to another. Works for a role, a user, or everything.");
		}
		//
		//Clear channel permissions
		private void RegisterClearChannelPerms()
		{
			String COMMAND_NAME = "clearchannelpermissions";
			String[] COMMAND_ALIASES = { "clchp" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Channel and Type", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!(userHasManageChannels(e.Server, e.User) && userHasManageRoles(e.Server, e.User)))
					return;
				attCount();

				//Check if channel exists
				Channel channel = getChannel(e.Server, e.GetArg(0), null);
				if (channel == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_ERROR), WAIT_TIME);
					return;
				}

				//See if the user can see and thus edit that channel
				if (!getChannelEditAbility(channel, e.User))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_PERMISSIONS_ERROR), WAIT_TIME);
					return;
				}

				//Check if channel has permissions to clear
				if (channel.PermissionOverwrites.Count() < 1)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Channel has no permissions to clear."), WAIT_TIME);
					return;
				}

				await clearChannelPermissions(e.Server, channel);
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully removed all channel permissions from {0} {1}.",
					(channel.Type == ChannelType.Voice ? "voice channel" : "text channel"),
					(channel.Type == ChannelType.Voice ? "`" + channel.Name + "`" : channel.Mention)),
					7500);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, String.Format(">clearchannelpermissions {0}", CHANNEL_INSTRUCTIONS), "Manage channels and manage roles",
				"Removes all permissions set on a channel.");
		}
		//
		//Change name of a channel
		private void RegisterChangeChannelName()
		{
			String COMMAND_NAME = "changechannelname";
			String[] COMMAND_ALIASES = { "cchn" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Channel and Text", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageChannels(e.Server, e.User))
					return;
				attCount();

				String[] input = e.GetArg(0).Split(new char[] { ' ' }, 2);
				if (input.Length != 2)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ARGUMENTS_ERROR), WAIT_TIME);
					return;
				}

				//Checking if valid name
				if (input[1].Contains(' '))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("No spaces allowed."), WAIT_TIME);
					return;
				}
				if (input[1].Length < 2 || input[1].Length > 100)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Name must be between 2 and 100 characters."), WAIT_TIME);
					return;
				}

				//Check if valid channel
				Channel channel = getChannel(e.Server, input[0], null);
				if (channel == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_ERROR), WAIT_TIME);
					return;
				}

				//See if the user can see and thus edit that channel
				if (!getChannelEditAbility(channel, e.User))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_PERMISSIONS_ERROR), WAIT_TIME);
					return;
				}

				String previousName = channel.Name;
				await channel.Edit(name: input[1]);
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully changed channel `{0}` to `{1}`.", previousName, input[1]), 5000);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, String.Format(">changechannelname {0} [New Name]", CHANNEL_INSTRUCTIONS), "Manage channels",
				"Changes the name of the channel. The new name CANNOT contain any spaces, use underscores or dashes instead.");
		}
		//
		//Change topic of a channel
		private void RegisterChangeChannelTopic()
		{
			String COMMAND_NAME = "changechanneltopic";
			String[] COMMAND_ALIASES = { "ccht" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Channel and Topic", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageChannels(e.Server, e.User))
					return;
				attCount();

				String[] input = e.GetArg(0).Split(new char[] { ' ' }, 2);
				if (input.Length != 2)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ARGUMENTS_ERROR), WAIT_TIME);
					return;
				}
				String newTopic = input[1];

				//See if valid length
				if (newTopic.Length > TOPIC_LENGTH)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Topics cannot be longer than 1024 characters in length."), WAIT_TIME);
					return;
				}

				//Test if valid channel
				Channel channel = getChannel(e.Server, input[0], ChannelType.Text);
				if (channel == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_ERROR), WAIT_TIME);
					return;
				}

				//See if the user can see and thus edit that channel
				if (!getChannelEditAbility(channel, e.User))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_PERMISSIONS_ERROR), WAIT_TIME);
					return;
				}

				//See what current topic is
				String currentTopic = channel.Topic;
				if (String.IsNullOrWhiteSpace(currentTopic))
				{
					currentTopic = "NOTHING";
				}

				await channel.Edit(topic: newTopic);
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully changed the topic in `{0}` from `{1}` to `{2}`.",
					channel.Name, currentTopic, newTopic == "" ? "NOTHING" : newTopic), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">changechanneltopic [#Channel] [New Topic]", "Manage channels",
				"Changes the subtext of a channel to whatever is input.");
		}
		//
		//Create instant invite for channel
		private void RegisterCreateInstantInvite()
		{
			String COMMAND_NAME = "createinstantinvite";
			String[] COMMAND_ALIASES = { "crinv" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Channel", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasCreateInstantInvite(e.Server, e.User))
					return;
				attCount();

				String[] input = e.GetArg(0).Split(new char[] { ' ' }, 4);
				if (input.Length != 4)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ARGUMENTS_ERROR), WAIT_TIME);
					return;
				}

				//Check validity of channel
				Channel channel = getChannel(e.Server, input[0], null);
				if (channel == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_ERROR), WAIT_TIME);
					return;
				}
				if (!e.User.Channels.Contains(channel))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("You do not have the ability to create an invite for that channel."), WAIT_TIME);
					return;
				}

				//Set the time in seconds
				Int32 time = 0;
				Int32? nullableTime = null;
				if (Int32.TryParse(input[1], out time))
				{
					Int32[] validTimes = { 1800, 3600, 21600, 43200, 86400 };
					if (validTimes.Contains(time))
					{
						nullableTime = time;
					}
				}

				//Set the max amount of users
				int users = 0;
				int? nullableUsers = null;
				if (int.TryParse(input[2], out users))
				{
					int[] validUsers = { 1, 5, 10, 25, 50, 100 };
					if (validUsers.Contains(users))
					{
						nullableUsers = users;
					}
				}

				//Set tempmembership
				bool tempMembership = false;
				if (input[3].Equals("true"))
				{
					tempMembership = true;
				}

				//Make into valid invite link
				Invite inv = await channel.CreateInvite(nullableTime, nullableUsers, tempMembership);
				Uri uri = new Uri(inv.Url);
				String localPath = uri.LocalPath.Replace("//", "/");
				String link = uri.Scheme + "://" + uri.Host + localPath;

				await sendMessage(e.Channel, String.Format("Here is your invite for {0}: {1}. It will last for{2}, {3} users{4}",
					channel.Type.Equals(ChannelType.Voice) ? "`" + channel.Name + "` (VOICE)" : channel.Mention,
					link,
					nullableTime == null ? "ever" : " " + time.ToString() + " seconds",
					nullableUsers == null ? (tempMembership ? "has no limit of users" : " and has no limit of users") :
											(tempMembership ? "has a limit of " + users.ToString() : " and has a limit of " + users.ToString()),
					tempMembership ? ", and users will only receive temporary membership." : "."));
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES,
				String.Format(">createinstantinvite {0} [Forever|1800|3600|21600|43200|86400] [Infinite|1|5|10|25|50|100] [True|False]", CHANNEL_INSTRUCTIONS),
				"Create instant invites",
				"The first argument is the channel. The second is how long the invite will last for. " +
				"The third is how many users can use the invite. The fourth is the temporary membership option.");
		}
		//
		//Move a user in voice chat
		private void RegisterMoveUser()
		{
			String COMMAND_NAME = "moveuser";
			String[] COMMAND_ALIASES = { "mu" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("User and Channel", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasMoveMembers(e.Server, e.User))
					return;
				attCount();

				//Input and splitting
				String[] input = e.GetArg(0).Split(new char[] { ' ' }, 2);

				//Check if valid user
				User user = getUser(e.Server, input[0]);
				if (user == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(USER_ERROR), WAIT_TIME);
					return;
				}

				//Check if user is in a voice channel
				if (user.VoiceChannel == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, "User is not in a voice channel.", WAIT_TIME);
					return;
				}

				//Check if valid channel
				Channel channel = getChannel(e.Server, input[1], ChannelType.Voice);
				if (channel == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_ERROR), WAIT_TIME);
					return;
				}

				//See if the user and the bot can move people into that channel
				if (!getChannelEditAbility(channel, e.User))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("You are not allowed to move users into that channel."), WAIT_TIME);
					return;
				}
				if (!getChannelEditAbility(channel, e.Server.GetUser(mDiscord.CurrentUser.Id)))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Bot is not allowed to move users into that channel."), WAIT_TIME);
					return;
				}

				//See if trying to put user in the exact same channel
				if (user.VoiceChannel == channel)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("User is already in that channel"), WAIT_TIME);
					return;
				}

				await user.Edit(voiceChannel: channel);
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully moved {0} to `{1}`.", user.Mention, channel.Name), 5000);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">moveuser [@User] [Channel]", "Move members", "Moves the user to the given voice channel.");
		}
		//
		//Mute a user in voice chat
		private void RegisterVoiceMuteUser()
		{
			String COMMAND_NAME = "mute";
			String[] COMMAND_ALIASES = { "m" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("User", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasMuteMembers(e.Server, e.User))
					return;
				attCount();

				//Test if valid user mention
				User user = getUser(e.Server, e.GetArg(0));
				if (user == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(USER_ERROR), WAIT_TIME);
					return;
				}

				//See if it should mute or unmute
				if (!user.IsServerMuted)
				{
					await user.Edit(true);
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully server muted {0}.", user.Mention), WAIT_TIME);
					return;
				}
				await user.Edit(isMuted: false);
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully removed the server mute on {0}.", user.Mention), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">voicemuteuser [@User]",
				"Mute members", "If the user is not server muted, this will mute them. If they are server muted, this will unmute them.");
		}
		//
		//Deafen a user in voice chat
		private void RegisterDeafenUser()
		{
			String COMMAND_NAME = "deafen";
			String[] COMMAND_ALIASES = { "dfn", "d" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("User", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasDeafenMembers(e.Server, e.User))
					return;
				attCount();

				//Test if valid user mention
				User user = getUser(e.Server, e.GetArg(0));
				if (user == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(USER_ERROR), WAIT_TIME);
					return;
				}

				//See if it should deafn or undeafen
				if (!user.IsServerDeafened)
				{
					await user.Edit(isDeafened: true);
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully server deafened {0}.", user.Mention), WAIT_TIME);
					return;
				}
				await user.Edit(isDeafened: false);
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully removed the server deafen on {0}.", user.Mention), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">deafen [@User]",
				"Deafen members", "If the user is not server deafened, this will deafen them. If they are server deafened, this will undeafen them.");
		}
		//
		//Nickname a user
		private void RegisterNicknameUser()
		{
			String COMMAND_NAME = "nickname";
			String[] COMMAND_ALIASES = { "nn" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("User", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageNicknames(e.Server, e.User))
					return;
				attCount();

				//Input and splitting
				String[] input = e.GetArg(0).Split(new char[] { ' ' }, 2);
				String nickname;
				if (input.Length == 2)
				{
					if (input[1].ToLower().Equals("remove"))
					{
						nickname = null;
					}
					else
					{
						nickname = input[1];
					}
				}
				else
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ARGUMENTS_ERROR), WAIT_TIME);
					return;
				}

				//Check if valid length
				if (nickname != null && nickname.Length > NICKNAME_LENGTH)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Nicknames cannot be longer than 32 characters."), WAIT_TIME);
					return;
				}

				//Check if valid user
				User user = getUser(e.Server, input[0]);
				if (user == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(USER_ERROR), WAIT_TIME);
					return;
				}

				//Checks for positions
				int nicknamePosition = getPosition(e.Server, user);
				if (nicknamePosition > (userHasOwner(e.Server, e.User) ? OWNER_POSITION : getPosition(e.Server, e.User)))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("User cannot be nicknamed by you."), WAIT_TIME);
					return;
				}
				if (nicknamePosition > getPosition(e.Server, e.Server.GetUser(mDiscord.CurrentUser.Id)))
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("User cannot be nicknamed by the bot."), WAIT_TIME);
					return;
				}

				await user.Edit(nickname: nickname);
				if (user.Nickname != null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Successfully gave the nickname `{0}` to {1}.", nickname, user.Mention), WAIT_TIME);
					return;
				}
				makeAndDeleteSecondaryMessage(e.Channel, e.Message, String.Format("Sucessfully removed the nickname from {0}.", user.Mention), WAIT_TIME);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">nickname [@User] [New Nickname|Remove]", "Manage nicknames", "Gives the user a nickname.");
		}
		//
		//Get a list of all users with a role
		private void RegisterUsersWithRole()
		{
			String COMMAND_NAME = "allwithrole";
			String[] COMMAND_ALIASES = { "awr" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Role", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasManageRoles(e.Server, e.User))
					return;
				attCount();

				//Initializing input and variables
				Role role = getRole(e.Server, e.GetArg(0));
				if (role == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ROLE_ERROR), WAIT_TIME);
					return;
				}
				List<String> users = new List<String>();
				List<String> fileUsers = new List<String>();
				int characters = 0;

				//Grab each user
				foreach (User user in e.Server.Users)
				{
					if (user.HasRole(role))
					{
						users.Add(user.Mention);
						fileUsers.Add(user.Name + "#" + user.Discriminator + " ID: " + user.Id);
						characters += (user.Mention.Length + 2);
					}
				}

				//Checking if the message can fit in a single message
				if (characters > 1950)
				{
					//Get the file path
					String allUsersFile = "AWR_" + DateTime.UtcNow.ToString("MM-dd_HH-mm-ss") + ".txt";
					String path = getServerFilePath(e.Server.Id, allUsersFile);

					//Create the temporary file
					if (!File.Exists(getServerFilePath(e.Server.Id, allUsersFile)))
					{
						System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
					}

					//Write to the temporary file
					using (StreamWriter writer = new StreamWriter(path, true))
					{
						writer.WriteLine(String.Join("\n", fileUsers));
					}

					//Upload the file
					Message msg = await sendMessage(e.Channel, "**AWR:**");
					sendAndDeleteFile(e.Server, e.Channel, msg, path);
				}
				else
				{
					await sendMessage(e.Channel, String.Format("**ALL USERS WITH `{0}`:**\n{1}", role.Name, String.Join(", ", users)));
				}
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">awr [Role]", "Manage roles", "Prints out a list of all users with the specified role.");
		}
		//
		//Do an action on all users with a role
		private void RegisterForUsersWithRole()
		{
			String COMMAND_NAME = "forallwithrole";
			String[] COMMAND_ALIASES = { "fawr" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Role and Role", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasAdmin(e.Server, e.User))
					return;
				attCount();

				//Separating input
				String[] input = e.GetArg(0).Split(new char[] { ' ' }, 3);
				String action = input[0];
				if (input.Length < 2)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ARGUMENTS_ERROR), WAIT_TIME);
					return;
				}

				//Check if valid length of roles input
				String[] values = input[1].Split('/');
				if (values.Length != 2)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ARGUMENTS_ERROR), WAIT_TIME);
					return;
				}

				//Get the max number of users allowed
				int maxLength = 100;
				if (input[2].ToLower().Equals("i_understand"))
				{
					maxLength = int.MaxValue;
				}

				if (action.Equals("give"))
				{
					if (values[0].Equals(values[1]))
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Cannot give the same role that is being gathered."), WAIT_TIME);
						return;
					}

					//Check if valid roles
					Role roleToGather = getRole(e.Server, values[0]);
					if (roleToGather == null)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Invalid role to gather."), WAIT_TIME);
						return;
					}

					//Get the roles and their edit ability
					Role roleToGive = getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, values[1]);
					if (roleToGive == null)
					{
						return;
					}

					//Check if trying to give @everyone
					if (roleToGive.IsEveryone)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, "You can't give the `@everyone` role.", WAIT_TIME);
						return;
					}

					//Grab each user and give them the role
					List<User> listUsersWithRole = new List<User>();
					foreach (User user in e.Server.Users)
					{
						if (user.HasRole(roleToGather))
						{
							listUsersWithRole.Add(user);
						}
					}

					//Checking if too many users listed
					if (listUsersWithRole.Count() > maxLength)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Too many users; max is 250."), WAIT_TIME);
						return;
					}
					foreach (User user in listUsersWithRole)
					{
						await giveRole(user, roleToGive);
					}

					await sendMessage(e.Channel, String.Format("Successfully gave `{0}` to all users{1}{2} ({3} users).",
						roleToGive.Name, roleToGather.IsEveryone ? "" : " with ", roleToGather.IsEveryone ? "" : "`" + roleToGather.Name + "`", listUsersWithRole.Count()));
				}
				else if (action.Equals("take"))
				{
					//Check if valid roles
					Role roleToGather = getRole(e.Server, values[0]);
					if (roleToGather == null)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Invalid role to gather."), WAIT_TIME);
						return;
					}
					Role roleToTake = getRoleEditAbility(e.Server, e.Channel, e.Message, e.User, values[1]);
					if (roleToTake == null)
					{
						return;
					}

					//Check if trying to take @everyone
					if (roleToTake.IsEveryone)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, "You can't take the `@everyone` role.", WAIT_TIME);
						return;
					}

					//Grab each user and give them the role
					List<User> listUsersWithRole = new List<User>();
					foreach (User user in e.Server.Users)
					{
						if (user.HasRole(roleToGather))
						{
							listUsersWithRole.Add(user);
						}
					}

					//Checking if too many users listed
					if (listUsersWithRole.Count() > maxLength)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Too many users; max is 250."), WAIT_TIME);
						return;
					}
					foreach (User user in listUsersWithRole)
					{
						await takeRole(user, roleToTake);
					}

					await sendMessage(e.Channel, String.Format("Successfully took `{0}` from all users{1}{2} ({3} users).",
						roleToTake.Name, roleToGather.IsEveryone ? "" : " with ", roleToGather.IsEveryone ? "" : "`" + roleToGather.Name + "`", listUsersWithRole.Count()));
				}
				else if (action.Equals("nickname"))
				{
					//Check if valid role
					Role roleToGather = getRole(e.Server, values[0]);
					if (roleToGather == null)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Invalid role to gather."), WAIT_TIME);
						return;
					}

					//Check if valid nickname length
					String inputNickname = values[1];
					if (inputNickname.Length > NICKNAME_LENGTH)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Nicknames cannot be longer than 32 charaters."), WAIT_TIME);
						return;
					}

					//Rename each user who has the role
					int botPosition = getPosition(e.Server, e.Server.GetUser(mDiscord.CurrentUser.Id));
					int commandUserPosition = userHasOwner(e.Server, e.User) ? OWNER_POSITION : getPosition(e.Server, e.User);
					List<User> listUsersWithRole = new List<User>();
					foreach (User user in e.Server.Users)
					{
						if (user.HasRole(roleToGather))
						{
							int userPosition = getPosition(e.Server, user);
							if (userPosition < commandUserPosition && userPosition < botPosition && e.Server.Owner.Id != user.Id)
							{
								listUsersWithRole.Add(user);
							}
						}
					}

					//Checking if too many users listed
					if (listUsersWithRole.Count() > maxLength)
					{
						makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Too many users; max is 250."), WAIT_TIME);
						return;
					}
					foreach (User user in listUsersWithRole)
					{
						await user.Edit(nickname: inputNickname);
					}

					await sendMessage(e.Channel, String.Format("Successfully gave the nickname `{0}` to all users{1}{2} ({3} users).",
						inputNickname, roleToGather.IsEveryone ? "" : " with ", roleToGather.IsEveryone ? "" : "`" + roleToGather.Name + "`", listUsersWithRole.Count()));
				}
				else
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR("Invalid action."), WAIT_TIME);
					return;
				}
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">fawr [Give|Take|Nickname] [Role]/[Role|Nickname]", "Administrator",
				"Can give a role to users, take a role from users, and nickname users who have a specific role. " +
				"The bot will hit the rate limit of actions every 10 users and then have to wait for ~9 seconds. " +
				"The max limit of 100 can be bypassed by saying 'i_understand' after the last argument. \n" +
				"Do not abuse this command.");
		}
		//
		//Test
		private void RegisterTest()
		{
			String COMMAND_NAME = "test";
			String[] COMMAND_ALIASES = { "t" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Whatever", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!e.User.Id.Equals(OWNER_ID))
					return;
				attCount();

				foreach (User user in e.Server.Users)
				{
					if (user.Name.ToLower().Equals(""))
					{
						Console.WriteLine(user.Discriminator + " id " + user.Id);
					}
				}

				await sendMessage(e.Channel, "cat");
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">test [???]", "Bot owner",
				"This is a test command. It is used for testing. Here's a screenshot of the current code: http://i.imgur.com/Pi8m1UC.png.");
		}
		//--------Commands_End--------

		//----------Small_Text_Commands----------
		//
		//Server ID
		private void RegisterServerID()
		{
			String COMMAND_NAME = "serverid";
			String[] COMMAND_ALIASES = { "sid" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Do(async (e) =>
			{
				if (!userHasSomething(e.Server, e.User))
					return;
				attCount();

				await sendMessage(e.Channel, String.Format("This server has the ID `{0}`.", e.Server.Id));
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">serverid",
				"Any", "Shows the ID of the server.");
		}
		//
		//Channel ID
		private void RegisterChannelID()
		{
			String COMMAND_NAME = "channelid";
			String[] COMMAND_ALIASES = { "cid" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Channel", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasSomething(e.Server, e.User))
					return;
				attCount();

				Channel channel = getChannel(e.Server, e.GetArg(0), null);
				if (channel == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(CHANNEL_ERROR), WAIT_TIME);
					return;
				}
				await sendMessage(e.Channel, String.Format("The {0} channel `{1}` has the ID `{2}`.", channel.Type, channel.Name, channel.Id));
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, String.Format(">channelid {0}", CHANNEL_INSTRUCTIONS),
				"Any", "Shows the ID of the given channel.");
		}
		//
		//Role ID
		private void RegisterRoleID()
		{
			String COMMAND_NAME = "roleid";
			String[] COMMAND_ALIASES = { "rid" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("Role", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasSomething(e.Server, e.User))
					return;
				attCount();

				Role role = getRole(e.Server, e.GetArg(0));
				if (role == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(ROLE_ERROR), WAIT_TIME);
					return;
				}
				await sendMessage(e.Channel, String.Format("The role `{0}` has the ID `{1}`.", role.Name, role.Id));
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">roleid [Role]",
				"Any", "Shows the ID of the given role.");
		}
		//
		//User ID
		private void RegisterUserID()
		{
			String COMMAND_NAME = "userid";
			String[] COMMAND_ALIASES = { "uid" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("User", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasSomething(e.Server, e.User))
					return;
				attCount();

				User user = getUser(e.Server, e.GetArg(0));
				if (user == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(USER_ERROR), WAIT_TIME);
					return;
				}
				await sendMessage(e.Channel, String.Format("{0} has the ID `{1}`.", user.Mention, user.Id));
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">userid [@User]",
				"Any", "Shows the ID of the given user.");
		}
		//
		//User Info
		private void RegisterUserInfo()
		{
			String COMMAND_NAME = "userinfo";
			String[] COMMAND_ALIASES = { "uinf" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Parameter("User", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				if (!userHasSomething(e.Server, e.User))
					return;
				attCount();

				User user = getUser(e.Server, e.GetArg(0));
				if (user == null)
				{
					makeAndDeleteSecondaryMessage(e.Channel, e.Message, ERROR(USER_ERROR), WAIT_TIME);
					return;
				}
				List<String> roles = new List<String>();
				foreach (Role role in user.Roles)
				{
					roles.Add(role.Name);
				}
				roles.Remove("@everyone");
				List<String> channels = new List<String>();
				foreach (Channel channel in user.Channels)
				{
					if (channel.Type.Equals("voice"))
					{
						channels.Add(channel.Name + " (voice)");
					}
					else
					{
						channels.Add(channel.Name);
					}
				}
				await sendMessage(e.Channel, String.Format(
					"{0}```" +
					"\nUsername: {1}#{2}" +
					"\nID: {3}" +
					"\n" +
					"\nNickname: {4}" +
					"\nJoined: {5}" +
					"\nLast online: {6}" +
					"\nRoles: {7}" +
					"\nAble to access: {8}" +
					"\n" +
					"\nIn voice channel: {9}" +
					"\nServer mute: {10}" +
					"\nServer deafen: {11}" +
					"\nSelf mute: {12}" +
					"\nSelf deafen: {13}" +
					"\n" +
					"\nCurrent game: {14}" +
					"\nAvatar URL: {15}" +
					"\nOnline status: {16}```",
					user.Mention,
					user.Name, user.Discriminator,
					user.Id,
					user.Nickname == null ? "N/A" : user.Nickname,
					user.JoinedAt.ToString(),
					user.LastOnlineAt == null ? "N/A" : user.LastActivityAt.ToString(),
					roles.Count() == 0 ? "N/A" : String.Join(", ", roles),
					channels.Count() == 0 ? "N/A" : String.Join(", ", channels),
					user.VoiceChannel == null ? "N/A" : user.VoiceChannel.ToString(),
					user.IsServerMuted,
					user.IsServerDeafened,
					user.IsSelfMuted,
					user.IsSelfDeafened,
					user.CurrentGame == null ? "N/A" : user.CurrentGame.Value.Name.ToString(),
					user.AvatarUrl, user.Status));
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">userinfo [@User]",
				"Any", "Displays various information about the user gotten from the server.");
		}
		//
		//Bot info
		private void RegisterBotInfo()
		{
			String COMMAND_NAME = "botinfo";
			String[] COMMAND_ALIASES = { "binf" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Do(async (e) =>
			{
				if (!userHasAdmin(e.Server, e.User))
					return;

				TimeSpan span = DateTime.UtcNow.Subtract(startupTime);

				await sendMessage(e.Channel, String.Format(
					"```" +
					"\nOnline since: {0}" +
					"\nUptime: {1}:{2}:{3}:{4}" +
					"\nTotal server count: {5}" +
					"\nTotal member count: {6}" +
					"\n" +
					"\nAttempted commands: {7}" +
					"\nSuccessful commands: {8}" +
					"\nFailed commands: {9}" +
					"\n" +
					"\nLogged deletes: {10}" +
					"\nLogged edits: {11}" +
					"\nLogged joins: {12}" +
					"\nLogged leaves: {13}" +
					"\nLogged user changes: {14}" +
					"\nLogged bans: {15}" +
					"\nLogged unbans: {16}" +
					"```",
					startupTime,
					span.Days, span.Hours.ToString("00"), span.Minutes.ToString("00"), span.Seconds.ToString("00"),
					totalServers,
					totalUsers,
					attemptedCommands,
					successfulCommands,
					attemptedCommands - successfulCommands,
					loggedDeletes,
					loggedEdits,
					loggedJoins,
					loggedLeaves,
					loggedUserChanged,
					loggedBans,
					loggedUnbans));
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">botinfo",
				"Administrator", "Shows some information about this bot.");
		}
		//
		//Send preferences file
		private void RegisterCurrentPreferences()
		{
			String COMMAND_NAME = "currentpreferences";
			String[] COMMAND_ALIASES = { "cpr" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Do(async (e) =>
			{
				if (!userHasAdmin(e.Server, e.User))
					return;
				attCount();

				Message message = await sendMessage(e.Channel, "Current server permissions:");
				sendFile(e.Server, e.Channel, message, PREFERENCES_FILE);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">currentpreferences",
				"Administrator", "Sends the file containing the current preferences.");
		}
		//
		//Send ban list file
		private void RegisterCurrentBanList()
		{
			String COMMAND_NAME = "currentbanlist";
			String[] COMMAND_ALIASES = { "cbl" };
			mCommands.CreateCommand(COMMAND_NAME)
			.Alias(COMMAND_ALIASES)
			.Do(async (e) =>
			{
				if (!userHasBanMembers(e.Server, e.User))
					return;
				attCount();

				Message message = await sendMessage(e.Channel, "Current server ban list:");
				sendFile(e.Server, e.Channel, message, BAN_REFERENCE_FILE);
				succCount();
			});
			RegisterHelp(COMMAND_NAME, COMMAND_ALIASES, ">currentbanlist",
				"Ban members", "Sends the file containing the current ban list.");
		}
		//--------Small_Text_Commands_End--------

		//----------Actions------------
		//Add to the attempted command count
		private void attCount()
		{
			++attemptedCommands;
		}
		//
		//Add to the successful command count
		private void succCount()
		{
			++successfulCommands;
		}
		//
		//Remove messages
		private async Task removeMessages(Channel channel, int requestCount)
		{
			//To remove the command itself
			++requestCount;

			//Console.WriteLine("Deleting " + requestCount + " messages.");
			while (requestCount > 0)
			{
				int deleteCount = Math.Min(MAX_MESSAGES_TO_GATHER, requestCount);
				Message[] messages = await channel.DownloadMessages(deleteCount);
				if (messages.Length == 0)
					break;
				await channel.DeleteMessages(messages);
				requestCount -= messages.Length;
			}
		}
		//
		//Remove messages given a user id
		private async Task removeMessages(Channel channel, int requestCount, User user)
		{
			//Make sure there's a user id
			if (null == user)
			{
				await removeMessages(channel, requestCount);
				return;
			}

			Console.WriteLine(String.Format("Deleting {0} messages.", requestCount));
			Message[] messages = await channel.DownloadMessages(MAX_MESSAGES_TO_GATHER);

			if (messages.Length == 0)
				return;

			//Get valid amount of messages to delete
			List<Message> list = messages.Where(x => user == x.User).ToList();
			if (requestCount > list.Count)
			{
				requestCount = list.Count;
			}
			else if (requestCount < list.Count)
			{
				list.RemoveRange(requestCount, list.Count - requestCount);
			}
			list.Insert(0, messages[0]); //Remove the initial command message

			Console.WriteLine(String.Format("Found {0} messages; deleting {1} from user {2}", messages.Length, list.Count - 1, user.Name));
			await channel.DeleteMessages(list.ToArray());
		}
		//
		//Remove command messages
		private void removeCommandMessages(Channel channel, Message[] messages, Int32 time)
		{
			Task t = Task.Run(() =>
			{
				System.Threading.Thread.Sleep(time);
				channel.DeleteMessages(messages);
			});
		}
		//
		//Wait then send a file
		private void sendFile(Server server, Channel channel, Message message, String filePath)
		{
			Task t = Task.Run(() =>
			{
				while (message.State == MessageState.Queued)
				{
					System.Threading.Thread.Sleep(100);
				}
				channel.SendFile(getServerFilePath(server.Id, filePath));
			});
		}
		//
		//Wait then send a file then delete the file
		private void sendAndDeleteFile(Server server, Channel channel, Message message, String filePath)
		{
			Task t = Task.Run(async () =>
			{
				while (message.State == MessageState.Queued)
				{
					System.Threading.Thread.Sleep(100);
				}
				Message msg = await channel.SendFile(getServerFilePath(server.Id, filePath));
				while (msg.State == MessageState.Queued)
				{
					System.Threading.Thread.Sleep(100);
				}
				File.Delete(filePath);
			});
		}
		//
		//Remove secondary messages
		private async void makeAndDeleteSecondaryMessage(Channel channel, Message curMsg, String secondStr, Int32 time)
		{
			Message secondMsg = await channel.SendMessage(ZERO_LENGTH_CHAR + secondStr);
			removeCommandMessages(channel, new Message[] { secondMsg, curMsg }, time);
		}
		//
		//Format error message
		private static String ERROR(String message)
		{
			return ZERO_LENGTH_CHAR + ERROR_MESSAGE + " " + message;
		}
		//
		//Format sendmessages with a zero width character before it
		private Task<Message> sendMessage(Channel channel, String message)
		{
			return channel.SendMessage(ZERO_LENGTH_CHAR + message);
		}
		//
		//Give roles
		private async Task giveRole(User user, Role role)
		{
			if (null == role)
				return;
			await user.AddRoles(role);
		}
		//
		//Take roles
		private async Task takeRole(User user, Role role)
		{
			if (null == role)
				return;
			await user.RemoveRoles(role);
		}
		//
		//Create roles
		private async Task<Role> createRole(Server server, User user, String roleName, ServerPermissions? permissions = null)
		{
			Role role = getRole(server, roleName);
			if (null == role)
			{
				if (userHasManageRoles(server, user))
				{
					role = await server.CreateRole(roleName, permissions);
				}
			}
			return role;
		}
		//
		//Create mute role
		private async Task<Role> createMuteRole(Server server, User user)
		{
			Role role = getRole(server, MUTE_ROLE_NAME);
			if (null == role)
			{
				role = await createRole(server, user, MUTE_ROLE_NAME, new ServerPermissions(0));
			}
			//Make it so the person getting muted can't speak or type
			foreach (Channel channel in server.TextChannels)
			{
				uint changeValue = 0;
				uint allowBits = 0;
				uint denyBits = 0;
				allowBits = channel.GetPermissionsRule(role).AllowValue & ~getBit(null, null, "sendmessages", changeValue);
				denyBits = channel.GetPermissionsRule(role).DenyValue | getBit(null, null, "sendmessages", changeValue);
				await channel.AddPermissionsRule(role, new ChannelPermissionOverrides(allowBits, denyBits));
			}
			foreach (Channel channel in server.VoiceChannels)
			{
				uint changeValue = 0;
				uint allowBits = 0;
				uint denyBits = 0;
				allowBits = channel.GetPermissionsRule(role).AllowValue & ~getBit(null, null, "speak", changeValue);
				denyBits = channel.GetPermissionsRule(role).DenyValue | getBit(null, null, "speak", changeValue);
				await channel.AddPermissionsRule(role, new ChannelPermissionOverrides(allowBits, denyBits));
			}
			return role;
		}
		//
		//Get integer
		private int getInteger(String inputString)
		{
			int number = 0;
			if (Int32.TryParse(inputString, out number))
			{
				return number;
			}
			return -1;
		}
		//
		//Get ulong
		private ulong getUlong(String inputString)
		{
			ulong number = 0;
			if (UInt64.TryParse(inputString, out number))
			{
				return number;
			}
			return 0;
		}
		//
		//Get user id
		private User getUser(Server server, String userName)
		{
			return server.GetUser(getUlong(userName.Trim(new char[] { '<', '>', '@', '!' })));
		}
		//
		//Get channel id
		private Channel getChannelID(Server server, String channelName)
		{
			Channel channel = null;
			ulong channelID = 0;
			if (UInt64.TryParse(channelName.Trim(new char[] { '<', '>', '#' }), out channelID))
			{
				channel = server.GetChannel(channelID);
			}
			return channel;
		}
		//
		//Get channel
		private Channel getChannel(Server server, String input, ChannelType type)
		{
			String[] values = input.Trim().Split(new char[] { '/' }, 2);
			String channelIDString = values[0].Trim(new char[] { '<', '#', '>' });
			ulong channelID = 0;
			Channel channel;

			//If a channel mention
			if (UInt64.TryParse(channelIDString, out channelID))
			{
				channel = server.GetChannel(channelID);
			}
			//If name and forced type
			else if (type != null)
			{
				channel = server.FindChannels(values[0], type).FirstOrDefault();
			}
			//If name and type in a single string
			else if (values.Length == 2)
			{
				channel = server.FindChannels(values[0], values[1]).FirstOrDefault();
			}
			else
			{
				return null;
			}

			return channel;
		}
		//
		//Get role id
		private Role getRole(Server server, String roleName)
		{
			return server.FindRoles(roleName).FirstOrDefault();
		}
		//
		//Get top role position from user
		private int getPosition(Server server, User user)
		{
			int position = 0;
			user.Roles.ToList().ForEach(x => position = Math.Max(position, x.Position));
			return position;
		}
		//
		//Load bans
		private void loadBans(Server server)
		{
			Dictionary<ulong, String> banList = null;
			if (mBanList.TryGetValue(server.Id, out banList))
			{
				return;
			}

			banList = new Dictionary<ulong, String>();
			mBanList[server.Id] = banList;

			String path = getServerFilePath(server.Id, BAN_REFERENCE_FILE);
			if (!System.IO.File.Exists(path))
			{
				return;
			}

			using (System.IO.StreamReader file = new System.IO.StreamReader(path))
			{
				Console.WriteLine(String.Format("{0}: bans for the server {1} have been loaded.", System.Reflection.MethodBase.GetCurrentMethod().Name, server));
				//Read the bans document for information
				String line;
				while ((line = file.ReadLine()) != null)
				{
					//If the line is empty, do nothing
					if (String.IsNullOrWhiteSpace(line))
					{
						continue;
					}
					//Split before and after the colon, before is the userID, after is the username and discriminator
					String[] values = line.Split(new char[] { ':' }, 2);
					if (values.Length == 2)
					{
						ulong userID = getUlong(values[0]);
						if (userID == 0)
						{
							continue;
						}
						banList[userID] = values[1];
					}
					else
					{
						Console.WriteLine("ERROR: " + line);
					}
				}
			}
		}
		//
		//Load preferences
		private void loadPreferences(Server server)
		{
			List<PreferenceCategory> categories;
			if (mCommandPreferences.TryGetValue(server.Id, out categories))
			{
				return;
			}

			categories = new List<PreferenceCategory>();
			mCommandPreferences[server.Id] = categories;

			String path = getServerFilePath(server.Id, PREFERENCES_FILE);
			if (!System.IO.File.Exists(path))
			{
				path = "DefaultCommandPreferences.txt";
			}

			using (System.IO.StreamReader file = new System.IO.StreamReader(path))
			{
				Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + ": preferences for the server " + server + " have been loaded.");
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
							Console.WriteLine("ERROR: " + line);
						}
					}
				}
			}
		}
		//
		//Get file paths
		private String getServerFilePath(ulong serverId, String fileName)
		{
			//Gets the appdata folder for usage, allowed to change
			String folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			//Combines the path for appdata and the preferences text file, allowed to change, but I'd recommend to keep the serverID part
			String directory = System.IO.Path.Combine(folder, "Discord_Servers", serverId.ToString());
			//This string will be similar to C:\Users\User\AppData\Roaming\ServerID
			String path = System.IO.Path.Combine(directory, fileName);
			return path;
		}
		//
		//Save preferences
		private void savePreferences(TextWriter writer, ulong serverID)
		{
			//Test if the categories exist
			List<PreferenceCategory> categories;
			if (!mCommandPreferences.TryGetValue(serverID, out categories))
			{
				return;
			}

			//If they exist, actually overwrite the new preferences file with the base preferences
			foreach (PreferenceCategory category in categories)
			{
				writer.WriteLine("@" + category.mName);
				foreach (PreferenceSetting setting in category.mSettings)
				{
					writer.WriteLine(setting.mName + ":" + setting.asString());
				}
				writer.Write("\n");
			}
		}
		//
		//Save preferences by server
		private void savePreferences(ulong serverID)
		{
			String path = getServerFilePath(serverID, PREFERENCES_FILE);
			System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
			using (System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false))
			{
				savePreferences(writer, serverID);
			}
		}
		//
		//Save bans
		private void saveBans(TextWriter writer, ulong serverID)
		{
			//Test if the bans exist
			Dictionary<ulong, String> banList;
			if (!mBanList.TryGetValue(serverID, out banList))
			{
				return;
			}

			foreach (ulong userID in banList.Keys)
			{
				writer.WriteLine(userID.ToString() + ":" + banList[userID]);
			}
		}
		//
		//Save bans by server
		private void saveBans(ulong serverID)
		{
			String path = getServerFilePath(serverID, BAN_REFERENCE_FILE);
			//Check if the location already exists
			//if (!System.IO.File.Exists(path))
			{
				System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
				using (System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false))
				{
					saveBans(writer, serverID);
				}
			}
		}
		//
		//Help command registration
		private void RegisterHelp(String name, String[] aliases, String usage, String basePerm, String text)
		{
			mHelpList.Add(new HelpEntry(name, aliases, usage, basePerm, text));
		}
		//
		//Text for the help command
		private class HelpEntry
		{
			public HelpEntry(String name, String[] aliases, String usage, String basePerm, String text)
			{
				mName = name;
				mAliases = aliases;
				mUsage = usage;
				mBasePerm = basePerm;
				mText = text;
			}

			public String Name
			{
				get { return mName; }
			}
			public String[] Aliases
			{
				get { return mAliases; }
			}
			public String Usage
			{
				get { return mUsage; }
			}
			public String basePerm
			{
				get { return mBasePerm; }
			}
			public String Text
			{
				get { return mText; }
			}

			public String FormatAliases()
			{
				return string.Join(", ", mAliases);
			}

			private String mName;
			private String[] mAliases;
			private String mUsage;
			private String mBasePerm;
			private String mText;
		}
		//
		//Categories for preferences
		private class PreferenceCategory
		{
			public PreferenceCategory(String name)
			{
				mName = name;
			}
			public String mName;
			public List<PreferenceSetting> mSettings = new List<PreferenceSetting>();
		}
		//
		//Storing the settings for preferences
		private class PreferenceSetting
		{
			public PreferenceSetting(String name, String value)
			{
				mName = name;
				mValue = value;
			}
			public String mName;
			private String mValue;

			//Return the value as a boolean
			public bool asBoolean()
			{
				String[] trueMatches = { "true", "on", "yes", "1" };
				//String[] falseMatches = { "false", "off", "no", "0" };
				return trueMatches.Any(x => String.Equals(mValue.Trim(), x, StringComparison.OrdinalIgnoreCase));
			}

			//Return the value as a string
			public String asString()
			{
				return mValue;
			}

			//Return the value as an int
			public int asInteger()
			{
				int value;
				if (Int32.TryParse(mValue, out value))
				{
					return value;
				}
				return -1;
			}
		}
		//
		//Get the commands from a category
		private String[] getCommands(Server server, int number)
		{
			List<PreferenceCategory> categories;
			if (!mCommandPreferences.TryGetValue(server.Id, out categories))
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
		//
		//Changes the text document holding the server or mod log
		private bool setServerOrModlog(Server server, Channel channel, Message message, String input, String serverOrMod)
		{
			//Get the channel
			Channel logChannel = null;
			if (String.IsNullOrWhiteSpace(input))
			{
				makeAndDeleteSecondaryMessage(channel, message, ERROR("No channel specified."), WAIT_TIME);
				return false;
			}
			else if (server.FindChannels(input).Any())
			{
				logChannel = server.FindChannels(input).FirstOrDefault();
			}
			else if (input.ToLower().Equals("off"))
			{
				logChannel = null;
			}
			else
			{
				makeAndDeleteSecondaryMessage(channel, message, ERROR("Incorrect input for channel."), WAIT_TIME);
				return false;
			}

			//Create the file if it doesn't exist
			String path = getServerFilePath(server.Id, SERVERLOG_AND_MODLOG);
			if (!File.Exists(path))
			{
				System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
				var newFile = System.IO.File.Create(path);
				newFile.Close();
			}

			//Find the lines that aren't the current serverlog line
			List<String> validLines = new List<String>();
			using (StreamReader reader = new StreamReader(path))
			{
				int counter = 0;
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (line.Contains(serverOrMod))
					{
						if ((logChannel != null) && (line.Contains(logChannel.Id.ToString())))
						{
							makeAndDeleteSecondaryMessage(channel, message, "Channel is already the current " + serverOrMod + ".", WAIT_TIME);
							return false;
						}
					}
					else if (!line.Contains(serverOrMod))
					{
						validLines.Add(line);
					}
					counter++;
				}
			}

			//Add the lines that do not include serverlog and  the new serverlog line
			using (StreamWriter writer = new StreamWriter(path))
			{
				if (logChannel == null)
				{
					writer.WriteLine(serverOrMod + ":" + null + "\n" + String.Join("\n", validLines));
					makeAndDeleteSecondaryMessage(channel, message, "Disabled the " + serverOrMod + ".", WAIT_TIME);
					return false;
				}
				else
				{
					writer.WriteLine(serverOrMod + ":" + logChannel.Id + "\n" + String.Join("\n", validLines));
				}
			}

			return true;
		}
		//
		//Checks what the serverlog is
		private Channel logChannelCheck(Server server, String serverOrMod)
		{
			String path = getServerFilePath(server.Id, SERVERLOG_AND_MODLOG);
			Channel logChannel = null;
			//Check if the file exists
			if (!File.Exists(path))
			{
				//Default to 'advobot' if it doesn't exist
				if (server.FindChannels(BASE_CHANNEL_NAME).Any())
				{
					logChannel = server.FindChannels(BASE_CHANNEL_NAME).FirstOrDefault();
					return logChannel;
				}
				//If the file and the channel both don't exist then return null
				else
					return null;
			}
			else
			{
				//Read the text document and find the serverlog 
				using (StreamReader reader = new StreamReader(path))
				{
					int counter = 0;
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						if (line.Contains("serverlog"))
						{
							String[] logChannelArray = line.Split(new Char[] { ':' }, 2);

							if (String.IsNullOrWhiteSpace(logChannelArray[1]) || (String.IsNullOrEmpty(logChannelArray[1])))
							{
								return null;
							}
							else
							{
								logChannel = server.GetChannel(Convert.ToUInt64(logChannelArray[1]));
								return logChannel;
							}
						}
						counter++;
					}
				}
			}
			return null;
		}
		//
		//Get the names of the permissions
		private static List<String> getPermissionNames()
		{
			return mPermissionValues.Keys.ToList();
		}
		//
		//Get the values of each permission
		private static List<int> getPermissionValues()
		{
			return mPermissionNames.Keys.ToList();
		}
		//
		//Get the name of a specific permission value
		private static String getPermissionName(int value)
		{
			if (mPermissionNames.ContainsKey(value))
			{
				return mPermissionNames[value];
			}
			else
			{
				return null;
			}
		}
		//
		//Get the value of a specific permission name
		private int getPermissionValue(String name)
		{
			return mPermissionValues[name];
		}
		//
		//Get the permission names of a value
		private static List<String> getPermissionNames(uint permissionValues)
		{
			List<String> permissionNames = new List<String>();
			for (int bit = 0; permissionValues != 0 && bit < 32; ++bit)
			{
				if ((permissionValues & (1 << bit)) != 0)
				{
					permissionValues &= ~(1U << bit);
					try
					{
						permissionNames.Add(getPermissionName(bit));
					}
					//Scuffed API not having those four new perms
					catch (Exception)
					{
					}
				}
			}
			return permissionNames;
		}
		//
		//Determine if the role can be edited
		private Role getRoleEditAbility(Server server, Channel channel, Message message, User user, String input)
		{
			//Check if valid role
			Role inputRole = getRole(server, input);
			if (inputRole == null)
			{
				makeAndDeleteSecondaryMessage(channel, message, ERROR(ROLE_ERROR), WAIT_TIME);
				return null;
			}

			//Determine if the user can edit this role
			int userPosition = server.Owner == user ? OWNER_POSITION : user.Roles.ToList().OrderByDescending(x => x.Position).First().Position;
			if (userPosition <= inputRole.Position)
			{
				makeAndDeleteSecondaryMessage(channel, message, ERROR(String.Format("`{0}` has a higher position than you are allowed to edit.", inputRole.Name)), WAIT_TIME);
				return null;
			}

			//Determine if the bot can edit the role
			if (getPosition(server, server.GetUser(mDiscord.CurrentUser.Id)) <= inputRole.Position)
			{
				makeAndDeleteSecondaryMessage(channel, message, ERROR(String.Format("Bot is unable to edit `{0}`.", inputRole.Name)), WAIT_TIME);
				return null;
			}

			return inputRole;
		}
		//
		//Clear permissions from a channel
		private async Task clearChannelPermissions(Server server, Channel channel)
		{
			foreach (var permission in channel.PermissionOverwrites)
			{
				if (permission.TargetType.Equals("role"))
				{
					await channel.RemovePermissionsRule(server.GetRole(permission.TargetId));
				}
				else if (permission.TargetType.Equals("member"))
				{
					await channel.RemovePermissionsRule(server.GetUser(permission.TargetId));
				}
			}
		}
		//
		//Get channel permission names and bit values
		private Dictionary<String, PermValue> getChannelPermissions(Channel.PermissionOverwrite overwrite, ChannelType channelType)
		{
			Dictionary<String, PermValue> channelPermOverridesDictionary = new Dictionary<String, PermValue>();
			PropertyInfo[] overwriteProps = typeof(Discord.Channel.PermissionOverwrite).GetProperties();
			foreach (var overwriteProp in overwriteProps)
			{
				if (overwriteProp.PropertyType == typeof(Discord.ChannelPermissionOverrides))
				{
					PropertyInfo[] overridesProps = typeof(Discord.ChannelPermissionOverrides).GetProperties();
					foreach (var overridesProp in overridesProps)
					{
						if (overridesProp.PropertyType == typeof(Discord.PermValue))
						{
							String permissionName = overridesProp.Name.ToLower();
							int permissionValue = mPermissionValues[permissionName];
							if (channelType == ChannelType.Text)
							{
								if ((TEXT_PERMISSIONS.RawValue & (1 << permissionValue)) == 0)
									continue;
							}
							else if (channelType == ChannelType.Voice)
							{
								if ((VOICE_PERMISSIONS.RawValue & (1 << permissionValue)) == 0)
									continue;
							}

							channelPermOverridesDictionary.Add(permissionName, (PermValue)overridesProp.GetValue(overwrite.Permissions));
						}
					}
				}
			}
			foreach (int permissionValue in Enum.GetValues(typeof(ChannelPermissionsNotInAPI)))
			{
				if (channelType == ChannelType.Text)
				{
					if ((TEXT_PERMISSIONS.RawValue & (1 << permissionValue)) == 0)
						continue;
				}
				else if (channelType == ChannelType.Voice)
				{
					if ((VOICE_PERMISSIONS.RawValue & (1 << permissionValue)) == 0)
						continue;
				}

				PermValue permValue;
				if ((overwrite.Permissions.AllowValue & (1 << permissionValue)) != 0)
					permValue = PermValue.Allow;
				else if ((overwrite.Permissions.DenyValue & (1 << permissionValue)) != 0)
					permValue = PermValue.Deny;
				else
					permValue = PermValue.Inherit;
				try
				{
					channelPermOverridesDictionary.Add(Enum.GetName(typeof(ChannelPermissionsNotInAPI), permissionValue), permValue);

				}
				catch (Exception)
				{
					Console.WriteLine("Wut? " + permissionValue.ToString());
				}
			}
			return channelPermOverridesDictionary;
		}
		//
		//Get bits
		private uint getBit(Channel channel, Message message, String permission, uint changeValue)
		{
			try
			{
				int bit = getPermissionValue(permission);
				changeValue |= (1U << bit);
				return changeValue;
			}
			catch (Exception)
			{
				makeAndDeleteSecondaryMessage(channel, message, ERROR(String.Format("Couldn't parse permission '{0}'", permission)), WAIT_TIME);
				return 0;
			}
		}
		//
		//Get the input string and permissions
		private bool getStringAndPermissions(String input, out String output, out List<String> permissions)
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
		//
		//Get role and permissions
		private bool getRoleAndPermissions(Server server, String input, out Role role, out List<String> permissions, out String output)
		{
			if (getStringAndPermissions(input, out output, out permissions))
			{
				role = getRole(server, output);
				return role != null;
			}

			role = null;
			return false;
		}
		//
		//Get user and permissions
		private bool getUserAndPermissions(Server server, String input, out User user, out List<String> permissions, out String output)
		{
			if (getStringAndPermissions(input, out output, out permissions))
			{
				user = getUser(server, output);
				return user != null;
			}

			user = null;
			return false;
		}
		//
		//See if the user can see this channel and edit it
		private bool getChannelEditAbility(Channel inputChannel, User user)
		{
			foreach (Channel channel in user.Channels)
			{
				if (channel.Id == inputChannel.Id)
				{
					return true;
				}
			}
			return false;
		}
		//
		//Edit message log message
		private void editMessage(Channel logChannel, String time, User user, Channel channel, String before, String after)
		{
			before = before.Replace("`", "'");
			after = after.Replace("`", "'");

			sendMessage(logChannel, String.Format("{0} **EDIT:** `{1}#{2}` **IN** `#{3}`\n**FROM:** `{4}`\n**TO:** `{5}`",
				time, user.Name, user.Discriminator, channel.Name, before, after));
		}
		//--------Actions_End----------

		//----------Checks----------
		//Owner
		private bool userHasOwner(Server server, User user)
		{
			return server.Owner == user;
		}
		//
		//Admin
		private bool userHasAdmin(Server server, User user)
		{
			return server.Owner == user || user.Roles.Any(x => x.Permissions.Administrator == true);
		}
		//
		//Manage roles
		private bool userHasManageRoles(Server server, User user)
		{
			return userHasAdmin(server, user) || user.Roles.Any(x => x.Permissions.ManageRoles == true);
		}
		//
		//Manage messages
		private bool userHasManageMessages(Server server, User user)
		{
			return userHasAdmin(server, user) || user.Roles.Any(x => x.Permissions.ManageMessages == true);
		}
		//
		//Manage channels
		private bool userHasManageChannels(Server server, User user)
		{
			return userHasAdmin(server, user) || user.Roles.Any(x => x.Permissions.ManageChannels == true);
		}
		//
		//Kick members
		private bool userHasKickMembers(Server server, User user)
		{
			return userHasAdmin(server, user) || user.Roles.Any(x => x.Permissions.KickMembers == true);
		}
		//
		//Ban members
		private bool userHasBanMembers(Server server, User user)
		{
			return userHasAdmin(server, user) || user.Roles.Any(x => x.Permissions.BanMembers == true);
		}
		//
		//Move members
		private bool userHasMoveMembers(Server server, User user)
		{
			return userHasAdmin(server, user) || user.Roles.Any(x => x.Permissions.MoveMembers == true);
		}
		//
		//Mute members
		private bool userHasMuteMembers(Server server, User user)
		{
			return userHasAdmin(server, user) || user.Roles.Any(x => x.Permissions.MuteMembers == true);
		}
		//
		//Deafen members
		private bool userHasDeafenMembers(Server server, User user)
		{
			return userHasAdmin(server, user) || user.Roles.Any(x => x.Permissions.DeafenMembers == true);
		}
		//
		//Manage nicknames
		private bool userHasManageNicknames(Server server, User user)
		{
			return userHasAdmin(server, user) || user.Roles.Any(x => x.Permissions.ManageNicknames == true);
		}
		//
		//Create instant invite
		private bool userHasCreateInstantInvite(Server server, User user)
		{
			return userHasAdmin(server, user) || user.Roles.Any(x => x.Permissions.CreateInstantInvite == true);
		}
		//
		//Anything above this
		private bool userHasSomething(Server server, User user)
		{
			return userHasAdmin(server, user)
				|| userHasManageRoles(server, user)
				|| userHasManageMessages(server, user)
				|| userHasManageChannels(server, user)
				|| userHasKickMembers(server, user)
				|| userHasBanMembers(server, user)
				|| userHasMoveMembers(server, user)
				|| userHasMuteMembers(server, user)
				|| userHasDeafenMembers(server, user)
				|| userHasManageNicknames(server, user)
				|| userHasCreateInstantInvite(server, user);
		}
		//--------Checks_End--------

		//Cx
	}
}