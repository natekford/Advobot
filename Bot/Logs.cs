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
	public class BotLogs
	{
		//When the bot turns on and a server shows up
		public static Task OnGuildAvailable(SocketGuild guild)
		{
			Console.WriteLine(String.Format("{0}: {1}#{2} is online now.", MethodBase.GetCurrentMethod().Name, guild.Name, guild.Id));
			Actions.loadPreferences(guild);
			Actions.loadBans(guild);

			//var t = Task.Run(async delegate
			//{
			//	IEnumerable<Invite> invs = await args.Server.GetInvites();
			//	List<Invite> invites = args.Server.GetInvites().Result.ToList();
			//	foreach (Invite inv in invites)
			//	{
			//		mInviteLinks[inv.Code] = inv.Uses;
			//	}
			//});

			Variables.TotalUsers += guild.MemberCount;
			Variables.TotalServers++;

			return Task.CompletedTask;
		}

		//When the bot joins a server
		public static Task OnJoinedGuild(SocketGuild guild)
		{
			Console.WriteLine(String.Format("{0}: Bot joined {1}#{2}.", MethodBase.GetCurrentMethod().Name, guild.Name, guild.Id));

			return Task.CompletedTask;
		}

		//When the bot leaves a server
		public static Task OnLeftGuild(SocketGuild guild)
		{
			Console.WriteLine(String.Format("{0}: Bot has left {1}#{2}.", MethodBase.GetCurrentMethod().Name, guild.Name, guild.Id));

			Variables.TotalUsers -= (guild.MemberCount + 1);
			Variables.TotalServers--;

			return Task.CompletedTask;
		}
	}

	public class ServerLogs
	{
		//Tell when a user joins the server
		public static async Task OnUserJoined(SocketGuildUser user)
		{
			++Variables.LoggedJoins;
			IMessageChannel logChannel = await Actions.logChannelCheck(user.Guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel != null)
			{
				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				if (user.IsBot)
				{
					await Actions.sendChannelMessage(logChannel, String.Format("{0} **BOT JOIN:** `{1}#{2}` **ID** `{3}`",
						time, user.Username, user.Discriminator, user.Id));
					return;
				}

				await Actions.sendChannelMessage(logChannel, String.Format("{0} **JOIN:** `{1}#{2}` **ID** `{3}`",
					time, user.Username, user.Discriminator, user.Id));
			}

			return;
		}
		
		//Tell when a user leaves the server
		public static async Task OnUserLeft(SocketGuildUser user)
		{
			++Variables.LoggedLeaves;
			IMessageChannel logChannel = await Actions.logChannelCheck(user.Guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel != null)
			{
				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				if (user.IsBot)
				{
					await Actions.sendChannelMessage(logChannel, String.Format("{0} **BOT LEAVE:** `{1}#{2}` **ID** `{3}`",
						time, user.Username, user.Discriminator, user.Id));
					return;
				}

				await Actions.sendChannelMessage(logChannel, String.Format("{0} **LEAVE:** `{1}#{2}` **ID** `{3}`",
					time, user.Username, user.Discriminator, user.Id));
			}

			return;
		}
		
		//Tell when a user is banned
		public static async Task OnUserBanned(SocketUser user, SocketGuild guild)
		{
			++Variables.LoggedBans;
			IMessageChannel logChannel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel != null)
			{
				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				await Actions.sendChannelMessage(logChannel, String.Format("{0} **BAN:** `{1}#{2}` **ID** `{3}`",
					time, user.Username, user.Discriminator, user.Id));
			}
			//Add the user to the ban list document
			Dictionary<ulong, String> banList = Variables.mBanList[guild.Id];
			banList[user.Id] = user.Username + "#" + user.Discriminator;
			Actions.saveBans(guild.Id);

			return;
		}
		
		//Tell when a user is unbanned
		public static async Task OnUserUnbanned(SocketUser user, SocketGuild guild)
		{
			++Variables.LoggedUnbans;
			Dictionary<ulong, String> banList = Variables.mBanList[guild.Id];

			IMessageChannel logChannel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel != null)
			{
				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				String[] usernameAndDiscriminator = banList[user.Id].Split('#');
				await Actions.sendChannelMessage(logChannel, String.Format("{0} **UNBAN:** `{1}#{2}` **ID** `{3}`",
					time, usernameAndDiscriminator[0], usernameAndDiscriminator[1], user.Id));
			}
			//Remove the user from the ban list document
			banList.Remove(user.Id);
			Actions.saveBans(guild.Id);

			return;
		}

		//Tell when a user has their name, nickname, or roles changed
		public static async Task OnGuildMemberUpdated(SocketGuildUser beforeUser, SocketGuildUser afterUser)
		{
			++Variables.LoggedUserChanges;
			IMessageChannel logChannel = await Actions.logChannelCheck(beforeUser.Guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel != null)
			{
				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				//Name change
				if (!beforeUser.Username.Equals(afterUser.Username))
				{
					await Actions.sendChannelMessage(logChannel, String.Format("{0} **NAME:** `{1}#{2}` **TO** `{3}#{4}`",
						time, beforeUser.Username, beforeUser.Discriminator, afterUser.Username, afterUser.Discriminator));
				}

				//Nickname change
				if ((String.IsNullOrWhiteSpace(beforeUser.Nickname) && !String.IsNullOrWhiteSpace(afterUser.Nickname))
					 || (!String.IsNullOrWhiteSpace(beforeUser.Nickname) && String.IsNullOrWhiteSpace(afterUser.Nickname)))
				{
					String originalNickname = beforeUser.Nickname;
					if (String.IsNullOrWhiteSpace(beforeUser.Nickname))
					{
						originalNickname = "NO NICKNAME";
					}
					String nicknameChange = afterUser.Nickname;
					if (String.IsNullOrWhiteSpace(afterUser.Nickname))
					{
						nicknameChange = "NO NICKNAME";
					}
					await Actions.sendChannelMessage(logChannel, String.Format("{0} **NICKNAME:** `{1}#{2}` **FROM** `{3}` **TO** `{4}`",
						time, beforeUser.Username, beforeUser.Discriminator, originalNickname, nicknameChange));
				}
				else if (!(String.IsNullOrWhiteSpace(beforeUser.Nickname) && String.IsNullOrWhiteSpace(afterUser.Nickname)))
				{
					if (!beforeUser.Nickname.Equals(afterUser.Nickname))
					{
						await Actions.sendChannelMessage(logChannel, String.Format("{0} **NICKNAME:** `{1}#{2}` **FROM** `{3}` **TO** `{4}`",
							time, beforeUser.Username, beforeUser.Discriminator, beforeUser.Nickname, afterUser.Nickname));
					}
				}

				//Role change
				String roles = null;
				List<ulong> firstNotSecond = beforeUser.RoleIds.ToList().Except(afterUser.RoleIds.ToList()).ToList();
				List<ulong> secondNotFirst = afterUser.RoleIds.ToList().Except(beforeUser.RoleIds.ToList()).ToList();
				if (firstNotSecond.Count() > 0)
				{
					roles = String.Join(", ", firstNotSecond);
					await Actions.sendChannelMessage(logChannel, String.Format("{0} **LOSS:** `{1}#{2}` **LOST** `{3}`",
						time, beforeUser.Username, beforeUser.Discriminator, roles));
				}
				else if (secondNotFirst.Count() > 0)
				{
					roles = String.Join(", ", secondNotFirst);
					await Actions.sendChannelMessage(logChannel, String.Format("{0} **GAIN:** `{1}#{2}` **GAINED** `{3}`",
						time, beforeUser.Username, beforeUser.Discriminator, roles));
				}

				return;
			}
		}

		//Tell when a message is edited 
		public static async Task OnMessageUpdated(Optional<SocketMessage> beforeMessage, SocketMessage afterMessage)
		{
			++Variables.LoggedEdits;
			IMessageChannel logChannel = await Actions.logChannelCheck((afterMessage.Channel as IGuildChannel).Guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel != null)
			{
				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				String beforeMsg = beforeMessage.Value.Content;
				String afterMsg = afterMessage.Content;

				//Bot cannot pick up messages from before it was started
				if (String.IsNullOrWhiteSpace(beforeMsg))
				{
					beforeMsg = "UNABLE TO BE GATHERED";
				}

				//Determine lengths for error checking
				if (beforeMsg.Length + afterMsg.Length < 1500)
				{
					await Actions.editMessage(logChannel, time, afterMessage.Author as IGuildUser, afterMessage.Channel, beforeMsg, afterMsg);
					return;
				}
				else
				{
					if (beforeMsg.Length > 750)
					{
						if (afterMsg.Length > 750)
						{
							await Actions.editMessage(logChannel, time, afterMessage.Author as IGuildUser, afterMessage.Channel, "SPAM", "SPAM");
							return;
						}
						await Actions.editMessage(logChannel, time, afterMessage.Author as IGuildUser, afterMessage.Channel, "SPAM", afterMsg);
						return;
					}
					await Actions.editMessage(logChannel, time, afterMessage.Author as IGuildUser, afterMessage.Channel, beforeMsg, "SPAM");
					return;
				}
			}
		}
		
		//Tell when a message is deleted
		public static async Task OnMessageDeleted(ulong messageID, Optional<SocketMessage> message)
		{
			++Variables.LoggedDeletes;
			IGuild guild = (message.Value.Channel as IGuildChannel).Guild;
			IUser user = message.Value.Author;
			IMessageChannel logChannel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
			if (Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING) != null)
			{
				//Got an error once time due to a null user when spam testing, so this check is here
				if (user.Equals(null))
					return;

				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				String outputMessage = String.Format("{0} **DELETED:** `{1}#{2}` **IN** `#{3}`\n```{4}```",
					time, user.Username, user.Discriminator, message.Value.Channel, message.Value.Content.Replace("`", "'"));

				//Get a list of the deleted messages per server
				List<String> mainMessages;
				if (!Variables.mDeletedMessages.TryGetValue(guild.Id, out mainMessages))
				{
					mainMessages = new List<String>();
					Variables.mDeletedMessages[guild.Id] = mainMessages;
				}
				lock (mainMessages)
				{
					mainMessages.Add(outputMessage);
					Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + " Maintask: " + mainMessages.Count());
				}

				//Use a token so the messages do not get sent prematurely
				CancellationTokenSource cancelToken;
				if (Variables.mCancelTokens.TryGetValue(guild.Id, out cancelToken))
				{
					cancelToken.Cancel();
				}
				cancelToken = new CancellationTokenSource();
				Variables.mCancelTokens[guild.Id] = cancelToken;

				//Make a separate task in order to not mess up the other commands
				var t = Task.Run(async delegate
				{
					try
					{
						//IGNORE THIS EXCEPTION OR ELSE THE BOT LOCKS EACH TIME MESSAGES ARE DELETED
						await Task.Delay(TimeSpan.FromSeconds(Constants.TIME_FOR_WAIT_BETWEEN_DELETING_MESSAGES_UNTIL_THEY_PRINT_TO_THE_SERVER_LOG), cancelToken.Token);
					}
					catch (TaskCanceledException)
					{
						Console.WriteLine("Expected exception occurred during deleting messages.");
						return;
					}
					int characterCounter = 0;
					List<String> deletedMessages;
					List<String> taskMessages = Variables.mDeletedMessages[guild.Id];
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
						await Actions.sendChannelMessage(logChannel, String.Join("\n", deletedMessages));
					}
					else
					{
						//Get the file path
						String deletedMessagesFile = "Deleted_Messages_" + DateTime.UtcNow.ToString("MM-dd_HH-mm-ss") + ".txt";
						String path = Actions.getServerFilePath(guild.Id, deletedMessagesFile);

						//Create the temporary file
						if (!File.Exists(Actions.getServerFilePath(guild.Id, deletedMessagesFile)))
						{
							System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
						}

						//Write to the temporary file
						using (StreamWriter writer = new StreamWriter(path, true))
						{
							writer.WriteLine(String.Join("\n-----\n", deletedMessages).Replace("*", "").Replace("`", ""));
						}

						//Upload the file
						IMessage msg = await Actions.sendChannelMessage(logChannel, time + "**DELETED:**");
						await logChannel.SendFileAsync(path);

						//Delete the file
						File.Delete(path);
					}
				});

				return;
			}
		}

		public class ModLogs
		{

		}
	}
}
