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
			++Variables.TotalUsers;
			if (Variables.STOP)
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
					++curInv.Uses;
				}
			}

			//Check if should add them to a slowmode for channel/guild
			if (Variables.SlowmodeGuilds.ContainsKey(user.Guild.Id) || user.Guild.TextChannels.Intersect(Variables.SlowmodeChannels.Keys).Any())
			{
				Actions.AddSlowmodeUser(user);
			}
			if (Variables.Guilds[user.Guild.Id].RaidPrevention)
			{
				await user.AddRolesAsync(Variables.Guilds[user.Guild.Id].MuteRole);
				Variables.Guilds[user.Guild.Id].UsersWhoHaveBeenMuted.Add(user);
			}

			var logChannel = await Actions.VerifyLogChannel(user.Guild);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			//Invite string
			var inviteString = "";
			if (curInv != null)
			{
				inviteString = String.Format("**Invite:** {0}", curInv.Code);
			}

			if (user.IsBot)
			{
				var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}\n{1}", user.Id, inviteString), Constants.JOIN);
				Actions.AddFooter(embed, "Bot Joined");
				Actions.AddAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl);
				await Actions.SendEmbedMessage(logChannel, embed);
			}
			else
			{
				var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}\n{1}", user.Id, inviteString), Constants.JOIN);
				Actions.AddFooter(embed, "User Joined");
				Actions.AddAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl);
				await Actions.SendEmbedMessage(logChannel, embed);
			}
			++Variables.LoggedJoins;
		}

		public static async Task OnUserLeft(SocketGuildUser user)
		{
			--Variables.TotalUsers;
			//Check if the bot was the one that left
			if (user == user.Guild.GetUser(Variables.Bot_ID))
			{
				Variables.Guilds.Remove(user.Guild.Id);
				return;
			}

			if (Variables.STOP)
				return;
			var logChannel = await Actions.VerifyLogChannel(user.Guild);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			//Form the length stayed string
			var lengthStayed = "";
			if (user.JoinedAt.HasValue)
			{
				var time = DateTime.UtcNow.Subtract(user.JoinedAt.Value.UtcDateTime);
				lengthStayed = String.Format("\n**Stayed for:** {0}:{1:00}:{2:00}:{3:00}", time.Days, time.Hours, time.Minutes, time.Seconds);
			}

			if (user.IsBot)
			{
				var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}{1}", user.Id, lengthStayed), Constants.LEAV);
				Actions.AddFooter(embed, "Bot Left");
				Actions.AddAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl);
				await Actions.SendEmbedMessage(logChannel, embed);
			}
			else
			{
				var embed = Actions.MakeNewEmbed(null, String.Format("**ID:** {0}{1}", user.Id, lengthStayed), Constants.LEAV);
				Actions.AddFooter(embed, "User Left");
				Actions.AddAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl);
				await Actions.SendEmbedMessage(logChannel, embed);
			}
			++Variables.LoggedLeaves;
		}

		public static async Task OnUserUnbanned(SocketUser user, SocketGuild guild)
		{
			if (Variables.STOP)
				return;
			var logChannel = await Actions.VerifyLogChannel(guild);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			//Get the username/discriminator via this dictionary since they don't exist otherwise
			var username = Variables.UnbannedUsers.ContainsKey(user.Id) ? Variables.UnbannedUsers[user.Id].Username : "null";
			var discriminator = Variables.UnbannedUsers.ContainsKey(user.Id) ? Variables.UnbannedUsers[user.Id].Discriminator : "0000";

			var embed = Actions.MakeNewEmbed(null, "**ID:** " + user.Id, Constants.UNBN);
			Actions.AddFooter(embed, "User Unbanned");
			Actions.AddAuthor(embed, String.Format("{0}#{1}", username, discriminator), user.AvatarUrl);
			await Actions.SendEmbedMessage(logChannel, embed);
			++Variables.LoggedUnbans;
		}

		public static async Task OnUserBanned(SocketUser user, SocketGuild guild)
		{
			if (Variables.STOP)
				return;
			//Check if the bot was the one banned
			if (user == guild.GetUser(Variables.Bot_ID))
			{
				Variables.Guilds.Remove(guild.Id);
				return;
			}

			var logChannel = await Actions.VerifyLogChannel(guild);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			var embed = Actions.MakeNewEmbed(null, "**ID:** " + user.Id, Constants.BANN);
			Actions.AddFooter(embed, "User Banned");
			Actions.AddAuthor(embed, String.Format("{0}#{1}", user.Username, user.Discriminator), user.AvatarUrl);
			await Actions.SendEmbedMessage(logChannel, embed);
			++Variables.LoggedBans;
		}

		public static async Task OnUserUpdated(SocketUser beforeUser, SocketUser afterUser)
		{
			if (beforeUser == null || afterUser == null || Variables.STOP)
				return;

			//Name change
			//TODO: Make this work somehow
			if (!beforeUser.Username.Equals(afterUser.Username, StringComparison.OrdinalIgnoreCase))
			{
				foreach (var guild in Variables.Client.GetGuilds().Where(x => x.Users.Contains(afterUser)))
				{
					var logChannel = await Actions.VerifyLogChannel(guild);
					if (logChannel == null ||
						!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
						return;

					var embed = Actions.MakeNewEmbed(null, null, Constants.UEDT);
					Actions.AddFooter(embed, "Name Changed");
					Actions.AddField(embed, "Before:", "`" + beforeUser.Username + "`");
					Actions.AddField(embed, "After:", "`" + afterUser.Username + "`", false);
					Actions.AddAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl);
					await Actions.SendEmbedMessage(logChannel, embed);
					++Variables.LoggedUserChanges;
				}
			}
		}

		public static async Task OnGuildMemberUpdated(SocketGuildUser beforeUser, SocketGuildUser afterUser)
		{
			if (Variables.STOP)
				return;
			var logChannel = await Actions.VerifyLogChannel(afterUser);
			if (logChannel == null)
				return;
			var guild = logChannel.Guild;
			if (guild == null || !Variables.Guilds[guild.Id].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
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
				Actions.AddAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl);
				await Actions.SendEmbedMessage(logChannel, embed);
			}
			else if (!(String.IsNullOrWhiteSpace(beforeUser.Nickname) && String.IsNullOrWhiteSpace(afterUser.Nickname)) && !beforeUser.Nickname.Equals(afterUser.Nickname))
			{
				var embed = Actions.MakeNewEmbed(null, null, Constants.UEDT);
				Actions.AddFooter(embed, "Nickname Changed");
				Actions.AddField(embed, "Before:", "`" + beforeUser.Nickname + "`");
				Actions.AddField(embed, "After:", "`" + afterUser.Nickname + "`", false);
				Actions.AddAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl);
				await Actions.SendEmbedMessage(logChannel, embed);
			}

			//Role change
			var firstNotSecond = beforeUser.RoleIds.Except(afterUser.RoleIds).Select(x => guild.GetRole(x)).ToList();
			var secondNotFirst = afterUser.RoleIds.Except(beforeUser.RoleIds).Select(x => guild.GetRole(x)).ToList();
			var rolesChange = new List<string>();
			if (firstNotSecond.Any())
			{
				//In separate task in case of a deleted role
				await Task.Run(async () =>
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

					var embed = Actions.MakeNewEmbed(null, String.Format("**Role{0} Lost:** {1}", rolesChange.Count != 1 ? "s" : "", String.Join(", ", rolesChange)), Constants.UEDT);
					Actions.AddFooter(embed, "Role Lost");
					Actions.AddAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl);
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
				Actions.AddAuthor(embed, String.Format("{0}#{1}", afterUser.Username, afterUser.Discriminator), afterUser.AvatarUrl);
				await Actions.SendEmbedMessage(logChannel, embed);
			}
			++Variables.LoggedUserChanges;
		}

		public static async Task OnMessageReceived(SocketMessage message)
		{
			if (message.Author.IsBot || Variables.STOP)
				return;

			//Check if the channel is a guild channel or DM channel
			var channel = message.Channel as IGuildChannel;
			//Check if someone trying to set themselves as bot owner
			if (channel == null)
			{
				await MessageReceivedActions.BotOwner(channel, message);
				return;
			}
			//Get the guild
			var guild = channel.Guild;
			if (guild == null)
				return;
			//Check if the log channel is valid and if image logging is enabled
			var logChannel = await Actions.VerifyLogChannel(guild);
			if (logChannel != null && !Variables.Guilds[guild.Id].LogActions.Any(x => Enum.GetName(typeof(LogActions), LogActions.ImageLog).IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
			{
				if (message.Attachments.Any())
				{
					await Actions.ImageLog(logChannel, message, false);
				}
				if (message.Embeds.Any())
				{
					await Actions.ImageLog(logChannel, message, true);
				}
			}
			//Check if the user should be spam prevented
			await MessageReceivedActions.SpamPrevention(guild, message);
			//Check if the users is voting on a spam prevention
			MessageReceivedActions.VotingOnSpamPrevention(guild, message);
			//Check if anything to do with deleting/enabling preferences
			await MessageReceivedActions.ModifyPreferences(guild, message);
			//Check if any active closewords
			await MessageReceivedActions.CloseWords(guild, message);
			//Check if slowmode or not banned phrases
			await MessageReceivedActions.SlowmodeOrBannedPhrases(guild, channel, message);

			++Variables.LoggedMessages;
		}

		public static async Task OnMessageUpdated(Optional<SocketMessage> beforeMessage, SocketMessage afterMessage)
		{
			if (afterMessage.Author.IsBot || Variables.STOP)
				return;
			await Actions.BannedPhrases(afterMessage);
			var logChannel = await Actions.VerifyLogChannel(afterMessage);
			if (logChannel == null || afterMessage == null || afterMessage.Author == null)
				return;
			var guild = logChannel.Guild;
			if (Variables.Guilds[guild.Id].IgnoredChannels.Contains(afterMessage.Channel.Id) ||
				!Variables.Guilds[guild.Id].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			//Set the content as strings
			var beforeMsg = Actions.ReplaceMarkdownChars(beforeMessage.IsSpecified ? beforeMessage.Value.Content : null);
			var afterMsg = Actions.ReplaceMarkdownChars(afterMessage.Content);

			//Null check
			if (String.IsNullOrWhiteSpace(beforeMsg))
			{
				beforeMsg = "Empty or unable to be gotten.";
			}
			if (String.IsNullOrWhiteSpace(afterMsg))
			{
				beforeMsg = "Empty or unable to be gotten.";
			}

			//Return if the messages are the same
			if (beforeMsg.Equals(afterMsg))
				return;

			//Check lengths
			if (!(beforeMsg.Length + afterMsg.Length < 1800))
			{
				beforeMsg = beforeMsg.Length > 667 ? "LONG MESSAGE" : beforeMsg;
				afterMsg = afterMsg.Length > 667 ? "LONG MESSAGE" : afterMsg;
			}

			//Set the user as a variable
			var user = afterMessage.Author;
			//Make the embed
			var embed = Actions.MakeNewEmbed(null, null, Constants.MEDT);
			Actions.AddFooter(embed, "Message Updated");
			Actions.AddField(embed, "Before:", "`" + beforeMsg + "`");
			Actions.AddField(embed, "After:", "`" + afterMsg + "`", false);
			Actions.AddAuthor(embed, String.Format("{0}#{1} in #{2}", user.Username, user.Discriminator, afterMessage.Channel), user.AvatarUrl);
			await Actions.SendEmbedMessage(logChannel, embed);

			++Variables.LoggedEdits;
		}

		public static async Task OnMessageDeleted(ulong messageID, Optional<SocketMessage> message)
		{
			if (!message.IsSpecified || Variables.STOP)
				return;
			var logChannel = await Actions.VerifyLogChannel(message.Value);
			if (logChannel == null)
				return;
			var guild = logChannel.Guild;
			if (Variables.Guilds[guild.Id].IgnoredChannels.Contains(message.Value.Channel.Id))
				return;

			++Variables.LoggedDeletes;

			//Check if logging deleted messages is on
			if (!Variables.Guilds[guild.Id].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			//Get a list of the deleted messages per guild
			var mainMessages = new List<SocketMessage>();
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
			Task t = Task.Run(async () =>
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
				List<SocketMessage> deletedMessages;
				var taskMessages = Variables.DeletedMessages[guild.Id];
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
							deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
								x.Author.Username,
								x.Author.Discriminator,
								x.Channel,
								x.CreatedAt.ToString("HH:mm:ss"),
								Actions.ReplaceMarkdownChars((String.IsNullOrEmpty(msgContent) ? msgContent : msgContent + "\n") + description)));
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
						var content = String.IsNullOrEmpty(x.Content) ? "EMPTY MESSAGE" : x.Content;
						deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
							x.Author.Username,
							x.Author.Discriminator,
							x.Channel,
							x.CreatedAt.ToString("HH:mm:ss"),
							Actions.ReplaceMarkdownChars(content + " + " + x.Attachments.ToList().First().Filename)));
					}
					//Else add the message in normally
					else
					{
						var content = String.IsNullOrEmpty(x.Content) ? "EMPTY MESSAGE" : x.Content;
						deletedMessagesContent.Add(String.Format("`{0}#{1}` **IN** `#{2}` **SENT AT** `[{3}]`\n```\n{4}```",
							x.Author.Username,
							x.Author.Discriminator,
							x.Channel,
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
			if (Variables.STOP)
				return;
			var logChannel = await Actions.VerifyLogChannel(role.Guild);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed("Role Created", String.Format("Name: `{0}`\nID: `{1}`", role.Name, role.Id), Constants.CCRE);
			Actions.AddFooter(embed, "Role Created");
			await Actions.SendEmbedMessage(logChannel, embed);
		}

		public static async Task OnRoleUpdated(SocketRole beforeRole, SocketRole afterRole)
		{
			if (Variables.STOP)
				return;
			var logChannel = await Actions.VerifyLogChannel(afterRole.Guild);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			if (!beforeRole.Name.Equals(afterRole.Name, StringComparison.OrdinalIgnoreCase))
			{
				//Make the embed
				var embed = Actions.MakeNewEmbed("Role Name Changed", null, Constants.REDT);
				Actions.AddFooter(embed, "Role Name Changed");
				Actions.AddField(embed, "Before:", "`" + beforeRole.Name + "`");
				Actions.AddField(embed, "After:", "`" + afterRole.Name + "`", false);
				await Actions.SendEmbedMessage(logChannel, embed);
			}
		}

		public static async Task OnRoleDeleted(SocketRole role)
		{
			//Add this to prevent massive spam fests when a role is deleted
			Variables.DeletedRoles.Add(role.Id);

			if (Variables.STOP)
				return;
			var logChannel = await Actions.VerifyLogChannel(role.Guild);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed("Role Deleted", String.Format("Name: `{0}`\nID: `{1}`", role.Name, role.Id), Constants.CCRE);
			Actions.AddFooter(embed, "Role Deleted");
			await Actions.SendEmbedMessage(logChannel, embed);
		}

		public static async Task OnChannelCreated(SocketChannel channel)
		{
			if (Variables.STOP)
				return;
			var logChannel = await Actions.VerifyLogChannel(channel);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			var chan = channel as ITextChannel;

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
			if (Variables.STOP)
				return;
			var logChannel = await Actions.VerifyLogChannel(afterChannel);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			//Create a variable of beforechannel and afterchannel as an IGuildChannel for later use
			var bChan = beforeChannel as IGuildChannel;
			var aChan = afterChannel as IGuildChannel;

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

			if (!aChan.Name.Equals(bChan.Name, StringComparison.OrdinalIgnoreCase))
			{
				//Make the embed
				var embed = Actions.MakeNewEmbed("Channel Name Changed", null, Constants.CEDT);
				Actions.AddFooter(embed, "Channel Name Changed");
				Actions.AddField(embed, "Before:", "`" + bChan.Name + "`");
				Actions.AddField(embed, "After:", "`" + aChan.Name + "`", false);
				await Actions.SendEmbedMessage(logChannel, embed);
			}
		}

		public static async Task OnChannelDeleted(SocketChannel channel)
		{
			if (Variables.STOP)
				return;
			var logChannel = await Actions.VerifyLogChannel(channel);
			if (logChannel == null)
				return;
			if (!Variables.Guilds[logChannel.GuildId].LogActions.Any(x => MethodBase.GetCurrentMethod().Name.IndexOf(Enum.GetName(typeof(LogActions), x), StringComparison.OrdinalIgnoreCase) >= 0))
				return;

			//Convert the channel to an IGuildChannel
			var chan = channel as IGuildChannel;
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
			if (Variables.STOP)
				return;

			Actions.WriteLine(String.Format("'{0}' on {1}: \'{2}\'", Actions.FormatUser(context.User), Actions.FormatGuild(context.Guild), context.Message.Content));

			var logChannel = await Actions.VerifyLogChannel(context.Guild, Constants.MOD_LOG_CHECK_STRING);
			if (logChannel == null)
				return;
			if (Variables.Guilds[context.Guild.Id].IgnoredChannels.Contains(context.Channel.Id))
				return;

			//Make the embed
			var embed = Actions.MakeNewEmbed(description: context.Message.Content);
			Actions.AddFooter(embed, "Mod Log");
			Actions.AddAuthor(embed, String.Format("{0} in #{1}", Actions.FormatUser(context.User), context.Channel.Name), context.User.AvatarUrl);
			await Actions.SendEmbedMessage(logChannel, embed);
		}
	}

	public class MessageReceivedActions : ModuleBase
	{
		public static async Task BotOwner(IGuildChannel channel, IMessage message)
		{
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
						await Actions.SendDMMessage(message.Channel as IDMChannel, "Congratulations, you are now the owner of the bot.");
					}
					else
					{
						Variables.PotentialBotOwners.Remove(message.Author.Id);
						await Actions.SendDMMessage(message.Channel as IDMChannel, "That is the incorrect key.");
					}
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
					else if (Variables.GuildsDeletingPreferences.Contains(guild))
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

		public static async Task SlowmodeOrBannedPhrases(IGuild guild, IGuildChannel channel, SocketMessage message)
		{
			//Check if the guild has slowmode enabled currently
			if (Variables.SlowmodeGuilds.ContainsKey(guild.Id) || Variables.SlowmodeChannels.ContainsKey(channel))
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
			var spamPrevention = Variables.Guilds[guild.Id].SpamPrevention;
			//Check if the spam prevention exists and if the message has more mentions than are allowed
			if (spamPrevention != null && message.MentionedUserIds.Distinct().Count() >= spamPrevention.Mentions)
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
				++user.CurrentSpamAmount;
				//Check if that's greater than the allowed amount
				if (user.CurrentSpamAmount >= spamPrevention.AmountOfMessages)
				{
					//Send a message telling the users of the guild they can vote to ban this person
					await Actions.SendChannelMessage(message.Channel, String.Format("The user `{0}#{1}` needs `{2}` votes to be kicked. Vote to kick them by mentioning them.",
						user.User.Username, user.User.Discriminator, spamPrevention.VotesNeededForKick));
					//Enable them to be kicked
					user.PotentialKick = true;
				}
			}
		}

		public static void VotingOnSpamPrevention(IGuild guild, IMessage message)
		{
			//Get the users primed to be kicked/banned by the spam prevention and get the spam prevention the guild has
			var users = Variables.Guilds[guild.Id].SpamPreventionUsers.Where(x => x.PotentialKick).ToList();
			var spamPrevention = Variables.Guilds[guild.Id].SpamPrevention;
			if (spamPrevention != null && users.Any())
			{
				//Cross reference the almost kicked users and the mentioned users
				users.ForEach(async x =>
				{
					//Check if mentioned users contains any users almost kicked. Check if the person has already voted
					if (message.MentionedUserIds.Contains(x.User.Id) && !x.UsersWhoHaveAlreadyVoted.Contains(message.Author.Id))
					{
						//Don't allow users to vote on themselves
						if (x.User.Id == message.Author.Id)
							return;
						//Increment their votes
						++x.VotesToKick;
						//Add the user to the already voted list
						x.UsersWhoHaveAlreadyVoted.Add(message.Author.Id);
						//Check if the bot can even kick/ban this user
						if (Actions.GetPosition(guild, x.User) >= Actions.GetPosition(guild, await guild.GetUserAsync(Variables.Bot_ID)))
							return;
						//Check if they should be punished
						if (x.VotesToKick >= spamPrevention.VotesNeededForKick)
						{
							//Check if they've already been kicked (which means they should be banned now)
							if (x.AlreadyKicked)
							{
								await guild.AddBanAsync(x.User);
							}
							//Otherwise just kick them
							else
							{
								await x.User.KickAsync();
							}
						}
					}
				});
			}
		}
	}
}