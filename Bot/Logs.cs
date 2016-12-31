﻿using System;
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
	public class BotLogs : ModuleBase
	{
		//The console log
		public static Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		//When the bot turns on and a server shows up
		public static Task OnGuildAvailable(SocketGuild guild)
		{
			Console.WriteLine(String.Format("{0}: {1}#{2} is online now.", MethodBase.GetCurrentMethod().Name, guild.Name, guild.Id));
			Actions.loadPreferences(guild);

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

		//Reset the server count and cumulative member count when the bot disconnects or else it doubles it
		public static Task OnDisconnected(Exception exception)
		{
			Variables.TotalGuilds = 0;
			Variables.TotalUsers = 0;

			return Task.CompletedTask;
		}
	}

	public class ServerLogs : ModuleBase
	{
		//Tell when a user joins the server
		public static async Task OnUserJoined(SocketGuildUser user)
		{
			++Variables.LoggedJoins;

			ITextChannel logChannel = await Actions.logChannelCheck(user.Guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel != null)
			{
				if (user.IsBot)
				{
					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.JOIN, description: "**ID:** " + user.Id.ToString()), "Bot Join");
					await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl));
				}
				else
				{
					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.JOIN, description: "**ID:** " + user.Id.ToString()), "Join");
					await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl));
				}
			}
		}

		//Tell when a user leaves the server
		public static async Task OnUserLeft(SocketGuildUser user)
		{
			if (user == Variables.Bot)
			{
				Variables.Guilds.Remove(user.Guild);
				return;
			}

			++Variables.LoggedLeaves;

			ITextChannel logChannel = await Actions.logChannelCheck(user.Guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel != null)
			{
				if (user.IsBot)
				{
					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.LEAVE, description: "**ID:** " + user.Id.ToString()), "Bot Leave");
					await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl));
				}
				else
				{
					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.LEAVE, description: "**ID:** " + user.Id.ToString()), "Leave");
					await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl));
				}
			}
		}

		//Tell when a user is unbanned
		public static async Task OnUserUnbanned(SocketUser user, SocketGuild guild)
		{
			++Variables.LoggedUnbans;

			ITextChannel logChannel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel != null)
			{
				//Get the username/discriminator via this dictionary since they don't exist otherwise
				String username = Variables.UnbannedUsers.ContainsKey(user.Id) ? Variables.UnbannedUsers[user.Id].Username : "null";
				String discriminator = Variables.UnbannedUsers.ContainsKey(user.Id) ? Variables.UnbannedUsers[user.Id].Discriminator : "0000";

				EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.UNBAN, description: "**ID:** " + user.Id.ToString()), "Unban");
				await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", username, discriminator), user.AvatarUrl));
			}
		}

		//Tell when a user is banned
		public static async Task OnUserBanned(SocketUser user, SocketGuild guild)
		{
			if (user == Variables.Bot)
			{
				Variables.Guilds.Remove(guild);
				return;
			}

			++Variables.LoggedBans;

			ITextChannel logChannel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel != null)
			{
				EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.BAN, description: "**ID:** " + user.Id.ToString()), "Ban");
				await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl));
			}
		}

		//Tell when a user has their name, nickname, or roles changed
		public static async Task OnGuildMemberUpdated(SocketGuildUser beforeUser, SocketGuildUser afterUser)
		{
			++Variables.LoggedUserChanges;

			ITextChannel logChannel = await Actions.logChannelCheck(beforeUser.Guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel != null)
			{
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
					//These ones are across more lines than the previous ones up above because it makes it easier to remember what is doing what
					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.UEDIT), "Nickname");
					Actions.addField(embed, "Before:", originalNickname);
					Actions.addField(embed, "After:", nicknameChange, false);
					await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl));
				}
				else if (!(String.IsNullOrWhiteSpace(beforeUser.Nickname) && String.IsNullOrWhiteSpace(afterUser.Nickname)))
				{
					if (!beforeUser.Nickname.Equals(afterUser.Nickname))
					{
						EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.UEDIT), "Nickname");
						Actions.addField(embed, "Before:", beforeUser.Nickname);
						Actions.addField(embed, "After:", afterUser.Nickname, false);
						await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl));
					}
				}

				//Role change
				List<ulong> firstNotSecond = beforeUser.RoleIds.ToList().Except(afterUser.RoleIds.ToList()).ToList();
				List<ulong> secondNotFirst = afterUser.RoleIds.ToList().Except(beforeUser.RoleIds.ToList()).ToList();
				List<String> rolesChange = new List<String>();
				if (firstNotSecond.Count > 0)
				{
					firstNotSecond.ForEach(x => rolesChange.Add(afterUser.Guild.GetRole(x).Name));

					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.UEDIT, description: "**Lost:** " + String.Join(", ", rolesChange)), "Role Loss");
					await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl));
				}
				else if (secondNotFirst.Count > 0)
				{
					secondNotFirst.ForEach(x => rolesChange.Add(afterUser.Guild.GetRole(x).Name));

					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.UEDIT, description: "**Gained:** " + String.Join(", ", rolesChange)), "Role Gain");
					await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl));
				}
			}
		}

		//Tell when a user updates their name/game/status
		public static async Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser)
		{
			++Variables.LoggedUserChanges;

			//Name change
			//TODO: Make this work
			if (!beforeUser.Username.Equals(afterUser.Username))
			{
				foreach (var guild in CommandHandler.client.Guilds.ToList().Where(x => x.Users.Contains(afterUser)).ToList())
				{
					ITextChannel logChannel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
					if (logChannel == null)
						return;

					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.UEDIT), "Name Change");
					Actions.addField(embed, "Before:", beforeUser.Username);
					Actions.addField(embed, "After:", afterUser.Username, false);
					await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl));
				}
			}
		}

		//Tell when a message is edited 
		public static async Task OnMessageUpdated(Optional<SocketMessage> beforeMessage, SocketMessage afterMessage)
		{
			if (afterMessage.Author.Id == CommandHandler.client.CurrentUser.Id)
				return;

			++Variables.LoggedEdits;

			ITextChannel logChannel = await Actions.logChannelCheck((afterMessage.Channel as IGuildChannel).Guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel != null && beforeMessage.IsSpecified)
			{
				//Check if regular messages are equal
				if (beforeMessage.Value.Embeds.Count != afterMessage.Embeds.Count)
				{
					ImageLog(logChannel, afterMessage, true);
					return;
				}

				//Set the content as strings
				String beforeMsg = Actions.replaceMessageCharacters(beforeMessage.Value.Content);
				String afterMsg = Actions.replaceMessageCharacters(afterMessage.Content);

				//Set the user as a variable to save some space
				IUser user = afterMessage.Author;

				//Bot cannot pick up messages from before it was started
				if (String.IsNullOrWhiteSpace(beforeMsg))
				{
					beforeMsg = "UNABLE TO BE GATHERED";
					if (String.IsNullOrWhiteSpace(afterMsg))
					{
						--Variables.LoggedEdits;
						return;
					}
				}
				else if (String.IsNullOrWhiteSpace(afterMsg))
				{
					afterMsg = "UNABLE TO BE GATHERED";
				}

				//Check lengths
				if (!(beforeMsg.Length + afterMsg.Length < 1800))
				{
					beforeMsg = beforeMsg.Length > 900 ? "SPAM" : beforeMsg;
					afterMsg = afterMsg.Length > 900 ? "SPAM" : afterMsg;
				}

				EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.MEDIT), "Edit");
				Actions.addField(embed, "Before:", beforeMsg);
				Actions.addField(embed, "After:", afterMsg, false);
				Actions.addAuthor(embed, String.Format("{0}#{1} in #{2}", user.Username, user.Discriminator, afterMessage.Channel), user.AvatarUrl);
				await Actions.sendEmbedMessage(logChannel, embed);
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
			ITextChannel logChannel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);

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
						//See if any embeds deleted
						if (x.Embeds.Count > 0)
						{
							//Get the embed
							Embed embed = x.Embeds.ToList().FirstOrDefault(y => y.Description != null);

							String author = embed.Author.ToString();
							if (author != null)
							{
								//I don't know how to regex well
								Regex regex = new Regex("#([0-9]*) in #");
								String[] authorAndChannel = regex.Split(author);

								deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
									authorAndChannel[0],
									authorAndChannel[1],
									authorAndChannel[2], 
									x.CreatedAt.ToString("HH:mm:ss"),
									Actions.replaceMessageCharacters(embed.Description)));
							}
							else
							{
								deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
									x.Author.Username, x.Author.Discriminator, x.Channel, x.CreatedAt.ToString("HH:mm:ss"), Actions.replaceMessageCharacters(embed.Description)));
							}
						}
						//See if any attachments were put in
						else if (x.Attachments.Count > 0)
						{
							String content = String.IsNullOrEmpty(x.Content) ? "EMPTY MESSAGE" : x.Content;
							deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
								x.Author.Username, x.Author.Discriminator, x.Channel, x.CreatedAt.ToString("HH:mm:ss"),
								Actions.replaceMessageCharacters(content + " + " + x.Attachments.ToList().First().Filename)));
						}
						//Else add the message in normally
						else
						{
							String content = String.IsNullOrEmpty(x.Content) ? "EMPTY MESSAGE" : x.Content;
							deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
								x.Author.Username, x.Author.Discriminator, x.Channel, x.CreatedAt.ToString("HH:mm:ss"), Actions.replaceMessageCharacters(content)));
						}
					});

					if (deletedMessages.Count == 0)
						return;
					else if ((deletedMessages.Count <= 5) && (characterCount < 2000))
					{
						//If there aren't many messages send the small amount in a message instead of a file or link
						EmbedBuilder embed = Actions.makeNewEmbed(Constants.MDEL, "Deleted Messages", String.Join("\n", deletedMessagesContent));
						await Actions.sendEmbedMessage(logChannel, Actions.addFooter(embed, "Deleted Messages"));
					}
					else
					{
						if (!Constants.TEXT_FILE)
						{
							//Upload the embed with the hastebin links
							EmbedBuilder embed = Actions.makeNewEmbed(Constants.MDEL, "Deleted Messages", Actions.uploadToHastebin(deletedMessagesContent));
							await Actions.sendEmbedMessage(logChannel, Actions.addFooter(embed, "Deleted Messages"));
						}
						else
						{
							//Upload the file. This is way harder to try and keep than the hastebin links
							await Actions.uploadTextFile(guild, logChannel, deletedMessagesContent, "Deleted_Messages_", "Deleted Messages");
						}
					}
				});
			}
		}

		//Get all images uploaded
		public static async Task OnMessageReceived(SocketMessage message)
		{
			if (message.Author.IsBot)
				return;

			++Variables.LoggedMessages;

			ITextChannel logChannel = await Actions.logChannelCheck((message.Channel as IGuildChannel).Guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel != null)
			{
				if (message.Attachments.Count > 0)
				{
					ImageLog(logChannel, message, false);
				}
				else if (message.Embeds.Count > 0)
				{
					ImageLog(logChannel, message, true);
				}
			}
		}

		//Logging images
		public static void ImageLog(IMessageChannel channel, SocketMessage message, bool embeds)
		{
			if (message.Author.Id == CommandHandler.client.CurrentUser.Id)
				return;

			//Get the links
			var t = Task.Run(async delegate
			{
				List<String> attachmentURLs = new List<String>();
				List<String> embedURLs = new List<String>();
				List<Embed> videoEmbeds = new List<Embed>();
				if (!embeds && message.Attachments.Count > 0)
				{
					//If attachment, the file is hosted on discord which has a concrete URL name for files (cdn.discordapp.com/attachments/.../x.png)
					message.Attachments.ToList().ForEach(x => attachmentURLs.Add(x.Url));
				}
				if (embeds && message.Embeds.Count > 0)
				{
					//If embed this is slightly trickier, but only images/videos can embed (AFAIK)
					message.Embeds.ToList().ForEach(x =>
					{
						if (x.Video == null)
						{
							//If no video then it has to be just an image
							if (!String.IsNullOrEmpty(x.Thumbnail.ToString()))
							{
								embedURLs.Add(x.Thumbnail.ToString());
							}
							if (!String.IsNullOrEmpty(x.Image.ToString()))
							{
								embedURLs.Add(x.Image.ToString());
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
				foreach (String URL in attachmentURLs.Distinct())
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
				foreach (String URL in embedURLs.Distinct())
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
					String title = Constants.VALIDGIFEXTENTIONS.Contains(Path.GetExtension(embedObject.Thumbnail.Value.Url).ToLower()) ? "Gif" : "Video";

					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.ATTACH, title, embedObject.Url, embedObject.Thumbnail.Value.Url), "Embedded " + title);
					Actions.addAuthor(embed, String.Format("{0}#{1} in #{2}", user.Username, user.Discriminator, message.Channel), user.AvatarUrl);
					await Actions.sendEmbedMessage(channel, embed);
				}
			});
		}
	}

	public class ModLogs : ModuleBase
	{
		//Log each command
		public static async Task LogCommand(ICommandContext context)
		{
			++Variables.LoggedCommands;
			Actions.writeLine(context.User.Id + " used command \'" + context.Message + "\' and succeeded.");

			ITextChannel logChannel = await Actions.logChannelCheck(context.Guild, Constants.MOD_LOG_CHECK_STRING);
			if (logChannel != null)
			{
				EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(description: context.Message.Content), "Mod Log");
				Actions.addAuthor(embed, context.User.Username + "#" + context.User.Discriminator + " in #" + context.Channel.Name, context.User.AvatarUrl);
				await Actions.sendEmbedMessage(logChannel, embed);
			}
		}
	}
}