using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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
			Actions.WriteLine(String.Format("{0}: {1} is now online on shard {2}.", MethodBase.GetCurrentMethod().Name, Actions.FormatGuild(guild), Variables.Client.GetShardFor(guild).ShardId));

			if (!Variables.Guilds.ContainsKey(guild.Id))
			{
				//Put the guild into a list
				Variables.Guilds.Add(guild.Id, new BotGuildInfo(guild));
				//Put the invites into a list holding mainly for usage checking
				Task.Run(async () =>
				{
					//Get all of the invites and add their guildID, code, and current uses to the usage check list
					Variables.Guilds[guild.Id].Invites = (await guild.GetInvitesAsync()).ToList().Select(x => new BotInvite(x.GuildId, x.Code, x.Uses)).ToList();
				});

				//Incrementing
				Variables.TotalUsers += guild.MemberCount;
				Variables.TotalGuilds++;

				//Loading everything
				if (Variables.Bot_ID != 0)
				{
					Task.Run(async () =>
					{
						await Actions.LoadGuild(guild);
					});
				}
				else
				{
					Variables.GuildsToBeLoaded.Add(guild);
				}
			}

			//Check if the bot's the only one in the guild
			if (guild.MemberCount == 1)
			{
				//Delete it
				Task.Run(async () =>
				{
					await guild.DeleteAsync();
				});
			}

			return Task.CompletedTask;
		}

		public static Task OnGuildUnavailable(SocketGuild guild)
		{
			Actions.WriteLine(String.Format("{0}: Guild is now offline {1}.", MethodBase.GetCurrentMethod().Name, Actions.FormatGuild(guild)));

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
			Actions.WriteLine(String.Format("{0}: Bot has joined {1}.", MethodBase.GetCurrentMethod().Name, Actions.FormatGuild(guild)));

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
				//Allows up to 25 bots
				percentage = .25;
			}
			else
			{
				percentage = .15;
			}

			//Leave if too many bots
			if (botCount / (users * 1.0) > percentage)
			{
				Task.Run(async () =>
				{
					await guild.LeaveAsync();
				});
			}

			//Warn if at the maximum
			var guilds = Variables.Client.GetGuilds().Count;
			var shards = Variables.Client.GetShards().Count;
			if (guilds + 100 >= shards * 2500)
			{
				Actions.WriteLine(String.Format("The bot currently has {0} out of {1} possible spots for servers filled. Please increase the shard count.", guilds, shards * 2500));
			}
			//Leave the guild
			if (guilds > shards * 2500)
			{
				Task.Run(async () =>
				{
					await guild.LeaveAsync();
				});
				//Send a message to the console
				Actions.WriteLine(String.Format("Left the guild {0} due to having too many guilds on the client and not enough shards.", Actions.FormatGuild(guild)));
			}

			return Task.CompletedTask;
		}

		public static Task OnLeftGuild(SocketGuild guild)
		{
			Actions.WriteLine(String.Format("{0}: Bot has left {1}.", MethodBase.GetCurrentMethod().Name, Actions.FormatGuild(guild)));

			Variables.TotalUsers -= (guild.MemberCount + 1);
			Variables.TotalGuilds--;

			return Task.CompletedTask;
		}
	}

	public class ServerLogs : ModuleBase
	{
		//TODO: Remove most of the events that will get replaced by the audit log
		public static async Task OnUserJoined(SocketGuildUser user)
		{
			//Increment the user total user count
			++Variables.TotalUsers;
			//Get the guild and log channel
			var guildAndLogChannel = await Actions.VerifyGuildAndLogChannel(user, LogActions.UserJoined);
			var logChannel = guildAndLogChannel.Item1;
			var guild = guildAndLogChannel.Item2;
			if (logChannel == null || guild == null)
				return;

			//Get the current invites
			var curInvs = await user.Guild.GetInvitesAsync();
			//Get the invites that have already been put on the bot
			var botInvs = Variables.Guilds.ContainsKey(user.Guild.Id) ? Variables.Guilds[user.Guild.Id].Invites : null;
			//Set an invite to hold the current invite the user joined on
			BotInvite curInv = null;
			if (botInvs != null)
			{
				//Find the first invite where the bot invite has the same code as the current invite but different use counts
				curInv = botInvs.FirstOrDefault(bI => curInvs.Any(cI => cI.Code == bI.Code && cI.Uses != bI.Uses));

				//If the invite is null, take that as meaning there are new invites on the guild
				if (curInv == null)
				{
					//Get the new invites on the guild by finding which guild invites aren't on the bot invites list
					var newInvs = curInvs.Where(x => !botInvs.Select(y => y.Code).Contains(x.Code));
					//If there's only one, then use that as the current inv. If there's more than one then there's no way to know what invite it was on
					if (newInvs.Count() == 0 && user.Guild.Features.Contains(Constants.VANITY_URL, StringComparer.OrdinalIgnoreCase))
					{
						curInv = new BotInvite(user.Guild.Id, "Vanity URL", 0);
					}
					else if (newInvs.Count() == 1)
					{
						curInv = new BotInvite(newInvs.First().GuildId, newInvs.First().Code, newInvs.First().Uses);
					}
					//Add all of the invites to the bot invites list
					botInvs.AddRange(newInvs.Select(x => new BotInvite(x.GuildId, x.Code, x.Uses)));
				}
				else
				{
					//Increment the invite the bot is holding if a curInv was found so as to match with the current invite uses count
					curInv.IncreaseUses();
				}
			}

			//Check if should add them to a slowmode for channel/guild
			if (Variables.SlowmodeGuilds.ContainsKey(user.Guild.Id) || user.Guild.TextChannels.Intersect(Variables.SlowmodeChannels.Keys).Any())
			{
				//Add them to the slowmode user list
				await Actions.AddSlowmodeUser(user);
			}
			if (Variables.Guilds[user.Guild.Id].RaidPrevention)
			{
				//Give them the mute role
				await user.AddRolesAsync(Variables.Guilds[user.Guild.Id].MuteRole);
				//Add them to the list of users who have been muted
				Variables.Guilds[user.Guild.Id].UsersWhoHaveBeenMuted.Add(user);
			}

			//Invite string
			var inviteString = "";
			if (curInv != null)
			{
				inviteString = String.Format("**Invite:** {0}", curInv.Code);
			}

			//Make the embed
			if (user.IsBot)
			{
				var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}\n{1}", user.Id, inviteString), Constants.JOIN);
				Actions.AddFooter(embed, "Bot Joined");
				Actions.AddAuthor(embed, Actions.FormatUser(user), user.GetAvatarUrl());
				await Actions.SendEmbedMessage(logChannel, embed);
			}
			else
			{
				var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}\n{1}", user.Id, inviteString), Constants.JOIN);
				Actions.AddFooter(embed, "User Joined");
				Actions.AddAuthor(embed, Actions.FormatUser(user), user.GetAvatarUrl());
				await Actions.SendEmbedMessage(logChannel, embed);
			}

			//Increment the logged joins
			++Variables.LoggedJoins;
		}

		public static async Task OnUserLeft(SocketGuildUser user)
		{
			//Decrease the total users count
			--Variables.TotalUsers;
			//Get the guild and log channel
			var guildAndLogChannel = await Actions.VerifyGuildAndLogChannel(user, LogActions.UserLeft);
			var logChannel = guildAndLogChannel.Item1;
			var guild = guildAndLogChannel.Item2;
			if (logChannel == null || guild == null)
				return;

			//Check if the bot was the one that left
			if (user == user.Guild.GetUser(Variables.Bot_ID))
			{
				Variables.Guilds.Remove(user.Guild.Id);
				return;
			}
			//Check if the bot's the only user left in the guild
			else if (user.Guild.MemberCount == 1)
			{
				//Delete it
				await user.Guild.DeleteAsync();
			}

			//Form the length stayed string
			var lengthStayed = "";
			if (user.JoinedAt.HasValue)
			{
				var time = DateTime.UtcNow.Subtract(user.JoinedAt.Value.UtcDateTime);
				lengthStayed = String.Format("\n**Stayed for:** {0}:{1:00}:{2:00}:{3:00}", time.Days, time.Hours, time.Minutes, time.Seconds);
			}

			//Make the embed
			if (user.IsBot)
			{
				var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}{1}", user.Id, lengthStayed), Constants.LEAV);
				Actions.AddFooter(embed, "Bot Left");
				Actions.AddAuthor(embed, Actions.FormatUser(user), user.GetAvatarUrl());
				await Actions.SendEmbedMessage(logChannel, embed);
			}
			else
			{
				var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}{1}", user.Id, lengthStayed), Constants.LEAV);
				Actions.AddFooter(embed, "User Left");
				Actions.AddAuthor(embed, Actions.FormatUser(user), user.GetAvatarUrl());
				await Actions.SendEmbedMessage(logChannel, embed);
			}

			//Increment the leaves count
			++Variables.LoggedLeaves;
		}

		public static async Task OnUserUnbanned(SocketUser user, SocketGuild guild)
		{
			//Get the guild and log channel
			var guildAndLogChannel = await Actions.VerifyGuildAndLogChannel(guild, user, LogActions.UserUnbanned);
			var logChannel = guildAndLogChannel.Item1;
			if (logChannel == null)
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed(null, "**ID:** " + user.Id, Constants.UNBN);
			Actions.AddFooter(embed, "User Unbanned");
			Actions.AddAuthor(embed, Actions.FormatUser(user), user.GetAvatarUrl());
			await Actions.SendEmbedMessage(logChannel, embed);

			//Increment the unban count
			++Variables.LoggedUnbans;
		}

		public static async Task OnUserBanned(SocketUser user, SocketGuild guild)
		{
			//Check if the bot was the one banned
			if (user == guild.GetUser(Variables.Bot_ID))
			{
				Variables.Guilds.Remove(guild.Id);
				return;
			}
			//Get the guild and log channel
			var guildAndLogChannel = await Actions.VerifyGuildAndLogChannel(guild, user, LogActions.UserBanned);
			var logChannel = guildAndLogChannel.Item1;
			if (logChannel == null)
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed(null, "**ID:** " + user.Id, Constants.BANN);
			Actions.AddFooter(embed, "User Banned");
			Actions.AddAuthor(embed, Actions.FormatUser(user), user.GetAvatarUrl());
			await Actions.SendEmbedMessage(logChannel, embed);

			//Increment the ban count
			++Variables.LoggedBans;
		}

		public static async Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser)
		{
			if (beforeUser.Username == null || afterUser.Username == null || Variables.Pause)
				return;

			//Name change
			if (!beforeUser.Username.Equals(afterUser.Username, StringComparison.OrdinalIgnoreCase))
			{
				await Variables.Client.GetGuilds().Where(x => x.Users.Contains(afterUser)).ToList().ForEachAsync(async guild =>
				{
					//Get the log channel
					var guildAndLogChannel = await Actions.VerifyGuildAndLogChannel(afterUser, LogActions.UserUpdated);
					var logChannel = guildAndLogChannel.Item1;
					if (logChannel == null || guild == null)
						return;

					//Make the embed
					var embed = Actions.MakeNewEmbed(null, null, Constants.UEDT);
					Actions.AddFooter(embed, "Name Changed");
					Actions.AddField(embed, "Before:", "`" + beforeUser.Username + "`");
					Actions.AddField(embed, "After:", "`" + afterUser.Username + "`", false);
					Actions.AddAuthor(embed, Actions.FormatUser(afterUser), afterUser.GetAvatarUrl());
					await Actions.SendEmbedMessage(logChannel, embed);

					//Increment the logged user changed counter
					++Variables.LoggedUserChanges;
				});
			}
		}

		public static async Task OnGuildMemberUpdated(SocketGuildUser beforeUser, SocketGuildUser afterUser)
		{
			//Get the guild and log channel
			var guildAndLogChannel = await Actions.VerifyGuildAndLogChannel(afterUser, LogActions.GuildMemberUpdated);
			var logChannel = guildAndLogChannel.Item1;
			var guild = guildAndLogChannel.Item2;
			if (logChannel == null || guild == null)
				return;

			//Nickname change
			if ((String.IsNullOrWhiteSpace(beforeUser.Nickname) && !String.IsNullOrWhiteSpace(afterUser.Nickname)) ||
				(!String.IsNullOrWhiteSpace(beforeUser.Nickname) && String.IsNullOrWhiteSpace(afterUser.Nickname)))
			{
				var originalNickname = beforeUser.Nickname;
				if (String.IsNullOrWhiteSpace(beforeUser.Nickname))
				{
					originalNickname = "NO NICKNAME";
				}
				var nicknameChange = afterUser.Nickname;
				if (String.IsNullOrWhiteSpace(afterUser.Nickname))
				{
					nicknameChange = "NO NICKNAME";
				}
				//These ones are across more lines than the previous ones up above because it makes it easier to remember what is doing what
				var embed = Actions.MakeNewEmbed(null, null, Constants.UEDT);
				Actions.AddFooter(embed, "Nickname Changed");
				Actions.AddField(embed, "Before:", "`" + originalNickname + "`");
				Actions.AddField(embed, "After:", "`" + nicknameChange + "`", false);
				Actions.AddAuthor(embed, Actions.FormatUser(afterUser), afterUser.GetAvatarUrl());
				await Actions.SendEmbedMessage(logChannel, embed);
			}
			else if (!(String.IsNullOrWhiteSpace(beforeUser.Nickname) && String.IsNullOrWhiteSpace(afterUser.Nickname)) && !beforeUser.Nickname.Equals(afterUser.Nickname))
			{
				var embed = Actions.MakeNewEmbed(null, null, Constants.UEDT);
				Actions.AddFooter(embed, "Nickname Changed");
				Actions.AddField(embed, "Before:", "`" + beforeUser.Nickname + "`");
				Actions.AddField(embed, "After:", "`" + afterUser.Nickname + "`", false);
				Actions.AddAuthor(embed, Actions.FormatUser(afterUser), afterUser.GetAvatarUrl());
				await Actions.SendEmbedMessage(logChannel, embed);
			}

			//Role change
			var firstNotSecond = beforeUser.Roles.Except(afterUser.Roles).ToList();
			var secondNotFirst = afterUser.Roles.Except(beforeUser.Roles).ToList();
			var rolesChange = new List<string>();
			if (firstNotSecond.Any())
			{
				//In separate task in case of a deleted role
				await Task.Run(async () =>
				{
					var users = await guild.GetUsersAsync();
					var maxUsers = 0;
					firstNotSecond.ForEach(x => maxUsers = Math.Max(maxUsers, users.Where(y => y.RoleIds.Contains(x.Id)).Count()));

					await Task.Delay(maxUsers * 2);

					firstNotSecond.ForEach(x =>
					{
						//Return to ignore deleted roles
						if (Variables.DeletedRoles.Contains(x.Id))
							return;
						rolesChange.Add(x.Name);
					});

					//If no roles then return so as to not send a blank message
					if (!rolesChange.Any())
						return;

					var embed = Actions.MakeNewEmbed(null, String.Format("**Role{0} Lost:** {1}", rolesChange.Count != 1 ? "s" : "", String.Join(", ", rolesChange)), Constants.UEDT);
					Actions.AddFooter(embed, "Role Lost");
					Actions.AddAuthor(embed, Actions.FormatUser(afterUser), afterUser.GetAvatarUrl());
					await Actions.SendEmbedMessage(logChannel, embed);
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

				var embed = Actions.MakeNewEmbed(null, String.Format("**Role{0} Gained:** {1}", rolesChange.Count != 1 ? "s" : "", String.Join(", ", rolesChange)), Constants.UEDT);
				Actions.AddFooter(embed, "Role Gained");
				Actions.AddAuthor(embed, Actions.FormatUser(afterUser), afterUser.GetAvatarUrl());
				await Actions.SendEmbedMessage(logChannel, embed);
			}
			//Increment the user changes count
			++Variables.LoggedUserChanges;
		}

		public static async Task OnMessageReceived(SocketMessage message)
		{
			//Get the guild
			var guild = Actions.GetGuildFromMessage(message);
			//Check if the user is trying to become the bot owner by DMing the bot is key
			if (guild == null)
			{
				await MessageReceivedActions.BotOwner(message);
				return;
			}
			//Check if the user should be spam prevented
			await MessageReceivedActions.SpamPrevention(guild, message);
			//Check if the users is voting on a spam prevention
			await MessageReceivedActions.VotingOnSpamPrevention(guild, message);
			//Check if anything to do with deleting/enabling preferences
			await MessageReceivedActions.ModifyPreferences(guild, message);
			//Check if any active closewords
			await MessageReceivedActions.CloseWords(guild, message);
			//Check if slowmode or not banned phrases
			await MessageReceivedActions.SlowmodeOrBannedPhrases(guild, message);

			//Get the log channel
			var guildAndLogChannel = await Actions.VerifyGuildAndLogChannel(message, LogActions.MessageReceived);
			var logChannel = guildAndLogChannel.Item1;
			var newGuild = guildAndLogChannel.Item2;
			if (logChannel == null || newGuild == null)
				return;
			//Check if image logging should happen
			await MessageReceivedActions.ImageLog(logChannel, message);

			//Increment the logged messages count
			++Variables.LoggedMessages;
		}
		
		public static async Task OnMessageUpdated(Cacheable<IMessage, ulong> beforeMessage, SocketMessage afterMessage, ISocketMessageChannel channel)
		{
			//Get the before message's value
			var beforeMessageValue = beforeMessage.HasValue ? beforeMessage.Value : null;
			//Check if the updated message has any banned phrases and should be deleted
			await Actions.BannedPhrases(afterMessage);
			//Get the guild and log channel
			var guildAndLogChannel = await Actions.VerifyGuildAndLogChannel(afterMessage, LogActions.MessageUpdated);
			var logChannel = guildAndLogChannel.Item1;
			var guild = guildAndLogChannel.Item2;
			if (logChannel == null || guild == null)
				return;
			//If the before message is not specified always take that as it should be logged. If the embed counts are greater take that as logging too.
			if (!beforeMessage.HasValue || beforeMessageValue.Embeds.Count() < afterMessage.Embeds.Count())
				await MessageReceivedActions.ImageLog(logChannel, afterMessage);

			//Set the content as strings
			var beforeMsgContent = Actions.ReplaceMarkdownChars(beforeMessageValue?.Content ?? "");
			var afterMsgContent = Actions.ReplaceMarkdownChars(afterMessage.Content);
			//Null check
			beforeMsgContent = String.IsNullOrWhiteSpace(beforeMsgContent) ? "Empty or unable to be gotten." : beforeMsgContent;
			afterMsgContent = String.IsNullOrWhiteSpace(afterMsgContent) ? "Empty or unable to be gotten." : afterMsgContent;
			//Return if the messages are the same
			if (beforeMsgContent.Equals(afterMsgContent))
				return;
			//Check lengths
			if (beforeMsgContent.Length + afterMsgContent.Length > Constants.LENGTH_CHECK)
			{
				beforeMsgContent = beforeMsgContent.Length > 667 ? "LONG MESSAGE" : beforeMsgContent;
				afterMsgContent = afterMsgContent.Length > 667 ? "LONG MESSAGE" : afterMsgContent;
			}

			//Make the embed
			var embed = Actions.MakeNewEmbed(null, null, Constants.MEDT);
			Actions.AddFooter(embed, "Message Updated");
			Actions.AddField(embed, "Before:", "`" + beforeMsgContent + "`");
			Actions.AddField(embed, "After:", "`" + afterMsgContent + "`", false);
			Actions.AddAuthor(embed, String.Format("{0} in #{1}", Actions.FormatUser(afterMessage.Author), afterMessage.Channel), afterMessage.Author.GetAvatarUrl());
			await Actions.SendEmbedMessage(logChannel, embed);

			//Increment the edit count
			++Variables.LoggedEdits;
		}

		public static async Task OnMessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
		{
			//Get the message's value
			var messageValue = message.HasValue ? message.Value : null;
			//Get the guild and log channel
			var guildAndLogChannel = await Actions.VerifyGuildAndLogChannel(messageValue, LogActions.MessageDeleted);
			var logChannel = guildAndLogChannel.Item1;
			var guild = guildAndLogChannel.Item2;
			if (logChannel == null || guild == null)
				return;

			//Increment the deleted messages count
			++Variables.LoggedDeletes;

			//Get a list of the deleted messages per guild
			var mainMessages = new List<IMessage>();
			if (!Variables.DeletedMessages.TryGetValue(guild.Id, out mainMessages))
			{
				mainMessages = new List<IMessage>();
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
					await Task.Delay(TimeSpan.FromSeconds(Constants.TIME_FOR_WAIT_BETWEEN_DELETING_MESSAGES_UNTIL_THEY_PRINT_TO_THE_SERVER_LOG), cancelToken.Token);
				}
				catch (TaskCanceledException)
				{
					//Actions.WriteLine("Expected exception occurred during deleting messages.");
					return;
				}

				int characterCount = 0;
				List<IMessage> deletedMessages;
				var taskMessages = Variables.DeletedMessages[guild.Id];
				lock (taskMessages)
				{
					//Give the messages to a new list so they can be removed from the old one
					deletedMessages = new List<IMessage>(taskMessages);

					//Get the character count
					taskMessages.ForEach(x => characterCount += (x.Content.Length));
					characterCount += taskMessages.Count * 100;

					//Clear the messages
					taskMessages.Clear();
				}

				//Sort by oldest to newest
				var deletedMessagesSorted = deletedMessages.Where(x => x.CreatedAt != null).OrderBy(x => x.CreatedAt.Ticks).ToList();
				if (Constants.NEWEST_DELETED_MESSAGES_AT_TOP)
				{
					deletedMessagesSorted.Reverse();
				}
				//Put the message content into a list of strings for easy usage
				var deletedMessagesContent = new List<string>();
				deletedMessagesSorted.ForEach(x =>
				{
					//See if any embeds deleted
					if (x.Embeds.Any())
					{
						//Get the first embed with a valid description
						var embed = x.Embeds.FirstOrDefault(desc => desc.Description != null);
						//If no embed with valid description, try for valid URL
						embed = embed ?? x.Embeds.FirstOrDefault(url => url.Url != null);
						//If no valid URL, try for valid image
						embed = embed ?? x.Embeds.FirstOrDefault(img => img.Image != null);

						if (embed != null)
						{
							var msgContent = String.IsNullOrWhiteSpace(x.Content) ? "" : "Message Content: " + x.Content;
							var description = String.IsNullOrWhiteSpace(embed.Description) ? "" : "Embed Description: " + embed.Description;
							deletedMessagesContent.Add(String.Format("`{0}` **IN** `{1}` **SENT AT** `[{2}]`\n```\n{3}```",
								Actions.FormatUser(x.Author),
								Actions.FormatChannel(x.Channel),
								x.CreatedAt.ToString("HH:mm:ss"),
								Actions.ReplaceMarkdownChars((String.IsNullOrEmpty(msgContent) ? msgContent : msgContent + "\n") + description)));
						}
						else
						{
							deletedMessagesContent.Add(String.Format("`{0}` **IN** `{1}` **SENT AT** `[{2}]`\n```\n{3}```",
								Actions.FormatUser(x.Author),
								Actions.FormatChannel(x.Channel),
								x.CreatedAt.ToString("HH:mm:ss"),
								"An embed which was unable to be gotten."));
						}
					}
					//See if any attachments were put in
					else if (x.Attachments.Any())
					{
						var content = String.IsNullOrEmpty(x.Content) ? "EMPTY MESSAGE" : x.Content;
						deletedMessagesContent.Add(String.Format("`{0}` **IN** `{1}` **SENT AT** `[{2}]`\n```\n{3}```",
							Actions.FormatUser(x.Author),
							Actions.FormatChannel(x.Channel),
							x.CreatedAt.ToString("HH:mm:ss"),
							Actions.ReplaceMarkdownChars(content + " + " + x.Attachments.ToList().First().Filename)));
					}
					//Else add the message in normally
					else
					{
						var content = String.IsNullOrEmpty(x.Content) ? "EMPTY MESSAGE" : x.Content;
						deletedMessagesContent.Add(String.Format("`{0}` **IN** `{1}` **SENT AT** `[{2}]`\n```\n{3}```",
							Actions.FormatUser(x.Author),
							Actions.FormatChannel(x.Channel),
							x.CreatedAt.ToString("HH:mm:ss"),
							Actions.ReplaceMarkdownChars(content)));
					}
				});

				if (deletedMessages.Count == 0)
					return;
				else if ((deletedMessages.Count <= 5) && (characterCount < Constants.LENGTH_CHECK))
				{
					//If there aren't many messages send the small amount in a message instead of a file or link
					var embed = Actions.MakeNewEmbed("Deleted Messages", String.Join("\n", deletedMessagesContent), Constants.MDEL);
					Actions.AddFooter(embed, "Deleted Messages");
					await Actions.SendEmbedMessage(logChannel, embed);
				}
				else
				{
					if (!Constants.TEXT_FILE)
					{
						//Upload the embed with the hastebin links
						var embed = Actions.MakeNewEmbed("Deleted Messages", Actions.UploadToHastebin(deletedMessagesContent), Constants.MDEL);
						Actions.AddFooter(embed, "Deleted Messages");
						await Actions.SendEmbedMessage(logChannel, embed);
					}
					else
					{
						//Upload the file. This is way harder to try and keep than the hastebin links
						await Actions.UploadTextFile(guild, logChannel, deletedMessagesContent, "Deleted_Messages_", "Deleted Messages");
					}
				}
			});
		}

		public static async Task OnRoleCreated(SocketRole role)
		{
			//Get the guild and log channel
			var guildAndLogChannel = await Actions.VerifyGuildAndLogChannel(role, LogActions.RoleCreated);
			var logChannel = guildAndLogChannel.Item1;
			var guild = guildAndLogChannel.Item2;
			if (logChannel == null || guild == null)
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed("Role Created", String.Format("Name: `{0}`\nID: `{1}`", role.Name, role.Id), Constants.CCRE);
			Actions.AddFooter(embed, "Role Created");
			await Actions.SendEmbedMessage(logChannel, embed);
		}

		public static async Task OnRoleUpdated(SocketRole beforeRole, SocketRole afterRole)
		{
			//Get the guild and log channel
			var guildAndLogChannel = await Actions.VerifyGuildAndLogChannel(afterRole, LogActions.RoleUpdated);
			var logChannel = guildAndLogChannel.Item1;
			var guild = guildAndLogChannel.Item2;
			if (logChannel == null || guild == null)
				return;

			//Make sure the role's name is not the same
			if (beforeRole.Name.Equals(afterRole.Name, StringComparison.OrdinalIgnoreCase))
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed("Role Name Changed", null, Constants.REDT);
			Actions.AddFooter(embed, "Role Name Changed");
			Actions.AddField(embed, "Before:", "`" + beforeRole.Name + "`");
			Actions.AddField(embed, "After:", "`" + afterRole.Name + "`", false);
			await Actions.SendEmbedMessage(logChannel, embed);
		}

		public static async Task OnRoleDeleted(SocketRole role)
		{
			//Add this to prevent massive spam fests when a role is deleted
			Variables.DeletedRoles.Add(role.Id);
			//Get the guild and log channel
			var guildAndLogChannel = await Actions.VerifyGuildAndLogChannel(role, LogActions.RoleDeleted);
			var logChannel = guildAndLogChannel.Item1;
			var guild = guildAndLogChannel.Item2;
			if (logChannel == null || guild == null)
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed("Role Deleted", String.Format("Name: `{0}`\nID: `{1}`", role.Name, role.Id), Constants.CCRE);
			Actions.AddFooter(embed, "Role Deleted");
			await Actions.SendEmbedMessage(logChannel, embed);
		}

		public static async Task OnChannelCreated(SocketChannel channel)
		{
			//Convert the channel to a textchannel
			var chan = channel as IGuildChannel;
			//Get the guild and log channel
			var guildAndLogChannel = await Actions.VerifyGuildAndLogChannel(chan, LogActions.ChannelCreated);
			var logChannel = guildAndLogChannel.Item1;
			var guild = guildAndLogChannel.Item2;
			if (logChannel == null || guild == null)
				return;

			//Check if the channel trying to be made is a bot channel
			if (chan != null && chan.Name == Variables.Bot_Channel && await Actions.GetDuplicateBotChan(chan.Guild))
			{
				await chan.DeleteAsync();
				return;
			}

			//Make the embed
			var embed = Actions.MakeNewEmbed("Channel Created", String.Format("Name: `{0}`\nID: `{1}`", chan.Name, chan.Id), Constants.CCRE);
			Actions.AddFooter(embed, "Channel Created");
			await Actions.SendEmbedMessage(logChannel, embed);
		}

		public static async Task OnChannelUpdated(SocketChannel beforeChannel, SocketChannel afterChannel)
		{
			//Create a variable of beforechannel and afterchannel as an IGuildChannel for later use
			var bChan = beforeChannel as IGuildChannel;
			var aChan = afterChannel as IGuildChannel;
			//Get the guild and log channel
			var guildAndLogChannel = await Actions.VerifyGuildAndLogChannel(aChan, LogActions.ChannelUpdated);
			var logChannel = guildAndLogChannel.Item1;
			var guild = guildAndLogChannel.Item2;
			if (logChannel == null || guild == null)
				return;

			//Check if the name is the bot channel name
			if (aChan != null && aChan.Name.Equals(Variables.Bot_Channel, StringComparison.OrdinalIgnoreCase))
			{
				//If the name wasn't the bot channel name to start with then set it back to its start name
				if (!bChan.Name.Equals(Variables.Bot_Channel, StringComparison.OrdinalIgnoreCase) && await Actions.GetDuplicateBotChan(bChan.Guild))
				{
					await (await bChan.Guild.GetChannelAsync(bChan.Id)).ModifyAsync(x => x.Name = bChan.Name);
					return;
				}
			}

			//Check if the before and after name are the same
			if (aChan.Name.Equals(bChan.Name, StringComparison.OrdinalIgnoreCase))
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed("Channel Name Changed", null, Constants.CEDT);
			Actions.AddFooter(embed, "Channel Name Changed");
			Actions.AddField(embed, "Before:", "`" + bChan.Name + "`");
			Actions.AddField(embed, "After:", "`" + aChan.Name + "`", false);
			await Actions.SendEmbedMessage(logChannel, embed);
		}

		public static async Task OnChannelDeleted(SocketChannel channel)
		{
			//Convert the channel to an IGuildChannel
			var chan = channel as IGuildChannel;
			//Get the guild and log channel
			var guildAndLogChannel = await Actions.VerifyGuildAndLogChannel(chan, LogActions.ChannelDeleted);
			var logChannel = guildAndLogChannel.Item1;
			var guild = guildAndLogChannel.Item2;
			if (logChannel == null || guild == null)
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed("Channel Deleted", String.Format("Name: `{0}`\nID: `{1}`", chan.Name, chan.Id), Constants.CDEL);
			Actions.AddFooter(embed, "Channel Deleted");
			await Actions.SendEmbedMessage(logChannel, embed);
		}
	}

	public class ModLogs : ModuleBase
	{
		public static async Task LogCommand(CommandContext context)
		{
			//Write into the console what the command was and who said it
			Actions.WriteLine(String.Format("'{0}' on {1}: \'{2}\'", Actions.FormatUser(context.User), Actions.FormatGuild(context.Guild), context.Message.Content));

			//Get the guild and log channel
			var guildAndLogChannel = await Actions.VerifyGuildAndModLogChannel(context, LogActions.CommandLog);
			var logChannel = guildAndLogChannel.Item1;
			var guild = guildAndLogChannel.Item2;
			if (logChannel == null || guild == null)
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed(description: context.Message.Content);
			Actions.AddFooter(embed, "Mod Log");
			Actions.AddAuthor(embed, String.Format("{0} in #{1}", Actions.FormatUser(context.User), context.Channel.Name), context.User.GetAvatarUrl());
			await Actions.SendEmbedMessage(logChannel, embed);
		}
	}

	public class MessageReceivedActions : ModuleBase
	{
		public static async Task ImageLog(ITextChannel channel, SocketMessage message)
		{
			if (false
				|| channel == null
				|| message.Author.IsBot
				|| Variables.Guilds[channel.GuildId].IgnoredLogChannels.Contains(channel.Id)
				|| !Variables.Guilds[channel.GuildId].LogActions.Contains(LogActions.ImageLog))
				return;

			if (message.Attachments.Any())
			{
				await Actions.ImageLog(channel, message, false);
			}
			if (message.Embeds.Any())
			{
				await Actions.ImageLog(channel, message, true);
			}
		}

		public static async Task BotOwner(SocketMessage message)
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
					await Actions.SendDMMessage(message.Channel as IDMChannel, "Congratulations, you are now the owner of the bot.");
				}
				else
				{
					Variables.PotentialBotOwners.Remove(message.Author.Id);
					await Actions.SendDMMessage(message.Channel as IDMChannel, "That is the incorrect key.");
				}
			}
		}

		public static async Task ModifyPreferences(IGuild guild, IMessage message)
		{
			//Check if it's the owner of the guild saying something
			if (message.Author.Id == guild.OwnerId)
			{
				//If the message is only 'yes' then check if they're enabling or deleting preferences
				if (message.Content.Equals("yes", StringComparison.OrdinalIgnoreCase))
				{
					if (Variables.GuildsEnablingPreferences.Contains(guild))
					{
						//Enable preferences
						await Actions.EnablePreferences(guild, message as IUserMessage);
					}
					if (Variables.GuildsDeletingPreferences.Contains(guild))
					{
						//Delete preferences
						await Actions.DeletePreferences(guild, message as IUserMessage);
					}
				}
			}
		}

		public static async Task CloseWords(IGuild guild, IMessage message)
		{
			if (Constants.CLOSEWORDSPOSITIONS.Contains(message.Content))
			{
				//Get the number
				var number = Actions.GetInteger(message.Content);
				var closeWordList = Variables.ActiveCloseWords.FirstOrDefault(x => x.User == message.Author as IGuildUser);
				if (closeWordList.User != null)
				{
					//Get the remind
					var remind = Variables.Guilds[guild.Id].Reminds.FirstOrDefault(x => x.Name.Equals(closeWordList.List[number - 1].Name, StringComparison.OrdinalIgnoreCase));

					//Send the remind
					await Actions.SendChannelMessage(message.Channel, remind.Text);

					//Remove that list
					Variables.ActiveCloseWords.Remove(closeWordList);
				}
				else
				{
					var closeHelpList = Variables.ActiveCloseHelp.FirstOrDefault(x => x.User == message.Author as IGuildUser);
					if (closeHelpList.User != null)
					{
						//Get the remind
						var help = closeHelpList.List[number - 1].Help;

						//Send the remind
						await Actions.SendEmbedMessage(message.Channel, Actions.AddFooter(Actions.MakeNewEmbed(help.Name, Actions.GetHelpString(help)), "Help"));

						//Remove that list
						Variables.ActiveCloseHelp.Remove(closeHelpList);
					}
				}
			}
		}

		public static async Task SlowmodeOrBannedPhrases(IGuild guild, IMessage message)
		{
			//Make sure the message is a valid message to do this to
			if (!Actions.VerifyMessage(message))
				return;

			//Check if the guild has slowmode enabled currently
			if (Variables.SlowmodeGuilds.ContainsKey(guild.Id) || Variables.SlowmodeChannels.ContainsKey(message.Channel as IGuildChannel))
			{
				await Actions.Slowmode(message);
			}
			//Check if any banned phrases
			else if (Variables.Guilds[guild.Id].BannedPhrases.Any() || Variables.Guilds[guild.Id].BannedRegex.Any())
			{
				await Actions.BannedPhrases(message);
			}
		}

		public static async Task SpamPrevention(IGuild guild, IMessage message)
		{
			//Get the spam prevention the guild has
			var spamPrevention = Variables.Guilds[guild.Id].MentionSpamPrevention;
			//Check if the spam prevention exists and if the message has more mentions than are allowed
			if (spamPrevention != null && spamPrevention.Enabled && message.MentionedUserIds.Distinct().Count() >= spamPrevention.AmountOfMentionsPerMsg)
			{
				//Set the user as a variable for easy typing
				var author = message.Author as IGuildUser;
				//Check if the bot can even kick/ban this user
				if (Actions.GetPosition(guild, author) >= Actions.GetPosition(guild, await guild.GetUserAsync(Variables.Bot_ID)))
					return;
				//Get the user
				var user = Variables.Guilds[guild.Id].SpamPreventionUsers.FirstOrDefault(x => x.User == author);
				if (user == null)
				{
					user = new SpamPreventionUser(author, 0);
					Variables.Guilds[guild.Id].SpamPreventionUsers.Add(user);
				}
				//Add one to their spam count
				user.IncreaseCurrentSpamAmount();
				//Check if that's greater than the allowed amount
				if (user.CurrentSpamAmount >= spamPrevention.AmountOfMessages)
				{
					//Send a message telling the users of the guild they can vote to ban this person
					await Actions.SendChannelMessage(message.Channel, String.Format("The user `{0}` needs `{1}` votes to be kicked. Vote to kick them by mentioning them.",
						Actions.FormatUser(user.User), spamPrevention.VotesNeededForKick));
					//Enable them to be kicked
					user.EnablePotentialKick();
				}
			}
		}

		public static async Task VotingOnSpamPrevention(IGuild guild, IMessage message)
		{
			//Get the users primed to be kicked/banned by the spam prevention
			var users = Variables.Guilds[guild.Id].SpamPreventionUsers.Where(x => x.PotentialKick).ToList();
			//Return if it's empty
			if (!users.Any())
				return;
			//Get the spam prevention the guild has
			var spamPrevention = Variables.Guilds[guild.Id].MentionSpamPrevention;
			//Return if it's null
			if (spamPrevention == null || !spamPrevention.Enabled)
				return;

			//Cross reference the almost kicked users and the mentioned users
			await users.Where(x => message.MentionedUserIds.Contains(x.User.Id)).ToList().ForEachAsync(async x =>
			{
				//Check if mentioned users contains any users almost kicked. Check if the person has already voted. Don't allow users to vote on themselves.
				if (x.UsersWhoHaveAlreadyVoted.Contains(message.Author.Id) || x.User.Id == message.Author.Id)
					return;
				//Increment the votes on that user
				x.IncreaseVotesToKick();
				//Add the author to the already voted list
				x.AddUserToVotedList(message.Author.Id);
				//Check if the bot can even kick/ban this user or if they should be punished
				if (Actions.GetPosition(guild, x.User) >= Actions.GetPosition(guild, await guild.GetUserAsync(Variables.Bot_ID)) || x.VotesToKick < spamPrevention.VotesNeededForKick)
					return;
				//Check if they've already been kicked to determine if they should be banned or kicked
				await (x.AlreadyKicked ? guild.AddBanAsync(x.User, 1) : x.User.KickAsync());
			});
		}
	}
}