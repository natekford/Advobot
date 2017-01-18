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
			Actions.writeLine(String.Format("{0}: {1}#{2} is online now.", MethodBase.GetCurrentMethod().Name, guild.Name, guild.Id));
			Actions.loadPreferences(guild);
			Actions.loadBannedPhrasesAndPunishments(guild);

			Variables.TotalUsers += guild.MemberCount;
			Variables.TotalGuilds++;
			Variables.Guilds.Add(guild);

			return Task.CompletedTask;
		}

		//When the bot joins a server
		public static Task OnJoinedGuild(SocketGuild guild)
		{
			Actions.writeLine(String.Format("{0}: Bot joined {1}#{2}.", MethodBase.GetCurrentMethod().Name, guild.Name, guild.Id));

			return Task.CompletedTask;
		}

		//When the bot leaves a server
		public static Task OnLeftGuild(SocketGuild guild)
		{
			Actions.writeLine(String.Format("{0}: Bot has left {1}#{2}.", MethodBase.GetCurrentMethod().Name, guild.Name, guild.Id));

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
			//Check if should add them to a slowmode for channel/guild
			if (Variables.SlowmodeGuilds.ContainsKey(user.Guild.Id) || (await user.Guild.GetTextChannelsAsync()).Intersect(Variables.SlowmodeChannels.Keys).Any())
			{
				await Actions.slowmodeAddUser(user);
			}

			ITextChannel logChannel = await Actions.logChannelCheck(user.Guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel == null)
				return;
			if (!await Actions.permissionCheck(logChannel))
				return;
			++Variables.LoggedJoins;

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

		//Tell when a user leaves the server
		public static async Task OnUserLeft(SocketGuildUser user)
		{
			//Check if the bot was the one that left
			if (user == user.Guild.GetUser(Variables.Bot_ID))
			{
				Variables.Guilds.Remove(user.Guild);
				return;
			}

			ITextChannel logChannel = await Actions.logChannelCheck(user.Guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel == null)
				return;
			if (!await Actions.permissionCheck(logChannel))
				return;
			++Variables.LoggedLeaves;

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

		//Tell when a user is unbanned
		public static async Task OnUserUnbanned(SocketUser user, SocketGuild guild)
		{
			ITextChannel logChannel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel == null)
				return;
			if (!await Actions.permissionCheck(logChannel))
				return;
			++Variables.LoggedUnbans;

			//Get the username/discriminator via this dictionary since they don't exist otherwise
			string username = Variables.UnbannedUsers.ContainsKey(user.Id) ? Variables.UnbannedUsers[user.Id].Username : "null";
			string discriminator = Variables.UnbannedUsers.ContainsKey(user.Id) ? Variables.UnbannedUsers[user.Id].Discriminator : "0000";

			EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.UNBAN, description: "**ID:** " + user.Id.ToString()), "Unban");
			await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", username, discriminator), user.AvatarUrl));
		}

		//Tell when a user is banned
		public static async Task OnUserBanned(SocketUser user, SocketGuild guild)
		{
			//Check if the bot was the one banned
			if (user == guild.GetUser(Variables.Bot_ID))
			{
				Variables.Guilds.Remove(guild);
				return;
			}

			ITextChannel logChannel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel == null)
				return;
			if (!await Actions.permissionCheck(logChannel))
				return;
			++Variables.LoggedBans;

			EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.BAN, description: "**ID:** " + user.Id.ToString()), "Ban");
			await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl));
		}

		//Tell when a user has their name, nickname, or roles changed
		public static async Task OnGuildMemberUpdated(SocketGuildUser beforeUser, SocketGuildUser afterUser)
		{
			ITextChannel logChannel = await Actions.logChannelCheck(beforeUser.Guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel == null)
				return;
			if (!await Actions.permissionCheck(logChannel))
				return;
			++Variables.LoggedUserChanges;

			//Nickname change
			if ((String.IsNullOrWhiteSpace(beforeUser.Nickname) && !String.IsNullOrWhiteSpace(afterUser.Nickname)) || (!String.IsNullOrWhiteSpace(beforeUser.Nickname) && String.IsNullOrWhiteSpace(afterUser.Nickname)))
			{
				string originalNickname = beforeUser.Nickname;
				if (String.IsNullOrWhiteSpace(beforeUser.Nickname))
				{
					originalNickname = "NO NICKNAME";
				}
				string nicknameChange = afterUser.Nickname;
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
			List<string> rolesChange = new List<string>();
			if (firstNotSecond.Any())
			{
				firstNotSecond.ForEach(x => rolesChange.Add(afterUser.Guild.GetRole(x).Name));

				EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.UEDIT, description: "**Lost:** " + String.Join(", ", rolesChange)), "Role Loss");
				await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl));
			}
			else if (secondNotFirst.Any())
			{
				secondNotFirst.ForEach(x => rolesChange.Add(afterUser.Guild.GetRole(x).Name));

				EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.UEDIT, description: "**Gained:** " + String.Join(", ", rolesChange)), "Role Gain");
				await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl));
			}
		}

		//Tell when a user updates their name/game/status
		public static async Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser)
		{
			//Name change
			//TODO: Make this work somehow
			if (!beforeUser.Username.Equals(afterUser.Username))
			{
				foreach (var guild in CommandHandler.Client.Guilds.ToList().Where(x => x.Users.Contains(afterUser)).ToList())
				{
					ITextChannel logChannel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
					if (logChannel == null)
						return;
					if (!await Actions.permissionCheck(logChannel))
						return;
					++Variables.LoggedUserChanges;

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
			//If bot then ignore
			if (afterMessage.Author.IsBot)
				return;
			//If DM then ignore
			if (afterMessage.Channel as IGuildChannel == null)
				return;
			//Check if the guild exists
			IGuild guild = (afterMessage.Channel as IGuildChannel).Guild;
			if (guild == null)
				return;
			//Check if any banned phrases
			await Actions.bannedPhrases(afterMessage);
			//Check if logchannel exists
			ITextChannel logChannel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel == null || !beforeMessage.IsSpecified)
				return;
			++Variables.LoggedEdits;

			//Check if regular messages are equal
			if (beforeMessage.Value.Embeds.Count != afterMessage.Embeds.Count)
			{
				await ImageLog(logChannel, afterMessage, true);
				return;
			}

			//Set the content as strings
			string beforeMsg = Actions.replaceMessageCharacters(beforeMessage.Value.Content ?? "NOTHING");
			string afterMsg = Actions.replaceMessageCharacters(afterMessage.Content ?? "NOTHING");

			//Double checking because sending a field with null is not good
			beforeMsg = String.IsNullOrWhiteSpace(beforeMsg) ? "NOTHING" : beforeMsg;
			afterMsg = String.IsNullOrWhiteSpace(afterMsg) ? "NOTHING" : afterMsg;

			//Set the user as a variable
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
			if (String.IsNullOrWhiteSpace(afterMsg))
			{
				afterMsg = "UNABLE TO BE GATHERED";
			}

			//Check lengths
			if (!(beforeMsg.Length + afterMsg.Length < 1800))
			{
				beforeMsg = beforeMsg.Length > 667 ? "LONG MESSAGE" : beforeMsg;
				afterMsg = afterMsg.Length > 667 ? "LONG MESSAGE" : afterMsg;
			}

			EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.MEDIT), "Edit");
			Actions.addField(embed, "Before:", beforeMsg);
			Actions.addField(embed, "After:", afterMsg, false);
			Actions.addAuthor(embed, String.Format("{0}#{1} in #{2}", user.Username, user.Discriminator, afterMessage.Channel), user.AvatarUrl);
			await Actions.sendEmbedMessage(logChannel, embed);
		}

		//Tell when a message is deleted
		public static async Task OnMessageDeleted(ulong messageID, Optional<SocketMessage> message)
		{
			//If DM ignore
			if (message.Value.Channel as IGuildChannel == null)
				return;
			//Check if guild exists
			IGuild guild = (message.Value.Channel as IGuildChannel).Guild;
			if (guild == null)
				return;
			//Check if valid log channel
			ITextChannel logChannel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel == null)
				return;
			//If null message ignore
			if (!message.IsSpecified)
				return;
			//Got an error once time due to a null user when spam testing, so this check is here
			if (message.Value.Author == null)
				return;
			++Variables.LoggedDeletes;

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
				List<string> deletedMessagesContent = new List<string>();
				deletedMessagesSorted.ForEach(x =>
				{
					//See if any embeds deleted
					if (x.Embeds.Any())
					{
						//Get the embed
						Embed embed = x.Embeds.ToList().FirstOrDefault(y => y.Description != null);

						if (embed != null)
						{
							if (embed.Author != null)
							{
								string author = embed.Author.ToString();

								//I don't know how to regex well
								Regex regex = new Regex("#([0-9]*) in #");
								string[] authorAndChannel = regex.Split(author);

								deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
									authorAndChannel.Length > 0 ? authorAndChannel[0] : "null",
									authorAndChannel.Length > 1 ? authorAndChannel[1] : "0000",
									authorAndChannel.Length > 2 ? authorAndChannel[2] : "null",
									x.CreatedAt.ToString("HH:mm:ss"),
									Actions.replaceMessageCharacters(embed.Description)));
							}
							else
							{
								deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
									x.Author.Username, x.Author.Discriminator, x.Channel, x.CreatedAt.ToString("HH:mm:ss"), Actions.replaceMessageCharacters(embed.Description)));
							}
						}
						else
						{
							deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
								x.Author.Username, x.Author.Discriminator, x.Channel, x.CreatedAt.ToString("HH:mm:ss"), "An embed which was unable to be gotten."));
						}
					}
					//See if any attachments were put in
					else if (x.Attachments.Any())
					{
						string content = String.IsNullOrEmpty(x.Content) ? "EMPTY MESSAGE" : x.Content;
						deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
							x.Author.Username, x.Author.Discriminator, x.Channel, x.CreatedAt.ToString("HH:mm:ss"),
							Actions.replaceMessageCharacters(content + " + " + x.Attachments.ToList().First().Filename)));
					}
					//Else add the message in normally
					else
					{
						string content = String.IsNullOrEmpty(x.Content) ? "EMPTY MESSAGE" : x.Content;
						deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
							x.Author.Username, x.Author.Discriminator, x.Channel, x.CreatedAt.ToString("HH:mm:ss"), Actions.replaceMessageCharacters(content)));
					}
				});

				if (deletedMessages.Count == 0)
					return;
				else if ((deletedMessages.Count <= 5) && (characterCount < Constants.LENGTH_CHECK))
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

		//Get all images uploaded and other things on messages received
		public static async Task OnMessageReceived(SocketMessage message)
		{
			//If bot then ignore
			if (message.Author.IsBot)
				return;
			//If DM then ignore
			if (message.Channel as IGuildChannel == null)
				return;
			//Check if valid guild
			IGuild guild = (message.Channel as IGuildChannel).Guild;
			if (guild == null)
				return;
			++Variables.LoggedMessages;

			//Check if the guild has slowmode enabled currently
			if (Variables.SlowmodeGuilds.ContainsKey(guild.Id) || Variables.SlowmodeChannels.ContainsKey(message.Channel as IGuildChannel))
			{
				await Actions.slowmode(message);
				return;
			}
			//Check if any banned phrases
			else if (Variables.BannedPhrases.ContainsKey(guild.Id) || Variables.BannedRegex.ContainsKey(guild.Id))
			{
				await Actions.bannedPhrases(message);
				return;
			}

			//Check if it's the owner of the guild saying something
			if (message.Author.Id == guild.OwnerId)
			{
				//If the message is only 'yes' then check if they're enabling or deleting preferences
				if (message.Content.ToLower().Equals("yes"))
				{
					if (Variables.GuildsEnablingPreferences.Contains(guild))
					{
						//Enable preferences
						await Actions.enablePreferences(guild, message as IUserMessage);
					}
					else if (Variables.GuildsDeletingPreferences.Contains(guild))
					{
						//Delete preferences
						await Actions.deletePreferences(guild, message as IUserMessage);
					}
				}
			}

			//Check if it is going to be image logged
			ITextChannel logChannel = await Actions.logChannelCheck(guild, Constants.SERVER_LOG_CHECK_STRING);
			if (logChannel == null)
				return;

			if (message.Attachments.Any())
			{
				await ImageLog(logChannel, message, false);
			}
			else if (message.Embeds.Any())
			{
				await ImageLog(logChannel, message, true);
			}
		}

		//Logging images
		public static async Task ImageLog(IMessageChannel channel, SocketMessage message, bool embeds)
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
						if (!String.IsNullOrEmpty(x.Thumbnail.Value.Url))
						{
							embedURLs.Add(x.Thumbnail.Value.Url);
						}
						else if (!String.IsNullOrEmpty(x.Image.Value.Url))
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

		//Make sure no duplicate bot channels are made
		public static async Task OnChannelCreated(SocketChannel channel)
		{
			ITextChannel tChan = channel as ITextChannel;
			if (tChan != null && tChan.Name == Variables.Bot_Channel && await Actions.getDuplicateBotChan(tChan.Guild))
			{
				await tChan.DeleteAsync();
			}
		}

		//See if the channel had its name changed to the bot channel name
		public static async Task OnChannelUpdated(SocketChannel beforeChannel, SocketChannel afterChannel)
		{
			//Check if the name is the bot channel name
			if ((afterChannel as IGuildChannel).Name.Equals(Variables.Bot_Channel, StringComparison.OrdinalIgnoreCase))
			{
				//Create a variable of beforechannel as an IGuildChannel for later use
				var bChan = beforeChannel as IGuildChannel;

				//If the name wasn't the bot channel name to start with then set it back to its start name
				if (!bChan.Name.Equals(Variables.Bot_Channel, StringComparison.OrdinalIgnoreCase) && await Actions.getDuplicateBotChan(bChan.Guild))
				{
					await (await bChan.Guild.GetChannelAsync(bChan.Id)).ModifyAsync(x => x.Name = bChan.Name);
				}
			}
		}
	}

	public class ModLogs : ModuleBase
	{
		//Log each command
		public static async Task LogCommand(ICommandContext context)
		{
			Actions.writeLine(context.User.Id + " used command \'" + context.Message + "\' and succeeded.");

			ITextChannel logChannel = await Actions.logChannelCheck(context.Guild, Constants.MOD_LOG_CHECK_STRING);
			if (logChannel == null)
				return;
			if (!await Actions.permissionCheck(logChannel))
				return;
			++Variables.LoggedCommands;

			EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(description: context.Message.Content), "Mod Log");
			Actions.addAuthor(embed, context.User.Username + "#" + context.User.Discriminator + " in #" + context.Channel.Name, context.User.AvatarUrl);
			await Actions.sendEmbedMessage(logChannel, embed);
		}
	}
}