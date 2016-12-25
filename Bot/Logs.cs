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
	public class BotLogs
	{
		//When the bot turns on and a server shows up
		public static Task OnGuildAvailable(SocketGuild guild)
		{
			Console.WriteLine(String.Format("{0}: {1}#{2} is online now.", MethodBase.GetCurrentMethod().Name, guild.Name, guild.Id));
			Actions.loadPreferences(guild);

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
			Variables.TotalGuilds++;
			Variables.Guilds.Add(guild);

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
			Variables.TotalGuilds--;
			Variables.Guilds.Remove(guild);

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
		}

		//Tell when a user is unbanned
		public static async Task OnUserUnbanned(SocketUser user, SocketGuild guild)
		{
			++Variables.LoggedUnbans;

			IMessageChannel logChannel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel != null)
			{
				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				await Actions.sendChannelMessage(logChannel, String.Format("{0} **UNBAN:** `{1}`",
					time, user.Id));
			}
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
					await Actions.sendChannelMessage(logChannel, String.Format("{0} **NAME:** `{1}#{2}` **FROM:** `{2}` **TO** `{3}`",
						time, afterUser.Username, afterUser.Discriminator, beforeUser.Username, afterUser.Username));
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
						time, afterUser.Username, afterUser.Discriminator, originalNickname, nicknameChange));
				}
				else if (!(String.IsNullOrWhiteSpace(beforeUser.Nickname) && String.IsNullOrWhiteSpace(afterUser.Nickname)))
				{
					if (!beforeUser.Nickname.Equals(afterUser.Nickname))
					{
						await Actions.sendChannelMessage(logChannel, String.Format("{0} **NICKNAME:** `{1}#{2}` **FROM** `{3}` **TO** `{4}`",
							time, afterUser.Username, afterUser.Discriminator, beforeUser.Nickname, afterUser.Nickname));
					}
				}

				//Role change
				String roles = null;
				List<ulong> firstNotSecond = beforeUser.RoleIds.ToList().Except(afterUser.RoleIds.ToList()).ToList();
				List<ulong> secondNotFirst = afterUser.RoleIds.ToList().Except(beforeUser.RoleIds.ToList()).ToList();
				List<String> rolesChange = new List<String>();
				if (firstNotSecond.Count() > 0)
				{
					firstNotSecond.ForEach(x => rolesChange.Add(afterUser.Guild.GetRole(x).Name));
					roles = String.Join(", ", rolesChange);
					await Actions.sendChannelMessage(logChannel, String.Format("{0} **LOSS:** `{1}#{2}` **LOST** `{3}`",
						time, afterUser.Username, afterUser.Discriminator, roles));
				}
				else if (secondNotFirst.Count() > 0)
				{
					secondNotFirst.ForEach(x => rolesChange.Add(afterUser.Guild.GetRole(x).Name));
					roles = String.Join(", ", rolesChange);
					await Actions.sendChannelMessage(logChannel, String.Format("{0} **GAIN:** `{1}#{2}` **GAINED** `{3}`",
						time, afterUser.Username, afterUser.Discriminator, roles));
				}
			}
		}

		//Tell when a message is edited 
		public static async Task OnMessageUpdated(Optional<SocketMessage> beforeMessage, SocketMessage afterMessage)
		{
			++Variables.LoggedEdits;

			IMessageChannel logChannel = await Actions.logChannelCheck((afterMessage.Channel as IGuildChannel).Guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel != null && beforeMessage.IsSpecified)
			{
				if (beforeMessage.Value.Content.Equals(afterMessage.Content))
				{
					if (afterMessage.Embeds.Count() > 0
						&& afterMessage.Embeds.Count() != afterMessage.Attachments.Count()
						&& beforeMessage.Value.Embeds.Count != afterMessage.Embeds.Count())
					{
						await ImageLog(logChannel, afterMessage);
						return;
					}
				}

				String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
				String beforeMsg = Actions.replaceMessageCharacters(beforeMessage.Value.Content);
				String afterMsg = Actions.replaceMessageCharacters(afterMessage.Content);

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

			//Skip null messages
			if (!message.IsSpecified)
				return;

			//Initialize the guild and channel
			IGuild guild = (message.Value.Channel as IGuildChannel).Guild;
			IMessageChannel logChannel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);

			if (logChannel != null)
			{
				//Got an error once time due to a null user when spam testing, so this check is here
				if (message.Value.Author.Equals(null))
					return;

				//Get a list of the deleted messages per server
				List<SocketMessage> mainMessages;
				if (!Variables.DeletedMessages.TryGetValue(guild.Id, out mainMessages))
				{
					mainMessages = new List<SocketMessage>();
					Variables.DeletedMessages[guild.Id] = mainMessages;
				}
				lock (mainMessages)
				{
					mainMessages.Add(message.Value);
					Console.WriteLine(MethodBase.GetCurrentMethod().Name + " Maintask: " + mainMessages.Count());
				}

				//Use a token so the messages do not get sent prematurely
				CancellationTokenSource cancelToken;
				if (Variables.CancelTokens.TryGetValue(guild.Id, out cancelToken))
				{
					cancelToken.Cancel();
				}
				cancelToken = new CancellationTokenSource();
				Variables.CancelTokens[guild.Id] = cancelToken;

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

					int characterCount = 0;
					List<SocketMessage> deletedMessages;
					List<SocketMessage> taskMessages = Variables.DeletedMessages[guild.Id];
					lock (taskMessages)
					{
						//Give the messages to a new list so they can be removed from the old one
						deletedMessages = new List<SocketMessage>(taskMessages);

						//Get the character count
						taskMessages.ForEach(x => characterCount += (x.Content.Length));
						characterCount += taskMessages.Count * 100;

						//Clear the messages
						taskMessages.Clear();
					}
					Console.WriteLine(MethodBase.GetCurrentMethod().Name + " Deleting: " + deletedMessages.Count());

					//Sort by oldest to newest
					List<SocketMessage> deletedMessagesSorted = deletedMessages.Where(x => x.CreatedAt != null).OrderBy(x => x.CreatedAt.Ticks).ToList();
					if (Constants.NEWEST_DELETED_MESSAGES_AT_TOP)
					{
						deletedMessagesSorted.Reverse();
					}
					//Put the message content into a list of strings for easy usage
					List<String> deletedMessagesContent = new List<String>();
					deletedMessagesSorted.ForEach(x =>
					{
						deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
							x.Author.Username, x.Author.Discriminator, x.Channel, x.CreatedAt.ToString("HH:mm:ss"), Actions.replaceMessageCharacters(x.Content)));
					});

					if (deletedMessages.Count() == 0)
						return;
					else if ((deletedMessages.Count() <= 5) && (characterCount < 2000))
					{
						//If there aren't many messages send the small amount in a message instead of a file
						await Actions.sendChannelMessage(logChannel, "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]` **DELETED:**\n" + String.Join("\n", deletedMessagesContent));
					}
					else
					{
						if (!Constants.TEXT_FILE)
						{
							await Actions.sendChannelMessage(
								logChannel, "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]` **DELETED:**\n" + Actions.uploadToHastebin(logChannel, deletedMessagesContent));
						}
						else
						{
							await Actions.uploadTextFile(guild, logChannel, deletedMessagesContent, "Deleted_Messages_", "DELETED");
						}
					}
				});
			}
		}

		//Get all images uploaded
		public static async Task OnMessageReceived(SocketMessage message)
		{
			if (message == null || message.Author == null)
				return;
			if (message.Author.Id == CommandHandler.client.CurrentUser.Id)
				return;
			++Variables.LoggedMessages;

			IMessageChannel logChannel = await Actions.logChannelCheck((message.Channel as IGuildChannel).Guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel != null)
			{
				if (message.Attachments.Count() > 0 || message.Embeds.Count() > 0)
				{
					await ImageLog(logChannel, message);
				}
			}
		}

		//Logging images
		public static async Task ImageLog(IMessageChannel channel, SocketMessage message)
		{
			if (message.Author.Id == CommandHandler.client.CurrentUser.Id)
				return;

			String time = "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]`";
			List<String> URLs = new List<String>();
			if (message.Attachments.Count() > 0)
			{
				message.Attachments.ToList().ForEach(x => URLs.Add(x.Url));
			}
			if (message.Embeds.Count() > 0)
			{
				message.Embeds.ToList().ForEach(x => URLs.Add(x.Url));
			}
			await Actions.sendChannelMessage(channel, String.Format("{0} **ATTACHMENT(S):** `{1}#{2}` **URL(S):** {3}",
					time, message.Author.Username, message.Author.Discriminator, String.Join(", ", URLs)));
		}
	}

	public class ModLogs
	{

	}
}
