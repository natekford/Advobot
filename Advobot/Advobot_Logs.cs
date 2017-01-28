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
	//Logs are commands which fire on actions. Most of these are solely for logging, but a few are for deleting certain messages.
	public class BotLogs : ModuleBase
	{
		public static Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		public static Task OnGuildAvailable(SocketGuild guild)
		{
			Actions.writeLine(String.Format("{0}: {1}#{2} is online now.", MethodBase.GetCurrentMethod().Name, guild.Name, guild.Id));

			if (!Variables.Guilds.ContainsKey(guild.Id))
			{
				//Put the guild into a list
				Variables.Guilds.Add(guild.Id, new MyGuildInfo(guild));
				//Put the invites into a list holding mainly for usage checking
				var t = Task.Run(async () =>
				{
					//Get all of the invites and add their guildID, code, and current uses to the usage check list
					Variables.Guilds[guild.Id].Invites = (await guild.GetInvitesAsync()).ToList().Select(x => new MyInvite(x.GuildId, x.Code, x.Uses)).ToList();
				});

				//Incrementing
				Variables.TotalUsers += guild.MemberCount;
				Variables.TotalGuilds++;

				//Loading everything
				if (Variables.Bot_ID != 0)
				{
					Actions.loadPreferences(guild);
					Actions.loadBannedPhrasesAndPunishments(guild);
					Actions.loadSelfAssignableRoles(guild);
				}
				else
				{
					Variables.GuildsToBeLoaded.Add(guild);
				}
			}

			return Task.CompletedTask;
		}

		public static Task OnGuildUnavailable(SocketGuild guild)
		{
			Actions.writeLine(String.Format("{0}: Guild is down: {1}#{2}.", MethodBase.GetCurrentMethod().Name, guild.Name, guild.Id));

			Variables.TotalUsers -= (guild.MemberCount + 1);
			if (Variables.TotalUsers < 0)
				Variables.TotalUsers = 0;
			Variables.TotalGuilds--;
			if (Variables.TotalGuilds < 0)
				Variables.TotalGuilds = 0;

			return Task.CompletedTask;
		}

		public static Task OnJoinedGuild(SocketGuild guild)
		{
			Actions.writeLine(String.Format("{0}: Bot joined {1}#{2}.", MethodBase.GetCurrentMethod().Name, guild.Name, guild.Id));

			//Check how many bots are in the guild
			int botCount = 0;
			guild.Users.ToList().ForEach(x =>
			{
				if (x.IsBot)
				{
					++botCount;
				}
			});

			//Get the number of users in the guild
			var users = guild.MemberCount;
			//Determine what percentage of bot users to leave at
			double percentage;
			if (users <= 10)
			{
				//Allows up to 7 bots
				percentage = .7;
			}
			else if (users <= 25)
			{
				//Allows up to 12 bots
				percentage = .5;
			}
			else if (users <= 50)
			{
				//Allows up to 17 bots
				percentage = .3;
			}
			else if (users <= 100)
			{
				//Allows up to 20 bots
				percentage = .2;
			}
			else
			{
				percentage = .1;
			}

			//Leave the guild
			var t = Task.Run(async () =>
			{
				if (botCount / (users * 1.0) > percentage)
				{
					await guild.LeaveAsync();
				}
			});

			return Task.CompletedTask;
		}

		public static Task OnLeftGuild(SocketGuild guild)
		{
			Actions.writeLine(String.Format("{0}: Bot has left {1}#{2}.", MethodBase.GetCurrentMethod().Name, guild.Name, guild.Id));

			Variables.TotalUsers -= (guild.MemberCount + 1);
			Variables.TotalGuilds--;

			return Task.CompletedTask;
		}

		public static Task OnDisconnected(Exception exception)
		{
			Actions.writeLine(String.Format("{0}: Bot has been disconnected.", MethodBase.GetCurrentMethod().Name));

			Variables.TotalUsers = 0;
			Variables.TotalGuilds = 0;

			return Task.CompletedTask;
		}

		public static Task OnConnected()
		{
			Actions.writeLine(String.Format("{0}: Bot has connected.", MethodBase.GetCurrentMethod().Name));

			return Task.CompletedTask;
		}
	}

	public class ServerLogs : ModuleBase
	{
		public static async Task OnUserJoined(SocketGuildUser user)
		{
			++Variables.LoggedJoins;
			++Variables.TotalUsers;

			//Get the current invites
			var curInvs = await user.Guild.GetInvitesAsync();
			//Get the invites that have already been put on the bot
			var botInvs = Variables.Guilds[user.Guild.Id].Invites;
			//Set an invite to hold the current invite the user joined on
			MyInvite curInv = null;
			if (botInvs != null)
			{
				//Find the first invite where the bot invite has the same code as the current invite but different use counts
				curInv = botInvs.FirstOrDefault(bI => curInvs.Any(cI => cI.Code == bI.Code && cI.Uses != bI.Uses));

				//If the invite is null, take that as meaning there are new invites on the server
				if (curInv == null)
				{
					//Get the new invites on the server by finding which guild invites aren't on the bot invites list
					var newInvs = curInvs.Where(x => !botInvs.Select(y => y.Code).Contains(x.Code));
					//If there's only one, then use that as the current inv. If there's more than one then there's no way to know what invite it was on
					if (newInvs.Count() == 1)
					{
						curInv = new MyInvite(newInvs.First().GuildId, newInvs.First().Code, newInvs.First().Uses);
					}
					//Add all of the invites to the bot invites list
					botInvs.AddRange(newInvs.Select(x => new MyInvite(x.GuildId, x.Code, x.Uses)));
				}
				else
				{
					//Increment the invite the bot is holding if a curInv was found so as to match with the current invite uses count
					++curInv.Uses;
				}
			}

			//Check if should add them to a slowmode for channel/guild
			if (Variables.SlowmodeGuilds.ContainsKey(user.Guild.Id) || (await user.Guild.GetTextChannelsAsync()).Intersect(Variables.SlowmodeChannels.Keys).Any())
			{
				await Actions.slowmodeAddUser(user);
			}

			var logChannel = await Actions.verifyLogChannel(user.Guild);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			//Invite string
			string inviteString = "";
			if (curInv != null)
			{
				inviteString = String.Format("**Invite:** {0}", curInv.Code);
			}

			if (user.IsBot)
			{
				EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.JOIN, null, String.Format("**ID:** {0}\n{1}", user.Id, inviteString)), "Bot Joined");
				await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl));
			}
			else
			{
				EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.JOIN, null, String.Format("**ID:** {0}\n{1}", user.Id, inviteString)), "User Joined");
				await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl));
			}
		}

		public static async Task OnUserLeft(SocketGuildUser user)
		{
			++Variables.LoggedLeaves;
			--Variables.TotalUsers;

			//Check if the bot was the one that left
			if (user == user.Guild.GetUser(Variables.Bot_ID))
			{
				Variables.Guilds.Remove(user.Guild.Id);
				return;
			}

			var logChannel = await Actions.verifyLogChannel(user.Guild);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			if (user.IsBot)
			{
				EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.LEAV, null, "**ID:** " + user.Id.ToString()), "Bot Left");
				await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl));
			}
			else
			{
				EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.LEAV, null, "**ID:** " + user.Id.ToString()), "User Left");
				await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl));
			}
		}

		public static async Task OnUserUnbanned(SocketUser user, SocketGuild guild)
		{
			++Variables.LoggedUnbans;

			var logChannel = await Actions.verifyLogChannel(guild);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			//Get the username/discriminator via this dictionary since they don't exist otherwise
			string username = Variables.UnbannedUsers.ContainsKey(user.Id) ? Variables.UnbannedUsers[user.Id].Username : "null";
			string discriminator = Variables.UnbannedUsers.ContainsKey(user.Id) ? Variables.UnbannedUsers[user.Id].Discriminator : "0000";

			EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.UNBN, null, "**ID:** " + user.Id.ToString()), "User Unbanned");
			await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", username, discriminator), user.AvatarUrl));
		}

		public static async Task OnUserBanned(SocketUser user, SocketGuild guild)
		{
			++Variables.LoggedBans;

			//Check if the bot was the one banned
			if (user == guild.GetUser(Variables.Bot_ID))
			{
				Variables.Guilds.Remove(guild.Id);
				return;
			}

			var logChannel = await Actions.verifyLogChannel(guild);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.BANN, null, "**ID:** " + user.Id.ToString()), "User Banned");
			await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl));
		}

		public static async Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser)
		{
			//Name change
			//TODO: Make this work somehow
			if (!beforeUser.Username.Equals(afterUser.Username, StringComparison.OrdinalIgnoreCase))
			{
				foreach (var guild in CommandHandler.Client.Guilds.Where(x => x.Users.Contains(afterUser)))
				{
					var logChannel = await Actions.verifyLogChannel(guild);
					if (logChannel == null)
						return;
					if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
						return;
					++Variables.LoggedUserChanges;

					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.UEDT), "Name Changed");
					Actions.addField(embed, "Before:", "`" + beforeUser.Username + "`");
					Actions.addField(embed, "After:", "`" + afterUser.Username + "`", false);
					await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl));
				}
			}
		}

		public static async Task OnGuildMemberUpdated(SocketGuildUser beforeUser, SocketGuildUser afterUser)
		{
			++Variables.LoggedUserChanges;

			var logChannel = await Actions.verifyLogChannel(afterUser);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;
			IGuild guild = afterUser.Guild;

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
				EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.UEDT), "Nickname Changed");
				Actions.addField(embed, "Before:", "`" + originalNickname + "`");
				Actions.addField(embed, "After:", "`" + nicknameChange + "`", false);
				await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl));
			}
			else if (!(String.IsNullOrWhiteSpace(beforeUser.Nickname) && String.IsNullOrWhiteSpace(afterUser.Nickname)))
			{
				if (!beforeUser.Nickname.Equals(afterUser.Nickname))
				{
					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.UEDT), "Nickname Changed");
					Actions.addField(embed, "Before:", "`" + beforeUser.Nickname + "`");
					Actions.addField(embed, "After:", "`" + afterUser.Nickname + "`", false);
					await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl));
				}
			}

			//Role change
			var firstNotSecond = beforeUser.RoleIds.Except(afterUser.RoleIds).Select(x => guild.GetRole(x)).ToList();
			var secondNotFirst = afterUser.RoleIds.Except(beforeUser.RoleIds).Select(x => guild.GetRole(x)).ToList();
			List<string> rolesChange = new List<string>();
			if (firstNotSecond.Any())
			{
				//In separate task in case of a deleted role
				var t = Task.Run(async () =>
				{
					var users = await guild.GetUsersAsync();
					int maxUsers = 0;
					firstNotSecond.ForEach(x => maxUsers = Math.Max(maxUsers, users.Where(y => y.RoleIds.Contains(x.Id)).Count()));

					await Task.Delay(maxUsers * 2);

					firstNotSecond.ForEach(x =>
					{
						//Return to ignore deleted roles
						if (Variables.DeletedRoles.Contains(x.Id))
							return;
						rolesChange.Add(x.Name);
					});

					//If no roles the return so as to not send a blank message
					if (!rolesChange.Any())
						return;

					EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.UEDT, null, "**Role(s) Lost:** " + String.Join(", ", rolesChange)), "Role Lost");
					await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl));
				});
			}
			else if (secondNotFirst.Any())
			{
				//Not necessary to have in a separate task like the method above
				secondNotFirst.ForEach(x =>
				{
					rolesChange.Add(x.Name);
				});

				if (!rolesChange.Any())
					return;

				EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.UEDT, null, "**Role(s) Gained:** " + String.Join(", ", rolesChange)), "Role Gained");
				await Actions.sendEmbedMessage(logChannel, Actions.addAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl));
			}
		}

		public static async Task OnMessageReceived(SocketMessage message)
		{
			++Variables.LoggedMessages;

			if (message.Author.IsBot)
				return;

			//If DM then ignore for the most part
			var channel = message.Channel as IGuildChannel;
			if (channel == null)
			{
				//See if they're on the list to be a potential bot owner
				if (Variables.PotentialBotOwners.Contains(message.Author.Id))
				{
					//If the key they input is the same as the bots key then they become owner
					if (message.Content.Trim().Equals(Properties.Settings.Default.BotKey))
					{
						Properties.Settings.Default.BotOwner = message.Author.Id;
						Properties.Settings.Default.Save();
						Variables.PotentialBotOwners.Clear();
						await Actions.sendDMMessage(message.Channel as IDMChannel, "Congratulations, you are now the owner of the bot.");
					}
					else
					{
						Variables.PotentialBotOwners.Remove(message.Author.Id);
						await Actions.sendDMMessage(message.Channel as IDMChannel, "That is the incorrect key.");
					}
				}
				return;
			}

			var guild = channel.Guild;
			if (guild == null)
				return;

			//Check if it's the owner of the guild saying something
			if (message.Author.Id == guild.OwnerId)
			{
				//If the message is only 'yes' then check if they're enabling or deleting preferences
				if (message.Content.Equals("yes", StringComparison.OrdinalIgnoreCase))
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

			//Check if the guild has slowmode enabled currently
			if (Variables.SlowmodeGuilds.ContainsKey(guild.Id) || Variables.SlowmodeChannels.ContainsKey(channel))
			{
				await Actions.slowmode(message);
			}
			//Check if any banned phrases
			else if (Variables.Guilds[guild.Id].BannedPhrases.Any() || Variables.Guilds[guild.Id].BannedRegex.Any())
			{
				await Actions.bannedPhrases(message);
			}
			//Check if it is going to be image logged
			else
			{
				var logChannel = await Actions.verifyLogChannel(guild);
				if (logChannel == null)
					return;
				if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
					return;
				if (Variables.Guilds[guild.Id].IgnoredChannels.Contains(channel.Id))
					return;

				if (message.Attachments.Any())
				{
					await Actions.imageLog(logChannel, message, false);
				}
				else if (message.Embeds.Any())
				{
					await Actions.imageLog(logChannel, message, true);
				}
			}
		}

		public static async Task OnMessageUpdated(Optional<SocketMessage> beforeMessage, SocketMessage afterMessage)
		{
			++Variables.LoggedEdits;

			if (afterMessage.Author.IsBot)
				return;
			await Actions.bannedPhrases(afterMessage);
			var logChannel = await Actions.verifyLogChannel(afterMessage);
			if (logChannel == null || afterMessage == null || afterMessage.Author == null)
				return;
			var guild = (afterMessage.Channel as IGuildChannel).Guild;
			if (!Variables.Guilds[guild.Id].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;
			if (Variables.Guilds[guild.Id].IgnoredChannels.Contains(afterMessage.Channel.Id))
				return;

			//Check if regular messages are equal
			if (beforeMessage.Value.Embeds.Count != afterMessage.Embeds.Count)
			{
				await Actions.imageLog(logChannel, afterMessage, true);
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

			//Make the embed
			EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(Constants.MEDT), "Message Updated");
			Actions.addField(embed, "Before:", "`" + beforeMsg + "`");
			Actions.addField(embed, "After:", "`" + afterMsg + "`", false);
			Actions.addAuthor(embed, String.Format("{0}#{1} in #{2}", user.Username, user.Discriminator, afterMessage.Channel), user.AvatarUrl);
			await Actions.sendEmbedMessage(logChannel, embed);
		}

		public static async Task OnMessageDeleted(ulong messageID, Optional<SocketMessage> message)
		{
			++Variables.LoggedDeletes;

			if (!message.IsSpecified)
				return;
			var logChannel = await Actions.verifyLogChannel(message.Value);
			if (logChannel == null)
				return;
			var guild = (message.Value.Channel as IGuildChannel).Guild;
			if (!Variables.Guilds[guild.Id].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;
			if (Variables.Guilds[guild.Id].IgnoredChannels.Contains(message.Value.Channel.Id))
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
			var t = Task.Run(async () =>
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
						//Get the first embed with a valid description
						Embed embed = x.Embeds.FirstOrDefault(desc => desc.Description != null);
						//If no embed with valid description, try for valid URL
						embed = embed ?? x.Embeds.FirstOrDefault(url => url.Url != null);
						//If no valid URL, try for valid image
						embed = embed ?? x.Embeds.FirstOrDefault(img => img.Image != null);

						if (embed != null)
						{
							deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
								x.Author.Username,
								x.Author.Discriminator,
								x.Channel,
								x.CreatedAt.ToString("HH:mm:ss"),
								Actions.replaceMessageCharacters(embed.Description)));
						}
						else
						{
							deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
								x.Author.Username,
								x.Author.Discriminator,
								x.Channel,
								x.CreatedAt.ToString("HH:mm:ss"),
								"An embed which was unable to be gotten."));
						}
					}
					//See if any attachments were put in
					else if (x.Attachments.Any())
					{
						string content = String.IsNullOrEmpty(x.Content) ? "EMPTY MESSAGE" : x.Content;
						deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
							x.Author.Username,
							x.Author.Discriminator,
							x.Channel,
							x.CreatedAt.ToString("HH:mm:ss"),
							Actions.replaceMessageCharacters(content + " + " + x.Attachments.ToList().First().Filename)));
					}
					//Else add the message in normally
					else
					{
						string content = String.IsNullOrEmpty(x.Content) ? "EMPTY MESSAGE" : x.Content;
						deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
							x.Author.Username,
							x.Author.Discriminator,
							x.Channel,
							x.CreatedAt.ToString("HH:mm:ss"),
							Actions.replaceMessageCharacters(content)));
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

		public static async Task OnRoleCreated(SocketRole role)
		{
			var logChannel = await Actions.verifyLogChannel(role.Guild);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			EmbedBuilder embed = Actions.makeNewEmbed(Constants.CCRE, "Role Created", String.Format("Name: `{0}`\nID: `{1}`", role.Name, role.Id));
			await Actions.sendEmbedMessage(logChannel, Actions.addFooter(embed, "Role Created"));
		}

		public static async Task OnRoleUpdated(SocketRole beforeRole, SocketRole afterRole)
		{
			var logChannel = await Actions.verifyLogChannel(afterRole.Guild);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			if (!beforeRole.Name.Equals(afterRole.Name, StringComparison.OrdinalIgnoreCase))
			{
				EmbedBuilder embed = Actions.makeNewEmbed(Constants.REDT, "Role Name Changed");
				Actions.addField(embed, "Before:", "`" + beforeRole.Name + "`");
				Actions.addField(embed, "After:", "`" + afterRole.Name + "`", false);
				await Actions.sendEmbedMessage(logChannel, Actions.addFooter(embed, "Role Name Changed"));
			}
		}

		public static async Task OnRoleDeleted(SocketRole role)
		{
			//Add this to prevent massive spam fests when a role is deleted
			Variables.DeletedRoles.Add(role.Id);

			var logChannel = await Actions.verifyLogChannel(role.Guild);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			EmbedBuilder embed = Actions.makeNewEmbed(Constants.CCRE, "Role Deleted", String.Format("Name: `{0}`\nID: `{1}`", role.Name, role.Id));
			await Actions.sendEmbedMessage(logChannel, Actions.addFooter(embed, "Role Deleted"));
		}

		public static async Task OnChannelCreated(SocketChannel channel)
		{
			var logChannel = await Actions.verifyLogChannel(channel);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			var chan = channel as IGuildChannel;

			//Check if the channel trying to be made is a bot channel
			if ((chan as ITextChannel) != null && chan.Name == Variables.Bot_Channel && await Actions.getDuplicateBotChan(chan.Guild))
			{
				await chan.DeleteAsync();
				return;
			}

			EmbedBuilder embed = Actions.makeNewEmbed(Constants.CCRE, "Channel Created", String.Format("Name: `{0}`\nID: `{1}`", chan.Name, chan.Id));
			await Actions.sendEmbedMessage(logChannel, Actions.addFooter(embed, "Channel Created"));
		}

		public static async Task OnChannelUpdated(SocketChannel beforeChannel, SocketChannel afterChannel)
		{
			var logChannel = await Actions.verifyLogChannel(afterChannel);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			//Create a variable of beforechannel and afterchannel as an IGuildChannel for later use
			var bChan = beforeChannel as IGuildChannel;
			var aChan = afterChannel as IGuildChannel;

			//Check if the name is the bot channel name
			if ((aChan as ITextChannel) != null && aChan.Name.Equals(Variables.Bot_Channel, StringComparison.OrdinalIgnoreCase))
			{
				//If the name wasn't the bot channel name to start with then set it back to its start name
				if (!bChan.Name.Equals(Variables.Bot_Channel, StringComparison.OrdinalIgnoreCase) && await Actions.getDuplicateBotChan(bChan.Guild))
				{
					await (await bChan.Guild.GetChannelAsync(bChan.Id)).ModifyAsync(x => x.Name = bChan.Name);
					return;
				}
			}

			if (!aChan.Name.Equals(bChan.Name, StringComparison.OrdinalIgnoreCase))
			{
				EmbedBuilder embed = Actions.makeNewEmbed(Constants.CEDT, "Channel Name Changed");
				Actions.addField(embed, "Before:", "`" + bChan.Name + "`");
				Actions.addField(embed, "After:", "`" + aChan.Name + "`", false);
				await Actions.sendEmbedMessage(logChannel, Actions.addFooter(embed, "Channel Name Changed"));
			}
		}

		public static async Task OnChannelDeleted(SocketChannel channel)
		{
			var logChannel = await Actions.verifyLogChannel(channel);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			var chan = channel as IGuildChannel;

			EmbedBuilder embed = Actions.makeNewEmbed(Constants.CDEL, "Channel Deleted", String.Format("Name: `{0}`\nID: `{1}`", chan.Name, chan.Id));
			await Actions.sendEmbedMessage(logChannel, Actions.addFooter(embed, "Channel Deleted"));
		}
	}

	public class ModLogs : ModuleBase
	{
		public static async Task LogCommand(CommandContext context)
		{
			string userString = String.Format("{0}#{1} ({2})", context.User.Username, context.User.Discriminator, context.User.Id);
			string guildString = String.Format("{0} ({1})", context.Guild.Name, context.Guild.Id);
			Actions.writeLine(String.Format("{0} on {1}: \'{2}\'", userString, guildString, context.Message.Content));

			var logChannel = await Actions.verifyLogChannel(context.Guild, Constants.MOD_LOG_CHECK_STRING);
			if (logChannel == null)
				return;
			if (Variables.Guilds[context.Guild.Id].IgnoredChannels.Contains(context.Channel.Id))
				return;

			EmbedBuilder embed = Actions.addFooter(Actions.makeNewEmbed(description: context.Message.Content), "Mod Log");
			Actions.addAuthor(embed, context.User.Username + "#" + context.User.Discriminator + " in #" + context.Channel.Name, context.User.AvatarUrl);
			await Actions.sendEmbedMessage(logChannel, embed);
		}
	}
}